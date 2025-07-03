using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Orchestration;

namespace OpenManus.Agent
{
    public class AgentService : IAgent
    {
        private readonly ISemanticKernel _kernel;

        public AgentService(ISemanticKernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public async Task<string> ExecuteTaskAsync(string taskDescription)
        {
            if (string.IsNullOrWhiteSpace(taskDescription))
            {
                throw new ArgumentException("Task description cannot be null or empty.", nameof(taskDescription));
            }

            var context = new ContextVariables();
            context.Set("taskDescription", taskDescription);

            try
            {
                var result = await _kernel.RunAsync("TaskExecutor", context);
                return result.ToString();
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                throw new OpenManusException("An error occurred while executing the task.", ex);
            }
        }
    }
}