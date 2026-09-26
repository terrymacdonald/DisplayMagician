using System;
using System.Collections.Generic;
using System.Linq;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

public sealed class ControlStateCoordinator
{
    private static readonly TimeSpan AgentHeartbeatTimeout = TimeSpan.FromSeconds(45);
    private readonly object _syncRoot = new object();
    private readonly Dictionary<int, RegisteredAgent> _agentsBySession = new Dictionary<int, RegisteredAgent>();
    private readonly DisplayControlLeaseStore? _leaseStore;
    private DisplayControlLease? _displayControlLease;

    public ControlStateCoordinator()
    {
    }

    public ControlStateCoordinator(DisplayControlLeaseStore leaseStore)
    {
        _leaseStore = leaseStore ?? throw new ArgumentNullException(nameof(leaseStore));
        _displayControlLease = _leaseStore.Load();
    }

    public void RegisterAgent(AgentRegistration registration, DateTime utcNow)
    {
        if (!TryRegisterAgentConnection(registration, utcNow, out string message))
        {
            throw new InvalidOperationException(message);
        }
    }

    public bool TryRegisterAgentConnection(AgentRegistration registration, DateTime utcNow, out string message)
    {
        ArgumentNullException.ThrowIfNull(registration);

        lock (_syncRoot)
        {
            if (_agentsBySession.TryGetValue(registration.SessionId, out RegisteredAgent? existingAgent))
            {
                if (!string.Equals(existingAgent.Registration.UserSid, registration.UserSid, StringComparison.OrdinalIgnoreCase)
                    || existingAgent.Registration.ProcessId != registration.ProcessId)
                {
                    message = $"Another User Agent is already connected for session {registration.SessionId}.";
                    return false;
                }

                existingAgent.ConnectionCount++;
                UpdateRegistration(existingAgent, registration, utcNow);
                message = "Agent connection registered.";
                return true;
            }

            _agentsBySession[registration.SessionId] = new RegisteredAgent(CopyRegistration(registration), utcNow);
            ConfirmRecoveryRestored(registration);
            message = "Agent connection registered.";
            return true;
        }
    }

    public bool UpdateAgentRegistration(AgentRegistration registration, DateTime utcNow, out string message)
    {
        ArgumentNullException.ThrowIfNull(registration);

        lock (_syncRoot)
        {
            if (!_agentsBySession.TryGetValue(registration.SessionId, out RegisteredAgent? existingAgent)
                || !string.Equals(existingAgent.Registration.UserSid, registration.UserSid, StringComparison.OrdinalIgnoreCase)
                || existingAgent.Registration.ProcessId != registration.ProcessId)
            {
                message = "The User Agent is no longer the registered owner of this session.";
                return false;
            }

            UpdateRegistration(existingAgent, registration, utcNow);
            message = "Agent registration updated.";
            return true;
        }
    }

    public void RecordHeartbeat(string userSid, int sessionId, AgentOperationState operationState, bool isRecoveryRequired, DateTime utcNow)
    {
        lock (_syncRoot)
        {
            if (!_agentsBySession.TryGetValue(sessionId, out RegisteredAgent? agent) || !string.Equals(agent.Registration.UserSid, userSid, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            agent.LastHeartbeatUtc = utcNow;
            agent.Registration.OperationState = operationState;
            agent.Registration.IsRecoveryRequired = isRecoveryRequired;

            if (_displayControlLease != null && _displayControlLease.OwnerSessionId == sessionId)
            {
                _displayControlLease.LastHeartbeatUtc = utcNow;
                _displayControlLease.IsRecoveryRequired = isRecoveryRequired;
                if (operationState == AgentOperationState.Running || operationState == AgentOperationState.Restoring)
                {
                    _displayControlLease.ActiveOperationId ??= Guid.NewGuid();
                }
                else if (operationState == AgentOperationState.Idle && !isRecoveryRequired)
                {
                    _displayControlLease.ActiveOperationId = null;
                }
                PersistLease();
            }
        }
    }

    public LeaseDecision TryAcquireDisplayControl(string userSid, int sessionId, int activeConsoleSessionId, DateTime utcNow)
    {
        lock (_syncRoot)
        {
            ReleaseIdleLeaseIfUnavailable(activeConsoleSessionId, utcNow);

            if (sessionId != activeConsoleSessionId)
            {
                return Deny(ControlErrorCode.NotActiveConsoleUser, "Only the active physical-console user can control displays.");
            }

            if (!_agentsBySession.TryGetValue(sessionId, out RegisteredAgent? agent) || !string.Equals(agent.Registration.UserSid, userSid, StringComparison.OrdinalIgnoreCase))
            {
                return Deny(ControlErrorCode.AgentNotConnected, "The User Agent is not connected for this session.");
            }

            if (utcNow - agent.LastHeartbeatUtc > AgentHeartbeatTimeout)
            {
                return Deny(ControlErrorCode.AgentNotHealthy, "The User Agent has not sent a recent heartbeat.");
            }

            if (agent.Registration.IsRecoveryRequired)
            {
                return Deny(ControlErrorCode.RecoveryRequired, "The User Agent must restore temporary state before a new operation can begin.");
            }

            if (_displayControlLease != null && (_displayControlLease.OwnerSessionId != sessionId || !string.Equals(_displayControlLease.OwnerUserSid, userSid, StringComparison.OrdinalIgnoreCase)))
            {
                return Deny(ControlErrorCode.DisplayControlBusy, "Another user currently owns DisplayMagician display control.");
            }

            _displayControlLease ??= new DisplayControlLease
            {
                OwnerUserSid = userSid,
                OwnerSessionId = sessionId,
                AcquiredUtc = utcNow
            };
            _displayControlLease.LastHeartbeatUtc = utcNow;
            PersistLease();

            return new LeaseDecision
            {
                IsGranted = true,
                Lease = CopyLease(_displayControlLease),
                Message = "Display control acquired."
            };
        }
    }

    public DisplayControlLease? GetDisplayControlLease()
    {
        lock (_syncRoot)
        {
            return _displayControlLease == null ? null : CopyLease(_displayControlLease);
        }
    }

    public bool TryBeginDisplayOperation(string userSid, int sessionId, Guid operationId, DateTime utcNow)
    {
        if (operationId == Guid.Empty)
        {
            return false;
        }

        lock (_syncRoot)
        {
            if (_displayControlLease == null || _displayControlLease.ActiveOperationId.HasValue || _displayControlLease.IsRecoveryRequired ||
                _displayControlLease.OwnerSessionId != sessionId || !string.Equals(_displayControlLease.OwnerUserSid, userSid, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _displayControlLease.ActiveOperationId = operationId;
            _displayControlLease.LastHeartbeatUtc = utcNow;
            PersistLease();
            return true;
        }
    }

    public void CompleteDisplayOperation(string userSid, int sessionId, Guid operationId, bool requiresRecovery, DateTime utcNow)
    {
        lock (_syncRoot)
        {
            if (_displayControlLease == null || _displayControlLease.ActiveOperationId != operationId ||
                _displayControlLease.OwnerSessionId != sessionId || !string.Equals(_displayControlLease.OwnerUserSid, userSid, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _displayControlLease.LastHeartbeatUtc = utcNow;
            _displayControlLease.IsRecoveryRequired = requiresRecovery;
            _displayControlLease.ActiveOperationId = requiresRecovery ? operationId : null;
            PersistLease();
        }
    }

    public DisplayControlLease? ForceReleaseDisplayControl()
    {
        lock (_syncRoot)
        {
            if (_displayControlLease == null)
            {
                return null;
            }

            DisplayControlLease releasedLease = CopyLease(_displayControlLease);
            _displayControlLease = null;
            ClearPersistedLease();
            return releasedLease;
        }
    }

    public AgentRegistration? GetAgentRegistration(string userSid, int sessionId)
    {
        lock (_syncRoot)
        {
            if (!_agentsBySession.TryGetValue(sessionId, out RegisteredAgent? agent)
                || !string.Equals(agent.Registration.UserSid, userSid, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return CopyRegistration(agent.Registration);
        }
    }

    public AgentRegistration? GetReadyAgentRegistration(string userSid, int sessionId)
    {
        lock (_syncRoot)
        {
            if (!_agentsBySession.TryGetValue(sessionId, out RegisteredAgent? agent)
                || !agent.Registration.IsReady
                || !string.Equals(agent.Registration.UserSid, userSid, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return CopyRegistration(agent.Registration);
        }
    }

    public AgentRegistration[] GetAgentRegistrations()
    {
        lock (_syncRoot)
        {
            return _agentsBySession.Values.Select(agent => CopyRegistration(agent.Registration)).ToArray();
        }
    }

    public void UnregisterAgent(string userSid, int sessionId, int processId)
    {
        lock (_syncRoot)
        {
            if (!_agentsBySession.TryGetValue(sessionId, out RegisteredAgent? agent) || !string.Equals(agent.Registration.UserSid, userSid, StringComparison.OrdinalIgnoreCase) || agent.Registration.ProcessId != processId)
            {
                return;
            }

            agent.ConnectionCount--;
            if (agent.ConnectionCount > 0)
            {
                return;
            }

            _agentsBySession.Remove(sessionId);
            if (_displayControlLease == null || _displayControlLease.OwnerSessionId != sessionId)
            {
                return;
            }

            if (_displayControlLease.ActiveOperationId.HasValue || _displayControlLease.IsRecoveryRequired)
            {
                _displayControlLease.IsRecoveryRequired = true;
                PersistLease();
                return;
            }

            _displayControlLease = null;
            ClearPersistedLease();
        }
    }

    public ControlServiceStatus GetStatus(DateTime utcNow)
    {
        lock (_syncRoot)
        {
            List<AgentStatus> agents = new List<AgentStatus>();
            foreach (RegisteredAgent agent in _agentsBySession.Values)
            {
                agents.Add(new AgentStatus
                {
                    UserSid = agent.Registration.UserSid,
                    SessionId = agent.Registration.SessionId,
                    ProcessId = agent.Registration.ProcessId,
                    OperationState = agent.Registration.OperationState,
                    IsRecoveryRequired = agent.Registration.IsRecoveryRequired,
                    LastHeartbeatUtc = agent.LastHeartbeatUtc,
                    IsHealthy = utcNow - agent.LastHeartbeatUtc <= AgentHeartbeatTimeout,
                    IsReady = agent.Registration.IsReady
                });
            }

            return new ControlServiceStatus
            {
                Agents = agents.ToArray(),
                DisplayControlLease = _displayControlLease == null ? null : CopyLease(_displayControlLease)
            };
        }
    }

    private void UpdateRegistration(RegisteredAgent existingAgent, AgentRegistration registration, DateTime utcNow)
    {
        existingAgent.Registration.Version = registration.Version;
        existingAgent.Registration.StartupMode = registration.StartupMode;
        existingAgent.Registration.OperationState = registration.OperationState;
        existingAgent.Registration.IsRecoveryRequired = registration.IsRecoveryRequired;
        existingAgent.Registration.IsReady = registration.IsReady;
        existingAgent.Registration.CommandPipeName = registration.CommandPipeName;
        existingAgent.LastHeartbeatUtc = utcNow;
        ConfirmRecoveryRestored(registration);
    }

    private static LeaseDecision Deny(ControlErrorCode errorCode, string message)
    {
        return new LeaseDecision
        {
            ErrorCode = errorCode,
            Message = message
        };
    }

    private void ReleaseIdleLeaseIfUnavailable(int activeConsoleSessionId, DateTime utcNow)
    {
        if (_displayControlLease == null || _displayControlLease.ActiveOperationId.HasValue || _displayControlLease.IsRecoveryRequired)
        {
            return;
        }

        bool ownerIsNoLongerActiveConsoleUser = _displayControlLease.OwnerSessionId != activeConsoleSessionId;
        bool ownerHeartbeatIsStale = utcNow - _displayControlLease.LastHeartbeatUtc > AgentHeartbeatTimeout;
        if (ownerIsNoLongerActiveConsoleUser || ownerHeartbeatIsStale)
        {
            _displayControlLease = null;
            ClearPersistedLease();
        }
    }

    private void ConfirmRecoveryRestored(AgentRegistration registration)
    {
        if (_displayControlLease == null || !_displayControlLease.IsRecoveryRequired || registration.IsRecoveryRequired ||
            _displayControlLease.OwnerSessionId != registration.SessionId ||
            !string.Equals(_displayControlLease.OwnerUserSid, registration.UserSid, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _displayControlLease.IsRecoveryRequired = false;
        _displayControlLease.ActiveOperationId = null;
        PersistLease();
    }

    private void PersistLease()
    {
        if (_leaseStore != null && _displayControlLease != null)
        {
            _leaseStore.Save(CopyLease(_displayControlLease));
        }
    }

    private void ClearPersistedLease()
    {
        _leaseStore?.Clear();
    }

    private static DisplayControlLease CopyLease(DisplayControlLease lease)
    {
        return new DisplayControlLease
        {
            OwnerUserSid = lease.OwnerUserSid,
            OwnerSessionId = lease.OwnerSessionId,
            AcquiredUtc = lease.AcquiredUtc,
            LastHeartbeatUtc = lease.LastHeartbeatUtc,
            ActiveOperationId = lease.ActiveOperationId,
            IsRecoveryRequired = lease.IsRecoveryRequired
        };
    }

    private static AgentRegistration CopyRegistration(AgentRegistration registration)
    {
        return new AgentRegistration
        {
            UserSid = registration.UserSid,
            SessionId = registration.SessionId,
            ProcessId = registration.ProcessId,
            Version = registration.Version,
            StartupMode = registration.StartupMode,
            OperationState = registration.OperationState,
            IsRecoveryRequired = registration.IsRecoveryRequired,
            IsReady = registration.IsReady,
            CommandPipeName = registration.CommandPipeName
        };
    }

    private sealed class RegisteredAgent
    {
        public RegisteredAgent(AgentRegistration registration, DateTime lastHeartbeatUtc)
        {
            Registration = registration;
            LastHeartbeatUtc = lastHeartbeatUtc;
        }

        public AgentRegistration Registration { get; }

        public DateTime LastHeartbeatUtc { get; set; }

        public int ConnectionCount { get; set; } = 1;
    }
}
