# DisplayMagician.ConfigurationDefinitions

This class library owns portable persisted configuration definitions, their schema versions, JSON conversion, and pure configuration validation. `ShortcutDefinition` is the current primary model.

It deliberately has no dependency on WinForms, named pipes, hardware, files, processes, game discovery, or display/audio runtime behaviour. Keep persisted member names backwards compatible; a substantive format change needs an explicit migration in the owning persistence layer.

## Build

```powershell
dotnet build .\DisplayMagician.ConfigurationDefinitions\DisplayMagician.ConfigurationDefinitions.csproj
```

Use `DisplayMagician.Contracts` for cross-process request, response, and enum types. Use this project only for portable persisted definitions.
