# DisplayMagician v4 internal component communication

**Status:** Implemented architecture description  
**Protocol version:** 1  
**Last verified:** 2026-10-08

## 1. Purpose

This document explains how DisplayMagician v4 (DMv4) components exchange commands, state, progress, decisions, and lifecycle information.

It is an architecture guide, not a public integration contract. Internal pipe names and internal message types are implementation details. Local integrations must use the public ControlService client and event pipes described in the [IPC specification](IPC-Specification.md) and [local IPC client guide](Local-IPC-Client-Guide.md).

## 2. Component responsibilities

| Component | Owns | Does not own |
|---|---|---|
| WinForms | User interaction, input validation, and presentation | Display/audio execution, authoritative runtime state, or direct per-user runtime persistence |
| DisplayMagicianConsole | Command-line interaction and presentation | Direct UserAgent control or authoritative runtime state |
| ControlService | Caller/session routing, machine-wide coordination, display-operation serialization, operation/decision state, machine settings, audit, and event distribution | Interactive desktop work, game/process monitoring, or user dialogs |
| UserAgent | Per-user definitions, display/audio execution, shortcuts, game/process monitoring, recovery execution, messages, and interactive-session behavior | Machine-wide authorization, remote-device trust, or general client routing |
| SessionLauncher | Starting or stopping the fixed installed UserAgent payload in an authorized interactive session | General process launching, command routing, display/audio work, or UI |
| Gateway | HTTPS, remote-client authentication, REST translation, and SSE presentation | Direct UserAgent access or independent authorization decisions |
| Local integration | A supported subset of same-user controller workflows through public IPC | Internal component pipes or authoritative DMv4 state |

All runtime clients use contracts from `DisplayMagician.Contracts`. Portable persisted definitions are owned by `DisplayMagician.ConfigurationDefinitions` and are loaded or changed by the UserAgent.

## 3. Communication topology

```text
WinForms --------\
Console ----------+--> public client pipe ----\
Local integration-/                            |
                                               v
                                      +----------------+
                                      | ControlService |
                                      +----------------+
                                       ^   |    |    ^
                registration/status ---+   |    |    +--- Gateway control pipe --- Gateway --- HTTPS clients
                                           |    |
                            Agent command --+    +--- SessionLauncher pipe --- SessionLauncher
                                           |
                                           v
                                       UserAgent

ControlService --> public event pipe --> WinForms / Console / local integrations
```

Public clients never select a UserAgent command pipe or supply an authoritative Windows SID/session. ControlService derives the caller identity from the named-pipe connection and routes the request to the registered UserAgent for that user and session.

## 4. Endpoint inventory

| Pipe | Initiator | Server | Connection model | Information exchanged |
|---|---|---|---|---|
| `DisplayMagician.ControlService.Client.v1` | WinForms, Console, or local integration | ControlService | One request and response per connection | Public same-user commands and typed results |
| `DisplayMagician.ControlService.ClientEvents.v1` | WinForms, Console, or local integration | ControlService | Long-lived subscription | Operation updates, decisions, sync completion, and heartbeats |
| `DisplayMagician.ControlService.v1` | UserAgent | ControlService | Persistent authenticated connection | Registration, health, lease requests, progress, completion, reconciliation, and decision requests |
| `DisplayMagician.UserAgent.Command.v1.{processId}` | ControlService | UserAgent | One routed command and response | Interactive-session commands and per-user data results |
| `DisplayMagician.SessionLauncher.v1` | ControlService | SessionLauncher | One request and response per connection | Authorized fixed-payload UserAgent launch or stop |
| `DisplayMagician.Gateway.Control.v1` | Gateway | ControlService | One request and response per connection | Gateway registration, authentication, pairing, status, and scoped remote commands |

The UserAgent command-pipe suffix is the registered UserAgent process ID. ControlService validates that the registration, caller process, installed executable, user, session, and pipe name agree before trusting it.

## 5. Ownership and authoritative state

| Information | Authority | Consumers | Transfer path |
|---|---|---|---|
| Caller user and session | Windows named-pipe/process identity | ControlService routers | Derived at connection time; never trusted from a public request |
| Agent registration and health | ControlService coordinator, reported by UserAgent | ControlService clients and routers | UserAgent service pipe registration and heartbeats |
| Display/audio profiles and shortcuts | UserAgent per-user storage | WinForms, Console, integrations, Gateway | Client/Gateway request → ControlService → UserAgent command pipe → response |
| Current hardware/layout state | UserAgent in the interactive session | Clients requesting current state | Routed UserAgent command response |
| Display-control ownership | ControlService | UserAgent and operation routers | Lease acquisition and service-side coordination |
| Operation status/history | ControlService operation store, updated by UserAgent | Local clients, Gateway, event subscribers | Agent progress/completion → store → response/events |
| Pending decisions | ControlService decision store, requested by UserAgent | Authorized same-user clients and paired clients with scope | Agent request → store/events → client response → Agent polling |
| Machine settings and paired clients | ControlService machine-owned stores | WinForms/admin clients and Gateway | Public administrative IPC or Gateway control pipe |
| User messages and definition changes | UserAgent | Desktop clients | Routed command responses and sync-completed events |
| Anonymous metrics configuration/schedule | ControlService | Desktop clients and sync coordinator | Public client IPC and service-owned scheduling |
| Remote credentials and authorization | ControlService | Gateway | Gateway control pipe |
| HTTPS/TLS host identity | Gateway | Remote clients and ControlService registration | Gateway registration and HTTPS identity response |

## 6. Component startup and Agent health

1. The UserAgent opens its process-specific command pipe.
2. It connects to the ControlService service pipe.
3. It sends `AgentRegistration` containing its user, session, process, version, startup mode, operation state, recovery state, and command-pipe name.
4. ControlService verifies the operating-system identity and installed UserAgent executable before recording the registration.
5. The UserAgent sends periodic `AgentHeartbeat` messages on the persistent connection.
6. Updated registration or heartbeat information changes the service's view of Agent health and recovery state.
7. If the connection closes, ControlService unregisters that Agent and no longer routes work to it.

When an authorized request needs an absent UserAgent:

1. ControlService verifies the target user/session and request authority.
2. ControlService asks SessionLauncher to launch the fixed installed UserAgent.
3. SessionLauncher validates that the caller is ControlService and starts only the expected signed payload in the requested interactive session.
4. The new UserAgent registers with ControlService.
5. ControlService routes the original work only after a valid registration is available.

SessionLauncher does not receive or route the original display, audio, profile, or shortcut command.

## 7. Reading user-scoped information

Listing profiles, audio profiles, shortcuts, games, applications, messages, or repository data follows this pattern:

```text
client
  -> public client request
ControlService
  -> derive caller user/session
  -> locate or start the matching UserAgent when the operation permits it
  -> forward the typed request to the registered Agent command pipe
UserAgent
  -> read per-user definitions or interactive-session state
  -> return a typed ControlResponse
ControlService
  -> return the correlated response to the client
```

ControlService does not copy UserAgent-owned definitions into a competing authoritative store. It routes and coordinates access.

## 8. Applying profiles and running shortcuts

Display-profile, audio-profile, and shortcut execution is asynchronous:

1. The client submits an apply/run command with separate request and operation IDs.
2. ControlService derives the caller identity, validates the request, and applies mutation replay protection where supported.
3. ControlService locates or starts the caller's UserAgent.
4. For display-changing work, ControlService verifies session state, recovery state, and display-control availability.
5. ControlService creates or updates the operation record and routes the command to the UserAgent.
6. The UserAgent validates the referenced definition and performs the interactive display, audio, program, game, or recovery work.
7. The UserAgent sends `OperationProgress` updates and one terminal `OperationCompleted` update over its persistent service connection.
8. ControlService validates sequence/update identity, stores the authoritative status, and publishes a client event.
9. Clients observe the operation through the initial response, `GetOperationStatus`, `ListOperationStatuses`, or the event subscription.

A successful command response means the work was accepted. Completion is determined from terminal operation state, not from transport success.

## 9. Cancellation

1. A client sends `CancelOperation` through the public client pipe, or Gateway sends an authorized remote cancellation when that REST capability exists.
2. ControlService verifies user/session ownership and operation state.
3. ControlService forwards cancellation to the UserAgent that owns the operation.
4. The UserAgent stops at an operation-safe boundary and performs any required restoration.
5. Progress and the terminal cancellation result return through the normal operation-status flow.

## 10. Decisions

When UserAgent execution needs a `Continue` or `StopAndRestore` answer:

1. UserAgent sends `RequestOperationDecision` to ControlService.
2. ControlService stores the decision, moves the operation to `AwaitingUserDecision`, and publishes operation and decision events.
3. A same-user desktop/local client lists or receives the pending decision. A paired remote client can do so only through an authorized Gateway capability.
4. One authorized client resolves the decision.
5. ControlService records the first valid answer and publishes the resolved decision.
6. UserAgent polls `GetOperationDecision` on its persistent connection until the decision is resolved or expires.
7. An expired recovery decision uses the documented default choice of `Continue`.
8. UserAgent resumes or restores and continues publishing operation status.

The decision is service-owned so any authorized client for that user can answer it; it is not tied to the client that started the operation.

## 11. Event distribution and recovery

ControlService is the event fan-out point:

1. A public client subscribes on the event pipe.
2. ControlService acknowledges with active operation statuses and pending decisions.
3. Service-owned stores publish operation, decision, and client-sync changes to the event hub.
4. ControlService sends matching `ClientEvent` envelopes to same-user/session subscribers.
5. Heartbeats keep an otherwise idle subscription observable.
6. After disconnect, the client reconnects and treats the new acknowledgement snapshot as authoritative before processing later events.

Events are notifications, not a separate source of truth. Clients recover with the acknowledgement snapshot and command queries.

## 12. Definition and message synchronization

UserAgent owns user-scoped definitions and messages. ControlService owns the machine-level schedule and manifest coordination:

1. ControlService determines that client synchronization is due or receives `SyncClient`.
2. It routes applicable synchronization work to the UserAgent.
3. UserAgent applies client-safe definition/message updates in per-user storage.
4. UserAgent returns the synchronization result.
5. ControlService publishes `ClientSyncCompleted` so desktop clients can refresh affected views.

Clients should re-query the relevant list after a sync-completed event rather than treating the event as a full repository replacement.

## 13. Gateway and remote clients

Gateway never connects to UserAgent directly:

1. Gateway terminates HTTPS and translates a REST request into a narrow Gateway control message.
2. For protected requests, Gateway asks ControlService to authenticate the device/request and determine its user and granted capabilities.
3. ControlService validates pairing, capability, expiry/replay requirements, and target-session rules.
4. ControlService maps an allowed remote operation to the corresponding internal command and routes it to the user's UserAgent.
5. The typed result returns through ControlService to Gateway and is translated into the REST response.
6. Gateway status/SSE reads service-owned operation and decision state through the same narrow internal pipe.

Remote inputs are never converted into an unrestricted public local-client request.

## 14. Machine settings, diagnostics, and support data

- ControlService directly handles machine-owned Gateway settings, pairing state, metrics settings, recovery administration, and diagnostic-level coordination.
- Administrator-required actions are checked against the operating-system identity of the public pipe caller.
- UserAgent supplies approved per-user logs/configuration for support collection.
- ControlService stages machine-owned support sources that desktop clients cannot read directly.
- Results and warnings return through typed public contracts; protected machine paths are not exposed as a shortcut around the service.

## 15. Failure and restart behavior

- A missing or unhealthy UserAgent causes an explicit unavailable/health error unless authorized demand-start succeeds.
- Agent reconnection includes `ReconcileOperationStatuses` so ControlService can recover active operation state.
- Lost client responses do not prove a mutation failed; replay-protected requests must be retried with the same request ID and payload.
- Lost event connections recover from a new subscription acknowledgement and status queries.
- Recovery-required state blocks unsafe display-changing work until the UserAgent/service recovery workflow clears it.
- Component logs preserve the same request and operation correlation identifiers across routing boundaries.

## 16. Public versus internal contracts

The shared `ControlMessageType` enum contains both public and internal values. Sharing an enum does not make every value valid on every pipe.

- Public clients use only the messages catalogued in the [IPC specification](IPC-Specification.md).
- Local controller integrations use the narrower profile in the [local IPC client guide](Local-IPC-Client-Guide.md).
- UserAgent, SessionLauncher, and Gateway use only their dedicated internal endpoints.
- Pipe servers reject unsupported messages even when the numeric value exists in the shared contract assembly.

