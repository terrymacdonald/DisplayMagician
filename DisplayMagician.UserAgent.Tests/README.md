# DisplayMagician.UserAgent.Tests

This xUnit test project verifies User Agent behaviour and the shared contracts it exercises. Coverage includes Agent command pipes and identity, shortcut execution and recovery, process monitoring, game detection, repository connections, serialization, support bundles, logging layout, retry policy, and build-version consistency.

## Run

```powershell
dotnet test .\DisplayMagician.UserAgent.Tests\DisplayMagician.UserAgent.Tests.csproj
```

Tests must not require a real display topology, audio device, game installation, or installed service. Add tests alongside changes to User Agent-owned behaviour.

Regression cases cover full-path process stopping with executable name collisions, newest-root selection, launcher handover beyond the discovery timeout, independent recovery snapshots and legacy recovery records, and desktop shortcut commit failures/draft isolation. Process tests launch only disposable test executables and clean up their own process trees.
