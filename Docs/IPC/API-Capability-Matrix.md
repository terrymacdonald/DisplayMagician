# DMv4 API capability matrix

**Verified against source:** 2026-10-07

Legend:

- **Yes**: implemented on that surface.
- **Partial**: some workflow elements exist, but the capability is incomplete.
- **No (scope)**: intentionally outside the selected REST remote-controller scope.
- **Internal**: available only to DMv4 components.
- **Gap**: required by the selected scope but missing or incomplete.

## Developer-facing capabilities

| Capability | Shared contract | Local IPC | Local event | REST/SSE | Tests/evidence | Assessment |
|---|---|---|---|---|---|---|
| Protocol negotiation | `ProtocolHello`, `ProtocolWelcome` | Yes, every request | Yes, subscription acknowledgement | No | Negotiation tests | REST gap; local complete |
| Service/user status | `ControlServiceStatus`, `RemoteUserStatus` | Yes | Operation updates only | `GET /v1/status` | Status stores/tests | REST intentionally returns user-scoped status |
| Continuous status | `ControlClientEvent`, `RemoteUserStatus` | Snapshot + event pipe | Yes | `GET /v1/status/stream` | Event hub tests | Implemented |
| List display profiles | `ProfileListResult` | Yes | No | `GET /v1/display-profiles` | Router tests | Implemented |
| Detailed display profile | `ProfileListRequest` | Yes | No | No | Contract/client | REST gap if remote preview is required |
| Apply display profile | `ApplyProfileRequest` | Yes | Progress/completion | `POST /v1/display-profiles/apply` | Router tests | Implemented |
| Create/rename/update/delete display profiles | Shared edit requests | Yes | No | No (scope) | Router/client | IPC-only by design |
| List audio profiles | `AudioProfileListResult` | Yes | No | `GET /v1/audio-profiles` | Router/client | Implemented |
| Apply audio profile | `ApplyAudioProfileRequest` | Yes | Progress/completion | `POST /v1/audio-profiles/apply` | Router/client | Implemented |
| Create/rename/update/delete audio profiles | Shared edit requests | Yes | No | No (scope) | Router/client | IPC-only by design |
| List shortcuts | `ShortcutListResult` | Yes | No | `GET /v1/shortcuts` | Router/client | Implemented |
| Run shortcut | `StartShortcutRequest` | Yes | Progress/completion | `POST /v1/shortcuts/run` | Router/client | Implemented |
| Edit shortcuts | Repository snapshot/commit | Partial | No | No (scope) | Repository connection | Local bulk-edit primitive; dedicated API gap |
| List games/apps | `GameListResult`, `AppListResult` | Yes | No | No (scope) | Client/router | IPC-only by design |
| Get one operation | `OperationStatusRequest` | Yes | Updates | Via aggregate status | Store tests | REST has no dedicated route |
| List operations | `OperationStatus[]` | Yes | Updates | Via status/SSE | Store tests | Implemented for remote controller |
| Cancel operation | `CancelOperationRequest` | Yes | Updates | No | Client/router | REST gap for remote-started work |
| List decisions | `OperationDecision[]` | Yes | Updates | Included in status | Decision-store tests | Capability-scope issue noted below |
| Answer decision | `ResolveOperationDecisionRequest` | Yes | Updates | `POST /v1/decisions/answer` | Decision-store tests | Implemented |
| Request pairing | Pairing contracts | Local creates QR | No | `POST /v1/pairing/request` | Pairing tests | Implemented |
| Poll pairing status | `DevicePairingStatusRequest` | Internal Gateway pipe | No | `POST /v1/pairing/status` | Pairing tests | Implemented |
| Approve/reject pairing from authorised paired device | Approval/rejection contracts | Yes | No | No | Coordinator supports paired approval | **Gap** |
| Revoke/list paired devices remotely | Pairing views | Yes | No | No | Local coordinator tests | **Gap** for multi-phone device management |
| Remote authentication/replay protection | Signed-request contracts | N/A | N/A | Signed P-256 headers | Authenticator tests | Implemented |
| Messages/client sync | Shared contracts | Yes | Sync-completed event | No (scope) | Sync tests | IPC-only by design |
| Diagnostics/support bundle | Shared contracts | Yes | No | No (scope) | Diagnostics tests | IPC-only by design |
| Gateway/machine settings | Shared contracts | Yes, admin checks | No | No (scope) | Settings code | IPC-only by design |

## Local client-pipe dispatch coverage

These message types are implemented by `ControlClientPipeServer`.

| Area | Implemented messages |
|---|---|
| Profiles | `ListProfiles`, `ApplyProfile`, `CreateProfileFromCurrent`, `RenameProfile`, `DeleteProfile`, `UpdateProfileFromCurrent`, `UpdateDisplayProfileSettings` |
| Audio | `ListAudioProfiles`, `ApplyAudioProfile`, `CreateAudioProfileFromCurrent`, `RenameAudioProfile`, `DeleteAudioProfile`, `UpdateAudioProfileFromCurrent` |
| Discovery/shortcuts | `ListGames`, `ListApps`, `ListShortcuts`, `StartShortcut` |
| Repository editing | `GetRepositorySnapshot`, `CommitRepositorySnapshot` |
| Operations | `GetOperationStatus`, `ListOperationStatuses`, `CancelOperation` |
| Decisions | `ListOperationDecisions`, `ResolveOperationDecision` |
| Status/Agent | `GetServiceStatus`, `StopAgentIfIdle`, `RestartUserAgent` |
| Messages/sync | `ListMessages`, `SetMessageReadState`, `SyncMessages`, `SyncClient` |
| Metrics | `GetAnonymousMetricsSettings`, `UpdateAnonymousMetricsSettings`, `InitializeAnonymousMetrics`, `ReportAnonymousMetricsUsage` |
| Pairing/Gateway | `GetGatewaySettings`, `UpdateGatewaySettings`, `GetGatewayIdentity`, `CreateDevicePairingQr`, `ListDevicePairingRequests`, `ApproveDevicePairing`, `RejectDevicePairing`, `ListPairedClients`, `RevokePairedClient` |
| Diagnostics/admin | `CreateUserSupportBundle`, `ForceReleaseDisplayControl`, `RecordRecoveryAdministration`, `SetTemporaryDiagnosticLogLevel`, `ReleaseTemporaryDiagnosticLogLevel` |

## Internal-only message groups

The following are not public client-pipe API even though their contracts share `ControlMessageType`:

- Agent registration, heartbeat, progress, completion, reconciliation, and decision requests
- Agent launch/stop commands
- Display-control lease acquisition
- Gateway registration, authentication, and routed remote commands
- Pairing submission/status calls made by Gateway

## REST routes

| Method | Path | Authentication | Required scope |
|---|---|---|---|
| GET | `/v1/identity` | Public | None |
| POST | `/v1/pairing/request` | One-time pairing secret in body | Requested capabilities validated |
| POST | `/v1/pairing/status` | One-time pairing secret in body | None |
| GET | `/v1/status` | Signed request | `status-read` |
| GET | `/v1/status/stream` | Signed request | `status-read` |
| GET | `/v1/display-profiles` | Signed request | `profiles-read` |
| POST | `/v1/display-profiles/apply` | Signed request | `profiles-apply` |
| GET | `/v1/audio-profiles` | Signed request | `audio-profiles-read` |
| POST | `/v1/audio-profiles/apply` | Signed request | `audio-profiles-apply` |
| GET | `/v1/shortcuts` | Signed request | `shortcuts-read` |
| POST | `/v1/shortcuts/run` | Signed request | `shortcuts-run` |
| POST | `/v1/decisions/answer` | Signed request | `decisions-answer` |

`decisions-read` and `pairing-approve` are defined remote scopes but do not currently have independently enforceable REST workflows.
