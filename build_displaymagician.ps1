<#
.SYNOPSIS
    Builds the full DisplayMagician installer chain from the command line.

.DESCRIPTION
    Runs in order:
      1. Build + pack the sparse MSIX identity package (DisplayMagicianIdentityPkg)
      2. Build + publish the app and create the MSI (DisplayMagicianPackage)
      3. Build the Burn bootstrapper (DisplayMagicianBundle)

    Mirrors what Visual Studio does when you build the solution with project
    dependencies configured.

.PARAMETER Configuration
    Debug or Release (default: Debug).

.PARAMETER Platform
    x64 or x86 (default: x64).

.EXAMPLE
    .\build_displaymagician.ps1
    .\build_displaymagician.ps1 -Configuration Release -Platform x64
#>
[CmdletBinding()]
param (
    [ValidateSet('Debug','Release')]
    [string] $Configuration = 'Debug',

    [ValidateSet('x64','x86')]
    [string] $Platform = 'x64'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot

# ---------------------------------------------------------------------------
# Locate VS MSBuild.exe (required for ResolveComReference in DisplayMagicianShared)
# ---------------------------------------------------------------------------
function Find-MSBuild {
    # Try vswhere first (present with VS 2017+)
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $vsPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' 2>$null |
                  Select-Object -First 1
        if ($vsPath -and (Test-Path $vsPath)) { return $vsPath }
    }
    # Fallback: well-known VS 2022/2026 paths
    $candidates = @(
        "${env:ProgramFiles}\Microsoft Visual Studio\2026\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2026\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2026\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
    )
    foreach ($c in $candidates) { if (Test-Path $c) { return $c } }
    throw "MSBuild.exe not found. Please run from a Visual Studio Developer PowerShell or install Visual Studio."
}

$msbuild = Find-MSBuild
Write-Host "Using MSBuild: $msbuild" -ForegroundColor DarkGray

function Invoke-Step {
    param([string] $Label, [scriptblock] $Action)
    Write-Host ""
    Write-Host "==> $Label" -ForegroundColor Cyan
    & $Action
    if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) {
        Write-Error "Step failed: $Label (exit code $LASTEXITCODE)"
        exit $LASTEXITCODE
    }
}

# ---------------------------------------------------------------------------
# 1. Build identity MSIX
# ---------------------------------------------------------------------------
Invoke-Step "Build DisplayMagicianIdentityPkg ($Configuration)" {
    $proj = Join-Path $root 'DisplayMagicianIdentityPkg\DisplayMagicianIdentityPkg.proj'
    & $msbuild $proj -t:Build -p:Configuration=$Configuration -p:Platform=$Platform -nologo -v:minimal
}

# ---------------------------------------------------------------------------
# 2. Restore + build the MSI (which publishes the app internally)
# ---------------------------------------------------------------------------
Invoke-Step "Restore DisplayMagicianPackage" {
    $proj = Join-Path $root 'DisplayMagicianPackage\DisplayMagicianPackage.wixproj'
    & $msbuild $proj -t:Restore -nologo -v:minimal
}

Invoke-Step "Build DisplayMagicianPackage ($Configuration|$Platform)" {
    $proj = Join-Path $root 'DisplayMagicianPackage\DisplayMagicianPackage.wixproj'
    & $msbuild $proj -t:Build -p:Configuration=$Configuration -p:Platform=$Platform -nologo -v:minimal
}

# ---------------------------------------------------------------------------
# 3. Build the Burn bootstrapper
# ---------------------------------------------------------------------------
Invoke-Step "Build DisplayMagicianBundle ($Configuration|$Platform)" {
    $proj = Join-Path $root 'DisplayMagicianBundle\DisplayMagicianBundle.wixproj'
    & $msbuild $proj -t:Build -p:Configuration=$Configuration -p:Platform=$Platform -nologo -v:minimal
}

Write-Host ""
Write-Host "Build complete: $Configuration|$Platform" -ForegroundColor Green
