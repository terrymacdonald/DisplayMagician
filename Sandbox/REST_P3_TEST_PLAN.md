# REST P3 installed validation

Status: prepared, not executed on the development PC. The owner has left installed Windows and native device acceptance for their test PC and device environments (2026-10-22). Record the actual installer version, Windows version, date, and each result when running this plan.

## Prepare on the host

1. Build the current Debug Bundle with `build_displaymagician.ps1`.
2. Run `test_rest_p3.ps1 -PrepareSandbox`. This builds the developer-only harness and copies it into the existing read-only `Sandbox/Local` mapping. It does not run installed tests on the host.
3. Launch Windows Sandbox through `debug_displaymagician.ps1`, install the Bundle, start WinForms, and enable Gateway in its Server Settings if required. Use the registered advertised HTTPS origin and port shown there.
4. Create two disposable saved display profiles, audio profiles, and shortcuts where the sandbox supports them. Unsupported hardware or missing saved data must be recorded as untested rather than passed.

## Automated installed checks

In the sandbox, run the staged executable with the configured registered origin, for example:

```powershell
& 'C:\DisplayMagician\Local\RestP3Harness\RestP3Harness.exe' `
    'https://YOUR-REGISTERED-HOST:22846' `
    "$env:USERPROFILE\Desktop\rest-p3-results.json"
```

The harness pairs disposable clients through same-user IPC. It needs the installed ControlService and Gateway but no copied bearer credential. It revokes its test devices in cleanup. Inspect any remaining **REST P3 test device** rows in WinForms after an interrupted run and revoke them.

The report checks actual HTTPS pinning and protocol welcomes; paged display/audio/shortcut/operation/decision resources; strong ETags and 304 responses; first SSE snapshots; retained cursor reconnects; unknown cursor snapshots; one-stream-per-device and four-streams-per-user limits; revocation closing a stream; and subsequent 401 responses. `needs-data` is an outstanding test. Copy the sanitized report out before closing Sandbox.

## Service and event continuity

Use an authorized SSE client and the generated phone foundations alongside WinForms:

| Case | Expected result |
|---|---|
| Create, rename, update, and delete a disposable saved profile/audio profile/shortcut | A correctly granted client receives its saved ID and `upsert`/`deleted`; deleted events have null links. Unrelated users and denied grants receive none. |
| Edit during SSE connection setup | The change appears in snapshot state or in subsequent replay; it is not lost. Operation clients discard lower sequences. |
| Disconnect, edit, reconnect with the last event ID | Only later retained events are replayed, without an initial snapshot. |
| Restart Gateway, reconnect with the last event ID | Retained events remain resumable. No duplicate mutation occurs. |
| Restart ControlService after events are durably saved | Retained cursors remain valid. A detected interrupted publication instead forces a fresh snapshot. |
| Kill ControlService while committing an edit, then restart | The saved edit is either absent or present once; its pending marker forces continuity reset if event publication was interrupted. Re-fetch saved collections after the reset. |
| Leave a stream idle | A keep-alive comment arrives within 15 seconds. Revocation or a failed credential recheck closes it within five seconds, including during a blocked pipe/read/write. |
| Expire or exhaust retained history | Reconnect gets a fresh snapshot. An empty replay store still provides a usable snapshot cursor. Item/byte/time boundaries are also covered by automated service tests. |
| Disconnect UserAgent during an operation | GET status reports the retained operation as stale; it does not claim completion. Reconnection restores authority. |

Fault injection for journal permissions/storage exhaustion belongs only in a disposable sandbox. Restore permissions/storage before continuing, and retain sanitized outcomes. Do not change machine-owned data ACLs on a normal installation to perform this test.

## Installer lifecycle and TLS

Use a new sandbox or reset snapshot for each lifecycle case. Preserve the approved older Bundle separately for upgrade tests.

1. Fresh install: Gateway and ControlService start under their intended accounts; the configured HTTPS listener works on Private/Domain networks; Public remains opt-in.
2. Restart both services and Windows: saved credentials, listener settings, and host/TLS identities remain usable; a phone unused for six months has no routine credential expiry.
3. Change the configured port through WinForms: listener and installer-owned firewall rules agree after restart; old port is not left unintentionally open.
4. Renew the certificate with the same P-256 key in a disposable test install: SPKI pin remains unchanged and clients reconnect without pairing again. Use a short-lived test certificate to exercise the actual renewal scheduler; the portable client tests separately cover two certificates with the same key.
5. Approve TLS key replacement locally: credentials are revoked and clients require pairing again. Resetting the host identity also invalidates pairings.
6. Upgrade from the approved older installer: service payloads and identity state remain consistent. Interrupt the update in another disposable sandbox, then verify rollback/recovery leaves a usable install.
7. Uninstall: shipped services/listeners and installer-owned firewall rules are removed according to the installer policy. No orphan Gateway process remains.

## Native phone and watch acceptance

Run [the client acceptance checklist](../Clients/README.md) on an iOS 16+ device/simulator with Xcode and an Android API 26+ device/emulator. A native compiler check or JVM test does not validate Keychain, Android Keystore, URLSession trust challenges, Android hostname verification, WatchConnectivity, or Wear OS Data Layer on a device. Telegram conformance remains conditional on starting that product.
