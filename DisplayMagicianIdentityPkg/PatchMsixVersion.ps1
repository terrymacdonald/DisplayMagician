param(
    [Parameter(Mandatory)][string]$ExePath,
    [Parameter(Mandatory)][string]$ManifestPath,
    [string]$Publisher
)
$ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($ExePath).FileVersion
if (-not $ver) { throw "Could not read FileVersion from $ExePath" }
if (-not (Test-Path -LiteralPath $ManifestPath)) {
    throw "AppxManifest.xml not found: $ManifestPath"
}

$xml = Get-Content -LiteralPath $ManifestPath -Raw -ErrorAction Stop
$versionPattern = '(<Identity\b[^>]*\bVersion=")[\d.]+"'
if (-not [System.Text.RegularExpressions.Regex]::IsMatch($xml, $versionPattern)) {
    throw "Could not find an Identity Version attribute in AppxManifest.xml: $ManifestPath"
}

$xml = $xml -replace $versionPattern, "`${1}$ver`""
if ($Publisher) {
    $publisherPattern = '(<Identity\b[^>]*\bPublisher=")[^"]+"'
    if (-not [System.Text.RegularExpressions.Regex]::IsMatch($xml, $publisherPattern)) {
        throw "Could not find an Identity Publisher attribute in AppxManifest.xml: $ManifestPath"
    }

    $xml = $xml -replace $publisherPattern, "`${1}$Publisher`""
}
[System.IO.File]::WriteAllText($ManifestPath, $xml, [System.Text.Encoding]::UTF8)
Write-Host "Patched AppxManifest.xml Identity Version to $ver"
if ($Publisher) {
    Write-Host "Patched AppxManifest.xml Identity Publisher for this build"
}
