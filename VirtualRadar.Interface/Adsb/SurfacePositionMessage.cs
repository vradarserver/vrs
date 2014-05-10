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
    /// A class describing the content of a surface position message.
    /// </summary>
    public class SurfacePositionMessage
    {
        /// <summary>
        /// Gets or sets the ground speed of the vehicle in knots.
        /// </summary>
        public double? GroundSpeed { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the vehicle's ground speed is higher than ADS-B can convey.
        /// </summary>
        /// <remarks>
        /// When this is true the actual speed of the vehicle is higher than the value in <see cref="GroundSpeed"/>.</remarks>
        public bool GroundSpeedExceeded { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the vehicle is reversing.
        /// </summary>
        public bool IsReversing { get; set; }

        /// <summary>
        /// Gets or sets the ground track of the vehicle, where 0 is north, 90 east, 180 south and 270 west.
        /// </summary>
        public double? GroundTrack { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the time at which the horizontal position was recorded is an exact 0.2 second UTC epoch or not.
        /// </summary>
        public bool PositionTimeIsExact { get; set; }

        /// <summary>
        /// Gets or sets the compact position report coordinate from the message.
        /// </summary>
        public CompactPositionReportingCoordinate CompactPosition { get; set; }

        /// <summary>
        /// Returns an English description of the message.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var result = new StringBuilder("SFC");
            if(GroundSpeed != null) result.AppendFormat(" GSPD:{0}{1}", GroundSpeed, GroundSpeedExceeded ? "*" : "");
            result.AppendFormat(" REV:{0}", IsReversing);
            if(GroundTrack != null) result.AppendFormat(" GTRK:{0}", GroundTrack);
            result.AppendFormat(" T:{0}", PositionTimeIsExact ? "1" : "0");
            if(CompactPosition != null) result.AppendFormat(" CPR:{0}", CompactPosition);

            return result.ToString();
        }
    }
}
