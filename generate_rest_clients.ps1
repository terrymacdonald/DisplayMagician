[CmdletBinding()]
param([switch]$Check)

$ErrorActionPreference = 'Stop'
$generator = Join-Path $PSScriptRoot 'BuildScripts/generate_rest_clients.mjs'
if ($Check) { & node $generator --check } else { & node $generator }
if ($LASTEXITCODE -ne 0) { throw "REST client generation failed ($LASTEXITCODE)." }
