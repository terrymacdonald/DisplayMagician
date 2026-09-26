#Requires -Version 5.1

param(
    [Parameter(Mandatory = $true)]
    [string] $BundlePath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$bundleDirectory = 'C:\DisplayMagician\Bundle'
$testCertificatePath = 'C:\DisplayMagician\Local\DisplayMagicianTest.cer'
$remoteDebuggerPath = 'C:\DisplayMagician\RemoteDebugger\msvsmon.exe'

$desktopPath = [Environment]::GetFolderPath('Desktop')
$releaseShortcutPath = Join-Path $desktopPath 'DisplayMagician Releases.url'
$instructionsPath = Join-Path $desktopPath 'DisplayMagician Sandbox Debugging.txt'

# ---------------------------------------------------------------------------
# Validate mapped files
# ---------------------------------------------------------------------------

if (-not (Test-Path -LiteralPath $BundlePath)) {
    throw "The DisplayMagician Debug Bundle was not found: $BundlePath"
}

if (-not (Test-Path -LiteralPath $testCertificatePath)) {
    throw @"
The DisplayMagician test signing certificate was not found.

Expected:
  $testCertificatePath

Run prepare_displaymagician.ps1 on the host before starting Windows Sandbox.
"@
}

if (-not (Test-Path -LiteralPath $remoteDebuggerPath)) {
    throw "Visual Studio Remote Debugger was not found: $remoteDebuggerPath"
}

# ---------------------------------------------------------------------------
# Trust the DisplayMagician test signing certificate
# ---------------------------------------------------------------------------

Write-Host ''
Write-Host 'Trusting DisplayMagician test signing certificate...' -ForegroundColor Cyan

$sourceCertificate = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(
    $testCertificatePath
)

$alreadyTrusted = Get-ChildItem 'Cert:\LocalMachine\TrustedPeople' -ErrorAction SilentlyContinue |
    Where-Object { $_.Thumbprint -eq $sourceCertificate.Thumbprint } |
    Select-Object -First 1

if ($alreadyTrusted) {
    Write-Host "  Certificate already trusted: $($sourceCertificate.Thumbprint)" -ForegroundColor Green
}
else {
    $importedCertificate = Import-Certificate `
        -FilePath $testCertificatePath `
        -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople'

    if ($null -eq $importedCertificate) {
        throw 'Failed to import the DisplayMagician test signing certificate.'
    }

    Write-Host "  Certificate trusted: $($sourceCertificate.Thumbprint)" -ForegroundColor Green
}

# ---------------------------------------------------------------------------
# Create useful desktop files
# ---------------------------------------------------------------------------

@(
    '[InternetShortcut]',
    'URL=https://github.com/terrymacdonald/DisplayMagician/releases'
) | Set-Content -LiteralPath $releaseShortcutPath -Encoding ASCII

@(
    'DisplayMagician Windows Sandbox Debugging',
    '',
    "Debug Bundle: $BundlePath",
    '',
    'The DisplayMagician test signing certificate has been imported into:',
    '  LocalMachine\TrustedPeople',
    '',
    'The Debug Bundle is launched automatically when the Sandbox starts.',
    '',
    'Remote debugging:',
    '1. Configure the elevated Visual Studio Remote Debugger window to use Windows Authentication.',
    '2. In Visual Studio on the host, use Debug > Attach to Process with Connection type Remote (Windows).',
    '3. Attach to DisplayMagician.exe, DisplayMagician.UserAgent.exe, DisplayMagician.ControlService.exe, DisplayMagician.SessionLauncher.exe, or DisplayMagicianConsole.exe.',
    '',
    'Use the DisplayMagician Releases desktop shortcut to download an earlier version for upgrade testing.',
    'The mapped Bundle, Sandbox scripts, certificate, and Remote Debugger tools are read-only host mappings.'
) | Set-Content -LiteralPath $instructionsPath -Encoding UTF8

# ---------------------------------------------------------------------------
# Start debugging tools
# ---------------------------------------------------------------------------

# Configure firewall rule and start the Visual Studio Remote Debugger
New-NetFirewallRule `
    -DisplayName 'Visual Studio Remote Debugger' `
    -Direction Inbound `
    -Program $remoteDebuggerPath `
    -Protocol TCP `
    -LocalPort 4026 `
    -Action Allow `
    -Profile Any `
    -ErrorAction SilentlyContinue

Start-Process -FilePath $remoteDebuggerPath -Verb RunAs


# ---------------------------------------------------------------------------
# Start  installer
# ---------------------------------------------------------------------------
Start-Process -FilePath $BundlePath