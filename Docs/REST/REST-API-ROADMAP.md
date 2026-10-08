# DisplayMagician v4 REST API implementation roadmap

**Status:** Delivery plan for the target API - not fully implemented

**Documentation reorganized:** 2026-10-08

## 1. Purpose

This document tracks work needed to deliver the [target specification](REST-API-Specification.md). Client walkthroughs are in the [client guide](REST-API-Client-Guide.md); component topology and design rationale are in the [architecture guide](REST-API-Architecture.md).

There is no released REST API to migrate. Update the implementation and [openapi.yaml](openapi.yaml) directly as the target is built. OpenAPI must continue to describe only behavior available in the corresponding build.

## 2. Delivery plan

### P0: Approve and freeze the target contract

1. Review the target design requirements in the [specification](REST-API-Specification.md#2-target-design-requirements).
2. Confirm the target route tree.
3. Confirm bearer credentials without routine expiry.
4. Confirm TLS SPKI/host-identity pinning and renewal behavior.
5. Confirm server-generated operation IDs.
6. Confirm capability renames.
7. Confirm removal of Windows SID/session fields.
8. Finalize target schemas and update [openapi.yaml](openapi.yaml) alongside implemented routes. Keep any design-only schemas explicitly marked as unavailable; do not publish them as the implemented API.
9. Resolve the decisions in the [contract completion register](#6-contract-completion-register) before declaring the target contract frozen. Record each decision in the specification and applicable schemas.
10. Check that pairing, trust setup, credentials, paging, and operation polling can be implemented with ordinary Swift and Kotlin HTTP/JSON clients. Keep SSE optional for phone clients, and avoid schema unions where a simple status field is sufficient.

P0 status (2026-10-08): the project owner approved the target direction and the SSE reconnect rule. The target contract is **not yet frozen**. Remaining P0 gates are precise saved-resource ID constraints; bootstrap idempotency storage/replay details; operation sequence continuity, cancellation eligibility, and failure details; schema/example validation; and a Swift/Kotlin generation and TLS-trust feasibility check. The current [openapi.yaml](openapi.yaml) continues to describe the implemented prototype while the design-only [target-openapi.json](target-openapi.json) is completed and validated.

### P1: Identity, credential, and HTTP foundation

1. Implement automatic TLS certificate renewal with the existing TLS key and a DNS/IP subject alternative name matching the advertised HTTPS URI.
2. Replace the prototype certificate fingerprint with the target Base64 SHA-256 SPKI pin, and replace the prototype serialized-JWK hash with the RFC 7638 `hostId` thumbprint. Normal certificate renewal retains the TLS key; TLS private-key replacement requires local WinForms approval and re-pairing. Host-identity reset invalidates all paired credentials and pairing sessions.
3. Implement one-time issuance and hashed storage of device credentials.
4. Implement bearer authentication and revocation.
5. Implement RFC 9457 Problem Details.
6. Implement idempotency storage and conflict detection.
7. Implement trace/request correlation.
8. Add authentication, capability, cross-user, revocation, and rate-limit tests.
9. Implement the documented request, pairing-poll, failed-authentication, and concurrent-stream limits, including bounded work queues and `Retry-After` where applicable. Test enforcement as well as configured values.
10. Keep bearer credentials and credential hashes out of logs, audit events, support ZIPs, and configuration snapshots. Update the existing machine-owned support snapshot before storing credential hashes in paired-client state.
11. Enforce the approved LAN-only boundary in Gateway listener settings and the installer firewall rule. Define the allowed local interfaces and source-address scope, keep the rule in sync with configured port and network changes, and test that ordinary off-LAN access and router port forwarding do not expose the service. Treat deliberate firewall or network-address-translation overrides as outside the application's enforceable boundary.

### P2: Resource routes

1. Implement capabilities and identity routes.
2. Implement pairing-request resources.
3. Implement device resources, including authenticated `GET /v1/devices/current` grant discovery without the `devices-read` grant.
4. Implement display-profile list/detail/application routes.
5. Implement audio-profile list/detail/application routes.
6. Implement shortcut list/detail/run routes.
7. Implement operation list/detail/cancellation routes.
8. Implement nested decisions and pending-decision inbox.
9. Update the local WinForms QR, pairing-approval, and paired-device management workflows for the target credential and capability model. Verify that local approval and authorized remote approval produce the same user-scoped result.
10. Remove prototype-only routes so the first released API contains only the approved target route tree.

### P3: Events, efficiency, and clients

1. Implement SSE snapshots, event IDs, resume, and reconnect behavior.
2. Implement ETags and compact/artwork representations.
3. Implement cursor pagination and bounded retention.
4. Generate Swift and Kotlin client foundations.
5. Add handwritten pairing, pinning, credential, and idempotency layers. Add SSE support only for phone clients that choose to use events; ordinary HTTPS polling must cover the complete phone workflow.
6. Add phone/watch relay contract tests.
7. Add Telegram connector conformance tests when that product is started.
8. Add Windows Sandbox installed-environment validation.
9. Test the pairing, TLS trust, credential storage, and operation polling flows on the supported phone platforms, including suspension and process restart. Test event recovery only for clients that elect to use SSE.
10. Verify install, service start/restart, upgrade, and uninstall behavior for the Gateway, ControlService, local UI, certificate renewal, configured listener port, and firewall rule. Check that an interrupted update leaves a usable installation and valid identity state.

## 3. Definition of done for each route

A route is not complete until it has:

- a stable typed contract;
- an explicit capability or explicit public designation;
- authenticated user/default-target isolation;
- field, body-size, and collection-size validation;
- idempotency behavior for mutations;
- correct HTTP status and Problem Details mapping;
- request and operation correlation;
- positive, negative, cross-user, revocation, retry, and rate tests;
- OpenAPI schemas and examples;
- readable string enums;
- no SID/session/secret leakage;
- support-log correlation without sensitive data;
- mobile-client handling;
- consistency with the approved target OpenAPI contract.

## 4. Explicitly deferred

- Direct Gateway connectivity from watches
- Direct Internet exposure of Gateway
- Cloud relay hosted by DisplayMagician
- Remote profile/audio/shortcut editing
- Browser clients
- General third-party developer registration
- Marketplace receipt verification by Gateway
- Client TLS certificates
- Custom HMAC authentication
- Remote machine/service administration
- Support bundle and diagnostic-log APIs
- Dashboard aggregation, mDNS discovery, and multi-target selection in the first released API

## 5. Approved target decisions

The project owner approved the target design in the [specification](REST-API-Specification.md) on 2026-10-08. These decisions define the P0 baseline. The wire details in the [contract completion register](#6-contract-completion-register) still need resolution before the contract is frozen.

- [x] Gateway remains LAN-only.
- [x] HTTPS is mandatory.
- [x] Pairing pins TLS SPKI plus stable host identity.
- [x] TLS renewal normally retains the pinned TLS key.
- [x] Devices receive long-lived bearer credentials with no routine expiry.
- [x] ControlService stores only credential hashes.
- [x] Custom per-request ECDSA signatures, timestamps, and nonces are removed from the target REST design.
- [x] Mutations use `Idempotency-Key`.
- [x] Correlation uses trace/request IDs separately from idempotency.
- [x] REST errors use RFC 9457 Problem Details.
- [x] REST enums use readable strings.
- [x] Async commands return `202 Accepted` and `Location`.
- [x] Operation IDs are server-generated.
- [x] Operations are top-level resources.
- [x] Decisions are canonical children of operations, with a top-level pending inbox.
- [x] Windows SIDs and session IDs are not exposed.
- [x] Version 1 normally uses the pairing-bound/default target.
- [x] Watches relay through companion phones only.
- [x] Telegram uses a locally running, outbound-polling, separately paired REST connector.
- [x] Marketplace licensing remains separate from Gateway authentication.
- [x] The API uses `display-profiles-read/apply` capability names.
- [x] Only the approved target route tree is included in the first released REST API.

## 6. Contract completion register

The target design sets the API direction, but the following details must be settled before developers can implement conforming clients. These are open contract items, not silently chosen defaults.

| Area | Required completion | Why clients need it |
|---|---|---|
| Shared compatibility | Header encoding, inclusion on public bootstrap requests, shared `ControlProtocol.TryCreateWelcome` validation, and `400`/`409`/`412` negotiation status mapping are approved in the specification; finalize stable Problem Details error codes | API discovery alone is not shared protocol negotiation |
| Complete schemas | The design-only [target-openapi.json](target-openapi.json) now covers the full first-release route tree; validate cross-field rules, examples, IDs, and client generation before freezing it | Examples are not sufficient for generated or independently implemented clients |
| Capabilities and grants | Public `/v1/capabilities` describes server support; authenticated `/v1/devices/current` reports the caller's current grants without `devices-read`. Finalize their schemas | Clients must not confuse server support with permission |
| TLS trust | Base64 SHA-256 of DER SPKI, RFC 7638 `hostId`, versioned `displaymagician://pair` QR, certificate validity/SAN checks, and re-pairing after key replacement/reset are documented; test Apple/Android trust integration | Developers must not disable TLS verification or infer trust from an IP address |
| Pairing bootstrap | A `displaymagician://pair` QR carries versioned base64url JSON and expires after 10 minutes; the candidate generates a separate 256-bit polling secret and Gateway stores its hash. The design-only schema includes a typed create response | A lost submission response must permit an identical safe retry |
| Credential delivery | The first approved poll atomically commits the new credential hash and consumes delivery; loss after commit requires re-pairing. Expired/consumed polls return `410`. The design-only schema uses one status-bearing response object with conditional fields | One-time delivery cannot rely on stored plaintext or a recoverable response |
| Pairing grants | Partial grants are approved; approving a re-pair for the same user/device ID replaces its old credential and grants. A new-key decision after resolution returns `409`; the original key replays its result. Approval and denial use one simple decision object with conditional grants | The prototype's exact-match and active-ID checks must be changed |
| Mutations | Require canonical lowercase UUID v4 keys, including pairing submission, with 24-hour durable result retention; compare method, escaped path, raw query, content type, and body bytes. Concurrent identical retries wait up to two seconds, then receive retryable `409` with `Retry-After: 1` if still pending | Prevent duplicate work and ambiguous retry behavior |
| Bootstrap idempotency | Scope pairing-submission keys to the validated one-time pairing session and secret; define storage and replay details | Device-scoped idempotency does not describe an unpaired candidate |
| Resource identity | Opaque IDs survive rename and host reset; deletion/recreation gets a new ID. Import collisions assign new IDs to imported copies and rewrite their internal references. Finalize precise ID constraints | Clients cannot safely derive IDs from display names |
| Collections | Default page size 25, maximum 100; JSON responses cap at 1 MiB and artwork at 256 KiB per item. The specification defines snapshot cursors, ordering, and expiry; validate implementation and generated-client behavior | Independently implemented clients need bounded and predictable lists |
| Operations | Terminal retention is seven days or 100 per user, whichever limit is reached first; retained snapshots remain readable with `isStale` while the Agent is unavailable, and unsafe cancellation returns `409`. Define sequence continuity across restart, cancellation eligibility, and failure details | Clients need authoritative recovery and completion rules |
| Decisions | A retry with the original `Idempotency-Key` replays `200`; a new-key answer after another client or expiry default resolved the decision returns `409`. The draft schema includes prompt, expiry/default, and route-authoritative IDs; finalize event and error details | The first answer, a retry of that answer, and another client's answer must be distinguishable |
| SSE | Gateway will offer the event catalogue, typed payloads, grant filtering, per-user/host IDs, heartbeat, bounded replay, and restart continuity. A valid retained cursor replays missed events without a snapshot; a new, unknown, or expired cursor gets a snapshot. The replay-to-live handoff must have no event gap. A phone may implement its complete workflow with polling and skip SSE | Event clients must recover without inventing a private format |
| Revocation | An accepted operation continues after device revocation while new requests fail and streams close; self-revocation is permitted with `devices-revoke`; define stream closure timing | Revoking a credential is not inherently cancelling work |
| Correlation/errors | `DisplayMagician-Request-Id`, stable Problem Details codes/types, and the IPC-to-HTTP mapping are documented and in the design-only schema. Publish the problem type pages and validate every implemented mapping | Support and generated clients require exact names and shapes |
| Limits | Request bodies cap at 64 KiB; JSON responses at 1 MiB; artwork at 256 KiB per item; pairing polls have a two-second minimum interval; authenticated devices get 120 requests and 10 mutations per minute; source IPs get 20 failed authentications per minute. Gateway uses bounded queues and `429` with `Retry-After`; verify actual enforcement | "Bounded" is not a measurable contract |
| Optional features | Dashboard, mDNS, and multi-target selection are deferred from the first release; there is no separate minimum app-version restriction | Optional examples must not become accidental release requirements |
| Licensing separation | Confirm entitlement checks remain client-owned and never substitute for pairing | Avoid unintended coupling between store purchases and API trust |

Resolve policy questions with the project owner before consequential implementation. Record approved answers in the specification and implemented OpenAPI, not only in this table.

## 7. Documentation and release verification

- Keep the specification, client guide, architecture guide, and implemented OpenAPI cross-linked.
- Update shared contracts and Gateway schemas together; do not add a separate client-specific ControlService model.
- Validate OpenAPI syntax, all examples against finalized schemas, and generated Swift/Kotlin client compilation.
- Contract-test route identifiers, HTTP methods/statuses, capabilities, JSON enums, Problem Details, and request correlation.
- Publish the `https://displaymagician.org/problems/{errorCode}` documentation pages used as Problem Details type URIs before release.
- Test accepted-but-not-completed behavior, duplicate mutations, lost responses, decisions answered by another client, SSE continuity loss, and service/Agent restarts.
- Test cross-user isolation, revoked credentials, forged discovery, unexpected TLS key changes, and secret redaction.
- Verify fresh installation, upgrade, uninstall, certificate renewal, LAN listener/firewall scope, and component identity using Windows Sandbox where applicable; use a separate network test setup for off-LAN and port-forward checks.
- Inspect support ZIP contents and logs for bearer credentials, credential hashes, pairing secrets, and private keys.
- Publish concrete limits and retention guarantees before claiming the API is ready for client use.
- Keep incomplete features visibly marked unavailable; do not describe prototype endpoints as implementing the target contract.
