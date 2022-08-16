// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Tests that a coordinate falls within a geofence described by a top-left and
    /// bottom-right pair of coordinates on the surface of the Earth.
    /// </summary>
    /// <remarks>
    /// This was split out of WebSite.AircraftListJsonBuilder.
    /// </remarks>
    public static class GeofenceChecker
    {
        /// <summary>
        /// Returns true if an aircraft at the latitude and longitude passed across is within the 'rectangle' on the surface
        /// of the earth described by the pair of coordinates passed across, where the first coordinate is top-left and
        /// the second is bottom-right.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static bool IsWithinBounds(double? latitude, double? longitude, Pair<Coordinate> bounds)
        {
            return IsWithinBounds(
                latitude, longitude,
                bounds.First.Latitude, bounds.First.Longitude,
                bounds.Second.Latitude, bounds.Second.Longitude
            );
        }

        /// <summary>
        /// Returns true if an aircraft at the latitude and longitude passed across is within the 'rectangle' on the surface
        /// of the earth described by the pair of coordinates passed across.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        /// <param name="bottom"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static bool IsWithinBounds(double? latitude, double? longitude, double top, double left, double bottom, double right)
        {
            var result = latitude != null && longitude != null;

            if(result) {
                // Latitude is simple because we assume there is nothing past the poles... from north to south the earth is flat :)
                result = top >= latitude && bottom <= latitude;

                if(result) {
                    // Longitude is harder because if the bounding box straddles the anti-meridian then the normal comparison of coordinates
                    // fails. When it straddles the anti-meridian the left edge is a +ve value < 180 and the right edge is a -ve value > -180.
                    // You can also end up with a left edge that is larger than the right edge (e.g. left is 170, Alaska-ish, while right is
                    // 60, Russia-ish). On top of that -180 and 180 are the same value. The easiest way to cope is to normalise all longitudes
                    // to a linear scale of angles from 0 through 360 and then check that the longitude lies between the left and right.
                    //
                    // If the left degree is larger than the right degree then the bounds straddle the meridian, in which case we need to allow
                    // all longitudes from the left to 0/360 and all longitudes from 0/360 to the right. If left < right then it's easier, we
                    // just have to have a longitude between left and right.
                    //
                    // One final twist - if you zoom out enough so that you can see the entire span of the globe in one go then Google will give
                    // up and report a boundary of -180 on the left and 180 on the right... in other words, the same longitude. Still, there's
                    // not much else they can do.
                    longitude = ConvertLongitudeToLinear(longitude.Value);

                    left = ConvertLongitudeToLinear(left);
                    right = ConvertLongitudeToLinear(right);
                    if(left != 180.0 || right != 180.0) {
                        if(left == right)     result = longitude == left;
                        else if(left > right) result = (longitude >= left && longitude <= 360.0) || (longitude >= 0.0 && longitude <= right);
                        else                  result = longitude >= left && longitude <= right;
                    }
                }
            }

            return result;
        }

        private static double ConvertLongitudeToLinear(double longitude) => longitude >= 0.0 ? longitude : longitude + 360.0;
    }
}
