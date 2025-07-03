using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenManus.Core.Models;
using OpenManus.Tools.FileOperations;

namespace OpenManus.Tools.FileSystem
{
    /// <summary>
    /// File editing commands supported by StrReplaceEditorTool.
    /// </summary>
    public enum EditorCommand
    {
        View,
        Create,
        StrReplace,
        Insert,
        UndoEdit
    }

    /// <summary>
    /// A tool for viewing, creating, and editing files with support for various operations.
    /// Equivalent to Python's StrReplaceEditor tool.
    /// </summary>
    public class StrReplaceEditorTool : BaseTool
    {
        private const int SnippetLines = 4;
        private const int MaxResponseLength = 16000;
        private const string TruncatedMessage =
            "<response clipped><NOTE>To save on context only part of this file has been shown to you. " +
            "You should retry this tool after you have searched inside the file with `grep -n` " +
            "in order to find the line numbers of what you are looking for.</NOTE>";

        private const string ToolDescription =
            "Custom editing tool for viewing, creating and editing files\n" +
            "* State is persistent across command calls and discussions with the user\n" +
            "* If `path` is a file, `view` displays the result of applying `cat -n`. If `path` is a directory, `view` lists non-hidden files and directories up to 2 levels deep\n" +
            "* The `create` command cannot be used if the specified `path` already exists as a file\n" +
            "* If a `command` generates a long output, it will be truncated and marked with `<response clipped>`\n" +
            "* The `undo_edit` command will revert the last edit made to the file at `path`\n\n" +
            "Notes for using the `str_replace` command:\n" +
            "* The `old_str` parameter should match EXACTLY one or more consecutive lines from the original file. Be mindful of whitespaces!\n" +
            "* If the `old_str` parameter is not unique in the file, the replacement will not be performed. Make sure to include enough context in `old_str` to make it unique\n" +
            "* The `new_str` parameter should contain the edited lines that should replace the `old_str`";

        private readonly IFileOperator _fileOperator;
        private readonly Dictionary<string, List<string>> _fileHistory;

        public StrReplaceEditorTool(IFileOperator fileOperator, ILogger<BaseTool> logger) : base(logger)
        {
            _fileOperator = fileOperator ?? throw new ArgumentNullException(nameof(fileOperator));
            _fileHistory = new Dictionary<string, List<string>>();

            Parameters = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["command"] = new Dictionary<string, object>
                    {
                        ["description"] = "The commands to run. Allowed options are: `view`, `create`, `str_replace`, `insert`, `undo_edit`.",
                        ["enum"] = new[] { "view", "create", "str_replace", "insert", "undo_edit" },
                        ["type"] = "string"
                    },
                    ["path"] = new Dictionary<string, object>
                    {
                        ["description"] = "Absolute path to file or directory.",
                        ["type"] = "string"
                    },
                    ["file_text"] = new Dictionary<string, object>
                    {
                        ["description"] = "Required parameter of `create` command, with the content of the file to be created.",
                        ["type"] = "string"
                    },
                    ["old_str"] = new Dictionary<string, object>
                    {
                        ["description"] = "Required parameter of `str_replace` command containing the string in `path` to replace.",
                        ["type"] = "string"
                    },
                    ["new_str"] = new Dictionary<string, object>
                    {
                        ["description"] = "Optional parameter of `str_replace` command containing the new string (if not given, no string will be added). Required parameter of `insert` command containing the string to insert.",
                        ["type"] = "string"
                    },
                    ["insert_line"] = new Dictionary<string, object>
                    {
                        ["description"] = "Required parameter of `insert` command. The `new_str` will be inserted AFTER the line `insert_line` of `path`.",
                        ["type"] = "integer"
                    },
                    ["view_range"] = new Dictionary<string, object>
                    {
                        ["description"] = "Optional parameter of `view` command when `path` points to a file. If none is given, the full file is shown. If provided, the file will be shown in the indicated line number range, e.g. [11, 12] will show lines 11 and 12. Indexing at 1 to start. Setting `[start_line, -1]` shows all lines from `start_line` to the end of the file.",
                        ["type"] = "array",
                        ["items"] = new Dictionary<string, object> { ["type"] = "integer" }
                    }
                },
                ["required"] = new[] { "command", "path" }
            };
        }

        /// <inheritdoc />
        public override string Name => "str_replace_editor";

        /// <inheritdoc />
        public override string Description => ToolDescription;

        /// <inheritdoc />
        protected override async Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters)
        {
            ValidateRequiredParameters(parameters, "command", "path");

            var commandStr = GetParameter<string>(parameters, "command");
            var path = GetParameter<string>(parameters, "path");

            if (!Enum.TryParse<EditorCommand>(commandStr, true, out var command))
            {
                throw new ArgumentException($"Unrecognized command {commandStr}. The allowed commands are: view, create, str_replace, insert, undo_edit");
            }

            // Validate path and command combination
            await ValidatePathAsync(command, path);

            // Execute the appropriate command
            string result = command switch
            {
                EditorCommand.View => await ViewAsync(path, parameters),
                EditorCommand.Create => await CreateAsync(path, parameters),
                EditorCommand.StrReplace => await StrReplaceAsync(path, parameters),
                EditorCommand.Insert => await InsertAsync(path, parameters),
                EditorCommand.UndoEdit => await UndoEditAsync(path),
                _ => throw new ArgumentException($"Unrecognized command: {command}")
            };

            return ToolResult.Success(result);
        }

        /// <summary>
        /// Validates path and command combination.
        /// </summary>
        private async Task ValidatePathAsync(EditorCommand command, string path)
        {
            // Check if path is absolute
            if (!Path.IsPathFullyQualified(path))
            {
                throw new ArgumentException($"The path {path} is not an absolute path");
            }

            // Only check if path exists for non-create commands
            if (command != EditorCommand.Create)
            {
                if (!await _fileOperator.ExistsAsync(path))
                {
                    throw new FileNotFoundException($"The path {path} does not exist. Please provide a valid path.");
                }

                // Check if path is a directory
                var isDirectory = await _fileOperator.IsDirectoryAsync(path);
                if (isDirectory && command != EditorCommand.View)
                {
                    throw new ArgumentException($"The path {path} is a directory and only the `view` command can be used on directories");
                }
            }
            // Check if file exists for create command
            else if (command == EditorCommand.Create)
            {
                var exists = await _fileOperator.ExistsAsync(path);
                if (exists)
                {
                    throw new InvalidOperationException($"File already exists at: {path}. Cannot overwrite files using command `create`.");
                }
            }
        }

        /// <summary>
        /// View file or directory content.
        /// </summary>
        private async Task<string> ViewAsync(string path, Dictionary<string, object>? parameters)
        {
            var isDirectory = await _fileOperator.IsDirectoryAsync(path);

            if (isDirectory)
            {
                // Directory handling
                var viewRange = GetParameter<int[]>(parameters, "view_range");
                if (viewRange != null)
                {
                    throw new ArgumentException("The `view_range` parameter is not allowed when `path` points to a directory.");
                }

                return await ViewDirectoryAsync(path);
            }
            else
            {
                // File handling
                var viewRange = GetParameter<int[]>(parameters, "view_range");
                return await ViewFileAsync(path, viewRange);
            }
        }

        /// <summary>
        /// View directory contents up to 2 levels deep.
        /// </summary>
        private async Task<string> ViewDirectoryAsync(string path)
        {
            try
            {
                // Use PowerShell command to list directory contents (Windows equivalent of find)
                var command = $"Get-ChildItem -Path '{path}' -Recurse -Depth 1 | Where-Object {{ !$_.Name.StartsWith('.') }} | ForEach-Object {{ $_.FullName }}";

                var (exitCode, stdout, stderr) = await _fileOperator.RunCommandAsync(command);

                if (!string.IsNullOrEmpty(stderr))
                {
                    return $"Error listing directory contents: {stderr}";
                }

                var result = $"Here's the files and directories up to 2 levels deep in {path}, excluding hidden items:\n{stdout}\n";
                return MaybeTruncate(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing directory: {Path}", path);
                return $"Error viewing directory: {ex.Message}";
            }
        }

        /// <summary>
        /// View file content, optionally within a specified line range.
        /// </summary>
        private async Task<string> ViewFileAsync(string path, int[]? viewRange)
        {
            var fileContent = await _fileOperator.ReadFileAsync(path);
            var initLine = 1;

            // Apply view range if specified
            if (viewRange != null)
            {
                if (viewRange.Length != 2 || viewRange.Any(i => i == 0))
                {
                    throw new ArgumentException("Invalid `view_range`. It should be a list of two integers.");
                }

                var fileLines = fileContent.Split('\n');
                var nLinesFile = fileLines.Length;
                initLine = viewRange[0];
                var finalLine = viewRange[1];

                // Validate view range
                if (initLine < 1 || initLine > nLinesFile)
                {
                    throw new ArgumentException($"Invalid `view_range`: [{initLine}, {finalLine}]. Its first element `{initLine}` should be within the range of lines of the file: [1, {nLinesFile}]");
                }
                if (finalLine > nLinesFile)
                {
                    throw new ArgumentException($"Invalid `view_range`: [{initLine}, {finalLine}]. Its second element `{finalLine}` should be smaller than the number of lines in the file: `{nLinesFile}`");
                }
                if (finalLine != -1 && finalLine < initLine)
                {
                    throw new ArgumentException($"Invalid `view_range`: [{initLine}, {finalLine}]. Its second element `{finalLine}` should be larger or equal than its first `{initLine}`");
                }

                // Apply range
                if (finalLine == -1)
                {
                    fileContent = string.Join("\n", fileLines.Skip(initLine - 1));
                }
                else
                {
                    fileContent = string.Join("\n", fileLines.Skip(initLine - 1).Take(finalLine - initLine + 1));
                }
            }

            return MakeOutput(fileContent, path, initLine);
        }

        /// <summary>
        /// Create a new file with specified content.
        /// </summary>
        private async Task<string> CreateAsync(string path, Dictionary<string, object>? parameters)
        {
            var fileText = GetParameter<string>(parameters, "file_text");
            if (string.IsNullOrEmpty(fileText))
            {
                throw new ArgumentException("Parameter `file_text` is required for command: create");
            }

            await _fileOperator.WriteFileAsync(path, fileText);

            // Save to history
            if (!_fileHistory.ContainsKey(path))
            {
                _fileHistory[path] = new List<string>();
            }
            _fileHistory[path].Add(fileText);

            return $"File created successfully at: {path}";
        }

        /// <summary>
        /// Replace a unique string in a file with a new string.
        /// </summary>
        private async Task<string> StrReplaceAsync(string path, Dictionary<string, object>? parameters)
        {
            var oldStr = GetParameter<string>(parameters, "old_str");
            if (string.IsNullOrEmpty(oldStr))
            {
                throw new ArgumentException("Parameter `old_str` is required for command: str_replace");
            }

            var newStr = GetParameter<string>(parameters, "new_str") ?? "";

            // Read file content and expand tabs
            var fileContent = (await _fileOperator.ReadFileAsync(path)).Replace("\t", "    ");
            oldStr = oldStr.Replace("\t", "    ");
            newStr = newStr.Replace("\t", "    ");

            // Check if old_str is unique in the file
            var occurrences = CountOccurrences(fileContent, oldStr);
            if (occurrences == 0)
            {
                throw new ArgumentException($"No replacement was performed, old_str `{oldStr}` did not appear verbatim in {path}.");
            }
            else if (occurrences > 1)
            {
                // Find line numbers of occurrences
                var fileLines = fileContent.Split('\n');
                var lines = new List<int>();
                for (int i = 0; i < fileLines.Length; i++)
                {
                    if (fileLines[i].Contains(oldStr))
                    {
                        lines.Add(i + 1);
                    }
                }
                throw new ArgumentException($"No replacement was performed. Multiple occurrences of old_str `{oldStr}` in lines {string.Join(", ", lines)}. Please ensure it is unique");
            }

            // Replace old_str with new_str
            var newFileContent = fileContent.Replace(oldStr, newStr);

            // Write the new content to the file
            await _fileOperator.WriteFileAsync(path, newFileContent);

            // Save the original content to history
            if (!_fileHistory.ContainsKey(path))
            {
                _fileHistory[path] = new List<string>();
            }
            _fileHistory[path].Add(fileContent);

            // Create a snippet of the edited section
            var replacementLine = fileContent.Split(oldStr)[0].Count(c => c == '\n');
            var startLine = Math.Max(0, replacementLine - SnippetLines);
            var endLine = replacementLine + SnippetLines + newStr.Count(c => c == '\n');
            var newFileLines = newFileContent.Split('\n');
            var snippet = string.Join("\n", newFileLines.Skip(startLine).Take(endLine - startLine + 1));

            // Prepare the success message
            var successMsg = $"The file {path} has been edited. ";
            successMsg += MakeOutput(snippet, $"a snippet of {path}", startLine + 1);
            successMsg += "Review the changes and make sure they are as expected. Edit the file again if necessary.";

            return successMsg;
        }

        /// <summary>
        /// Insert text at a specific line in a file.
        /// </summary>
        private async Task<string> InsertAsync(string path, Dictionary<string, object>? parameters)
        {
            var insertLineObj = GetParameter<object>(parameters, "insert_line");
            if (insertLineObj == null)
            {
                throw new ArgumentException("Parameter `insert_line` is required for command: insert");
            }

            if (!int.TryParse(insertLineObj.ToString(), out var insertLine))
            {
                throw new ArgumentException("Parameter `insert_line` must be an integer");
            }

            var newStr = GetParameter<string>(parameters, "new_str");
            if (string.IsNullOrEmpty(newStr))
            {
                throw new ArgumentException("Parameter `new_str` is required for command: insert");
            }

            // Read and prepare content
            var fileText = (await _fileOperator.ReadFileAsync(path)).Replace("\t", "    ");
            newStr = newStr.Replace("\t", "    ");
            var fileTextLines = fileText.Split('\n').ToList();
            var nLinesFile = fileTextLines.Count;

            // Validate insert_line
            if (insertLine < 0 || insertLine > nLinesFile)
            {
                throw new ArgumentException($"Invalid `insert_line` parameter: {insertLine}. It should be within the range of lines of the file: [0, {nLinesFile}]");
            }

            // Perform insertion
            var newStrLines = newStr.Split('\n');
            var newFileTextLines = new List<string>();
            newFileTextLines.AddRange(fileTextLines.Take(insertLine));
            newFileTextLines.AddRange(newStrLines);
            newFileTextLines.AddRange(fileTextLines.Skip(insertLine));

            // Create a snippet for preview
            var snippetLines = new List<string>();
            snippetLines.AddRange(fileTextLines.Skip(Math.Max(0, insertLine - SnippetLines)).Take(Math.Min(SnippetLines, insertLine)));
            snippetLines.AddRange(newStrLines);
            snippetLines.AddRange(fileTextLines.Skip(insertLine).Take(SnippetLines));

            // Join lines and write to file
            var newFileText = string.Join("\n", newFileTextLines);
            var snippet = string.Join("\n", snippetLines);

            await _fileOperator.WriteFileAsync(path, newFileText);

            // Save to history
            if (!_fileHistory.ContainsKey(path))
            {
                _fileHistory[path] = new List<string>();
            }
            _fileHistory[path].Add(fileText);

            // Prepare success message
            var successMsg = $"The file {path} has been edited. ";
            successMsg += MakeOutput(snippet, "a snippet of the edited file", Math.Max(1, insertLine - SnippetLines + 1));
            successMsg += "Review the changes and make sure they are as expected (correct indentation, no duplicate lines, etc). Edit the file again if necessary.";

            return successMsg;
        }

        /// <summary>
        /// Revert the last edit made to a file.
        /// </summary>
        private async Task<string> UndoEditAsync(string path)
        {
            if (!_fileHistory.ContainsKey(path) || !_fileHistory[path].Any())
            {
                throw new InvalidOperationException($"No edit history found for {path}.");
            }

            var oldText = _fileHistory[path].Last();
            _fileHistory[path].RemoveAt(_fileHistory[path].Count - 1);

            await _fileOperator.WriteFileAsync(path, oldText);

            return $"Last edit to {path} undone successfully. {MakeOutput(oldText, path)}";
        }

        /// <summary>
        /// Format file content for display with line numbers.
        /// </summary>
        private string MakeOutput(string fileContent, string fileDescriptor, int initLine = 1, bool expandTabs = true)
        {
            fileContent = MaybeTruncate(fileContent);

            if (expandTabs)
            {
                fileContent = fileContent.Replace("\t", "    ");
            }

            // Add line numbers to each line
            var lines = fileContent.Split('\n');
            var numberedLines = lines.Select((line, index) => $"{index + initLine,6}\t{line}");
            var numberedContent = string.Join("\n", numberedLines);

            return $"Here's the result of running `cat -n` on {fileDescriptor}:\n{numberedContent}\n";
        }

        /// <summary>
        /// Truncate content and append a notice if content exceeds the specified length.
        /// </summary>
        private static string MaybeTruncate(string content, int truncateAfter = MaxResponseLength)
        {
            if (content.Length <= truncateAfter)
            {
                return content;
            }
            return content.Substring(0, truncateAfter) + TruncatedMessage;
        }

        /// <summary>
        /// Count occurrences of a substring in a string.
        /// </summary>
        private static int CountOccurrences(string text, string pattern)
        {
            var count = 0;
            var index = 0;
            while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += pattern.Length;
            }
            return count;
        }
    }
}
