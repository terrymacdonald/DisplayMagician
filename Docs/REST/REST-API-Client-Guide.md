# DisplayMagician v4 REST API client guide

**Audience:** Phone/tablet developers and explicitly paired integration developers

**Status:** Target client workflows - not fully implemented

**Transport:** HTTPS and Server-Sent Events (SSE) on local and routed private networks

## 1. Start here

Read the [specification](REST-API-Specification.md) for the target contract and the [architecture guide](REST-API-Architecture.md) for the trust model. [openapi.yaml](openapi.yaml) describes the current prototype, not these target workflows.

The examples below illustrate target interactions. Exact schemas and several lifecycle details still need to be finalized in the [roadmap](REST-API-ROADMAP.md). Do not generate a target production client from the prototype OpenAPI or assume an example is a complete schema.

Every request also sends `DisplayMagician-Protocol-Hello` containing unpadded base64url encoded UTF-8 JSON for the shared `ProtocolHello` contract. Every successful response supplies `DisplayMagician-Protocol-Welcome` in the same encoding for `ProtocolWelcome`. The examples omit these repeated headers for readability; they are required even on public pairing and identity requests.

## 2. Client interaction walkthroughs

These walkthroughs apply the specification's lifecycle and recovery rules. They do not define a separate API contract.

### 2.1 First launch and pairing

The local WinForms application creates a pairing QR valid for 10 minutes. It encodes `displaymagician://pair?payload=<unpad-base64url-utf8-json>` with:

- Gateway HTTPS URI;
- stable host ID;
- stable host public identity;
- expected TLS SPKI pin;
- pairing request/session ID;
- one-time pairing secret;
- expiration time.

Treat the whole app link as a one-time secret. Do not open it in a browser, log it, or put it in support data.

The phone then:

1. scans the QR code;
2. connects only to the advertised HTTPS Gateway;
3. validates that the TLS connection presents the expected SPKI;
4. calls `GET /v1/identity`;
5. verifies the returned host ID and public identity against the QR payload;
6. submits its pairing request;
7. polls the pairing-request resource using a separate 256-bit secret it generated before submission;
8. receives its bearer credential once after approval;
9. stores the credential in platform-secure storage;
10. discards the QR secret and temporary pairing authorization;
11. retrieves public server capabilities, its current device grants, and any relevant operation/decision resources. An SSE client can also request an initial event snapshot.

Submit pairing:

```http
POST /v1/pairing-requests HTTP/1.1
Content-Type: application/json
Idempotency-Key: 8a0bc7e5-37d4-4f78-aa42-80319a3e3c72

{
  "pairingRequestId": "205242b3-cb35-41e0-a109-7fab351fe17e",
  "pairingSecret": "<one-time-secret>",
  "pollingSecret": "<phone-generated-256-bit-secret>",
  "deviceId": "terrys-iphone",
  "displayName": "Terry's iPhone",
  "clientType": "displaymagician-ios",
  "requestedCapabilities": [
    "status-read",
    "decisions-read",
    "decisions-answer",
    "display-profiles-read",
    "display-profiles-apply",
    "audio-profiles-read",
    "audio-profiles-apply",
    "shortcuts-read",
    "shortcuts-run",
    "operations-cancel"
  ]
}
```

Accepted response:

```http
HTTP/1.1 202 Accepted
Location: /v1/pairing-requests/205242b3-cb35-41e0-a109-7fab351fe17e
Cache-Control: no-store
```

```json
{
  "pairingRequestId": "205242b3-cb35-41e0-a109-7fab351fe17e",
  "status": "awaiting-approval",
  "href": "/v1/pairing-requests/205242b3-cb35-41e0-a109-7fab351fe17e",
  "expiresAt": "2026-10-08T10:10:00Z"
}
```

Poll while awaiting approval, no faster than once every two seconds:

```http
GET /v1/pairing-requests/205242b3-cb35-41e0-a109-7fab351fe17e HTTP/1.1
Authorization: DisplayMagician-Pairing <phone-generated-polling-secret>
```

Approved response:

```json
{
  "pairingRequestId": "205242b3-cb35-41e0-a109-7fab351fe17e",
  "status": "approved",
  "deviceId": "terrys-iphone",
  "credential": "<returned-once-device-credential>",
  "grantedCapabilities": [
    "status-read",
    "decisions-read",
    "decisions-answer",
    "display-profiles-read",
    "display-profiles-apply",
    "audio-profiles-read",
    "audio-profiles-apply",
    "shortcuts-read",
    "shortcuts-run",
    "operations-cancel"
  ]
}
```

The first approved poll consumes the one-time bearer-credential delivery. An expired request or consumed delivery returns `410 Gone` with a specific Problem Details code. The credential is never returned again by another poll, general status, device-list, or support API. If that response is lost, or the app later loses the credential, the device must pair again. A lost submission response can be retried with the same `Idempotency-Key`, pairing secret, polling secret, and body while the pairing session is valid.

The QR expires after 10 minutes if pairing has not completed. This does not expire a successful pairing: the stored bearer credential has no routine expiry and may still work after six months without opening the app. A submission retry after the QR expires returns `410 pairing-expired`, including when its original request received `202`.

### 2.2 Normal application startup

On later launches, the phone:

1. loads the saved host identity, TLS SPKI pin, and device credential;
2. discovers or reconnects to the saved Gateway;
3. validates the TLS SPKI before sending the credential;
4. calls `GET /v1/capabilities`;
5. calls `GET /v1/devices/current` to refresh its own authoritative grants, then disables UI for unsupported or unauthorized features;
6. retrieves the required resource collections and operation/decision state;
7. resumes polling any active operation known locally and reads the pending-decision inbox when granted `decisions-read`;
8. optionally opens `GET /v1/events` and reconciles its initial snapshot with locally displayed state.

Example protected request:

```http
GET /v1/operations HTTP/1.1
Authorization: Bearer <device-credential>
Accept: application/json
traceparent: 00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01
```

If Gateway returns `401 Unauthorized`, the app must treat the credential as invalid or revoked. It must not repeatedly retry the same credential. The app should remove it from active use and guide the user through pairing again.

If the TLS SPKI changes unexpectedly, the app must not send its bearer credential or offer a bypass button that silently trusts the new key. In the first release, TLS key replacement requires approval in local WinForms and a fresh QR pairing. Host-identity loss or reset also invalidates every paired credential and requires fresh pairing.

### 2.3 List and apply a display profile

Retrieve profiles:

```http
GET /v1/display-profiles?includeArtwork=true HTTP/1.1
Authorization: Bearer <device-credential>
If-None-Match: "display-profiles-42"
```

Possible results:

- `200 OK` with the current list and a new ETag;
- `304 Not Modified` when the saved representation remains current;
- `403 Forbidden` when `display-profiles-read` is absent;
- `503 Service Unavailable` when the target UserAgent is temporarily unavailable.

Apply one selected profile:

```http
POST /v1/display-profiles/racing/applications HTTP/1.1
Authorization: Bearer <device-credential>
Idempotency-Key: 6f057fc5-22a9-4532-ac15-c76e04e31f08
Content-Length: 0
```

Accepted response:

```http
HTTP/1.1 202 Accepted
Location: /v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd
Idempotency-Key: 6f057fc5-22a9-4532-ac15-c76e04e31f08
```

```json
{
  "operationId": "4eca79f6-8fcf-418c-b55d-61dfa14ba5bd",
  "status": "requested",
  "href": "/v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd"
}
```

`202 Accepted` means the display-profile application has started, not that the display has changed successfully. The device needs `status-read` to track it. Save the returned `operationId` and `Location`, then:

1. `GET` the operation URL, and poll it with bounded backoff while `isFinished` is `false`. An SSE client may use `operation.updated` events between reads.
2. Show `phase` and `message` as progress. Use `sequence` to ignore an older update, and show a temporarily unavailable state when `isStale` is `true`.
3. Show Cancel only when the device has `operations-cancel` and the latest operation says `canCancel: true`. Submit `POST /v1/operations/{operationId}/cancellations` with a new `Idempotency-Key`, then keep tracking that same operation.
4. Finish only when `isFinished` is `true`: show success for `succeeded`, the safe `error` for `failed`, or cancellation for `cancelled`. A `202` cancellation response is not the final result.

If the application submission response is lost, retry the identical request with the same `Idempotency-Key`. Do not create a new key until the user intentionally starts another application. The common [tracking](#26-track-and-recover-an-operation) and [cancellation](#27-cancel-an-operation) sections cover reconnects and `409` races.

### 2.4 List and apply an audio profile

Retrieve saved audio profiles with `GET /v1/audio-profiles`, or one selected item with `GET /v1/audio-profiles/{profileId}`. Listing needs `audio-profiles-read`; applying needs `audio-profiles-apply`. Use the returned opaque ID, not the profile name.

```http
POST /v1/audio-profiles/speakers/applications HTTP/1.1
Authorization: Bearer <device-credential>
Idempotency-Key: b79c8074-e21e-4f37-b2ba-105fd3263a09
Content-Length: 0
```

On `202 Accepted`, save the returned operation `Location` and `operationId`. The device needs `status-read` to track it. An audio change can take longer than the HTTP request. Retrieve `GET /v1/operations/{operationId}` immediately, then poll with bounded backoff while `isFinished` is `false`; SSE is optional. Display the latest `phase` and `message`, such as `applying-audio-profile`, and ignore updates with an older `sequence`.

Keep the operation visible until it reaches `succeeded`, `failed`, or `cancelled`; show its safe `error` when failed. If it is stale, show that progress is temporarily unknown and continue recovery rather than reporting success. If the device has `operations-cancel` and `canCancel` is true, offer Cancel through `POST /v1/operations/{operationId}/cancellations` with a new `Idempotency-Key`. Continue watching the original operation after `202` until it becomes terminal. Handle a later `409 operation-not-cancellable` by refreshing its status. Retry a lost application response with the original request and idempotency key.

### 2.5 Run a shortcut and follow the game

Retrieve saved shortcuts with `GET /v1/shortcuts`, or one selected item with `GET /v1/shortcuts/{shortcutId}`. Listing needs `shortcuts-read`; running needs `shortcuts-run`. The phone submits only the saved shortcut ID; it does not send executable paths or game arguments.

```http
POST /v1/shortcuts/iracing/runs HTTP/1.1
Authorization: Bearer <device-credential>
Idempotency-Key: d2418817-5e7c-448e-bd99-e38139539e2c
Content-Length: 0
```

On `202 Accepted`, save the operation `Location` and `operationId`, and `GET /v1/operations/{operationId}` using the `status-read` grant. Poll that resource with bounded backoff until `isFinished` is true. A shortcut can report `starting-game`, `waiting-for-game-to-start`, and `waiting-for-game-to-close` while `status` remains `running`. The game closing may be followed by after-programs and display/audio restoration. **Do not treat game launch or game exit alone as completion**; wait for terminal `succeeded`, `failed`, or `cancelled`. Show the server's current `phase` and `message` so the user can see when the game is running and when cleanup is in progress.

Use `sequence` to reject old updates after reconnecting. If `isStale` is true, show that progress is temporarily unknown and refresh when UserAgent returns. If `status` becomes `waiting-for-decision`, a device with `decisions-read` can fetch the pending decision; answering it also needs `decisions-answer`. Offer Cancel only with `operations-cancel` and `canCancel: true`; submit `POST /v1/operations/{operationId}/cancellations` using a new `Idempotency-Key`, then continue tracking the same operation through any restoration. A `409 operation-not-cancellable` means refresh the operation and remove the Cancel action. Show the safe `error` on terminal failure. Retry a lost run submission response with the original request and idempotency key so it does not start the game twice.

### 2.6 Track and recover an operation

With `status-read`, the phone retrieves the latest retained operation state:

```http
GET /v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd HTTP/1.1
Authorization: Bearer <device-credential>
```

Normal tracking with polling:

1. process the initial operation representation;
2. poll the operation resource with bounded backoff while it is active;
3. ignore duplicate or older sequence numbers;
4. display the server-provided phase and message;
5. stop polling when the operation is terminal.

SSE is optional for the phone. A client that uses it can process `operation.updated` events between authoritative reads. After app suspension, network loss, or SSE disconnection:

1. reconnect to `/v1/events` using `Last-Event-ID` when available;
2. apply missed events after a valid retained cursor, with no new snapshot; if the cursor is unknown or expired, replace included local sections from the fresh authoritative snapshot;
3. retrieve any operation still shown as active locally;
4. replace local state only with an equal or newer authoritative sequence.

Polling `GET /v1/operations/{operationId}` is a complete supported workflow. Clients should use bounded backoff, honor `429` and `Retry-After`, and avoid rapid polling while a healthy SSE stream is active. Keep the operation ID across app suspension or restart so a later launch can retrieve its latest retained state.

### 2.7 Cancel an operation

Use the latest operation's `canCancel` value to enable or disable Cancel. It is false for stale and terminal operations. If the value changes before the cancellation request arrives, handle `409 operation-not-cancellable` and refresh the operation.

```http
POST /v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd/cancellations HTTP/1.1
Authorization: Bearer <device-credential>
Idempotency-Key: 841134a1-0c97-4c52-892b-7207b7cc553f
Content-Length: 0
```

`202 Accepted` means cancellation was accepted for processing. It does not mean restoration or cancellation has completed. The phone continues observing the original operation until it becomes terminal.

Expected failures include:

- `403 Forbidden` without `operations-cancel`;
- `404 Not Found` for an unknown or non-visible operation;
- `409 Conflict` when the operation is already terminal or cannot be cancelled safely.

### 2.8 Receive and answer a decision

The phone may learn about a decision from:

- a `decision.updated` SSE event;
- `GET /v1/decisions?status=pending`;
- the operation's nested decision collection.

Retrieve the authoritative decision:

```http
GET /v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd/decisions/7ce641c9-31f0-4461-af04-5c2dd68574f4 HTTP/1.1
Authorization: Bearer <device-credential>
```

The client displays only the choices returned by the decision.

Answer:

```http
PUT /v1/operations/4eca79f6-8fcf-418c-b55d-61dfa14ba5bd/decisions/7ce641c9-31f0-4461-af04-5c2dd68574f4/response HTTP/1.1
Authorization: Bearer <device-credential>
Idempotency-Key: 5a98ed53-c8ef-4b5c-858d-390337dafd98
Content-Type: application/json

{
  "choice": "stop-and-restore"
}
```

Repeating the identical response is safe. A different second response, an expired decision, or a response to an already-resolved decision returns `409 Conflict` Problem Details.

The phone continues tracking the parent operation after the answer.

### 2.9 Watch interaction

The watch sends a narrow internal request to its companion phone, for example:

```json
{
  "command": "apply-display-profile",
  "profileId": "racing",
  "clientRequestId": "watch-7821"
}
```

The phone:

1. verifies that the watch request is valid for the signed-in app;
2. verifies that its Gateway grant includes the required capability;
3. submits the normal REST request with a new idempotency key;
4. receives the operation ID;
5. tracks operation status through SSE/GET;
6. relays compact status to the watch.

Example compact relay:

```json
{
  "clientRequestId": "watch-7821",
  "operationId": "4eca79f6-8fcf-418c-b55d-61dfa14ba5bd",
  "title": "Racing",
  "status": "running",
  "message": "Applying display profile"
}
```

The watch never receives the Gateway bearer credential and never calls Gateway directly.

### 2.10 Telegram connector interaction

The local connector long-polls Telegram over outbound HTTPS. After validating the bot, immutable Telegram user ID, and private-chat ID, it maps an approved command to the same Gateway REST workflow.

Example:

```text
/apply racing
```

Connector flow:

1. resolve `racing` against `GET /v1/display-profiles`;
2. submit `POST /v1/display-profiles/{profileId}/applications`;
3. use a new idempotency key for the Telegram update;
4. track the returned operation;
5. edit or reply to the Telegram message with phase and terminal status.

The connector's Gateway credential is independent of the phone credential and normally excludes device-management and pairing-approval capabilities.

### 2.11 Device revocation

An authorized phone lists devices:

```http
GET /v1/devices HTTP/1.1
Authorization: Bearer <device-credential>
```

It revokes one:

```http
DELETE /v1/devices/old-phone HTTP/1.1
Authorization: Bearer <device-credential>
Idempotency-Key: 96d0e496-62d7-4a69-a636-014290702c33
```

After `204 No Content`, the revoked credential must fail immediately with `401 Unauthorized`, including on an existing SSE reconnect. Device revocation does not revoke other independently paired clients.

### 2.12 General client error behavior

Clients should handle errors by HTTP status and Problem Details:

- do not retry `400`, `401`, `403`, `404`, or `422` without changing input, credentials, or user action;
- treat `409` protocol-version conflicts and `412` unsupported required protocol features as client compatibility failures; retry a `409` idempotency-pending result only when its Problem Details marks it retryable and honor `Retry-After`;
- treat `409` according to the resource state, retrieving the current resource when useful;
- honor `Retry-After` for `429` and `503`;
- retry transient transport failures with bounded exponential backoff and jitter;
- reuse the same idempotency key only for the identical mutation;
- never send bearer credentials after an unexpected TLS pin change;
- surface the Problem Details `requestId` in support UI without showing credentials or secrets.

## 3. Operation polling and watch presentation

### 3.1 Phone polling and events

The phone may poll:

```text
GET /v1/operations/{operationId}
```

The simple phone flow is:

1. submit an application/run command;
2. receive `202 Accepted` and the operation `Location`;
3. retrieve the initial operation representation;
4. poll the operation resource with bounded backoff until terminal;
5. retrieve the operation after reconnecting or whenever an authoritative refresh is required.

Clients may add `GET /v1/events` later for faster updates. No SSE parser is required for a first phone client.

SSE `operation.updated` data carries the full latest operation representation defined in the [target schema](target-openapi.json). `GET /v1/operations/{operationId}` remains the authoritative resource and recovery path.

### 3.2 Watch status relay

Watches do not call the operation endpoint directly. The companion phone receives or retrieves the operation state, converts it into a compact watch message, and relays it through WatchConnectivity or the Wear OS Data Layer.

Example internal phone-to-watch message:

```json
{
  "operationId": "4eca79f6-8fcf-418c-b55d-61dfa14ba5bd",
  "title": "iRacing",
  "status": "running",
  "message": "Game running"
}
```

This compact relay is an internal application message, not a separate Gateway REST representation.

## 4. Building a client

### 4.1 Minimum controller workflow

A minimal phone or integration client needs:

1. Trusted pairing and secure credential storage.
2. A TLS transport configured for the paired host, not a generic HTTP client that disables verification.
3. Capability-aware feature availability.
4. One saved-resource list and one apply/run action.
5. Tracking through the returned operation resource.
6. Error presentation and safe retry.

SSE can be added after authoritative operation polling works. Device administration and pairing approval are not required for a minimal controller. Dashboard aggregation is deferred from the first released API.

Use this distinction when selecting grants:

| Client feature | Grants needed |
|---|---|
| Display-profile picker and apply with progress | `display-profiles-read`, `display-profiles-apply`, `status-read` |
| Audio-profile picker and apply with progress | `audio-profiles-read`, `audio-profiles-apply`, `status-read` |
| Shortcut picker and run with progress | `shortcuts-read`, `shortcuts-run`, `status-read` |
| Show and answer recovery decisions | `decisions-read`, `decisions-answer` |
| Offer cancellation | `operations-cancel`, plus `status-read` to observe the outcome |
| Manage paired devices | `devices-read`, `devices-revoke` |
| Approve another device | `pairing-approve` |

Known server capabilities are not the device's permission set. Read `GET /v1/devices/current` after authentication for current grants; use the stored pairing grant only until that request succeeds. Always handle `403` as an authoritative denial.

### 4.2 Base URL, resource IDs, and credentials

- Obtain the HTTPS origin from trusted pairing data; do not hard-code the example host or port.
- Resolve relative `Location`/`href` values against that trusted origin.
- Verify the origin of a returned URL before attaching credentials. Do not automatically forward authentication on redirects to another origin.
- Encode each opaque resource ID as one URL path segment, and encode query values separately.
- Preserve the method, escaped path, raw query, content type, and exact body bytes for an idempotent retry.
- Replace `Authorization: Bearer <device-credential>` with the securely stored credential. Angle-bracket strings in examples are placeholders, not literal values.
- The temporary `DisplayMagician-Pairing` authorization uses the phone-generated polling secret. It is not a device bearer credential and must not be used for ordinary commands.
- Keep secrets out of URLs, crash reports, analytics, screenshot-based support capture, and HTTP debug logging.

Do not use `curl -k`, a trust-all TLS callback, or another certificate-verification bypass as a production transport. A runnable pairing sample needs the finalized certificate-validation and SPKI rules first.

### 4.3 Response processing

Process the HTTP status before decoding a success model:

1. On `200`, decode the documented route-specific representation.
2. On `202`, retain the operation or pairing resource URL and observe that resource; do not report completion.
3. On `204`, do not attempt to decode a JSON body.
4. On `304`, retain the representation associated with the conditional request; there is no replacement JSON body.
5. On an error, decode Problem Details when the content type is `application/problem+json`.
6. If an intermediary returns non-JSON error content, show an explicit transport/server error rather than treating it as an empty resource.
7. Do not parse the human-readable `message`, `title`, or `detail` to determine machine state. Use status, phase, error code, and documented fields.

Treat UTC timestamps as timestamps, not display strings. Format them locally for UI. Do not calculate an invented progress percentage from elapsed time.

### 4.4 Safe retries and mobile lifecycle

Persist an unresolved mutation's idempotency key and exact request identity before sending it if the application needs to recover that action after process termination.

On a lost submission response:

```text
keep the original method, URI, content type, body bytes, and idempotency key
retry only while the server's documented retention guarantee applies
if the retained result is returned, follow its operation resource
if the outcome cannot be recovered, surface uncertainty rather than start new work
```

The server retains an accepted REST idempotency result for 24 hours, including across Gateway and ControlService restarts. A retry after that window can create a new operation; the client must reconcile operation history or ask the user before repeating the action.

After mobile suspension or a reconnect:

- validate TLS trust before transmitting the credential;
- reconnect the event stream or retrieve current operation state;
- do not assume the server cancelled work while the phone was offline;
- reconcile a retained operation by ID rather than resubmitting it;
- if an operation returns `404` after retention, do not translate that into success or failure;
- handle expired decisions and answers submitted by another client;
- do not reconnect indefinitely after an authentication rejection.

If a replacement host identity or service restart invalidates continuity, follow the documented snapshot/reconciliation path. Per-operation sequence numbers are not a global event cursor.

### 4.5 SSE transport

Use an SSE client that can attach bearer authorization in an HTTP header. Do not put a credential in a query parameter to work around a library limitation.

The client parser must support:

- UTF-8 streaming across arbitrary network chunk boundaries;
- blank-line event delimiters;
- `event`, `id`, and `data` fields, including multiple data lines;
- keep-alive comments;
- a snapshot on a new stream or after replay continuity is lost, and replayed events without a snapshot when `Last-Event-ID` is valid;
- cancellation/disposal when the application no longer needs the stream;
- reconnect and `Last-Event-ID` without treating event IDs as operation sequence numbers.

Capability filtering can omit entire resource classes. Absence of decisions from a snapshot is not proof that no decision exists when the caller lacks `decisions-read`.

Use the event names and payloads in the [target schema](target-openapi.json). Do not invent event names from IPC numeric event types.

### 4.6 Platform and generated-client integration

- Review platform local-network permissions and background execution constraints for the supported phone OS versions.
- Store credentials in the platform secure store; avoid ordinary preferences or plain files.
- Keep TLS trust, credential injection, retry/idempotency state, and SSE handling in one client transport layer.
- Generate route models only from OpenAPI matching the server build being targeted.
- Add handwritten lifecycle and secure-transport code around generated clients; generated request methods do not automatically implement pairing, pinning, retries, or event recovery.
- Keep phone/watch credentials on the phone.
- Browser clients are deferred; do not assume CORS or browser EventSource support.

Runtime cross-process views and shared protocol types belong in `DisplayMagician.Contracts`. Swift/Kotlin code generation consumes the REST schema; it does not create a new server-side authority or justify client-specific ControlService DTOs.

### 4.7 Duplicate watch and Telegram commands

The phone should associate the watch's `clientRequestId` with the REST request/idempotency key and eventual operation. If the same watch request is redelivered, recover that association rather than create another run.

Likewise, a Telegram connector should associate an already-handled Telegram update with its submitted request/operation. Outbound long polling can redeliver updates; receiving the same update is not a new user command.

Any persisted integration-side mapping must protect credentials and must not replace Gateway's own idempotency enforcement.

## 5. Client test checklist

Test against the API contract available in the server build:

- [ ] trusted QR bootstrap, approval, rejection, expiry, and lost credential delivery;
- [ ] certificate renewal retaining SPKI, unexpected key change, and invalid TLS trust;
- [ ] revoked/missing credential and missing capability;
- [ ] unavailable or locked interactive target;
- [ ] stale saved-resource ID;
- [ ] accepted command versus successful/failed/cancelled terminal operation;
- [ ] lost mutation response retried without duplicate execution;
- [ ] wrong or expired idempotency key reuse;
- [ ] event disconnect, snapshot recovery, duplicate/out-of-order updates;
- [ ] app suspension/process restart while work continues;
- [ ] decision answered by another client or expired before submission;
- [ ] cancellation and restoration still in progress;
- [ ] `204`, `304`, Problem Details, malformed/non-JSON error responses;
- [ ] no credentials in URLs, logs, watch messages, or support data;
- [ ] duplicate watch/Telegram requests do not start duplicate work.

Do not run disruptive hardware tests on a production machine without explicit test setup. Installed-component validation should use the repository's Windows Sandbox workflow where applicable, followed by deliberate hardware testing on a suitable test PC.
