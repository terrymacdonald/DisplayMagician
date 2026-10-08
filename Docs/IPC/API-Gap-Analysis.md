# DMv4 API gap analysis

**Baseline:** implementation as verified on 2026-10-07  
**Chosen REST product boundary:** remote status/events, list/apply/run, decisions, and pairing  
**Remote client model:** Android and Apple phones pair directly; watch apps relay through their companion phones  
**Not a REST parity goal:** editing, diagnostics, support bundles, machine settings, metrics, messages, and service administration

## 1. Executive summary

DMv4 has a substantial versioned local protocol and a deliberately narrower remote Gateway. The most important work is not adding every local command to REST. It is tightening the public contract around the intended workflows, making it independently consumable, and closing a small set of remote-controller and protocol-consistency gaps.

The highest-priority confirmed gaps are:

1. There is no published integration SDK/package or generated client independent of the WinForms/Console implementations.
2. REST does not expose protocol/capability negotiation despite the transport-neutral compatibility contract.
3. An already-paired authorised phone cannot approve or reject a new phone pairing request through REST, although the capability and coordinator behavior exist.
4. Remote-started operations cannot be cancelled remotely.
5. `status-read` currently returns pending decisions, so the defined `decisions-read` scope is not independently enforced.
6. REST failures are generally returned as HTTP 200 with a failed `ControlResponse`, which weakens normal HTTP semantics.
7. Pairing and Gateway-setting mutations are not covered by the local 24-hour request replay store.
8. There is no dedicated automated contract test that validates these Markdown, JSON Schema, AsyncAPI, and OpenAPI documents against C# contracts/routes.
9. Paired phones cannot list or revoke their user's other paired devices through REST.

## 2. Confirmed gaps

### G-01: No independently consumable local IPC SDK

**Priority:** High  
**Status:** Confirmed

The reusable contracts and a [local IPC client guide](Local-IPC-Client-Guide.md) exist, while connection, validation, retries, event reconnection, and response handling are implemented separately in WinForms and Console clients. A third-party developer must still implement the documented framing and lifecycle behavior.

**Recommendation:** create a supported `DisplayMagician.Client` library after the protocol is stable. It should reference `DisplayMagician.Contracts`, expose typed async methods, centralize request/response validation and event reconnection, and identify itself as `LocalIntegration`. Do not expose internal pipes.

The initial library should expose the reduced local-controller profile from the client guide rather than every public application-support and administrative command. This is a supported-surface decision under the existing same-user trust model, not capability-based local authorization.

### G-02: REST compatibility/capability discovery is absent

**Priority:** High  
**Status:** Confirmed

`ProtocolHello`/`ProtocolWelcome` are described as transport-neutral, but Gateway routes do not currently negotiate them or provide an equivalent capabilities endpoint.

**Recommendation:** add an authenticated and/or public-safe `/v1/capabilities` route returning protocol version, API version, supported remote operations, and applicable scopes. Decide whether REST requests carry `ProtocolHello` or whether HTTP content negotiation plus the capabilities document is the REST-specific equivalent. Do not confuse this with authentication.

### G-03: Remote pairing approval workflow is incomplete

**Priority:** High  
**Status:** Confirmed

`pairing-approve` exists, and the pairing coordinator can approve on behalf of an already-paired authorised device. No REST routes list pending requests or approve/reject them.

**Recommendation:** add scoped routes such as:

```text
GET  /v1/pairing/requests
POST /v1/pairing/requests/{pairingSessionId}/approve
POST /v1/pairing/requests/{pairingSessionId}/reject
```

Approval MUST remain user-scoped, MUST NOT allow a candidate to approve itself, and MUST validate granted capabilities against the candidate request.

### G-04: Remote operation cancellation is absent

**Priority:** High  
**Status:** Confirmed

Remote devices can start profile/audio/shortcut operations and observe them, but cannot cancel eligible operations.

**Recommendation:** define an `operations-cancel` remote scope and add `POST /v1/operations/{operationId}/cancel`. ControlService must verify ownership, target session, operation state, and capability.

### G-04A: Remote paired-device management is absent

**Priority:** High  
**Status:** Confirmed

The local API can list and revoke paired devices, but an authorised phone cannot manage the user's paired-device set remotely.

**Recommendation:** add user-scoped `GET /v1/devices` and `DELETE /v1/devices/{deviceId}` routes with separate `devices-read` and `devices-revoke` capabilities. Exclude pairing secrets and public-key material from results.

### G-05: Decision read authority is coupled to status authority

**Priority:** High  
**Status:** Confirmed

`RemoteUserStatus` contains both operations and pending decisions. The status routes require `status-read`; the defined `decisions-read` scope is not checked independently.

**Recommendation:** either:

- require both `status-read` and `decisions-read` before including decisions, omitting decisions otherwise; or
- split decisions into `/v1/decisions` and remove them from general status.

The second option provides the clearest least-privilege boundary.

### G-06: REST responses do not consistently use HTTP error semantics

**Priority:** Medium  
**Status:** Confirmed

Most protected action/list routes return `ControlResponse` directly. A failed ControlService operation can therefore be serialized with HTTP 200. Pairing request and authentication failures use more appropriate status codes, but behavior is inconsistent.

**Recommendation:** define a stable REST error body and map at least:

- validation → 400
- authentication → 401
- scope/authority → 403
- missing resource → 404
- lease/conflict/recovery → 409
- unavailable Agent/session → 503
- unexpected execution failure → 500

Preserve the shared `ControlErrorCode` in the body.

### G-07: Pairing/Gateway mutations are outside local replay protection

**Priority:** Medium  
**Status:** Confirmed

The replay store covers many mutating public client operations but not Gateway setting updates, QR creation, pairing approval/rejection, paired-client revocation, or temporary diagnostic-level changes.

**Recommendation:** classify each mutation for safe replay and add it where duplicate execution could produce a second side effect or ambiguous result. QR creation may intentionally require a fresh secret; if so, explicitly reject reused request IDs rather than silently creating another session.

### G-08: Event subscription negotiation failures are silent disconnects

**Priority:** Medium  
**Status:** Confirmed

The event pipe closes when the message type, outer protocol version, or `Hello` is invalid instead of returning a structured failed `ControlResponse`.

**Recommendation:** return a correlated failure acknowledgement when a complete readable request was received. Abrupt close should be reserved for framing, identity, or transport failures where replying is unsafe.

### G-09: No paging or explicit list retention contract

**Priority:** Medium  
**Status:** Confirmed

The protocol has a 5 MiB limit, but most list methods do not expose paging. Operation history is bounded internally, while the public retention/count guarantees are not specified.

**Recommendation:** establish maximum list sizes and retention contracts. Add cursor paging before any list can realistically approach the frame limit. Keep ordinary profile lists compact.

### G-10: Nested JSON-string payload reduces tooling and validation

**Priority:** Medium  
**Status:** Confirmed protocol limitation

`ControlEnvelope.Payload` is JSON text inside a JSON string. This is valid and must remain stable for version 1, but generic schema validators and code generators cannot select a payload schema without parsing twice.

**Recommendation:** retain version 1 behavior. For a future breaking version, consider `JsonElement`/object payloads or a discriminated envelope. Do not change version 1 in place.

### G-11: Error-version terminology overlaps

**Priority:** Low  
**Status:** Confirmed

`UnsupportedProtocolVersion` reports an outer-envelope mismatch while `IncompatibleProtocolVersion` reports non-overlapping Hello ranges. The distinction is real but not obvious from the names alone.

**Recommendation:** preserve numeric values and document the distinction. A future major version can rationalize naming without reusing values.

### G-12: Machine-readable specifications are not generated or contract-tested

**Priority:** Medium  
**Status:** Confirmed

The checked-in JSON Schema, AsyncAPI, and OpenAPI files can drift from C#.

**Recommendation:** add a documentation verification test/tool that:

- enumerates `ControlMessageType`, errors, capabilities, and event types;
- verifies route method/path inventory;
- validates JSON/YAML syntax;
- checks example envelopes deserialize;
- checks documented constants against `ControlProtocol`;
- fails CI on drift.

## 3. Local API completeness observations

These are not REST gaps.

### Dedicated shortcut CRUD

Local shortcut editing uses repository snapshot/commit rather than dedicated create/get/update/delete requests. This preserves optimistic concurrency but exposes a broad document-oriented primitive.

**Decision needed later:** keep repository commits as an advanced local API or add typed shortcut CRUD with stable validation/conflict semantics.

### Get-one resources

Profiles use a list request with optional detail selection. Audio profiles and shortcuts primarily use lists. Typed get-one operations would simplify clients and reduce large responses but are not required for the current UI.

### Explicit display-control operations

The original plan listed acquire/release commands. Acquisition exists as an internal Agent/service concern; no public release command is dispatched. This appears consistent with ControlService owning serialization rather than exposing leases as a normal third-party primitive. Keep it internal unless a concrete integration workflow requires manual lease control.

## 4. Intentional REST exclusions

The following should be marked **IPC-only by design**, not missing:

- Creating, renaming, updating, and deleting display/audio profiles
- Editing shortcut definitions
- Game/application discovery used by editors
- Repository snapshot/commit
- Messages and client-update synchronization
- Anonymous-metrics settings/reporting
- Support-bundle creation
- Diagnostic log-level controls
- Gateway listener settings
- UserAgent restart/stop controls
- Forced display-control release and recovery administration
- Machine-wide service status that would reveal other users/sessions

If product scope changes, these require a fresh threat model and explicit remote capabilities; they must not be exposed merely for parity.

## 5. Recommended delivery order

### Phase 1: Stabilize and verify documentation

1. Keep this specification aligned with protocol version 1.
2. Add syntax/schema validation in CI.
3. Add example-deserialization tests.
4. Decide which local operations receive long-term third-party support.

### Phase 2: Complete the remote-controller contract

1. Add REST capabilities/version discovery.
2. Separate decision-read authority from status-read.
3. Add remote pairing approval/rejection for authorised existing devices.
4. Add remote operation cancellation.
5. Normalize REST status codes and error bodies.
6. Add Gateway endpoint/authentication/authorization tests.

### Phase 3: Developer experience

1. Publish a typed local IPC client library.
2. Generate reference pages from shared contracts without making generated output the source of truth.
3. Provide a small integration sample and conformance test.
4. Generate remote clients from OpenAPI once REST error and negotiation semantics stabilize.

## 6. Definition of API readiness

An operation is considered ready for supported third-party use only when all applicable items are true:

- Stable shared request/response types
- Stable message or route identifier
- Compatibility/capability discovery
- Authentication and authorization rules
- Input limits and validation
- Correct identity/session scoping
- Idempotency/retry behavior
- Operation progress/completion behavior
- Defined error mapping
- Event or polling recovery behavior
- Automated positive and negative tests
- Human-readable documentation
- Machine-readable schema where practical
- At least one valid wire example
