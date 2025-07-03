// IAgent interface defines the methods that the agent service should implement.
namespace OpenManus.Agent
{
    public interface IAgent
    {
        // Initializes the agent with necessary configurations.
        void Initialize();

        // Executes a task based on the provided input.
        Task<string> ExecuteTaskAsync(string input);

        // Retrieves the status of the agent.
        string GetStatus();

        // Shuts down the agent gracefully.
        void Shutdown();
    }
}