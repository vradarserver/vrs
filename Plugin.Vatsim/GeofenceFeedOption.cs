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
    class GeofenceFeedOption
    {
        public string FeedName { get; set; }

        public GeofenceCentreOn CentreOn { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public DistanceUnit DistanceUnit { get; set; }

        public int? PilotCid { get; set; }

        public string AirportCode { get; set; }

        public override string ToString()
        {
            switch(CentreOn) {
                case GeofenceCentreOn.Airport:      return $"{Width} x {Height} {DistanceUnit} centred on airport {AirportCode}";
                case GeofenceCentreOn.Coordinate:   return $"{Width} x {Height} {DistanceUnit} centred on {Latitude} x {Longitude}";
                case GeofenceCentreOn.PilotCid:     return $"{Width} x {Height} {DistanceUnit} centred on pilot {PilotCid}";
                default:                            throw new NotImplementedException();
            }
        }

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
