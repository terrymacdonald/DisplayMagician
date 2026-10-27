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

## Batch 2: service routing, leases and local replay

The user approved both recovery decisions: an unreadable local replay ledger blocks state-changing calls while reads continue, and replay-protected mutations reserve their request identity durably before execution. Interrupted reservations remain blocked; confirmed failures remain retryable.

Changes and evidence:

- Remote shortcut starts now call the same guarded shortcut router as local starts. Tests prove another session's active machine lease prevents dispatch and successful calls preserve request/operation IDs.
- Asynchronous display-profile acceptance no longer releases its machine lease. Matching terminal Agent statuses (including reconciled statuses) release or retain recovery ownership. Stale idle heartbeats cannot clear an explicit reservation or a recovery requirement; foreign users reusing a Windows session ID cannot update the old owner's lease. A reconnecting running Agent cannot claim recovery has completed.
- Launcher failures return structured responses for profile, shortcut and restart paths. An unconfirmed forced stop leaves the original registration in place and does not start a replacement. Invalid Agent JSON, null replies and frame-version mismatches become invalid-data failures; uncertain mutations report OutcomeUnknown rather than being retried automatically.
- The local replay store owns reservation/dispatch/completion serialization. Completed results retain the existing 24-hour lifetime. Pending reservations do not expire or get evicted. Existing unversioned completed entries are read as version 1; version 2 adds reservation metadata, and the first atomic write backs up the original ledger. Corrupt/future/duplicate records, missing-primary-with-backup and write failures do not silently reset replay history. Capacity exhaustion rejects new reservations rather than evicting unexpired results.
- JSON/credential-bearing administrative flows outside the existing replay-protected mutation set retain their domain transition rules; no new credential response cache was added. All non-read-only calls are still blocked if the local ledger becomes unavailable.
- Gateway internal commands validate their outer protocol version and return structured errors for malformed nested JSON or null grant lists. Capability checks and operating-system caller authentication remain in force.
- OutcomeUnknown is additive error code 27 in Contracts, mapped to non-retryable HTTP 409 operation-outcome-unknown. IPC/REST schemas, documentation and the generated client manifest were updated. Existing IPC schema bounds were also brought into line with already-defined error codes 24-26.

Regression coverage exercises real temporary ledgers and isolated test pipes, durable reservation visibility before the execution callback, concurrent duplicates, success/rejection/uncertainty, corrupt nested records, cross-user keys, atomic completion failure, legacy backups, capacity, remote routing, stale heartbeats and launcher failures. The forced-stop test waits through the real ten-second deadline using fake process clients; it never stops an installed process.

Verification: the service build passes with warnings treated as errors; all 219 service tests pass. The final test_displaymagician.ps1 run passed all four projects: ControlService 219, Gateway 48, UserAgent/desktop 153 and Console 4; 424 passed with zero failures/skips and no build warnings/errors. This batch adds 43 regression cases. Kotlin compilation/contract checks, generated-client checks, REST declarations and the logging audit pass. Hardware, privileged installed-service lifecycle and native Swift execution remain outside these automated checks.

Next batch: Agent-side nullable/nested request validation, command-cache in-flight duplicate handling, then repository/migration concurrency and WinForms event/disposal paths. Continue the deeper vendor/native and installed Sandbox passes afterwards. The entire-codebase review remains ongoing.

## Batch 3: Agent command concurrency and input validation

- Fixed the gap between successful-response lookup and execution: matching in-flight requests now await one owner rather than executing twice. Request-ID reuse with a changed type/payload is rejected while pending and after success. Different IDs are not serialized, allowing cancellation commands to execute while another command is waiting.
- Every arrival, including cache hits, validates the protocol version, Hello/capabilities, ControlService client kind, nonempty request ID and non-null payload before dispatch/replay. Operating-system pipe permissions are unchanged.
- Cancelling a duplicate waiter does not cancel its owner. Owner cancellation or an exception completes existing waiters and removes the in-flight entry. The existing successful-only, 24-hour in-memory cache and failure retry semantics are unchanged; the service's durable ledger remains responsible for blocking uncertain mutations across restarts.
- Message read-state requests reject null/empty ID lists and blank entries. Client-sync manifests reject null message/media lists and null nested entries before conversion or storage changes.
- Undefined repository values no longer fall through to AudioProfiles. Null/blank commit content is rejected before repository access. Full repository document validation and concurrent commit handling remain part of the next storage pass.
- Display/audio profile creation and renaming reject null/blank names before filename validation, capture, or mutation.

Regression tests use deterministic task barriers, the production command dispatch method, fake operation services and temporary user storage. They cover duplicate success/conflicts, independent cancellation commands, cancelled waiters/owners, exception/failure cleanup, invalid negotiation on cache hits, malformed nested inputs, unchanged audio/message data, and retained profile names. No installed services, hardware operations or pipe permissions were changed.

Verification: the affected project build passes with warnings treated as errors. The final test_displaymagician.ps1 run passed all four projects: ControlService 219, Gateway 48, UserAgent/desktop 186 and Console 4; 457 passed with zero failures/skips and no build warnings/errors. This batch adds 33 regression cases. Logging audit: zero issues. The first full run passed the first three suites but hit NU1301/SSPI errors restoring Console inside the restricted network; rerunning with normal network access succeeded without changing package sources or verification.

Next batch: repository document validation, revision/commit concurrency and migration failure paths; then the remaining WinForms event/disposal paths and vendor/native/installed Sandbox checks. The entire-codebase review remains ongoing.

## Batch 4: repository commits and migration failures

The user approved stopping migration when an existing marker is unreadable or unsupported, preserving both old and new data until repair.

- Moved display/audio snapshot persistence from the command handler into the owning repositories. Their locks now cover revision comparison, atomic replacement and cache installation, and are shared with runtime edits, saves and loads. Shortcut commits continue using the ShortcutStore lock. Competing commits return one winning snapshot and conflicts for the other writers.
- Added portable repository envelope validation in ConfigurationDefinitions: the correct collection, object entries, nonempty distinct UUID strings. Display/audio commits also deserialize through the existing restricted JSON.NET binder before writing; malformed known property types cannot replace the current file. Unknown fields survive snapshot commits. This is structural/type validation, not validation against live hardware or every native/vendor invariant.
- Reused the Agent's existing atomic writer for snapshot and shortcut migration writes. It flushes before replacement, retains the prior `.bak` and removes abandoned temporary files after failure. Versioned shortcut backups have unique names so a failed migration can be retried immediately. Both shortcut migration stages leave newer schemas untouched.
- Retained validated committed profile definitions in the cache while the normal lazy refresh remains pending. A runtime save arriving first cannot replace the committed profiles with an empty cache.
- The immediate-save regression exposed false audio save failures caused by comparison of reloaded wrapper objects. Display/audio save verification now compares the complete persisted text with the serialized input, including IDs/names and settings. Audio serialization errors stop persistence rather than allowing partially serialized data to replace the file. Display serialization errors already stopped saves.
- Legacy import validates the candidate bytes/JSON (including transformed display-profile structure) before destination replacement. The missing-source branch now handles unreadable/invalid destinations as ordinary migration failures. Retired `.old` files in subdirectories remain backups during retry.
- Concurrent calls on the service's migration runner serialize. A completed marker must parse as supported schema 1 with its timestamp, source location and successful file list. Corrupt, incomplete, future-schema, locked and directory markers stop import, report the marker path and preserve files. No automatic marker deletion, fallback or import replay was added.

Regression coverage uses real temporary files and concurrent task barriers. The first full-script milestone passed 514 tests across all four projects. After adding missing-marker-list coverage and the immediate-runtime-save regressions, the final test_displaymagician.ps1 run passed ControlService 237, Gateway 48, UserAgent/desktop 228 and Console 4: 517 passed, zero failed/skipped and no build warnings/errors. This batch adds 60 regression cases over the preceding 457-test milestone. Affected project builds pass with warnings treated as errors; logging audit reports zero issues. No persisted schema versions/member names changed, and no hardware or installed services were modified.

Next batch: remaining WinForms forms, tray/event refresh and disposal races, plus caller paths that retain mutable profile references across repository changes. Continue deeper native/vendor model validation, diagnostics and installed Sandbox checks afterwards. The entire-codebase review remains ongoing.

## Decisions and next work

The user approved the coordinated SessionLauncher handshake on 27 October 2026. Both launch and stop now carry a ControlService hello requiring protocol-negotiation/session-launcher. The authenticated broker negotiates before dispatch and returns the shared welcome/error metadata; the caller validates successful replies, endpoint kind, frame version/type and request identity. The version-1 envelope/pipe remain unchanged, but both components must be deployed together because unnegotiated successes are rejected. Twenty-four additional regression cases exercise real framing and isolated same-user test pipes with substituted process callbacks. No installed process launch/stop or pipe authentication rules were changed. Full follow-up verification passed: 178 service/broker tests, 47 Gateway tests, 152 Agent/UI tests and 4 Console tests (381 total), with zero failures/skips and no build warnings/errors. This adds 24 protocol regression cases to the preceding 357-test milestone. The affected build also passed with warnings treated as errors; the logging audit reported zero issues.

The deeper service routing/replay pass is recorded in Batch 2 above. Agent validation and in-flight command handling are recorded in Batch 3. Repository commit and migration checks are recorded in Batch 4. Continue with the remaining forms, mutable-profile caller paths and vendor/native integration. Record each subsequent check and test result here rather than resetting the review.

Item 7 from the preceding window review (completed window-handle reservation) remains deferred as agreed.
