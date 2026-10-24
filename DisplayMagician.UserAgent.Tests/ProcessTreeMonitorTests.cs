using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using DisplayMagician.Processes;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ProcessTreeMonitorTests
{
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
