@echo off
setlocal
set "ROOT=%~dp0"
if not exist "%ROOT%scripts\Build.ps1" (
  echo BUILD FAILED: scripts\Build.ps1 was not found.
  echo Extract the ZIP completely before running BUILD.cmd.
  pause
  exit /b 2
)
echo Running Revit Model Health Check builds for Revit 2025, 2026, and 2027...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%scripts\Build.ps1"
set "EC=%ERRORLEVEL%"
echo.
if not "%EC%"=="0" echo BUILD FAILED with exit code %EC%.
if "%EC%"=="0" echo ALL BUILDS SUCCEEDED.
pause
exit /b %EC%
