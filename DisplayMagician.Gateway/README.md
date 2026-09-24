# DisplayMagician.Gateway

`DisplayMagician.Gateway` is the machine-level HTTPS and WebSocket adapter for paired remote devices. It runs as the low-privilege Windows `LocalService` account, listens on every network interface at TCP port `22846`, and never directly accesses UserAgent, user profile data, display APIs, or game execution.

The Gateway owns two distinct machine P-256 identities: a host signing identity for client host verification, and a self-signed TLS certificate. The certificate SHA-256 fingerprint and host public JWK are placed in the pairing QR payload. Private keys remain in the Windows machine key store and must never appear in logs, support ZIPs, configuration, or backups.

At this stage the only network endpoint is `GET /v1/identity`, which exposes only the public host identity and TLS fingerprint. Pairing, authenticated commands, status events, and ControlService routing follow in later slices.

## Development

```powershell
dotnet build .\DisplayMagician.Gateway\DisplayMagician.Gateway.csproj
```

Do not run this host directly on a production machine until it has been installed by the DisplayMagician package: the installer provisions the `LocalService` account, firewall rule, and machine data access.
