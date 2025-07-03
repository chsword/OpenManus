using System;

namespace OpenManus.Core.Models
{
    /// <summary>
    /// Represents a tool choice option for controlling tool usage in agent execution.
    /// </summary>
    public enum ToolChoice
    {
        /// <summary>
        /// Automatically decide whether to use tools based on context.
        /// </summary>
        Auto,

        /// <summary>
        /// Never use tools, only respond with text.
        /// </summary>
        None,

        /// <summary>
        /// Tools are required for the response.
        /// </summary>
        Required
    }
}
