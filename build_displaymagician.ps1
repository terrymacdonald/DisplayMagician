<#
.SYNOPSIS
    Builds the full DisplayMagician solution from the command line.

.DESCRIPTION
    Builds the entire DisplayMagician.sln using MSBuild, which respects the
    project dependency order defined in the solution file.

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
# Clean, restore + build the solution (project order is determined by solution dependencies)
# ---------------------------------------------------------------------------
Invoke-Step "Clean DisplayMagician.sln" {
    $sln = Join-Path $root 'DisplayMagician.sln'
    & $msbuild $sln -t:Clean -p:Configuration=$Configuration -p:Platform=$Platform -nologo -v:minimal
}

Invoke-Step "Restore DisplayMagician.sln" {
    $sln = Join-Path $root 'DisplayMagician.sln'
    & $msbuild $sln -t:Restore -nologo -v:minimal
}

Invoke-Step "Build DisplayMagician.sln ($Configuration|$Platform)" {
    $sln = Join-Path $root 'DisplayMagician.sln'
    & $msbuild $sln -t:Build -p:Configuration=$Configuration -p:Platform=$Platform -nologo -v:minimal
}

Write-Host ""
Write-Host "Build complete: $Configuration|$Platform" -ForegroundColor Green
