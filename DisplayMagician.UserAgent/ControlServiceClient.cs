using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.UserAgent;

public sealed class ControlServiceClient
{
    public async Task<ControlResponse> RegisterAsync(AgentRegistration registration, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(registration);

        using NamedPipeClientStream pipe = new NamedPipeClientStream(
            ".",
            ControlProtocol.ServicePipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        await pipe.ConnectAsync(cancellationToken).ConfigureAwait(false);

        ControlEnvelope request = new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentRegistration,
            Payload = JsonSerializer.Serialize(registration)
        };
        await ControlEnvelopeSerializer.WriteAsync(pipe, request, cancellationToken).ConfigureAwait(false);

        ControlEnvelope? response = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
        if (response == null || response.RequestId != request.RequestId)
        {
            throw new InvalidDataException("The Control Service returned an invalid registration response.");
        }

        return JsonSerializer.Deserialize<ControlResponse>(response.Payload)
            ?? throw new InvalidDataException("The Control Service returned an unreadable registration response.");
    }
}
