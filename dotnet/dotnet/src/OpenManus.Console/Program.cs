using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenManus.Agent;
using OpenManus.Agent.Specialized;
using OpenManus.Core.Models;
using OpenManus.Core.Services;
using OpenManus.Tools;
using OpenManus.Tools.Core;
using OpenManus.Tools.FileOperations;
using OpenManus.Tools.FileSystem;
using OpenManus.Tools.System;
using OpenManus.Tools.Web;

namespace OpenManus.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            System.Console.WriteLine("🚀 OpenManus .NET Console Application");
            System.Console.WriteLine("=====================================\n");

            var host = CreateHostBuilder(args).Build();

            // Get services
            var serviceProvider = host.Services;
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Demo the system
                await RunDemoAsync(serviceProvider, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Application failed");
                System.Console.WriteLine($"❌ Error: {ex.Message}");
            }

            System.Console.WriteLine("\n👋 Press any key to exit...");
            System.Console.ReadKey();
        }

        private static async Task RunDemoAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            logger.LogInformation("Setting up OpenManus components...");

            // Create a basic kernel for demonstration
            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-3.5-turbo", "your-api-key-here") // Replace with actual key
                .Build();

            // Create tool manager and register tools
            var toolManager = new ToolManager(serviceProvider.GetRequiredService<ILogger<ToolManager>>());
            var fileOperator = new LocalFileOperator(serviceProvider.GetRequiredService<ILogger<LocalFileOperator>>());

            // Register various tools
            toolManager.RegisterTool(new TerminateTool(serviceProvider.GetRequiredService<ILogger<BaseTool>>()));
            toolManager.RegisterTool(new AskHumanTool(serviceProvider.GetRequiredService<ILogger<BaseTool>>()));
            toolManager.RegisterTool(new CreateChatCompletionTool(kernel, serviceProvider.GetRequiredService<ILogger<BaseTool>>()));
            toolManager.RegisterTool(new BashTool(fileOperator, serviceProvider.GetRequiredService<ILogger<BaseTool>>()));
            toolManager.RegisterTool(new PythonExecuteTool(fileOperator, serviceProvider.GetRequiredService<ILogger<BaseTool>>()));
            toolManager.RegisterTool(new FileOperationsTool(fileOperator, serviceProvider.GetRequiredService<ILogger<BaseTool>>()));
            // Skip WebSearchTool for now as it requires additional dependencies
            // toolManager.RegisterTool(new WebSearchTool(...));

            logger.LogInformation("Registered {ToolCount} tools: {Tools}",
                toolManager.ToolCount,
                string.Join(", ", toolManager.GetAvailableTools()));

            // Demo 1: List available tools
            System.Console.WriteLine("📋 Available Tools:");
            foreach (var toolName in toolManager.GetAvailableTools())
            {
                var toolInfo = toolManager.GetToolInfo(toolName);
                System.Console.WriteLine($"  • {toolName}: {toolInfo?.Description}");
            }
            System.Console.WriteLine();

            // Demo 2: Execute a simple tool
            System.Console.WriteLine("🔧 Demo: Executing Terminate Tool");
            var terminateResult = await toolManager.ExecuteToolAsync("terminate", new Dictionary<string, object>
            {
                ["reason"] = "Demo completed successfully"
            });

            System.Console.WriteLine($"Result: {(terminateResult.IsSuccess() ? "✅ Success" : "❌ Failed")}");
            System.Console.WriteLine($"Output: {terminateResult.Result()}");
            System.Console.WriteLine();

            // Demo 3: Agent Framework Demo (simplified)
            System.Console.WriteLine("🤖 Demo: Agent Framework");

            try
            {
                // Create a simple agent memory demonstration
                var memory = new Memory();
                memory.AddMessage(Message.CreateUserMessage("Hello, can you help me analyze some data?"));
                memory.AddMessage(Message.CreateSystemMessage("Agent is ready to help with data analysis tasks."));
                memory.AddMessage(Message.CreateAssistantMessage("I'd be happy to help you analyze data! What kind of data are you working with?"));

                System.Console.WriteLine($"Agent Memory Demo: {memory.Count} messages");

                // Display the messages
                foreach (var message in memory.Messages)
                {
                    var preview = message.Content?.Length > 50
                        ? message.Content.Substring(0, 50) + "..."
                        : message.Content ?? "(no content)";
                    System.Console.WriteLine($"  {message.Role}: {preview}");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not demo agent framework");
                System.Console.WriteLine("⚠️ Agent framework demo failed");
                System.Console.WriteLine($"   Error: {ex.Message}");
            }

            System.Console.WriteLine();

            // Demo 4: File operations
            System.Console.WriteLine("📁 Demo: File Operations Tool");
            try
            {
                var testFilePath = Path.Combine(Path.GetTempPath(), "openmanus_test.txt");
                var testContent = "Hello from OpenManus!\nThis is a test file created by the FileOperations tool.";

                // Create file
                var createResult = await toolManager.ExecuteToolAsync("file_operations", new Dictionary<string, object>
                {
                    ["operation"] = "write",
                    ["path"] = testFilePath,
                    ["content"] = testContent
                });

                System.Console.WriteLine($"Create file: {(createResult.IsSuccess() ? "✅ Success" : "❌ Failed")}");

                if (createResult.IsSuccess())
                {
                    // Read file
                    var readResult = await toolManager.ExecuteToolAsync("file_operations", new Dictionary<string, object>
                    {
                        ["operation"] = "read",
                        ["path"] = testFilePath
                    });

                    System.Console.WriteLine($"Read file: {(readResult.IsSuccess() ? "✅ Success" : "❌ Failed")}");

                    // Clean up
                    try { File.Delete(testFilePath); } catch { }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "File operations demo failed");
                System.Console.WriteLine("⚠️ File operations demo failed");
            }

            System.Console.WriteLine();

            // Demo 5: Python execution (if Python is available)
            System.Console.WriteLine("🐍 Demo: Python Execution Tool");
            try
            {
                var pythonCode = @"
print('Hello from Python!')
print('Calculating 2 + 2 =', 2 + 2)
import sys
print(f'Python version: {sys.version_info.major}.{sys.version_info.minor}')
";

                var pythonResult = await toolManager.ExecuteToolAsync("python_execute", new Dictionary<string, object>
                {
                    ["code"] = pythonCode,
                    ["timeout"] = 10
                });

                System.Console.WriteLine($"Python execution: {(pythonResult.IsSuccess() ? "✅ Success" : "❌ Failed")}");
                if (!pythonResult.IsSuccess())
                {
                    System.Console.WriteLine($"Error: {pythonResult.ErrorMessage()}");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Python execution demo failed");
                System.Console.WriteLine("⚠️ Python execution demo failed (Python may not be installed)");
            }

            System.Console.WriteLine("\n🎉 Demo completed!");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register core services
                    services.AddScoped<IToolManager, ToolManager>();
                    services.AddScoped<IFileOperator, LocalFileOperator>();

                    // Register logging
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(LogLevel.Information);
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);
                });
    }
}
