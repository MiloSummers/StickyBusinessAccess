@echo off
set "uninstallScript=%~dp0uninstall.ps1"
if exist "%~dp0support\uninstall.ps1" set "uninstallScript=%~dp0support\uninstall.ps1"
"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File "%uninstallScript%" %*
set "setupResult=%errorlevel%"
echo Press any key to close this window.
pause >nul
exit /b %setupResult%
