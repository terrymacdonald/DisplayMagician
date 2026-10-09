# DisplayMagician.Gateway

`DisplayMagician.Gateway` is the machine-level HTTPS adapter for paired remote devices. It runs as the low-privilege Windows `LocalService` account, listens on the configured interface and port (all interfaces and TCP `22846` by default), and never directly accesses UserAgent, user profile data, display APIs, or game execution. The installer enables firewall access on Private and Domain Windows networks; an administrator can opt in to Public networks from the desktop Server Settings form.

The Gateway owns two distinct machine P-256 identities: a host signing identity for client host verification, and a self-signed TLS certificate. The host ID uses the RFC 7638 SHA-256 JWK thumbprint. Pairing QRs carry a Base64 SHA-256 pin of the TLS SubjectPublicKeyInfo, which remains stable when the certificate renews with the same key. Private keys remain in the Windows machine key store and must never appear in logs, support ZIPs, configuration, or backups.

On startup, Gateway includes the configured local and future remote DNS/IP hosts plus loopback names in the TLS certificate's subject alternative names. A certificate with an expiring or mismatched name is regenerated with the existing TLS key. Gateway checks hourly for certificates expiring within 30 days and switches Kestrel to the renewed certificate without restarting; certificate renewal retains the configured TLS key. ControlService accepts pairing QRs only for origins that the running Gateway registered.
If the TLS key is lost or must be replaced, an administrator can approve replacement in WinForms Server Settings. Gateway accepts that approval for ten minutes. ControlService revokes paired credentials and expires pairing sessions when the new SPKI pin registers; phones must re-pair. A changed host-identity key also invalidates every pairing.

At startup, and periodically afterwards, the Gateway registers its public identity with ControlService through a dedicated LocalService-only named pipe. ControlService uses that registered identity when it creates pairing sessions, so a desktop client cannot substitute a different host key or TLS SPKI pin.

Current network endpoints include authenticated profile, audio-profile, shortcut, operation-decision, status, and SSE status-stream routes. QR pairing binds a paired device to the verified local session that created the QR. ControlService uses that preferred session while it has a healthy, ready User Agent, or automatically uses the only healthy session for that SID. A remote client may provide `targetSessionId` only when it needs to resolve multiple available sessions; Gateway never chooses a console session implicitly.

Every HTTP request sends `DisplayMagician-Protocol-Hello` as unpadded base64url UTF-8 JSON for the shared `ProtocolHello` contract. Gateway validates it before authentication and returns `DisplayMagician-Protocol-Welcome` on successful responses. Every response includes `DisplayMagician-Request-Id`. Protected routes use a long-lived per-device bearer credential; ControlService holds only its SHA-256 hash and current grants. Pairing requests use a candidate-generated polling secret and deliver the credential on the first approved poll only. The current route layout remains the prototype layout until P2. [openapi.yaml](../Docs/REST/openapi.yaml) describes available routes; [target-openapi.json](../Docs/REST/target-openapi.json) is the approved target.

Gateway limits HTTP request bodies to 64 KiB and non-streaming JSON responses to 1 MiB. Oversized JSON results return `500 response-too-large` until the corresponding target collection supports paging. Protected devices are limited to 120 requests and 10 mutations per minute; source IPs are limited to 20 failed authentications per minute. Gateway also bounds concurrent requests and returns `429` with `Retry-After` when a limit is reached. Mutations require a lowercase UUID v4 `Idempotency-Key`; ControlService retains accepted results for 24 hours.

## Development

```powershell
dotnet build .\DisplayMagician.Gateway\DisplayMagician.Gateway.csproj
```

Do not run this host directly on a production machine until it has been installed by the DisplayMagician package: the installer provisions the `LocalService` account, firewall rule, and machine data access.
