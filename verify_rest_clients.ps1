[CmdletBinding()]
param(
    [switch]$CompileKotlin,
    [switch]$InstallTools,
    [string]$JavaPath,
    [string]$AndroidJar
)

$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'generate_rest_clients.ps1') -Check
if (-not $CompileKotlin) {
    Write-Output 'Generated sources match OpenAPI. Use -CompileKotlin for Android compilation and JVM contract tests; run swift test in Clients/Swift on macOS.'
    return
}
$tools = Join-Path $PSScriptRoot '.tools/Kotlin'
$versions = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'Clients/tool_versions.json') -Raw | ConvertFrom-Json
New-Item -ItemType Directory -Path $tools -Force | Out-Null
$compilerRoot = Join-Path $tools 'kotlinc'
$jsonJar = Join-Path $tools 'json-20250517.jar'
if ($InstallTools) {
    if (-not (Test-Path -LiteralPath (Join-Path $compilerRoot 'lib/kotlin-compiler.jar'))) {
        $archive = Join-Path $tools "kotlin-compiler-$($versions.kotlinVersion).zip"
        if (-not (Test-Path -LiteralPath $archive)) { Invoke-WebRequest -Uri $versions.kotlinCompilerUrl -OutFile $archive }
        if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $versions.kotlinCompilerSha256) { throw 'Kotlin compiler checksum mismatch.' }
        Expand-Archive -LiteralPath $archive -DestinationPath $tools -Force
    }
    if (-not (Test-Path -LiteralPath $jsonJar)) { Invoke-WebRequest -Uri $versions.jsonUrl -OutFile $jsonJar }
}
if (-not (Test-Path -LiteralPath $compilerRoot) -or -not (Test-Path -LiteralPath $jsonJar)) { throw 'Kotlin verification tools are missing. Run with -CompileKotlin -InstallTools.' }
if ((Get-FileHash -LiteralPath $jsonJar -Algorithm SHA256).Hash -ne $versions.jsonSha256) { throw 'JSON-java checksum mismatch.' }
if (-not $JavaPath) {
    $javaCommand = Get-Command java -ErrorAction SilentlyContinue
    if ($javaCommand) { $JavaPath = $javaCommand.Source }
    elseif (Test-Path -LiteralPath 'C:/Program Files/Android/openjdk/jdk-21.0.8/bin/java.exe') { $JavaPath = 'C:/Program Files/Android/openjdk/jdk-21.0.8/bin/java.exe' }
    else { throw 'Pass -JavaPath with a JDK 17 or later java executable.' }
}
if (-not $AndroidJar) {
    foreach ($sdkRoot in @($env:ANDROID_HOME, $env:ANDROID_SDK_ROOT, 'C:/Program Files (x86)/Android/android-sdk')) {
        if (-not $sdkRoot) { continue }
        $platforms = Join-Path $sdkRoot 'platforms'
        if (Test-Path -LiteralPath $platforms) {
            $platform = Get-ChildItem -LiteralPath $platforms -Directory | Sort-Object Name -Descending | Select-Object -First 1
            if ($platform) { $AndroidJar = Join-Path $platform.FullName 'android.jar'; break }
        }
    }
}
if (-not $AndroidJar -or -not (Test-Path -LiteralPath $AndroidJar)) { throw 'Pass -AndroidJar with an installed Android SDK platform android.jar.' }
$separator = [IO.Path]::PathSeparator
$compilerClasspath = Join-Path $compilerRoot 'lib/*'
$coroutines = Join-Path $compilerRoot 'lib/kotlinx-coroutines-core-jvm.jar'
$stdlib = Join-Path $compilerRoot 'lib/kotlin-stdlib.jar'
$libraryJar = Join-Path $tools 'gateway-client.jar'
$testJar = Join-Path $tools 'gateway-client-tests.jar'
$sources = @(Get-ChildItem (Join-Path $PSScriptRoot 'Clients/Kotlin/src/main') -Filter '*.kt' -Recurse | ForEach-Object FullName)
& $JavaPath -cp $compilerClasspath org.jetbrains.kotlin.cli.jvm.K2JVMCompiler -kotlin-home $compilerRoot -jvm-target 1.8 -classpath ($AndroidJar + $separator + $coroutines) -d $libraryJar @sources
if ($LASTEXITCODE -ne 0) { throw 'Android client compilation failed.' }
$tests = @(Get-ChildItem (Join-Path $PSScriptRoot 'Clients/Kotlin/src/test') -Filter '*.kt' -Recurse | ForEach-Object FullName)
& $JavaPath -cp $compilerClasspath org.jetbrains.kotlin.cli.jvm.K2JVMCompiler -kotlin-home $compilerRoot -jvm-target 1.8 "-Xfriend-paths=$libraryJar" -classpath (@($libraryJar, $jsonJar, $AndroidJar, $coroutines) -join $separator) -d $testJar @tests
if ($LASTEXITCODE -ne 0) { throw 'Kotlin contract test compilation failed.' }
& $JavaPath -cp (@($libraryJar, $testJar, $stdlib, $jsonJar) -join $separator) org.displaymagician.gateway.ClientContractTestsKt (Join-Path $PSScriptRoot 'Clients/Fixtures/mobile_contract.json')
if ($LASTEXITCODE -ne 0) { throw 'Kotlin contract tests failed.' }
