# DMv4 REST API product specification and roadmap

**Status:** Target product contract; planned routes are not yet implemented  
**Current implementation:** [openapi.yaml](openapi.yaml)  
**Target clients:** Android and Apple phone/tablet applications  
**Watch model:** Companion-phone relay only

## 1. Product objective

Provide one stable, capability-controlled LAN API for first-party DisplayMagician phone applications. The same API should remain suitable for a future public developer program without exposing machine administration or local editing workflows.

The phone applications are full Gateway clients. Apple Watch and Samsung/Wear OS applications are presentation and input extensions of their companion phone applications:

```text
Apple Watch ---- WatchConnectivity ---- Apple phone app --\
                                                          +--> HTTPS Gateway --> ControlService
Wear OS watch -- Wear Data Layer -------- Android app ----/
```

The companion phone performs Gateway requests using its own paired identity. Watch apps:

- do not connect directly to Gateway;
- do not discover the DMv4 host;
- do not pin the Gateway certificate;
- do not hold a Gateway device private key;
- do not independently pair or receive remote capabilities;
- receive only the minimum state required for their UI.

Direct-watch Gateway connectivity is deferred and is not a version 1 requirement.

## 2. Design principles

1. **One remote contract.** Android and Apple consume the same routes and shared transport models.
2. **Least privilege.** Each protected operation has an explicit remote capability.
3. **User and session isolation.** Every request is scoped to the paired Windows user and a verified eligible session.
4. **Asynchronous operations.** Command acceptance is distinct from completion.
5. **Safe retries.** Mutations have request identity and idempotency semantics.
6. **Efficient LAN use.** Lists are compact, cacheable, bounded, and optionally include artwork.
7. **Recoverable events.** Clients can reconnect and obtain an authoritative snapshot.
8. **Normal HTTP behavior.** Status codes and error bodies describe failures consistently.
9. **No trust fallback.** A failed remote request never falls back to same-user local-pipe authority.
10. **Marketplace-neutral server.** Store purchase validation remains in each client application and is not Gateway authentication.

### OpenAPI method naming

OpenAPI `operationId` values use concise `{verb}{Resource}` names because every route in this document already belongs to the remote REST API. Do not add redundant `Remote`, `Gateway`, or `Device` words when the route and resource make the context clear.

Examples:

```text
getIdentity
requestPairing
getStatus
streamStatus
listDisplayProfiles
applyDisplayProfile
listAudioProfiles
applyAudioProfile
listShortcuts
runShortcut
answerDecision
```

Planned routes should follow the same convention, for example `getCapabilities`, `listOperations`, `getOperation`, `cancelOperation`, `listDecisions`, `getDecision`, `listDevices`, and `revokeDevice`.

Runtime `operationId` fields remain execution GUIDs in the shared contracts. OpenAPI `operationId` values are generated client method names; documentation and generated SDK guidance must call them API method names to avoid conflating the two concepts.

## 3. Required feature set

### 3.1 Identity and API discovery

#### Existing

```text
GET /v1/identity
```

Returns the public host identity and TLS certificate fingerprint used during pairing.

#### Required

```text
GET /v1/capabilities
```

Return only public-safe compatibility metadata:

- REST API versions
- DMv4 protocol version range
- Gateway product/version
- stable host ID
- supported controller features
- known remote capability identifiers
- maximum request size
- supported event transports
- minimum supported client API version, if one is introduced

This endpoint is compatibility discovery, not authentication or licensing. It must not expose user, session, profile, operation, or paired-device data.

### 3.2 Pairing

#### Existing

```text
POST /v1/pairing/request
POST /v1/pairing/status
```

#### Required for a complete multi-phone product

```text
GET  /v1/pairing/requests
POST /v1/pairing/requests/{pairingSessionId}/approve
POST /v1/pairing/requests/{pairingSessionId}/reject
```

Rules:

- Listing, approval, and rejection require `pairing-approve`.
- A candidate device cannot approve itself.
- Requests are visible only to devices paired to the same DMv4 user.
- The candidate's requested capabilities cannot be expanded during approval.
- Granted capabilities must be an allowed subset of the candidate request.
- Expired, rejected, and already-resolved requests return stable errors.
- Pairing secrets and private keys never appear in list responses or logs.

Local WinForms approval remains supported.

### 3.3 Paired-device management

Required:

```text
GET    /v1/devices
DELETE /v1/devices/{deviceId}
```

Add remote capabilities:

```text
devices-read
devices-revoke
```

Rules:

- Results are user-scoped.
- Public-key material and pairing secrets are excluded.
- A device may be allowed to revoke itself.
- Revoking another device requires `devices-revoke`.
- Revocation takes effect before the response reports success.
- The product must define whether the final device holding `devices-revoke` may revoke itself; default recommendation is yes because local WinForms can recover access.

Renaming a paired device is useful but not required for the first release.

### 3.4 Status and events

#### Existing

```text
GET /v1/status
GET /v1/status/stream
```

#### Required behavior

- `status-read` grants operation status only.
- Pending decisions require `decisions-read`.
- Status responses never reveal another Windows user's data.
- `targetSessionId` is optional and must always be revalidated.
- A client can obtain a complete authoritative snapshot after reconnect.
- Responses include a server change token or timestamp suitable for incremental refresh.

SSE requirements:

- Send an authoritative snapshot immediately after connection.
- Send typed status and decision updates after the snapshot.
- Include SSE `id` values or another resume token.
- Honor `Last-Event-ID` when retained history permits.
- If history is unavailable, send a fresh snapshot rather than pretending continuity.
- Send keep-alive comments.
- Close cleanly when authentication or capability grants become invalid.
- Document client reconnect backoff.

Watch UIs receive the required subset through the phone. The watch must not be expected to maintain an SSE connection.

### 3.5 Display profiles

#### Existing

```text
GET  /v1/display-profiles
POST /v1/display-profiles/apply
```

Required improvements:

- Preserve stable profile IDs.
- Return compact saved-profile views by default.
- Support `includeArtwork=false|true`.
- Support an optional detailed profile ID only when a phone needs a preview.
- Bound artwork and total response sizes.
- Return an ETag and honor `If-None-Match`.
- Applying requires `profiles-apply`.
- Listing requires `profiles-read`.
- Apply requests contain a non-empty operation ID.

Remote profile editing is out of scope.

### 3.6 Audio profiles

#### Existing

```text
GET  /v1/audio-profiles
POST /v1/audio-profiles/apply
```

Required improvements:

- Preserve stable profile IDs.
- Keep list responses compact and cacheable.
- Return unavailable-device information without exposing sensitive hardware data unnecessarily.
- Return an ETag and honor `If-None-Match`.
- Applying requires `audio-profiles-apply`.
- Listing requires `audio-profiles-read`.
- Apply requests contain a non-empty operation ID.

Remote audio-profile editing is out of scope.

### 3.7 Shortcuts

#### Existing

```text
GET  /v1/shortcuts
POST /v1/shortcuts/run
```

Required improvements:

- Preserve stable shortcut IDs.
- Support `includeArtwork=false|true`.
- Bound icon and response sizes.
- Return an ETag and honor `If-None-Match`.
- Running requires `shortcuts-run`.
- Listing requires `shortcuts-read`.
- Run requests contain a non-empty operation ID.
- Never accept executable paths or arbitrary process arguments from a remote client; UserAgent executes the saved shortcut definition.

Remote shortcut editing is out of scope.

### 3.8 Operations

Required:

```text
GET  /v1/operations
GET  /v1/operations/{operationId}
POST /v1/operations/{operationId}/cancel
```

Add:

```text
operations-cancel
```

Rules:

- Reading requires `status-read`.
- Cancellation requires `operations-cancel`.
- Only an operation visible to the paired user and verified target session can be cancelled.
- Terminal operations cannot be cancelled.
- Cancellation acceptance is not completion; clients continue observing status.
- Define retention and maximum result count for recent terminal operations.
- Support a change token or `changedSinceUtc`.

The existing aggregate status endpoint may remain a convenient dashboard snapshot, but dedicated operation routes provide stable resource semantics.

### 3.9 Operation decisions

Required:

```text
GET  /v1/decisions
GET  /v1/decisions/{promptId}
POST /v1/decisions/{promptId}/answer
```

Rules:

- Reading requires `decisions-read`.
- Answering requires `decisions-answer`.
- The answer body contains only the selected shared enum choice.
- The prompt ID in the route is authoritative.
- The requested choice must be allowed by the prompt.
- Expired, resolved, or unavailable prompts return a stable conflict/not-found response.
- A response must not disclose prompts belonging to another user or session.

Remove pending decisions from status responses for devices without `decisions-read`. The preferred long-term shape is to keep decision resources separate and let a dashboard endpoint include them only when authorized.

## 4. HTTP contract improvements

### 4.1 Standard error body

Define a shared REST error response:

```json
{
  "errorCode": 16,
  "error": "validation-failed",
  "message": "A valid profile ID is required.",
  "requestId": "8c632c3c-3053-49e5-b6e0-5db238e3b8eb",
  "operationId": null,
  "retryable": false
}
```

Do not return a success-shaped `ControlResponse` with HTTP 200 for a failed REST operation.

Minimum mapping:

| Condition | HTTP status |
|---|---:|
| Invalid JSON/input | 400 |
| Missing/invalid/expired signature | 401 |
| Missing capability or wrong user/session | 403 |
| Resource not found | 404 |
| Expired/resolved decision, busy lease, or recovery conflict | 409 |
| Body too large | 413 |
| Rate limited | 429 |
| Agent/session temporarily unavailable | 503 |
| Unexpected execution failure | 500 |

### 4.2 Correlation and idempotency

Protected requests should accept:

```text
X-DisplayMagician-Request-Id: <GUID>
```

Mutating requests must use this ID as the routed ControlService request ID. Retrying the identical request reuses the ID and returns the retained result. Reusing it with a different method, path, target session, or body is an error.

The response should return the request ID in a header and error body. Existing operation IDs remain separate from request IDs.

### 4.3 Signed request canonicalization

Continue signing:

```text
UPPERCASE_METHOD
PATH_WITHOUT_QUERY
UPPERCASE_SHA256_BODY_HEX
UTC_TIMESTAMP_IN_O_FORMAT
NONCE
```

Before adding signed query parameters beyond the current target-session and change filters, decide whether the canonical signature must include a normalized query string. The current signature covers only `Request.Path`; this limitation must be explicit. Security-relevant selectors should not be added as unsigned query parameters without review.

### 4.4 Request limits and rate behavior

Define:

- maximum body size;
- maximum list result size;
- maximum concurrent SSE connections per device/user;
- pairing polling interval;
- nonce and timestamp limits;
- bounded retry guidance;
- `Retry-After` behavior for 429 and 503.

Rate limiting must not turn normal watch-through-phone interactions into failures. The phone should coalesce duplicate watch and phone refreshes.

## 5. LAN discovery and trust

QR pairing remains the trust bootstrap. For reconnect after DHCP/address changes, add optional LAN discovery:

```text
_displaymagician._tcp.local
```

Recommended public-safe discovery metadata:

- stable host ID
- API major version
- HTTPS port
- product name

Discovery data is untrusted until matched to a previously paired host identity and pinned TLS fingerprint. Discovery must not advertise user names, profile names, device lists, pairing secrets, or capabilities granted to a device.

Phone platforms require their normal local-network permissions. Failure to discover must not erase a saved, previously trusted host.

## 6. Mobile client requirements

### Phone key storage

- Generate the P-256 private key on the phone.
- Store it in Android Keystore or Apple Keychain/Secure Enclave where available.
- Never export it to a watch, logs, backups that do not preserve platform protection, or support bundles.

### Watch relay

- Use WatchConnectivity for Apple Watch.
- Use the Wear OS Data Layer for Samsung/Wear OS.
- Define a narrow phone/watch message contract for controller commands and compact status.
- Validate watch messages in the phone app before calling Gateway.
- Coalesce repeated status refreshes.
- Return only the status required by the watch UI.
- Do not treat the watch as an independently paired Gateway device.

The phone/watch message contract is an application-internal protocol and is not part of the DMv4 Gateway REST API.

### Marketplace entitlement

Each client application owns marketplace purchase validation. Gateway pairing and request authentication must not accept store receipts as credentials and must not infer official-client status from `clientType`, display name, or user agent.

## 7. Remote capability target

Existing:

```text
status-read
decisions-read
decisions-answer
profiles-read
profiles-apply
audio-profiles-read
audio-profiles-apply
shortcuts-read
shortcuts-run
pairing-approve
```

Add:

```text
operations-cancel
devices-read
devices-revoke
```

Recommended full phone-controller grant:

```text
status-read
decisions-read
decisions-answer
profiles-read
profiles-apply
audio-profiles-read
audio-profiles-apply
shortcuts-read
shortcuts-run
operations-cancel
pairing-approve
devices-read
devices-revoke
```

Users should be able to grant a smaller subset. A client must disable unavailable UI rather than repeatedly invoking unauthorized routes.

## 8. Delivery plan

### P0: Contract and security correctness

1. Add `/v1/capabilities`.
2. Define the REST error model and HTTP mapping.
3. Add request correlation/idempotency.
4. Separate `decisions-read` from `status-read`.
5. Add `operations-cancel`.
6. Add authorization tests for every route and capability.
7. Decide and document query-string signature coverage before expanding signed queries.

### P1: Complete controller workflows

1. Add operation list/get/cancel routes.
2. Add decision list/get/answer routes.
3. Add pairing request list/approve/reject routes.
4. Add device list/revoke routes.
5. Make SSE snapshot/resume/reconnect behavior explicit.
6. Add compact/artwork list controls and ETags.
7. Add optional trusted mDNS discovery.

### P2: Client and release experience

1. Generate Swift and Kotlin client foundations from OpenAPI.
2. Add shared handwritten signing, pinning, pairing, retry, and SSE layers.
3. Add Gateway conformance tests that execute the shipping OpenAPI examples.
4. Add Android and Apple end-to-end pairing tests.
5. Add phone/watch relay contract tests.
6. Add installed-environment tests using the Windows Sandbox workflow.

## 9. Definition of done for each route

A route is not complete until it has:

- a shared transport contract where applicable;
- an explicit remote capability;
- authenticated user and target-session validation;
- request size and field validation;
- idempotency behavior for mutations;
- correct HTTP statuses and REST error bodies;
- request/operation correlation;
- positive, negative, cross-user, cross-session, and replay tests;
- OpenAPI schemas and examples;
- mobile-client handling;
- logging that excludes secrets and preserves correlation;
- backward-compatibility consideration.

## 10. Features explicitly deferred

- Direct Gateway connectivity from watches
- Remote profile/audio/shortcut editing
- Cloud relay or operation outside the local network
- Push notifications through Apple/Google cloud services
- Browser clients
- General third-party developer registration
- Marketplace receipt verification by Gateway
- Remote machine/service administration

These may be revisited later without expanding the version 1 controller API prematurely.
