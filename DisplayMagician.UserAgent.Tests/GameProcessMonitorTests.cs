using System;
using System.Reflection;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class GameProcessMonitorTests
{
    [Fact]
    public void BeginWatching_ReturnsNullWhenTheConfiguredExecutableDoesNotExist()
    {
        Type monitorType = typeof(AgentIdentity).Assembly.GetType("DisplayMagician.Processes.ProcessTreeMonitor")
            ?? throw new InvalidOperationException("The User Agent does not contain its process-tree monitor.");
        MethodInfo beginWatching = monitorType.GetMethod("BeginWatching", BindingFlags.Public | BindingFlags.Static)
            ?? throw new InvalidOperationException("The User Agent process-tree monitor does not expose BeginWatching.");

        object? monitor = beginWatching.Invoke(null, new object[] { "C:\\this-file-does-not-exist\\game.exe", 5 });

        Assert.Null(monitor);
    }
}
