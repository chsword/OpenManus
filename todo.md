# OpenManus .NET 版本功能对齐 TODO

基于 Python 版与 .NET 版功能差异分析，以下是需要实现的功能清单。

## 已完成的基础设施工作

### ✅ 项目结构和基础架构

- [x] 升级到 .NET 9.0 目标框架
- [x] 集成 Microsoft.SemanticKernel 1.59.0
- [x] 升级所有 NuGet 包到最新稳定版本
- [x] 建立模块化项目结构（Core、Agent、Flow、Tools、Mcp、Console 等）
- [x] 实现基础依赖注入和服务注册
- [x] 集成 Serilog 日志记录系统
- [x] 配置 JSON 配置管理
- [x] 建立测试项目结构

### ✅ 基础接口定义

- [x] 实现 `IAgent` 接口和基础 `AgentService`
- [x] 实现 `IFlowService` 接口和基础 `FlowService`
- [x] 实现 `IToolService` 接口和基础 `ToolService`
- [x] 实现 `IMcpService` 接口和基础 `McpService`
- [x] 建立项目间引用关系

### ✅ 构建和编译

- [x] 修复所有 Semantic Kernel API 变更导致的编译错误
- [x] 确保所有 src 项目能够成功编译
- [x] 配置 Directory.Build.props 统一包管理
- [x] 修复 System.CommandLine 2.0 API 变更问题

## 1. Agent 架构和类型实现

### 1.1 ReAct 框架实现

- [ ] 扩展 `IAgent` 接口，包含状态管理、内存管理、步骤执行
- [ ] 实现 `IBaseAgent` 基础接口
- [ ] 实现 `ReActAgent` 抽象类，提供 Think-Act 循环
- [ ] 实现 `ToolCallAgent` 类，支持工具调用和执行
- [ ] 添加 Agent 状态枚举（IDLE, RUNNING, FINISHED）
- [ ] 实现 Agent 内存管理系统

### 1.2 专业化 Agent 实现

- [ ] 实现 `ManusAgent` - 通用多功能 Agent
- [ ] 实现 `DataAnalysisAgent` - 数据分析专用 Agent
- [ ] 实现 `BrowserAgent` - 浏览器操作专用 Agent
- [ ] 实现 `SWEAgent` - 软件工程 Agent
- [ ] 实现 `MCPAgent` - MCP 协议客户端 Agent

## 2. 工具生态系统开发

### 2.1 核心工具接口

- [x] 建立基础 `IToolService` 接口
- [x] 实现基础 `ToolService` 类（使用 Semantic Kernel）
- [ ] 实现 `IBaseTool` 接口和 `BaseTool` 抽象类
- [ ] 实现 `ToolCollection` 类用于工具管理
- [ ] 实现 `ToolResult` 类统一工具返回结果

### 2.2 网络搜索工具

- [ ] 实现 `WebSearchTool` 类
- [ ] 支持多搜索引擎（Google、Bing、DuckDuckGo）
- [ ] 实现搜索结果内容获取
- [ ] 实现搜索引擎故障转移机制
- [ ] 支持多语言和地区配置

### 2.3 浏览器自动化工具

- [ ] 实现 `BrowserUseTool` 类
- [ ] 集成 Playwright 或 Selenium
- [ ] 支持页面导航（go_to_url, go_back, refresh）
- [ ] 支持元素交互（click_element, input_text）
- [ ] 支持页面滚动（scroll_down, scroll_up, scroll_to_text）
- [ ] 支持下拉框操作（get_dropdown_options, select_dropdown_option）
- [ ] 支持标签页管理（switch_tab, open_tab, close_tab）
- [ ] 支持内容提取和分析（extract_content）
- [ ] 支持键盘操作（send_keys）

### 2.4 代码执行工具

- [ ] 实现 `PythonExecuteTool` 类
- [ ] 实现 `BashTool` 类（命令行执行）
- [ ] 实现代码执行沙箱环境
- [ ] 支持执行结果捕获和错误处理

### 2.5 文件操作工具

- [ ] 实现 `StrReplaceEditorTool` 类
- [ ] 支持文件读取、写入、编辑
- [ ] 实现文件内容搜索和替换
- [ ] 支持多种文件格式处理

### 2.6 数据分析和可视化工具

- [ ] 实现 `DataVisualizationTool` 类
- [ ] 实现 `VisualizationPrepareTool` 类
- [ ] 集成图表生成库（如 Plotly.NET、ScottPlot）
- [ ] 支持多种图表类型（线图、柱图、散点图等）

### 2.7 人机交互工具

- [ ] 实现 `AskHumanTool` 类
- [ ] 实现 `TerminateTool` 类
- [ ] 实现 `CreateChatCompletionTool` 类

### 2.8 规划工具

- [ ] 实现 `PlanningTool` 类
- [ ] 支持任务分解和步骤管理
- [ ] 实现计划状态追踪

## 3. Flow 工作流系统

### 3.1 Flow 架构

- [x] 建立基础 `IFlowService` 接口
- [x] 实现基础 `FlowService` 类
- [ ] 实现 `BaseFlow` 抽象类
- [ ] 支持多 Agent 管理和协作
- [ ] 实现 Agent 动态分配机制

### 3.2 具体 Flow 实现

- [ ] 实现 `PlanningFlow` 类
- [ ] 支持步骤级别的执行追踪
- [ ] 支持流程状态管理
- [ ] 实现错误处理和重试机制

### 3.3 Flow 工厂

- [ ] 实现 `FlowFactory` 类
- [ ] 支持不同类型的工作流创建
- [ ] 实现工作流配置管理

## 4. MCP (Model Context Protocol) 支持

### 4.1 MCP 客户端实现

- [x] 建立基础 `IMcpService` 接口
- [x] 实现基础 `McpService` 类（使用 Semantic Kernel）
- [ ] 实现完整的 `MCPClients` 类
- [ ] 支持 SSE 传输方式
- [ ] 支持 stdio 传输方式

### 4.2 MCP 功能特性

- [ ] 实现多服务器连接管理
- [ ] 实现动态工具发现
- [ ] 实现工具模式变更检测
- [ ] 实现连接生命周期管理
- [ ] 实现 `MCPClientTool` 代理工具

## 5. 配置和启动系统

### 5.1 配置管理

- [ ] 实现统一的配置系统
- [ ] 支持 MCP 服务器配置
- [ ] 支持搜索引擎配置
- [ ] 支持浏览器配置
- [ ] 支持工作区配置

### 5.2 启动模式

- [x] 建立基础 Console 应用结构
- [x] 实现基础命令行界面（FlowCommand、McpCommand）
- [ ] 实现基础 Agent 运行模式
- [ ] 实现 Flow 工作流运行模式
- [ ] 实现 MCP 专用运行模式
- [ ] 扩展 Console 应用以支持多种模式

## 6. 基础设施改进

### 6.1 异步编程模型

- [x] 在所有接口中采用异步/等待模式
- [x] 实现异步服务调用
- [ ] 实现异步工具执行
- [ ] 实现异步 Agent 运行

### 6.2 错误处理和日志

- [x] 集成 Serilog 日志记录系统
- [x] 在所有服务中实现基础错误处理
- [ ] 实现统一的错误处理机制
- [ ] 实现重试机制
- [ ] 实现调试和追踪功能

### 6.3 依赖注入和服务注册

- [x] 建立基础依赖注入容器配置
- [x] 注册核心服务（Agent、Flow、Tool、Mcp）
- [ ] 扩展服务注册到所有工具
- [ ] 实现服务生命周期管理

## 7. 测试和质量保证

### 7.1 单元测试

- [x] 建立测试项目结构（Core.Tests、Agent.Tests、Flow.Tests）
- [x] 配置测试依赖（xunit、Moq、Microsoft.NET.Test.Sdk）
- [ ] 为所有 Agent 类编写单元测试
- [ ] 为所有 Tool 类编写单元测试
- [ ] 为 Flow 系统编写单元测试
- [ ] 为 MCP 客户端编写单元测试

### 7.2 集成测试

- [ ] 编写 Agent-Tool 集成测试
- [ ] 编写 Flow 端到端测试
- [ ] 编写 MCP 连接测试

### 7.3 性能测试

- [ ] 测试大规模工具执行性能
- [ ] 测试多 Agent 并发性能
- [ ] 测试内存使用情况

## 8. 文档和示例

### 8.1 API 文档

- [ ] 为所有公共接口生成 XML 文档
- [ ] 编写使用指南
- [ ] 编写工具开发指南

### 8.2 示例项目

- [ ] 创建基础 Agent 使用示例
- [ ] 创建数据分析示例
- [ ] 创建浏览器自动化示例
- [ ] 创建 MCP 集成示例

## 9. 部署和打包

### 9.1 NuGet 包

- [ ] 创建核心库 NuGet 包
- [ ] 创建工具扩展 NuGet 包
- [ ] 创建 MCP 客户端 NuGet 包

### 9.2 容器化

- [ ] 创建 Docker 镜像
- [ ] 支持容器化部署
- [ ] 实现健康检查

## 阶段进度总结

### ✅ 已完成的基础阶段（约 30% 进度）

1. **项目基础架构**：完成 .NET 9.0 升级、Semantic Kernel 集成、包管理
2. **核心接口定义**：建立所有主要服务接口
3. **项目结构**：完成模块化设计和项目间依赖
4. **基础设施**：日志、配置、依赖注入基础实现
5. **测试框架**：建立测试项目和依赖

### 🚧 当前优先任务（下一步）

1. **Agent 架构扩展**：实现 ReAct 框架和状态管理
2. **工具系统**：实现 BaseTool 和 ToolCollection
3. **核心工具**：文件操作、命令执行工具

## 功能对齐优先级

### 极高优先级（P0）- 核心架构

**目标**：建立与 Python 版等价的基础框架

1. **Agent 架构扩展**

   - 实现 ReAct 思考-行动循环
   - Agent 状态管理（IDLE, RUNNING, FINISHED）
   - 内存和消息历史管理

2. **工具基础框架**

   - 工具接口标准化（IBaseTool、ToolResult）
   - 工具集合管理（ToolCollection）
   - 与 Semantic Kernel 1.59.0 集成

3. **基础工具实现**
   - 文件操作工具（对标 StrReplaceEditor）
   - 命令行工具（PowerShell 适配）

### 高优先级（P1）- 核心功能

**目标**：实现基本可用的 Agent 系统

1. **通用 Agent**

   - ManusAgent 通用多功能实现
   - 工具调用和执行机制
   - 基础对话能力

2. **核心工具扩展**
   - Python 代码执行工具
   - 人机交互工具（AskHuman、Terminate）

### 中优先级（P2）- 高级功能

**目标**：达到 Python 版功能对等

1. **浏览器自动化**

   - 基于 Playwright.NET 的完整实现
   - 17 种浏览器操作对标
   - 截图和内容提取

2. **网络搜索工具**

   - 多搜索引擎支持
   - 内容获取和故障转移

3. **MCP 协议完整实现**

   - 多服务器连接管理
   - SSE/stdio 传输支持
   - 动态工具发现

4. **专业化 Agent**
   - MCPAgent、BrowserAgent 等

### 低优先级（P3）- 完善优化

**目标**：功能完善和用户体验

1. **数据分析和可视化**
2. **Flow 工作流系统**
3. **配置和部署优化**

## 特殊要求

### 技术约束

- **Semantic Kernel 版本锁定**：必须使用 1.59.0，不升级
- **PowerShell 适配**：命令行工具适配 Windows PowerShell 环境
- **Python 代码不修改**：仅作参考，不修改 Python 端任何代码

### 架构要求

- **功能对等**：.NET 版本必须与 Python 版功能完全对等
- **API 兼容性**：适配 SK 1.59.0 的 API 变更
- **异步优先**：所有操作采用异步模式
- **.NET 生态适配**：使用 Microsoft.Extensions.\* 系列包

### 开发流程

- **阶段化实施**：按优先级分阶段实现，确保每个阶段可用
- **测试驱动**：每个功能配套相应测试用例
- **文档同步**：重要决策和需求持久化记录

## 里程碑计划

**阶段 0**：基础设施建设（已完成）

- ✅ 项目架构和依赖升级
- ✅ 核心接口和服务定义
- ✅ 构建和测试环境配置

**阶段 1**：核心架构实现（当前阶段 - 4-6 周）

- Agent 架构扩展（ReAct、状态管理）
- 基础工具系统（文件操作、命令行）
- ManusAgent 基础实现

**阶段 2**：功能扩展（6-8 周）

- 浏览器自动化工具
- 网络搜索工具
- 完整工具集合

**阶段 3**：高级功能（4-6 周）

- MCP 协议完整支持
- 专业化 Agent 实现
- Flow 工作流系统

**总预计时间**：16-23 周

---

_注：此 TODO 清单基于 Python 版与 .NET 版功能差异分析生成，旨在将 .NET 版本功能对齐至 Python 版本水平。已完成的基础设施工作包括：Semantic Kernel 1.59.0 集成、.NET 9.0 升级、全面包更新、项目结构建立和核心接口定义。_
