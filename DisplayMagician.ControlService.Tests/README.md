# DisplayMagician.ControlService.Tests

This xUnit test project verifies Control Service behaviour without installing the Windows service. It covers pipe security, control-state coordination, operation routing/status persistence, diagnostics and recovery, migration, client sync, metrics, and machine scheduling.

## Run

```powershell
dotnet test .\DisplayMagician.ControlService.Tests\DisplayMagician.ControlService.Tests.csproj
```

Keep tests deterministic and isolated from real user configuration, services, hardware, and network resources. Add coverage here when changing Control Service-owned behaviour.
