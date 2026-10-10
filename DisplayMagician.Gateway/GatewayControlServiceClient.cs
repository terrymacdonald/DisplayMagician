using System;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.AspNetCore.Http;

namespace DisplayMagician.Gateway;

public interface IGatewayAuthenticationClient
{
    Task<GatewayAuthenticationResult> AuthenticateAsync(GatewayAuthenticationRequest request, CancellationToken cancellationToken);
}

internal sealed class GatewayControlServiceUnavailableException : Exception
{
    public GatewayControlServiceUnavailableException() : base("Control Service could not verify the Gateway request.") { }
}

/// <summary>Uses the narrow LocalService-only pipe; it cannot invoke normal desktop client operations.</summary>
public sealed class GatewayControlServiceClient : IGatewayAuthenticationClient
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GatewayControlServiceClient(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public Task<ControlResponse> ExecuteRemoteAsync(ControlMessageType messageType, GatewayAuthenticationResult authentication, int? targetSessionId, string payload, CancellationToken cancellationToken) => SendAsync(messageType, JsonSerializer.Serialize(new GatewayRemoteCommand { Authentication = authentication, TargetSessionId = targetSessionId, Payload = payload }), cancellationToken);
    public Task<ControlResponse> RemoteResourceAsync(ControlMessageType messageType, GatewayAuthenticationResult authentication, string payload, CancellationToken cancellationToken) => ExecuteRemoteAsync(messageType, authentication, null, payload, cancellationToken);
    public async Task<ControlResponse> ListRemoteAsync(ControlMessageType messageType, GatewayAuthenticationResult authentication, int? targetSessionId, CancellationToken cancellationToken)
    {
        return await SendAsync(messageType, JsonSerializer.Serialize(new GatewayRemoteCommand { Authentication = authentication, TargetSessionId = targetSessionId }), cancellationToken).ConfigureAwait(false);
    }

    public async Task<RemoteUserStatus> GetRemoteUserStatusAsync(GatewayAuthenticationResult authentication, int? targetSessionId, DateTime? changedSinceUtc, CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(ControlMessageType.GetRemoteUserStatus, JsonSerializer.Serialize(new GatewayRemoteStatusRequest { Authentication = authentication, TargetSessionId = targetSessionId, ChangedSinceUtc = changedSinceUtc }), cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.RemoteUserStatus != null ? response.RemoteUserStatus : throw new InvalidOperationException(response.Message);
    }

    public async Task<GatewayAuthenticationResult> AuthenticateAsync(GatewayAuthenticationRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ControlResponse response = await SendAsync(ControlMessageType.AuthenticateGatewayRequest, JsonSerializer.Serialize(request), cancellationToken).ConfigureAwait(false);
        if (response.ErrorCode == ControlErrorCode.AgentUnavailable || response.IsSuccessful && response.GatewayAuthentication == null)
        {
            throw new GatewayControlServiceUnavailableException();
        }
        return response.IsSuccessful && response.GatewayAuthentication != null ? response.GatewayAuthentication : new GatewayAuthenticationResult { Message = string.IsNullOrWhiteSpace(response.Message) ? "Authentication could not be verified." : response.Message };
    }

    public async Task<GatewayHttpIdempotencyResult> BeginIdempotencyAsync(GatewayHttpIdempotencyRequest request, CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(ControlMessageType.GatewayIdempotencyBegin, JsonSerializer.Serialize(request), cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.GatewayIdempotency != null ? response.GatewayIdempotency : throw new InvalidOperationException("Control Service could not reserve the idempotency key.");
    }

    public async Task CompleteIdempotencyAsync(GatewayHttpIdempotencyRequest request, CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(ControlMessageType.GatewayIdempotencyComplete, JsonSerializer.Serialize(request), cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessful) throw new InvalidOperationException("Control Service could not retain the idempotency result.");
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
            : new DevicePairingResult { State = response.ErrorCode == ControlErrorCode.AgentUnavailable ? DevicePairingState.Unknown : DevicePairingState.Rejected, ProblemCode = response.ErrorCode == ControlErrorCode.AgentUnavailable ? "target-unavailable" : "validation-failed", Message = string.IsNullOrWhiteSpace(response.Message) ? "The pairing request could not be processed." : response.Message };
    }

    public async Task<DevicePairingState> ValidatePairingSubmissionAsync(DevicePairingRequest request, CancellationToken cancellationToken)
    {
        ControlResponse response = await SendAsync(ControlMessageType.ValidateDevicePairingSession, JsonSerializer.Serialize(request), cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful ? response.DevicePairingResult?.State ?? DevicePairingState.Unknown : DevicePairingState.Unknown;
    }

    public async Task<DevicePairingResult> GetDevicePairingStatusAsync(DevicePairingStatusRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ControlResponse response = await SendAsync(ControlMessageType.GetDevicePairingStatus, JsonSerializer.Serialize(request), cancellationToken).ConfigureAwait(false);
        return response.IsSuccessful && response.DevicePairingResult != null ? response.DevicePairingResult : new DevicePairingResult { State = response.ErrorCode == ControlErrorCode.AgentUnavailable ? DevicePairingState.Unknown : DevicePairingState.Rejected, ProblemCode = response.ErrorCode == ControlErrorCode.AgentUnavailable ? "target-unavailable" : "validation-failed", Message = string.IsNullOrWhiteSpace(response.Message) ? "The pairing status could not be retrieved." : response.Message };
    }

    private async Task<ControlResponse> SendAsync(ControlMessageType messageType, string payload, CancellationToken cancellationToken)
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
            if (Guid.TryParse(_httpContextAccessor.HttpContext?.Response.Headers[GatewayProtocolMiddleware.RequestIdHeaderName], out Guid requestId))
            {
                request.RequestId = requestId;
            }
            await ControlEnvelopeSerializer.WriteAsync(pipe, request, timeout.Token).ConfigureAwait(false);
            ControlEnvelope? envelope = await ControlEnvelopeSerializer.ReadAsync(pipe, timeout.Token).ConfigureAwait(false);
            ControlResponse? response = envelope == null ? null : JsonSerializer.Deserialize<ControlResponse>(envelope.Payload);
            return response ?? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "Control Service did not return a Gateway response." };
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is TimeoutException || ex is OperationCanceledException || ex is JsonException)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "Control Service is unavailable." };
        }
    }
}
