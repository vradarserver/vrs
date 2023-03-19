// Copyright © 2012 onwards, Andrew Whewell
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
    public class Round_Tests
    {
        [TestMethod]
        public void Round_GroundSpeed_Rounds_Speeds_Correctly()
        {
            var speeds = new double?[]   { null, 0, 1.2345, 22.24999, 22.25, 22.9999, 999.44, 999.45, 1.10, 1.11, 1.12, 1.13, 1.14, 1.15, 1.16, 1.17, 1.18, 1.19, -1.14, -1.15, };
            var expected = new double?[] { null, 0, 1.2,    22.2,     22.3,  23.0,    999.4,  999.5,  1.1,  1.1,  1.1,  1.1,  1.1,  1.2,  1.2,  1.2,  1.2,  1.2,  -1.1,  -1.2, };

            for(var i = 0;i < speeds.Length;++i) {
                Assert.AreEqual(expected[i], Round.GroundSpeed(speeds[i]));
            }
        }

        [TestMethod]
        public void Round_GroundSpeed_Does_Not_Crash_If_Passed_Excessive_Value()
        {
            Round.GroundSpeed(((double)int.MaxValue) * 100);
            Round.GroundSpeed(((double)int.MinValue) * -100);
        }

        [TestMethod]
        public void Round_Track_Rounds_Tracks_Correctly()
        {
            var tracks = new double?[]   { null, 0, 1.2345, 22.24999, 22.25, 22.9999, 359.94, 359.95, 1.10, 1.11, 1.12, 1.13, 1.14, 1.15, 1.16, 1.17, 1.18, 1.19, };
            var expected = new double?[] { null, 0, 1.2,    22.2,     22.3,  23.0,    359.9,  0,      1.1,  1.1,  1.1,  1.1,  1.1,  1.2,  1.2,  1.2,  1.2,  1.2, };

            for(var i = 0;i < tracks.Length;++i) {
                Assert.AreEqual(expected[i], Round.Track(tracks[i]));
            }
        }

        [TestMethod]
        public void Round_Track_Does_Not_Crash_If_Passed_Excessive_Value()
        {
            Round.Track(((double)int.MaxValue) * 100);
            Round.Track(((double)int.MinValue) * -100);
        }

        [TestMethod]
        public void Round_TrackHeading_Rounds_Tracks_Correctly()
        {
            var tracks = new double?[]   { null, 0, 1.2345, 22.4999, 22.5, 24.9999, 357.49, 357.50, 359.99999, };
            var expected = new double?[] { null, 0, 0,      20,      25,   25,      355,    0,      0, };

            for(var i = 0;i < tracks.Length;++i) {
                Assert.AreEqual(expected[i], Round.TrackHeading(tracks[i]));
            }
        }

        [TestMethod]
        public void Round_TrackHeading_Does_Not_Crash_If_Passed_Excessive_Value()
        {
            Round.TrackHeading(((double)int.MaxValue) * 100);
            Round.TrackHeading(((double)int.MinValue) * -100);
        }

        [TestMethod]
        public void Round_Coordinate_Rounds_Coordinates_Quickly()
        {
            var coordinates = new double?[] { null, 0, 0.0000004, 0.0000005, -0.0000004, -0.0000005, 179.9999994, 179.9999995, -179.9999994, -179.9999995, };
            var expected    = new double?[] { null, 0, 0.000000,  0.000001,  0.000000,   -0.000001,  179.999999,  180.000000,  -179.999999,  -180.000000, };

            for(var i = 0;i < coordinates.Length;++i) {
                Assert.AreEqual(expected[i], Round.Coordinate(coordinates[i]), String.Format("{0:N7}", coordinates[i]));
            }
        }

        [TestMethod]
        public void Round_Coordinate_Does_Not_Crash_If_Passed_Excessive_Value()
        {
            Round.Coordinate(((double)int.MaxValue) * 100.0);
            Round.Coordinate(((double)int.MinValue) * -100.0);
        }

        [TestMethod]
        public void Round_TrackAltitude_Rounds_Altitudes_Correctly()
        {
            var altitudes = new int?[] { null, 0, -0, 1, 249,  250, 500, 749,  750, -1, -249, -250,  };
            var expected = new int?[]  { null, 0,  0, 0,   0,  500, 500, 500, 1000, 0,     0, -500,  };

            for(var i = 0;i < altitudes.Length;++i) {
                Assert.AreEqual(expected[i], Round.TrackAltitude(altitudes[i]));
            }
        }

        [TestMethod]
        public void Round_TrackAltitude_Does_Not_Crash_If_Passed_Excessive_Value()
        {
            Round.TrackAltitude(int.MaxValue);
            Round.TrackAltitude(int.MinValue);
        }

        [TestMethod]
        public void Round_TrackGroundSpeed_Rounds_GroundSpeeds_Correctly()
        {
            var speeds = new double?[]   { null, 0.0, -0.0, 4.99,  5.0,  5.1,  9.9, 10.0, 10.1, 10.499, 15.0, 19.99, -0.1, -4.9,  -5.0,  -9.9, -10, -14.9, -15, -19.9, -20, };
            var expected = new double?[] { null, 0.0,  0.0, 0.00, 10.0, 10.0, 10.0, 10.0, 10.0, 10.000, 20.0, 20.00,  0.0,  0.0, -10.0, -10.0, -10, -10.0, -20, -20.0, -20, };

            for(var i = 0;i < speeds.Length;++i) {
                Assert.AreEqual(expected[i], Round.TrackGroundSpeed(speeds[i]));
            }
        }

    }
}
