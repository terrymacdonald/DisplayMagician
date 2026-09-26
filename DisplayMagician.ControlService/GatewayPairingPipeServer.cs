using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
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
    private const int MaximumConnectedClients = 16;
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly DevicePairingCoordinator _pairingCoordinator;
    private readonly GatewayIdentityRegistry _identityRegistry;
    private readonly GatewayRequestAuthenticator _requestAuthenticator;
    private readonly OperationStatusStore _operationStatusStore;
    private readonly OperationDecisionStore _operationDecisionStore;
    private readonly ProfileOperationRouter _profileOperationRouter;
    private readonly SemaphoreSlim _connectedClientSlots = new SemaphoreSlim(MaximumConnectedClients, MaximumConnectedClients);

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
            NamedPipeServerStream? pipe = null;
            try
            {
                pipe = CreatePipe();
                await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                if (!await _connectedClientSlots.WaitAsync(0, cancellationToken).ConfigureAwait(false))
                {
                    Logger.Warn("GatewayPairingPipeServer/RunAsync: Rejected a Gateway pipe request because the connected-client limit of {0} was reached.", MaximumConnectedClients);
                    pipe.Dispose();
                    continue;
                }

                _ = HandleWithSlotAsync(pipe, cancellationToken);
                pipe = null;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { pipe?.Dispose(); return; }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is InvalidOperationException)
            {
                pipe?.Dispose();
                Logger.Warn(ex, "GatewayPairingPipeServer/RunAsync: Gateway pipe request failed.");
            }
        }
    }

    private async Task HandleWithSlotAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        using (pipe)
        {
            try
            {
                await HandleAsync(pipe, cancellationToken).ConfigureAwait(false);
            }
            catch (EndOfStreamException ex)
            {
                Logger.Debug(ex, "GatewayPairingPipeServer/HandleWithSlotAsync: The Gateway closed the Control Service pipe before completing its request.");
            }
            catch (IOException ex)
            {
                Logger.Debug(ex, "GatewayPairingPipeServer/HandleWithSlotAsync: The Gateway Control Service pipe was disconnected during a request.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Logger.Warn(ex, "GatewayPairingPipeServer/HandleWithSlotAsync: Rejected an unauthorised Gateway pipe caller.");
            }
            catch (TimeoutException ex)
            {
                Logger.Warn(ex, "GatewayPairingPipeServer/HandleWithSlotAsync: The Gateway did not send a complete request before the timeout.");
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is JsonException)
            {
                Logger.Warn(ex, "GatewayPairingPipeServer/HandleWithSlotAsync: The Gateway sent an invalid Control Service request.");
            }
            finally
            {
                _connectedClientSlots.Release();
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
        using CancellationTokenSource timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(RequestTimeout);
        ControlEnvelope? request;
        try
        {
            request = await ControlEnvelopeSerializer.ReadAsync(pipe, timeoutSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && timeoutSource.IsCancellationRequested)
        {
            throw new TimeoutException("The Gateway did not send a complete Control Service request within 10 seconds.", ex);
        }
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
                ControlMessageType.ResolveRemoteOperationDecision => ResolveRemoteDecision(request),
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
        GatewayRemoteStatusRequest? statusRequest = JsonSerializer.Deserialize<GatewayRemoteStatusRequest>(request.Payload);
        GatewayAuthenticationResult? authentication = statusRequest?.Authentication;
        if (authentication == null || !authentication.IsAuthenticated || !authentication.GrantedCapabilities.Contains(RemoteClientCapabilities.StatusRead, StringComparer.Ordinal))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.Unauthorized, Message = "The paired device is not authorised to read status." };
        }

        DateTime cursor = statusRequest!.ChangedSinceUtc?.ToUniversalTime() ?? DateTime.MinValue;
        OperationStatus[] operations = cursor == DateTime.MinValue ? _operationStatusStore.GetAll(authentication.OwnerUserSid) : _operationStatusStore.GetChangedSince(authentication.OwnerUserSid, cursor);
        DateTime nextCursor = operations.Length == 0 ? cursor : operations.Max(status => status.UpdatedUtc);
        return new ControlResponse { IsSuccessful = true, RemoteUserStatus = new RemoteUserStatus { Operations = operations, PendingDecisions = _operationDecisionStore.GetPending(authentication.OwnerUserSid, 0), NextChangedSinceUtc = nextCursor } };
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

    private ControlResponse ResolveRemoteDecision(ControlEnvelope request)
    {
        GatewayRemoteCommand? command = JsonSerializer.Deserialize<GatewayRemoteCommand>(request.Payload);
        GatewayAuthenticationResult? authentication = command?.Authentication;
        ResolveOperationDecisionRequest? resolution = command == null ? null : JsonSerializer.Deserialize<ResolveOperationDecisionRequest>(command.Payload);
        if (authentication == null || !authentication.IsAuthenticated || !authentication.GrantedCapabilities.Contains(RemoteClientCapabilities.DecisionsAnswer, StringComparer.Ordinal)) return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.Unauthorized, Message = "The paired device is not authorised to answer decisions." };
        if (resolution == null || resolution.PromptId == Guid.Empty || resolution.Choice == OperationDecisionChoice.Unknown) return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "A valid operation decision is required." };
        OperationDecision? decision = _operationDecisionStore.Resolve(authentication.OwnerUserSid, 0, resolution.PromptId, resolution.Choice, DateTime.UtcNow);
        return decision == null ? new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.DecisionUnavailable, Message = "The operation decision is unavailable, expired, or already resolved." } : new ControlResponse { IsSuccessful = true, Message = "Operation decision recorded.", OperationDecision = decision };
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
