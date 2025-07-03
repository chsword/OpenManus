using System;
using OpenManus.Core.Models;
using OpenManus.Agent;
using OpenManus.Flow;
using OpenManus.Llm;

namespace OpenManus.Examples.UseCase
{
    public class UseCaseExample
    {
        private readonly IAgent _agent;
        private readonly IFlowService _flowService;
        private readonly ILlmService _llmService;

        public UseCaseExample(IAgent agent, IFlowService flowService, ILlmService llmService)
        {
            _agent = agent;
            _flowService = flowService;
            _llmService = llmService;
        }

        public void Execute()
        {
            // Example of using the agent to perform a task
            var taskResult = _agent.PerformTask("Example task");
            Console.WriteLine($"Task Result: {taskResult}");

            // Example of using the flow service
            var flowResult = _flowService.ExecuteFlow("Example flow");
            Console.WriteLine($"Flow Result: {flowResult}");

            // Example of using the LLM service
            var llmResponse = _llmService.GenerateResponse("What is the capital of France?");
            Console.WriteLine($"LLM Response: {llmResponse}");
        }
    }
}