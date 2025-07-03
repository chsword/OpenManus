using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenManus.Core.Models;

namespace OpenManus.Tools
{
    /// <summary>
    /// Abstract base class for all tools in the OpenManus system.
    /// Provides common functionality and structure for tool implementations.
    /// </summary>
    public abstract class BaseTool : IBaseTool
    {
        protected readonly ILogger<BaseTool> _logger;

        /// <summary>
        /// Initialize BaseTool with logger
        /// </summary>
        /// <param name="logger">Logger instance</param>
        protected BaseTool(ILogger<BaseTool> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public virtual Dictionary<string, object>? Parameters { get; protected set; }

        /// <summary>
        /// Execute the tool with the given parameters.
        /// This is the main method that subclasses must implement.
        /// </summary>
        /// <param name="parameters">Parameters for tool execution</param>
        /// <returns>The result of the tool execution</returns>
        public async Task<ToolResult> ExecuteAsync(Dictionary<string, object>? parameters = null)
        {
            try
            {
                _logger.LogDebug("Executing tool {ToolName} with parameters: {@Parameters}", Name, parameters);
                
                var result = await ExecuteCoreAsync(parameters);
                
                _logger.LogDebug("Tool {ToolName} executed successfully", Name);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool {ToolName}", Name);
                return ToolResult.Failure($"Tool '{Name}' encountered an error: {ex.Message}");
            }
        }

        /// <summary>
        /// Core execution logic that must be implemented by subclasses.
        /// </summary>
        /// <param name="parameters">Parameters for tool execution</param>
        /// <returns>The result of the tool execution</returns>
        protected abstract Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters);

        /// <inheritdoc />
        public virtual Dictionary<string, object> ToParameter()
        {
            return new Dictionary<string, object>
            {
                ["type"] = "function",
                ["function"] = new Dictionary<string, object>
                {
                    ["name"] = Name,
                    ["description"] = Description,
                    ["parameters"] = Parameters ?? new Dictionary<string, object>()
                }
            };
        }

        /// <inheritdoc />
        public virtual Task CleanupAsync()
        {
            _logger.LogDebug("Cleaning up tool {ToolName}", Name);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Helper method to get a parameter value with type checking.
        /// </summary>
        /// <typeparam name="T">Expected parameter type</typeparam>
        /// <param name="parameters">Parameters dictionary</param>
        /// <param name="key">Parameter key</param>
        /// <param name="defaultValue">Default value if parameter not found</param>
        /// <returns>Parameter value or default</returns>
        protected T GetParameter<T>(Dictionary<string, object>? parameters, string key, T defaultValue = default(T))
        {
            if (parameters == null || !parameters.TryGetValue(key, out var value))
                return defaultValue;

            try
            {
                if (value is T directValue)
                    return directValue;

                // Try to convert the value
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to convert parameter {Key} to type {Type}, using default value", key, typeof(T).Name);
                return defaultValue;
            }
        }

        /// <summary>
        /// Helper method to validate required parameters.
        /// </summary>
        /// <param name="parameters">Parameters dictionary</param>
        /// <param name="requiredKeys">Required parameter keys</param>
        /// <exception cref="ArgumentException">Thrown when required parameters are missing</exception>
        protected void ValidateRequiredParameters(Dictionary<string, object>? parameters, params string[] requiredKeys)
        {
            if (parameters == null && requiredKeys.Length > 0)
                throw new ArgumentException($"Required parameters missing: {string.Join(", ", requiredKeys)}");

            foreach (var key in requiredKeys)
            {
                if (!parameters!.ContainsKey(key))
                    throw new ArgumentException($"Required parameter '{key}' is missing");
            }
        }
    }
}
