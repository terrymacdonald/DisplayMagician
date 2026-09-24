using System;

namespace DisplayMagician.Contracts;

public static class ControlProtocol
{
    public const int CurrentVersion = 1;
    public const string ServicePipeName = "DisplayMagician.ControlService.v1";
    public const string ClientPipeName = "DisplayMagician.ControlService.Client.v1";
    public const string ClientEventPipeName = "DisplayMagician.ControlService.ClientEvents.v1";
    public const string AgentCommandPipePrefix = "DisplayMagician.UserAgent.Command.v1.";
    public const string SessionLauncherPipeName = "DisplayMagician.SessionLauncher.v1";
    public const int DefaultAudioDeviceWaitMilliseconds = 20000;
    public const int MaximumMessageLength = 5 * 1024 * 1024;
    public static readonly TimeSpan ConnectionTimeout = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan ResponseTimeout = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan EventIdleTimeout = TimeSpan.FromSeconds(45);
}

public enum ControlMessageType
{
    Unknown = 0,
    AgentRegistration = 1,
    AgentHeartbeat = 2,
    AcquireDisplayControl = 3,
    OperationProgress = 6,
    OperationCompleted = 7,
    GetServiceStatus = 9,
    MigrateUserData = 10,
    ListProfiles = 11,
    ApplyProfile = 12,
    StopAgentIfIdle = 13,
    LaunchUserAgent = 14,
    CreateProfileFromCurrent = 15,
    RenameProfile = 16,
    DeleteProfile = 17,
    UpdateProfileFromCurrent = 18,
    ListAudioProfiles = 19,
    ApplyAudioProfile = 20,
    CreateAudioProfileFromCurrent = 21,
    RenameAudioProfile = 22,
    DeleteAudioProfile = 23,
    UpdateAudioProfileFromCurrent = 24,
    GetRepositorySnapshot = 25,
    CommitRepositorySnapshot = 26,
    StartShortcut = 27,
    GetOperationStatus = 28,
    ListOperationStatuses = 29,
    ListGames = 30,
    ListMessages = 31,
    SetMessageReadState = 32,
    SyncMessages = 33,
    ListApps = 34,
    ListShortcuts = 35,
    UpdateDisplayProfileSettings = 36,
    CancelOperation = 37,
    SyncClient = 38,
    ApplyClientSyncMessages = 39,
    GetAnonymousMetricsSettings = 40,
    UpdateAnonymousMetricsSettings = 41,
    InitializeAnonymousMetrics = 42,
    ReportAnonymousMetricsUsage = 43,
    ListOperationDecisions = 51,
    SubscribeClientEvents = 44,
    ClientEvent = 45,
    ForceReleaseDisplayControl = 47,
    CreateUserSupportBundle = 48,
    ResolveOperationDecision = 49,
    RequestOperationDecision = 50,
    RestartUserAgent = 52,
    StopUserAgent = 53,
    RecordRecoveryAdministration = 54,
    GetOperationDecision = 55,
    ReconcileOperationStatuses = 56
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
    AgentUnavailable = 11,
    AdministratorRequired = 12,
    ProfileNotFound = 13,
    AudioProfileNotFound = 14,
    ShortcutNotFound = 15,
    ValidationFailed = 16,
    ExecutionFailed = 17,
    OperationNotFound = 18,
    DecisionUnavailable = 19
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

    public bool IsReady { get; set; }

    public string CommandPipeName { get; set; } = string.Empty;
}

public sealed class GameView
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Library { get; set; }
    public string ExecutablePath { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
}

public sealed class GameListResult
{
    public GameView[] Games { get; set; } = Array.Empty<GameView>();
}

/// <summary>A client-safe installed application representation owned by the interactive User Agent.</summary>
public sealed class AppView
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Library { get; set; }
    public string ExecutablePath { get; set; } = string.Empty;
    public bool ExecutableArgumentsRequired { get; set; }
    public string Arguments { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public string Directory { get; set; } = string.Empty;
}

public sealed class AppListResult
{
    public AppView[] Apps { get; set; } = Array.Empty<AppView>();
}

/// <summary>A client-safe shortcut representation with an Agent-rendered icon.</summary>
public sealed class ShortcutView
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ShortcutCategory Category { get; set; }
    public string ProfileId { get; set; } = string.Empty;
    public string? IconPngBase64 { get; set; }
}

public sealed class ShortcutListResult
{
    public ShortcutView[] Shortcuts { get; set; } = Array.Empty<ShortcutView>();
}

/// <summary>A client-safe message representation owned by the interactive User Agent.</summary>
public sealed class MessageView
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public DateTime? PublishedUtc { get; set; }
    public DateTime ReceivedUtc { get; set; }
    public bool IsRead { get; set; }
    public bool ShowOnStartup { get; set; }
    public bool IsFaulty { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string? ReleaseVersion { get; set; }
    public string? ReleaseChannel { get; set; }
    public string? UpdateAction { get; set; }
}

public sealed class MessageListResult
{
    public MessageView[] Messages { get; set; } = Array.Empty<MessageView>();
    public int UnreadCount { get; set; }
}

public sealed class SetMessageReadStateRequest
{
    public string[] MessageIds { get; set; } = Array.Empty<string>();
    public bool IsRead { get; set; }
}

public sealed class MessageSyncResult
{
    public bool IsSuccessful { get; set; }
    public int NewMessagesCount { get; set; }
    public int UnreadCount { get; set; }
}

public sealed class ProfileListResult
{
    public DisplayProfileView[] SavedProfiles { get; set; } = Array.Empty<DisplayProfileView>();
    public DisplayProfileView? CurrentLayout { get; set; }
}

public sealed class ApplyProfileRequest
{
    public string ProfileId { get; set; } = string.Empty;

    public Guid OperationId { get; set; }
}

public sealed class ApplyProfileResult
{
    public bool WasCancelled { get; set; }
}

/// <summary>
/// Requests that the interactive User Agent run the shortcut identified by
/// <see cref="ShortcutId"/>. The Agent reads the current shortcut definition
/// from its own store rather than accepting a caller-supplied definition.
/// </summary>
public sealed class StartShortcutRequest
{
    public string ShortcutId { get; set; } = string.Empty;

    public Guid OperationId { get; set; }
}

/// <summary>Requests cancellation of an active operation owned by the caller's User Agent.</summary>
public sealed class CancelOperationRequest
{
    public Guid OperationId { get; set; }
}

public enum OperationPhase
{
    Unknown = 0,
    Requested = 1,
    Validating = 2,
    ApplyingDisplayProfile = 3,
    ApplyingAudioProfile = 4,
    StartingPrograms = 5,
    StartingGame = 6,
    WaitingForGameToStart = 7,
    WaitingForGameToClose = 8,
    RunningAfterPrograms = 9,
    RestoringDisplayProfile = 10,
    RestoringAudioProfile = 11,
    Completed = 12,
    Cancelled = 13,
    Failed = 14,
    AwaitingUserDecision = 15
}

public enum OperationDecisionChoice
{
    Unknown = 0,
    Continue = 1,
    StopAndRestore = 2
}

/// <summary>A service-owned prompt that an authorized client may resolve once for an active operation.</summary>
public sealed class OperationDecision
{
    public Guid PromptId { get; set; }

    public Guid OperationId { get; set; }

    public string OwnerUserSid { get; set; } = string.Empty;

    public int OwnerSessionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public OperationDecisionChoice[] AllowedChoices { get; set; } = Array.Empty<OperationDecisionChoice>();

    public OperationDecisionChoice DefaultChoice { get; set; } = OperationDecisionChoice.Continue;

    public DateTime CreatedUtc { get; set; }

    public DateTime ExpiresUtc { get; set; }

    public bool IsResolved { get; set; }

    public OperationDecisionChoice ResolvedChoice { get; set; }

    public DateTime? ResolvedUtc { get; set; }
}

public sealed class ResolveOperationDecisionRequest
{
    public Guid PromptId { get; set; }

    public OperationDecisionChoice Choice { get; set; }
}

public sealed class RequestOperationDecisionRequest
{
    public Guid OperationId { get; set; }

    public DisplayOperationType OperationType { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public OperationDecisionChoice[] AllowedChoices { get; set; } = Array.Empty<OperationDecisionChoice>();

    public OperationDecisionChoice DefaultChoice { get; set; } = OperationDecisionChoice.Continue;

    public int TimeoutSeconds { get; set; } = 60;
}

/// <summary>Gets the current state of a previously-created operation decision.</summary>
public sealed class OperationDecisionStatusRequest
{
    public Guid PromptId { get; set; }
}

/// <summary>Published by the User Agent as a shortcut or profile operation progresses.</summary>
public sealed class OperationStatusUpdate
{
    /// <summary>Stable delivery identity. Retrying the same update must not create another sequence entry.</summary>
    public Guid UpdateId { get; set; } = Guid.NewGuid();

    public Guid OperationId { get; set; }

    public DisplayOperationType OperationType { get; set; }

    public OperationPhase Phase { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsTerminal { get; set; }

    public bool IsSuccessful { get; set; }

    public ControlErrorCode ErrorCode { get; set; }

}

/// <summary>Agent snapshot used after a Control Service restart to restore live operation state.</summary>
public sealed class OperationStatusReconciliationRequest
{
    public OperationStatusUpdate[] ActiveUpdates { get; set; } = Array.Empty<OperationStatusUpdate>();
}

/// <summary>Service-owned, client-visible operation state. Sequence is per operation and always increases.</summary>
public sealed class OperationStatus
{
    public Guid OperationId { get; set; }

    public DisplayOperationType OperationType { get; set; }

    public string OwnerUserSid { get; set; } = string.Empty;

    public int OwnerSessionId { get; set; }

    public long Sequence { get; set; }

    public OperationPhase Phase { get; set; }

    public string Message { get; set; } = string.Empty;

    public DateTime StartedUtc { get; set; }

    public DateTime UpdatedUtc { get; set; }

    public bool IsTerminal { get; set; }

    public bool IsSuccessful { get; set; }

    public ControlErrorCode ErrorCode { get; set; }

    public Guid LastUpdateId { get; set; }

    public bool IsAuthoritative { get; set; } = true;

    public bool IsStale { get; set; }

    public string StaleReason { get; set; } = string.Empty;
}

public sealed class OperationStatusRequest
{
    public Guid OperationId { get; set; }
}

public enum RepositoryKind
{
    Unknown = 0,
    DisplayProfiles = 1,
    AudioProfiles = 2,
    Shortcuts = 3
}

public sealed class DisplayProfileView
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ThumbnailPngBase64 { get; set; }
    public int ConnectedDisplayCount { get; set; }
    public int PrimaryDisplayWidth { get; set; }
    public int PrimaryDisplayHeight { get; set; }
    public bool IsSaved { get; set; }
    public bool IsActive { get; set; }
    public bool IsValid { get; set; }
    public string DiagnosticMessage { get; set; } = string.Empty;
    public DisplayProfileSettings Settings { get; set; } = new DisplayProfileSettings();
}

public sealed class DisplayProfileSettings
{
    public bool ApplyWallpaper { get; set; }
    public string BackgroundDescription { get; set; } = string.Empty;
    public bool ForceExplorerRestart { get; set; }
    public int ApplyProfileCount { get; set; } = 1;
    public int ApplyProfileDelay { get; set; }
}

public sealed class UpdateDisplayProfileSettingsRequest
{
    public string ProfileId { get; set; } = string.Empty;
    public DisplayProfileSettings Settings { get; set; } = new DisplayProfileSettings();
}

public sealed class AudioProfileView
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSaved { get; set; }
    public bool IsActive { get; set; }
    public string SettingsText { get; set; } = string.Empty;
    public string[] UnavailableDeviceNames { get; set; } = Array.Empty<string>();
}

public sealed class AudioProfileListResult
{
    public AudioProfileView[] SavedProfiles { get; set; } = Array.Empty<AudioProfileView>();
    public AudioProfileView? CurrentLayout { get; set; }
    public bool CanAccessAudioSettings { get; set; }
}

public sealed class ApplyAudioProfileRequest { public string ProfileId { get; set; } = string.Empty; public int DeviceWaitMilliseconds { get; set; } = ControlProtocol.DefaultAudioDeviceWaitMilliseconds; }

public sealed class CreateProfileRequest { public string Name { get; set; } = string.Empty; }

public sealed class RenameProfileRequest { public string ProfileId { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; }

public sealed class DeleteProfileRequest { public string ProfileId { get; set; } = string.Empty; }

public sealed class RepositorySnapshotRequest
{
    public RepositoryKind Repository { get; set; }
}

public sealed class RepositorySnapshot
{
    public RepositoryKind Repository { get; set; }
    public long Revision { get; set; }
    public string Json { get; set; } = string.Empty;
}

public sealed class RepositoryCommitRequest
{
    public RepositoryKind Repository { get; set; }
    public long ExpectedRevision { get; set; }
    public string Json { get; set; } = string.Empty;
}

public sealed class RepositoryCommitResult
{
    public bool WasConflict { get; set; }
    public RepositorySnapshot? Snapshot { get; set; }
}

/// <summary>
/// Supplies a repository with its current User Agent snapshot and accepts an
/// optimistic-concurrency commit. The shared repositories own their in-memory
/// models; the WinForms application only supplies this connection.
/// </summary>
public interface IUserAgentRepositoryConnection
{
    RepositorySnapshot GetRepositorySnapshot(RepositoryKind repository);

    RepositoryCommitResult CommitRepositorySnapshot(RepositoryCommitRequest request);
}

public sealed class UserAgentLaunchRequest
{
    public string UserSid { get; set; } = string.Empty;

    public int SessionId { get; set; }

    public Guid? OperationId { get; set; }
}

public sealed class UserAgentLaunchResult
{
    public bool IsSuccessful { get; set; }

    public string Message { get; set; } = string.Empty;
}

public sealed class UserAgentStopRequest
{
    public string UserSid { get; set; } = string.Empty;
    public int SessionId { get; set; }
    public int ProcessId { get; set; }
}

public sealed class RecoveryAdministrationRequest
{
    public string Action { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
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

public sealed class CreateUserSupportBundleRequest
{
    public string DestinationPath { get; set; } = string.Empty;

    public string MachineLogsStagingPath { get; set; } = string.Empty;

    public string MachineConfigurationStagingPath { get; set; } = string.Empty;

    public string[] MachineCollectionWarnings { get; set; } = Array.Empty<string>();
}

public sealed class UserSupportBundleResult
{
    public string DestinationPath { get; set; } = string.Empty;

    public string[] Warnings { get; set; } = Array.Empty<string>();
}

public sealed class RecoveryAdministrationRecord
{
    public DateTime OccurredUtc { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public string AdministratorSid { get; set; } = string.Empty;

    public int AdministratorSessionId { get; set; }

    public DisplayControlLease? ReleasedLease { get; set; }
}

public sealed class RecoveryAdministrationHistory
{
    public RecoveryAdministrationRecord[] Records { get; set; } = Array.Empty<RecoveryAdministrationRecord>();
}

public sealed class ControlResponse
{
    public bool IsSuccessful { get; set; }

    public ControlErrorCode ErrorCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public LeaseDecision? LeaseDecision { get; set; }

    public ControlServiceStatus? ServiceStatus { get; set; }

    public ProfileListResult? ProfileList { get; set; }

    public AudioProfileListResult? AudioProfileList { get; set; }

    public RepositorySnapshot? RepositorySnapshot { get; set; }

    public RepositoryCommitResult? RepositoryCommit { get; set; }

    public ApplyProfileResult? ApplyProfile { get; set; }

    public OperationStatus? OperationStatus { get; set; }

    public OperationStatus[] OperationStatuses { get; set; } = Array.Empty<OperationStatus>();

    public OperationDecision? OperationDecision { get; set; }

    public OperationDecision[] OperationDecisions { get; set; } = Array.Empty<OperationDecision>();

    public GameListResult? GameList { get; set; }

    public AppListResult? AppList { get; set; }

    public ShortcutListResult? ShortcutList { get; set; }

    public MessageListResult? MessageList { get; set; }

    public MessageSyncResult? MessageSync { get; set; }

    public ClientSyncResult? ClientSync { get; set; }

    public AnonymousMetricsSettings? AnonymousMetricsSettings { get; set; }

    public UserSupportBundleResult? UserSupportBundle { get; set; }
}

public sealed class ControlServiceStatus
{
    public AgentStatus[] Agents { get; set; } = Array.Empty<AgentStatus>();

    public DisplayControlLease? DisplayControlLease { get; set; }

    public RecoveryAdministrationRecord? LatestRecoveryAdministration { get; set; }

    public RecoveryAdministrationRecord[] RecoveryAdministrations { get; set; } = Array.Empty<RecoveryAdministrationRecord>();
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

    public bool IsReady { get; set; }
}
