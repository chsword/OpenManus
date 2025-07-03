using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenManus.Core.Models;

namespace OpenManus.Tools.Core
{
    /// <summary>
    /// Tool that terminates the agent execution.
    /// This is typically used when the agent has completed its task.
    /// </summary>
    public class TerminateTool : BaseTool
    {
        public TerminateTool(ILogger<BaseTool> logger) : base(logger)
        {
            Parameters = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["reason"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Reason for termination"
                    }
                },
                ["required"] = new[] { "reason" }
            };
        }

        /// <inheritdoc />
        public override string Name => "terminate";

        /// <inheritdoc />
        public override string Description => "Terminates the agent execution when the task is complete.";

        /// <inheritdoc />
        protected override Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters)
        {
            var reason = GetParameter(parameters, "reason", "Task completed successfully");
            
            _logger.LogInformation("Agent execution terminated. Reason: {Reason}", reason);
            
            return Task.FromResult(ToolResult.Success($"Execution terminated: {reason}"));
        }
    }
}
