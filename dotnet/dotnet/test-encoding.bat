@echo off
chcp 65001 > nul
echo.
echo =======================================
echo   OpenManus 快速构建测试
echo =======================================
echo.
echo 测试中文字符显示: ✓ 成功 ✗ 失败
echo Testing English characters: ✓ Success ✗ Failed
echo Testing special chars: 🚀 📁 🔨 ⚡
echo.
echo 按任意键继续构建...
pause > nul
echo.
echo 开始构建...
dotnet build OpenManus.sln --configuration Debug --verbosity quiet
if %ERRORLEVEL% equ 0 (
    echo.
    echo ✅ 构建成功完成！
    echo ✅ Build completed successfully!
) else (
    echo.
    echo ❌ 构建失败！
    echo ❌ Build failed!
)
echo.
pause
