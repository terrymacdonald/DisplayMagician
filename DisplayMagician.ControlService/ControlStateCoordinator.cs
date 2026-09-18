using System;
using System.Collections.Generic;
using DisplayMagician.Contracts;

namespace DisplayMagician.ControlService;

public sealed class ControlStateCoordinator
{
    private readonly object _syncRoot = new object();
    private readonly Dictionary<int, RegisteredAgent> _agentsBySession = new Dictionary<int, RegisteredAgent>();
    private DisplayControlLease? _displayControlLease;

    public void RegisterAgent(AgentRegistration registration, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(registration);

        lock (_syncRoot)
        {
            _agentsBySession[registration.SessionId] = new RegisteredAgent(registration, utcNow);
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
            }
        }
    }

    public LeaseDecision TryAcquireDisplayControl(string userSid, int sessionId, int activeConsoleSessionId, DateTime utcNow)
    {
        lock (_syncRoot)
        {
            if (sessionId != activeConsoleSessionId)
            {
                return Deny(ControlErrorCode.NotActiveConsoleUser, "Only the active physical-console user can control displays.");
            }

            if (!_agentsBySession.TryGetValue(sessionId, out RegisteredAgent? agent) || !string.Equals(agent.Registration.UserSid, userSid, StringComparison.OrdinalIgnoreCase))
            {
                return Deny(ControlErrorCode.AgentNotConnected, "The User Agent is not connected for this session.");
            }

            if (utcNow - agent.LastHeartbeatUtc > TimeSpan.FromSeconds(45))
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

    public void UnregisterAgent(string userSid, int sessionId, int processId)
    {
        lock (_syncRoot)
        {
            if (!_agentsBySession.TryGetValue(sessionId, out RegisteredAgent? agent) || !string.Equals(agent.Registration.UserSid, userSid, StringComparison.OrdinalIgnoreCase) || agent.Registration.ProcessId != processId)
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
                return;
            }

            _displayControlLease = null;
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
                    IsHealthy = utcNow - agent.LastHeartbeatUtc <= TimeSpan.FromSeconds(45)
                });
            }

            return new ControlServiceStatus
            {
                Agents = agents.ToArray(),
                DisplayControlLease = _displayControlLease == null ? null : CopyLease(_displayControlLease)
            };
        }
    }

    private static LeaseDecision Deny(ControlErrorCode errorCode, string message)
    {
        return new LeaseDecision
        {
            ErrorCode = errorCode,
            Message = message
        };
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

    private sealed class RegisteredAgent
    {
        public RegisteredAgent(AgentRegistration registration, DateTime lastHeartbeatUtc)
        {
            Registration = registration;
            LastHeartbeatUtc = lastHeartbeatUtc;
        }

        public AgentRegistration Registration { get; }

        public DateTime LastHeartbeatUtc { get; set; }
    }
}
