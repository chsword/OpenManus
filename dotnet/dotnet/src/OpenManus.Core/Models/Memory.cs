using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenManus.Core.Models
{
    /// <summary>
    /// Memory store for managing conversation messages
    /// </summary>
    public class Memory
    {
        private readonly List<Message> _messages;

        /// <summary>
        /// Maximum number of messages to keep in memory
        /// </summary>
        public int MaxMessages { get; set; } = 100;

        /// <summary>
        /// List of messages in memory (read-only)
        /// </summary>
        public IReadOnlyList<Message> Messages => _messages.AsReadOnly();

        /// <summary>
        /// Initialize memory with empty message list
        /// </summary>
        public Memory()
        {
            _messages = new List<Message>();
        }

        /// <summary>
        /// Initialize memory with existing messages
        /// </summary>
        /// <param name="messages">Initial messages</param>
        public Memory(IEnumerable<Message> messages)
        {
            _messages = new List<Message>(messages);
            TrimMessages();
        }

        /// <summary>
        /// Add a message to memory
        /// </summary>
        /// <param name="message">Message to add</param>
        public void AddMessage(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            _messages.Add(message);
            TrimMessages();
        }

        /// <summary>
        /// Add multiple messages to memory
        /// </summary>
        /// <param name="messages">Messages to add</param>
        public void AddMessages(IEnumerable<Message> messages)
        {
            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            _messages.AddRange(messages);
            TrimMessages();
        }

        /// <summary>
        /// Clear all messages from memory
        /// </summary>
        public void Clear()
        {
            _messages.Clear();
        }

        /// <summary>
        /// Get the n most recent messages
        /// </summary>
        /// <param name="n">Number of recent messages to retrieve</param>
        /// <returns>List of recent messages</returns>
        public List<Message> GetRecentMessages(int n)
        {
            if (n <= 0)
                return new List<Message>();

            return _messages.TakeLast(Math.Min(n, _messages.Count)).ToList();
        }

        /// <summary>
        /// Get messages by role
        /// </summary>
        /// <param name="role">Role to filter by</param>
        /// <returns>Messages with the specified role</returns>
        public List<Message> GetMessagesByRole(Role role)
        {
            return _messages.Where(m => m.Role == role).ToList();
        }

        /// <summary>
        /// Convert all messages to dictionary list format
        /// </summary>
        /// <returns>List of message dictionaries</returns>
        public List<Dictionary<string, object>> ToDictionaryList()
        {
            return _messages.Select(m => m.ToDictionary()).ToList();
        }

        /// <summary>
        /// Get total message count
        /// </summary>
        public int Count => _messages.Count;

        /// <summary>
        /// Check if memory is empty
        /// </summary>
        public bool IsEmpty => _messages.Count == 0;

        /// <summary>
        /// Trim messages to respect MaxMessages limit
        /// </summary>
        private void TrimMessages()
        {
            if (_messages.Count > MaxMessages)
            {
                var excessCount = _messages.Count - MaxMessages;
                _messages.RemoveRange(0, excessCount);
            }
        }

        /// <summary>
        /// Find messages containing specific content
        /// </summary>
        /// <param name="searchContent">Content to search for</param>
        /// <param name="ignoreCase">Whether to ignore case when searching</param>
        /// <returns>Messages containing the search content</returns>
        public List<Message> FindMessages(string searchContent, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(searchContent))
                return new List<Message>();

            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            return _messages.Where(m =>
                !string.IsNullOrEmpty(m.Content) &&
                m.Content.Contains(searchContent, comparison)
            ).ToList();
        }

        /// <summary>
        /// Get the last message of a specific role
        /// </summary>
        /// <param name="role">Role to search for</param>
        /// <returns>Last message with the specified role, or null if not found</returns>
        public Message? GetLastMessageByRole(Role role)
        {
            return _messages.LastOrDefault(m => m.Role == role);
        }
    }
}
