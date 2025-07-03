using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace OpenManus.Mcp
{
    public class McpService : IMcpService
    {
        private readonly Kernel _kernel;

        public McpService(Kernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public async Task<string> ProcessRequestAsync(string request)
        {
            if (string.IsNullOrWhiteSpace(request))
            {
                throw new ArgumentException("Request cannot be null or empty.", nameof(request));
            }

            try
            {
                var result = await _kernel.InvokePromptAsync(request);
                return result.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to process request: {ex.Message}", ex);
            }
        }

        public async Task<string> GetStatusAsync()
        {
            // Placeholder implementation
            await Task.CompletedTask;
            return "MCP Service is running";
        }

        public async Task<string> ResetAsync()
        {
            // Placeholder implementation
            await Task.CompletedTask;
            return "MCP Service has been reset";
        }
    }
}
