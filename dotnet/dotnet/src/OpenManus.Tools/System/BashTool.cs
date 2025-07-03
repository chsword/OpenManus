using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenManus.Core.Models;
using OpenManus.Tools.FileOperations;

namespace OpenManus.Tools.System
{
    /// <summary>
    /// Tool for executing shell commands in PowerShell (Windows) or Bash (Unix-like systems).
    /// Adapted for Windows PowerShell environment.
    /// </summary>
    public class BashTool : BaseTool
    {
        private readonly IFileOperator _fileOperator;
        private const int DefaultTimeoutSeconds = 120;

        public BashTool(IFileOperator fileOperator, ILogger<BaseTool> logger) : base(logger)
        {
            _fileOperator = fileOperator ?? throw new ArgumentNullException(nameof(fileOperator));
            
            Parameters = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["command"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "The shell command to execute. On Windows, this will be executed using PowerShell."
                    },
                    ["timeout"] = new Dictionary<string, object>
                    {
                        ["type"] = "integer",
                        ["description"] = "Timeout in seconds for command execution (default: 120)",
                        ["default"] = DefaultTimeoutSeconds
                    }
                },
                ["required"] = new[] { "command" }
            };
        }

        /// <inheritdoc />
        public override string Name => "bash";

        /// <inheritdoc />
        public override string Description => 
            "Execute shell commands in PowerShell (Windows) or Bash (Unix-like systems). " +
            "Use this tool to run system commands, scripts, or interact with the operating system. " +
            "Commands are executed in the current working directory with appropriate environment variables.";

        /// <inheritdoc />
        protected override async Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters)
        {
            var command = GetParameter(parameters, "command", string.Empty);
            if (string.IsNullOrWhiteSpace(command))
            {
                return ToolResult.Failure("Command parameter is required and cannot be empty");
            }

            var timeoutObj = GetParameter<object>(parameters, "timeout", DefaultTimeoutSeconds);
            if (!int.TryParse(timeoutObj?.ToString(), out var timeout))
            {
                timeout = DefaultTimeoutSeconds;
            }

            _logger.LogInformation("🔧 Executing command: {Command}", command);

            try
            {
                var (exitCode, stdout, stderr) = await _fileOperator.RunCommandAsync(command, timeout);

                var result = new
                {
                    ExitCode = exitCode,
                    Output = stdout,
                    Error = stderr,
                    Success = exitCode == 0
                };

                var output = FormatCommandResult(command, exitCode, stdout, stderr);

                if (exitCode == 0)
                {
                    _logger.LogInformation("✅ Command executed successfully with exit code {ExitCode}", exitCode);
                    return ToolResult.Success(output);
                }
                else
                {
                    _logger.LogWarning("⚠️ Command failed with exit code {ExitCode}", exitCode);
                    // Even if command failed, we return success as the tool executed correctly
                    // The agent can decide how to handle the command failure
                    return ToolResult.Success(output);
                }
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "⏱️ Command timed out after {Timeout}s", timeout);
                return ToolResult.Failure($"Command timed out after {timeout} seconds: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error executing command");
                return ToolResult.Failure($"Failed to execute command: {ex.Message}");
            }
        }

        /// <summary>
        /// Format the command execution result for display.
        /// </summary>
        private static string FormatCommandResult(string command, int exitCode, string stdout, string stderr)
        {
            var result = $"Command: {command}\n";
            result += $"Exit Code: {exitCode}\n";
            
            if (!string.IsNullOrWhiteSpace(stdout))
            {
                result += $"\nOutput:\n{stdout.TrimEnd()}\n";
            }
            
            if (!string.IsNullOrWhiteSpace(stderr))
            {
                result += $"\nError Output:\n{stderr.TrimEnd()}\n";
            }
            
            if (string.IsNullOrWhiteSpace(stdout) && string.IsNullOrWhiteSpace(stderr))
            {
                result += "\n(No output)\n";
            }

            return result;
        }
    }
}
