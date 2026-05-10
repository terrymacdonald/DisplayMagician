@echo off
:: Standalone script to pack and sign the MSIX identity package.
:: The MSBuild build (DisplayMagicianPackage.wixproj) runs this automatically — use this script
:: only when you want to rebuild the MSIX independently of the full solution build.
::
:: Prerequisites:
::   - Windows SDK 10.0.26100.0 installed (makeappx.exe, signtool.exe)
::   - A code-signing PFX configured (see create_new_self-signed_cert.ps1 for dev certs)
::
:: Usage:
::   build_identity_pkg.cmd [PfxPath] [PfxPassword]
::   If PfxPath is omitted the MSIX is packed but NOT signed.

setlocal

set MAKEAPPX="C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe"
set SIGNTOOL="C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"
set MSIX_OUT=DisplayMagicianPackage\Packages\DisplayMagicianIdentityPkg.msix
set PFX_PATH=%1
set PFX_PASS=%2

echo Packing MSIX identity package...
%MAKEAPPX% pack /o /d DisplayMagicianIdentityPkg /p %MSIX_OUT% /nv
if %ERRORLEVEL% neq 0 (
    echo ERROR: makeappx failed with exit code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)
echo Packed: %MSIX_OUT%

if "%PFX_PATH%"=="" (
    echo WARNING: No PFX path provided — MSIX was NOT signed.
    echo Usage: build_identity_pkg.cmd ^<PfxPath^> ^<PfxPassword^>
    exit /b 0
)

echo Signing MSIX...
%SIGNTOOL% sign /fd SHA256 /f "%PFX_PATH%" /p "%PFX_PASS%" "%MSIX_OUT%"
if %ERRORLEVEL% neq 0 (
    echo ERROR: signtool failed with exit code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)
echo Signed: %MSIX_OUT%
