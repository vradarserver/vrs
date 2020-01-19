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
using System.Net;
using System.Threading;
using AWhewell.Owin.Interface;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Default implementation of <see cref="ILoopbackHost"/>.
    /// </summary>
    class LoopbackHost : ILoopbackHost
    {
        /// <summary>
        /// The pipeline that will be used to process messages.
        /// </summary>
        private AWhewell.Owin.Interface.IPipeline _Pipeline;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Action<IDictionary<string, object>> ModifyEnvironmentAction { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ConfigureStandardPipeline()
        {
            if(_Pipeline != null) {
                throw new InvalidOperationException($"You cannot configure an {nameof(ILoopbackHost)} more than once");
            }

            var webSitePipelineBuilder = Factory.ResolveSingleton<IWebSitePipelineBuilder>();
            ConfigureCustomPipeline(webSitePipelineBuilder.PipelineBuilder);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pipelineBuilder"></param>
        public void ConfigureCustomPipeline(IPipelineBuilder pipelineBuilder)
        {
            if(_Pipeline != null) {
                throw new InvalidOperationException($"You cannot configure an {nameof(ILoopbackHost)} more than once");
            }

            var environment = Factory.Resolve<IPipelineBuilderEnvironment>();
            _Pipeline = pipelineBuilder.CreatePipeline(environment);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="pathAndFile"></param>
        /// <param name="environment"></param>
        /// <returns></returns>
        public SimpleContent SendSimpleRequest(string pathAndFile, IDictionary<string, object> environment = null)
        {
            if(_Pipeline == null) {
                throw new InvalidOperationException($"{nameof(ILoopbackHost)} needs to be configured before use");
            }

            using(var responseStream = new MemoryStream()) {
                var fakeEnvironment = CreateCompliantOwinEnvironment(pathAndFile, responseStream, environment);
                ModifyEnvironmentAction?.Invoke(fakeEnvironment);

                var task = _Pipeline.ProcessRequest(fakeEnvironment);
                task.Wait();

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

            var environmentContext = OwinContext.Create(environment);

            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) {
                [EnvironmentKey.CallCancelled] =    new CancellationToken(),
                [EnvironmentKey.Version] =          "1.0.0",

                [EnvironmentKey.RequestBody] =          Stream.Null,
                [EnvironmentKey.RequestHeaders] =       new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) {
                                                            ["User-Agent"] = new string[] {
                                                                environmentContext.RequestHeadersDictionary?.UserAgent ?? "FAKE REQUEST"
                                                            },
                                                        },
                [EnvironmentKey.RequestMethod] =        "GET",
                [EnvironmentKey.RequestProtocol] =      "HTTP/1.1",
                [EnvironmentKey.RequestScheme] =        "http",
                [EnvironmentKey.RequestPathBase] =      "/VirtualRadar",
                [EnvironmentKey.RequestPath] =          justPathAndFile,
                [EnvironmentKey.RequestQueryString] =   queryString,

                [EnvironmentKey.ResponseBody] =         responseStream,
                [EnvironmentKey.ResponseHeaders] =      new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase),
                [EnvironmentKey.ResponseStatusCode] =   200,

                [EnvironmentKey.ServerRemoteIpAddress] = "127.0.0.1",

                [VrsEnvironmentKey.IsLoopbackRequest] = true,
            };

            var context = OwinContext.Create(result);

            if(environment != null) {
                context.Environment[EnvironmentKey.ServerRemoteIpAddress] = environmentContext.ServerRemoteIpAddress;
                context.Environment[EnvironmentKey.ServerRemotePort] =      environmentContext.ServerRemotePort;

                foreach(var header in environmentContext.RequestHeaders) {
                    context.RequestHeaders[header.Key] = header.Value;
                }
            }

            return result;
        }
    }
}
