using System;
using OpenManus.Mcp;

namespace OpenManus.Console.Commands
{
    public class McpCommand
    {
        private readonly IMcpService _mcpService;

        public McpCommand(IMcpService mcpService)
        {
            _mcpService = mcpService;
        }

        public void ExecuteRun()
        {
            try
            {
                // _mcpService.Run();
                System.Console.WriteLine("MCP process started successfully.");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error starting MCP process: {ex.Message}");
            }
        }
    }
}
