using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using OpenManus.Tools.Plugins;
using Xunit;

namespace OpenManus.Tools.Tests.Plugins
{
    public class FileSystemPluginTests : IDisposable
    {
        private readonly Mock<ILogger<FileSystemPlugin>> _mockLogger;
        private readonly FileSystemPlugin _plugin;
        private readonly string _testDirectory;
        private readonly string _testFilePath;

        public FileSystemPluginTests()
        {
            _mockLogger = new Mock<ILogger<FileSystemPlugin>>();
            _plugin = new FileSystemPlugin(_mockLogger.Object);

            // Create a temporary test directory
            _testDirectory = Path.Combine(Path.GetTempPath(), "OpenManus_FileSystemPlugin_Tests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testDirectory);
            _testFilePath = Path.Combine(_testDirectory, "test.txt");
        }

        public void Dispose()
        {
            // Clean up test directory
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }

        [Fact]
        public async Task WriteFileAsync_ShouldCreateFileWithContent()
        {
            // Arrange
            var content = "Hello, World!";

            // Act
            var result = await _plugin.WriteFileAsync(_testFilePath, content);

            // Assert
            Assert.Contains("Successfully wrote", result);
            Assert.True(File.Exists(_testFilePath));
            var fileContent = await File.ReadAllTextAsync(_testFilePath);
            Assert.Equal(content, fileContent);
        }

        [Fact]
        public async Task WriteFileAsync_ShouldCreateDirectoryIfNotExists()
        {
            // Arrange
            var nestedPath = Path.Combine(_testDirectory, "nested", "folder", "file.txt");
            var content = "Test content";

            // Act
            var result = await _plugin.WriteFileAsync(nestedPath, content);

            // Assert
            Assert.Contains("Successfully wrote", result);
            Assert.True(File.Exists(nestedPath));
            var fileContent = await File.ReadAllTextAsync(nestedPath);
            Assert.Equal(content, fileContent);
        }

        [Fact]
        public async Task ReadFileAsync_ShouldReturnFileContent()
        {
            // Arrange
            var content = "Test file content\nLine 2";
            await File.WriteAllTextAsync(_testFilePath, content);

            // Act
            var result = await _plugin.ReadFileAsync(_testFilePath);

            // Assert
            Assert.Equal(content, result);
        }

        [Fact]
        public async Task ReadFileAsync_ShouldReturnErrorForNonExistentFile()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.txt");

            // Act
            var result = await _plugin.ReadFileAsync(nonExistentPath);

            // Assert
            Assert.Contains("Error: File not found", result);
        }

        [Fact]
        public void ListDirectory_ShouldReturnDirectoryContents()
        {
            // Arrange
            var file1 = Path.Combine(_testDirectory, "file1.txt");
            var file2 = Path.Combine(_testDirectory, "file2.txt");
            var subDir = Path.Combine(_testDirectory, "subdir");

            File.WriteAllText(file1, "content1");
            File.WriteAllText(file2, "content2");
            Directory.CreateDirectory(subDir);

            // Act
            var result = _plugin.ListDirectory(_testDirectory);

            // Assert
            Assert.Contains("Contents of", result);
            Assert.Contains("file1.txt", result);
            Assert.Contains("file2.txt", result);
            Assert.Contains("subdir", result);
        }

        [Fact]
        public void ListDirectory_ShouldReturnEmptyMessage()
        {
            // Arrange
            var emptyDir = Path.Combine(_testDirectory, "empty");
            Directory.CreateDirectory(emptyDir);

            // Act
            var result = _plugin.ListDirectory(emptyDir);

            // Assert
            Assert.Contains("is empty", result);
        }

        [Fact]
        public void ListDirectory_ShouldReturnErrorForNonExistentDirectory()
        {
            // Arrange
            var nonExistentPath = Path.Combine(_testDirectory, "nonexistent");

            // Act
            var result = _plugin.ListDirectory(nonExistentPath);

            // Assert
            Assert.Contains("Error: Directory not found", result);
        }

        [Fact]
        public async Task WriteFileAsync_ShouldOverwriteExistingFile()
        {
            // Arrange
            var initialContent = "Initial content";
            var newContent = "New content";
            await File.WriteAllTextAsync(_testFilePath, initialContent);

            // Act
            var result = await _plugin.WriteFileAsync(_testFilePath, newContent);

            // Assert
            Assert.Contains("Successfully wrote", result);
            var fileContent = await File.ReadAllTextAsync(_testFilePath);
            Assert.Equal(newContent, fileContent);
        }

        [Fact]
        public async Task WriteFileAsync_ShouldHandleEmptyContent()
        {
            // Arrange
            var emptyContent = string.Empty;

            // Act
            var result = await _plugin.WriteFileAsync(_testFilePath, emptyContent);

            // Assert
            Assert.Contains("Successfully wrote 0 characters", result);
            Assert.True(File.Exists(_testFilePath));
            var fileContent = await File.ReadAllTextAsync(_testFilePath);
            Assert.Equal(emptyContent, fileContent);
        }
    }
}
