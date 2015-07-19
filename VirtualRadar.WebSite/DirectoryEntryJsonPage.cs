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
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Serves the DirectoryEntry.json page.
    /// </summary>
    class DirectoryEntryJsonPage : Page
    {
        /// <summary>
        /// The directory entry key that the user has configured. We only respond to requests that contain this key.
        /// </summary>
        private string _Key;

        /// <summary>
        /// The object that gives us access to the feeds.
        /// </summary>
        private IFeedManager _FeedManager;

        /// <summary>
        /// The object that gives us access to the version number.
        /// </summary>
        private IApplicationInformation _ApplicationInformation;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="webSite"></param>
        public DirectoryEntryJsonPage(WebSite webSite) : base(webSite)
        {
            _FeedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;
            _ApplicationInformation = Factory.Singleton.Resolve<IApplicationInformation>();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="configuration"></param>
        protected override void DoLoadConfiguration(Configuration configuration)
        {
            _Key = (configuration.GoogleMapSettings.DirectoryEntryKey ?? "").Trim();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args)
        {
            if("/DirectoryEntry.json".Equals(args.PathAndFile, StringComparison.OrdinalIgnoreCase)) {
                var key = (args.QueryString["key"] ?? "");
                if(!String.IsNullOrEmpty(_Key) && _Key.Equals(key, StringComparison.OrdinalIgnoreCase)) {
                    var directoryEntry = BuildDirectoryEntry();
                    Responder.SendJson(args.Request, args.Response, directoryEntry, null, null);
                    args.Handled = true;
                }
            }

            return args.Handled;
        }

        /// <summary>
        /// Creates and populates a <see cref="DirectoryEntryJson"/> object.
        /// </summary>
        /// <returns></returns>
        private DirectoryEntryJson BuildDirectoryEntry()
        {
            var feeds = _FeedManager.VisibleFeeds.Where(r => r.AircraftList != null).ToArray();
            var result = new DirectoryEntryJson() {
                Version = _ApplicationInformation.ShortVersion,
                NumberOfFeeds = feeds.Length,
                NumberOfAircraft = feeds.Length == 0 ? 0 : feeds.Select(r => r.AircraftList.Count).Max(),
            };

            return result;
        }
    }
}
