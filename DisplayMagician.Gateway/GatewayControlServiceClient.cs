using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.Gateway;

/// <summary>Uses the narrow LocalService-only pipe; it cannot invoke normal desktop client operations.</summary>
public sealed class GatewayControlServiceClient
{
    public Task<ControlResponse> RegisterAsync(GatewayPairingIdentity identity, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(identity);
        return SendAsync(ControlMessageType.GatewayRegistration, JsonSerializer.Serialize(new GatewayRegistration { Identity = identity }), cancellationToken);
    }

    public async Task<DevicePairingResult> SubmitDevicePairingAsync(DevicePairingRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ControlResponse response = await SendAsync(ControlMessageType.SubmitDevicePairing, JsonSerializer.Serialize(request), cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.DevicePairingResult != null
            ? response.DevicePairingResult
            : new DevicePairingResult { State = DevicePairingState.Rejected, Message = string.IsNullOrWhiteSpace(response.Message) ? "The pairing request could not be processed." : response.Message };
    }

    private static async Task<ControlResponse> SendAsync(ControlMessageType messageType, string payload, CancellationToken cancellationToken)
    {
        try
        {
            using NamedPipeClientStream pipe = new NamedPipeClientStream(".", ControlProtocol.GatewayControlPipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(ControlProtocol.ResponseTimeout);
            await pipe.ConnectAsync(timeout.Token).ConfigureAwait(false);
            ControlEnvelope request = new ControlEnvelope
            {
                MessageType = messageType,
                Hello = ControlProtocol.CreateHello(ControlClientKind.Gateway, "DisplayMagician.Gateway", "DisplayMagician Gateway"),
                Payload = payload
            };
            await ControlEnvelopeSerializer.WriteAsync(pipe, request, timeout.Token).ConfigureAwait(false);
            ControlEnvelope? envelope = await ControlEnvelopeSerializer.ReadAsync(pipe, timeout.Token).ConfigureAwait(false);
            ControlResponse? response = envelope == null ? null : JsonSerializer.Deserialize<ControlResponse>(envelope.Payload);
            return response ?? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "Control Service did not return a Gateway response." };
        }
        catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is OperationCanceledException || ex is JsonException)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "Control Service is unavailable." };
        }
    }
}
