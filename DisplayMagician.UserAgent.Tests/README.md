# DisplayMagician.UserAgent.Tests

This xUnit test project verifies User Agent behaviour and the shared contracts it exercises. Coverage includes Agent command pipes and identity, shortcut execution and recovery, process monitoring, game detection, repository connections, serialization, support bundles, logging layout, retry policy, and build-version consistency.

## Run

```powershell
dotnet test .\DisplayMagician.UserAgent.Tests\DisplayMagician.UserAgent.Tests.csproj
```

Tests must not require a real display topology, audio device, game installation, or installed service. Add tests alongside changes to User Agent-owned behaviour.

Regression cases cover full-path process stopping with executable name collisions, newest-root selection, launcher handover beyond the discovery timeout, independent recovery snapshots and legacy recovery records, and desktop shortcut commit failures/draft isolation. Process tests launch only disposable test executables and clean up their own process trees.

Monitor regressions additionally verify helper ownership/cleanup isolation, stale child and reused parent PIDs, deferred handover, bounded failed/incomplete snapshot retries, recovery after a successful retry, and legacy game status after tracking failure. Executable shortcut integration tests verify ordinary monitoring with stale alternative-path text and broker-started alternative targets while old matching processes remain alive. Both JSON serializers and the desktop model are checked for the canonical `MonitorLaunchedExecutable` field. Migration tests verify renaming both old field names, preservation of true/false selections and unrelated settings, backups, idempotence, and the unchanged schema version.
