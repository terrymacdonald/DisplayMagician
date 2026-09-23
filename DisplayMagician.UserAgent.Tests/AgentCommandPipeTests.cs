using System;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using DisplayMagician.UserAgent;
using Xunit;

namespace DisplayMagician.UserAgent.Tests;

public sealed class AgentCommandPipeTests
{
    [Fact]
    public void CreateName_UsesTheVersionedCommandPipePrefixAndProcessId()
    {
        Assert.Equal($"{ControlProtocol.AgentCommandPipePrefix}1234", AgentCommandPipe.CreateName(1234));
    }

    [Fact]
    public void CreateName_RejectsInvalidProcessIds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AgentCommandPipe.CreateName(0));
    }

    [Fact]
    public async Task ExecuteCommandAsync_RejectsAnUnsupportedProtocolVersionWithoutExecutingTheCommand()
    {
        bool wasCalled = false;
        ControlResponse response = await AgentCommandServer.ExecuteCommandAsync(new ControlEnvelope
        {
            ProtocolVersion = ControlProtocol.CurrentVersion + 1,
            MessageType = ControlMessageType.ListProfiles
        }, (_, _) =>
        {
            wasCalled = true;
            return Task.FromResult(new ControlResponse { IsSuccessful = true });
        }, CancellationToken.None);

        Assert.False(wasCalled);
        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.UnsupportedProtocolVersion, response.ErrorCode);
    }
}
