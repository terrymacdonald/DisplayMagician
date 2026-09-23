using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AgentCommandServerTests
{
    [Fact]
    public async Task ExecuteCommandAsync_ReturnsInvalidRequestWhenTheHandlerRejectsMalformedJson()
    {
        ControlResponse response = await AgentCommandServer.ExecuteCommandAsync(
            new ControlEnvelope { MessageType = ControlMessageType.ApplyProfile },
            (_, _) => Task.FromException<ControlResponse>(new JsonException("Invalid payload.")),
            CancellationToken.None);

        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.InvalidRequest, response.ErrorCode);
    }
}
