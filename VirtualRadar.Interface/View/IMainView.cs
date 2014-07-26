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
using System.Linq;
using System.Net;
using System.Text;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// The interface for the view that the user sees when they first start the application.
    /// </summary>
    public interface IMainView : IView, IBusyView
    {
        #region Properties
        /// <summary>
        /// Gets or sets a count of the number of plugins that could not be loaded at startup.
        /// </summary>
        /// <remarks>
        /// If this number is zero then any UI associated with reporting the count of invalid plugins should be hidden from view.
        /// </remarks>
        int InvalidPluginCount { get; set; }

        /// <summary>
        /// Gets or sets the full path to the log file that will be displayed if the user indicates they want to see its content.
        /// </summary>
        string LogFileName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the user should be informed that a new version of the application is available.
        /// </summary>
        bool NewVersionAvailable { get; set; }

        /// <summary>
        /// Gets or sets the URL to send users to when a new version is detected.
        /// </summary>
        string NewVersionDownloadUrl { get; set; }

        /// <summary>
        /// Gets or sets localised text that describes the configuration of the rebroadcast servers.
        /// </summary>
        string RebroadcastServersConfiguration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that control of a UPnP router has been enabled in the configuration.
        /// </summary>
        bool UPnpEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that a UPnP router is present on the network.
        /// </summary>
        bool UPnpRouterPresent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that there is a port forwarding mapping on the router to our webserver.
        /// </summary>
        bool UPnpPortForwardingActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server is online and listening to requests.
        /// </summary>
        bool WebServerIsOnline { get; set; }

        /// <summary>
        /// Gets or sets the address of the web server on the local loopback.
        /// </summary>
        string WebServerLocalAddress { get; set; }

        /// <summary>
        /// Gets or sets the address of the web server on the LAN.
        /// </summary>
        string WebServerNetworkAddress { get; set; }

        /// <summary>
        /// Gets or sets the address of the web server on the Internet.
        /// </summary>
        string WebServerExternalAddress { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the user wants to manually check for a new version of the application.
        /// </summary>
        event EventHandler CheckForNewVersion;

        /// <summary>
        /// Raised when the user wants to manually cycle the connection to the feed.
        /// </summary>
        event EventHandler<EventArgs<IFeed>> ReconnectFeed;

        /// <summary>
        /// Raised when the user has indicated that they want to toggle the server's online status.
        /// </summary>
        event EventHandler ToggleServerStatus;

        /// <summary>
        /// Raised when the user has indicated that they want to put the server onto or take the server off the Internet.
        /// </summary>
        event EventHandler ToggleUPnpStatus;

        /// <summary>
        /// Raised on the GUI thread when it's time to refresh the display.
        /// </summary>
        event EventHandler RefreshTimerTicked;
        #endregion

        #region Methods
        /// <summary>
        /// Records references to objects that will be set on <see cref="IMainPresenter"/> when it is created.
        /// </summary>
        /// <param name="uPnpManager"></param>
        /// <param name="flightSimulatorXAircraftList"></param>
        void Initialise(IUniversalPlugAndPlayManager uPnpManager, ISimpleAircraftList flightSimulatorXAircraftList);

        /// <summary>
        /// Throws the exception passed across as an inner exception to an ApplicationException on the GUI thread.
        /// </summary>
        /// <param name="ex"></param>
        void BubbleExceptionToGui(Exception ex);

        /// <summary>
        /// Show the result of a manual check for a new version of the application to the user.
        /// </summary>
        /// <param name="newVersionAvailable"></param>
        void ShowManualVersionCheckResult(bool newVersionAvailable);

        /// <summary>
        /// Updates the list of feeds shown to the user. Old feeds are removed, new ones added.
        /// </summary>
        /// <param name="feeds"></param>
        void ShowFeeds(IFeed[] feeds);

        /// <summary>
        /// Updates the list of feeds with fresh counts.
        /// </summary>
        /// <param name="feeds"></param>
        void UpdateFeedCounters(IFeed[] feeds);

        /// <summary>
        /// Updates the display for a feed with a fresh connection status.
        /// </summary>
        /// <param name="feed"></param>
        void ShowFeedConnectionStatus(IFeed feed);

        /// <summary>
        /// Updates the display with the current set of connections to the rebroadcast servers.
        /// </summary>
        /// <param name="connections"></param>
        void ShowRebroadcastServerStatus(IList<RebroadcastServerConnection> connections);

        /// <summary>
        /// Updates a display that shows the requests that the web server is responding to. 
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="url"></param>
        /// <param name="bytesSent"></param>
        void ShowWebRequestHasBeenServiced(IPEndPoint remoteEndPoint, string url, long bytesSent);
        #endregion
    }
}
