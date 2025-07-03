using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenManus.Core.Models;

namespace OpenManus.Agent
{
    /// <summary>
    /// Abstract base class for managing agent state and execution.
    /// Provides foundational functionality for state transitions, memory management,
    /// and a step-based execution loop.
    /// </summary>
    public abstract class BaseAgent : IBaseAgent
    {
        protected readonly ILogger<BaseAgent> _logger;
        protected readonly Kernel _kernel;

        /// <summary>
        /// Unique name of the agent
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// Optional description of the agent
        /// </summary>
        public virtual string? Description { get; protected set; }

        /// <summary>
        /// System-level instruction prompt
        /// </summary>
        public virtual string? SystemPrompt { get; set; }

        /// <summary>
        /// Prompt for determining next action
        /// </summary>
        public virtual string? NextStepPrompt { get; set; }

        /// <summary>
        /// Agent's memory store
        /// </summary>
        public Memory Memory { get; protected set; }

        /// <summary>
        /// Current state of the agent
        /// </summary>
        public AgentState State { get; protected set; } = AgentState.Idle;

        /// <summary>
        /// Maximum steps before termination
        /// </summary>
        public int MaxSteps { get; set; } = 10;

        /// <summary>
        /// Current step number in execution
        /// </summary>
        public int CurrentStep { get; protected set; } = 0;

        /// <summary>
        /// Threshold for detecting duplicate responses
        /// </summary>
        public int DuplicateThreshold { get; set; } = 2;

        /// <summary>
        /// Initialize BaseAgent with required dependencies
        /// </summary>
        /// <param name="kernel">Semantic Kernel instance</param>
        /// <param name="logger">Logger instance</param>
        /// <param name="name">Agent name</param>
        /// <param name="description">Agent description</param>
        protected BaseAgent(Kernel kernel, ILogger<BaseAgent> logger, string name, string? description = null)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description;
            Memory = new Memory();
        }

        /// <summary>
        /// Initializes the agent with necessary configurations
        /// </summary>
        public virtual void Initialize()
        {
            _logger.LogInformation("Initializing agent: {AgentName}", Name);
            State = AgentState.Idle;
            CurrentStep = 0;

            // Add system prompt if provided
            if (!string.IsNullOrEmpty(SystemPrompt))
            {
                Memory.AddMessage(Message.CreateSystemMessage(SystemPrompt));
            }
        }

        /// <summary>
        /// Executes a task based on the provided input
        /// </summary>
        /// <param name="input">Input task description</param>
        /// <returns>Task execution result</returns>
        public virtual async Task<string> ExecuteTaskAsync(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentException("Input cannot be null or empty", nameof(input));

            return await RunAsync(input);
        }

        /// <summary>
        /// Execute the agent's main loop asynchronously
        /// </summary>
        /// <param name="request">Optional initial user request</param>
        /// <returns>Execution results summary</returns>
        public virtual async Task<string> RunAsync(string? request = null)
        {
            if (State != AgentState.Idle)
            {
                throw new InvalidOperationException($"Cannot run agent from state: {State}");
            }

            if (!string.IsNullOrEmpty(request))
            {
                UpdateMemory(Role.User, request);
            }

            var results = new List<string>();

            try
            {
                await using var stateContext = CreateStateContext(AgentState.Running);

                while (CurrentStep < MaxSteps && State != AgentState.Finished)
                {
                    CurrentStep++;
                    _logger.LogInformation("Executing step {CurrentStep}/{MaxSteps} for agent {AgentName}",
                        CurrentStep, MaxSteps, Name);

                    var stepResult = await StepAsync();

                    // Check for stuck state
                    if (IsStuck())
                    {
                        HandleStuckState();
                    }

                    results.Add($"Step {CurrentStep}: {stepResult}");
                }

                if (CurrentStep >= MaxSteps)
                {
                    CurrentStep = 0;
                    State = AgentState.Idle;
                    results.Add($"Terminated: Reached max steps ({MaxSteps})");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during agent execution for {AgentName}", Name);
                State = AgentState.Error;
                results.Add($"Error: {ex.Message}");
            }

            return results.Count > 0 ? string.Join("\n", results) : "No steps executed";
        }

        /// <summary>
        /// Execute a single step in the agent's workflow.
        /// Must be implemented by subclasses to define specific behavior.
        /// </summary>
        /// <returns>Result of the step execution</returns>
        public abstract Task<string> StepAsync();

        /// <summary>
        /// Check if the agent is stuck in a loop by detecting duplicate content
        /// </summary>
        /// <returns>True if the agent is stuck</returns>
        public virtual bool IsStuck()
        {
            if (Memory.Count < 2)
                return false;

            var lastMessage = Memory.Messages.LastOrDefault();
            if (lastMessage?.Content == null)
                return false;

            // Count identical content occurrences
            var duplicateCount = Memory.Messages
                .Reverse()
                .Skip(1) // Skip the last message itself
                .Where(m => m.Role == Role.Assistant && m.Content == lastMessage.Content)
                .Count();

            return duplicateCount >= DuplicateThreshold;
        }

        /// <summary>
        /// Handle stuck state by adding a prompt to change strategy
        /// </summary>
        public virtual void HandleStuckState()
        {
            const string stuckPrompt =
                "Observed duplicate responses. Consider new strategies and avoid repeating ineffective paths already attempted.";

            NextStepPrompt = $"{stuckPrompt}\n{NextStepPrompt}";

            _logger.LogWarning("Agent {AgentName} detected stuck state. Added prompt: {StuckPrompt}", Name, stuckPrompt);
        }

        /// <summary>
        /// Adds a message to the agent's memory
        /// </summary>
        /// <param name="role">Message role</param>
        /// <param name="content">Message content</param>
        /// <param name="base64Image">Optional base64 encoded image</param>
        public virtual void UpdateMemory(Role role, string content, string? base64Image = null)
        {
            var message = role switch
            {
                Role.User => Message.CreateUserMessage(content, base64Image),
                Role.System => Message.CreateSystemMessage(content),
                Role.Assistant => Message.CreateAssistantMessage(content, base64Image),
                Role.Tool => throw new ArgumentException("Tool messages require additional parameters. Use UpdateMemory(Message) instead."),
                _ => throw new ArgumentException($"Unsupported message role: {role}")
            };

            Memory.AddMessage(message);
        }

        /// <summary>
        /// Adds a message to the agent's memory
        /// </summary>
        /// <param name="message">Message to add</param>
        public virtual void UpdateMemory(Message message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            Memory.AddMessage(message);
        }

        /// <summary>
        /// Retrieves the status of the agent
        /// </summary>
        /// <returns>Current agent status</returns>
        public virtual string GetStatus()
        {
            return $"Agent: {Name}, State: {State}, Step: {CurrentStep}/{MaxSteps}, Messages: {Memory.Count}";
        }

        /// <summary>
        /// Shuts down the agent gracefully
        /// </summary>
        public virtual void Shutdown()
        {
            _logger.LogInformation("Shutting down agent: {AgentName}", Name);
            State = AgentState.Idle;
            CurrentStep = 0;
            // Note: We don't clear memory to preserve conversation history
        }

        /// <summary>
        /// Create a state context for safe agent state transitions
        /// </summary>
        /// <param name="newState">The state to transition to</param>
        /// <returns>Disposable state context</returns>
        protected virtual StateContext CreateStateContext(AgentState newState)
        {
            return new StateContext(this, newState);
        }

        /// <summary>
        /// Context for managing agent state transitions safely
        /// </summary>
        protected class StateContext : IAsyncDisposable
        {
            private readonly BaseAgent _agent;
            private readonly AgentState _previousState;

            public StateContext(BaseAgent agent, AgentState newState)
            {
                _agent = agent;
                _previousState = agent.State;
                _agent.State = newState;
            }

            public async ValueTask DisposeAsync()
            {
                // Revert to previous state unless an error occurred
                if (_agent.State != AgentState.Error)
                {
                    _agent.State = _previousState;
                }
                await Task.CompletedTask;
            }
        }
    }
}
