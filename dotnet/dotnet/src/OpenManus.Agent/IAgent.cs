using System.Threading.Tasks;
using OpenManus.Core.Models;

namespace OpenManus.Agent
{
    /// <summary>
    /// Base interface for all agents with basic functionality
    /// </summary>
    public interface IAgent
    {
        /// <summary>
        /// Unique name of the agent
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Optional description of the agent
        /// </summary>
        string? Description { get; }

        /// <summary>
        /// Current state of the agent
        /// </summary>
        AgentState State { get; }

        /// <summary>
        /// Agent's memory store
        /// </summary>
        Memory Memory { get; }

        /// <summary>
        /// Current step number in execution
        /// </summary>
        int CurrentStep { get; }

        /// <summary>
        /// Maximum steps before termination
        /// </summary>
        int MaxSteps { get; set; }

        /// <summary>
        /// Initializes the agent with necessary configurations
        /// </summary>
        void Initialize();

        /// <summary>
        /// Executes a task based on the provided input
        /// </summary>
        /// <param name="input">Input task description</param>
        /// <returns>Task execution result</returns>
        Task<string> ExecuteTaskAsync(string input);

        /// <summary>
        /// Retrieves the status of the agent
        /// </summary>
        /// <returns>Current agent status</returns>
        string GetStatus();

        /// <summary>
        /// Shuts down the agent gracefully
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Adds a message to the agent's memory
        /// </summary>
        /// <param name="role">Message role</param>
        /// <param name="content">Message content</param>
        /// <param name="base64Image">Optional base64 encoded image</param>
        void UpdateMemory(Role role, string content, string? base64Image = null);

        /// <summary>
        /// Adds a message to the agent's memory
        /// </summary>
        /// <param name="message">Message to add</param>
        void UpdateMemory(Message message);

        /// <summary>
        /// Execute the agent's main loop asynchronously
        /// </summary>
        /// <param name="request">Optional initial user request</param>
        /// <returns>Execution results summary</returns>
        Task<string> RunAsync(string? request = null);
    }

    /// <summary>
    /// Extended interface for agents with step-based execution
    /// </summary>
    public interface IBaseAgent : IAgent
    {
        /// <summary>
        /// System-level instruction prompt
        /// </summary>
        string? SystemPrompt { get; set; }

        /// <summary>
        /// Prompt for determining next action
        /// </summary>
        string? NextStepPrompt { get; set; }

        /// <summary>
        /// Threshold for detecting duplicate responses
        /// </summary>
        int DuplicateThreshold { get; set; }

        /// <summary>
        /// Execute a single step in the agent's workflow
        /// </summary>
        /// <returns>Result of the step execution</returns>
        Task<string> StepAsync();

        /// <summary>
        /// Check if the agent is stuck in a loop
        /// </summary>
        /// <returns>True if the agent is stuck</returns>
        bool IsStuck();

        /// <summary>
        /// Handle stuck state by adjusting strategy
        /// </summary>
        void HandleStuckState();
    }
}
