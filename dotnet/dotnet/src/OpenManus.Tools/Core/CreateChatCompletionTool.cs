using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenManus.Core.Models;

namespace OpenManus.Tools.Core
{
    /// <summary>
    /// Tool that creates a chat completion using the configured LLM.
    /// This allows agents to generate responses or ask questions.
    /// </summary>
    public class CreateChatCompletionTool : BaseTool
    {
        private readonly Kernel _kernel;

        public CreateChatCompletionTool(Kernel kernel, ILogger<BaseTool> logger) : base(logger)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

            Parameters = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["messages"] = new Dictionary<string, object>
                    {
                        ["type"] = "array",
                        ["description"] = "List of messages for the chat completion",
                        ["items"] = new Dictionary<string, object>
                        {
                            ["type"] = "object",
                            ["properties"] = new Dictionary<string, object>
                            {
                                ["role"] = new Dictionary<string, object>
                                {
                                    ["type"] = "string",
                                    ["enum"] = new[] { "user", "assistant", "system" }
                                },
                                ["content"] = new Dictionary<string, object>
                                {
                                    ["type"] = "string",
                                    ["description"] = "Message content"
                                }
                            },
                            ["required"] = new[] { "role", "content" }
                        }
                    },
                    ["prompt"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Simple prompt text (alternative to messages)"
                    }
                }
            };
        }

        /// <inheritdoc />
        public override string Name => "create_chat_completion";

        /// <inheritdoc />
        public override string Description => "Creates a chat completion using the configured LLM model.";

        /// <inheritdoc />
        protected override async Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters)
        {
            try
            {
                // Check if we have a simple prompt
                var prompt = GetParameter(parameters, "prompt", string.Empty);

                if (!string.IsNullOrEmpty(prompt))
                {
                    _logger.LogDebug("Creating chat completion with prompt: {Prompt}", prompt);

                    var result = await _kernel.InvokePromptAsync(prompt);
                    var response = result.ToString();

                    _logger.LogDebug("Chat completion result: {Response}", response);

                    return ToolResult.Success(response);
                }

                // Handle messages format (more complex scenario)
                // For now, we'll use a simple implementation
                // In a full implementation, this would parse the messages array
                // and construct appropriate chat history

                _logger.LogWarning("Messages format not yet fully implemented, using basic prompt mode");
                return ToolResult.Failure("Messages format not yet implemented. Please use 'prompt' parameter.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chat completion");
                return ToolResult.Failure($"Failed to create chat completion: {ex.Message}");
            }
        }
    }
}
