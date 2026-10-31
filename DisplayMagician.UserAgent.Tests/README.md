# DisplayMagician.UserAgent.Tests

This xUnit test project verifies User Agent behaviour and the shared contracts it exercises. Coverage includes Agent command pipes and identity, shortcut execution and recovery, process monitoring, game detection, repository connections, serialization, support bundles, logging layout, retry policy, and build-version consistency.

Agent reliability regressions cover stale cached profile identities, failed restoration results, recovery-file preservation, profile write rollback, audio operation admission and idle-stop protection, interrupted-operation reconciliation, and read/unread edits during message downloads. Failure and concurrency tests use temporary storage, controlled HTTP responses, and substituted execution services without changing actual display or audio settings.

## Run

Additional regression cases cover locked message-index writes, omission of launch arguments from logs, and the desktop Messages form's disconnected-service, timeout, and rejected-request paths. Form tests use an injected client and do not connect to installed services or initialize WebView2.

```powershell
dotnet test .\DisplayMagician.UserAgent.Tests\DisplayMagician.UserAgent.Tests.csproj
```

Tests must not require a real display topology, audio device, game installation, or installed service. Add tests alongside changes to User Agent-owned behaviour.

Shortcut/tray UI cases cover coalesced completion lookups, cache ordering, cross-thread calls before/after handle creation, polling/placement failures, closure during start/status/save, duplicate cancel/clear/save, preservation of selection/drafts, audio deletion/pre-save validation, failed async save-on-close and loading-window closure before handle creation. Desktop caches are isolated/restored; tests use the STA runner, fake service replies and dummy images, without starting games or installed services.

Gateway dialog regressions exercise QR endpoint races, expired replies, stale pairing lists/status/settings, closure, initial-load failures, duplicate decisions/revocations/recovery, port validation, rejected confirmations and settings-save/UAC outcomes. A shared STA message-pump runner executes the real event handlers with delayed fake pipe responses and a substituted elevated-process runner. No privileged process is launched or real pairing data used. Follow-up reads must preserve newer recovery outcomes and rejected pairing decisions.

Profile form concurrency tests run on isolated STA threads with a WinForms message pump and delayed fake Control Service responses. They cover out-of-order selection replies, disposed/cancelled-close handling, service errors, repeated apply clicks, original target IDs, empty/unsaved selection controls, remaining active audio selection and refresh calls from another thread. No installed pipe connections or interactive dialogs are used. Repository tests cover edits through stale/deleted references, preservation of newer fields and rollback after a locked save fails.

Repository regressions cover malformed envelopes/entries, duplicate IDs, typed profile errors, competing commits with one winner, exact conflict snapshots, previous-file backups, locked replacement cleanup/retry and an immediate runtime save after a profile commit. Shortcut migration tests preserve newer schemas and verify immediate retry after a locked destination without leaking temporary files. All storage is disposable test data.

Command-dispatch regressions use deterministic task barriers to verify single execution for overlapping duplicate IDs, changed-payload/type conflicts, independent cancellation commands, cancelled waiters/owners, failure cleanup and protocol validation before cached replies. Handler tests reject null message/media collections, undefined repository kinds, null repository content and blank profile names without changing temporary stored data. These checks use the production dispatch path with fake operation services and do not change the pipe ACL or start an installed Agent.

Regression cases cover full-path process stopping with executable name collisions, newest-root selection, launcher handover beyond the discovery timeout, independent recovery snapshots and legacy recovery records, and desktop shortcut commit failures/draft isolation. Process tests launch only disposable test executables and clean up their own process trees.

Monitor regressions additionally verify helper ownership/cleanup isolation, stale child and reused parent PIDs, deferred handover, bounded failed/incomplete snapshot retries, recovery after a successful retry, and legacy game status after tracking failure. Executable shortcut integration tests verify ordinary monitoring with stale alternative-path text and broker-started alternative targets while old matching processes remain alive. Both JSON serializers and the desktop model are checked for the canonical `MonitorLaunchedExecutable` field. Migration tests verify renaming both old field names, preservation of true/false selections and unrelated settings, backups, and idempotence.

ProgramDelayTests covers schema 9 zero defaults and existing-value preservation, legacy collection migration, integer/range validation and safe commit rejection, copies and portable conversion, Designer controls, measured waits after successful final Start/Stop actions, cancellable cleanup, skipped/disabled/failed actions, and inactive future-use After Program delays.

DesktopCacheRepositoryTests exercises unreadable shortcut refreshes, failed reconnects, invalid commit replies, retained rollback revisions/drafts, authoritative committed snapshots, reader isolation, refresh/commit serialization, missing program collections and cancelled profile reads. Tests restore static state and logging configuration and use isolated temporary icon paths; they do not contact installed services or operate hardware.

Shortcut conflict regressions cover changes/deletions after background refresh, unrelated-change rebasing and retry exhaustion, preserving newer window captures, cancel/save-as-new/reopen choices, retained start-program/placement identities, and failed reopen reads. Choice callbacks avoid interactive dialogs while exercising the production editor persistence path.

CatalogueReviewTests adds regression coverage for colliding game/application titles and IDs across libraries, saved-game selection after renames/removal, exact runner lookup, explicit versus legacy cached game-list requests, malformed requests, scan rollback/publication/serialization, partial first-start results, monitor-path validation, picker cancellation/closure, and hotkey removal/persistence/list ownership. Provider scans, pipe replies and input factories are substituted; temporary stores/settings are restored and no game is launched or input device is required. The test project references the existing ImageListViewCore DLL to exercise actual list-row events.

OperationDeliveryOrderingTests covers overlapping status flush/reconciliation, stable update-ID retries, enqueue ordering, cancelled gate waiters, stale cancellation IDs, terminal event guards and retention, service-instance cursor resets, resolved/expired decisions, reconnect dialog reconciliation, failed reads and deadline closure. Tests substitute service/pipe replies and use restored repository state, temporary outboxes and STA controls; no game, installed service or hardware operation is required.

OperationOutboxPersistenceTests covers corrupt/unsupported/incomplete outboxes, invalid/duplicate/conflicting delivery identities, preserved backups and directory targets, in-memory degraded queues, acknowledgement-write rollback, idempotent enqueue and service response classification. Integrated fake-transport tests use the real service store/outbox and file locks to verify continued terminal delivery plus retained ordered retries across service restart. Repository static paths are restored and all files live in disposable temporary directories.

SupportBundleExportSafetyTests covers interrupted collection/publication, preservation of existing exports, exclusion of the destination and temporary archive, manifest byte lengths, external/cyclic NTFS junctions, linked machine-staging ancestors and unreadable-folder warnings. It uses owned temporary directories, file locks and a directory ACL restored before cleanup; it does not contact installed services.

SharedLoggingClientTests verifies common desktop/Agent requests, snapshot validation and cached expiry. SharedLoggingSettingsFormTests uses injected pipe replies and hidden STA controls to verify failed saves retain the draft, repeated close attempts issue one request and disposed forms ignore late replies. The old independent per-user logging reader/tests are replaced by the authoritative shared mechanism.

PreferencesConsolidationTests covers Settings v7-to-v8 donation migration/backups/idempotency, missing-primary and invalid-source cases, primary-only donation writes, one-time injected updater import, concurrent updater changes, failed-save rollback, atomic reminder/skip updates and preservation of unknown settings. Tests isolate static desktop paths/owners and do not read actual registry preferences.


ProgramDelayTests allows 20 ms Windows timer precision when observing a requested one-second Task.Delay with Stopwatch; skipped waits, step ordering, cancellation and cleanup remain asserted. Runtime delay behaviour is unchanged.
