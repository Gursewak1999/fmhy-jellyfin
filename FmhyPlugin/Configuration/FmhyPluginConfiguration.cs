using System;
using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace FmhyPlugin.Configuration
{
    /// <summary>
    /// Configuration for the FMHY Free Movies plugin.
    /// </summary>
    public class FmhyPluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or sets the list of enabled streaming sources.
        /// </summary>
        public List<StreamingSource> EnabledSources { get; set; } = new List<StreamingSource>();

        /// <summary>
        /// Gets or sets the cache duration in minutes for source data.
        /// </summary>
        public int CacheDurationMinutes { get; set; } = 60;

        /// <summary>
        /// Gets or sets whether to show adult content sources.
        /// </summary>
        public bool ShowAdultContent { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum number of results per source.
        /// </summary>
        public int MaxResultsPerSource { get; set; } = 50;

        /// <summary>
        /// Gets or sets whether to enable auto-play next episode.
        /// </summary>
        public bool AutoPlayNext { get; set; } = true;

        /// <summary>
        /// Gets or sets the preferred quality (4K, 1080p, 720p, 480p).
        /// </summary>
        public string PreferredQuality { get; set; } = "1080p";

        /// <summary>
        /// Gets or sets the user agent for HTTP requests.
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

        /// <summary>
        /// Gets or sets the request timeout in seconds.
        /// </summary>
        public int RequestTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets whether to use a proxy for streaming.
        /// </summary>
        public bool UseProxy { get; set; } = false;

        /// <summary>
        /// Gets or sets the proxy URL.
        /// </summary>
        public string ProxyUrl { get; set; } = "";

        /// <summary>
        /// Gets or sets the default language for content.
        /// </summary>
        public string DefaultLanguage { get; set; } = "en";

        /// <summary>
        /// Gets or sets whether to include anime content.
        /// </summary>
        public bool IncludeAnime { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include TV shows.
        /// </summary>
        public bool IncludeTVShows { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to include documentaries.
        /// </summary>
        public bool IncludeDocumentaries { get; set; } = true;
    }

    /// <summary>
    /// Represents a streaming source from fmhy.net.
    /// </summary>
    public class StreamingSource
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string Quality { get; set; }
        public bool RequiresAdblock { get; set; }
        public bool HasAutoNext { get; set; }
        public string Language { get; set; } = "en";
    }
}
