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

### P1: Identity, credential, and HTTP foundation

1. Implement automatic TLS certificate renewal.
2. Implement SPKI and host-identity pairing data.
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
3. Implement device resources.
4. Implement display-profile list/detail/application routes.
5. Implement audio-profile list/detail/application routes.
6. Implement shortcut list/detail/run routes.
7. Implement operation list/detail/cancellation routes.
8. Implement nested decisions and pending-decision inbox.
9. Update the local WinForms QR, pairing-approval, and paired-device management workflows for the target credential and capability model. Verify that local approval and authorized remote approval produce the same user-scoped result.
10. Implement optional dashboard.
11. Remove prototype-only routes so the first released API contains only the approved target route tree.

### P3: Events, efficiency, and clients

1. Implement SSE snapshots, event IDs, resume, and reconnect behavior.
2. Implement ETags and compact/artwork representations.
3. Implement cursor pagination and bounded retention.
4. Add optional trusted mDNS discovery.
5. Generate Swift and Kotlin client foundations.
6. Add handwritten pairing, pinning, credential, idempotency, and SSE layers.
7. Add phone/watch relay contract tests.
8. Add Telegram connector conformance tests when that product is started.
9. Add Windows Sandbox installed-environment validation.
10. Test the pairing, TLS trust, credential storage, operation polling, and event recovery flows on the supported phone platforms, including suspension and process restart.
11. Verify install, service start/restart, upgrade, and uninstall behavior for the Gateway, ControlService, local UI, certificate renewal, configured listener port, and firewall rule. Check that an interrupted update leaves a usable installation and valid identity state.

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

## 5. Review checklist

Please confirm or change each item before implementation:

- [ ] Gateway remains LAN-only.
- [ ] HTTPS is mandatory.
- [ ] Pairing pins TLS SPKI plus stable host identity.
- [ ] TLS renewal normally retains the pinned TLS key.
- [ ] Devices receive long-lived bearer credentials with no routine expiry.
- [ ] ControlService stores only credential hashes.
- [ ] Custom per-request ECDSA signatures, timestamps, and nonces are removed from the target REST design.
- [ ] Mutations use `Idempotency-Key`.
- [ ] Correlation uses trace/request IDs separately from idempotency.
- [ ] REST errors use RFC 9457 Problem Details.
- [ ] REST enums use readable strings.
- [ ] Async commands return `202 Accepted` and `Location`.
- [ ] Operation IDs are server-generated.
- [ ] Operations are top-level resources.
- [ ] Decisions are canonical children of operations, with a top-level pending inbox.
- [ ] Windows SIDs and session IDs are not exposed.
- [ ] Version 1 normally uses the pairing-bound/default target.
- [ ] Watches relay through companion phones only.
- [ ] Telegram uses a locally running, outbound-polling, separately paired REST connector.
- [ ] Marketplace licensing remains separate from Gateway authentication.
- [ ] The API uses `display-profiles-read/apply` capability names.
- [ ] Only the approved target route tree is included in the first released REST API.

## 6. Contract completion register

The target design sets the API direction, but the following details must be settled before developers can implement conforming clients. These are open contract items, not silently chosen defaults.

| Area | Required completion | Why clients need it |
|---|---|---|
| Shared compatibility | Define REST carriage of `ProtocolHello`/`ProtocolWelcome`, bootstrap exceptions if any, required-capability validation, and failure responses | API discovery alone is not shared protocol negotiation |
| Complete schemas | Define every request/response property, required/optional/null rules, string enum, ID format, validation constraint, and error extension | Examples are not sufficient for generated or independently implemented clients |
| Capabilities and grants | Distinguish public known/supported capabilities from authoritative current device grants; specify version and feature discovery | Clients must not confuse server support with permission |
| TLS trust | Specify SPKI hash/encoding, QR representation, certificate validity/hostname validation for the pinned self-signed host, and platform trust integration | Developers must not disable TLS verification or infer trust from an IP address |
| Pairing bootstrap | Define QR fields, temporary authorization issuance, create-response schema, and expiry/polling rules | The example currently references a temporary credential without specifying its delivery |
| Credential delivery | Define atomic one-time issuance and behavior when approval/credential delivery or its acknowledgement is lost | Hashed storage and one-time delivery require a deliberate recovery policy |
| Pairing grants | Confirm partial grant versus exact-match approval, denial, and repeated decision semantics | The target permits grants no greater than requested; prototype constraints must be reconciled |
| Mutations | Specify whether `Idempotency-Key` is mandatory, format/length, retention, canonical request identity including query, pending retries, errors, concurrency, and restart durability | Prevent duplicate work and ambiguous retry behavior |
| Bootstrap idempotency | Define mutation scoping before a device has authenticated, including pairing submission | Device-scoped idempotency does not describe an unpaired candidate |
| Resource identity | Define ID formats and stability across rename, deletion/recreation, import, and host reset | Clients cannot safely derive IDs from display names |
| Collections | Define compact/detail/artwork schemas, byte/item limits, cursor ordering, cursor expiry, and filtering | Independently implemented clients need bounded and predictable lists |
| Operations | Define retention, missing/expired lookup behavior, sequence continuity across restart, cancellation eligibility, failure details, and target-unavailable reads | Clients need authoritative recovery and completion rules |
| Decisions | Define prompt schema, expiration/default fields, authoritative identifiers, and identical response versus conflicting/already-resolved response semantics | The first answer, a retry of that answer, and another client's answer must be distinguishable |
| SSE | Define event catalogue, initial snapshot, payload schemas, ID scope, replay retention, continuity reset, capability filtering, and heartbeat/reconnect behavior | Clients must recover without inventing a private event format |
| Revocation | Define ongoing-operation policy, open-stream closure timing, retained-result access, and any self-revocation exception | Revoking a credential is not inherently cancelling work |
| Correlation/errors | Specify request-ID header name, trace handling, stable error codes/types, and full IPC-to-HTTP mapping | Support and generated clients require exact names and shapes |
| Limits | Publish request/response sizes, list/page bounds, pairing poll rate, SSE stream limits, and `Retry-After` behavior | "Bounded" is not a measurable contract |
| Optional features | Explicitly accept or defer dashboard, mDNS, multi-target selection, and client minimum-version restrictions | Optional examples must not become accidental release requirements |
| Licensing separation | Confirm entitlement checks remain client-owned and never substitute for pairing | Avoid unintended coupling between store purchases and API trust |

Resolve policy questions with the project owner before consequential implementation. Record approved answers in the specification and implemented OpenAPI, not only in this table.

## 7. Documentation and release verification

- Keep the specification, client guide, architecture guide, and implemented OpenAPI cross-linked.
- Update shared contracts and Gateway schemas together; do not add a separate client-specific ControlService model.
- Validate OpenAPI syntax, all examples against finalized schemas, and generated Swift/Kotlin client compilation.
- Contract-test route identifiers, HTTP methods/statuses, capabilities, JSON enums, Problem Details, and request correlation.
- Test accepted-but-not-completed behavior, duplicate mutations, lost responses, decisions answered by another client, SSE continuity loss, and service/Agent restarts.
- Test cross-user isolation, revoked credentials, forged discovery, unexpected TLS key changes, and secret redaction.
- Verify fresh installation, upgrade, uninstall, certificate renewal, LAN listener/firewall scope, and component identity using Windows Sandbox where applicable; use a separate network test setup for off-LAN and port-forward checks.
- Inspect support ZIP contents and logs for bearer credentials, credential hashes, pairing secrets, and private keys.
- Publish concrete limits and retention guarantees before claiming the API is ready for client use.
- Keep incomplete features visibly marked unavailable; do not describe prototype endpoints as implementing the target contract.
