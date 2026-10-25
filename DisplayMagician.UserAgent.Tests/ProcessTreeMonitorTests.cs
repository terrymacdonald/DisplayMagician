using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using DisplayMagician.Processes;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ProcessTreeMonitorTests
{
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
