using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class UserAgentCommandLineTests
{
    [Theory]
    [InlineData("--once", UserAgentStartupAction.RegisterOnce)]
    [InlineData("--acquire-display-control", UserAgentStartupAction.AcquireDisplayControl)]
    [InlineData("--migrate-user-data", UserAgentStartupAction.MigrateUserData)]
    public void Parse_RecognisesSingleArgumentActions(string argument, UserAgentStartupAction expectedAction)
    {
        UserAgentStartupRequest request = UserAgentCommandLine.Parse(new[] { argument });

        Assert.Equal(expectedAction, request.Action);
        Assert.Null(request.ProfileId);
    }

    [Fact]
    public void Parse_UsesNormalRunForRetiredProfileApplyAction()
    {
        UserAgentStartupRequest request = UserAgentCommandLine.Parse(new[] { "--apply-profile", "profile-123" });

        Assert.Equal(UserAgentStartupAction.Run, request.Action);
        Assert.Null(request.ProfileId);
    }

    [Theory]
    [InlineData()]
    [InlineData("--apply-profile")]
    [InlineData("--apply-profile", " ")]
    [InlineData("--unknown")]
    [InlineData("--once", "unexpected")]
    public void Parse_UsesNormalRunForUnsupportedOrIncompleteArguments(params string[] arguments)
    {
        UserAgentStartupRequest request = UserAgentCommandLine.Parse(arguments);

        Assert.Equal(UserAgentStartupAction.Run, request.Action);
        Assert.Null(request.ProfileId);
    }
}
