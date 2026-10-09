# DisplayMagicianPackage

The Gateway installer creates separate inbound TCP firewall rules for Private, Domain, and Public Windows network profiles. Private and Domain are enabled by default; Public is disabled until an administrator enables it in WinForms Server Settings. ControlService keeps their ports and enabled state aligned with the saved Gateway settings.

This WiX project builds the DisplayMagician MSI. It publishes and packages the desktop application, console, Control Service, Session Launcher, User Agent, the sparse MSIX identity package, and installer resources. It also defines installation folders, service registration, context-menu integration, and installer UI.

## Build

Use the repository release build workflow:

```powershell
.\prepare_displaymagician.ps1 # one-time interactive setup on a new machine
.\build_displaymagician.ps1
```

Run the preparation step in an elevated PowerShell session. It generates the local MSIX manifest and runtime bundle config, configures a development signing certificate, and exports a public certificate for Windows Sandbox. The build script checks for those generated prerequisites before cleaning or building the solution.

For development, build the WiX project after its payload projects have built:

```powershell
dotnet build .\DisplayMagicianPackage\DisplayMagicianPackage.wixproj
```

The project accepts a `UseExistingIdentityPackage=true` build property when CI supplies a previously signed MSIX. Signing configuration is intentionally local and is loaded from the repository-level `SigningConfig.props` when present. For a local developer build, the configured certificate signs both the sparse MSIX and the resulting MSI. CI deliberately uses SignPath-signed artifacts instead.

Do not hand-edit generated publish output or `Packages\` contents; change the relevant component project, WiX source, or build workflow instead.
