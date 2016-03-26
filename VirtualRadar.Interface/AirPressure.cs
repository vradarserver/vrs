// Copyright © 2016 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// An air pressure reading, as downloaded from the SDM site.
    /// </summary>
    public class AirPressure
    {
        /// <summary>
        /// The standard air pressure value in inches of mercury.
        /// </summary>
        public static readonly float StandardPressureInHg = 29.92F;

        /// <summary>
        /// Gets or sets the air pressure in inches of mercury.
        /// </summary>
        [JsonProperty(PropertyName = "InHg")]
        public float PressureInHg { get; set; }

        /// <summary>
        /// Gets or sets the latitude where the observation was made.
        /// </summary>
        [JsonProperty(PropertyName = "Lat")]
        public float Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude where the observation was made.
        /// </summary>
        [JsonProperty(PropertyName = "Lng")]
        public float Longitude { get; set; }

        /// <summary>
        /// Gets or sets the age of the observation in seconds (from when the
        /// fetch occurred).
        /// </summary>
        [JsonProperty(PropertyName = "Age")]
        public short AgeSeconds { get; set; }

        /// <summary>
        /// Returns the time that the air pressure observation was made.
        /// </summary>
        /// <param name="fetchTimeUtc"></param>
        /// <returns></returns>
        public DateTime ObservationTimeUtc(DateTime fetchTimeUtc)
        {
            return fetchTimeUtc.AddSeconds(AgeSeconds);
        }

        /// <summary>
        /// Takes a natural altitude and returns the pressure altitude for a given air pressure reading.
        /// </summary>
        /// <param name="correctedAltitude"></param>
        /// <returns></returns>
        public int? CorrectedAltitudeToPressureAltitude(int? correctedAltitude)
        {
            return CorrectedAltitudeToPressureAltitude(correctedAltitude, PressureInHg);
        }

        /// <summary>
        /// Takes a natural altitude and air pressure reading and returns the pressure altitude for a given air pressure reading.
        /// </summary>
        /// <param name="correctedAltitude"></param>
        /// <param name="airPressureInHg"></param>
        /// <returns></returns>
        public static int? CorrectedAltitudeToPressureAltitude(int? correctedAltitude, float airPressureInHg)
        {
            var result = correctedAltitude;

            if(result != null && airPressureInHg > 0 && airPressureInHg != StandardPressureInHg) {
                result = (int)(0.5F + (correctedAltitude + 1000 * (StandardPressureInHg - airPressureInHg)));
            }

            return result;
        }

        /// <summary>
        /// Takes a pressure altitude and returns the corrected altitude.
        /// </summary>
        /// <param name="pressureAltitude"></param>
        /// <returns></returns>
        public int? PressureAltitudeToCorrectedAltitude(int? pressureAltitude)
        {
            return PressureAltitudeToCorrectedAltitude(pressureAltitude, PressureInHg);
        }

        /// <summary>
        /// Takes a pressure altitude and returns an altitude that has been corrected for the
        /// pressure reading.
        /// </summary>
        /// <param name="pressureAltitude"></param>
        /// <param name="airPressureInHg"></param>
        /// <returns></returns>
        public static int? PressureAltitudeToCorrectedAltitude(int? pressureAltitude, float airPressureInHg)
        {
            var result = pressureAltitude;

            if(result != null && airPressureInHg > 0 && airPressureInHg != StandardPressureInHg) {
                result = (int)(0.5F + (pressureAltitude - 1000 * (StandardPressureInHg - airPressureInHg)));
            }

            return result;
        }
    }
}
