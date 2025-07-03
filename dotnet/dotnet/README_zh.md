# OpenManus .NET 项目

欢迎使用 OpenManus 项目！这是一个多功能代理，能够使用多种工具解决各种任务。该项目使用 .NET 9 开发，并集成了最新的 Semantic Kernel alpha 版本。

## 项目结构

- **src**: 包含所有源代码和项目文件。
  - **OpenManus.Core**: 核心库，包含数据模型、异常处理、配置和日志服务。
  - **OpenManus.Agent**: 代理服务，定义代理接口和实现。
  - **OpenManus.Flow**: 流程服务，管理流程相关的功能。
  - **OpenManus.Llm**: LLM 服务，提供与 LLM 相关的功能。
  - **OpenManus.Mcp**: MCP 服务，处理与 MCP 相关的功能。
  - **OpenManus.Tools**: 工具服务，提供各种工具功能。
  - **OpenManus.Sandbox**: 沙盒服务，提供安全的执行环境。
  - **OpenManus.Prompt**: 提示服务，处理与提示相关的功能。
  - **OpenManus.Console**: 控制台应用程序，提供命令行接口。

- **tests**: 包含所有单元测试项目，确保代码的质量和稳定性。

- **config**: 包含应用程序的配置文件。

- **examples**: 提供使用示例，展示如何使用该项目的功能。

- **workspace**: 保持工作区目录的存在。

## 安装和使用

1. 克隆该项目到本地：
   ```
   git clone https://github.com/FoundationAgents/OpenManus.git
   ```

2. 进入项目目录：
   ```
   cd OpenManus/dotnet
   ```

3. 使用 .NET CLI 构建项目：
   ```
   dotnet build
   ```

4. 运行控制台应用程序：
   ```
   dotnet run --project src/OpenManus.Console/OpenManus.Console.csproj
   ```

## 贡献

欢迎任何形式的贡献！请查看 [贡献指南](CONTRIBUTING.md) 以获取更多信息。

## 许可证

该项目采用 MIT 许可证，详细信息请查看 [LICENSE](LICENSE) 文件。

感谢您使用 OpenManus 项目！如有任何问题，请随时联系开发团队。