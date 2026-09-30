using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Plugins;
using FmhyPlugin.Models;
using FmhyPlugin.Services;
using FmhyPlugin.Configuration;

namespace FmhyPlugin.Controllers
{
    /// <summary>
    /// API controller for FMHY Free Movies plugin.
    /// </summary>
    [ApiController]
    [Route("FmhyPlugin")]
    public class FmhyController : ControllerBase
    {
        private readonly FmhySourceService _sourceService;

        public FmhyController(FmhySourceService sourceService)
        {
            _sourceService = sourceService;
        }

        /// <summary>
        /// Gets all available streaming sources.
        /// </summary>
        [HttpGet("Sources")]
        public ActionResult<List<StreamingSource>> GetSources()
        {
            return Ok(_sourceService.GetAllSources());
        }

        /// <summary>
        /// Gets enabled streaming sources.
        /// </summary>
        [HttpGet("Sources/Enabled")]
        public ActionResult<List<StreamingSource>> GetEnabledSources()
        {
            return Ok(_sourceService.GetEnabledSources());
        }

        /// <summary>
        /// Searches for movies across all enabled sources.
        /// </summary>
        [HttpGet("Search")]
        public async Task<ActionResult<SearchResult>> SearchMovies(string query, int page = 1, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required");
            }

            var result = await _sourceService.SearchMoviesAsync(query, page, pageSize);
            return Ok(result);
        }

        /// <summary>
        /// Gets featured/trending movies.
        /// </summary>
        [HttpGet("Featured")]
        public async Task<ActionResult<List<MovieItem>>> GetFeaturedMovies()
        {
            var movies = await _sourceService.GetFeaturedMoviesAsync();
            return Ok(movies);
        }

        /// <summary>
        /// Gets movies by category.
        /// </summary>
        [HttpGet("Category/{category}")]
        public async Task<ActionResult<List<MovieItem>>> GetMoviesByCategory(string category)
        {
            var movies = await _sourceService.GetMoviesByCategoryAsync(category);
            return Ok(movies);
        }

        /// <summary>
        /// Gets all available categories.
        /// </summary>
        [HttpGet("Categories")]
        public ActionResult<List<CategoryItem>> GetCategories()
        {
            return Ok(_sourceService.GetCategories());
        }

        /// <summary>
        /// Gets stream links for a specific movie.
        /// </summary>
        [HttpGet("StreamLinks/{sourceId}/{movieId}")]
        public async Task<ActionResult<List<StreamLink>>> GetStreamLinks(string sourceId, string movieId)
        {
            var links = await _sourceService.GetStreamLinksAsync(sourceId, movieId);
            return Ok(links);
        }

        /// <summary>
        /// Gets plugin configuration.
        /// </summary>
        [HttpGet("Configuration")]
        public ActionResult<FmhyPluginConfiguration> GetConfiguration()
        {
            return Ok(FmhyPlugin.Instance?.Configuration);
        }
    }
}