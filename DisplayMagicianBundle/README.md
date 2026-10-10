# DisplayMagicianBundle

This WiX Burn project creates the end-user `DisplayMagicianSetup` bootstrapper. It chains the DisplayMagician MSI with the required .NET desktop runtime package and presents the themed installer experience.

## Build

Build through the repository release workflow:

```powershell
.\build_displaymagician.ps1
```

Run `.\prepare_displaymagician.ps1` once to obtain the .NET runtime installers used by the bundle. Later runs reuse valid signed installers already in `Packages`; use `.\prepare_displaymagician.ps1 -RefreshRuntimes` when you want to download newer runtime versions.

For a development build, build the MSI first and then the bundle:

```powershell
dotnet build .\DisplayMagicianPackage\DisplayMagicianPackage.wixproj
dotnet build .\DisplayMagicianBundle\DisplayMagicianBundle.wixproj
```

The bundle renames its completed output through `renamedisplaymagician.ps1`. When the untracked repository-level `SigningConfig.props` contains the development PFX created by `prepare_displaymagician.ps1`, the final versioned bundle EXE is signed after that rename. Test the generated setup executable in the Windows Sandbox workflow described in [Sandbox](../Sandbox/README.md), not on a development machine that contains an unrelated installation.

Local Burn signing uses WiX's supported two-stage flow: it signs the detached Burn engine, reattaches it, then signs the completed bundle. This preserves the engine signature needed for prerequisite, repair, and uninstall operations.

GitHub Actions deliberately does not use the local PFX. The SignPath workflows sign the final bundle with the project test or production certificate.
