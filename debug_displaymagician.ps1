#Requires -Version 5.1
#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Publishes and deploys Debug runtime payloads into an existing DisplayMagician installation.

.DESCRIPTION
    This script is the fast iteration path for debugging the installed v4 stack.
    It publishes the desktop client, Control Service, Session Launcher, and User
    Agent without building WiX, the bundle, or the sparse MSIX package. It then
    copies the Debug payloads into the installed folders, starts both services,
    and launches the installed desktop client.

    Use build_displaymagician.ps1 for full installer verification. Run this
    script only after a Debug MSI has established the installed layout.

.PARAMETER NoLaunch
    Deploy and start the services without launching DisplayMagician.exe.
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [switch] $NoLaunch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Find-MSBuild {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $msbuildPath = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' 2>$null |
            Select-Object -First 1
        if ($msbuildPath -and (Test-Path $msbuildPath)) {
            return $msbuildPath
        }
    }

    throw 'MSBuild.exe was not found. Install Visual Studio with the MSBuild component.'
}

function Publish-Project {
    param(
        [string] $ProjectPath,
        [string] $PublishDirectory,
        [string] $Platform,
        [string] $ExpectedExecutable,
        [string] $PublishProfile
    )

    New-Item -ItemType Directory -Path $PublishDirectory -Force | Out-Null
    $properties = "Configuration=Debug;Platform=$Platform;RuntimeIdentifier=win-x64;SelfContained=false;PublishDir=$PublishDirectory\"
    if (-not [string]::IsNullOrWhiteSpace($PublishProfile)) {
        $properties = "$properties;PublishProfile=$PublishProfile"
    }

    Write-Host "Publishing $(Split-Path $ProjectPath -Leaf)..." -ForegroundColor Cyan
    & $script:msbuild $ProjectPath '-t:Publish' "-p:$properties" '-nologo' '-v:minimal'
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed for $ProjectPath with exit code $LASTEXITCODE."
    }

    $executablePath = Join-Path $PublishDirectory $ExpectedExecutable
    if (-not (Test-Path $executablePath)) {
        throw "Publish output is missing $executablePath."
    }
}

$root = $PSScriptRoot
$msbuild = Find-MSBuild
$stageRoot = Join-Path $root 'output\debug-deployment\publish'

$installedProperties = Get-ItemProperty -Path 'HKLM:\SOFTWARE\DisplayMagician' -ErrorAction SilentlyContinue
if ($null -eq $installedProperties) {
    throw 'DisplayMagician v4 or higher is not installed. Build and install the Debug MSI before running this script.'
}

$installRoot = [string] $installedProperties.InstalledDir
if ([string]::IsNullOrWhiteSpace($installRoot) -or -not (Test-Path $installRoot)) {
    throw 'DisplayMagician v4 or higher is not installed. Build and install the Debug MSI before running this script.'
}

$desktopDirectory = $installRoot
$controlServiceDirectory = Join-Path $installRoot 'ControlService'
$sessionLauncherDirectory = Join-Path $installRoot 'SessionLauncher'
$userAgentDirectory = Join-Path $installRoot 'UserAgent'

foreach ($installedPath in @($desktopDirectory, $controlServiceDirectory, $sessionLauncherDirectory, $userAgentDirectory)) {
    if (-not (Test-Path $installedPath)) {
        throw "The DisplayMagician v4 installation is incomplete because this required component directory is missing: $installedPath. Reinstall the Debug MSI before running this script."
    }
}

$runningProcesses = @(Get-Process -Name 'DisplayMagician', 'DisplayMagician.UserAgent' -ErrorAction SilentlyContinue)
if ($runningProcesses.Count -gt 0) {
    $processNames = ($runningProcesses | Select-Object -ExpandProperty ProcessName -Unique) -join ', '
    throw "Close the running DisplayMagician desktop and User Agent processes before deploying Debug files: $processNames"
}

if (Test-Path $stageRoot) {
    Remove-Item -Path $stageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $stageRoot -Force | Out-Null

Publish-Project (Join-Path $root 'DisplayMagician\DisplayMagician.csproj') (Join-Path $stageRoot 'DisplayMagician') 'AnyCPU' 'DisplayMagician.exe' 'DebugPublishProfile'
Publish-Project (Join-Path $root 'DisplayMagician.ControlService\DisplayMagician.ControlService.csproj') (Join-Path $stageRoot 'ControlService') 'x64' 'DisplayMagician.ControlService.exe' ''
Publish-Project (Join-Path $root 'DisplayMagician.SessionLauncher\DisplayMagician.SessionLauncher.csproj') (Join-Path $stageRoot 'SessionLauncher') 'x64' 'DisplayMagician.SessionLauncher.exe' ''
Publish-Project (Join-Path $root 'DisplayMagician.UserAgent\DisplayMagician.UserAgent.csproj') (Join-Path $stageRoot 'UserAgent') 'x64' 'DisplayMagician.UserAgent.exe' ''

foreach ($serviceName in @('DisplayMagicianControlService', 'DisplayMagicianSessionLauncher')) {
    $service = Get-Service -Name $serviceName -ErrorAction Stop
    if ($service.Status -ne 'Stopped' -and $PSCmdlet.ShouldProcess($serviceName, 'Stop service')) {
        Write-Host "Stopping $serviceName..." -ForegroundColor Cyan
        Stop-Service -Name $serviceName -ErrorAction Stop
        $service.WaitForStatus('Stopped', [TimeSpan]::FromSeconds(30))
    }
}

if ($PSCmdlet.ShouldProcess($desktopDirectory, 'Copy Debug desktop payload')) {
    Get-ChildItem -Path (Join-Path $stageRoot 'DisplayMagician') -Force | Copy-Item -Destination $desktopDirectory -Recurse -Force
}
if ($PSCmdlet.ShouldProcess($controlServiceDirectory, 'Copy Debug Control Service payload')) {
    Get-ChildItem -Path (Join-Path $stageRoot 'ControlService') -Force | Copy-Item -Destination $controlServiceDirectory -Recurse -Force
}
if ($PSCmdlet.ShouldProcess($sessionLauncherDirectory, 'Copy Debug Session Launcher payload')) {
    Get-ChildItem -Path (Join-Path $stageRoot 'SessionLauncher') -Force | Copy-Item -Destination $sessionLauncherDirectory -Recurse -Force
}
if ($PSCmdlet.ShouldProcess($userAgentDirectory, 'Copy Debug User Agent payload')) {
    Get-ChildItem -Path (Join-Path $stageRoot 'UserAgent') -Force | Copy-Item -Destination $userAgentDirectory -Recurse -Force
}

foreach ($serviceName in @('DisplayMagicianControlService', 'DisplayMagicianSessionLauncher')) {
    $service = Get-Service -Name $serviceName -ErrorAction Stop
    if ($service.Status -ne 'Running' -and $PSCmdlet.ShouldProcess($serviceName, 'Start service')) {
        Write-Host "Starting $serviceName..." -ForegroundColor Cyan
        Start-Service -Name $serviceName -ErrorAction Stop
        $service.WaitForStatus('Running', [TimeSpan]::FromSeconds(30))
    }
}

if (-not $NoLaunch -and $PSCmdlet.ShouldProcess($desktopDirectory, 'Launch installed DisplayMagician')) {
    Start-Process -FilePath (Join-Path $desktopDirectory 'DisplayMagician.exe') -ArgumentList '--debug' -WorkingDirectory $desktopDirectory
}

Write-Host ''
Write-Host 'Debug deployment complete.' -ForegroundColor Green
Write-Host 'Attach Visual Studio to DisplayMagician.exe, DisplayMagician.UserAgent.exe, and DisplayMagician.ControlService.exe as needed.' -ForegroundColor Yellow