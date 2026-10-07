using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class ControlServiceRetryPolicyTests
{
    [Fact]
    public void ShouldRetryAfterStartingAgent_RetriesOnlyAgentUnavailable()
    {
        ControlResponse response = new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable };

        Assert.True(ControlServiceRetryPolicy.ShouldRetryAfterStartingAgent(response));
    }

    [Theory]
    [InlineData(ControlErrorCode.None)]
    [InlineData(ControlErrorCode.NotActiveConsoleUser)]
    [InlineData(ControlErrorCode.DisplayControlBusy)]
    [InlineData(ControlErrorCode.InvalidRequest)]
    public void ShouldRetryAfterStartingAgent_DoesNotRetryTerminalResponses(ControlErrorCode errorCode)
    {
        ControlResponse response = new ControlResponse { IsSuccessful = false, ErrorCode = errorCode };

        Assert.False(ControlServiceRetryPolicy.ShouldRetryAfterStartingAgent(response));
        Assert.False(ControlServiceRetryPolicy.ShouldRetryAfterStartingAgent(new ControlResponse { IsSuccessful = true }));
    }
}
