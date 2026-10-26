using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using DisplayMagician.Processes;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ProcessTreeMonitorTests
{
    [Fact]
    public void SelectNewestExpectedProcessTree_ExcludesOldAndOtherNewCopies()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianProcessTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string executable = Path.Combine(root, "dm-monitor-test.exe");
        File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe"), executable);
        try
        {
            using Process old = Process.Start(new ProcessStartInfo(executable, "/d /c ping -n 30 127.0.0.1 >nul") { UseShellExecute = false, CreateNoWindow = true })!;
            using ProcessTreeMonitor monitor = ProcessTreeMonitor.BeginWatching(executable, 10, continueTrackingDescendants: true)!;
            using Process libraryClient = Process.GetCurrentProcess();
            monitor.RegisterLaunchedProcesses(new[] { libraryClient }, requireExpectedExecutable: true);
            Assert.False(monitor.HasObservedExpectedProcess);
            using Process first = Process.Start(new ProcessStartInfo(executable, "/d /c ping -n 30 127.0.0.1 >nul") { UseShellExecute = false, CreateNoWindow = true })!;
            Thread.Sleep(100);
            using Process newest = Process.Start(new ProcessStartInfo(executable, "/d /c ping -n 30 127.0.0.1 >nul") { UseShellExecute = false, CreateNoWindow = true })!;
            Assert.True(SpinWait.SpinUntil(() =>
            {
                var tracked = monitor.GetTrackedProcesses();
                try { return tracked.Any(item => item.Id == newest.Id); }
                finally { ProcessUtils.DisposeProcesses(tracked); }
            }, TimeSpan.FromSeconds(5)));

            // Reusing a PID must replace its old identity even though the PID is
            // already in the tracked set when discovery runs before retirement.
            var identities = (Dictionary<int, DateTime>)typeof(ProcessTreeMonitor).GetField("_trackedProcessStartTimes", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(monitor)!;
            lock (typeof(ProcessTreeMonitor).GetField("_syncRoot", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(monitor)!)
                identities[newest.Id] = DateTime.MinValue;
            typeof(ProcessTreeMonitor).GetMethod("TryTrackExpectedProcess", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(monitor, new object[] { newest.Id });

            Assert.True(monitor.SelectNewestExpectedProcessTree());
            Assert.True(monitor.IsInDirectLaunchTree(newest.Id));
            Assert.False(monitor.IsInDirectLaunchTree(first.Id));
            Assert.False(monitor.IsInDirectLaunchTree(old.Id));
            newest.Kill(entireProcessTree: true);
            newest.WaitForExit();
            Assert.True(SpinWait.SpinUntil(() => !monitor.IsRunning, TimeSpan.FromSeconds(5)));
            Assert.False(old.HasExited);
            Assert.False(first.HasExited);
        }
        finally { StopTestProcesses(executable); Directory.Delete(root, true); }
    }

    [Fact]
    public void IsRunning_DiscoversAChildAfterItsParentExitsBeyondTheStartupDeadline()
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianProcessTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string executable = Path.Combine(root, "dm-monitor-test.exe");
        File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe"), executable);
        try
        {
            string arguments = $"/d /c ping -n 3 127.0.0.1 >nul & start \"\" /b \"{executable}\" /d /c ping -n 30 127.0.0.1 >nul";
            using ProcessTreeMonitor monitor = ProcessTreeMonitor.BeginWatching(executable, 1, continueTrackingDescendants: true)!;
            using Process parent = Process.Start(new ProcessStartInfo(executable, arguments) { UseShellExecute = false, CreateNoWindow = true })!;
            monitor.RegisterLaunchedProcesses(new[] { parent }, requireExpectedExecutable: true);
            Assert.True(monitor.SelectNewestExpectedProcessTree());
            // Force the next scan to happen after the handover, outside startup discovery.
            Timer timer = (Timer)typeof(ProcessTreeMonitor).GetField("_snapshotTimer", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(monitor)!;
            timer.Change(Timeout.Infinite, Timeout.Infinite);
            Assert.True(parent.WaitForExit(10000));

            Assert.True(monitor.IsRunning);
            var tracked = monitor.GetTrackedProcesses();
            try { Assert.Contains(tracked, process => process.Id != parent.Id && process.MainModule?.FileName == executable); }
            finally { ProcessUtils.DisposeProcesses(tracked); }
        }
        finally { StopTestProcesses(executable); Directory.Delete(root, true); }
    }

    private static void StopTestProcesses(string executable)
    {
        foreach (ProcessIdentity identity in ProcessTreeMonitor.FindRunningProcesses(executable).Identities)
        {
            try
            {
                using Process process = Process.GetProcessById(identity.ProcessId);
                if (!process.HasExited && process.StartTime.ToUniversalTime() == identity.StartTimeUtc)
                {
                    process.Kill(entireProcessTree: true);
                    process.WaitForExit(5000);
                }
            }
            catch (ArgumentException) { /* The test process exited before cleanup. */ }
        }
    }

    [Fact]
    public void FindRunningProcesses_AndAttachSelectedIdentity_KeepCopiesSeparate()
    {
        string executable = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe");
        using Process first = Process.Start(new ProcessStartInfo(executable, "/d /c ping -n 15 127.0.0.1 >nul") { UseShellExecute = false, CreateNoWindow = true })!;
        try
        {
            System.Threading.Thread.Sleep(150);
            using Process second = Process.Start(new ProcessStartInfo(executable, "/d /c ping -n 15 127.0.0.1 >nul") { UseShellExecute = false, CreateNoWindow = true })!;
            try
            {
                ProcessTreeMonitor.ProcessScan scan = ProcessTreeMonitor.FindRunningProcesses(executable);
                Assert.True(scan.IsComplete);
                Assert.Equal(second.Id, scan.Identities.OrderByDescending(identity => identity.StartTimeUtc).First().ProcessId);
                using ProcessTreeMonitor exactMonitor = ProcessTreeMonitor.BeginWatchingExisting(executable,
                    new ProcessIdentity(first.Id, first.StartTime.ToUniversalTime()), 2)!;
                Assert.NotNull(exactMonitor);
                Assert.True(exactMonitor.IsInDirectLaunchTree(first.Id));
                Assert.False(exactMonitor.IsInDirectLaunchTree(second.Id));
                using ProcessTreeMonitor monitor = ProcessTreeMonitor.BeginWatchingExisting(executable,
                    new ProcessIdentity(second.Id, second.StartTime.ToUniversalTime()), 2)!;
                Assert.NotNull(monitor);
                Assert.True(monitor.IsInDirectLaunchTree(second.Id));
                Assert.False(monitor.IsInDirectLaunchTree(first.Id));
                Assert.Null(ProcessTreeMonitor.BeginWatchingExisting(executable,
                    new ProcessIdentity(first.Id, DateTime.MinValue), 2));
                second.Kill(entireProcessTree: true);
                second.WaitForExit();
                Assert.True(System.Threading.SpinWait.SpinUntil(() => !monitor.IsRunning, TimeSpan.FromSeconds(3)));
                Assert.False(first.HasExited);
            }
            finally { if (!second.HasExited) second.Kill(entireProcessTree: true); }
        }
        finally { if (!first.HasExited) first.Kill(entireProcessTree: true); }
    }

    [Fact]
    public void GetTrackedProcesses_RejectsAReusedProcessId()
    {
        using Process process = Process.GetCurrentProcess();
        using ProcessTreeMonitor monitor = ProcessTreeMonitor.BeginWatching(Environment.ProcessPath!, 1)!;
        FieldInfo identitiesField = typeof(ProcessTreeMonitor).GetField("_trackedProcessStartTimes", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Dictionary<int, DateTime> identities = (Dictionary<int, DateTime>)identitiesField.GetValue(monitor)!;
        identities[process.Id] = DateTime.MinValue;

        Assert.Empty(monitor.GetTrackedProcesses());
    }

    [Fact]
    public void BeginWatching_UsesTheFullGameLaunchTimeout()
    {
        DateTime beforeStart = DateTime.UtcNow;
        using ProcessTreeMonitor monitor = ProcessTreeMonitor.BeginWatching(Environment.ProcessPath!, 60)!;
        DateTime deadline = (DateTime)typeof(ProcessTreeMonitor).GetField("_deadlineUtc", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(monitor)!;

        Assert.InRange(deadline - beforeStart, TimeSpan.FromSeconds(59), TimeSpan.FromSeconds(62));
    }

    [Fact]
    public void IsExecutableRunning_ReturnsTrueForCurrentProcessImage()
    {
        Type monitorType = typeof(ShortcutRunner).Assembly.GetType("DisplayMagician.Processes.ProcessTreeMonitor", throwOnError: true)!;
        MethodInfo isExecutableRunning = monitorType.GetMethod("IsExecutableRunning", BindingFlags.Public | BindingFlags.Static)!;

        Assert.True((bool)isExecutableRunning.Invoke(null, new object[] { Environment.ProcessPath! })!);
    }
}
