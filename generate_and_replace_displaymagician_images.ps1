#Requires -Version 5.1
<#
.SYNOPSIS
    Generates DisplayMagician branding derivatives and deploys them to consumers.

.DESCRIPTION
    Creates all canonical icon and MSIX image assets from Branding\displaymagician-1024.png,
    validates them, then copies them to the projects that package or consume them. The MSI
    installer artwork (WixUIBannerBmp.png and WixUIDialogBmp.png) is deliberately not changed.

    Run prepare_displaymagician.ps1 first to install the repository-local ImageMagick tool.
#>

[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'Medium')]
param ()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
$sourceImage = Join-Path $repoRoot 'Branding\displaymagician-1024.png'
$generatedRoot = Join-Path $repoRoot 'Branding\generated'
$msixGeneratedRoot = Join-Path $generatedRoot 'msix'
$stagingRoot = Join-Path $generatedRoot '.staging'
$magickExe = Join-Path $repoRoot '.tools\ImageMagick\magick.exe'

if (-not (Test-Path -LiteralPath $sourceImage)) { throw "The master branding image was not found: $sourceImage" }
if (-not (Test-Path -LiteralPath $magickExe)) { throw "ImageMagick is not installed at '$magickExe'. Run .\prepare_displaymagician.ps1 first." }
if ($WhatIfPreference) {
    Write-Host 'WhatIf: would generate canonical branding assets under Branding\generated and replace the active application, installer, and MSIX copies.'
    return
}

function Invoke-Magick {
    param([string[]]$Arguments)
    & $magickExe @Arguments
    if ($LASTEXITCODE -ne 0) { throw "ImageMagick failed with exit code $LASTEXITCODE." }
}

function New-SquarePng {
    param([string]$Destination, [int]$Size)
    Invoke-Magick @($sourceImage, '-resize', "${Size}x${Size}", '-background', 'none', '-gravity', 'center', '-extent', "${Size}x${Size}", $Destination)
}

function Assert-ImageDimensions {
    param([string]$Path, [int]$Width, [int]$Height)
    $dimensions = & $magickExe 'identify' '-format' '%w,%h' $Path
    if ($LASTEXITCODE -ne 0 -or $dimensions.Trim() -ne "$Width,$Height") { throw "Generated image '$Path' is not $Width x $Height pixels." }
}

Write-Host 'Generating DisplayMagician branding assets...' -ForegroundColor Cyan
New-Item -ItemType Directory -Path $generatedRoot, $msixGeneratedRoot, $stagingRoot -Force | Out-Null

$iconPath = Join-Path $generatedRoot 'DisplayMagician.ico'
$iconSizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
$iconFrames = @()
foreach ($iconSize in $iconSizes) {
    $iconFrame = Join-Path $stagingRoot "DisplayMagician-$iconSize.png"
    New-SquarePng -Destination $iconFrame -Size $iconSize
    $iconFrames += $iconFrame
}
Invoke-Magick ($iconFrames + @($iconPath))

$pngAssets = @(
    @{ Path = (Join-Path $generatedRoot 'applogo.png'); Width = 48; Height = 48 },
    @{ Path = (Join-Path $msixGeneratedRoot 'StoreLogo.png'); Width = 50; Height = 50 },
    @{ Path = (Join-Path $msixGeneratedRoot 'Square44x44Logo.png'); Width = 44; Height = 44 },
    @{ Path = (Join-Path $msixGeneratedRoot 'Square44x44Logo.scale-200.png'); Width = 88; Height = 88 },
    @{ Path = (Join-Path $msixGeneratedRoot 'Square44x44Logo.targetsize-44_altform-unplated.png'); Width = 44; Height = 44 },
    @{ Path = (Join-Path $msixGeneratedRoot 'Square44x44Logo.targetsize-24_altform-unplated.png'); Width = 24; Height = 24 },
    @{ Path = (Join-Path $msixGeneratedRoot 'Square150x150Logo.png'); Width = 150; Height = 150 },
    @{ Path = (Join-Path $msixGeneratedRoot 'Square150x150Logo.scale-200.png'); Width = 300; Height = 300 },
    @{ Path = (Join-Path $msixGeneratedRoot 'LockScreenLogo.scale-200.png'); Width = 96; Height = 96 }
)
foreach ($asset in $pngAssets) {
    New-SquarePng -Destination $asset.Path -Size $asset.Width
    Assert-ImageDimensions -Path $asset.Path -Width $asset.Width -Height $asset.Height
}

$wideLogoPath = Join-Path $msixGeneratedRoot 'Wide310x150Logo.scale-200.png'
Invoke-Magick @($sourceImage, '-resize', '260x260', '-background', 'none', '-gravity', 'center', '-extent', '620x300', $wideLogoPath)
Assert-ImageDimensions -Path $wideLogoPath -Width 620 -Height 300

$wideLogoBasePath = Join-Path $msixGeneratedRoot 'Wide310x150Logo.png'
Invoke-Magick @($sourceImage, '-resize', '130x130', '-background', 'none', '-gravity', 'center', '-extent', '310x150', $wideLogoBasePath)
Assert-ImageDimensions -Path $wideLogoBasePath -Width 310 -Height 150

$iconDimensions = & $magickExe 'identify' '-format' '%w,%h\n' $iconPath
if ($LASTEXITCODE -ne 0 -or @($iconDimensions | Where-Object { $_.Trim() -match '^(16,16|20,20|24,24|32,32|40,40|48,48|64,64|128,128|256,256)$' }).Count -lt $iconSizes.Count) {
    throw "Generated icon '$iconPath' does not contain all expected icon sizes."
}

$deployments = @(
    @{ Source = $iconPath; Destination = (Join-Path $repoRoot 'DisplayMagician\Properties\DisplayMagician.ico') },
    @{ Source = $iconPath; Destination = (Join-Path $repoRoot 'DisplayMagicianConsole\DisplayMagician.ico') },
    @{ Source = $iconPath; Destination = (Join-Path $repoRoot 'DisplayMagicianShared\Properties\DisplayMagician.ico') },
    @{ Source = $iconPath; Destination = (Join-Path $repoRoot 'DisplayMagicianPackage\DisplayMagician.ico') },
    @{ Source = $iconPath; Destination = (Join-Path $repoRoot 'DisplayMagicianBundle\DisplayMagician.ico') },
    @{ Source = (Join-Path $generatedRoot 'applogo.png'); Destination = (Join-Path $repoRoot 'DisplayMagician\Properties\applogo.png') },
    @{ Source = (Join-Path $msixGeneratedRoot 'StoreLogo.png'); Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\StoreLogo.png') },
    @{ Source = (Join-Path $msixGeneratedRoot 'Square44x44Logo.png'); Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\Square44x44Logo.png') },
    @{ Source = (Join-Path $msixGeneratedRoot 'Square44x44Logo.scale-200.png'); Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\Square44x44Logo.scale-200.png') },
    @{ Source = (Join-Path $msixGeneratedRoot 'Square44x44Logo.targetsize-44_altform-unplated.png'); Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\Square44x44Logo.targetsize-44_altform-unplated.png') },
    @{ Source = (Join-Path $msixGeneratedRoot 'Square44x44Logo.targetsize-24_altform-unplated.png'); Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\Square44x44Logo.targetsize-24_altform-unplated.png') },
    @{ Source = (Join-Path $msixGeneratedRoot 'Square150x150Logo.png'); Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\Square150x150Logo.png') },
    @{ Source = (Join-Path $msixGeneratedRoot 'Square150x150Logo.scale-200.png'); Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\Square150x150Logo.scale-200.png') },
    @{ Source = $wideLogoBasePath; Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\Wide310x150Logo.png') },
    @{ Source = $wideLogoPath; Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\Wide310x150Logo.scale-200.png') },
    @{ Source = (Join-Path $msixGeneratedRoot 'LockScreenLogo.scale-200.png'); Destination = (Join-Path $repoRoot 'DisplayMagicianIdentityPkg\Assets\LockScreenLogo.scale-200.png') }
)

foreach ($deployment in $deployments) {
    if ($PSCmdlet.ShouldProcess($deployment.Destination, "Copy $($deployment.Source)")) {
        Copy-Item -LiteralPath $deployment.Source -Destination $deployment.Destination -Force
        Write-Host "  Updated $($deployment.Destination.Substring($repoRoot.Length + 1))" -ForegroundColor Green
    }
}

Remove-Item -LiteralPath $stagingRoot -Recurse -Force
Write-Host 'Branding asset generation and replacement completed. Installer artwork was not changed.' -ForegroundColor Green
