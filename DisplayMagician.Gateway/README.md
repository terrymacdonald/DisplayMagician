# DisplayMagician.Gateway

`DisplayMagician.Gateway` is the machine-level HTTPS adapter for paired remote devices. It runs as the low-privilege Windows `LocalService` account, listens on every network interface at TCP port `22846`, and never directly accesses UserAgent, user profile data, display APIs, or game execution.

The Gateway owns two distinct machine P-256 identities: a host signing identity for client host verification, and a self-signed TLS certificate. The certificate SHA-256 fingerprint and host public JWK are placed in the pairing QR payload. Private keys remain in the Windows machine key store and must never appear in logs, support ZIPs, configuration, or backups.

At startup, and periodically afterwards, the Gateway registers its public identity with ControlService through a dedicated LocalService-only named pipe. ControlService uses that registered identity when it creates pairing sessions, so a desktop client cannot substitute a different host key or certificate fingerprint.

Current network endpoints include authenticated profile, audio-profile, shortcut, operation-decision, status, and SSE status-stream routes. QR pairing binds a paired device to the verified local session that created the QR. ControlService uses that preferred session while it has a healthy, ready User Agent, or automatically uses the only healthy session for that SID. A remote client may provide `targetSessionId` only when it needs to resolve multiple available sessions; Gateway never chooses a console session implicitly.

Every HTTP request now sends `DisplayMagician-Protocol-Hello` as unpadded base64url UTF-8 JSON for the shared `ProtocolHello` contract. Gateway validates it before authentication and returns `DisplayMagician-Protocol-Welcome` on successful responses. Every response includes `DisplayMagician-Request-Id`; negotiation failures use Problem Details with `400`, `409`, or `412`. The current protected routes still use the prototype signed-request authentication while the approved bearer-credential routes are implemented. [openapi.yaml](../Docs/REST/openapi.yaml) describes the available routes; [target-openapi.json](../Docs/REST/target-openapi.json) is the design-only target.

## Development

```powershell
dotnet build .\DisplayMagician.Gateway\DisplayMagician.Gateway.csproj
```

Do not run this host directly on a production machine until it has been installed by the DisplayMagician package: the installer provisions the `LocalService` account, firewall rule, and machine data access.
