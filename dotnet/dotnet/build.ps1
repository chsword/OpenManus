# OpenManus .NET 项目编译 PowerShell 脚本
# 提供更高级的编译功能和选项

param(
    [Parameter(HelpMessage="编译配置 (Debug/Release)")]
    [ValidateSet("Debug", "Release", "Both")]
    [string]$Configuration = "Both",

    [Parameter(HelpMessage="是否清理之前的构建")]
    [switch]$Clean = $true,

    [Parameter(HelpMessage="是否运行测试")]
    [switch]$Test = $false,

    [Parameter(HelpMessage="是否显示详细输出")]
    [switch]$Verbose = $false,

    [Parameter(HelpMessage="编译输出目录")]
    [string]$OutputPath = "",

    [Parameter(HelpMessage="显示帮助信息")]
    [switch]$Help
)

# 显示帮助信息
if ($Help) {
    Write-Host @"
OpenManus .NET 项目编译脚本

用法: .\build.ps1 [参数]

参数:
  -Configuration <config>   编译配置: Debug, Release, Both (默认: Both)
  -Clean                    清理之前的构建 (默认: true)
  -Test                     编译后运行测试 (默认: false)
  -Verbose                  显示详细输出 (默认: false)
  -OutputPath <path>        指定输出目录
  -Help                     显示此帮助信息

示例:
  .\build.ps1                                # 编译 Debug 和 Release 版本
  .\build.ps1 -Configuration Release         # 只编译 Release 版本
  .\build.ps1 -Test                         # 编译并运行测试
  .\build.ps1 -Configuration Debug -Verbose # Debug 编译，显示详细信息
"@
    exit 0
}

# 设置错误处理
$ErrorActionPreference = "Stop"

# 脚本开始时间
$startTime = Get-Date

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "      OpenManus .NET 项目编译" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host

# 检查 dotnet CLI
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ 检测到 .NET SDK 版本: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ 错误: 未找到 dotnet CLI。请安装 .NET SDK。" -ForegroundColor Red
    exit 1
}

# 切换到脚本目录
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# 解决方案文件
$solutionFile = "OpenManus.sln"
if (-not (Test-Path $solutionFile)) {
    Write-Host "✗ 错误: 未找到解决方案文件 $solutionFile" -ForegroundColor Red
    exit 1
}

Write-Host "📁 工作目录: $scriptPath" -ForegroundColor Yellow
Write-Host "📄 解决方案文件: $solutionFile" -ForegroundColor Yellow
Write-Host

try {
    # 步骤计数器
    $step = 1
    $totalSteps = 3 + ($Configuration -eq "Both" ? 2 : 1) + ($Test ? 1 : 0)

    # 清理项目
    if ($Clean) {
        Write-Host "[$step/$totalSteps] 🧹 清理项目..." -ForegroundColor Yellow
        $cleanArgs = @("clean", $solutionFile)
        if ($Verbose) { $cleanArgs += "--verbosity", "normal" }

        & dotnet @cleanArgs
        if ($LASTEXITCODE -ne 0) { throw "清理失败" }
        Write-Host "✓ 清理完成" -ForegroundColor Green
        Write-Host
        $step++
    }

    # 还原包
    Write-Host "[$step/$totalSteps] 📦 还原 NuGet 包..." -ForegroundColor Yellow
    $restoreArgs = @("restore", $solutionFile)
    if ($Verbose) { $restoreArgs += "--verbosity", "normal" }

    & dotnet @restoreArgs
    if ($LASTEXITCODE -ne 0) { throw "包还原失败" }
    Write-Host "✓ 包还原完成" -ForegroundColor Green
    Write-Host
    $step++

    # 编译函数
    function Build-Configuration {
        param([string]$config)

        Write-Host "[$step/$totalSteps] 🔨 编译解决方案 ($config 配置)..." -ForegroundColor Yellow

        $buildArgs = @("build", $solutionFile, "--configuration", $config, "--no-restore")
        if ($OutputPath) { $buildArgs += "--output", $OutputPath }
        if ($Verbose) { $buildArgs += "--verbosity", "normal" }

        & dotnet @buildArgs
        if ($LASTEXITCODE -ne 0) { throw "$config 编译失败" }
        Write-Host "✓ $config 编译完成" -ForegroundColor Green
        Write-Host

        $script:step++
    }

    # 根据配置参数进行编译
    switch ($Configuration) {
        "Debug" { Build-Configuration "Debug" }
        "Release" { Build-Configuration "Release" }
        "Both" {
            Build-Configuration "Debug"
            Build-Configuration "Release"
        }
    }

    # 运行测试
    if ($Test) {
        Write-Host "[$step/$totalSteps] 🧪 运行测试..." -ForegroundColor Yellow

        $testArgs = @("test", $solutionFile, "--no-build")
        if ($Configuration -ne "Both") { $testArgs += "--configuration", $Configuration }
        if ($Verbose) { $testArgs += "--verbosity", "normal" }

        & dotnet @testArgs
        if ($LASTEXITCODE -ne 0) { throw "测试失败" }
        Write-Host "✓ 所有测试通过" -ForegroundColor Green
        Write-Host
    }

    # 成功完成
    $endTime = Get-Date
    $duration = $endTime - $startTime

    Write-Host "========================================" -ForegroundColor Green
    Write-Host "        编译成功完成！" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host
    Write-Host "📊 编译统计:" -ForegroundColor Yellow
    Write-Host "   • 配置: $Configuration" -ForegroundColor White
    Write-Host "   • 耗时: $($duration.ToString('mm\:ss'))" -ForegroundColor White
    if ($OutputPath) { Write-Host "   • 输出目录: $OutputPath" -ForegroundColor White }
    Write-Host
    Write-Host "📁 已编译的项目:" -ForegroundColor Yellow
    $projects = @(
        "OpenManus.Core",
        "OpenManus.Agent",
        "OpenManus.Flow",
        "OpenManus.Llm",
        "OpenManus.Mcp",
        "OpenManus.Tools",
        "OpenManus.Sandbox",
        "OpenManus.Prompt",
        "OpenManus.Console"
    )
    foreach ($project in $projects) {
        Write-Host "   ✓ $project" -ForegroundColor Green
    }
    Write-Host

} catch {
    $endTime = Get-Date
    $duration = $endTime - $startTime

    Write-Host
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "          编译失败！" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host
    Write-Host "❌ 错误: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "⏱️ 失败前耗时: $($duration.ToString('mm\:ss'))" -ForegroundColor Yellow
    Write-Host
    Write-Host "🔧 建议检查:" -ForegroundColor Yellow
    Write-Host "   • 确保所有 NuGet 包版本兼容" -ForegroundColor White
    Write-Host "   • 检查代码语法错误" -ForegroundColor White
    Write-Host "   • 确保 .NET SDK 版本正确" -ForegroundColor White
    Write-Host "   • 查看详细错误信息" -ForegroundColor White
    Write-Host
    exit 1
}
