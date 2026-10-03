# DisplayMagicianBundle

This WiX Burn project creates the end-user `DisplayMagicianSetup` bootstrapper. It chains the DisplayMagician MSI with the required .NET Desktop Runtime 10 package and presents the themed installer experience. ASP.NET Core is not bundled.

## Build

Run `prepare_displaymagician.ps1` before building the bundle. It downloads the current Microsoft-signed .NET Desktop Runtime 10 installer and writes the local, ignored `RuntimeConfig.props` file containing its exact version and filename.

Build the full installer through the repository entry point:

```powershell
.\build_displaymagician.ps1
```

The bundle project creates `DisplayMagicianSetup_v<version>.exe` after WiX produces the bootstrapper. Test the generated setup executable in the Windows Sandbox workflow described in [Sandbox](../Sandbox/README.md), not on a development machine containing an unrelated installation.
