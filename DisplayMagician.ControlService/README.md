# DisplayMagician.ControlService

This project hosts the machine-wide Windows service named **DisplayMagician Control Service**. It authenticates local clients, routes requests to the correct per-user User Agent, serialises machine display operations, retains operation status and pending decisions, coordinates persistence/recovery, and owns machine-level scheduling, audit, diagnostics, and the local integration foundation.

It does not show UI, launch games, inspect Steam or desktop windows, or change display/audio settings directly. Those interactive-session responsibilities belong to `DisplayMagician.UserAgent`.

When an authorised active user has no connected Agent, the service can request `DisplayMagician.SessionLauncher` to demand-start one in that user's session.

Control Service routes work only to a ready Agent whose process and command-pipe identity match the fixed installed payload. It bounds pipe handshakes and subscribers, retains successful mutating client responses for 24 hours for safe retry, and makes lagging event subscribers reconnect for an authoritative status/decision snapshot.

## Development

```powershell
dotnet build .\DisplayMagician.ControlService\DisplayMagician.ControlService.csproj
dotnet test .\DisplayMagician.ControlService.Tests\DisplayMagician.ControlService.Tests.csproj
```

The service is installed and configured by the WiX package; do not manually register a development build as a production service. Build the complete installer through `build_displaymagician.ps1`.

Machine-owned state, audit records, and logs live under `C:\ProgramData\DisplayMagician\Machine\` in an installed environment. See the [architecture plan](../DISPLAYMAGICIAN_V4_CONTROL_SERVICE_PLAN.md) for IPC, security, and ownership rules.
