// ISandboxService interface defines the methods that the Sandbox service should implement.
public interface ISandboxService
{
    // Initializes the sandbox environment.
    void Initialize();

    // Executes a command within the sandbox.
    string ExecuteCommand(string command);

    // Cleans up the sandbox environment.
    void Cleanup();
}