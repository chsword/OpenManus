using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenManus.Core.Models;
using OpenManus.Tools.FileOperations;

namespace OpenManus.Tools.FileSystem
{
    /// <summary>
    /// Tool for file operations like reading, writing, and managing files.
    /// Provides file system interaction capabilities for agents.
    /// </summary>
    public class FileOperationsTool : BaseTool
    {
        private readonly IFileOperator _fileOperator;

        public FileOperationsTool(IFileOperator fileOperator, ILogger<BaseTool> logger) : base(logger)
        {
            _fileOperator = fileOperator ?? throw new ArgumentNullException(nameof(fileOperator));

            Parameters = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["operation"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["enum"] = new[] { "read", "write", "exists", "is_directory", "list_directory" },
                        ["description"] = "The file operation to perform"
                    },
                    ["path"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "File or directory path"
                    },
                    ["content"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Content to write (only used for 'write' operation)"
                    },
                    ["encoding"] = new Dictionary<string, object>
                    {
                        ["type"] = "string",
                        ["description"] = "Text encoding (default: utf-8)",
                        ["default"] = "utf-8"
                    }
                },
                ["required"] = new[] { "operation", "path" }
            };
        }

        /// <inheritdoc />
        public override string Name => "file_operations";

        /// <inheritdoc />
        public override string Description =>
            "Perform file system operations including reading files, writing files, checking file existence, " +
            "and listing directory contents. Use this tool to interact with the file system safely.";

        /// <inheritdoc />
        protected override async Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters)
        {
            var operation = GetParameter(parameters, "operation", string.Empty);
            var path = GetParameter(parameters, "path", string.Empty);

            if (string.IsNullOrWhiteSpace(operation))
            {
                return ToolResult.Failure("Operation parameter is required");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return ToolResult.Failure("Path parameter is required");
            }

            _logger.LogInformation("📁 Executing file operation: {Operation} on path: {Path}", operation, path);

            try
            {
                switch (operation.ToLowerInvariant())
                {
                    case "read":
                        return await ExecuteReadAsync(path);

                    case "write":
                        var content = GetParameter(parameters, "content", string.Empty);
                        return await ExecuteWriteAsync(path, content);

                    case "exists":
                        return await ExecuteExistsAsync(path);

                    case "is_directory":
                        return await ExecuteIsDirectoryAsync(path);

                    case "list_directory":
                        return await ExecuteListDirectoryAsync(path);

                    default:
                        return ToolResult.Failure($"Unknown operation: {operation}. Supported operations: read, write, exists, is_directory, list_directory");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "🔒 Access denied for file operation on {Path}", path);
                return ToolResult.Failure($"Access denied: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogError(ex, "📂 Directory not found: {Path}", path);
                return ToolResult.Failure($"Directory not found: {ex.Message}");
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "📄 File not found: {Path}", path);
                return ToolResult.Failure($"File not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error during file operation");
                return ToolResult.Failure($"File operation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Execute file read operation
        /// </summary>
        private async Task<ToolResult> ExecuteReadAsync(string path)
        {
            try
            {
                var content = await _fileOperator.ReadFileAsync(path);
                _logger.LogInformation("✅ Successfully read file: {Path} ({Length} characters)", path, content.Length);

                var result = new
                {
                    Operation = "read",
                    Path = path,
                    Content = content,
                    Size = content.Length,
                    Success = true
                };

                return ToolResult.Success($"File read successfully:\nPath: {path}\nSize: {content.Length} characters\n\nContent:\n{content}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to read file: {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Execute file write operation
        /// </summary>
        private async Task<ToolResult> ExecuteWriteAsync(string path, string content)
        {
            try
            {
                await _fileOperator.WriteFileAsync(path, content);
                _logger.LogInformation("✅ Successfully wrote file: {Path} ({Length} characters)", path, content.Length);

                var result = new
                {
                    Operation = "write",
                    Path = path,
                    Size = content.Length,
                    Success = true
                };

                return ToolResult.Success($"File written successfully:\nPath: {path}\nSize: {content.Length} characters");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to write file: {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Execute file existence check
        /// </summary>
        private async Task<ToolResult> ExecuteExistsAsync(string path)
        {
            try
            {
                var exists = await _fileOperator.ExistsAsync(path);
                _logger.LogInformation("✅ Checked existence of: {Path} - {Exists}", path, exists ? "EXISTS" : "NOT EXISTS");

                var result = new
                {
                    Operation = "exists",
                    Path = path,
                    Exists = exists,
                    Success = true
                };

                return ToolResult.Success($"Path existence check:\nPath: {path}\nExists: {exists}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to check existence of: {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Execute directory check operation
        /// </summary>
        private async Task<ToolResult> ExecuteIsDirectoryAsync(string path)
        {
            try
            {
                var isDirectory = await _fileOperator.IsDirectoryAsync(path);
                _logger.LogInformation("✅ Checked if directory: {Path} - {IsDirectory}", path, isDirectory ? "IS DIRECTORY" : "NOT DIRECTORY");

                var result = new
                {
                    Operation = "is_directory",
                    Path = path,
                    IsDirectory = isDirectory,
                    Success = true
                };

                return ToolResult.Success($"Directory check:\nPath: {path}\nIs Directory: {isDirectory}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to check if directory: {Path}", path);
                throw;
            }
        }

        /// <summary>
        /// Execute directory listing operation
        /// </summary>
        private async Task<ToolResult> ExecuteListDirectoryAsync(string path)
        {
            try
            {
                // Check if path is a directory first
                var isDirectory = await _fileOperator.IsDirectoryAsync(path);
                if (!isDirectory)
                {
                    return ToolResult.Failure($"Path is not a directory: {path}");
                }

                // For now, we'll use a simple directory listing approach
                // In the future, we could add this method to IFileOperator
                var directoryInfo = new DirectoryInfo(path);
                var entries = directoryInfo.GetFileSystemInfos();

                var listing = string.Join("\n", entries.Select(entry =>
                {
                    var type = entry is DirectoryInfo ? "DIR" : "FILE";
                    var size = entry is FileInfo fileInfo ? $" ({fileInfo.Length} bytes)" : "";
                    return $"{type,-4} {entry.Name}{size}";
                }));

                _logger.LogInformation("✅ Listed directory: {Path} ({Count} entries)", path, entries.Length);

                var result = new
                {
                    Operation = "list_directory",
                    Path = path,
                    EntryCount = entries.Length,
                    Entries = entries.Select(e => new { Name = e.Name, Type = e is DirectoryInfo ? "directory" : "file" }),
                    Success = true
                };

                return ToolResult.Success($"Directory listing:\nPath: {path}\nEntries: {entries.Length}\n\n{listing}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Failed to list directory: {Path}", path);
                throw;
            }
        }
    }
}
