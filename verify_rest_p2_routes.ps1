[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repositoryRoot = $PSScriptRoot
$specPath = Join-Path $repositoryRoot 'Docs/REST/openapi.yaml'
$spec = Get-Content -Raw -LiteralPath $specPath | ConvertFrom-Json
$mappedRoutes = @{}
$documentedRoutes = @{}
$errors = [System.Collections.Generic.List[string]]::new()

foreach ($relativePath in @('DisplayMagician.Gateway/Program.cs', 'DisplayMagician.Gateway/GatewayResourceRoutes.cs', 'DisplayMagician.Gateway/GatewayEventRoutes.cs')) {
    $source = Get-Content -Raw -LiteralPath (Join-Path $repositoryRoot $relativePath)
    foreach ($match in [regex]::Matches($source, 'app\.Map(Get|Post|Put|Delete)\("([^"]+)"')) {
        $method = $match.Groups[1].Value.ToUpperInvariant()
        $path = $match.Groups[2].Value -replace ':guid', ''
        $route = "$method $path"
        if ($mappedRoutes.ContainsKey($route)) {
            $errors.Add("Duplicate Gateway mapping: $route")
        }
        $mappedRoutes[$route] = $true
    }
}

foreach ($pathEntry in $spec.paths.PSObject.Properties) {
    foreach ($operationEntry in $pathEntry.Value.PSObject.Properties) {
        $method = $operationEntry.Name.ToUpperInvariant()
        if ($method -notin @('GET', 'POST', 'PUT', 'DELETE')) { continue }
        $route = "$method $($pathEntry.Name)"
        $documentedRoutes[$route] = $true
        $parameters = @($pathEntry.Value.parameters) + @($operationEntry.Value.parameters)
        $parameterRefs = @($parameters | Where-Object { $null -ne $_ -and $null -ne $_.'$ref' } | ForEach-Object { $_.'$ref' })
        if ('#/components/parameters/ProtocolHello' -notin $parameterRefs) {
            $errors.Add("Missing ProtocolHello parameter: $route")
        }
        if ($method -ne 'GET' -and '#/components/parameters/IdempotencyKey' -notin $parameterRefs) {
            $errors.Add("Missing Idempotency-Key parameter: $route")
        }
        if (-not @($operationEntry.Value.responses.PSObject.Properties.Name | Where-Object { $_ -match '^2\d\d$' }).Count) {
            $errors.Add("Missing success response: $route")
        }
        if ($null -eq $operationEntry.Value.responses.default) {
            $errors.Add("Missing Problem Details response: $route")
        }

        $expectedSecurity = if ($route -eq 'GET /v1/pairing-requests/{pairingRequestId}') {
            'pairingPoll'
        } elseif ($route -in @('GET /v1/identity', 'GET /v1/capabilities', 'POST /v1/pairing-requests')) {
            ''
        } else {
            'deviceBearer'
        }
        $actualSecurity = @($operationEntry.Value.security | ForEach-Object { $_.PSObject.Properties.Name }) -join ','
        if ($actualSecurity -ne $expectedSecurity) {
            $errors.Add("Unexpected security scheme for $($route): expected '$expectedSecurity', found '$actualSecurity'")
        }
    }
}

foreach ($route in $mappedRoutes.Keys) {
    if (-not $documentedRoutes.ContainsKey($route)) { $errors.Add("Gateway route missing from implemented OpenAPI: $route") }
}
foreach ($route in $documentedRoutes.Keys) {
    if (-not $mappedRoutes.ContainsKey($route)) { $errors.Add("OpenAPI route missing from Gateway: $route") }
}
$events = $spec.paths.'/v1/events'.get
if ($null -eq $events -or $events.operationId -ne 'streamEvents' -or
    $null -eq $events.responses.'200'.content.'text/event-stream' -or
    'Last-Event-ID' -notin @($events.parameters | ForEach-Object { $_.name })) {
    $errors.Add('The implemented SSE route is missing its event-stream or resume declaration.')
}
foreach ($name in @('snapshot', 'operation.updated', 'decision.updated', 'display-profile.changed', 'audio-profile.changed', 'shortcut.changed')) {
    if ($null -eq $events.responses.'200'.'x-sse-eventSchemas'.$name) {
        $errors.Add("The implemented SSE route is missing the $name event schema.")
    }
}
if ($mappedRoutes.Count -ne 26) {
    $errors.Add("Expected 26 implemented routes; found $($mappedRoutes.Count).")
}

if ($errors.Count -gt 0) {
    $errors | Sort-Object | ForEach-Object { Write-Error $_ }
    exit 1
}

Write-Output "REST route declaration check passed: $($mappedRoutes.Count) Gateway methods/paths match implemented OpenAPI, including protocol, mutation-key, response, and security declarations."
