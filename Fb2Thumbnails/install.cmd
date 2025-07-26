@echo off
setlocal

set "BASE_DIR=%~dp0"
if "%BASE_DIR:~-1%"=="\" set "BASE_DIR=%BASE_DIR:~0,-1%"
set "EXE_PATH=%BASE_DIR%\Fb2Thumbnails.dll"

echo Installing thumbnails provider
%windir%\Microsoft.NET\Framework\v4.0.30319\RegAsm /codebase %EXE_PATH%
%windir%\Microsoft.NET\Framework64\v4.0.30319\RegAsm /codebase %EXE_PATH%
reg add "HKCR\.fb2\shellex\{9ba63b33-9569-4d0c-97a4-5b0f7774c0aa}" /ve /d "{a547a343-8d94-4b84-8649-30f648de3fe2}" /f
reg add "HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Shell Extensions\Approved" /v "{a547a343-8d94-4b84-8649-30f648de3fe2}" /t REG_SZ /d "Fb2Kindle.ThumbnailProvider" /f

pause
