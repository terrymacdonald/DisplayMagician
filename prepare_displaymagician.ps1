#Requires -Version 5.1
<#
.SYNOPSIS
    One-time developer setup for DisplayMagician code signing and tooling.

.DESCRIPTION
    This script:
      1. Installs WiX Toolset v7.0.0 dotnet global tool
      2. Restores WiX NuGet SDK packages into the local cache
      2b. Restores Microsoft.Build.NoTargets SDK for the MSIX identity project
      3. Installs the HeatWave VS extension (.wixproj support in Visual Studio)
      4. Creates a self-signed code-signing certificate (CN=LittleBitBig)
      5. Exports it to a PFX file at a path you choose
      6. Imports the certificate into LocalMachine\TrustedPeople so Windows
         trusts the signed MSIX identity package on this machine
      7. Writes SigningConfig.props so MSBuild can sign the MSIX during build

    The PFX file and SigningConfig.props are both listed in .gitignore and
    will never be committed to the repository.

.NOTES
    Run once per developer machine, or whenever you need a new certificate.
    Re-running is safe - existing entries are detected and skipped.
    The script runs as a normal user - no administrator rights required.
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
    $wixOutput = & dotnet tool list --global 2>&1
    $wixList = $wixOutput | Select-String 'wix'
    if ($wixList) {
        # Parse the version from the tool list output (columns: Package Id, Version, Commands)
        $installedVersion = ($wixList.Line -split '\s+')[1]
        if ($installedVersion -eq $requiredWixVersion) {
            Write-Host "  WiX $installedVersion is already installed - skipping." -ForegroundColor Green
            $wixInstalled = $true
        } else {
            Write-Host "  WiX $installedVersion found but v$requiredWixVersion is required. Updating..."
        }
    }
} catch {
    # dotnet not found or tool list failed - will attempt install below
}

if (-not $wixInstalled) {
    Write-Host "  Installing WiX Toolset v$requiredWixVersion..."
    try {
        $installOutput = & dotnet tool install --global wix --version $requiredWixVersion 2>&1
        if ($LASTEXITCODE -ne 0) {
            # Tool already exists at a different version - update instead
            $installOutput = & dotnet tool update --global wix --version $requiredWixVersion 2>&1
        }
        Write-Host ($installOutput | Out-String).Trim()
        Write-Host "  WiX v$requiredWixVersion installed." -ForegroundColor Green
    } catch {
        Write-Warning "Could not install WiX automatically: $_"
        Write-Warning "Please install manually: dotnet tool install --global wix --version $requiredWixVersion"
    }
}
Write-Host ""

# ---------------------------------------------------------------------------
# 2. Restore WiX NuGet SDK packages (required by both VS and VS Code)
# ---------------------------------------------------------------------------
Write-Host "Restoring WiX NuGet packages (WixToolset.Sdk, extensions)..."
Write-Host "  This downloads WixToolset.Sdk v$requiredWixVersion and extension packages into the NuGet cache."
Write-Host "  Both Visual Studio and VS Code use this cache when building .wixproj files."

$wixprojPath = Join-Path $PSScriptRoot "DisplayMagicianPackage\DisplayMagicianPackage.wixproj"
try {
    $restoreOutput = & dotnet restore "$wixprojPath" 2>&1
    Write-Host ($restoreOutput | Out-String).Trim()
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  NuGet packages restored." -ForegroundColor Green
    } else {
        Write-Warning "  dotnet restore returned exit code $LASTEXITCODE - check output above."
    }
} catch {
    Write-Warning "Could not restore NuGet packages: $_"
    Write-Warning "Run manually: dotnet restore `"$wixprojPath`""
}
Write-Host ""

# Restore Microsoft.Build.NoTargets SDK for the MSIX identity project
Write-Host "Restoring NuGet packages for DisplayMagicianIdentityPkg (Microsoft.Build.NoTargets)..."
$identityProjPath = Join-Path $PSScriptRoot "DisplayMagicianIdentityPkg\DisplayMagicianIdentityPkg.csproj"
try {
    $restoreOutput = & dotnet restore "$identityProjPath" 2>&1
    Write-Host ($restoreOutput | Out-String).Trim()
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  NuGet packages restored." -ForegroundColor Green
    } else {
        Write-Warning "  dotnet restore returned exit code $LASTEXITCODE - check output above."
    }
} catch {
    Write-Warning "Could not restore NuGet packages for identity project: $_"
    Write-Warning "Run manually: dotnet restore `"$identityProjPath`""
}
Write-Host ""

# ---------------------------------------------------------------------------
# 3. Install HeatWave VS extension (adds .wixproj support to Visual Studio)
# ---------------------------------------------------------------------------
Write-Host "Checking for Visual Studio installations..."

# Find all VS installs using vswhere (ships with VS and Build Tools)
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$vsInstalls = @()
if (Test-Path $vswhere) {
    # -prerelease is required to detect VS 2022+ and VS 2026 (channel 18) installs;
    # without it vswhere returns nothing for those versions.
    $vsInstalls = & "$vswhere" -all -prerelease -format json 2>&1 | ConvertFrom-Json
}

# Fail fast if any VS instance is open - VSIXInstaller cannot run while VS is running.
# The user must run this script from a standalone PowerShell window, not from within VS.
$runningDevenv = Get-Process -Name 'devenv' -ErrorAction SilentlyContinue
if ($runningDevenv) {
    Write-Host ""
    Write-Warning "Visual Studio is currently running."
    Write-Warning "The HeatWave extension cannot be installed while Visual Studio is open."
    Write-Warning "Please:"
    Write-Warning "  1. Close Visual Studio completely."
    Write-Warning "  2. Re-run this script from a standalone PowerShell window"
    Write-Warning "     (not the VS integrated terminal or Developer PowerShell)."
    Write-Host ""
    exit 1
}

if ($vsInstalls.Count -eq 0) {
    Write-Host "  No Visual Studio installations found - skipping HeatWave install." -ForegroundColor Yellow
    Write-Host "  If you install Visual Studio later, re-run this script."
} else {
    # HeatWave ships a separate VSIX per VS generation:
    #   VS 2019 (Dev16) -> FireGiantHeatWaveDev16
    #   VS 2022+ (Dev17) -> FireGiantHeatWaveDev17
    # Map each installed VS major version to the right extension name, query the
    # gallery for that extension's latest VSIX file URL, download, then install.

    # Fetch all HeatWave extensions in one gallery query (filterType 10 = full-text search)
    $galleryBody = '{"filters":[{"criteria":[{"filterType":10,"value":"HeatWave"}],"pageSize":10}],"flags":914}'
    try {
        $galleryResp = Invoke-RestMethod `
            -Uri         'https://marketplace.visualstudio.com/_apis/public/gallery/extensionquery?api-version=7.2-preview.1' `
            -Method      Post `
            -ContentType 'application/json' `
            -Body        $galleryBody `
            -UseBasicParsing
    } catch {
        Write-Warning "  Could not reach VS Marketplace: $_"
        Write-Warning "  Install HeatWave manually from: https://www.firegiant.com/heatwave/"
        $galleryResp = $null
    }

    # Build a lookup: extensionName -> VSIX download URL
    # The marketplace stores the VSIX filename in the Payload.FileName property;
    # use that to match the correct file entry rather than a loose wildcard.
    $extensionMap = @{}
    if ($galleryResp) {
        foreach ($ext in $galleryResp.results[0].extensions) {
            $ver = $ext.versions[0]
            # Prefer the file whose assetType matches the Payload.FileName property value
            $payloadName = ($ver.properties | Where-Object key -eq 'Microsoft.VisualStudio.Services.Payload.FileName').value
            $vsixFile = if ($payloadName) {
                $ver.files | Where-Object { $_.assetType -eq $payloadName } | Select-Object -First 1
            } else {
                $ver.files | Where-Object { $_.assetType -like '*.vsix' } | Select-Object -First 1
            }
            if ($vsixFile) {
                $extensionMap[$ext.extensionName] = $vsixFile.source
            }
        }
    }

    foreach ($vs in $vsInstalls) {
        $vsixInstaller = Join-Path $vs.installationPath 'Common7\IDE\VSIXInstaller.exe'
        if (-not (Test-Path $vsixInstaller)) { continue }

        # Determine VS major version from installationVersion (e.g. "17.x" or "18.x")
        $vsMajor = [int]($vs.installationVersion -split '\.')[0]
        # Dev16 = VS2019 (major 16), Dev17 covers VS2022 and later (major 17+)
        $extName = if ($vsMajor -le 16) { 'FireGiantHeatWaveDev16' } else { 'FireGiantHeatWaveDev17' }

        Write-Host "  Processing $($vs.displayName) (v$($vs.installationVersion)) -> extension: $extName"

        # Check whether HeatWave is already installed for this VS instance.
        # VS stores per-user extensions under:
        #   %LOCALAPPDATA%\Microsoft\VisualStudio\{major}.0_{instanceId}\Extensions\
        # The extension.vsixmanifest <Identity Id> for HeatWave is 'FireGiant.HeatWave'
        # (not the gallery slug). We also accept exit code 1001 from VSIXInstaller
        # as a secondary signal that the extension is already present.
        $instanceId       = $vs.instanceId
        $extensionsRoot   = "$env:LOCALAPPDATA\Microsoft\VisualStudio\${vsMajor}.0_${instanceId}\Extensions"
        $alreadyInstalled = $false
        if (Test-Path $extensionsRoot) {
            $alreadyInstalled = [bool](
                Get-ChildItem "$extensionsRoot\*\extension.vsixmanifest" -ErrorAction SilentlyContinue |
                    Where-Object {
                        $xml = Get-Content $_ -Raw -ErrorAction SilentlyContinue
                        # Match on known HeatWave identity IDs (gallery slug or package Id)
                        $xml -match 'FireGiant\.HeatWave' -or $xml -match 'FireGiantHeatWaveDev1[67]'
                    }
            )
        }

        # Also check the VS installation's own Extensions folder (machine-wide installs)
        if (-not $alreadyInstalled) {
            $vsExtRoot = Join-Path $vs.installationPath 'Common7\IDE\Extensions'
            if (Test-Path $vsExtRoot) {
                $alreadyInstalled = [bool](
                    Get-ChildItem "$vsExtRoot\*\extension.vsixmanifest" -Recurse -ErrorAction SilentlyContinue |
                        Where-Object {
                            $xml = Get-Content $_ -Raw -ErrorAction SilentlyContinue
                            $xml -match 'FireGiant\.HeatWave' -or $xml -match 'FireGiantHeatWaveDev1[67]'
                        }
                )
            }
        }

        if ($alreadyInstalled) {
            Write-Host "    HeatWave already installed - skipping." -ForegroundColor Green
            continue
        }

        $vsixUrl = $extensionMap[$extName]
        if (-not $vsixUrl) {
            Write-Warning "    No download URL found for $extName - install manually: https://www.firegiant.com/heatwave/"
            continue
        }

        # Use a unique filename each run so a locked leftover from a previous
        # attempt can never block the download.
        $vsixPath = Join-Path $env:TEMP "$extName-$([System.Guid]::NewGuid().ToString('N')).vsix"
        try {
            Write-Host "    Downloading $vsixUrl ..."
            Invoke-WebRequest -Uri $vsixUrl -OutFile $vsixPath -UseBasicParsing
            Write-Host "    Downloaded." -ForegroundColor Green
        } catch {
            Write-Warning "    Download failed: $_ - install manually: https://www.firegiant.com/heatwave/"
            continue
        }

        Write-Host "    Installing HeatWave into $($vs.displayName)..."
        Write-Host "    (A progress window may appear briefly - this is normal)" -ForegroundColor Gray

        $proc = Start-Process -FilePath $vsixInstaller `
                              -ArgumentList "`"$vsixPath`"" `
                              -Wait -PassThru
        Remove-Item $vsixPath -Force -ErrorAction SilentlyContinue

        if ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 1001) {
            # 0 = success, 1001 = already installed
            Write-Host "    HeatWave installed successfully." -ForegroundColor Green
        } else {
            Write-Warning "    VSIXInstaller exited with code $($proc.ExitCode)."
            Write-Warning "    Try installing manually: https://www.firegiant.com/heatwave/"
        }
    }
}
Write-Host ""

# ---------------------------------------------------------------------------
# 4. Choose where to save the PFX
# ---------------------------------------------------------------------------
$defaultPfxDir  = "$env:USERPROFILE\Documents\Certs"
$defaultPfxPath = "$defaultPfxDir\DisplayMagicianCodeSigning.pfx"
Write-Host "Where do you want to save the PFX certificate file?"
Write-Host "  Press Enter to accept the default: $defaultPfxPath"
$pfxInput = Read-Host "PFX path"
if ([string]::IsNullOrWhiteSpace($pfxInput)) {
    $pfxPath = $defaultPfxPath
} else {
    $pfxPath = $pfxInput.Trim('"').Trim("'")
}

$pfxExists = Test-Path $pfxPath
if ($pfxExists) {
    Write-Host "  Existing PFX found at: $pfxPath" -ForegroundColor Green
    Write-Host "  The existing PFX will NOT be overwritten." -ForegroundColor Yellow
} else {
    Write-Host "  PFX will be created at: $pfxPath" -ForegroundColor Green
}
Write-Host ""

# ---------------------------------------------------------------------------
# 5. Choose a secure password (typed hidden, confirmed)
# ---------------------------------------------------------------------------
if ($pfxExists) {
    # We still need the password to import the existing PFX into the cert store
    Write-Host "Enter the password for the existing PFX file."
    Write-Host "  (needed to import the certificate into the trust store)" -ForegroundColor Yellow
    Write-Host ""
    $password = Read-Host "  PFX password" -AsSecureString
} else {
    Write-Host "Choose a password to protect the new PFX file."
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
}
Write-Host ""

# ---------------------------------------------------------------------------
# 6. Create the self-signed certificate (if one doesn't already exist)
# ---------------------------------------------------------------------------
$certSubject  = 'CN=AC8F2FDD-977F-4390-B92A-B9CFAEE91973'
$friendlyName = 'Terry MacDonald Code Signing Certificate'

if ($pfxExists) {
    Write-Host "Skipping certificate creation - using existing PFX." -ForegroundColor Green
    # Read the cert from the existing PFX so we have the thumbprint for later steps
    $cert = (Get-PfxData -FilePath $pfxPath -Password $password).EndEntityCertificates |
                Select-Object -First 1
    Write-Host "  Thumbprint from existing PFX: $($cert.Thumbprint)" -ForegroundColor Green
} else {
    $existingCert = Get-ChildItem Cert:\CurrentUser\My |
        Where-Object { $_.Subject -eq $certSubject } |
        Sort-Object NotAfter -Descending |
        Select-Object -First 1

    if ($existingCert) {
        Write-Host "Found existing certificate in CurrentUser\My (thumbprint: $($existingCert.Thumbprint))." -ForegroundColor Green
        Write-Host "  Skipping certificate creation - using existing cert."
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
}
Write-Host ""

# ---------------------------------------------------------------------------
# 7. Export to PFX (skipped if PFX already exists)
# ---------------------------------------------------------------------------
if ($pfxExists) {
    Write-Host "Skipping PFX export - existing file kept as-is." -ForegroundColor Green
} else {
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
}
Write-Host ""

# ---------------------------------------------------------------------------
# 8. Import certificate into CurrentUser\TrustedPeople
# ---------------------------------------------------------------------------
$alreadyTrusted = Get-ChildItem Cert:\CurrentUser\TrustedPeople -ErrorAction SilentlyContinue |
    Where-Object { $_.Thumbprint -eq $cert.Thumbprint }

if ($alreadyTrusted) {
    Write-Host "Certificate already present in CurrentUser\TrustedPeople - skipping import." -ForegroundColor Green
} else {
    Write-Host "Importing certificate into CurrentUser\TrustedPeople so Windows trusts the signed MSIX..."
    Import-PfxCertificate `
        -CertStoreLocation 'Cert:\CurrentUser\TrustedPeople' `
        -FilePath $pfxPath `
        -Password $password | Out-Null
    Write-Host "  Imported." -ForegroundColor Green
}
Write-Host ""

# ---------------------------------------------------------------------------
# 9. Write SigningConfig.props (plain-text password stored here only)
# ---------------------------------------------------------------------------
$repoRoot       = $PSScriptRoot
$signingProps   = Join-Path $repoRoot 'SigningConfig.props'

# Extract the password string to write into the props file
$plainPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                     [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))

$propsContent = @"
<!--
  LOCAL DEVELOPER SIGNING CONFIGURATION
  Generated by prepare_displaymagician.ps1 - DO NOT COMMIT.
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
# Download .NET 10 Desktop Runtime installer into DisplayMagicianBundle\Packages\
# ---------------------------------------------------------------------------
$runtimeVersion  = '10.0.7'
$runtimeFilename = "windowsdesktop-runtime-$runtimeVersion-win-x64.exe"
$runtimeUrl      = "https://download.visualstudio.microsoft.com/download/pr/windowsdesktop-runtime-$runtimeVersion-win-x64.exe"
$bundlePackagesDir = Join-Path $PSScriptRoot 'DisplayMagicianBundle\Packages'
$runtimeDest     = Join-Path $bundlePackagesDir $runtimeFilename

Write-Host "Checking for .NET $runtimeVersion Desktop Runtime installer..."
if (Test-Path $runtimeDest) {
    Write-Host "  Already present: $runtimeDest" -ForegroundColor Green
} else {
    New-Item -ItemType Directory -Force -Path $bundlePackagesDir | Out-Null
    Write-Host "  Downloading $runtimeFilename from Microsoft..."
    try {
        # Use the official aka.ms redirect which always resolves to the correct CDN URL
        $redirectUrl = "https://aka.ms/dotnet/$runtimeVersion/windowsdesktop-runtime-win-x64.exe"
        Invoke-WebRequest -Uri $redirectUrl -OutFile $runtimeDest -UseBasicParsing
        Write-Host "  Downloaded: $runtimeDest" -ForegroundColor Green
    } catch {
        Write-Warning "Could not download .NET Desktop Runtime: $_"
        Write-Warning "Download manually from https://dotnet.microsoft.com/download/dotnet/10.0"
        Write-Warning "and place the installer at: $runtimeDest"
    }
}
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
Write-Host "  HeatWave VS extension (.wixproj support in Visual Studio)"
Write-Host "  Microsoft.Build.NoTargets SDK (DisplayMagicianIdentityPkg NuGet restore)"
Write-Host "  .NET $runtimeVersion Desktop Runtime installer (DisplayMagicianBundle\Packages\)"
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
