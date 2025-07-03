using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenManus.Core.Models;

namespace OpenManus.Tools.Core
{
    /// <summary>
    /// Tool that allows agents to ask questions to humans and wait for responses.
    /// This enables human-in-the-loop interactions during agent execution.
    /// </summary>
    public class AskHumanTool : BaseTool
    {
        public AskHumanTool(ILogger<BaseTool> logger) : base(logger)
        {
            Parameters = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["question"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "The question to ask the human user"
                    }
                },
                ["required"] = new[] { "question" }
            };
        }

        /// <inheritdoc />
        public override string Name => "ask_human";

        /// <inheritdoc />
        public override string Description => "Ask a question to the human user and wait for their response. Use this when you need clarification, confirmation, or additional information from the user.";

        /// <inheritdoc />
        protected override async Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters)
        {
            var question = GetParameter(parameters, "question", string.Empty);

            if (string.IsNullOrWhiteSpace(question))
            {
                return ToolResult.Failure("Question parameter is required and cannot be empty");
            }

            _logger.LogInformation("🤔 Agent is asking human: {Question}", question);

            try
            {
                // Display the question to the user
                Console.WriteLine($"\n🤖 Agent Question: {question}");
                Console.Write("👤 Your Response: ");

                // Wait for user input
                var response = await Task.Run(() => Console.ReadLine());

                if (string.IsNullOrWhiteSpace(response))
                {
                    response = "(No response provided)";
                }

                _logger.LogInformation("👤 Human responded: {Response}", response);

                return ToolResult.Success($"Human response: {response}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting human input");
                return ToolResult.Failure($"Failed to get human input: {ex.Message}");
            }
        }
    }
}
