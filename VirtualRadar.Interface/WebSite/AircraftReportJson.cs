// Copyright © 2010 onwards, Andrew Whewell
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
using System.Runtime.Serialization;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The top-level JSON object for reports on the flights taken by a single aircraft.
    /// </summary>
    [DataContract]
    public class AircraftReportJson : ReportRowsJson
    {
        /// <summary>
        /// Gets or sets the object that describes the aircraft that is the subject of the report.
        /// </summary>
        [DataMember(Name="aircraftDetail", IsRequired=true)]
        public ReportAircraftJson Aircraft { get; set; }

        /// <summary>
        /// Gets a list of airports that are referred to by the routes.
        /// </summary>
        [DataMember(Name="airports", IsRequired=true)]
        public List<ReportAirportJson> Airports { get; private set; }

        /// <summary>
        /// Gets a list of routes that are referred to by the flights.
        /// </summary>
        [DataMember(Name="routes", IsRequired=true)]
        public List<ReportRouteJson> Routes { get; private set; }

        /// <summary>
        /// Gets a list of flights taken by the aircraft that match the report critera.
        /// </summary>
        [DataMember(Name="flights", IsRequired=true)]
        public List<ReportFlightJson> Flights { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AircraftReportJson()
        {
            Airports = new List<ReportAirportJson>();
            Routes = new List<ReportRouteJson>();
            Flights = new List<ReportFlightJson>();
        }
    }
}
