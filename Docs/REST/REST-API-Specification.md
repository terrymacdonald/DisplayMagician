# DisplayMagician v4 REST API specification

**Status:** Target API specification - not fully implemented

**API major version:** 1

**Documentation reorganized:** 2026-10-08

**Available prototype contract:** [openapi.yaml](openapi.yaml)

**Design-only target schemas in progress:** [target-openapi.json](target-openapi.json)

## 1. Purpose and contract status

This document defines the target remote-controller API for DisplayMagician v4 (DMv4). It specifies API behavior, not the implementation schedule.

No REST client has shipped against the prototype. Implement the target directly; no migration routes, compatibility aliases, or deprecation periods are required.

The target supports Android and Apple phone/tablet applications, companion-relayed watches, and explicitly paired integrations. The first release supports local and routed private networks. Gateway does not configure router port forwarding or offer user-enabled Internet access in that release. Editing, diagnostics, and machine administration remain local IPC responsibilities.

This specification is not a claim that the target routes already exist. [openapi.yaml](openapi.yaml) documents the available prototype behavior, which still differs in routes, authentication, response bodies, and events. Use that file when testing the current build. Target examples must not be used unchanged against the prototype.

See the [client guide](REST-API-Client-Guide.md) for usage, the [architecture guide](REST-API-Architecture.md) for topology and rationale, and the [roadmap](REST-API-ROADMAP.md) for delivery work and unresolved contract details.

The key words **MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY**, when used in uppercase, have the meanings defined by RFC 2119 and RFC 8174. Existing lower-case requirements describe the target design but do not make incomplete schemas implementation-ready.

## 2. Target design requirements

| Area | Target decision |
|---|---|
| Network exposure | First-release support includes local and routed private networks, without a hard-coded same-subnet client check. Installer firewall rules allow Private and Domain Windows network profiles by default; an administrator may also enable the Public profile in WinForms Server Settings. This setting does not configure router forwarding or advertise WAN access. Official user-enabled port-forwarding support is deferred; deliberate router/firewall changes may still make Gateway reachable from the Internet. |
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
- `hostId` is the unpadded base64url-encoded SHA-256 [RFC 7638 JWK thumbprint](https://www.rfc-editor.org/rfc/rfc7638.html) of the public P-256 key, computed from only the canonical `crv`, `kty`, `x`, and `y` JWK members;
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
6. TLS private-key replacement is an exceptional security event. In the first release it requires explicit approval in the local WinForms application, invalidates existing paired-device credentials, and requires devices to verify a fresh QR payload and pair again. There is no silent host-identity-signed TLS key rotation.
7. Clients must never silently accept an unrelated TLS key because the host name or IP address matches.

The certificate remains necessary for HTTPS, but certificate expiry should not invalidate the paired host identity.

Loss or explicit reset of the stable host identity key also invalidates all paired-device credentials and pairing sessions. Devices must verify the new identity locally through a fresh QR payload and pair again. A backup/restore procedure must not silently pair a new host identity with old device credentials.

The `tlsSpkiSha256` pin is padded RFC 4648 Base64 of SHA-256 over the certificate's DER-encoded X.509 SubjectPublicKeyInfo. It covers the algorithm identifier and public key, not the whole certificate or a JWK serialization. This is the [SPKI fingerprint definition](https://www.rfc-editor.org/rfc/rfc7469.html#section-2.4); the API does not use the HTTP Public-Key-Pins header.

The pairing QR supplies the expected pin before the phone makes any authenticated request. For each HTTPS connection, the client checks the presented certificate's SPKI against that pin, its validity dates and server-use constraints, and a DNS or IP subject alternative name matching the URI host. A self-signed certificate may be trusted only as the explicitly pinned Gateway certificate in the app's server-trust evaluation. Neither a matching IP address alone nor a matching pin permits an expired, malformed, or name-mismatched certificate. The [X.509 name forms](https://www.rfc-editor.org/rfc/rfc5280.html#section-4.2.1.6) and [IP-address matching rule](https://www.rfc-editor.org/rfc/rfc6125.html#section-3.1.3.2) govern the certificate and URI names.

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

If the client loses the one-time credential delivery or later loses its stored credential, it must pair again. The server does not provide a credential-recovery endpoint or return the credential through device, status, or support resources.

The 10-minute QR and pairing-request expiry applies only while establishing a new pairing. A paired device's bearer credential has no routine expiry: a phone may reconnect after six months of inactivity with the same credential, provided it has not been revoked or invalidated by a host-identity or TLS-key reset.

The target REST API does not require custom request signatures, timestamps, or nonce headers. HTTPS protects the credential in transit. Capability checks, revocation, rate limits, and idempotency remain mandatory.

## 4. Resource model

### 4.1 Saved definitions

These are reusable definitions:

- display profiles;
- audio profiles;
- shortcuts.

They can be listed and individually retrieved. Remote editing is intentionally excluded.

Saved display-profile, audio-profile, and shortcut IDs are opaque strings returned exactly as stored in the user's repository. Lookup is case-insensitive, matching the current repositories, and responses always return the stored spelling. Clients should retain and send the returned spelling. Renaming an item retains its ID. Deleting an item and later creating another with the same name assigns a new ID; clients must not use names as identity. An imported item keeps its ID when that ID is free in the user's repository under case-insensitive comparison. If it collides, the imported copy receives a new ID and references within the same import are rewritten to that new ID. Host-identity reset does not change the underlying saved-definition IDs, though remote clients must re-pair before reading them.

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

Version 1 routes to the healthy pairing-bound/default interactive target automatically. Multi-target selection is deferred from the first released API. A later version may add:

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
- shared-protocol version and feature support (without a separate minimum app-version rule in the first release).

Neither endpoint exposes user, session, profile, operation, or paired-device data.

Known capabilities describe server support, not the authenticated device's grants. Clients must distinguish the two. Public-safe compatibility metadata does not authorize commands or expose a user's status. After pairing, `GET /v1/devices/current` returns the caller's authoritative current grants.

Every REST request, including public identity and pairing requests, carries `DisplayMagician-Protocol-Hello`. Its value is unpadded base64url encoding of UTF-8 JSON for the shared `ProtocolHello` contract. The metadata keeps its shared-contract property names and numeric `ClientKind` even though REST resource bodies use camelCase and string enums. Every successful response, including an established SSE response, carries `DisplayMagician-Protocol-Welcome` using the same encoding for `ProtocolWelcome`. Gateway validates the version range and required capabilities before route work; negotiation does not authenticate or grant capabilities. A missing or malformed hello receives `400 Bad Request`, no compatible shared protocol version receives `409 Conflict`, and an unsupported required protocol feature receives `412 Precondition Failed`. These failures use REST Problem Details without a welcome header.

The hello's `RequiredCapabilities` and `OptionalCapabilities` are shared protocol features such as `operation-status`, not the hyphenated REST authorization grants listed in section 7. Gateway must use the shared `ControlProtocol.TryCreateWelcome` validation and version-selection rules for the REST handshake. `SupportedCapabilities` and `NegotiatedOptionalCapabilities` in the welcome report protocol features only; they never report the paired device's grants. A client must check the selected version and its required protocol features, then obtain its actual grants through the paired-device contract before enabling protected actions.

The untrusted `ClientKind`, `ClientId`, `DeviceId`, and `DisplayName` values in the hello are compatibility and diagnostic hints. They never establish the authenticated device identity or override the bearer credential, polling secret, or ControlService user association.

For example, a phone may encode this shared-contract JSON as the hello header value (the JSON shown here is decoded for readability):

```json
{
  "MinimumProtocolVersion": 1,
  "MaximumProtocolVersion": 1,
  "ClientKind": 5,
  "ClientId": "displaymagician-ios",
  "DeviceId": "",
  "DisplayName": "DisplayMagician iOS",
  "RequiredCapabilities": ["protocol-negotiation"],
  "OptionalCapabilities": ["operation-status", "client-events"]
}
```

`ClientKind: 5` is the shared `RemoteApplication` enum value. The client sends this header even before pairing; `DeviceId` may be empty then. The header's JSON property order and insignificant whitespace do not affect negotiation.

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

The local WinForms application encodes a pairing QR as `displaymagician://pair?payload=<unpad-base64url-utf8-json>`. Its versioned JSON payload contains `version` (integer `1`), `gatewayUri` (the HTTPS origin), `hostId`, `hostIdentityPublicKeyJwk`, `tlsSpkiSha256`, `pairingRequestId`, `pairingSecret`, and `expiresAt` (UTC). The QR expires 10 minutes after creation. The app link is an OS handoff containing a one-time secret; clients must not paste it into a browser, send it to an HTTP server, log it, or include it in support data. The candidate checks the expiry, URI, pin, and host identity before it submits anything. An expired QR requires a new local QR.

The candidate also generates a separate, cryptographically random 256-bit polling secret before submission and sends it in the request body. Gateway stores only its hash. The candidate retains the secret locally for `DisplayMagician-Pairing` authorization when polling and for an identical idempotent submission retry if the `202` response is lost. The QR pairing secret and polling secret have different purposes; neither is sent in an HTTP URL, log, approval list, or support bundle.

Response:

```http
HTTP/1.1 202 Accepted
Location: /v1/pairing-requests/{pairingRequestId}
```

The JSON body contains `pairingRequestId`, `status: "awaiting-approval"`, `href`, and `expiresAt`. It does not return a second polling secret. The candidate's `Idempotency-Key` is scoped to the verified QR pairing session, including when the first `202` response is lost.

#### Poll candidate pairing status

The unpaired candidate calls:

```http
GET /v1/pairing-requests/{pairingRequestId}
Authorization: DisplayMagician-Pairing <phone-generated-polling-secret>
```

The polling secret must not appear in an HTTP URL or query string. It remains valid to read `awaiting-approval` or `rejected` until the pairing request expires. Approval records the grant but does not create a plaintext device credential. On the first authenticated poll after approval, ControlService generates the credential, commits its hash and the consumed-delivery state atomically, and returns the plaintext only in that poll response. An expired request or consumed delivery returns `410 Gone` Problem Details with distinct `pairing-expired` or `pairing-delivery-consumed` codes. A crash or lost response after the commit requires the candidate to pair again; there is no plaintext recovery store or acknowledgement window.

The polling response is one JSON object with a `status` field. For `awaiting-approval` or `rejected`, it contains `pairingRequestId` and `expiresAt`. For `approved`, it contains `pairingRequestId`, `deviceId`, `credential`, and `grantedCapabilities`. Clients can decode one model and switch on `status`; the server rejects missing fields for the chosen status and does not include a credential in other states.

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

Both choices use the same JSON object shape. `grantedCapabilities` is required and nonempty for `approved`, must be a subset of the requested capabilities, and must be absent for `rejected`. Clients can submit one model with an optional grants field.

Rules:

- a candidate cannot approve itself;
- requests are user-scoped;
- grants cannot exceed the candidate request;
- the approver may grant a strict subset of the requested capabilities;
- when the same user approves re-pairing with an existing device ID, ControlService replaces that device's old credential and grants; the old credential is invalid immediately, and a lost new-credential delivery requires another pairing;
- the same opaque device ID under a different Windows user is a separate association and cannot be replaced by this request;
- expired or resolved requests return Problem Details;
- a rejected candidate can read its `rejected` status until the pairing request's 10-minute expiry; it never receives a credential;
- a decision submitted with a new `Idempotency-Key` after the request is already resolved returns `409 Conflict`, even if the choice matches; retrying the first accepted key returns its retained result;
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
GET    /v1/devices/current
DELETE /v1/devices/{deviceId}
```

Rules:

- reading `/v1/devices/current` requires a valid device bearer credential but no `devices-read` grant; it returns only that credential's device ID, display name, and current granted capabilities;
- listing requires `devices-read`;
- revocation requires `devices-revoke`, including when the caller revokes its own credential;
- results are scoped to the paired user;
- credentials, credential hashes, pairing secrets, and public-key material are excluded;
- revocation takes effect before success is returned;
- revocation prevents new requests and closes existing event streams, but an operation already accepted for execution continues to its normal terminal result;
- successful deletion returns `204 No Content`.

A successful self-revocation response is still delivered to the caller. That credential then fails on subsequent requests, and its established event streams close. Any accepted operation continues, but the revoked device cannot read its result without pairing again.

Authentication precedes idempotency lookup. A retry of self-revocation using the revoked credential receives `401 Unauthorized`, even with the original `Idempotency-Key`.

OpenAPI method names:

```text
listDevices
getCurrentDevice
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
- cancellation of work that can no longer be stopped safely returns `409 Conflict` with `operation-not-cancellable` and leaves the operation running;
- every operation representation includes `canCancel`; it is `true` only while that operation is active, authoritative, and currently at a safe cancellation point. Clients may use it to enable Cancel, but must handle a later `409` if eligibility changes before the request arrives;
- cancellation acceptance is not operation completion;
- terminal history is retained for up to seven days and capped at 100 operations per user; the oldest terminal records are evicted first when the cap is reached;
- once a terminal operation is evicted, its detail route returns the same `404` as an unknown or non-visible operation; clients must not infer whether an inaccessible ID ever existed;
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
  "isFinished": false,
  "isStale": false,
  "staleReason": null,
  "canCancel": false,
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

The sequence is persisted with the operation and remains monotonic across ControlService and UserAgent restarts. While UserAgent is unavailable, ControlService returns its retained active snapshot with `isStale: true` and `canCancel: false`. When UserAgent reconnects, ControlService reconciles active operation IDs. If the Agent no longer has an operation, ControlService advances its sequence and marks it terminal `failed` with a safe `target-unavailable` error. A client must not automatically resubmit that operation, because partial execution may have occurred.

When an operation is terminal:

- `isFinished` is `true`;
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
| Target unavailable, retained operation snapshot exists | `200 OK` with `isStale: true` and a safe `staleReason`; the snapshot remains readable but does not imply current progress |
| Target unavailable and no retained snapshot is available | `503 Service Unavailable` Problem Details |

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
- retrying the first accepted `PUT` with its original `Idempotency-Key` replays `200` and the retained decision result;
- a new-key answer after any other client or the expiry default resolved the decision returns `409 Conflict`, even if it chooses the same option;
- expiry resolves a pending decision to its `defaultChoice` (`continue` for recovery decisions unless the user changes that policy); expired, resolved, and unavailable decisions return Problem Details when a new answer is attempted;
- no decision from another user/target is disclosed.

OpenAPI method names:

```text
listOperationDecisions
getOperationDecision
answerDecision
listPendingDecisions
```

### 5.9 Dashboard snapshot (deferred)

This optional convenience endpoint is deferred from the first released API:

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

SSE is an optional client feature. A phone can complete pairing, invoke actions, read status, and answer decisions using ordinary HTTPS requests and polling. Gateway still offers the event stream for clients that choose it.

- authenticate before establishing the stream;
- send an authoritative snapshot of the caller's visible active/recent operations and pending decisions on a new stream without `Last-Event-ID`, or when its cursor cannot provide continuous replay; other resource collections are fetched through their own routes;
- emit only resource classes allowed by the device's capabilities;
- use typed event names;
- include SSE `id` values;
- for a valid retained `Last-Event-ID`, replay only events after that cursor, then continue with live events; do not send a snapshot on that connection;
- retain replayable event history for up to 24 hours within a published per-user item/byte bound;
- cap replay history at 10,000 events or 10 MiB per user, whichever bound is reached first;
- send a fresh snapshot when the cursor is absent, unknown, or expired; the snapshot's SSE `id` marks its capture boundary for later reconnects;
- send keep-alive comments;
- close when the credential is revoked;
- document bounded reconnect backoff;
- allow at most one concurrent stream per device and four per user; an excess connection receives `429` with `Retry-After` before the SSE response starts.

Event names and payloads in the [design-only target schema](target-openapi.json) are:

| Event | Data | Required read grant |
|---|---|---|
| `snapshot` | Capture time, visible operation snapshots, pending decisions, and any continuity-reset reason; unavailable sections are omitted | Each included section's grant |
| `operation.updated` | Full latest operation representation | `status-read` |
| `decision.updated` | Full latest decision representation | `decisions-read` |
| `display-profile.changed` | Resource ID, `upsert`/`deleted`, and link when present | `display-profiles-read` |
| `audio-profile.changed` | Resource ID, `upsert`/`deleted`, and link when present | `audio-profiles-read` |
| `shortcut.changed` | Resource ID, `upsert`/`deleted`, and link when present | `shortcuts-read` |

An SSE `id` is an opaque, user/host-scoped replay cursor, not the operation's `sequence`. Event order and replay IDs survive Gateway and ControlService restarts while retained. A client must not parse an ID as a number or share it across paired users or hosts. Snapshot sections contain at most 100 items each and indicate whether more are available through the corresponding paged route. Unavailable sections are omitted rather than returned as empty arrays. Keep-alive comments are sent at least every 15 seconds when there is no event, and clients reconnect with jittered exponential backoff starting at one second and capped at 30 seconds. Revocation closes an established stream within five seconds.

Gateway must establish the snapshot boundary or retained replay position atomically with subscription to live events. An event committed during connection setup appears either in the snapshot state or after its cursor, never in a gap between replay and live delivery. Clients use the latest received SSE `id` for the next reconnect; a fresh snapshot replaces their locally retained visible operation and pending-decision state for the sections included in that snapshot.

Example event:

```text
id: 1842
event: display-profile.changed
data: {"resourceType":"display-profile","id":"racing","action":"upsert","href":"/v1/display-profiles/racing"}
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
| Missing or malformed `DisplayMagician-Protocol-Hello` | 400 |
| No compatible shared protocol version | 409 |
| Unsupported required shared protocol feature | 412 |
| Expired pairing request or consumed one-time delivery | 410 |
| Missing, invalid, or revoked credential | 401 |
| Missing route capability | 403 |
| Resource not found, expired from retention, or outside the caller's user/target scope | 404 |
| Expired/resolved decision, busy operation, recovery conflict | 409 |
| Syntactically valid but invalid domain input | 422 |
| Body too large | 413 |
| Rate limited | 429 |
| Agent/target temporarily unavailable | 503 |
| Unexpected execution failure | 500 |

Use `Retry-After` for retryable 429 and 503 responses where the server can provide useful guidance.

The stable `errorCode` values and their primary HTTP statuses are:

| `errorCode` | Status | Meaning |
|---|---:|---|
| `protocol-hello-invalid` | 400 | Missing, malformed, or invalid shared hello |
| `protocol-version-incompatible` | 409 | No shared protocol version overlaps |
| `protocol-capability-unavailable` | 412 | A required shared protocol feature is unsupported |
| `idempotency-key-required`, `idempotency-key-invalid` | 400 | Missing or malformed UUID v4 mutation key |
| `idempotency-key-conflict` | 409 | Key reused for a different request identity |
| `idempotency-pending` | 409 | Same-key work is still being accepted; retryable with `Retry-After` |
| `authentication-required`, `credential-invalid` | 401 | Missing or invalid route credential |
| `capability-denied` | 403 | Authenticated device lacks a route grant |
| `resource-not-found` | 404 | Unknown, expired from retention, or outside the caller's scope |
| `pairing-expired`, `pairing-delivery-consumed` | 410 | Pairing request no longer usable by its candidate |
| `pairing-already-resolved`, `decision-already-resolved`, `operation-not-cancellable`, `operation-busy`, `recovery-conflict` | 409 | A visible resource conflicts with the attempted transition |
| `cursor-invalid` | 400 | Malformed or mismatched collection cursor |
| `cursor-expired` | 410 | A collection cursor's snapshot is no longer retained |
| `validation-failed` | 422 | Syntactically valid input violates a domain rule |
| `request-too-large` | 413 | Request body exceeds 64 KiB |
| `rate-limited` | 429 | Published poll, auth, request, or stream limit reached |
| `target-unavailable` | 503 | Required target state is unavailable and no usable retained response exists |
| `execution-failed`, `internal-error` | 500 | Synchronous execution or unexpected server failure |

`type` is `https://displaymagician.org/problems/{errorCode}`. The type pages must be published before release. A failed asynchronous operation instead remains an operation resource with `status: "failed"` and an `error` object; its original `202` is not retroactively changed into an HTTP error. Map shared `ControlErrorCode` values at the Gateway boundary while preserving the original code internally for support correlation. Do not expose Windows SIDs, session IDs, or unredacted internal messages in `detail`.

The IPC-to-HTTP mapping for shared failure codes is:

| Shared `ControlErrorCode` | REST status and `errorCode` |
|---|---|
| `UnsupportedProtocolVersion`, `IncompatibleProtocolVersion` | `409 protocol-version-incompatible` |
| `RequiredCapabilityUnavailable` | `412 protocol-capability-unavailable` |
| `InvalidRequest` | `400 protocol-hello-invalid` for hello validation; otherwise `422 validation-failed` for a well-formed route request |
| `CallerIdentityMismatch` | `500 internal-error`; this indicates a Gateway/ControlService trust failure, not a device grant failure |
| `AgentNotConnected`, `AgentNotHealthy`, `NotActiveConsoleUser`, `SessionLocked`, `AgentUnavailable` | `503 target-unavailable` when a route requires live target work; retained operation reads follow section 5.7 |
| `DisplayControlBusy` | `409 operation-busy` |
| `RecoveryRequired` | `409 recovery-conflict` |
| `Unauthorized`, `AdministratorRequired` | `403 capability-denied` after valid device authentication |
| `ProfileNotFound`, `AudioProfileNotFound`, `ShortcutNotFound`, `OperationNotFound` | `404 resource-not-found` |
| `ValidationFailed` | `422 validation-failed` |
| `ExecutionFailed` | `500 execution-failed` for synchronous failure; asynchronous failure is retained in the operation resource |
| `DecisionUnavailable` | `409 decision-already-resolved` for a visible resolved decision; otherwise `404 resource-not-found` |
| `AuthenticationRequired`, `PairingRequired` | `401 authentication-required` |

Gateway-specific pairing, replay, cursor, and rate-limit failures use the stable codes in the preceding table. A downstream code is never exposed without rechecking authenticated user scope; cross-user resource IDs resolve as `404 resource-not-found`.

### 6.3 Idempotency

Every mutation, including unauthenticated pairing submission, requires a fresh, client-generated UUID version 4 in canonical lowercase hyphenated form:

```http
Idempotency-Key: 6f057fc5-22a9-4532-ac15-c76e04e31f08
```

Rules:

- a missing key is a `400 Bad Request` Problem Details response before mutation work begins;
- a malformed or non-v4 key is a `400 Bad Request` Problem Details response before mutation work begins;
- the key is scoped to the authenticated device;
- retrying the same HTTP method, escaped path, raw query string, content type, and request-body bytes returns the retained mutation result, including its status, `Location` if present, and resource body; per-request correlation and compatibility headers are generated for the retry; clients should preserve the request bytes for a retry;
- reusing a key with different request content returns `409 Conflict`;
- a key and its first accepted result are retained for 24 hours after acceptance, including across Gateway/ControlService restarts;
- concurrent identical retries wait up to two seconds for the first HTTP result; if it is still pending, they return retryable `409 idempotency-pending` with `Retry-After: 1` rather than starting duplicate work;
- the key is not an authentication credential;
- Gateway maps it to internal ControlService replay protection;
- the response echoes the accepted key where appropriate.

Pairing submission has no authenticated device yet; its key is scoped to the one-time pairing session and validated QR secret. ControlService stores the session ID, a hash of the validated QR secret, the key, the request-identity hash, and the accepted HTTP result; it never stores the plaintext QR or polling secret in the replay record. A retry must supply the same phone-generated polling secret and request content. Gateway checks the QR session's 10-minute expiry before replay lookup: after expiry, even an identical retry of an accepted submission returns `410 pairing-expired`, not its former `202`. The 24-hour idempotency record does not extend the pairing session or the lifetime of an unused QR. Once a phone has received and stored its device credential, this QR expiry has no effect on that pairing.

### 6.4 Correlation and tracing

Clients may send W3C Trace Context:

```http
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
```

Gateway:

- preserves valid trace context;
- creates correlation when none is supplied;
- returns `DisplayMagician-Request-Id` containing a server-generated canonical GUID on every response, including errors and idempotent replays;
- preserves `operation_id` and `request_id` through ControlService/UserAgent;
- never uses trace IDs as idempotency keys or credentials.

The Problem Details `requestId` equals the response's `DisplayMagician-Request-Id`. A replay gets a new request ID for the new HTTP attempt while retaining the original mutation result and operation ID.

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
- default `limit` is 25 and maximum `limit` is 100 for paged resources;
- all collection routes in the first release use the same `items`/`nextCursor` page shape;
- saved definitions sort by name using ordinal, case-insensitive comparison and then opaque ID using ordinal comparison; devices, pairing requests, operations, and decisions sort by their creation/pairing/start time descending and then ID using ordinal comparison;
- a cursor is bound to its paired user/device, route, filters, and ordering, and retains a stable snapshot for 10 minutes; changing any of those inputs requires a new first-page request;
- timestamps are not the sole continuation mechanism;
- malformed or mismatched cursors return `400 cursor-invalid`; expired snapshot cursors return `410 cursor-expired`;
- a new first-page request starts a new snapshot, so clients should restart pagination after cursor expiry;
- the server may return fewer than `limit` items to stay within the 1 MiB JSON response bound; `nextCursor` is `null` only when that snapshot has no more visible items.

### 6.7 Caching

Display profile, audio profile, and shortcut list/detail routes:

- return ETags;
- honor `If-None-Match` against the representation for the exact route and query, returning `304` with no body when unchanged;
- return at most 256 KiB of decoded PNG artwork per item when `includeArtwork=true`; artwork remains optional if unavailable or cannot fit the response bound;
- default to compact representations;
- use explicit artwork/detail inclusion flags.

Sensitive status, operation, decision, pairing, and device responses use appropriate no-store cache directives.

### 6.8 Rate and connection limits

Document and enforce:

- maximum request body size of 64 KiB;
- maximum JSON response body size of 1 MiB, with artwork bounded to 256 KiB per item;
- a minimum two-second interval between candidate pairing polls for the same request;
- one concurrent SSE stream per device and four per user;
- 120 authenticated requests per device per rolling minute, of which no more than 10 may be mutation requests;
- 20 failed authentications per source IP per rolling minute;
- bounded server retention for idempotency and events;
- SSE replay retention of at most 24 hours, 10,000 events, or 10 MiB per user;
- terminal-operation retention of seven days or 100 records per user, whichever limit is reached first;
- `Retry-After` behavior.

The phone should coalesce duplicate watch/phone refreshes.

Excess requests receive `429 rate-limited` before route work, with `Retry-After` in whole seconds at least as long as the remaining applicable wait. The first authenticated request/poll in a window is allowed; successful authentication does not erase a source IP's failed-authentication count. Gateway also uses bounded work queues so requests cannot accumulate without limit; queue capacity is an implementation setting, not a promise to accept work beyond the published rate limits.

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
| Read current device and grants | Paired-device bearer credential | None beyond authentication |
| Revoke a device | Paired-device bearer credential | `devices-revoke`, including self-revocation |
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
| Deferred dashboard | Paired-device bearer credential | Each included section's read grant if introduced later |
| Events | Paired-device bearer credential | Each emitted resource class's read grant |

A write capability does not imply a read capability. For example, a controller that applies a profile and displays its progress needs `display-profiles-apply` and `status-read`; displaying the profile picker also needs `display-profiles-read`.

Authentication, capability checks, and user/default-target routing must occur before executing work. Neither a resource ID nor an operation ID conveys authority. Authorization is enforced by ControlService even when Gateway has already checked an HTTP request.

The [shared protocol](../IPC/IPC-Specification.md#7-compatibility-negotiation) uses the REST compatibility headers described in section 5.1. Public bootstrap requests carry a hello as well. API version discovery does not replace negotiation, authentication, or pairing.

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

The [design-only target schema](target-openapi.json) records the intended routes, wire types, headers, and limits. It is separate from the [implemented prototype contract](openapi.yaml). The [roadmap's contract completion register](REST-API-ROADMAP.md#6-contract-completion-register) tracks the remaining contract decisions and checks, including mobile client feasibility, restart continuity, and publishing Problem Details type pages. Validate each conditional field rule in the Gateway and client conformance checks as routes are implemented. Update the implemented OpenAPI alongside each route; do not present the design draft as an available API.
