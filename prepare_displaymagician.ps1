#Requires -RunAsAdministrator
<#
.SYNOPSIS
    One-time developer setup for DisplayMagician code signing and tooling.

.DESCRIPTION
    This script:
      1. Installs WiX Toolset v7.0.0 dotnet global tool
      2. Creates a self-signed code-signing certificate (CN=LittleBitBig)
      3. Exports it to a PFX file at a path you choose
      4. Imports the certificate into LocalMachine\TrustedPeople so Windows
         trusts the signed MSIX identity package on this machine
      5. Writes SigningConfig.props so MSBuild can sign the MSIX during build

    The PFX file and SigningConfig.props are both listed in .gitignore and
    will never be committed to the repository.

.NOTES
    Run once per developer machine, or whenever you need a new certificate.
    Re-running is safe — existing entries are detected and skipped.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "=== DisplayMagician Developer Setup ===" -ForegroundColor Cyan
Write-Host ""

# ---------------------------------------------------------------------------
# 1. Install WiX Toolset v7.0.0 dotnet global tool
# ---------------------------------------------------------------------------
$requiredWixVersion = '7.0.0'
Write-Host "Checking WiX Toolset dotnet global tool..."

$wixInstalled = $false
try {
    $wixList = & dotnet tool list --global 2>$null | Select-String 'wix'
    if ($wixList) {
        # Parse the version from the tool list output (columns: Package Id, Version, Commands)
        $installedVersion = ($wixList -split '\s+')[1]
        if ($installedVersion -eq $requiredWixVersion) {
            Write-Host "  WiX $installedVersion is already installed — skipping." -ForegroundColor Green
            $wixInstalled = $true
        } else {
            Write-Host "  WiX $installedVersion found but v$requiredWixVersion is required. Updating..."
        }
    }
} catch {
    # dotnet not found or tool list failed — will attempt install below
}

if (-not $wixInstalled) {
    Write-Host "  Installing WiX Toolset v$requiredWixVersion..."
    try {
        & dotnet tool install --global wix --version $requiredWixVersion 2>&1 | Write-Host
        if ($LASTEXITCODE -ne 0) {
            # Already installed at a different version — update instead
            & dotnet tool update --global wix --version $requiredWixVersion 2>&1 | Write-Host
        }
        Write-Host "  WiX v$requiredWixVersion installed." -ForegroundColor Green
    } catch {
        Write-Warning "Could not install WiX automatically: $_"
        Write-Warning "Please install manually: dotnet tool install --global wix --version $requiredWixVersion"
    }
}
Write-Host ""

# ---------------------------------------------------------------------------
# 2. Choose where to save the PFX
# ---------------------------------------------------------------------------
$defaultPfxPath = "$env:USERPROFILE\DisplayMagicianCodeSigning.pfx"
Write-Host "Where do you want to save the PFX certificate file?"
Write-Host "  Press Enter to accept the default: $defaultPfxPath"
$pfxInput = Read-Host "PFX path"
if ([string]::IsNullOrWhiteSpace($pfxInput)) {
    $pfxPath = $defaultPfxPath
} else {
    $pfxPath = $pfxInput.Trim('"').Trim("'")
}
Write-Host "  PFX will be saved to: $pfxPath" -ForegroundColor Green
Write-Host ""

# ---------------------------------------------------------------------------
# 3. Choose a secure password (typed hidden, confirmed)
# ---------------------------------------------------------------------------
Write-Host "Choose a password to protect the PFX file."
Write-Host "  The password is never stored in plain text anywhere." -ForegroundColor Yellow
Write-Host ""

do {
    $password  = Read-Host "  Enter PFX password" -AsSecureString
    $password2 = Read-Host "  Confirm PFX password" -AsSecureString

    $plain1 = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                  [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))
    $plain2 = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                  [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password2))

    if ($plain1 -ne $plain2) {
        Write-Warning "Passwords do not match. Please try again."
    }
} while ($plain1 -ne $plain2)

# Clear plain-text copies immediately
$plain1 = $null
$plain2 = $null
[GC]::Collect()

Write-Host ""

# ---------------------------------------------------------------------------
# 4. Create the self-signed certificate (if one doesn't already exist)
# ---------------------------------------------------------------------------
$certSubject  = 'CN=LittleBitBig'
$friendlyName = 'LittleBitBig Code Signing Certificate'

$existingCert = Get-ChildItem Cert:\CurrentUser\My |
    Where-Object { $_.Subject -eq $certSubject } |
    Sort-Object NotAfter -Descending |
    Select-Object -First 1

if ($existingCert) {
    Write-Host "Found existing certificate in CurrentUser\My (thumbprint: $($existingCert.Thumbprint))." -ForegroundColor Green
    Write-Host "  Skipping certificate creation — using existing cert."
    $cert = $existingCert
} else {
    Write-Host "Creating self-signed code-signing certificate ($certSubject)..."
    $cert = New-SelfSignedCertificate `
        -Type Custom `
        -KeyUsage DigitalSignature `
        -CertStoreLocation 'Cert:\CurrentUser\My' `
        -TextExtension @('2.5.29.37={text}1.3.6.1.5.5.7.3.3', '2.5.29.19={text}') `
        -Subject $certSubject `
        -FriendlyName $friendlyName
    Write-Host "  Created. Thumbprint: $($cert.Thumbprint)" -ForegroundColor Green
}
Write-Host ""

# ---------------------------------------------------------------------------
# 5. Export to PFX
# ---------------------------------------------------------------------------
$pfxDir = Split-Path $pfxPath -Parent
if (-not (Test-Path $pfxDir)) {
    New-Item $pfxDir -ItemType Directory -Force | Out-Null
}

Write-Host "Exporting PFX to $pfxPath ..."
Export-PfxCertificate `
    -Cert "Cert:\CurrentUser\My\$($cert.Thumbprint)" `
    -FilePath $pfxPath `
    -Password $password | Out-Null
Write-Host "  Exported." -ForegroundColor Green
Write-Host ""

# ---------------------------------------------------------------------------
# 6. Import into LocalMachine\TrustedPeople (requires elevation)
# ---------------------------------------------------------------------------
$alreadyTrusted = Get-ChildItem Cert:\LocalMachine\TrustedPeople |
    Where-Object { $_.Thumbprint -eq $cert.Thumbprint }

if ($alreadyTrusted) {
    Write-Host "Certificate already present in LocalMachine\TrustedPeople — skipping import." -ForegroundColor Green
} else {
    Write-Host "Importing certificate into LocalMachine\TrustedPeople so Windows trusts the signed MSIX..."
    Import-PfxCertificate `
        -CertStoreLocation 'Cert:\LocalMachine\TrustedPeople' `
        -FilePath $pfxPath `
        -Password $password | Out-Null
    Write-Host "  Imported." -ForegroundColor Green
}
Write-Host ""

# ---------------------------------------------------------------------------
# 7. Write SigningConfig.props (plain-text password stored here only)
# ---------------------------------------------------------------------------
$repoRoot       = $PSScriptRoot
$signingProps   = Join-Path $repoRoot 'SigningConfig.props'

# Extract the password string to write into the props file
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                     [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

$propsContent = @"
<!--
  LOCAL DEVELOPER SIGNING CONFIGURATION
  Generated by prepare_displaymagician.ps1 — DO NOT COMMIT.
  This file is listed in .gitignore.
-->
<Project>
  <PropertyGroup>
    <SigningCertificatePfx>$pfxPath</SigningCertificatePfx>
    <SigningCertificatePassword>$plainPassword</SigningCertificatePassword>
    <!-- Optional: set to a timestamp server URL for production builds, e.g. http://timestamp.digicert.com -->
    <SigningTimestampUrl></SigningTimestampUrl>
  </PropertyGroup>
</Project>
"@

# Overwrite immediately and clear the plain-text variable
$propsContent | Set-Content -Path $signingProps -Encoding UTF8
$plainPassword = $null
[GC]::Collect()

Write-Host "SigningConfig.props written to: $signingProps" -ForegroundColor Green
Write-Host "  (This file is gitignored and will not be committed.)" -ForegroundColor Yellow
Write-Host ""

# ---------------------------------------------------------------------------
# Done
# ---------------------------------------------------------------------------
Write-Host "=== Setup complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "You can now build DisplayMagicianPackage or DisplayMagicianBundle in Visual Studio"
Write-Host "and the MSIX identity package will be packed and signed automatically."
Write-Host ""
Write-Host "Tools installed:" -ForegroundColor White
Write-Host "  WiX Toolset v$requiredWixVersion (dotnet global tool)"
Write-Host ""
Write-Host "Files created/updated:" -ForegroundColor White
Write-Host "  $pfxPath"
Write-Host "  $signingProps"
Write-Host ""
Write-Host "REMINDER: Keep your PFX file safe. If you lose it you will need to re-run" -ForegroundColor Yellow
Write-Host "this script and reinstall your application on all test machines." -ForegroundColor Yellow
Write-Host ""
Write-Host "REMINDER: Keep your PFX file safe. If you lose it you will need to re-run" -ForegroundColor Yellow
Write-Host "this script and reinstall your application on all test machines." -ForegroundColor Yellow
Write-Host ""
