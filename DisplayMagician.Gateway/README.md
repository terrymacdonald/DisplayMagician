# DisplayMagician.Gateway

`DisplayMagician.Gateway` is the machine-level HTTPS adapter for paired remote devices. It runs as the low-privilege Windows `LocalService` account, listens on the configured interface and port (all interfaces and TCP `22846` by default), and never directly accesses UserAgent, user profile data, display APIs, or game execution. The installer enables firewall access on Private and Domain Windows networks; an administrator can opt in to Public networks from the desktop Server Settings form.

The Gateway owns two distinct machine P-256 identities: a host signing identity for client host verification, and a self-signed TLS certificate. The host ID uses the RFC 7638 SHA-256 JWK thumbprint. Pairing QRs carry a Base64 SHA-256 pin of the TLS SubjectPublicKeyInfo, which remains stable when the certificate renews with the same key. Private keys remain in the Windows machine key store and must never appear in logs, support ZIPs, configuration, or backups.

On startup, Gateway includes the configured local and future remote DNS/IP hosts plus loopback names in the TLS certificate's subject alternative names. A certificate with an expiring or mismatched name is regenerated with the existing TLS key. Gateway checks hourly for certificates expiring within 30 days and switches Kestrel to the renewed certificate without restarting; certificate renewal retains the configured TLS key. ControlService accepts pairing QRs only for origins that the running Gateway registered.
If the TLS key is lost or must be replaced, an administrator can approve replacement in WinForms Server Settings. Gateway accepts that approval for ten minutes. ControlService revokes paired credentials and expires pairing sessions when the new SPKI pin registers; phones must re-pair. A changed host-identity key also invalidates every pairing.

At startup, and periodically afterwards, the Gateway registers its public identity with ControlService through a dedicated LocalService-only named pipe. ControlService uses that registered identity when it creates pairing sessions, so a desktop client cannot substitute a different host key or TLS SPKI pin.

Current network endpoints include public identity and capabilities, pairing and device management, saved display/audio/shortcut resources, operation status and cancellation, operation decisions, and optional `/v1/events` SSE streaming. QR pairing binds a paired device to the verified local session that created the QR. ControlService uses that preferred session while it has a healthy, ready User Agent, or automatically uses the only healthy session for that SID. Collection GETs use stable ten-minute cursor pages; saved profile and shortcut GETs support ETags and conditional revalidation. A new SSE stream sends an operation and decision snapshot; a retained `Last-Event-ID` resumes typed events. Phones can use ordinary operation polling without SSE. Streams are limited to one per device and four per user, and recheck credentials every four seconds.

Every HTTP request sends `DisplayMagician-Protocol-Hello` as unpadded base64url UTF-8 JSON for the shared `ProtocolHello` contract. Gateway validates it before authentication and returns `DisplayMagician-Protocol-Welcome` on successful responses. Every response includes `DisplayMagician-Request-Id`. Protected routes use a long-lived per-device bearer credential; ControlService holds only its SHA-256 hash and current grants. Pairing requests use a candidate-generated polling secret and deliver the credential on the first approved poll only. Approval grants every capability requested by the candidate. [openapi.yaml](../Docs/REST/openapi.yaml) describes available routes; [target-openapi.json](../Docs/REST/target-openapi.json) is the approved target.

The single pairing-request GET is reserved for candidate polling with `DisplayMagician-Pairing` authorization. Already-paired approvers use the pairing-request list and decision routes. If ControlService cannot verify a bearer credential, Gateway returns retryable `503` without counting it as an invalid credential.

SSE revalidation has a five-second cancellation deadline covering pipe calls and stream writes, so a stalled I/O operation cannot indefinitely postpone credential checks. Public capabilities advertise the `sse` event transport. `Gateway.Tests` covers the actual stream handler's snapshot/resume, grant omission, connection-limit, revoked-credential, and stalled-read paths. [Phone client foundations](../Clients/README.md) use ordinary polling and do not require SSE. [Installed P3 validation](../Sandbox/REST_P3_TEST_PLAN.md) exercises the real HTTPS host with a developer-only harness.

Gateway limits HTTP request bodies to 64 KiB and non-streaming JSON responses to 1 MiB. Collection pages default to 25 items and allow up to 100. The in-memory snapshot store retains at most eight snapshots per device and 128 overall under a 64 MiB serialized snapshot budget; an oversized collection returns `500 response-too-large`. Protected devices are limited to 120 requests and 10 mutations per minute; source IPs are limited to 20 failed authentications per minute. Gateway also bounds concurrent requests and returns `429` with `Retry-After` when a limit is reached. Mutations require a lowercase UUID v4 `Idempotency-Key`; ControlService retains completed results for 24 hours. An interrupted reservation returns `409 idempotency-outcome-unknown` and stays blocked from duplicate execution; operation actions include a reserved status URL in `Location`.

## Development

Unconfirmed Agent mutation outcomes map to non-retryable `409 operation-outcome-unknown`. The request may have run; inspect operation/resource state before starting another action. Remote shortcuts use ControlService's normal shortcut lease/serialization path.

```powershell
dotnet build .\DisplayMagician.Gateway\DisplayMagician.Gateway.csproj
```

Run `verify_rest_p2_routes.ps1` from the repository root to compare every mapped method/path with the implemented OpenAPI document and check its protocol, idempotency, response, and security declarations. This checks route declarations; the Gateway and ControlService test projects cover behavior.

Do not run this host directly on a production machine until it has been installed by the DisplayMagician package: the installer provisions the `LocalService` account, firewall rule, and machine data access.


Gateway bind and advertisement preferences now come from the Gateway section of the protected Machine/ControlServiceSettings.json schema-1 document owned by ControlService. The startup reader validates the shared document and uses existing defaults with a warning when it is missing, unreadable or unsupported; it never rewrites evidence or reads stale GatewaySettings.json after consolidation. Upgrade Gateway together with ControlService. Bind changes retain the existing restart requirement.
