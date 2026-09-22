[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDirectory,

    [Parameter(Mandatory = $true)]
    [string]$ProjectName
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$components = @{
    'DisplayMagician' = 'DesktopApp'
    'DisplayMagicianConsole' = 'DesktopConsole'
    'DisplayMagician.UserAgent' = 'UserAgent'
    'DisplayMagician.ControlService' = 'ControlService'
    'DisplayMagician.SessionLauncher' = 'SessionLauncher'
}

if (-not $components.ContainsKey($ProjectName))
{
    exit 0
}

$programPath = Join-Path $ProjectDirectory 'Program.cs'
if (-not (Test-Path -LiteralPath $programPath))
{
    throw "Support logging verification could not find $programPath."
}

$source = [System.IO.File]::ReadAllText($programPath)
$component = $components[$ProjectName]
$expectedLayout = '${displaymagicianlog:component=' + $component + '}'
if (-not $source.Contains('SupportLogLayout.Register();'))
{
    throw "$ProjectName must register SupportLogLayout before configuring its retained log target."
}

if (-not $source.Contains($expectedLayout))
{
    throw "$ProjectName must configure its retained log target with $expectedLayout."
}
