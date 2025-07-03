using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace OpenManus.Llm
{
    public class LlmService : ILlmService
    {
        private readonly Kernel _kernel;

        public LlmService(Kernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public async Task<string> GenerateResponseAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));
            }

            try
            {
                var result = await _kernel.InvokePromptAsync(prompt);
                return result.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to generate response: {ex.Message}", ex);
            }
        }

        public async Task<string> SummarizeTextAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }

            var prompt = $"Please summarize the following text:\n\n{text}";
            return await GenerateResponseAsync(prompt);
        }

        public async Task<string> AnalyzeSentimentAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text cannot be null or empty.", nameof(text));
            }

            var prompt = $"Please analyze the sentiment of the following text and respond with either 'Positive', 'Negative', or 'Neutral':\n\n{text}";
            return await GenerateResponseAsync(prompt);
        }
    }
}
