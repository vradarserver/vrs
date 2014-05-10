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

namespace VirtualRadar.Interface.Database
{
    /// <summary>
    /// A simple data transfer object that describes a record in the BaseStation database for a sighting of an aircraft.
    /// </summary>
    public class BaseStationFlight
    {
        /// <summary>
        /// Gets or sets the aircraft that performed the flight. Some <see cref="IBaseStationDatabase"/> methods may not read
        /// the record, in which case this will be null.
        /// </summary>
        public BaseStationAircraft Aircraft { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the <see cref="BaseStationAircraft"/> record that corresponds to this flight.
        /// </summary>
        public int AircraftID { get; set; }

        /// <summary>
        /// Gets or sets the callsign that the flight was operating under.
        /// </summary>
        public string Callsign { get; set; }

        /// <summary>
        /// Gets or sets the time of the last message received from the aircraft (UTC).
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the altitude (in feet) the aircraft was at when the first message was received.
        /// </summary>
        public int? FirstAltitude { get; set; }

        /// <summary>
        /// Gets or sets the ground speed (in knots) the aircraft was travelling at when the first message was received.
        /// </summary>
        public float? FirstGroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the aircraft was on the ground when the first message was received.
        /// </summary>
        public bool FirstIsOnGround { get; set; }

        /// <summary>
        /// Gets or sets the latitude that the aircraft was over when the first message was received.
        /// </summary>
        public double? FirstLat { get; set; }

        /// <summary>
        /// Gets or sets the longitude that the aircraft was over when the first message was received.
        /// </summary>
        public double? FirstLon { get; set; }

        /// <summary>
        /// Gets or sets the squawk code that was being transmitted by the aircraft when the first message was received.
        /// </summary>
        public int? FirstSquawk { get; set; }

        /// <summary>
        /// Gets or sets the track (degrees from north of heading over the ground) that the aircraft was pointing in when the first message was received.
        /// </summary>
        public float? FirstTrack { get; set; }

        /// <summary>
        /// Gets or sets the rate of ascent (+ve) or descent (-ve) in feet per second that the aircraft was undertaking when the first message was received.
        /// </summary>
        public int? FirstVerticalRate { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the flight record on the database.
        /// </summary>
        public int FlightID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the aircraft changed squawk during the flight.
        /// </summary>
        public bool HadAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the aircraft transmitted an emergency squawk code during the flight.
        /// </summary>
        public bool HadEmergency { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pilot pressed the IDENT button during the flight.
        /// </summary>
        public bool HadSpi { get; set; }

        /// <summary>
        /// Gets or sets the altitude (in feet) the aircraft was at when the last message was received.
        /// </summary>
        public int? LastAltitude { get; set; }

        /// <summary>
        /// Gets or sets the ground speed (in knots) the aircraft was travelling at when the last message was received.
        /// </summary>
        public float? LastGroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the aircraft was on the ground when the last message was received.
        /// </summary>
        public bool LastIsOnGround { get; set; }

        /// <summary>
        /// Gets or sets the latitude that the aircraft was over when the last message was received.
        /// </summary>
        public double? LastLat { get; set; }

        /// <summary>
        /// Gets or sets the longitude that the aircraft was over when the last message was received.
        /// </summary>
        public double? LastLon { get; set; }

        /// <summary>
        /// Gets or sets the squawk code that was being transmitted by the aircraft when the last message was received.
        /// </summary>
        public int? LastSquawk { get; set; }

        /// <summary>
        /// Gets or sets the track (degrees from north of heading over the ground) that the aircraft was pointing in when the last message was received.
        /// </summary>
        public float? LastTrack { get; set; }

        /// <summary>
        /// Gets or sets the rate of ascent (+ve) or descent (-ve) in feet per second that the aircraft was undertaking when the last message was received.
        /// </summary>
        public int? LastVerticalRate { get; set; }

        /// <summary>
        /// Gets or sets the total number of ADSB messages received during the flight.
        /// </summary>
        public int? NumADSBMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the total number of Mode-S messages received during the flight.
        /// </summary>
        public int? NumModeSMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the total number of ID messages received during the flight.
        /// </summary>
        public int? NumIDMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the number of surface position messages received during the flight.
        /// </summary>
        public int? NumSurPosMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the number of air position messages received during the flight.
        /// </summary>
        public int? NumAirPosMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the number of air velocity messages received during the flight.
        /// </summary>
        public int? NumAirVelMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the number of surface ALT messages received during the flight.
        /// </summary>
        public int? NumSurAltMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the number of surface ID messages received during the flight.
        /// </summary>
        public int? NumSurIDMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the number of air to air messages received during the flight.
        /// </summary>
        public int? NumAirToAirMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the number of all call report messages received during the flight.
        /// </summary>
        public int? NumAirCallRepMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the total number of messages which contained a position during the flight.
        /// </summary>
        public int? NumPosMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the <see cref="BaseStationSession"/> record that was current
        /// when the flight was recorded.
        /// </summary>
        public int SessionID { get; set; }

        /// <summary>
        /// Gets or sets the time that the first message was received from the aircraft (UTC).
        /// </summary>
        public DateTime StartTime { get; set; }
    }
}
