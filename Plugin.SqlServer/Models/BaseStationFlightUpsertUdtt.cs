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
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Plugin.SqlServer.Models
{
    /// <summary>
    /// Describes the BaseStationFlightUpsert UDTT.
    /// </summary>
    class BaseStationFlightUpsertUdtt
    {
        /// <summary>
        /// The UDTT properties for the type.
        /// </summary>
        public static UdttProperty<BaseStationFlightUpsertUdtt>[] UdttProperties { get; private set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(0)]
        public int SessionID { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(1)]
        public int AircraftID { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(2)]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(3)]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(4)]
        public string Callsign { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(5)]
        public int? NumPosMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(6)]
        public int? NumADSBMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(7)]
        public int? NumModeSMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(8)]
        public int? NumIDMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(9)]
        public int? NumSurPosMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(10)]
        public int? NumAirPosMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(11)]
        public int? NumAirVelMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(12)]
        public int? NumSurAltMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(13)]
        public int? NumSurIDMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(14)]
        public int? NumAirToAirMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(15)]
        public int? NumAirCallRepMsgRec { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(16)]
        public bool FirstIsOnGround { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(17)]
        public bool LastIsOnGround { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(18)]
        public double? FirstLat { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(19)]
        public double? LastLat { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(20)]
        public double? FirstLon { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(21)]
        public double? LastLon { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(22)]
        public float? FirstGroundSpeed { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(23)]
        public float? LastGroundSpeed { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(24)]
        public int? FirstAltitude { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(25)]
        public int? LastAltitude { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(26)]
        public int? FirstVerticalRate { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(27)]
        public int? LastVerticalRate { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(28)]
        public float? FirstTrack { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(29)]
        public float? LastTrack { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(30)]
        public int? FirstSquawk { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(31)]
        public int? LastSquawk { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(32)]
        public bool HadAlert { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(33)]
        public bool HadEmergency { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(34)]
        public bool HadSPI { get; set; }

        /// <summary>
        /// See UDTT declaration.
        /// </summary>
        [Ordinal(35)]
        public string UserNotes { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BaseStationFlightUpsertUdtt()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="flight"></param>
        public BaseStationFlightUpsertUdtt(BaseStationFlightUpsert flight)
        {
            AircraftID =            flight.AircraftID;
            Callsign =              flight.Callsign;
            EndTime =               flight.EndTime;
            FirstAltitude =         flight.FirstAltitude;
            FirstGroundSpeed =      flight.FirstGroundSpeed;
            FirstIsOnGround =       flight.FirstIsOnGround;
            FirstLat =              flight.FirstLat;
            FirstLon =              flight.FirstLon;
            FirstSquawk =           flight.FirstSquawk;
            FirstTrack =            flight.FirstTrack;
            FirstVerticalRate =     flight.FirstVerticalRate;
            HadAlert =              flight.HadAlert;
            HadEmergency =          flight.HadEmergency;
            HadSPI =                flight.HadSpi;
            LastAltitude =          flight.LastAltitude;
            LastGroundSpeed =       flight.LastGroundSpeed;
            LastIsOnGround =        flight.LastIsOnGround;
            LastLat =               flight.LastLat;
            LastLon =               flight.LastLon;
            LastSquawk =            flight.LastSquawk;
            LastTrack =             flight.LastTrack;
            LastVerticalRate =      flight.LastVerticalRate;
            NumADSBMsgRec =         flight.NumADSBMsgRec;
            NumModeSMsgRec =        flight.NumModeSMsgRec;
            NumIDMsgRec =           flight.NumIDMsgRec;
            NumSurPosMsgRec =       flight.NumSurPosMsgRec;
            NumAirPosMsgRec =       flight.NumAirPosMsgRec;
            NumAirVelMsgRec =       flight.NumAirVelMsgRec;
            NumSurAltMsgRec =       flight.NumSurAltMsgRec;
            NumSurIDMsgRec =        flight.NumSurIDMsgRec;
            NumAirToAirMsgRec =     flight.NumAirToAirMsgRec;
            NumAirCallRepMsgRec =   flight.NumAirCallRepMsgRec;
            NumPosMsgRec =          flight.NumPosMsgRec;
            SessionID =             flight.SessionID;
            StartTime =             flight.StartTime;
            UserNotes =             flight.UserNotes;
        }

        /// <summary>
        /// Static constructor.
        /// </summary>
        static BaseStationFlightUpsertUdtt()
        {
            UdttProperties = UdttProperty<BaseStationFlightUpsertUdtt>.GetUdttProperties();
        }
    }
}
