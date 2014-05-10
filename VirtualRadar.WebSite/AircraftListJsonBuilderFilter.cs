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
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// The arguments passed to <see cref="AircraftListJsonBuilder.Build"/> that can be used to suppress
    /// aircraft from the list returned to the browser.
    /// </summary>
    class AircraftListJsonBuilderFilter
    {
        /// <summary>
        /// Gets or sets an airport code that the aircraft has to be flying from, to or via in order to pass the filter.
        /// </summary>
        public FilterString Airport { get; set; }

        /// <summary>
        /// Gets or sets the range of altitudes that an aircraft can be flying at in order to pass the filter.
        /// </summary>
        public FilterRange<int> Altitude { get; set; }

        /// <summary>
        /// Gets or sets the text that will be compared to aircraft's callsign before it can pass the filter.
        /// </summary>
        public FilterString Callsign { get; set; }

        /// <summary>
        /// Gets or sets the range of distances in kilometres that the aircraft can be at before it can pass the filter.
        /// </summary>
        public FilterRange<double> Distance { get; set; }

        /// <summary>
        /// Gets or sets the engine type that the aircraft must have before it can pass the filter.
        /// </summary>
        public FilterEnum<EngineType> EngineType { get; set; }

        /// <summary>
        /// Gets or sets the ICAO24 code that the aircraft must have before it can pass the filter.
        /// </summary>
        public FilterString Icao24 { get; set; }

        /// <summary>
        /// Gets or sets the text that will be compared to an aircraft's ICAO24 country before it can pass the filter.
        /// </summary>
        public FilterString Icao24Country { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft must be flagged as Interested in the BaseStation database before it can pass the filter.
        /// </summary>
        public FilterBool IsInteresting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft must be operated by the military before it can pass the filter.
        /// </summary>
        public FilterBool IsMilitary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft must (or must not) be transmitting a position before it can pass the filter.
        /// </summary>
        public FilterBool MustTransmitPosition { get; set; }

        /// <summary>
        /// Gets or sets the text that will be compared to an aircraft's operator to pass the filter.
        /// </summary>
        public FilterString Operator { get; set; }

        /// <summary>
        /// Gets or sets the lines of latitude and longitude that the aircraft must be within before it can pass the filter.
        /// The first coordinate is Top-Left and the second is Bottom-Right.
        /// </summary>
        public Pair<Coordinate> PositionWithin { get; set; }

        /// <summary>
        /// Gets or sets the text that will be compared to an aircraft's registration before it can pass the filter.
        /// </summary>
        public FilterString Registration { get; set; }

        /// <summary>
        /// Gets or sets the aircraft species that is allowed to pass the filter.
        /// </summary>
        public FilterEnum<Species> Species { get; set; }

        /// <summary>
        /// Gets or sets the range of squawk values that allow an aircraft to pass the filter.
        /// </summary>
        public FilterRange<int> Squawk { get; set; }

        /// <summary>
        /// Gets or sets the text will be compared to an aircraft's type before it can pass the filter.
        /// </summary>
        public FilterString Type { get; set; }

        /// <summary>
        /// Gets or sets the wake turbulence category that the aircraft must have before it can pass the filter.
        /// </summary>
        public FilterEnum<WakeTurbulenceCategory> WakeTurbulenceCategory { get; set; }
    }
}
