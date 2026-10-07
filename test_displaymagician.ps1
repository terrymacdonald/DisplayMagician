<#
.SYNOPSIS
    Builds and runs every DisplayMagician test project.

.DESCRIPTION
    Finds every *.Tests.csproj below the repository root, builds each project
    with Visual Studio MSBuild (required by projects with COM references), then
    runs it through dotnet test without rebuilding it.

.PARAMETER Configuration
    Debug or Release (default: Debug).

.PARAMETER Platform
    x64 or x86 (default: x64).

.EXAMPLE
    .\test_displaymagician.ps1
    .\test_displaymagician.ps1 -Configuration Release -Platform x64
#>
[CmdletBinding()]
param (
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [ValidateSet('x64', 'x86')]
    [string] $Platform = 'x64'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Find-MSBuild {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $path = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' 2>$null |
            Select-Object -First 1
        if ($path -and (Test-Path $path)) {
            return $path
        }
    }

    throw 'MSBuild.exe was not found. Install Visual Studio with MSBuild support or run from a Visual Studio Developer PowerShell.'
}

$root = $PSScriptRoot
$msbuild = Find-MSBuild
$testProjects = @(Get-ChildItem -Path $root -Recurse -Filter '*.Tests.csproj' -File | Sort-Object FullName)
if ($testProjects.Count -eq 0) {
    throw 'No DisplayMagician test projects were found.'
}

Write-Host "Using MSBuild: $msbuild" -ForegroundColor DarkGray
foreach ($testProject in $testProjects) {
    $relativePath = $testProject.FullName.Substring($root.Length).TrimStart('\')
    Write-Host "" 
    Write-Host "==> Build $relativePath ($Configuration|$Platform)" -ForegroundColor Cyan
    & $msbuild $testProject.FullName -t:Build -p:Configuration=$Configuration -p:Platform=$Platform -nologo -v:minimal
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host "==> Test $relativePath" -ForegroundColor Cyan
    & dotnet test $testProject.FullName --no-build --no-restore --configuration $Configuration --property:Platform=$Platform --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }
}

Write-Host "" 
Write-Host "All $($testProjects.Count) DisplayMagician test projects passed." -ForegroundColor Green
