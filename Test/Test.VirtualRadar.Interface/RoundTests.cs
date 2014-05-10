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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class RoundTests
    {
        #region GroundSpeed
        [TestMethod]
        public void Round_GroundSpeed_Rounds_Speeds_Correctly()
        {
            var speeds = new float?[]   { null, 0F, 1.2345F, 22.24999F, 22.25F, 22.9999F, 999.44F, 999.45F, 1.10F, 1.11F, 1.12F, 1.13F, 1.14F, 1.15F, 1.16F, 1.17F, 1.18F, 1.19F, -1.14F, -1.15F, };
            var expected = new float?[] { null, 0F, 1.2F,    22.2F,     22.3F,  23.0F,    999.4F,  999.5F,  1.1F,  1.1F,  1.1F,  1.1F,  1.1F,  1.2F,  1.2F,  1.2F,  1.2F,  1.2F,  -1.1F,  -1.2F, };

            for(var i = 0;i < speeds.Length;++i) {
                Assert.AreEqual(expected[i], Round.GroundSpeed(speeds[i]));
            }
        }

        [TestMethod]
        public void Round_GroundSpeed_Does_Not_Crash_If_Passed_Excessive_Value()
        {
            Round.GroundSpeed(((float)int.MaxValue) * 100F);
            Round.GroundSpeed(((float)int.MinValue) * -100F);
        }
        #endregion

        #region Track
        [TestMethod]
        public void Round_Track_Rounds_Tracks_Correctly()
        {
            var tracks = new float?[]   { null, 0F, 1.2345F, 22.24999F, 22.25F, 22.9999F, 359.94F, 359.95F, 1.10F, 1.11F, 1.12F, 1.13F, 1.14F, 1.15F, 1.16F, 1.17F, 1.18F, 1.19F, };
            var expected = new float?[] { null, 0F, 1.2F,    22.2F,     22.3F,  23.0F,    359.9F,  0F,      1.1F,  1.1F,  1.1F,  1.1F,  1.1F,  1.2F,  1.2F,  1.2F,  1.2F,  1.2F, };

            for(var i = 0;i < tracks.Length;++i) {
                Assert.AreEqual(expected[i], Round.Track(tracks[i]));
            }
        }

        [TestMethod]
        public void Round_Track_Does_Not_Crash_If_Passed_Excessive_Value()
        {
            Round.Track(((float)int.MaxValue) * 100F);
            Round.Track(((float)int.MinValue) * -100F);
        }
        #endregion

        #region TrackHeading
        [TestMethod]
        public void Round_TrackHeading_Rounds_Tracks_Correctly()
        {
            var tracks = new float?[]   { null, 0F, 1.2345F, 22.4999F, 22.5F, 24.9999F, 357.49F, 357.50F, 359.99999F, };
            var expected = new float?[] { null, 0F, 0F,      20F,      25F,   25F,      355F,    0F,      0F, };

            for(var i = 0;i < tracks.Length;++i) {
                Assert.AreEqual(expected[i], Round.TrackHeading(tracks[i]));
            }
        }

        [TestMethod]
        public void Round_TrackHeading_Does_Not_Crash_If_Passed_Excessive_Value()
        {
            Round.TrackHeading(((float)int.MaxValue) * 100F);
            Round.TrackHeading(((float)int.MinValue) * -100F);
        }
        #endregion

        #region Coordindate
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
        #endregion

        #region TrackAltitude
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
        #endregion

        #region TrackGroundSpeed
        [TestMethod]
        public void Round_TrackGroundSpeed_Rounds_GroundSpeeds_Correctly()
        {
            var speeds = new float?[]   { null, 0.0f, -0.0f, 4.99f,  5.0f,  5.1f,  9.9f, 10.0f, 10.1f, 10.499f, 15.0f, 19.99f, -0.1f, -4.9f,  -5.0f,  -9.9f, -10f, -14.9f, -15f, -19.9f, -20f };
            var expected = new float?[] { null, 0.0f,  0.0f, 0.00f, 10.0f, 10.0f, 10.0f, 10.0f, 10.0f, 10.000f, 20.0f, 20.00f,  0.0f,  0.0f, -10.0f, -10.0f, -10f, -10.0f, -20f, -20.0f, -20f };

            for(var i = 0;i < speeds.Length;++i) {
                Assert.AreEqual(expected[i], Round.TrackGroundSpeed(speeds[i]));
            }
        }
        #endregion
    }
}
