using System.ComponentModel.DataAnnotations;

namespace OpenManus.Core.Models
{
    /// <summary>
    /// Represents a function call in a message
    /// </summary>
    public class Function
    {
        /// <summary>
        /// Name of the function
        /// </summary>
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// Arguments for the function (JSON string)
        /// </summary>
        [Required]
        public required string Arguments { get; set; }
    }

    /// <summary>
    /// Represents a tool/function call in a message
    /// </summary>
    public class ToolCall
    {
        /// <summary>
        /// Unique identifier for the tool call
        /// </summary>
        [Required]
        public required string Id { get; set; }

        /// <summary>
        /// Type of the tool call (usually "function")
        /// </summary>
        public string Type { get; set; } = "function";

        /// <summary>
        /// Function call details
        /// </summary>
        [Required]
        public required Function Function { get; set; }
    }
}
