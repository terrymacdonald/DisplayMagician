using System;
using System.Diagnostics;
using DisplayMagician.Contracts;
using DisplayMagician.SessionLauncher;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class SessionLauncherValidationTests
{
    [Theory]
    [InlineData("", 1)]
    [InlineData("owner", -1)]
    public void InvalidLaunchRequestsAreRejectedBeforeAccessingSessionTokens(string sid, int session)
    {
        UserAgentLaunchResult result = new InteractiveUserProcessLauncher().Launch(new UserAgentLaunchRequest { UserSid = sid, SessionId = session });
        Assert.False(result.IsSuccessful);
        Assert.Contains("invalid", result.Message);
    }

    [Theory]
    [InlineData("", 1, 1)]
    [InlineData("owner", -1, 1)]
    [InlineData("owner", 1, 0)]
    public void InvalidStopRequestsCannotStopAProcess(string sid, int session, int pid)
    {
        UserAgentLaunchResult result = new InteractiveUserProcessLauncher().Stop(new UserAgentStopRequest { UserSid = sid, SessionId = session, ProcessId = pid });
        Assert.False(result.IsSuccessful);
        Assert.Contains("invalid", result.Message);
    }

    [Fact]
    public void AProcessInTheRequestedSessionIsNotEnoughToAuthorizeStoppingIt()
    {
        using Process current = Process.GetCurrentProcess();
        UserAgentLaunchResult result = new InteractiveUserProcessLauncher().Stop(new UserAgentStopRequest
        { UserSid = "owner", SessionId = current.SessionId, ProcessId = current.Id });
        Assert.False(result.IsSuccessful);
        Assert.Contains("verified User Agent", result.Message);
    }
}
