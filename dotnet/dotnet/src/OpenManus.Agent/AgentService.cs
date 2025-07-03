using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using OpenManus.Core.Models;

namespace OpenManus.Agent
{
    public class AgentService : IAgent
    {
        private readonly Kernel _kernel;
        private bool _isInitialized;

        public AgentService(Kernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
            _isInitialized = false;
            Name = "DefaultAgent";
            Description = "Default agent service implementation";
            Memory = new Memory();
            State = AgentState.Idle;
        }

        // IAgent implementation
        public string Name { get; set; }
        public string? Description { get; set; }
        public AgentState State { get; set; }
        public Memory Memory { get; set; }
        public int CurrentStep { get; set; }
        public int MaxSteps { get; set; } = 10;

        public void UpdateMemory(Role role, string content, string? toolCallId = null)
        {
            var message = new Message
            {
                Role = role,
                Content = content,
                ToolCallId = toolCallId,
                CreatedAt = DateTime.UtcNow
            };
            Memory.AddMessage(message);
        }

        public void UpdateMemory(Message message)
        {
            Memory.AddMessage(message);
        }

        public async Task<string> RunAsync(string? request = null)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            State = AgentState.Running;
            CurrentStep = 0;

            try
            {
                var input = request ?? "Hello, how can I help you?";
                var result = await ExecuteTaskAsync(input);

                State = AgentState.Finished;
                return result;
            }
            catch (Exception ex)
            {
                State = AgentState.Idle;
                throw new InvalidOperationException($"Failed to run agent: {ex.Message}", ex);
            }
        }

        // Existing methods
        public void Initialize()
        {
            // Initialize the agent
            _isInitialized = true;
        }

        public async Task<string> ExecuteTaskAsync(string input)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Agent must be initialized before executing tasks.");
            }

            if (string.IsNullOrWhiteSpace(input))
            {
                throw new ArgumentException("Input cannot be null or empty.", nameof(input));
            }

            try
            {
                var result = await _kernel.InvokePromptAsync(input);
                return result.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute task: {ex.Message}", ex);
            }
        }

        public string GetStatus()
        {
            return _isInitialized ? "Agent is running" : "Agent is not initialized";
        }

        public void Shutdown()
        {
            // Graceful shutdown
            _isInitialized = false;
        }
    }
}
