// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Text;

namespace VirtualRadar.Interface.Adsb
{
    /// <summary>
    /// Describes the content of a TIS-B coarse airborne position message.
    /// </summary>
    public class CoarseTisbAirbornePosition
    {
        /// <summary>
        /// Gets the state of the Mode-A transponder.
        /// </summary>
        public SurveillanceStatus SurveillanceStatus { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the TIS-B station that generated the message.
        /// </summary>
        public byte ServiceVolumeID { get; set; }

        /// <summary>
        /// Gets or sets the altitude in feet relative to a standard pressure of 1013.25mb / 29.92Hg.
        /// </summary>
        public int? BarometricAltitude { get; set; }

        /// <summary>
        /// Gets or sets the ground track, where 0 is north, 90 east, 180 south and 270 west.
        /// </summary>
        public double? GroundTrack { get; set; }

        /// <summary>
        /// Gets or sets the ground speed in knots.
        /// </summary>
        public double? GroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets the location of the aircraft encoded in a CPR.
        /// </summary>
        public CompactPositionReportingCoordinate CompactPosition { get; set; }

        /// <summary>
        /// Returns an English description of the message content.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("TIS-B(C)");

            result.AppendFormat(" SS:{0}", SurveillanceStatus);
            result.AppendFormat(" SVID:{0}", ServiceVolumeID);
            if(BarometricAltitude != null) {
                result.AppendFormat(" ALT:{0}", BarometricAltitude);
            }
            if(GroundTrack != null) {
                result.AppendFormat(" TRK:{0}", GroundTrack);
            }
            if(GroundSpeed != null) {
                result.AppendFormat(" SPD:{0}", GroundSpeed);
            }
            if(CompactPosition != null) {
                result.AppendFormat(" SPR:{0}", CompactPosition);
            }

            return result.ToString();
        }
    }
}
