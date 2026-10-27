# DisplayMagician desktop application

This project is the designer-backed WinForms desktop application. It provides the main window, system-tray experience, profile and shortcut editors, notifications, and support-ZIP export.

The desktop application is a client of `DisplayMagician.ControlService`; it must not directly perform Agent-owned display, audio, game, process, or authoritative per-user storage work. Shared transport types come from `DisplayMagician.Contracts`, while portable shortcut definitions come from `DisplayMagician.ConfigurationDefinitions`.

While an eligible shortcut runs, Save Window Positions is available beside Cancel in the Shortcut Library, in its running notification, and in the system tray menu. The tray menu uses cached session state and refreshes it asynchronously so opening the menu does not wait for a service response. Saving replaces the shortcut's previous complete window snapshot after confirmation. A confirmed Clear Saved Window Positions action in the Shortcut Library context menu removes an existing snapshot. The desktop shows one short restoration summary with the count and program names; per-window details go to Agent logs. The Windows desktop background context menu remains unchanged.

The game shortcut editor accepts an actual `.exe` for Monitor different executable. Game launches and automatic game detection require this path when the game library cannot provide a usable executable path. Previously saved scripts or links remain in the shortcut data and show a validation warning until changed.

Executable and unpackaged application shortcuts wait for whichever executable the user selected for monitoring. Selecting the launched executable ignores retained alternative-path text. Selecting an alternative waits for a new matching process, including broker launches, and then follows that selected process tree. Existing matching processes do not keep the shortcut running after its selected instance closes.

The desktop shortcut model and saved JSON use `MonitorLaunchedExecutable`. The existing legacy configuration migration converts the older flag name, and UserAgent's existing version 8 shortcut migration completes normalization without a new schema version.

Shortcut editors work on a separate draft and close successfully only after the service accepts the save. A failed save keeps the editor open with its proposed changes and restores the library cache to its authoritative snapshot. Revision conflicts refresh the library and tell the user to retry. Creation, copying, and deletion propagate commit failures, and local icon/hotkey cleanup occurs only after a successful commit. Retrying an edit preserves window snapshots captured or cleared through their separate actions.

The pairing and server-settings forms show the local/routed-network workflow for the first REST release. Their Remote Network controls remain in the Designer and code but are hidden until user-enabled Internet access is supported.
Server Settings can also allow paired devices on Windows Public networks after administrator approval and a confirmation. This changes the Gateway firewall rule for Public networks; it does not configure router port forwarding.
Server Settings also has an administrator-confirmed TLS key replacement action for identity recovery. The Gateway restarts and all paired devices must pair again after its TLS pin changes.

## Development

The Messages form retains a usable window when initial service loading fails and handles failed read/unread actions without an unhandled asynchronous event exception. Its fake-client regression tests live in `DisplayMagician.UserAgent.Tests` alongside the desktop repository tests.

Build the project from the repository root:

```powershell
dotnet build .\DisplayMagician\DisplayMagician.csproj
```

The desktop project has no build-time COM references; its shell shortcut calls use Windows COM at runtime.

Run the project from Visual Studio in an interactive Windows session. The forms are designer-backed: change static layout in `UIForms\*.Designer.cs` and behaviour in the matching form file.

The Display Profile and Game Shortcut forms request a detailed layout image for the selected profile; ordinary profile lists use compact thumbnails. The detailed image includes saved per-monitor wallpapers when the profile applies them. Profile desktop shortcut icons use the multi-size `.ico` rendered by UserAgent (with a compact-thumbnail fallback for older Agents); game desktop shortcuts use the saved game-and-layout composite. These `.ico` files are kept under the current user's LocalAppData `DisplayMagician\Icons` folder so Explorer can read them after restarts and upgrades.

For a complete installable build, use `build_displaymagician.ps1` from the repository root rather than treating this project output as an installer.

Display/audio profile forms ignore superseded refresh replies, cancel pending waits when closed or disposed, and contain service failures at their asynchronous action boundary. Each form prevents overlapping actions and retains the original target ID while awaiting replies. Buttons reflect the selected profile and audio access state. Display-change refreshes marshal to the UI thread and also work when the window handle exists but its controls are hidden. Forms remain backed by the existing Designer files and use the shared Control Service contracts; the injectable internal client/error reporter support isolated UI tests.

Pairing QR/list refreshes and server-status/settings reads also ignore stale or post-close results. Changing the pairing endpoint clears the previous QR immediately. Pairing decisions, revocation, settings saves and recovery actions reject overlapping submissions. Gateway settings must load before saving; Refresh retries a failed initial load without reloading an existing draft. Emergency recovery remains available if settings cannot load. An already-submitted settings save/restart retains its existing completion behaviour after closure, with UI updates suppressed. Restart/UAC failures distinguish saved settings from a failed or cancelled restart. Service authorization, pairing grants and administrator confirmations remain unchanged.

Shortcut editor metadata refreshes preserve unsaved drafts and reject obsolete preview/audio replies. Saving refreshes the chosen Audio Profile first; a deleted profile stays unselected until another is chosen or the user explicitly selects “Don't change audio settings”. Failed validation/commits keep save-on-close open. Each library edit uses a fresh disposed editor, and loading-window close requests work before handle creation. Library closure stops local polling rather than cancelling accepted shortcuts; placement-status failures do not stop shortcut monitoring. Snapshot and cancellation actions reject duplicate clicks.

Tray refreshes marshal to the UI thread, coalesce follow-up requests and reject superseded/completed-operation replies. Menu rebuilds defer while open, and asynchronous profile-view refreshes retain the newest result. Modal profile/library windows are disposed after use; minimize-to-tray keeps refreshes available.

## Related components

- `DisplayMagician.ControlService` routes desktop requests and owns machine-wide coordination.
- `DisplayMagician.UserAgent` executes interactive-session display, audio, and shortcut work.
- `DisplayMagicianConsole` offers the same local service through a command-line interface.

See the [repository README](../README.md) for product usage and the [control-service plan](../DISPLAYMAGICIAN_V4_CONTROL_SERVICE_PLAN.md) for the component architecture.

Desktop shortcut-cache reads and commits share a transaction lock. List readers receive a stable membership snapshot; editors continue using separate drafts. Refreshes publish only fully parsed/linked data, failed reconnects retain the previous connection, and successful commits install the returned authoritative snapshot. Missing program collections load as empty lists. Display-profile views and nested settings are copied on cache entry and read, and cancelled refresh replies cannot publish.

When an existing shortcut changes or is deleted elsewhere, saving its draft offers Reopen latest version, Save as new shortcut, or Cancel. Reopen replaces the draft with the latest data and keeps the editor open; Save as new preserves it under a new UUID and icon path with a (Copy) name. Unrelated shortcut changes permit one bounded retry; a newer window-position capture is preserved without treating it as an editor conflict.

The game editor uses library plus ID for row clicks, saved selections, validation and commits, including renamed titles. Refresh games list requests a UserAgent rescan while preserving the draft. Missing usable game executable paths require Monitor different executable. The application picker uses each row identity and its library, supports packaged-app preselection without an executable path, ignores late replies after closure, and remains open when file selection is cancelled. Hotkey removal handles all matching keyboard/joystick entries, retains unrelated entries, and reports settings-write failure. Joystick labels use stored button data even for disconnected devices; input bindings own their key/button lists and polling reads snapshots.
