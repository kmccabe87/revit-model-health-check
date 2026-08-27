@echo off
setlocal
set "ROOT=%~dp0"
if not exist "%ROOT%scripts\InstallMenu.ps1" (
  echo INSTALL FAILED: scripts\InstallMenu.ps1 was not found.
  echo Extract the ZIP completely before running INSTALL.cmd.
  pause
  exit /b 2
)
echo Opening Revit Model Health Check installer...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%scripts\InstallMenu.ps1"
set "EC=%ERRORLEVEL%"
if not "%EC%"=="0" (
  echo.
  echo INSTALLER FAILED with exit code %EC%.
  pause
)
exit /b %EC%
