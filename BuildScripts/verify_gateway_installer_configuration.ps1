[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$wixPath = Join-Path $repositoryRoot 'DisplayMagicianPackage\DisplayMagicianComponents.wxs'
$projectPath = Join-Path $repositoryRoot 'DisplayMagicianPackage\DisplayMagicianPackage.wixproj'
$wix = [System.IO.File]::ReadAllText($wixPath)
$project = [System.IO.File]::ReadAllText($projectPath)

$requiredWixFragments = @(
    'Name="DisplayMagicianGateway"',
    'Account="NT AUTHORITY\LocalService"',
    'Start="auto"',
    'FirstFailureActionType="restart"',
    'Port="22846"',
    'Protocol="tcp"',
    'Scope="any"',
    'Source="$(var.DmGatewayPublishDir)DisplayMagician.Gateway.exe"'
)

foreach ($fragment in $requiredWixFragments)
{
    if (-not $wix.Contains($fragment))
    {
        throw "Gateway installer configuration is missing: $fragment"
    }
}

if (-not $project.Contains('DisplayMagician.Gateway\DisplayMagician.Gateway.csproj'))
{
    throw 'The package project does not publish DisplayMagician.Gateway.'
}

Write-Host 'Gateway installer configuration is valid.' -ForegroundColor Green
