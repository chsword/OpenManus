using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using OpenManus.Flow;

namespace OpenManus.Console.Commands
{
    public class FlowCommand
    {
        private readonly IFlowService _flowService;

        public FlowCommand(IFlowService flowService)
        {
            _flowService = flowService ?? throw new ArgumentNullException(nameof(flowService));
        }

        public Command CreateCommand()
        {
            var command = new Command("flow", "Handles flow-related commands");

            command.AddCommand(CreateStartCommand());
            command.AddCommand(CreateStopCommand());

            return command;
        }

        private Command CreateStartCommand()
        {
            var command = new Command("start", "Starts a flow");

            command.Handler = CommandHandler.Create(() =>
            {
                // Logic to start the flow
                _flowService.StartFlow();
                Console.WriteLine("Flow started.");
            });

            return command;
        }

        private Command CreateStopCommand()
        {
            var command = new Command("stop", "Stops a flow");

            command.Handler = CommandHandler.Create(() =>
            {
                // Logic to stop the flow
                _flowService.StopFlow();
                Console.WriteLine("Flow stopped.");
            });

            return command;
        }
    }
}