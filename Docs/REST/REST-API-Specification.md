# DisplayMagician v4 REST API specification

**Status:** Target API specification - not fully implemented

**API major version:** 1

**Documentation reorganized:** 2026-10-08

**Available prototype contract:** [openapi.yaml](openapi.yaml)

## 1. Purpose and contract status

This document defines the target remote-controller API for DisplayMagician v4 (DMv4). It specifies API behavior, not the implementation schedule.

No REST client has shipped against the prototype. Implement the target directly; no migration routes, compatibility aliases, or deprecation periods are required.

The target supports Android and Apple phone/tablet applications, companion-relayed watches, and explicitly paired integrations. Gateway remains LAN-only. Editing, diagnostics, and machine administration remain local IPC responsibilities.

This specification is not a claim that the target routes already exist. [openapi.yaml](openapi.yaml) documents the available prototype behavior, which still differs in routes, authentication, response bodies, and events. Use that file when testing the current build. Target examples must not be used unchanged against the prototype.

See the [client guide](REST-API-Client-Guide.md) for usage, the [architecture guide](REST-API-Architecture.md) for topology and rationale, and the [roadmap](REST-API-ROADMAP.md) for delivery work and unresolved contract details.

The key words **MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY**, when used in uppercase, have the meanings defined by RFC 2119 and RFC 8174. Existing lower-case requirements describe the target design but do not make incomplete schemas implementation-ready.

## 2. Target design requirements

| Area | Target decision |
|---|---|
| Network exposure | Gateway listens on the local network only. No direct Internet exposure or port forwarding. |
| Transport | All Gateway requests use HTTPS. |
| Server trust | Pairing pins the Gateway TLS public key/SPKI and stable host identity, not a short-lived complete certificate fingerprint. |
| TLS renewal | Gateway renews its TLS certificate automatically while retaining the pinned TLS key/SPKI whenever possible. |
| Client authentication | Pairing issues a long-lived, random, per-device bearer credential. |
| Client credential expiry | Device credentials have no routine expiry; they remain valid until revoked or explicitly rotated. |
| Request signatures | Remove the custom per-request P-256 signature, timestamp, and nonce scheme from the target REST API. |
| HMAC and mTLS | Do not use HMAC or client TLS certificates for normal clients. |
| Idempotency | Mutations use the standard `Idempotency-Key` header independently of authentication and tracing. |
| Correlation | Use W3C `traceparent` where available and return a DMv4 request ID for support correlation. |
| Async commands | Applying/running creates an operation and returns `202 Accepted` with a `Location` header. |
| Operation IDs | Gateway/ControlService generates the operation ID; REST clients do not need to generate it. |
| Errors | Use RFC 9457 Problem Details with DMv4 extension properties. |
| JSON enums | REST uses stable readable string values rather than numeric enum values. |
| Windows identity | Do not expose Windows SIDs or raw Windows session IDs to remote clients. |
| Watches | Watches communicate through their companion phone and never pair directly with Gateway in version 1. |
| Telegram | A local outbound-polling connector pairs with Gateway as a restricted REST client. Telegram never connects directly to Gateway. |
| Marketplace licensing | Store entitlement remains a client responsibility and is not Gateway authentication. |

## 3. Credential and certificate lifecycle

### 3.1 Stable host identity

Gateway owns a stable P-256 host identity created during installation:

- private key remains in the Windows machine key store;
- public key is included in the trusted pairing payload;
- it has no routine expiry;
- it changes only after reinstall, explicit identity reset, or key loss;
- private keys are never placed in logs, support bundles, or normal configuration exports; public pairing identity remains available for verification.

### 3.2 TLS certificate and key

Gateway also owns an HTTPS TLS key and certificate.

Target behavior:

1. Pairing records the TLS public-key/SPKI pin and stable host identity.
2. Gateway renews/reissues the certificate well before expiration.
3. Normal renewal retains the TLS private key and therefore the SPKI pin.
4. Certificate renewal does not require devices to re-pair.
5. Old and new certificates may overlap during a controlled transition.
6. TLS key replacement is an exceptional security event and requires a host-identity-authorized rotation design or explicit local approval/re-pairing.
7. Clients must never silently accept an unrelated TLS key because the host name or IP address matches.

The certificate remains necessary for HTTPS, but certificate expiry should not invalidate the paired host identity.

### 3.3 Per-device bearer credential

After pairing approval, Gateway returns a random credential once:

```http
Authorization: Bearer <256-bit-random-device-credential>
```

Requirements:

- unique per paired device or connector;
- at least 256 bits of cryptographic randomness;
- no routine expiry;
- stored only as a cryptographic hash by ControlService;
- stored in Apple Keychain, Android secure storage backed by Keystore where available, or Windows DPAPI;
- never placed in URLs, logs, SSE data, support bundles, or later pairing/device-list responses;
- revocable independently;
- replaceable by explicit re-pairing or credential rotation;
- compared in constant time after hashing;
- accepted only over HTTPS.

The target REST API does not require custom request signatures, timestamps, or nonce headers. HTTPS protects the credential in transit. Capability checks, revocation, rate limits, and idempotency remain mandatory.

## 4. Resource model

### 4.1 Saved definitions

These are reusable definitions:

- display profiles;
- audio profiles;
- shortcuts.

They can be listed and individually retrieved. Remote editing is intentionally excluded.

### 4.2 Operations

An operation is one asynchronous execution, such as:

- applying a display profile;
- applying an audio profile;
- starting a shortcut;
- restoring temporary state.

An operation has its own identity and lifecycle because it can:

- outlive the request that created it;
- span display, audio, program, game, and restoration work;
- emit progress;
- require a decision;
- be cancelled;
- finish after the phone disconnects;
- remain in bounded recent history.

Operations are therefore top-level resources:

```text
/v1/operations/{operationId}
```

They link back to their source definition rather than being permanently nested under it.

### 4.3 Decisions

A decision is a prompt owned by an active operation. Its canonical location is beneath that operation:

```text
/v1/operations/{operationId}/decisions/{decisionId}
```

A top-level pending-decision collection is also provided as an inbox:

```text
GET /v1/decisions?status=pending
```

The inbox item links to the canonical operation/decision resource.

### 4.4 Targets and Windows sessions

Remote clients must not receive Windows user SIDs or raw Windows session IDs.

Version 1 should normally route to the healthy pairing-bound/default interactive target automatically. If real multi-target selection is required, add:

```text
GET /v1/targets
```

Targets use stable opaque IDs and client-safe display names. Do not expose `targetSessionId` as the long-term public selector.

## 5. Target routes

### 5.1 Identity and capabilities

```text
GET /v1/identity
GET /v1/capabilities
```

`GET /v1/identity` returns public pairing identity information.

`GET /v1/capabilities` returns only public-safe compatibility metadata:

- REST API versions;
- product/Gateway version;
- stable host ID;
- supported controller features;
- known capability identifiers;
- maximum request size;
- event transports;
- minimum supported client version, if introduced.

Neither endpoint exposes user, session, profile, operation, or paired-device data.

Known capabilities describe server support, not the authenticated device's grants. Clients must distinguish the two. Public-safe compatibility metadata does not authorize commands or expose a user's status.

OpenAPI method names:

```text
getIdentity
getCapabilities
```

### 5.2 Pairing requests

```text
POST /v1/pairing-requests
GET  /v1/pairing-requests/{pairingRequestId}
GET  /v1/pairing-requests
PUT  /v1/pairing-requests/{pairingRequestId}/decision
```

#### Create pairing request

`POST /v1/pairing-requests` submits the scanned one-time pairing secret, device public metadata, and requested capabilities.

Response:

```http
HTTP/1.1 202 Accepted
Location: /v1/pairing-requests/{pairingRequestId}
```

#### Poll candidate pairing status

The unpaired candidate calls:

```http
GET /v1/pairing-requests/{pairingRequestId}
Authorization: DisplayMagician-Pairing <one-time-pairing-credential>
```

The one-time credential must not appear in the URL or query string.

#### List and decide from an existing paired client

Listing and deciding require `pairing-approve`.

Decision body:

```json
{
  "decision": "approved",
  "grantedCapabilities": [
    "status-read",
    "display-profiles-read",
    "display-profiles-apply"
  ]
}
```

or:

```json
{
  "decision": "rejected"
}
```

Rules:

- a candidate cannot approve itself;
- requests are user-scoped;
- grants cannot exceed the candidate request;
- expired or resolved requests return Problem Details;
- pairing secrets and credentials never appear in list responses.

OpenAPI method names:

```text
requestPairing
getPairingRequest
listPairingRequests
decidePairingRequest
```

### 5.3 Paired devices

```text
GET    /v1/devices
DELETE /v1/devices/{deviceId}
```

Rules:

- listing requires `devices-read`;
- revocation requires `devices-revoke`, except an explicitly permitted self-revocation policy;
- results are scoped to the paired user;
- credentials, credential hashes, pairing secrets, and public-key material are excluded;
- revocation takes effect before success is returned;
- successful deletion returns `204 No Content`.

OpenAPI method names:

```text
listDevices
revokeDevice
```

### 5.4 Display profiles

```text
GET  /v1/display-profiles
GET  /v1/display-profiles/{profileId}
POST /v1/display-profiles/{profileId}/applications
```

Rules:

- listing/reading requires `display-profiles-read`;
- applying requires `display-profiles-apply`;
- IDs are stable;
- list results are compact and bounded;
- artwork is optional through `includeArtwork`;
- detail/artwork sizes are bounded;
- list/detail responses return ETags and honor `If-None-Match`;
- applying accepts no arbitrary display configuration from the remote client;
- successful application submission returns an operation.

Apply response:

```http
HTTP/1.1 202 Accepted
Location: /v1/operations/{operationId}
```

```json
{
  "operationId": "4eca79f6-8fcf-418c-b55d-61dfa14ba5bd",
  "status": "requested",
  "href": "/v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd"
}
```

OpenAPI method names:

```text
listDisplayProfiles
getDisplayProfile
applyDisplayProfile
```

### 5.5 Audio profiles

```text
GET  /v1/audio-profiles
GET  /v1/audio-profiles/{profileId}
POST /v1/audio-profiles/{profileId}/applications
```

Rules mirror display profiles:

- `audio-profiles-read`;
- `audio-profiles-apply`;
- stable IDs;
- compact and cacheable results;
- bounded response sizes;
- no remote editing;
- application creates an operation and returns `202 Accepted`.

OpenAPI method names:

```text
listAudioProfiles
getAudioProfile
applyAudioProfile
```

### 5.6 Shortcuts

```text
GET  /v1/shortcuts
GET  /v1/shortcuts/{shortcutId}
POST /v1/shortcuts/{shortcutId}/runs
```

Rules:

- listing/reading requires `shortcuts-read`;
- running requires `shortcuts-run`;
- icon inclusion is optional and bounded;
- results use ETags;
- remote clients submit only a saved shortcut ID;
- remote clients cannot submit executable paths, arguments, or replacement shortcut definitions;
- running creates an operation and returns `202 Accepted`.

OpenAPI method names:

```text
listShortcuts
getShortcut
runShortcut
```

### 5.7 Operations

```text
GET  /v1/operations
GET  /v1/operations/{operationId}
POST /v1/operations/{operationId}/cancellations
```

Rules:

- reading requires `status-read`;
- cancellation requires `operations-cancel`;
- operation IDs are server-generated;
- only visible operations for the authenticated user/default target are returned;
- terminal operations cannot be cancelled;
- cancellation acceptance is not operation completion;
- recent terminal history is bounded and documented;
- list results use cursor pagination;
- resource bodies use readable string states and phases.

#### Operation status resource

`GET /v1/operations/{operationId}` returns the latest authoritative status snapshot for one operation. The phone uses it to show what DMv4 is currently doing on the PC, recover after reconnecting, and verify the result of a command whose HTTP response was lost.

Example:

```json
{
  "id": "4eca79f6-8fcf-418c-b55d-61dfa14ba5bd",
  "type": "run-shortcut",
  "status": "running",
  "phase": "waiting-for-game-to-close",
  "message": "Waiting for iRacing to close.",
  "sequence": 14,
  "startedAt": "2026-10-08T09:58:12Z",
  "updatedAt": "2026-10-08T10:16:42Z",
  "completedAt": null,
  "isTerminal": false,
  "source": {
    "type": "shortcut",
    "id": "iracing",
    "name": "iRacing",
    "href": "/v1/shortcuts/iracing"
  },
  "error": null,
  "links": {
    "self": "/v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd",
    "decisions": "/v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd/decisions",
    "cancellations": "/v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd/cancellations"
  }
}
```

`status` describes the overall lifecycle:

```text
requested
running
waiting-for-decision
succeeded
failed
cancelled
```

`phase` gives the more detailed current activity:

```text
requested
validating
applying-display-profile
applying-audio-profile
starting-programs
starting-game
waiting-for-game-to-start
waiting-for-game-to-close
running-after-programs
restoring-display-profile
restoring-audio-profile
waiting-for-decision
completed
failed
cancelled
```

An operation is not required to report a percentage. Many phases have an unknown duration, so clients should present the readable phase and server-provided message rather than inventing progress.

The operation `sequence` increases whenever authoritative status changes. Clients must ignore an event or cached representation with a lower sequence than the newest representation already processed for that operation.

When an operation is terminal:

- `isTerminal` is `true`;
- `status` is `succeeded`, `failed`, or `cancelled`;
- `completedAt` is populated;
- `error` is populated only for failure information;
- later reads continue returning the retained terminal result until operation-history retention expires.

The route returns:

| Condition | Response |
|---|---|
| Visible retained operation | `200 OK` with the latest representation |
| Conditional request unchanged | `304 Not Modified` |
| Unknown, expired, or non-visible operation | `404 Not Found` Problem Details |
| Temporarily unavailable target | `503 Service Unavailable` Problem Details |

Operation responses use `Cache-Control: no-store`. An ETag may still be used for short-lived conditional polling without permitting persistent caching.





Cancellation response:

```http
HTTP/1.1 202 Accepted
Location: /v1/operations/{operationId}
```

OpenAPI method names:

```text
listOperations
getOperation
cancelOperation
```

### 5.8 Decisions

Canonical routes:

```text
GET /v1/operations/{operationId}/decisions
GET /v1/operations/{operationId}/decisions/{decisionId}
PUT /v1/operations/{operationId}/decisions/{decisionId}/response
```

Convenience inbox:

```text
GET /v1/decisions?status=pending
```

Rules:

- reading requires `decisions-read`;
- responding requires `decisions-answer`;
- choices use readable string values such as `continue` and `stop-and-restore`;
- the route IDs are authoritative;
- a response choice must be allowed by the decision;
- identical repeated `PUT` responses are idempotent;
- a different second answer returns `409 Conflict`;
- expired, resolved, and unavailable decisions return Problem Details;
- no decision from another user/target is disclosed.

OpenAPI method names:

```text
listOperationDecisions
getOperationDecision
answerDecision
listPendingDecisions
```

### 5.9 Dashboard snapshot

Optional convenience endpoint:

```text
GET /v1/dashboard
```

The dashboard may aggregate:

- active/recent operations when the caller has `status-read`;
- pending decisions when the caller has `decisions-read`;
- active display/audio profile summary when corresponding read capabilities exist.

It must omit unauthorized sections rather than using one capability to disclose another resource class.

OpenAPI method:

```text
getDashboard
```

### 5.10 Events

```text
GET /v1/events
```

SSE requirements:

- authenticate before establishing the stream;
- send an authoritative initial snapshot;
- emit only resource classes allowed by the device's capabilities;
- use typed event names;
- include SSE `id` values;
- honor `Last-Event-ID` when retained history permits;
- send a fresh snapshot when continuity cannot be guaranteed;
- send keep-alive comments;
- close when the credential is revoked;
- document bounded reconnect backoff;
- limit concurrent streams per device/user.

Example event:

```text
id: 1842
event: operation.updated
data: {"operationId":"4eca79f6-8fcf-418c-b55d-61dfa14ba5bd","phase":"waiting-for-game-to-close","status":"running"}
```

OpenAPI method:

```text
streamEvents
```

## 6. HTTP behavior

### 6.1 Success responses

| Operation | Response |
|---|---|
| Get/list resource | `200 OK` with typed resource body |
| Conditional get unchanged | `304 Not Modified` |
| Async command accepted | `202 Accepted` with `Location` |
| Pairing request accepted | `202 Accepted` with `Location` |
| Successful idempotent decision response | `200 OK` with updated decision |
| Successful revocation | `204 No Content` |

REST routes must not return the sparse IPC `ControlResponse` as their public success body. Gateway translates successful internal responses into route-specific REST resources.

### 6.2 RFC 9457 Problem Details

Errors use `application/problem+json`:

```json
{
  "type": "https://displaymagician.org/problems/validation-failed",
  "title": "Validation failed",
  "status": 422,
  "detail": "The display profile is unavailable.",
  "instance": "/v1/display-profiles/racing/applications",
  "errorCode": "validation-failed",
  "requestId": "8c632c3c-3053-49e5-b6e0-5db238e3b8eb",
  "operationId": null,
  "retryable": false
}
```

Minimum mapping:

| Condition | Status |
|---|---:|
| Malformed JSON/request | 400 |
| Missing, invalid, or revoked credential | 401 |
| Missing capability or wrong user/target | 403 |
| Resource not found | 404 |
| Expired/resolved decision, busy operation, recovery conflict | 409 |
| Syntactically valid but invalid domain input | 422 |
| Body too large | 413 |
| Rate limited | 429 |
| Agent/target temporarily unavailable | 503 |
| Unexpected execution failure | 500 |

Use `Retry-After` for retryable 429 and 503 responses where the server can provide useful guidance.

### 6.3 Idempotency

Every mutation accepts:

```http
Idempotency-Key: <opaque-client-generated-value>
```

Rules:

- the key is scoped to the authenticated device;
- retrying the same method, target URI, and body returns the retained result;
- reusing a key with different request content returns `409 Conflict`;
- retained results have a documented lifetime;
- the key is not an authentication credential;
- Gateway maps it to internal ControlService replay protection;
- the response echoes the accepted key where appropriate.

### 6.4 Correlation and tracing

Clients may send W3C Trace Context:

```http
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
```

Gateway:

- preserves valid trace context;
- creates correlation when none is supplied;
- returns a DMv4 request ID header;
- preserves `operation_id` and `request_id` through ControlService/UserAgent;
- never uses trace IDs as idempotency keys or credentials.

### 6.5 JSON representation

REST JSON uses:

- camelCase property names;
- stable string enum values;
- ISO 8601 UTC timestamps;
- GUIDs serialized as strings; other resource IDs follow their resource schema and are treated as opaque by clients;
- null only when absence is semantically meaningful;
- typed route-specific resources;
- no Windows SID or raw session ID fields.

IPC numeric enum values remain unchanged.

### 6.6 Pagination and filtering

Changing collections use cursor pagination:

```text
GET /v1/operations?limit=50&cursor=<opaque-token>
```

```json
{
  "items": [],
  "nextCursor": null
}
```

Requirements:

- cursor is opaque;
- maximum and default `limit` values are documented;
- stable deterministic ordering is defined;
- timestamps are not the sole continuation mechanism;
- unknown/expired cursors return Problem Details;
- small definition collections may remain unpaged but have explicit item and byte limits.

### 6.7 Caching

Display profile, audio profile, and shortcut list/detail routes:

- return ETags;
- honor `If-None-Match`;
- bound artwork size;
- default to compact representations;
- use explicit artwork/detail inclusion flags.

Sensitive status, operation, decision, pairing, and device responses use appropriate no-store cache directives.

### 6.8 Rate and connection limits

Document and enforce:

- maximum request body size;
- maximum response/list size;
- maximum pairing polling rate;
- maximum concurrent SSE streams per device/user;
- maximum failed-authentication rate;
- bounded server retention for idempotency and events;
- `Retry-After` behavior.

The phone should coalesce duplicate watch/phone refreshes.

## 7. Capability model

Proposed readable capability identifiers:

```text
status-read
decisions-read
decisions-answer
display-profiles-read
display-profiles-apply
audio-profiles-read
audio-profiles-apply
shortcuts-read
shortcuts-run
operations-cancel
pairing-approve
devices-read
devices-revoke
```

The target uses `display-profiles-read` and `display-profiles-apply` consistently with the display-profile resource names.

Recommended full phone-controller grant:

```text
status-read
decisions-read
decisions-answer
display-profiles-read
display-profiles-apply
audio-profiles-read
audio-profiles-apply
shortcuts-read
shortcuts-run
operations-cancel
pairing-approve
devices-read
devices-revoke
```

Recommended Telegram connector grant:

```text
status-read
decisions-read
decisions-answer
display-profiles-read
display-profiles-apply
audio-profiles-read
audio-profiles-apply
shortcuts-read
shortcuts-run
operations-cancel
```

Clients must disable unavailable UI rather than repeatedly invoking unauthorized routes.

## 8. Authentication and capability reference

All routes use HTTPS. This table summarizes the target access requirements; it does not mean these routes are implemented.

| Resource or action | Authentication | Required grant |
|---|---|---|
| Identity | Public pairing identity | None |
| Capabilities | Public-safe compatibility metadata; exact request negotiation remains to be finalized | None for public metadata |
| Submit pairing request | Valid one-time QR pairing secret | No device grant yet |
| Poll candidate pairing request | Temporary pairing authorization for that request | No device grant yet |
| List or decide pairing requests | Paired-device bearer credential | `pairing-approve` |
| List devices | Paired-device bearer credential | `devices-read` |
| Revoke a device | Paired-device bearer credential | `devices-revoke`; no self-revocation exception is specified yet |
| Read display profiles | Paired-device bearer credential | `display-profiles-read` |
| Apply display profile | Paired-device bearer credential | `display-profiles-apply` |
| Read audio profiles | Paired-device bearer credential | `audio-profiles-read` |
| Apply audio profile | Paired-device bearer credential | `audio-profiles-apply` |
| Read shortcuts | Paired-device bearer credential | `shortcuts-read` |
| Run shortcut | Paired-device bearer credential | `shortcuts-run` |
| Read operations | Paired-device bearer credential | `status-read` |
| Cancel operation | Paired-device bearer credential | `operations-cancel` |
| Read decisions, including the pending inbox | Paired-device bearer credential | `decisions-read` |
| Answer decision | Paired-device bearer credential | `decisions-answer` |
| Optional dashboard | Paired-device bearer credential | Each included section's read grant |
| Events | Paired-device bearer credential | Each emitted resource class's read grant |

A write capability does not imply a read capability. For example, a controller that applies a profile and displays its progress needs `display-profiles-apply` and `status-read`; displaying the profile picker also needs `display-profiles-read`.

Authentication, capability checks, and user/default-target routing must occur before executing work. Neither a resource ID nor an operation ID conveys authority. Authorization is enforced by ControlService even when Gateway has already checked an HTTP request.

The [shared protocol](../IPC/IPC-Specification.md#7-compatibility-negotiation) requires `ProtocolHello`/`ProtocolWelcome` compatibility metadata. The exact REST representation and handling of public bootstrap requests are not yet specified. API version discovery must not be presented as a replacement for the shared negotiation requirement without an explicit contract decision.

## 9. Contract completeness and interpretation

### 9.1 Examples are not complete schemas

Examples illustrate behavior and resource relationships. They are not a substitute for required/optional property definitions, constraints, or route-specific schemas. In particular:

- IDs should be treated as opaque strings. Sample names such as `racing` are illustrative; do not derive IDs from names or assume all saved-resource IDs are GUIDs.
- Operation IDs are server-generated. Do not confuse an operation ID with an OpenAPI `operationId`, which names a generated client method.
- `href` and `Location` identify resources, not an authorization grant.
- Problem Details `requestId` is for support correlation, not duplicate prevention.
- A successful HTTP submission and a successful terminal operation are distinct results.
- Capability availability, pairing authorization, and marketplace entitlement are distinct concepts.

### 9.2 Items required before the target is implementation-ready

The [roadmap's contract completion register](REST-API-ROADMAP.md#6-contract-completion-register) records unresolved details. Developers must not infer the following from illustrative examples:

- complete identity, capability, pairing, device, profile, shortcut, operation, decision, dashboard, and event schemas;
- exact credential-delivery and lost-response recovery behavior during pairing;
- TLS trust validation and SPKI encoding rules across supported platforms;
- required shared compatibility metadata on REST requests and responses;
- SSE snapshot format, event payloads, replay boundaries, and event-ID scope;
- concrete idempotency retention, collection limits, polling rates, and operation-history retention;
- exact correlation header names and error-code mappings;
- policy for revocation while work is running and any optional self-revocation exception.

These gaps do not weaken existing requirements. They prevent clients from guessing unspecified behavior. OpenAPI must be updated with complete schemas and examples as each route is implemented.
