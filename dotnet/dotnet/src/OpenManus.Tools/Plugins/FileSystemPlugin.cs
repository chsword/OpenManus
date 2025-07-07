using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenManus.Core.Models;

namespace OpenManus.Tools.Plugins
{
    /// <summary>
    /// A Semantic Kernel plugin for performing file system operations.
    /// </summary>
    public class FileSystemPlugin
    {
        private readonly ILogger<FileSystemPlugin> _logger;

        public FileSystemPlugin(ILogger<FileSystemPlugin> logger)
        {
            _logger = logger;
        }

        [KernelFunction("ReadFile"), Description("Reads the entire content of a specified file.")]
        public async Task<string> ReadFileAsync(
            [Description("The absolute path of the file to read.")] string path)
        {
            _logger.LogInformation("Reading file: {Path}", path);
            if (!File.Exists(path))
            {
                _logger.LogWarning("File not found: {Path}", path);
                return $"Error: File not found at '{path}'.";
            }
            try
            {
                return await File.ReadAllTextAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file: {Path}", path);
                return $"Error reading file: {ex.Message}";
            }
        }

        [KernelFunction("WriteFile"), Description("Writes the given content to a specified file. This will create the file if it does not exist, or overwrite it if it does.")]
        public async Task<string> WriteFileAsync(
            [Description("The absolute path of the file to write to.")] string path,
            [Description("The content to write to the file.")] string content)
        {
            _logger.LogInformation("Writing to file: {Path}", path);
            try
            {
                var directory = Path.GetDirectoryName(path);
                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await File.WriteAllTextAsync(path, content);
                return $"Successfully wrote {content.Length} characters to '{path}'.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing to file: {Path}", path);
                return $"Error writing file: {ex.Message}";
            }
        }

        [KernelFunction("ListDirectory"), Description("Lists the files and subdirectories in a specified directory.")]
        public string ListDirectory(
            [Description("The absolute path of the directory to list the contents of.")] string path)
        {
            _logger.LogInformation("Listing directory: {Path}", path);
            if (!Directory.Exists(path))
            {
                _logger.LogWarning("Directory not found: {Path}", path);
                return $"Error: Directory not found at '{path}'.";
            }
            try
            {
                var entries = Directory.GetFileSystemEntries(path)
                    .Select(Path.GetFileName)
                    .ToList();

                if (!entries.Any())
                {
                    return $"Directory '{path}' is empty.";
                }

                return $"Contents of '{path}':\n" + string.Join("\n", entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing directory: {Path}", path);
                return $"Error listing directory: {ex.Message}";
            }
        }
    }
}
