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
                 DM Engine and hardware
```

The goal is to make future integrations possible without allowing arbitrary clients to manipulate displays, launch programs, or bypass DM's shortcut lifecycle.

## v4.0.0 Scope

v4.0.0 must deliver:

- A Windows Control Service, installed with DM.
- A per-user User Agent running in the interactive desktop session.
- Existing WinForms UI and console commands functioning through the service.
- Safe migration from existing per-user AppData storage.
- Per-user profiles, audio profiles, shortcuts, and user settings.
- One machine-wide physical display/shortcut operation at a time.
- Existing game, Steam, Steam Big Picture, display, audio, and process monitoring continuing in the User Agent.
- Machine-owned anonymous metrics, client sync, and message gathering.
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
| Agent startup | Reuse the existing per-user `StartOnBootUp` and `MinimiseOnStart` behaviour. The existing HKCU Run entry starts DM at Windows sign-in; DM starts/keeps its User Agent ready. When disabled, start the Agent with the WinForms UI and stop it after UI close when no operation is active. |
| Locked session | Applying a profile is allowed while locked. Starting a new game/application shortcut is denied while locked. Existing shortcuts continue to be monitored and restored. |
| Recovery | Agent/service loss during temporary state requires safe restoration before further display-changing work. |
| Recovery administration | A Service Recovery page under Settings > Diagnostics is visible only to an elevated DM administrator. Normal restoration retries only through the affected user's Agent after sign-in. An emergency, UAC-elevated `Force release DM control` action requires an explicit confirmation phrase, releases the lease, marks recovery abandoned, and creates a high-severity audit record. |
| Pairing | Explicit, user-confirmed pairing. Exact pairing UX is technology-specific and deferred behind abstractions. |
| Metrics/messages | Existing anonymous metrics, client sync, and message gathering become machine-level Control Service responsibilities. |
| Remote scope | No public endpoint in v4.0.0. Localhost integration foundation only. |
| Build version | The root `version.json` is the sole version authority. Every shipped v4 executable, library, service registration, installer/package, diagnostics record, and protocol registration derives its version from the same Nerdbank.GitVersioning build metadata. |

## Core Rules

```text
Control Service: decides, authorizes, coordinates, persists, audits.
User Agent: executes user-session desktop work and reports progress.
DM Engine: reusable profile and shortcut orchestration.
UI/API clients: request actions and render results.
```

The Control Service must never launch Steam, inspect Big Picture windows, show dialogs, show toasts, or directly own interactive desktop monitoring. That work remains in the User Agent because it runs in the logged-in user's session.

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
DisplayMagician.Engine
DisplayMagician.ControlService
DisplayMagician.UserAgent
DisplayMagician.WinForms
DisplayMagician.Console
DisplayMagicianShared
```

| Project | Responsibility |
|---|---|
| `DisplayMagician.Contracts` | Versioned requests, responses, events, protocol constants, and error codes. No UI, hardware, files, or static application state. |
| `DisplayMagician.Engine` | Reusable validation and orchestration interfaces. No WinForms, REST, named-pipe transport, or dialogs. |
| `DisplayMagician.ControlService` | Windows Service, ownership, authorization, machine queue, persistence coordination, audit, local API, service health, and Agent routing. |
| `DisplayMagician.UserAgent` | Interactive-session executor: display/audio changes, game library loading, Steam/Big Picture monitoring, shortcut lifecycle, and notifications. |
| `DisplayMagician.WinForms` | Current designer-backed UI, converted to a Control Service client. |
| `DisplayMagician.Console` | Current command-line interface, converted to a Control Service client. |
| `DisplayMagicianShared` | Existing GPU/vendor/native integration remains here initially. Do not rewrite GPU libraries as part of the structure change. |

Dependency direction:

```text
WinForms / Console / UserAgent / ControlService --> Contracts
UserAgent / ControlService                   --> Engine
Engine / UserAgent                           --> DisplayMagicianShared
WinForms / Console                           --> Control Service IPC client
ControlService                               --> User Agent command/event channel
```

Do not allow Engine code to depend on WinForms, the service host, REST, or static `Program` UI state.

## Build Versioning

The existing `build_displaymagician.ps1` invokes MSBuild for the full solution. Nerdbank.GitVersioning discovers the root `version.json` during that build and produces the shared version metadata. Its Git commit height since the most recent base-version update provides the build/revision component, so builds after a `version.json` change receive a common increasing version number without manually setting a per-build number. The script triggers this process; Nerdbank.GitVersioning performs the calculation. For v4.0.0, preserve this as the only release-version mechanism; do not add component-specific version constants or manually edit assembly versions for a release.

Requirements:

- Keep the release base version in the root `version.json` (currently `4.0.0`).
- Treat the Git commit height since that base-version update as the common build/revision number. Release builds must retain the Git history required for Nerdbank.GitVersioning to calculate it correctly; do not shallow-clone or override it with a manually supplied revision.
- Add the existing Nerdbank.GitVersioning package/configuration to every project that produces a shipped v4 binary: WinForms, Console, Shared, Contracts, Engine, ControlService, and UserAgent.
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
- Own audit records, diagnostic aggregation, recovery state, client sync, messages, and anonymous metrics.

### Service account

Start with the least privileged practical account, preferably `LocalService`. Do not default to `LocalSystem`. Any future privileged session-launch functionality must be isolated, justified, and use minimal permissions.

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

Reuse the existing `StartupManager` mechanism. It maintains the per-user `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` entry from the `StartOnBootUp` setting; do not create a scheduled task for the User Agent.

```text
StartOnBootUp enabled:
  - Existing DM startup registration starts DM at that user's Windows sign-in.
  - MinimiseOnStart preserves the current visible/minimised startup behaviour.
  - DM starts/connects the User Agent, which registers with Control Service.

StartOnBootUp disabled:
  - No DM startup registry value exists for that user.
  - Opening WinForms starts/connects that user's User Agent.
  - Closing WinForms stops the Agent only when no operation is active.
  - An active operation keeps the Agent alive until safe completion/recovery.
```

### Responsibilities

- Run in a logged-in interactive Windows session.
- Apply profile and audio changes using existing DM/native code.
- Run existing shortcut lifecycle, including pre/after/stop programs.
- Load game libraries and monitor normal game process trees.
- Handle Steam Big Picture mode detection.
- Capture/restore temporary state.
- Show user-session notifications.
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

WinForms remains responsible for validation presentation, modal dialogs, and marshaling updates to the UI thread. Engine/Agent code returns structured results and never shows `MessageBox` dialogs.

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

Current DM uses `AnonymousMetricsService`, `ClientSyncService`, and `MessageSyncService` from the WinForms `Program` process. v4 moves their machine-wide ownership to the Control Service.

```text
User Agent --> Control Service
  Local health heartbeat, active-runtime updates, operation progress.

Control Service --> DM metrics endpoint
  Existing opted-in anonymous metrics heartbeat.

Control Service --> DM sync endpoint
  Existing client sync, updates, messages, and media gathering.
```

The active User Agent/UI displays user-facing message/update notifications. The service gathers, validates, stores, schedules, and emits the machine-level events.

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
- Create ProgramData storage and ACLs.
- Integrate User Agent startup with the existing per-user `StartupManager` HKCU Run registration.
- Install WinForms and Console clients.
- Install the User Agent and Control Service using the build version derived from the root `version.json`.
- Preserve and migrate user data safely.
- Stop/start service safely during upgrades.
- Preserve user data by default on uninstall.
- Provide useful installer/migration failure reporting.

Future packaged WinUI 3 remains viable: a full-trust WinUI 3 desktop client can use named pipes and Win32 APIs. User-session monitoring remains in the User Agent regardless of UI technology.

## Implementation Phases

### Phase A — Structure and IPC

- [x] Create Contracts, Engine, ControlService, and UserAgent projects.
- [x] Add protocol versioning and common result/error models.
- [x] Implement authenticated named-pipe transport.
- [ ] Confirm the production pipe ACL permits authenticated local users while remote callers are rejected by mandatory Windows SID/session/process verification.
- [x] Implement Agent registration, heartbeat, and diagnostics status.
- [x] Implement active-console and machine-operation lease state.
- [ ] Apply the root `version.json`/Nerdbank.GitVersioning configuration to all new v4 shipped projects and remove hard-coded Agent/Service version strings.

**Exit criteria:** Service can show a verified Agent SID/session and deny a second conflicting display lease.

### Phase B — Storage and migration

- [x] Add ProgramData storage abstraction and per-SID paths.
- [x] Add safe write/backup helpers.
- [x] Add v4 migration runner and migration markers; integrate legacy `ConfigMigrationRunner` once repositories use the new storage paths.
- [x] Add opt-in repository storage-path configuration; do not activate it before the User Agent migration hand-off is implemented.
- [x] Add an identity-verified User Agent migration request; keep it explicit until repository ownership moves to the Agent.
- [x] Redirect current repository paths only when a completed, validated per-SID migration marker is present.
- [ ] Migrate display profiles first.
- [ ] Migrate audio profiles, shortcuts, user settings, and message state.
- [ ] Rename each successful legacy source file to `.old`.

**Exit criteria:** Existing DM user sees unchanged data after migration; legacy files remain as `.old`; repeated startup does not import duplicates.

### Phase C — First end-to-end profile operation

- [ ] Implement `ListProfiles` through service/repositories.
- [ ] Refactor WinForms profile list to use service requests.
- [ ] Implement `ApplyProfile` operation routing.
- [ ] Agent invokes existing display-application behaviour.
- [ ] Agent returns progress/final result; WinForms displays it.

**Exit criteria:** Active console user applies their own migrated profile; another session is denied.

### Phase D — Shortcut lifecycle and recovery

- [ ] Move shortcut execution into UserAgent.
- [ ] Preserve pre/after/stop programs, audio, and temporary restoration.
- [ ] Persist and test recovery records.
- [ ] Move normal Steam game monitoring.
- [ ] Add synthetic Steam Big Picture `SteamGame` behaviour.
- [ ] Implement cancellation and locked-session rules.

**Exit criteria:** Big Picture shortcut applies temporary state, monitors correctly, and restores it after exit.

### Phase E — Existing background functionality

- [ ] Move anonymous metrics ownership to service.
- [ ] Move client sync/message gathering/storage to service.
- [ ] Forward update/message events to Agent/UI.
- [ ] Add audit and diagnostics bundle support.

**Exit criteria:** One machine produces one metrics/sync schedule regardless of UI/Agent count.

### Phase F — Local REST foundation

- [ ] Add disabled-by-default loopback API host.
- [ ] Add identity/status endpoints.
- [ ] Add pairing/provider abstractions and scoped credentials.
- [ ] Add profile list/apply and operation-status endpoints.
- [ ] Add authorization, rate-limit, audit, and loopback-only tests.

**Exit criteria:** Paired local test client can list/apply permitted profiles; unpaired client cannot.

### Phase G — Installer, verification, release readiness

- [ ] Update installer and upgrade path.
- [ ] Verify the release build gives every shipped v4 component, service registration, and installer artifact the common `version.json` build version.
- [ ] Test fresh install, upgrade, repair, uninstall/reinstall.
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
- Steam installed/absent/already running.
- Big Picture and ordinary Steam game lifecycle.
- Standard and administrator accounts.
- Lock/unlock, fast user switching, and RDP.
- Agent automatic-start opt-out.

## v4.0.0 Acceptance Criteria

v4.0.0 is ready when:

- [ ] Control Service installs and starts reliably.
- [ ] User Agent startup follows existing `StartOnBootUp`/`MinimiseOnStart` settings and honors per-user opt-out.
- [ ] Existing user data migrates safely and source files remain with `.old` suffixes.
- [ ] Users see and manage only their own profiles/shortcuts.
- [ ] Only active console user may apply profiles/run game shortcuts.
- [ ] Only one physical-state operation runs at a time.
- [ ] Existing display/audio/game behaviour executes in User Agent.
- [ ] Steam Big Picture appears as a normal game and restores temporary state correctly.
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

