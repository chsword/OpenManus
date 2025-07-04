@echo off
chcp 65001 > nul
REM Quick build script - build only, no clean
cd /d "%~dp0"
echo Building OpenManus project...
dotnet build OpenManus.sln --configuration Debug
if %ERRORLEVEL% equ 0 (
    echo [SUCCESS] Build completed successfully!
) else (
    echo [ERROR] Build failed!
)
