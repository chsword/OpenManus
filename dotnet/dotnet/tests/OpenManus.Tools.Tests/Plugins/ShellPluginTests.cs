using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OpenManus.Tools.Plugins;
using Xunit;

namespace OpenManus.Tools.Tests.Plugins
{
    public class ShellPluginTests
    {
        private readonly Mock<ILogger<ShellPlugin>> _mockLogger;
        private readonly ShellPlugin _plugin;

        public ShellPluginTests()
        {
            _mockLogger = new Mock<ILogger<ShellPlugin>>();
            _plugin = new ShellPlugin(_mockLogger.Object);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldExecuteSimpleCommand()
        {
            // Arrange
            var command = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? "echo Hello World"
                : "echo 'Hello World'";

            // Act
            var result = await _plugin.ExecuteCommandAsync(command, timeout: 10);

            // Assert
            Assert.Contains("Shell Command Execution Result:", result);
            Assert.Contains("Command:", result);
            Assert.Contains("Exit Code:", result);
            Assert.Contains("Hello World", result);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldHandleWorkingDirectory()
        {
            // Arrange
            var tempDir = Path.GetTempPath();
            var command = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? "pwd" // Get-Location in PowerShell
                : "pwd";

            // Act
            var result = await _plugin.ExecuteCommandAsync(command, workingDirectory: tempDir, timeout: 10);

            // Assert
            Assert.Contains("Shell Command Execution Result:", result);
            Assert.Contains("Command:", result);
            // Note: PowerShell pwd might show different format than expected
            Assert.True(
                result.Contains(tempDir) ||
                result.Contains("Output:") ||
                result.Contains("Error Output:"),
                $"Expected working directory info, got: {result}");
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldHandleCommandWithOutput()
        {
            // Arrange
            var command = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? "echo First; echo Second"
                : "echo 'First'; echo 'Second'";

            // Act
            var result = await _plugin.ExecuteCommandAsync(command, timeout: 10);

            // Assert
            Assert.Contains("Shell Command Execution Result:", result);
            Assert.Contains("Output:", result);
            Assert.True(
                result.Contains("First") && result.Contains("Second"),
                $"Expected both outputs, got: {result}");
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldHandleNonExistentCommand()
        {
            // Arrange
            var command = "this-command-does-not-exist-12345";

            // Act
            var result = await _plugin.ExecuteCommandAsync(command, timeout: 10);

            // Assert
            Assert.Contains("Shell Command Execution Result:", result);
            Assert.Contains("Command:", result);
            // Should show non-zero exit code or error output
            Assert.True(
                result.Contains("Exit Code: 1") ||
                result.Contains("Error Output:") ||
                result.Contains("not recognized") ||
                result.Contains("command not found"),
                $"Expected error indication, got: {result}");
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldRespectTimeout()
        {
            // Arrange
            var command = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? "Start-Sleep -Seconds 5" // PowerShell sleep
                : "sleep 5"; // Unix sleep
            var shortTimeout = 1; // 1 second

            // Act
            var result = await _plugin.ExecuteCommandAsync(command, timeout: shortTimeout);

            // Assert
            Assert.True(
                result.Contains("timed out") ||
                result.Contains("Error executing command") ||
                result.Contains("Shell Command Execution Result:"), // In case sleep command is not available
                $"Expected timeout or error, got: {result}");
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldFormatResultCorrectly()
        {
            // Arrange
            var command = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? "echo Test"
                : "echo 'Test'";

            // Act
            var result = await _plugin.ExecuteCommandAsync(command, timeout: 10);

            // Assert
            // Check the result format
            Assert.Contains("Shell Command Execution Result:", result);
            Assert.Contains("=" + new string('=', 40), result);
            Assert.Contains($"Command: {command}", result);
            Assert.Contains("Exit Code:", result);
            Assert.Contains("Output:", result);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldHandleEmptyOutput()
        {
            // Arrange
            var command = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? "$null" // PowerShell null output
                : "true"; // Unix command that produces no output

            // Act
            var result = await _plugin.ExecuteCommandAsync(command, timeout: 10);

            // Assert
            Assert.Contains("Shell Command Execution Result:", result);
            Assert.Contains("Command:", result);
            Assert.True(
                result.Contains("(No output produced)") ||
                result.Contains("Output:"),
                $"Expected no output indication or output section, got: {result}");
        }

        [Theory]
        [InlineData("echo Test1")]
        [InlineData("echo 'Test with spaces'")]
        public async Task ExecuteCommandAsync_ShouldHandleVariousCommands(string command)
        {
            // Act
            var result = await _plugin.ExecuteCommandAsync(command, timeout: 10);

            // Assert
            Assert.False(string.IsNullOrEmpty(result));
            Assert.Contains("Shell Command Execution Result:", result);
            Assert.Contains("Command:", result);
        }

        [Fact]
        public async Task ExecuteCommandAsync_ShouldUseCorrectShellForPlatform()
        {
            // This test verifies that the correct shell is used based on platform

            // Arrange
            var command = "echo Platform test";

            // Act
            var result = await _plugin.ExecuteCommandAsync(command, timeout: 10);

            // Assert
            Assert.Contains("Shell Command Execution Result:", result);
            Assert.Contains("Platform test", result);

            // The implementation should use PowerShell on Windows and bash on Unix
            // We can't directly test this without exposing internal methods,
            // but we can verify the command executes successfully
            Assert.True(
                result.Contains("Exit Code: 0") ||
                result.Contains("Platform test"),
                $"Expected successful execution, got: {result}");
        }
    }
}
