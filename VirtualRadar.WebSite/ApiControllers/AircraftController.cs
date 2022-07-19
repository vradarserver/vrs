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
using System.Net;
using AWhewell.Owin.Interface.WebApi;
using AWhewell.Owin.Utility;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite.ApiControllers
{
    /// <summary>
    /// Handles requests for aircraft data.
    /// </summary>
    public class AircraftController : BaseApiController
    {
        /// <summary>
        /// Returns a collection of AirportData.com thumbnails for an aircraft.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="reg"></param>
        /// <param name="numThumbs"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/3.00/aircraft/{icao}/airport-data-thumbnails")]
        [Route("AirportDataThumbnails.json")]                               // Pre-version 3 route
        public AirportDataThumbnailsJson GetAirportDataDotComThumbnails(string icao, string reg = null, int numThumbs = 1)
        {
            AirportDataThumbnailsJson result = null;

            if(String.IsNullOrEmpty(icao) || CustomConvert.Icao24(icao) == -1) {
                result = new AirportDataThumbnailsJson() { Error = "Invalid ICAO" };
            } else {
                try {
                    var airportDataDotCom = Factory.Resolve<IAirportDataDotCom>();
                    var outcome = airportDataDotCom.GetThumbnails((icao ?? "").ToUpperInvariant(), (reg ?? "").ToUpperInvariant(), numThumbs);
                    if(outcome.Result == null) {
                        result = new AirportDataThumbnailsJson() {
                            Status = (int)outcome.HttpStatusCode,
                            Error =  "Could not retrieve thumbnails"
                        };
                    } else {
                        result = outcome.Result;
                    }
                } catch(Exception ex) {
                    result = new AirportDataThumbnailsJson() {
                        Status = (int)HttpStatusCode.InternalServerError,
                        Error =  $"Exception caught while fetching thumbnails: {ex.Message}"
                    };
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the closest aircraft to a location. Itended for use with the proximity gadget.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lng"></param>
        /// <returns></returns>
        /// <remarks><para>
        /// This API endpoint has a couple of kludges to work around bugs and limitations in different
        /// versions of the proximity gadget. The two to be most aware of are: 1) values that would
        /// normally be integers or floating point numbers are returned as strings and 2) the result
        /// is always JSON, regardless of the type requested, and the JSON is always sent with a mime
        /// type of text rather than JSON.
        /// </para></remarks>
        [HttpGet]
        [Route("api/3.00/aircraft/closest")]
        [Route("ClosestAircraft.json")]             // pre-version 3 route
        public void GetClosest(double? lat = null, double? lng = null)
        {
            ProximityGadgetAircraftJson result = null;

            var context = Context;
            var config = Factory.ResolveSingleton<ISharedConfiguration>().Get();
            if(!context.IsInternet || config.InternetClientSettings.AllowInternetProximityGadgets) {
                var feedManager = Factory.ResolveSingleton<IFeedManager>();
                var feed = feedManager.GetByUniqueId(config.GoogleMapSettings.ClosestAircraftReceiverId, ignoreInvisibleFeeds: true);

                if(feed?.AircraftList != null) {
                    var aircraftList = feed.AircraftList.TakeSnapshot(out var unused1, out var unused2);
                    result = ProximityGadgetAircraftJson.ToModel(aircraftList, lat, lng);
                }

                if(result == null) {
                    result = new ProximityGadgetAircraftJson() {
                        WarningMessage = "Receiver is offline",
                    };
                }
            }

            _WebApiResponder.ReturnJsonObject(context, result, null, null, MimeType.Text);
        }
    }
}
