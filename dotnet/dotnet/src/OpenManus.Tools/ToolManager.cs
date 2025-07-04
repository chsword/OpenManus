using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenManus.Core.Models;
using OpenManus.Core.Services;

namespace OpenManus.Tools
{
    /// <summary>
    /// Default implementation of IToolManager for managing and executing tools
    /// </summary>
    public class ToolManager : IToolManager
    {
        private readonly Dictionary<string, IBaseTool> _tools = new();
        private readonly ILogger<ToolManager> _logger;

        /// <summary>
        /// Initialize ToolManager with logger
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public ToolManager(ILogger<ToolManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public void RegisterTool(IBaseTool tool)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            if (string.IsNullOrWhiteSpace(tool.Name))
                throw new ArgumentException("Tool name cannot be empty", nameof(tool));

            _tools[tool.Name] = tool;
            _logger.LogInformation("🔧 Registered tool: {ToolName}", tool.Name);
        }

        /// <inheritdoc />
        public void UnregisterTool(string toolName)
        {
            if (string.IsNullOrWhiteSpace(toolName))
                return;

            if (_tools.Remove(toolName))
            {
                _logger.LogInformation("🗑️ Unregistered tool: {ToolName}", toolName);
            }
        }

        /// <inheritdoc />
        public IBaseTool? GetTool(string toolName)
        {
            if (string.IsNullOrWhiteSpace(toolName))
                return null;

            _tools.TryGetValue(toolName, out var tool);
            return tool;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAvailableTools()
        {
            return _tools.Keys.ToList();
        }

        /// <inheritdoc />
        public IList<ToolDefinition> GetToolDefinitions()
        {
            var definitions = new List<ToolDefinition>();

            foreach (var tool in _tools.Values)
            {
                definitions.Add(new ToolDefinition
                {
                    Type = "function",
                    Function = new FunctionDefinition
                    {
                        Name = tool.Name,
                        Description = tool.Description,
                        Parameters = tool.Parameters
                    }
                });
            }

            return definitions;
        }

        /// <inheritdoc />
        public async Task<ToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object>? parameters = null)
        {
            if (string.IsNullOrWhiteSpace(toolName))
            {
                var error = "Tool name cannot be empty";
                _logger.LogError("❌ {Error}", error);
                return ToolResult.Failure(error);
            }

            var tool = GetTool(toolName);
            if (tool == null)
            {
                var error = $"Tool not found: {toolName}";
                _logger.LogError("❌ {Error}", error);
                return ToolResult.Failure(error);
            }

            try
            {
                _logger.LogInformation("🚀 Executing tool: {ToolName}", toolName);
                var result = await tool.ExecuteAsync(parameters);

                if (result.IsSuccess())
                {
                    _logger.LogInformation("✅ Tool executed successfully: {ToolName}", toolName);
                }
                else
                {
                    _logger.LogWarning("⚠️ Tool execution failed: {ToolName} - {Error}", toolName, result.ErrorMessage());
                }

                return result;
            }
            catch (Exception ex)
            {
                var error = $"Tool execution threw exception: {ex.Message}";
                _logger.LogError(ex, "💥 {Error}", error);
                return ToolResult.Failure(error);
            }
        }

        /// <inheritdoc />
        public async Task<ToolResult> ExecuteToolCallAsync(ToolCall toolCall)
        {
            if (toolCall?.Function == null)
            {
                var error = "Invalid tool call: missing function";
                _logger.LogError("❌ {Error}", error);
                return ToolResult.Failure(error);
            }

            var functionName = toolCall.Function.Name;
            var argumentsJson = toolCall.Function.Arguments;

            // Parse arguments from JSON string to dictionary
            Dictionary<string, object>? parameters = null;

            if (!string.IsNullOrEmpty(argumentsJson))
            {
                try
                {
                    parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(argumentsJson);
                }
                catch (JsonException ex)
                {
                    var error = $"Failed to parse tool arguments: {ex.Message}";
                    _logger.LogError(ex, "❌ {Error}", error);
                    return ToolResult.Failure(error);
                }
            }

            return await ExecuteToolAsync(functionName, parameters);
        }

        /// <inheritdoc />
        public bool HasTool(string toolName)
        {
            return !string.IsNullOrWhiteSpace(toolName) && _tools.ContainsKey(toolName);
        }

        /// <inheritdoc />
        public ToolInfo? GetToolInfo(string toolName)
        {
            var tool = GetTool(toolName);
            if (tool == null)
                return null;

            return new ToolInfo
            {
                Name = tool.Name,
                Description = tool.Description,
                Parameters = tool.Parameters,
                IsAvailable = true
            };
        }

        /// <summary>
        /// Register multiple tools at once
        /// </summary>
        /// <param name="tools">Tools to register</param>
        public void RegisterTools(IEnumerable<IBaseTool> tools)
        {
            foreach (var tool in tools)
            {
                RegisterTool(tool);
            }
        }

        /// <summary>
        /// Get count of registered tools
        /// </summary>
        public int ToolCount => _tools.Count;

        /// <summary>
        /// Clear all registered tools
        /// </summary>
        public void ClearTools()
        {
            var count = _tools.Count;
            _tools.Clear();
            _logger.LogInformation("🧹 Cleared all tools ({Count} removed)", count);
        }
    }
}
