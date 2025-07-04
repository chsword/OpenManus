@echo off
chcp 65001 > nul
REM OpenManus .NET 项目编译脚本
REM 编译所有 OpenManus 项目

echo ========================================
echo      OpenManus .NET 项目编译
echo ========================================
echo.

REM 检查是否存在 dotnet CLI
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo 错误: 未找到 dotnet CLI。请安装 .NET SDK。
    exit /b 1
)

REM 显示 .NET 版本信息
echo 当前 .NET 版本:
dotnet --version
echo.

REM 切换到脚本所在目录
cd /d "%~dp0"

echo 开始编译 OpenManus 解决方案...
echo.

REM 清理之前的构建输出
echo [1/4] 清理项目...
dotnet clean OpenManus.sln --configuration Release
if %ERRORLEVEL% neq 0 (
    echo 错误: 清理失败
    goto :error
)
echo 清理完成。
echo.

REM 还原 NuGet 包
echo [2/4] 还原 NuGet 包...
dotnet restore OpenManus.sln
if %ERRORLEVEL% neq 0 (
    echo 错误: 包还原失败
    goto :error
)
echo 包还原完成。
echo.

REM 编译解决方案 (Debug 配置)
echo [3/4] 编译解决方案 (Debug 配置)...
dotnet build OpenManus.sln --configuration Debug --no-restore
if %ERRORLEVEL% neq 0 (
    echo 错误: Debug 编译失败
    goto :error
)
echo Debug 编译完成。
echo.

REM 编译解决方案 (Release 配置)
echo [4/4] 编译解决方案 (Release 配置)...
dotnet build OpenManus.sln --configuration Release --no-restore
if %ERRORLEVEL% neq 0 (
    echo 错误: Release 编译失败
    goto :error
)
echo Release 编译完成。
echo.

echo ========================================
echo        编译成功完成！
echo ========================================
echo.
echo 所有项目已成功编译：
echo - OpenManus.Core
echo - OpenManus.Agent
echo - OpenManus.Flow
echo - OpenManus.Llm
echo - OpenManus.Mcp
echo - OpenManus.Tools
echo - OpenManus.Sandbox
echo - OpenManus.Prompt
echo - OpenManus.Console
echo.
exit /b 0

:error
echo.
echo ========================================
echo          编译失败！
echo ========================================
echo 请检查错误信息并修复后重试。
echo.
exit /b 1
