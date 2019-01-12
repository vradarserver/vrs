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

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// Describes an airport in the track history database.
    /// </summary>
    public class TrackHistoryAirport
    {
        /// <summary>
        /// Gets or sets the record's unique ID.
        /// </summary>
        public int AirportID { get; set; }

        /// <summary>
        /// Gets or sets the airport's ICAO code.
        /// </summary>
        public string Icao { get; set; }

        /// <summary>
        /// Gets or sets the airport's IATA code.
        /// </summary>
        public string Iata { get; set; }

        /// <summary>
        /// Gets or sets the airport's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the latitude component of the airport's location.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude component of the airport's location.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the country that the airport is within.
        /// </summary>
        public int? CountryID { get; set; }

        /// <summary>
        /// Gets or sets the time when the record was first created.
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// Gets or sets the time that the record was last updated.
        /// </summary>
        public DateTime UpdatedUtc { get; set; }
    }
}
