# FMHY Free Movies Jellyfin Plugin

A Jellyfin plugin that fetches free movie content from sources defined in [fmhy.net](https://fmhy.net) and allows users to stream them directly in Jellyfin.

## Features

- **200+ Streaming Sources**: Curated list from fmhy.net including stream aggregators, P-Stream forks, dedicated servers, multi-server sites, legal free-with-ads services, video platforms, and classics/public domain archives
- **Categories**: Browse by Stream Aggregators, P-Stream Forks, Dedicated-Server, Multi-Server, Free w/ Ads, Video Streaming, Classics/Public Domain
- **Search**: Search across all enabled sources simultaneously
- **Featured Movies**: Trending/popular movies from top sources
- **Quality Selection**: Multiple quality options (4K, 1080p, 720p, 480p)
- **Auto-Next**: Automatic next episode playback for supported sources
- **Configuration**: Enable/disable sources, set preferences, configure proxy
- **Responsive UI**: Works on desktop, tablet, and mobile

## Source Categories

| Category | Count | Description |
|----------|-------|-------------|
| Stream Aggregators | 50+ | Sites with custom players (Movy, Cinejoy, PopcornMovies, etc.) |
| P-Stream Forks | 13 | movie-web/P-Stream forks with extra sources (Cinecat, Aether, etc.) |
| Dedicated-Server | 30 | Single player/server focus (Atlantic, NEPU, EE3, etc.) |
| Multi-Server | 50+ | Multiple player choices (1Shows, AniCine, HydraHD, etc.) |
| Free w/ Ads (Legal) | 20 | Legal free streaming (Tubi, Plex, Pluto, Kanopy, etc.) |
| Video Streaming | 10 | General platforms (YouTube, Vimeo, Dailymotion, etc.) |
| Classics/Public Domain | 100+ | Archives (Internet Archive, BFI, Library of Congress, etc.) |

## Installation

### Method 1: Manual Installation (Recommended)

1. Download the latest release `FmhyPlugin.zip`
2. Extract to your Jellyfin plugins directory:
   - **Linux**: `/var/lib/jellyfin/plugins/`
   - **Windows**: `C:\ProgramData\Jellyfin\Server\plugins\`
   - **Docker**: `/config/plugins/`
3. Restart Jellyfin
4. Go to **Dashboard → Plugins → FMHY Free Movies** to configure

### Method 2: Repository Installation

Add this repository to Jellyfin's plugin repositories:
```
https://github.com/your-repo/fmhy-jellyfin-plugin/releases/latest
```

## Configuration

1. Navigate to **Dashboard → Plugins → FMHY Free Movies**
2. **General Settings**:
   - Enable/disable adult content, anime, TV shows, documentaries
   - Set preferred quality (4K, 1080p, 720p, 480p)
   - Configure cache duration, request timeout, user agent
   - Set up proxy if needed
3. **Streaming Sources**:
   - Enable/disable individual sources
   - Use "Enable Recommended" for a curated list of reliable sources
   - Sources are grouped by category for easy management

## Usage

### Browse Movies
- Go to **Plugins → FMHY Browse** in Jellyfin web UI
- Browse featured movies on the home page
- Filter by category using the category buttons
- Search for specific movies using the search box

### Stream a Movie
1. Click on any movie card to open details
2. Select a stream link (multiple qualities available)
3. Click "Play" to start streaming in Jellyfin's player

## Supported Clients

- ✅ Jellyfin Web
- ✅ Jellyfin Android TV / Fire TV
- ✅ Jellyfin iOS / Android
- ✅ Jellyfin Desktop (Electron)
- ✅ Jellyfin Kodi Addon
- ✅ Jellyfin Roku (limited)

## Requirements

- Jellyfin 10.9.0 or later
- .NET 8.0 Runtime
- Internet connection for fetching content

## Legal Disclaimer

This plugin only provides links to publicly available streaming sources listed on fmhy.net. The plugin does not host any content. Users are responsible for complying with their local laws regarding streaming content. The plugin author is not responsible for any content accessed through this plugin.

Some sources may require:
- Ad blocker (uBlock Origin recommended)
- VPN for geo-restricted content
- Account registration

## Building from Source

```bash
# Clone the repository
git clone https://github.com/your-repo/fmhy-jellyfin-plugin.git
cd fmhy-jellyfin-plugin/FmhyPlugin

# Build
dotnet build -c Release

# Output: bin/Release/net8.0/FmhyPlugin.dll
```

## Plugin Structure

```
FmhyPlugin/
├── Controllers/
│   └── FmhyController.cs      # API endpoints
├── Configuration/
│   └── FmhyPluginConfiguration.cs  # Plugin settings
├── Models/
│   └── MovieItem.cs           # Data models
├── Services/
│   └── FmhySourceService.cs   # Source management & scraping
├── Web/
│   ├── configuration.html     # Admin configuration page
│   └── browse.html            # User browse page
├── FmhyPlugin.cs              # Main plugin class
├── FmhyPlugin.csproj          # Project file
└── plugin.json                # Plugin manifest
```

## API Endpoints

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/FmhyPlugin/Sources` | GET | All available sources |
| `/FmhyPlugin/Sources/Enabled` | GET | Enabled sources only |
| `/FmhyPlugin/Search` | GET | Search movies (query, page, pageSize) |
| `/FmhyPlugin/Featured` | GET | Featured/trending movies |
| `/FmhyPlugin/Category/{category}` | GET | Movies by category |
| `/FmhyPlugin/Categories` | GET | Available categories |
| `/FmhyPlugin/StreamLinks/{sourceId}/{movieId}` | GET | Stream links for movie |
| `/FmhyPlugin/Configuration` | GET/POST | Plugin configuration |

## Troubleshooting

### No movies showing
1. Check if sources are enabled in configuration
2. Verify internet connectivity
3. Check Jellyfin logs for errors

### Streams not playing
1. Some sources require ad blocker - enable in browser
2. Try different quality option
3. Check if source requires VPN (geo-blocked)
4. Some sources use Cloudflare - may need proxy

### Performance issues
1. Reduce "Max Results Per Source" in configuration
2. Increase cache duration
3. Disable slow sources

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

MIT License - See LICENSE file for details

## Credits

- [FMHY](https://fmhy.net) for the comprehensive source list
- [Jellyfin](https://jellyfin.org) for the amazing media server
- All the streaming sites listed on FMHY