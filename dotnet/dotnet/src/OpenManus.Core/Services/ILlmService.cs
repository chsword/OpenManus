using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenManus.Core.Models;

namespace OpenManus.Core.Services
{
    /// <summary>
    /// Interface for Large Language Model services
    /// </summary>
    public interface ILlmService
    {
        /// <summary>
        /// Generate a response from the LLM based on message history
        /// </summary>
        /// <param name="messages">List of conversation messages</param>
        /// <param name="temperature">Sampling temperature (0.0 to 1.0)</param>
        /// <param name="maxTokens">Maximum tokens to generate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated response text</returns>
        Task<string> GenerateResponseAsync(
            IList<Message> messages, 
            double temperature = 0.7, 
            int? maxTokens = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate a response with tool calling capabilities
        /// </summary>
        /// <param name="messages">List of conversation messages</param>
        /// <param name="availableTools">Available tools for the LLM to call</param>
        /// <param name="toolChoice">Tool choice strategy</param>
        /// <param name="temperature">Sampling temperature</param>
        /// <param name="maxTokens">Maximum tokens to generate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated response with potential tool calls</returns>
        Task<LlmResponse> GenerateResponseWithToolsAsync(
            IList<Message> messages,
            IList<ToolDefinition>? availableTools = null,
            ToolChoice toolChoice = ToolChoice.Auto,
            double temperature = 0.7,
            int? maxTokens = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the model name or identifier
        /// </summary>
        string ModelName { get; }

        /// <summary>
        /// Check if the service is configured and ready
        /// </summary>
        bool IsConfigured { get; }
    }

    /// <summary>
    /// Response from LLM with potential tool calls
    /// </summary>
    public class LlmResponse
    {
        /// <summary>
        /// Generated text content
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Tool calls requested by the LLM
        /// </summary>
        public IList<ToolCall>? ToolCalls { get; set; }

        /// <summary>
        /// Finish reason (completed, length, tool_calls, etc.)
        /// </summary>
        public string? FinishReason { get; set; }

        /// <summary>
        /// Token usage information
        /// </summary>
        public TokenUsage? Usage { get; set; }
    }

    /// <summary>
    /// Token usage information
    /// </summary>
    public class TokenUsage
    {
        /// <summary>
        /// Number of tokens in the prompt
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Number of tokens in the completion
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Total tokens used
        /// </summary>
        public int TotalTokens => PromptTokens + CompletionTokens;
    }

    /// <summary>
    /// Tool definition for LLM tool calling
    /// </summary>
    public class ToolDefinition
    {
        /// <summary>
        /// Type of tool (usually "function")
        /// </summary>
        public string Type { get; set; } = "function";

        /// <summary>
        /// Function definition
        /// </summary>
        public FunctionDefinition? Function { get; set; }
    }

    /// <summary>
    /// Function definition for tool calling
    /// </summary>
    public class FunctionDefinition
    {
        /// <summary>
        /// Function name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Function description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// JSON schema for function parameters
        /// </summary>
        public object? Parameters { get; set; }
    }
}
