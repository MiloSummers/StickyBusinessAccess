@echo off
title Sticky Access Setup
set "setupScript=%~dp0setup.ps1"
if exist "%~dp0support\setup.ps1" set "setupScript=%~dp0support\setup.ps1"
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%setupScript%" %*
set "setupResult=%errorlevel%"
echo.
echo Press any key to close this window.
pause >nul
exit /b %setupResult%
