using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace OpenManus.Tools.Plugins
{
    /// <summary>
    /// A Semantic Kernel plugin for executing Python code with timeout and safety restrictions.
    /// </summary>
    public class PythonPlugin
    {
        private readonly ILogger<PythonPlugin> _logger;
        private const int DefaultTimeoutSeconds = 30;
        private const string TempFilePrefix = "openmanus_python_";

        public PythonPlugin(ILogger<PythonPlugin> logger)
        {
            _logger = logger;
        }

        [KernelFunction, Description("Executes Python code with timeout and safety restrictions. Only print outputs are visible, function return values are not captured. Use print statements to see results.")]
        public async Task<string> ExecutePythonAsync(
            [Description("The Python code to execute. Use print statements to see results.")] string code,
            [Description("Timeout in seconds for code execution (default: 30).")] int timeout = DefaultTimeoutSeconds,
            [Description("Working directory for code execution (optional).")] string? workingDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return "Error: Code parameter is required and cannot be empty";
            }

            workingDirectory ??= Path.GetTempPath();

            _logger.LogInformation("🐍 Executing Python code (timeout: {Timeout}s)", timeout);

            string tempFilePath = "";
            try
            {
                // Create temporary Python file
                tempFilePath = Path.Combine(Path.GetTempPath(), $"{TempFilePrefix}{Guid.NewGuid()}.py");
                await File.WriteAllTextAsync(tempFilePath, code);

                // Execute Python code
                var result = await ExecutePythonFileAsync(tempFilePath, workingDirectory, timeout);

                var formattedResult = FormatExecutionResult(code, result.exitCode, result.stdout, result.stderr);

                if (result.exitCode == 0)
                {
                    _logger.LogInformation("✅ Python code executed successfully");
                }
                else
                {
                    _logger.LogWarning("⚠️ Python code execution failed with exit code {ExitCode}", result.exitCode);
                }

                return formattedResult;
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "⏱️ Python code execution timed out after {Timeout}s", timeout);
                return $"Python execution timed out after {timeout} seconds: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error executing Python code");
                return $"Failed to execute Python code: {ex.Message}";
            }
            finally
            {
                // Clean up temporary file
                if (!string.IsNullOrEmpty(tempFilePath))
                {
                    try
                    {
                        if (File.Exists(tempFilePath))
                        {
                            File.Delete(tempFilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to delete temporary Python file: {FilePath}", tempFilePath);
                    }
                }
            }
        }

        /// <summary>
        /// Execute Python file using python interpreter
        /// </summary>
        private async Task<(int exitCode, string stdout, string stderr)> ExecutePythonFileAsync(
            string pythonFilePath, string workingDirectory, int timeoutSeconds)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"\"{pythonFilePath}\"",
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Add safety environment variables
            processStartInfo.Environment["PYTHONDONTWRITEBYTECODE"] = "1";
            processStartInfo.Environment["PYTHONUNBUFFERED"] = "1";

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

            var completed = await Task.Run(() => process.WaitForExit(timeoutSeconds * 1000));

            if (!completed)
            {
                try
                {
                    process.Kill();
                    await Task.Run(() => process.WaitForExit(5000)); // Wait up to 5 seconds for cleanup
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to kill Python process");
                }

                throw new TimeoutException($"Python execution exceeded timeout of {timeoutSeconds} seconds");
            }

            var stdout = outputBuilder.ToString().TrimEnd();
            var stderr = errorBuilder.ToString().TrimEnd();

            return (process.ExitCode, stdout, stderr);
        }

        /// <summary>
        /// Format the Python execution result for display
        /// </summary>
        private static string FormatExecutionResult(string code, int exitCode, string stdout, string stderr)
        {
            var result = "Python Code Execution Result:\n";
            result += "=" + new string('=', 40) + "\n\n";

            // Show a snippet of the code
            var codeLines = code.Split('\n');
            var codePreview = codeLines.Length > 10
                ? string.Join("\n", codeLines.Take(10)) + "\n... (truncated)"
                : code;

            result += $"Code:\n{codePreview}\n\n";

            result += $"Exit Code: {exitCode}\n\n";

            if (!string.IsNullOrWhiteSpace(stdout))
            {
                result += $"Output:\n{stdout}\n\n";
            }

            if (!string.IsNullOrWhiteSpace(stderr))
            {
                result += $"Error Output:\n{stderr}\n\n";
            }

            if (string.IsNullOrWhiteSpace(stdout) && string.IsNullOrWhiteSpace(stderr))
            {
                result += "(No output produced)\n\n";
            }

            result += "=" + new string('=', 40);

            return result;
        }
    }
}
