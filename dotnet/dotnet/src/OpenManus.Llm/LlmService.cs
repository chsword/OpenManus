using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace OpenManus.Llm
{
    public class LlmService : ILlmService
    {
        private readonly ISemanticKernel _semanticKernel;

        public LlmService(ISemanticKernel semanticKernel)
        {
            _semanticKernel = semanticKernel ?? throw new ArgumentNullException(nameof(semanticKernel));
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));
            }

            var result = await _semanticKernel.InvokeAsync(prompt);
            return result;
        }
    }
}