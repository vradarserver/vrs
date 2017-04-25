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
using System.Globalization;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The DTO that carries information about the closest aircraft to the Windows Vista/7 proximity gadget.
    /// </summary>
    /// <remarks>
    /// Note that values that would normally be integers and floating point numbers are represented here (and
    /// in the resulting JSON) as strings. This is a hangover from a bug in an earlier version of the proximity
    /// gadget.
    /// </remarks>
    [DataContract]
    public class ProximityGadgetClosestAircraftJson
    {
        /// <summary>
        /// Gets or sets the 24-bit Mode-S identifier of the aircraft.
        /// </summary>
        [DataMember(Name="icao", IsRequired=true)]
        public string Icao24 { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Icao24"/> code is wrong - either it is an unallocated code
        /// or the aircraft is known to be transmitting the wrong code.
        /// </summary>
        [DataMember(Name="invalid", EmitDefaultValue=false)]
        public bool Icao24Invalid { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the aircraft is transmitting a mayday squawk.
        /// </summary>
        [DataMember(Name="help", EmitDefaultValue=false)]
        public bool Emergency { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that a picture exists for the aircraft.
        /// </summary>
        [DataMember(Name="hasPicture", EmitDefaultValue=false)]
        public bool HasPicture { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's registration.
        /// </summary>
        [DataMember(Name="reg", EmitDefaultValue=false)]
        public string Registration { get; set; }

        /// <summary>
        /// Gets or sets the ICAO8643 type code of the aircraft.
        /// </summary>
        [DataMember(Name="type", EmitDefaultValue=false)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the aircraft's manufacturer.
        /// </summary>
        [DataMember(Name="man", EmitDefaultValue=false)]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the model of the aircraft.
        /// </summary>
        [DataMember(Name="model", EmitDefaultValue=false)]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's altitude in feet.
        /// </summary>
        [DataMember(Name="alt", EmitDefaultValue=false)]
        public string Altitude { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the aircraft.
        /// </summary>
        [DataMember(Name="lat", EmitDefaultValue=false)]
        public string Latitude { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's longitude.
        /// </summary>
        [DataMember(Name="lng", EmitDefaultValue=false)]
        public string Longitude { get; set; }

        /// <summary>
        /// Gets or sets the ground speed of the aircraft in knots.
        /// </summary>
        [DataMember(Name="spd", EmitDefaultValue=false)]
        public string GroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets the vertical speed in feet per second.
        /// </summary>
        [DataMember(Name="vsi", EmitDefaultValue=false)]
        public string VerticalRate { get; set; }

        /// <summary>
        /// Gets or sets the distance from the browser's location to the aircraft in kilometres.
        /// </summary>
        [DataMember(Name="km", EmitDefaultValue=false)]
        public string DistanceFromHere { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's callsign.
        /// </summary>
        [DataMember(Name="call", EmitDefaultValue=false)]
        public string Callsign { get; set; }

        /// <summary>
        /// Gets or sets the squawk currently transmitted by the aircraft.
        /// </summary>
        [DataMember(Name="squawk", EmitDefaultValue=false)]
        public string Squawk { get; set; }

        /// <summary>
        /// Gets or sets the operator's name.
        /// </summary>
        [DataMember(Name="op", EmitDefaultValue=false)]
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the operator's ICAO code.
        /// </summary>
        [DataMember(Name="opIcao", EmitDefaultValue=false)]
        public string OperatorIcao { get; set; }

        /// <summary>
        /// Gets or sets the airport that the aircraft set out from.
        /// </summary>
        [DataMember(Name="from", EmitDefaultValue=false)]
        public string Origin { get; set; }

        /// <summary>
        /// Gets or sets the airport that the aircraft is travelling to.
        /// </summary>
        [DataMember(Name="to", EmitDefaultValue=false)]
        public string Destination { get; set; }

        /// <summary>
        /// Gets or sets a list of airports that the aircraft will be stopping at on its way to <see cref="Destination"/>.
        /// </summary>
        [DataMember(Name="via", EmitDefaultValue=false)]
        public List<string> Stopovers { get; private set; } = new List<string>();

        /// <summary>
        /// Gets or sets the bearing from the browser to the aircraft in degrees from 0° north.
        /// </summary>
        [DataMember(Name="brng", EmitDefaultValue=false)]
        public string BearingFromHere { get; set; }

        /// <summary>
        /// Gets or sets the heading that the aircraft is tracking across the ground in degrees from 0° north.
        /// </summary>
        [DataMember(Name="hdg", EmitDefaultValue=false)]
        public string Track { get; set; }

        /// <summary>
        /// Creates a new model from an aircraft object passed across. The aircraft passed in is assumed to be a clone, i.e.
        /// the properties on the aircraft cannot change over the lifetime of the call.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="originLatitude">The latitude to measure distances from.</param>
        /// <param name="originLongitude">The longitude to measure distances from.</param>
        /// <returns></returns>
        public static ProximityGadgetClosestAircraftJson ToModel(IAircraft aircraft, double? originLatitude, double? originLongitude)
        {
            ProximityGadgetClosestAircraftJson result = null;

            if(aircraft != null) {
                double? distance = null;
                if(aircraft.Latitude != null && aircraft.Longitude != null) {
                    distance = GreatCircleMaths.Distance(originLatitude, originLongitude, aircraft.Latitude, aircraft.Longitude);
                }

                result = new ProximityGadgetClosestAircraftJson() {
                    Altitude = FormatNullable(aircraft.Altitude),
                    BearingFromHere = FormatNullable(GreatCircleMaths.Bearing(originLatitude, originLongitude, aircraft.Latitude, aircraft.Longitude, null, false, false), null, 1),
                    Callsign = aircraft.Callsign,
                    Destination = aircraft.Destination,
                    DistanceFromHere = FormatNullable(distance, null, 2),
                    Emergency = aircraft.Emergency.GetValueOrDefault(),
                    GroundSpeed = FormatNullable(aircraft.GroundSpeed),
                    HasPicture = !String.IsNullOrEmpty(aircraft.PictureFileName),
                    Icao24 = aircraft.Icao24 ?? "",
                    Icao24Invalid = aircraft.Icao24Invalid,
                    Latitude = FormatNullable(aircraft.Latitude),
                    Longitude = FormatNullable(aircraft.Longitude),
                    Manufacturer = aircraft.Manufacturer,
                    Model = aircraft.Model,
                    Operator = aircraft.Operator,
                    OperatorIcao = aircraft.OperatorIcao,
                    Origin = aircraft.Origin,
                    Registration = aircraft.Registration,
                    Squawk = aircraft.Squawk.GetValueOrDefault() == 0 ? null : String.Format("{0:0000}", aircraft.Squawk),
                    Track = FormatNullable(aircraft.Track, null, 1),
                    Type = aircraft.Type,
                    VerticalRate = FormatNullable(aircraft.VerticalRate),
                };
            }

            return result;
        }

        /// <summary>
        /// Formats the nullable value as a string using the invariant culture and plain formatting (no thousands separators etc).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <param name="nullValue"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        private static string FormatNullable<T>(T? value, string nullValue = null, int decimalPlaces = -1)
            where T: struct
        {
            string result = nullValue;

            if(value != null) {
                var formatString = decimalPlaces < 0 ? "{0}" : String.Format("{{0:F{0}}}", decimalPlaces);
                result = String.Format(CultureInfo.InvariantCulture, formatString, value);
            }

            return result;
        }
    }
}
