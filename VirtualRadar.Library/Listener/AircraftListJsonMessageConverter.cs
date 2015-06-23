// Copyright © 2015 onwards, Andrew Whewell
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
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="IAircraftListJsonMessageConverter"/>.
    /// </summary>
    class AircraftListJsonMessageConverter : IAircraftListJsonMessageConverter
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftListJson"></param>
        /// <returns></returns>
        public IEnumerable<BaseStationMessage> ConvertIntoBaseStationMessages(AircraftListJson aircraftListJson)
        {
            var result = new List<BaseStationMessage>();

            foreach(var json in aircraftListJson.Aircraft) {
                var message = new BaseStationMessage() {
                    MessageType = BaseStationMessageType.Transmission,
                    StatusCode = BaseStationStatusCode.None,
                };

                if(json.Altitude != null)       message.Altitude = json.Altitude;
                if(json.Callsign != null)       message.Callsign = json.Callsign;
                if(json.Emergency != null)      message.Emergency = json.Emergency;
                if(json.GroundSpeed != null)    message.GroundSpeed = json.GroundSpeed;
                if(json.Icao24 != null)         message.Icao24 = json.Icao24;
                if(json.Latitude != null)       message.Latitude = json.Latitude;
                if(json.Longitude != null)      message.Longitude = json.Longitude;
                if(json.OnGround != null)       message.OnGround = json.OnGround;
                if(json.SignalLevel != null)    message.SignalLevel = json.SignalLevel;
                if(json.Track != null)          message.Track = json.Track;
                if(json.VerticalRate != null)   message.VerticalRate = json.VerticalRate;

                if(json.Squawk != null) {
                    int squawk;
                    if(int.TryParse(json.Squawk, out squawk)) message.Squawk = squawk;
                }

                if(json.AltitudeType != null)       AddSupplementaryValue(message, r => r.AltitudeIsGeometric = json.AltitudeType == 1);
                if(json.CallsignIsSuspect != null)  AddSupplementaryValue(message, r => r.CallsignIsSuspect = json.CallsignIsSuspect);
                if(json.SpeedType != null)          AddSupplementaryValue(message, r => r.SpeedType = (SpeedType)json.SpeedType);
                if(json.TargetAltitude != null)     AddSupplementaryValue(message, r => r.TargetAltitude = json.TargetAltitude);
                if(json.TargetTrack != null)        AddSupplementaryValue(message, r => r.TargetHeading = json.TargetTrack);
                if(json.TrackIsHeading != null)     AddSupplementaryValue(message, r => r.TrackIsHeading = json.TrackIsHeading);
                if(json.TransponderType != null)    AddSupplementaryValue(message, r => r.TransponderType = (TransponderType)json.TransponderType);
                if(json.VerticalRateType != null)   AddSupplementaryValue(message, r => r.VerticalRateIsGeometric = json.VerticalRateType == 1);

                result.Add(message);
            }

            return result;
        }

        private void AddSupplementaryValue(BaseStationMessage message, Action<BaseStationSupplementaryMessage> setValue)
        {
            if(message.Supplementary == null) message.Supplementary = new BaseStationSupplementaryMessage();
            setValue(message.Supplementary);
        }
    }
}
