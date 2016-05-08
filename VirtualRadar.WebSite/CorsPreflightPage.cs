// Copyright © 2016 onwards, Andrew Whewell
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
using System.Text;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// A page that can handle CORS preflight requests.
    /// </summary>
    class CorsPreflightPage : Page
    {
        private bool _CorsEnabled;

        private string[] _AllowedDomains = new string[] {};

        private bool _AllowAllDomains;

        public CorsPreflightPage(WebSite webSite) : base(webSite)
        {
        }

        protected override void DoLoadConfiguration(Configuration configuration)
        {
            _CorsEnabled = configuration.GoogleMapSettings.EnableCorsSupport;
            if(!_CorsEnabled) {
                _AllowedDomains = new string[] {};
                _AllowAllDomains = false;
            } else {
                var allowCorsDomains = (configuration.GoogleMapSettings.AllowCorsDomains ?? "").Trim();
                _AllowAllDomains = allowCorsDomains == "*";
                _AllowedDomains = _AllowAllDomains 
                    ? new string[] {}
                    : allowCorsDomains
                        .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim().ToLowerInvariant())
                        .Distinct()
                        .ToArray();
            }
        }

        protected override bool DoHandleRequest(IWebServer server, RequestReceivedEventArgs args)
        {
            var handled = false;

            var request = args.Request;
            var response = args.Response;

            if(String.Equals(request.HttpMethod, "OPTIONS", StringComparison.OrdinalIgnoreCase)) {
                handled = true;
                HandleOptionsRequest(request, response);
            } else {
                if(_CorsEnabled) {
                    var origin = request.CorsOrigin;
                    if(OriginIsAllowed(request, origin)) {
                        request.IsValidCorsRequest = true;
                    }
                }
            }

            return handled;
        }

        private void HandleOptionsRequest(IRequest request, IResponse response)
        {
            var origin = request.CorsOrigin;
            var allowed = OriginIsAllowed(request, origin);

            response.StatusCode = allowed ? HttpStatusCode.OK : HttpStatusCode.Forbidden;
            if(allowed) {
                var requestMethod = (request.Headers["Access-Control-Request-Method"] ?? "").ToUpper();
                var allowedMethods = new List<string>() { "POST", "GET", "OPTIONS" };
                if(!allowedMethods.Contains(requestMethod)) {
                    allowedMethods.Add(requestMethod);
                }

                var requestHeaders = (request.Headers["Access-Control-Request-Headers"] ?? "")
                        .Split(',')
                        .Select(r => (r ?? "").Trim().ToUpper())
                        .Where(r => r != "")
                        .ToArray();
                var allowedHeaders = new string[] { "X-VirtualRadarServer-AircraftIds" }
                        .Concat(requestHeaders)
                        .Distinct()
                        .ToArray();

                response.AddHeader("Access-Control-Allow-Origin", origin);
                response.AddHeader("Access-Control-Allow-Methods", String.Join(", ", allowedMethods.ToArray()));
                response.AddHeader("Access-Control-Allow-Headers", String.Join(", ", allowedHeaders));
            }
            response.ContentLength = 0;
        }

        private bool OriginIsAllowed(IRequest request, string origin)
        {
            var result = false;

            var normalisedOrigin = (origin ?? "").ToLowerInvariant();
            if(_CorsEnabled) {
                result = _AllowAllDomains;
                if(!result && origin != "") {
                    var allowedDomains = _AllowedDomains;
                    if(allowedDomains.Contains(normalisedOrigin)) {
                        result = true;
                    }
                }
            }

            return result;
        }
    }
}
