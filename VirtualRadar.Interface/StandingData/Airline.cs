// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text.RegularExpressions;

namespace VirtualRadar.Interface.StandingData
{
    /// <summary>
    /// Describes an airline in the standing data files.
    /// </summary>
    public class Airline
    {
        /// <summary>
        /// Gets or sets the IATA code of the airline, if any.
        /// </summary>
        public string IataCode { get; set; }

        /// <summary>
        /// Gets or sets the ICAO code of the airline, if any.
        /// </summary>
        public string IcaoCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the airline.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the regular expression that, when matched, indicates that
        /// a flight for this airline is a positioning or ferry flight.
        /// </summary>
        public string PositioningFlightPattern { get; set; }

        /// <summary>
        /// Gets or sets the regular expression that, when matched, indicates that
        /// a flight for this airline is a charter flight.
        /// </summary>
        public string CharterFlightPattern { get; set; }

        /// <inheritdoc/>
        public override string ToString() => $"{(!String.IsNullOrEmpty(IcaoCode) ? IcaoCode : IataCode)} {Name}";

        /// <summary>
        /// Returns true if the flight number represents a positioning flight.
        /// </summary>
        /// <param name="flightNumber"></param>
        /// <returns></returns>
        public bool IsPositioningFlightNumber(string flightNumber) => MatchesFlightNumber(PositioningFlightPattern, flightNumber);

        /// <summary>
        /// Returns true if the flight number represents a charter flight.
        /// </summary>
        /// <param name="flightNumber"></param>
        /// <returns></returns>
        public bool IsCharterFlightNumber(string flightNumber) => MatchesFlightNumber(CharterFlightPattern, flightNumber);

        private bool MatchesFlightNumber(string regex, string flightNumber)
        {
            var result = !String.IsNullOrEmpty(regex) && !String.IsNullOrEmpty(flightNumber);
            if(result) {
                result = Regex.IsMatch(flightNumber, regex);
            }

            return result;
        }
    }
}
