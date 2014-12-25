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
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Plugin.WebAdmin.View
{
    /// <summary>
    /// A stub that represents the main view.
    /// </summary>
    class MainView : BaseView, IMainView
    {
        #region Fields
        private IMainPresenter _Presenter;
        #endregion

        #region Properties
        public int InvalidPluginCount { get; set; }

        public string LogFileName { get; set; }

        public bool NewVersionAvailable { get; set; }

        public string NewVersionDownloadUrl { get; set; }

        public string RebroadcastServersConfiguration { get; set; }

        public bool UPnpEnabled { get; set; }

        public bool UPnpRouterPresent { get; set; }

        public bool UPnpPortForwardingActive { get; set; }

        public bool WebServerIsOnline { get; set; }

        public string WebServerLocalAddress { get; set; }

        public string WebServerNetworkAddress { get; set; }

        public string WebServerExternalAddress { get; set; }
        #endregion

        #region Events exposed
        public event EventHandler CheckForNewVersion;
        public void OnCheckForNewVersion(EventArgs args)
        {
            if(CheckForNewVersion != null) CheckForNewVersion(this, args);
        }

        public event EventHandler<EventArgs<IFeed>> ReconnectFeed;
        public void OnReconnectFeed(EventArgs<IFeed> args)
        {
            if(ReconnectFeed != null) ReconnectFeed(this, args);
        }

        public event EventHandler ToggleServerStatus;
        public void OnToggleServerStatus(EventArgs args)
        {
            if(ToggleServerStatus != null) ToggleServerStatus(this, args);
        }

        public event EventHandler ToggleUPnpStatus;
        public void OnToggleUPnpStatus(EventArgs args)
        {
            if(ToggleUPnpStatus != null) ToggleUPnpStatus(this, args);
        }

        public event EventHandler RefreshTimerTicked;
        public void OnRefreshTimerTicked(EventArgs args)
        {
            if(RefreshTimerTicked != null) RefreshTimerTicked(this, args);
        }
        #endregion

        #region Form methods
        public void Initialise(IUniversalPlugAndPlayManager uPnpManager, ISimpleAircraftList flightSimulatorXAircraftList)
        {
            _Presenter = Factory.Singleton.Resolve<IMainPresenter>();
            _Presenter.UPnpManager = uPnpManager;
        }

        public void ShowManualVersionCheckResult(bool newVersionAvailable)
        {
            ;
        }

        public void ShowFeeds(IFeed[] feeds)
        {
            ;
        }

        public void UpdateFeedCounters(IFeed[] feeds)
        {
            ;
        }

        public void ShowFeedConnectionStatus(IFeed feed)
        {
            ;
        }

        public void ShowRebroadcastServerStatus(IList<RebroadcastServerConnection> connections)
        {
            ;
        }

        public void ShowWebRequestHasBeenServiced(IPEndPoint remoteEndPoint, string url, long bytesSent)
        {
            ;
        }

        public void ShowSettingsConfigurationUI(string openOnPageTitle, object openOnConfigurationObject)
        {
            ;
        }

        public override DialogResult ShowView()
        {
            _Presenter.Initialise(this);
            return base.ShowView();
        }
        #endregion
    }
}
