@echo off
:: InstallIdentityPkg.cmd
:: Installs or removes the DisplayMagician sparse MSIX identity package.
:: Called by the WiX bundle chain with /install or /uninstall.
::
:: Usage:
::   InstallIdentityPkg.cmd /install   <MsixPath> <ExternalLocation>
::   InstallIdentityPkg.cmd /uninstall

setlocal EnableDelayedExpansion

set ACTION=%1
set MSIX_PATH=%2
set EXTERNAL_LOCATION=%3

if /i "%ACTION%"=="/install" goto INSTALL
if /i "%ACTION%"=="/uninstall" goto UNINSTALL

echo Unknown action: %ACTION%
exit /b 1

:INSTALL
echo Installing DisplayMagician identity package...
powershell -NonInteractive -ExecutionPolicy Bypass -Command "Add-AppxPackage -ExternalLocation '%EXTERNAL_LOCATION%' -Path '%MSIX_PATH%'"
if %ERRORLEVEL% neq 0 (
    echo Failed to install identity package. Exit code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)
echo Identity package installed successfully.
exit /b 0

:UNINSTALL
echo Removing DisplayMagician identity package...
powershell -NonInteractive -ExecutionPolicy Bypass -Command "Get-AppxPackage -Name '4f6354a7-065d-432a-bb6b-b65acc257555' | Remove-AppxPackage -ErrorAction SilentlyContinue"
echo Identity package removed.
exit /b 0
