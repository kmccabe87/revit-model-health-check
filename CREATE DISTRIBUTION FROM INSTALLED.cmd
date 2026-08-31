@echo off
setlocal
set "ROOT=%~dp0"
if not exist "%ROOT%scripts\Package.ps1" (
  echo PACKAGE FAILED: scripts\Package.ps1 was not found.
  echo Extract the source ZIP completely before running this file.
  pause
  exit /b 2
)
echo Creating distribution from the exact locally installed Revit Model Health Check files...
echo.
echo Required sequence: BUILD - INSTALL - TEST IN REVIT - CLOSE REVIT - PACKAGE
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%scripts\Package.ps1"
set "EC=%ERRORLEVEL%"
echo.
if not "%EC%"=="0" echo PACKAGE FAILED with exit code %EC%.
if "%EC%"=="0" echo DISTRIBUTION PACKAGE CREATED SUCCESSFULLY FROM INSTALLED FILES.
pause
exit /b %EC%
