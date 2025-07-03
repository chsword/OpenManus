// ILlmService.cs
using System.Threading.Tasks;

namespace OpenManus.Llm
{
    public interface ILlmService
    {
        Task<string> GenerateResponseAsync(string prompt);
        Task<string> SummarizeTextAsync(string text);
        Task<string> AnalyzeSentimentAsync(string text);
    }
}