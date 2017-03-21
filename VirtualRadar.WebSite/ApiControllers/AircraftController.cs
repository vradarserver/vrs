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
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Owin;

namespace VirtualRadar.WebSite.ApiControllers
{
    /// <summary>
    /// Handles requests for aircraft data.
    /// </summary>
    public class AircraftController : PipelineApiController
    {
        /// <summary>
        /// Returns a collection of AirportData.com thumbnails for an aircraft.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="reg"></param>
        /// <param name="numThumbs"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/1.00/aircraft/{icao}/airport-data-thumbnails")]
        [Route("AirportDataThumbnails.json")]                               // Pre-version 3 route
        public AirportDataThumbnailsJson GetAirportDataDotComThumbnails(string icao, string reg = null, int numThumbs = 1)
        {
            AirportDataThumbnailsJson result = null;

            if(String.IsNullOrEmpty(icao) || CustomConvert.Icao24(icao) == -1) {
                result = new AirportDataThumbnailsJson() { Error = "Invalid ICAO" };
            } else {
                try {
                    var airportDataDotCom = Factory.Singleton.Resolve<IAirportDataDotCom>();
                    var outcome = airportDataDotCom.GetThumbnails((icao ?? "").ToUpper(), (reg ?? "").ToUpper(), numThumbs);
                    if(outcome.Result == null) {
                        result = new AirportDataThumbnailsJson() { Status = (int)outcome.HttpStatusCode, Error = "Could not retrieve thumbnails" };
                    } else {
                        result = outcome.Result;
                    }
                } catch(Exception ex) {
                    result = new AirportDataThumbnailsJson() { Status = (int)HttpStatusCode.InternalServerError, Error = $"Exception caught while fetching thumbnails: {ex.Message}" };
                }
            }

            return result;
        }
    }
}
