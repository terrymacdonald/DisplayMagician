using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using DisplayMagician.Processes;
using DisplayMagician.GameLibraries;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ProcessTreeMonitorRegressionTests
{
    [Fact]
    public void StartupStatusDoesNotClaimRunningBeforeAnyProcessOrFailureHasBeenObserved()
    {
        using ProcessTreeMonitor monitor = new ProcessTreeMonitor(Environment.ProcessPath!, 10, false,
            () => new(new(), DateTime.UtcNow, false));
        Assert.False(monitor.IsRunning);
        Assert.False(monitor.HasObservedExpectedProcess);
        Assert.False(monitor.IsTrackingFailed);
    }

    [Fact]
    public void RegisteredHelpersPruneOtherRootsAndCleanupCannotStopTheOtherHelper()
    {
        WithExecutable(executable =>
        {
            using ProcessTreeMonitor firstMonitor = ProcessTreeMonitor.BeginWatching(executable, 10, true)!;
            using ProcessTreeMonitor secondMonitor = ProcessTreeMonitor.BeginWatching(executable, 10, true)!;
            using Process first = Start(executable);
            using Process second = Start(executable);
            Assert.True(SpinWait.SpinUntil(() => Contains(firstMonitor, second.Id), TimeSpan.FromSeconds(5)));
            firstMonitor.RegisterLaunchedProcesses(new[] { first });
            secondMonitor.RegisterLaunchedProcesses(new[] { second });
            using Process userLaunch = Start(executable);
            Thread.Sleep(400);

            Assert.True(firstMonitor.IsRunning);
            Assert.True(secondMonitor.IsRunning);
            Assert.False(Contains(firstMonitor, second.Id));
            Assert.False(Contains(firstMonitor, userLaunch.Id));
            Assert.False(Contains(secondMonitor, first.Id));
            var cleanup = firstMonitor.GetTrackedProcesses();
            try { Assert.True(ProcessUtils.StopProcess(cleanup)); }
            finally { ProcessUtils.DisposeProcesses(cleanup); }
            Assert.True(first.HasExited);
            Assert.False(second.HasExited);
            Assert.False(userLaunch.HasExited);
            Assert.True(secondMonitor.IsRunning);
        });
    }

    [Fact]
    public void ChildPidReusedAfterSnapshotIsNotAttachedToTheOldSnapshotParent()
    {
        WithExecutable(executable =>
        {
            Process? parent = null;
            Process? unrelated = null;
            DateTime snapshotUtc = default;
            using ProcessTreeMonitor monitor = new ProcessTreeMonitor(executable, 10, true, () => new(
                new List<ProcessTreeMonitor.ProcessSnapshot>
                {
                    new(parent!.Id, 0, Path.GetFileName(executable)),
                    new(unrelated!.Id, parent.Id, Path.GetFileName(executable))
                }, snapshotUtc, true));
            try
            {
                parent = Start(executable);
                snapshotUtc = DateTime.UtcNow;
                Thread.Sleep(50);
                unrelated = Start(executable);
                Assert.True(unrelated.StartTime.ToUniversalTime() > snapshotUtc);
                monitor.RegisterLaunchedProcesses(new[] { parent });
                Assert.True(monitor.IsRunning);
                Assert.False(Contains(monitor, unrelated.Id));
            }
            finally { parent?.Dispose(); unrelated?.Dispose(); }
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ReusedParentPidPreservesOnlyChildrenBornBeforeTheReplacementParent(bool childPredatesReplacement)
    {
        WithExecutable(executable =>
        {
            using Process earlier = Start(executable);
            Thread.Sleep(50);
            using Process later = Start(executable);
            Process child = childPredatesReplacement ? earlier : later;
            Process replacementParent = childPredatesReplacement ? later : earlier;
            using ProcessTreeMonitor monitor = new ProcessTreeMonitor(executable, 10, true, () => new(
                new List<ProcessTreeMonitor.ProcessSnapshot>
                {
                    new(child.Id, replacementParent.Id, Path.GetFileName(executable)),
                    new(replacementParent.Id, 0, Path.GetFileName(executable))
                }, DateTime.UtcNow, true));
            // Simulate the original parent's known identity while the PID now belongs
            // to a replacement process. No actual PID churn or unrelated processes.
            var tracked = (Dictionary<int, DateTime>)Field("_trackedProcessStartTimes").GetValue(monitor)!;
            tracked[replacementParent.Id] = earlier.StartTime.ToUniversalTime().AddMinutes(-1);
            Field("_hasObservedExpectedProcess").SetValue(monitor, true);
            Field("_trackOnlySelectedTree").SetValue(monitor, true);

            Assert.Equal(childPredatesReplacement, monitor.IsRunning);
            Assert.Equal(childPredatesReplacement, Contains(monitor, child.Id));
            Assert.False(Contains(monitor, replacementParent.Id));
        });
    }

    [Fact]
    public void DeferredChildKeepsItsExitedParentUntilAFreshSnapshotCanVerifyTheHandover()
    {
        WithExecutable(executable =>
        {
            ProcessTreeMonitor.ProcessSnapshotScan scan = new(new(), DateTime.UtcNow, false);
            using ProcessTreeMonitor monitor = new ProcessTreeMonitor(executable, 10, true, () => scan);
            using Process parent = Start(executable);
            scan = new(new() { new(parent.Id, 0, Path.GetFileName(executable)) }, DateTime.UtcNow, true);
            monitor.RegisterLaunchedProcesses(new[] { parent });
            parent.Kill(entireProcessTree: true);
            parent.WaitForExit(5000);
            DateTime staleBoundary = DateTime.UtcNow;
            Thread.Sleep(50);
            using Process child = Start(executable);
            scan = new(new() { new(child.Id, parent.Id, Path.GetFileName(executable)) }, staleBoundary, true);
            ProcessUtils.DisposeProcesses(monitor.GetTrackedProcesses(refresh: true));

            var tracked = (Dictionary<int, DateTime>)Field("_trackedProcessStartTimes").GetValue(monitor)!;
            Assert.True(tracked.ContainsKey(parent.Id));
            Assert.False(Contains(monitor, child.Id));
            Assert.True(monitor.IsRunning);
            Field("_nextSnapshotAttemptUtc").SetValue(monitor, DateTime.MinValue);
            scan = scan with { CapturedUtc = DateTime.UtcNow };
            Assert.True(monitor.IsRunning);
            Assert.True(Contains(monitor, child.Id));
            Assert.False(monitor.HadTrackingError);
        });
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void FailedOrIncompleteScansRetryThenFailExplicitlyWithoutCompletingDiscovery(bool complete, bool hasEntries)
    {
        int attempts = 0;
        using ProcessTreeMonitor monitor = new ProcessTreeMonitor(Environment.ProcessPath!, 10, true, () =>
        {
            attempts++;
            var entries = new List<ProcessTreeMonitor.ProcessSnapshot>();
            if (hasEntries) entries.Add(new(0, 0, "System"));
            return new(entries, DateTime.UtcNow, complete);
        });
        for (int attempt = 1; attempt <= 2; attempt++)
        {
            Assert.True(monitor.IsRunning);
            Assert.False(monitor.IsTrackingFailed);
            Assert.Equal(attempt, attempts);
            Assert.Equal(0, (int)Field("_completedSnapshotCount").GetValue(monitor)!);
            // Extra status polls must not consume retries before the retry interval.
            Field("_nextSnapshotAttemptUtc").SetValue(monitor, DateTime.MaxValue);
            Assert.True(monitor.IsRunning);
            Assert.Equal(attempt, attempts);
            Field("_nextSnapshotAttemptUtc").SetValue(monitor, DateTime.MinValue);
        }
        Assert.Throws<ProcessTrackingException>(() => monitor.IsRunning);
        Assert.True(monitor.IsTrackingFailed);
        Assert.Equal(3, attempts);
        Assert.Equal(0, (int)Field("_completedSnapshotCount").GetValue(monitor)!);
        Assert.Throws<ProcessTrackingException>(() => monitor.IsRunning);
        Assert.Equal(3, attempts);
    }

    [Fact]
    public void SuccessfulRetryRecoversWithoutCountingFailedScansAsCompleted()
    {
        int attempts = 0;
        using ProcessTreeMonitor monitor = new ProcessTreeMonitor(Environment.ProcessPath!, 10, true, () =>
        {
            attempts++;
            return new(new List<ProcessTreeMonitor.ProcessSnapshot> { new(0, 0, "System") }, DateTime.UtcNow, attempts > 1);
        });
        Assert.True(monitor.IsRunning);
        Assert.True(monitor.HadTrackingError);
        Assert.Equal(0, (int)Field("_completedSnapshotCount").GetValue(monitor)!);
        Field("_nextSnapshotAttemptUtc").SetValue(monitor, DateTime.MinValue);
        Assert.False(monitor.IsRunning);
        Assert.False(monitor.IsTrackingFailed);
        Assert.False(monitor.HadTrackingError);
        Assert.True((int)Field("_completedSnapshotCount").GetValue(monitor)! > 0);
        Assert.Equal(0, (int)Field("_consecutiveSnapshotFailures").GetValue(monitor)!);
    }

    [Fact]
    public void StartupOnlyMonitorRetriesPastDeadlineAndLegacyGameStatusDoesNotClaimExit()
    {
        int attempts = 0;
        using ProcessTreeMonitor monitor = new ProcessTreeMonitor(Environment.ProcessPath!, 1, false, () =>
        {
            attempts++;
            return new(new List<ProcessTreeMonitor.ProcessSnapshot>(), DateTime.UtcNow, false);
        });
        Thread.Sleep(1100);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            Field("_nextSnapshotAttemptUtc").SetValue(monitor, DateTime.MinValue);
            typeof(ProcessTreeMonitor).GetMethod("CaptureProcessTree", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(monitor, new object?[] { null });
            if (attempt < 2)
            {
                Assert.False(monitor.IsDiscoveryComplete);
                Assert.True(monitor.IsRunning);
            }
        }
        Assert.Equal(3, attempts);
        Assert.Equal(0, (int)Field("_completedSnapshotCount").GetValue(monitor)!);
        Assert.True(monitor.IsTrackingFailed);
        Assert.Throws<ProcessTrackingException>(() => monitor.IsRunning);
        Assert.True(new StatusGame(monitor).IsRunning);
    }

    private sealed class StatusGame : Game
    {
        public StatusGame(ProcessTreeMonitor monitor) => typeof(Game).GetField("_processTreeMonitor", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(this, monitor);
        public override bool IsRunning { get => IsProcessTreeRunning; set { } }
    }

    private static FieldInfo Field(string name) => typeof(ProcessTreeMonitor).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static bool Contains(ProcessTreeMonitor monitor, int processId)
    {
        var tracked = monitor.GetTrackedProcesses();
        try { return tracked.Any(process => process.Id == processId); }
        finally { ProcessUtils.DisposeProcesses(tracked); }
    }

    private static Process Start(string executable) => Process.Start(new ProcessStartInfo(executable, "/d /c ping -n 30 127.0.0.1 >nul")
        { UseShellExecute = false, CreateNoWindow = true })!;

    private static void WithExecutable(Action<string> test)
    {
        string root = Path.Combine(Path.GetTempPath(), "DisplayMagicianProcessTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string executable = Path.Combine(root, "dm-owned-process-test.exe");
        File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe"), executable);
        try { test(executable); }
        finally
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
                catch (ArgumentException) { /* The test process already exited. */ }
            }
            Directory.Delete(root, true);
        }
    }
}
