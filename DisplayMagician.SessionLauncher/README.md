# DisplayMagician.SessionLauncher

This project hosts the demand-start Windows service named **DisplayMagician Session Launcher**. It is the narrowly scoped LocalSystem broker that starts the signed `DisplayMagician.UserAgent` in an already-authorised interactive session when the Control Service needs an Agent and none is connected.

It accepts authenticated local requests only from the Control Service, accepts only a session ID plus the fixed signed Agent payload, reports launch status, and stops when idle. It never accepts remote clients, displays UI, or performs display/audio/shortcut work.

## Build

```powershell
dotnet build .\DisplayMagician.SessionLauncher\DisplayMagician.SessionLauncher.csproj
```

The installer configures its LocalSystem identity and pipe ACL. Do not run it as a general-purpose launcher or widen its IPC permissions.

See the [Control Service README](../DisplayMagician.ControlService/README.md) and [architecture plan](../DISPLAYMAGICIAN_V4_CONTROL_SERVICE_PLAN.md) for its security boundary.
