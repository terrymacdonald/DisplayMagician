# DisplayMagician v4 local IPC specification

**Status:** Implemented protocol documentation  
**Protocol version:** 1  
**Last verified:** 2026-10-07

## 1. Scope

This document specifies the supported local integration protocol between a same-device client and DisplayMagician.ControlService. It covers request/response commands and server-pushed client events.

The key words **MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY** are to be interpreted as described by RFC 2119 and RFC 8174.

Public clients MUST communicate with ControlService. They MUST NOT connect directly to UserAgent, SessionLauncher, Gateway control, or ControlService's Agent-registration pipe. Those endpoints are internal implementation details described in Appendix A.

## 2. Architecture and trust boundary

```text
same Windows user

local integration ----\
WinForms ---------------+--> ControlService client pipe --> UserAgent
Console ----------------/               |
                                         +--> machine coordination/state
                                         +--> SessionLauncher (process start only)
```

The public local API relies on Windows named-pipe identity. ControlService derives the caller SID and Windows session from the connected process; a client does not supply an authoritative SID or session.

A caller on another device or in another Windows user context MUST use the paired HTTPS Gateway API. ControlService MUST NOT be exposed directly to a network.

## 3. Public endpoints

| Purpose | Pipe name | Direction | Connection model |
|---|---|---|---|
| Commands | `DisplayMagician.ControlService.Client.v1` | Duplex | One request and one response per connection |
| Events | `DisplayMagician.ControlService.ClientEvents.v1` | Duplex | One subscription request, one acknowledgement, then server-pushed events |

Both pipes use byte transmission mode. The server currently permits at most 16 concurrently handled command connections and 16 event subscribers. Clients SHOULD use short-lived command connections and one reconnectable event subscription.

The pipe ACL permits authenticated Windows users and privileged DMv4 service identities. Successful connection does not override per-operation identity, session, or administrator checks.

## 4. Framing

Every frame is:

```text
+---------------------------+-------------------------------+
| payload length (4 bytes)  | UTF-8 JSON payload            |
| signed Int32, little-endian| exactly payload length bytes |
+---------------------------+-------------------------------+
```

- The JSON payload MUST contain one `ControlEnvelope`.
- The payload length MUST be greater than zero and no greater than 5,242,880 bytes (5 MiB).
- A sender MUST flush the frame after writing it.
- A receiver MUST read exactly the advertised number of bytes.
- An incomplete prefix or payload is a protocol error.
- The current implementation uses Windows little-endian `BitConverter` encoding. Cross-platform bridge implementations MUST explicitly emit little-endian Int32 values.

## 5. Serialization

Protocol version 1 uses the default `System.Text.Json` representation:

- Property names are PascalCase when sent over named pipes.
- Property-name matching is case-insensitive when reading an envelope.
- Enums are JSON numbers, not enum names.
- GUIDs use the standard JSON string representation.
- Date/time values use ISO 8601 strings.
- Null-valued properties are emitted by the default serializer.

`Payload` is itself a JSON **string**. A client MUST serialize the operation-specific object to JSON and place that JSON text in `Payload`. Consequently, quotes inside the payload are escaped in the outer envelope. Responses and events use the same nested JSON-string convention.

See [the examples](examples/) for complete frames after the four-byte length prefix.

## 6. Envelope

```json
{
  "ProtocolVersion": 1,
  "MessageType": 11,
  "RequestId": "5e81ee74-6ae3-4866-aac4-88b1ace2ef59",
  "Hello": {
    "MinimumProtocolVersion": 1,
    "MaximumProtocolVersion": 1,
    "ClientKind": 4,
    "ClientId": "example.integration",
    "DeviceId": "",
    "DisplayName": "Example integration",
    "RequiredCapabilities": ["protocol-negotiation", "profiles"],
    "OptionalCapabilities": ["client-events"]
  },
  "Payload": "{\"DetailedProfileId\":\"\",\"IncludeActiveProfileDetail\":false,\"IncludeCurrentLayoutDetail\":false}"
}
```

### 6.1 Envelope fields

| Field | Requirement |
|---|---|
| `ProtocolVersion` | MUST be `1` for the current pipe endpoints. |
| `MessageType` | MUST be a supported numeric `ControlMessageType`. |
| `RequestId` | MUST be a non-empty GUID. It correlates the response and provides mutation replay protection. |
| `Hello` | MUST contain valid compatibility metadata on every request. |
| `Payload` | MUST be an empty string when no payload is needed, or operation-specific JSON text. |

A command response MUST preserve the request's `ProtocolVersion`, `MessageType`, and `RequestId`. Clients MUST reject a response with a mismatched message type or request ID.

The outer response envelope's `Hello` property is not the negotiation result. A successful response carries `ProtocolWelcome` inside the serialized `ControlResponse` payload.

## 7. Compatibility negotiation

Every request MUST contain a `ProtocolHello`.

| `ProtocolHello` field | Rule |
|---|---|
| `MinimumProtocolVersion` | Positive and no greater than `MaximumProtocolVersion`. |
| `MaximumProtocolVersion` | Highest version the client can process. |
| `ClientKind` | MUST NOT be `Unknown` (`0`). Use `LocalIntegration` (`4`) for third-party local clients. |
| `ClientId` | Stable, non-blank identifier, maximum 128 characters. |
| `DeviceId` | Optional, maximum 128 characters. |
| `DisplayName` | Optional, maximum 128 characters. |
| `RequiredCapabilities` | At most 64 unique capability IDs. Every value must be supported. |
| `OptionalCapabilities` | At most 64 unique capability IDs. Unsupported values are omitted from the negotiated subset. |

Capability IDs contain only lowercase ASCII letters, digits, and hyphens and are at most 64 characters. A capability MUST NOT appear in both lists.

Current ControlService capabilities are:

```text
protocol-negotiation
operation-status
operation-decisions
profiles
audio-profiles
shortcuts
client-events
client-sync
diagnostics
```

On success, `ControlResponse.ProtocolWelcome` reports the selected version, endpoint kind, service-instance ID, supported capabilities, and negotiated optional capabilities. Clients MUST validate that the selected version is in their advertised range.

Negotiation is compatibility discovery. It is not authentication, pairing, licensing, or an authorisation grant.

## 8. Timeouts and connection behavior

| Setting | Value |
|---|---|
| Client connection timeout | 10 seconds |
| Request/response timeout | 30 seconds |
| Server wait for initial command or subscription | 10 seconds |
| Event keep-alive interval | 20 seconds |
| Recommended client event idle timeout | 45 seconds |

A client SHOULD reconnect after transient connection, timeout, or event-idle failures. It MUST NOT assume that a lost response means a mutating request failed.

## 9. Request, response, retry, and idempotency

The response payload is a serialized `ControlResponse`:

```json
{
  "IsSuccessful": true,
  "ErrorCode": 0,
  "Message": "Profiles returned.",
  "ProtocolWelcome": {
    "SelectedProtocolVersion": 1,
    "EndpointKind": "ControlService",
    "ServiceInstanceId": "7c342c5905304c7e94f97fe5f4d29624",
    "SupportedCapabilities": ["protocol-negotiation", "profiles"],
    "NegotiatedOptionalCapabilities": []
  },
  "ProfileList": {
    "SavedProfiles": [],
    "CurrentLayout": null
  }
}
```

- `IsSuccessful=false` means the operation was rejected or failed even if a transport response was received.
- Clients MUST inspect `ErrorCode` and MUST NOT infer success from a non-empty result property.
- A client MAY retry an identical operation with the same `RequestId`.
- A client MUST NOT reuse a `RequestId` for a different message type or payload.
- ControlService retains successful replay-protected mutation results for 24 hours, bounded to 1,000 records.
- A retry of a retained mutation returns the original response instead of performing the mutation again.

Replay protection currently covers profile/audio mutations, applying/running/cancelling, repository commits, message read state, decision resolution, metrics settings/initialization, forced release/recovery administration, UserAgent restart, and support-bundle creation. Device-pairing and Gateway-setting mutations are not currently included; see the gap analysis.

## 10. Public command catalogue

The numeric values are stable wire values. “Local” means callable through the public client pipe. Administrative and application-support commands are public-pipe operations but are not recommended as general third-party integration primitives.

### 10.1 Profiles and audio profiles

| Value | Message | Request payload | Primary response | Notes |
|---:|---|---|---|---|
| 11 | `ListProfiles` | `ProfileListRequest` or empty | `ProfileListResult` | Lists saved profiles and current layout. |
| 12 | `ApplyProfile` | `ApplyProfileRequest` | Operation status via response/events | Client supplies non-empty operation ID. |
| 15 | `CreateProfileFromCurrent` | `CreateProfileRequest` | General success | Local editing. |
| 16 | `RenameProfile` | `RenameProfileRequest` | General success | Local editing. |
| 17 | `DeleteProfile` | `DeleteProfileRequest` | General success | Local editing. |
| 18 | `UpdateProfileFromCurrent` | `DeleteProfileRequest` | General success | Payload property is `ProfileId`. |
| 19 | `ListAudioProfiles` | Empty | `AudioProfileListResult` | Lists saved and current audio layouts. |
| 20 | `ApplyAudioProfile` | `ApplyAudioProfileRequest` | Operation status via response/events | Default device wait is 20 seconds. |
| 21 | `CreateAudioProfileFromCurrent` | `CreateProfileRequest` | General success | Local editing. |
| 22 | `RenameAudioProfile` | `RenameProfileRequest` | General success | Local editing. |
| 23 | `DeleteAudioProfile` | `DeleteProfileRequest` | General success | Local editing. |
| 24 | `UpdateAudioProfileFromCurrent` | `DeleteProfileRequest` | General success | Payload property is `ProfileId`. |
| 36 | `UpdateDisplayProfileSettings` | `UpdateDisplayProfileSettingsRequest` | General success | Local editing. |

### 10.2 Shortcuts, games, applications, and repositories

| Value | Message | Request payload | Primary response | Notes |
|---:|---|---|---|---|
| 27 | `StartShortcut` | `StartShortcutRequest` | Operation status via response/events | Client supplies non-empty operation ID. |
| 30 | `ListGames` | Empty | `GameListResult` | UserAgent-owned discovery. |
| 34 | `ListApps` | Empty | `AppListResult` | UserAgent-owned discovery. |
| 35 | `ListShortcuts` | Empty | `ShortcutListResult` | Client-safe shortcut views. |
| 25 | `GetRepositorySnapshot` | `RepositorySnapshotRequest` | `RepositorySnapshot` | Advanced local editing primitive. |
| 26 | `CommitRepositorySnapshot` | `RepositoryCommitRequest` | `RepositoryCommitResult` | Optimistic concurrency by revision. |

### 10.3 Operations and decisions

| Value | Message | Request payload | Primary response |
|---:|---|---|---|
| 28 | `GetOperationStatus` | `OperationStatusRequest` | `OperationStatus` |
| 29 | `ListOperationStatuses` | Empty | `OperationStatus[]` |
| 37 | `CancelOperation` | `CancelOperationRequest` | General success |
| 51 | `ListOperationDecisions` | Empty | `OperationDecision[]` |
| 49 | `ResolveOperationDecision` | `ResolveOperationDecisionRequest` | `OperationDecision` |

Operation phases are numeric: `Unknown=0`, `Requested=1`, `Validating=2`, `ApplyingDisplayProfile=3`, `ApplyingAudioProfile=4`, `StartingPrograms=5`, `StartingGame=6`, `WaitingForGameToStart=7`, `WaitingForGameToClose=8`, `RunningAfterPrograms=9`, `RestoringDisplayProfile=10`, `RestoringAudioProfile=11`, `Completed=12`, `Cancelled=13`, `Failed=14`, and `AwaitingUserDecision=15`.

Decision choices are `Unknown=0`, `Continue=1`, and `StopAndRestore=2`. Expired recovery decisions default to `Continue` unless product policy changes.

### 10.4 Status, events, messages, and application support

| Value | Message | Request payload | Primary response |
|---:|---|---|---|
| 9 | `GetServiceStatus` | Empty | `ControlServiceStatus` |
| 38 | `SyncClient` | `ClientSyncRequest` | `ClientSyncResult` |
| 31 | `ListMessages` | Empty | `MessageListResult` |
| 32 | `SetMessageReadState` | `SetMessageReadStateRequest` | `MessageListResult` |
| 33 | `SyncMessages` | Empty | `MessageSyncResult` |
| 40 | `GetAnonymousMetricsSettings` | Empty | `AnonymousMetricsSettings` |
| 41 | `UpdateAnonymousMetricsSettings` | `AnonymousMetricsSettings` | `AnonymousMetricsSettings` |
| 42 | `InitializeAnonymousMetrics` | `InitializeAnonymousMetricsRequest` | General success |
| 43 | `ReportAnonymousMetricsUsage` | `AnonymousMetricsUsageReport` | General success |

### 10.5 Pairing, settings, diagnostics, and administration

| Value | Message | Access | Notes |
|---:|---|---|---|
| 59 | `GetGatewaySettings` | Local | Machine-owned settings view. |
| 60 | `UpdateGatewaySettings` | Administrator | Updates listener/advertisement settings. |
| 61 | `CreateDevicePairingQr` | Local | Creates a one-hour, one-time-secret QR session. |
| 62 | `ListDevicePairingRequests` | Local user | Pending requests for the caller. |
| 63 | `ApproveDevicePairing` | Local user | Granted capabilities must match the request. |
| 64 | `RejectDevicePairing` | Local user | Rejects a pending request. |
| 65 | `GetGatewayIdentity` | Local | Returns the registered public Gateway identity. |
| 66 | `ListPairedClients` | Local user | Excludes pairing secrets and private keys. |
| 67 | `RevokePairedClient` | Local user | Revokes one paired device. |
| 48 | `CreateUserSupportBundle` | Local | Creates the approved support ZIP. |
| 52 | `RestartUserAgent` | Local | Application/service management. |
| 47 | `ForceReleaseDisplayControl` | Administrator | Recovery administration. |
| 54 | `RecordRecoveryAdministration` | Administrator | Records recovery action/outcome. |
| 78 | `SetTemporaryDiagnosticLogLevel` | Local | Temporary diagnostics. |
| 79 | `ReleaseTemporaryDiagnosticLogLevel` | Local | Releases a temporary override. |

Other `ControlMessageType` values are internal and MUST NOT be sent to the public client pipe.

## 11. Operation model

Applying a profile/audio profile or starting a shortcut is asynchronous:

1. Generate an operation ID.
2. Generate a request ID.
3. Submit the command.
4. Treat successful submission as acceptance, not completion.
5. Read the returned/current operation status.
6. Subscribe to events or poll `GetOperationStatus`.
7. Continue until a terminal phase is observed.

`OperationStatus.Sequence` increases per operation. `LastUpdateId` deduplicates retried Agent updates. A status can be authoritative, stale, or terminal. Clients SHOULD display the server-provided phase and message and MUST use `IsTerminal` and `IsSuccessful` for completion.

## 12. Event subscription

Connect to `DisplayMagician.ControlService.ClientEvents.v1` and send one `SubscribeClientEvents` (`44`) envelope with a valid `Hello`.

The acknowledgement:

- Preserves message type and request ID.
- Contains a successful `ControlResponse`.
- Includes the negotiated `ProtocolWelcome`.
- Includes authoritative active `OperationStatuses`.
- Includes pending `OperationDecisions`.

After the acknowledgement the server sends `ClientEvent` (`45`) envelopes. Event types are:

| Value | Event | Payload property |
|---:|---|---|
| 1 | `ClientSyncCompleted` | `ClientSync` |
| 2 | `OperationStatusUpdated` | `OperationStatus` |
| 3 | `OperationDecisionUpdated` | `OperationDecision` |
| 4 | `SubscriptionHeartbeat` | No domain payload |

Event scope is `Session=0` or `User=1`. A client MUST ignore nullable payload properties unrelated to the current event type.

The server sends a heartbeat after 20 seconds without a domain event. A client SHOULD treat 45 seconds without any envelope as an idle failure, reconnect, and use the new acknowledgement snapshot before processing later events. A slow subscriber can be disconnected and MUST recover from the snapshot.

## 13. Error model

| Value | Error | Typical meaning |
|---:|---|---|
| 0 | `None` | No error. |
| 1 | `UnsupportedProtocolVersion` | Outer envelope version is not the current pipe version. |
| 2 | `InvalidRequest` | Unsupported operation, malformed payload, or invalid request-ID reuse. |
| 3 | `CallerIdentityMismatch` | Claimed and OS-derived identities differ. |
| 4 | `AgentNotConnected` | No registered UserAgent. |
| 5 | `AgentNotHealthy` | Agent heartbeat/readiness failure. |
| 6 | `NotActiveConsoleUser` | Session is not eligible for display control. |
| 7 | `DisplayControlBusy` | Another owner holds display control. |
| 8 | `RecoveryRequired` | Recovery must complete before new work. |
| 9 | `SessionLocked` | Target session is locked. |
| 10 | `Unauthorized` | Caller lacks authority. |
| 11 | `AgentUnavailable` | Agent/Gateway/service dependency is unavailable. |
| 12 | `AdministratorRequired` | Operation requires elevation/administrator identity. |
| 13 | `ProfileNotFound` | Display profile ID is unknown. |
| 14 | `AudioProfileNotFound` | Audio profile ID is unknown. |
| 15 | `ShortcutNotFound` | Shortcut ID is unknown. |
| 16 | `ValidationFailed` | Domain validation failed. |
| 17 | `ExecutionFailed` | Work started but failed. |
| 18 | `OperationNotFound` | Operation ID is unknown or no longer retained. |
| 19 | `DecisionUnavailable` | Prompt is missing, expired, or already resolved. |
| 20 | `IncompatibleProtocolVersion` | Hello version ranges do not overlap. |
| 21 | `RequiredCapabilityUnavailable` | A required capability is unsupported. |
| 22 | `AuthenticationRequired` | Remote authentication is missing. |
| 23 | `PairingRequired` | Remote device must pair. |

Clients MAY retry transport failures and transient Agent availability errors with backoff. Validation, authorisation, not-found, and compatibility failures SHOULD NOT be retried without changing the relevant input or environment.

## 14. Security and privacy requirements

- Clients MUST use `.` as the local named-pipe server and MUST NOT bridge the public pipe directly to a network.
- Clients MUST NOT claim or trust a SID/session from payload data.
- Clients MUST NOT log pairing secrets, private keys, tokens, passwords, full sensitive arguments, or unredacted support data.
- A local bridge for runtimes without named-pipe support SHOULD expose only the narrow operations required by that integration.
- Remote clients MUST use the paired Gateway and its signed-request protocol.
- `ProtocolHello` and `ProtocolWelcome` MUST NOT be treated as authentication.

## 15. Versioning rules

- Preserve public serialized property names and enum numeric values.
- Add optional fields with safe defaults where possible.
- Do not reuse removed enum values.
- Additive message types require a new capability when clients need feature discovery.
- Breaking framing, envelope, or required-field changes require a new protocol version.
- A new incompatible endpoint SHOULD use a new versioned pipe name.

## Appendix A: Internal IPC architecture

The following endpoints are DMv4 implementation details and are unsupported for third-party clients:

| Pipe | Participants | Purpose |
|---|---|---|
| `DisplayMagician.ControlService.v1` | UserAgent → ControlService | Agent registration, heartbeat, progress, decisions, and service coordination |
| `DisplayMagician.UserAgent.Command.v1.{session}` | ControlService → UserAgent | Interactive display/audio/profile/shortcut commands |
| `DisplayMagician.SessionLauncher.v1` | ControlService → SessionLauncher | Starts the fixed signed UserAgent payload in an authorised interactive session |
| `DisplayMagician.Gateway.Control.v1` | Gateway ↔ ControlService | Gateway identity registration, remote authentication, pairing, and routed remote commands |

Internal endpoints use the same framing and shared contracts where applicable, but have narrower ACLs, caller-kind checks, and identity verification. Their presence does not make their message types public.

## Appendix B: Related machine-readable documents

- [JSON Schema](schemas/displaymagician-ipc.schema.json)
- [AsyncAPI catalogue](asyncapi.yaml)
- [Capability matrix](API-Capability-Matrix.md)
- [Gap analysis](API-Gap-Analysis.md)
- [REST OpenAPI description](../REST/openapi.yaml)

