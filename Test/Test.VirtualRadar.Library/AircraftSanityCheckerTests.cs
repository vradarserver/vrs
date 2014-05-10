// Copyright © 2014 onwards, Andrew Whewell
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
using InterfaceFactory;
using Test.Framework;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class AircraftSanityCheckerTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IAircraftSanityChecker _Checker;

        [TestInitialize]
        public void TestInitialise()
        {
            _Checker = Factory.Singleton.Resolve<IAircraftSanityChecker>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }
        #endregion

        #region CheckAltitude
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "SaneAltitude$")]
        public void AircraftSanityChecker_CheckAltitude_Returns_Correct_Values()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            //if(!worksheet.NBool("JustThis").GetValueOrDefault()) continue;

            var comments = worksheet.String("Comments");
            for(var i = 1;i <= 5;++i) {
                var altitude = worksheet.NInt(String.Format("Altitude{0}", i));
                if(altitude != null) {
                    var seconds = worksheet.Double(String.Format("Seconds{0}", i));
                    var time = new DateTime(2014, 8, 3).AddSeconds(seconds);
                    var expectedResult = worksheet.ParseEnum<Certainty>(String.Format("Result{0}", i));
                    var actualResult = _Checker.CheckAltitude(1, time, altitude.Value);
                    Assert.AreEqual(expectedResult, actualResult, String.Format("Column {0} {1}", i, comments));
                }
            }
        }
        #endregion

        #region FirstGoodAltitude
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "SaneAltitude$")]
        public void AircraftSanityChecker_FirstGoodAltitude_Returns_Correct_Values()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            //if(!worksheet.NBool("JustThis").GetValueOrDefault()) continue;

            var comments = worksheet.String("Comments");
            for(var i = 1;i <= 5;++i) {
                var altitude = worksheet.NInt(String.Format("Altitude{0}", i));
                if(altitude != null) {
                    var seconds = worksheet.Double(String.Format("Seconds{0}", i));
                    var time = new DateTime(2014, 8, 3).AddSeconds(seconds);
                    var expectedResult = worksheet.NInt(String.Format("1stGood{0}", i));
                    _Checker.CheckAltitude(1, time, altitude.Value);
                    var actualResult = _Checker.FirstGoodAltitude(1);
                    Assert.AreEqual(expectedResult, actualResult, String.Format("Column {0} {1}", i, comments));
                }
            }
        }

        [TestMethod]
        public void AircraftSanityChecker_FirstGoodAltitude_Returns_Null_When_Passed_Unknown_ID()
        {
            Assert.IsNull(_Checker.FirstGoodAltitude(1));
        }
        #endregion

        #region CheckPosition
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "SanePosition$")]
        public void AircraftSanityChecker_CheckPosition_Returns_Correct_Values()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            //if(!worksheet.NBool("JustThis").GetValueOrDefault()) continue;

            var comments = worksheet.String("Comments");
            for(var i = 1;i <= 5;++i) {
                var distance = worksheet.NDouble(String.Format("Distance{0}", i));
                if(distance != null) {
                    var seconds = worksheet.Double(String.Format("Seconds{0}", i));
                    var time = new DateTime(2014, 8, 3).AddSeconds(seconds);
                    var expectedResult = worksheet.ParseEnum<Certainty>(String.Format("Result{0}", i));

                    double? latitude, longitude;
                    GreatCircleMaths.Destination(51.0, -0.6, 90.0, distance, out latitude, out longitude);

                    var actualResult = _Checker.CheckPosition(1, time, latitude.Value, longitude.Value);
                    Assert.AreEqual(expectedResult, actualResult, String.Format("Column {0} {1}", i, comments));
                }
            }
        }

        [TestMethod]
        public void AircraftSanityChecker_CheckPosition_Always_Rejects_0_0()
        {
            var time = DateTime.UtcNow;
            _Checker.CheckPosition(1, time, 0.0001, 0.0001);
            _Checker.CheckPosition(1, time.AddSeconds(1), 0.0001, 0.0001);

            Assert.AreEqual(Certainty.Uncertain, _Checker.CheckPosition(1, time.AddSeconds(2), 0, 0));
        }
        #endregion

        #region FirstGoodPosition
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "SanePosition$")]
        public void AircraftSanityChecker_FirstGoodPosition_Returns_Correct_Values()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            //if(!worksheet.NBool("JustThis").GetValueOrDefault()) continue;

            var comments = worksheet.String("Comments");
            for(var i = 1;i <= 5;++i) {
                var distance = worksheet.NDouble(String.Format("Distance{0}", i));
                if(distance != null) {
                    var seconds = worksheet.Double(String.Format("Seconds{0}", i));
                    var time = new DateTime(2014, 8, 3).AddSeconds(seconds);
                    var expectedResult = worksheet.NDouble(String.Format("1stGood{0}", i));

                    double? latitude, longitude;
                    GreatCircleMaths.Destination(51.0, -0.6, 90.0, distance, out latitude, out longitude);

                    _Checker.CheckPosition(1, time, latitude.Value, longitude.Value);
                    var globalCoordinates = _Checker.FirstGoodPosition(1);

                    double? actualResult = null;
                    if(globalCoordinates != null) actualResult = GreatCircleMaths.Distance(51.0, -0.6, globalCoordinates.Latitude, globalCoordinates.Longitude);

                    var message = String.Format("Column {0} {1}", i, comments);
                    if(expectedResult == null) Assert.IsNull(actualResult, message);
                    else                       Assert.AreEqual(expectedResult.Value, actualResult ?? double.MinValue, 0.001, message);
                }
            }
        }

        [TestMethod]
        public void AircraftSanityChecker_FirstGoodPosition_Returns_Null_When_Passed_Unknown_ID()
        {
            Assert.IsNull(_Checker.FirstGoodPosition(1));
        }
        #endregion

        #region ResetAircraft
        [TestMethod]
        public void AircraftSanityChecker_ResetAircraft_Resets_State_For_Altitude_Check()
        {
            var now = DateTime.UtcNow;
            _Checker.CheckAltitude(1, now, 100);
            _Checker.CheckAltitude(1, now, 100);

            _Checker.ResetAircraft(1);

            Assert.AreEqual(Certainty.Uncertain, _Checker.CheckAltitude(1, now, 40000));
        }

        [TestMethod]
        public void AircraftSanityChecker_ResetAircraft_Does_Not_Reset_Altitude_State_For_Other_Aircraft()
        {
            var now = DateTime.UtcNow;
            _Checker.CheckAltitude(1, now, 100);
            _Checker.CheckAltitude(1, now, 100);

            _Checker.ResetAircraft(2);

            Assert.AreEqual(Certainty.CertainlyWrong, _Checker.CheckAltitude(1, now, 40000));
        }

        [TestMethod]
        public void AircraftSanityChecker_ResetAircraft_Resets_State_For_Position_Check()
        {
            var now = DateTime.UtcNow;
            _Checker.CheckPosition(1, now, 51.0, 6.0);
            _Checker.CheckPosition(1, now.AddMilliseconds(1), 51.0, 6.0);

            _Checker.ResetAircraft(1);

            Assert.AreEqual(Certainty.Uncertain, _Checker.CheckPosition(1, now.AddMilliseconds(2), -51.0, -6.0));
        }

        [TestMethod]
        public void AircraftSanityChecker_ResetAircraft_Does_Not_Reset_Position_State_For_Other_Aircraft()
        {
            var now = DateTime.UtcNow;
            _Checker.CheckPosition(1, now, 51.0, 6.0);
            _Checker.CheckPosition(1, now.AddMilliseconds(1), 51.0, 6.0);

            _Checker.ResetAircraft(2);

            Assert.AreEqual(Certainty.CertainlyWrong, _Checker.CheckPosition(1, now.AddMilliseconds(2), -51.0, -6.0));
        }
        #endregion
    }
}
