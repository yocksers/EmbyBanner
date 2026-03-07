using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EmbyBannerTextPlugin.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace EmbyBannerTextPlugin
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IHasThumbImage, IDisposable
    {
        private BannerTextUpdater _updater;
        private bool _disposed = false;
        private System.Threading.CancellationTokenSource? _startupCts;

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer, ILibraryManager libraryManager, IServerConfigurationManager configManager, ISessionManager sessionManager, ILogManager logManager)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
            _updater = new BannerTextUpdater(libraryManager, configManager, sessionManager, logManager);
            
            _startupCts = new System.Threading.CancellationTokenSource();
            var cts = _startupCts;
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2000, cts.Token);
                    if (!cts.Token.IsCancellationRequested)
                    {
                        var config = Configuration;
                        if (config != null)
                        {
                            _updater?.Start();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch
                {
                }
            }, cts.Token);
        }

        public static Plugin? Instance { get; private set; }

        public override string Name => "EmbyBanner";

        public override Guid Id => Guid.Parse("8a8f5c7d-9e2a-4b6f-a3d1-5e8c9b7a6f4e");

        public override string Description => "Manage server banner text with customizable content including latest additions and library statistics";

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".Images.logo.png")!;
        }

        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        public void RestartUpdater()
        {
            _updater?.Start();
        }

        public void UpdateBannerNow()
        {
            _updater?.UpdateBannerText();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _startupCts?.Cancel();
                _startupCts?.Dispose();
                _startupCts = null;
                
                _updater?.Dispose();
                _updater = null!;
                
                if (Instance == this)
                {
                    Instance = null;
                }
                
                _disposed = true;
            }
        }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = Name,
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                },
                new PluginPageInfo
                {
                    Name = "BannerTextConfigurationjs",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.js"
                }
            };
        }
    }
}
