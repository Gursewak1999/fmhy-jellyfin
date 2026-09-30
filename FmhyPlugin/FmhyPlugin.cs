using System;
using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Plugins;
using FmhyPlugin.Services;
using FmhyPlugin.Configuration;

namespace FmhyPlugin
{
    /// <summary>
    /// FMHY Free Movies Plugin for Jellyfin.
    /// Fetches free movie content from sources defined in fmhy.net
    /// and lists them in Jellyfin for streaming.
    /// </summary>
    public class FmhyPlugin : BasePlugin<FmhyPluginConfiguration>, IHasWebPages
    {
        private readonly FmhySourceService _sourceService;

        public FmhyPlugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer,
            FmhySourceService sourceService)
            : base(applicationPaths, xmlSerializer)
        {
            _sourceService = sourceService;
            Instance = this;
        }

        public static FmhyPlugin Instance { get; private set; }

        public override string Name => "FMHY Free Movies";
        public override string Description => "Stream free movies from sources listed on fmhy.net";
        public override Guid Id => new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

        /// <summary>
        /// Gets the plugin's web pages for the Jellyfin dashboard.
        /// </summary>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "fmhy_configuration",
                    EmbeddedResourcePath = "FmhyPlugin.Web.configuration.html",
                    EnableInMainMenu = true,
                    MenuSection = "Plugins",
                    DisplayName = "FMHY Free Movies"
                },
                new PluginPageInfo
                {
                    Name = "fmhy_browse",
                    EmbeddedResourcePath = "FmhyPlugin.Web.browse.html",
                    EnableInMainMenu = true,
                    MenuSection = "Plugins",
                    DisplayName = "FMHY Browse"
                }
            };
        }

        /// <summary>
        /// Gets the plugin's source service.
        /// </summary>
        public FmhySourceService SourceService => _sourceService;
    }
}