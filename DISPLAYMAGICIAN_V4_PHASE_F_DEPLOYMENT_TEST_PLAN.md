# DisplayMagician v4 Phase F Deployment Verification Plan

## Purpose

Verify that the v4 installer deploys, upgrades, repairs, and removes the Control Service platform without losing user data or weakening the machine/per-user storage boundary.

This plan covers the two remaining Phase F outcomes:

- Fresh installation and upgrade preserve migrated user data and restore Control Service/User Agent connectivity.
- Installed components report the common version derived from the root `version.json`.

Run destructive cases only in a disposable VM or after taking a full snapshot. Record the MSI path, installer log, OS build, test account SID, installed version, and outcome for every run.

## Test environment

Use a Windows 10 or Windows 11 VM with a physical or virtual interactive console session. Run at least these account configurations:

- Standard interactive user.
- Elevated administrator who installs, repairs, and uninstalls DisplayMagician.
- A second standard user for per-SID storage and fast-user-switch checks.

Prepare these installer payloads:

- A current v4 MSI built from the candidate commit.
- The preceding v4 MSI for upgrade testing.
- A legacy DisplayMagician build or a representative legacy `%LOCALAPPDATA%\DisplayMagician` fixture containing profiles, audio profiles, shortcuts, settings, and message state.

Before each case, record these values from an elevated PowerShell session. The SID-root ACL is present only after that user has completed Agent provisioning:

```powershell
$installDir = 'C:\Program Files\DisplayMagician'
$userSid = [System.Security.Principal.WindowsIdentity]::GetCurrent().User.Value
Get-Service DisplayMagicianControlService, DisplayMagicianSessionLauncher -ErrorAction SilentlyContinue
@('C:\ProgramData\DisplayMagician', "C:\ProgramData\DisplayMagician\Users\$userSid") |
    Where-Object { Test-Path -LiteralPath $_ } |
    ForEach-Object { Get-Acl -LiteralPath $_ | Format-List }
```

If the product is installed elsewhere, replace `$installDir` with the selected installation directory. Capture MSI logs with `/l*v` and retain them with the test result.

## Acceptance tests

| ID | Scenario | Procedure | Expected result |
|---|---|---|---|
| F-01 | Fresh installation | Start from a VM with no DisplayMagician installation or ProgramData directory. Install v4 as an administrator and launch WinForms as the standard user. | Installer succeeds; `DisplayMagicianControlService` is installed and running; `DisplayMagicianSessionLauncher` is installed and stopped/demand-start; WinForms starts the hidden User Agent and connects to the service. |
| F-02 | Fresh-install storage boundary | After F-01, inspect ProgramData ACLs. Attempt to create a file in `Machine` as the standard user, then use WinForms to create/update a profile for that user. | Standard user cannot create or change machine-owned files. The Control Service can write machine data. The user can use only their own `Users\\<SID>` data after Agent provisioning. |
| F-03 | Per-user isolation | Sign in as a second standard user, launch WinForms, and allow Agent migration/provisioning. Inspect both SID folders and attempt cross-SID access from each account. | Each SID has a separate data root. A user can modify their own root but cannot read or modify another user's data. Administrators, SYSTEM, and LocalService retain required administrative/service access. |
| F-04 | Legacy first-run migration | Populate the test user's legacy AppData fixture, install/launch v4, and wait for Agent migration to finish. Reopen WinForms. | Profiles, audio profiles, shortcuts, settings, and message state load from the SID root. Successfully migrated legacy files are renamed individually with `.old`; `Migration.json` is written; reopening does not duplicate imports. |
| F-05 | Migration failure safety | Make one fixture file unreadable or invalid while leaving another valid. Start the Agent migration, then restore the fixture and retry. | The failed source remains untouched, a useful failure is reported/logged, and no incomplete marker permits silent fallback. After correction, retry succeeds without duplicating already migrated data. |
| F-06 | Upgrade with active data | Install the preceding v4 MSI, create representative profiles/shortcuts, then upgrade to the candidate MSI without selecting data removal. Launch WinForms. | Upgrade succeeds; ProgramData and per-SID data remain intact; service registrations/ACLs remain correct; Agent reconnects; profiles and shortcuts are unchanged. |
| F-07 | Repair | On a working candidate installation, stop the Control Service and remove a non-user-data ProgramData subdirectory or alter its ACL in the disposable VM. Run MSI repair as administrator, then launch WinForms. | Repair restores service binaries/registration and installer-owned ProgramData directories/ACLs without deleting per-SID data. The service starts and Agent reconnects. |
| F-08 | Default uninstall and reinstall | Uninstall without selecting the explicit user-data removal option. Confirm service removal, then install the candidate MSI again and launch WinForms. | Services and binaries are removed on uninstall; `ProgramData\\DisplayMagician` and user data remain. Reinstall restores the services and connects without a second migration or data loss. |
| F-09 | Explicit data-removal uninstall | In a disposable VM, uninstall with the user-data removal option selected. | Installer clearly requires the explicit option. After success, legacy AppData data and ProgramData data are removed; service registrations and binaries are removed. This case must never run on a non-disposable machine. |
| F-10 | Service recovery and upgrade behavior | Confirm both services' start modes and recovery configuration before and after F-06/F-07. Reboot the VM and open WinForms. | Control Service is automatic and running after boot. Session Launcher remains demand-start. Failure-recovery configuration survives upgrade/repair. WinForms starts or reconnects the Agent in the active interactive session. |
| F-11 | User Agent lifecycle | With WinForms open, minimize to tray, then fully exit while idle. Repeat with an active shortcut if hardware is available. | Agent stays connected while WinForms is visible or minimized to tray. It stops after full idle exit. During an active operation it remains until safe completion/recovery. |
| F-12 | Installed version consistency | Read file and product versions from the installed WinForms, Console, Contracts, ControlService, SessionLauncher, and UserAgent binaries. Compare them to the candidate build metadata generated from root `version.json`. | Every shipped v4 component reports the same `4.0.0` base version and compatible generated build/revision metadata. No component reports a development-only or hard-coded registration version. |

## Version evidence

For F-12, collect the following from the installed payload, adjusting the paths if a non-default installation directory was selected:

```powershell
$components = @(
    "$installDir\DisplayMagician.exe",
    "$installDir\DisplayMagicianConsole.exe",
    "$installDir\DisplayMagician.Contracts.dll",
    "$installDir\ControlService\DisplayMagician.ControlService.exe",
    "$installDir\SessionLauncher\DisplayMagician.SessionLauncher.exe",
    "$installDir\UserAgent\DisplayMagician.UserAgent.exe"
)

$components | ForEach-Object {
    $version = (Get-Item -LiteralPath $_).VersionInfo
    [PSCustomObject]@{
        Path = $_
        ProductVersion = $version.ProductVersion
        FileVersion = $version.FileVersion
    }
} | Format-Table -AutoSize
```

Attach this output and the candidate commit/version metadata to the test record. Treat a missing payload or any different base version as a release blocker.

## Evidence and pass criteria

Each test record must include:

- Installer command line, MSI log, and result code.
- Service state before/after the operation.
- ProgramData ACL output for the root, Machine branch, Users branch, and tested SID roots.
- Screenshots or exported lists proving the migrated profiles, audio profiles, and shortcuts are present.
- Relevant Control Service and User Agent logs, with secrets/redacted arguments excluded.
- Version output for F-12.

Phase F passes only when F-01 through F-12 pass on a clean VM. Any data loss, cross-SID access, inability to repair service connectivity, duplicate migration, or version mismatch blocks the v4.0.0 release.
