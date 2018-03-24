// Copyright © 2018 onwards, Andrew Whewell
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
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Listener
{
    /// <summary>
    /// Describes an aircraft in an <see cref="AirnavXRangeJson"/> object.
    /// </summary>
    [DataContract]
    public class AirnavXRangeAircraftJson
    {
        /// <summary>
        /// Gets or sets the aircraft's ICAO. Note that this is lower-case.
        /// </summary>
        [DataMember(Name = "hex", IsRequired = true)]
        public string RawIcao24 { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's ICAO in upper-case.
        /// </summary>
        [IgnoreDataMember]
        public string Icao24 {
            get => RawIcao24?.ToUpper();
            set => RawIcao24 = value?.ToLower();
        }

        /// <summary>
        /// Gets or sets the number of messages picked up from the aircraft.
        /// </summary>
        [DataMember(Name = "messages", IsRequired = true)]
        public int CountMessages { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds (single digit floating point) since the aircraft was first seen.
        /// </summary>
        [DataMember(Name = "seen", IsRequired = true)]
        public float SecondsSinceFirstSeen { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's octal squawk code as a string.
        /// </summary>
        [DataMember(Name = "squawk", EmitDefaultValue = false, IsRequired = false)]
        public string RawSquawk { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's squawk code as an integer.
        /// </summary>
        [IgnoreDataMember]
        public int? Squawk
        {
            get {
                if(String.IsNullOrEmpty(RawSquawk) || !int.TryParse(RawSquawk, out var result)) {
                    return (int?)null;
                }
                return result;
            }
            set => RawSquawk = value?.ToString("0000", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets or sets the integer altitude of the aircraft in feet or the string 'ground' when the aircraft is
        /// on the ground.
        /// </summary>
        [DataMember(Name = "altitude", EmitDefaultValue = false, IsRequired = false)]
        public string RawAltitude { get; set; }

        /// <summary>
        /// Gets or sets the altitude of the aircraft in feet.
        /// </summary>
        [IgnoreDataMember]
        public int? Altitude
        {
            get {
                if(RawAltitude == "ground") {
                    return 0;
                }
                if(String.IsNullOrEmpty(RawAltitude) || !int.TryParse(RawAltitude, out var result)) {
                    return (int?)null;
                }
                return result;
            }
            set => RawAltitude = value?.ToString("0", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Gets or sets a flag indicating that the aircraft is on the ground.
        /// </summary>
        [IgnoreDataMember]
        public bool? OnGround
        {
            get => RawAltitude == null ? (bool?)null : RawAltitude == "ground";
            set => RawAltitude = value.GetValueOrDefault() ? "ground" : RawAltitude == "ground" ? null : RawAltitude;
        }

        /// <summary>
        /// Gets or sets the vertical rate in feet per minute.
        /// </summary>
        [DataMember(Name = "vert_rate", EmitDefaultValue = false, IsRequired = false)]
        public int? VerticalRate { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's ground track in degrees from 0 north.
        /// </summary>
        [DataMember(Name = "track", EmitDefaultValue = false, IsRequired = false)]
        public int? Track { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's speed in knots.
        /// </summary>
        [DataMember(Name = "speed", EmitDefaultValue = false, IsRequired = false)]
        public int? GroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's callsign with trailing space padding.
        /// </summary>
        [DataMember(Name = "flight", EmitDefaultValue = false, IsRequired = false)]
        public string RawCallsign { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's callsign.
        /// </summary>
        [IgnoreDataMember]
        public string Callsign
        {
            get => RawCallsign?.Trim();
            set => RawCallsign = value == null ? null : String.Format("{0,-8}", value);
        }

        /// <summary>
        /// Gets or sets the aircraft's latitude.
        /// </summary>
        [DataMember(Name = "lat", EmitDefaultValue = false, IsRequired = false)]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's longitude.
        /// </summary>
        [DataMember(Name = "lon", EmitDefaultValue = false, IsRequired = false)]
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds (single digit floating point) since the last position message was seen.
        /// </summary>
        [DataMember(Name = "seen_pos", EmitDefaultValue = false, IsRequired = false)]
        public float SecondsSinceLastPosition { get; set; }

        /// <summary>
        /// Gets or sets some category or other... not sure what this is yet.
        /// </summary>
        [DataMember(Name = "category", EmitDefaultValue = false, IsRequired = false)]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the Navigational Uncertainty Category - position.
        /// </summary>
        [DataMember(Name = "nucp", EmitDefaultValue = false, IsRequired = false)]
        public int? NUCp { get; set; }

        /// <summary>
        /// Gets or sets the signal strength. -min value is weakest signal, 0 is strongest.
        /// </summary>
        [DataMember(Name = "rssi", EmitDefaultValue = false, IsRequired = false)]
        public float? SignalLevel { get; set; }
    }
}
