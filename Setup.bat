@echo off
setlocal
title Sticky Access Setup
set "setupScript=%~dp0setup.ps1"
if exist "%~dp0support\setup.ps1" set "setupScript=%~dp0support\setup.ps1"
if not exist "%setupScript%" goto missingScript
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%setupScript%" %*
set "setupResult=%errorlevel%"
goto finish
:missingScript
echo StickyBusinessAccess installation files are not present.
echo Expected installer script: "%setupScript%"
echo Download StickyBusinessAccess-0.8.2-setup2-player.zip from the Assets section at:
echo https://github.com/MiloSummers/StickyBusinessAccess/releases/latest
echo Do not use GitHub's 'Source code (zip)' or 'Source code (tar.gz)' downloads.
echo Extract the entire player ZIP before running Setup.bat. Keep support beside Setup.bat.
echo No game files were changed.
set "setupResult=1"
:finish
echo.
echo Press any key to close this window.
pause >nul
exit /b %setupResult%
