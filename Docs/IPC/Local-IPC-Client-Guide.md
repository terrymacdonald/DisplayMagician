# DisplayMagician v4 local IPC client guide

**Audience:** Same-PC integrations such as the DisplayMagician Stream Deck client  
**Transport:** Windows named pipes  
**Trust model:** Same signed-in Windows user  
**Protocol version:** 1

## 1. Purpose

This guide explains how a local controller integration connects to DMv4, discovers saved actions, starts work, follows progress, and responds to decisions.

The [IPC specification](IPC-Specification.md) remains normative for framing, serialization, message values, payload contracts, errors, timeouts, and compatibility. This guide describes the recommended integration workflow and supported reduced control profile.

## 2. Supported local-controller profile

A Stream Deck-style integration normally needs only these operations:

| Workflow | Message | Required compatibility capability |
|---|---|---|
| Check service/Agent state | `GetServiceStatus` | `protocol-negotiation` |
| List display profiles | `ListProfiles` | `profiles` |
| Apply a display profile | `ApplyProfile` | `profiles`, `operation-status` |
| List audio profiles | `ListAudioProfiles` | `audio-profiles` |
| Apply an audio profile | `ApplyAudioProfile` | `audio-profiles`, `operation-status` |
| List shortcuts | `ListShortcuts` | `shortcuts` |
| Run a shortcut | `StartShortcut` | `shortcuts`, `operation-status` |
| Read operation state | `GetOperationStatus`, `ListOperationStatuses` | `operation-status` |
| Cancel integration-started work | `CancelOperation` | `operation-status` |
| Receive progress and completion | `SubscribeClientEvents` | `client-events` |
| Show pending recovery decisions | `ListOperationDecisions` | `operation-decisions` |
| Answer a displayed decision | `ResolveOperationDecision` | `operation-decisions` |

Decision support is optional for a simple button-only integration, but an integration that displays operation status should either support decisions or clearly direct the user to the main DisplayMagician application when an operation is waiting for one.

The following public-pipe areas are outside the local-controller profile:

- profile/audio-profile creation, editing, rename, and deletion;
- repository snapshot/commit;
- game and application discovery for editors;
- message and client synchronization;
- anonymous metrics configuration/reporting;
- Gateway settings, pairing approval, and paired-device administration;
- support-bundle and diagnostic-level controls;
- UserAgent lifecycle and recovery-administration commands;
- forced display-control release.

These exclusions define the supported integration surface, not a Windows security boundary. Under the current same-user trust model, ControlService does not authorize commands from `ProtocolHello.ClientKind` or its capability lists. Required/optional capabilities negotiate compatibility; they do not grant permission.

## 3. Required architecture

```text
Stream Deck host
      |
      | plugin/vendor IPC if required by that runtime
      v
local integration process
      |
      | Windows named pipes
      v
DisplayMagician.ControlService
      |
      | internal routing
      v
DisplayMagician.UserAgent
```

The local integration process:

- connects only to `.` (the local computer);
- uses the public command and event pipes;
- identifies itself as `LocalIntegration`;
- does not connect to UserAgent, SessionLauncher, or Gateway internal pipes;
- does not read or write DMv4 persistence directly;
- does not expose the named-pipe API to the network;
- may provide a narrow private adapter to its own plugin runtime when that runtime cannot use Windows named pipes.

The DisplayMagician Stream Deck executable can itself be that adapter. A separate generic bridge is not required.

## 4. Pipes and connection lifetime

| Purpose | Pipe | Client behavior |
|---|---|---|
| Commands | `DisplayMagician.ControlService.Client.v1` | Open, send one request, receive one response, close |
| Events | `DisplayMagician.ControlService.ClientEvents.v1` | Open, subscribe once, receive acknowledgement, keep connected |

Use `NamedPipeClientStream` with server name `"."`, `PipeDirection.InOut`, and asynchronous options.

Recommended limits:

- allow up to 10 seconds to connect;
- allow up to 30 seconds for a command response;
- expect an event or heartbeat within 45 seconds;
- keep only one event subscription per integration process;
- reconnect with bounded backoff after transient failures.

## 5. Client identity and compatibility

Every request carries a `ProtocolHello`. A local integration should use:

```json
{
  "MinimumProtocolVersion": 1,
  "MaximumProtocolVersion": 1,
  "ClientKind": 4,
  "ClientId": "vendor.product.integration",
  "DeviceId": "",
  "DisplayName": "Product integration",
  "RequiredCapabilities": [
    "protocol-negotiation"
  ],
  "OptionalCapabilities": [
    "profiles",
    "audio-profiles",
    "shortcuts",
    "operation-status",
    "operation-decisions",
    "client-events"
  ]
}
```

Guidance:

- Keep `ClientId` stable across versions.
- Use a human-readable `DisplayName`.
- Require only capabilities without which the integration cannot function.
- Put independently optional features in `OptionalCapabilities`.
- Validate `ProtocolWelcome.SelectedProtocolVersion`.
- Disable or hide an optional feature when its capability was not negotiated.
- Do not treat negotiation as authentication or licensing.

## 6. Sending a command

For each command:

1. Create a new `NamedPipeClientStream` for the command pipe.
2. Connect with the connection timeout.
3. Serialize the operation-specific request object to JSON.
4. Create a `ControlEnvelope` with protocol version `1`, numeric message type, a non-empty request GUID, `ProtocolHello`, and the serialized JSON text in `Payload`.
5. Serialize the outer envelope to UTF-8 JSON.
6. Write its byte length as a four-byte little-endian signed integer.
7. Write exactly that many JSON bytes and flush.
8. Read and validate the framed response.
9. Verify response protocol version, message type, and request ID match the request.
10. Deserialize `ControlResponse` from the response envelope's `Payload` string.
11. Validate `ProtocolWelcome`, then inspect `IsSuccessful` and `ErrorCode`.
12. Close the command connection.

The nested payload is intentional in protocol version 1:

```json
{
  "ProtocolVersion": 1,
  "MessageType": 11,
  "RequestId": "5e81ee74-6ae3-4866-aac4-88b1ace2ef59",
  "Hello": {
    "MinimumProtocolVersion": 1,
    "MaximumProtocolVersion": 1,
    "ClientKind": 4,
    "ClientId": "vendor.product.integration",
    "DeviceId": "",
    "DisplayName": "Product integration",
    "RequiredCapabilities": ["protocol-negotiation"],
    "OptionalCapabilities": ["profiles"]
  },
  "Payload": "{\"DetailedProfileId\":\"\",\"IncludeActiveProfileDetail\":false,\"IncludeCurrentLayoutDetail\":false}"
}
```

Use the [wire examples](examples/) and [JSON Schema](schemas/displaymagician-ipc.schema.json) when implementing framing and serialization.

## 7. Startup workflow

On integration startup:

1. Start the event subscription if the integration presents live status.
2. Validate the subscription acknowledgement and retain its active-operation and pending-decision snapshot.
3. Call `GetServiceStatus`.
4. List only the resource types the integration presents.
5. Build buttons/actions from stable resource IDs rather than display names.
6. Reconcile any operation IDs persisted by the integration with the acknowledgement snapshot or `GetOperationStatus`.
7. If ControlService is unavailable, show an unavailable state and reconnect with bounded backoff.

Do not cache a list indefinitely. Refresh on explicit user request, relevant sync completion, reconnect, or a suitable integration-specific interval.

## 8. Listing actions

### 8.1 Display profiles

Send `ListProfiles` (`11`) with a `ProfileListRequest`. For a compact controller list:

- leave `DetailedProfileId` empty;
- set `IncludeActiveProfileDetail` to `false`;
- set `IncludeCurrentLayoutDetail` to `false`.

Use each returned profile ID as the command identifier and the profile name only for display.

### 8.2 Audio profiles

Send `ListAudioProfiles` (`19`) with an empty payload. Retain the returned stable audio-profile ID.

### 8.3 Shortcuts

Send `ListShortcuts` (`35`) with an empty payload. Use the client-safe shortcut view returned by ControlService; do not read the underlying repository files.

## 9. Starting and tracking work

For `ApplyProfile`, `ApplyAudioProfile`, or `StartShortcut`:

1. Generate one operation GUID for the work.
2. Generate a separate request GUID for the submission.
3. Build the typed request with the selected saved-resource ID and operation ID.
4. Send the command.
5. Treat a successful response as acceptance, not completion.
6. Record the operation ID with the integration action that started it.
7. Follow `OperationStatusUpdated` events.
8. If events are unavailable, poll `GetOperationStatus`.
9. Continue until `IsTerminal` is true.
10. Use `IsSuccessful`, phase, and server message to present the outcome.

`OperationStatus.Sequence` must increase for newer state. Ignore an update older than the latest sequence already processed for that operation.

Do not infer success because the active profile changed, because the pipe response arrived, or because a fixed timer elapsed.

## 10. Event subscription

1. Open the event pipe.
2. Send one `SubscribeClientEvents` (`44`) envelope with a fresh request ID and normal `ProtocolHello`.
3. Validate the correlated `ControlResponse` acknowledgement.
4. Process the acknowledgement's operation and decision snapshots before later events.
5. Read `ClientEvent` (`45`) envelopes until cancellation or failure.
6. Handle only the payload property appropriate to the numeric event type.
7. Treat a heartbeat as connection health, not a domain change.
8. If no envelope arrives for 45 seconds, reconnect.
9. On reconnect, use the new acknowledgement as the recovery snapshot.

Event callbacks must be marshalled to the integration's UI thread when required by its UI framework.

## 11. Decisions

When an operation enters `AwaitingUserDecision`:

- show the service-provided title and message;
- show only the allowed choices;
- identify the affected operation;
- send `ResolveOperationDecision` with the prompt ID and selected choice;
- handle `DecisionUnavailable` as already answered, expired, or no longer valid;
- continue tracking the operation after the answer.

A decision may be answered by another authorized DMv4 client. The integration must accept a resolved-decision event even when it did not send the response.

If the integration does not implement decisions, it should display a concise instruction to answer in the main DisplayMagician application rather than leaving the operation apparently stuck.

## 12. Cancellation

Offer cancellation only for a non-terminal operation that the integration can identify. Send `CancelOperation` with the operation ID, then continue tracking status until the terminal cancellation, failure, or successful completion state arrives.

A successful cancellation request means cancellation was accepted; it does not mean restoration has already completed.

## 13. Retry and duplicate prevention

- A read-only command can be retried with a new request ID.
- A replay-protected mutation can be retried with the same request ID only when message type and payload are byte-for-byte equivalent at the semantic contract level.
- Never reuse a request ID for different work.
- Preserve the operation ID when retrying a lost apply/run submission.
- Do not automatically retry a mutation with a new request ID after an ambiguous timeout.
- Respect the protocol's 24-hour bounded replay behavior; it is recovery protection, not permanent operation history.

If a mutation is not documented as replay-protected, present an explicit uncertain outcome and reconcile state before offering another attempt.

## 14. Error handling

Handle transport and protocol failures separately:

| Failure | Client behavior |
|---|---|
| Pipe not found/unavailable | Show DisplayMagician unavailable; retry with bounded backoff |
| Connection or response timeout | Close the pipe; reconcile before retrying a mutation |
| Invalid frame or mismatched response | Reject the response and reconnect |
| Incompatible protocol/capability | Disable the affected integration feature and explain the required DMv4 version |
| Agent unavailable/unhealthy | Keep ControlService connectivity but show that interactive actions are unavailable |
| Session locked/not active | Do not loop retries; show the server message |
| Display control busy/recovery required | Keep tracking existing state and direct the user to DMv4 when intervention is required |
| Resource not found | Refresh the corresponding list and remove/update stale buttons |
| Operation/decision not found | Reconcile from operation/decision lists or the event snapshot |

Never convert a failed `ControlResponse` into a success-shaped empty list or silently ignore it.

## 15. Data handling and security

- Trust ControlService's same-user routing; do not put a Windows SID/session into a public request as authority.
- Do not log payloads that may contain secrets, paths, program arguments, or private user data.
- Do not expose the public named pipe through TCP, HTTP, WebSocket, or another network listener.
- Keep any plugin-to-adapter protocol private and restricted to the exact controller operations required.
- Do not send internal `ControlMessageType` values to the public pipe.
- Do not use Gateway REST from a same-PC integration merely to avoid implementing named pipes.

## 16. Recommended client abstraction

Until a supported `DisplayMagician.Client` package exists, keep protocol handling behind one integration-owned client abstraction that provides:

- typed list/apply/run/status/decision methods;
- one envelope/framing implementation;
- response correlation and negotiation validation;
- mutation retry/idempotency handling;
- one reconnectable event subscription;
- cancellation and deterministic disposal;
- a small mapping from DMv4 results to the host integration's button/action model.

Do not spread numeric message values, pipe names, framing code, or nested-payload serialization throughout individual plugin actions.

## 17. Conformance checklist

A local controller integration is ready when it:

- [ ] connects only to the two public local pipes;
- [ ] uses `ClientKind=LocalIntegration`;
- [ ] sends and validates `ProtocolHello`/`ProtocolWelcome`;
- [ ] implements exact framing and nested payload serialization;
- [ ] correlates message type and request ID;
- [ ] exposes only the supported local-controller profile;
- [ ] uses stable saved-resource IDs;
- [ ] treats apply/run commands as asynchronous operations;
- [ ] reconnects events and processes the acknowledgement snapshot;
- [ ] handles pending decisions or directs the user to the main application;
- [ ] preserves mutation request/operation IDs across ambiguous retries;
- [ ] distinguishes transport, protocol, Agent, and operation failures;
- [ ] never bridges trusted local IPC to the network;
- [ ] has tests for framing, negotiation, retries, event recovery, and stale saved-resource IDs.

