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

WinForms retains a failed event-refresh request and retries up to three total attempts with short coalesced delays. Exhaustion is logged; the next user/event trigger can try again without an endless retry loop. Windows notifications do not cover every driver setting. They improve client freshness but are never the authority for deciding that an apply can be skipped. Gamma, gamut and other vendor-only changes are discovered at the next explicit capture or operation even without a Windows event. With WinForms closed, UserAgent still performs fresh operation reads and attempts an initial capture before registering ready.

A successful capture publishes layout and connected identifiers together in one DisplayDetectionSnapshot. A client response pins that frame for current-layout and missing-display diagnostics, including the data-only Intel hidden-monitor calculation. A failed capture or whole connectivity-enumeration failure leaves the previous completed frame available. An explicit fresh-read request reports failure rather than presenting that frame as freshly captured. Failed initial capture does not prevent UserAgent command availability.

Saved model constructors, JSON loading, validation and preview rendering use stored data only. In particular, GetLibrary and IsInstalled can initialize native APIs, so they are unsuitable for those paths. Native library fields are refreshed under hardware coordination; partially read configurations are not published as successful state.

## Coordination and responsiveness

Display and audio hardware access uses one re-entrant synchronous monitor. Audio capture, endpoint availability checks, profile application and volume operations share DisplayHardwareAccess; client audio list/create/update captures use the same bounded queue as display captures. A read cannot observe the middle of a native display/audio phase. Original recovery capture is taken before changes; selected audio discovery/waiting/application stays after display completion. Acquire it before the repository metadata lock whenever both are required. Definition reads/edits use the metadata lock without acquiring hardware access. Never retain hardware access across await or for a game's lifetime.

An apply uses a private configuration copy, keeping recreated handles through its existing repeated attempts. Saved definitions and published snapshots do not share that mutable working copy. Adapter remapping uses a fresh Windows adapter map within the hardware phase. The adapter dictionary is rebuilt from original entries so ID swaps/chains stay consistent with native paths/modes and later repeats; existing unmatched-adapter fallbacks remain. Capturing over an existing definition uses a private candidate, then checks for a concurrent edit before committing. Apply count and delay are user preferences: capture preserves them, while new profiles receive their constructor defaults.

Existing competing-apply admission still returns busy; this is not a new FIFO apply queue or display lease. Native capture requests use fewer than the four pipe request slots so cancellation/status traffic retains capacity. Queued captures and applies can be cancelled while waiting for hardware access. Original shortcut capture also receives the operation token. Apply checks cancellation before native changes and between attempts; its between-attempt pause is cancellable. Native settling waits and restoration tokens retain their existing policy. Cancellation cannot forcibly interrupt a driver call already executing.

The monitor coordinates DM's own operations. It cannot prevent Windows, a vendor control panel or another program from changing hardware during a multi-API read. Fresh boundary reads and existing verification reduce that risk; they do not create an atomic operating-system transaction.

## Results and native compatibility

Missing/empty per-source DPI lists and rejected DPI setters remain scaling warnings; they do not re-index missing entries from error logging or prevent later sources/HDR work.

Required topology, enabled displays, vendor grouping, resolution, rotation and position failures are fatal. Shortcut execution uses the existing Continue/StopAndRestore decision and skips saved window placement for that run. Refresh rate, HDR, scaling and optional vendor overrides report warnings without blocking normal continuation/placement. Restoration failures warn and still allow audio/volume restoration under the existing recovery policy.

Preserve vendor order, wake-up/enable-all steps, native structure layout, fallback techniques and configured delays. Honour wrapper boolean results: a returned false must not produce a success log. Intel Combined Display checks saved children before disabling a working group, checks disable/enable results and requires usable enable readback. Dimension differences before the Windows stage remain warnings; Windows retains geometry verification. Optional Intel setter rejections retain their display/adapter identity in structured diagnostics.

Unexpected Intel Combined Display/inventory read exceptions fail capture instead of publishing missing/default topology. Known unsupported Combined Display returns remain normal, and optional feature reads retain their existing handling.

Intel connectivity IDs include the physical adapter key before the final encoder number. Hidden-output matching uses adapter/encoder pairs, including valid output zero. Windows logical-monitor warnings are suppressed only when their adapter model maps to one physical Intel card; ambiguous identical-card paths retain the warning.

Top-level Eyefinity, Surround and Combined Display flags mean the configuration contains that grouping on at least one adapter. Per-adapter settings still determine which card is changed.

## Windows validation and bounded compatibility recovery

Windows layout execution is shared with callback tests in Windows/WinLibrary.Configuration.cs. The public WinLibrary APIs remain; copy both partial files when testing in VideoInfo. Standalone IsValidConfig calls only the shared validation routine and never applies a layout.

| Stage | Behaviour |
| --- | --- |
| Full-layout validation | Preserve the supplied format. Use virtual-mode awareness for virtual paths; Windows 11 also starts with refresh-rate awareness. |
| Refresh-awareness fallback | On ERROR_INVALID_PARAMETER or ERROR_NOT_SUPPORTED, remove only SDC_VIRTUAL_REFRESH_RATE_AWARE. Keep SDC_VIRTUAL_MODE_AWARE and the same saved paths/modes. |
| Two full-layout apply attempts | Retain the existing first attempt and second attempt after 2 x the native delay. A refresh-awareness compatibility retry retains virtual-mode awareness. |
| Modern topology-only attempt | After another 3 x the native delay, use the existing temporary topology helper with clone membership preserved and no mode array. Retain its refresh-awareness compatibility retry. |
| One final legacy route | On the same parameter/support errors, after failed validation or exhausted modern attempts, read fresh legacy routes, build a faithful topology-only request, validate once and apply once. Skip this additional route if an equivalent legacy topology attempt already ran. |
| Verification | Continue the existing DPI/HDR/vendor overrides and layout verification. Accepted native calls are still subject to existing required-layout versus optional-setting severity. |

Every successful native apply retains the existing settling delay. Native vendor sequencing, wake-up steps, outer ApplyProfileCount/ApplyProfileDelay and restoration policy remain. Validation access/session errors and other non-compatibility validation failures do not start an apply; the additional legacy route is limited to the two compatibility codes. There is no unbounded retry loop.

The fallback obtains raw routes using the existing QDC_ALL_PATHS reader without virtual-awareness flags, before capture normalisation changes any IDs. This additional read occurs only if the final route is needed, after earlier calls may have changed routing. A failed read reports why recovery is unavailable. The normal pre-apply all-path read remains.

DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE is reporting-only; conversion never clears it by hand. Instead, select Windows-provided nonvirtual routes for the exact saved adapter/target identities. Paths in a saved clone group need a common valid legacy source; extended groups need distinct adapter/source pairs. Matching may reassign available sources to preserve all groups rather than greedily merging them. Missing targets, unavailable/virtual-only routes, unknown grouping or unrepresentable cross-adapter clones skip this final fallback with diagnostics. Saved profiles and returned inventory arrays remain intact; each validation/apply receives a temporary copy. Active target order is preserved and inactive paths are not enabled.

The legacy request has invalid source/target mode indexes, no mode array and neither awareness flag. Its validation/apply differ only by SDC_VALIDATE/SDC_APPLY. A validation failure prevents the legacy apply, and a legacy apply failure ends this route. Logs identify validation, full attempts, modern topology recovery, conversion failures and final legacy results with native status/flags.

If legacy recovery selects different source IDs, DPI writes follow uniquely matched converted targets. Shared clone sources are written once; conflicting saved scaling or ambiguous targets produce scaling warnings. Unsupported/uncaptured zero DPI does not override another member's captured preference. These mappings do not modify stored settings.

Microsoft references: [SetDisplayConfig](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setdisplayconfig), [path flags](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-displayconfig_path_info), [source fields](https://learn.microsoft.com/en-us/windows/win32/api/wingdi/ns-wingdi-displayconfig_path_source_info).

## Intel identity migration

DisplayProfiles.json version 6 retains the version-5 PCI adapter/display keys and adds adapter-scoped connectivity identifiers and a persistent IdentityRequiresRecapture flag. It keys Intel adapters by the saved model tuple plus PCI bus/device/function. Display keys include that adapter identity plus encoder ID, so identical cards using the same encoder number remain distinct. Driver handles and enumeration indexes are not persisted as the identity. This identifies a PCI location rather than a card serial number: changing slots, firmware topology or hardware can require recapture.

UserAgent's Runtime/Display/ConfigMigrationRunner upgrades supported earlier repositories (including version 5) before exposing them. It changes identity fields using saved AdapterProperties.AdapterBdf, retains unrelated/unknown JSON fields, creates a separate migration backup, and uses AtomicFile replacement. Repeated reads do not rewrite an already migrated file. Unsupported versions and migration collisions are rejected without replacing the source or its existing backup. Repository commits and saved recovery snapshots use the same identity conversion; recovery does not need hardware to deserialize.

If a legacy Intel profile has no saved PCI location, its original keys/settings remain in the migrated file and IdentityRequiresRecapture stays true through repository and recovery serialization. Defaulted zero-valued properties cannot become a captured location. Already captured PCI keys remain authoritative when optional property data is absent/defaulted. Applying it reports a fatal AdapterIdentity result requiring recapture before any display writes. Terry explicitly chose recapture rather than matching by model or guessing between identical cards. Old connectivity identifiers migrate only when their saved adapter matches uniquely; ambiguity also requires recapture. Recapturing overwrites those legacy keys with current per-card identities.

## Validation

Automated tests use supplied snapshots and native callbacks to exercise serialization, isolation, cancellation, failed enumeration, rejected Intel writes, Combined Display sequencing, migration preservation and identity collisions. They do not write live display settings.

Terry's consolidated VideoInfo/installed checks remain required: ordinary/grouped transitions, multiple/identical adapters, missing displays, vendor-only changes, simultaneous clients, repeated applies and recovery, mixed DPI, Windows fallback paths and real-driver readback timing. Copy IntelLibrary.cs, IntelLibrary.Configuration.cs, IntelAdapterIdentity.cs and IntelDisplayTopology.cs together with their diagnostics/snapshot dependencies when testing the Intel changes in VideoInfo. Record OS/driver/wrapper versions and timings. See CODEBASE_BUG_REVIEW.md for the checklist and test evidence.
