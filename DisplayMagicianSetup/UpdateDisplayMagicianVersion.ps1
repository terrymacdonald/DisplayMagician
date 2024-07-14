 $vdprojPath = "path\to\your\installer.vdproj"
 $newVersion = "1.0.0.0" # Acquire this dynamically from your main project
 $vdprojContent = Get-Content $vdprojPath -Raw
 $vdprojContent = $vdprojContent -replace '"ProductVersion"\s*:\s*"[^"]+"', "`"ProductVersion`": `"$newVersion`""
 Set-Content $vdprojPath -Value $vdprojContent