// Copyright © 2012 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// Describes the content of an ADS-B airborne position message.
    /// </summary>
    public class AirbornePositionMessage
    {
        /// <summary>
        /// Gets or sets an indication of the meaning of the Mode-A transponder attached to the aircraft.
        /// </summary>
        public SurveillanceStatus SurveillanceStatus { get; set; }

        /// <summary>
        /// Gets or sets the NIC-B Supplement bit from the message. This is either 0x00 or 0x02.
        /// </summary>
        /// <remarks><para>
        /// Note that the NIC supplement is three bits that are spread over different messages. If this is non-null then the message
        /// carries ONE of those bits. Each bit is set to the correct position (i.e. a NIC-A bit would show a value here of 4,
        /// NIC-B 2 and NIC-C 1) so something that was building the overall NIC value could OR together the value here with previous
        /// NIC bits to eventually arrive at the full NIC supplement.
        /// </para><para>
        /// Note also that these bits were previously assigned different meanings in their respective messages in older versions of
        /// the ADS-B transmission. Those older meanings are not represented in this class.
        /// </para>
        /// </remarks>
        public byte NicB { get; set; }

        /// <summary>
        /// Gets or sets the altitude in feet relative to a standard pressure of 1013.25mb / 29.92Hg.
        /// </summary>
        public int? BarometricAltitude { get; set; }

        /// <summary>
        /// Gets or sets the altitude in feet as the height above the elipsoid.
        /// </summary>
        public int? GeometricAltitude { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the time at which the horizontal position was recorded is an exact 0.2 second UTC epoch or not.
        /// </summary>
        public bool PositionTimeIsExact { get; set; }

        /// <summary>
        /// Gets or sets the compact position report coordinate from the message.
        /// </summary>
        public CompactPositionReportingCoordinate CompactPosition { get; set; }

        /// <summary>
        /// Returns an English description of the message content.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("AIR");
            result.AppendFormat(" SS:{0}", (int)SurveillanceStatus);
            result.AppendFormat(" NIC-B:{0}", NicB);
            if(BarometricAltitude != null) result.AppendFormat(" BA:{0}", BarometricAltitude);
            if(GeometricAltitude != null) result.AppendFormat(" GA:{0}", GeometricAltitude);
            result.AppendFormat(" T:{0}", PositionTimeIsExact ? "1" : "0");
            if(CompactPosition != null) result.AppendFormat(" CPR:{0}", CompactPosition);

            return result.ToString();
        }
    }
}
