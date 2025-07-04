using System.Collections.Generic;
using System.Threading.Tasks;
using OpenManus.Core.Models;

namespace OpenManus.Core.Services
{
    /// <summary>
    /// Interface for managing and executing tools
    /// </summary>
    public interface IToolManager
    {
        /// <summary>
        /// Register a tool for use by agents
        /// </summary>
        /// <param name="tool">Tool to register</param>
        void RegisterTool(IBaseTool tool);

        /// <summary>
        /// Unregister a tool
        /// </summary>
        /// <param name="toolName">Name of tool to unregister</param>
        void UnregisterTool(string toolName);

        /// <summary>
        /// Get a tool by name
        /// </summary>
        /// <param name="toolName">Name of the tool</param>
        /// <returns>Tool instance or null if not found</returns>
        IBaseTool? GetTool(string toolName);

        /// <summary>
        /// Get all available tool names
        /// </summary>
        /// <returns>List of available tool names</returns>
        IEnumerable<string> GetAvailableTools();

        /// <summary>
        /// Get tool definitions for LLM tool calling
        /// </summary>
        /// <returns>List of tool definitions</returns>
        IList<ToolDefinition> GetToolDefinitions();

        /// <summary>
        /// Execute a tool by name with parameters
        /// </summary>
        /// <param name="toolName">Name of the tool to execute</param>
        /// <param name="parameters">Parameters for the tool</param>
        /// <returns>Tool execution result</returns>
        Task<ToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object>? parameters = null);

        /// <summary>
        /// Execute a tool call from LLM
        /// </summary>
        /// <param name="toolCall">Tool call to execute</param>
        /// <returns>Tool execution result</returns>
        Task<ToolResult> ExecuteToolCallAsync(ToolCall toolCall);

        /// <summary>
        /// Check if a tool is registered
        /// </summary>
        /// <param name="toolName">Name of the tool</param>
        /// <returns>True if tool is registered</returns>
        bool HasTool(string toolName);

        /// <summary>
        /// Get information about a specific tool
        /// </summary>
        /// <param name="toolName">Name of the tool</param>
        /// <returns>Tool information or null if not found</returns>
        ToolInfo? GetToolInfo(string toolName);
    }

    /// <summary>
    /// Information about a tool
    /// </summary>
    public class ToolInfo
    {
        /// <summary>
        /// Tool name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tool description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tool parameters schema
        /// </summary>
        public object? Parameters { get; set; }

        /// <summary>
        /// Whether the tool is available
        /// </summary>
        public bool IsAvailable { get; set; } = true;
    }
}
