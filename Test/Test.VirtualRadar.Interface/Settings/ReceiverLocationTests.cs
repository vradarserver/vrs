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
using VirtualRadar.Interface.Settings;
using Test.Framework;

namespace Test.VirtualRadar.Interface.Settings
{
    [TestClass]
    public class ReceiverLocationTests
    {
        [TestMethod]
        public void ReceiverLocation_Initialises_To_Known_State_And_Properties_Work()
        {
            var location = new ReceiverLocation();

            TestUtilities.TestProperty(location, r => r.IsBaseStationLocation, false);
            TestUtilities.TestProperty(location, r => r.Latitude, 0.0, 123.567);
            TestUtilities.TestProperty(location, r => r.Longitude, 0.0, -123.456);
            TestUtilities.TestProperty(location, r => r.Name, null, "Home");
            TestUtilities.TestProperty(location, r => r.UniqueId, 0, 123);
        }

        [TestMethod]
        public void ReceiverLocation_Equals_Returns_Correct_Values()
        {
            Func<ReceiverLocation> createStandardValues = () => { return new ReceiverLocation() { IsBaseStationLocation = true, Latitude = 1.0, Longitude = 2.0, Name = "A", UniqueId = 1 }; };
            var obj1 = createStandardValues();
            var obj2 = createStandardValues();

            Assert.AreEqual(obj1, obj2);

            foreach(var property in typeof(ReceiverLocation).GetProperties()) {
                switch(property.Name) {
                    case "IsBaseStationLocation":   obj2.IsBaseStationLocation = false; break;
                    case "Latitude":                obj2.Latitude = 1.11; break;
                    case "Longitude":               obj2.Longitude = 2.22; break;
                    case "Name":                    obj2.Name = "B"; break;
                    case "UniqueId":                obj2.UniqueId = 7; break;
                    default:                        throw new NotImplementedException();
                }

                Assert.AreNotEqual(obj1, obj2);
                obj2 = createStandardValues();
            }
        }

        [TestMethod]
        public void ReceiverLocation_Clone_Creates_Copy()
        {
            var original = new ReceiverLocation() {
                IsBaseStationLocation = true,
                Latitude = 1.0,
                Longitude = 2.0,
                Name = "A",
                UniqueId = 1,
            };

            var copy = (ReceiverLocation)original.Clone();
            Assert.AreNotSame(original, copy);

            foreach(var property in typeof(ReceiverLocation).GetProperties()) {
                switch(property.Name) {
                    case "IsBaseStationLocation":   Assert.AreEqual(true, copy.IsBaseStationLocation); break;
                    case "Latitude":                Assert.AreEqual(1.0, copy.Latitude); break;
                    case "Longitude":               Assert.AreEqual(2.0, copy.Longitude); break;
                    case "Name":                    Assert.AreEqual("A", copy.Name); break;
                    case "UniqueId":                Assert.AreEqual(1, copy.UniqueId); break;
                    default:                        throw new NotImplementedException();
                }
            }
        }
    }
}
