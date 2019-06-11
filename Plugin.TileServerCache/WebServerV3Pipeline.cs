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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Owin;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Plugin.TileServerCache
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Registers the plugin's OWIN middleware as a part of VRS's web request pipeline.
    /// </summary>
    class WebServerV3Pipeline : IPipeline
    {
        // The object that does all of the web request handling work for us.
        private WebRequestHandler _WebRequestHandler = new WebRequestHandler();

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="webAppConfiguration"></param>
        public void Register(IWebAppConfiguration webAppConfiguration)
        {
            webAppConfiguration.AddCallback(UseWebRequestMiddleware, StandardPipelinePriority.HighestVrsContentMiddlewarePriority + 1000);
        }

        private void UseWebRequestMiddleware(IAppBuilder app)
        {
            var middleware = new Func<AppFunc, AppFunc>(HandleRequest);
            app.Use(middleware);
        }

        private AppFunc HandleRequest(AppFunc next)
        {
            AppFunc appFunc = async(IDictionary<string, object> environment) => {
                var context = PipelineContext.GetOrCreate(environment);
                if(!ProcessRequest(context)) {
                    await next.Invoke(environment);
                }
            };

            return appFunc;
        }

        private bool ProcessRequest(PipelineContext context)
        {
            var options = Plugin.Singleton?.Options;
            var result = options != null && _WebRequestHandler.IsTileServerCacheRequest(context.Request.PathParts);

            if(result) {
                var outcome = _WebRequestHandler.ProcessRequest(
                    options,
                    context.Request.PathParts,
                    context.Request.FileName,
                    context.Request.ClientIpAddressParsed,
                    context.Request.Headers,
                    context.Request.UserAgent
                );

                var response = context.Response;
                response.StatusCode = (int)outcome.StatusCode;
                if(outcome.ImageBytes != null) {
                    switch((outcome.ImageExtension ?? "").ToLower()) {
                        case ".bmp":    response.ContentType = MimeType.BitmapImage; break;
                        case ".gif":    response.ContentType = MimeType.GifImage; break;
                        case ".jpg":    response.ContentType = MimeType.JpegImage; break;
                        case ".png":    response.ContentType = MimeType.PngImage; break;
                    }
                    response.ContentLength = outcome.ImageBytes.Length;
                    response.Body.Write(outcome.ImageBytes, 0, outcome.ImageBytes.Length);
                }
            }

            return result;
        }
    }
}
