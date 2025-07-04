using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace OpenManus.Tools.Plugins
{
    /// <summary>
    /// A Semantic Kernel plugin for executing shell commands.
    /// This plugin adapts to the current platform - PowerShell on Windows, Bash on Unix.
    /// </summary>
    public class ShellPlugin
    {
        private readonly ILogger<ShellPlugin> _logger;

        public ShellPlugin(ILogger<ShellPlugin> logger)
        {
            _logger = logger;
        }

        [KernelFunction, Description("Executes a shell command and returns the output. On Windows, this uses PowerShell. On Unix systems, this uses bash.")]
        public async Task<string> ExecuteCommandAsync(
            [Description("The shell command to execute.")] string command,
            [Description("Timeout in seconds for command execution (default: 30).")] int timeout = 30,
            [Description("Working directory for command execution (optional).")] string? workingDirectory = null)
        {
            _logger.LogInformation("🔧 Executing shell command: {Command}", command);

            try
            {
                var (fileName, arguments) = GetShellExecutorInfo(command);

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = processStartInfo };

                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        outputBuilder.AppendLine(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        errorBuilder.AppendLine(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var completed = await Task.Run(() => process.WaitForExit(timeout * 1000));

                if (!completed)
                {
                    try
                    {
                        process.Kill();
                        await Task.Run(() => process.WaitForExit(5000));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to kill process");
                    }

                    return $"Error: Command timed out after {timeout} seconds";
                }

                var stdout = outputBuilder.ToString().TrimEnd();
                var stderr = errorBuilder.ToString().TrimEnd();

                var result = FormatCommandResult(command, process.ExitCode, stdout, stderr);

                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("✅ Shell command executed successfully");
                }
                else
                {
                    _logger.LogWarning("⚠️ Shell command failed with exit code {ExitCode}", process.ExitCode);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing shell command");
                return $"Error executing command: {ex.Message}";
            }
        }

        private static (string fileName, string arguments) GetShellExecutorInfo(string command)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Use PowerShell on Windows
                return ("powershell.exe", $"-NoProfile -Command \"{command.Replace("\"", "`\"")}\"");
            }
            else
            {
                // Use bash on Unix systems
                return ("/bin/bash", $"-c \"{command.Replace("\"", "\\\"")}\"");
            }
        }

        private static string FormatCommandResult(string command, int exitCode, string stdout, string stderr)
        {
            var result = new StringBuilder();
            result.AppendLine("Shell Command Execution Result:");
            result.AppendLine("=" + new string('=', 40));
            result.AppendLine();

            // Show command
            result.AppendLine($"Command: {command}");
            result.AppendLine($"Exit Code: {exitCode}");
            result.AppendLine();

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                result.AppendLine("Output:");
                result.AppendLine(stdout);
                result.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                result.AppendLine("Error Output:");
                result.AppendLine(stderr);
                result.AppendLine();
            }

            if (string.IsNullOrWhiteSpace(stdout) && string.IsNullOrWhiteSpace(stderr))
            {
                result.AppendLine("(No output produced)");
                result.AppendLine();
            }

            result.AppendLine("=" + new string('=', 40));

            return result.ToString();
        }
    }
}
