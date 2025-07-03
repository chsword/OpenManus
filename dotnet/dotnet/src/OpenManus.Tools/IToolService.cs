// IToolService.cs
using System.Threading.Tasks;

namespace OpenManus.Tools
{
    public interface IToolService
    {
        Task<string> ExecuteToolAsync(string toolName, object parameters);
        Task<string> GetToolInfoAsync(string toolName);
        Task<string[]> ListAvailableToolsAsync();
    }
}