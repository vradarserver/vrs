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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class CoordinateTests
    {
        [TestMethod]
        public void Coordinate_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var coordinate1 = new Coordinate(1.1, 2.2);
            Assert.AreEqual(0L, coordinate1.DataVersion);
            Assert.AreEqual(0L, coordinate1.Tick);
            Assert.AreEqual(1.1, coordinate1.Latitude);
            Assert.AreEqual(2.2, coordinate1.Longitude);
            Assert.AreEqual(null, coordinate1.Heading);
            Assert.AreEqual(null, coordinate1.Altitude);
            Assert.AreEqual(null, coordinate1.GroundSpeed);

            var coordinate2 = new Coordinate(1L, 2L, 3.1, 4.1, 5f);
            Assert.AreEqual(1L, coordinate2.DataVersion);
            Assert.AreEqual(2L, coordinate2.Tick);
            Assert.AreEqual(3.1, coordinate2.Latitude);
            Assert.AreEqual(4.1, coordinate2.Longitude);
            Assert.AreEqual(5f, coordinate2.Heading);
            Assert.AreEqual(null, coordinate1.Altitude);
            Assert.AreEqual(null, coordinate1.GroundSpeed);

            var coordinate3 = new Coordinate(1L, 2L, 3.1, 4.1, 5f, 6, 7f);
            Assert.AreEqual(1L, coordinate3.DataVersion);
            Assert.AreEqual(2L, coordinate3.Tick);
            Assert.AreEqual(3.1, coordinate3.Latitude);
            Assert.AreEqual(4.1, coordinate3.Longitude);
            Assert.AreEqual(5f, coordinate3.Heading);
            Assert.AreEqual(6, coordinate3.Altitude);
            Assert.AreEqual(7f, coordinate3.GroundSpeed);
        }

        [TestMethod]
        public void Coordinate_Equals_Compares_Two_Coordinates_As_Equal_If_Their_Latitude_And_Longitude_And_Altitude_And_Speed_Match()
        {
            foreach(var property in typeof(Coordinate).GetProperties()) {
                var ignore = false;
                switch(property.Name) {
                    case "DataVersion":
                    case "Tick":
                    case "Heading":
                        ignore = true;
                        break;
                }
                if(ignore) continue;

                foreach(var expectedEquals in new bool[] { true, false }) {
                    double latitude1 = 1.0, latitude2 = 1.0;
                    double longitude1 = 2.0, longitude2 = 2.0;
                    int? altitude1 = null, altitude2 = null;
                    float? groundSpeed1 = null, groundSpeed2 = null;

                    if(!expectedEquals) {
                        switch(property.Name) {
                            case "Latitude":    latitude2 = 8.0; break;
                            case "Longitude":   longitude2 = 8.0; break;
                            case "Altitude":    altitude2 = 1; break;
                            case "GroundSpeed": groundSpeed2 = 1f; break;
                            default:            throw new NotImplementedException();
                        }
                    }

                    var obj1 = new Coordinate(1, 1, latitude1, longitude1, 1f, altitude1, groundSpeed1);
                    var obj2 = new Coordinate(expectedEquals ? 2 : 1, expectedEquals ? 2 : 1, latitude2, longitude2, expectedEquals ? 2f : 1f, altitude2, groundSpeed2);

                    Assert.AreEqual(expectedEquals, obj1.Equals(obj2), "{0}", property.Name);
                }
            }
        }

        [TestMethod]
        public void Coordinate_GetHashCode_Returns_Same_Value_For_Two_Objects_That_Compare_As_Equal()
        {
            var c1 = new Coordinate(1, 2, 99, 100, 37.2f, 7, 8);
            var c2 = new Coordinate(5, 6, 99, 100, 99.5f, 7, 8);

            Assert.AreEqual(c1.GetHashCode(), c2.GetHashCode());
        }
    }
}
