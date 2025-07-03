using System;
using System.CommandLine;
using System.CommandLine.Invocation;
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

        public Command CreateCommand()
        {
            var command = new Command("mcp", "Handles MCP related commands");

            command.AddCommand(CreateRunCommand());

            return command;
        }

        private Command CreateRunCommand()
        {
            var runCommand = new Command("run", "Runs the MCP process");

            runCommand.Handler = CommandHandler.Create(() =>
            {
                try
                {
                    _mcpService.Run();
                    Console.WriteLine("MCP process started successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting MCP process: {ex.Message}");
                }
            });

            return runCommand;
        }
    }
}