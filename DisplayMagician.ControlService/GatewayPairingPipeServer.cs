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
    private readonly GatewayRequestAuthenticator _requestAuthenticator;
    private readonly OperationStatusStore _operationStatusStore;
    private readonly OperationDecisionStore _operationDecisionStore;
    private readonly ProfileOperationRouter _profileOperationRouter;

    public GatewayPairingPipeServer(DevicePairingCoordinator pairingCoordinator, GatewayIdentityRegistry identityRegistry, GatewayRequestAuthenticator requestAuthenticator, OperationStatusStore operationStatusStore, OperationDecisionStore operationDecisionStore, ProfileOperationRouter profileOperationRouter)
    {
        _pairingCoordinator = pairingCoordinator ?? throw new ArgumentNullException(nameof(pairingCoordinator));
        _identityRegistry = identityRegistry ?? throw new ArgumentNullException(nameof(identityRegistry));
        _requestAuthenticator = requestAuthenticator ?? throw new ArgumentNullException(nameof(requestAuthenticator));
        _operationStatusStore = operationStatusStore ?? throw new ArgumentNullException(nameof(operationStatusStore));
        _operationDecisionStore = operationDecisionStore ?? throw new ArgumentNullException(nameof(operationDecisionStore));
        _profileOperationRouter = profileOperationRouter ?? throw new ArgumentNullException(nameof(profileOperationRouter));
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
                ControlMessageType.GetDevicePairingStatus => GetStatus(request),
                ControlMessageType.AuthenticateGatewayRequest => Authenticate(request),
                ControlMessageType.GetRemoteUserStatus => GetRemoteUserStatus(request),
                ControlMessageType.ListRemoteProfiles => ListRemote(request, RemoteClientCapabilities.ProfilesRead, ControlMessageType.ListProfiles),
                ControlMessageType.ListRemoteAudioProfiles => ListRemote(request, RemoteClientCapabilities.AudioProfilesRead, ControlMessageType.ListAudioProfiles),
                ControlMessageType.ListRemoteShortcuts => ListRemote(request, RemoteClientCapabilities.ShortcutsRead, ControlMessageType.ListShortcuts),
                ControlMessageType.ApplyRemoteProfile => ExecuteRemote(request, RemoteClientCapabilities.ProfilesApply, ControlMessageType.ApplyProfile),
                ControlMessageType.ApplyRemoteAudioProfile => ExecuteRemote(request, RemoteClientCapabilities.AudioProfilesApply, ControlMessageType.ApplyAudioProfile),
                ControlMessageType.StartRemoteShortcut => ExecuteRemote(request, RemoteClientCapabilities.ShortcutsRun, ControlMessageType.StartShortcut),
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

    private ControlResponse GetStatus(ControlEnvelope request)
    {
        DevicePairingStatusRequest? statusRequest = JsonSerializer.Deserialize<DevicePairingStatusRequest>(request.Payload);
        return statusRequest == null
            ? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The pairing status request is invalid." }
            : new ControlResponse { IsSuccessful = true, DevicePairingResult = _pairingCoordinator.GetStatus(statusRequest, DateTime.UtcNow) };
    }

    private ControlResponse Authenticate(ControlEnvelope request)
    {
        GatewayAuthenticationRequest? authenticationRequest = JsonSerializer.Deserialize<GatewayAuthenticationRequest>(request.Payload);
        return authenticationRequest == null
            ? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The Gateway authentication request is invalid." }
            : new ControlResponse { IsSuccessful = true, GatewayAuthentication = _requestAuthenticator.Authenticate(authenticationRequest, DateTime.UtcNow) };
    }

    private ControlResponse GetRemoteUserStatus(ControlEnvelope request)
    {
        GatewayAuthenticationResult? authentication = JsonSerializer.Deserialize<GatewayAuthenticationResult>(request.Payload);
        if (authentication == null || !authentication.IsAuthenticated || !authentication.GrantedCapabilities.Contains(RemoteClientCapabilities.StatusRead, StringComparer.Ordinal))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.Unauthorized, Message = "The paired device is not authorised to read status." };
        }

        return new ControlResponse { IsSuccessful = true, RemoteUserStatus = new RemoteUserStatus { Operations = _operationStatusStore.GetAll(authentication.OwnerUserSid), PendingDecisions = _operationDecisionStore.GetPending(authentication.OwnerUserSid, 0) } };
    }

    private ControlResponse ListRemote(ControlEnvelope request, string requiredCapability, ControlMessageType messageType)
    {
        GatewayAuthenticationResult? authentication = JsonSerializer.Deserialize<GatewayAuthenticationResult>(request.Payload);
        if (authentication == null || !authentication.IsAuthenticated || !authentication.GrantedCapabilities.Contains(requiredCapability, StringComparer.Ordinal)) return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.Unauthorized, Message = "The paired device is not authorised for this resource." };
        int sessionId = ConsoleSessionLocator.GetActiveConsoleSessionId();
        return messageType == ControlMessageType.ListProfiles
            ? _profileOperationRouter.ListProfilesAsync(authentication.OwnerUserSid, sessionId, CancellationToken.None).GetAwaiter().GetResult()
            : _profileOperationRouter.ManageProfileAsync(authentication.OwnerUserSid, sessionId, new ControlEnvelope { MessageType = messageType }, CancellationToken.None).GetAwaiter().GetResult();
    }

    private ControlResponse ExecuteRemote(ControlEnvelope request, string requiredCapability, ControlMessageType messageType)
    {
        GatewayRemoteCommand? command = JsonSerializer.Deserialize<GatewayRemoteCommand>(request.Payload);
        GatewayAuthenticationResult? authentication = command?.Authentication;
        if (authentication == null || !authentication.IsAuthenticated || !authentication.GrantedCapabilities.Contains(requiredCapability, StringComparer.Ordinal)) return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.Unauthorized, Message = "The paired device is not authorised for this action." };
        GatewayRemoteCommand validCommand = command!;
        int sessionId = ConsoleSessionLocator.GetActiveConsoleSessionId();
        ControlEnvelope agentRequest = new ControlEnvelope { MessageType = messageType, Payload = validCommand.Payload, RequestId = Guid.NewGuid() };
        ApplyProfileRequest? applyProfileRequest = messageType == ControlMessageType.ApplyProfile ? JsonSerializer.Deserialize<ApplyProfileRequest>(validCommand.Payload) : null;
        if (messageType == ControlMessageType.ApplyProfile && (applyProfileRequest == null || string.IsNullOrWhiteSpace(applyProfileRequest.ProfileId))) return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A display profile is required." };
        return messageType == ControlMessageType.ApplyProfile
            ? _profileOperationRouter.ApplyProfileAsync(authentication.OwnerUserSid, sessionId, applyProfileRequest!.ProfileId, applyProfileRequest.OperationId, agentRequest.RequestId, CancellationToken.None).GetAwaiter().GetResult()
            : _profileOperationRouter.ManageProfileAsync(authentication.OwnerUserSid, sessionId, agentRequest, CancellationToken.None).GetAwaiter().GetResult();
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

    public GatewayPairingIdentity? Get()
    {
        GatewayPairingIdentity? identity = _identity;
        return identity == null ? null : new GatewayPairingIdentity { GatewayUri = identity.GatewayUri, HostId = identity.HostId, HostIdentityPublicKeyJwk = identity.HostIdentityPublicKeyJwk, TlsCertificateSha256 = identity.TlsCertificateSha256 };
    }

    public void Set(GatewayPairingIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identity = new GatewayPairingIdentity { GatewayUri = identity.GatewayUri, HostId = identity.HostId, HostIdentityPublicKeyJwk = identity.HostIdentityPublicKeyJwk, TlsCertificateSha256 = identity.TlsCertificateSha256 };
    }
}
