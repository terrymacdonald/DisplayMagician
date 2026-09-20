using System;
using System.Reflection;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ProcessTreeMonitorTests
{
    [Fact]
    public void IsExecutableRunning_ReturnsTrueForCurrentProcessImage()
    {
        Type monitorType = typeof(ShortcutRunner).Assembly.GetType("DisplayMagician.Processes.ProcessTreeMonitor", throwOnError: true)!;
        MethodInfo isExecutableRunning = monitorType.GetMethod("IsExecutableRunning", BindingFlags.Public | BindingFlags.Static)!;

        Assert.True((bool)isExecutableRunning.Invoke(null, new object[] { Environment.ProcessPath! })!);
    }
}