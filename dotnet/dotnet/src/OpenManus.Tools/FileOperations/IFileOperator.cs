using System;
using System.Threading.Tasks;

namespace OpenManus.Tools.FileOperations
{
    /// <summary>
    /// Interface for file operations in different environments (local/sandbox).
    /// </summary>
    public interface IFileOperator
    {
        /// <summary>
        /// Read content from a file.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <returns>File content as string</returns>
        Task<string> ReadFileAsync(string path);

        /// <summary>
        /// Write content to a file.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="content">Content to write</param>
        Task WriteFileAsync(string path, string content);

        /// <summary>
        /// Check if path points to a directory.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if path is a directory</returns>
        Task<bool> IsDirectoryAsync(string path);

        /// <summary>
        /// Check if path exists.
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if path exists</returns>
        Task<bool> ExistsAsync(string path);

        /// <summary>
        /// Run a shell command and return result.
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="timeoutSeconds">Command timeout in seconds</param>
        /// <returns>Tuple of (exitCode, stdout, stderr)</returns>
        Task<(int exitCode, string stdout, string stderr)> RunCommandAsync(string command, int timeoutSeconds = 120);
    }
}
