using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AgentIdentityTests
{
    [Fact]
    public void CurrentBuildVersion_ComesFromTheAgentAssemblyFileVersion()
    {
        string assemblyFileVersion = typeof(AgentBuildVersion).Assembly
            .GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version;

        Assert.Equal(assemblyFileVersion, AgentBuildVersion.Current);
        Assert.NotEqual("4.0.0-development", AgentBuildVersion.Current);
    }

    [Fact]
    public void CreateRegistration_DescribesTheCurrentInteractiveProcess()
    {
        AgentRegistration registration = AgentIdentity.CreateRegistration("4.0.0-test", "test");
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();

        Assert.Equal(identity.User!.Value, registration.UserSid);
        Assert.Equal(Process.GetCurrentProcess().SessionId, registration.SessionId);
        Assert.Equal(Environment.ProcessId, registration.ProcessId);
        Assert.Equal("4.0.0-test", registration.Version);
        Assert.Equal("test", registration.StartupMode);
        Assert.Equal(AgentOperationState.Idle, registration.OperationState);
        Assert.False(registration.IsRecoveryRequired);
        Assert.Equal(AgentCommandPipe.CreateName(Environment.ProcessId), registration.CommandPipeName);
    }
}
