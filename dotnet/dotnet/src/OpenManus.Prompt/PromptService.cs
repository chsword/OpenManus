using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace OpenManus.Prompt
{
    public class PromptService : IPromptService
    {
        private readonly ISemanticKernel _semanticKernel;

        public PromptService(ISemanticKernel semanticKernel)
        {
            _semanticKernel = semanticKernel ?? throw new ArgumentNullException(nameof(semanticKernel));
        }

        public async Task<string> GeneratePromptAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or empty.", nameof(input));
            }

            // Use the Semantic Kernel to generate a prompt based on the input
            var prompt = await _semanticKernel.GenerateAsync(input);
            return prompt;
        }
    }
}