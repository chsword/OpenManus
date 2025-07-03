using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenManus.Tools.Web.Models;

namespace OpenManus.Tools.Web.Engines
{
    /// <summary>
    /// Base interface for web search engines.
    /// </summary>
    public interface IWebSearchEngine
    {
        /// <summary>
        /// The name of this search engine.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Perform a web search and return search results.
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="numResults">Number of results to return</param>
        /// <param name="language">Language code (e.g., "en")</param>
        /// <param name="country">Country code (e.g., "us")</param>
        /// <returns>List of search items</returns>
        Task<List<SearchItem>> PerformSearchAsync(string query, int numResults = 10, string language = "en", string country = "us");
    }
}
