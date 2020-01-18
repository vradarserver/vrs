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
using System.Threading.Tasks;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Utility;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Plugin.TileServerCache
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Registers the plugin's OWIN middleware as a part of VRS's web request pipeline.
    /// </summary>
    class WebServerV3Middleware
    {
        // The object that does all of the web request handling work for us.
        private readonly WebRequestHandler _WebRequestHandler = new WebRequestHandler();

        /// <summary>
        /// Creates the OWIN AppFunc.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc AppFuncBuilder(AppFunc next)
        {
            return async(IDictionary<string, object> environment) => {
                var context = OwinContext.Create(environment);
                if(!ProcessRequest(context)) {
                    await next(environment);
                }
            };
        }

        private bool ProcessRequest(OwinContext context)
        {
            var options = Plugin.Singleton?.Options;
            var result = options != null && _WebRequestHandler.IsTileServerCacheRequest(context.RequestPathParts);

            if(result) {
                var outcome = _WebRequestHandler.ProcessRequest(
                    options,
                    context.RequestPathParts,
                    context.RequestPathFileName,
                    context.ClientIpAddressParsed,
                    context.RequestHeadersDictionary
                );

                context.ResponseStatusCode = (int)outcome.StatusCode;
                if(outcome.ImageBytes != null) {
                    var mimeType = "";
                    switch((outcome.ImageExtension ?? "").ToLower()) {
                        case ".bmp":    mimeType = MimeType.BitmapImage; break;
                        case ".gif":    mimeType = MimeType.GifImage; break;
                        case ".jpg":    mimeType = MimeType.JpegImage; break;
                        case ".png":    mimeType = MimeType.PngImage; break;
                    }
                    context.ReturnBytes(mimeType, outcome.ImageBytes);
                }
            }

            return result;
        }
    }
}
