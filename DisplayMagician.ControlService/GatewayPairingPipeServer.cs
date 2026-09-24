using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

/// <summary>Accepts only LocalService Gateway pairing traffic; it is deliberately not a general Control Service client pipe.</summary>
public sealed class GatewayPairingPipeServer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly DevicePairingCoordinator _pairingCoordinator;
    private readonly GatewayIdentityRegistry _identityRegistry;

    public GatewayPairingPipeServer(DevicePairingCoordinator pairingCoordinator, GatewayIdentityRegistry identityRegistry)
    {
        _pairingCoordinator = pairingCoordinator ?? throw new ArgumentNullException(nameof(pairingCoordinator));
        _identityRegistry = identityRegistry ?? throw new ArgumentNullException(nameof(identityRegistry));
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using NamedPipeServerStream pipe = CreatePipe();
                await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                await HandleAsync(pipe, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { return; }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is InvalidOperationException)
            {
                Logger.Warn(ex, "GatewayPairingPipeServer/RunAsync: Gateway pipe request failed.");
            }
        }
    }

    private static NamedPipeServerStream CreatePipe()
    {
        PipeSecurity security = new PipeSecurity();
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
        security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));
        return NamedPipeServerStreamAcl.Create(ControlProtocol.GatewayControlPipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 0, 0, security, HandleInheritability.None);
    }

    private async Task HandleAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        ControlEnvelope? request = await ControlEnvelopeSerializer.ReadAsync(pipe, cancellationToken).ConfigureAwait(false);
        ControlResponse response;
        if (request == null || !IsLocalService(pipe))
        {
            response = new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.Unauthorized, Message = "Only the Gateway service may use this pipe." };
        }
        else if (!ControlProtocol.TryCreateWelcome(request.Hello, "ControlService.Gateway", ControlProtocol.ControlServiceCapabilities, out ProtocolWelcome? welcome, out ControlErrorCode errorCode, out string message) || request.Hello.ClientKind != ControlClientKind.Gateway)
        {
            response = new ControlResponse { IsSuccessful = false, ErrorCode = errorCode == ControlErrorCode.None ? ControlErrorCode.InvalidRequest : errorCode, Message = message };
        }
        else
        {
            response = request.MessageType switch
            {
                ControlMessageType.GatewayRegistration => Register(request),
                ControlMessageType.SubmitDevicePairing => Submit(request),
                _ => new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The Gateway operation is not supported." }
            };
            response.ProtocolWelcome = welcome;
        }

        await ControlEnvelopeSerializer.WriteAsync(pipe, new ControlEnvelope { MessageType = request?.MessageType ?? ControlMessageType.Unknown, RequestId = request?.RequestId ?? Guid.NewGuid(), Payload = JsonSerializer.Serialize(response) }, cancellationToken).ConfigureAwait(false);
    }

    private ControlResponse Register(ControlEnvelope request)
    {
        GatewayRegistration? registration = JsonSerializer.Deserialize<GatewayRegistration>(request.Payload);
        if (registration?.Identity == null || string.IsNullOrWhiteSpace(registration.Identity.GatewayUri) || string.IsNullOrWhiteSpace(registration.Identity.HostId) || string.IsNullOrWhiteSpace(registration.Identity.HostIdentityPublicKeyJwk) || string.IsNullOrWhiteSpace(registration.Identity.TlsCertificateSha256))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The Gateway identity is invalid." };
        }

        _identityRegistry.Set(registration.Identity);
        return new ControlResponse { IsSuccessful = true, Message = "Gateway identity registered." };
    }

    private ControlResponse Submit(ControlEnvelope request)
    {
        DevicePairingRequest? pairingRequest = JsonSerializer.Deserialize<DevicePairingRequest>(request.Payload);
        if (pairingRequest == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The pairing request is invalid." };
        }

        return new ControlResponse { IsSuccessful = true, Message = "Pairing request processed.", DevicePairingResult = _pairingCoordinator.Submit(pairingRequest, DateTime.UtcNow) };
    }

    private static bool IsLocalService(NamedPipeServerStream pipe)
    {
        string? sid = null;
        pipe.RunAsClient(() => sid = WindowsIdentity.GetCurrent(TokenAccessLevels.Query).User?.Value);
        return string.Equals(sid, new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null).Value, StringComparison.Ordinal);
    }
}

public sealed class GatewayIdentityRegistry
{
    private GatewayPairingIdentity? _identity;
    public GatewayPairingIdentity? Get() => _identity;
    public void Set(GatewayPairingIdentity identity) => _identity = identity ?? throw new ArgumentNullException(nameof(identity));
}
