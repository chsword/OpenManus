using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenManus.Core.Models;

namespace OpenManus.Tools
{
    /// <summary>
    /// Base interface for all tools in the OpenManus system.
    /// </summary>
    public interface IBaseTool
    {
        /// <summary>
        /// The unique name of the tool.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of what this tool does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// JSON schema defining the parameters this tool accepts.
        /// </summary>
        Dictionary<string, object>? Parameters { get; }

        /// <summary>
        /// Execute the tool with the given parameters.
        /// </summary>
        /// <param name="parameters">Parameters for tool execution</param>
        /// <returns>The result of the tool execution</returns>
        Task<ToolResult> ExecuteAsync(Dictionary<string, object>? parameters = null);

        /// <summary>
        /// Convert the tool to function call parameter format for LLM.
        /// </summary>
        /// <returns>Dictionary representing the tool in function call format</returns>
        Dictionary<string, object> ToParameter();

        /// <summary>
        /// Cleanup any resources used by the tool.
        /// </summary>
        /// <returns>Task representing the cleanup operation</returns>
        Task CleanupAsync();
    }
}
