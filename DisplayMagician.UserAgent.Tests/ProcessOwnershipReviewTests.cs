using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using DisplayMagician.ConfigurationDefinitions;
using DisplayMagician.Processes;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ProcessOwnershipReviewTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "DisplayMagician-ProcessOwnership", Guid.NewGuid().ToString("N"));
    private readonly List<Process> _children = new();
    private string Executable => Path.Combine(_root, "dm-owned-test.exe");

    public ProcessOwnershipReviewTests()
    {
        Directory.CreateDirectory(_root);
        File.Copy(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ping.exe"), Executable);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnknownProcessStatusIsRetriedAndFailsInsteadOfReportingExit(bool discoveryComplete)
    {
        bool fail = false;
        int queries = 0;
        using ProcessTreeMonitor monitor = new(Executable, 10, false, Snapshot, readProcessStartTime: id =>
        {
            if (fail) { queries++; throw new Win32Exception(5); }
            return ReadStartTime(id);
        });
        Process child = Start();
        monitor.RegisterLaunchedProcesses(new[] { child });
        if (discoveryComplete) Call(monitor, "CompleteDiscovery");
        fail = true;
        Assert.True(monitor.IsRunning);
        Assert.True(monitor.HadTrackingError);
        Set(monitor, "_nextStatusAttemptUtc", DateTime.MaxValue);
        Assert.True(monitor.IsRunning);
        Assert.Equal(1, queries);
        Set(monitor, "_nextStatusAttemptUtc", DateTime.MinValue);
        Assert.True(monitor.IsRunning);
        Set(monitor, "_nextStatusAttemptUtc", DateTime.MinValue);
        Assert.Throws<ProcessTrackingException>(() => monitor.IsRunning);
        Assert.Equal(3, queries);
        Assert.True(monitor.IsTrackingFailed);
        Assert.False(child.HasExited);
    }

    [Fact]
    public void TransientStatusFailureRecoversAndAConfirmedExitStillCompletes()
    {
        bool fail = false;
        using ProcessTreeMonitor monitor = new(Executable, 10, false, Snapshot,
            readProcessStartTime: id => fail ? throw new InvalidOperationException("Transient query failure.") : ReadStartTime(id));
        Process child = Start();
        monitor.RegisterLaunchedProcesses(new[] { child });
        fail = true;
        Assert.True(monitor.IsRunning);
        fail = false;
        Set(monitor, "_nextStatusAttemptUtc", DateTime.MinValue);
        Assert.True(monitor.IsRunning);
        Assert.False(monitor.HadTrackingError);
        child.Kill();
        child.WaitForExit(5000);
        Assert.False(monitor.IsRunning);
    }

    [Fact]
    public void RegistrationRetriesBeforeSelectingOnlyTheVerifiedLaunchTree()
    {
        int attempts = 0;
        using ProcessTreeMonitor monitor = new(Executable, 10, true, Snapshot, readLaunchStartTime: process =>
        {
            if (++attempts < 3) throw new Win32Exception(5);
            return process.StartTime.ToUniversalTime();
        });
        Process owned = Start();
        Process unrelated = Start();
        Call(monitor, "CaptureProcessTree", new object?[] { null });
        monitor.RegisterLaunchedProcesses(new[] { owned });
        Assert.Equal(3, attempts);
        Assert.True(monitor.IsRunning);
        Assert.True(monitor.IsInDirectLaunchTree(owned.Id));
        Assert.False(monitor.IsInDirectLaunchTree(unrelated.Id));
        List<Process> tracked = monitor.GetTrackedProcesses();
        try { Assert.Equal(owned.Id, Assert.Single(tracked).Id); }
        finally { ProcessUtils.DisposeProcesses(tracked); }
    }

    [Fact]
    public void FailedRegistrationCannotExposeSpeculativeCopiesToPlacementOrCleanup()
    {
        int attempts = 0;
        using ProcessTreeMonitor monitor = new(Executable, 10, true, Snapshot, readLaunchStartTime: _ =>
        {
            attempts++;
            throw new Win32Exception(5);
        });
        Process owned = Start();
        Process unrelated = Start();
        Call(monitor, "CaptureProcessTree", new object?[] { null });
        Assert.Throws<ProcessTrackingException>(() => monitor.RegisterLaunchedProcesses(new[] { owned }));
        Assert.Equal(3, attempts);
        Assert.Throws<ProcessTrackingException>(() => monitor.IsRunning);
        Assert.Empty(monitor.GetTrackedProcesses());
        Assert.False(monitor.IsInDirectLaunchTree(unrelated.Id));
        Assert.False(owned.HasExited);
        Assert.False(unrelated.HasExited);
    }

    [Fact]
    public void CancelledRegistrationDoesNotCloseOrPlaceSpeculativelyObservedCopies()
    {
        using CancellationTokenSource cancellation = new();
        int attempts = 0;
        using ProcessTreeMonitor monitor = new(Executable, 10, true, Snapshot, readLaunchStartTime: _ =>
        {
            attempts++;
            cancellation.Cancel();
            throw new Win32Exception(5);
        });
        Process child = Start();
        Process unrelated = Start();
        Call(monitor, "CaptureProcessTree", new object?[] { null });
        Assert.ThrowsAny<OperationCanceledException>(() => monitor.RegisterLaunchedProcesses(new[] { child }, false, cancellation.Token));
        Assert.Equal(1, attempts);
        ShortcutRunner.WindowPlacementRun run = new(Guid.NewGuid(), new ShortcutDefinition());
        run.Monitors.Add(("helper", monitor));
        Assert.Empty(run.GetSources());
        List<ShortcutRunner.StartedProgram> started = new() { new(0, 0, new() { Process.GetProcessById(child.Id) }, monitor) };
        ShortcutRunner.RunPostGameProgramCleanup(started, new(), stopProcesses: processes =>
        {
            Assert.Empty(processes);
            return true;
        });
        Assert.False(child.HasExited);
        Assert.False(unrelated.HasExited);
    }

    [Fact]
    public void RegistrationWithNoReturnedHandlesLeavesDiscoveryAvailable()
    {
        using ProcessTreeMonitor monitor = new(Executable, 10, true, Snapshot);
        monitor.RegisterLaunchedProcesses(Array.Empty<Process>());
        Assert.False((bool)typeof(ProcessTreeMonitor).GetField("_trackOnlySelectedTree", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(monitor)!);
        Assert.False(monitor.IsTrackingFailed);
    }

    [Fact]
    public void RegisteringAPreExistingProcessIsAnExplicitFailure()
    {
        Process old = Start();
        Thread.Sleep(20);
        using ProcessTreeMonitor monitor = new(Executable, 10, true, Snapshot);
        Assert.Throws<ProcessTrackingException>(() => monitor.RegisterLaunchedProcesses(new[] { old }));
        Assert.False(old.HasExited);
    }

    [Fact]
    public void ProcessExitedCannotConvertRepeatedQueryFailuresIntoSuccess()
    {
        Process child = Start();
        int attempts = 0;
        Assert.Throws<ProcessTrackingException>(() => ProcessUtils.CheckProcessExited(child, _ =>
        {
            attempts++;
            throw new Win32Exception(5);
        }));
        Assert.Equal(3, attempts);
        Assert.False(child.HasExited);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ProcessExitedRecoversFromATransientFailure(bool exit)
    {
        Process child = Start();
        int attempts = 0;
        Assert.Equal(exit, ProcessUtils.CheckProcessExited(child, process =>
        {
            if (++attempts == 1) throw new Win32Exception(5);
            if (exit) { child.Kill(); child.WaitForExit(5000); }
            return process.HasExited;
        }));
        Assert.Equal(2, attempts);
    }

    [Theory]
    [InlineData("existing")]
    [InlineData("unknown_snapshot")]
    [InlineData("old_time")]
    [InlineData("wrong_app")]
    [InlineData("unreadable_identity")]
    public void PackagedActivationWithoutProvenNewOwnershipIsUsableButCannotBeClosed(string scenario)
    {
        Process child = Start();
        Process? activated = UwpApplicationLauncher.Start("fixture!app", "private arguments", out bool owns,
            activate: (_, _) => (0, (uint)child.Id),
            snapshotProcessIds: () => scenario == "unknown_snapshot" ? throw new Win32Exception(5)
                : scenario == "existing" ? new() { child.Id } : new(),
            readStartTime: _ => scenario == "old_time" ? DateTime.MinValue
                : scenario == "unreadable_identity" ? throw new Win32Exception(5) : DateTime.UtcNow,
            readAppId: _ => scenario == "wrong_app" ? "another!app" : "fixture!app");
        using (activated)
        {
            Assert.NotNull(activated);
            Assert.False(owns);
            Assert.False(child.HasExited);
        }
    }

    [Fact]
    public void ANewMatchingPackagedInstanceCanBeMarkedForCleanup()
    {
        Process? child = null;
        using Process? activated = UwpApplicationLauncher.Start("fixture!app", "", out bool owns,
            snapshotProcessIds: () => new(), activate: (_, _) =>
            {
                Thread.Sleep(20);
                child = Start();
                return (0, (uint)child.Id);
            }, readAppId: _ => "FIXTURE!APP");
        Assert.NotNull(activated);
        Assert.True(owns);
        Assert.Equal(child!.Id, activated!.Id);
    }

    [Theory]
    [InlineData(-1, 1u)]
    [InlineData(0, 0u)]
    [InlineData(0, uint.MaxValue)]
    public void FailedOrInvalidPackagedActivationNeverClaimsOwnership(int result, uint processId)
    {
        using Process? activated = UwpApplicationLauncher.Start("fixture!app", "", out bool owns,
            snapshotProcessIds: () => new(), activate: (_, _) => (result, processId));
        Assert.Null(activated);
        Assert.False(owns);
    }

    [Fact]
    public void PackagedActivationExceptionsReturnFailure()
    {
        using Process? activated = UwpApplicationLauncher.Start("fixture!app", "", out bool owns,
            snapshotProcessIds: () => new(), activate: (_, _) => throw new COMException("Fixture activation failure."));
        Assert.Null(activated);
        Assert.False(owns);
    }

    private ProcessTreeMonitor.ProcessSnapshotScan Snapshot() => new(_children.Select(process =>
        new ProcessTreeMonitor.ProcessSnapshot(process.Id, 0, Path.GetFileName(Executable))).Append(new(0, 0, "System")).ToList(), DateTime.UtcNow, true);

    private static DateTime? ReadStartTime(int id)
    {
        using Process process = Process.GetProcessById(id);
        return process.HasExited ? null : process.StartTime.ToUniversalTime();
    }

    private Process Start()
    {
        Process child = Process.Start(new ProcessStartInfo(Executable, "-n 60 127.0.0.1") { UseShellExecute = false, CreateNoWindow = true })!;
        _children.Add(child);
        return child;
    }

    private static void Set(ProcessTreeMonitor monitor, string field, object value)
        => typeof(ProcessTreeMonitor).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(monitor, value);

    private static void Call(ProcessTreeMonitor monitor, string method, params object?[] arguments)
        => typeof(ProcessTreeMonitor).GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(monitor, arguments);

    public void Dispose()
    {
        foreach (Process process in _children)
        {
            try { if (!process.HasExited) { process.Kill(); process.WaitForExit(5000); } }
            finally { process.Dispose(); }
        }
        Directory.Delete(_root, true);
    }
}
