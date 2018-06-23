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
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Owin.StreamManipulator
{
    /// <summary>
    /// Default implementation of <see cref="IMapPluginHtmlManipulator"/>.
    /// </summary>
    class MapPluginHtmlManipulator : IMapPluginHtmlManipulator
    {
        private ISharedConfiguration _SharedConfiguration;

        public MapPluginHtmlManipulator()
        {
            _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="textContent"></param>
        public void ManipulateTextResponse(IDictionary<string, object> environment, TextContent textContent)
        {
            string mapStylesheet = null;
            string mapJavascript = null;

            switch(_SharedConfiguration.Get().GoogleMapSettings.MapProvider) {
                case MapProvider.GoogleMaps:
                    mapJavascript = @"<script src=""script/jquiplugin/jquery.vrs.map-google.js"" type=""text/javascript""></script>";
                    break;
                case MapProvider.Leaflet:
                    mapStylesheet = @"<link rel=""stylesheet"" href=""css/leaflet/leaflet.css"" type=""text/css"" media=""screen"" />";
                    mapJavascript =  @"<script src=""script/leaflet-src.js"" type=""text/javascript""></script>
                                      <script src=""script/jquiplugin/jquery.vrs.map-leaflet.js"" type=""text/javascript""></script>";
                    break;
                default:
                    throw new NotImplementedException();
            }

            if(!String.IsNullOrEmpty(mapStylesheet)) {
                textContent.Content = textContent.Content.Replace("<!-- [[ MAP STYLESHEET ]] -->", mapStylesheet);
            }
            if(!String.IsNullOrEmpty(mapJavascript)) {
                textContent.Content = textContent.Content.Replace("<!-- [[ MAP PLUGIN ]] -->", mapJavascript);
            }
        }
    }
}
