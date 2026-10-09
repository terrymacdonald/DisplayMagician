# DisplayMagician desktop application

This project is the designer-backed WinForms desktop application. It provides the main window, system-tray experience, profile and shortcut editors, notifications, and support-ZIP export.

The desktop application is a client of `DisplayMagician.ControlService`; it must not directly perform Agent-owned display, audio, game, process, or authoritative per-user storage work. Shared transport types come from `DisplayMagician.Contracts`, while portable shortcut definitions come from `DisplayMagician.ConfigurationDefinitions`.

The pairing and server-settings forms show the local/routed-network workflow for the first REST release. Their Remote Network controls remain in the Designer and code but are hidden until user-enabled Internet access is supported.
Server Settings can also allow paired devices on Windows Public networks after administrator approval and a confirmation. This changes the Gateway firewall rule for Public networks; it does not configure router port forwarding.
Server Settings also has an administrator-confirmed TLS key replacement action for identity recovery. The Gateway restarts and all paired devices must pair again after its TLS pin changes.

## Development

Build the project from the repository root:

```powershell
dotnet build .\DisplayMagician\DisplayMagician.csproj
```

Run the project from Visual Studio in an interactive Windows session. The forms are designer-backed: change static layout in `UIForms\*.Designer.cs` and behaviour in the matching form file.

The Display Profile and Game Shortcut forms request a detailed layout image for the selected profile; ordinary profile lists use compact thumbnails. The detailed image includes saved per-monitor wallpapers when the profile applies them. Profile desktop shortcut icons use the multi-size `.ico` rendered by UserAgent (with a compact-thumbnail fallback for older Agents); game desktop shortcuts use the saved game-and-layout composite. These `.ico` files are kept under the current user's LocalAppData `DisplayMagician\Icons` folder so Explorer can read them after restarts and upgrades.

For a complete installable build, use `build_displaymagician.ps1` from the repository root rather than treating this project output as an installer.

## Related components

- `DisplayMagician.ControlService` routes desktop requests and owns machine-wide coordination.
- `DisplayMagician.UserAgent` executes interactive-session display, audio, and shortcut work.
- `DisplayMagicianConsole` offers the same local service through a command-line interface.

See the [repository README](../README.md) for product usage and the [control-service plan](../DISPLAYMAGICIAN_V4_CONTROL_SERVICE_PLAN.md) for the component architecture.
