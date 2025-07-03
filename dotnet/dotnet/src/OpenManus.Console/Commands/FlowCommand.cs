using System;
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

        public void ExecuteStart()
        {
            // _flowService.StartFlow();
            System.Console.WriteLine("Flow started.");
        }

        public void ExecuteStop()
        {
            // _flowService.StopFlow();
            System.Console.WriteLine("Flow stopped.");
        }
    }
}
