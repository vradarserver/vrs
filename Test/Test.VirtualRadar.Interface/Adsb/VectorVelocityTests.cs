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
using VirtualRadar.Interface.Adsb;
using Test.Framework;

namespace Test.VirtualRadar.Interface.Adsb
{
    [TestClass]
    public class VectorVelocityTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void VectorVelocity_Initialises_To_Known_State_And_Properties_Work()
        {
            var velocity = new VectorVelocity();
            Assert.IsNull(velocity.Bearing);
            Assert.IsNull(velocity.Speed);

            TestUtilities.TestProperty(velocity, r => r.EastWestExceeded, false);
            TestUtilities.TestProperty(velocity, r => r.EastWestVelocity, null, (short)1234);
            TestUtilities.TestProperty(velocity, r => r.IsSoutherlyVelocity, false);
            TestUtilities.TestProperty(velocity, r => r.IsWesterlyVelocity, false);
            TestUtilities.TestProperty(velocity, r => r.NorthSouthExceeded, false);
            TestUtilities.TestProperty(velocity, r => r.NorthSouthVelocity, null, (short)1234);
        }

        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "VectorVelocity$")]
        public void VectorVelocity_Calculates_Velocity_And_Bearing_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var velocity = new VectorVelocity() {
                IsWesterlyVelocity = worksheet.Bool("IsWesterlyVelocity"),
                IsSoutherlyVelocity = worksheet.Bool("IsSoutherlyVelocity"),
                EastWestVelocity = worksheet.NShort("EastWestVelocity"),
                EastWestExceeded = worksheet.Bool("EastWestExceeded"),
                NorthSouthVelocity = worksheet.NShort("NorthSouthVelocity"),
                NorthSouthExceeded = worksheet.Bool("NorthSouthExceeded"),
            };

            if(worksheet.String("Speed") == null) Assert.IsNull(velocity.Speed);
            else Assert.AreEqual(worksheet.Double("Speed"), velocity.Speed.Value, 0.000001);

            if(worksheet.String("Bearing") == null) Assert.IsNull(velocity.Bearing);
            else Assert.AreEqual(worksheet.Double("Bearing"), velocity.Bearing.Value, 0.000001);
        }

        [TestMethod]
        public void VectorVelocity_Recalculates_When_Properties_Change()
        {
            var velocity = new VectorVelocity();
            DoCheckCalculationAfterPropertyChange(velocity, v => { v.EastWestVelocity = 0; v.NorthSouthVelocity = 0; }, 0, null);
            DoCheckCalculationAfterPropertyChange(velocity, v => v.EastWestVelocity = 1, 1, 90);
            DoCheckCalculationAfterPropertyChange(velocity, v => v.IsWesterlyVelocity = true, 1, 270);
            DoCheckCalculationAfterPropertyChange(velocity, v => { v.EastWestVelocity = 0; v.NorthSouthVelocity = 1; }, 1, 0);
            DoCheckCalculationAfterPropertyChange(velocity, v => v.IsSoutherlyVelocity = true, 1, 180);
            DoCheckCalculationAfterPropertyChange(velocity, v => { v.EastWestVelocity = 0; v.NorthSouthVelocity = 0; }, 0, null);
        }

        private void DoCheckCalculationAfterPropertyChange(VectorVelocity velocity, Action<VectorVelocity> changeVelocity, double? expectedVelocity, double? expectedBearing)
        {
            changeVelocity(velocity);
            Assert.AreEqual(expectedVelocity, velocity.Speed);
            Assert.AreEqual(expectedBearing, velocity.Bearing);
        }
    }
}
