using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;
using NLog;

namespace DisplayMagician.ControlService;

public sealed class ProfileOperationRouter
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly ControlStateCoordinator _coordinator;
    private readonly IAgentCommandClient _agentCommandClient;
    private readonly ISessionLauncherClient _sessionLauncherClient;
    private readonly RecoveryAdministrationStore? _recoveryAdministrationStore;
    private readonly MachineDiagnosticLogLevelStore? _machineDiagnosticLogLevelStore;
    private readonly GatewayEventJournal? _gatewayEventJournal;
    private readonly SemaphoreSlim _agentLaunchLock = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim _savedResourceMutationLock = new SemaphoreSlim(1, 1);

    public ProfileOperationRouter(ControlStateCoordinator coordinator, IAgentCommandClient agentCommandClient)
        : this(coordinator, agentCommandClient, new UnavailableSessionLauncherClient())
    {
    }

    public ProfileOperationRouter(ControlStateCoordinator coordinator, IAgentCommandClient agentCommandClient, ISessionLauncherClient sessionLauncherClient)
        : this(coordinator, agentCommandClient, sessionLauncherClient, (RecoveryAdministrationStore?)null)
    {
    }

    public ProfileOperationRouter(ControlStateCoordinator coordinator, IAgentCommandClient agentCommandClient, ISessionLauncherClient sessionLauncherClient, Func<int> ignoredActiveConsoleSessionId)
        : this(coordinator, agentCommandClient, sessionLauncherClient)
    {
    }

    public ProfileOperationRouter(ControlStateCoordinator coordinator, IAgentCommandClient agentCommandClient, ISessionLauncherClient sessionLauncherClient, RecoveryAdministrationStore? recoveryAdministrationStore = null)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _agentCommandClient = agentCommandClient ?? throw new ArgumentNullException(nameof(agentCommandClient));
        _sessionLauncherClient = sessionLauncherClient ?? throw new ArgumentNullException(nameof(sessionLauncherClient));
        _recoveryAdministrationStore = recoveryAdministrationStore;
    }

    public ProfileOperationRouter(ControlStateCoordinator coordinator, IAgentCommandClient agentCommandClient, ISessionLauncherClient sessionLauncherClient, RecoveryAdministrationStore? recoveryAdministrationStore, MachineDiagnosticLogLevelStore machineDiagnosticLogLevelStore, GatewayEventJournal? gatewayEventJournal = null)
        : this(coordinator, agentCommandClient, sessionLauncherClient, recoveryAdministrationStore)
    {
        _machineDiagnosticLogLevelStore = machineDiagnosticLogLevelStore ?? throw new ArgumentNullException(nameof(machineDiagnosticLogLevelStore));
        _gatewayEventJournal = gatewayEventJournal;
    }

    public Task<ControlResponse> ListProfilesAsync(string userSid, int sessionId, CancellationToken cancellationToken)
    {
        return SendToAgentAsync(userSid, sessionId, new ControlEnvelope
        {
            MessageType = ControlMessageType.ListProfiles,
            Payload = JsonSerializer.Serialize(new ProfileListRequest())
        }, cancellationToken);
    }

    public Task<ControlResponse> ListProfilesAsync(string userSid, int sessionId, ControlEnvelope request, CancellationToken cancellationToken)
    {
        return SendToAgentAsync(userSid, sessionId, request, cancellationToken);
    }

    public Task<ControlResponse> StopAgentIfIdleAsync(string userSid, int sessionId, CancellationToken cancellationToken)
    {
        return SendToAgentAsync(userSid, sessionId, new ControlEnvelope { MessageType = ControlMessageType.StopAgentIfIdle }, false, cancellationToken);
    }

    public async Task<ControlResponse> RestartUserAgentAsync(string userSid, int sessionId, Guid requestId, CancellationToken cancellationToken)
    {
        try
        {
            return await RestartUserAgentCoreAsync(userSid, sessionId, requestId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is SessionLauncherUnavailableException || ex is IOException || ex is InvalidDataException || ex is TimeoutException || ex is UnauthorizedAccessException)
        {
            Logger.Warn(ex, "ProfileOperationRouter/RestartUserAgentAsync: Could not restart User Agent for SID {0}, session {1}.", userSid, sessionId);
            RecordRestart(userSid, sessionId, "Failed");
            return new ControlResponse { ErrorCode = ControlErrorCode.OutcomeUnknown, Message = "The Session Launcher could not confirm the User Agent restart. No replacement was started after an unconfirmed stop." };
        }
    }

    private async Task<ControlResponse> RestartUserAgentCoreAsync(string userSid, int sessionId, Guid requestId, CancellationToken cancellationToken)
    {
        AgentRegistration? existingAgent = _coordinator.GetAgentRegistration(userSid, sessionId);
        if (existingAgent != null)
        {
            ControlResponse stopResponse = await SendToAgentAsync(userSid, sessionId, new ControlEnvelope { MessageType = ControlMessageType.StopAgentIfIdle, RequestId = requestId }, false, cancellationToken).ConfigureAwait(false);
            if (!stopResponse.IsSuccessful)
            {
                RecordRestart(userSid, sessionId, "Failed");
                return stopResponse;
            }

            DateTime stopDeadlineUtc = DateTime.UtcNow.AddSeconds(10);
            while (_coordinator.GetAgentRegistration(userSid, sessionId) != null && DateTime.UtcNow < stopDeadlineUtc)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
            }

            if (_coordinator.GetAgentRegistration(userSid, sessionId) != null)
            {
                UserAgentLaunchResult forcedStopResult = await _sessionLauncherClient.StopUserAgentAsync(userSid, sessionId, existingAgent.ProcessId, requestId, cancellationToken).ConfigureAwait(false);
                if (!forcedStopResult.IsSuccessful)
                {
                    RecordRestart(userSid, sessionId, "Failed");
                    return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = "The User Agent did not stop in time and could not be safely replaced." };
                }

                _coordinator.UnregisterAgent(userSid, sessionId, existingAgent.ProcessId);
            }
        }

        AgentRegistration? restartedAgent = await GetOrStartAgentAsync(userSid, sessionId, requestId, null, cancellationToken).ConfigureAwait(false);
        if (restartedAgent == null)
        {
            RecordRestart(userSid, sessionId, "Failed");
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent could not be started for this session." };
        }

        RecordRestart(userSid, sessionId, "Succeeded");
        return new ControlResponse { IsSuccessful = true, Message = "The User Agent was restarted and is ready." };
    }

    private void RecordRestart(string userSid, int sessionId, string outcome)
    {
        _recoveryAdministrationStore?.Record("RestartUserAgent", outcome, userSid, sessionId);
    }

    public async Task<ControlResponse> ManageProfileAsync(string userSid, int sessionId, ControlEnvelope request, CancellationToken cancellationToken)
    {
        RepositoryKind kind = request.MessageType switch
        {
            ControlMessageType.CreateProfileFromCurrent or ControlMessageType.RenameProfile or ControlMessageType.DeleteProfile or ControlMessageType.UpdateProfileFromCurrent or ControlMessageType.UpdateDisplayProfileSettings => RepositoryKind.DisplayProfiles,
            ControlMessageType.CreateAudioProfileFromCurrent or ControlMessageType.RenameAudioProfile or ControlMessageType.DeleteAudioProfile or ControlMessageType.UpdateAudioProfileFromCurrent => RepositoryKind.AudioProfiles,
            ControlMessageType.CommitRepositorySnapshot => JsonSerializer.Deserialize<RepositoryCommitRequest>(request.Payload)?.Repository ?? RepositoryKind.Unknown,
            _ => RepositoryKind.Unknown
        };
        if (request.MessageType == ControlMessageType.CommitRepositorySnapshot && kind is not (RepositoryKind.DisplayProfiles or RepositoryKind.AudioProfiles or RepositoryKind.Shortcuts))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "The saved collection type is invalid." };
        }
        if (_gatewayEventJournal == null || kind == RepositoryKind.Unknown) return await SendToAgentAsync(userSid, sessionId, request, cancellationToken).ConfigureAwait(false);

        await _savedResourceMutationLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            RepositoryCommitRequest? commit = request.MessageType == ControlMessageType.CommitRepositorySnapshot ? JsonSerializer.Deserialize<RepositoryCommitRequest>(request.Payload) : null;
            RepositorySnapshot? before = null;
            if (commit != null)
            {
                ControlResponse snapshotResponse = await SendToAgentAsync(userSid, sessionId,
                    new ControlEnvelope { MessageType = ControlMessageType.GetRepositorySnapshot, Payload = JsonSerializer.Serialize(new RepositorySnapshotRequest { Repository = kind }) }, cancellationToken).ConfigureAwait(false);
                if (!snapshotResponse.IsSuccessful) return snapshotResponse;
                before = snapshotResponse.RepositorySnapshot;
                if (before == null || before.Repository != kind) return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The saved collection could not be read before the edit. Retry the edit." };
                try
                {
                    SavedResourceChangeDetector.Validate(kind, before.Json);
                    SavedResourceChangeDetector.Validate(kind, commit.Json);
                }
                catch (JsonException)
                {
                    return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ValidationFailed, Message = "The saved collection could not be validated before the edit." };
                }
            }

            GatewayEventJournal.SavedResourceEdit edit;
            try { edit = _gatewayEventJournal.BeginSavedResourceEdit(userSid); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Logger.Error(ex, "ProfileOperationRouter/ManageProfileAsync: Could not prepare saved-resource event recovery for {0}.", request.MessageType);
                return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The saved edit cannot be safely recorded now. Retry the edit." };
            }
            using (edit)
            {
                ControlResponse response = await SendToAgentAsync(userSid, sessionId, request, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessful) return response;
                if (response.RepositoryCommit?.WasConflict == true) { edit.Complete(); return response; }

                try
                {
                    GatewaySavedResourceChange[] changes = commit != null
                        ? SavedResourceChangeDetector.Detect(kind, before!.Json, commit.Json)
                        : response.SavedResourceChanges.Where(change => change.Repository == kind)
                            .Select(change => SavedResourceChangeDetector.Create(change.Repository, change.ResourceId, change.Action)).ToArray();
                    if (commit == null && changes.Length == 0)
                    {
                        Logger.Error("ProfileOperationRouter/ManageProfileAsync: User Agent reported no saved-resource change for successful {0} in SID {1}, session {2}.", request.MessageType, userSid, sessionId);
                    }
                    else
                    {
                        _gatewayEventJournal.AppendBatch(userSid, sessionId, changes, DateTime.UtcNow);
                        edit.Complete();
                    }
                }
                catch (Exception ex) when (ex is ArgumentException or JsonException or IOException or UnauthorizedAccessException)
                {
                    Logger.Error(ex, "ProfileOperationRouter/ManageProfileAsync: Could not retain saved-resource changes for successful {0} in SID {1}, session {2}.", request.MessageType, userSid, sessionId);
                }
                return response;
            }
        }
        finally
        {
            _savedResourceMutationLock.Release();
        }
    }

    public async Task<ControlResponse> ApplyProfileAsync(string userSid, int sessionId, string profileId, CancellationToken cancellationToken)
    {
        return await ApplyProfileAsync(userSid, sessionId, profileId, Guid.NewGuid(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<ControlResponse> ApplyProfileAsync(string userSid, int sessionId, string profileId, Guid requestId, CancellationToken cancellationToken)
    {
        return await ApplyProfileAsync(userSid, sessionId, profileId, Guid.NewGuid(), requestId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ControlResponse> ApplyProfileAsync(string userSid, int sessionId, string profileId, Guid operationId, Guid requestId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(profileId) || operationId == Guid.Empty || requestId == Guid.Empty)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A display profile ID and correlation IDs are required." };
        }

        AgentRegistration? agent;
        try { agent = await GetOrStartAgentAsync(userSid, sessionId, requestId, operationId, cancellationToken).ConfigureAwait(false); }
        catch (SessionLauncherUnavailableException ex)
        {
            return new ControlResponse { ErrorCode = ControlErrorCode.ExecutionFailed, Message = ex.Message };
        }
        if (agent == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent is not connected for this session." };
        }

        LeaseDecision leaseDecision = _coordinator.TryAcquireDisplayControl(userSid, sessionId, DateTime.UtcNow);
        if (!leaseDecision.IsGranted)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = leaseDecision.ErrorCode, Message = leaseDecision.Message, LeaseDecision = leaseDecision };
        }

        if (!_coordinator.TryBeginDisplayOperation(userSid, sessionId, operationId, DateTime.UtcNow))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.DisplayControlBusy, Message = "Display control is already being used by another operation." };
        }

        try
        {
            ControlResponse response = await _agentCommandClient.SendAsync(agent, new ControlEnvelope
            {
                MessageType = ControlMessageType.ApplyProfile,
                RequestId = requestId,
                Payload = JsonSerializer.Serialize(new ApplyProfileRequest { ProfileId = profileId, OperationId = operationId })
            }, cancellationToken).ConfigureAwait(false);
            // Acceptance of an asynchronous operation is not completion. Keep its
            // lease until the Agent publishes the matching terminal status.
            if (!response.IsSuccessful || response.OperationStatus == null || response.OperationStatus.IsTerminal)
                _coordinator.CompleteDisplayOperation(userSid, sessionId, operationId, false, DateTime.UtcNow);
            return response;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is IOException || ex is InvalidDataException || ex is TimeoutException || ex is UnauthorizedAccessException)
        {
            _coordinator.CompleteDisplayOperation(userSid, sessionId, operationId, true, DateTime.UtcNow);
            return new ControlResponse { ErrorCode = ControlErrorCode.OutcomeUnknown, Message = "The User Agent could not confirm the command result. The operation may have started; inspect its status before retrying." };
        }
    }

    public async Task<ControlResponse> StartShortcutAsync(string userSid, int sessionId, string shortcutId, CancellationToken cancellationToken)
    {
        return await StartShortcutAsync(userSid, sessionId, shortcutId, Guid.NewGuid(), Guid.NewGuid(), cancellationToken).ConfigureAwait(false);
    }

    public async Task<ControlResponse> StartShortcutAsync(string userSid, int sessionId, string shortcutId, Guid operationId, Guid requestId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(shortcutId) || operationId == Guid.Empty || requestId == Guid.Empty)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A shortcut ID and correlation IDs are required." };
        }

        ControlEnvelope command = new ControlEnvelope
        {
            MessageType = ControlMessageType.StartShortcut,
            RequestId = requestId,
            Payload = JsonSerializer.Serialize(new StartShortcutRequest { ShortcutId = shortcutId, OperationId = operationId })
        };
        AgentRegistration? agent;
        try { agent = await GetOrStartAgentAsync(userSid, sessionId, requestId, operationId, cancellationToken).ConfigureAwait(false); }
        catch (SessionLauncherUnavailableException ex)
        {
            return new ControlResponse { ErrorCode = ControlErrorCode.ExecutionFailed, Message = ex.Message };
        }
        if (agent == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent is not connected for this session." };
        }

        LeaseDecision leaseDecision = _coordinator.TryAcquireDisplayControl(userSid, sessionId, DateTime.UtcNow);
        if (!leaseDecision.IsGranted)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = leaseDecision.ErrorCode, Message = leaseDecision.Message, LeaseDecision = leaseDecision };
        }

        if (!_coordinator.TryBeginDisplayOperation(userSid, sessionId, operationId, DateTime.UtcNow))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.DisplayControlBusy, Message = "Display control is already being used by another operation." };
        }

        try
        {
            ControlResponse response = await _agentCommandClient.SendAsync(agent, command, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessful)
            {
                _coordinator.CompleteDisplayOperation(userSid, sessionId, operationId, false, DateTime.UtcNow);
            }

            return response;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is IOException || ex is InvalidDataException || ex is TimeoutException || ex is UnauthorizedAccessException)
        {
            _coordinator.CompleteDisplayOperation(userSid, sessionId, operationId, true, DateTime.UtcNow);
            return new ControlResponse { ErrorCode = ControlErrorCode.OutcomeUnknown, Message = "The User Agent could not confirm the command result. The operation may have started; inspect its status before retrying." };
        }
    }

    public Task<ControlResponse> CancelOperationAsync(string userSid, int sessionId, Guid operationId, CancellationToken cancellationToken)
    {
        if (operationId == Guid.Empty)
        {
            return Task.FromResult(new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "An operation ID is required." });
        }

        return SendToAgentAsync(userSid, sessionId, new ControlEnvelope
        {
            MessageType = ControlMessageType.CancelOperation,
            Payload = JsonSerializer.Serialize(new CancelOperationRequest { OperationId = operationId })
        }, false, cancellationToken);
    }

    public Task<ControlResponse> ManageActiveWindowPlacementsAsync(string userSid, int sessionId, ControlEnvelope request, CancellationToken cancellationToken)
    {
        return SendToAgentAsync(userSid, sessionId, request, request.MessageType == ControlMessageType.ClearWindowPlacements, cancellationToken);
    }

    private async Task<ControlResponse> SendToAgentAsync(string userSid, int sessionId, ControlEnvelope command, CancellationToken cancellationToken)
    {
        return await SendToAgentAsync(userSid, sessionId, command, true, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ControlResponse> SendToAgentAsync(string userSid, int sessionId, ControlEnvelope command, bool startAgentIfMissing, CancellationToken cancellationToken)
    {
        AgentRegistration? agent;
        try
        {
            agent = startAgentIfMissing
                ? await GetOrStartAgentAsync(userSid, sessionId, command.RequestId, GetOperationId(command), cancellationToken).ConfigureAwait(false)
                : _coordinator.GetReadyAgentRegistration(userSid, sessionId);
        }
        catch (SessionLauncherUnavailableException ex)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.ExecutionFailed, Message = ex.Message };
        }
        if (agent == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent is not connected for this session." };
        }

        try
        {
            return await _agentCommandClient.SendAsync(agent, command, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.IO.IOException || ex is InvalidDataException || ex is TimeoutException)
        {
            return new ControlResponse { ErrorCode = ControlRequestReplayStore.IsReadOnlyRequest(command.MessageType) ? ControlErrorCode.AgentUnavailable : ControlErrorCode.OutcomeUnknown,
                Message = "The User Agent could not confirm the command result. Inspect the current state before retrying a mutation." };
        }
    }

    private async Task<AgentRegistration?> GetOrStartAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, CancellationToken cancellationToken)
    {
        AgentRegistration? agent = _coordinator.GetReadyAgentRegistration(userSid, sessionId);
        if (agent != null)
        {
            return agent;
        }

        await _agentLaunchLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            agent = _coordinator.GetReadyAgentRegistration(userSid, sessionId);
            if (agent != null)
            {
                return agent;
            }

            if (_coordinator.GetAgentRegistration(userSid, sessionId) != null)
            {
                return await WaitForReadyAgentAsync(userSid, sessionId, cancellationToken).ConfigureAwait(false);
            }

            UserAgentLaunchResult launchResult;
            try
            {
                string diagnosticLogLevel = _machineDiagnosticLogLevelStore?.GetActiveLevel(DateTime.UtcNow) ?? "Info";
                launchResult = await _sessionLauncherClient.LaunchUserAgentAsync(userSid, sessionId, requestId, operationId, diagnosticLogLevel, cancellationToken).ConfigureAwait(false);
            }
            catch (SessionLauncherUnavailableException)
            {
                throw;
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is TimeoutException || ex is InvalidOperationException)
            {
                return null;
            }

            if (!launchResult.IsSuccessful)
            {
                Logger.Error("ProfileOperationRouter/GetOrStartAgentAsync: Session Launcher could not start User Agent for SID {0}, session {1}. {2}", userSid, sessionId, launchResult.Message);
                throw new SessionLauncherUnavailableException(launchResult.Message, new InvalidOperationException("The Session Launcher rejected the User Agent launch request."));
            }

            return await WaitForReadyAgentAsync(userSid, sessionId, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _agentLaunchLock.Release();
        }
    }

    private async Task<AgentRegistration?> WaitForReadyAgentAsync(string userSid, int sessionId, CancellationToken cancellationToken)
    {
        const int maximumAttempts = 40;
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
            AgentRegistration? agent = _coordinator.GetReadyAgentRegistration(userSid, sessionId);
            if (agent != null)
            {
                return agent;
            }

            if (_coordinator.GetAgentRegistration(userSid, sessionId) == null)
            {
                return null;
            }
        }

        return null;
    }

    private sealed class UnavailableSessionLauncherClient : ISessionLauncherClient
    {
        public Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, string? diagnosticLogLevel, CancellationToken cancellationToken)
        {
            return Task.FromResult(new UserAgentLaunchResult { IsSuccessful = false, Message = "The Session Launcher is not configured." });
        }

        public Task<UserAgentLaunchResult> StopUserAgentAsync(string userSid, int sessionId, int processId, Guid requestId, CancellationToken cancellationToken)
        {
            return Task.FromResult(new UserAgentLaunchResult { IsSuccessful = false, Message = "The Session Launcher is not configured." });
        }
    }

    private static Guid? GetOperationId(ControlEnvelope command)
    {
        if (command.MessageType == ControlMessageType.StartShortcut)
        {
            Guid operationId = JsonSerializer.Deserialize<StartShortcutRequest>(command.Payload)?.OperationId ?? Guid.Empty;
            return operationId == Guid.Empty ? null : operationId;
        }

        if (command.MessageType == ControlMessageType.CancelOperation)
        {
            Guid operationId = JsonSerializer.Deserialize<CancelOperationRequest>(command.Payload)?.OperationId ?? Guid.Empty;
            return operationId == Guid.Empty ? null : operationId;
        }

        return null;
    }
}
