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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite.ApiControllers
{
    /// <summary>
    /// Serves results of report requests.
    /// </summary>
    public class ReportsController : PipelineApiController
    {
        private ISharedConfiguration _SharedConfiguration;

        private ISharedConfiguration InitialiseSharedConfiguration()
        {
            var result = _SharedConfiguration;
            if(result == null) {
                result = Factory.Singleton.Resolve<ISharedConfiguration>().Singleton;
                _SharedConfiguration = result;
            }

            return result;
        }

        [HttpGet]
        [Route("ReportRows.json")]                      // V2 route
        public HttpResponseMessage ReportRowsV2()
        {
            HttpResponseMessage result = null;

            if(PipelineRequest.IsLocalOrLan || InitialiseSharedConfiguration().Get().InternetClientSettings.CanRunReports) {
                var jsonObj = new FlightReportJson() {
                    CountRows =         0,
                    GroupBy =           "",
                    ProcessingTime =    "0.000",
                };

                var json = JsonConvert.SerializeObject(jsonObj);
                result = new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(json, Encoding.UTF8, MimeType.Json),
                };
                result.Headers.CacheControl = new CacheControlHeaderValue() {
                    MaxAge = TimeSpan.Zero,
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true,
                };
            }

            return result ?? new HttpResponseMessage(HttpStatusCode.Forbidden);
        }
    }
}
