# DisplayMagician.UserAgent

This project is the per-user background executor that runs in the interactive Windows session. It receives authenticated commands through the Control Service and performs display and audio changes, shortcut lifecycle and restoration, process/game monitoring, game-library discovery, vendor/native display integration, per-user message storage/sync, and user notifications.

The Agent is the owner of interactive desktop work. It starts its command pipe and restores pending state before it reports itself ready to the Control Service. The service performs machine-wide coordination and routing, but must not take over these responsibilities.

Profile list responses include compact layout thumbnails. A client may request a labelled preview and multi-size desktop `.ico` for one selected profile or a preview for the current layout; the Agent renders these from its authoritative display and saved wallpaper configuration, falling back to the display colour when a saved wallpaper cannot be read. Compact thumbnails are used for list and game shortcut overlays. Shortcut-list icons prefer the user's saved composite image over an executable-derived fallback.

## Development

```powershell
dotnet build .\DisplayMagician.UserAgent\DisplayMagician.UserAgent.csproj
dotnet test .\DisplayMagician.UserAgent.Tests\DisplayMagician.UserAgent.Tests.csproj
```

Run an Agent build only in an interactive Windows session. It needs the installed native/vendor dependencies and the Control Service connection to exercise real operations.

Configuration and recovery data are user scoped. Maintain backward-compatible JSON and explicit migrations when changing persisted models. See the [architecture plan](../DISPLAYMAGICIAN_V4_CONTROL_SERVICE_PLAN.md) for the Agent's ownership boundary.

Saved display-profile wallpaper copies live in the user's `Wallpaper` directory. On profile deletion or replacement, the Agent removes an old copy only after the profile snapshot has saved successfully, only when no profile still references it, and only when the file is directly inside that managed directory. Original wallpaper paths outside DisplayMagician storage are never deleted by this cleanup.

The user-selected Support ZIP includes current user configuration and retained logs, approved pre-v4 configuration backups, legacy/backup logs, and the Control Service's staged machine snapshot. Migration backups are limited to known configuration filenames; staging files, icons, wallpapers, and unrelated files are not collected. `support-manifest.json` records included entries and collection warnings.
