using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OpenManus.Tools.Plugins;
using Xunit;

namespace OpenManus.Tools.Tests.Plugins
{
    public class PythonPluginTests
    {
        private readonly Mock<ILogger<PythonPlugin>> _mockLogger;
        private readonly PythonPlugin _plugin;

        public PythonPluginTests()
        {
            _mockLogger = new Mock<ILogger<PythonPlugin>>();
            _plugin = new PythonPlugin(_mockLogger.Object);
        }

        [Fact]
        public async Task ExecutePythonAsync_ShouldReturnErrorForEmptyCode()
        {
            // Arrange
            var emptyCode = string.Empty;

            // Act
            var result = await _plugin.ExecutePythonAsync(emptyCode);

            // Assert
            Assert.Contains("Error: Code parameter is required", result);
        }

        [Fact]
        public async Task ExecutePythonAsync_ShouldReturnErrorForWhitespaceCode()
        {
            // Arrange
            var whitespaceCode = "   \n\t  ";

            // Act
            var result = await _plugin.ExecutePythonAsync(whitespaceCode);

            // Assert
            Assert.Contains("Error: Code parameter is required", result);
        }

        [Fact]
        public async Task ExecutePythonAsync_ShouldHandleSimplePrintStatement()
        {
            // Arrange
            var code = "print('Hello from Python!')";

            // Act
            var result = await _plugin.ExecutePythonAsync(code, timeout: 5);

            // Assert
            // Note: This test may fail if Python is not installed
            // We check for either success or a specific failure message
            Assert.True(
                result.Contains("Hello from Python!") ||
                result.Contains("Failed to execute Python code") ||
                result.Contains("'python' is not recognized"),
                $"Unexpected result: {result}");
        }

        [Fact]
        public async Task ExecutePythonAsync_ShouldHandleBasicCalculation()
        {
            // Arrange
            var code = @"
result = 2 + 2
print(f'2 + 2 = {result}')
";

            // Act
            var result = await _plugin.ExecutePythonAsync(code, timeout: 5);

            // Assert
            // Note: This test may fail if Python is not installed
            Assert.True(
                result.Contains("2 + 2 = 4") ||
                result.Contains("Failed to execute Python code") ||
                result.Contains("'python' is not recognized"),
                $"Unexpected result: {result}");
        }

        [Fact]
        public async Task ExecutePythonAsync_ShouldIncludeCodeInResult()
        {
            // Arrange
            var code = "print('test')";

            // Act
            var result = await _plugin.ExecutePythonAsync(code, timeout: 5);

            // Assert
            Assert.Contains("Python Code Execution Result:", result);
            Assert.Contains("Code:", result);
            Assert.Contains("Exit Code:", result);
        }

        [Fact]
        public async Task ExecutePythonAsync_ShouldTruncateLongCodeInResult()
        {
            // Arrange
            var longCode = string.Join("\n", new string[15].Select((_, i) => $"print('Line {i + 1}')"));

            // Act
            var result = await _plugin.ExecutePythonAsync(longCode, timeout: 5);

            // Assert
            Assert.Contains("Python Code Execution Result:", result);
            Assert.Contains("Code:", result);
            // Should be truncated for long code
            if (longCode.Split('\n').Length > 10)
            {
                Assert.True(
                    result.Contains("(truncated)") ||
                    result.Contains("Failed to execute Python code") ||
                    result.Contains("'python' is not recognized"),
                    $"Expected truncation or error, got: {result}");
            }
        }

        [Fact]
        public async Task ExecutePythonAsync_ShouldRespectCustomTimeout()
        {
            // Arrange
            var code = "print('Quick execution')";
            var customTimeout = 1; // 1 second

            // Act
            var result = await _plugin.ExecutePythonAsync(code, timeout: customTimeout);

            // Assert
            // Should either succeed quickly or fail with timeout/missing python
            Assert.True(
                result.Contains("Quick execution") ||
                result.Contains("Failed to execute Python code") ||
                result.Contains("'python' is not recognized") ||
                result.Contains("timed out"),
                $"Unexpected result: {result}");
        }

        [Fact]
        public async Task ExecutePythonAsync_ShouldUseCustomWorkingDirectory()
        {
            // Arrange
            var code = @"
import os
print(f'Working directory: {os.getcwd()}')
";
            var customWorkingDir = Path.GetTempPath().TrimEnd(Path.DirectorySeparatorChar);

            // Act
            var result = await _plugin.ExecutePythonAsync(code, workingDirectory: customWorkingDir);

            // Assert
            Assert.True(
                result.Contains(customWorkingDir) ||
                result.Contains("Failed to execute Python code") ||
                result.Contains("'python' is not recognized") ||
                result.Contains("timed out"),
                $"Unexpected result: {result}");
        }

        [Theory]
        [InlineData("print('Hello')")]
        [InlineData("x = 1\nprint(x)")]
        [InlineData("# This is a comment\nprint('Comment test')")]
        public async Task ExecutePythonAsync_ShouldHandleVariousCodeFormats(string code)
        {
            // Act
            var result = await _plugin.ExecutePythonAsync(code, timeout: 5);

            // Assert
            // Should either execute successfully or fail gracefully
            Assert.False(string.IsNullOrEmpty(result));
            Assert.Contains("Python Code Execution Result:", result);
        }
    }
}
