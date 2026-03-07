using System.Collections.Generic;
using MediaBrowser.Model.Plugins;

namespace EmbyBannerTextPlugin.Configuration
{
    public class BannerTextEntry
    {
        public string Text { get; set; }

        public BannerTextEntry()
        {
            Text = string.Empty;
        }
    }

    public class PluginConfiguration : BasePluginConfiguration
    {
        public List<BannerTextEntry> BannerEntries { get; set; }
        public int UpdateIntervalSeconds { get; set; }
        public int LatestMoviesCount { get; set; }
        public int LatestShowsCount { get; set; }
        public bool EnableLogging { get; set; }
        
        public PluginConfiguration()
        {
            BannerEntries = new List<BannerTextEntry>();
            UpdateIntervalSeconds = 60;
            LatestMoviesCount = 5;
            LatestShowsCount = 5;
            EnableLogging = false;
        }
    }
}
