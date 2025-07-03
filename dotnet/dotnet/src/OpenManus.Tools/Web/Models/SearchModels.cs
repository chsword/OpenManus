using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OpenManus.Tools.Web.Models
{
    /// <summary>
    /// Represents a single search result item.
    /// </summary>
    public class SearchItem
    {
        /// <summary>
        /// The title of the search result.
        /// </summary>
        [Required]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The URL of the search result.
        /// </summary>
        [Required]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// A description or snippet of the search result.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The search engine that provided this result.
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Position in search results (1-based).
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Raw content from the search result page if fetched.
        /// </summary>
        public string? RawContent { get; set; }

        public override string ToString()
        {
            return $"{Title} ({Url})";
        }
    }

    /// <summary>
    /// Metadata about the search operation.
    /// </summary>
    public class SearchMetadata
    {
        /// <summary>
        /// Total number of results found.
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// Language code used for the search.
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Country code used for the search.
        /// </summary>
        public string Country { get; set; } = "us";

        /// <summary>
        /// Search engine used.
        /// </summary>
        public string SearchEngine { get; set; } = string.Empty;

        /// <summary>
        /// Time taken for the search operation.
        /// </summary>
        public TimeSpan SearchDuration { get; set; }
    }

    /// <summary>
    /// Structured response from the web search tool.
    /// </summary>
    public class SearchResponse
    {
        /// <summary>
        /// The search query that was executed.
        /// </summary>
        public string Query { get; set; } = string.Empty;

        /// <summary>
        /// List of search results.
        /// </summary>
        public List<SearchItem> Results { get; set; } = new();

        /// <summary>
        /// Metadata about the search.
        /// </summary>
        public SearchMetadata? Metadata { get; set; }

        /// <summary>
        /// Whether the search was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if the search failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Convert search response to formatted string output.
        /// </summary>
        public string ToFormattedString()
        {
            if (!IsSuccess)
            {
                return $"Search failed: {ErrorMessage}";
            }

            var resultText = new List<string> { $"Search results for '{Query}':" };

            for (int i = 0; i < Results.Count; i++)
            {
                var result = Results[i];
                
                // Add title with position number
                var title = !string.IsNullOrWhiteSpace(result.Title) ? result.Title : "No title";
                resultText.Add($"\n{i + 1}. {title}");

                // Add URL with proper indentation
                resultText.Add($"   URL: {result.Url}");

                // Add description if available
                if (!string.IsNullOrWhiteSpace(result.Description))
                {
                    resultText.Add($"   Description: {result.Description}");
                }

                // Add content preview if available
                if (!string.IsNullOrWhiteSpace(result.RawContent))
                {
                    var contentPreview = result.RawContent.Length > 1000 
                        ? result.RawContent.Substring(0, 1000) + "..."
                        : result.RawContent;
                    contentPreview = contentPreview.Replace("\n", " ").Trim();
                    resultText.Add($"   Content: {contentPreview}");
                }
            }

            // Add metadata if available
            if (Metadata != null)
            {
                resultText.AddRange(new[]
                {
                    "\nMetadata:",
                    $"- Total results: {Metadata.TotalResults}",
                    $"- Language: {Metadata.Language}",
                    $"- Country: {Metadata.Country}",
                    $"- Search engine: {Metadata.SearchEngine}",
                    $"- Duration: {Metadata.SearchDuration.TotalMilliseconds:F0}ms"
                });
            }

            return string.Join("\n", resultText);
        }
    }
}
