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
using VirtualRadar.Interface;
using InterfaceFactory;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Handles requests for data from the www.airport-data.com API.
    /// </summary>
    class AirportDataProxyPage : Page
    {
        #region Fields
        /// <summary>
        /// The object that manages the calls to www.airport-data.com for us. This object is thread-safe.
        /// </summary>
        private IAirportDataDotCom _AirportDataApi;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AirportDataProxyPage(WebSite webSite) : base(webSite)
        {
            _AirportDataApi = Factory.Singleton.Resolve<IAirportDataDotCom>();
        }
        #endregion

        #region DoHandleRequest
        /// <summary>
        /// Proxies requests for airport-data.com data.
        /// </summary>
        /// <param name="server"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override bool DoHandleRequest(Interface.WebServer.IWebServer server, Interface.WebServer.RequestReceivedEventArgs args)
        {
            var result = false;

            if(args.PathAndFile.Equals("/AirportDataThumbnails.json", StringComparison.OrdinalIgnoreCase)) {
                result = true;

                var icao = QueryIcao(args, "icao");
                var registration = QueryString(args, "reg", true);
                var maxThumbnails = QueryInt(args, "numThumbs", 1);

                AirportDataThumbnailsJson json;
                if(icao == null) json = new AirportDataThumbnailsJson() { Error = "Invalid ICAO" };
                else {
                    try {
                        var response = _AirportDataApi.GetThumbnails(icao, registration, maxThumbnails);
                        if(response.Result == null) json = new AirportDataThumbnailsJson() { Status = (int)response.HttpStatusCode, Error = "Could not retrieve thumbnails", };
                        else                        json = response.Result;
                    } catch(Exception ex) {
                        json = new AirportDataThumbnailsJson() { Status = 500, Error = String.Format("Exception caught while fetching thumbnails: {0}", ex.Message), };
                    }
                }

                Responder.SendJson(args.Request, args.Response, json, null, null);
            }

            return result;
        }
        #endregion
    }
}
