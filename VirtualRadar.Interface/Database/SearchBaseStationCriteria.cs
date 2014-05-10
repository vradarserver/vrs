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
using System.Text;

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// An object that carries search criteria for use in searching the BaseStation database. All string criteria
    /// are case insensitive.
    /// </summary>
    public class SearchBaseStationCriteria
    {
        /// <summary>
        /// Gets or sets the callsign to search for.
        /// </summary>
        public FilterString Callsign { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that alternate callsigns should be searched for.
        /// </summary>
        /// <remarks>
        /// Alternate callsigns are callsigns where leading zeros are added to the flight number portion of the callsign.
        /// It also includes callsigns where the IATA or ICAO code is swapped out for the ICAO or IATA code. It does not
        /// have anything to do with alphanumeric alternates for a standard CODE/NUMBER callsign.
        /// </remarks>
        public bool UseAlternateCallsigns { get; set; }

        /// <summary>
        /// Gets or sets the date range to search across.
        /// </summary>
        public FilterRange<DateTime> Date { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that only flights that broadcast an emergency squawk code should be included.
        /// </summary>
        public FilterBool IsEmergency { get; set; }

        /// <summary>
        /// Gets or sets the aircraft owner / operator to search for.
        /// </summary>
        public FilterString Operator { get; set; }

        /// <summary>
        /// Gets or sets the registration to search for.
        /// </summary>
        public FilterString Registration { get; set; }

        /// <summary>
        /// Gets or sets the ICAO24 code to search for.
        /// </summary>
        public FilterString Icao { get; set; }

        /// <summary>
        /// Gets or sets the ICAO24 country to search for.
        /// </summary>
        public FilterString Country { get; set; }

        /// <summary>
        /// Gets or sets the model ICAO code to search for.
        /// </summary>
        public FilterString Type { get; set; }
    }
}
