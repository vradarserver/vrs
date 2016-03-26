// Copyright © 2016 onwards, Andrew Whewell
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class AirPressureTests
    {
        class ConvertParameters
        {
            public int? InputAltitude { get; set; }
            public int? ExpectedAltitude { get; set; }
            public float PressureInHg { get; set; }

            public ConvertParameters(int? inputAltitude, int? expectedAltitude, float pressureHg)
            {
                InputAltitude = inputAltitude;
                ExpectedAltitude = expectedAltitude;
                PressureInHg = pressureHg;
            }
        }

        [TestMethod]
        public void AirPressure_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var airPressure = new AirPressure();

            TestUtilities.TestProperty(airPressure, r => r.AgeSeconds, (short)0, (short)1);
            TestUtilities.TestProperty(airPressure, r => r.Latitude, 0F, 1.1F);
            TestUtilities.TestProperty(airPressure, r => r.Longitude, 0F, 1.2F);
            TestUtilities.TestProperty(airPressure, r => r.PressureInHg, 0F, 1.3F);
        }

        [TestMethod]
        public void AirPressure_ObservationTimeUtc_Returns_Correct_Time()
        {
            var airPressure = new AirPressure() { AgeSeconds = 10 };
            var fetchTime = DateTime.UtcNow;
            var expected = fetchTime.AddSeconds(10);

            Assert.AreEqual(expected, airPressure.ObservationTimeUtc(fetchTime));
        }

        [TestMethod]
        public void AirPressure_CorrectedAltitudeToPressureAltitude_Returns_Correct_Values()
        {
            var testValues = new List<ConvertParameters>() {
                new ConvertParameters(null, null, 29.00F),
                new ConvertParameters( 500, 1100, 29.32F),
                new ConvertParameters(1000, 1000, 29.92F),
                new ConvertParameters( 500,  270, 30.15F),
            };

            foreach(var useStaticVersion in new bool[] { true, false }) {
                foreach(var testValue in testValues) {
                    int? actual;
                    if(useStaticVersion) {
                        actual = AirPressure.CorrectedAltitudeToPressureAltitude(testValue.InputAltitude, testValue.PressureInHg);
                    } else {
                        var airPressure = new AirPressure() { PressureInHg = testValue.PressureInHg };
                        actual = airPressure.CorrectedAltitudeToPressureAltitude(testValue.InputAltitude);
                    }
                    Assert.AreEqual(testValue.ExpectedAltitude, actual, "Altitude: {0}, Pressure: {1}", testValue.InputAltitude, testValue.PressureInHg);
                }
            }
        }

        [TestMethod]
        public void AirPressure_PressureAltitudeToCorrectedAltitude_Returns_Correct_Values()
        {
            var testValues = new List<ConvertParameters>() {
                new ConvertParameters(null, null, 29.00F),
                new ConvertParameters(1100,  500, 29.32F),
                new ConvertParameters(1000, 1000, 29.92F),
                new ConvertParameters( 270,  500, 30.15F),
            };

            foreach(var useStaticVersion in new bool[] { true, false }) {
                foreach(var testValue in testValues) {
                    int? actual;
                    if(useStaticVersion) {
                        actual = AirPressure.PressureAltitudeToCorrectedAltitude(testValue.InputAltitude, testValue.PressureInHg);
                    } else {
                        var airPressure = new AirPressure() { PressureInHg = testValue.PressureInHg };
                        actual = airPressure.PressureAltitudeToCorrectedAltitude(testValue.InputAltitude);
                    }
                    Assert.AreEqual(testValue.ExpectedAltitude, actual, "Altitude: {0}, Pressure: {1}", testValue.InputAltitude, testValue.PressureInHg);
                }
            }
        }
    }
}
