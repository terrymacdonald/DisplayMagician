# DisplayMagician.ControlService

This project hosts the machine-wide Windows service named **DisplayMagician Control Service**. It authenticates local clients, routes requests to the correct per-user User Agent, serialises machine display operations, retains operation status and pending decisions, coordinates persistence/recovery, and owns machine-level scheduling, audit, diagnostics, and the local integration foundation.

It does not show UI, launch games, inspect Steam or desktop windows, or change display/audio settings directly. Those interactive-session responsibilities belong to `DisplayMagician.UserAgent`. It owns approved remote-device grants and credential hashes, ten-minute pairing sessions, host/TLS identity state, and the Gateway's 24-hour HTTP idempotency ledger. Local and paired-client approval grant exactly the candidate-requested capabilities. `DisplayMagician.Gateway` is the only network-facing adapter and asks the service to enforce these rules.

When an authorised interactive user session has no connected Agent, the service can request `DisplayMagician.SessionLauncher` to demand-start one in that verified SID/session. A physical-console session is not required. QR pairing records that local SID/session as the paired device's preferred route. ControlService revalidates it before every remote request, falls back only when the owner has exactly one healthy ready Agent, and otherwise requires an explicit target selection.

Control Service routes work only to a ready Agent whose process and command-pipe identity match the fixed installed payload. It bounds pipe handshakes and subscribers, retains successful mutating client responses for 24 hours for safe retry, and makes lagging event subscribers reconnect for an authoritative status/decision snapshot.

An authorised session requesting display control takes over an idle lease from another session, including when the previous Agent is still heartbeating. Active operations and recovery-required leases remain reserved for their owner until the work or recovery is resolved.

Every local request carries the shared transport-neutral protocol hello and receives a negotiated protocol/capability welcome before work is processed. This is a compatibility boundary only; remote authentication is verified by ControlService over the Gateway-only local pipe. ControlService remains the source of truth for pairing approval, expiry, revocation, ownership, and capability grants. Its support snapshot includes a redacted paired-device list and excludes credentials, credential hashes, polling secrets, and private keys.

Administrator Gateway settings updates validate bind addresses, advertised DNS/IP hosts, and ports before saving so invalid endpoints cannot prevent Gateway startup.
Pairing QR creation also checks that the requested HTTPS origin matches an endpoint registered by the running Gateway.
ControlService reconciles the installer-owned Private, Domain, and Public Gateway firewall rules with the saved listener port and `AllowPublicNetworks` setting. Public is off by default; only an administrator may change the setting.

## Development

```powershell
dotnet build .\DisplayMagician.ControlService\DisplayMagician.ControlService.csproj
dotnet test .\DisplayMagician.ControlService.Tests\DisplayMagician.ControlService.Tests.csproj
```

The service is installed and configured by the WiX package; do not manually register a development build as a production service. Build the complete installer through `build_displaymagician.ps1`.

Machine-owned state, audit records, and logs live under `C:\ProgramData\DisplayMagician\Machine\` in an installed environment. See the [architecture plan](../DISPLAYMAGICIAN_V4_CONTROL_SERVICE_PLAN.md) for IPC, security, and ownership rules.

For a user-selected Support ZIP, ControlService stages retained machine logs, the installer transaction log, the audit log, and an allowlisted machine configuration snapshot (including gateway and temporary diagnostic settings and their backups). It does not stage pairing sessions, replay records, gateway identities, or other secret-bearing machine state.
