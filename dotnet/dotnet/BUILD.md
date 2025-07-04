# OpenManus .NET 项目构建脚本

本目录包含了用于编译 OpenManus .NET 项目的多种构建脚本。

## 构建脚本说明

### 1. build.bat

**完整的批处理构建脚本**

- 自动检测 .NET SDK
- 清理 → 还原包 → 编译 Debug → 编译 Release
- 显示详细的构建过程和结果
- 适合完整的构建流程

```cmd
# 运行完整构建
build.bat
```

### 2. build.ps1

**高级 PowerShell 构建脚本**

提供更多选项和灵活性：

```powershell
# 显示帮助
.\build.ps1 -Help

# 只编译 Release 版本
.\build.ps1 -Configuration Release

# 编译并运行测试
.\build.ps1 -Test

# 详细输出的 Debug 编译
.\build.ps1 -Configuration Debug -Verbose

# 不清理的快速编译
.\build.ps1 -Clean:$false

# 指定输出目录
.\build.ps1 -OutputPath "C:\Build\Output"
```

### 3. quick-build.bat

**快速编译脚本（中文版）**

- 只进行 Debug 编译，不清理
- 适合开发过程中的快速测试
- 最快的编译方式
- 已修复中文显示乱码问题

```cmd
# 快速编译
quick-build.bat
```

### 4. quick-build-en.bat

**快速编译脚本（英文版）**

- 英文界面，避免编码问题
- 功能与中文版相同
- 适合国际化环境

```cmd
# 快速编译（英文版）
quick-build-en.bat
```

## 项目结构

编译脚本会构建以下项目：

```
src/
├── OpenManus.Core/      # 核心功能模块
├── OpenManus.Agent/     # 智能体服务
├── OpenManus.Flow/      # 流程管理
├── OpenManus.Llm/       # LLM 服务
├── OpenManus.Mcp/       # MCP 服务
├── OpenManus.Tools/     # 工具集合
├── OpenManus.Sandbox/   # 沙箱服务
├── OpenManus.Prompt/    # 提示服务
└── OpenManus.Console/   # 控制台应用
```

## 构建输出

编译完成后，输出文件将位于：

- Debug 版本: `src/[ProjectName]/bin/Debug/net9.0/`
- Release 版本: `src/[ProjectName]/bin/Release/net9.0/`

## 故障排除

### 常见问题

1. **找不到 dotnet 命令**

   - 确保已安装 .NET 9.0 SDK
   - 检查 PATH 环境变量

2. **NuGet 包还原失败**

   - 检查网络连接
   - 清理 NuGet 缓存: `dotnet nuget locals all --clear`
   - 重新运行构建脚本

3. **编译错误**

   - 检查代码语法错误
   - 确保所有依赖项版本兼容
   - 查看详细的错误信息

4. **PowerShell 执行策略问题**
   ```powershell
   # 临时允许脚本执行
   Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
   .\build.ps1
   ```

### 手动构建命令

如果脚本遇到问题，可以使用以下手动命令：

```cmd
# 清理
dotnet clean OpenManus.sln

# 还原包
dotnet restore OpenManus.sln

# 编译 Debug
dotnet build OpenManus.sln --configuration Debug

# 编译 Release
dotnet build OpenManus.sln --configuration Release

# 运行测试
dotnet test OpenManus.sln
```

## 开发建议

- **日常开发**: 使用 `quick-build.bat` 进行快速编译测试
- **完整构建**: 使用 `build.bat` 进行完整的构建验证
- **CI/CD 集成**: 使用 `build.ps1` 配合参数进行自动化构建
- **发布准备**: 使用 `build.ps1 -Configuration Release -Test` 进行发布前验证

## 性能优化

- 使用 `--no-restore` 跳过不必要的包还原
- 使用 `--no-build` 在测试时跳过重复编译
- 考虑使用增量编译来加快构建速度

## 编码问题修复

所有批处理脚本已添加 `chcp 65001` 命令来解决中文显示乱码问题：

- 自动设置 UTF-8 编码
- 确保中文字符正确显示
- 兼容不同的 Windows 代码页设置

## 使用体验优化

所有构建脚本已优化用户体验：

- ✓ **无需按键继续**: 脚本执行完毕自动返回命令行
- ✓ **中文显示正常**: 修复编码问题，支持中文字符
- ✓ **清晰的状态提示**: 成功/失败状态明确显示
- ✓ **快速执行**: 优化构建流程，提高效率
