using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Xunit;

namespace DisplayMagician.ControlService.Tests;

public sealed class AgentCommandClientTests
{
    [Fact]
    public async Task SendAsync_TimesOutWhenTheUserAgentDoesNotRespond()
    {
        string pipeName = $"displaymagician-test-{Guid.NewGuid():N}";
        using NamedPipeServerStream server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        Task waitForConnection = server.WaitForConnectionAsync();
        AgentCommandClient client = new AgentCommandClient(TimeSpan.FromMilliseconds(100));
        AgentRegistration agent = new AgentRegistration { CommandPipeName = pipeName };

        Task<ControlResponse> send = client.SendAsync(agent, new ControlEnvelope { MessageType = ControlMessageType.ListProfiles }, CancellationToken.None);
        await waitForConnection;
        await ControlEnvelopeSerializer.ReadAsync(server, CancellationToken.None);

        await Assert.ThrowsAsync<TimeoutException>(() => send);
    }

    [Fact]
    public async Task SendAsync_RejectsAResponseWithTheWrongRequestId()
    {
        string pipeName = $"displaymagician-test-{Guid.NewGuid():N}";
        using NamedPipeServerStream server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        Task waitForConnection = server.WaitForConnectionAsync();
        AgentCommandClient client = new AgentCommandClient(TimeSpan.FromSeconds(1));
        AgentRegistration agent = new AgentRegistration { CommandPipeName = pipeName };
        ControlEnvelope request = new ControlEnvelope { MessageType = ControlMessageType.ListProfiles };

        Task<ControlResponse> send = client.SendAsync(agent, request, CancellationToken.None);
        await waitForConnection;
        await ControlEnvelopeSerializer.ReadAsync(server, CancellationToken.None);
        await ControlEnvelopeSerializer.WriteAsync(server, new ControlEnvelope
        {
            MessageType = request.MessageType,
            RequestId = Guid.NewGuid(),
            Payload = JsonSerializer.Serialize(new ControlResponse { IsSuccessful = true })
        }, CancellationToken.None);

        await Assert.ThrowsAsync<InvalidDataException>(() => send);
    }
}
