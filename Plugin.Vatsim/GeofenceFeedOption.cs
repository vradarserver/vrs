// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Plugin.Vatsim
{
    /// <summary>
    /// Describes a feed that is composed of aircraft within a geofence.
    /// </summary>
    public class GeofenceFeedOption
    {
        /// <summary>
        /// Gets or sets the unique ID of the feed options.
        /// </summary>
        public Guid ID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the feed's name. This will be prefixed with VATSIM:.
        /// </summary>
        public string FeedName { get; set; }

        /// <summary>
        /// Gets or sets the element that the feed is centred on.
        /// </summary>
        public GeofenceCentreOn CentreOn { get; set; }

        /// <summary>
        /// Gets or sets the latitude that coordinate feeds are centred on.
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude that coordinate feeds are centred on.
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the CID of the pilot that pilot feeds are centred on.
        /// </summary>
        public int? PilotCid { get; set; }

        /// <summary>
        /// Gets or sets the IATA or ICAO code of the airport that airport feeds are centred on.
        /// </summary>
        public string AirportCode { get; set; }

        /// <summary>
        /// Gets or sets with width in <see cref="DistanceUnit"/> of the geofence.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// Gets or sets the height in <see cref="DistanceUnit"/> of the geofence.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// Gets the unit of distance that <see cref="Width"/> and <see cref="Height"/> are
        /// measured in.
        /// </summary>
        public DistanceUnit DistanceUnit { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch(CentreOn) {
                case GeofenceCentreOn.Airport:      return $"{Width} x {Height} {DistanceUnit} centred on airport {AirportCode}";
                case GeofenceCentreOn.Coordinate:   return $"{Width} x {Height} {DistanceUnit} centred on {Latitude} x {Longitude}";
                case GeofenceCentreOn.PilotCid:     return $"{Width} x {Height} {DistanceUnit} centred on pilot {PilotCid}";
                default:                            throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns a deep (ish - the object has no depth) copy of the object.
        /// </summary>
        /// <returns></returns>
        public GeofenceFeedOption Clone() => (GeofenceFeedOption)MemberwiseClone();

        /// <summary>
        /// Returns a geofence built from the options. This can be empty if there is not enough information
        /// but it will never be null.
        /// </summary>
        /// <param name="pilotLatitude"></param>
        /// <param name="pilotLongitude"></param>
        /// <returns></returns>
        public GeofenceCWH CreateGeofence(double? pilotLatitude = null, double? pilotLongitude = null)
        {
            GeofenceCWH result;

            switch(CentreOn) {
                case GeofenceCentreOn.Airport:
                    var standingData = Factory.ResolveSingleton<IStandingDataManager>();
                    var airport = standingData.FindAirportForCode(AirportCode);
                    result = CreateOptionalOrEmptyGeofence(airport?.Latitude, airport?.Longitude);
                    break;
                case GeofenceCentreOn.Coordinate:
                    result = CreateOptionalOrEmptyGeofence(Latitude, Longitude);
                    break;
                case GeofenceCentreOn.PilotCid:
                    result = CreateOptionalOrEmptyGeofence(pilotLatitude, pilotLongitude);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        private GeofenceCWH CreateOptionalOrEmptyGeofence(double? latitude, double? longitude)
        {
            return latitude == null || longitude == null
                ? GeofenceCWH.Empty
                : new GeofenceCWH(
                      centreLatitude:   latitude.Value,
                      centreLongitude:  longitude.Value,
                      width:            Width,
                      height:           Height,
                      distanceUnit:     DistanceUnit
                  );
        }
    }
}
