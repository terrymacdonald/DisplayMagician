param (
    [string]$outputDir = (Get-Location),
    [string]$Configuration = "Debug"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Resolve paths relative to this script, not the caller's current directory.
$scriptDir = $PSScriptRoot

$assemblyPath = Join-Path `
    -Path $scriptDir `
    -ChildPath "..\DisplayMagician\bin\$Configuration\DisplayMagician.dll"

$assemblyPath = [System.IO.Path]::GetFullPath($assemblyPath)

if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "DisplayMagician assembly was not found: $assemblyPath"
}

$fileVersionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($assemblyPath)
$versionInfo = $fileVersionInfo.FileVersion

if ([string]::IsNullOrWhiteSpace($versionInfo)) {
    throw "Could not determine the DisplayMagician file version from: $assemblyPath"
}

$outputDir = $outputDir.Trim().Trim('"').TrimEnd('\')
$outputDir = [System.IO.Path]::GetFullPath($outputDir)

Write-Host "OutputDir passed was '$outputDir'"

$sourcePath = Join-Path $outputDir 'DisplayMagicianSetup.exe'
$newSetupFileName = "DisplayMagicianSetup_v$versionInfo.exe"
$destinationPath = Join-Path $outputDir $newSetupFileName

if (-not (Test-Path -LiteralPath $sourcePath)) {
    throw "Bundle output was not found: $sourcePath"
}

# Remove an existing versioned output if present.
if (Test-Path -LiteralPath $destinationPath) {
    Remove-Item `
        -LiteralPath $destinationPath `
        -Force `
        -ErrorAction Stop
}

# WiX/MSBuild may briefly retain a handle to the bundle after the build.
# Retry the rename until the handle is released.
$maxAttempts = 40
$delayMilliseconds = 250

for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
    try {
        Move-Item `
            -LiteralPath $sourcePath `
            -Destination $destinationPath `
            -Force `
            -ErrorAction Stop

        Write-Host "Renamed '$sourcePath' to '$newSetupFileName'"
        return
    }
    catch {
        if ($attempt -eq $maxAttempts) {
            throw "Could not rename '$sourcePath' after $maxAttempts attempts. Last error: $($_.Exception.Message)"
        }

        Start-Sleep -Milliseconds $delayMilliseconds
    }
}