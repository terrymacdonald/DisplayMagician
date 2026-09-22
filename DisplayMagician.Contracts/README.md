# DisplayMagician.Contracts

This class library is the stable cross-process contract shared by the desktop application, console, Control Service, Session Launcher, and User Agent.

It contains versioned control envelopes, protocol message types, request/response models, shared enums, client events, retry policy, and the common support-log layout and correlation scope. It contains no UI, hardware access, files, or static application state.

## Compatibility rules

- Treat public serialized fields and enum numeric values as compatibility contracts.
- Add protocol changes in a backwards-compatible form and preserve the serializer conventions in `ControlEnvelopeSerializer`.
- Do not place persisted configuration definitions here; they belong in `DisplayMagician.ConfigurationDefinitions`.

## Build

```powershell
dotnet build .\DisplayMagician.Contracts\DisplayMagician.Contracts.csproj
```
