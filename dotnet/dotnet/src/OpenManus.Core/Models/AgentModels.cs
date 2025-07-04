using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace OpenManus.Core.Models
{
    /// <summary>
    /// Represents a tool call request from the LLM
    /// </summary>
    public class ToolCall
    {
        /// <summary>
        /// Unique identifier for this tool call
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The type of call (usually "function")
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = "function";

        /// <summary>
        /// Function call details
        /// </summary>
        [JsonPropertyName("function")]
        public Function? Function { get; set; }
    }

    /// <summary>
    /// Represents a function call within a tool call
    /// </summary>
    public class Function
    {
        /// <summary>
        /// Name of the function to call
        /// </summary>
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// JSON string containing the function arguments
        /// </summary>
        [JsonPropertyName("arguments")]
        public string Arguments { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the tool choice mode for LLM interactions
    /// </summary>
    public enum ToolChoice
    {
        /// <summary>
        /// No tools are available
        /// </summary>
        None,

        /// <summary>
        /// LLM can choose whether to use tools
        /// </summary>
        Auto,

        /// <summary>
        /// LLM must use at least one tool
        /// </summary>
        Required
    }

    /// <summary>
    /// Represents the role in a conversation
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// System message
        /// </summary>
        System,

        /// <summary>
        /// User message
        /// </summary>
        User,

        /// <summary>
        /// Assistant/AI message
        /// </summary>
        Assistant,

        /// <summary>
        /// Tool result message
        /// </summary>
        Tool
    }

    /// <summary>
    /// Represents a message in the conversation history
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The role of the message sender
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// The content of the message
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// When the message was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Tool calls associated with this message (for assistant messages)
        /// </summary>
        public List<ToolCall>? ToolCalls { get; set; }

        /// <summary>
        /// Tool call ID this message is responding to (for tool messages)
        /// </summary>
        public string? ToolCallId { get; set; }

        /// <summary>
        /// Name of the tool (for tool messages)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Base64 encoded image data (optional)
        /// </summary>
        public string? Base64Image { get; set; }

        /// <summary>
        /// Create a system message
        /// </summary>
        public static Message CreateSystemMessage(string content)
        {
            return new Message
            {
                Role = Role.System,
                Content = content
            };
        }

        /// <summary>
        /// Create a user message
        /// </summary>
        public static Message CreateUserMessage(string content)
        {
            return new Message
            {
                Role = Role.User,
                Content = content
            };
        }

        /// <summary>
        /// Create a user message with optional base64 image
        /// </summary>
        public static Message CreateUserMessage(string content, string? base64Image)
        {
            return new Message
            {
                Role = Role.User,
                Content = content,
                Base64Image = base64Image
            };
        }

        /// <summary>
        /// Create an assistant message
        /// </summary>
        public static Message CreateAssistantMessage(string content, List<ToolCall>? toolCalls = null)
        {
            return new Message
            {
                Role = Role.Assistant,
                Content = content,
                ToolCalls = toolCalls
            };
        }

        /// <summary>
        /// Create an assistant message with optional base64 image
        /// </summary>
        public static Message CreateAssistantMessage(string content, string? base64Image)
        {
            return new Message
            {
                Role = Role.Assistant,
                Content = content,
                Base64Image = base64Image
            };
        }

        /// <summary>
        /// Create a tool result message
        /// </summary>
        public static Message CreateToolMessage(string toolCallId, string content, string? name = null)
        {
            return new Message
            {
                Role = Role.Tool,
                Content = content,
                ToolCallId = toolCallId,
                Name = name
            };
        }
    }

    /// <summary>
    /// Represents agent execution state
    /// </summary>
    public enum AgentState
    {
        /// <summary>
        /// Agent is idle and ready to receive requests
        /// </summary>
        Idle,

        /// <summary>
        /// Agent is currently running
        /// </summary>
        Running,

        /// <summary>
        /// Agent has finished execution
        /// </summary>
        Finished,

        /// <summary>
        /// Agent encountered an error
        /// </summary>
        Error
    }

    /// <summary>
    /// Memory store for conversation history
    /// </summary>
    public class Memory
    {
        private readonly List<Message> _messages = new();

        /// <summary>
        /// Get the number of messages in memory
        /// </summary>
        public int Count => _messages.Count;

        /// <summary>
        /// All messages in the conversation
        /// </summary>
        public IReadOnlyList<Message> Messages => _messages.AsReadOnly();

        /// <summary>
        /// Add a message to the conversation history
        /// </summary>
        public void AddMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _messages.Add(message);
        }

        /// <summary>
        /// Clear all messages
        /// </summary>
        public void Clear()
        {
            _messages.Clear();
        }

        /// <summary>
        /// Get the last N messages
        /// </summary>
        public IEnumerable<Message> GetLastMessages(int count)
        {
            return _messages.TakeLast(count);
        }
    }
}
