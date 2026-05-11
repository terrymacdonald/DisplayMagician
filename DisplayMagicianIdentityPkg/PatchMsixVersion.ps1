param(
    [Parameter(Mandatory)][string]$ExePath,
    [Parameter(Mandatory)][string]$ManifestPath
)
$ver = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($ExePath).FileVersion
if (-not $ver) { throw "Could not read FileVersion from $ExePath" }
$xml = Get-Content $ManifestPath -Raw
$xml = $xml -replace '(<Identity\b[^>]*\bVersion=")[\d.]+"', "`${1}$ver`""
[System.IO.File]::WriteAllText($ManifestPath, $xml, [System.Text.Encoding]::UTF8)
Write-Host "Patched AppxManifest.xml Identity Version to $ver"
