[CmdletBinding()]
param(
    [string]$GatewayUri,
    [switch]$PrepareSandbox,
    [string]$ResultsPath = (Join-Path $PSScriptRoot '.tools/RestP3Harness/results.json')
)

$ErrorActionPreference = 'Stop'
if (-not $PrepareSandbox -and -not $GatewayUri) { throw 'Pass -GatewayUri with the running registered HTTPS origin, or use -PrepareSandbox.' }
$project = Join-Path $PSScriptRoot 'BuildScripts/RestP3Harness/RestP3Harness.csproj'
$output = Join-Path $PSScriptRoot '.tools/RestP3Harness/publish'
& dotnet publish $project -c Release -r win-x64 --self-contained false -o $output --verbosity quiet
if ($LASTEXITCODE -ne 0) { throw 'Could not build the installed REST test harness.' }
if ($PrepareSandbox) {
    $staging = Join-Path $PSScriptRoot 'Sandbox/Local/RestP3Harness'
    New-Item -ItemType Directory -Path $staging -Force | Out-Null
    Copy-Item -Path (Join-Path $output '*') -Destination $staging -Recurse -Force
    Write-Output 'Harness staged. Start Windows Sandbox through debug_displaymagician.ps1, install the current Bundle, then follow Sandbox/REST_P3_TEST_PLAN.md.'
    return
}
& (Join-Path $output 'RestP3Harness.exe') $GatewayUri $ResultsPath
if ($LASTEXITCODE -ne 0) { throw "Installed REST P3 checks failed. Review the sanitized report at $ResultsPath" }
