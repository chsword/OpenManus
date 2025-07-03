// IFlowService.cs
using System.Threading.Tasks;

namespace OpenManus.Flow
{
    public interface IFlowService
    {
        Task<string> ExecuteFlowAsync(string flowName, object input);
        Task<object> GetFlowStatusAsync(string flowId);
        Task CancelFlowAsync(string flowId);
    }
}