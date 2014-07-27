// Copyright © 2010 onwards, Andrew Whewell
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using System.Threading;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="IMainPresenter"/>.
    /// </summary>
    class MainPresenter : IMainPresenter
    {
        #region Fields
        /// <summary>
        /// The object that manages the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The UTC time that the version check was last performed.
        /// </summary>
        private DateTime _LastVersionCheck;

        /// <summary>
        /// A copy of a reference to the singleton feed manager - saves having to fetch it on every fast tick.
        /// </summary>
        private IFeedManager _FeedManager;

        /// <summary>
        /// The web server that we're using.
        /// </summary>
        private IWebServer _WebServer;

        /// <summary>
        /// The rebroadcast server manager that we're using.
        /// </summary>
        private IRebroadcastServerManager _RebroadcastServerManager;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IMainView View { get; private set; }

        private IUniversalPlugAndPlayManager _UPnpManager;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IUniversalPlugAndPlayManager UPnpManager
        {
            get { return _UPnpManager; }
            set
            {
                _UPnpManager = value;
                if(_UPnpManager != null) {
                    _UPnpManager.StateChanged += UPnpManager_StateChanged;
                    if(View != null) {
                        View.UPnpEnabled = _UPnpManager.IsEnabled;
                        View.UPnpRouterPresent = _UPnpManager.IsRouterPresent;
                        View.UPnpPortForwardingActive = _UPnpManager.PortForwardingPresent;
                    }
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MainPresenter()
        {
            _Clock = Factory.Singleton.Resolve<IClock>();
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(IMainView view)
        {
            if(view == null) throw new ArgumentNullException("view");
            View = view;
            View.LogFileName = Factory.Singleton.Resolve<ILog>().Singleton.FileName;
            View.InvalidPluginCount = Factory.Singleton.Resolve<IPluginManager>().Singleton.IgnoredPlugins.Count;

            var heartbeatService = Factory.Singleton.Resolve<IHeartbeatService>().Singleton;
            heartbeatService.SlowTick += HeartbeatService_SlowTick;
            heartbeatService.FastTick += HeartbeatService_FastTick;

            var newVersionChecker = Factory.Singleton.Resolve<INewVersionChecker>().Singleton;
            newVersionChecker.NewVersionAvailable += NewVersionChecker_NewVersionAvailable;

            _FeedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
            _FeedManager.ConnectionStateChanged += FeedManager_ConnectionStateChanged;
            _FeedManager.FeedsChanged += FeedManager_FeedsChanged;
            View.ShowFeeds(_FeedManager.Feeds);

            _WebServer = Factory.Singleton.Resolve<IAutoConfigWebServer>().Singleton.WebServer;
            _WebServer.ExternalAddressChanged += WebServer_ExternalAddressChanged;
            _WebServer.OnlineChanged += WebServer_OnlineChanged;
            _WebServer.ResponseSent += WebServer_ResponseSent;
            View.WebServerIsOnline = _WebServer.Online;
            View.WebServerLocalAddress = _WebServer.LocalAddress;
            View.WebServerNetworkAddress = _WebServer.NetworkAddress;
            View.WebServerExternalAddress = _WebServer.ExternalAddress;

            var pluginManager = Factory.Singleton.Resolve<IPluginManager>().Singleton;
            foreach(var plugin in pluginManager.LoadedPlugins) {
                plugin.GuiThreadStartup();
            }

            _RebroadcastServerManager = Factory.Singleton.Resolve<IRebroadcastServerManager>().Singleton;
            DoDisplayRebroadcastServerConnections();

            var configurationStorage = DisplayConfigurationSettings();
            configurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;

            View.CheckForNewVersion += View_CheckForNewVersion;
            View.ReconnectFeed += View_ReconnectFeed;
            View.ToggleServerStatus += View_ToggleServerStatus;
            View.ToggleUPnpStatus += View_ToggleUPnpStatus;
            View.RefreshTimerTicked += View_RefreshTimerTicked;
        }

        /// <summary>
        /// Updates the display of any configuration settings we're showing on the view.
        /// </summary>
        /// <returns></returns>
        private IConfigurationStorage DisplayConfigurationSettings()
        {
            var result = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;

            var configuration = result.Load();
            View.RebroadcastServersConfiguration = Describe.RebroadcastSettingsCollection(configuration.RebroadcastSettings);

            return result;
        }
        #endregion

        #region GetReceiverFeeds
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IFeed[] GetReceiverFeeds()
        {
            return Factory.Singleton.Resolve<IFeedManager>().Singleton.Feeds.Where(r => !(r.Listener is IMergedFeedListener)).ToArray();
        }
        #endregion

        #region PerformPeriodicChecks
        /// <summary>
        /// Performs checks, usually invoked on a background thread by the heartbeat service.
        /// </summary>
        private void PerformPeriodicChecks()
        {
            var configuration = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Load();

            var now = _Clock.UtcNow;

            if(configuration.VersionCheckSettings.CheckAutomatically && configuration.VersionCheckSettings.CheckPeriodDays > 0) {
                if((now - _LastVersionCheck).TotalDays >= configuration.VersionCheckSettings.CheckPeriodDays) {
                    try {
                        var newVersionChecker = Factory.Singleton.Resolve<INewVersionChecker>().Singleton;
                        newVersionChecker.CheckForNewVersion();
                    } catch(Exception ex) {
                        Debug.WriteLine(String.Format("MainPresenter.PerformPeriodicChecks caught exception: {0}", ex.ToString()));
                        var log = Factory.Singleton.Resolve<ILog>().Singleton;
                        log.WriteLine("Caught exception while automatically checking for new version: {0}", ex.ToString());
                    }
                    _LastVersionCheck = now;
                }
            }
        }
        #endregion

        #region Do...
        /// <summary>
        /// Runs the check for a new version manually. Unlike the periodic check this will be called on the GUI thread.
        /// </summary>
        private void DoManualCheckForNewVersion()
        {
            bool newVersionAvailable;

            var previousState = View.ShowBusy(true, null);
            try {
                var versionChecker = Factory.Singleton.Resolve<INewVersionChecker>().Singleton;
                newVersionAvailable = versionChecker.CheckForNewVersion();
            } finally {
                View.ShowBusy(false, previousState);
            }

            View.ShowManualVersionCheckResult(newVersionAvailable);
        }

        private void DoDisplayRebroadcastServerConnections()
        {
            var connections = _RebroadcastServerManager == null ? null : _RebroadcastServerManager.GetConnections();
            if(connections != null) View.ShowRebroadcastServerStatus(connections);
        }
        #endregion

        #region Events consumed
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            DisplayConfigurationSettings();
        }

        private void HeartbeatService_SlowTick(object sender, EventArgs args)
        {
            PerformPeriodicChecks();
        }

        private void HeartbeatService_FastTick(object sender, EventArgs args)
        {
            View.UpdateFeedCounters(_FeedManager.Feeds);
        }

        private void NewVersionChecker_NewVersionAvailable(object sender, EventArgs args)
        {
            var checker = (INewVersionChecker)sender;
            View.NewVersionDownloadUrl = checker.DownloadUrl;
            View.NewVersionAvailable = checker.IsNewVersionAvailable;
        }

        private void FeedManager_ConnectionStateChanged(object sender, EventArgs<IFeed> args)
        {
            View.ShowFeedConnectionStatus(args.Value);
        }

        private void FeedManager_FeedsChanged(object sender, EventArgs args)
        {
            var feedManager = (IFeedManager)sender;
            View.ShowFeeds(feedManager.Feeds);
        }

        private void View_CheckForNewVersion(object sender, EventArgs args)
        {
            DoManualCheckForNewVersion();
        }

        private void View_ReconnectFeed(object sender, EventArgs<IFeed> args)
        {
            ThreadPool.QueueUserWorkItem(r => {
                try {
                    var feed = args.Value;
                    feed.Listener.Disconnect();
                    feed.Listener.Connect(false);
                } catch(Exception ex) {
                    View.BubbleExceptionToGui(ex);
                }
            });
        }

        private void View_ToggleServerStatus(object sender, EventArgs args)
        {
            _WebServer.Online = !_WebServer.Online;
        }

        private void View_ToggleUPnpStatus(object sender, EventArgs args)
        {
            if(UPnpManager.PortForwardingPresent) UPnpManager.TakeServerOffInternet();
            else                                  UPnpManager.PutServerOntoInternet();
        }

        private void View_RefreshTimerTicked(object sender, EventArgs args)
        {
            DoDisplayRebroadcastServerConnections();
        }

        private void UPnpManager_StateChanged(object sender, EventArgs args)
        {
            View.UPnpPortForwardingActive = UPnpManager.PortForwardingPresent;
            View.UPnpEnabled = UPnpManager.IsEnabled;
            View.UPnpRouterPresent = UPnpManager.IsRouterPresent;
        }

        private void WebServer_ExternalAddressChanged(object sender, EventArgs args)
        {
            View.WebServerExternalAddress = _WebServer.ExternalAddress;
        }

        private void WebServer_OnlineChanged(object sender, EventArgs args)
        {
            View.WebServerIsOnline = _WebServer.Online;
        }

        private void WebServer_ResponseSent(object sender, ResponseSentEventArgs args)
        {
            View.ShowWebRequestHasBeenServiced(args.Request.RemoteEndPoint, args.UrlRequested, args.BytesSent);
        }
        #endregion
    }
}
