using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace OpenManus.Agent
{
    /// <summary>
    /// Abstract base class for ReAct (Reasoning and Acting) agents.
    /// Implements the Think-Act cycle for agent execution.
    /// </summary>
    public abstract class ReActAgent : BaseAgent, IReActAgent
    {
        /// <summary>
        /// Initialize ReActAgent with required dependencies
        /// </summary>
        /// <param name="kernel">Semantic Kernel instance</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="name">Agent name</param>
        /// <param name="description">Agent description</param>
        protected ReActAgent(Kernel kernel, ILogger<BaseAgent> logger, string name, string? description = null)
            : base(kernel, logger, name, description)
        {
        }

        /// <summary>
        /// Execute a single step: think and act
        /// </summary>
        /// <returns>Result of the step execution</returns>
        public override async Task<string> StepAsync()
        {
            _logger.LogDebug("Starting ReAct step for agent {AgentName}", Name);

            var shouldAct = await ThinkAsync();
            if (!shouldAct)
            {
                _logger.LogDebug("Agent {AgentName} thinking complete - no action needed", Name);
                return "Thinking complete - no action needed";
            }

            _logger.LogDebug("Agent {AgentName} proceeding to act", Name);
            return await ActAsync();
        }

        /// <summary>
        /// Process current state and decide next action (Think phase).
        /// Must be implemented by subclasses to define specific thinking logic.
        /// </summary>
        /// <returns>True if an action should be taken, false otherwise</returns>
        public abstract Task<bool> ThinkAsync();

        /// <summary>
        /// Execute decided actions (Act phase).
        /// Must be implemented by subclasses to define specific action logic.
        /// </summary>
        /// <returns>Result of the action execution</returns>
        public abstract Task<string> ActAsync();
    }
}
