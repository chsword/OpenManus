using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenManus.Agent.Specialized;
using OpenManus.Tools;
using OpenManus.Tools.Core;
using OpenManus.Tools.FileOperations;
using OpenManus.Tools.FileSystem;
using OpenManus.Tools.System;
using OpenManus.Tools.Web;

namespace OpenManus.Agent.Specialized
{
    /// <summary>
    /// A versatile general-purpose agent with support for multiple tools.
    /// Equivalent to Python's Manus agent - the primary multi-functional agent.
    /// </summary>
    public class ManusAgent : ToolCallAgent
    {
        private const string DefaultSystemPrompt = @"You are Manus, a versatile AI assistant that can help with various tasks using multiple tools.

You have access to the following capabilities:
- File operations: creating, editing, viewing files and directories
- Command execution: running shell commands and scripts
- Human interaction: asking questions when you need clarification
- Task completion management

Your approach should be:
1. Think carefully about what the user is asking
2. Break down complex tasks into smaller steps
3. Use the appropriate tools to accomplish each step
4. Ask for clarification when needed
5. Provide clear explanations of what you're doing

Always be helpful, accurate, and explain your reasoning. When working with files or commands, be careful and double-check your work.

Current working directory: {0}";

        private const string DefaultNextStepPrompt = "Based on the current conversation and available tools, what should be the next step to help the user?";

        /// <summary>
        /// Initialize ManusAgent with comprehensive tool collection.
        /// </summary>
        public ManusAgent(
            Kernel kernel,
            ILogger<BaseAgent> logger,
            IFileOperator fileOperator,
            string? workingDirectory = null,
            string? name = null,
            string? description = null,
            HttpClient? httpClient = null,
            IConfiguration? configuration = null,
            ILoggerFactory? loggerFactory = null)
            : base(kernel, logger, CreateToolCollection(fileOperator, logger), name ?? "Manus",
                  description ?? "A versatile agent that can solve various tasks using multiple tools")
        {
            // Configure the agent
            SystemPrompt = string.Format(DefaultSystemPrompt, workingDirectory ?? Environment.CurrentDirectory);
            NextStepPrompt = DefaultNextStepPrompt;
            MaxSteps = 20;
            MaxObserve = 10000;

            _logger.LogInformation("🚀 Manus agent initialized with working directory: {WorkingDirectory}",
                workingDirectory ?? Environment.CurrentDirectory);
        }

        /// <summary>
        /// Factory method to create ManusAgent with all required dependencies.
        /// </summary>
        public static ManusAgent Create(
            Kernel kernel,
            ILogger<BaseAgent> logger,
            IFileOperator? fileOperator = null,
            string? workingDirectory = null)
        {
            // Create default file operator if none provided
            fileOperator ??= new LocalFileOperator(
                Microsoft.Extensions.Logging.Abstractions.NullLogger<LocalFileOperator>.Instance);

            return new ManusAgent(kernel, logger, fileOperator, workingDirectory);
        }

        /// <summary>
        /// Create and configure the tool collection for Manus agent.
        /// </summary>
        private static ToolCollection CreateToolCollection(IFileOperator fileOperator, ILogger logger)
        {
            var toolLogger = logger as ILogger<BaseTool> ??
                Microsoft.Extensions.Logging.Abstractions.NullLogger<BaseTool>.Instance;

            var toolCollection = new ToolCollection(
                Microsoft.Extensions.Logging.Abstractions.NullLogger<ToolCollection>.Instance);

            // Add core tools
            toolCollection.AddTools(
                new StrReplaceEditorTool(fileOperator, toolLogger),
                new BashTool(fileOperator, toolLogger),
                new AskHumanTool(toolLogger),
                new TerminateTool(toolLogger)
            );

            return toolCollection;
        }

        /// <summary>
        /// Enhanced run method with initialization logging.
        /// </summary>
        public override async Task<string> RunAsync(string? request = null)
        {
            _logger.LogInformation("🎯 Manus agent starting task: {Request}", request ?? "General assistance");

            try
            {
                var result = await base.RunAsync(request);
                _logger.LogInformation("✅ Manus agent completed task successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Manus agent encountered an error during execution");
                throw;
            }
        }

        /// <summary>
        /// Add a custom tool to the agent's tool collection.
        /// </summary>
        public void AddTool(IBaseTool tool)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            _toolCollection.AddTool(tool);
            _logger.LogInformation("🔧 Added custom tool to Manus agent: {ToolName}", tool.Name);
        }

        /// <summary>
        /// Get information about available tools.
        /// </summary>
        public string GetAvailableTools()
        {
            var tools = string.Join(", ", _toolCollection.ToolNames);
            return $"Available tools: {tools}";
        }
    }
}
