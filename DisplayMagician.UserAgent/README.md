# DisplayMagician.UserAgent

This project is the per-user background executor that runs in the interactive Windows session. It receives authenticated commands through the Control Service and performs display and audio changes, shortcut lifecycle and restoration, process/game monitoring, game-library discovery, vendor/native display integration, per-user message storage/sync, and user notifications.

The Agent is the owner of interactive desktop work. It starts its command pipe and restores pending state before it reports itself ready to the Control Service. The service performs machine-wide coordination and routing, but must not take over these responsibilities.

For shortcuts with a saved window snapshot, UserAgent tracks windows of the main game/executable and Start Programs launched in that run, then restores their normal bounds, maximized position, and normal/minimized/maximized state after launch. For automatically detected games it attaches the exact newly detected process before starting helper programs, follows that process's descendants, and waits for that selected tree to close instead of older copies. It skips uncertain matches, missing displays, and runs whose display-profile application failed. When multiple launches use the same executable, it prefers the process tree owned by each launch. A placement that does not take effect is retried at most three times, with failed attempts logged. An active run can replace its entire snapshot through the shared service API; already-running Start Programs are excluded. Failed window inspection keeps the existing snapshot. Shortcut file version 8 adds optional maximized coordinates to version 7's stable Start Program IDs and window-placement fields. The Agent backs up and atomically migrates older shortcut files before use.

Automatic game detection compares the PID and start time of processes matching the configured game executable. It ignores processes present at detector startup, retains a newly detected process while another operation is busy, and passes that exact identity to ShortcutRunner. A game library without a usable `.exe` requires the shortcut's Monitor different executable path. Process identities remain in UserAgent memory and are never persisted.

Manual game launches also exclude existing process identities and select the newest observed matching root. The selected tree owns both window placement and shortcut completion, including descendants that survive a launcher. Descendants are discovered before exited parent identities are retired, with creation-time checks to exclude reused PIDs. Start/After Programs check full executable paths, and Stop Programs verify full paths and process identities before stopping anything.

Shortcut JSON uses `MonitorLaunchedExecutable` as its canonical monitoring flag. The existing version 8 migration normalizes the older `ProcessNameToMonitorUsesExecutable` and `MonitorExecutablePath` names, including development files already marked version 8, without increasing the schema version. Existing canonical values take precedence, unrelated fields are retained, and the original file is backed up before replacement.

Process-tree callers retain distinct selection rules:

| Caller | Root selection and lifetime |
| --- | --- |
| Automatically detected game | The exact PID/start-time identity detected before helper programs run, plus descendants. |
| Manually launched game | Discover new matching identities, then select the newest observed root and its descendants. A library client such as Steam is not accepted as the game. |
| Executable / unpackaged application, launched executable selected | Register the actual launch handles and follow only their owned trees. Retained alternative-path text is ignored. |
| Executable / unpackaged application, alternative executable selected | Keep discovery open until a new matching alternative executable appears, including when started by a broker outside the launcher's tree; then select its newest root. |
| Start Program / direct StartAndCapture caller | Register its launch handles and restrict tracking and cleanup to those owned trees. Another helper or user launch of the same executable is excluded. |
| Legacy game-library status monitor | Retain startup discovery behavior; after an explicit tracking failure, status remains conservatively unknown/running rather than claiming the game exited. |

Native snapshots use a collection-time boundary to reject identities born after the snapshot began. When a parent's PID has been reused, child creation times distinguish surviving children of the original parent from children of its replacement. Ambiguous snapshot entries defer parent retirement until a fresh scan can verify the handover.

Failed, empty, incomplete, or unverifiable snapshots do not count as completed discovery. Retry attempts are separated by at least 250 ms and stop after three consecutive failures, with an explicit error log. Active shortcut lifecycle checks report tracking failure through the shared operation status and restore temporary settings, rather than reporting normal process completion. A successful retry clears transient snapshot uncertainty so window capture can resume. Cleanup refreshes the owned tree before selecting processes to stop.

Shortcut recovery record version 2 embeds the original display (including wallpaper) and audio configurations before temporary changes. Restoration uses these snapshots independently of the saved profile collections. Version 1 records continue to recover by their existing profile IDs without being rewritten: the original configuration cannot be reconstructed after a change has already occurred. Missing or unsupported recovery data remains in place and requires recovery rather than silently discarding it. Snapshot serialization excludes rendered icons and cache files.

Profile list responses include compact layout thumbnails. A client may request a labelled preview and multi-size desktop `.ico` for one selected profile or a preview for the current layout; the Agent renders these from its authoritative display and saved wallpaper configuration, falling back to the display colour when a saved wallpaper cannot be read. Compact thumbnails are used for list and game shortcut overlays. Shortcut-list icons prefer the user's saved composite image over an executable-derived fallback.

Successful direct display and audio profile edits return the saved profile ID and change action to ControlService. This lets the service publish precise saved-resource events after the Agent commits the edit. Bulk repository edits are compared by saved ID in ControlService.

Shortcut startup refreshes current display/audio state before deciding whether a profile is already active. A shortcut is successful only after its required restoration succeeds; incomplete restoration retains its recovery file and reports `RecoveryRequired`. Display/audio profile edits propagate write failures and roll back their in-memory changes. Icon deletion occurs after the profile collection is saved.

Direct audio, display, and shortcut execution share Agent admission checks. Active work prevents an idle stop and overlapping execution; once stopping is accepted, new operations are rejected. On a new Agent process, startup recovery resolves previously persisted active operations as interrupted failures. A Control Service reconnect preserves operations still executing in the same Agent process.

MessageSyncService owns message-index writes. Sync downloads are serialized, while final metadata commits merge the latest read/unread states under the same lock used for read-state edits. Downloads do not block a user from marking messages read or unread. Existing persisted schemas and transport contracts are unchanged.

## Development

Message read-state changes and synchronization report index-write failures instead of reporting an unsaved change as successful. Process launch logs include the executable and process ID, with raw command arguments omitted from support logs.

```powershell
dotnet build .\DisplayMagician.UserAgent\DisplayMagician.UserAgent.csproj
dotnet test .\DisplayMagician.UserAgent.Tests\DisplayMagician.UserAgent.Tests.csproj
```

Run an Agent build only in an interactive Windows session. It needs the installed native/vendor dependencies and the Control Service connection to exercise real operations.
Windows shortcut creation and inspection use the registered `WScript.Shell` COM object at runtime, so `dotnet build` does not need to resolve a COM type library.

Configuration and recovery data are user scoped. Maintain backward-compatible JSON and explicit migrations when changing persisted models. See the [architecture plan](../DISPLAYMAGICIAN_V4_CONTROL_SERVICE_PLAN.md) for the Agent's ownership boundary.

Saved display-profile wallpaper copies live in the user's `Wallpaper` directory. On profile deletion or replacement, the Agent removes an old copy only after the profile snapshot has saved successfully, only when no profile still references it, and only when the file is directly inside that managed directory. Original wallpaper paths outside DisplayMagician storage are never deleted by this cleanup.

The user-selected Support ZIP includes current user configuration and retained logs, approved pre-v4 configuration backups, legacy/backup logs, and the Control Service's staged machine snapshot. Migration backups are limited to known configuration filenames; staging files, icons, wallpapers, and unrelated files are not collected. `support-manifest.json` records included entries and collection warnings.
