using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class InteractiveSessionPolicyTests
{
    [Theory]
    [InlineData(InteractiveSessionState.Locked)]
    [InlineData(InteractiveSessionState.Unknown)]
    public void CanStartShortcut_DeniesLockedOrUnverifiedSessions(InteractiveSessionState state)
    {
        Assert.False(InteractiveSessionPolicy.CanStartShortcut(state));
    }

    [Fact]
    public void CanStartShortcut_AllowsUnlockedSessions()
    {
        Assert.True(InteractiveSessionPolicy.CanStartShortcut(InteractiveSessionState.Unlocked));
    }
}