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

    [Fact]
    public async Task ExecuteCommandAsync_RejectsAnUnavailableRequiredCapabilityWithoutExecutingTheCommand()
    {
        bool wasCalled = false;
        ControlResponse response = await AgentCommandServer.ExecuteCommandAsync(new ControlEnvelope
        {
            MessageType = ControlMessageType.ListProfiles,
            Hello = new ProtocolHello { ClientKind = ControlClientKind.ControlService, ClientId = "DisplayMagician.Test", RequiredCapabilities = new[] { "remote-only-capability" } }
        }, (_, _) =>
        {
            wasCalled = true;
            return Task.FromResult(new ControlResponse { IsSuccessful = true });
        }, CancellationToken.None);

        Assert.False(wasCalled);
        Assert.False(response.IsSuccessful);
        Assert.Equal(ControlErrorCode.RequiredCapabilityUnavailable, response.ErrorCode);
    }

    [Fact]
    public async Task ExecuteCommandAsync_AddsACompatibleWelcomeToSuccessfulResponses()
    {
        ControlEnvelope request = new ControlEnvelope { MessageType = ControlMessageType.ListProfiles, Hello = ControlProtocol.CreateHello(ControlClientKind.ControlService, "DisplayMagician.Test") };
        ControlResponse response = await AgentCommandServer.ExecuteCommandAsync(request, (_, _) => Task.FromResult(new ControlResponse { IsSuccessful = true }), CancellationToken.None);

        Assert.True(response.IsSuccessful);
        Assert.True(ControlProtocol.IsCompatibleWelcome(request.Hello, response.ProtocolWelcome));
        Assert.Contains("profiles", response.ProtocolWelcome!.SupportedCapabilities);
    }
}
