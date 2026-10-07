# DisplayMagicianIdentityPkg

This MSBuild no-targets project creates the sparse MSIX identity package used by DisplayMagician. The identity package supplies Windows package identity and taskbar integration while the WiX MSI installs the full-trust desktop application and services.

The build patches the MSIX manifest version from `DisplayMagician.exe`, generates PRI resources with the installed Windows SDK tools, packs the MSIX, and signs it when `SigningConfig.props` supplies a certificate.

`AppxManifest.template.xml` is source-controlled. `prepare_displaymagician.ps1` creates the ignored local `AppxManifest.xml` from it when the local manifest is missing or invalid, using the selected signing certificate's subject as its publisher. Run the setup script before building on a new machine.

## Build

Build `DisplayMagician` first, then build this project:

```powershell
dotnet build .\DisplayMagician\DisplayMagician.csproj
dotnet build .\DisplayMagicianIdentityPkg\DisplayMagicianIdentityPkg.csproj
```

It requires the Windows 10 SDK tools (`makeappx.exe`, `makepri.exe`, and optionally `signtool.exe`). The output MSIX is placed in `DisplayMagicianPackage\Packages\` for the MSI build.

Use the repository release build script for a signed release rather than manually editing `AppxManifest.xml` version data.
