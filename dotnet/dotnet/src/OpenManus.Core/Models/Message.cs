using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace OpenManus.Core.Models
{
    /// <summary>
    /// Represents a chat message in the conversation
    /// </summary>
    public class Message
    {
        /// <summary>
        /// Role of the message sender
        /// </summary>
        [Required]
        public required Role Role { get; set; }

        /// <summary>
        /// Content of the message
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Tool calls in this message (for assistant messages)
        /// </summary>
        public List<ToolCall>? ToolCalls { get; set; }

        /// <summary>
        /// Name field (used for tool messages)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Tool call ID (used for tool messages)
        /// </summary>
        public string? ToolCallId { get; set; }

        /// <summary>
        /// Base64 encoded image (optional)
        /// </summary>
        public string? Base64Image { get; set; }

        /// <summary>
        /// Timestamp when the message was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Create a user message
        /// </summary>
        /// <param name="content">Message content</param>
        /// <param name="base64Image">Optional base64 encoded image</param>
        /// <returns>User message</returns>
        public static Message CreateUserMessage(string content, string? base64Image = null)
        {
            return new Message
            {
                Role = Role.User,
                Content = content,
                Base64Image = base64Image
            };
        }

        /// <summary>
        /// Create a system message
        /// </summary>
        /// <param name="content">System message content</param>
        /// <returns>System message</returns>
        public static Message CreateSystemMessage(string content)
        {
            return new Message
            {
                Role = Role.System,
                Content = content
            };
        }

        /// <summary>
        /// Create an assistant message
        /// </summary>
        /// <param name="content">Assistant message content</param>
        /// <param name="base64Image">Optional base64 encoded image</param>
        /// <returns>Assistant message</returns>
        public static Message CreateAssistantMessage(string? content = null, string? base64Image = null)
        {
            return new Message
            {
                Role = Role.Assistant,
                Content = content,
                Base64Image = base64Image
            };
        }

        /// <summary>
        /// Create a tool message
        /// </summary>
        /// <param name="content">Tool response content</param>
        /// <param name="name">Tool name</param>
        /// <param name="toolCallId">Tool call ID</param>
        /// <param name="base64Image">Optional base64 encoded image</param>
        /// <returns>Tool message</returns>
        public static Message CreateToolMessage(string content, string name, string toolCallId, string? base64Image = null)
        {
            return new Message
            {
                Role = Role.Tool,
                Content = content,
                Name = name,
                ToolCallId = toolCallId,
                Base64Image = base64Image
            };
        }

        /// <summary>
        /// Create an assistant message with tool calls
        /// </summary>
        /// <param name="toolCalls">List of tool calls</param>
        /// <param name="content">Optional message content</param>
        /// <param name="base64Image">Optional base64 encoded image</param>
        /// <returns>Assistant message with tool calls</returns>
        public static Message CreateToolCallMessage(List<ToolCall> toolCalls, string? content = null, string? base64Image = null)
        {
            return new Message
            {
                Role = Role.Assistant,
                Content = content,
                ToolCalls = toolCalls,
                Base64Image = base64Image
            };
        }

        /// <summary>
        /// Convert message to dictionary format for LLM APIs
        /// </summary>
        /// <returns>Dictionary representation of the message</returns>
        public Dictionary<string, object> ToDictionary()
        {
            var message = new Dictionary<string, object>
            {
                ["role"] = Role.ToString().ToLowerInvariant()
            };

            if (!string.IsNullOrEmpty(Content))
                message["content"] = Content;

            if (ToolCalls != null && ToolCalls.Count > 0)
                message["tool_calls"] = ToolCalls;

            if (!string.IsNullOrEmpty(Name))
                message["name"] = Name;

            if (!string.IsNullOrEmpty(ToolCallId))
                message["tool_call_id"] = ToolCallId;

            if (!string.IsNullOrEmpty(Base64Image))
                message["base64_image"] = Base64Image;

            return message;
        }
    }
}
