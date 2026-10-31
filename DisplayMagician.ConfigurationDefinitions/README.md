# DisplayMagician.ConfigurationDefinitions

This class library owns portable persisted configuration definitions, their schema versions, JSON conversion, and pure configuration validation. `ShortcutDefinition` is the current primary model.

It deliberately has no dependency on WinForms, named pipes, hardware, files, processes, game discovery, or display/audio runtime behaviour. Keep persisted member names backwards compatible; a substantive format change needs an explicit migration in the owning persistence layer.

`RepositoryDocumentValidation` checks repository JSON envelopes before persistence: the correct collection must be an array of objects with distinct, nonempty UUID strings. It leaves unknown fields intact and does not apply hardware validation or rewrite schema versions. Runtime profile repositories additionally validate deserialization through their existing restricted JSON.NET binder.

Shortcut window placements are optional portable definitions. They store stable Start Program IDs, executable/window matching information, normal bounds, optional maximized position, show state, and required display names; runtime PIDs and handles stay in UserAgent memory. Version 8 keeps version 7 snapshots without maximized coordinates for best-effort restoration until recaptured.

`MonitorLaunchedExecutable` is the property and JSON field used throughout the shortcut model. True selects the launched executable; false selects `DifferentExecutablePathToMonitor`. Both choices require lifetime monitoring. The Agent-owned shortcut migration renames `ProcessNameToMonitorUsesExecutable` and the earlier development name `MonitorExecutablePath`, preserving their values and backing up the source file. No alias fields are written.

Shortcut schema 9 adds `WaitAfterStartSecs` to Start/After Programs and `WaitAfterStopSecs` to Stop Programs, defaulting missing values to zero. Start/Stop delays must be integers from 0 to 999 seconds; repository validation rejects invalid values without changing the stored file. After Program delays are preserved for future use but are not executed.

## Build

```powershell
dotnet build .\DisplayMagician.ConfigurationDefinitions\DisplayMagician.ConfigurationDefinitions.csproj
```

Use `DisplayMagician.Contracts` for cross-process request, response, and enum types. Use this project only for portable persisted definitions.
