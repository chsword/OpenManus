using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SemanticKernel;

namespace OpenManus.Prompt
{
    public class PromptService : IPromptService
    {
        private readonly Kernel _kernel;
        private readonly List<string> _availablePrompts;

        public PromptService(Kernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _availablePrompts = new List<string>
            {
                "Default Prompt",
                "Chat Prompt",
                "Analysis Prompt",
                "Summary Prompt"
            };
        }

        public string GeneratePrompt(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or empty.", nameof(input));
            }

            // Generate a simple prompt based on the input
            return $"Based on the following input, please provide a helpful response: {input}";
        }

        public bool ValidatePrompt(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return false;
            }

            // Basic validation - check if prompt is not too short or too long
            return prompt.Length >= 10 && prompt.Length <= 10000;
        }

        public IEnumerable<string> GetAvailablePrompts()
        {
            return _availablePrompts.AsReadOnly();
        }
    }
}
