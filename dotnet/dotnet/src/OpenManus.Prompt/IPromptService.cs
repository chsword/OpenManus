// IPromptService interface defines the contract for prompt services in the OpenManus application.
namespace OpenManus.Prompt
{
    public interface IPromptService
    {
        // Generates a prompt based on the provided input.
        string GeneratePrompt(string input);

        // Validates the given prompt.
        bool ValidatePrompt(string prompt);

        // Retrieves a list of available prompts.
        IEnumerable<string> GetAvailablePrompts();
    }
}