#Requires -Version 5.1
#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Starts a local or Windows Sandbox DisplayMagician Debug workflow.

.DESCRIPTION
    The local workflow publishes Debug runtime payloads into an existing
    DisplayMagician installation. The Windows Sandbox workflow builds the
    Debug Bundle, generates a machine-specific Sandbox configuration, and
    verifies the local Debug signing certificate is available, then starts
    Sandbox with the Bundle, certificate, and Visual Studio Remote Debugger mapped
    read-only.

.PARAMETER NoLaunch
    In the local workflow, deploy and start the services without launching
    DisplayMagician.exe.
#>
[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [switch] $NoLaunch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Find-MSBuild {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"

    if (-not (Test-Path -LiteralPath $vswhere)) {
        throw 'Visual Studio Installer was not found. Install Visual Studio with the MSBuild component.'
    }

    $msbuildPath = & $vswhere `
        -latest `
        -prerelease `
        -requires Microsoft.Component.MSBuild `
        -find 'MSBuild\**\Bin\MSBuild.exe' 2>$null |
        Select-Object -First 1

    if ($msbuildPath -and (Test-Path -LiteralPath $msbuildPath)) {
        return $msbuildPath
    }

    throw 'MSBuild.exe was not found. Install Visual Studio with the MSBuild component.'
}

function Find-RemoteDebugger {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (-not (Test-Path -LiteralPath $vswhere)) {
        throw 'Visual Studio Installer was not found. Install Visual Studio with the Remote Debugger tools.'
    }

    $visualStudioPath = & $vswhere -latest -prerelease -requires Microsoft.Component.MSBuild -property installationPath 2>$null |
        Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($visualStudioPath)) {
        throw 'A Visual Studio installation with MSBuild could not be found.'
    }

    $remoteDebuggerPath = Join-Path $visualStudioPath 'Common7\IDE\Remote Debugger\x64'
    if (-not (Test-Path -LiteralPath (Join-Path $remoteDebuggerPath 'msvsmon.exe'))) {
        throw "Visual Studio Remote Debugger was not found at $remoteDebuggerPath. Install the Remote Debugger tools for this Visual Studio installation."
    }

    return $remoteDebuggerPath
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
    $properties = "Configuration=Debug;Platform=$Platform;RuntimeIdentifier=win-x64;SelfContained=false;PublishDir=$PublishDirectory\\"
    if (-not [string]::IsNullOrWhiteSpace($PublishProfile)) {
        $properties = "$properties;PublishProfile=$PublishProfile"
    }

    Write-Host "Publishing $(Split-Path $ProjectPath -Leaf)..." -ForegroundColor Cyan
    & $script:msbuild $ProjectPath '-t:Publish' "-p:$properties" '-nologo' '-v:minimal'
    if ($LASTEXITCODE -ne 0) {
        throw "Publish failed for $ProjectPath with exit code $LASTEXITCODE."
    }

    $executablePath = Join-Path $PublishDirectory $ExpectedExecutable
    if (-not (Test-Path -LiteralPath $executablePath)) {
        throw "Publish output is missing $executablePath."
    }
}

function Assert-WindowsSandboxAvailable {
    $sandboxFeature = Get-WindowsOptionalFeature -Online -FeatureName 'Containers-DisposableClientVM' -ErrorAction Stop
    if ($sandboxFeature.State -ne 'Enabled') {
        throw 'Windows Sandbox is not enabled. Enable the Windows Sandbox optional feature, restart Windows if prompted, and run this script again.'
    }
}

function Start-LocalDebugging {
    $stageRoot = Join-Path $script:root 'output\debug-deployment\publish'
    $installedProperties = Get-ItemProperty -Path 'HKLM:\SOFTWARE\DisplayMagician' -ErrorAction SilentlyContinue
    if ($null -eq $installedProperties) {
        throw 'DisplayMagician v4 or higher is not installed. Build and install the Debug MSI before running local debugging.'
    }

    $installRoot = [string] $installedProperties.InstalledDir
    if ([string]::IsNullOrWhiteSpace($installRoot) -or -not (Test-Path -LiteralPath $installRoot)) {
        throw 'DisplayMagician v4 or higher is not installed. Build and install the Debug MSI before running local debugging.'
    }

    $desktopDirectory = $installRoot
    $controlServiceDirectory = Join-Path $installRoot 'ControlService'
    $sessionLauncherDirectory = Join-Path $installRoot 'SessionLauncher'
    $userAgentDirectory = Join-Path $installRoot 'UserAgent'
    foreach ($installedPath in @($desktopDirectory, $controlServiceDirectory, $sessionLauncherDirectory, $userAgentDirectory)) {
        if (-not (Test-Path -LiteralPath $installedPath)) {
            throw "The DisplayMagician v4 installation is incomplete because this required component directory is missing: $installedPath. Reinstall the Debug MSI before running local debugging."
        }
    }

    $runningProcesses = @(Get-Process -Name 'DisplayMagician', 'DisplayMagician.UserAgent' -ErrorAction SilentlyContinue)
    if ($runningProcesses.Count -gt 0) {
        $processNames = ($runningProcesses | Select-Object -ExpandProperty ProcessName -Unique) -join ', '
        throw "Close the running DisplayMagician desktop and User Agent processes before deploying Debug files: $processNames"
    }

    if (Test-Path -LiteralPath $stageRoot) {
        Remove-Item -LiteralPath $stageRoot -Recurse -Force
    }
    New-Item -ItemType Directory -Path $stageRoot -Force | Out-Null

    Publish-Project (Join-Path $script:root 'DisplayMagician\DisplayMagician.csproj') (Join-Path $stageRoot 'DisplayMagician') 'AnyCPU' 'DisplayMagician.exe' 'DebugPublishProfile'
    Publish-Project (Join-Path $script:root 'DisplayMagician.ControlService\DisplayMagician.ControlService.csproj') (Join-Path $stageRoot 'ControlService') 'x64' 'DisplayMagician.ControlService.exe' ''
    Publish-Project (Join-Path $script:root 'DisplayMagician.SessionLauncher\DisplayMagician.SessionLauncher.csproj') (Join-Path $stageRoot 'SessionLauncher') 'x64' 'DisplayMagician.SessionLauncher.exe' ''
    Publish-Project (Join-Path $script:root 'DisplayMagician.UserAgent\DisplayMagician.UserAgent.csproj') (Join-Path $stageRoot 'UserAgent') 'x64' 'DisplayMagician.UserAgent.exe' ''

    foreach ($serviceName in @('DisplayMagicianControlService', 'DisplayMagicianSessionLauncher')) {
        $service = Get-Service -Name $serviceName -ErrorAction Stop
        if ($service.Status -ne 'Stopped' -and $PSCmdlet.ShouldProcess($serviceName, 'Stop service')) {
            Write-Host "Stopping $serviceName..." -ForegroundColor Cyan
            Stop-Service -Name $serviceName -ErrorAction Stop
            $service.WaitForStatus('Stopped', [TimeSpan]::FromSeconds(30))
        }
    }

    if ($PSCmdlet.ShouldProcess($desktopDirectory, 'Copy Debug desktop payload')) {
        Get-ChildItem -LiteralPath (Join-Path $stageRoot 'DisplayMagician') -Force | Copy-Item -Destination $desktopDirectory -Recurse -Force
    }
    if ($PSCmdlet.ShouldProcess($controlServiceDirectory, 'Copy Debug Control Service payload')) {
        Get-ChildItem -LiteralPath (Join-Path $stageRoot 'ControlService') -Force | Copy-Item -Destination $controlServiceDirectory -Recurse -Force
    }
    if ($PSCmdlet.ShouldProcess($sessionLauncherDirectory, 'Copy Debug Session Launcher payload')) {
        Get-ChildItem -LiteralPath (Join-Path $stageRoot 'SessionLauncher') -Force | Copy-Item -Destination $sessionLauncherDirectory -Recurse -Force
    }
    if ($PSCmdlet.ShouldProcess($userAgentDirectory, 'Copy Debug User Agent payload')) {
        Get-ChildItem -LiteralPath (Join-Path $stageRoot 'UserAgent') -Force | Copy-Item -Destination $userAgentDirectory -Recurse -Force
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
    Write-Host 'Local Debug deployment complete.' -ForegroundColor Green
    Write-Host 'Attach Visual Studio to DisplayMagician.exe, DisplayMagician.UserAgent.exe, and DisplayMagician.ControlService.exe as needed.' -ForegroundColor Yellow
}

function Start-WindowsSandboxDebugging {
    $sandboxRoot = Join-Path $script:root 'Sandbox'
    $templatePath = Join-Path $sandboxRoot 'DisplayMagician-Debug.wsb.template'
    $generatedRoot = Join-Path $sandboxRoot 'Generated'
    $localAssetsRoot = Join-Path $sandboxRoot 'Local'
    $testCertificatePath = Join-Path $localAssetsRoot 'DisplayMagicianTest.cer'
    $bundleProject = Join-Path $script:root 'DisplayMagicianBundle\DisplayMagicianBundle.wixproj'
    if (-not (Test-Path -LiteralPath $templatePath) -or -not (Test-Path -LiteralPath $bundleProject)) {
        throw 'The Windows Sandbox harness files are missing. Restore the Sandbox folder from source control.'
    }

    if (-not (Test-Path -LiteralPath $testCertificatePath)) {
        throw @"
The DisplayMagician test signing certificate was not found.
Expected:
  $testCertificatePath
Run prepare_displaymagician.ps1 first. It exports the public test-signing
certificate used by the Debug MSIX into Sandbox\Local for Windows Sandbox.
The Sandbox\Local directory is intentionally excluded from Git.
"@
    }

    Assert-WindowsSandboxAvailable

    Write-Host 'Building the Debug DisplayMagician Bundle...' -ForegroundColor Cyan
    & $script:msbuild $bundleProject '-t:Build' '-p:Configuration=Debug' '-p:Platform=x64' '-nologo' '-v:minimal'
    if ($LASTEXITCODE -ne 0) {
        throw "Debug Bundle build failed with exit code $LASTEXITCODE."
    }

    $bundleDirectory = Join-Path $script:root 'DisplayMagicianBundle\bin\x64\Debug'
    $bundlePath = Get-ChildItem -LiteralPath $bundleDirectory -Filter 'DisplayMagicianSetup_v*.exe' -File |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1
    if ($null -eq $bundlePath) {
        throw "The Debug Bundle was not found in $bundleDirectory."
    }

    $remoteDebuggerPath = Find-RemoteDebugger
    New-Item -ItemType Directory -Path $generatedRoot -Force | Out-Null
    $generatedConfigurationPath = Join-Path $generatedRoot 'DisplayMagician-Debug.wsb'
    $configuration = Get-Content -LiteralPath $templatePath -Raw
    $requiredPlaceholders = @(
        '__BUNDLE_HOST_FOLDER__',
        '__BUNDLE_FILENAME__',
        '__SANDBOX_HOST_FOLDER__',
        '__SANDBOX_LOCAL_HOST_FOLDER__',
        '__REMOTE_DEBUGGER_HOST_FOLDER__'
    )
    foreach ($placeholder in $requiredPlaceholders) {
        if (-not $configuration.Contains($placeholder)) {
            throw "The Windows Sandbox template is missing required placeholder '$placeholder': $templatePath"
        }
    }
    $configuration = $configuration.Replace('__BUNDLE_HOST_FOLDER__', [System.Security.SecurityElement]::Escape($bundleDirectory))
    $configuration = $configuration.Replace('__BUNDLE_FILENAME__', [System.Security.SecurityElement]::Escape($bundlePath.Name))
    $configuration = $configuration.Replace('__SANDBOX_HOST_FOLDER__', [System.Security.SecurityElement]::Escape($sandboxRoot))
    $configuration = $configuration.Replace('__SANDBOX_LOCAL_HOST_FOLDER__', [System.Security.SecurityElement]::Escape($localAssetsRoot))
    $configuration = $configuration.Replace('__REMOTE_DEBUGGER_HOST_FOLDER__', [System.Security.SecurityElement]::Escape($remoteDebuggerPath))
    [System.IO.File]::WriteAllText($generatedConfigurationPath, $configuration, [System.Text.UTF8Encoding]::new($true))

    Write-Host "Starting Windows Sandbox with Debug Bundle $($bundlePath.Name)..." -ForegroundColor Cyan
    if ($PSCmdlet.ShouldProcess($generatedConfigurationPath, 'Start Windows Sandbox Debugging')) {
        Start-Process -FilePath $generatedConfigurationPath
    }
}

$root = $PSScriptRoot
$msbuild = Find-MSBuild

Write-Host ''
Write-Host 'DisplayMagician debugging target' -ForegroundColor Cyan
Write-Host ''
Write-Host '  1. Local installed Debug build'
Write-Host '     Publish Debug components into the existing host installation.'
Write-Host ''
Write-Host '  2. Windows Sandbox based Debugging'
Write-Host '     Build the Debug Bundle and start the configured Windows Sandbox.'
Write-Host ''
Write-Host '  Q. Quit'
Write-Host ''

do {
    $selection = Read-Host 'Select an option'
    switch ($selection.Trim().ToUpperInvariant()) {
        '1' { Start-LocalDebugging; $selected = $true }
        '2' { Start-WindowsSandboxDebugging; $selected = $true }
        'Q' { Write-Host 'No debugging workflow was started.' -ForegroundColor Yellow; $selected = $true }
        default { Write-Host 'Enter 1, 2, or Q.' -ForegroundColor Yellow; $selected = $false }
    }
} while (-not $selected)
