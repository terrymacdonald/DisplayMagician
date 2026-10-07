# DisplayMagician.UserAgent.Tests

This xUnit test project verifies User Agent behaviour and the shared contracts it exercises. Coverage includes Agent command pipes and identity, shortcut execution and recovery, process monitoring, game detection, repository connections, serialization, support bundles, logging layout, retry policy, and build-version consistency.

## Run

```powershell
dotnet test .\DisplayMagician.UserAgent.Tests\DisplayMagician.UserAgent.Tests.csproj
```

Tests must not require a real display topology, audio device, game installation, or installed service. Add tests alongside changes to User Agent-owned behaviour.
