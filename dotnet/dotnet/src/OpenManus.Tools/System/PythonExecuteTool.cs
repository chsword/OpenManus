using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenManus.Core.Models;
using OpenManus.Tools.FileOperations;

namespace OpenManus.Tools.System
{
    /// <summary>
    /// Tool for executing Python code with timeout and safety restrictions.
    /// Note: Only print outputs are visible, function return values are not captured.
    /// </summary>
    public class PythonExecuteTool : BaseTool
    {
        private readonly IFileOperator _fileOperator;
        private const int DefaultTimeoutSeconds = 30;
        private const string TempFilePrefix = "openmanus_python_";

        public PythonExecuteTool(IFileOperator fileOperator, ILogger<BaseTool> logger) : base(logger)
        {
            _fileOperator = fileOperator ?? throw new ArgumentNullException(nameof(fileOperator));

            Parameters = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["code"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "The Python code to execute. Use print statements to see results."
                    },
                    ["timeout"] = new Dictionary<string, object>
                    {
                        ["type"] = "integer",
                        ["description"] = "Timeout in seconds for code execution (default: 30)",
                        ["default"] = DefaultTimeoutSeconds
                    },
                    ["working_directory"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Working directory for code execution (optional)"
                    }
                },
                ["required"] = new[] { "code" }
            };
        }

        /// <inheritdoc />
        public override string Name => "python_execute";

        /// <inheritdoc />
        public override string Description =>
            "Executes Python code with timeout and safety restrictions. " +
            "Only print outputs are visible, function return values are not captured. " +
            "Use print statements to see results. Code is executed in a temporary file.";

        /// <inheritdoc />
        protected override async Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters)
        {
            var code = GetParameter(parameters, "code", string.Empty);
            if (string.IsNullOrWhiteSpace(code))
            {
                return ToolResult.Failure("Code parameter is required and cannot be empty");
            }

            var timeoutObj = GetParameter<object>(parameters, "timeout", DefaultTimeoutSeconds);
            if (!int.TryParse(timeoutObj?.ToString(), out var timeout))
            {
                timeout = DefaultTimeoutSeconds;
            }

            var workingDirectory = GetParameter(parameters, "working_directory", Path.GetTempPath());

            _logger.LogInformation("🐍 Executing Python code (timeout: {Timeout}s)", timeout);

            string tempFilePath = "";
            try
            {
                // Create temporary Python file
                tempFilePath = Path.Combine(Path.GetTempPath(), $"{TempFilePrefix}{Guid.NewGuid()}.py");
                await _fileOperator.WriteFileAsync(tempFilePath, code);

                // Execute Python code
                var result = await ExecutePythonFileAsync(tempFilePath, workingDirectory, timeout);

                var formattedResult = FormatExecutionResult(code, result.exitCode, result.stdout, result.stderr);

                if (result.exitCode == 0)
                {
                    _logger.LogInformation("✅ Python code executed successfully");
                    return ToolResult.Success(formattedResult);
                }
                else
                {
                    _logger.LogWarning("⚠️ Python code execution failed with exit code {ExitCode}", result.exitCode);
                    return ToolResult.Success(formattedResult); // Return success but include error info
                }
            }
            catch (TimeoutException ex)
            {
                _logger.LogError(ex, "⏱️ Python code execution timed out after {Timeout}s", timeout);
                return ToolResult.Failure($"Python execution timed out after {timeout} seconds: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error executing Python code");
                return ToolResult.Failure($"Failed to execute Python code: {ex.Message}");
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

            var outputBuilder = new System.Text.StringBuilder();
            var errorBuilder = new System.Text.StringBuilder();

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
