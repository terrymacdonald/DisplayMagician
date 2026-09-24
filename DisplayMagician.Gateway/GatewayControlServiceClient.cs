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
    public Task<ControlResponse> ExecuteRemoteAsync(ControlMessageType messageType, GatewayAuthenticationResult authentication, string payload, CancellationToken cancellationToken) => SendAsync(messageType, JsonSerializer.Serialize(new GatewayRemoteCommand { Authentication = authentication, Payload = payload }), cancellationToken);
    public async Task<ControlResponse> ListRemoteAsync(ControlMessageType messageType, GatewayAuthenticationResult authentication, CancellationToken cancellationToken)
    {
        return await SendAsync(messageType, JsonSerializer.Serialize(authentication), cancellationToken).ConfigureAwait(false);
    }

    public async Task<RemoteUserStatus> GetRemoteUserStatusAsync(GatewayAuthenticationResult authentication, CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(ControlMessageType.GetRemoteUserStatus, JsonSerializer.Serialize(authentication), cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.RemoteUserStatus != null ? response.RemoteUserStatus : throw new InvalidOperationException(response.Message);
    }

    public async Task<GatewayAuthenticationResult> AuthenticateAsync(GatewayAuthenticationRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ControlResponse response = await SendAsync(ControlMessageType.AuthenticateGatewayRequest, JsonSerializer.Serialize(request), cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.GatewayAuthentication != null ? response.GatewayAuthentication : new GatewayAuthenticationResult { Message = string.IsNullOrWhiteSpace(response.Message) ? "Authentication could not be verified." : response.Message };
    }

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

    public async Task<DevicePairingResult> GetDevicePairingStatusAsync(DevicePairingStatusRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ControlResponse response = await SendAsync(ControlMessageType.GetDevicePairingStatus, JsonSerializer.Serialize(request), cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.DevicePairingResult != null ? response.DevicePairingResult : new DevicePairingResult { State = DevicePairingState.Rejected, Message = string.IsNullOrWhiteSpace(response.Message) ? "The pairing status could not be retrieved." : response.Message };
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
