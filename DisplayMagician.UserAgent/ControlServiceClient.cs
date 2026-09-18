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
    public async Task RunAsync(AgentRegistration registration, TimeSpan heartbeatInterval, bool acquireDisplayControl, CancellationToken cancellationToken)
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
        ControlResponse registrationResponse = await SendAndReceiveAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        if (!registrationResponse.IsSuccessful)
        {
            throw new InvalidOperationException(registrationResponse.Message);
        }

        if (acquireDisplayControl)
        {
            ControlResponse leaseResponse = await SendAndReceiveAsync(pipe, new ControlEnvelope
            {
                MessageType = ControlMessageType.AcquireDisplayControl
            }, cancellationToken).ConfigureAwait(false);
            if (!leaseResponse.IsSuccessful)
            {
                throw new InvalidOperationException(leaseResponse.Message);
            }
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(heartbeatInterval, cancellationToken).ConfigureAwait(false);
            ControlEnvelope heartbeat = new ControlEnvelope
            {
                MessageType = ControlMessageType.AgentHeartbeat,
                Payload = JsonSerializer.Serialize(new AgentHeartbeat
                {
                    OperationState = registration.OperationState,
                    IsRecoveryRequired = registration.IsRecoveryRequired
                })
            };
            ControlResponse heartbeatResponse = await SendAndReceiveAsync(pipe, heartbeat, cancellationToken).ConfigureAwait(false);
            if (!heartbeatResponse.IsSuccessful)
            {
                throw new InvalidOperationException(heartbeatResponse.Message);
            }
        }
    }

    public async Task<ControlResponse> RegisterOnceAsync(AgentRegistration registration, CancellationToken cancellationToken)
    {
        using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.ServicePipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        await pipe.ConnectAsync(cancellationToken).ConfigureAwait(false);
        return await SendAndReceiveAsync(pipe, new ControlEnvelope
        {
            MessageType = ControlMessageType.AgentRegistration,
            Payload = JsonSerializer.Serialize(registration)
        }, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<ControlResponse> SendAndReceiveAsync(NamedPipeClientStream pipe, ControlEnvelope request, CancellationToken cancellationToken)
    {
        await ControlEnvelopeSerializer.WriteAsync(pipe, request, cancellationToken).ConfigureAwait(false);
        ControlEnvelope? response = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
        if (response == null || response.RequestId != request.RequestId)
        {
            throw new InvalidDataException("The Control Service returned an invalid response.");
        }

        return JsonSerializer.Deserialize<ControlResponse>(response.Payload)
            ?? throw new InvalidDataException("The Control Service returned an unreadable registration response.");
    }
}
