# DisplayMagician.SessionLauncher

This project hosts the demand-start Windows service named **DisplayMagician Session Launcher**. It is the narrowly scoped LocalSystem broker that starts the signed `DisplayMagician.UserAgent` in an already-authorised interactive session when the Control Service needs an Agent and none is connected.

It accepts authenticated local requests only from the Control Service, accepts only a session ID plus the fixed signed Agent payload, reports launch status, and stops when idle. It never accepts remote clients, displays UI, or performs display/audio/shortcut work.

Launch and stop requests must carry a ControlService `ProtocolHello`. After authenticating the Windows caller, the broker checks the version range and required capabilities before dispatching either command. Results use the shared `UserAgentLaunchResult` with `ErrorCode` and a negotiated `ProtocolWelcome`. ControlService verifies successful replies against the `SessionLauncher` endpoint and the `protocol-negotiation`/`session-launcher` capabilities. Deploy both components together; unnegotiated legacy successes are not accepted. Pipe permissions and fixed-executable restrictions remain unchanged.

## Build

Launch fails explicitly if Windows cannot create the target user's environment; it does not start UserAgent with a missing environment block. Unprivileged request-validation regression tests run through `DisplayMagician.ControlService.Tests`. Actual token acquisition and installed service launch still require the Windows Sandbox workflow.

```powershell
dotnet build .\DisplayMagician.SessionLauncher\DisplayMagician.SessionLauncher.csproj
```

The installer configures its LocalSystem identity and pipe ACL. Do not run it as a general-purpose launcher or widen its IPC permissions.

See the [Control Service README](../DisplayMagician.ControlService/README.md) and [architecture plan](../DISPLAYMAGICIAN_V4_CONTROL_SERVICE_PLAN.md) for its security boundary.

SessionLauncher reads the protected ControlService-owned Machine/ControlServiceSettings.json preferences and Diagnostics/DiagnosticLoggingSessions.json lease documents every second. It applies the shared saved level and the most verbose unexpired diagnostic session, including while no launch request is arriving. Each section retains its last valid snapshot during read/format failures; sessions still expire on schedule. Before the first valid read, the baseline defaults to Warn and sessions are empty. Files remain service-authored and are never rewritten by this broker. A process launch no longer independently changes the broker logging threshold. No new broker command, caller permission or process-launch capability was added.
