using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

public interface IAgentCommandClient
{
    Task<ControlResponse> SendAsync(AgentRegistration agent, ControlEnvelope request, CancellationToken cancellationToken);
}

public sealed class AgentCommandClient : IAgentCommandClient
{
    private readonly TimeSpan _responseTimeout;

    public AgentCommandClient()
        : this(ControlProtocol.ResponseTimeout)
    {
    }

    public AgentCommandClient(TimeSpan responseTimeout)
    {
        if (responseTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(responseTimeout));
        }

        _responseTimeout = responseTimeout;
    }

    public async Task<ControlResponse> SendAsync(AgentRegistration agent, ControlEnvelope request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(agent.CommandPipeName))
        {
            throw new InvalidOperationException("The User Agent did not register a command pipe.");
        }

        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", agent.CommandPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(ControlProtocol.ConnectionTimeout, cancellationToken).ConfigureAwait(false);
        using CancellationTokenSource responseTimeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        responseTimeoutSource.CancelAfter(_responseTimeout);
        try
        {
            await ControlEnvelopeSerializer.WriteAsync(pipe, request, responseTimeoutSource.Token).ConfigureAwait(false);
            ControlEnvelope? response = await ControlEnvelopeSerializer.ReadAsync(pipe, responseTimeoutSource.Token).ConfigureAwait(false);
            if (response == null || response.RequestId != request.RequestId || response.MessageType != request.MessageType)
            {
                throw new InvalidDataException("The User Agent returned an invalid command response.");
            }

            ControlResponse controlResponse = JsonSerializer.Deserialize<ControlResponse>(response.Payload)
                ?? throw new InvalidDataException("The User Agent returned an unreadable command response.");
            if (controlResponse.IsSuccessful && !ControlProtocol.IsCompatibleWelcome(request.Hello, controlResponse.ProtocolWelcome))
            {
                throw new InvalidDataException("The User Agent did not complete a compatible protocol negotiation.");
            }

            return controlResponse;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The User Agent did not respond within the permitted time.");
        }
    }
}
