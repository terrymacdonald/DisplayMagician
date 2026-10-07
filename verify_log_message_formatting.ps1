[CmdletBinding()]
param(
    [string[]] $SourceDirectory = @(
        'DisplayMagician',
        'DisplayMagicianConsole',
        'DisplayMagician.UserAgent',
        'DisplayMagician.ControlService',
        'DisplayMagician.SessionLauncher'),
    [string] $ReportPath = 'NLogSourcePrefixMigrationReport.csv'
)

$ErrorActionPreference = 'Stop'

# This deliberately uses a line-based regex audit. It does not change source files.
# A source may use ~ before a method name to identify destruction/cleanup activity.
$loggerCallPattern = '(?i)\b(?:[A-Za-z_][A-Za-z0-9_.]*logger)\.(Trace|Debug|Info|Warn|Error|Fatal)\s*\('
$sourcePrefixPattern = '[A-Za-z_][A-Za-z0-9_.]*/~?[A-Za-z_][A-Za-z0-9_]*:'
$repositoryRoot = (Get-Location).Path

$files = foreach ($directory in $SourceDirectory) {
    $path = Join-Path $repositoryRoot $directory
    if (-not (Test-Path -LiteralPath $path)) {
        Write-Warning "Skipping missing source directory: $directory"
        continue
    }

    Get-ChildItem -LiteralPath $path -Recurse -Filter '*.cs' -File |
        Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
}

$issues = foreach ($file in $files) {
    Select-String -LiteralPath $file.FullName -Pattern $loggerCallPattern | ForEach-Object {
        if ($_.Line -notmatch $sourcePrefixPattern) {
            [pscustomobject]@{
                Path = $_.Path.Substring($repositoryRoot.Length).TrimStart('\\')
                Line = $_.LineNumber
                LogCall = $_.Matches[0].Value.Trim()
                Message = $_.Line.Trim()
                RequiredPrefix = 'ClassName/MethodName:'
            }
        }
    }
}

$issues | Export-Csv -LiteralPath (Join-Path $repositoryRoot $ReportPath) -NoTypeInformation
Write-Output "NLog source-prefix audit found $($issues.Count) issue(s). Report: $ReportPath"

if ($issues.Count -gt 0) {
    $issues | Format-Table Path, Line, LogCall -AutoSize
}
