param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Register', 'Unregister')]
    [string] $Action,
    [string] $MsixPath,
    [string] $ExternalLocation,
    [Parameter(Mandatory = $true)]
    [string] $PackageName
)

$ErrorActionPreference = 'Stop'

try {
    if ($Action -eq 'Unregister') {
        Get-AppxPackage -Name $PackageName | Remove-AppxPackage -ErrorAction Stop
        exit 0
    }

    try {
        Add-AppxPackage -Path $MsixPath -ExternalLocation $ExternalLocation -ErrorAction Stop
    }
    catch {
        if (('{0:X8}' -f $_.Exception.HResult) -ne '80073CFB') {
            throw
        }

        Get-AppxPackage -Name $PackageName | Remove-AppxPackage -ErrorAction SilentlyContinue
        Add-AppxPackage -Path $MsixPath -ExternalLocation $ExternalLocation -ErrorAction Stop
    }
}
catch {
    Write-Error $_
    exit 1
}
