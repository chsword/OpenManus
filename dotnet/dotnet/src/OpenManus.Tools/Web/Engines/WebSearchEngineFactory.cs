using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OpenManus.Tools.Web.Engines
{
    /// <summary>
    /// Factory for creating web search engine instances.
    /// </summary>
    public class WebSearchEngineFactory
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IConfiguration _configuration;

        public WebSearchEngineFactory(HttpClient httpClient, ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Create a search engine by name.
        /// </summary>
        /// <param name="engineName">Name of the search engine (e.g., "duckduckgo", "google")</param>
        /// <returns>IWebSearchEngine instance</returns>
        public IWebSearchEngine CreateSearchEngine(string engineName)
        {
            return engineName?.ToLowerInvariant() switch
            {
                "duckduckgo" or "ddg" => new DuckDuckGoSearchEngine(
                    _httpClient,
                    _loggerFactory.CreateLogger<DuckDuckGoSearchEngine>()),

                "google" => CreateGoogleSearchEngine(),

                _ => new DuckDuckGoSearchEngine(
                    _httpClient,
                    _loggerFactory.CreateLogger<DuckDuckGoSearchEngine>()) // Default to DuckDuckGo
            };
        }

        /// <summary>
        /// Get list of available search engines.
        /// </summary>
        /// <returns>List of available search engines with their configuration status</returns>
        public List<EngineInfo> GetAvailableEngines()
        {
            var engines = new List<EngineInfo>
            {
                new EngineInfo
                {
                    Name = "DuckDuckGo",
                    Id = "duckduckgo",
                    IsConfigured = true,
                    RequiresApiKey = false,
                    Description = "Privacy-focused search engine with instant answers API"
                }
            };

            // Check Google configuration
            var googleApiKey = _configuration["WebSearch:Google:ApiKey"];
            var googleSearchEngineId = _configuration["WebSearch:Google:SearchEngineId"];
            var isGoogleConfigured = !string.IsNullOrWhiteSpace(googleApiKey) &&
                                   !string.IsNullOrWhiteSpace(googleSearchEngineId);

            engines.Add(new EngineInfo
            {
                Name = "Google",
                Id = "google",
                IsConfigured = isGoogleConfigured,
                RequiresApiKey = true,
                Description = "Google Custom Search API (requires API key and Search Engine ID)"
            });

            return engines;
        }

        private GoogleSearchEngine CreateGoogleSearchEngine()
        {
            var apiKey = _configuration["WebSearch:Google:ApiKey"] ?? "";
            var searchEngineId = _configuration["WebSearch:Google:SearchEngineId"] ?? "";

            return new GoogleSearchEngine(
                _httpClient,
                _loggerFactory.CreateLogger<GoogleSearchEngine>(),
                apiKey,
                searchEngineId);
        }
    }

    /// <summary>
    /// Information about a search engine.
    /// </summary>
    public class EngineInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public bool IsConfigured { get; set; }
        public bool RequiresApiKey { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
