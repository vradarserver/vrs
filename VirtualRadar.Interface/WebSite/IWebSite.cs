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
using System.Text;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The interface for objects that bring together a collection of pages into the website that is
    /// presented to the browser.
    /// </summary>
    public interface IWebSite
    {
        #region Properties
        /// <summary>
        /// Gets or sets the testing provider.
        /// </summary>
        IWebSiteProvider Provider { get; set; }

        /// <summary>
        /// Gets or sets the aircraft list to use when browsers ask for the FSX aircraft list.
        /// </summary>
        ISimpleAircraftList FlightSimulatorAircraftList { get; set; }

        /// <summary>
        /// Gets or sets the BaseStation database that the site will use when generating reports.
        /// </summary>
        IBaseStationDatabase BaseStationDatabase { get; set; }

        /// <summary>
        /// Gets or sets the object that can lookup entries in the standing data on our behalf.
        /// </summary>
        IStandingDataManager StandingDataManager { get; set; }

        /// <summary>
        /// Gets the web server that the site is attached to.
        /// </summary>
        IWebServer WebServer { get; }
        #endregion

        #region AttachSiteToServer
        /// <summary>
        /// Attaches the website to a server.
        /// </summary>
        /// <param name="server"></param>
        void AttachSiteToServer(IWebServer server);
        #endregion

        #region AddSiteRoot, RemoveSiteRoot, IsSiteRootActive, GetSiteRootFolders
        /// <summary>
        /// Adds a folder from which files can be served by the site.
        /// </summary>
        /// <param name="siteRoot"></param>
        void AddSiteRoot(SiteRoot siteRoot);

        /// <summary>
        /// Removes a site root that had been previously added to the site via <see cref="AddSiteRoot"/>.
        /// </summary>
        /// <param name="siteRoot"></param>
        void RemoveSiteRoot(SiteRoot siteRoot);

        /// <summary>
        /// Returns true if the site root is currently being used to serve files.
        /// </summary>
        /// <param name="siteRoot"></param>
        /// <param name="folderMustMatch"></param>
        /// <returns></returns>
        bool IsSiteRootActive(SiteRoot siteRoot, bool folderMustMatch);

        /// <summary>
        /// Returns a collection of all of the folders from which content will be served.
        /// </summary>
        /// <returns></returns>
        List<string> GetSiteRootFolders();
        #endregion

        #region AddHtmlContentInjector, RemoveHtmlContentInjector
        /// <summary>
        /// Adds an object that can cause content to be injected into HTML files served by the site.
        /// </summary>
        /// <param name="contentInjector"></param>
        void AddHtmlContentInjector(HtmlContentInjector contentInjector);

        /// <summary>
        /// Removes an HtmlContentInjector previously added by <see cref="AddHtmlContentInjector"/>.
        /// </summary>
        /// <param name="contentInjector"></param>
        void RemoveHtmlContentInjector(HtmlContentInjector contentInjector);
        #endregion

        #region RequestContent, RequestSimpleContent
        /// <summary>
        /// Processes the request for content as if it had been a request received event raised by a web server.
        /// </summary>
        /// <param name="args"></param>
        /// <remarks>
        /// This method allows other parts of the system to fetch pages as a browser would see them but without
        /// having to mimic an IWebServer, or interfere with the live IWebServer.
        /// </remarks>
        void RequestContent(RequestReceivedEventArgs args);

        /// <summary>
        /// Processes the request for content indicated by the path from root and file passed across.
        /// </summary>
        /// <param name="pathAndFile">The path and file portion of a URL.</param>
        /// <returns>An object describing the result of the request.</returns>
        SimpleContent RequestSimpleContent(string pathAndFile);
        #endregion
    }
}
