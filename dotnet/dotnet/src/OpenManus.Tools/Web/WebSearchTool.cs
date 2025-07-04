using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenManus.Tools.Web.Engines;
using OpenManus.Tools.Web.Models;
using OpenManus.Core.Models;

namespace OpenManus.Tools.Web
{
    /// <summary>
    /// Web search tool that supports multiple search engines.
    /// Provides web search capabilities with fallback options and content fetching.
    /// </summary>
    [Description("Web search tool for finding information on the internet")]
    public class WebSearchTool : BaseTool
    {
        private readonly WebSearchEngineFactory _engineFactory;
        private readonly HttpClient _httpClient;
        private readonly List<string> _fallbackEngines;

        public override string Name => "web_search";
        public override string Description => "Search the web for information using various search engines";

        public WebSearchTool(
            ILogger<BaseTool> logger,
            HttpClient httpClient,
            ILoggerFactory loggerFactory,
            IConfiguration configuration) : base(logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _engineFactory = new WebSearchEngineFactory(httpClient, loggerFactory, configuration);

            // Default fallback order: DuckDuckGo first (no API key required), then Google
            _fallbackEngines = new List<string> { "duckduckgo", "google" };
        }

        /// <summary>
        /// Perform a web search using the specified query and options.
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="numResults">Number of results to return (default: 10, max: 50)</param>
        /// <param name="language">Language code for search results (default: "en")</param>
        /// <param name="country">Country code for search results (default: "us")</param>
        /// <param name="preferredEngine">Preferred search engine ("duckduckgo", "google", or null for auto)</param>
        /// <param name="fetchContent">Whether to fetch content from result URLs (default: false)</param>
        /// <returns>Search results formatted as string</returns>
        [KernelFunction]
        [Description("Search the web for information")]
        public async Task<ToolResult> SearchAsync(
            [Description("The search query")] string query,
            [Description("Number of results to return (1-50)")] int numResults = 10,
            [Description("Language code (e.g., 'en', 'zh', 'ja')")] string language = "en",
            [Description("Country code (e.g., 'us', 'cn', 'jp')")] string country = "us",
            [Description("Preferred search engine ('duckduckgo', 'google', or leave empty for auto)")] string? preferredEngine = null,
            [Description("Whether to fetch content from result URLs")] bool fetchContent = false)
        {
            try
            {
                _logger.LogInformation("Starting web search with query: {Query}", query);

                if (string.IsNullOrWhiteSpace(query))
                {
                    return ToolResult.Failure("Search query cannot be empty");
                }

                // Validate and clamp numResults
                numResults = Math.Max(1, Math.Min(50, numResults));

                var stopwatch = Stopwatch.StartNew();
                var searchResponse = await PerformSearchWithFallbackAsync(
                    query, numResults, language, country, preferredEngine);

                stopwatch.Stop();

                if (!searchResponse.IsSuccess)
                {
                    return ToolResult.Failure($"Search failed: {searchResponse.ErrorMessage}");
                }

                // Fetch content from URLs if requested
                if (fetchContent && searchResponse.Results.Count > 0)
                {
                    await FetchContentForResultsAsync(searchResponse.Results);
                }

                // Update metadata
                if (searchResponse.Metadata != null)
                {
                    searchResponse.Metadata.SearchDuration = stopwatch.Elapsed;
                }

                var formattedResult = searchResponse.ToFormattedString();

                _logger.LogInformation("Web search completed successfully. Found {ResultCount} results in {Duration}ms",
                    searchResponse.Results.Count, stopwatch.ElapsedMilliseconds);

                return ToolResult.Success(formattedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing web search for query: {Query}", query);
                return ToolResult.Failure($"Search error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get information about available search engines.
        /// </summary>
        [KernelFunction]
        [Description("Get information about available search engines")]
        public Task<ToolResult> GetAvailableEnginesAsync()
        {
            try
            {
                var engines = _engineFactory.GetAvailableEngines();
                var engineInfo = engines.Select(e => new
                {
                    Name = e.Name,
                    Id = e.Id,
                    IsConfigured = e.IsConfigured,
                    RequiresApiKey = e.RequiresApiKey,
                    Description = e.Description
                }).ToList();

                var result = JsonSerializer.Serialize(engineInfo, new JsonSerializerOptions { WriteIndented = true });

                return Task.FromResult(ToolResult.Success(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available search engines");
                return Task.FromResult(ToolResult.Failure($"Error getting engines: {ex.Message}"));
            }
        }

        /// <summary>
        /// Core execution method required by BaseTool
        /// </summary>
        protected override async Task<ToolResult> ExecuteCoreAsync(Dictionary<string, object>? parameters)
        {
            var query = GetParameter<string>(parameters, "query", "");
            var numResults = GetParameter<int>(parameters, "numResults", 10);
            var language = GetParameter<string>(parameters, "language", "en");
            var country = GetParameter<string>(parameters, "country", "us");
            var preferredEngine = GetParameter<string?>(parameters, "preferredEngine", null);
            var fetchContent = GetParameter<bool>(parameters, "fetchContent", false);

            return await SearchAsync(query, numResults, language, country, preferredEngine, fetchContent);
        }

        private async Task<SearchResponse> PerformSearchWithFallbackAsync(
            string query, int numResults, string language, string country, string? preferredEngine)
        {
            var engines = preferredEngine != null
                ? new List<string> { preferredEngine }.Concat(_fallbackEngines).Distinct()
                : _fallbackEngines;

            SearchResponse? lastResponse = null;
            Exception? lastException = null;

            foreach (var engineName in engines)
            {
                try
                {
                    _logger.LogDebug("Trying search engine: {EngineName}", engineName);

                    var engine = _engineFactory.CreateSearchEngine(engineName);
                    var results = await engine.PerformSearchAsync(query, numResults, language, country);

                    if (results.Count > 0)
                    {
                        return new SearchResponse
                        {
                            Query = query,
                            Results = results,
                            IsSuccess = true,
                            Metadata = new SearchMetadata
                            {
                                TotalResults = results.Count,
                                Language = language,
                                Country = country,
                                SearchEngine = engine.Name
                            }
                        };
                    }

                    lastResponse = new SearchResponse
                    {
                        Query = query,
                        Results = new List<SearchItem>(),
                        IsSuccess = false,
                        ErrorMessage = $"No results found using {engine.Name}",
                        Metadata = new SearchMetadata
                        {
                            SearchEngine = engine.Name,
                            Language = language,
                            Country = country
                        }
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Search engine {EngineName} failed", engineName);
                    lastException = ex;
                    lastResponse = new SearchResponse
                    {
                        Query = query,
                        Results = new List<SearchItem>(),
                        IsSuccess = false,
                        ErrorMessage = $"Error with {engineName}: {ex.Message}"
                    };
                }
            }

            // If we get here, all engines failed
            return lastResponse ?? new SearchResponse
            {
                Query = query,
                Results = new List<SearchItem>(),
                IsSuccess = false,
                ErrorMessage = lastException?.Message ?? "All search engines failed"
            };
        }

        private async Task FetchContentForResultsAsync(List<SearchItem> results)
        {
            var tasks = results.Select(async result =>
            {
                try
                {
                    _logger.LogDebug("Fetching content from: {Url}", result.Url);

                    var response = await _httpClient.GetAsync(result.Url);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        // Basic content extraction (remove HTML tags, get text content)
                        var textContent = ExtractTextFromHtml(content);
                        result.RawContent = textContent.Length > 5000
                            ? textContent.Substring(0, 5000) + "..."
                            : textContent;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch content from {Url}", result.Url);
                    // Continue without content - don't fail the entire search
                }
            });

            await Task.WhenAll(tasks);
        }

        private static string ExtractTextFromHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // Simple HTML tag removal (for basic content extraction)
            // In production, you might want to use HtmlAgilityPack
            var text = Regex.Replace(html, @"<[^>]+>", " ");
            text = Regex.Replace(text, @"\s+", " ");
            return text.Trim();
        }
    }
}
