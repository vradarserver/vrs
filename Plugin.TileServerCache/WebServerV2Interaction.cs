// Copyright © 2019 onwards, Andrew Whewell
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
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Plugin.TileServerCache
{
    /// <summary>
    /// Deals with hooking the plugin into the v2 web server and handling requests
    /// for fake tile server URLs.
    /// </summary>
    class WebServerV2Interaction
    {
        /// <summary>
        /// The object that contains the web request handling logic. This tells us what to do.
        /// </summary>
        private WebRequestHandler _WebRequestHandler = new WebRequestHandler();

        /// <summary>
        /// The object that will set up responses for us.
        /// </summary>
        private IResponder _Responder;

        /// <summary>
        /// Does whatever is necessary to get ourselves into the web request processing chain.
        /// </summary>
        public void Initialise()
        {
            _Responder = Factory.Resolve<IResponder>();

            var configWebServer = Factory.ResolveSingleton<IAutoConfigWebServer>();
            var webServer = configWebServer.WebServer;

            webServer.AfterRequestReceived += WebServer_AfterRequestReceived;
        }

        /// <summary>
        /// Called after the main web site processes the incoming request. Handles the request
        /// if it's for a fake tile server URL.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebServer_AfterRequestReceived(object sender, RequestReceivedEventArgs e)
        {
            if(!e.Handled && _WebRequestHandler.IsTileServerCacheRequest(e.PathParts)) {
                e.Handled = true;

                var options = Plugin.Singleton?.Options;
                if(options != null) {
                    var outcome = _WebRequestHandler.ProcessRequest(
                        options,
                        e.PathParts,
                        e.File,
                        e.Request.RemoteEndPoint.Address,
                        e.Request.Headers,
                        e.Request.UserAgent
                    );

                    e.Response.StatusCode = outcome.StatusCode;

                    if(outcome.ImageBytes?.Length > 0) {
                        var mimeType = MimeType.JpegImage;
                        switch(outcome.ImageExtension.ToUpper()) {
                            case ".BMP":    mimeType = MimeType.BitmapImage; break;
                            case ".GIF":    mimeType = MimeType.GifImage; break;
                            case ".PNG":    mimeType = MimeType.PngImage; break;
                        }

                        _Responder.SendBinary(
                            e.Request,
                            e.Response,
                            outcome.ImageBytes,
                            mimeType,
                            compressResponse: false
                        );
                    }
                }
            }
        }
    }
}
