using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using EmbyBannerTextPlugin.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using System.Reflection;

namespace EmbyBannerTextPlugin
{
    public class BannerTextUpdater : IDisposable
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IServerConfigurationManager _configManager;
        private readonly ISessionManager _sessionManager;
        private readonly ILogger _logger;
        private System.Timers.Timer? _timer;
        private int _currentEntryIndex = 0;
        private readonly object _lock = new object();
        private bool _disposed = false;

        public BannerTextUpdater(ILibraryManager libraryManager, IServerConfigurationManager configManager, ISessionManager sessionManager, ILogManager logManager)
        {
            _libraryManager = libraryManager;
            _configManager = configManager;
            _sessionManager = sessionManager;
            _logger = logManager.GetLogger(GetType().Name);
        }

        public void Start()
        {
            if (_disposed) return;
            
            lock (_lock)
            {
                Stop();
            
                var config = Plugin.Instance?.Configuration;
            if (config == null || config.BannerEntries == null || config.BannerEntries.Count == 0)
            {
                if (config?.EnableLogging == true)
                {
                    _logger.Info("No banner entries configured");
                }
                return;
            }

            var intervalSeconds = Math.Max(1, config.UpdateIntervalSeconds);
            
            _timer = new System.Timers.Timer(intervalSeconds * 1000);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
            
            if (config.EnableLogging)
            {
                _logger.Info($"Banner text updater started with interval of {intervalSeconds} seconds");
            }
            
                UpdateBannerText();
            }
        }

        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Dispose();
                _timer = null;
                
                var config = Plugin.Instance?.Configuration;
                if (config?.EnableLogging == true)
                {
                    _logger.Info("Banner text updater stopped");
                }
            }
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            UpdateBannerText();
        }

        public void UpdateBannerText()
        {
            if (_disposed) return;
            
            lock (_lock)
            {
                if (_disposed) return;
                
                try
                {
                    var config = Plugin.Instance?.Configuration;
                if (config == null || config.BannerEntries == null || config.BannerEntries.Count == 0)
                {
                    if (config?.EnableLogging == true)
                    {
                        _logger.Error("Plugin configuration is null or has no entries");
                    }
                    return;
                }

                if (_currentEntryIndex >= config.BannerEntries.Count)
                {
                    _currentEntryIndex = 0;
                }

                var entry = config.BannerEntries[_currentEntryIndex];
                var bannerText = entry.Text;

                if (!string.IsNullOrWhiteSpace(bannerText))
                {
                    bannerText = ProcessPlaceholders(bannerText, config);
                }

                var serverConfig = _configManager.Configuration;
                
                var bannerTextProperty = serverConfig.GetType().GetProperty("BannerText");
                if (bannerTextProperty != null)
                {
                    bannerTextProperty.SetValue(serverConfig, bannerText);
                    
                    _configManager.SaveConfiguration();
                    System.Threading.Thread.Sleep(10);
                    
                    if (config.EnableLogging)
                    {
                        _logger.Info($"Banner text updated (Entry {_currentEntryIndex + 1}/{config.BannerEntries.Count}): {bannerText}");
                    }
                }
                else
                {
                    if (config.EnableLogging)
                    {
                        _logger.Error("BannerText property not found in ServerConfiguration");
                    }
                }

                _currentEntryIndex++;
                if (_currentEntryIndex >= config.BannerEntries.Count)
                {
                    _currentEntryIndex = 0;
                }
                }
                catch (Exception ex)
                {
                    var config = Plugin.Instance?.Configuration;
                    if (config?.EnableLogging == true)
                    {
                        _logger.ErrorException("Error updating banner text", ex);
                    }
                }
            }
        }

        private string ProcessPlaceholders(string text, PluginConfiguration config)
        {
            if (text.Contains("{MovieCount}"))
            {
                try
                {
                    var movieCount = _libraryManager.GetItemList(new InternalItemsQuery
                    {
                        IsVirtualItem = false,
                        IncludeItemTypes = new[] { typeof(Movie).Name }
                    }).Length;
                    text = text.Replace("{MovieCount}", movieCount.ToString());
                }
                catch (Exception ex)
                {
                    if (config.EnableLogging)
                    {
                        _logger.ErrorException("Error getting movie count", ex);
                    }
                    text = text.Replace("{MovieCount}", "0");
                }
            }

            if (text.Contains("{ShowCount}"))
            {
                try
                {
                    var showCount = _libraryManager.GetItemList(new InternalItemsQuery
                    {
                        IsVirtualItem = false,
                        IncludeItemTypes = new[] { typeof(Series).Name }
                    }).Length;
                    text = text.Replace("{ShowCount}", showCount.ToString());
                }
                catch (Exception ex)
                {
                    if (config.EnableLogging)
                    {
                        _logger.ErrorException("Error getting show count", ex);
                    }
                    text = text.Replace("{ShowCount}", "0");
                }
            }

            if (text.Contains("{EpisodeCount}"))
            {
                try
                {
                    var episodeCount = _libraryManager.GetItemList(new InternalItemsQuery
                    {
                        IsVirtualItem = false,
                        IncludeItemTypes = new[] { typeof(Episode).Name }
                    }).Length;
                    text = text.Replace("{EpisodeCount}", episodeCount.ToString());
                }
                catch (Exception ex)
                {
                    if (config.EnableLogging)
                    {
                        _logger.ErrorException("Error getting episode count", ex);
                    }
                    text = text.Replace("{EpisodeCount}", "0");
                }
            }

            if (text.Contains("{VideoCount}"))
            {
                try
                {
                    var videoCount = _libraryManager.GetItemList(new InternalItemsQuery
                    {
                        IsVirtualItem = false,
                        IncludeItemTypes = new[] { typeof(Video).Name }
                    }).Length;
                    text = text.Replace("{VideoCount}", videoCount.ToString());
                }
                catch (Exception ex)
                {
                    if (config.EnableLogging)
                    {
                        _logger.ErrorException("Error getting video count", ex);
                    }
                    text = text.Replace("{VideoCount}", "0");
                }
            }

            if (text.Contains("{RunningStreams}"))
            {
                try
                {
                    var sessions = _sessionManager.Sessions;
                    if (sessions != null)
                    {
                        var sessionList = sessions.ToList();
                        var activeStreams = sessionList.Count(s => s != null && s.NowPlayingItem != null);
                        text = text.Replace("{RunningStreams}", activeStreams.ToString());
                    }
                    else
                    {
                        text = text.Replace("{RunningStreams}", "0");
                    }
                }
                catch (Exception ex)
                {
                    if (config.EnableLogging)
                    {
                        _logger.ErrorException("Error getting running streams count", ex);
                    }
                    text = text.Replace("{RunningStreams}", "0");
                }
            }

            if (text.Contains("{LatestMovies}"))
            {
                try
                {
                    var latestMovies = _libraryManager.GetItemList(new InternalItemsQuery
                    {
                        IsVirtualItem = false,
                        IncludeItemTypes = new[] { typeof(Movie).Name },
                        OrderBy = new[] { ("DateCreated", SortOrder.Descending) },
                        Limit = config.LatestMoviesCount
                    });

                    if (latestMovies != null && latestMovies.Length > 0)
                    {
                        var movieTitles = string.Join(", ", latestMovies.Select(m => m.Name));
                        text = text.Replace("{LatestMovies}", movieTitles);
                    }
                    else
                    {
                        text = text.Replace("{LatestMovies}", "");
                    }
                }
                catch (Exception ex)
                {
                    if (config.EnableLogging)
                    {
                        _logger.ErrorException("Error getting latest movies", ex);
                    }
                    text = text.Replace("{LatestMovies}", "");
                }
            }

            if (text.Contains("{LatestShows}"))
            {
                try
                {
                    var latestShows = _libraryManager.GetItemList(new InternalItemsQuery
                    {
                        IsVirtualItem = false,
                        IncludeItemTypes = new[] { typeof(Series).Name },
                        OrderBy = new[] { ("DateCreated", SortOrder.Descending) },
                        Limit = config.LatestShowsCount
                    });

                    if (latestShows != null && latestShows.Length > 0)
                    {
                        var showTitles = string.Join(", ", latestShows.Select(s => s.Name));
                        text = text.Replace("{LatestShows}", showTitles);
                    }
                    else
                    {
                        text = text.Replace("{LatestShows}", "");
                    }
                }
                catch (Exception ex)
                {
                    if (config.EnableLogging)
                    {
                        _logger.ErrorException("Error getting latest shows", ex);
                    }
                    text = text.Replace("{LatestShows}", "");
                }
            }

            return text;
        }

        public void Dispose()
        {
            if (_disposed) return;
            
            lock (_lock)
            {
                if (_disposed) return;
                _disposed = true;
                
                Stop();
            }
        }
    }
}
