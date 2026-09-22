# DisplayMagicianPackage

This WiX project builds the DisplayMagician MSI. It publishes and packages the desktop application, console, Control Service, Session Launcher, User Agent, the sparse MSIX identity package, and installer resources. It also defines installation folders, service registration, context-menu integration, and installer UI.

## Build

Use the repository release build workflow:

```powershell
.\build_displaymagician.ps1
```

For development, build the WiX project after its payload projects have built:

```powershell
dotnet build .\DisplayMagicianPackage\DisplayMagicianPackage.wixproj
```

The project accepts a `UseExistingIdentityPackage=true` build property when CI supplies a previously signed MSIX. Signing configuration is intentionally local and is loaded from the repository-level `SigningConfig.props` when present.

Do not hand-edit generated publish output or `Packages\` contents; change the relevant component project, WiX source, or build workflow instead.
