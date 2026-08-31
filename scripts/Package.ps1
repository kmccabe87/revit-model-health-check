$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = Split-Path -Parent $here
$version = "0.6.16"
$years = @(2025, 2026, 2027)

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "Revit Model Health Check v$version - Installed Payload Packager" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "This package step captures the exact locally installed add-in files." -ForegroundColor Gray
Write-Host "Recipients do NOT build source and do NOT need a .NET SDK or Visual Studio." -ForegroundColor Gray

$running = @(Get-Process Revit -ErrorAction SilentlyContinue)
if ($running.Count -gt 0) {
    Write-Host "PACKAGE FAILED: Revit is currently running." -ForegroundColor Red
    Write-Host "Close Revit after testing, then run this package step so installed files are not in use." -ForegroundColor Yellow
    exit 3
}

$installed = @{}
$missing = @()
foreach ($year in $years) {
    $addinRoot = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$year"
    $pluginDir = Join-Path $addinRoot "Revit Model Health Check"
    $manifestPath = Join-Path $addinRoot "Revit Model Health Check.addin"
    $dll = Join-Path $pluginDir "SVMModelHealth.dll"

    if (!(Test-Path $pluginDir) -or !(Test-Path $dll) -or !(Test-Path $manifestPath)) {
        $missing += $year
        continue
    }

    try {
        $assemblyVersion = [System.Reflection.AssemblyName]::GetAssemblyName($dll).Version.ToString(3)
    }
    catch {
        Write-Host "PACKAGE FAILED for Revit ${year}: could not read installed DLL version: $dll" -ForegroundColor Red
        exit 4
    }

    if ($assemblyVersion -ne $version) {
        Write-Host "PACKAGE FAILED for Revit ${year}: installed DLL is v$assemblyVersion, expected v$version." -ForegroundColor Red
        Write-Host "Run BUILD.cmd, then INSTALL.cmd, test the add-in in Revit $year, close Revit, and package again." -ForegroundColor Yellow
        exit 5
    }

    $installed[$year] = [pscustomobject]@{
        PluginDir = $pluginDir
        ManifestPath = $manifestPath
        Dll = $dll
        Version = $assemblyVersion
    }
}

if ($missing.Count -gt 0) {
    Write-Host "PACKAGE FAILED: tested installed payload is missing for Revit year(s): $($missing -join ', ')" -ForegroundColor Red
    Write-Host "Required release sequence:" -ForegroundColor Yellow
    Write-Host "  1. BUILD.cmd" -ForegroundColor Yellow
    Write-Host "  2. INSTALL.cmd" -ForegroundColor Yellow
    Write-Host "  3. Test in each supported Revit version" -ForegroundColor Yellow
    Write-Host "  4. Close Revit" -ForegroundColor Yellow
    Write-Host "  5. CREATE DISTRIBUTION FROM INSTALLED.cmd" -ForegroundColor Yellow
    exit 1
}

$distRoot = Join-Path $root "dist"
$stage = Join-Path $distRoot "Revit_Model_Health_Check_v${version}_Distribution"
$zip = Join-Path $distRoot "Revit_Model_Health_Check_v${version}_Distribution.zip"

if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
if (Test-Path $zip) { Remove-Item $zip -Force }
New-Item -ItemType Directory -Force -Path $stage | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $stage "payload") | Out-Null

foreach ($year in $years) {
    $target = Join-Path $stage "payload\$year\Revit Model Health Check"
    New-Item -ItemType Directory -Force -Path $target | Out-Null
    Copy-Item (Join-Path $installed[$year].PluginDir "*") $target -Recurse -Force
}

# The recipient installer is intentionally plain batch only. It performs file deployment;
# it does not invoke PowerShell, dotnet, MSBuild, NuGet, or any compiler.
$installCmd = @'
@echo off
setlocal EnableExtensions
set "APPNAME=Revit Model Health Check"
set "DLLNAME=SVMModelHealth.dll"
set "ADDINID=8E4E28AA-1D79-43E6-A44C-3DBA6AA82617"
set "ROOT=%~dp0"

:MENU
cls
echo ============================================================
echo Revit Model Health Check Installer
echo Precompiled file deployment only - no SDK required
echo ============================================================
echo.
echo   [1] Install for Revit 2025
echo   [2] Install for Revit 2026
echo   [3] Install for Revit 2027
echo   [A] Install All
echo   [Q] Quit
echo.
choice /C 123AQ /N /M "Choose: "
if errorlevel 5 goto :EOF
if errorlevel 4 goto :INSTALL_ALL
if errorlevel 3 goto :PICK_2027
if errorlevel 2 goto :PICK_2026
if errorlevel 1 goto :PICK_2025

goto :MENU

:PICK_2025
call :INSTALL_YEAR 2025
goto :AFTER

:PICK_2026
call :INSTALL_YEAR 2026
goto :AFTER

:PICK_2027
call :INSTALL_YEAR 2027
goto :AFTER

:INSTALL_ALL
call :INSTALL_YEAR 2025
call :INSTALL_YEAR 2026
call :INSTALL_YEAR 2027
goto :AFTER

:INSTALL_YEAR
set "YEAR=%~1"
set "SOURCE=%ROOT%payload\%YEAR%\%APPNAME%"
set "ADDINROOT=%APPDATA%\Autodesk\Revit\Addins\%YEAR%"
set "TARGET=%ADDINROOT%\%APPNAME%"
set "MANIFEST=%ADDINROOT%\%APPNAME%.addin"
set "ASSEMBLY=%TARGET%\%DLLNAME%"

echo.
echo Installing %APPNAME% for Revit %YEAR%...

if not exist "%SOURCE%\%DLLNAME%" (
  echo FAILED: packaged payload is missing for Revit %YEAR%.
  exit /b 10
)

tasklist /FI "IMAGENAME eq Revit.exe" 2>NUL | find /I "Revit.exe" >NUL
if not errorlevel 1 (
  echo FAILED: Revit is running. Save your work, close all Revit sessions, and run INSTALL.cmd again.
  exit /b 11
)

if not exist "%ADDINROOT%" mkdir "%ADDINROOT%" >NUL 2>&1
if errorlevel 1 (
  echo FAILED: could not create "%ADDINROOT%".
  exit /b 12
)

if exist "%TARGET%" rmdir /S /Q "%TARGET%" >NUL 2>&1
mkdir "%TARGET%" >NUL 2>&1
if errorlevel 1 (
  echo FAILED: could not create "%TARGET%".
  exit /b 13
)

xcopy "%SOURCE%\*" "%TARGET%\" /E /I /Y /Q >NUL
if errorlevel 1 (
  echo FAILED: could not copy the compiled add-in files.
  exit /b 14
)

>"%MANIFEST%" echo ^<?xml version="1.0" encoding="utf-8" standalone="no"?^>
>>"%MANIFEST%" echo ^<RevitAddIns^>
>>"%MANIFEST%" echo   ^<AddIn Type="Application"^>
>>"%MANIFEST%" echo     ^<Name^>%APPNAME%^</Name^>
>>"%MANIFEST%" echo     ^<Assembly^>%ASSEMBLY%^</Assembly^>
>>"%MANIFEST%" echo     ^<AddInId^>%ADDINID%^</AddInId^>
>>"%MANIFEST%" echo     ^<FullClassName^>SVMModelHealth.App^</FullClassName^>
>>"%MANIFEST%" echo     ^<VendorId^>SVM^</VendorId^>
>>"%MANIFEST%" echo     ^<VendorDescription^>Silicon Valley Mechanical^</VendorDescription^>
>>"%MANIFEST%" echo   ^</AddIn^>
>>"%MANIFEST%" echo ^</RevitAddIns^>

if not exist "%ASSEMBLY%" (
  echo FAILED: DLL verification failed after copy.
  exit /b 15
)
if not exist "%MANIFEST%" (
  echo FAILED: manifest verification failed after copy.
  exit /b 16
)

if exist "%ADDINROOT%\SVMModelHealth.addin" del /Q "%ADDINROOT%\SVMModelHealth.addin" >NUL 2>&1
if exist "%ADDINROOT%\SVMModelHealth" rmdir /S /Q "%ADDINROOT%\SVMModelHealth" >NUL 2>&1

echo SUCCESS: Revit %YEAR% installed.
echo   %MANIFEST%
echo   %TARGET%
exit /b 0

:AFTER
echo.
echo Press any key to return to the installer menu...
pause >NUL
goto :MENU
'@
Set-Content -Path (Join-Path $stage "INSTALL.cmd") -Value $installCmd -Encoding ASCII

$uninstallCmd = @'
@echo off
setlocal EnableExtensions
set "APPNAME=Revit Model Health Check"

tasklist /FI "IMAGENAME eq Revit.exe" 2>NUL | find /I "Revit.exe" >NUL
if not errorlevel 1 (
  echo Revit is running. Close all Revit sessions before uninstalling.
  pause
  exit /b 11
)

for %%Y in (2025 2026 2027) do (
  set "ADDINROOT=%APPDATA%\Autodesk\Revit\Addins\%%Y"
  call :REMOVE_YEAR %%Y
)
echo.
echo Uninstall complete.
pause
exit /b 0

:REMOVE_YEAR
set "YEAR=%~1"
set "ADDINROOT=%APPDATA%\Autodesk\Revit\Addins\%YEAR%"
if exist "%ADDINROOT%\%APPNAME%.addin" del /Q "%ADDINROOT%\%APPNAME%.addin" >NUL 2>&1
if exist "%ADDINROOT%\%APPNAME%" rmdir /S /Q "%ADDINROOT%\%APPNAME%" >NUL 2>&1
echo Revit %YEAR%: removed if present.
exit /b 0
'@
Set-Content -Path (Join-Path $stage "UNINSTALL.cmd") -Value $uninstallCmd -Encoding ASCII

$readme = @"
Revit Model Health Check v$version - Distribution Package
============================================================

This ZIP contains PRECOMPILED add-in files captured from the developer's locally installed and tested Revit add-in folders.

Recipient requirements:
- Windows
- The compatible Revit version/update for the packaged binary
- Permission to write to the current user's %%APPDATA%%\Autodesk\Revit\Addins\<year> folder

Recipient does NOT need:
- Visual Studio
- .NET SDK
- source code
- Git
- NuGet
- PowerShell

Install:
1. Extract this ZIP completely.
2. Save work and close all Revit sessions.
3. Double-click INSTALL.cmd.
4. Choose Revit 2025, 2026, 2027, or Install All.

The batch installer only copies the packaged compiled files and creates the per-user .addin manifest.
It does not compile anything.

IMPORTANT COMPATIBILITY NOTE
The packaged DLL for each year matches the Revit update/runtime used when it was built and tested. Revit 2025 and 2026 have .NET 8 / .NET 10 update transitions, so release testing must match the intended recipient update level.
"@
Set-Content -Path (Join-Path $stage "README.txt") -Value $readme -Encoding ASCII

$payloadInfo = @()
$payloadInfo += "Revit Model Health Check v$version"
$payloadInfo += "Captured from installed per-user add-in folders"
$payloadInfo += "Created: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')"
foreach ($year in $years) {
    $payloadDll = Join-Path $stage "payload\$year\Revit Model Health Check\SVMModelHealth.dll"
    $hash = (Get-FileHash -Algorithm SHA256 -Path $payloadDll).Hash
    $payloadInfo += "Revit $year | Assembly $version | SHA256 $hash"
}
Set-Content -Path (Join-Path $stage "PAYLOAD_INFO.txt") -Value $payloadInfo -Encoding ASCII

Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $zip -CompressionLevel Optimal
Write-Host "DISTRIBUTION PACKAGE CREATED FROM INSTALLED FILES:" -ForegroundColor Green
Write-Host $zip -ForegroundColor Green
Write-Host "Recipients only extract the ZIP, close Revit, and run INSTALL.cmd." -ForegroundColor Gray
Write-Host "No SDK, Visual Studio, PowerShell, or source code is required on the recipient computer." -ForegroundColor Gray
exit 0
