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
using OpenManus.Tools.Plugins;

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

            // Create a kernel with plugins
            var kernelBuilder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-3.5-turbo", "your-api-key-here"); // Replace with actual key

            // Add plugin instances
            var fileSystemPlugin = new FileSystemPlugin(serviceProvider.GetRequiredService<ILogger<FileSystemPlugin>>());
            var shellPlugin = new ShellPlugin(serviceProvider.GetRequiredService<ILogger<ShellPlugin>>());
            var pythonPlugin = new PythonPlugin(serviceProvider.GetRequiredService<ILogger<PythonPlugin>>());

            kernelBuilder.Plugins.AddFromObject(fileSystemPlugin, "FileSystem");
            kernelBuilder.Plugins.AddFromObject(shellPlugin, "Shell");
            kernelBuilder.Plugins.AddFromObject(pythonPlugin, "Python");

            var kernel = kernelBuilder.Build();

            // Create tool manager for legacy tools and register remaining tools
            var toolManager = new ToolManager(serviceProvider.GetRequiredService<ILogger<ToolManager>>());

            // Register non-plugin tools
            toolManager.RegisterTool(new TerminateTool(serviceProvider.GetRequiredService<ILogger<BaseTool>>()));
            toolManager.RegisterTool(new AskHumanTool(serviceProvider.GetRequiredService<ILogger<BaseTool>>()));
            toolManager.RegisterTool(new CreateChatCompletionTool(kernel, serviceProvider.GetRequiredService<ILogger<BaseTool>>()));

            logger.LogInformation("Registered {ToolCount} legacy tools: {Tools}",
                toolManager.ToolCount,
                string.Join(", ", toolManager.GetAvailableTools()));

            // Demo 1: List available tools and plugins
            System.Console.WriteLine("📋 Available Tools:");

            // Show legacy tools
            foreach (var toolName in toolManager.GetAvailableTools())
            {
                var toolInfo = toolManager.GetToolInfo(toolName);
                System.Console.WriteLine($"  • {toolName}: {toolInfo?.Description}");
            }

            // Show plugins
            foreach (var plugin in kernel.Plugins)
            {
                System.Console.WriteLine($"  • Plugin '{plugin.Name}' with {plugin.FunctionCount} functions");
                foreach (var function in plugin)
                {
                    System.Console.WriteLine($"    - {function.Name}: {function.Description}");
                }
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

            // Demo 4: File operations using FileSystemPlugin
            System.Console.WriteLine("📁 Demo: File Operations Plugin");
            try
            {
                var testFilePath = Path.Combine(Path.GetTempPath(), "openmanus_test.txt");
                var testContent = "Hello from OpenManus!\nThis is a test file created by the FileSystem plugin.";

                // Create file using plugin
                var writeFunction = kernel.Plugins["FileSystem"]["WriteFile"];
                var writeResult = await kernel.InvokeAsync(writeFunction, new KernelArguments
                {
                    ["path"] = testFilePath,
                    ["content"] = testContent
                });

                System.Console.WriteLine($"Create file: ✅ Success");
                System.Console.WriteLine($"Output: {writeResult.GetValue<string>()}");

                // Read file using plugin
                var readFunction = kernel.Plugins["FileSystem"]["ReadFile"];
                var readResult = await kernel.InvokeAsync(readFunction, new KernelArguments
                {
                    ["path"] = testFilePath
                });

                System.Console.WriteLine($"Read file: ✅ Success");
                var content = readResult.GetValue<string>();
                System.Console.WriteLine($"Content preview: {(content?.Length > 50 ? content.Substring(0, 50) + "..." : content)}");

                // Clean up
                try { File.Delete(testFilePath); } catch { }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "File operations demo failed");
                System.Console.WriteLine("⚠️ File operations demo failed");
                System.Console.WriteLine($"   Error: {ex.Message}");
            }

            System.Console.WriteLine();

            // Demo 5: Python execution using PythonPlugin
            System.Console.WriteLine("🐍 Demo: Python Execution Plugin");
            try
            {
                var pythonCode = @"
print('Hello from Python!')
print('Calculating 2 + 2 =', 2 + 2)
import sys
print(f'Python version: {sys.version_info.major}.{sys.version_info.minor}')
";

                var pythonFunction = kernel.Plugins["Python"]["ExecutePython"];
                var pythonResult = await kernel.InvokeAsync(pythonFunction, new KernelArguments
                {
                    ["code"] = pythonCode,
                    ["timeout"] = 10
                });

                System.Console.WriteLine($"Python execution: ✅ Success");
                var output = pythonResult.GetValue<string>();
                System.Console.WriteLine($"Output preview: {(output?.Length > 100 ? output.Substring(0, 100) + "..." : output)}");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Python execution demo failed");
                System.Console.WriteLine("⚠️ Python execution demo failed (Python may not be installed)");
                System.Console.WriteLine($"   Error: {ex.Message}");
            }

            System.Console.WriteLine("\n🎉 Demo completed!");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Register core services
                    services.AddScoped<IToolManager, ToolManager>();

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
