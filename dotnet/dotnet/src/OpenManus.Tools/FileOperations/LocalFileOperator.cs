using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenManus.Tools.FileOperations
{
    /// <summary>
    /// Local file system implementation of IFileOperator.
    /// </summary>
    public class LocalFileOperator : IFileOperator
    {
        private readonly ILogger<LocalFileOperator> _logger;
        private const string DefaultEncoding = "utf-8";

        public LocalFileOperator(ILogger<LocalFileOperator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<string> ReadFileAsync(string path)
        {
            try
            {
                _logger.LogDebug("Reading file: {Path}", path);
                var content = await File.ReadAllTextAsync(path, Encoding.UTF8);
                _logger.LogDebug("Successfully read {Length} characters from {Path}", content.Length, path);
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read file: {Path}", path);
                throw new InvalidOperationException($"Failed to read {path}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public async Task WriteFileAsync(string path, string content)
        {
            try
            {
                _logger.LogDebug("Writing {Length} characters to file: {Path}", content.Length, path);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug("Created directory: {Directory}", directory);
                }

                await File.WriteAllTextAsync(path, content, Encoding.UTF8);
                _logger.LogDebug("Successfully wrote to file: {Path}", path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write to file: {Path}", path);
                throw new InvalidOperationException($"Failed to write to {path}: {ex.Message}", ex);
            }
        }

        /// <inheritdoc />
        public Task<bool> IsDirectoryAsync(string path)
        {
            try
            {
                var result = Directory.Exists(path);
                _logger.LogDebug("Directory check for {Path}: {Result}", path, result);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking if path is directory: {Path}", path);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc />
        public Task<bool> ExistsAsync(string path)
        {
            try
            {
                var result = File.Exists(path) || Directory.Exists(path);
                _logger.LogDebug("Existence check for {Path}: {Result}", path, result);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking if path exists: {Path}", path);
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc />
        public async Task<(int exitCode, string stdout, string stderr)> RunCommandAsync(string command, int timeoutSeconds = 120)
        {
            try
            {
                _logger.LogDebug("Running command: {Command}", command);

                using var process = new Process();
                
                // Configure process for PowerShell on Windows
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    process.StartInfo.FileName = "powershell.exe";
                    process.StartInfo.Arguments = $"-Command \"{command}\"";
                }
                else
                {
                    process.StartInfo.FileName = "/bin/bash";
                    process.StartInfo.Arguments = $"-c \"{command}\"";
                }

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;

                var stdoutBuilder = new StringBuilder();
                var stderrBuilder = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        stdoutBuilder.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        stderrBuilder.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                var completed = await process.WaitForExitAsync(TimeSpan.FromSeconds(timeoutSeconds));
                
                if (!completed)
                {
                    _logger.LogWarning("Command timed out after {Timeout}s: {Command}", timeoutSeconds, command);
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception killEx)
                    {
                        _logger.LogWarning(killEx, "Failed to kill timed out process");
                    }
                    throw new TimeoutException($"Command '{command}' timed out after {timeoutSeconds} seconds");
                }

                var exitCode = process.ExitCode;
                var stdout = stdoutBuilder.ToString().TrimEnd();
                var stderr = stderrBuilder.ToString().TrimEnd();

                _logger.LogDebug("Command completed with exit code {ExitCode}: {Command}", exitCode, command);
                
                return (exitCode, stdout, stderr);
            }
            catch (Exception ex) when (!(ex is TimeoutException))
            {
                _logger.LogError(ex, "Error running command: {Command}", command);
                throw new InvalidOperationException($"Failed to run command '{command}': {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Extension methods for Process class to support async wait with timeout.
    /// </summary>
    public static class ProcessExtensions
    {
        public static async Task<bool> WaitForExitAsync(this Process process, TimeSpan timeout)
        {
            using var cts = new System.Threading.CancellationTokenSource(timeout);
            var tcs = new TaskCompletionSource<bool>();

            void ProcessExited(object? sender, EventArgs e) => tcs.TrySetResult(true);
            
            process.EnableRaisingEvents = true;
            process.Exited += ProcessExited;

            using (cts.Token.Register(() => tcs.TrySetResult(false)))
            {
                return await tcs.Task;
            }
        }
    }
}
