using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenManus.Core.Models
{
    /// <summary>
    /// Base interface for all tools
    /// </summary>
    public interface IBaseTool
    {
        /// <summary>
        /// Unique name of the tool
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of what the tool does
        /// </summary>
        string Description { get; }

        /// <summary>
        /// JSON schema for tool parameters
        /// </summary>
        Dictionary<string, object>? Parameters { get; }

        /// <summary>
        /// Execute the tool with given parameters
        /// </summary>
        /// <param name="parameters">Tool parameters</param>
        /// <returns>Tool execution result</returns>
        Task<ToolResult> ExecuteAsync(Dictionary<string, object>? parameters = null);

        /// <summary>
        /// Converts the tool to a parameter format for LLM function calling.
        /// </summary>
        /// <returns>A dictionary representing the tool's parameters</returns>
        Dictionary<string, object> ToParameter();

        /// <summary>
        /// Cleans up any resources used by the tool.
        /// </summary>
        /// <returns>A task representing the cleanup operation</returns>
        Task CleanupAsync();
    }
}
