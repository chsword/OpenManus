using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenManus.Core.Models;

namespace OpenManus.Tools
{
    /// <summary>
    /// A collection of tools that can be managed and executed together.
    /// </summary>
    public class ToolCollection : IEnumerable<IBaseTool>
    {
        private readonly ILogger<ToolCollection> _logger;
        private readonly List<IBaseTool> _tools;
        private readonly Dictionary<string, IBaseTool> _toolMap;

        /// <summary>
        /// Initialize ToolCollection with logger
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="tools">Initial tools to add to the collection</param>
        public ToolCollection(ILogger<ToolCollection> logger, params IBaseTool[] tools)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tools = new List<IBaseTool>();
            _toolMap = new Dictionary<string, IBaseTool>();

            foreach (var tool in tools)
            {
                AddTool(tool);
            }
        }

        /// <summary>
        /// Gets the number of tools in the collection.
        /// </summary>
        public int Count => _tools.Count;

        /// <summary>
        /// Gets all tool names in the collection.
        /// </summary>
        public IEnumerable<string> ToolNames => _toolMap.Keys;

        /// <summary>
        /// Adds a tool to the collection.
        /// </summary>
        /// <param name="tool">The tool to add</param>
        /// <returns>This ToolCollection instance for method chaining</returns>
        public ToolCollection AddTool(IBaseTool tool)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));

            if (_toolMap.ContainsKey(tool.Name))
            {
                _logger.LogWarning("Tool {ToolName} already exists in collection, skipping", tool.Name);
                return this;
            }

            _tools.Add(tool);
            _toolMap[tool.Name] = tool;
            _logger.LogDebug("Added tool {ToolName} to collection", tool.Name);

            return this;
        }

        /// <summary>
        /// Adds multiple tools to the collection.
        /// </summary>
        /// <param name="tools">The tools to add</param>
        /// <returns>This ToolCollection instance for method chaining</returns>
        public ToolCollection AddTools(params IBaseTool[] tools)
        {
            foreach (var tool in tools)
            {
                AddTool(tool);
            }
            return this;
        }

        /// <summary>
        /// Gets a tool by name.
        /// </summary>
        /// <param name="name">The name of the tool</param>
        /// <returns>The tool if found, null otherwise</returns>
        public IBaseTool? GetTool(string name)
        {
            return _toolMap.TryGetValue(name, out var tool) ? tool : null;
        }

        /// <summary>
        /// Checks if a tool with the given name exists in the collection.
        /// </summary>
        /// <param name="name">The name of the tool</param>
        /// <returns>True if the tool exists, false otherwise</returns>
        public bool HasTool(string name)
        {
            return _toolMap.ContainsKey(name);
        }

        /// <summary>
        /// Removes a tool from the collection.
        /// </summary>
        /// <param name="name">The name of the tool to remove</param>
        /// <returns>True if the tool was removed, false if it didn't exist</returns>
        public bool RemoveTool(string name)
        {
            if (!_toolMap.TryGetValue(name, out var tool))
                return false;

            _tools.Remove(tool);
            _toolMap.Remove(name);
            _logger.LogDebug("Removed tool {ToolName} from collection", name);

            return true;
        }

        /// <summary>
        /// Executes a tool by name with the given parameters.
        /// </summary>
        /// <param name="name">The name of the tool to execute</param>
        /// <param name="parameters">Parameters for tool execution</param>
        /// <returns>The result of the tool execution</returns>
        public async Task<ToolResult> ExecuteAsync(string name, Dictionary<string, object>? parameters = null)
        {
            var tool = GetTool(name);
            if (tool == null)
            {
                var error = $"Tool '{name}' is not available";
                _logger.LogError(error);
                return new ToolFailure(error);
            }

            try
            {
                _logger.LogDebug("Executing tool {ToolName} from collection", name);
                return await tool.ExecuteAsync(parameters);
            }
            catch (Exception ex)
            {
                var error = $"Error executing tool '{name}': {ex.Message}";
                _logger.LogError(ex, error);
                return new ToolFailure(error);
            }
        }

        /// <summary>
        /// Executes all tools in the collection sequentially.
        /// </summary>
        /// <returns>List of results from all tool executions</returns>
        public async Task<List<ToolResult>> ExecuteAllAsync()
        {
            var results = new List<ToolResult>();

            foreach (var tool in _tools)
            {
                try
                {
                    _logger.LogDebug("Executing tool {ToolName} in batch execution", tool.Name);
                    var result = await tool.ExecuteAsync();
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    var error = $"Error executing tool '{tool.Name}': {ex.Message}";
                    _logger.LogError(ex, error);
                    results.Add(new ToolFailure(error));
                }
            }

            return results;
        }

        /// <summary>
        /// Converts all tools to parameter format for LLM function calling.
        /// </summary>
        /// <returns>List of tool parameters</returns>
        public List<Dictionary<string, object>> ToParameters()
        {
            return _tools.Select(tool => tool.ToParameter()).ToList();
        }

        /// <summary>
        /// Cleans up all tools in the collection.
        /// </summary>
        /// <returns>Task representing the cleanup operation</returns>
        public async Task CleanupAllAsync()
        {
            _logger.LogInformation("Cleaning up all tools in collection");

            var tasks = _tools.Select(async tool =>
            {
                try
                {
                    await tool.CleanupAsync();
                    _logger.LogDebug("Cleaned up tool {ToolName}", tool.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up tool {ToolName}", tool.Name);
                }
            });

            await Task.WhenAll(tasks);
            _logger.LogInformation("Cleanup complete for all tools in collection");
        }

        /// <summary>
        /// Gets an enumerator for the tools in the collection.
        /// </summary>
        /// <returns>Tool enumerator</returns>
        public IEnumerator<IBaseTool> GetEnumerator()
        {
            return _tools.GetEnumerator();
        }

        /// <summary>
        /// Gets an enumerator for the tools in the collection.
        /// </summary>
        /// <returns>Tool enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
