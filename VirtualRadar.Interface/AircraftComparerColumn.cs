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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A list of column names that can be used with the <see cref="IAircraftComparer"/>. These names are
    /// sent to the server from Javascript on the browser, if you change these then you must also change
    /// the Javascript.
    /// </summary>
    public static class AircraftComparerColumn
    {
        /// <summary>
        /// The altitude that the aircraft is flying at.
        /// </summary>
        public const string Altitude = "ALT";

        /// <summary>
        /// The callsign the aircraft is flying under.
        /// </summary>
        public const string Callsign = "CALL";

        /// <summary>
        /// The country that the aircraft's ICAO24 code is allocated to.
        /// </summary>
        public const string Icao24Country = "COU";

        /// <summary>
        /// The airport where the aircraft began its flight.
        /// </summary>
        public const string Origin = "FROM";

        /// <summary>
        /// The manufacturer and model of the aircraft.
        /// </summary>
        public const string Model = "MODEL";

        /// <summary>
        /// The aircraft's unique ICAO24 identifier.
        /// </summary>
        public const string Icao24 = "ICAO";

        /// <summary>
        /// The company or individual operating the aircraft.
        /// </summary>
        public const string Operator = "OPERATOR";

        /// <summary>
        /// The ICAO code of the company operating the aircraft.
        /// </summary>
        public const string OperatorIcao = "OPERATORICAO";

        /// <summary>
        /// The aircraft's registration.
        /// </summary>
        public const string Registration = "REG";

        /// <summary>
        /// The aircraft's ground speed.
        /// </summary>
        public const string GroundSpeed = "SPD";

        /// <summary>
        /// The squawk code assigned to the aircraft by ATC.
        /// </summary>
        public const string Squawk = "SQK";

        /// <summary>
        /// The time the aircraft was first seen by the server.
        /// </summary>
        public const string FirstSeen = "TIMESEEN";

        /// <summary>
        /// The airport that the aircraft is flying to.
        /// </summary>
        public const string Destination = "TO";

        /// <summary>
        /// The ICAO8643 type code of the aircraft model.
        /// </summary>
        public const string Type = "TYPE";

        /// <summary>
        /// The aircraft's rate of ascent or descent.
        /// </summary>
        public const string VerticalRate = "VSI";

        /// <summary>
        /// The distance from the browser to the aircraft.
        /// </summary>
        public const string DistanceFromHere = "DIST";

        /// <summary>
        /// The number of engines on the aircraft (note that 'C' is a valid number of engines).
        /// </summary>
        public const string NumberOfEngines = "ENG";

        /// <summary>
        /// An indication of the type of vehicle, e.g. helicopter, landplane etc.
        /// </summary>
        public const string Species = "SPC";

        /// <summary>
        /// The ATC wake turbulence category for the aircraft.
        /// </summary>
        public const string WakeTurbulenceCategory = "WTC";

        /// <summary>
        /// The number of flights recorded for the aircraft in the BaseStation database.
        /// </summary>
        public const string FlightsCount = "FCNT";
    }
}
