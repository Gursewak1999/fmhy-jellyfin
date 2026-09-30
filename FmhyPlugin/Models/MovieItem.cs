using System;
using System.Collections.Generic;

namespace FmhyPlugin.Models
{
    /// <summary>
    /// Represents a movie item from an FMHY source.
    /// </summary>
    public class MovieItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string OriginalTitle { get; set; }
        public string Overview { get; set; }
        public string Year { get; set; }
        public string Rating { get; set; }
        public string PosterUrl { get; set; }
        public string BackdropUrl { get; set; }
        public string StreamUrl { get; set; }
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string Quality { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public List<string> Cast { get; set; } = new List<string>();
        public string Duration { get; set; }
        public string Language { get; set; }
        public string ImdbId { get; set; }
        public List<StreamLink> StreamLinks { get; set; } = new List<StreamLink>();
        public DateTime DateAdded { get; set; }
        public string Category { get; set; }
    }

    /// <summary>
    /// Represents a stream link with quality options.
    /// </summary>
    public class StreamLink
    {
        public string Url { get; set; }
        public string Quality { get; set; }
        public string Format { get; set; }
        public string Language { get; set; }
        public bool IsDirectPlay { get; set; }
    }

    /// <summary>
    /// Represents a search result from FMHY sources.
    /// </summary>
    public class SearchResult
    {
        public List<MovieItem> Movies { get; set; } = new List<MovieItem>();
        public int TotalResults { get; set; }
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Represents a category/genre from FMHY sources.
    /// </summary>
    public class CategoryItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ItemCount { get; set; }
    }
}
