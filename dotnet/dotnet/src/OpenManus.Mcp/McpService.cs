using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace OpenManus.Mcp
{
    public class McpService : IMcpService
    {
        private readonly ISemanticKernel _semanticKernel;

        public McpService(ISemanticKernel semanticKernel)
        {
            _semanticKernel = semanticKernel ?? throw new ArgumentNullException(nameof(semanticKernel));
        }

        public async Task<string> ExecuteTaskAsync(string taskName, object parameters)
        {
            if (string.IsNullOrWhiteSpace(taskName))
            {
                throw new ArgumentException("Task name cannot be null or empty.", nameof(taskName));
            }

            // Use the Semantic Kernel to execute the task
            var result = await _semanticKernel.ExecuteAsync(taskName, parameters);
            return result;
        }

        // Additional MCP functionality can be implemented here
    }
}