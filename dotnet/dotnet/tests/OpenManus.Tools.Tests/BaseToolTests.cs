using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OpenManus.Core.Models;
using OpenManus.Tools;
using Xunit;

namespace OpenManus.Tools.Tests
{
    // Test implementation of BaseTool for testing purposes
    public class TestTool : BaseTool
    {
        public override string Name => "test_tool";
        public override string Description => "A test tool for unit testing";

        private readonly bool _shouldThrowException;
        private readonly string _returnValue;

        public TestTool(ILogger<BaseTool> logger, bool shouldThrowException = false, string returnValue = "success")
            : base(logger)
        {
            _shouldThrowException = shouldThrowException;
            _returnValue = returnValue;

            Parameters = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["input"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Test input parameter"
                    }
                },
                ["required"] = new[] { "input" }
            };
        }

        protected override Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters)
        {
            if (_shouldThrowException)
            {
                throw new InvalidOperationException("Test exception");
            }

            var input = GetParameter(parameters, "input", "default");
            return Task.FromResult(ToolResult.Success($"{_returnValue}: {input}"));
        }
    }

    public class BaseToolTests
    {
        private readonly Mock<ILogger<BaseTool>> _mockLogger;

        public BaseToolTests()
        {
            _mockLogger = new Mock<ILogger<BaseTool>>();
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestTool(null!));
        }

        [Fact]
        public void Name_ShouldReturnCorrectValue()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);

            // Act & Assert
            Assert.Equal("test_tool", tool.Name);
        }

        [Fact]
        public void Description_ShouldReturnCorrectValue()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);

            // Act & Assert
            Assert.Equal("A test tool for unit testing", tool.Description);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnSuccessResult()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object, returnValue: "test_result");
            var parameters = new Dictionary<string, object>
            {
                ["input"] = "test_input"
            };

            // Act
            var result = await tool.ExecuteAsync(parameters);

            // Assert
            Assert.True(result.IsSuccess());
            Assert.Equal("test_result: test_input", result.Result());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldReturnFailureResult_WhenExceptionThrown()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object, shouldThrowException: true);
            var parameters = new Dictionary<string, object>
            {
                ["input"] = "test_input"
            };

            // Act
            var result = await tool.ExecuteAsync(parameters);

            // Assert
            Assert.False(result.IsSuccess());
            Assert.Contains("Test exception", result.ErrorMessage());
        }

        [Fact]
        public async Task ExecuteAsync_ShouldHandleNullParameters()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);

            // Act
            var result = await tool.ExecuteAsync(null);

            // Assert
            Assert.True(result.IsSuccess());
            Assert.Equal("success: default", result.Result());
        }

        [Fact]
        public void ToParameter_ShouldReturnCorrectFormat()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);

            // Act
            var parameter = tool.ToParameter();

            // Assert
            Assert.Equal("function", parameter["type"]);

            var function = parameter["function"] as Dictionary<string, object>;
            Assert.NotNull(function);
            Assert.Equal("test_tool", function["name"]);
            Assert.Equal("A test tool for unit testing", function["description"]);
            Assert.NotNull(function["parameters"]);
        }

        [Fact]
        public async Task CleanupAsync_ShouldCompleteSuccessfully()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);

            // Act & Assert
            await tool.CleanupAsync(); // Should not throw
        }

        [Fact]
        public void GetParameter_ShouldReturnCorrectValue()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);
            var parameters = new Dictionary<string, object>
            {
                ["string_param"] = "string_value",
                ["int_param"] = 42,
                ["bool_param"] = true
            };

            // Act & Assert
            Assert.Equal("string_value", tool.GetParameter(parameters, "string_param", "default"));
            Assert.Equal(42, tool.GetParameter(parameters, "int_param", 0));
            Assert.True(tool.GetParameter(parameters, "bool_param", false));
        }

        [Fact]
        public void GetParameter_ShouldReturnDefaultValue_WhenParameterNotFound()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);
            var parameters = new Dictionary<string, object>();

            // Act & Assert
            Assert.Equal("default", tool.GetParameter(parameters, "missing_param", "default"));
            Assert.Equal(0, tool.GetParameter(parameters, "missing_param", 0));
        }

        [Fact]
        public void GetParameter_ShouldReturnDefaultValue_WhenParametersIsNull()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);

            // Act & Assert
            Assert.Equal("default", tool.GetParameter<string>(null, "any_param", "default"));
        }

        [Fact]
        public void ValidateRequiredParameters_ShouldNotThrow_WhenAllParametersPresent()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);
            var parameters = new Dictionary<string, object>
            {
                ["param1"] = "value1",
                ["param2"] = "value2"
            };

            // Act & Assert
            tool.ValidateRequiredParameters(parameters, "param1", "param2"); // Should not throw
        }

        [Fact]
        public void ValidateRequiredParameters_ShouldThrow_WhenParameterMissing()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);
            var parameters = new Dictionary<string, object>
            {
                ["param1"] = "value1"
            };

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                tool.ValidateRequiredParameters(parameters, "param1", "missing_param"));
        }

        [Fact]
        public void ValidateRequiredParameters_ShouldThrow_WhenParametersIsNull()
        {
            // Arrange
            var tool = new TestTool(_mockLogger.Object);

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                tool.ValidateRequiredParameters(null, "param1"));
        }
    }
}
