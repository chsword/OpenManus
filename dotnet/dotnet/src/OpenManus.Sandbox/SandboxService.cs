using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

namespace OpenManus.Sandbox
{
    public class SandboxService : ISandboxService
    {
        private readonly IKernel _kernel;

        public SandboxService(IKernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public async Task<string> ExecuteSandboxedTaskAsync(string taskName, object input)
        {
            try
            {
                // Execute the task in a sandboxed environment
                var result = await _kernel.RunAsync(taskName, input);
                return result.ToString();
            }
            catch (Exception ex)
            {
                // Handle exceptions and log errors
                throw new OpenManusException("An error occurred while executing the sandboxed task.", ex);
            }
        }
    }
}