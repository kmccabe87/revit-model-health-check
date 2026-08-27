@echo off
setlocal
set "ROOT=%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%ROOT%scripts\Uninstall.ps1"
set "EC=%ERRORLEVEL%"
pause
exit /b %EC%
