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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="IMainPresenter"/>.
    /// </summary>
    class MainPresenter : IMainPresenter
    {
        #region Fields
        /// <summary>
        /// The number of milliseconds to show a remote endpoint for before removing it.
        /// </summary>
        private const int ServerRequestMillisecondsBeforeDelete = 60000;

        /// <summary>
        /// The object that manages the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The UTC time that the version check was last performed.
        /// </summary>
        private DateTime _LastVersionCheck;

        /// <summary>
        /// The UTC time that the last save of polar plots was performed.
        /// </summary>
        private DateTime _LastAutoSavePolarPlots;

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

        /// <summary>
        /// The object that looks after fetching the configuration for us.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// A map between the feed counter identifiers and their cooked values.
        /// </summary>
        private Dictionary<int, FeedStatus> _FeedCounterMap = new Dictionary<int,FeedStatus>();

        /// <summary>
        /// A spin lock that protects access to the <see cref="_FeedCounterMap"/> add / remove / lookup operations.
        /// </summary>
        private ObsoleteSpinLock _FeedCounterMapSpinLock = new ObsoleteSpinLock();

        /// <summary>
        /// The object that helps assigning values to <see cref="FeedStatus"/> objects.
        /// </summary>
        private ViewDtoHelper<FeedStatus> _FeedStatusHelper = new ViewDtoHelper<FeedStatus>((r,v) => r.DataVersion = v);

        /// <summary>
        /// A list of current connections to the web server.
        /// </summary>
        private List<ServerRequest> _ServerRequests = new List<ServerRequest>();

        /// <summary>
        /// A spin lock that protects access to the <see cref="_ServerRequests"/> add / remove / find operations.
        /// </summary>
        private ObsoleteSpinLock _ServerRequestsSpinLock = new ObsoleteSpinLock();
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
            _Clock = Factory.Resolve<IClock>();
            _LastAutoSavePolarPlots = _Clock.UtcNow;
            _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
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
            View.LogFileName = Factory.ResolveSingleton<ILog>().FileName;
            View.InvalidPluginCount = Factory.ResolveSingleton<IPluginManager>().IgnoredPlugins.Count;

            var heartbeatService = Factory.ResolveSingleton<IHeartbeatService>();
            heartbeatService.SlowTick += HeartbeatService_SlowTick;
            heartbeatService.FastTick += HeartbeatService_FastTick;

            var newVersionChecker = Factory.ResolveSingleton<INewVersionChecker>();
            newVersionChecker.NewVersionAvailable += NewVersionChecker_NewVersionAvailable;

            _FeedManager = Factory.ResolveSingleton<IFeedManager>();
            _FeedManager.ConnectionStateChanged += FeedManager_ConnectionStateChanged;
            _FeedManager.FeedsChanged += FeedManager_FeedsChanged;
            UpdateFeedCounters();
            View.ShowFeeds(GetFeedCounters());

            _WebServer = Factory.ResolveSingleton<IAutoConfigWebServer>().WebServer;
            _WebServer.ExternalAddressChanged += WebServer_ExternalAddressChanged;
            _WebServer.OnlineChanged += WebServer_OnlineChanged;
            _WebServer.ResponseSent += WebServer_ResponseSent;
            View.WebServerIsOnline = _WebServer.Online;
            View.WebServerLocalAddress = _WebServer.LocalAddress;
            View.WebServerNetworkAddress = _WebServer.NetworkAddress;
            View.WebServerExternalAddress = _WebServer.ExternalAddress;

            _RebroadcastServerManager = Factory.ResolveSingleton<IRebroadcastServerManager>();
            DoDisplayRebroadcastServerConnections();

            _SharedConfiguration.ConfigurationChanged += SharedConfiguration_ConfigurationChanged;

            DisplayConfigurationSettings();

            View.CheckForNewVersion += View_CheckForNewVersion;
            View.ReconnectFeed += View_ReconnectFeed;
            View.ResetPolarPlot += View_ResetPolarPlot;
            View.ToggleServerStatus += View_ToggleServerStatus;
            View.ToggleUPnpStatus += View_ToggleUPnpStatus;
        }

        /// <summary>
        /// Updates the display of any configuration settings we're showing on the view.
        /// </summary>
        private void DisplayConfigurationSettings()
        {
            var configuration = _SharedConfiguration.Get();
            View.RebroadcastServersConfiguration = Describe.RebroadcastSettingsCollection(configuration.RebroadcastSettings);
        }
        #endregion

        #region GetReceiverFeeds, GetFeedConfigurationObject
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IFeed[] GetReceiverFeeds()
        {
            return Factory.ResolveSingleton<IFeedManager>()
                .Feeds
                .Where(r => !(r is IMergedFeedFeed))
                .ToArray();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="feedId"></param>
        /// <returns></returns>
        public object GetFeedConfigurationObject(int feedId)
        {
            object result = null;

            var configuration = _SharedConfiguration.Get();
            result = configuration.Receivers.FirstOrDefault(r => r.UniqueId == feedId);
            if(result == null) result = configuration.MergedFeeds.FirstOrDefault(r => r.UniqueId == feedId);

            return result;
        }
        #endregion

        #region PerformPeriodicChecks
        /// <summary>
        /// Performs checks, usually invoked on a background thread by the heartbeat service.
        /// </summary>
        private void PerformPeriodicChecks()
        {
            var configuration = _SharedConfiguration.Get();
            var now = _Clock.UtcNow;
            var log = Factory.ResolveSingleton<ILog>();

            try {
                PerformVersionCheck(configuration, now);
            } catch(Exception ex) {
                log.WriteLine("Caught exception while checking for new version: {0}", ex.ToString());
            }

            try {
                PerformAutoSavePolarPlots(configuration, now);
            } catch(Exception ex) {
                log.WriteLine("Caught exception while auto-saving polar plots: {0}", ex.ToString());
            }

            try {
                RemoveOldServerRequestEntries();
            } catch(Exception ex) {
                log.WriteLine("Caught exception while cleaning up server requests: {0}", ex);
            }
        }

        private void PerformVersionCheck(Configuration configuration, DateTime now)
        {
            if(configuration.VersionCheckSettings.CheckAutomatically && configuration.VersionCheckSettings.CheckPeriodDays > 0) {
                if((now - _LastVersionCheck).TotalDays >= configuration.VersionCheckSettings.CheckPeriodDays) {
                    _LastVersionCheck = now;

                    var newVersionChecker = Factory.ResolveSingleton<INewVersionChecker>();
                    newVersionChecker.CheckForNewVersion();
                }
            }
        }

        private void PerformAutoSavePolarPlots(Configuration configuration, DateTime now)
        {
            if(configuration.BaseStationSettings.AutoSavePolarPlotsMinutes > 0) {
                if((now - _LastAutoSavePolarPlots).TotalMinutes >= configuration.BaseStationSettings.AutoSavePolarPlotsMinutes) {
                    _LastAutoSavePolarPlots = now;

                    var storage = Factory.ResolveSingleton<ISavedPolarPlotStorage>();
                    storage.Save();
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
                var versionChecker = Factory.ResolveSingleton<INewVersionChecker>();
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

        #region ServerRequest - UpdateServerRequestEntry
        /// <summary>
        /// Called every time the web server sends a response to a request. Uses this information to update
        /// the <see cref="_ServerRequests"/> list.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="url"></param>
        /// <param name="bytesSent"></param>
        /// <param name="userName"></param>
        private void UpdateServerRequestEntry(IPEndPoint remoteEndPoint, string url, long bytesSent, string userName)
        {
            if(bytesSent > 0) {
                ServerRequest serverRequest = null;
                _ServerRequestsSpinLock.Lock();
                try {
                    serverRequest = _ServerRequests.FirstOrDefault(r => {
                        return /* showPortNumber ? r.RemoteEndPoint.Equals(remoteEndPoint) :*/ r.RemoteEndPoint.Address.Equals(remoteEndPoint.Address);
                    });
                    if(serverRequest == null) {
                        serverRequest = new ServerRequest() {
                            RemoteEndPoint = remoteEndPoint,
                        };
                        _ServerRequests.Add(serverRequest);
                    }
                } finally {
                    _ServerRequestsSpinLock.Unlock();
                }

                ++serverRequest.DataVersion;
                serverRequest.LastRequest = _Clock.UtcNow;
                serverRequest.BytesSent += bytesSent;
                serverRequest.LastUrl = url;
                serverRequest.UserName = userName;
            }
        }

        /// <summary>
        /// Deletes server request entries that have not been updated within so-many milliseconds.
        /// </summary>
        private void RemoveOldServerRequestEntries()
        {
            _ServerRequestsSpinLock.Lock();
            try {
                var threshold = _Clock.UtcNow.AddMilliseconds(-ServerRequestMillisecondsBeforeDelete);
                _ServerRequests.RemoveAll(r => r.LastRequest <= threshold);
            } finally {
                _ServerRequestsSpinLock.Unlock();
            }
        }

        /// <summary>
        /// Returns an array of all server request objects being held by the presenter.
        /// </summary>
        /// <returns></returns>
        private ServerRequest[] GetServerRequests()
        {
            _ServerRequestsSpinLock.Lock();
            try {
                return _ServerRequests.ToArray();
            } finally {
                _ServerRequestsSpinLock.Unlock();
            }
        }
        #endregion

        #region UpdateFeedCounters
        /// <summary>
        /// Returns an array of feed counters.
        /// </summary>
        /// <returns></returns>
        private FeedStatus[] GetFeedCounters()
        {
            _FeedCounterMapSpinLock.Lock();
            try {
                return _FeedCounterMap.Values.Select(r => r.Clone()).OfType<FeedStatus>().ToArray();
            } finally {
                _FeedCounterMapSpinLock.Unlock();
            }
        }

        /// <summary>
        /// Returns the feed counter for a feed.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        private FeedStatus GetFeedCounter(int uniqueId)
        {
            FeedStatus result = null;

            _FeedCounterMapSpinLock.Lock();
            try {
                _FeedCounterMap.TryGetValue(uniqueId, out result);
            } finally {
                _FeedCounterMapSpinLock.Unlock();
            }

            return result;
        }

        /// <summary>
        /// Updates the feed counters and sends the resulting list of cooked counters to the view.
        /// </summary>
        private void UpdateFeedCounters()
        {
            var feeds = _FeedManager.Feeds.ToArray();
            ++_FeedStatusHelper.DataVersion;

            _FeedCounterMapSpinLock.Lock();
            try {
                var deletedFeeds = _FeedCounterMap.Where(r => !feeds.Any(i => i.UniqueId == r.Key)).ToArray();
                foreach(var deletedFeed in deletedFeeds) {
                    _FeedCounterMap.Remove(deletedFeed.Key);
                }
            } finally {
                _FeedCounterMapSpinLock.Unlock();
            }

            using(new CultureSwitcher()) {
                foreach(var feed in feeds) {
                    UpdateFeedCounter(feed, suppressCultureSwitch: true);
                }
            }
        }

        /// <summary>
        /// Adds or updates a feed counter in <see cref="_FeedCounterMap"/>.
        /// </summary>
        /// <param name="feed"></param>
        /// <param name="suppressCultureSwitch"></param>
        private FeedStatus UpdateFeedCounter(IFeed feed, bool suppressCultureSwitch = false)
        {
            var cultureSwitcher = suppressCultureSwitch ? null : new CultureSwitcher();

            try {
                FeedStatus feedStatus;
                _FeedCounterMapSpinLock.Lock();
                try {
                    if(!_FeedCounterMap.TryGetValue(feed.UniqueId, out feedStatus)) {
                        feedStatus = new FeedStatus() {
                            FeedId = feed.UniqueId,
                        };
                        _FeedCounterMap.Add(feedStatus.FeedId, feedStatus);
                    }
                } finally {
                    _FeedCounterMapSpinLock.Unlock();
                }

                var listener = (feed as INetworkFeed)?.Listener;
                var aircraftList = feed.AircraftList;

                var isMerged = listener is IMergedFeedListener;
                var hasPolarPlot = (aircraftList as IPolarPlottingAircraftList)?.PolarPlotter != null;
                var hasAircraftList = aircraftList?.IsTracking ?? false;
                var connectionStatus = feed.ConnectionStatus;
                var connectionStatusDescription = Describe.ConnectionStatus(connectionStatus);
                var totalAircraft = aircraftList?.Count ?? 0;
                var totalBadMessages = listener?.TotalBadMessages ?? 0;
                var totalMessages = listener?.TotalMessages ?? 0;

                _FeedStatusHelper.Update(feedStatus, feedStatus.ConnectionStatus,               connectionStatus,               (r,v) => r.ConnectionStatus = v);
                _FeedStatusHelper.Update(feedStatus, feedStatus.ConnectionStatusDescription,    connectionStatusDescription,    (r,v) => r.ConnectionStatusDescription = v);
                _FeedStatusHelper.Update(feedStatus, feedStatus.HasAircraftList,                hasAircraftList,                (r,v) => r.HasAircraftList = v);
                _FeedStatusHelper.Update(feedStatus, feedStatus.HasPolarPlot,                   hasPolarPlot,                   (r,v) => r.HasPolarPlot = v);
                _FeedStatusHelper.Update(feedStatus, feedStatus.IsMergedFeed,                   isMerged,                       (r,v) => r.IsMergedFeed = v);
                _FeedStatusHelper.Update(feedStatus, feedStatus.Name,                           feed.Name,                      (r,v) => r.Name = v);
                _FeedStatusHelper.Update(feedStatus, feedStatus.TotalAircraft,                  totalAircraft,                  (r,v) => r.TotalAircraft = v);
                _FeedStatusHelper.Update(feedStatus, feedStatus.TotalBadMessages,               totalBadMessages,               (r,v) => r.TotalBadMessages = v);
                _FeedStatusHelper.Update(feedStatus, feedStatus.TotalMessages,                  totalMessages,                  (r,v) => r.TotalMessages = v);

                return feedStatus;
            } finally {
                if(cultureSwitcher != null) cultureSwitcher.Dispose();
            }
        }
        #endregion

        #region Events consumed
        private void SharedConfiguration_ConfigurationChanged(object sender, EventArgs args)
        {
            DisplayConfigurationSettings();
        }

        private void HeartbeatService_SlowTick(object sender, EventArgs args)
        {
            PerformPeriodicChecks();
        }

        private void HeartbeatService_FastTick(object sender, EventArgs args)
        {
            View.ShowServerRequests(GetServerRequests());

            UpdateFeedCounters();
            View.UpdateFeedCounters(GetFeedCounters());

            DoDisplayRebroadcastServerConnections();
        }

        private void NewVersionChecker_NewVersionAvailable(object sender, EventArgs args)
        {
            var checker = (INewVersionChecker)sender;
            View.NewVersionDownloadUrl = checker.DownloadUrl;
            View.NewVersionAvailable = checker.IsNewVersionAvailable;
        }

        private void FeedManager_ConnectionStateChanged(object sender, EventArgs<IFeed> args)
        {
            if(args.Value != null) {
                var feedStatus = UpdateFeedCounter(args.Value);
                View.ShowFeedConnectionStatus(feedStatus);
            }
        }

        private void FeedManager_FeedsChanged(object sender, EventArgs args)
        {
            UpdateFeedCounters();
            View.ShowFeeds(GetFeedCounters());
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
                    feed.Disconnect();
                    feed.Connect();
                } catch(Exception ex) {
                    View.BubbleExceptionToGui(ex);
                }
            });
        }

        void View_ResetPolarPlot(object sender, EventArgs<IFeed> args)
        {
            var feed = args.Value;
            if(feed != null) {
                var aircraftList = feed.AircraftList;
                var polarPlotter = aircraftList == null ? null : (aircraftList as IPolarPlottingAircraftList)?.PolarPlotter;
                if(polarPlotter != null) {
                    polarPlotter.ClearPolarPlots();
                }
            }
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
            UpdateServerRequestEntry(args.Request.RemoteEndPoint, args.UrlRequested, args.BytesSent, args.UserName);
        }
        #endregion
    }
}
