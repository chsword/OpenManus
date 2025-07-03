using System.Threading.Tasks;

namespace OpenManus.Agent
{
    /// <summary>
    /// Interface for ReAct (Reasoning and Acting) agents
    /// </summary>
    public interface IReActAgent : IBaseAgent
    {
        /// <summary>
        /// Process current state and decide next action (Think phase)
        /// </summary>
        /// <returns>True if an action should be taken, false otherwise</returns>
        Task<bool> ThinkAsync();

        /// <summary>
        /// Execute decided actions (Act phase)
        /// </summary>
        /// <returns>Result of the action execution</returns>
        Task<string> ActAsync();
    }
}
