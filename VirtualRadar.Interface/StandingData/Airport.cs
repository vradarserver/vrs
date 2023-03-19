﻿// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// Describes an airport in the standing data files.
    /// </summary>
    public class Airport
    {
        /// <summary>
        /// Gets or sets the IATA code for the airport.
        /// </summary>
        public string IataCode { get; set; }

        /// <summary>
        /// Gets or sets the ICAO code for the airport.
        /// </summary>
        public string IcaoCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the airport.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the country that the airport is in.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the latitude part of the airport's coordinate.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude part of the airport's coordinate.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the altitude in feet of the airport.
        /// </summary>
        public int? AltitudeFeet { get; set; }

        /// <summary>
        /// See Object.ToString().
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append(!String.IsNullOrEmpty(IcaoCode)
                ? IcaoCode
                : IataCode
            );
            result.Append(' ');
            result.Append(Name);
            if(!String.IsNullOrEmpty(Country)) {
                result.Append(", ");
                result.Append(Country);
            }

            return result.ToString();
        }
    }
}
