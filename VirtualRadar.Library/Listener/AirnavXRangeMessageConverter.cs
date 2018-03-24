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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// Default implementation of <see cref="IAirnavXRangeMessageConverter"/>.
    /// </summary>
    class AirnavXRangeMessageConverter : IAirnavXRangeMessageConverter
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public IEnumerable<BaseStationMessageEventArgs> ConvertIntoBaseStationMessageEventArgs(AirnavXRangeJson json)
        {
            var result = new List<BaseStationMessageEventArgs>();

            foreach(var jsonAircraft in json.Aircraft) {
                var message = new BaseStationMessage() {
                    MessageType = BaseStationMessageType.Transmission,
                    TransmissionType = BaseStationTransmissionType.AirToAir,        // This needs to be something other than "None" or "AllCallReply" if we want to have these messages work with the message compressor
                    StatusCode = BaseStationStatusCode.None,
                };

                if(jsonAircraft.SignalLevel != null)    message.SignalLevel = (int)(jsonAircraft.SignalLevel + -0.5);
                if(jsonAircraft.Icao24 != null)         message.Icao24 = jsonAircraft.Icao24;
                if(jsonAircraft.Altitude != null)       message.Altitude = jsonAircraft.Altitude;
                if(jsonAircraft.Callsign != null)       message.Callsign = jsonAircraft.Callsign;
                if(jsonAircraft.GroundSpeed != null)    message.GroundSpeed = jsonAircraft.GroundSpeed;
                if(jsonAircraft.Track != null)          message.Track = jsonAircraft.Track;
                if(jsonAircraft.Squawk != null)         message.Squawk = jsonAircraft.Squawk;
                if(jsonAircraft.VerticalRate != null)   message.VerticalRate = jsonAircraft.VerticalRate;
                if(jsonAircraft.OnGround != null)       message.OnGround = jsonAircraft.OnGround;

                if(jsonAircraft.Latitude != null || jsonAircraft.Longitude != null) {
                    if(jsonAircraft.Latitude != null)       message.Latitude = jsonAircraft.Latitude;
                    if(jsonAircraft.Longitude != null)      message.Longitude = jsonAircraft.Longitude;
                }

                result.Add(new BaseStationMessageEventArgs(message));
            }

            return result;
        }
    }
}
