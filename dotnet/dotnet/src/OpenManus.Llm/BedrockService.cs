using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;

namespace OpenManus.Llm
{
    public class BedrockService : ILlmService
    {
        private readonly IKernel _kernel;

        public BedrockService(IKernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public async Task<string> GenerateResponseAsync(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or empty.", nameof(input));
            }

            var request = new AIRequest
            {
                Prompt = input,
                MaxTokens = 150,
                Temperature = 0.7
            };

            var response = await _kernel.InvokeAsync(request);
            return response;
        }
    }
}