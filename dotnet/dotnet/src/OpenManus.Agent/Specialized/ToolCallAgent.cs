using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenManus.Agent;
using OpenManus.Core.Models;
using OpenManus.Tools;
using OpenManus.Tools.Core;

namespace OpenManus.Agent.Specialized
{
    /// <summary>
    /// Agent specialized in executing tool calls with enhanced abstraction.
    /// This agent can handle complex tool interactions and manage tool execution lifecycle.
    /// </summary>
    public class ToolCallAgent : ReActAgent
    {
        private const string DefaultSystemPrompt = @"You are a helpful assistant that can use tools to accomplish tasks.
When you need to use tools, think carefully about which tools are appropriate and how to use them effectively.
Always provide clear explanations of your actions and reasoning.";

        private const string DefaultNextStepPrompt = "Based on the current conversation and available tools, what should be the next step?";

        private readonly IChatCompletionService _chatService;
        private readonly ToolCollection _toolCollection;
        private readonly List<string> _specialToolNames;

        public override string? SystemPrompt { get; set; } = DefaultSystemPrompt;
        public override string? NextStepPrompt { get; set; } = DefaultNextStepPrompt;
        public ToolChoice ToolChoiceMode { get; set; } = ToolChoice.Auto;
        public new int MaxSteps { get; set; } = 30;
        public int? MaxObserve { get; set; } = null;

        /// <summary>
        /// Current tool calls from the last thinking phase
        /// </summary>
        public List<ToolCall> CurrentToolCalls { get; private set; } = new();

        /// <summary>
        /// Initialize ToolCallAgent with default tools
        /// </summary>
        public ToolCallAgent(
            Kernel kernel,
            ILogger<BaseAgent> logger,
            ToolCollection toolCollection,
            string? name = null,
            string? description = null)
            : base(kernel, logger, name ?? "toolcall", description ?? "An agent that can execute tool calls.")
        {
            _chatService = kernel.GetRequiredService<IChatCompletionService>();
            _toolCollection = toolCollection ?? throw new ArgumentNullException(nameof(toolCollection));
            _specialToolNames = new List<string> { "terminate" };

            // Add default tools if not already present
            if (!_toolCollection.HasTool("terminate"))
            {
                _toolCollection.AddTool(new TerminateTool(logger as ILogger<BaseTool> ??
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<BaseTool>.Instance));
            }

            if (!_toolCollection.HasTool("create_chat_completion"))
            {
                _toolCollection.AddTool(new CreateChatCompletionTool(kernel, logger as ILogger<BaseTool> ??
                    Microsoft.Extensions.Logging.Abstractions.NullLogger<BaseTool>.Instance));
            }
        }

        /// <inheritdoc />
        public override async Task<bool> ThinkAsync()
        {
            try
            {
                _logger.LogDebug("ToolCallAgent {AgentName} starting to think", Name);

                // Add next step prompt if configured
                if (!string.IsNullOrEmpty(NextStepPrompt))
                {
                    var userMessage = new Message
                    {
                        Role = Role.User,
                        Content = NextStepPrompt,
                        CreatedAt = DateTime.UtcNow
                    };
                    Memory.AddMessage(userMessage);
                }

                // Prepare chat history
                var chatHistory = new ChatHistory();

                // Add system message
                if (!string.IsNullOrEmpty(SystemPrompt))
                {
                    chatHistory.AddSystemMessage(SystemPrompt);
                }

                // Add conversation history
                foreach (var message in Memory.Messages)
                {
                    switch (message.Role)
                    {
                        case Role.User:
                            chatHistory.AddUserMessage(message.Content ?? string.Empty);
                            break;
                        case Role.Assistant:
                            chatHistory.AddAssistantMessage(message.Content ?? string.Empty);
                            break;
                        case Role.System:
                            chatHistory.AddSystemMessage(message.Content ?? string.Empty);
                            break;
                        case Role.Tool:
                            // For tool messages, we'll add them as user messages with special formatting
                            chatHistory.AddUserMessage($"Tool result: {message.Content}");
                            break;
                    }
                }

                // Get available tools for function calling
                var tools = _toolCollection.ToParameters();
                var kernelFunctions = tools.Select(tool =>
                {
                    var functionData = tool["function"] as Dictionary<string, object>;
                    return KernelFunctionFactory.CreateFromPrompt(
                        functionData?["description"]?.ToString() ?? "No description",
                        functionName: functionData?["name"]?.ToString() ?? "unknown_tool");
                }).ToArray();

                // Execute chat completion
                PromptExecutionSettings? executionSettings = null;
                if (ToolChoiceMode != ToolChoice.None && tools.Any())
                {
                    executionSettings = new PromptExecutionSettings
                    {
                        // Note: Tool calling configuration would depend on the specific LLM provider
                        // This is a simplified version
                    };
                }

                var response = await _chatService.GetChatMessageContentAsync(chatHistory, executionSettings, _kernel);

                // Parse response for tool calls
                CurrentToolCalls.Clear();
                var content = response.Content ?? string.Empty;

                _logger.LogInformation("✨ {AgentName}'s thoughts: {Content}", Name, content);

                // For now, we'll implement a simple tool call detection
                // In a full implementation, this would parse actual function calls from the LLM response
                var shouldAct = await ShouldExecuteToolsAsync(content);

                // Add assistant message to memory
                var assistantMessage = new Message
                {
                    Role = Role.Assistant,
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    ToolCalls = CurrentToolCalls.Any() ? CurrentToolCalls : null
                };
                Memory.AddMessage(assistantMessage);

                _logger.LogInformation("🛠️ {AgentName} selected {ToolCount} tools to use", Name, CurrentToolCalls.Count);

                return shouldAct;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ToolCallAgent thinking phase");

                var errorMessage = new Message
                {
                    Role = Role.Assistant,
                    Content = $"Error encountered while processing: {ex.Message}",
                    CreatedAt = DateTime.UtcNow
                };
                Memory.AddMessage(errorMessage);

                return false;
            }
        }

        /// <inheritdoc />
        public override async Task<string> ActAsync()
        {
            try
            {
                if (!CurrentToolCalls.Any())
                {
                    if (ToolChoiceMode == ToolChoice.Required)
                    {
                        throw new InvalidOperationException("Tool calls required but none provided");
                    }

                    // Return last message content if no tool calls
                    var lastMessage = Memory.Messages.LastOrDefault();
                    return lastMessage?.Content ?? "No content or commands to execute";
                }

                var results = new List<string>();

                foreach (var toolCall in CurrentToolCalls)
                {
                    _logger.LogInformation("🔧 Activating tool: '{ToolName}'...", toolCall.Function?.Name);

                    var result = await ExecuteToolCallAsync(toolCall);

                    if (MaxObserve.HasValue && result.Length > MaxObserve.Value)
                    {
                        result = result.Substring(0, MaxObserve.Value);
                    }

                    _logger.LogInformation("🎯 Tool '{ToolName}' completed its mission! Result: {Result}",
                        toolCall.Function?.Name, result);

                    // Add tool result to memory
                    var toolMessage = new Message
                    {
                        Role = Role.Tool,
                        Content = result,
                        CreatedAt = DateTime.UtcNow,
                        ToolCallId = toolCall.Id,
                        Name = toolCall.Function?.Name
                    };
                    Memory.AddMessage(toolMessage);

                    results.Add(result);

                    // Handle special tools
                    await HandleSpecialToolAsync(toolCall.Function?.Name ?? string.Empty, result);
                }

                return string.Join("\n\n", results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ToolCallAgent action phase");
                return $"Error executing tools: {ex.Message}";
            }
        }

        /// <summary>
        /// Executes a single tool call
        /// </summary>
        private async Task<string> ExecuteToolCallAsync(ToolCall toolCall)
        {
            if (toolCall?.Function == null || string.IsNullOrEmpty(toolCall.Function.Name))
            {
                return "Error: Invalid tool call format";
            }

            var toolName = toolCall.Function.Name;
            if (!_toolCollection.HasTool(toolName))
            {
                return $"Error: Unknown tool '{toolName}'";
            }

            try
            {
                // Parse arguments
                var args = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(toolCall.Function.Arguments))
                {
                    var jsonElement = JsonSerializer.Deserialize<JsonElement>(toolCall.Function.Arguments);
                    args = JsonElementToDictionary(jsonElement);
                }

                // Execute the tool
                var toolResult = await _toolCollection.ExecuteAsync(toolName, args);

                // Format result for display
                var observation = toolResult.HasContent
                    ? $"Observed output of cmd `{toolName}` executed:\n{toolResult}"
                    : $"Cmd `{toolName}` completed with no output";

                return observation;
            }
            catch (JsonException ex)
            {
                var error = $"Error parsing arguments for {toolName}: Invalid JSON format";
                _logger.LogError(ex, "📝 Invalid JSON arguments for '{ToolName}', arguments: {Arguments}",
                    toolName, toolCall.Function.Arguments);
                return $"Error: {error}";
            }
            catch (Exception ex)
            {
                var error = $"⚠️ Tool '{toolName}' encountered a problem: {ex.Message}";
                _logger.LogError(ex, error);
                return $"Error: {error}";
            }
        }

        /// <summary>
        /// Handles special tool execution and state changes
        /// </summary>
        private async Task HandleSpecialToolAsync(string toolName, string result)
        {
            if (!IsSpecialTool(toolName))
                return;

            if (ShouldFinishExecution(toolName, result))
            {
                _logger.LogInformation("🏁 Special tool '{ToolName}' has completed the task!", toolName);
                State = AgentState.Finished;
            }

            await Task.CompletedTask; // Placeholder for async operations
        }

        /// <summary>
        /// Determines if execution should finish based on tool execution
        /// </summary>
        private static bool ShouldFinishExecution(string toolName, string result)
        {
            return true; // For now, all special tools finish execution
        }

        /// <summary>
        /// Checks if a tool is in the special tools list
        /// </summary>
        private bool IsSpecialTool(string toolName)
        {
            return _specialToolNames.Any(name =>
                string.Equals(name, toolName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines if tools should be executed based on content analysis
        /// </summary>
        private Task<bool> ShouldExecuteToolsAsync(string content)
        {
            // This is a simplified implementation
            // In a real implementation, this would parse function calls from the LLM response

            // For now, we'll create mock tool calls based on content analysis
            if (content.ToLowerInvariant().Contains("terminate") || content.ToLowerInvariant().Contains("finish"))
            {
                CurrentToolCalls.Add(new ToolCall
                {
                    Id = Guid.NewGuid().ToString(),
                    Function = new Function
                    {
                        Name = "terminate",
                        Arguments = JsonSerializer.Serialize(new { reason = "Task completed" })
                    }
                });
                return Task.FromResult(true);
            }

            return Task.FromResult(CurrentToolCalls.Any());
        }

        /// <summary>
        /// Converts JsonElement to Dictionary for tool parameters
        /// </summary>
        private static Dictionary<string, object> JsonElementToDictionary(JsonElement element)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var property in element.EnumerateObject())
            {
                dictionary[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                    JsonValueKind.Number => property.Value.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => (object?)null,
                    JsonValueKind.Object => JsonElementToDictionary(property.Value),
                    JsonValueKind.Array => property.Value.EnumerateArray()
                        .Select(item => JsonElementToDictionary(item)).ToArray(),
                    _ => property.Value.ToString() ?? string.Empty
                } ?? string.Empty;
            }

            return dictionary;
        }

        /// <summary>
        /// Cleanup resources when agent is disposed
        /// </summary>
        public async Task CleanupAsync()
        {
            _logger.LogInformation("🧹 Cleaning up resources for ToolCallAgent '{AgentName}'...", Name);

            try
            {
                await _toolCollection.CleanupAllAsync();
                _logger.LogInformation("✨ Cleanup complete for ToolCallAgent '{AgentName}'.", Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 Error during ToolCallAgent cleanup");
            }
        }

        /// <summary>
        /// Run the agent with automatic cleanup
        /// </summary>
        public override async Task<string> RunAsync(string? request = null)
        {
            try
            {
                return await base.RunAsync(request);
            }
            finally
            {
                await CleanupAsync();
            }
        }
    }
}
