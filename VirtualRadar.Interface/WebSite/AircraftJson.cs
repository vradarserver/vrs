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
    /// The object that describes an aircraft in JSON files that are sent to the browser.
    /// </summary>
    [DataContract]
    public class AircraftJson
    {
        /// <summary>
        /// Gets or sets the unique identifier of the aircraft.
        /// </summary>
        [DataMember(Name="Id")]
        public int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the receiver that is currently tracking the aircraft.
        /// </summary>
        [DataMember(Name="Rcvr", IsRequired=false, EmitDefaultValue=false)]
        public int? ReceiverId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the receiver emitted a signal level for the aircraft.
        /// </summary>
        [DataMember(Name="HasSig", IsRequired=false, EmitDefaultValue=false)]
        public bool? HasSignalLevel { get; set; }

        /// <summary>
        /// Gets or sets the signal level of the last message received for the aircraft. Use in conjunction with HasSignalLevel.
        /// </summary>
        [DataMember(Name="Sig", IsRequired=false, EmitDefaultValue=false)]
        public int? SignalLevel { get; set; }

        /// <summary>
        /// Gets or sets the 24-bit Mode-S identifier of the aircraft.
        /// </summary>
        [DataMember(Name="Icao", IsRequired=false, EmitDefaultValue=false)]
        public string Icao24 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Icao24"/> code is wrong - either it is an unallocated code
        /// or the aircraft is known to be transmitting the wrong code.
        /// </summary>
        [DataMember(Name="Bad", IsRequired=false, EmitDefaultValue=false)]
        public bool? Icao24Invalid { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's registration.
        /// </summary>
        [DataMember(Name="Reg", IsRequired=false, EmitDefaultValue=false)]
        public string Registration { get; set; }

        /// <summary>
        /// Gets or sets the date and time (UTC) that a transmission from the aircraft was first received by the server.
        /// </summary>
        [DataMember(Name="FSeen", IsRequired=false, EmitDefaultValue=false)]
        public DateTime? FirstSeen { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds that the aircraft has been tracked for.
        /// </summary>
        [DataMember(Name="TSecs", IsRequired=false, EmitDefaultValue=false)]
        public long SecondsTracked { get; set; }

        /// <summary>
        /// Gets or sets the number of messages received for the aircraft.
        /// </summary>
        [DataMember(Name="CMsgs", IsRequired=false, EmitDefaultValue=false)]
        public long? CountMessagesReceived { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's pressure altitude in feet.
        /// </summary>
        [DataMember(Name="Alt", IsRequired=false, EmitDefaultValue=false)]
        public int? Altitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's geometric altitude in feet.
        /// </summary>
        [DataMember(Name="GAlt", IsRequired=false, EmitDefaultValue=false)]
        public int? GeometricAltitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's air pressure setting in inches of mercury.
        /// </summary>
        [DataMember(Name="InHg", IsRequired=false, EmitDefaultValue=false)]
        public float? AirPressureInHg { get; set; }

        /// <summary>
        /// Gets or sets the type of altitude transmitted by the aircraft.
        /// </summary>
        [DataMember(Name="AltT", IsRequired=false, EmitDefaultValue=false)]
        public int? AltitudeType { get; set; }

        /// <summary>
        /// Gets or sets the altitude set on the autopilot / FMS etc.
        /// </summary>
        [DataMember(Name="TAlt", IsRequired=false, EmitDefaultValue=false)]
        public int? TargetAltitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's callsign.
        /// </summary>
        [DataMember(Name="Call", IsRequired=false, EmitDefaultValue=false)]
        public string Callsign { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the aircraft.
        /// </summary>
        [DataMember(Name="Lat", IsRequired=false, EmitDefaultValue=false)]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's longitude.
        /// </summary>
        [DataMember(Name="Long", IsRequired=false, EmitDefaultValue=false)]
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the time that the <see cref="Latitude"/> and <see cref="Longitude"/> were
        /// transmitted as a number of .NET ticks.
        /// </summary>
        [DataMember(Name="PosTime", IsRequired=false, EmitDefaultValue=false)]
        public long? PositionTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Latitude"/> and <see cref="Longitude"/>
        /// were calculated by an MLAT source.
        /// </summary>
        [DataMember(Name="Mlat", IsRequired=false, EmitDefaultValue=false)]
        public bool? PositionIsMlat { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the position is older than the display timeout.
        /// </summary>
        /// <remarks>
        /// This is usually only seen on merged feeds that include an MLAT feed, and only for aircraft
        /// that are not transmitting positions. The aircraft can appear to freeze on the map when it
        /// moves out of range of the MLAT receiver, this flag controls whether we should keep showing
        /// the aircraft on the map.
        /// </remarks>
        [DataMember(Name="PosStale", IsRequired=false, EmitDefaultValue=false)]
        public bool? PositionIsStale { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the last message received for this aircraft was from a TIS-B source.
        /// </summary>
        [DataMember(Name="Tisb", IsRequired=false, EmitDefaultValue=false)]
        public bool? IsTisb { get; set; }

        /// <summary>
        /// Gets or sets the ground speed of the aircraft in knots.
        /// </summary>
        [DataMember(Name="Spd", IsRequired=false, EmitDefaultValue=false)]
        public float? GroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets the heading that the aircraft is tracking across the ground in degrees from 0° north.
        /// </summary>
        [DataMember(Name="Trak", IsRequired=false, EmitDefaultValue=false)]
        public float? Track { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the track is the aircraft's heading, not its ground track.
        /// </summary>
        [DataMember(Name="TrkH", IsRequired=false, EmitDefaultValue=false)]
        public bool? TrackIsHeading { get; set; }

        /// <summary>
        /// Gets or sets the heading on the aircraft's autopilot or FMS.
        /// </summary>
        [DataMember(Name="TTrk", IsRequired=false, EmitDefaultValue=false)]
        public float? TargetTrack { get; set; }

        /// <summary>
        /// Gets or sets the ICAO8643 type code of the aircraft.
        /// </summary>
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the English description of the aircraft model. Usually includes the manufacturer.
        /// </summary>
        [DataMember(Name="Mdl", IsRequired=false, EmitDefaultValue=false)]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer's name.
        /// </summary>
        [DataMember(Name="Man", IsRequired=false, EmitDefaultValue=false)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the construction / serial number of the aircraft.
        /// </summary>
        [DataMember(Name="CNum", IsRequired=false, EmitDefaultValue=false)]
        public string ConstructionNumber { get; set; }

        /// <summary>
        /// Gets or sets the airport that the aircraft set out from.
        /// </summary>
        [DataMember(Name="From", IsRequired=false, EmitDefaultValue=false)]
        public string Origin { get; set; }

        /// <summary>
        /// Gets or sets the airport that the aircraft is travelling to.
        /// </summary>
        [DataMember(Name="To", IsRequired=false, EmitDefaultValue=false)]
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets a list of airports that the aircraft will be stopping at on its way to <see cref="Destination"/>.
        /// </summary>
        [DataMember(Name="Stops", IsRequired=false, EmitDefaultValue=false)]
        public List<string> Stopovers { get; set; }

        /// <summary>
        /// Gets or sets the operator's name.
        /// </summary>
        [DataMember(Name="Op", IsRequired=false, EmitDefaultValue=false)]
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the operator's ICAO code.
        /// </summary>
        [DataMember(Name="OpIcao", IsRequired=false, EmitDefaultValue=false)]
        public string OperatorIcao { get; set; }

        /// <summary>
        /// Gets or sets the squawk currently transmitted by the aircraft.
        /// </summary>
        [DataMember(Name="Sqk", IsRequired=false, EmitDefaultValue=false)]
        public string Squawk { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the aircraft is transmitting a mayday squawk.
        /// </summary>
        [DataMember(Name="Help", IsRequired=false, EmitDefaultValue=false)]
        public bool? Emergency { get; set; }

        /// <summary>
        /// Gets or sets the vertical speed in feet per second.
        /// </summary>
        [DataMember(Name="Vsi", IsRequired=false, EmitDefaultValue=false)]
        public int? VerticalRate { get; set; }

        /// <summary>
        /// Gets or sets the type of altitude reported in <see cref="VerticalRate"/>.
        /// </summary>
        [DataMember(Name="VsiT", IsRequired=false, EmitDefaultValue=false)]
        public int? VerticalRateType { get; set; }

        /// <summary>
        /// Gets or sets the distance from the browser's location to the aircraft in kilometres.
        /// </summary>
        [DataMember(Name="Dst", IsRequired=false, EmitDefaultValue=false)]
        public double? DistanceFromHere { get; set; }

        /// <summary>
        /// Gets or sets the bearing from the browser to the aircraft in degrees from 0° north.
        /// </summary>
        [DataMember(Name="Brng", IsRequired=false, EmitDefaultValue=false)]
        public double? BearingFromHere { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the wake turbulence category of the aircraft (see <see cref="WakeTurbulenceCategory"/>).
        /// </summary>
        [DataMember(Name="WTC", IsRequired=false, EmitDefaultValue=false)]
        public int? WakeTurbulenceCategory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the species of aircraft (see <see cref="Species"/>).
        /// </summary>
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public int? Species { get; set; }

        /// <summary>
        /// Gets or sets the number of engines that the aircraft has - note that this is a copy of the ICAO8643 engine count which is not
        /// always a number!
        /// </summary>
        [DataMember(Name="Engines", IsRequired=false, EmitDefaultValue=false)]
        public string NumberOfEngines { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the type of engines that the aircraft uses (see <see cref="EngineType"/>).
        /// </summary>
        [DataMember(Name="EngType", IsRequired=false, EmitDefaultValue=false)]
        public int? EngineType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the placement of the engines mounted to the aircraft (see <see cref="EnginePlacement"/>).
        /// </summary>
        [DataMember(Name="EngMount", IsRequired=false, EmitDefaultValue=false)]
        public int? EnginePlacement { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is operated by a country's military.
        /// </summary>
        [DataMember(Name="Mil", IsRequired=false, EmitDefaultValue=false)]
        public bool? IsMilitary { get; set; }

        /// <summary>
        /// Gets or sets the country that the aircraft's <see cref="Icao24"/> was allocated to.
        /// </summary>
        [DataMember(Name="Cou", IsRequired=false, EmitDefaultValue=false)]
        public string Icao24Country { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server can supply a picture of the aircraft.
        /// </summary>
        [DataMember(Name="HasPic", IsRequired=false, EmitDefaultValue=false)]
        public bool? HasPicture { get; set; }

        /// <summary>
        /// Gets or sets the width of the aircraft picture in pixels.
        /// </summary>
        [DataMember(Name="PicX", IsRequired=false, EmitDefaultValue=false)]
        public int? PictureWidth { get; set; }

        /// <summary>
        /// Gets or sets the height of the aircraft picture in pixels.
        /// </summary>
        [DataMember(Name="PicY", IsRequired=false, EmitDefaultValue=false)]
        public int? PictureHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is flagged as interesting in the BaseStation database.
        /// </summary>
        [DataMember(Name="Interested", IsRequired=false, EmitDefaultValue=false)]
        public bool? IsInteresting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how many flights this aircraft has logged in the BaseStation database.
        /// </summary>
        [DataMember(Name="FlightsCount", IsRequired=false, EmitDefaultValue=false)]
        public int? FlightsCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft is on the ground.
        /// </summary>
        [DataMember(Name="Gnd", IsRequired=false, EmitDefaultValue=false)]
        public bool? OnGround { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the type of speed the aircraft is transmitting.
        /// </summary>
        [DataMember(Name="SpdTyp", IsRequired=false, EmitDefaultValue=false)]
        public int? SpeedType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Callsign"/> came from an unreliable source.
        /// </summary>
        [DataMember(Name="CallSus", IsRequired=false, EmitDefaultValue=false)]
        public bool? CallsignIsSuspect { get; set; }

        /// <summary>
        /// Gets or sets the user tag from the aircraft's database record.
        /// </summary>
        [DataMember(Name="Tag", IsRequired=false, EmitDefaultValue=false)]
        public string UserTag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the server wants all trails for the aircraft to be reset.
        /// </summary>
        [DataMember(IsRequired=false, EmitDefaultValue=false)]
        public bool ResetTrail { get; set; }

        /// <summary>
        /// Gets or sets the trail type. It is empty for plain trails (just position), 'a' for trails that have altitude
        /// attached and 's' for trails with speeds attached.
        /// </summary>
        [DataMember(Name="TT", IsRequired=false, EmitDefaultValue=false)]
        public string TrailType { get; set; }

        /// <summary>
        /// Gets or sets the transponder type.
        /// </summary>
        [DataMember(Name="Trt", IsRequired=false, EmitDefaultValue=false)]
        public int? TransponderType { get; set; }

        /// <summary>
        /// Gets or sets the year the aircraft was manufactured.
        /// </summary>
        [DataMember(Name="Year", EmitDefaultValue=false)]
        public string YearBuilt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the aircraft was seen on a SatCom feed.
        /// </summary>
        [DataMember(Name="Sat", EmitDefaultValue=false)]
        public bool IsSatcomFeed { get; set; }

        /// <summary>
        /// Gets or sets a list of coordinates representing the full trail for the aircraft. If <see cref="ResetTrail"/>
        /// is true then it is the entire trail, otherwise it extends the existing trail.
        /// </summary>
        /// <remarks>
        /// This is a set of 3-tuples - latitude, longitude and the heading. If <see cref="TrailType"/> is 'a' or 's'
        /// then it's a set of 4-tuples, with altitude or speed being added to the end of the tuple.
        /// </remarks>
        [DataMember(Name="Cot", IsRequired=false, EmitDefaultValue=false)]
        public List<double?> FullCoordinates { get; set; }

        /// <summary>
        /// Gets or sets a list of coordinates representing the short trail for the aircraft. If <see cref="ResetTrail"/>
        /// is true then it is the entire trail, otherwise it extends the existing trail.
        /// </summary>
        /// <remarks>
        /// This is a set of 3-tuples - latitude, longitude and the time of the position in Javascript ticks. When <see cref="TrailType"/>
        /// is 'a' or 's' then it's a set of 4-tuples, where altitude or speed are added to the end of the tuple.
        /// </remarks>
        [DataMember(Name="Cos", IsRequired=false, EmitDefaultValue=false)]
        public List<double?> ShortCoordinates { get; set; }
    }
}
