// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class GreatCircleMaths_Tests
    {
        [TestMethod]
        [DataRow(null,    null,    null,    null,    null)]
        [DataRow(null,    6.0,     50.0,    6.0,     null)]
        [DataRow(51.0,    null,    50.0,    6.0,     null)]
        [DataRow(51.0,    6.0,     null,    6.0,     null)]
        [DataRow(51.0,    6.0,     50.0,    null,    null)]
        [DataRow(51.0,    6.0,     50.0,    6.0,     111.1949)]
        [DataRow(51.0,    6.0,     52.0,    6.0,     111.1949)]
        [DataRow(50.0359, -5.4253, 58.3838, -3.0412, 940.9476)]
        [DataRow(1.0,     0.0,     -1.0,    0.0,     222.3898)]
        [DataRow(-0.3,    110.0,   -1.0,    130.0,   2225.1019)]

        public void Distance_Calculates_Correct_Distances(
            double? startLatitude,
            double? startLongitude,
            double? endLatitude,
            double? endLongitude,
            double? expected
        )
        {
            var actual = GreatCircleMaths.Distance(startLatitude, startLongitude, endLatitude, endLongitude);

            if(expected == null) {
                Assert.IsNull(actual);
            } else {
                Assert.AreEqual((double)expected, (double)actual, 0.0001);
            }
        }

        [TestMethod]
        [DataRow(null,    null,    null,    null,    100.0, false, true,  null)]
        [DataRow(null,    6.0,     52.0,    6.0,     100.0, false, true,  null)]
        [DataRow(51.0,    null,    52.0,    6.0,     100.0, false, true,  null)]
        [DataRow(51.0,    6.0,     null,    6.0,     100.0, false, true,  null)]
        [DataRow(51.0,    6.0,     52.0,    null,    100.0, false, true,  null)]
        [DataRow(51.0,    6.0,     51.0,    6.0,     100.0, false, true,  null)]
        [DataRow(51.0,    6.0,     52.0,    6.0,     100.0, false, true,  0.0)]
        [DataRow(51.0,    6.0,     50.0,    6.0,     100.0, false, true,  180.0)]
        [DataRow(51.0,    6.0,     51.0,    7.0,     100.0, false, true,  89.6114)]
        [DataRow(51.0,    6.0,     51.0,    5.0,     100.0, false, true,  270.3885)]
        [DataRow(-51.0,   6.0,     -52.0,   6.0,     100.0, false, true,  180.0)]
        [DataRow(-51.0,   -6.0,    -52.0,   -6.0,    100.0, false, true,  180.0)]
        [DataRow(-51.0,   -6.0,    -51.0,   -5.0,    100.0, false, true,  90.3885)]
        [DataRow(50.0359, -5.4253, 58.3838, -3.0412, 100.0, false, true,  8.5220)]
        [DataRow(1.0,     0.0,     -1.0,    0.0,     100.0, false, true,  180.0)]
        [DataRow(-0.3,    110.0,   -1.0,    130.0,   100.0, false, true,  92.0988)]
        [DataRow(51.0,    6.0,     51.0,    6.0,     100.0, false, false, 100.0)]
        [DataRow(51.0,    6.0,     52.0,    6.0,     100.0, true,  true,  180.0)]
        [DataRow(51.0,    6.0,     50.0,    6.0,     100.0, true,  true,  0.0)]
        [DataRow(51.0,    6.0,     51.0,    7.0,     100.0, true,  true,  269.6114)]
        public void Bearing_Calculates_Correct_Bearing(
            double? startLatitude,
            double? startLongitude,
            double? endLatitude,
            double? endLongitude,
            double? currentTrack,
            bool reverseBearing,
            bool ignoreCurrentTrack,
            double? expected
        )
        {
            var actual = GreatCircleMaths.Bearing(startLatitude, startLongitude, endLatitude, endLongitude, currentTrack, reverseBearing, ignoreCurrentTrack);

            if(expected == null) {
                Assert.IsNull(actual);
            } else {
                Assert.AreEqual((double)expected, (double)actual, 0.0001);
            }
        }

        [TestMethod]
        [DataRow(53.1927, -0.6129, 90.0,  30.0,   53.1919,  -0.1626)]
        [DataRow(10.0,    20.0,    210.0, 5000.0, -28.6757, -3.7496)]
        [DataRow(null,    -0.6129, 90.0,  30.0,   null,     null)]
        [DataRow(53.1927, null,    90.0,  30.0,   null,     null)]
        [DataRow(53.1927, -0.6129, null,  30.0,   null,     null)]
        [DataRow(53.1927, -0.6129, 90.0,  null,   null,     null)]
        [DataRow(-90.0,   -180.0,  0.0,   1.0,    -89.9910, -180.0)]
        [DataRow(-89.75,  180.0,   0.0,   1.0,    -89.7410, 180.0)]
        [DataRow(-90.0,   -180.0,  315.0, 1.0,    -89.9910, 90.0)]
        [DataRow(-90.0,   180.0,   315.0, 1.0,    -89.9910, 90.0)]
        [DataRow(-90.0,   -180.0,  135.0, 1.0,    -89.9910, -90.0)]
        [DataRow(-90.0,   180.0,   135.0, 1.0,    -89.9910, -90.0)]
        public void Destination_Calculates_Correct_Destination(
            double? startLatitude,
            double? startLongitude,
            double? bearing,
            double? distance,
            double? expectedLatitude,
            double? expectedLongitude
        )
        {
            double? endLatitude, endLongitude;
            GreatCircleMaths.Destination(startLatitude, startLongitude, bearing, distance, out endLatitude, out endLongitude);

            if(expectedLatitude == null || expectedLongitude == null) {
                Assert.AreEqual(expectedLatitude, endLatitude);
                Assert.AreEqual(expectedLongitude, endLongitude);
            } else {
                Assert.AreEqual(expectedLatitude.Value, endLatitude.Value, 0.0001);
                Assert.AreEqual(expectedLongitude.Value, endLongitude.Value, 0.0001);
            }
        }
    }
}
