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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A helper class that can round numbers quickly, but only if they are within certain ranges.
    /// </summary>
    public static class Round
    {
        /// <summary>
        /// Rounds ground speeds to 1 decimal place quickly.
        /// </summary>
        /// <param name="groundSpeed"></param>
        /// <returns></returns>
        public static float? GroundSpeed(float? groundSpeed)
        {
            float? result = groundSpeed;
            if(result != null) {
                var rounder = result >= 0 ? 0.5F : -0.5F;
                var rounded = (int)((result * 10) + rounder);
                result = (float)rounded / 10F;
            }

            return result;
        }

        /// <summary>
        /// Rounds tracks to 1 or 0 decimal places quickly.
        /// </summary>
        /// <param name="track"></param>
        /// <param name="roundToZeroPlaces"></param>
        /// <returns></returns>
        public static float? Track(float? track, bool roundToZeroPlaces = false)
        {
            float? result = track;
            if(result != null) {
                var rounded = (int)((result * 10) + 0.5F);
                if(roundToZeroPlaces) result = (int)(rounded / 10F);
                else result = (float)rounded / 10F;
                if(result >= 360F) result -= 360F;
            }

            return result;
        }

        /// <summary>
        /// Rounds coordinates to 6 decimal places quickly.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        /// <remarks>
        /// 6 decimals is just enough to resolve movements of 1.3 metres, which is the resolution of surface positions.
        /// </remarks>
        public static double? Coordinate(double? coordinate)
        {
            double? result = coordinate;
            if(result != null) {
                var rounder = result >= 0 ? 0.5 : -0.5;
                var rounded = (int)((result * 1000000.0) + rounder);
                result = (double)rounded / 1000000.0;
            }

            return result;
        }

        /// <summary>
        /// Rounds headings for use on recorded tracks. Rounds to the nearest x degrees,
        /// don't use on normal track values!
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public static float? TrackHeading(float? track)
        {
            float? result = track;
            if(result != null) {
                result /= 5F;
                result += result > 0 ? 0.5F : -0.5F;
                result = (int)result * 5;
                if(result >= 360F) result -= 360F;
            }

            return result;
        }

        /// <summary>
        /// Rounds altitudes for use in tracks.
        /// </summary>
        /// <param name="altitude"></param>
        /// <returns></returns>
        public static int? TrackAltitude(int? altitude)
        {
            var result = altitude;
            if(result != null) {
                var alt = (double)altitude.Value / 500.0;
                alt += alt > 0 ? 0.5 : -0.5;
                result = (int)alt * 500;
            }

            return result;
        }

        /// <summary>
        /// Rounds ground speeds for use in tracks.
        /// </summary>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static float? TrackGroundSpeed(float? speed)
        {
            var result = speed;
            if(result != null) {
                var spd = speed.Value / 10f;
                spd += spd > 0 ? 0.5f : -0.5f;
                result = (int)spd * 10;
            }

            return result;
        }
    }
}
