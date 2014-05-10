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
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class GreatCircleMathsTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Distance$")]
        public void GreatCircleMaths_Distance_Calculates_Correct_Distances()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var startLatitude = worksheet.NDouble("StartLatitude");
            var startLongitude = worksheet.NDouble("StartLongitude");
            var endLatitude = worksheet.NDouble("EndLatitude");
            var endLongitude = worksheet.NDouble("EndLongitude");
            var expected = worksheet.NDouble("Distance");

            var actual = GreatCircleMaths.Distance(startLatitude, startLongitude, endLatitude, endLongitude);

            if(expected == null) Assert.IsNull(actual);
            else                 Assert.AreEqual((double)expected, (double)actual, 0.0001);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Bearing$")]
        public void GreatCircleMaths_Bearing_Calculates_Correct_Bearing()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var startLatitude = worksheet.NDouble("StartLatitude");
            var startLongitude = worksheet.NDouble("StartLongitude");
            var endLatitude = worksheet.NDouble("EndLatitude");
            var endLongitude = worksheet.NDouble("EndLongitude");
            var currentTrack = worksheet.NDouble("CurrentTrack");
            var expected = worksheet.NDouble("Bearing");

            var actual = GreatCircleMaths.Bearing(startLatitude, startLongitude, endLatitude, endLongitude, currentTrack, worksheet.Bool("ReverseBearing"), worksheet.Bool("IgnoreCurrentTrack"));

            if(expected == null) Assert.IsNull(actual);
            else                 Assert.AreEqual((double)expected, (double)actual, 0.0001);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Destination$")]
        public void GreatCircleMaths_Destination_Calculates_Correct_Destination()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var startLatitude = worksheet.NDouble("StartLatitude");
            var startLongitude = worksheet.NDouble("StartLongitude");
            var bearing = worksheet.NDouble("Bearing");
            var distance = worksheet.NDouble("Distance");

            double? endLatitude, endLongitude;
            GreatCircleMaths.Destination(startLatitude, startLongitude, bearing, distance, out endLatitude, out endLongitude);

            var expectedLatitude = worksheet.NDouble("EndLatitude");
            var expectedLongitude = worksheet.NDouble("EndLongitude");

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
