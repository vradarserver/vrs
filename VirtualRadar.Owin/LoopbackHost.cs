// Copyright © 2017 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using InterfaceFactory;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Owin;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Owin
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Default implementation of <see cref="ILoopbackHost"/>.
    /// </summary>
    class LoopbackHost : ILoopbackHost
    {
        /// <summary>
        /// The app builder used to create the host.
        /// </summary>
        private IAppBuilder _AppBuilder = new AppBuilder();

        /// <summary>
        /// The middleware chain that was built by <see cref="_AppBuilder"/>.
        /// </summary>
        private AppFunc _MiddlewareChain = null;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Action<IDictionary<string, object>> ModifyEnvironmentAction { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ConfigureStandardPipeline()
        {
            if(_MiddlewareChain != null) {
                throw new InvalidOperationException($"You cannot configure an {nameof(ILoopbackHost)} more than once");
            }

            var webAppConfiguration = Factory.Singleton.Resolve<IWebAppConfiguration>();
            var standardPipeline = Factory.Singleton.Resolve<IStandardPipeline>();

            standardPipeline.Register(webAppConfiguration);
            ConfigureCustomPipeline(webAppConfiguration);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="webAppConfiguration"></param>
        public void ConfigureCustomPipeline(IWebAppConfiguration webAppConfiguration)
        {
            if(_MiddlewareChain != null) {
                throw new InvalidOperationException($"You cannot configure an {nameof(ILoopbackHost)} more than once");
            }

            webAppConfiguration.Configure(_AppBuilder);
            _MiddlewareChain = _AppBuilder.Build<AppFunc>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public SimpleContent SendSimpleRequest(string pathAndFile, IDictionary<string, object> environment = null)
        {
            if(_MiddlewareChain == null) {
                throw new InvalidOperationException($"{nameof(ILoopbackHost)} needs to be configured before use");
            }

            using(var responseStream = new MemoryStream()) {
                var fakeEnvironment = CreateCompliantOwinEnvironment(pathAndFile, responseStream, environment);
                ModifyEnvironmentAction?.Invoke(fakeEnvironment);

                _MiddlewareChain.Invoke(fakeEnvironment);

                return new SimpleContent() {
                    HttpStatusCode = (HttpStatusCode)fakeEnvironment["owin.ResponseStatusCode"],
                    Content = responseStream.ToArray(),
                };
            }
        }

        private IDictionary<string, object> CreateCompliantOwinEnvironment(string pathAndFile, Stream responseStream, IDictionary<string, object> environment)
        {
            if(pathAndFile == null) {
                throw new ArgumentNullException(nameof(pathAndFile));
            }

            var queryStringIndex = pathAndFile.IndexOf('?');
            var justPathAndFile = queryStringIndex == -1 ? pathAndFile : pathAndFile.Substring(0, queryStringIndex);
            var queryString = queryStringIndex == -1 ? "" : pathAndFile.Substring(queryStringIndex + 1);

            justPathAndFile = UriHelper.FlattenPath(justPathAndFile);

            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                ["owin.CallCancelled"] = new CancellationToken(),
                ["owin.Version"] = "1.0.0",

                ["owin.RequestBody"] = Stream.Null,
                ["owin.RequestHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase),
                ["owin.RequestMethod"] = "GET",
                ["owin.RequestProtocol"] = "HTTP/1.1",
                ["owin.RequestScheme"] = "http",
                ["owin.RequestPathBase"] = "/VirtualRadar",
                ["owin.RequestPath"] = justPathAndFile,
                ["owin.RequestQueryString"] = queryString,

                ["owin.ResponseBody"] = responseStream,
                ["owin.ResponseHeaders"] = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase),
                ["owin.ResponseStatusCode"] = 200,              // I would prefer 404, and AppBuilder supplies a default of 404... but the spec says the default is 200

                ["server.RemoteIpAddress"] = "127.0.0.1",

                [EnvironmentKey.IsLoopbackRequest] = true,
            };

            var environmentContext = environment == null ? null : PipelineContext.GetOrCreate(environment);
            var context = PipelineContext.GetOrCreate(result);

            context.Request.UserAgent = environmentContext?.Request.UserAgent ?? "FAKE REQUEST";
            if(environmentContext != null) {
                context.Request.RemoteIpAddress = environmentContext.Request.RemoteIpAddress;
                context.Request.RemotePort = environmentContext.Request.RemotePort;

                foreach(var header in environmentContext.Request.Headers) {
                    if(context.Request.Headers[header.Key] != null) {
                        context.Request.Headers.Remove(header.Key);
                    }

                    context.Request.Headers.Add(header.Key, header.Value);
                }
            }

            return result;
        }
    }
}
