// Copyright © 2022 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class GeofenceCWH_Tests
    {
        [TestMethod]
        public void Ctor_Fills_Correct_Properties()
        {
            var rect = new GeofenceCWH(
                1.5,
                2.4,
                3.3,
                4.2,
                DistanceUnit.Miles
            );

            Assert.AreEqual(1.5, rect.CentreLatitude);
            Assert.AreEqual(2.4, rect.CentreLongitude);
            Assert.AreEqual(3.3, rect.Width);
            Assert.AreEqual(4.2, rect.Height);
            Assert.AreEqual(DistanceUnit.Miles, rect.DistanceUnit);
        }

        [TestMethod]
        public void Boundaries_Are_Calculated_Correctly()
        {
            new InlineDataTest(this).TestAndAssert(
                new dynamic[] {
                    new { Lat =  0.0,  Lng =    0.0, W = 111.194927, H = 222.389853, D = DistanceUnit.Kilometres, ExW =  -1.0,     ExN =  2.0,     ExE =    1.0,     ExS = -2.0 },
                    new { Lat = 50.0,  Lng = -179.0, W = 900.0,      H = 900.0,      D = DistanceUnit.Miles,      ExW = 161.20584, ExN = 63.02586, ExE = -159.20585, ExS = 36.97414 },
                },
                (dynamic row) => {
                    var rect = new GeofenceCWH(
                        row.Lat,
                        row.Lng,
                        row.W,
                        row.H,
                        row.D
                    );

                    Assert.AreEqual(row.ExW, rect.WesterlyLongitude, 0.0001, "West");
                    Assert.AreEqual(row.ExN, rect.NortherlyLatitude, 0.0001, "North");
                    Assert.AreEqual(row.ExE, rect.EasterlyLongitude, 0.0001, "East");
                    Assert.AreEqual(row.ExS, rect.SoutherlyLatitude, 0.0001, "South");
                }
            );
        }

        [TestMethod]
        public void IsWithinBounds_Returns_Correct_Value()
        {
            new InlineDataTest(this).TestAndAssert(
                new dynamic[] {
                    // W = -1  N = 1  E = 1  S = 1
                    new { CLat =  0.0, CLng = 0.0, W = 111.194927, H = 111.194927, D = DistanceUnit.Kilometres, Lat = (double?)null, Lng = (double?)null, Expected = false },
                    new { CLat =  0.0, CLng = 0.0, W = 111.194927, H = 111.194927, D = DistanceUnit.Kilometres, Lat = -1.1, Lng =  0.0, Expected = false },
                    new { CLat =  0.0, CLng = 0.0, W = 111.194927, H = 111.194927, D = DistanceUnit.Kilometres, Lat =  1.1, Lng =  0.0, Expected = false },
                    new { CLat =  0.0, CLng = 0.0, W = 111.194927, H = 111.194927, D = DistanceUnit.Kilometres, Lat =  0.0, Lng = -1.1, Expected = false },
                    new { CLat =  0.0, CLng = 0.0, W = 111.194927, H = 111.194927, D = DistanceUnit.Kilometres, Lat =  0.0, Lng =  1.1, Expected = false },
                    new { CLat =  0.0, CLng = 0.0, W = 111.194927, H = 111.194927, D = DistanceUnit.Kilometres, Lat = -0.9, Lng =  0.0, Expected = true },
                    new { CLat =  0.0, CLng = 0.0, W = 111.194927, H = 111.194927, D = DistanceUnit.Kilometres, Lat =  0.9, Lng =  0.0, Expected = true },
                    new { CLat =  0.0, CLng = 0.0, W = 111.194927, H = 111.194927, D = DistanceUnit.Kilometres, Lat =  0.0, Lng = -0.9, Expected = true },
                    new { CLat =  0.0, CLng = 0.0, W = 111.194927, H = 111.194927, D = DistanceUnit.Kilometres, Lat =  0.0, Lng =  0.9, Expected = true },

                    // W = 161.20584, N = 63.02586, E = -159.20585, S = 36.97414
                    new { CLat = 50.0, CLng = -179.0, W = 900.0, H = 900.0, D = DistanceUnit.Miles, Lat = 63.0, Lng = 162,  Expected = true },
                    new { CLat = 50.0, CLng = -179.0, W = 900.0, H = 900.0, D = DistanceUnit.Miles, Lat = 63.0, Lng = 160,  Expected = false },
                    new { CLat = 50.0, CLng = -179.0, W = 900.0, H = 900.0, D = DistanceUnit.Miles, Lat = 63.0, Lng = -160, Expected = true },
                    new { CLat = 50.0, CLng = -179.0, W = 900.0, H = 900.0, D = DistanceUnit.Miles, Lat = 63.0, Lng = -158, Expected = false },
                },
                (dynamic row) => {
                    var rect = new GeofenceCWH(
                        row.CLat,
                        row.CLng,
                        row.W,
                        row.H,
                        row.D
                    );

                    var actual = rect.IsWithinBounds(row.Lat, row.Lng);

                    Assert.AreEqual(row.Expected, actual);
                }
            );
        }

        [TestMethod]
        public void Newtonsoft_Json_Deserialisation_Works()
        {
            var rect = new GeofenceCWH(
                4.2,
                3.3,
                2.4,
                1.5,
                DistanceUnit.Kilometres
            );

            var jsonText = JsonConvert.SerializeObject(rect);
            var deserialised = JsonConvert.DeserializeObject<GeofenceCWH>(jsonText);

            Assert.AreEqual(4.2, deserialised.CentreLatitude);
            Assert.AreEqual(3.3, deserialised.CentreLongitude);
            Assert.AreEqual(2.4, deserialised.Width);
            Assert.AreEqual(1.5, deserialised.Height);
            Assert.AreEqual(DistanceUnit.Kilometres, deserialised.DistanceUnit);
            Assert.AreEqual(rect.WesterlyLongitude, deserialised.WesterlyLongitude);
            Assert.AreEqual(rect.NortherlyLatitude, deserialised.NortherlyLatitude);
            Assert.AreEqual(rect.EasterlyLongitude, deserialised.EasterlyLongitude);
            Assert.AreEqual(rect.SoutherlyLatitude, deserialised.SoutherlyLatitude);
        }
    }
}
