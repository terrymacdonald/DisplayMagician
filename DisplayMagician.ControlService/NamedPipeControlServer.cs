using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using Microsoft.Win32.SafeHandles;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class NamedPipeControlServer
{
    private const int MaximumConnectedClients = 16;
    private static readonly TimeSpan RegistrationTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan AgentMessageTimeout = TimeSpan.FromSeconds(45);
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly ControlStateCoordinator _coordinator;
    private readonly StoragePaths _storagePaths;
    private readonly UserDataMigrationRunner _userDataMigrationRunner;
    private readonly OperationStatusStore _operationStatusStore;
    private readonly ClientSyncCoordinator _clientSyncCoordinator;
    private readonly OperationDecisionStore _operationDecisionStore;
    private readonly SemaphoreSlim _connectedClientSlots = new SemaphoreSlim(MaximumConnectedClients, MaximumConnectedClients);

    public NamedPipeControlServer(ControlStateCoordinator coordinator, StoragePaths storagePaths, UserDataMigrationRunner userDataMigrationRunner, OperationStatusStore operationStatusStore, ClientSyncCoordinator clientSyncCoordinator, OperationDecisionStore operationDecisionStore)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _storagePaths = storagePaths ?? throw new ArgumentNullException(nameof(storagePaths));
        _userDataMigrationRunner = userDataMigrationRunner ?? throw new ArgumentNullException(nameof(userDataMigrationRunner));
        _operationStatusStore = operationStatusStore ?? throw new ArgumentNullException(nameof(operationStatusStore));
        _clientSyncCoordinator = clientSyncCoordinator ?? throw new ArgumentNullException(nameof(clientSyncCoordinator));
        _operationDecisionStore = operationDecisionStore ?? throw new ArgumentNullException(nameof(operationDecisionStore));
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
                    _logger.Warn("NamedPipeControlServer/RunAsync: Rejected a Control Service pipe client because the connected-client limit of {0} was reached.", MaximumConnectedClients);
                    pipe.Dispose();
                    continue;
                }

                _ = HandleClientWithSlotAsync(pipe, cancellationToken);
                pipe = null;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                pipe?.Dispose();
            }
            catch (Exception ex)
            {
                pipe?.Dispose();
                _logger.Error(ex, "NamedPipeControlServer/RunAsync: Unable to accept a Control Service pipe client.");
            }
        }
    }

    private async Task HandleClientWithSlotAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        try
        {
            await HandleClientAsync(pipe, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _connectedClientSlots.Release();
        }
    }

    private static NamedPipeServerStream CreatePipe()
    {
        PipeSecurity pipeSecurity = new PipeSecurity();
        pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
        pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null), PipeAccessRights.FullControl, AccessControlType.Allow));

        return NamedPipeServerStreamAcl.Create(
            ControlProtocol.ServicePipeName,
            PipeDirection.InOut,
            NamedPipeServerStream.MaxAllowedServerInstances,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous,
            0,
            0,
            pipeSecurity,
            HandleInheritability.None);
    }

    private async Task HandleClientAsync(NamedPipeServerStream pipe, CancellationToken cancellationToken)
    {
        using (pipe)
        {
            PipeClientIdentity? registeredIdentity = null;
            try
            {
                ControlEnvelope? envelope = await ReadEnvelopeWithTimeoutAsync(pipe, RegistrationTimeout, cancellationToken).ConfigureAwait(false);
                if (envelope == null)
                {
                    return;
                }

                PipeClientIdentity identity = GetClientIdentity(pipe);

                if (envelope.ProtocolVersion != ControlProtocol.CurrentVersion)
                {
                    await SendResultAsync(pipe, envelope.MessageType, envelope.RequestId, false, ControlErrorCode.UnsupportedProtocolVersion, "The client uses an unsupported protocol version.", cancellationToken).ConfigureAwait(false);
                    return;
                }

                if (!ControlProtocol.TryCreateWelcome(envelope.Hello, "ControlService", ControlProtocol.ControlServiceCapabilities, out ProtocolWelcome? registrationWelcome, out ControlErrorCode registrationNegotiationError, out string registrationNegotiationMessage))
                {
                    await SendResultAsync(pipe, envelope.MessageType, envelope.RequestId, false, registrationNegotiationError, registrationNegotiationMessage, cancellationToken).ConfigureAwait(false);
                    return;
                }

                if (envelope.MessageType != ControlMessageType.AgentRegistration)
                {
                    await SendResultAsync(pipe, envelope.MessageType, envelope.RequestId, false, ControlErrorCode.InvalidRequest, "The first pipe message must be an Agent registration.", cancellationToken).ConfigureAwait(false);
                    return;
                }

                AgentRegistration? registration = JsonSerializer.Deserialize<AgentRegistration>(envelope.Payload);
                if (registration == null || !string.Equals(registration.UserSid, identity.UserSid, StringComparison.OrdinalIgnoreCase) || registration.SessionId != identity.SessionId || registration.ProcessId != identity.ProcessId || !IsInstalledUserAgent(identity, registration))
                {
                    _logger.Warn("NamedPipeControlServer/HandleClientAsync: Rejected Agent registration because its claimed identity did not match the Windows pipe client. ProcessId={0}", identity.ProcessId);
                    await SendResultAsync(pipe, envelope.MessageType, envelope.RequestId, false, ControlErrorCode.CallerIdentityMismatch, "The claimed Agent identity does not match the Windows pipe client.", cancellationToken).ConfigureAwait(false);
                    return;
                }

                _coordinator.RegisterAgent(registration, DateTime.UtcNow);
                registeredIdentity = identity;
                _logger.Info("NamedPipeControlServer/HandleClientAsync: Registered User Agent for SID {0}, session {1}, process {2}.", identity.UserSid, identity.SessionId, identity.ProcessId);
                await SendResultAsync(pipe, envelope.MessageType, envelope.RequestId, true, ControlErrorCode.None, "Agent registration accepted.", cancellationToken, protocolWelcome: registrationWelcome).ConfigureAwait(false);
                try
                {
                    await _clientSyncCoordinator.SendLatestManifestAsync(registration, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is InvalidOperationException || ex is IOException || ex is TimeoutException)
                {
                    _logger.Warn(ex, "NamedPipeControlServer/HandleClientAsync: Could not deliver the latest client-sync messages to Agent SID {0}, session {1}.", identity.UserSid, identity.SessionId);
                }

                while (!cancellationToken.IsCancellationRequested && pipe.IsConnected)
                {
                    ControlEnvelope? request = await ReadEnvelopeWithTimeoutAsync(pipe, AgentMessageTimeout, cancellationToken).ConfigureAwait(false);
                    if (request == null)
                    {
                        return;
                    }

                    if (request.ProtocolVersion != ControlProtocol.CurrentVersion)
                    {
                        await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.UnsupportedProtocolVersion, "The client uses an unsupported protocol version.", cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    if (!ControlProtocol.TryCreateWelcome(request.Hello, "ControlService", ControlProtocol.ControlServiceCapabilities, out _, out ControlErrorCode negotiationError, out string negotiationMessage))
                    {
                        await SendResultAsync(pipe, request.MessageType, request.RequestId, false, negotiationError, negotiationMessage, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    await HandleAgentMessageAsync(pipe, identity, request, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Service shutdown is expected and does not require an error log.
            }
            catch (TimeoutException ex)
            {
                _logger.Warn(ex, "NamedPipeControlServer/HandleClientAsync: Pipe client did not register or send a heartbeat before its deadline.");
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "NamedPipeControlServer/HandleClientAsync: Pipe client processing failed.");
            }
            finally
            {
                if (registeredIdentity != null)
                {
                    _coordinator.UnregisterAgent(registeredIdentity.UserSid, registeredIdentity.SessionId, registeredIdentity.ProcessId);
                    _logger.Info("NamedPipeControlServer/HandleClientAsync: User Agent disconnected for SID {0}, session {1}, process {2}.", registeredIdentity.UserSid, registeredIdentity.SessionId, registeredIdentity.ProcessId);
                }
            }
        }
    }

    private static async Task<ControlEnvelope?> ReadEnvelopeWithTimeoutAsync(NamedPipeServerStream pipe, TimeSpan timeout, CancellationToken serviceCancellationToken)
    {
        using CancellationTokenSource timeoutCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(serviceCancellationToken);
        timeoutCancellationTokenSource.CancelAfter(timeout);
        try
        {
            return await ControlEnvelopeSerializer.ReadAsync(pipe, timeoutCancellationTokenSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!serviceCancellationToken.IsCancellationRequested && timeoutCancellationTokenSource.IsCancellationRequested)
        {
            throw new TimeoutException($"The pipe client did not send a complete message within {timeout.TotalSeconds:0} seconds.", ex);
        }
    }

    private async Task HandleAgentMessageAsync(NamedPipeServerStream pipe, PipeClientIdentity identity, ControlEnvelope request, CancellationToken cancellationToken)
    {
        using IDisposable requestScope = SupportLogScope.BeginRequest(request.RequestId);
        if (request.MessageType == ControlMessageType.AgentHeartbeat)
        {
            AgentHeartbeat? heartbeat = JsonSerializer.Deserialize<AgentHeartbeat>(request.Payload);
            if (heartbeat == null)
            {
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.InvalidRequest, "The Agent heartbeat was invalid.", cancellationToken).ConfigureAwait(false);
                return;
            }

            _coordinator.RecordHeartbeat(identity.UserSid, identity.SessionId, heartbeat.OperationState, heartbeat.IsRecoveryRequired, DateTime.UtcNow);
            await SendResultAsync(pipe, request.MessageType, request.RequestId, true, ControlErrorCode.None, "Agent heartbeat recorded.", cancellationToken, _coordinator.GetStatus(DateTime.UtcNow)).ConfigureAwait(false);
            return;
        }

        if (request.MessageType == ControlMessageType.OperationProgress || request.MessageType == ControlMessageType.OperationCompleted)
        {
            OperationStatusUpdate? update = JsonSerializer.Deserialize<OperationStatusUpdate>(request.Payload);
            if (update == null || update.OperationId == Guid.Empty)
            {
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.InvalidRequest, "The operation status update was invalid.", cancellationToken).ConfigureAwait(false);
                return;
            }

            if (request.MessageType == ControlMessageType.OperationCompleted)
            {
                update.IsTerminal = true;
            }

            using IDisposable operationScope = SupportLogScope.BeginOperation(update.OperationId);
            try
            {
                OperationStatus status = _operationStatusStore.Publish(identity.UserSid, identity.SessionId, update, DateTime.UtcNow);
                await SendResultAsync(pipe, request.MessageType, request.RequestId, true, ControlErrorCode.None, "Operation status recorded.", cancellationToken, operationStatus: status).ConfigureAwait(false);
            }
            catch (InvalidOperationException ex)
            {
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.CallerIdentityMismatch, ex.Message, cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        if (request.MessageType == ControlMessageType.ReconcileOperationStatuses)
        {
            OperationStatusReconciliationRequest? reconciliation = JsonSerializer.Deserialize<OperationStatusReconciliationRequest>(request.Payload);
            if (reconciliation == null || reconciliation.ActiveUpdates == null || Array.Exists(reconciliation.ActiveUpdates, update => update == null || update.OperationId == Guid.Empty))
            {
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.InvalidRequest, "The active operation reconciliation was invalid.", cancellationToken).ConfigureAwait(false);
                return;
            }

            foreach (OperationStatusUpdate update in reconciliation.ActiveUpdates!)
            {
                _operationStatusStore.Publish(identity.UserSid, identity.SessionId, update, DateTime.UtcNow);
            }
            await SendResultAsync(pipe, request.MessageType, request.RequestId, true, ControlErrorCode.None, "Active operation statuses reconciled.", cancellationToken).ConfigureAwait(false);
            return;
        }

        if (request.MessageType == ControlMessageType.RequestOperationDecision)
        {
            RequestOperationDecisionRequest? decisionRequest = JsonSerializer.Deserialize<RequestOperationDecisionRequest>(request.Payload);
            if (decisionRequest == null || decisionRequest.OperationId == Guid.Empty || decisionRequest.AllowedChoices.Length == 0 || Array.IndexOf(decisionRequest.AllowedChoices, decisionRequest.DefaultChoice) < 0)
            {
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.InvalidRequest, "The operation decision request was invalid.", cancellationToken).ConfigureAwait(false);
                return;
            }

            DateTime now = DateTime.UtcNow;
            int timeoutSeconds = Math.Clamp(decisionRequest.TimeoutSeconds, 5, 300);
            OperationDecision decision = _operationDecisionStore.Create(identity.UserSid, identity.SessionId, decisionRequest.OperationId, decisionRequest.Title, decisionRequest.Message, decisionRequest.AllowedChoices, decisionRequest.DefaultChoice, now.AddSeconds(timeoutSeconds), now);
            _operationStatusStore.Publish(identity.UserSid, identity.SessionId, new OperationStatusUpdate { OperationId = decisionRequest.OperationId, OperationType = decisionRequest.OperationType, Phase = OperationPhase.AwaitingUserDecision, Message = decisionRequest.Message }, now);
            await SendResultAsync(pipe, request.MessageType, request.RequestId, true, ControlErrorCode.None, "Operation decision created.", cancellationToken, operationDecision: decision).ConfigureAwait(false);
            return;
        }

        if (request.MessageType == ControlMessageType.GetOperationDecision)
        {
            OperationDecisionStatusRequest? decisionStatusRequest = JsonSerializer.Deserialize<OperationDecisionStatusRequest>(request.Payload);
            if (decisionStatusRequest == null || decisionStatusRequest.PromptId == Guid.Empty)
            {
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.InvalidRequest, "An operation decision prompt ID is required.", cancellationToken).ConfigureAwait(false);
                return;
            }

            _operationDecisionStore.Expire(DateTime.UtcNow);
            OperationDecision? decision = _operationDecisionStore.Get(identity.UserSid, decisionStatusRequest.PromptId);
            if (decision == null)
            {
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.DecisionUnavailable, "The operation decision is unavailable.", cancellationToken).ConfigureAwait(false);
                return;
            }

            await SendResultAsync(pipe, request.MessageType, request.RequestId, true, ControlErrorCode.None, decision.IsResolved ? "Operation decision resolved." : "Operation decision is pending.", cancellationToken, operationDecision: decision).ConfigureAwait(false);
            return;
        }

        if (request.MessageType == ControlMessageType.AcquireDisplayControl)
        {
            LeaseDecision decision;
            try
            {
                decision = _coordinator.TryAcquireDisplayControl(identity.UserSid, identity.SessionId, ConsoleSessionLocator.GetActiveConsoleSessionId(), DateTime.UtcNow);
            }
            catch (InvalidOperationException ex)
            {
                decision = new LeaseDecision
                {
                    ErrorCode = ControlErrorCode.NotActiveConsoleUser,
                    Message = ex.Message
                };
            }

            await SendResultAsync(pipe, request.MessageType, request.RequestId, decision.IsGranted, decision.ErrorCode, decision.Message, cancellationToken, leaseDecision: decision).ConfigureAwait(false);
            return;
        }

        if (request.MessageType == ControlMessageType.GetServiceStatus)
        {
            await SendResultAsync(pipe, request.MessageType, request.RequestId, true, ControlErrorCode.None, "Service status returned.", cancellationToken, _coordinator.GetStatus(DateTime.UtcNow)).ConfigureAwait(false);
            return;
        }

        if (request.MessageType == ControlMessageType.MigrateUserData)
        {
            string? legacyAppDataPath = null;
            pipe.RunAsClient(() => legacyAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DisplayMagician"));
            if (string.IsNullOrWhiteSpace(legacyAppDataPath))
            {
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.InvalidRequest, "The User Agent's legacy application-data path could not be determined.", cancellationToken).ConfigureAwait(false);
                return;
            }

            UserDataMigrationResult migrationResult;
            try
            {
                UserStoragePaths userPaths = _storagePaths.ProvisionUserStorage(identity.UserSid);
                migrationResult = _userDataMigrationRunner.Migrate(legacyAppDataPath, userPaths);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
            {
                _logger.Error(ex, "NamedPipeControlServer/HandleAgentMessageAsync: Could not provision v4 storage for SID {0}.", identity.UserSid);
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.InvalidRequest, "DisplayMagician could not provision writable storage for this user.", cancellationToken).ConfigureAwait(false);
                return;
            }

            if (migrationResult.IsSuccessful)
            {
                _logger.Info("NamedPipeControlServer/HandleAgentMessageAsync: Completed user-data migration for SID {0} from {1}.", identity.UserSid, legacyAppDataPath);
                await SendResultAsync(pipe, request.MessageType, request.RequestId, true, ControlErrorCode.None, migrationResult.Message, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                _logger.Error("NamedPipeControlServer/HandleAgentMessageAsync: User-data migration failed for SID {0}. {1}", identity.UserSid, migrationResult.Message);
                await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.InvalidRequest, migrationResult.Message, cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        await SendResultAsync(pipe, request.MessageType, request.RequestId, false, ControlErrorCode.InvalidRequest, "The Agent message type is not supported.", cancellationToken).ConfigureAwait(false);
    }

    private static PipeClientIdentity GetClientIdentity(NamedPipeServerStream pipe)
    {
        string? userSid = null;
        pipe.RunAsClient(() =>
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent(TokenAccessLevels.Query);
            userSid = identity.User?.Value;
        });

        // Windows does not provide a client process ID for a remote named-pipe client, so this also rejects remote callers.
        if (string.IsNullOrWhiteSpace(userSid) || !GetNamedPipeClientProcessId(pipe.SafePipeHandle, out uint processId))
        {
            throw new UnauthorizedAccessException("The Control Service could not verify the named-pipe client identity.");
        }

        using Process process = Process.GetProcessById(checked((int)processId));
        return new PipeClientIdentity(userSid, process.SessionId, checked((int)processId));
    }

    private static bool IsInstalledUserAgent(PipeClientIdentity identity, AgentRegistration registration)
    {
        if (!string.Equals(registration.CommandPipeName, $"{ControlProtocol.AgentCommandPipePrefix}{identity.ProcessId}", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            using Process process = Process.GetProcessById(identity.ProcessId);
            string expectedPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "UserAgent", "DisplayMagician.UserAgent.exe"));
            return string.Equals(process.MainModule?.FileName, expectedPath, StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is UnauthorizedAccessException)
        {
            _logger.Warn(ex, "NamedPipeControlServer/IsInstalledUserAgent: Could not verify the User Agent executable for pipe process {0}.", identity.ProcessId);
            return false;
        }
    }

    private static Task SendResultAsync(NamedPipeServerStream pipe, ControlMessageType messageType, Guid requestId, bool isSuccessful, ControlErrorCode errorCode, string message, CancellationToken cancellationToken, ControlServiceStatus? serviceStatus = null, LeaseDecision? leaseDecision = null, OperationStatus? operationStatus = null, OperationDecision? operationDecision = null, ProtocolWelcome? protocolWelcome = null)
    {
        ControlEnvelope response = new ControlEnvelope
        {
            MessageType = messageType,
            RequestId = requestId,
            Payload = JsonSerializer.Serialize(new ControlResponse
            {
                IsSuccessful = isSuccessful,
                ErrorCode = errorCode,
                Message = message,
                ServiceStatus = serviceStatus,
                LeaseDecision = leaseDecision,
                OperationStatus = operationStatus,
                OperationDecision = operationDecision,
                ProtocolWelcome = protocolWelcome ?? new ProtocolWelcome { SelectedProtocolVersion = ControlProtocol.CurrentVersion, EndpointKind = "ControlService", ServiceInstanceId = ControlProtocol.EndpointInstanceId, SupportedCapabilities = ControlProtocol.ControlServiceCapabilities }
            })
        };

        return ControlEnvelopeSerializer.WriteAsync(pipe, response, cancellationToken);
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetNamedPipeClientProcessId(SafePipeHandle pipe, out uint clientProcessId);

    private sealed class PipeClientIdentity
    {
        public PipeClientIdentity(string userSid, int sessionId, int processId)
        {
            UserSid = userSid;
            SessionId = sessionId;
            ProcessId = processId;
        }

        public string UserSid { get; }

        public int SessionId { get; }

        public int ProcessId { get; }
    }

}
