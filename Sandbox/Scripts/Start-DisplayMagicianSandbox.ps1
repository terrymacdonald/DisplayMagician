#Requires -Version 5.1

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$bundlePath = 'C:\DisplayMagician\Bundle'
$remoteDebuggerPath = 'C:\DisplayMagician\RemoteDebugger\msvsmon.exe'
$desktopPath = [Environment]::GetFolderPath('Desktop')
$releaseShortcutPath = Join-Path $desktopPath 'DisplayMagician Releases.url'
$instructionsPath = Join-Path $desktopPath 'DisplayMagician Sandbox Debugging.txt'

@(
    '[InternetShortcut]',
    'URL=https://github.com/terrymacdonald/DisplayMagician/releases'
) | Set-Content -LiteralPath $releaseShortcutPath -Encoding ASCII

@(
    'DisplayMagician Windows Sandbox Debugging',
    '',
    '1. Install the Debug Bundle from C:\DisplayMagician\Bundle.',
    '2. Configure the elevated Visual Studio Remote Debugger window to use Windows Authentication.',
    '3. In Visual Studio on the host, use Debug > Attach to Process with Connection type Remote (Windows).',
    '4. Attach to DisplayMagician.exe, DisplayMagician.UserAgent.exe, DisplayMagician.ControlService.exe, DisplayMagician.SessionLauncher.exe, or DisplayMagicianConsole.exe.',
    '',
    'Use the DisplayMagician Releases desktop shortcut to download an earlier version for upgrade testing.',
    'The mapped Bundle, Sandbox scripts, and Remote Debugger tools are read-only host mappings.'
) | Set-Content -LiteralPath $instructionsPath -Encoding UTF8

Start-Process -FilePath 'explorer.exe' -ArgumentList $bundlePath
Start-Process -FilePath 'notepad.exe' -ArgumentList $instructionsPath
Start-Process -FilePath $remoteDebuggerPath -Verb RunAs
