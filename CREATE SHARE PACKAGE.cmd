@echo off
setlocal
set "ROOT=%~dp0"
if not exist "%ROOT%scripts\Package.ps1" (
  echo PACKAGE FAILED: scripts\Package.ps1 was not found.
  echo Extract the source ZIP completely before running this file.
  pause
  exit /b 2
)
echo Creating shareable Revit Model Health Check installer package...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%scripts\Package.ps1"
set "EC=%ERRORLEVEL%"
echo.
if not "%EC%"=="0" echo PACKAGE FAILED with exit code %EC%.
if "%EC%"=="0" echo SHARE PACKAGE CREATED SUCCESSFULLY.
pause
exit /b %EC%
