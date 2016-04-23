// Copyright © 2014 onwards, Andrew Whewell
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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Plugin.WebAdmin.View.Main;

namespace VirtualRadar.Plugin.WebAdmin.View
{
    /// <summary>
    /// The class that carries the main view's data to the site.
    /// </summary>
    public class MainView : IMainView
    {
        private object _SyncLock = new object();
        private IMainPresenter _Presenter;
        private ViewModel _ViewModel;
        private IUniversalPlugAndPlayManager _UPnpManager;
        private ISimpleAircraftList _FlightSimulatorXAircraftList;

        public int InvalidPluginCount { get; set; }

        public bool NewVersionAvailable { get; set; }

        public string NewVersionDownloadUrl { get; set; }

        public bool UPnpEnabled { get; set; }

        public bool UPnpRouterPresent { get; set; }

        public bool UPnpPortForwardingActive { get; set; }

        public bool WebServerIsOnline { get; set; }

        public string WebServerLocalAddress { get; set; }

        public string WebServerNetworkAddress { get; set; }

        public string WebServerExternalAddress { get; set; }

        public string RebroadcastServersConfiguration { get; set; }

        public string LogFileName { get; set; }

        public event EventHandler CheckForNewVersion;

        public event EventHandler<EventArgs<IFeed>> ReconnectFeed;

        public event EventHandler<EventArgs<IFeed>> ResetPolarPlot;

        public event EventHandler ToggleUPnpStatus;

        #pragma warning disable 0067
        public event EventHandler ToggleServerStatus;
        #pragma warning restore 0067

        public MainView(IUniversalPlugAndPlayManager uPnpManager, ISimpleAircraftList flightSimulatorXAircraftList)
        {
            _UPnpManager = uPnpManager;
            _FlightSimulatorXAircraftList = flightSimulatorXAircraftList;
        }

        public void Dispose()
        {
            ;
        }

        public void BubbleExceptionToGui(Exception ex)
        {
            ;
        }

        public object ShowBusy(bool isBusy, object previousState)
        {
            return null;
        }

        public void Initialise(IUniversalPlugAndPlayManager unused1, ISimpleAircraftList unused2)
        {
            ;
        }

        public void ShowManualVersionCheckResult(bool newVersionAvailable)
        {
            ;
        }

        public void ShowServerRequests(ServerRequest[] serverRequests)
        {
            lock(_SyncLock) {
                _ViewModel.RefreshServerRequests(serverRequests);
            }
        }

        public void ShowFeeds(FeedStatus[] feeds)
        {
            lock(_SyncLock) {
                _ViewModel.RefreshFeedStatuses(feeds);
            }
        }

        public void ShowFeedConnectionStatus(FeedStatus feed)
        {
            ;
        }

        public void UpdateFeedCounters(FeedStatus[] feeds)
        {
            lock(_SyncLock) {
                ShowFeeds(feeds);
            }
        }

        public void ShowRebroadcastServerStatus(IList<RebroadcastServerConnection> connections)
        {
            lock(_SyncLock) {
                _ViewModel.RefreshRebroadcastServerConnections(connections);
            }
        }

        public void ShowSettingsConfigurationUI(string openOnPageTitle, object openOnConfigurationObject)
        {
            ;
        }

        public DialogResult ShowView()
        {
            _ViewModel = new ViewModel(this);

            _Presenter = Factory.Singleton.Resolve<IMainPresenter>();
            _Presenter.Initialise(this);
            _Presenter.UPnpManager = _UPnpManager;

            return DialogResult.OK;
        }

        [WebAdminMethod]
        public ViewModel GetState()
        {
            lock(_SyncLock) {
                _ViewModel.RefreshFromView(this);
            }
            return _ViewModel;
        }

        [WebAdminMethod]
        public void RaiseCheckForNewVersion()
        {
            EventHelper.Raise(CheckForNewVersion, this, EventArgs.Empty);
        }

        [WebAdminMethod]
        public void RaiseReconnectFeed(int feedId)
        {
            var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
            var feed = feedManager.GetByUniqueId(feedId, false);
            if(feed != null) {
                EventHelper.Raise(ReconnectFeed, this, new EventArgs<IFeed>(feed));
            }
        }

        [WebAdminMethod]
        public void RaiseResetPolarPlot(int feedId)
        {
            var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
            var feed = feedManager.GetByUniqueId(feedId, false);
            if(feed != null) {
                EventHelper.Raise(ResetPolarPlot, this, new EventArgs<IFeed>(feed));
            }
        }

        [WebAdminMethod(DeferExecution=true)]
        public void RaiseToggleUPnpStatus()
        {
            EventHelper.Raise(ToggleUPnpStatus, this, EventArgs.Empty);
        }
    }
}
