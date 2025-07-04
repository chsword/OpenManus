using System;

namespace OpenManus.Core.Models
{
    /// <summary>
    /// Represents the result of a tool execution.
    /// </summary>
    public class ToolResult
    {
        /// <summary>
        /// The output of the tool execution.
        /// </summary>
        public object? Output { get; set; }

        /// <summary>
        /// Error message if the tool execution failed.
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Base64 encoded image data if the tool produced image output.
        /// </summary>
        public string? Base64Image { get; set; }

        /// <summary>
        /// System-level information about the tool execution.
        /// </summary>
        public string? System { get; set; }

        /// <summary>
        /// Indicates if the tool result has any meaningful content.
        /// </summary>
        public bool HasContent => Output != null || !string.IsNullOrEmpty(Error) ||
                                 !string.IsNullOrEmpty(Base64Image) || !string.IsNullOrEmpty(System);

        /// <summary>
        /// Creates a successful tool result.
        /// </summary>
        /// <param name="output">The output of the tool execution</param>
        /// <param name="base64Image">Optional base64 image data</param>
        /// <param name="system">Optional system information</param>
        /// <returns>A new ToolResult instance</returns>
        public static ToolResult Success(object? output, string? base64Image = null, string? system = null)
        {
            return new ToolResult
            {
                Output = output,
                Base64Image = base64Image,
                System = system
            };
        }

        /// <summary>
        /// Creates a failed tool result.
        /// </summary>
        /// <param name="error">The error message</param>
        /// <returns>A new ToolResult instance</returns>
        public static ToolResult Failure(string error)
        {
            return new ToolResult
            {
                Error = error
            };
        }

        /// <summary>
        /// Combines two tool results.
        /// </summary>
        /// <param name="other">The other tool result to combine with</param>
        /// <returns>A new combined ToolResult</returns>
        /// <exception cref="InvalidOperationException">When both results have conflicting non-concatenable fields</exception>
        public ToolResult Combine(ToolResult other)
        {
            if (other == null) return this;

            string? CombineFields(string? field1, string? field2, bool concatenate = true)
            {
                if (!string.IsNullOrEmpty(field1) && !string.IsNullOrEmpty(field2))
                {
                    if (concatenate)
                        return field1 + field2;
                    throw new InvalidOperationException("Cannot combine tool results with conflicting non-concatenable fields");
                }
                return field1 ?? field2;
            }

            object? CombineOutput(object? output1, object? output2)
            {
                if (output1 != null && output2 != null)
                {
                    return output1.ToString() + output2.ToString();
                }
                return output1 ?? output2;
            }

            return new ToolResult
            {
                Output = CombineOutput(Output, other.Output),
                Error = CombineFields(Error, other.Error),
                Base64Image = CombineFields(Base64Image, other.Base64Image, false),
                System = CombineFields(System, other.System)
            };
        }

        /// <summary>
        /// Creates a copy of this ToolResult with specified fields replaced.
        /// </summary>
        public ToolResult Replace(object? output = null, string? error = null,
                                 string? base64Image = null, string? system = null)
        {
            return new ToolResult
            {
                Output = output ?? Output,
                Error = error ?? Error,
                Base64Image = base64Image ?? Base64Image,
                System = system ?? System
            };
        }

        /// <summary>
        /// Returns the string representation of the tool result.
        /// </summary>
        public override string ToString()
        {
            return !string.IsNullOrEmpty(Error) ? $"Error: {Error}" : Output?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// A ToolResult that represents a tool execution failure.
    /// </summary>
    public class ToolFailure : ToolResult
    {
        public ToolFailure(string error) : base()
        {
            Error = error;
        }
    }

    /// <summary>
    /// A ToolResult that can be rendered as CLI output.
    /// </summary>
    public class CLIResult : ToolResult
    {
        public CLIResult(object? output = null, string? error = null) : base()
        {
            Output = output;
            Error = error;
        }
    }

    /// <summary>
    /// Extension methods for ToolResult
    /// </summary>
    public static class ToolResultExtensions
    {
        /// <summary>
        /// Indicates if the tool execution was successful (no error)
        /// </summary>
        public static bool IsSuccess(this ToolResult result)
        {
            return string.IsNullOrEmpty(result.Error);
        }

        /// <summary>
        /// Gets the error message if any
        /// </summary>
        public static string? ErrorMessage(this ToolResult result)
        {
            return result.Error;
        }

        /// <summary>
        /// Gets the result output as string
        /// </summary>
        public static string? Result(this ToolResult result)
        {
            return result.Output?.ToString();
        }
    }
}
