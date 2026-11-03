# Display detection and coordination

UserAgent owns interactive display hardware access. WinForms, Console and authorised integrations request work through ControlService using the shared Contracts protocol. ControlService admits/routes display operations; UserAgent serializes their synchronous hardware phases through DisplayHardwareAccess. No desktop client initializes the display libraries.

## Fresh decisions and completed snapshots

| Caller/action | Source of display state |
| --- | --- |
| Ordinary profile lists, menus and previews | Last completed layout/connectivity snapshot and saved definitions |
| View current; explicit ProfileListRequest.RefreshCurrentLayout | Coordinated fresh hardware capture |
| Windows display-change notification | Hint that requests an explicit refresh through ControlService |
| Capture/create/overwrite a saved layout; original-state recovery capture | Coordinated fresh hardware capture |
| Apply/restore, including configured repeated attempts | Fresh library reads before eligibility/comparison; existing post-apply verification |

Windows notifications do not cover every driver setting. They improve client freshness but are never the authority for deciding that an apply can be skipped. Gamma, gamut and other vendor-only changes are discovered at the next explicit capture or operation even without a Windows event. With WinForms closed, UserAgent still performs fresh operation reads and attempts an initial capture before registering ready.

A successful capture publishes layout and connected identifiers together in one DisplayDetectionSnapshot. A client response pins that frame for current-layout and missing-display diagnostics, including the data-only Intel hidden-monitor calculation. A failed capture or whole connectivity-enumeration failure leaves the previous completed frame available. An explicit fresh-read request reports failure rather than presenting that frame as freshly captured. Failed initial capture does not prevent UserAgent command availability.

Saved model constructors, JSON loading, validation and preview rendering use stored data only. In particular, GetLibrary and IsInstalled can initialize native APIs, so they are unsuitable for those paths. Native library fields are refreshed under hardware coordination; partially read configurations are not published as successful state.

## Coordination and responsiveness

Hardware access uses one re-entrant synchronous monitor. Acquire it before the repository metadata lock whenever both are required. Definition reads/edits use the metadata lock without acquiring hardware access. Never retain hardware access across await or for a game's lifetime.

An apply uses a private configuration copy, keeping recreated handles through its existing repeated attempts. Saved definitions and published snapshots do not share that mutable working copy. Adapter remapping uses a fresh Windows adapter map within the hardware phase. Capturing over an existing definition uses a private candidate, then checks for a concurrent edit before committing.

Existing competing-apply admission still returns busy; this is not a new FIFO apply queue or display lease. Native capture requests use fewer than the four pipe request slots so cancellation/status traffic retains capacity. A queued capture can be cancelled while waiting for hardware access. Cancellation cannot forcibly interrupt a driver call already executing.

The monitor coordinates DM's own operations. It cannot prevent Windows, a vendor control panel or another program from changing hardware during a multi-API read. Fresh boundary reads and existing verification reduce that risk; they do not create an atomic operating-system transaction.

## Results and native compatibility

Required topology, enabled displays, vendor grouping, resolution, rotation and position failures are fatal. Shortcut execution uses the existing Continue/StopAndRestore decision and skips saved window placement for that run. Refresh rate, HDR, scaling and optional vendor overrides report warnings without blocking normal continuation/placement. Restoration failures warn and still allow audio/volume restoration under the existing recovery policy.

Preserve vendor order, wake-up/enable-all steps, native structure layout, fallback techniques and configured delays. Honour wrapper boolean results: a returned false must not produce a success log. Intel Combined Display checks saved children before disabling a working group, checks disable/enable results and requires usable enable readback. Dimension differences before the Windows stage remain warnings; Windows retains geometry verification. Optional Intel setter rejections retain their display/adapter identity in structured diagnostics.

Top-level Eyefinity, Surround and Combined Display flags mean the configuration contains that grouping on at least one adapter. Per-adapter settings still determine which card is changed.

## Intel identity migration

DisplayProfiles.json version 5 keys Intel adapters by the saved model tuple plus PCI bus/device/function. Display keys include that adapter identity plus encoder ID, so identical cards using the same encoder number remain distinct. Driver handles and enumeration indexes are not persisted as the identity. This identifies a PCI location rather than a card serial number: changing slots, firmware topology or hardware can require recapture.

UserAgent's Runtime/Display/ConfigMigrationRunner upgrades supported earlier repositories before exposing them. It changes identity fields using saved AdapterProperties.AdapterBdf, retains unrelated/unknown JSON fields, creates a separate migration backup, and uses AtomicFile replacement. Repeated reads do not rewrite an already migrated file. Unsupported versions and migration collisions are rejected without replacing the source or its existing backup. Repository commits and saved recovery snapshots use the same identity conversion; recovery does not need hardware to deserialize.

If a legacy Intel profile has no saved PCI location, its original keys/settings remain in the migrated file. Applying it reports a fatal AdapterIdentity result requiring recapture before any display writes. Terry explicitly chose recapture rather than matching by model or guessing between identical cards. Recapturing overwrites those legacy keys with current per-card identities.

## Validation

Automated tests use supplied snapshots and native callbacks to exercise serialization, isolation, cancellation, failed enumeration, rejected Intel writes, Combined Display sequencing, migration preservation and identity collisions. They do not write live display settings.

Terry's consolidated VideoInfo/installed checks remain required: ordinary/grouped transitions, multiple/identical adapters, missing displays, vendor-only changes, simultaneous clients, repeated applies and recovery, mixed DPI, Windows fallback paths and real-driver readback timing. Copy IntelLibrary.cs, IntelLibrary.Configuration.cs, IntelAdapterIdentity.cs and IntelDisplayTopology.cs together with their diagnostics/snapshot dependencies when testing the Intel changes in VideoInfo. Record OS/driver/wrapper versions and timings. See CODEBASE_BUG_REVIEW.md for the checklist and test evidence.
