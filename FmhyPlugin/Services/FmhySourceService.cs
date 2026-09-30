using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using MediaBrowser.Controller.Configuration;
using FmhyPlugin.Configuration;
using FmhyPlugin.Models;
using HtmlAgilityPack;

namespace FmhyPlugin.Services
{
    /// <summary>
    /// Service for fetching and managing content from FMHY streaming sources.
    /// </summary>
    public class FmhySourceService
    {
        private readonly IServerConfigurationManager _configManager;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, List<MovieItem>> _cache;
        private DateTime _lastCacheUpdate;

        // FMHY streaming sources - curated list from fmhy.net
        private static readonly List<StreamingSource> DefaultSources = new List<StreamingSource>
        {
            // Stream Aggregators
            new StreamingSource { Id = "movy", Name = "Movy", Url = "https://www.movy.sx/", Category = "aggregator", Description = "Movies / TV / Anime / 4K / Auto-Next", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "cinejoy", Name = "Cinejoy", Url = "https://cinejoy.pk/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "popcornmovies", Name = "PopcornMovies", Url = "https://popcornmovies.ac/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "7movies", Name = "7Movies", Url = "https://7movies.ac/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "flixer", Name = "Flixer", Url = "https://flixer.gd", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "rive", Name = "Rive", Url = "https://www.rivestream.app/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "67movies", Name = "67Movies", Url = "https://67movies.st/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "shuttletv", Name = "ShuttleTV", Url = "https://shuttletv.su/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "streamo", Name = "Streamo", Url = "https://streamo.pro/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "stellar", Name = "Stellar", Url = "https://stellar.gdn/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "toustream", Name = "TouStream", Url = "https://toustream.xyz/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "vivarium", Name = "Vivarium", Url = "https://viv.st/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "reelix", Name = "Reelix", Url = "https://reelix.ac/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "arrowtv", Name = "ArrowTV", Url = "https://arrowtv.net/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "way2movies", Name = "Way2Movies", Url = "https://beta.way2movies.live/", Category = "aggregator", Description = "Movies / TV / Anime / 4K", Quality = "4K", HasAutoNext = false },
            new StreamingSource { Id = "movienight", Name = "Movie Night", Url = "https://movienig.ht/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "flystream", Name = "FlyStream", Url = "https://flystream.net/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "spacedom", Name = "Spacedom", Url = "https://spacedom.live/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "cinemabz", Name = "Cinema (BZ)", Url = "https://cinema.army/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "svstream", Name = "SvStream", Url = "https://svstream.cc/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "movish", Name = "Movish", Url = "https://movish.to/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "chillflix", Name = "Chillflix", Url = "https://chillflix.lol/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "moviebite", Name = "MovieBite", Url = "https://moviebite.org/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "tonkacine", Name = "TonkaCine", Url = "https://tonkacine.watch/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "cinetaro", Name = "Cinetaro", Url = "https://cinetaro.to/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "cinemaos", Name = "CinemaOS", Url = "https://cinemaos.live/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "vuflix", Name = "Vuflix", Url = "https://vuflix.co/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "gaiaflix", Name = "GaiaFlix", Url = "https://gaiaflix.live/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vidplay", Name = "VidPlay", Url = "https://vidplay.to/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "moonflix", Name = "Moonflix", Url = "https://moonflix.website/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "toflix", Name = "Toflix", Url = "https://toflix.co/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "cinegram", Name = "Cinegram", Url = "https://cinegram.tv/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "willow", Name = "Willow", Url = "https://willow.arlen.icu/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "hivex", Name = "HiveX", Url = "https://hivex.stream/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "cinemove", Name = "Cinemove", Url = "https://cinemove.cc/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "lightflix", Name = "Lightflix", Url = "https://lightflix.app/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "boomflix", Name = "Boomflix", Url = "https://boomflix.pages.dev/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "overlook", Name = "Overlook", Url = "https://overlook.cx/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "stigstream", Name = "Stigstream", Url = "https://stigstream.ru/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "cineapse", Name = "Cineapse", Url = "https://www.cineapse.net/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "bingr", Name = "Bingr", Url = "https://bingr.one/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "bingebang", Name = "BingeBang", Url = "https://bingebang.st/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "frame", Name = "FRAME", Url = "https://www.framemovie.online/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "moanatv", Name = "moanatv", Url = "https://lfmx.app/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "allyoucanwatch", Name = "All You Can Watch", Url = "https://allyoucanwatch.net/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "3eyedraven", Name = "3eyedraven", Url = "https://3eyedraven.watch/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "vaultplayer", Name = "VaultPlayer", Url = "https://vaultplayer.co.uk/", Category = "aggregator", Description = "Movies / TV / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "dulo", Name = "dulo", Url = "https://dulo.mov/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "flixtrz", Name = "Flixtrz", Url = "https://flixtrz.com/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "zxcstream", Name = "ZXCSTREAM", Url = "https://zxcprime.icu/", Category = "aggregator", Description = "Movies / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "watchott", Name = "Watchott", Url = "https://watchott.org/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "mapple", Name = "Mapple", Url = "https://mapple.fun/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "movienerds", Name = "MovieNerds", Url = "https://movienerds.site/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "smashystream", Name = "Smashystream", Url = "https://smashystream.xyz/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "novera", Name = "NOVERA", Url = "https://novera.tv/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "nova", Name = "NOVA", Url = "https://novahd.cc/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "netplay", Name = "NetPlay", Url = "https://netplayz.icu/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "screenscape", Name = "Screenscape", Url = "https://screenscape.me/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "kofi", Name = "Kofi", Url = "https://kofi.mov/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "popcorntime", Name = "PopcornTime", Url = "https://popwatch.to/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "koddo", Name = "koddo", Url = "https://koddo.ch/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "surface", Name = "Surface Stream", Url = "https://watchsurface.stream/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "framextv", Name = "frameXTV", Url = "https://framextv.tech/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "apexmovies", Name = "Apexmovies", Url = "https://apexmovies.net/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "reelzone", Name = "ReelZone", Url = "https://reelzone.icu/", Category = "aggregator", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "streamvaults", Name = "StreamVaults", Url = "https://streamvaults.ru/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nxsha", Name = "Nxsha", Url = "https://web.nxsha.app/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vegetatv", Name = "Vegeta TV", Url = "http://vegetatv.duckdns.org/", Category = "aggregator", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },

            // P-Stream Forks
            new StreamingSource { Id = "cinecat", Name = "Cinecat", Url = "https://cinecat.eu/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "aether", Name = "Aether", Url = "https://aether.ist/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "kstream", Name = "kstream", Url = "https://kdesa.stream/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "cinefork", Name = "Cinefork", Url = "https://cinefork.net/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "basement", Name = "Basement", Url = "https://basementx.lol/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "pstream", Name = "P-Stream", Url = "https://pstream.cfd/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "rizzstream", Name = "Rizz Stream", Url = "https://rizzking.org/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "streamwatch", Name = "StreamWatch", Url = "https://streamwatch.online/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "novashow", Name = "NovaShow", Url = "https://novashow.live/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "fmfau", Name = "FMFAU", Url = "https://fmfau.com/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "icefy", Name = "IceFY", Url = "https://icefy.top/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "peestream", Name = "peestream", Url = "https://peestream.in/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "cinebygdn", Name = "Cineby (gdn)", Url = "https://cineby.gdn/", Category = "pstream", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },

            // Dedicated-Server
            new StreamingSource { Id = "atlantic", Name = "Atlantic", Url = "https://atlantic.st/", Category = "dedicated", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "nepu", Name = "NEPU", Url = "https://nepu.io/", Category = "dedicated", Description = "Movies / TV / Anime / Auto-Next / 4K", Quality = "4K", HasAutoNext = true },
            new StreamingSource { Id = "ee3", Name = "EE3", Url = "https://ee3.me/", Category = "dedicated", Description = "Movies / Invite Required", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "pressplay", Name = "PressPlay", Url = "https://pressplayz.to/", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "streamingunity", Name = "Streaming Unity", Url = "https://streamingunity.vip/", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "abibli", Name = "Abibli", Url = "https://abibli.com/", Category = "dedicated", Description = "Movies / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "soapgo", Name = "SoapGo", Url = "https://soapgo.to/", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinemacity", Name = "CinemaCity", Url = "https://cinemacity.cc/", Category = "dedicated", Description = "Movies / TV / Anime / Requires Sign-Up", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "arc018", Name = "arc018", Url = "https://arc018.stream/", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ridomovies", Name = "RidoMovies", Url = "https://ridomovies.is/", Category = "dedicated", Description = "Movies / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "filmo", Name = "Filmo", Url = "https://filmo.to/", Category = "dedicated", Description = "Movies / 3rd Party Host", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "azmovies", Name = "AZMovies", Url = "https://azmovies.to/", Category = "dedicated", Description = "Movies / TV / Anime / Some 3rd Party Hosts", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "onionplay", Name = "OnionPlay", Url = "https://onionplay.st/", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "showbox", Name = "ShowBox", Url = "https://www.showbox.media/", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "uniquestream", Name = "UniqueStream", Url = "https://uniquestream.net/", Category = "dedicated", Description = "Movies / TV / Anime / 720p", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "bflix", Name = "BFLIX", Url = "https://bbflix.one/", Category = "dedicated", Description = "Movies / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "fsharetv", Name = "FshareTV", Url = "https://fsharetv.co/", Category = "dedicated", Description = "Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "movienestbd", Name = "MovieNestBD", Url = "https://movienestbd.best/", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "m4uhd", Name = "M4uHD", Url = "https://m4uhd.vip", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "levidia", Name = "Levidia", Url = "https://www.levidia.ch/", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "subsl", Name = "SubSL", Url = "https://subsl.top/", Category = "dedicated", Description = "Movies / TV / 720p", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "primewire", Name = "PrimeWire", Url = "https://www.primewire.mov/", Category = "dedicated", Description = "Movies / TV / Anime / Mostly 3rd Party Hosts", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "yesmovie", Name = "YesMovie", Url = "https://ww1.yesmovies.ag/", Category = "dedicated", Description = "Movies / TV / 720p", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "hollymoviehd", Name = "HollyMovieHD", Url = "https://hollymoviehd.cc/", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "downloads-anymovies", Name = "Downloads-Anymovies", Url = "https://www.downloads-anymovies.co/", Category = "dedicated", Description = "Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "moviebox", Name = "MovieBox", Url = "https://movieboxonline.net", Category = "dedicated", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "lookmovie", Name = "LookMovie", Url = "https://lookmovie2.to/", Category = "dedicated", Description = "Movies / TV / Auto-Next / 480p", Quality = "480p", HasAutoNext = true },

            // Multi-Server
            new StreamingSource { Id = "1shows", Name = "1Shows", Url = "http://1shows.bz", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "anicine", Name = "AniCine", Url = "https://anicine.xyz/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "zetmoon", Name = "ZetMoon", Url = "https://zetmoon.live/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "primeshows", Name = "Primeshows", Url = "https://www.primeshows.org/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "moovie", Name = "Moovie", Url = "https://moovie.fun/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "anixtv", Name = "Anixtv", Url = "https://anixx.fun/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "aurorascreen", Name = "AuroraScreen", Url = "https://aurorascreen.org/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "hydrahd", Name = "HydraHD", Url = "https://hydrahd.ws/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "fireflix", Name = "Fireflix", Url = "https://fireflix4.pages.dev/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "vidbox", Name = "Vidbox", Url = "https://vidbox.vc/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "zencine", Name = "Zencine", Url = "https://zencine.org/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinewave", Name = "CineWave", Url = "https://watch.cinewave.qzz.io/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "cinestream", Name = "CineStream", Url = "https://cinestream.kje.us/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "youflex", Name = "Youflex", Url = "https://youflex.top/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "flixzy", Name = "Flixzy", Url = "https://flixzy.pages.dev/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "filmcave", Name = "FilmCave", Url = "https://filmcave.ru/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "321movies", Name = "321Movies", Url = "https://321movies.xyz/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "kirastreams", Name = "KiraStreams", Url = "https://kirastreams.pages.dev/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "flyflix", Name = "Flyflix", Url = "https://flyflix.net/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "fluxtv", Name = "FluxTV", Url = "https://fluxtv.co.uk/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinevibe", Name = "CineVibe", Url = "https://cinevibe.cc/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "moviefy", Name = "MovieFY", Url = "https://player.xtra.wtf/search", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "flixvo", Name = "Flixvo", Url = "https://flixvo.live/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "candlestream", Name = "CandleStream", Url = "https://candlestream.xyz/#home", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "zflix", Name = "ZFlix", Url = "https://zflix.me/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "flicker", Name = "Flicker", Url = "https://flicker-mini.pages.dev/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "onoflix", Name = "ONOFLIX", Url = "https://onoflix.live/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "bingeflix", Name = "Bingeflix", Url = "https://bingeflix.tv/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "7reels", Name = "7REELS", Url = "https://7reels.cc/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ernax", Name = "Ernax", Url = "https://ernax.pro/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "amberflix", Name = "AmberFlix", Url = "https://www.amberflix.xyz/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "flnk", Name = "FLNK", Url = "https://flnk.fun/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "watchseries", Name = "WatchSeries", Url = "https://watchseries.show/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "pawflix", Name = "Pawflix", Url = "https://pawflix.foo.ng/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "kaitovault", Name = "KaitoVault", Url = "https://www.kaitovault.com/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinebytv", Name = "CinebyTV", Url = "https://cinebytv.com", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "tvids", Name = "TVids", Url = "https://www.tvids.to/", Category = "multi", Description = "Movies / TV / Anime / Auto-Next", Quality = "1080p", HasAutoNext = true },
            new StreamingSource { Id = "moviestowatch", Name = "Movies To Watch", Url = "https://www.moviestowatch.top/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "streamex", Name = "StreameX", Url = "https://streamex.sh/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "zerostream", Name = "Zerostream", Url = "https://zerostream.alwaysdata.net/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "fishystream", Name = "FishyStream", Url = "https://fishystream-app.pages.dev/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "snowstream", Name = "Snowstream", Url = "https://snowstream.vercel.app/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "streamgoblin", Name = "StreamGoblin", Url = "https://streamgoblin.cc/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "watchorbit", Name = "WatchOrbit", Url = "https://watchorbit.me/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinenest", Name = "CineNest", Url = "https://cine-nest-nine.vercel.app/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "duafile", Name = "DuaFile", Url = "https://download.duafile.com/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "fliker", Name = "FLIKER", Url = "https://fliker.freebuff.app/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "meowly", Name = "Meowly", Url = "https://meowly.qzz.io/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "warflix", Name = "Warflix", Url = "https://warflix.im/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "heartive", Name = "Heartive", Url = "https://heartivelovestv.pages.dev/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinego", Name = "CineGo", Url = "https://cinego.co/", Category = "multi", Description = "Movies / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "moviepire", Name = "Moviepire", Url = "https://moviepire.org/", Category = "multi", Description = "Movies / TV / Anime", Quality = "1080p", HasAutoNext = false },

            // Free w/ Ads (Legal)
            new StreamingSource { Id = "tubi", Name = "Tubi", Url = "https://tubitv.com", Category = "free-ads", Description = "Movies / TV / 720p", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "plex", Name = "Plex", Url = "https://watch.plex.tv/", Category = "free-ads", Description = "Movies / TV / 720p", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "pluto", Name = "Pluto", Url = "https://pluto.tv/", Category = "free-ads", Description = "Movies / TV / 720p", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "popcornflix", Name = "PopcornFlix", Url = "https://popcornflix.com/", Category = "free-ads", Description = "Movies / TV", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "fawesome", Name = "Fawesome", Url = "https://fawesome.tv/", Category = "free-ads", Description = "Movies / TV", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "sling", Name = "Sling", Url = "https://watch.sling.com/", Category = "free-ads", Description = "Movies / TV / Live News / US Only", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "fandango", Name = "Fandango", Url = "https://athome.fandango.com/content/browse/free", Category = "free-ads", Description = "Movies / TV / US Only", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "shouttv", Name = "Shout! TV", Url = "https://shout-tv.com/", Category = "free-ads", Description = "Movies / TV / US + CA Only / Auto-Next", Quality = "720p", HasAutoNext = true },
            new StreamingSource { Id = "kanopy", Name = "Kanopy", Url = "https://kanopy.com/", Category = "free-ads", Description = "Movies / TV / US Only / Requires Library Card", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "hoopla", Name = "hoopla", Url = "https://www.hoopladigital.com/", Category = "free-ads", Description = "Movies / TV / US Only / Requires Library Card", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "foundtv", Name = "Found TV", Url = "https://watch.foundtv.com/", Category = "free-ads", Description = "Found Footage Movies / Requires Sign-Up", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "byutv", Name = "BYUtv", Url = "https://www.byutv.org/", Category = "free-ads", Description = "TV / Family-Friendly", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "yowtv", Name = "YOW.tv", Url = "https://yow.tv/", Category = "free-ads", Description = "Movies / TV / Indie", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "7plus", Name = "7+", Url = "https://7plus.com.au/", Category = "free-ads", Description = "Movies / TV / US + AU Only", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "playary", Name = "Playary", Url = "https://www.playary.com/", Category = "free-ads", Description = "Movies / TV", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "filmzie", Name = "Filmzie", Url = "https://filmzie.com/", Category = "free-ads", Description = "Movies / TV", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "arte", Name = "ARTE", Url = "https://www.arte.tv/en", Category = "free-ads", Description = "Movies / TV / Auto-Next", Quality = "720p", HasAutoNext = true },
            new StreamingSource { Id = "flixhouse", Name = "FlixHouse", Url = "https://flixhouse.com/", Category = "free-ads", Description = "Indie Movies", Quality = "720p", HasAutoNext = false },
            new StreamingSource { Id = "cineverse", Name = "Cineverse", Url = "https://streaming.cineverse.com/", Category = "free-ads", Description = "Persian Movies / TV w/ English Subs", Quality = "720p", HasAutoNext = false },

            // Video Streaming
            new StreamingSource { Id = "youtube", Name = "YouTube", Url = "https://www.youtube.com/", Category = "video", Description = "Video Streaming", Quality = "4K", HasAutoNext = false },
            new StreamingSource { Id = "dailymotion", Name = "Dailymotion", Url = "https://www.dailymotion.com/", Category = "video", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vimeo", Name = "Vimeo", Url = "https://vimeo.com/watch", Category = "video", Description = "Short Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "odysee", Name = "Odysee", Url = "https://odysee.com/", Category = "video", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rumble", Name = "Rumble", Url = "https://rumble.com/", Category = "video", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },

            // Classics / Public Domain
            new StreamingSource { Id = "archive", Name = "Internet Archive", Url = "https://archive.org/details/moviesandfilms", Category = "classics", Description = "Classic / Silent Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "wikiflix", Name = "WikiFlix", Url = "https://wikiflix.toolforge.org/", Category = "classics", Description = "Classic Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "classicmovies", Name = "The Classic Movies", Url = "https://www.the-classic-movies.com/", Category = "classics", Description = "Classic Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "retroflix", Name = "RetroFlix", Url = "https://retroflix.org/", Category = "classics", Description = "Classic Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "publicdomain", Name = "Public Domain Movies", Url = "https://publicdomainmovie.net/", Category = "classics", Description = "Public Domain Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "footagefarm", Name = "FootageFarm", Url = "https://footagefarm.com/", Category = "classics", Description = "Public Domain Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "bfi", Name = "BFIPlayer", Url = "https://player.bfi.org.uk/free", Category = "classics", Description = "British Film Institute / Requires UK VPN", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "britishpathe", Name = "British Pathé", Url = "https://www.britishpathe.com/", Category = "classics", Description = "British Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nfb", Name = "nfb.ca", Url = "https://www.nfb.ca/", Category = "classics", Description = "Canadian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nfsa", Name = "NFSA", Url = "https://www.nfsa.gov.au/", Category = "classics", Description = "Australian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "historicfilms", Name = "Historic Films", Url = "https://www.historicfilms.com/", Category = "classics", Description = "Historic Footage / Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "prelinger", Name = "Prelinger Archives", Url = "https://www.panix.com/~footage/", Category = "classics", Description = "Ephemeral Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "loc", Name = "Library of Congress", Url = "https://www.loc.gov/film-and-videos/", Category = "classics", Description = "Movies / Short Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "europeanfilm", Name = "European Film Gateway", Url = "https://www.europeanfilmgateway.eu/", Category = "classics", Description = "European Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "filmarchives", Name = "National Film Archive of Japan", Url = "https://meiji.filmarchives.jp/", Category = "classics", Description = "Japanese Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinematheque", Name = "Cinematheque", Url = "https://www.cinematheque.fr/henri/english/", Category = "classics", Description = "Rare French Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rarefilmm", Name = "RareFilmm", Url = "https://rarefilmm.com/", Category = "classics", Description = "Rare Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "0xdb", Name = "0xDB", Url = "https://0xdb.org/", Category = "classics", Description = "Rare Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "gizmoplex", Name = "GizmoPlex", Url = "https://www.gizmoplex.com/mst3k", Category = "classics", Description = "MST3K Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ubu", Name = "Ubu", Url = "https://ubu.com/film/", Category = "classics", Description = "Short Films / Avant-Garde", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "viddsee", Name = "Viddsee", Url = "https://www.viddsee.com/", Category = "classics", Description = "Short Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "shortverse", Name = "Shortverse", Url = "https://www.shortverse.com/explore", Category = "classics", Description = "Short Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "shortoftheweek", Name = "Short of the Week", Url = "https://www.shortoftheweek.com/", Category = "classics", Description = "Short Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "lightcone", Name = "LightCone", Url = "https://lightcone.org/en", Category = "classics", Description = "Short / Experimental Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "lecinemaclub", Name = "Le Cinéma Club", Url = "https://www.lecinemaclub.com/", Category = "classics", Description = "Hand-Picked Indie Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "lima", Name = "LIMA", Url = "https://www.li-ma.nl/", Category = "classics", Description = "Short / Experimental Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nasa", Name = "NASA+", Url = "https://plus.nasa.gov/", Category = "classics", Description = "Official NASA Streaming Service", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "redbull", Name = "Red Bull TV", Url = "https://www.redbull.tv/", Category = "classics", Description = "Full Red Bull TV Episodes / Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "adultswim", Name = "Adult Swim", Url = "https://www.adultswim.com/videos/", Category = "classics", Description = "Full Adult Swim Episodes", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "disneynow", Name = "DisneyNOW", Url = "https://disneynow.com/", Category = "classics", Description = "Full Disney Episodes / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "history", Name = "HISTORY", Url = "https://play.history.com/", Category = "classics", Description = "Full History / A+E Episodes", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "sfbtv", Name = "SFBTV", Url = "https://diva.cfsu.edu/collections/sfbatv", Category = "classics", Description = "San Francisco TV Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "hdclump", Name = "HDclump", Url = "https://hdclump.com/", Category = "classics", Description = "Documentaries / Cooking / Gardening", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "awardstreams", Name = "AwardStreams", Url = "https://awardstreams.pages.dev/", Category = "classics", Description = "Live Award Show Replays", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "twcclassics", Name = "TWC Classics", Url = "https://twcclassics.com/", Category = "classics", Description = "Weather Channel Replays", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "taskmaster", Name = "Taskmaster", Url = "https://rentry.co/bbbr4cfr", Category = "classics", Description = "Full Taskmaster Episodes", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "silentlibrary", Name = "The Silent Library", Url = "https://thesilentlibrary.com/", Category = "classics", Description = "Japanese Game Shows", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "myrunningman", Name = "My Running Man", Url = "https://myrunningman.com/", Category = "classics", Description = "My Running Man Episodes", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "dpan", Name = "DPAN", Url = "https://dpan.tv/", Category = "classics", Description = "Deaf Entertainment / News", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "deaffest", Name = "Deaffest", Url = "https://deaffestonlinecinema.eventive.org/", Category = "classics", Description = "Deaf Entertainment / Sign-Up", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "dmdb", Name = "DMDb", Url = "https://deafmovie.org/free/", Category = "classics", Description = "Deaf Entertainment / News", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "lumotv", Name = "Lumo TV", Url = "https://lumotv.co.uk/", Category = "classics", Description = "Deaf Entertainment / News", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "audiovault", Name = "Audiovault", Url = "https://audiovault.net/", Category = "classics", Description = "Descriptive Audio for Blind Users", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "deafvideo", Name = "DeafVideo", Url = "https://www.deafvideo.tv/", Category = "classics", Description = "ASL Vlogs + Videos", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "globalshakespeares", Name = "GlobalShakespeares", Url = "https://globalshakespeares.mit.edu/", Category = "classics", Description = "Shakespeare Performance Recordings", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "tvark", Name = "TVARK", Url = "https://tvark.org/", Category = "classics", Description = "Commercial / TV Promo Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "dailycommercials", Name = "Daily Commercials", Url = "https://dailycommercials.com/", Category = "classics", Description = "Commercial / TV Promo Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "moviecommentaries", Name = "MovieCommentaries", Url = "https://www.youtube.com/@moviecommentaries", Category = "classics", Description = "Movie / TV Director Commentaries", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "filmsbytheyear", Name = "FilmsByTheYear", Url = "https://www.youtube.com/@FilmsbytheYear/playlists", Category = "classics", Description = "Classic Films Playlists", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "wutang", Name = "Wu Tang Collection", Url = "https://www.thewutangcollection.com/", Category = "classics", Description = "Classic Martial Arts Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "silenthall", Name = "Silent Hall of Fame", Url = "https://silent-hall-of-fame.org/", Category = "classics", Description = "Silent Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "danishsilent", Name = "Danish Silent Film", Url = "https://www.stumfilm.dk/en/stumfilm", Category = "classics", Description = "Silent Danish Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinetimes", Name = "Cinetimes", Url = "https://cinetimes.org/en/", Category = "classics", Description = "Public Domain Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "wikimedia", Name = "Wikimedia Commons", Url = "https://commons.wikimedia.org/wiki/Category:Videos", Category = "classics", Description = "Wiki Commons Video Files", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "huntley", Name = "HuntleyArchives", Url = "https://www.huntleyarchives.com/", Category = "classics", Description = "Rare / Forgotten Short Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "easterneuropean", Name = "Eastern European Movies", Url = "https://easterneuropeanmovies.com/", Category = "classics", Description = "Eastern European Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "indiancine", Name = "IndianCine", Url = "https://indiancine.ma/", Category = "classics", Description = "Indian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "russianfilmhub", Name = "RussianFilmHub", Url = "https://russianfilmhub.com/", Category = "classics", Description = "Russian Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinemata", Name = "Cinemata", Url = "https://cinemata.org/", Category = "classics", Description = "Asia-Pacific Social Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "austrianfilm", Name = "Austrian Film Museum", Url = "https://www.filmmuseum.at/jart/prj3/filmmuseum/main.jart?rel=en", Category = "classics", Description = "Austrian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "35mm", Name = "35mm", Url = "https://35mm.online/en", Category = "classics", Description = "Polish Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "travelfilm", Name = "The Travel Film Archive", Url = "https://travelfilmarchive.com/", Category = "classics", Description = "Archival Travel Footage", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "dvidshub", Name = "DVIDS", Url = "https://www.dvidshub.net/", Category = "classics", Description = "Military Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "iwm", Name = "Imperial War Museums", Url = "https://www.iwm.org.uk/", Category = "classics", Description = "British & Commonwealth War Footage", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "atomcentral", Name = "Atom Central", Url = "https://www.atomcentral.com/", Category = "classics", Description = "Historical Archive of Atomic & Nuclear Tests", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "oldtimey", Name = "Old Timey Computer Show", Url = "https://otcs.minuspoint.com/schedule.html", Category = "classics", Description = "Live Retro Computer / Game Media Streams", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "retrostrange", Name = "RetroStrange", Url = "https://retrostrange.com/", Category = "classics", Description = "Rare / Vintage / Obscure Media Stream", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "exp tv", Name = "EXP TV", Url = "https://linktr.ee/exp.tv", Category = "classics", Description = "Rare / Vintage / Obscure Media Stream", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "versusmedia", Name = "VersusMedia", Url = "https://watch.versusmedia.com/live-channel", Category = "classics", Description = "Independent Film Stream", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ytch", Name = "YTCH", Url = "https://ytch.tv/", Category = "classics", Description = "Random YouTube Streams / Custom Channels", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "channelsurfer", Name = "Channel Surfer", Url = "https://channelsurfer.tv/", Category = "classics", Description = "Random YouTube Streams / Custom Channels", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "freetvz", Name = "FreeTVz", Url = "https://freetvz.com/", Category = "classics", Description = "Random YouTube Streams / Custom Channels", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rifftrx", Name = "RiffTrax Twitch", Url = "https://www.twitch.tv/rifftrax", Category = "classics", Description = "RiffTrax Live Streams / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ttn", Name = "TTN", Url = "https://watchttn.com/", Category = "classics", Description = "Random Streams / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "reacttv", Name = "React.tv", Url = "https://react.tv/", Category = "classics", Description = "Random Streams", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "baked", Name = "Baked.live", Url = "https://baked.live/", Category = "classics", Description = "Random User Channels / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cytube", Name = "cytube", Url = "https://cytu.be/", Category = "classics", Description = "Random User Channels / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vaughnlive", Name = "VaughnLive", Url = "https://vaughn.live/browse/misc", Category = "classics", Description = "Random User Channels / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "motionbox", Name = "MotionBox", Url = "https://omega.ggg/MotionBox/", Category = "classics", Description = "Online Video Aggregation App", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "peertube", Name = "PeerTube", Url = "https://joinpeertube.org/", Category = "classics", Description = "Decentralized Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rutube", Name = "rutube", Url = "https://rutube.ru", Category = "classics", Description = "Russian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "videa", Name = "Videa", Url = "https://videa.hu/", Category = "classics", Description = "Hungarian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vanillo", Name = "Vanillo", Url = "https://vanillo.tv/", Category = "classics", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "naver", Name = "Naver", Url = "https://tv.naver.com/", Category = "classics", Description = "Korean Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ultimedia", Name = "ultimedia", Url = "https://www.ultimedia.com/", Category = "classics", Description = "French Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vk", Name = "VK", Url = "https://vkvideo.ru/", Category = "classics", Description = "Russian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ok", Name = "OK", Url = "https://ok.ru/video", Category = "classics", Description = "Russian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "bilibili", Name = "BiliBili", Url = "https://www.bilibili.com/", Category = "classics", Description = "Chinese Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nicovideo", Name = "Nico Nico", Url = "https://www.nicovideo.jp/", Category = "classics", Description = "Japanese Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "typetype", Name = "TypeType", Url = "https://watch.typetype.video/", Category = "classics", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "tape", Name = "Tape", Url = "https://github.com/tapexyz/tape", Category = "classics", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "interactive", Name = "Interactive Player", Url = "https://github.com/Eveep23/Interactive-Player", Category = "classics", Description = "Interactive Content Player / Emulator", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "zero1cine", Name = "Zero1Cine", Url = "https://zero1cine.com/", Category = "classics", Description = "AI Generated Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "homemovies", Name = "HomeMovies101", Url = "https://www.homemovies100.it/en/", Category = "classics", Description = "Home Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "3dsmovies", Name = "3DS Movies", Url = "https://rentry.co/FMHYB64#_3dsm", Category = "classics", Description = "3D Movies for 3DS Handhelds", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "kodasusaka", Name = "Kodasusaka", Url = "https://kodasusaka.com/movies", Category = "classics", Description = "Rare / Classic Japanese Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rohesia", Name = "Rohesia Hamilton Metcalfe", Url = "https://www.panix.com/~hamiltro/links/", Category = "classics", Description = "Experimental Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "arko", Name = "Argo", Url = "https://web.watchargo.com/", Category = "classics", Description = "Short Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "wco", Name = "WCO", Url = "https://www.wco.tv/", Category = "classics", Description = "TV / Movies / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "funnier", Name = "FunnierMoments", Url = "https://www.funniermoments.net/", Category = "classics", Description = "TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "megacartoons", Name = "MegaCartoons", Url = "https://www.megacartoons.net/", Category = "classics", Description = "TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "b98", Name = "B98", Url = "https://www.b98.tv/home/", Category = "classics", Description = "Classic / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "toontales", Name = "ToonTales", Url = "https://www.toontales.net/", Category = "classics", Description = "Classic / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "supercartoons", Name = "SuperCartoons", Url = "https://www.supercartoons.net/", Category = "classics", Description = "Classic / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "japaneseanimated", Name = "Japanese Animated Film Classics", Url = "https://animation.filmarchives.jp/index.html", Category = "classics", Description = "Japanese Animation Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "eddvault", Name = "Eddvault", Url = "https://eddvault.notion.site/Eddvault-3b7700da57bd4d429b927c1733ccb772", Category = "classics", Description = "Eddsworld Content Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "piratexplay", Name = "PirateXplay", Url = "https://piratexplay.cc/home", Category = "classics", Description = "TV / Movies / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "justwatch", Name = "JustWatch", Url = "https://www.justwatch.com/us?monetization_types=free", Category = "classics", Description = "Free w/ Ads Directory", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "freegreatmovies", Name = "FreeGreatMovies", Url = "https://www.freegreatmovies.com/", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "voleflix", Name = "Voleflix", Url = "https://vole.wtf/voleflix/", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "openculture", Name = "OpenCulture", Url = "https://www.openenculture.com/freemoviesonline", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "moviesfoundonline", Name = "MoviesFoundOnline", Url = "https://moviesfoundonline.com/", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "officialytmovies", Name = "Official YT Movies", Url = "https://www.youtube.com/feed/storefront?bp=ogUCKAY%3D", Category = "classics", Description = "YouTube Movie Collections / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "primevideo", Name = "Prime Video Free", Url = "https://www.amazon.com/gp/video/storefront/?ie=UTF8&contentId=freetv", Category = "classics", Description = "Movies / TV / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "roku", Name = "Roku", Url = "https://therokuchannel.roku.com/", Category = "classics", Description = "Movies / TV / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "darkroom", Name = "DarkRoom", Url = "https://www.darkroom.film/", Category = "classics", Description = "Movies / TV / US Only / Requires Sign-Up", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "yfanefa", Name = "yfanefa", Url = "https://www.yfanefa.com/", Category = "classics", Description = "Yorkshire Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nls", Name = "NLS", Url = "https://www.nls.uk/", Category = "classics", Description = "Scottish Film & Video Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "northernireland", Name = "Northern Ireland Screen", Url = "https://digitalfilmarchive.net/", Category = "classics", Description = "Irish Film Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ifiarchive", Name = "IFI Archive", Url = "https://ifiarchiveplayer.ie/", Category = "classics", Description = "Irish Film Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "texasarchive", Name = "TexasArchive", Url = "https://texasarchive.org/", Category = "classics", Description = "Texas Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "chicago", Name = "Chicago Film Archives", Url = "https://www.chicagofilmarchives.org/", Category = "classics", Description = "Chicago Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "platfo", Name = "Platfo Filmo", Url = "https://filmo.platfo.es/pages/home", Category = "classics", Description = "Spanish Film Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cclm", Name = "CCLM", Url = "https://www.cclm.cl/cineteca-online", Category = "classics", Description = "Chilean Film Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "stiftungimai", Name = "Stiftung imai", Url = "https://stiftung-imai.de/", Category = "classics", Description = "German Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "digitaler", Name = "Digitaler Lesesaal", Url = "https://digitaler-lesesaal.bundesarchiv.de/en", Category = "classics", Description = "German Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "filmportal", Name = "Film Portal", Url = "https://www.filmportal.de/en/videos", Category = "classics", Description = "German Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ukdefence", Name = "UK Defence Imagery", Url = "https://www.defenceimagery.mod.uk/", Category = "classics", Description = "Military Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "australiandefence", Name = "Australian Defence Imagery", Url = "https://images.defence.gov.au/assets/", Category = "classics", Description = "Military Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ngataonga", Name = "NGA Taonga", Url = "https://www.ngataonga.org.nz/search-use-collection/search/", Category = "classics", Description = "New Zealand's Audiovisual Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nzonscreen", Name = "NZOnScreen", Url = "https://www.nzonscreen.com/", Category = "classics", Description = "New Zealand's Audiovisual Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "movingimage", Name = "Moving Image", Url = "https://movingimage.nls.uk/", Category = "classics", Description = "Scottish Film & Video Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "colonialfilm", Name = "ColonialFilm", Url = "http://www.colonialfilm.org.uk/", Category = "classics", Description = "British Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "yfanefa2", Name = "yfanefa", Url = "https://www.yfanefa.com/", Category = "classics", Description = "Yorkshire Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nfsa2", Name = "NFSA", Url = "https://www.nfsa.gov.au/", Category = "classics", Description = "Australian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "bfi2", Name = "BFIPlayer", Url = "https://player.bfi.org.uk/free", Category = "classics", Description = "British Film Institute / Requires UK VPN", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "bfiarchive", Name = "BFI Archive", Url = "https://www.bfi.org.uk/bfi-national-archive", Category = "classics", Description = "British Film Institute / Requires UK VPN", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "britishpathe2", Name = "British Pathé", Url = "https://www.britishpathe.com/", Category = "classics", Description = "British Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nfb2", Name = "nfb.ca", Url = "https://www.nfb.ca/", Category = "classics", Description = "Canadian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nfssa", Name = "NFSA", Url = "https://www.nfsa.gov.au/", Category = "classics", Description = "Australian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nga", Name = "NGA Taonga", Url = "https://www.ngataonga.org.nz/search-use-collection/search/", Category = "classics", Description = "New Zealand's Audiovisual Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nzonscreen2", Name = "NZOnScreen", Url = "https://www.nzonscreen.com/", Category = "classics", Description = "New Zealand's Audiovisual Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nls2", Name = "NLS", Url = "https://www.nls.uk/", Category = "classics", Description = "Scottish Film & Video Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "movingimage2", Name = "Moving Image", Url = "https://movingimage.nls.uk/", Category = "classics", Description = "Scottish Film & Video Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "northernireland2", Name = "Northern Ireland Screen", Url = "https://digitalfilmarchive.net/", Category = "classics", Description = "Irish Film Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ifiarchive2", Name = "IFI Archive", Url = "https://ifiarchiveplayer.ie/", Category = "classics", Description = "Irish Film Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "texasarchive2", Name = "TexasArchive", Url = "https://texasarchive.org/", Category = "classics", Description = "Texas Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "chicago2", Name = "Chicago Film Archives", Url = "https://www.chicagofilmarchives.org/", Category = "classics", Description = "Chicago Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "indiancine2", Name = "IndianCine", Url = "https://indiancine.ma/", Category = "classics", Description = "Indian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "russianfilmhub2", Name = "RussianFilmHub", Url = "https://russianfilmhub.com/", Category = "classics", Description = "Russian Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinemata2", Name = "Cinemata", Url = "https://cinemata.org/", Category = "classics", Description = "Asia-Pacific Social Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "austrianfilm2", Name = "Austrian Film Museum", Url = "https://www.filmmuseum.at/jart/prj3/filmmuseum/main.jart?rel=en", Category = "classics", Description = "Austrian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "35mm2", Name = "35mm", Url = "https://35mm.online/en", Category = "classics", Description = "Polish Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "travelfilm2", Name = "The Travel Film Archive", Url = "https://travelfilmarchive.com/", Category = "classics", Description = "Archival Travel Footage", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "dvidshub2", Name = "DVIDS", Url = "https://www.dvidshub.net/", Category = "classics", Description = "Military Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "iwm2", Name = "Imperial War Museums", Url = "https://www.iwm.org.uk/", Category = "classics", Description = "British & Commonwealth War Footage", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "atomcentral2", Name = "Atom Central", Url = "https://www.atomcentral.com/", Category = "classics", Description = "Historical Archive of Atomic & Nuclear Tests", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "oldtimey2", Name = "Old Timey Computer Show", Url = "https://otcs.minuspoint.com/schedule.html", Category = "classics", Description = "Live Retro Computer / Game Media Streams", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "retrostrange2", Name = "RetroStrange", Url = "https://retrostrange.com/", Category = "classics", Description = "Rare / Vintage / Obscure Media Stream", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "exptv2", Name = "EXP TV", Url = "https://linktr.ee/exp.tv", Category = "classics", Description = "Rare / Vintage / Obscure Media Stream", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "versusmedia2", Name = "VersusMedia", Url = "https://watch.versusmedia.com/live-channel", Category = "classics", Description = "Independent Film Stream", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ytch2", Name = "YTCH", Url = "https://ytch.tv/", Category = "classics", Description = "Random YouTube Streams / Custom Channels", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "channelsurfer2", Name = "Channel Surfer", Url = "https://channelsurfer.tv/", Category = "classics", Description = "Random YouTube Streams / Custom Channels", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "freetvz2", Name = "FreeTVz", Url = "https://freetvz.com/", Category = "classics", Description = "Random YouTube Streams / Custom Channels", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rifftrx2", Name = "RiffTrax Twitch", Url = "https://www.twitch.tv/rifftrax", Category = "classics", Description = "RiffTrax Live Streams / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ttn2", Name = "TTN", Url = "https://watchttn.com/", Category = "classics", Description = "Random Streams / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "reacttv2", Name = "React.tv", Url = "https://react.tv/", Category = "classics", Description = "Random Streams", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "baked2", Name = "Baked.live", Url = "https://baked.live/", Category = "classics", Description = "Random User Channels / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cytube2", Name = "cytube", Url = "https://cytu.be/", Category = "classics", Description = "Random User Channels / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vaughnlive2", Name = "VaughnLive", Url = "https://vaughn.live/browse/misc", Category = "classics", Description = "Random User Channels / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "motionbox2", Name = "MotionBox", Url = "https://omega.ggg/MotionBox/", Category = "classics", Description = "Online Video Aggregation App", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "peertube2", Name = "PeerTube", Url = "https://joinpeertube.org/", Category = "classics", Description = "Decentralized Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rutube2", Name = "rutube", Url = "https://rutube.ru", Category = "classics", Description = "Russian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "videa2", Name = "Videa", Url = "https://videa.hu/", Category = "classics", Description = "Hungarian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vanillo2", Name = "Vanillo", Url = "https://vanillo.tv/", Category = "classics", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "naver2", Name = "Naver", Url = "https://tv.naver.com/", Category = "classics", Description = "Korean Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ultimedia2", Name = "ultimedia", Url = "https://www.ultimedia.com/", Category = "classics", Description = "French Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vk2", Name = "VK", Url = "https://vkvideo.ru/", Category = "classics", Description = "Russian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ok2", Name = "OK", Url = "https://ok.ru/video", Category = "classics", Description = "Russian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "bilibili2", Name = "BiliBili", Url = "https://www.bilibili.com/", Category = "classics", Description = "Chinese Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nicovideo2", Name = "Nico Nico", Url = "https://www.nicovideo.jp/", Category = "classics", Description = "Japanese Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "typetype2", Name = "TypeType", Url = "https://watch.typetype.video/", Category = "classics", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "tape2", Name = "Tape", Url = "https://github.com/tapexyz/tape", Category = "classics", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "interactive2", Name = "Interactive Player", Url = "https://github.com/Eveep23/Interactive-Player", Category = "classics", Description = "Interactive Content Player / Emulator", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "zero1cine2", Name = "Zero1Cine", Url = "https://zero1cine.com/", Category = "classics", Description = "AI Generated Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "homemovies2", Name = "HomeMovies101", Url = "https://www.homemovies100.it/en/", Category = "classics", Description = "Home Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "3dsmovies2", Name = "3DS Movies", Url = "https://rentry.co/FMHYB64#_3dsm", Category = "classics", Description = "3D Movies for 3DS Handhelds", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "kodasusaka2", Name = "Kodasusaka", Url = "https://kodasusaka.com/movies", Category = "classics", Description = "Rare / Classic Japanese Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rohesia2", Name = "Rohesia Hamilton Metcalfe", Url = "https://www.panix.com/~hamiltro/links/", Category = "classics", Description = "Experimental Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "arko2", Name = "Argo", Url = "https://web.watchargo.com/", Category = "classics", Description = "Short Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "wco2", Name = "WCO", Url = "https://www.wco.tv/", Category = "classics", Description = "TV / Movies / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "funnier2", Name = "FunnierMoments", Url = "https://www.funniermoments.net/", Category = "classics", Description = "TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "megacartoons2", Name = "MegaCartoons", Url = "https://www.megacartoons.net/", Category = "classics", Description = "TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "b982", Name = "B98", Url = "https://www.b98.tv/home/", Category = "classics", Description = "Classic / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "toontales2", Name = "ToonTales", Url = "https://www.toontales.net/", Category = "classics", Description = "Classic / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "supercartoons2", Name = "SuperCartoons", Url = "https://www.supercartoons.net/", Category = "classics", Description = "Classic / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "japaneseanimated2", Name = "Japanese Animated Film Classics", Url = "https://animation.filmarchives.jp/index.html", Category = "classics", Description = "Japanese Animation Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "eddvault2", Name = "Eddvault", Url = "https://eddvault.notion.site/Eddvault-3b7700da57bd4d429b927c1733ccb772", Category = "classics", Description = "Eddsworld Content Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "piratexplay2", Name = "PirateXplay", Url = "https://piratexplay.cc/home", Category = "classics", Description = "TV / Movies / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "justwatch2", Name = "JustWatch", Url = "https://www.justwatch.com/us?monetization_types=free", Category = "classics", Description = "Free w/ Ads Directory", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "freegreatmovies2", Name = "FreeGreatMovies", Url = "https://www.freegreatmovies.com/", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "voleflix2", Name = "Voleflix", Url = "https://vole.wtf/voleflix/", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "openculture2", Name = "OpenCulture", Url = "https://www.openenculture.com/freemoviesonline", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "moviesfoundonline2", Name = "MoviesFoundOnline", Url = "https://moviesfoundonline.com/", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "officialytmovies2", Name = "Official YT Movies", Url = "https://www.youtube.com/feed/storefront?bp=ogUCKAY%3D", Category = "classics", Description = "YouTube Movie Collections / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "primevideo2", Name = "Prime Video Free", Url = "https://www.amazon.com/gp/video/storefront/?ie=UTF8&contentId=freetv", Category = "classics", Description = "Movies / TV / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "roku2", Name = "Roku", Url = "https://therokuchannel.roku.com/", Category = "classics", Description = "Movies / TV / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "darkroom2", Name = "DarkRoom", Url = "https://www.darkroom.film/", Category = "classics", Description = "Movies / TV / US Only / Requires Sign-Up", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "yfanefa3", Name = "yfanefa", Url = "https://www.yfanefa.com/", Category = "classics", Description = "Yorkshire Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nls3", Name = "NLS", Url = "https://www.nls.uk/", Category = "classics", Description = "Scottish Film & Video Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "northernireland3", Name = "Northern Ireland Screen", Url = "https://digitalfilmarchive.net/", Category = "classics", Description = "Irish Film Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ifiarchive3", Name = "IFI Archive", Url = "https://ifiarchiveplayer.ie/", Category = "classics", Description = "Irish Film Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "texasarchive3", Name = "TexasArchive", Url = "https://texasarchive.org/", Category = "classics", Description = "Texas Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "chicago3", Name = "Chicago Film Archives", Url = "https://www.chicagofilmarchives.org/", Category = "classics", Description = "Chicago Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "indiancine3", Name = "IndianCine", Url = "https://indiancine.ma/", Category = "classics", Description = "Indian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "russianfilmhub3", Name = "RussianFilmHub", Url = "https://russianfilmhub.com/", Category = "classics", Description = "Russian Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cinemata3", Name = "Cinemata", Url = "https://cinemata.org/", Category = "classics", Description = "Asia-Pacific Social Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "austrianfilm3", Name = "Austrian Film Museum", Url = "https://www.filmmuseum.at/jart/prj3/filmmuseum/main.jart?rel=en", Category = "classics", Description = "Austrian Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "35mm3", Name = "35mm", Url = "https://35mm.online/en", Category = "classics", Description = "Polish Film Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "travelfilm3", Name = "The Travel Film Archive", Url = "https://travelfilmarchive.com/", Category = "classics", Description = "Archival Travel Footage", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "dvidshub3", Name = "DVIDS", Url = "https://www.dvidshub.net/", Category = "classics", Description = "Military Video Archives", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "iwm3", Name = "Imperial War Museums", Url = "https://www.iwm.org.uk/", Category = "classics", Description = "British & Commonwealth War Footage", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "atomcentral3", Name = "Atom Central", Url = "https://www.atomcentral.com/", Category = "classics", Description = "Historical Archive of Atomic & Nuclear Tests", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "oldtimey3", Name = "Old Timey Computer Show", Url = "https://otcs.minuspoint.com/schedule.html", Category = "classics", Description = "Live Retro Computer / Game Media Streams", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "retrostrange3", Name = "RetroStrange", Url = "https://retrostrange.com/", Category = "classics", Description = "Rare / Vintage / Obscure Media Stream", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "exptv3", Name = "EXP TV", Url = "https://linktr.ee/exp.tv", Category = "classics", Description = "Rare / Vintage / Obscure Media Stream", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "versusmedia3", Name = "VersusMedia", Url = "https://watch.versusmedia.com/live-channel", Category = "classics", Description = "Independent Film Stream", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ytch3", Name = "YTCH", Url = "https://ytch.tv/", Category = "classics", Description = "Random YouTube Streams / Custom Channels", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "channelsurfer3", Name = "Channel Surfer", Url = "https://channelsurfer.tv/", Category = "classics", Description = "Random YouTube Streams / Custom Channels", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "freetvz3", Name = "FreeTVz", Url = "https://freetvz.com/", Category = "classics", Description = "Random YouTube Streams / Custom Channels", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rifftrx3", Name = "RiffTrax Twitch", Url = "https://www.twitch.tv/rifftrax", Category = "classics", Description = "RiffTrax Live Streams / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ttn3", Name = "TTN", Url = "https://watchttn.com/", Category = "classics", Description = "Random Streams / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "reacttv3", Name = "React.tv", Url = "https://react.tv/", Category = "classics", Description = "Random Streams", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "baked3", Name = "Baked.live", Url = "https://baked.live/", Category = "classics", Description = "Random User Channels / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "cytube3", Name = "cytube", Url = "https://cytu.be/", Category = "classics", Description = "Random User Channels / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vaughnlive3", Name = "VaughnLive", Url = "https://vaughn.live/browse/misc", Category = "classics", Description = "Random User Channels / Chat", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "motionbox3", Name = "MotionBox", Url = "https://omega.ggg/MotionBox/", Category = "classics", Description = "Online Video Aggregation App", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "peertube3", Name = "PeerTube", Url = "https://joinpeertube.org/", Category = "classics", Description = "Decentralized Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rutube3", Name = "rutube", Url = "https://rutube.ru", Category = "classics", Description = "Russian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "videa3", Name = "Videa", Url = "https://videa.hu/", Category = "classics", Description = "Hungarian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vanillo3", Name = "Vanillo", Url = "https://vanillo.tv/", Category = "classics", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "naver3", Name = "Naver", Url = "https://tv.naver.com/", Category = "classics", Description = "Korean Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ultimedia3", Name = "ultimedia", Url = "https://www.ultimedia.com/", Category = "classics", Description = "French Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "vk3", Name = "VK", Url = "https://vkvideo.ru/", Category = "classics", Description = "Russian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "ok3", Name = "OK", Url = "https://ok.ru/video", Category = "classics", Description = "Russian Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "bilibili3", Name = "BiliBili", Url = "https://www.bilibili.com/", Category = "classics", Description = "Chinese Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "nicovideo3", Name = "Nico Nico", Url = "https://www.nicovideo.jp/", Category = "classics", Description = "Japanese Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "typetype3", Name = "TypeType", Url = "https://watch.typetype.video/", Category = "classics", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "tape3", Name = "Tape", Url = "https://github.com/tapexyz/tape", Category = "classics", Description = "Video Streaming", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "interactive3", Name = "Interactive Player", Url = "https://github.com/Eveep23/Interactive-Player", Category = "classics", Description = "Interactive Content Player / Emulator", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "zero1cine3", Name = "Zero1Cine", Url = "https://zero1cine.com/", Category = "classics", Description = "AI Generated Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "homemovies3", Name = "HomeMovies101", Url = "https://www.homemovies100.it/en/", Category = "classics", Description = "Home Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "3dsmovies3", Name = "3DS Movies", Url = "https://rentry.co/FMHYB64#_3dsm", Category = "classics", Description = "3D Movies for 3DS Handhelds", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "kodasusaka3", Name = "Kodasusaka", Url = "https://kodasusaka.com/movies", Category = "classics", Description = "Rare / Classic Japanese Movies", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "rohesia3", Name = "Rohesia Hamilton Metcalfe", Url = "https://www.panix.com/~hamiltro/links/", Category = "classics", Description = "Experimental Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "arko3", Name = "Argo", Url = "https://web.watchargo.com/", Category = "classics", Description = "Short Films", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "wco3", Name = "WCO", Url = "https://www.wco.tv/", Category = "classics", Description = "TV / Movies / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "funnier3", Name = "FunnierMoments", Url = "https://www.funniermoments.net/", Category = "classics", Description = "TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "megacartoons3", Name = "MegaCartoons", Url = "https://www.megacartoons.net/", Category = "classics", Description = "TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "b983", Name = "B98", Url = "https://www.b98.tv/home/", Category = "classics", Description = "Classic / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "toontales3", Name = "ToonTales", Url = "https://www.toontales.net/", Category = "classics", Description = "Classic / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "supercartoons3", Name = "SuperCartoons", Url = "https://www.supercartoons.net/", Category = "classics", Description = "Classic / TV", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "japaneseanimated3", Name = "Japanese Animated Film Classics", Url = "https://animation.filmarchives.jp/index.html", Category = "classics", Description = "Japanese Animation Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "eddvault3", Name = "Eddvault", Url = "https://eddvault.notion.site/Eddvault-3b7700da57bd4d429b927c1733ccb772", Category = "classics", Description = "Eddsworld Content Archive", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "piratexplay3", Name = "PirateXplay", Url = "https://piratexplay.cc/home", Category = "classics", Description = "TV / Movies / Anime", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "justwatch3", Name = "JustWatch", Url = "https://www.justwatch.com/us?monetization_types=free", Category = "classics", Description = "Free w/ Ads Directory", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "freegreatmovies3", Name = "FreeGreatMovies", Url = "https://www.freegreatmovies.com/", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "voleflix3", Name = "Voleflix", Url = "https://vole.wtf/voleflix/", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "openculture3", Name = "OpenCulture", Url = "https://www.openenculture.com/freemoviesonline", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "moviesfoundonline3", Name = "MoviesFoundOnline", Url = "https://moviesfoundonline.com/", Category = "classics", Description = "YouTube Movie Collections", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "officialytmovies3", Name = "Official YT Movies", Url = "https://www.youtube.com/feed/storefront?bp=ogUCKAY%3D", Category = "classics", Description = "YouTube Movie Collections / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "primevideo3", Name = "Prime Video Free", Url = "https://www.amazon.com/gp/video/storefront/?ie=UTF8&contentId=freetv", Category = "classics", Description = "Movies / TV / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "roku3", Name = "Roku", Url = "https://therokuchannel.roku.com/", Category = "classics", Description = "Movies / TV / US Only", Quality = "1080p", HasAutoNext = false },
            new StreamingSource { Id = "darkroom3", Name = "DarkRoom", Url = "https://www.darkroom.film/", Category = "classics", Description = "Movies / TV / US Only / Requires Sign-Up", Quality = "1080p", HasAutoNext = false }
        };

        public FmhySourceService(IServerConfigurationManager configManager)
        {
            _configManager = configManager;
            _cache = new Dictionary<string, List<MovieItem>>();
            _lastCacheUpdate = DateTime.MinValue;

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer()
            };

            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Gets all available streaming sources from fmhy.net.
        /// </summary>
        public List<StreamingSource> GetAllSources()
        {
            return DefaultSources;
        }

        /// <summary>
        /// Gets enabled sources based on configuration.
        /// </summary>
        public List<StreamingSource> GetEnabledSources()
        {
            var config = FmhyPlugin.Instance?.Configuration;
            if (config?.EnabledSources?.Count > 0)
            {
                return DefaultSources.Where(s => config.EnabledSources.Any(es => es.Id == s.Id && es.IsEnabled)).ToList();
            }
            return DefaultSources.Where(s => s.IsEnabled).ToList();
        }

        /// <summary>
        /// Searches for movies across all enabled sources.
        /// </summary>
        public async Task<SearchResult> SearchMoviesAsync(string query, int page = 1, int pageSize = 20)
        {
            var sources = GetEnabledSources();
            var allMovies = new List<MovieItem>();

            foreach (var source in sources)
            {
                try
                {
                    var movies = await SearchSourceAsync(source, query);
                    allMovies.AddRange(movies);
                }
                catch
                {
                    // Continue with other sources if one fails
                }
            }

            var totalResults = allMovies.Count;
            var totalPages = (int)Math.Ceiling((double)totalResults / pageSize);
            var pagedMovies = allMovies.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new SearchResult
            {
                Movies = pagedMovies,
                TotalResults = totalResults,
                Page = page,
                TotalPages = totalPages
            };
        }

        /// <summary>
        /// Gets featured/trending movies from all sources.
        /// </summary>
        public async Task<List<MovieItem>> GetFeaturedMoviesAsync()
        {
            var cacheKey = "featured";
            if (_cache.ContainsKey(cacheKey) && DateTime.Now - _lastCacheUpdate < TimeSpan.FromMinutes(60))
            {
                return _cache[cacheKey];
            }

            var sources = GetEnabledSources();
            var allMovies = new List<MovieItem>();

            foreach (var source in sources.Take(10))
            {
                try
                {
                    var movies = await GetFeaturedFromSourceAsync(source);
                    allMovies.AddRange(movies);
                }
                catch
                {
                    // Continue with other sources
                }
            }

            _cache[cacheKey] = allMovies;
            _lastCacheUpdate = DateTime.Now;
            return allMovies;
        }

        /// <summary>
        /// Gets movies by category/genre.
        /// </summary>
        public async Task<List<MovieItem>> GetMoviesByCategoryAsync(string category)
        {
            var sources = GetEnabledSources().Where(s => s.Category == category).ToList();
            var allMovies = new List<MovieItem>();

            foreach (var source in sources)
            {
                try
                {
                    var movies = await GetFeaturedFromSourceAsync(source);
                    allMovies.AddRange(movies);
                }
                catch
                {
                    // Continue with other sources
                }
            }

            return allMovies;
        }

        /// <summary>
        /// Gets all available categories.
        /// </summary>
        public List<CategoryItem> GetCategories()
        {
            return new List<CategoryItem>
            {
                new CategoryItem { Id = "aggregator", Name = "Stream Aggregators", Description = "Sites that check and play streams within their own custom player", ItemCount = 50 },
                new CategoryItem { Id = "pstream", Name = "P-Stream Forks", Description = "Forks of the original P-Stream / movie-web projects", ItemCount = 13 },
                new CategoryItem { Id = "dedicated", Name = "Dedicated-Server", Description = "Sites with a focus on a single player / server", ItemCount = 30 },
                new CategoryItem { Id = "multi", Name = "Multi-Server", Description = "Sites that allow users to choose between multiple different players", ItemCount = 50 },
                new CategoryItem { Id = "free-ads", Name = "Free w/ Ads", Description = "Legal free streaming with advertisements", ItemCount = 20 },
                new CategoryItem { Id = "video", Name = "Video Streaming", Description = "General video streaming platforms", ItemCount = 10 },
                new CategoryItem { Id = "classics", Name = "Classics / Public Domain", Description = "Classic and public domain films", ItemCount = 100 }
            };
        }

        /// <summary>
        /// Gets stream URL for a specific movie.
        /// </summary>
        public async Task<List<StreamLink>> GetStreamLinksAsync(string sourceId, string movieId)
        {
            var source = DefaultSources.FirstOrDefault(s => s.Id == sourceId);
            if (source == null) return new List<StreamLink>();

            // Generate stream links based on source
            var links = new List<StreamLink>();

            // For most FMHY sources, we construct the watch URL
            var watchUrl = $"{source.Url}/watch/{movieId}";
            links.Add(new StreamLink
            {
                Url = watchUrl,
                Quality = source.Quality,
                Format = "hls",
                Language = "en",
                IsDirectPlay = false
            });

            // Add alternative quality links
            if (source.Quality == "4K")
            {
                links.Add(new StreamLink { Url = watchUrl, Quality = "1080p", Format = "hls", Language = "en", IsDirectPlay = false });
                links.Add(new StreamLink { Url = watchUrl, Quality = "720p", Format = "hls", Language = "en", IsDirectPlay = false });
            }
            else if (source.Quality == "1080p")
            {
                links.Add(new StreamLink { Url = watchUrl, Quality = "720p", Format = "hls", Language = "en", IsDirectPlay = false });
            }

            return links;
        }

        private async Task<List<MovieItem>> SearchSourceAsync(StreamingSource source, string query)
        {
            var movies = new List<MovieItem>();

            try
            {
                // Construct search URL based on source type
                var searchUrl = $"{source.Url}/search?q={HttpUtility.UrlEncode(query)}";

                var response = await _httpClient.GetAsync(searchUrl);
                if (!response.IsSuccessStatusCode) return movies;

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Parse search results - this is a generic parser
                // Each site has different HTML structure, so we use common patterns
                var movieNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'movie') or contains(@class, 'film') or contains(@class, 'item')]");

                if (movieNodes != null)
                {
                    foreach (var node in movieNodes.Take(20))
                    {
                        var titleNode = node.SelectSingleNode(".//h2|.//h3|.//a[contains(@class, 'title')]|.//span[contains(@class, 'title')]");
                        var linkNode = node.SelectSingleNode(".//a[@href]");
                        var imgNode = node.SelectSingleNode(".//img[@src]");

                        if (titleNode != null && linkNode != null)
                        {
                            movies.Add(new MovieItem
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                Title = titleNode.InnerText.Trim(),
                                StreamUrl = linkNode.GetAttributeValue("href", ""),
                                PosterUrl = imgNode?.GetAttributeValue("src", "") ?? "",
                                SourceId = source.Id,
                                SourceName = source.Name,
                                Quality = source.Quality,
                                Category = source.Category
                            });
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return movies;
        }

        private async Task<List<MovieItem>> GetFeaturedFromSourceAsync(StreamingSource source)
        {
            var movies = new List<MovieItem>();

            try
            {
                var response = await _httpClient.GetAsync(source.Url);
                if (!response.IsSuccessStatusCode) return movies;

                var html = await response.Content.ReadAsStringAsync();
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Parse featured movies from homepage
                var movieNodes = doc.DocumentNode.SelectNodes("//div[contains(@class, 'movie') or contains(@class, 'film') or contains(@class, 'item') or contains(@class, 'card')]");

                if (movieNodes != null)
                {
                    foreach (var node in movieNodes.Take(15))
                    {
                        var titleNode = node.SelectSingleNode(".//h2|.//h3|.//a[contains(@class, 'title')]|.//span[contains(@class, 'title')]|.//img[@alt]");
                        var linkNode = node.SelectSingleNode(".//a[@href]");
                        var imgNode = node.SelectSingleNode(".//img[@src]");

                        if (titleNode != null)
                        {
                            var title = titleNode.InnerText.Trim();
                            if (string.IsNullOrEmpty(title) && imgNode != null)
                            {
                                title = imgNode.GetAttributeValue("alt", "");
                            }

                            if (!string.IsNullOrEmpty(title))
                            {
                                movies.Add(new MovieItem
                                {
                                    Id = Guid.NewGuid().ToString("N"),
                                    Title = title,
                                    StreamUrl = linkNode?.GetAttributeValue("href", "") ?? $"{source.Url}/watch",
                                    PosterUrl = imgNode?.GetAttributeValue("src", "") ?? "",
                                    SourceId = source.Id,
                                    SourceName = source.Name,
                                    Quality = source.Quality,
                                    Category = source.Category
                                });
                            }
                        }
                    }
                }
            }
            catch
            {
                // Return empty list on error
            }

            return movies;
        }
    }
}
