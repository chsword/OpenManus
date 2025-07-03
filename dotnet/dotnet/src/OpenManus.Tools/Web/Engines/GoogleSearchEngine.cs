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
    /// Google Custom Search Engine implementation.
    /// Requires Google Custom Search API key and Search Engine ID.
    /// </summary>
    public class GoogleSearchEngine : IWebSearchEngine, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleSearchEngine> _logger;
        private readonly string _apiKey;
        private readonly string _searchEngineId;
        private bool _disposed = false;

        public string Name => "Google";

        public GoogleSearchEngine(HttpClient httpClient, ILogger<GoogleSearchEngine> logger,
            string apiKey, string searchEngineId)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _searchEngineId = searchEngineId ?? throw new ArgumentNullException(nameof(searchEngineId));
        }

        public async Task<List<SearchItem>> PerformSearchAsync(string query, int numResults = 10, string language = "en", string country = "us")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException("Query cannot be null or empty", nameof(query));
            }

            if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(_searchEngineId))
            {
                _logger.LogWarning("Google Search API key or Search Engine ID not configured");
                return new List<SearchItem>();
            }

            try
            {
                _logger.LogInformation("Performing Google search for query: {Query}, numResults: {NumResults}", query, numResults);

                var results = new List<SearchItem>();
                var batchSize = Math.Min(10, numResults); // Google API limit is 10 per request
                var startIndex = 1;

                while (results.Count < numResults && startIndex <= 91) // Google limit is 100 results total
                {
                    var batchResults = await PerformBatchSearchAsync(query, batchSize, startIndex, language, country);
                    if (batchResults.Count == 0)
                        break;

                    results.AddRange(batchResults);
                    startIndex += batchSize;
                }

                // Trim to requested number of results
                if (results.Count > numResults)
                {
                    results = results.Take(numResults).ToList();
                }

                // Assign positions and source
                for (int i = 0; i < results.Count; i++)
                {
                    results[i].Position = i + 1;
                    results[i].Source = Name;
                }

                _logger.LogInformation("Google search completed. Found {ResultCount} results", results.Count);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing Google search for query: {Query}", query);
                throw;
            }
        }

        private async Task<List<SearchItem>> PerformBatchSearchAsync(string query, int num, int start, string language, string country)
        {
            var encodedQuery = HttpUtility.UrlEncode(query);
            var url = $"https://www.googleapis.com/customsearch/v1" +
                     $"?key={_apiKey}" +
                     $"&cx={_searchEngineId}" +
                     $"&q={encodedQuery}" +
                     $"&num={num}" +
                     $"&start={start}" +
                     $"&lr=lang_{language}" +
                     $"&gl={country}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;

            var results = new List<SearchItem>();

            if (root.TryGetProperty("items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in items.EnumerateArray())
                {
                    var title = item.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "" : "";
                    var link = item.TryGetProperty("link", out var linkProp) ? linkProp.GetString() ?? "" : "";
                    var snippet = item.TryGetProperty("snippet", out var snippetProp) ? snippetProp.GetString() ?? "" : "";

                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(link))
                    {
                        results.Add(new SearchItem
                        {
                            Title = title,
                            Url = link,
                            Description = snippet,
                            Source = Name
                        });
                    }
                }
            }

            return results;
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
