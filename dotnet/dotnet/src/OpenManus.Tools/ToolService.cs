using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace OpenManus.Tools
{
    public class ToolService : IToolService
    {
        private readonly ISemanticKernel _semanticKernel;

        public ToolService(ISemanticKernel semanticKernel)
        {
            _semanticKernel = semanticKernel ?? throw new ArgumentNullException(nameof(semanticKernel));
        }

        public async Task<string> ExecuteToolAsync(string toolName, object parameters)
        {
            if (string.IsNullOrEmpty(toolName))
            {
                throw new ArgumentException("Tool name cannot be null or empty.", nameof(toolName));
            }

            // Assuming the Semantic Kernel has a method to execute tools
            var result = await _semanticKernel.ExecuteAsync(toolName, parameters);
            return result;
        }

        public void RegisterTool(string toolName, Func<object, Task<string>> toolFunction)
        {
            if (string.IsNullOrEmpty(toolName))
            {
                throw new ArgumentException("Tool name cannot be null or empty.", nameof(toolName));
            }

            if (toolFunction == null)
            {
                throw new ArgumentNullException(nameof(toolFunction));
            }

            // Assuming the Semantic Kernel has a method to register tools
            _semanticKernel.RegisterTool(toolName, toolFunction);
        }
    }
}