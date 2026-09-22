# DisplayMagician Windows Sandbox Debugging

Run `debug_displaymagician.ps1` as an administrator and select **2. Windows Sandbox based Debugging**.

Before using this workflow, enable the Windows Sandbox optional feature and restart the host if Windows requests it. The launcher checks this feature but does not enable it automatically.

The launcher builds `Debug|x64` Bundle output, discovers the installed Visual Studio x64 Remote Debugger, and writes `Sandbox\Generated\DisplayMagician-Debug.wsb` with the required absolute paths. It then starts Windows Sandbox.

The generated Sandbox maps these folders read-only:

- The Debug Bundle output, at `C:\DisplayMagician\Bundle`.
- This `Sandbox` source folder, at `C:\DisplayMagician\Sandbox`.
- Visual Studio Remote Debugger tools, at `C:\DisplayMagician\RemoteDebugger`.

Inside Sandbox, the startup script opens the Bundle folder, creates a GitHub Releases desktop shortcut for older-version upgrade tests, and starts elevated `msvsmon.exe`. Configure Remote Debugger with Windows Authentication before attaching from Visual Studio on the host.

This harness is for installer, service, migration, and storage debugging. It does not replace hardware validation for displays, GPUs, audio devices, Steam, or session-switching behaviour.

`Generated` is machine-specific and intentionally excluded from source control.
