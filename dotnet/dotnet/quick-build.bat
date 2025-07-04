@echo off
chcp 65001 > nul
REM 快速编译脚本 - 仅编译，不清理
cd /d "%~dp0"
echo 正在快速编译 OpenManus 项目...
dotnet build OpenManus.sln --configuration Debug
if %ERRORLEVEL% equ 0 (
    echo ✓ 编译成功！
) else (
    echo ✗ 编译失败！
)
