using System;

namespace DisplayMagician.Contracts;

public static class ControlProtocol
{
    public const int CurrentVersion = 1;
    public const string ServicePipeName = "DisplayMagician.ControlService.v1";
}

public enum ControlMessageType
{
    Unknown = 0,
    AgentRegistration = 1,
    AgentHeartbeat = 2,
    AcquireDisplayControl = 3,
    ReleaseDisplayControl = 4,
    ExecuteOperation = 5,
    OperationProgress = 6,
    OperationCompleted = 7,
    RecoveryStatus = 8,
    GetServiceStatus = 9
}

public enum ControlErrorCode
{
    None = 0,
    UnsupportedProtocolVersion = 1,
    InvalidRequest = 2,
    CallerIdentityMismatch = 3,
    AgentNotConnected = 4,
    AgentNotHealthy = 5,
    NotActiveConsoleUser = 6,
    DisplayControlBusy = 7,
    RecoveryRequired = 8,
    SessionLocked = 9,
    Unauthorized = 10,
    AgentUnavailable = 11
}

public enum DisplayOperationType
{
    Unknown = 0,
    ApplyDisplayProfile = 1,
    StartShortcut = 2,
    RestoreTemporaryState = 3
}

public enum AgentOperationState
{
    Idle = 0,
    Running = 1,
    Restoring = 2,
    RecoveryRequired = 3
}

public sealed class ControlEnvelope
{
    public int ProtocolVersion { get; set; } = ControlProtocol.CurrentVersion;

    public ControlMessageType MessageType { get; set; }

    public Guid RequestId { get; set; } = Guid.NewGuid();

    public string Payload { get; set; } = string.Empty;
}

public sealed class AgentRegistration
{
    public string UserSid { get; set; } = string.Empty;

    public int SessionId { get; set; }

    public int ProcessId { get; set; }

    public string Version { get; set; } = string.Empty;

    public string StartupMode { get; set; } = string.Empty;

    public AgentOperationState OperationState { get; set; }

    public bool IsRecoveryRequired { get; set; }
}

public sealed class AgentHeartbeat
{
    public AgentOperationState OperationState { get; set; }

    public bool IsRecoveryRequired { get; set; }
}

public sealed class DisplayControlLease
{
    public string OwnerUserSid { get; set; } = string.Empty;

    public int OwnerSessionId { get; set; }

    public DateTime AcquiredUtc { get; set; }

    public DateTime LastHeartbeatUtc { get; set; }

    public Guid? ActiveOperationId { get; set; }

    public bool IsRecoveryRequired { get; set; }
}

public sealed class LeaseDecision
{
    public bool IsGranted { get; set; }

    public ControlErrorCode ErrorCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public DisplayControlLease? Lease { get; set; }
}

public sealed class ControlResponse
{
    public bool IsSuccessful { get; set; }

    public ControlErrorCode ErrorCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public LeaseDecision? LeaseDecision { get; set; }

    public ControlServiceStatus? ServiceStatus { get; set; }
}

public sealed class ControlServiceStatus
{
    public AgentStatus[] Agents { get; set; } = Array.Empty<AgentStatus>();

    public DisplayControlLease? DisplayControlLease { get; set; }
}

public sealed class AgentStatus
{
    public string UserSid { get; set; } = string.Empty;

    public int SessionId { get; set; }

    public int ProcessId { get; set; }

    public AgentOperationState OperationState { get; set; }

    public bool IsRecoveryRequired { get; set; }

    public DateTime LastHeartbeatUtc { get; set; }

    public bool IsHealthy { get; set; }
}
