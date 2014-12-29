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

namespace VirtualRadar.Plugin.WebAdmin.View
{
    /// <summary>
    /// The class that carries the main view's data to the site.
    /// </summary>
    class MainView : BaseView, IMainView
    {
        #region Fields
        private IMainPresenter _Presenter;
        private IUniversalPlugAndPlayManager _UPnpManager;
        private ISimpleAircraftList _FlightSimulatorXAircraftList;
        #endregion

        #region Properties
        [JsonProperty("BadPlugins")]
        public int InvalidPluginCount { get; set; }

        [JsonIgnore]
        public string LogFileName { get; set; }

        [JsonProperty("NewVer")]
        public bool NewVersionAvailable { get; set; }

        [JsonProperty("NewVerUrl")]
        public string NewVersionDownloadUrl { get; set; }

        [JsonIgnore]
        public string RebroadcastServersConfiguration { get; set; }

        [JsonProperty("Upnp")]
        public bool UPnpEnabled { get; set; }

        [JsonProperty("UpnpRouter")]
        public bool UPnpRouterPresent { get; set; }

        [JsonProperty("UpnpOn")]
        public bool UPnpPortForwardingActive { get; set; }

        [JsonIgnore]
        public bool WebServerIsOnline { get; set; }

        [JsonProperty("LocalRoot")]
        public string WebServerLocalAddress { get; set; }

        [JsonProperty("LanRoot")]
        public string WebServerNetworkAddress { get; set; }

        [JsonProperty("PublicRoot")]
        public string WebServerExternalAddress { get; set; }
        #endregion

        #region Extra properties
        public ServerRequest[] Requests { get; set; }

        public FeedStatus[] Feeds { get; set; }

        public RebroadcastServerConnection[] Rebroadcasters { get; set; }
        #endregion

        #region Events exposed
        public event EventHandler CheckForNewVersion;
        protected virtual void OnCheckForNewVersion(EventArgs args)
        {
            if(CheckForNewVersion != null) CheckForNewVersion(this, args);
        }

        public event EventHandler<EventArgs<IFeed>> ReconnectFeed;
        protected virtual void OnReconnectFeed(EventArgs<IFeed> args)
        {
            if(ReconnectFeed != null) ReconnectFeed(this, args);
        }

        public event EventHandler<EventArgs<IFeed>> ResetPolarPlot;
        protected virtual void OnResetPolarPlot(EventArgs<IFeed> args)
        {
            if(ResetPolarPlot != null) ResetPolarPlot(this, args);
        }

        public event EventHandler ToggleServerStatus;
        protected virtual void OnToggleServerStatus(EventArgs args)
        {
            if(ToggleServerStatus != null) ToggleServerStatus(this, args);
        }

        public event EventHandler ToggleUPnpStatus;
        protected virtual void OnToggleUPnpStatus(EventArgs args)
        {
            if(ToggleUPnpStatus != null) ToggleUPnpStatus(this, args);
        }
        #endregion

        #region Ctors
        public MainView(IUniversalPlugAndPlayManager uPnpManager, ISimpleAircraftList flightSimulatorXAircraftList)
        {
            Requests = new ServerRequest[0];
            Feeds = new FeedStatus[0];
            Rebroadcasters = new RebroadcastServerConnection[0];

            _UPnpManager = uPnpManager;
            _FlightSimulatorXAircraftList = flightSimulatorXAircraftList;
        }
        #endregion

        #region Form methods
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
            Requests = serverRequests.Select(r => r.Clone() as ServerRequest).OrderBy(r => r.RemoteAddress).ToArray();
        }

        public void ShowFeeds(FeedStatus[] feeds)
        {
            Feeds = feeds.Select(r => r.Clone() as FeedStatus).OrderBy(r => r.Name).ToArray();
        }

        public void ShowFeedConnectionStatus(FeedStatus feed)
        {
            // No need to have code for this - the next update of feed counters will include the new connection
            // status. We're not showing this stuff in real time.
            ;
        }

        public void UpdateFeedCounters(FeedStatus[] feeds)
        {
            ShowFeeds(feeds);
        }

        public void ShowRebroadcastServerStatus(IList<RebroadcastServerConnection> connections)
        {
            Rebroadcasters = connections.OrderBy(r => r.Name).ToArray();
        }

        public void ShowSettingsConfigurationUI(string openOnPageTitle, object openOnConfigurationObject)
        {
            ;
        }

        public override DialogResult ShowView()
        {
            if(!IsRunning) {
                _Presenter = Factory.Singleton.Resolve<IMainPresenter>();
                _Presenter.Initialise(this);
                _Presenter.UPnpManager = _UPnpManager;
            }

            return base.ShowView();
        }
        #endregion

        #region Actions and Events
        protected override void RaiseEvent(string eventName, NameValueCollection queryString)
        {
            IFeed receiverFeed = null;

            switch(eventName) {
                case "toggle-upnp-status":
                    OnToggleUPnpStatus(EventArgs.Empty);
                    break;
                case "reconnect-feed":
                    receiverFeed = ExtractReceiverFeedFromQueryString(queryString);
                    if(receiverFeed != null) OnReconnectFeed(new EventArgs<IFeed>(receiverFeed));
                    break;
                case "reset-polar-plot":
                    receiverFeed = ExtractReceiverFeedFromQueryString(queryString);
                    if(receiverFeed != null) OnResetPolarPlot(new EventArgs<IFeed>(receiverFeed));
                    break;
            }
        }

        protected IFeed ExtractReceiverFeedFromQueryString(NameValueCollection queryString)
        {
            IFeed result = null;

            int feedId;
            if(int.TryParse(queryString["feedid"], out feedId)) {
                var feeds = _Presenter.GetReceiverFeeds();
                result = feeds.FirstOrDefault(r => r.UniqueId == feedId);
            }

            return result;
        }
        #endregion
    }
}
