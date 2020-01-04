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
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Owin.Middleware
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Default implementation of <see cref="ICorsHandler"/>.
    /// </summary>
    class CorsHandler : ICorsHandler
    {
        private static readonly string[] _DefaultAllowableMethods = new string[] { "POST", "GET", "OPTIONS" };

        private ISharedConfiguration _SharedConfiguration;
        private DateTime _ConfigurationLastParsed;
        private bool _CorsEnabled;
        private string[] _AllowedDomains;
        private bool _AllowAllDomains;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public AppFunc HandleRequest(AppFunc next)
        {
            AppFunc appFunc = async(IDictionary<string, object> environment) => {
                if(AddCorsHeaders(environment)) {
                    await next.Invoke(environment);
                }
            };

            return appFunc;
        }

        /// <summary>
        /// Adds CORS headers to requests when appropriate.
        /// </summary>
        /// <param name="environment"></param>
        /// <returns>True if we should chain onto the next bit of middleware.</returns>
        private bool AddCorsHeaders(IDictionary<string, object> environment)
        {
            var result = true;
            var context = OwinContext.Create(environment);

            var origin = (context.RequestHeadersDictionary["Origin"] ?? "").ToLowerInvariant();
            if(context.RequestHttpMethod == HttpMethod.Options) {
                result = false;
                HandlePreflight(context, origin);
            } else {
                HandleSimpleRequest(context, origin);
            }

            return result;
        }

        /// <summary>
        /// Handles pre-flight requests.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="origin"></param>
        private void HandlePreflight(OwinContext context, string origin)
        {
            var forbidden = true;

            if(OriginIsAllowed(origin)) {
                var requestMethod = (context.RequestHeadersDictionary["Access-Control-Request-Method"] ?? "").Trim().ToUpperInvariant();
                if(requestMethod != "") {
                    forbidden = false;

                    context.ResponseHeadersDictionary["Access-Control-Allow-Origin"] = origin;
                    context.ResponseHeadersDictionary.SetValues("Access-Control-Allow-Methods", _DefaultAllowableMethods);

                    if(!_DefaultAllowableMethods.Contains(requestMethod)) {
                        context.ResponseHeadersDictionary.Append("Access-Control-Allow-Methods", requestMethod);
                    }

                    var requestHeaders = context.RequestHeadersDictionary["Access-Control-Request-Headers"];
                    if(requestHeaders != null) {
                        context.ResponseHeadersDictionary["Access-Control-Allow-Headers"] = requestHeaders;
                    }
                }
            }

            context.ResponseHttpStatusCode = forbidden ? HttpStatusCode.Forbidden : HttpStatusCode.OK;
        }

        /// <summary>
        /// Adds Allow-Origin headers to simple requests that match the origin.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="origin"></param>
        private void HandleSimpleRequest(OwinContext context, string origin)
        {
            if(OriginIsAllowed(origin)) {
                context.ResponseHeadersDictionary["Access-Control-Allow-Origin"] = origin;
            }
        }

        /// <summary>
        /// Returns true if the origin is allowed.
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        private bool OriginIsAllowed(string origin)
        {
            RefreshConfiguration();

            return _CorsEnabled && (_AllowAllDomains || _AllowedDomains.Contains(origin));
        }

        /// <summary>
        /// Reloads the configuration if it has changed since the last time we fetched it.
        /// </summary>
        private void RefreshConfiguration()
        {
            if(_SharedConfiguration == null) {
                _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
            }

            var configurationChangedUtc = _SharedConfiguration.GetConfigurationChangedUtc();
            if(_ConfigurationLastParsed < configurationChangedUtc) {
                _ConfigurationLastParsed = configurationChangedUtc;

                var configuration = _SharedConfiguration.Get();
                var allowDomains = (configuration.GoogleMapSettings.AllowCorsDomains ?? "").Trim();

                _CorsEnabled = configuration.GoogleMapSettings.EnableCorsSupport;
                _AllowAllDomains = allowDomains == "*";
                _AllowedDomains = _AllowAllDomains ? new string[0] :
                    allowDomains
                        .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim().ToLowerInvariant())
                        .Distinct()
                        .ToArray();
            }
        }
    }
}
