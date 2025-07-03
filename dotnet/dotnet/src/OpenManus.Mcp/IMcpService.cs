// IMcpService.cs
using System.Threading.Tasks;

namespace OpenManus.Mcp
{
    public interface IMcpService
    {
        Task<string> ProcessRequestAsync(string request);
        Task<string> GetStatusAsync();
        Task<string> ResetAsync();
    }
}