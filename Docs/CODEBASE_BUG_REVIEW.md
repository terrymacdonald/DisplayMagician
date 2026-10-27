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

## Batch 5: display/audio profile forms and stale edit references

- Display/audio profile list refreshes carry a local sequence counter, so an older result cannot replace a newer selection. Empty results clear stale selections; display actions remain disabled for missing/unsaved profiles. Audio refresh consistently selects the requested profile, remaining active profile or first available entry; deletion no longer clears the valid selection established by refresh.
- Added a single action boundary per form to contain asynchronous click-handler failures, restore control state and prevent overlapping commands. The target ID/name is captured before awaits. Error dialogs retain the owning form and are suppressed after closure. Existing destructive confirmations remain in place; operation progress still uses the service/event path.
- Lifetime cancellation covers pending reads/actions, with guards before applying delayed responses. A cancelled close leaves requests usable, disposal is idempotent and detached display-preview images are disposed. Neither form sends a remote undo/cancel just because its window closes; the shared service remains responsible for accepted operation state.
- Display-change RefreshCurrentView calls marshal onto the UI thread, recheck closure when dequeued and invoke refresh directly. They no longer depend on a simulated button click, which could ignore a hidden-window refresh. A new cross-thread regression exposed and verified this case after the first full-run attempt.
- Profile rename/capture/settings methods now resolve the current profile by ID under the existing repository lock. They reject deleted identities before mutation/hardware reads, preserve replacement data when an older object reference was retained and roll back the current definition after a failed save. Delete methods already operated by ID and were checked separately.
- The existing internal desktop pipe client gained a virtual dispatch seam for fake responses. Its production framing, negotiation, request IDs and shared DTOs are unchanged. Both forms keep parameterless Designer constructors; test-only overloads inject the client and error reporter.

Tests use isolated STA message pumps, delayed fake responses, original-ID assertions, temporary JSON and locked-file failures. The affected build passes with warnings treated as errors. The final test_displaymagician.ps1 run passed ControlService 237, Gateway 48, Agent/desktop 255 and Console 4: 544 passed, zero failed/skipped, with no build warnings/errors. The final cross-thread case and the preceding 254 Agent/desktop cases all pass together. Logging audit: zero issues. This batch adds 27 regressions (22 UI and 5 repository cases). No services, hardware settings, persisted schema or transport contract was changed.

Next batch: PairDeviceForm/ServerSettingsForm, shortcut editor/library refresh and disposal paths, then tray background refresh ordering. Pairing QR refreshes and tray lookups need their own out-of-order/closure checks; those forms were inspected but are not covered by this completed profile-form pass. Continue deeper mutable-list readers, native/vendor invariants, diagnostics and installed Sandbox validation afterwards. The entire-codebase review remains ongoing.

## Batch 6: pairing and server-settings dialogs

- Pairing QR, pending-request and paired-client refreshes now carry independent sequence counters. Superseded successes/failures and post-close replies cannot update the current view. Switching endpoints clears the previous QR immediately; already-expired replies are not offered for scanning. QR bitmaps are detached from their decoding stream before display.
- Approve/reject/revoke actions share an in-flight guard and preserve the selected request/device identity. Service/transport failures are contained at the event boundary, closure cancels pending pairing waits and control restoration skips disposed windows. A refreshed list cannot erase a rejected decision's failure message. Revocation also checks stale grid row bounds.
- Server Settings contains initial settings-load failures, prevents saves/pairing until settings have loaded and lets Refresh retry a failed load. Recovery actions remain available when settings/status are unavailable. Loaded settings and service status ignore old replies; background settings loads cannot replace a newer recovery outcome.
- Saves/recovery actions serialize and restore control state after failure. Ports receive the same 1-65535 validation as the service. The saved Public Network baseline comes from the submitted settings, so a later UI change cannot falsely record a different submitted choice. Existing Public Network, revoke/reject, recovery and TLS-key confirmations retain the owning form and administrator paths.
- Gateway restart after settings save uses the existing elevated-action boundary, which now handles UAC cancellation, failed starts and exit failures consistently. Result text preserves the fact that settings were saved even when restart was cancelled/failed. Submitted save/restart and recovery sequences retain their existing completion behaviour if the dialog closes; closed views stop updating. No service authorization or pairing capability policy changed.
- Both forms keep their Designer-backed layout and normal constructors. Internal overloads supply fake clients/confirmations/elevated runners for tests; production still launches the fixed application command via hidden runas/UAC. Extracted the preceding profile-form tests' STA message-pump routine into a shared test helper.

Verification: affected build passes with warnings treated as errors; 63 focused UI tests pass. After the 581-test milestone and four additional result-ordering cases, final test_displaymagician.ps1 verification passed ControlService 237, Gateway 48, Agent/desktop 296 and Console 4: 585 passed, zero failed/skipped, with no build warnings/errors. Logging audit: zero issues. This batch adds 41 regression cases, including actual startup/decision/recovery/save event handlers, delayed replies, close/repeated-dispose handling and unchanged saved-vs-restarted outcomes. Test operations use fake pipe/elevated responses and dummy QR data; no installed service, real pairing, privileged process or hardware operation is used. Installed UAC/service behaviour still requires the later Sandbox pass.

Next batch: shortcut editor/library async refresh/disposal and tray background refresh ordering; then remaining mutable-list readers, native/vendor invariants and diagnostics. The entire-codebase review remains ongoing.

## Batch 7: shortcut forms, loading windows and tray refreshes

The user approved leaving audio unselected when the chosen Audio Profile disappears, requiring another profile or an explicit “Don't change audio settings” choice before saving.

- Game-list refresh updates catalog controls while preserving draft fields and dirty state. Preview/audio reads use generations and lifetime cancellation. Editor actions contain service failures and reject overlap; save remains disabled until initial data/UI population complete.
- Audio refresh resolves the existing selection into the new list and clears missing selections without choosing a replacement. Saves refresh the audio list before committing. Async save-on-close keeps the window/token usable until validation/persistence finish; failed/conflicted saves retain the draft. Existing error forms remain modal and parent-centred.
- Each library edit uses a fresh disposed editor. Debounce timers and preview images are disposed. Splash close requests are remembered before handle creation and run on the splash's UI thread; each splash thread captures its own window. Missing/deleted startup shortcuts cannot enable save.
- Library polling captures shortcut/operation identity, rejects duplicate runs, stops at terminal status before a placement lookup and stops local waits on closure without cancelling accepted shortcuts. Placement-status failures disable capture and log once per failure streak while monitoring continues. A removed selection cannot break run error reporting.
- Cancel/save/clear actions reject duplicates and late results after closure; an old cancellation reply cannot re-enable a newer operation's button. Snapshot results refresh the current selection. Global save avoids confirmation on a disposed owner; owned modal profile/library windows are disposed after use.
- Tray refreshes marshal onto the UI thread, coalesce requests received during lookup and reject superseded/completed-operation replies. Menu rebuilds defer while open; completed capture actions disable immediately. Shutdown cancels waits while minimize-to-tray retains them. Asynchronous profile-cache reads also reject older results.
- Designer-backed layouts, ordinary constructors, shared protocol and persistence schemas are unchanged. Internal injected clients/callbacks support tests; shortcut execution and placement remain UserAgent-owned.

Verification: all 34 focused tests pass; affected builds pass with warnings treated as errors; logging audit reports zero issues. Final test_displaymagician.ps1 verification passed ControlService 237, Gateway 48, Agent/desktop 330 and Console 4: 619 passed, zero failed/skipped, with no build warnings/errors. This adds 34 regression cases over the preceding 585-test milestone. Cases use fake RPC replies, dummy images, isolated/restored caches and hidden STA controls; no actual game, installed service, display/audio operation or persistent user data is changed.

Next batch: deeper desktop repository/cache concurrency and caller-held mutable collections, then game/application catalog identity and refresh semantics. Inspect title-only/ID-only catalog matching across libraries through all callers before changing it. Continue native/vendor invariants, diagnostic collection, UI resource ownership and installed Sandbox validation afterwards. The entire-codebase review remains ongoing.

## Batch 8: desktop cache transactions and shortcut edit conflicts

Reviewed on 28 October 2026. The user clarified that an existing shortcut changed elsewhere should offer reopening the latest version or saving the current draft as a new shortcut. Cancel retains the draft. New shortcuts have no existing edited version to compare.

- Desktop shortcut loads, lookups, mutations and commits now share a transaction lock. A refresh cannot install a list and revision halfway through another commit. AllShortcuts retains its public type but returns a separate list of the current members, so a caller cannot clear the repository or invalidate another reader's enumeration by changing that returned list. ShortcutItem objects remain mutable desktop models; this is not a whole-model immutability conversion.
- Loads parse, link and sort a candidate list before publishing it. Unreadable JSON, missing collections and null shortcut entries cannot erase the last usable cache. Failed reconnects retain the prior working connection and revision. Absent program/image collections receive their existing empty-list defaults instead of remaining null after JSON.NET Populate.
- Commit replies are checked for the correct repository and parsed before advancing the revision/rollback snapshot. Successful commits install the returned authoritative data and separate it from the editor's draft. Failed commits retain the known-good cache and the separate draft. Existing service-side optimistic concurrency and persistence schemas remain unchanged.
- Editors retain an independent original snapshot. An update compares the current shortcut's editable definition and chosen image with that baseline, excluding window-placement captures because those have their own action. Same-shortcut edits/deletions detected after a desktop refresh or a service conflict trigger the user's choice. Changes confined to other shortcuts allow one bounded rebase/retry; a second conflicting commit stops and logs exhaustion. Newer placement captures survive the edit.
- Added the Designer-backed, modal, parent-centred ShortcutConflictForm with Reopen latest version, Save as new shortcut and Cancel. Reopen reads the latest shortcut/dependencies and keeps the editor open; failed reads preserve the draft. Save as new retains draft settings, scoped start-program IDs and placements under a new shortcut UUID/icon path with the existing '(Copy)' naming convention. It leaves the other client's shortcut intact. Save-on-close distinguishes reopening from a successful save.
- Display-profile cache responses and reads now copy mutable views and nested settings. Neither transport-response owners nor readers can alter the cached data through shared objects. Cancelled successful replies cannot publish. Earlier latest-request ordering remains covered.

File/caller inventory for this batch: all methods in DisplayMagician/ShortcutRepository.cs and DesktopProfileViewCache.cs; UserAgentRepositoryConnection.cs; ShortcutItem's editing-copy/configuration conversion, profile linking and relevant collection properties; shortcut editor save/load/close paths; library activation/edit/delete/copy/snapshot paths; cache reads in tray, context menus, validators, hotkey labels and image renderers; Program startup/capture refresh calls. Caller tracing does not mark those entire larger files complete.

Verification: added 21 regression cases in DesktopCacheRepositoryTests and updated a preceding UI test to seed private test state rather than mutate the now-detached public list. Final test_displaymagician.ps1 run passed ControlService 237, Gateway 48, UserAgent/desktop 351 and Console 4: 640 passed, zero failed/skipped, with no build warnings/errors. Tests use temporary icon paths, restored static state/logging, substituted pipe replies, STA controls and bounded synchronization barriers; no installed service or real display/audio/game operation runs. The first restricted-network full run passed the first three suites but Console restore failed with NU1301/SSPI errors and vulnerability-feed warnings. The normal-network rerun succeeded without weakening feeds/auditing. Final affected-project build passed with warnings treated as errors. Logging audit: zero issues; generated report removed. Diff and CRLF checks passed.

Follow-ups: game/application catalog identity and executable-selection callers are next. A deletion caller also exposed removal from the live hotkey lists during foreach in DirectInputManager.RemoveHotkeysByUUID; review that cleanup and polling/binding ownership with regression tests in the remaining desktop/hotkey pass. Remaining mutable ShortcutItem/image ownership and native/vendor, diagnostics, installer/Sandbox and native-client validation stay pending. The whole-app review remains ongoing.

## Decisions and next work

The user approved the coordinated SessionLauncher handshake on 27 October 2026. Both launch and stop now carry a ControlService hello requiring protocol-negotiation/session-launcher. The authenticated broker negotiates before dispatch and returns the shared welcome/error metadata; the caller validates successful replies, endpoint kind, frame version/type and request identity. The version-1 envelope/pipe remain unchanged, but both components must be deployed together because unnegotiated successes are rejected. Twenty-four additional regression cases exercise real framing and isolated same-user test pipes with substituted process callbacks. No installed process launch/stop or pipe authentication rules were changed. Full follow-up verification passed: 178 service/broker tests, 47 Gateway tests, 152 Agent/UI tests and 4 Console tests (381 total), with zero failures/skips and no build warnings/errors. This adds 24 protocol regression cases to the preceding 357-test milestone. The affected build also passed with warnings treated as errors; the logging audit reported zero issues.

The deeper service routing/replay pass is recorded in Batch 2 above. Agent validation and in-flight command handling are recorded in Batch 3. Repository commit and migration checks are recorded in Batch 4. Display/audio profile forms and stale edit references are recorded in Batch 5. Pairing/server-settings checks are recorded in Batch 6. Shortcut forms and tray refreshes are recorded in Batch 7. Desktop cache transactions and shortcut edit conflicts are recorded in Batch 8. Continue with game/application catalog identity, hotkey cleanup and remaining mutable-list/native integration paths. Record each subsequent check and test result here rather than resetting the review.

### Planned review batches

The planned sequence below is retained for continuity. Item 1 received its targeted pass in Batch 8; remaining mutable-model/resource checks stay open. Item 2 is next:

1. Desktop caches and repositories: concurrent refreshes, caller-held mutable objects/collections, lost edits and controls retaining replaced data.
2. Game and application detection: catalog identity across libraries, executable selection, launcher handoffs, and consistency between shortcuts and start programs.
3. Operation events and cancellation: delayed events after completion, operation identity, and cancellation targeting the intended run.
4. Support bundles and diagnostics: collection completeness, redaction, interrupted exports and unavailable component reporting.
5. Installation and service lifecycle: installed startup/shutdown, upgrades and recovery using Windows Sandbox, including SessionLauncher's privileged process-launch path.

For each batch, add meaningful regression tests for confirmed bugs, build affected projects and periodically run all test suites. Ask before changes that materially affect behaviour, data, security, compatibility or scope. Continue the remaining native/vendor invariants and UI resource-ownership checks as relevant to these batches.

SessionLauncher already has validation and protocol tests in DisplayMagician.ControlService.Tests; a separate test project is not required to extend that coverage. Substituted process callbacks do not validate real privileged launch/token handling, so installed Sandbox validation remains pending. The latest completed milestone is 640 passing tests; no code-coverage measurement has been collected, and test counts do not establish completeness.

## Whole-app completion checklist

Status at the end of Batch 8: eight batches completed; the whole-app review remains in progress. The latest full test run passed 640 tests (ControlService 237, Gateway 48, UserAgent/desktop 351, Console 4), with no failures/skips or build warnings/errors. These are recorded results from Batch 8, not a new run for this documentation update. No code-coverage report has been collected.

The five planned batches above are the immediate sequence. The following checklist also tracks deeper work needed before closing the whole-app review. Initial or targeted checks do not mean every file or caller path in a component has been reviewed.

| Area | Evidence so far | Remaining review / validation | Status |
| --- | --- | --- | --- |
| Desktop caches and repositories | Batch 8 transaction, rollback, reader isolation and stale-draft tests | Remaining mutable ShortcutItem/image ownership and broader caller checks | Targeted pass complete; follow-ups pending |
| Game/application detection | Earlier process/window work and existing regression suites | Cross-library catalog identity, executable selection, launcher handoffs, shortcut/start-program consistency and catalog refresh semantics | Next batch |
| Operation events and cancellation | Batch 2 leases/routing; Batch 3 command concurrency; Batch 7 UI polling | Delayed terminal/nonterminal events, cancellation identity, reconnects and multi-client ordering | Planned |
| Support bundles and diagnostics | Initial collection/filter/staging checks and existing tests | All retained component logs, manifest completeness, redaction, unavailable sources, reparse points and interrupted exports | Planned |
| Gateway remote boundary | Initial protocol/paging checks; Batch 2 routing/replay checks | Authentication/authorisation, pairing/revocation races, TLS renewal, replay lifecycle and event-stream disconnects | Deeper pass pending |
| ControlService coordination | Batches 1-4 decisions, leases, replay and persistence | Remaining scheduling, event-journal durability, restart recovery and concurrent machine-state changes | Deeper pass pending |
| UserAgent lifecycle | Batches 3-4 command handling/storage plus earlier lifecycle fixes | End-to-end shortcut execution/restoration, startup recovery, cancellation and interrupted operations | Deeper pass pending |
| Native display/audio integrations | Existing tests and initial failure-path checks | Windows/vendor structures, device removal and failed/partial changes; validate on real hardware | Source and hardware checks pending |
| Contracts and ConfigurationDefinitions | Negotiation/framing checks and Batches 3-4 validation/migrations | Remaining nested payloads, schema/migration chains, consumer agreement and compatibility | Deeper pass pending |
| Desktop forms and resources | Messages form plus Batches 5-7 profile, pairing, server, shortcut and tray work | Remaining forms/callers, dialog ownership, timers, images and native-resource disposal | Partial |
| Console client | Four tests; initial decision/reply fixes | Argument handling, retries/cancellation, error output and reconnect behaviour | Deeper pass pending |
| Kotlin and Swift clients | Generated-contract checks, Kotlin compilation/tests | Connection recovery, cancellation, TLS and secure credential storage; execute Swift tests on macOS/Xcode | Deeper/native checks pending |
| SessionLauncher | Validation and negotiated launch/stop pipe tests in ControlService.Tests | Real privileged tokens, fixed signed payload launch/stop and installed lifecycle in Windows Sandbox | Installed validation pending |
| Build, packaging and identity | Script failure handling and initial installer declarations | Release/package builds, prerequisites, identity registration, script failures and Sandbox install/upgrade/uninstall/recovery | Integration checks pending |

### File-level review ledger

This ledger starts with identifiable files from completed batches and the next batch. It is not yet a complete source-file inventory. At the start of each area, enumerate its production files and relevant scripts/configuration, then add rows or an explicitly listed file group. A file without recorded evidence remains unreviewed for completion tracking. Include tests and generated/native sources in the inventory, identifying generated files and reviewing their generators/contracts rather than claiming a manual review of every generated line.

| File or explicit file group | Recorded review evidence | Remaining work / status |
| --- | --- | --- |
| DisplayMagician/DesktopProfileViewCache.cs | Batches 7-8 ordering, cancellation and view/settings isolation tests | Targeted pass complete; broader UI/native consumers pending |
| DisplayMagician/ShortcutRepository.cs | Batch 8 full-method review; transaction, rollback, snapshot and edit-conflict tests | Targeted pass complete; mutable item/image ownership and hotkey cleanup follow-ups pending |
| DisplayMagician/UIForms/MainForm.cs | Batch 7 tray ordering, lifetime and stale-operation tests | Partial: remaining events and resource ownership |
| DisplayMagician/UIForms/ShortcutForm.cs; ShortcutLibraryForm.cs; ShortcutLoadingForm.cs | Batch 7 draft, disposal, polling and action tests | Batches 7-8 targeted passes complete; catalog identity and remaining resource checks pending |
| DisplayMagician/UIForms/DisplayProfileForm.cs; AudioProfilesForm.cs | Batch 5 refresh/action/lifetime tests | Targeted pass complete; remaining callers included in deeper UI/native checks |
| DisplayMagician/UIForms/PairDeviceForm.cs; ServerSettingsForm.cs | Batch 6 ordering, lifetime and action tests | Targeted pass complete; end-to-end pairing/service validation pending |
| DisplayMagician/UIForms/ShortcutConflictForm.cs; ShortcutConflictForm.Designer.cs | Batch 8 Designer-backed choice dialog and injected-choice workflow tests | Interactive visual/Designer validation pending |
| DisplayMagician/DirectInputManager.cs (RemoveHotkeysByUUID caller only) | Batch 8 deletion trace found live-list removal during foreach | Pending: cleanup regression tests and binding/polling ownership review |
| DisplayMagician/UIForms/MessagesForm.cs | Initial async failure/disposal fixes and tests | Targeted pass complete; remaining integration paths pending |
| DisplayMagician/ConfigMigrationRunner.cs; DisplayMagician.ControlService/UserDataMigrationRunner.cs; LegacyFileMigration.cs | Batch 4 migration validation/failure coverage | Targeted pass complete; remaining migration chains and installed upgrades pending |
| DisplayMagician.UserAgent/Processes/ProcessTreeMonitor.cs; WindowPlacementService.cs | Earlier process/window reviews and existing regression suites | Recheck through game detection, shortcut lifecycle and real-game validation; earlier reviews are not whole-file completion evidence |

For every subsequent ledger entry, record the files and caller paths inspected, findings/fixes or no findings, regression tests and commands/results, and outstanding manual checks. Link evidence to the corresponding batch. Mark an area complete only when its inventory has been accounted for and outstanding checks are completed or explicitly deferred with the user's agreement. Record unavailable hardware/platform validation as pending rather than passed.

- [x] Record completed Batches 1-8 and their verification evidence.
- [x] Record the next five batches and the broader remaining areas.
- [ ] Expand the file ledger as each area is reviewed; reconcile it against the final repository inventory.
- [ ] Collect test-coverage reports where supported and inspect untested critical branches; do not use a percentage as the sole completion criterion.
- [ ] Complete the remaining source, caller-path and regression-test checks above.
- [ ] Complete Windows Sandbox installation/service/broker validation.
- [ ] Complete real display/audio/game validation and macOS/Swift execution, or record explicitly agreed deferrals.
- [ ] Run final builds, relevant contract/generated-client/logging checks and all test suites; record results and any remaining limitations.

Item 7 from the preceding window review (completed window-handle reservation) remains deferred as agreed.
