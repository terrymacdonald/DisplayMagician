# DisplayMagician.ControlService.Tests

Regression coverage includes required-capability reply validation, truncated frame prefixes, decision answers at/after expiry, persisted retry-count bounds, and deterministic transient/permanent atomic-file replacement errors. This test project also references SessionLauncher to check invalid launch/stop requests and rejection of unrelated processes without installing services or obtaining privileged session tokens.

SessionLauncher protocol tests use unique same-user test pipes and substituted process callbacks. They exercise the real shared framing, ControlService send/receive path, broker negotiation/dispatch, both command types, execution failures, malformed or incompatible requests, missing capabilities, and mismatched/unnegotiated responses. They do not connect to installed services or launch/stop real processes.

This xUnit test project verifies Control Service behaviour without installing the Windows service. It covers pipe security, control-state coordination, operation routing/status persistence, diagnostics and recovery, migration, client sync, metrics, and machine scheduling.

## Run

`LocalReplayReliabilityTests` exercises reservation-before-dispatch, restart replay/unknown outcomes, concurrent duplicates, corrupt and missing ledgers, explicit-failure retries, capacity, legacy compatibility and failed atomic writes. `RoutingReliabilityTests` covers remote shortcut machine leases, asynchronous profile acceptance, stale/foreign heartbeats, running-Agent reconnects, launcher failures and malformed nested Gateway commands. The forced-restart failure test waits through the production ten-second stop deadline but never stops an installed process.

```powershell
dotnet test .\DisplayMagician.ControlService.Tests\DisplayMagician.ControlService.Tests.csproj
```

Keep tests deterministic and isolated from real user configuration, services, hardware, and network resources. Add coverage here when changing Control Service-owned behaviour.

Migration failure tests preserve an existing destination when a source is invalid, handle missing sources with corrupt/locked destinations, skip retired files on retry, reject unreadable/incomplete/future-schema markers without touching either side and serialize concurrent imports. The fixtures use temporary directories and never import actual user configuration.

Gateway event tests cover replay restart/user isolation, grant/session filtering, published retention bounds, empty-history snapshot cursors, interrupted publication markers, actual journal-write failures, and startup reconciliation against authoritative operation state. These use disposable temporary storage and do not alter installed machine data.

Gateway replay-ledger tests also verify that corrupt, incomplete, or inaccessible state blocks mutations while preserving the primary and backup files and allowing store construction for local service startup.

OperationEventOrderingTests uses temporary stores and deterministic barriers to verify commit-ordered notifications without holding the state lock, subscriber exception/mutation isolation, and independent deep event snapshots across authorised sessions. The existing delivery test checks event values rather than requiring a shared mutable object instance.
