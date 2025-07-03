using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using OpenManus.Tools.Web.Models;

namespace OpenManus.Tools.Web.Engines
{
    /// <summary>
    /// DuckDuckGo search engine implementation.
    /// Uses DuckDuckGo's Instant Answer API.
    /// </summary>
    public class DuckDuckGoSearchEngine : IWebSearchEngine, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DuckDuckGoSearchEngine> _logger;
        private bool _disposed = false;

        public string Name => "DuckDuckGo";

        public DuckDuckGoSearchEngine(HttpClient httpClient, ILogger<DuckDuckGoSearchEngine> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Set user agent to mimic a real browser
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        public async Task<List<SearchItem>> PerformSearchAsync(string query, int numResults = 10, string language = "en", string country = "us")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be null or empty", nameof(query));
            }

            try
            {
                _logger.LogInformation("Performing DuckDuckGo search for query: {Query}, numResults: {NumResults}", query, numResults);

                var results = new List<SearchItem>();

                // Try multiple approaches with DuckDuckGo

                // 1. Try Instant Answer API first
                var instantResults = await TryInstantAnswerApiAsync(query, language, country);
                if (instantResults.Count > 0)
                {
                    results.AddRange(instantResults.Take(numResults));
                }

                // 2. If we need more results, try HTML scraping (fallback)
                if (results.Count < numResults)
                {
                    var htmlResults = await TryHtmlScrapingAsync(query, numResults - results.Count, language, country);
                    results.AddRange(htmlResults);
                }

                // Assign positions
                for (int i = 0; i < results.Count; i++)
                {
                    results[i].Position = i + 1;
                    results[i].Source = Name;
                }

                _logger.LogInformation("DuckDuckGo search completed. Found {ResultCount} results", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing DuckDuckGo search for query: {Query}", query);
                throw;
            }
        }

        private async Task<List<SearchItem>> TryInstantAnswerApiAsync(string query, string language, string country)
        {
            try
            {
                // DuckDuckGo Instant Answer API
                var encodedQuery = HttpUtility.UrlEncode(query);
                var url = $"https://api.duckduckgo.com/?q={encodedQuery}&format=json&no_html=1&skip_disambig=1";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(content);
                var root = jsonDoc.RootElement;

                var results = new List<SearchItem>();

                // Check for instant answer
                if (root.TryGetProperty("AbstractText", out var abstractText) && !string.IsNullOrEmpty(abstractText.GetString()))
                {
                    if (root.TryGetProperty("AbstractURL", out var abstractUrl) && !string.IsNullOrEmpty(abstractUrl.GetString()))
                    {
                        results.Add(new SearchItem
                        {
                            Title = root.TryGetProperty("Heading", out var heading) ? heading.GetString() ?? query : query,
                            Url = abstractUrl.GetString()!,
                            Description = abstractText.GetString(),
                            Source = Name
                        });
                    }
                }

                // Check for related topics
                if (root.TryGetProperty("RelatedTopics", out var relatedTopics) && relatedTopics.ValueKind == JsonValueKind.Array)
                {
                    foreach (var topic in relatedTopics.EnumerateArray())
                    {
                        if (topic.TryGetProperty("Text", out var text) &&
                            topic.TryGetProperty("FirstURL", out var firstUrl) &&
                            !string.IsNullOrEmpty(text.GetString()) &&
                            !string.IsNullOrEmpty(firstUrl.GetString()))
                        {
                            var topicText = text.GetString()!;
                            var dashIndex = topicText.IndexOf(" - ");
                            var title = dashIndex > 0 ? topicText.Substring(0, dashIndex) : topicText;
                            var description = dashIndex > 0 ? topicText.Substring(dashIndex + 3) : "";

                            results.Add(new SearchItem
                            {
                                Title = title,
                                Url = firstUrl.GetString()!,
                                Description = description,
                                Source = Name
                            });
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to use DuckDuckGo Instant Answer API");
                return new List<SearchItem>();
            }
        }

        private async Task<List<SearchItem>> TryHtmlScrapingAsync(string query, int numResults, string language, string country)
        {
            try
            {
                // Note: HTML scraping of search results is more complex and may be blocked
                // This is a simplified implementation
                var encodedQuery = HttpUtility.UrlEncode(query);
                var url = $"https://html.duckduckgo.com/html/?q={encodedQuery}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // This would require HTML parsing library like HtmlAgilityPack
                // For now, return empty list as DuckDuckGo blocks most scraping attempts
                _logger.LogWarning("HTML scraping not implemented - DuckDuckGo blocks most scraping attempts");
                return new List<SearchItem>();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to scrape DuckDuckGo HTML results");
                return new List<SearchItem>();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // HttpClient is typically managed by DI container, so we don't dispose it here
                _disposed = true;
            }
        }
    }
}
