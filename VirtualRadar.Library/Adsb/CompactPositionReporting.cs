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
using VirtualRadar.Interface.Adsb;

namespace VirtualRadar.Library.Adsb
{
    /// <summary>
    /// See interface docs.
    /// </summary>
    /// <remarks>
    /// Some of the nomenclature (e.g. 'NL', 'DLAT' etc.) used in the code is taken from the description of the
    /// CPR algorithms in the various versions of the ICAO documentation.
    /// </remarks>
    class CompactPositionReporting : ICompactPositionReporting
    {
        /// <summary>
        /// The number of latitude zones in each hemisphere.
        /// </summary>
        private const int NumberOfLatitudeZones = 15;

        /// <summary>
        /// The maximum value of a 12-bit result.
        /// </summary>
        private const int MaxResult12Bit = 0x1000;

        /// <summary>
        /// The maximum value of a 14-bit result.
        /// </summary>
        private const int MaxResult14Bit = 0x4000;

        /// <summary>
        /// The maximum value of a 17-bit result.
        /// </summary>
        private const int MaxResult17Bit = 0x20000;

        /// <summary>
        /// The maximum value of a 19-bit result.
        /// </summary>
        private const int MaxResult19Bit = 0x80000;

        /// <summary>
        /// The number of latitude zones in even encoding (and decoding, for 17-bits or lower).
        /// </summary>
        const double DLatEven = 360.0 / (4 * NumberOfLatitudeZones);

        /// <summary>
        /// The number of latitude zones in odd encoding (and decoding, for 17-bits or lower).
        /// </summary>
        const double DLatOdd = 360.0 / ((4 * NumberOfLatitudeZones) - 1);

        /// <summary>
        /// The number of latitude zones in even decoding for 19-bits.
        /// </summary>
        const double DLatEven19Bit = 90.0 / (4 * NumberOfLatitudeZones);

        /// <summary>
        /// The number of latitude zones in odd decoding for 19-bits.
        /// </summary>
        const double DLatOdd19Bit = 90.0 / ((4 * NumberOfLatitudeZones) - 1);

        /// <summary>
        /// The table of pre-calculated latitude zone transition latitudes, where index 0 corresponds with NL 59,
        /// index 1 corresponds with the transition latitude for NL 58 and so on.
        /// </summary>
        private static double[] _NLTable;

        /// <summary>
        /// The public static constructor for the class.
        /// </summary>
        static CompactPositionReporting()
        {
            BuildNLTable();
        }

        /// <summary>
        /// Fills the <see cref="_NLTable"/> table with precalculated transition latitudes for each latitude zone.
        /// </summary>
        /// <returns></returns>
        private static void BuildNLTable()
        {
            _NLTable = new double[58];

            var radiansToDegrees = 180.0 / Math.PI;
            var numerator = 1.0 - Math.Cos(Math.PI / (2.0 * NumberOfLatitudeZones));
            for(int nl = 59, i = 0;i < 58;--nl, ++i) {
                var denominator = 1.0 - Math.Cos((2.0 * Math.PI) / nl);
                var fraction = numerator / denominator;
                var sqrootOfFraction = Math.Sqrt(fraction);
                var latitude = radiansToDegrees * Math.Acos(sqrootOfFraction);
                _NLTable[i] = latitude;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="globalCoordinate"></param>
        /// <param name="oddFormat"></param>
        /// <param name="numberOfBits"></param>
        /// <returns></returns>
        public CompactPositionReportingCoordinate Encode(GlobalCoordinate globalCoordinate, bool oddFormat, byte numberOfBits)
        {
            var maxResult = MaxResultSize(numberOfBits);
            var dlat = oddFormat ? DLatOdd : DLatEven;
            var yz = Math.Floor(maxResult * (CircleModulus(globalCoordinate.Latitude, dlat) / dlat) + 0.5);

            var rlat = dlat * ((yz / maxResult) + Math.Floor(globalCoordinate.Latitude / dlat));
            var oddEvenNL = NL(rlat) - (oddFormat ? 1 : 0);
            var dlon = oddEvenNL == 0 ? 360.0 : 360.0 / oddEvenNL;
            var xz = Math.Floor(maxResult * (CircleModulus(globalCoordinate.Longitude, dlon) / dlon) + 0.5);

            var encodedSize = numberOfBits == 19 ? MaxResultSize(17) : maxResult;
            return new CompactPositionReportingCoordinate((int)CircleModulus(yz, encodedSize), (int)CircleModulus(xz, encodedSize), oddFormat, numberOfBits);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="cprCoordinate"></param>
        /// <param name="referenceCoordinate"></param>
        /// <returns></returns>
        /// <remarks><para>
        /// It was found in testing that encoded latitudes of zero are incredibly sensitive to rounding errors introduced by the use of doubles. This
        /// can be fixed by using decimals, which have much greater precision, and rounding to a very large number of decimal places - but using decimals
        /// is bad news, they are a lot slower than doubles because all of the operations are done in software, there is no hardware FPU support for them.
        /// 10 million divisions in LINQPad took 58 seconds on my development machine when using decimals and less than a second using doubles.
        /// </para><para>
        /// However it's only on the boundary between zones where rounding errors cause huge problems (e.g. returning a longitude of -87 instead of -90).
        /// The problem revolves around the selection of the wrong value of m, rounding errors in doubles at boundaries between zones can push m out by
        /// one, placing the longitude into the wrong zone and getting a longitude that it out by miles. Once the longitude is above zero rounding errors
        /// can have an effect but the correct m is still selected and they only produce inaccuracies of a few feet. So the compromise was to use decimals
        /// when accuracy is paramount - i.e. when the encoded longitude is 0 - and doubles for the rest of the time.
        /// </para></remarks>
        public GlobalCoordinate LocalDecode(CompactPositionReportingCoordinate cprCoordinate, GlobalCoordinate referenceCoordinate)
        {
            var result = new GlobalCoordinate();

            var maxResult = MaxResultSize(cprCoordinate.NumberOfBits == 19 ? 17 : cprCoordinate.NumberOfBits);
            var dlat = cprCoordinate.NumberOfBits == 19 ? cprCoordinate.OddFormat ? DLatOdd19Bit : DLatEven19Bit : cprCoordinate.OddFormat ? DLatOdd : DLatEven;
            var dlonNumerator = cprCoordinate.NumberOfBits == 19 ? 90.0 : 360.0;

            var refLat = referenceCoordinate.Latitude;
            var encodedLatitudeSlice = (double)cprCoordinate.Latitude / maxResult;
            var j = Math.Floor(refLat / dlat) + Math.Floor(0.5 + (CircleModulus(refLat, dlat) / dlat) - encodedLatitudeSlice);
            result.Latitude = dlat * (j + encodedLatitudeSlice);

            var oddEvenNL = NL(result.Latitude) - (cprCoordinate.OddFormat ? 1 : 0);
            if(cprCoordinate.Longitude != 0) {
                // The version that uses doubles for speed at the expense of potential rounding errors. Any changes here need duplicating below!
                var refLon = referenceCoordinate.Longitude;
                var dlon = oddEvenNL == 0 ? dlonNumerator : dlonNumerator / oddEvenNL;
                var encodedLongitudeSlice = (double)cprCoordinate.Longitude / maxResult;
                var m = Math.Floor(refLon / dlon) + Math.Floor(0.5 + (CircleModulus(refLon, dlon) / dlon) - encodedLongitudeSlice);
                result.Longitude = dlon * (m + encodedLongitudeSlice);
            } else {
                // Same as the block above but uses decimals for accuracy at the expense of speed. It would be nice if I could use C# generic functions
                // to remove this redundancy, but they're not very good for that. Sometimes I miss C++, I'd have 1000 ways to make the code common :)
                // Anyway, any changes here need duplicating above!
                var refLon = (decimal)referenceCoordinate.Longitude;
                var dlon = oddEvenNL == 0 ? (decimal)dlonNumerator : (decimal)dlonNumerator / oddEvenNL;
                var encodedLongitudeSlice = (decimal)cprCoordinate.Longitude / maxResult;
                var m = Math.Floor(Math.Round(refLon / dlon, 20)) + Math.Floor(0.5M + (CircleModulus(refLon, dlon) / dlon) - encodedLongitudeSlice);
                result.Longitude = (double)(dlon * (m + encodedLongitudeSlice));
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="earlyCpr"></param>
        /// <param name="laterCpr"></param>
        /// <param name="receiverLocation"></param>
        /// <returns></returns>
        public GlobalCoordinate GlobalDecode(CompactPositionReportingCoordinate earlyCpr, CompactPositionReportingCoordinate laterCpr, GlobalCoordinate receiverLocation)
        {
            GlobalCoordinate result = null;
            if(earlyCpr != null && laterCpr != null && earlyCpr.OddFormat != laterCpr.OddFormat && earlyCpr.NumberOfBits == laterCpr.NumberOfBits && (laterCpr.NumberOfBits < 19 || receiverLocation != null)) {
                var numberOfBits = earlyCpr.NumberOfBits;
                var surfaceEncoding = numberOfBits == 19;
                var dlatOdd = surfaceEncoding ? DLatOdd19Bit : DLatOdd;
                var dlatEven = surfaceEncoding ? DLatEven19Bit : DLatEven;
                var maxResult = MaxResultSize(surfaceEncoding ? 17 : numberOfBits);

                var oddCoordinate = earlyCpr.OddFormat ? earlyCpr : laterCpr;
                var evenCoordinate = oddCoordinate == earlyCpr ? laterCpr : earlyCpr;

                var j = Math.Floor(((59 * (double)evenCoordinate.Latitude - 60 * (double)oddCoordinate.Latitude) / maxResult) + 0.5);

                var rlatEven = dlatEven * (CircleModulus(j, 60) + ((double)evenCoordinate.Latitude / maxResult));
                var rlatOdd = dlatOdd * (CircleModulus(j, 59) + ((double)oddCoordinate.Latitude / maxResult));
                if(rlatEven >= 270.0) rlatEven -= 360.0;
                if(rlatOdd >= 270.0) rlatOdd -= 360.0;
                var rlat = laterCpr.OddFormat ? rlatOdd : rlatEven;
                if(surfaceEncoding && AbsoluteDifference(rlat, receiverLocation.Latitude) > AbsoluteDifference(rlat - 90.0, receiverLocation.Latitude)) {
                    rlat -= 90.0;
                    rlatEven -= 90.0;
                    rlatOdd -= 90.0;
                }

                var nlEven = NL(rlatEven);
                var nlOdd = NL(rlatOdd);
                if(nlEven == nlOdd) {
                    var nl = nlEven;

                    var mNumerator = (double)evenCoordinate.Longitude * (nl - 1) - (double)oddCoordinate.Longitude * nl;
                    var m = Math.Floor((mNumerator / maxResult) + 0.5);
                    var ni = Math.Max(nl - (laterCpr.OddFormat ? 1 : 0), 1);
                    var dlon = (surfaceEncoding ? 90.0 : 360.0) / ni;
                    var rlon = dlon * (CircleModulus(m, ni) + ((double)laterCpr.Longitude / maxResult));
                    if(!surfaceEncoding) rlon = CircleModulus(rlon + 180.0, 360.0) - 180.0;
                    else {
                        var deltaDegrees = double.MaxValue;
                        var receiverBearing = LongitudeToBearing(receiverLocation.Longitude);
                        foreach(var possibleRlon in new double[] { rlon, rlon - 90, rlon - 180, rlon - 270 }) {
                            var adjustedRlon = CircleModulus(possibleRlon + 180.0, 360.0) - 180.0;

                            var rlonBearing = LongitudeToBearing(adjustedRlon);
                            var delta = SmallestDegreesBetweenBearings(rlonBearing, receiverBearing);
                            if(delta < 0.0) delta += 360.0;
                            if(delta < deltaDegrees) {
                                rlon = adjustedRlon;
                                deltaDegrees = delta;
                            }
                        }
                    }

                    result = new GlobalCoordinate(rlat, rlon);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the largest value for a given number of bits.
        /// </summary>
        /// <param name="numberOfBits"></param>
        /// <returns></returns>
        int MaxResultSize(int numberOfBits)
        {
            switch(numberOfBits) {
                case 12:    return MaxResult12Bit;
                case 14:    return MaxResult14Bit;
                case 17:    return MaxResult17Bit;
                case 19:    return MaxResult19Bit;
                default:    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Returns the number of the latitude zone that a latitude falls within.
        /// </summary>
        /// <param name="latitude"></param>
        /// <returns></returns>
        /// <remarks>
        /// There is some ambiguity in the documentation found on the Internet as to whether this method should return 1 or 2 for
        /// latitudes that are exactly 87 or -87. The recommendation put forward in 1090-WP-9-4 says that 87.0 should be a special
        /// case and return a value of 1. They make a good case for it. However the recommendation does not seem to have been
        /// taken up - it was made in 2002 and the encoding notes within ASP TSGWP11-01-Draft Doc.9871-E2 TSG-Paris June 2011
        /// state that 87.0 should return 2. Further, having an NL(87.0) of 2 allows the encoder to produce values that are
        /// compliant with the CPR101 test tables, while NL(87.0) = 1 does not. So this method returns 2 for 87.0/-87.0.
        /// </remarks>
        int NL(double latitude)
        {
            int result = 1;

            if(latitude < 0) latitude = -latitude;
            if(latitude <= 87.0) {
                for(int i = 0;i < _NLTable.Length;++i) {
                    if(latitude <= _NLTable[i]) {
                        result = 59 - i;
                        break;
                    }
                }
            }
    
            return result;
        }

        /// <summary>
        /// Returns the modulus of the angle divided by the slice. If the result is -ve then it is made +ve.
        /// </summary>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        /// <returns></returns>
        double CircleModulus(double numerator, double denominator)
        {
            var result = numerator - denominator * Math.Floor(numerator / denominator);
            return result < 0.0 ? result + denominator : result;
        }

        /// <summary>
        /// This version is as per the double version but it is much slower and avoids rounding errors. It only works with angles
        /// because it was only needed to mod() angles.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="slice"></param>
        /// <returns></returns>
        decimal CircleModulus(decimal angle, decimal slice)
        {
            if(angle < 0M) angle += 360M;
            var fraction = Math.Round(angle / slice, 20);
            var remainder = fraction - (int)fraction;
            return remainder * slice;
        }

        /// <summary>
        /// Determines which value is larger and subtracts the other value from it, returning the absolute value.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is used as a broad method of determining which of the 2 possible latitude results is the closest to the
        /// receiver when doing a surface position global decode. It makes the incorrect assumption that the distance
        /// between degrees of latitude are equal, but considering that the two possible latitude solutions are separated
        /// by 90 degrees it will (should?) be true that the solution that has the smaller number of degrees difference between it
        /// and the receiver's latitude is the closest to the receiver. Unfortunately if they're both the same number of
        /// degrees away from the receiver (i.e. the receiver is on the equator) then the correct solution can't be found.
        /// However all of the alternatives that I could think of (use a bearing calculation, like I do with longitude, or
        /// compare distances - which would be computationally expensive) wouldn't work either.
        /// </remarks>
        double AbsoluteDifference(double value1, double value2)
        {
            return Math.Abs(value1 > value2 ? value1 - value2 : value2 - value1);
        }

        /// <summary>
        /// Converts a longitude (-180 to 180, where -180 and +180 are the same longitude) to a bearing on a circle.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <remarks>
        /// Converting longitudes to a bearing makes it a lot easier when we try to figure out which longitude is closest to a
        /// reference longitude. In this scale longitude 180 = bearing 180, longitude 90 = bearing 90, longitude 0 = bearing 0,
        /// longitude -45 = bearing 315, longitude -90 = bearing 270, longitude -180 = bearing 180.
        /// </remarks>
        double LongitudeToBearing(double p)
        {
            return p < 0.0 ? 360.0 + p : p;
        }

        /// <summary>
        /// Returns the smallest difference between two bearings.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        /// <remarks>
        /// There are two ways you can travel around a circle to move from one bearing to another - clockwise and anticlockwise.
        /// The shortest direction will yield a difference in bearings that is &lt;= 180 degrees. This returns the number of degrees of
        /// movement for the shortest direction.
        /// </remarks>
        double SmallestDegreesBetweenBearings(double p1, double p2)
        {
            var result = ((p1 - p2) + 360.0) % 360.0;
            if(result > 180.0) result = ((p2 - p1) + 360.0) % 360.0;
            return result;
        }
    }
}
