using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OpenManus.Core.Models;
using OpenManus.Tools;
using Xunit;

namespace OpenManus.Tools.Tests
{
    public class ToolManagerTests
    {
        private readonly Mock<ILogger<ToolManager>> _mockLogger;
        private readonly ToolManager _toolManager;
        private readonly Mock<ILogger<BaseTool>> _mockToolLogger;

        public ToolManagerTests()
        {
            _mockLogger = new Mock<ILogger<ToolManager>>();
            _mockToolLogger = new Mock<ILogger<BaseTool>>();
            _toolManager = new ToolManager(_mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new ToolManager(null!));
        }

        [Fact]
        public void RegisterTool_ShouldAddToolToCollection()
        {
            // Arrange
            var tool = new TestTool(_mockToolLogger.Object);

            // Act
            _toolManager.RegisterTool(tool);

            // Assert
            Assert.Equal(1, _toolManager.ToolCount);
            Assert.Contains("test_tool", _toolManager.GetAvailableTools());
        }

        [Fact]
        public void RegisterTool_ShouldThrowArgumentNullException_WhenToolIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _toolManager.RegisterTool(null!));
        }

        [Fact]
        public void RegisterTool_ShouldOverwriteExistingTool()
        {
            // Arrange
            var tool1 = new TestTool(_mockToolLogger.Object, returnValue: "first");
            var tool2 = new TestTool(_mockToolLogger.Object, returnValue: "second");

            // Act
            _toolManager.RegisterTool(tool1);
            _toolManager.RegisterTool(tool2);

            // Assert
            Assert.Equal(1, _toolManager.ToolCount);
            var retrievedTool = _toolManager.GetTool("test_tool");
            Assert.NotNull(retrievedTool);
        }

        [Fact]
        public void UnregisterTool_ShouldRemoveToolFromCollection()
        {
            // Arrange
            var tool = new TestTool(_mockToolLogger.Object);
            _toolManager.RegisterTool(tool);

            // Act
            _toolManager.UnregisterTool("test_tool");

            // Assert
            Assert.Equal(0, _toolManager.ToolCount);
            Assert.DoesNotContain("test_tool", _toolManager.GetAvailableTools());
        }

        [Fact]
        public void UnregisterTool_ShouldHandleNonExistentTool()
        {
            // Act & Assert
            _toolManager.UnregisterTool("nonexistent_tool"); // Should not throw
            Assert.Equal(0, _toolManager.ToolCount);
        }

        [Fact]
        public void GetTool_ShouldReturnRegisteredTool()
        {
            // Arrange
            var tool = new TestTool(_mockToolLogger.Object);
            _toolManager.RegisterTool(tool);

            // Act
            var retrievedTool = _toolManager.GetTool("test_tool");

            // Assert
            Assert.NotNull(retrievedTool);
            Assert.Equal("test_tool", retrievedTool.Name);
        }

        [Fact]
        public void GetTool_ShouldReturnNull_WhenToolNotFound()
        {
            // Act
            var retrievedTool = _toolManager.GetTool("nonexistent_tool");

            // Assert
            Assert.Null(retrievedTool);
        }

        [Fact]
        public void GetAvailableTools_ShouldReturnAllToolNames()
        {
            // Arrange
            var tool1 = new TestTool(_mockToolLogger.Object);
            var tool2 = new TestTool2(_mockToolLogger.Object, "test_tool_2");

            _toolManager.RegisterTool(tool1);
            _toolManager.RegisterTool(tool2);

            // Act
            var toolNames = _toolManager.GetAvailableTools().ToList();

            // Assert
            Assert.Equal(2, toolNames.Count);
            Assert.Contains("test_tool", toolNames);
            Assert.Contains("test_tool_2", toolNames);
        }

        [Fact]
        public void GetToolDefinitions_ShouldReturnCorrectFormat()
        {
            // Arrange
            var tool = new TestTool(_mockToolLogger.Object);
            _toolManager.RegisterTool(tool);

            // Act
            var definitions = _toolManager.GetToolDefinitions();

            // Assert
            Assert.Single(definitions);
            var definition = definitions.First();
            Assert.Equal("function", definition.Type);
            Assert.NotNull(definition.Function);
            Assert.Equal("test_tool", definition.Function.Name);
        }

        [Fact]
        public async Task ExecuteToolAsync_ShouldReturnSuccessResult()
        {
            // Arrange
            var tool = new TestTool(_mockToolLogger.Object, returnValue: "execution_result");
            _toolManager.RegisterTool(tool);
            var parameters = new Dictionary<string, object>
            {
                ["input"] = "test_input"
            };

            // Act
            var result = await _toolManager.ExecuteToolAsync("test_tool", parameters);

            // Assert
            Assert.True(result.IsSuccess());
            Assert.Contains("execution_result: test_input", result.Result());
        }

        [Fact]
        public async Task ExecuteToolAsync_ShouldReturnFailure_WhenToolNotFound()
        {
            // Act
            var result = await _toolManager.ExecuteToolAsync("nonexistent_tool");

            // Assert
            Assert.False(result.IsSuccess());
            Assert.Contains("Tool not found", result.ErrorMessage());
        }

        [Fact]
        public async Task ExecuteToolAsync_ShouldReturnFailure_WhenToolNameIsEmpty()
        {
            // Act
            var result = await _toolManager.ExecuteToolAsync("");

            // Assert
            Assert.False(result.IsSuccess());
            Assert.Contains("Tool name cannot be empty", result.ErrorMessage());
        }

        [Fact]
        public async Task ExecuteToolAsync_ShouldHandleToolException()
        {
            // Arrange
            var tool = new TestTool(_mockToolLogger.Object, shouldThrowException: true);
            _toolManager.RegisterTool(tool);

            // Act
            var result = await _toolManager.ExecuteToolAsync("test_tool");

            // Assert
            Assert.False(result.IsSuccess());
            Assert.Contains("Tool 'test_tool' encountered an error:", result.ErrorMessage());
        }

        [Fact]
        public void HasTool_ShouldReturnTrue_WhenToolExists()
        {
            // Arrange
            var tool = new TestTool(_mockToolLogger.Object);
            _toolManager.RegisterTool(tool);

            // Act & Assert
            Assert.True(_toolManager.HasTool("test_tool"));
        }

        [Fact]
        public void HasTool_ShouldReturnFalse_WhenToolDoesNotExist()
        {
            // Act & Assert
            Assert.False(_toolManager.HasTool("nonexistent_tool"));
        }

        [Fact]
        public void HasTool_ShouldReturnFalse_WhenToolNameIsNull()
        {
            // Act & Assert
            Assert.False(_toolManager.HasTool(null!));
        }

        [Fact]
        public void GetToolInfo_ShouldReturnCorrectInfo()
        {
            // Arrange
            var tool = new TestTool(_mockToolLogger.Object);
            _toolManager.RegisterTool(tool);

            // Act
            var toolInfo = _toolManager.GetToolInfo("test_tool");

            // Assert
            Assert.NotNull(toolInfo);
            Assert.Equal("test_tool", toolInfo.Name);
            Assert.Equal("A test tool for unit testing", toolInfo.Description);
            Assert.True(toolInfo.IsAvailable);
        }

        [Fact]
        public void GetToolInfo_ShouldReturnNull_WhenToolNotFound()
        {
            // Act
            var toolInfo = _toolManager.GetToolInfo("nonexistent_tool");

            // Assert
            Assert.Null(toolInfo);
        }

        [Fact]
        public void RegisterTools_ShouldAddMultipleTools()
        {
            // Arrange
            var tools = new[]
            {
                new TestTool(_mockToolLogger.Object),
                new TestTool2(_mockToolLogger.Object, "test_tool_2"),
                new TestTool2(_mockToolLogger.Object, "test_tool_3")
            };

            // Act
            _toolManager.RegisterTools(tools);

            // Assert
            Assert.Equal(3, _toolManager.ToolCount);
        }

        [Fact]
        public void ClearTools_ShouldRemoveAllTools()
        {
            // Arrange
            var tool1 = new TestTool(_mockToolLogger.Object);
            var tool2 = new TestTool2(_mockToolLogger.Object, "test_tool_2");
            _toolManager.RegisterTool(tool1);
            _toolManager.RegisterTool(tool2);

            // Act
            _toolManager.ClearTools();

            // Assert
            Assert.Equal(0, _toolManager.ToolCount);
            Assert.Empty(_toolManager.GetAvailableTools());
        }
    }

    // Extended TestTool to support name override for testing
    public class TestTool2 : TestTool
    {
        private readonly string _name;

        public TestTool2(ILogger<BaseTool> logger, string name = "test_tool_2") : base(logger)
        {
            _name = name;
        }

        public override string Name => _name;
    }
}
