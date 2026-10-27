# DisplayMagician.ControlService.Tests

Regression coverage includes required-capability reply validation, truncated frame prefixes, decision answers at/after expiry, persisted retry-count bounds, and deterministic transient/permanent atomic-file replacement errors. This test project also references SessionLauncher to check invalid launch/stop requests and rejection of unrelated processes without installing services or obtaining privileged session tokens.

SessionLauncher protocol tests use unique same-user test pipes and substituted process callbacks. They exercise the real shared framing, ControlService send/receive path, broker negotiation/dispatch, both command types, execution failures, malformed or incompatible requests, missing capabilities, and mismatched/unnegotiated responses. They do not connect to installed services or launch/stop real processes.

This xUnit test project verifies Control Service behaviour without installing the Windows service. It covers pipe security, control-state coordination, operation routing/status persistence, diagnostics and recovery, migration, client sync, metrics, and machine scheduling.

## Run

```powershell
dotnet test .\DisplayMagician.ControlService.Tests\DisplayMagician.ControlService.Tests.csproj
```

Keep tests deterministic and isolated from real user configuration, services, hardware, and network resources. Add coverage here when changing Control Service-owned behaviour.

Gateway event tests cover replay restart/user isolation, grant/session filtering, published retention bounds, empty-history snapshot cursors, interrupted publication markers, actual journal-write failures, and startup reconciliation against authoritative operation state. These use disposable temporary storage and do not alter installed machine data.

Gateway replay-ledger tests also verify that corrupt, incomplete, or inaccessible state blocks mutations while preserving the primary and backup files and allowing store construction for local service startup.
