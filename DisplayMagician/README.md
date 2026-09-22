# DisplayMagician desktop application

This project is the designer-backed WinForms desktop application. It provides the main window, system-tray experience, profile and shortcut editors, notifications, and support-ZIP export.

The desktop application is a client of `DisplayMagician.ControlService`; it must not directly perform Agent-owned display, audio, game, process, or authoritative per-user storage work. Shared transport types come from `DisplayMagician.Contracts`, while portable shortcut definitions come from `DisplayMagician.ConfigurationDefinitions`.

## Development

Build the project from the repository root:

```powershell
dotnet build .\DisplayMagician\DisplayMagician.csproj
```

Run the project from Visual Studio in an interactive Windows session. The forms are designer-backed: change static layout in `UIForms\*.Designer.cs` and behaviour in the matching form file.

For a complete installable build, use `build_displaymagician.ps1` from the repository root rather than treating this project output as an installer.

## Related components

- `DisplayMagician.ControlService` routes desktop requests and owns machine-wide coordination.
- `DisplayMagician.UserAgent` executes interactive-session display, audio, and shortcut work.
- `DisplayMagicianConsole` offers the same local service through a command-line interface.

See the [repository README](../README.md) for product usage and the [control-service plan](../DISPLAYMAGICIAN_V4_CONTROL_SERVICE_PLAN.md) for the component architecture.
