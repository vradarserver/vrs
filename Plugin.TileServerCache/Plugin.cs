// Copyright © 2019 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AWhewell.Owin.Interface;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Plugin.TileServerCache
{
    /// <summary>
    /// The class that VRS creates and calls when it wants to let the plugin get its fingers into the pie.
    /// </summary>
    public class Plugin : IPlugin_V2
    {
        /// <summary>
        /// The single instance of the plugin created by VRS.
        /// </summary>
        internal static Plugin Singleton { get; private set; }

        /// <summary>
        /// The single instance of the tile server settings manager that is wrapping the property manager.
        /// </summary>
        internal static TileServerSettingsManagerWrapper TileServerSettingsManagerWrapper { get; private set; }

        /// <summary>
        /// Provides multi-threaded write protection for the <see cref="_Options"/> reference (but not the
        /// object's contents).
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The currently active options. Always take a copy of the reference before use, never change the
        /// content, always overwrite within a lock on _SyncLock.
        /// </summary>
        private Options _Options;

        /// <summary>
        /// Gets or sets the current options.
        /// </summary>
        internal Options Options
        {
            get {
                var copyOfReference = _Options;
                return copyOfReference;
            }
            set {
                lock(_SyncLock) {
                    _Options = value;
                    RefreshStatusDescription();
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Id => "VirtualRadar.Plugin.TileServerCache";

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string PluginFolder { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name => TileServerCacheStrings.PluginName;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Version => "3.0.0";

        private string _Status;
        /// <summary>
        /// See interfacce docs.
        /// </summary>
        public string Status
        {
            get => _Status;
            set {
                var normalised = (value ?? "").Trim();
                if(_Status != normalised) {
                    _Status = normalised;
                    OnStatusChanged(EventArgs.Empty);
                }
            }
        }

        private string _StatusDescription;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StatusDescription
        {
            get => _StatusDescription;
            set {
                var normalised = (value ?? "").Trim();
                if(_StatusDescription != normalised) {
                    _StatusDescription = normalised;
                    OnStatusChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool HasOptions => true;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler StatusChanged;

        /// <summary>
        /// Raises <see cref="StatusChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnStatusChanged(EventArgs args)
        {
            StatusChanged?.Invoke(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="parameters"></param>
        public void Startup(PluginStartupParameters parameters)
        {
            TileCache.Initialise();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void GuiThreadStartup()
        {
            var webAdminViewManager = Factory.ResolveSingleton<IWebAdminViewManager>();
            webAdminViewManager.RegisterTranslations(typeof(TileServerCacheStrings), "TileServerCachePlugin");
            webAdminViewManager.AddWebAdminView(
                new WebAdminView(
                    "/WebAdmin/",
                    "TileServerCachePluginOptions.html",
                    TileServerCacheStrings.WebAdminMenuName,
                    () => new WebAdmin.OptionsView(),
                    typeof(TileServerCacheStrings)
                ) {
                    Plugin = this,
                }
            );

            webAdminViewManager.AddWebAdminView(
                new WebAdminView(
                    "/WebAdmin/",
                    "TileServerCacheRecentRequests.html",
                    null,//TileServerCacheStrings.WebAdminRecentRequestsName,
                    () => new WebAdmin.RecentRequestsView(),
                    typeof(TileServerCacheStrings)
                )
            );

            webAdminViewManager.RegisterWebAdminViewFolder(PluginFolder, "Web");
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="classFactory"></param>
        public void RegisterImplementations(IClassFactory classFactory)
        {
            Singleton = this;
            lock(_SyncLock) {
                Options = OptionsStorage.Load();
            }

            TileServerSettingsManagerWrapper = TileServerSettingsManagerWrapper.Initialise(classFactory);
        }

        /// <summary>
        /// See V2 interface docs.
        /// </summary>
        public void RegisterOwinMiddleware()
        {
            Factory.ResolveSingleton<IWebSitePipelineBuilder>()
            .PipelineBuilder
            .RegisterCallback(
                (IPipelineBuilderEnvironment builderEnv) => {
                    var middleware = new WebServerV3Middleware();
                    builderEnv.UseMiddlewareBuilder(
                        middleware.AppFuncBuilder
                    );
                },
                StandardPipelinePriority.HighestVrsContentMiddlewarePriority + 1000
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ShowWinFormsOptionsUI()
        {
            using(var view = new WinForms.OptionsView()) {
                var options = OptionsStorage.Load();

                view.DataVersion =                      options.DataVersion;
                view.IsPluginEnabled =                  options.IsPluginEnabled;
                view.IsOfflineModeEnabled =             options.IsOfflineModeEnabled;
                view.UseDefaultCacheFolder =            options.UseDefaultCacheFolder;
                view.CacheFolderOverride =              options.CacheFolderOverride;
                view.TileServerTimeoutSeconds =         options.TileServerTimeoutSeconds;
                view.CacheMapTiles =                    options.CacheMapTiles;
                view.CacheLayerTiles =                  options.CacheLayerTiles;

                if(view.DisplayView()) {
                    options.DataVersion =                   view.DataVersion;
                    options.IsPluginEnabled =               view.IsPluginEnabled;
                    options.IsOfflineModeEnabled =          view.IsOfflineModeEnabled;
                    options.CacheFolderOverride =           view.UseDefaultCacheFolder ? null : view.CacheFolderOverride;
                    options.TileServerTimeoutSeconds =      view.TileServerTimeoutSeconds;
                    options.CacheMapTiles =                 view.CacheMapTiles;
                    options.CacheLayerTiles =               view.CacheLayerTiles;

                    OptionsStorage.Save(options);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Shutdown()
        {
        }

        /// <summary>
        /// Updates the plugin status.
        /// </summary>
        internal void RefreshStatusDescription()
        {
            Status =
                Options.IsPluginEnabled
                    ? Options.IsOfflineModeEnabled
                        ? TileServerCacheStrings.PluginEnabledAndOfflineModeOn
                        : TileServerCacheStrings.PluginEnabled
                    : TileServerCacheStrings.PluginDisabled;

            StatusDescription = $"{TileServerCacheStrings.TilesServedFromCache}: {WebRequestHandler.CountServedFromCache:N0}";
        }
    }
}
