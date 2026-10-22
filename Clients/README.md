# DisplayMagician phone client foundations

These are reusable API foundations, not complete phone/watch applications. The approved minimums are iOS 16 and Android 8/API 26. Polling covers the entire controller workflow; an SSE parser is optional and is not required by these clients.

## Generate and verify

Run these repository-root entry points:

```powershell
.\generate_rest_clients.ps1
.\verify_rest_clients.ps1 -CompileKotlin -InstallTools
```

Generation uses Node.js 18+ and the implemented [OpenAPI document](../Docs/REST/openapi.yaml). It produces 48 shared-schema models and typed methods for all 25 ordinary HTTP routes. `--check` compares generated sources with the schema, allowing Git's normal line-ending conversion; it does not claim a native build passed. String enums stay strings so newer server values do not crash an older decoder. Timestamps remain ISO 8601 strings; operation sequences use 64-bit integers. Protocol version constants are read from `DisplayMagician.Contracts`.

Kotlin verification compiles against the installed Android SDK, then runs portable JSON, QR, pinning, renewal, watch relay, and retry contract checks on a JVM. Its compiler and JSON-java test dependency are cached under ignored `.tools/Kotlin`, with pinned download hashes in `tool_versions.json`. Android itself supplies `org.json`; JSON-java is not shipped with the app. Pass `-JavaPath` and `-AndroidJar` if discovery cannot find your tools.

On macOS with Xcode selected:

```text
swift test --package-path Clients/Swift
```

The REST client CI workflow provides separate macOS Swift and Android/JVM checks when pushed to GitHub. It has been added locally; a GitHub run is not implied by this document. The Windows development run cannot compile Apple Security/Keychain code. Public-only test certificate fixtures can be refreshed with `node BuildScripts/create_rest_client_test_certificates.mjs`; never include real pairing material in fixtures.

## Swift integration

Add the local `Clients/Swift` package to the phone application and import `DisplayMagicianGateway`. See [Swift setup](Swift/README.md) for app permissions and TLS configuration.

```swift
let qr = try DMPairing.decodeAppLink(scannedLink)
let client = try DMGateway(target: DMTarget(qr: qr), deviceId: savedDeviceId,
                          storageKey: "gateway-" + qr.hostId)
_ = try await client.startPairing(qr: qr, displayName: "My iPhone")
// The owner approves locally. pollPairing enforces the two-second poll interval.
while try await client.pollPairing().value?.status == "awaiting-approval" { }
// Handle rejection or a 410 expiry/lost delivery before continuing.
let current = try await client.getCurrentDevice().value

let key = UUID().uuidString.lowercased()
let accepted = try await client.runShortcut(shortcutId: selectedId, idempotencyKey: key).value!
let finished = try await client.waitForOperation(accepted.operationId) { operation in
    // Marshal UI updates to MainActor. Show phase/message; show Cancel only with
    // the operations-cancel grant and operation.canCancel.
}
```

Keep one client actor per paired host. Store the operation ID in application state so polling can resume after suspension/relaunch. Preserve the application's stable device ID across launches. A run remains active through game monitoring and restoration until `isFinished` is true. `cancelOperation` submits a cancellation with its own new idempotency key; keep polling the same operation afterward.

## Kotlin integration

Add `Clients/Kotlin/src/main/kotlin` to an existing Android app's main source set. See [Kotlin setup](Kotlin/README.md). The only external runtime dependency is the app's usual `kotlinx-coroutines-android`; JSON, TLS, cryptography, and secure storage use Android/JVM APIs.

```kotlin
val qr = DMPairing.decodeAppLink(scannedLink)
val client = DMGateway(context, DMTarget.fromQr(qr), savedDeviceId, "gateway-" + qr.hostId)
client.startPairing(qr, "My Android phone")
// Call pollPairing from a coroutine until approved/rejected/expired.
val current = client.getCurrentDevice().value
val key = java.util.UUID.randomUUID().toString()
val accepted = client.runShortcut(selectedId, idempotencyKey = key).value!!
val finished = client.waitForOperation(accepted.operationId) { operation ->
    // Update the phone UI from the appropriate coroutine context.
}
```

All network methods are suspending. Cancel the polling coroutine when its screen no longer needs updates; cancellation of a phone's polling task does not cancel a running Gateway operation. Use the explicit `cancelOperation` API for that.

## Pairing, trust, and credentials

Both transports reject redirects, require HTTPS/TLS 1.2+, verify the certificate's SPKI SHA-256 against the QR pin, and check hostname and validity. They fetch and verify public host identity before submitting pairing secrets or bearer requests. The public host ID is checked as the RFC 7638 canonical P-256 JWK thumbprint. Certificate renewal with the same key remains trusted; changing the TLS key or host identity requires the approved local recovery and pairing flow.

Swift uses Keychain with `AfterFirstUnlockThisDeviceOnly`. Android encrypts state using an Android Keystore AES-GCM key and writes ciphertext atomically under `noBackupFilesDir`. Credentials have no routine expiry. Storage loss or an approved response lost before durable credential storage requires pairing again. `forgetPairing` is an explicit local application action: reconcile outstanding work and get the user's confirmation before discarding its pending mutation ledger; then dispose/recreate the client. It does not revoke the device on Gateway. Do not reset automatically on a transient 401/503 or a pin failure.

The fixed controller permission set matches the client guide: operation/decision reads and answers, display/audio/shortcut reads and actions, and operation cancellation. `startPairing` requests this set by default. There is no permission-selection UI in these foundations; the server approves exactly the requested grants. Read `getCurrentDevice` at startup for authoritative grants.

## Safe mutation recovery

Each generated mutation requires a lowercase UUID v4 key. The transport persists the exact method, path/query, and body before transmitting it. Reusing a key with different bytes is rejected locally. On relaunch, inspect `pendingMutations` and use `retryMutation` with its original key to send the saved bytes. A response's operation `Location` is retained when available. Once the application has durably recorded/reconciled the result, call `acknowledgeMutation`.

There are no automatic mutation retries with a new key. `409 idempotency-outcome-unknown` requires reading operation/device/decision state before a user intentionally starts another action. `Retry-After`, status, ETag, and Location are exposed on response/error objects. Conditional GET 304 returns no value so the app keeps its previously cached representation. The operation polling helper backs off and honors retryable 429/503, and stops on authentication or trust errors.

Do not log app links, generated pairing request/poll models, credentials, pending request bodies, or secure-store contents. Sensitive generated model descriptions and pending-mutation descriptions are redacted, but explicitly serializing them still yields their real transport data.

## Phone/watch relay contract

`DMWatchOperation` is a compact application message for WatchConnectivity or Wear OS Data Layer. Its JSON carries operation ID, title, status, message, sequence, finished state, and whether cancellation is available. It carries no Gateway credential, QR secret, host key, or Windows identity. Keep one `DMWatchOperationState` per tracked operation; it rejects repeated or older sequences. The companion phone revalidates grants and current `canCancel` before forwarding any watch cancellation. The actual platform channels and watch UI belong in their applications.

## Acceptance checklist

Record device/OS versions and actual results; do not mark a row passed from a compiler or JVM check.

| Check | Expected result |
|---|---|
| First pairing on iOS 16+/Android API 26+ | Native local-network prompts, QR decoding, host identity, pinned HTTPS, local approval, and one-time delivery succeed. |
| Wrong hostname/pin, expired certificate, HTTP origin, redirect | Connection/request is rejected before credential disclosure. |
| Same-key certificate renewal | A new valid certificate with the original SPKI reconnects without pairing. |
| Custom private DNS, IPv4, IPv6, .local | The chosen iOS ATS setup and Android hostname checks work with registered certificate names. |
| Six-month gap, suspend, process kill, reboot | Durable pairing survives without routine credential expiry; current grants are refreshed. Secure-store loss requires pairing again. |
| Lost mutation response/process killed during submission | The same saved key and request bytes are used after relaunch. Outcome-unknown is reconciled, not automatically restarted. |
| Display/audio apply and shortcut game run | Polling reaches completion through game monitoring and restoration; stale state is displayed; cancellation uses the operation API. |
| Pending decision answered elsewhere or expired | Refresh displays the resolved state; a new-key answer receives conflict. |
| Watch relay | Both platform channels decode the fixture shape, reject lower sequences, forward allowed commands through the phone, and never receive credentials. |
| Optional SSE | Native headers include ProtocolHello/Bearer; retained IDs resume; reset snapshots replace visible state; reconnect uses jittered 1-to-30-second backoff. |

The installed Windows plan is [Sandbox/REST_P3_TEST_PLAN.md](../Sandbox/REST_P3_TEST_PLAN.md). Telegram conformance remains deferred until that product is started.
