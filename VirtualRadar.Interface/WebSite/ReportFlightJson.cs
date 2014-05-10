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
    /// Describes the flight of an aircraft in a report.
    /// </summary>
    [DataContract]
    public class ReportFlightJson
    {
        /// <summary>
        /// Gets or sets the position of the flight within the set of flights retrieved by the criteria.
        /// </summary>
        [DataMember(Name="row", IsRequired=true)]
        public int RowNumber { get; set; }

        /// <summary>
        /// Gets or sets the index in the Aircraft list on either <see cref="AircraftReportJson"/> or <see cref="FlightReportJson"/>
        /// of the <see cref="ReportAircraftJson"/> for the aircraft's that undertook the flight.
        /// </summary>
        [DataMember(Name="acIdx")]
        public int? AircraftIndex { get; set; }

        /// <summary>
        /// Gets or sets the callsign that the aircraft used during the flight.
        /// </summary>
        [DataMember(Name="call", EmitDefaultValue=false)]
        public string Callsign { get; set; }

        /// <summary>
        /// Gets or sets the index in the Routes list on either <see cref="AircraftReportJson"/> or <see cref="FlightReportJson"/>
        /// of the <see cref="ReportRouteJson"/> for the route that the aircraft flew.
        /// </summary>
        [DataMember(Name="rtIdx", IsRequired=true)]
        public int RouteIndex { get; set; }

        /// <summary>
        /// Gets or sets the time that a transmission from the aircraft was first seen.
        /// </summary>
        [DataMember(Name="start", EmitDefaultValue=false)]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the time that a transmission from the aircraft was last seen.
        /// </summary>
        [DataMember(Name="end", EmitDefaultValue=false)]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the altitude that the aircraft was flying at when it was first seen.
        /// </summary>
        [DataMember(Name="fAlt", EmitDefaultValue=false)]
        public int FirstAltitude { get; set; }

        /// <summary>
        /// Gets or sets the ground speed that the aircraft was travelling at when it was first seen.
        /// </summary>
        [DataMember(Name="fSpd", EmitDefaultValue=false)]
        public int FirstGroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the aircraft was on the ground when it was first seen.
        /// </summary>
        [DataMember(Name="fOnGnd", EmitDefaultValue=false)]
        public bool FirstIsOnGround { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the aircraft when it was first seen.
        /// </summary>
        [DataMember(Name="fLat", EmitDefaultValue=false)]
        public double FirstLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the aircraft when it was first seen.
        /// </summary>
        [DataMember(Name="fLng", EmitDefaultValue=false)]
        public double FirstLongitude { get; set; }

        /// <summary>
        /// Gets or sets the squawk that the aircraft was transmitting when it was first seen.
        /// </summary>
        [DataMember(Name="fSqk", EmitDefaultValue=false)]
        public int FirstSquawk { get; set; }

        /// <summary>
        /// Gets or sets the heading that the aircraft was flying over the ground when it was first seen.
        /// </summary>
        [DataMember(Name="fTrk", EmitDefaultValue=false)]
        public float FirstTrack { get; set; }

        /// <summary>
        /// Gets or sets the rate of climb / descent that the aircraft was making when first seen.
        /// </summary>
        [DataMember(Name="fVsi", EmitDefaultValue=false)]
        public int FirstVerticalRate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft changed its squawk during the flight.
        /// </summary>
        [DataMember(Name="hAlrt", EmitDefaultValue=false)]
        public bool HadAlert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft transmitted an emergency squawk code during the flight.
        /// </summary>
        [DataMember(Name="hEmg", EmitDefaultValue=false)]
        public bool HadEmergency { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the pilot pressed the IDENT button at some point during the flight.
        /// </summary>
        [DataMember(Name="hSpi", EmitDefaultValue=false)]
        public bool HadSpi { get; set; }

        /// <summary>
        /// Gets or sets the altitude that the aircraft was flying at when it was last seen.
        /// </summary>
        [DataMember(Name="lAlt", EmitDefaultValue=false)]
        public int LastAltitude { get; set; }

        /// <summary>
        /// Gets or sets the ground speed that the aircraft was travelling at when it was last seen.
        /// </summary>
        [DataMember(Name="lSpd", EmitDefaultValue=false)]
        public int LastGroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the aircraft was on the ground when it was last seen.
        /// </summary>
        [DataMember(Name="lOnGnd", EmitDefaultValue=false)]
        public bool LastIsOnGround { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the aircraft when it was last seen.
        /// </summary>
        [DataMember(Name="lLat", EmitDefaultValue=false)]
        public double LastLatitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the aircraft when it was last seen.
        /// </summary>
        [DataMember(Name="lLng", EmitDefaultValue=false)]
        public double LastLongitude { get; set; }

        /// <summary>
        /// Gets or sets the squawk that the aircraft was transmitting when it was last seen.
        /// </summary>
        [DataMember(Name="lSqk", EmitDefaultValue=false)]
        public int LastSquawk { get; set; }

        /// <summary>
        /// Gets or sets the heading that the aircraft was flying over the ground when it was last seen.
        /// </summary>
        [DataMember(Name="lTrk", EmitDefaultValue=false)]
        public float LastTrack { get; set; }

        /// <summary>
        /// Gets or sets the rate of climb / descent that the aircraft was making when last seen.
        /// </summary>
        [DataMember(Name="lVsi", EmitDefaultValue=false)]
        public int LastVerticalRate { get; set; }

        /// <summary>
        /// Gets or sets the total number of ADSB messages received over the course of the flight.
        /// </summary>
        [DataMember(Name="cADSB", EmitDefaultValue=false)]
        public int NumADSBMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the total number of Mode-S messages received over the course of the flight.
        /// </summary>
        [DataMember(Name="cMDS", EmitDefaultValue=false)]
        public int NumModeSMsgRec { get; set; }

        /// <summary>
        /// Gets or sets the total number of messages that contained position information received over the course of the flight.
        /// </summary>
        [DataMember(Name="cPOS", EmitDefaultValue=false)]
        public int NumPosMsgRec { get; set; }
    }
}
