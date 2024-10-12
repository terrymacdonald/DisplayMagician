param (
    # Path passed as a parameter from the command line, defaulting to the current directory if not provided
    [string]$outputDir = (Get-Location)
)

# Get the current directory (where the script is being run)
$currentDir = Get-Location

# Path to the DisplayMagician assembly (DLL) containing the version info, using a relative path
$relativeAssemblyPath = "..\DisplayMagician\bin\Debug\DisplayMagician.dll"
$assemblyPath = Join-Path -Path $currentDir -ChildPath $relativeAssemblyPath

# Get the Version from the file
$fileVersionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($assemblyPath)
$versionInfo = $fileVersionInfo.FileVersion

# Output the new setup file name
Write-Host "OutputDir passed was '$outputDir'"

# Define the new setup file name with version
$newSetupFileName = "DisplayMagicianSetup_v$($versionInfo).exe"

# Rename the setup file
#Rename-Item -Path $setupFilePath -NewName $newSetupFileName -Force

Set-Location -Path $outputDir

Get-Childitem DisplayMagicianSetup.exe | ForEach-Object {
    Move-Item $_ $_.Name.Replace("DisplayMagicianSetup.exe", $newSetupFileName) -Force
}


# Output the new setup file name
Write-Host "Renamed '$outputDir\DisplayMagicianSetup.exe' to '$newSetupFileName'"