# Systematic codebase bug review

Started 27 October 2026. This is an ongoing review log, not a certification that the whole repository is bug-free. The initial pass targets component boundaries and failure paths. Deeper file-by-file review remains queued below.

## Method

Trace callers before changing behaviour. Add regression coverage for concrete bugs, make small fixes in the owning component, and build/test before moving on. Run all available suites at milestones. Preserve existing schemas, trust boundaries, and recovery policy. Ask before consequential product or compatibility decisions. Do not treat source inspection or a test count as proof of full code coverage.

## Sections and progress

| Order | Section | Initial checks performed | Further checks |
| --- | --- | --- | --- |
| 1 | Shared foundations | Wire framing, negotiation, retry policy and portable shortcut models. Fixed required-capability response checks and undefined client-kind acceptance; added framing coverage. | Exercise more malformed nested request payloads and shared-contract consumers. |
| 2 | Service coordination | Decision expiry, schedules, lease/recovery paths and atomic persistence. Fixed late decisions overriding defaults, failure-count overflow, and transient atomic replacement failures. | Continue through routing, replay stores, event journal durability, and pairing state machines file by file. |
| 3 | Remote boundary | Paging, response limits, protocol middleware, traffic limiting and registration paths. Fixed shared mutable objects in pagination snapshots. | Deep review of authentication, TLS renewal, idempotency, SSE cancellation, and concurrent resource changes. |
| 4 | Agent lifecycle/storage | Message read/write and synchronization boundaries, command failure handling; checked preceding lifecycle/recovery fixes. Fixed successful replies after failed message-index writes. | Shortcut repository/migration concurrency, cancellation, and interrupted storage operations. |
| 5 | Interactive execution | Process launch failure paths and diagnostic contents; existing process/window regression suites run. Removed raw launch arguments from logs. | Vendor/native display and audio structures, game-library discovery and process-monitor handovers. Hardware checks are not replaced by unit tests. |
| 6 | Desktop application | Messages form load, read/unread actions and disposal after awaits. Fixed unhandled asynchronous service errors and added fake-client form tests. | Other forms, tray/event races, dialog ownership, editor conflicts, and disposal during refresh. |
| 7 | Other clients | Console decision validation and pipe replies; generated Swift/Kotlin checks, Android/JVM compile and contract tests. Fixed empty IDs, undefined numeric decisions, and null pipe replies. | Deeper retry/cancellation and native TLS/key-storage checks. Swift requires macOS/Xcode. |
| 8 | Session broker | Fixed payload and pipe identity checks, launch/stop validation, environment creation and native-handle cleanup. Launch now fails if the user environment cannot be created; added unprivileged validation tests. | The approved Hello/Welcome handshake is implemented in caller and broker, with isolated pipe/dispatch tests. Privileged token/process launch still requires installed Sandbox validation. |
| 9 | Diagnostics | Shared layout audit, launch argument redaction, support-bundle collection/filter and staging boundaries; retained existing bundle tests. | Reparse-point traversal, partial collection failures, archive interruption and all retained log sources. |
| 10 | Build/installation | Test-script failure reporting, installer Gateway declarations, package/Sandbox workflow inspection, full test-script milestone. Fixed captured native stderr truncating test failure reports. | Full package build and isolated install/upgrade/uninstall runs in Windows Sandbox. No services were installed or restarted. |

## Regression fixes in this pass

- Protocol replies must include all required capabilities; undefined numeric client kinds are rejected. Existing protocol/schema versions are unchanged.
- A decision answered at or after its deadline is resolved with its existing default, and the late answer is rejected even before the expiry sweep.
- Persisted large client-sync failure counts remain within the existing capped backoff instead of overflowing into immediate retries.
- Gateway snapshots isolate mutable DTOs, nested arrays and returned page objects.
- Failed message-index writes return failure and retain the existing read state; synchronization stops when its index cannot be saved.
- Process launch messages omit raw arguments from support logs.
- The Messages form handles initial service failures and failed read/unread actions, and avoids updating disposed controls after relevant awaits.
- Console decision input rejects empty prompt IDs and undefined choices; a missing service reply is handled as invalid data.
- SessionLauncher checks user environment-block creation before starting UserAgent. Request-validation tests never obtain privileged tokens or start installed services.
- The machine atomic writer retries only Windows sharing/lock/replacement contention (errors 32, 33, 1175), for at most three attempts with 100/200 ms delays. It logs exhaustion and propagates failure. It retains atomic replacement and backups; it does not fall back to overwriting the live file.
- The test script lets native test output finish and uses the process exit code when stderr is captured under Windows PowerShell.

## Verification

- Initial baseline: repository clean. The root test script exposed an intermittent replay test failure. Direct reruns isolated a real Windows atomic-replacement contention error (1175), now covered by deterministic retry/exhaustion tests.
- Milestones before final run: ControlService 143 passed before adding broker/atomic tests; Gateway 47 passed; UserAgent 152 passed; Console 4 passed.
- Kotlin: compiled against the installed Android SDK; operation, 64-bit sequence, watch relay, retry, QR expiry, TLS pin rejection and same-key renewal checks passed.
- Generated clients: 48 models and 25 ordinary polling routes match OpenAPI. REST route declarations: 26 methods/paths passed.
- Logging audit: zero source-prefix issues. Gateway installer configuration check passed.
- Final all-project run: test_displaymagician.ps1 completed successfully with normal network access: ControlService 154, Gateway 47, UserAgent/desktop 152, Console 4; 357 passed, zero failed/skipped. This adds 29 regression cases over the starting 328 tests. The formerly intermittent replay test also passed three focused reruns after the atomic-replacement fix.
- Restricted-network runs emitted NU1900 warnings and Console restore failed with NU1301/SSPI errors. Retrying restore and the full script with normal network access succeeded without changing sources, audit settings, or verification. The final full-run log has no build warnings/errors.

## Decisions and next work

The user approved the coordinated SessionLauncher handshake on 27 October 2026. Both launch and stop now carry a ControlService hello requiring protocol-negotiation/session-launcher. The authenticated broker negotiates before dispatch and returns the shared welcome/error metadata; the caller validates successful replies, endpoint kind, frame version/type and request identity. The version-1 envelope/pipe remain unchanged, but both components must be deployed together because unnegotiated successes are rejected. Twenty-four additional regression cases exercise real framing and isolated same-user test pipes with substituted process callbacks. No installed process launch/stop or pipe authentication rules were changed. Full follow-up verification passed: 178 service/broker tests, 47 Gateway tests, 152 Agent/UI tests and 4 Console tests (381 total), with zero failures/skips and no build warnings/errors. This adds 24 protocol regression cases to the preceding 357-test milestone. The affected build also passed with warnings treated as errors; the logging audit reported zero issues.

Continue the deeper passes in section order, starting with service routing/replay and malformed nested requests; then Agent repository concurrency, other WinForms forms, and vendor/native integration. Record each subsequent check and test result here rather than resetting the review.

Item 7 from the preceding window review (completed window-handle reservation) remains deferred as agreed.
