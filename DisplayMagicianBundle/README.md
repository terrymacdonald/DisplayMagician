# DisplayMagicianBundle

This WiX Burn project creates the end-user `DisplayMagicianSetup` bootstrapper. It chains the DisplayMagician MSI with the required .NET desktop runtime package and presents the themed installer experience.

## Build

Build through the repository release workflow:

```powershell
.\build_displaymagician.ps1
```

For a development build, build the MSI first and then the bundle:

```powershell
dotnet build .\DisplayMagicianPackage\DisplayMagicianPackage.wixproj
dotnet build .\DisplayMagicianBundle\DisplayMagicianBundle.wixproj
```

The bundle renames its completed output through `renamedisplaymagician.ps1`. Test the generated setup executable in the Windows Sandbox workflow described in [Sandbox](../Sandbox/README.md), not on a development machine that contains an unrelated installation.
