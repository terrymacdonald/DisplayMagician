using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

public sealed class ProfileOperationRouter
{
    private readonly ControlStateCoordinator _coordinator;
    private readonly IAgentCommandClient _agentCommandClient;
    private readonly ISessionLauncherClient _sessionLauncherClient;
    private readonly Func<int> _getActiveConsoleSessionId;
    private readonly RecoveryAdministrationStore? _recoveryAdministrationStore;

    public ProfileOperationRouter(ControlStateCoordinator coordinator, IAgentCommandClient agentCommandClient)
        : this(coordinator, agentCommandClient, new UnavailableSessionLauncherClient(), ConsoleSessionLocator.GetActiveConsoleSessionId)
    {
    }

    public ProfileOperationRouter(ControlStateCoordinator coordinator, IAgentCommandClient agentCommandClient, ISessionLauncherClient sessionLauncherClient)
        : this(coordinator, agentCommandClient, sessionLauncherClient, ConsoleSessionLocator.GetActiveConsoleSessionId)
    {
    }

    public ProfileOperationRouter(ControlStateCoordinator coordinator, IAgentCommandClient agentCommandClient, ISessionLauncherClient sessionLauncherClient, RecoveryAdministrationStore recoveryAdministrationStore)
        : this(coordinator, agentCommandClient, sessionLauncherClient, ConsoleSessionLocator.GetActiveConsoleSessionId, recoveryAdministrationStore)
    {
    }

    public ProfileOperationRouter(ControlStateCoordinator coordinator, IAgentCommandClient agentCommandClient, ISessionLauncherClient sessionLauncherClient, Func<int> getActiveConsoleSessionId, RecoveryAdministrationStore? recoveryAdministrationStore = null)
    {
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _agentCommandClient = agentCommandClient ?? throw new ArgumentNullException(nameof(agentCommandClient));
        _sessionLauncherClient = sessionLauncherClient ?? throw new ArgumentNullException(nameof(sessionLauncherClient));
        _getActiveConsoleSessionId = getActiveConsoleSessionId ?? throw new ArgumentNullException(nameof(getActiveConsoleSessionId));
        _recoveryAdministrationStore = recoveryAdministrationStore;
    }

    public Task<ControlResponse> ListProfilesAsync(string userSid, int sessionId, CancellationToken cancellationToken)
    {
        return SendToAgentAsync(userSid, sessionId, new ControlEnvelope { MessageType = ControlMessageType.ListProfiles }, cancellationToken);
    }

    public Task<ControlResponse> StopAgentIfIdleAsync(string userSid, int sessionId, CancellationToken cancellationToken)
    {
        return SendToAgentAsync(userSid, sessionId, new ControlEnvelope { MessageType = ControlMessageType.StopAgentIfIdle }, false, cancellationToken);
    }

    public async Task<ControlResponse> RestartUserAgentAsync(string userSid, int sessionId, Guid requestId, CancellationToken cancellationToken)
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

    public Task<ControlResponse> ManageProfileAsync(string userSid, int sessionId, ControlEnvelope request, CancellationToken cancellationToken)
    {
        return SendToAgentAsync(userSid, sessionId, request, cancellationToken);
    }

    public async Task<ControlResponse> ApplyProfileAsync(string userSid, int sessionId, string profileId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(profileId))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.InvalidRequest, Message = "A display profile ID is required." };
        }

        LeaseDecision leaseDecision = _coordinator.TryAcquireDisplayControl(userSid, sessionId, _getActiveConsoleSessionId(), DateTime.UtcNow);
        if (!leaseDecision.IsGranted)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = leaseDecision.ErrorCode, Message = leaseDecision.Message, LeaseDecision = leaseDecision };
        }

        Guid operationId = Guid.NewGuid();
        if (!_coordinator.TryBeginDisplayOperation(userSid, sessionId, operationId, DateTime.UtcNow))
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.DisplayControlBusy, Message = "Display control is already being used by another operation." };
        }

        AgentRegistration? agent = await GetOrStartAgentAsync(userSid, sessionId, Guid.NewGuid(), operationId, cancellationToken).ConfigureAwait(false);
        if (agent == null)
        {
            _coordinator.CompleteDisplayOperation(userSid, sessionId, operationId, false, DateTime.UtcNow);
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent is not connected for this session." };
        }

        try
        {
            ControlResponse response = await _agentCommandClient.SendAsync(agent, new ControlEnvelope
            {
                MessageType = ControlMessageType.ApplyProfile,
                RequestId = Guid.NewGuid(),
                Payload = JsonSerializer.Serialize(new ApplyProfileRequest { ProfileId = profileId })
            }, cancellationToken).ConfigureAwait(false);
            _coordinator.CompleteDisplayOperation(userSid, sessionId, operationId, false, DateTime.UtcNow);
            return response;
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is IOException || ex is TimeoutException)
        {
            _coordinator.CompleteDisplayOperation(userSid, sessionId, operationId, true, DateTime.UtcNow);
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent command endpoint is unavailable." };
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

        if (sessionId != _getActiveConsoleSessionId())
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.NotActiveConsoleUser, Message = "Only the active physical-console user can control displays." };
        }

        ControlEnvelope command = new ControlEnvelope
        {
            MessageType = ControlMessageType.StartShortcut,
            RequestId = requestId,
            Payload = JsonSerializer.Serialize(new StartShortcutRequest { ShortcutId = shortcutId, OperationId = operationId })
        };
        AgentRegistration? agent = await GetOrStartAgentAsync(userSid, sessionId, requestId, operationId, cancellationToken).ConfigureAwait(false);
        if (agent == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent is not connected for this session." };
        }

        LeaseDecision leaseDecision = _coordinator.TryAcquireDisplayControl(userSid, sessionId, _getActiveConsoleSessionId(), DateTime.UtcNow);
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
        catch (Exception ex) when (ex is InvalidOperationException || ex is IOException || ex is TimeoutException)
        {
            _coordinator.CompleteDisplayOperation(userSid, sessionId, operationId, true, DateTime.UtcNow);
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent command endpoint is unavailable." };
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

    private async Task<ControlResponse> SendToAgentAsync(string userSid, int sessionId, ControlEnvelope command, CancellationToken cancellationToken)
    {
        return await SendToAgentAsync(userSid, sessionId, command, true, cancellationToken).ConfigureAwait(false);
    }

    private async Task<ControlResponse> SendToAgentAsync(string userSid, int sessionId, ControlEnvelope command, bool startAgentIfMissing, CancellationToken cancellationToken)
    {
        AgentRegistration? agent = startAgentIfMissing
            ? await GetOrStartAgentAsync(userSid, sessionId, command.RequestId, GetOperationId(command), cancellationToken).ConfigureAwait(false)
            : _coordinator.GetAgentRegistration(userSid, sessionId);
        if (agent == null)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent is not connected for this session." };
        }

        try
        {
            return await _agentCommandClient.SendAsync(agent, command, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is System.IO.IOException || ex is TimeoutException)
        {
            return new ControlResponse { IsSuccessful = false, ErrorCode = ControlErrorCode.AgentUnavailable, Message = "The User Agent command endpoint is unavailable." };
        }
    }

    private async Task<AgentRegistration?> GetOrStartAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, CancellationToken cancellationToken)
    {
        AgentRegistration? agent = _coordinator.GetAgentRegistration(userSid, sessionId);
        if (agent != null)
        {
            return agent;
        }

        UserAgentLaunchResult launchResult;
        try
        {
            launchResult = await _sessionLauncherClient.LaunchUserAgentAsync(userSid, sessionId, requestId, operationId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException || ex is TimeoutException || ex is InvalidOperationException)
        {
            return null;
        }

        if (!launchResult.IsSuccessful)
        {
            return null;
        }

        const int maximumAttempts = 40;
        for (int attempt = 1; attempt <= maximumAttempts; attempt++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken).ConfigureAwait(false);
            agent = _coordinator.GetAgentRegistration(userSid, sessionId);
            if (agent != null)
            {
                return agent;
            }
        }

        return null;
    }

    private sealed class UnavailableSessionLauncherClient : ISessionLauncherClient
    {
        public Task<UserAgentLaunchResult> LaunchUserAgentAsync(string userSid, int sessionId, Guid requestId, Guid? operationId, CancellationToken cancellationToken)
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
