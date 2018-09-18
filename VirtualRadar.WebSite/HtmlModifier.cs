// Copyright © 2018 onwards, Andrew Whewell
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
using HtmlAgilityPack;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// A class that can modify the content of HTML files.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This was added for version 2.4.1. Originally 2.4 was going to be the last in the line of version 2
    /// for VRS, the next version would move on to .NET 4.6 and replace the homebrew web server with
    /// Microsoft's Katana server and OWIN. However, Google threw a spanner in the works by changing the
    /// terms and conditions for Google Maps such that you couldn't register for an API without lodging
    /// credit card details with them and then they reduced the free tier's allowances.
    /// </para><para>
    /// I decided to add support for OpenStreetMap and I started doing that in the master branch... but there
    /// are a couple of problems there. First of all Google only gave 4 weeks notice of their changes and V3
    /// is not ready for release. Secondly even if V3 was ready there will be a significant number of users
    /// who can't switch to .NET 4.6 because they are running on XP or a version of Mono that doesn't support
    /// .NET 4.6.
    /// </para><para>
    /// So... 2.4.1 is a point release to add OSM support and fix a couple of bugs that were cleaned up in
    /// V3. There is a setting (copied from V3) to select between different map providers. The HTML files that
    /// previously had script references to Google Maps had those references replaced with an HTML comment
    /// of [[ MAP PLUGIN ]] and a new comment added to the CSS block of [[ MAP STYLESHEET ]].
    /// </para><para>
    /// In V3 I had a new scheme for modifying HTML content and I just wrote a little bit of code to search
    /// for those comments and replace them with the appropriate stylesheet and script references for the
    /// chosen provider.
    /// </para><para>
    /// That mechanism doesn't exist in V2, hence this monstrosity :) It just plugs into the web site in
    /// the same way as IBundler and does the substitutions of the MAP PLUGIN and MAP STYLESHEET comments. In
    /// an ideal world I would merge together all of the different bits of code that are messing with HTML
    /// files (like I did in V3) but it's not worth it, I don't intend to keep developing on this branch.
    /// </para>
    /// </remarks>
    class HtmlModifier
    {
        private ISharedConfiguration _SharedConfiguration;

        public HtmlModifier()
        {
            _SharedConfiguration = Factory.Singleton.Resolve<ISharedConfiguration>().Singleton;
        }

        public string ModifyHtml(string requestPathAndFile, string htmlContent)
        {
            if(requestPathAndFile == null) throw new ArgumentNullException("requestPathAndFile");
            if(htmlContent == null) throw new ArgumentNullException("htmlContent");

            string mapStylesheet = null;
            string mapJavascript = null;
            switch(_SharedConfiguration.Get().GoogleMapSettings.MapProvider) {
                case MapProvider.GoogleMaps:
                    mapJavascript = @"<script src=""script/jquiplugin/jquery.vrs.map-google.js"" type=""text/javascript""></script>";
                    break;
                case MapProvider.Leaflet:
                    mapStylesheet = @"<link rel=""stylesheet"" href=""css/leaflet/leaflet.css"" type=""text/css"" media=""screen"" />
                                      <link rel=""stylesheet"" href=""css/leaflet.markercluster/MarkerCluster.css"" type=""text/css"" media=""screen"" />
                                      <link rel=""stylesheet"" href=""css/leaflet.markercluster/MarkerCluster.Default.css"" type=""text/css"" media=""screen"" />";
                    mapJavascript = @"<script src=""script/leaflet-src.js"" type=""text/javascript""></script>
                                      <script src=""script/leaflet.markercluster-src.js"" type=""text/javascript""></script>
                                      <script src=""script/jquiplugin/jquery.vrs.map-leaflet.js"" type=""text/javascript""></script>";
                    break;
                default:
                    throw new NotImplementedException();
            }

            if(!String.IsNullOrEmpty(mapStylesheet)) {
                htmlContent = htmlContent.Replace("<!-- [[ MAP STYLESHEET ]] -->", mapStylesheet);
            }
            if(!String.IsNullOrEmpty(mapJavascript)) {
                htmlContent = htmlContent.Replace("<!-- [[ MAP PLUGIN ]] -->", mapJavascript);
            }

            return htmlContent;
        }
    }
}
