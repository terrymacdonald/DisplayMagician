# DisplayMagician v4.0.0 Control Service Plan

## Purpose

DisplayMagician v4.0.0 changes DM from a single WinForms process that owns UI, persistence, and desktop operations into a coordinated desktop platform:

```text
Clients (WinForms now, WinUI 3/Stream Deck/mobile later)
                         |
                 Control Service
                         |
                  active User Agent
                         |
             User Agent and Windows runtime
```

The goal is to make future integrations possible without allowing arbitrary clients to manipulate displays, launch programs, or bypass DM's shortcut lifecycle.

## v4.0.0 Scope

v4.0.0 must deliver:

- A Windows Control Service, installed with DM.
- A per-user User Agent running in the interactive desktop session.
- A demand-start `DisplayMagician.SessionLauncher` fallback for an authorized request when the active user's Agent is absent.
- Existing WinForms UI and console commands functioning through the service.
- Safe migration from existing per-user AppData storage.
- Per-user profiles, audio profiles, shortcuts, and user settings.
- One machine-wide physical display/shortcut operation at a time.
- Existing game, Steam, Steam Big Picture, display, audio, and process monitoring continuing in the User Agent.
- Machine-owned anonymous metrics and client-sync scheduling, with per-user messages owned by the User Agent and exposed to clients through Control Service contracts.
- An initially disabled localhost REST API foundation for future paired integrations.
- Diagnostics, audit records, recovery handling, installer support, and tests.

v4.0.0 does **not** need to deliver:

- A Stream Deck plugin.
- WinUI 3 UI.
- Mobile application.
- Central/fleet control server.
- Internet- or LAN-exposed API.
- Profile sharing UI or profile ACL editor.
- Multiple users independently applying display profiles at the same time.
- Cross-session game launching.

## Agreed Product Decisions

| Area | v4.0.0 decision |
|---|---|
| Service installation | Install the Control Service with DM. External-control features remain disabled until configured and paired. |
| UI migration | Keep WinForms for v4.0.0. WinUI 3 comes after structural work is stable. |
| Legacy data | Migrate safely, preserve UUIDs, retain original legacy files by renaming each migrated file with a `.old` suffix. Never delete legacy data during migration. |
| DM administrator | The elevated installer user administers installation, service configuration, and recovery. They do not automatically own other users' profiles. |
| User data | Each Windows user owns their own profiles, audio profiles, shortcuts, and user settings. |
| Display authority | Only the current active physical-console user may apply a display profile or run a game shortcut. One machine-wide operation can run at a time. |
| Agent startup | WinForms starts and retains the hidden, tray-less User Agent while WinForms is open or minimised to its existing tray icon. On full WinForms exit, an idle Agent stops; an Agent with an active shortcut, game monitor, apply, or recovery stays until safe completion. The demand-start Session Launcher starts a missing Agent only for an already-authorized request targeting the active console user. |
| Locked session | Applying a profile is allowed while locked. Starting a new game/application shortcut is denied while locked. Existing shortcuts continue to be monitored and restored. |
| Recovery | Agent/service loss during temporary state requires safe restoration before further display-changing work. |
| Recovery administration | A Service Recovery page under Settings > Diagnostics is visible only to an elevated DM administrator. Normal restoration retries only through the affected user's Agent after sign-in. An emergency, UAC-elevated `Force release DM control` action requires an explicit confirmation phrase, releases the lease, marks recovery abandoned, and creates a high-severity audit record. |
| Pairing | Explicit, user-confirmed pairing. Exact pairing UX is technology-specific and deferred behind abstractions. |
| Metrics/messages | Anonymous metrics and client-sync scheduling remain machine-level responsibilities. The User Agent owns each user's message cache, read state, content retrieval, and message RPCs; all UI clients view it through Control Service routing. |
| Remote scope | No public endpoint in v4.0.0. Localhost integration foundation only. |
| Build version | The root `version.json` is the sole version authority. Every shipped v4 executable, library, service registration, installer/package, diagnostics record, and protocol registration derives its version from the same Nerdbank.GitVersioning build metadata. |

## Core Rules

```text
Control Service: decides, authorizes, coordinates, persists, audits.
User Agent: executes all user-session desktop work and reports progress.
UI/API clients: request actions and render results.
```

The Control Service must never launch Steam, inspect Big Picture windows, show dialogs, show toasts, or directly own interactive desktop monitoring. That work remains in the User Agent because it runs in the logged-in user's session. The Control Service may request `DisplayMagician.SessionLauncher` to start an Agent, but never launches an interactive desktop process itself.

### Session rules

```text
- More than one Windows user may be signed in.
- More than one Agent may register with the service.
- Only the active physical-console user may acquire the display-control lease.
- An RDP or fast-switched background user can manage their own stored data but cannot change physical displays or start a game shortcut.
- The service never automatically hands display control to another user while a temporary shortcut is active.
```

## Target Projects and Dependencies

Create the following projects:

```text
DisplayMagician.Contracts
DisplayMagician.ControlService
DisplayMagician.SessionLauncher
DisplayMagician.UserAgent
DisplayMagician.WinForms
DisplayMagician.Console
```

| Project | Responsibility |
|---|---|
| `DisplayMagician.Contracts` | Versioned requests, responses, events, protocol constants, error codes, and stable cross-process enums. No UI, hardware, files, or static application state. |
| `DisplayMagician.ControlService` | Windows Service, ownership, authorization, machine queue, persistence coordination, audit, local API, service health, and Agent routing. |
| `DisplayMagician.SessionLauncher` | Demand-start `LocalSystem` broker. Accepts only authenticated local requests from Control Service; starts the signed User Agent in one already-authorized interactive session and returns launch status. Never accepts remote clients or performs display work. |
| `DisplayMagician.UserAgent` | Interactive-session executor: display/audio changes, game library loading, Steam game monitoring, vendor/native display runtime, shortcut lifecycle, per-user message storage/sync, and notifications. |
| `DisplayMagician.WinForms` | Current designer-backed UI, converted to a Control Service client. |
| `DisplayMagician.Console` | Current command-line interface, converted to a Control Service client. |

Dependency direction:

```text
WinForms / Console / UserAgent / ControlService --> Contracts
WinForms / Console                           --> Control Service IPC client
ControlService                               --> User Agent command/event channel
ControlService                               --> SessionLauncher launch request channel
SessionLauncher                              --> UserAgent process start only
```

Do not allow User Agent runtime code to depend on WinForms, the service host, REST, or static `Program` UI state.

## Build Versioning

The existing `build_displaymagician.ps1` invokes MSBuild for the full solution. Nerdbank.GitVersioning discovers the root `version.json` during that build and produces the shared version metadata. Its Git commit height since the most recent base-version update provides the build/revision component, so builds after a `version.json` change receive a common increasing version number without manually setting a per-build number. The script triggers this process; Nerdbank.GitVersioning performs the calculation. For v4.0.0, preserve this as the only release-version mechanism; do not add component-specific version constants or manually edit assembly versions for a release.

Requirements:

- Keep the release base version in the root `version.json` (currently `4.0.0`).
- Treat the Git commit height since that base-version update as the common build/revision number. Release builds must retain the Git history required for Nerdbank.GitVersioning to calculate it correctly; do not shallow-clone or override it with a manually supplied revision.
- Add the existing Nerdbank.GitVersioning package/configuration to every project that produces a shipped v4 binary: WinForms, Console, Contracts, ControlService, SessionLauncher, and UserAgent.
- Ensure each project emits assembly, file, and informational versions from the generated build metadata. Respect legacy projects that intentionally provide their own assembly attributes by retaining their existing `ThisAssembly`-based mechanism rather than enabling duplicate generated attributes.
- Replace hard-coded component registration versions (for example the User Agent's development string) with the generated assembly/file version so the Control Service records the actual installed build.
- Pass the same generated version into service installation metadata, MSI/package versioning, diagnostic bundles, audit records, and update/metrics payloads. Where an installer format has a different version shape, transform the same source value; do not introduce another authoritative version number.
- Build the complete solution through `build_displaymagician.ps1` for release candidates. A direct project build is permitted for development but is not release verification.
- Add a build/version test or release verification step that asserts all shipped v4 binaries report the same base version from `version.json`, with only the expected build/revision metadata differing.

## Storage and Ownership

### New storage layout

```text
C:\ProgramData\DisplayMagician\
  Machine\
    ServiceSettings.json
    Installation.json
    Audit\
    Messages\
    Metrics\
    Backups\
    Logs\
  Users\
    <Windows-SID>\
      Profiles\
      AudioProfiles\
      Shortcuts\
      Settings\
      Messages\
      Backups\
      Logs\
```

Use Windows SID as the user identifier; do not use account display names or account names as storage keys.

### Data classification

| Data | Scope |
|---|---|
| Display profiles, audio profiles, shortcuts, UI settings, Agent startup preference, message read state | Per user |
| Service configuration, install identity, telemetry consent/counters, client-sync cache, audit, recovery records | Machine |

### Persistence requirements

- Existing JSON models are backwards-compatible contracts.
- Preserve serialized member names and unrelated settings.
- Update data versions and add explicit `ConfigMigrationRunner` migrations when required.
- Back up before migration.
- Use atomic writes: write temporary file, flush, replace, retain backup.
- Repositories own data writes; REST clients and service handlers do not hand-edit JSON.

### Migration procedure

For each user's first v4 run:

1. Find current DM AppData files.
2. Create a backup in `ProgramData\DisplayMagician\Users\<SID>\Backups`.
3. Import profiles, audio profiles, shortcuts, user settings, and message state through repository/migration helpers.
4. Validate that migrated data can load from the new location.
5. Atomically persist migrated data.
6. Rename each successfully migrated source file to `<filename>.old`.
7. Write a migration marker with original path, imported file list, version, and timestamp.
8. Leave failed source files untouched and provide a retry/recovery path.

Never rename an entire legacy directory blindly. Never rename a file until its matching destination has been verified.

## Control Service

### Responsibilities

- Start at boot and maintain machine-level state.
- Track Windows session changes and registered User Agents.
- Verify caller SID/session identity.
- Enforce one display-control lease and one state-changing operation queue.
- Authorize requests and route approved execution jobs to the appropriate Agent.
- Own profile repository coordination and machine-level data.
- Host disabled-by-default localhost REST endpoints.
- Own audit records, diagnostic aggregation, recovery coordination, client-sync scheduling, and anonymous metrics; route User Agent-owned messages without persisting their content or read state.

### Service account

Control Service runs as `LocalService`. `DisplayMagician.SessionLauncher` is the sole `LocalSystem` component because Windows requires that trust level to obtain an existing interactive user's token for on-demand process launch. Its IPC ACL accepts only Control Service, it accepts only a session ID plus fixed signed Agent executable/arguments, and it stops when idle. Do not run Control Service as `LocalSystem`.

### Display-control lease

Persist a lease record:

```text
OwnerUserSid
OwnerSessionId
AcquiredUtc
LastHeartbeatUtc
ActiveOperationId
IsRecoveryRequired
```

Lease acquisition requires:

1. Caller is the active console user.
2. Matching Agent is connected and healthy.
3. No other display/shortcut operation is in progress.
4. No unresolved temporary-state recovery exists.

Never release a lease merely because a short timeout elapsed while a shortcut may still be running.

## User Agent

### Startup lifecycle

The hidden User Agent has no tray icon. WinForms owns the existing user-facing tray icon and starts/retains the Agent while it remains open, including when minimised to tray. Do not create a scheduled task for the User Agent.

```text
WinForms `StartOnBootUp` / `MinimiseOnStart`:
  - Continue to control whether WinForms starts and is minimised at sign-in.
  - When WinForms starts, it starts/connects the hidden Agent in the same user session.
  - WinForms minimised to tray remains open, so the Agent remains connected.

WinForms full exit:
  - Stop the Agent only when it is idle and has no apply, shortcut/game monitor, or recovery work.
  - An active operation keeps the Agent alive until safe completion/recovery.

Authorized request with missing Agent:
  - Control Service verifies the request's user scope and active-console session.
  - Control Service asks SessionLauncher to start the Agent in that same session.
  - Service waits for identity-verified registration before routing work.
  - If no eligible signed-in active user exists, return `AgentUnavailable`.
```

### Responsibilities

- Run in a logged-in interactive Windows session.
- Apply profile and audio changes using existing DM/native code.
- Run existing shortcut lifecycle, including pre/after/stop programs.
- Load game libraries and monitor normal game process trees.
- Handle Steam Big Picture mode detection.
- Capture/restore temporary state.
- Show user-session notifications.
- Own per-user message content, read state, and refresh; return client-safe message views through Control Service routing.
- Send Agent heartbeat, operation progress, result, and recovery status to the service.

### Registration

The Agent sends version, process ID, session ID, claimed SID, capabilities, startup mode, operation state, and recovery state. The service must obtain the actual pipe-client Windows identity and session from the OS and reject mismatches. Never trust identity fields supplied by the Agent alone.

### Steam Big Picture

Big Picture must appear to users as a normal game in the existing picker.

- Add it as a synthetic `SteamGame`, not a new generic `Game` subtype.
- Use a reserved non-colliding string ID such as `dm:steam:big-picture`.
- `SteamGame.Start` recognises the reserved ID and launches Big Picture.
- `SteamGame.IsRunning` recognises the reserved ID and uses specialised mode detection.
- Real Steam games retain their existing process-tree behaviour.
- All detection runs in the User Agent, never in the Control Service.

## Internal IPC

Use authenticated Windows named pipes for machine-internal traffic:

```text
WinForms --> Control Service
Console  --> Control Service
UserAgent <--> Control Service
```

Use a versioned, length-prefixed JSON or protobuf envelope:

```text
ProtocolVersion
MessageType
RequestId
Payload
```

Suggested logical channels:

```text
DisplayMagician.ControlService.v1
DisplayMagician.AgentEvents.v1
```

The service validates named-pipe client SID/session identity. Pipe ACLs must prevent unauthorised local clients from connecting.

## Contracts

### Required requests

```text
GetServiceStatus
GetCurrentUserStatus
AcquireDisplayControl
ReleaseDisplayControl

ListProfiles
GetProfile
CreateProfile
UpdateProfile
DeleteProfile
ApplyProfile

ListShortcuts
GetShortcut
CreateShortcut
UpdateShortcut
DeleteShortcut
RunShortcut
CancelOperation

GetOperation
ListRecentOperations
SubscribeToEvents
GetAgentStatus
GetDiagnosticsSummary
```

### Required errors

```text
NotAuthenticated
NotAuthorized
ExternalControlDisabled
NoActiveConsoleUser
NotActiveConsoleUser
DisplayControlHeldByAnotherUser
AgentUnavailable
SessionLocked
OperationAlreadyRunning
ProfileNotFound
ShortcutNotFound
ValidationFailed
ExecutionFailed
RecoveryRequired
```

### Operation states

```text
Queued
Authorizing
WaitingForAgent
Running
WaitingForGame
Restoring
Succeeded
Failed
Cancelled
RecoveryRequired
```

### Multi-controller operation status

The Control Service is the sole operation-status hub. The User Agent publishes authenticated progress and completion updates to it; it assigns a monotonically increasing sequence number per operation, persists active state and a bounded recent history, and fans the same status out to every authorised controller. A client must first fetch its visible operation snapshot and then subscribe from its last sequence number, so a late-connecting WinForms client, Stream Deck plugin, phone/watch app, or paired remote DM desktop app immediately shows the current phase and cannot miss an update during connection.

Operation status visibility is separate from command authority. Controllers can observe an operation only when their identity is authorised for that DM user/machine; the active-console and display-control rules still determine whether any controller may start or change an operation. The first local WinForms implementation may poll the hub. REST clients use the same records through an operation-status endpoint and later an SSE/WebSocket subscription, rather than communicating directly with a User Agent.

All state-changing requests return an operation ID. A successful request submission does not imply operation completion.

### Events

```text
AgentRegistered
AgentHeartbeat
AgentDisconnected
DisplayControlAcquired
DisplayControlReleased
OperationStarted
OperationProgress
OperationCompleted
OperationFailed
TemporaryStateCaptured
TemporaryStateRestored
RecoveryRequired
NewMessagesAvailable
UpdateAvailable
```

Every event includes UTC time, severity, stable code, human-readable message, SID, session ID, and operation ID when applicable.

## WinForms and Console Migration

Keep existing forms designer-backed. Do not rebuild forms at runtime.

Refactor WinForms in this order:

1. Service/Agent status display.
2. Profile list.
3. Apply profile operation and progress.
4. Create/edit/delete profiles.
5. Shortcut list and shortcut execution.
6. Shortcut editor.
7. Audio-profile operations.
8. Settings, migration, and diagnostics UI.

WinForms remains responsible for validation presentation, modal dialogs, and marshaling updates to the UI thread. User Agent code returns structured results and never shows `MessageBox` dialogs.

### WinForms repository cache transition

To retain the mature WinForms forms while moving authority to the User Agent, `ProfileRepository`, `AudioProfileRepository`, and `ShortcutRepository` in the WinForms process become interactive in-memory caches. They do not read or write authoritative files after v4 migration.

1. The Agent loads and migrates the authoritative per-SID repositories.
2. A versioned snapshot request returns the existing repository JSON schema plus a content revision. The contract wraps the JSON; it does not expose arbitrary paths or raw filesystem operations.
3. WinForms imports the snapshot into its existing repository collections solely for rendering, validation, and edit state. Imported objects are never considered persisted locally.
4. A form changes cached objects and explicitly submits a versioned repository commit or targeted mutation to the Agent.
5. The Agent validates the caller and expected revision, atomically writes the authoritative repository, and returns the updated snapshot/revision. Domain-reference and hardware-ownership validation is added alongside the Agent-owned shortcut runner.
6. WinForms replaces its cache from the returned snapshot. If a commit fails, it keeps the dirty edit in memory, clearly reports that it was not saved, and offers reload/retry rather than silently writing local files.

Display/audio apply already route through the Agent. Game launch, process monitoring, temporary-state capture, restoration, and shortcut runtime are Agent-owned through `ShortcutRunner`; the legacy WinForms `ShortcutRepository.RunShortcut` path has been removed. The WinForms cache must not invoke `ProfileItem` display APIs or `AudioProfileItem.TrySetActive`.

Prototype status: `ProfileRepository`, `AudioProfileRepository`, and `ShortcutRepository` now each expose `ConnectToUserAgent`. They deserialize Agent snapshots into their existing item collections, retain the current revision, and save through optimistic Agent commits. Forms establish the connection but do not resolve or configure authoritative profile, audio-profile, or shortcut storage paths.

For shortcut extraction, use the following names consistently:

- `ShortcutStore`: Agent-owned shortcut persistence, snapshots, revisions, and definition validation.
- `ShortcutRunner`: Agent-owned shortcut execution, process monitoring, temporary-state capture, and restoration.
- `ShortcutClient`: WinForms, Console, and future API-facing request adapter.
- `ShortcutEditor`: the existing WinForms editing workflow.

`DisplayMagician.ConfigurationDefinitions` owns portable persisted definitions, schema versions, JSON conversion, and pure configuration validation. It does not access files, hardware, processes, named pipes, or WinForms. `DisplayMagician.Contracts` owns transport messages and shared cross-process enums, including shortcut category/permanence/process priority, game launch mode, and supported game-library identifiers.

The User Agent owns game-library discovery, game launch, `Game.IsRunning`, process-tree monitoring, and vendor/native display runtime directly. Do not create a separate GameLibraries project and do not put this Windows runtime behaviour in ConfigurationDefinitions. WinForms receives game-library/game views through Agent contracts as the direct legacy implementation is retired.

### Automatically detected game starts

Game shortcuts persist a `GameLaunchMode`: `StartGame` (the existing default) or `DetectGameRunning`. The latter is for a user who starts the selected game from Steam, another launcher, a desktop shortcut, or another external source. It is not an instruction to start a second game process. It does not prevent the user manually choosing **Run Shortcut**: a manual `StartShortcut` request always runs the configured game normally.

`ShortcutRunner` must register every valid `DetectGameRunning` shortcut when the User Agent becomes active. It uses the same `Game.IsRunning` and process-tree/alternative-executable detection currently used after a normal game launch. When the detector sees a new process for that game, it acquires display control, applies the shortcut's pre-game work, monitors that already-running process until it exits, and finally performs normal rollback and post-game work. The runner must reject conflicting enabled automatic shortcuts for the same game/monitor target, and ignore processes already running when it registers so an Agent restart cannot incorrectly trigger a shortcut.

Before manually running a `DetectGameRunning` shortcut, `ShortcutRunner` must temporarily unregister that shortcut's detector. This prevents the game process started by the manual run from creating a competing automatic run. The runner must re-register it after the run completes, is cancelled, or fails to start. Both paths enter the same runtime lifecycle immediately after the launch decision: automatic detection enters with an already-running process; manual start enters with a process to launch.

The WinForms `ShortcutEditor` will expose this as **Automatically detect game running (do not start game)** once `ShortcutRunner` owns shortcut execution. Until then it must not present an option that the legacy WinForms runner cannot safely honour.

Refactor the console so existing commands remain compatible where practical:

```text
DisplayMagicianConsole ChangeProfile "TV Gaming"
```

The console must call the Control Service, respect ownership rules, print structured failures, return useful exit codes, and never modify profile files directly.

## REST Foundation

REST is disabled by default and is for future external/local integrations. Internal WinForms, Console, and Agent coordination use named pipes.

When enabled, bind only to:

```text
127.0.0.1
::1
```

Required initial endpoints:

```text
GET  /v1/identity
GET  /v1/status

POST /v1/pairing/request
POST /v1/pairing/complete
POST /v1/pairing/revoke

GET  /v1/profiles
POST /v1/profiles/{profileId}/apply

GET  /v1/operations/{operationId}
POST /v1/operations/{operationId}/cancel
```

Create abstractions now:

```text
IPairingProvider
IPairedClientRepository
IClientAuthorizationService
```

Pairing grants scoped credentials such as `profiles:read`, `profiles:apply`, `shortcuts:read`, and `shortcuts:run`. Never expose arbitrary executable, command-line, raw display, registry, or path-based operations.

## Current Heartbeats, Messages, and Metrics

Current DM uses `AnonymousMetricsService`, `ClientSyncService`, and `MessageSyncService` from the WinForms `Program` process. v4 separates their ownership: metrics and client-sync scheduling become machine-level service responsibilities, while messages are per-user Agent data exposed through contracts.

```text
User Agent --> Control Service
  Local health heartbeat, active-runtime updates, operation progress.

Control Service --> DM metrics endpoint
  Existing opted-in anonymous metrics heartbeat.

Control Service --> DM sync endpoint
  Existing machine-level client sync and update metadata gathering.

User Agent --> DM message endpoint
  Per-user message content and media gathering, message cache, read state, and client-safe message views.
```

The active User Agent/UI displays user-facing message/update notifications. The service schedules and emits machine-level update events; the Agent validates, stores, and returns the current user's messages.

## Logging, Audit, and Recovery

Use NLog with existing `ClassName/MethodName: message` conventions and include operation ID, SID, session, profile/shortcut UUID, and exception context when available.

Suggested log locations:

```text
C:\ProgramData\DisplayMagician\Machine\Logs\ControlService\
C:\ProgramData\DisplayMagician\Machine\Logs\UserAgents\
C:\ProgramData\DisplayMagician\Machine\Audit\
```

Write major service lifecycle/fatal errors to Windows Event Viewer under `DisplayMagician.ControlService`. Do not flood Event Viewer with normal progress messages.

Add a WinForms Diagnostics page:

```text
- Service status
- Active console user/session
- Agent health
- Current operation
- Recent operations and warnings
- Open log folder
- Copy redacted diagnostic bundle
```

Never place API tokens, pairing secrets, passwords, bot tokens, or sensitive unredacted arguments in logs or diagnostic bundles.

Before applying temporary state, persist a recovery record. Clear it only after the Agent confirms restoration. If an Agent dies, block new display-changing work until a valid Agent restores the recorded state.

### Recovery administration interface

Add a Service Recovery page under **Settings > Diagnostics**. It is available only to the elevated DM administrator and displays the current lease, recovery record, affected user/session, the last Agent heartbeat, and the most recent restoration result. A normal retry is sent only to the affected user's Agent after that user signs in; the service must not attempt desktop recovery itself or impersonate another user.

`Force release DM control` is an emergency action, not normal workflow. It must request UAC elevation, clearly explain that DM can no longer safely restore the previous temporary state, require the administrator to type a confirmation phrase, set the recovery record to forced/abandoned, release the lease, and write a high-severity audit entry. The current active user can then manually apply a known-good profile. A machine-wide fallback profile is deferred.

## Installer and Deployment

Use the existing installer/EXE/MSI-style deployment for v4.0.0. Do not block this structural release on MSIX or WinUI 3.

Installer requirements:

- Require elevation/UAC.
- Install and configure Control Service.
- Install SessionLauncher as a demand-start `LocalSystem` service with an IPC ACL restricted to Control Service.
- Create ProgramData storage and ACLs.
- Integrate hidden User Agent startup/shutdown with WinForms lifecycle: start/retain it while WinForms is visible or minimised to tray, and stop it only after full UI exit when idle. Do not use a scheduled task.
- Install WinForms and Console clients.
- Install the User Agent and Control Service using the build version derived from the root `version.json`.
- Preserve and migrate user data safely.
- Stop/start service safely during upgrades.
- Preserve user data by default on uninstall.
- Provide useful installer/migration failure reporting.

Future packaged WinUI 3 remains viable: a full-trust WinUI 3 desktop client can use named pipes and Win32 APIs. User-session monitoring remains in the User Agent regardless of UI technology.

## Implementation Phases

### Current migration status

- [x] `Processes` and `GameLibraries` have physical UserAgent-owned source folders; UserAgent no longer links either source tree from WinForms and builds independently.
- [x] `GameView` is returned by the Agent and the WinForms shortcut editor uses it for game selection, icon discovery, alternate-executable browsing, and persisted shortcut identity.
- [x] Per-user messaging is Agent-owned: list, read-state, and refresh commands route through Control Service; WinForms no longer reads the message store directly.
- [x] Cross-process shortcut/game enums are declared once in `DisplayMagician.Contracts`; persisted numeric values are unchanged.
- [x] WinForms shortcut launches, hotkeys, command-line activation, and tray actions route only through Agent `StartShortcut`; there is no local execution fallback.
- [x] Delete the now-unreachable legacy `ShortcutRepository.RunShortcut` implementation and its direct process/game runtime dependencies.
- [ ] The WiX payload does not yet publish/install ControlService, SessionLauncher, and UserAgent together.

### Phase A — Structure and IPC

- [x] Create Contracts, ControlService, and UserAgent projects.
- [x] Add protocol versioning and common result/error models.
- [x] Implement authenticated named-pipe transport.
- [x] Complete production pipe ACL and remote-rejection enforcement; verify caller SID/session/process identity for every client and Agent connection before entering Phase E.
- [x] Implement Agent registration, heartbeat, and diagnostics status.
- [x] Implement active-console and machine-operation lease state.
- [x] Apply the root `version.json`/Nerdbank.GitVersioning configuration to all new v4 shipped projects and remove hard-coded Agent/Service version strings.
- [x] Add the demand-start LocalSystem SessionLauncher project and its Control Service-only IPC contract.

**Exit criteria:** Service can show a verified Agent SID/session and deny a second conflicting display lease.

### Phase B — Storage and migration

- [x] Add ProgramData storage abstraction and per-SID paths.
- [x] Add safe write/backup helpers.
- [x] Add v4 migration runner and migration markers; integrate legacy `ConfigMigrationRunner` once repositories use the new storage paths.
- [x] Add opt-in repository storage-path configuration; do not activate it before the User Agent migration hand-off is implemented.
- [x] Add an identity-verified User Agent migration request; keep it explicit until repository ownership moves to the Agent.
- [x] Redirect current repository paths only when a completed, validated per-SID migration marker is present.
- [x] Migrate display profiles first.
- [x] Migrate audio profiles, shortcuts, user settings, and message state.
- [x] Rename each successful legacy source file to `.old`.

**Exit criteria:** Existing DM user sees unchanged data after migration; legacy files remain as `.old`; repeated startup does not import duplicates.

### Phase C — First end-to-end profile operation

- [x] Implement `ListProfiles` through service/repositories.
- [x] Refactor WinForms profile list to use service requests.
- [x] Implement `ApplyProfile` operation routing.
- [x] Start and retain the hidden User Agent while WinForms is visible or minimised to its tray icon; stop it after full WinForms exit only when idle.
- [x] When an authorized request has no Agent, use SessionLauncher to start it in the active user's session and wait for verified registration.
- [x] Agent invokes existing display-application behaviour.
- [x] Agent returns final profile-operation result; WinForms displays it. Progress events remain part of the shortcut/recovery phase.

**Exit criteria:** Active console user applies their own migrated profile; another session is denied.

### Phase D — Shortcut lifecycle and recovery

- [x] Move the Agent shortcut execution foundation into UserAgent.
- [x] Preserve pre/after/stop programs, audio volume overrides, temporary restoration, and recovery records in the Agent runner.
- [x] Enforce locked-session policy for new shortcut starts and publish operation progress.
- [x] Move normal game discovery and game/process runtime source ownership into UserAgent.
- [x] Route all desktop shortcut execution through `StartShortcut` with no local execution fallback.
- [x] Delete the now-unreachable WinForms `ShortcutRepository.RunShortcut` implementation and its direct process/game runtime dependencies.
- [x] Add client cancel-operation protocol and complete existing-game lifecycle, cancellation, and recovery parity testing.

**Exit criteria:** An ordinary game shortcut applies temporary state, monitors correctly, handles cancellation, and restores state after exit.

### Phase E — Security, background ownership, and diagnostics

- [ ] Move anonymous metrics ownership to service.
- [ ] Move machine-level client-sync/update scheduling to service.
- [x] Move per-user message gathering/storage/read state to UserAgent and expose it through contracts.
- [x] Move WinForms startup-message polling, unread indicators, and release-note lookup to Agent message views; stop direct desktop message-file access.
- [ ] Remove the legacy desktop messaging implementation after client-sync scheduling has moved to Control Service.
- [ ] Forward update/message events to Agent/UI.
- [ ] Add audit records, Event Viewer service errors, diagnostic bundle support, and the administrator-only Service Recovery page.
- [x] Remove the `--agent-hosted-operation` desktop-executable bridge; normal Agent profile work remains in `UserProfileOperationService`.
- [ ] Remove WinForms `Program` client-sync/metrics timers, message polling, message-file access, and the duplicated `Messaging` services after their Service/Agent replacements are live.
- [ ] Remove desktop AppData persistence fallbacks for Agent-owned profiles, audio profiles, shortcuts, and messages; retain only interactive in-memory caches backed by Agent snapshots and commits.
- [ ] Refactor shortcut/editor UI to consume Contracts/ConfigurationDefinitions view data, then remove duplicated WinForms `GameLibraries`, `Processes`, and `AppLibraries` runtime sources. Preserve only UI-specific helpers and pure configuration/editing types in the desktop project.
- [ ] Add a retirement verification scan/test that fails when WinForms again references Agent-owned runtime execution, storage, messaging, game-library, or process-monitoring implementations.

**Exit criteria:** Local clients and Agents are identity-verified and remote callers are rejected; one machine produces one metrics/client-sync schedule regardless of UI/Agent count; user messages remain per-user Agent data; WinForms is a UI/cache and Control Service client rather than a second runtime owner; diagnostics and recovery actions are auditable.

### Phase F — Deployment hardening

- [ ] Package and install ControlService, SessionLauncher, and UserAgent with the WinForms and Console clients.
- [ ] Create ProgramData directories and least-privilege ACLs for machine/service and per-SID Agent storage.
- [ ] Configure Control Service installation, start/stop, failure recovery, upgrade, repair, and uninstall behaviour.
- [ ] Verify fresh install and upgrade preserve migrated user data and restore service/Agent connectivity.
- [ ] Verify all installed components report the common build version derived from root `version.json`.

**Exit criteria:** A fresh install, upgrade, repair, and uninstall/reinstall deploy and recover the Control Service, SessionLauncher, UserAgent, WinForms, and Console without losing user data.

### Phase G — Local REST foundation

- [ ] Add disabled-by-default loopback API host.
- [ ] Add identity/status endpoints.
- [ ] Add pairing approval, protected credential storage, expiry/revocation, provider abstractions, and scoped credentials.
- [ ] Add profile list/apply and operation-status endpoints.
- [ ] Add authorization, rate-limit, audit, and loopback-only tests.

**Exit criteria:** Paired local test client can list/apply permitted profiles; unpaired client cannot.

### Phase H — Final v4.0.0 work — Steam Big Picture

- [ ] Add synthetic Steam Big Picture `SteamGame` behaviour, including Agent-side launch and running detection.
- [ ] Add Big Picture lifecycle and temporary-state restoration parity tests.

**Exit criteria:** Big Picture behaves as a normal game shortcut and restores temporary state after it exits.

### Phase I — Final verification and release readiness

- [ ] Run session/hardware/manual matrix.
- [ ] Build Diagnostics/support documentation.
- [ ] Run build, tests, formatting, and `git diff --check`.
- [ ] Prepare v4.0.0 release notes and migration guidance.

**Exit criteria:** v4.0.0 meets all prototype acceptance criteria below.

## Test Matrix

### Unit tests

- Contract serialization and protocol compatibility.
- SID/session/lease authorization rules.
- Operation queue serialization.
- Recovery state transitions.
- Migration and `.old` rename safety.
- Atomic writes/backups.
- Pairing scope validation.
- Metrics/client-sync schedules and backoff.

### Integration tests

- Service and Agent registration.
- First active console Agent acquires control.
- Second user cannot apply profile/run shortcut.
- WinForms and Console route through service.
- Agent events reach clients.
- Agent/service restart recovery.
- Locked-session profile apply succeeds.
- Locked-session game start is denied.
- REST rejects unpaired/incorrectly scoped clients.

### Manual hardware/session tests

- Single and multi-monitor configurations.
- NVIDIA, AMD, Intel, and mixed GPUs.
- Audio profile enabled/disabled.
- Steam installed/absent/already running for ordinary Steam game lifecycle.
- Big Picture lifecycle after the preceding v4 phases are complete.
- Standard and administrator accounts.
- Lock/unlock, fast user switching, and RDP.
- Agent automatic-start opt-out.

## v4.0.0 Acceptance Criteria

v4.0.0 is ready when:

- [ ] Control Service installs and starts reliably.
- [ ] Hidden User Agent has no tray icon and remains connected while WinForms is visible or minimised to tray; full WinForms exit stops it only when idle.
- [ ] An authorized request starts a missing Agent through SessionLauncher only in the active console user's existing session.
- [ ] Existing user data migrates safely and source files remain with `.old` suffixes.
- [ ] Users see and manage only their own profiles/shortcuts.
- [ ] Only active console user may apply profiles/run game shortcuts.
- [ ] Only one physical-state operation runs at a time.
- [ ] Existing display/audio/game behaviour executes in User Agent.
- [ ] Ordinary Steam game shortcuts monitor correctly and restore temporary state after exit.
- [ ] Steam Big Picture shortcuts monitor correctly and restore temporary state after exit.
- [ ] WinForms and Console cannot bypass service rules.
- [ ] Metrics/messages/client sync have a single machine owner.
- [ ] Local REST is unavailable until enabled and paired.
- [ ] Logs, audit, diagnostics, and recovery are usable.
- [ ] All affected projects build and relevant tests pass.
- [ ] All shipped v4 components report the common build version derived from root `version.json`.

## Future Work After v4.0.0

```text
v4.x: Stream Deck plugin using the localhost paired API.
v4.x: WinUI 3 client replacing WinForms incrementally.
v5+: Explicit profile sharing/ACLs and display-control handover.
v5+: Central game-centre service with outbound authenticated connection.
v5+: Android/iOS client through a secure relay.
```

