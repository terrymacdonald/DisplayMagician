using System;
using System.IO.Pipes;
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
}
