// Copyright © 2010 onwards, Andrew Whewell
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
    /// A collection of Great Circle math functions, mostly cribbed from
    /// http://www.movable-type.co.uk/scripts/latlong.html.
    /// </summary>
    public static class GreatCircleMaths
    {
        private const double RadiusOfEarthKm = 6371.0;

        private static double ConvertDegreesToRadians(double value) { return Math.PI * value / 180.0; }
        private static double ConvertRadiansToDegrees(double value) { return value * (180.0 / Math.PI); }

        /// <summary>
        /// Calculates the distance in km between two points on the earth at sea level.
        /// </summary>
        /// <param name="startLatitude"></param>
        /// <param name="startLongitude"></param>
        /// <param name="endLatitude"></param>
        /// <param name="endLongitude"></param>
        /// <returns></returns>
        public static double? Distance(double? startLatitude, double? startLongitude, double? endLatitude, double? endLongitude)
        {
            double? result = null;

            if(startLatitude != null && endLatitude != null && startLongitude != null && endLongitude != null) {
                double lat1 = ConvertDegreesToRadians(startLatitude.Value);
                double lng1 = ConvertDegreesToRadians(startLongitude.Value);
                double lat2 = ConvertDegreesToRadians(endLatitude.Value);
                double lng2 = ConvertDegreesToRadians(endLongitude.Value);

                result = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(lng2 - lng1)) * RadiusOfEarthKm;
            }

            return result;
        }

        /// <summary>
        /// Given two points on the earth's surface this calculates either the bearing to take from point A to get to point B. It can optionally take a
        /// pre-calculated bearing and return that if the start and end points are the same.
        /// </summary>
        /// <param name="startLatitude"></param>
        /// <param name="startLongitude"></param>
        /// <param name="endLatitude"></param>
        /// <param name="endLongitude"></param>
        /// <param name="currentTrack">The brearing to return if <em>ignoreCurrentTrack</em> is false and the start and end points are the same.</param>
        /// <param name="reverseBearing">True if the bearing should be reversed.</param>
        /// <param name="ignoreCurrentTrack">True if the bearing should always be calculated and possible shortcuts ignored.</param>
        /// <returns></returns>
        public static double? Bearing(double? startLatitude, double? startLongitude, double? endLatitude, double? endLongitude, double? currentTrack, bool reverseBearing, bool ignoreCurrentTrack)
        {
            double? result = null;

            if(startLatitude != null && startLongitude != null && endLatitude != null && endLongitude != null) {
                if(startLatitude == endLatitude && startLongitude == endLongitude) {
                    if(!ignoreCurrentTrack) result = currentTrack;
                } else {
                    double lat1 = ConvertDegreesToRadians(startLatitude.Value);
                    double lng1 = ConvertDegreesToRadians(startLongitude.Value);
                    double lat2 = ConvertDegreesToRadians(endLatitude.Value);
                    double lng2 = ConvertDegreesToRadians(endLongitude.Value);
                    double deltaLng = lng2 - lng1;

                    double y = Math.Sin(deltaLng) * Math.Cos(lat2);
                    double x = (Math.Cos(lat1) * Math.Sin(lat2)) - (Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLng));
                    result = ConvertRadiansToDegrees(Math.Atan2(y, x));
                    if(reverseBearing) result += 180.0;
                    result = (result + 360.0) % 360.0;
                }
            }

            return result;
        }

        /// <summary>
        /// Given a start coordinate, bearing and distance this calculates the final coordinate.
        /// </summary>
        /// <param name="startLatitude"></param>
        /// <param name="startLongitude"></param>
        /// <param name="bearing"></param>
        /// <param name="kilometres"></param>
        /// <param name="endLatitude"></param>
        /// <param name="endLongitude"></param>
        public static void Destination(double? startLatitude, double? startLongitude, double? bearing, double? kilometres, out double? endLatitude, out double? endLongitude)
        {
            if(startLatitude == null || startLongitude == null || bearing == null || kilometres == null) {
                endLatitude = endLongitude = null;
            } else {
                double lat = ConvertDegreesToRadians(startLatitude.Value);
                double lng = ConvertDegreesToRadians(startLongitude.Value);
                double brng = ConvertDegreesToRadians(bearing.Value);
                double angularDistance = kilometres.Value / RadiusOfEarthKm;
                double sinLat = Math.Sin(lat);
                double cosLat = Math.Cos(lat);
                double cosAngularDistance = Math.Cos(angularDistance);
                double sinAngularDistance = Math.Sin(angularDistance);

                endLatitude = Math.Asin(sinLat * cosAngularDistance + cosLat * sinAngularDistance * Math.Cos(brng));
                endLongitude = lng + Math.Atan2(Math.Sin(brng) * sinAngularDistance * cosLat, cosAngularDistance - sinLat * Math.Sin(endLatitude.Value));

                endLatitude = ConvertRadiansToDegrees(endLatitude.Value);
                endLongitude = ConvertRadiansToDegrees(endLongitude.Value);
                if(endLongitude < -180.0) endLongitude += 360.0;
                else if(endLongitude > 180.0) endLongitude -= 360.0;
            }
        }
    }
}
