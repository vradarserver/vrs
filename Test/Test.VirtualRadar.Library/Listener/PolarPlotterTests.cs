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
using VirtualRadar.Interface.Listener;
using InterfaceFactory;
using Test.Framework;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;
using Moq;
using VirtualRadar.Interface.Adsb;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class PolarPlotterTests
    {
        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;
        private IPolarPlotter _Plotter;

        private Mock<IAircraftSanityChecker> _SanityChecker;
        private Certainty _CheckAltitudeResult;
        private Certainty _CheckPositionResult;

        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Configuration _Configuration;
        private SavedPolarPlot _SavedPolarPlot;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(r => r.Load()).Returns(_Configuration);

            _Plotter = Factory.Resolve<IPolarPlotter>();
            _SanityChecker = TestUtilities.CreateMockImplementation<IAircraftSanityChecker>();

            _CheckAltitudeResult = Certainty.ProbablyRight;
            _SanityChecker.Setup(r => r.CheckAltitude(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>())).Returns((int id, DateTime date, int alt) => {
                return _CheckAltitudeResult;
            });
            _SanityChecker.Setup(r => r.FirstGoodAltitude(It.IsAny<int>())).Returns(() => {
                throw new InvalidOperationException();
            });

            _CheckPositionResult = Certainty.ProbablyRight;
            _SanityChecker.Setup(r => r.CheckPosition(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>())).Returns((int id, DateTime date, double lat, double lng) => {
                return _CheckPositionResult;
            });
            _SanityChecker.Setup(r => r.FirstGoodPosition(It.IsAny<int>())).Returns(() => {
                throw new InvalidOperationException();
            });

            _SavedPolarPlot = new SavedPolarPlot();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }
        #endregion

        #region Helper - StandardInitialise, ThrowsArgumentException
        private void StandardInitialise()
        {
            _Plotter.Initialise(51, -0.6, 0, 19, 10, 5);
        }

        private bool ThrowsArgumentException(Action action, bool resetEnvironment)
        {
            if(resetEnvironment) {
                TestCleanup();
                TestInitialise();
            }

            bool thrown = false;
            try {
                action();
            } catch(ArgumentException) {
                thrown = true;
            }

            return thrown;
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void PolarPlotter_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.AreEqual(0.0, _Plotter.Latitude);
            Assert.AreEqual(0.0, _Plotter.Longitude);
            Assert.AreEqual(0, _Plotter.RoundToDegrees);
            Assert.AreEqual(0, _Plotter.TakeSnapshot().Count);
        }
        #endregion

        #region Initialise - standard version
        [TestMethod]
        public void PolarPlotter_Initialise_Standard_Throws_If_Latitude_Out_Of_Bounds()
        {
            Assert.IsTrue (ThrowsArgumentException(() => _Plotter.Initialise(-90.0001, 0), true));
            Assert.IsFalse(ThrowsArgumentException(() => _Plotter.Initialise(-90.0000, 0), true));
            Assert.IsFalse(ThrowsArgumentException(() => _Plotter.Initialise( 90.0000, 0), true));
            Assert.IsTrue (ThrowsArgumentException(() => _Plotter.Initialise( 90.0001, 0), true));
        }

        [TestMethod]
        public void PolarPlotter_Initialise_Standard_Throws_If_Longitude_Out_Of_Bounds()
        {
            Assert.IsTrue (ThrowsArgumentException(() => _Plotter.Initialise(0, -180.0001), true));
            Assert.IsFalse(ThrowsArgumentException(() => _Plotter.Initialise(0, -180.0000), true));
            Assert.IsFalse(ThrowsArgumentException(() => _Plotter.Initialise(0,  180.0000), true));
            Assert.IsTrue (ThrowsArgumentException(() => _Plotter.Initialise(0,  180.0001), true));
        }

        [TestMethod]
        public void PolarPlotter_Initialise_Standard_Records_Latitude_And_Longitude()
        {
            _Plotter.Initialise(1.234, 5.678);
            Assert.AreEqual(1.234, _Plotter.Latitude);
            Assert.AreEqual(5.678, _Plotter.Longitude);
        }

        [TestMethod]
        public void PolarPlotter_Initialise_Standard_Sets_Correct_Rounding()
        {
            _Plotter.Initialise(0, 0);
            Assert.AreEqual(1, _Plotter.RoundToDegrees);
        }

        [TestMethod]
        public void PolarPlotter_Initialise_Standard_Sets_Correct_Slices()
        {
            _Plotter.Initialise(0, 0);
            var snapshot = _Plotter.TakeSnapshot();
            Assert.AreEqual(5, snapshot.Count);
            Assert.IsTrue(snapshot.Any(r => r.AltitudeLower == int.MinValue && r.AltitudeHigher == int.MaxValue));
            Assert.IsTrue(snapshot.Any(r => r.AltitudeLower == int.MinValue && r.AltitudeHigher == 9999));
            Assert.IsTrue(snapshot.Any(r => r.AltitudeLower == 10000        && r.AltitudeHigher == 19999));
            Assert.IsTrue(snapshot.Any(r => r.AltitudeLower == 20000        && r.AltitudeHigher == 29999));
            Assert.IsTrue(snapshot.Any(r => r.AltitudeLower == 30000        && r.AltitudeHigher == int.MaxValue));
        }

        [TestMethod]
        public void PolarPlotter_Initialise_Standard_Clears_Existing_Slices()
        {
            _Plotter.Initialise(0, 0);
            _Plotter.AddCoordinate(1, 1, 1, 1);
            _Plotter.Initialise(1, 1);

            var snapshot = _Plotter.TakeSnapshot();
            Assert.AreEqual(5, snapshot.Count);
            Assert.IsFalse(snapshot.Any(r => r.PolarPlots.Count != 360));
            Assert.IsFalse(snapshot.Any(r => r.PolarPlots.Where(i => i.Value.Distance != 0).Count() > 0));
        }
        #endregion

        #region Initialise - custom version
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "PolarPlotterInit$")]
        public void PolarPlotter_Initialise_Initialises_The_Plotter_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var roundDegrees = 0;
            var seenArgException = false;
            try {
                roundDegrees = worksheet.Int("RoundDegree");
                _Plotter.Initialise(
                    worksheet.Double("Latitude"),
                    worksheet.Double("Longitude"),
                    worksheet.Int("LowSlice"),
                    worksheet.Int("HighSlice"),
                    worksheet.Int("SliceHeight"),
                    roundDegrees
                );
            } catch(ArgumentException) {
                seenArgException = true;
            }

            Assert.AreEqual(worksheet.Bool("ArgException"), seenArgException);
            if(!seenArgException) {
                var slices = _Plotter.TakeSnapshot();
                Assert.AreEqual(worksheet.Double("Latitude"), _Plotter.Latitude);
                Assert.AreEqual(worksheet.Double("Longitude"), _Plotter.Longitude);
                Assert.AreEqual(worksheet.Int("RoundDegree"), _Plotter.RoundToDegrees);
                Assert.AreEqual(worksheet.Int("CountSlices"), slices.Count);

                var expectedPlotsPerSlice = 360 / roundDegrees;
                foreach(var slice in slices) {
                    Assert.AreEqual(expectedPlotsPerSlice, slice.PolarPlots.Count);
                }

                for(var i = 1;i <= 5;++i) {
                    var lowName = String.Format("Low{0}", i);
                    var highName = String.Format("High{0}", i);
                    var lowText = worksheet.String(lowName);
                    var highText = worksheet.String(highName);
                    if(lowText != null && highText != null) {
                        var expectedLow = lowText == "Min" ? int.MinValue : int.Parse(lowText);
                        var expectedHigh = highText == "Max" ? int.MaxValue : int.Parse(highText);
                        Assert.IsTrue(slices.Any(r => r.AltitudeLower == expectedLow && r.AltitudeHigher == expectedHigh), "Could not find {0}-{1}", lowText, highText);
                    }
                }
            }
        }

        [TestMethod]
        public void PolarPlotter_Initialise_Clears_Existing_Slices()
        {
            _Plotter.Initialise(0, 0, 0, 0, 100, 1);
            _Plotter.Initialise(0, 0, 0, 9, 10, 1);

            Assert.AreEqual(2, _Plotter.TakeSnapshot().Count);
        }
        #endregion

        #region AddCoordinate
        [TestMethod]
        public void PolarPlotter_AddCoordinate_Does_Nothing_If_Called_Before_Initialise()
        {
            _Plotter.AddCoordinate(1, 12, 52, 1);

            var slices = _Plotter.TakeSnapshot();
            Assert.IsFalse(slices.Any(r => r.PolarPlots.Count != 0));
        }

        [TestMethod]
        public void PolarPlotter_AddCoordinate_Ignores_Uncertain_Altitudes()
        {
            _CheckAltitudeResult = Certainty.Uncertain;

            StandardInitialise();
            _Plotter.AddCoordinate(1, 12, 52.231, 1.234);

            var slices = _Plotter.TakeSnapshot();
            Assert.AreEqual(0, slices.Count(r => r.PolarPlots.Count(i => i.Value.Distance != 0) > 0));
        }

        [TestMethod]
        public void PolarPlotter_AddCoordinate_Ignores_CertainlyWrong_Altitudes()
        {
            _CheckAltitudeResult = Certainty.CertainlyWrong;

            StandardInitialise();
            _Plotter.AddCoordinate(1, 12, 52.231, 1.234);

            var slices = _Plotter.TakeSnapshot();
            Assert.AreEqual(0, slices.Count(r => r.PolarPlots.Count(i => i.Value.Distance != 0) > 0));
        }

        [TestMethod]
        public void PolarPlotter_AddCoordinate_Allows_ProbablyRight_Altitudes()
        {
            _CheckAltitudeResult = Certainty.ProbablyRight;

            StandardInitialise();
            _Plotter.AddCoordinate(1, 12, 52.231, 1.234);

            var slices = _Plotter.TakeSnapshot();
            var plot = slices.Single(r => r.AltitudeLower == 10 && r.AltitudeHigher == 19).PolarPlots.Values.Single(r => r.Distance != 0);
            Assert.AreEqual(12, plot.Altitude);
        }

        [TestMethod]
        public void PolarPlotter_AddCoordinate_Ignores_Uncertain_Positions()
        {
            _CheckPositionResult = Certainty.Uncertain;

            StandardInitialise();
            _Plotter.AddCoordinate(1, 12, 52.231, 1.234);

            var slices = _Plotter.TakeSnapshot();
            Assert.AreEqual(0, slices.Count(r => r.PolarPlots.Count(i => i.Value.Distance != 0) > 0));
        }

        [TestMethod]
        public void PolarPlotter_AddCoordinate_Ignores_CertainlyWrong_Positions()
        {
            _CheckPositionResult = Certainty.CertainlyWrong;

            StandardInitialise();
            _Plotter.AddCoordinate(1, 12, 52.231, 1.234);

            var slices = _Plotter.TakeSnapshot();
            Assert.AreEqual(0, slices.Count(r => r.PolarPlots.Count(i => i.Value.Distance != 0) > 0));
        }

        [TestMethod]
        public void PolarPlotter_AddCoordinate_Allows_ProbablyRight_Positions()
        {
            _CheckPositionResult = Certainty.ProbablyRight;

            StandardInitialise();
            _Plotter.AddCoordinate(1, 12, 52.231, 1.234);

            var slices = _Plotter.TakeSnapshot();
            var plot = slices.Single(r => r.AltitudeLower == 10 && r.AltitudeHigher == 19).PolarPlots.Values.Single(r => r.Distance != 0);
            Assert.AreEqual(12, plot.Altitude);
        }

        [TestMethod]
        public void PolarPlotter_AddCoordinate_Ignores_Positions_Further_Than_Radar_Range()
        {
            _Configuration.RawDecodingSettings.ReceiverRange = 650;
            StandardInitialise();
            double? latitude, longitude;
            GreatCircleMaths.Destination(_Plotter.Latitude, _Plotter.Longitude, 90, 650.001, out latitude, out longitude);

            _Plotter.AddCoordinate(1, 12, latitude.Value, longitude.Value);

            var slices = _Plotter.TakeSnapshot();
            Assert.AreEqual(0, slices.Count(r => r.PolarPlots.Count(i => i.Value.Distance != 0) > 0));
        }

        [TestMethod]
        public void PolarPlotter_AddCoordinate_Records_Plot_Correctly()
        {
            StandardInitialise();
            _Plotter.AddCoordinate(1, 12, 52.231, 1.234);

            var slices = _Plotter.TakeSnapshot();
            var plot = slices.Single(r => r.AltitudeLower == 10 && r.AltitudeHigher == 19).PolarPlots.Values.Single(r => r.Distance != 0);

            Assert.AreEqual(12, plot.Altitude);
            Assert.AreEqual(52.231, plot.Latitude);
            Assert.AreEqual(1.234, plot.Longitude);
            Assert.AreEqual(40, plot.Angle);
            Assert.AreEqual(186.4583, plot.Distance, 0.0001);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "PolarPlotterAdd$")]
        public void PolarPlotter_AddCoordinate_Adds_Plot_To_All_Applicable_Slices()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var comment = worksheet.String("Comments");
            //if(comment != "comment here") return;

            StandardInitialise();

            for(var i = 1;i <= 2;++i) {
                var altitude = worksheet.NInt(String.Format("Altitude{0}", i));
                var latitude = worksheet.NDouble(String.Format("Latitude{0}", i));
                var longitude = worksheet.NDouble(String.Format("Longitude{0}", i));
                if(altitude != null && latitude != null && longitude != null) {
                    _Plotter.AddCoordinate(1, altitude.Value, latitude.Value, longitude.Value);
                }
            }

            foreach(var slice in _Plotter.TakeSnapshot()) {
                string suffix;
                switch(slice.AltitudeHigher) {
                    case 9:     suffix = "-09"; break;
                    case 19:    suffix = slice.AltitudeLower == 10 ? "-19" : "-RG"; break;
                    default:    suffix = "-ALL"; break;
                }
                var nonZeroPolarPlots = slice.PolarPlots.Where(r => r.Value.Distance != 0).ToDictionary(r => r.Key, r => r.Value);
                Assert.AreEqual(worksheet.Int(String.Format("Count{0}", suffix)), nonZeroPolarPlots.Count, suffix);
                for(var i = 1;i <= nonZeroPolarPlots.Count;++i) {
                    var bearing = worksheet.Int(String.Format("Ang{0}{1}", i, suffix));
                    var plot = nonZeroPolarPlots[bearing];
                    Assert.AreEqual(worksheet.Int(String.Format("Alt{0}{1}", i, suffix)), plot.Altitude, suffix);
                    Assert.AreEqual(worksheet.Double(String.Format("Dist{0}{1}", i, suffix)), plot.Distance, 0.0001, suffix);
                    Assert.AreEqual(bearing, plot.Angle, suffix);
                }
            }
        }

        [TestMethod]
        public void PolarPlotter_AddCoordinate_Rounds_Bearings_Correctly()
        {
            TestBearings(1, new Dictionary<double, int> {
                { 0.0, 0 },
                { 0.1, 0 },
                { 0.4, 0 },
                { 0.5, 1 },
                { 0.9, 1 },
                { 1.4, 1 },
                { 1.5, 2 },
                { 359.4, 359 },
                { 359.5, 0 },
            });

            TestBearings(2, new Dictionary<double, int> {
                { 0.9999, 0 },
                { 1, 2 },
                { 2.9999, 2 },
                { 3, 4 },
                { 4.9999, 4 },
                { 5, 6 },
                { 358.9999, 358 },
                { 359.0001, 0 },
            });

            TestBearings(90, new Dictionary<double, int> {
                { 0.0, 0 },
                { 44.9, 0 },
                { 45.0, 90 },
                { 224.999, 180 },
                { 225.001, 270 },
                { 314.999, 270 },
                { 315.001, 0 },
            });

            TestBearings(180, new Dictionary<double, int> {
                { 270.001, 0 },
                { 89.999, 0 },
                { 90.001, 180 },
                { 269.999, 180 },
            });
        }

        private void TestBearings(int roundToDegrees, Dictionary<double, int> bearings)
        {
            foreach(var kvp in bearings) {
                var actualBearing = kvp.Key;
                var expectedBearing = kvp.Value;

                _Plotter.Initialise(51, -0.6, 0, 19, 10, roundToDegrees);

                double? latitude, longitude;
                GreatCircleMaths.Destination(_Plotter.Latitude, _Plotter.Longitude, actualBearing, 500, out latitude, out longitude);
                _Plotter.AddCoordinate(1, 5, latitude.Value, longitude.Value);

                var slices = _Plotter.TakeSnapshot();
                var plot = slices.Single(r => r.AltitudeLower == 0 && r.AltitudeHigher == 9).PolarPlots.Values.Single(i => i.Distance != 0);

                Assert.AreEqual(expectedBearing, plot.Angle, "actualBearing {0} rounded to {1}°", actualBearing, roundToDegrees);
            }
        }
        #endregion

        #region AddCoordinate
        [TestMethod]
        public void PolarPlotter_AddCheckedCoordinate_Does_Nothing_If_Called_Before_Initialise()
        {
            _Plotter.AddCheckedCoordinate(1, 12, 52, 1);

            var slices = _Plotter.TakeSnapshot();
            Assert.IsFalse(slices.Any(r => r.PolarPlots.Count != 0));
        }

        [TestMethod]
        public void PolarPlotter_AddCheckedCoordinate_Does_Not_Check_Altitudes_For_Sanity()
        {
            StandardInitialise();
            _Plotter.AddCheckedCoordinate(1, 12, 52.231, 1.234);
            _SanityChecker.Verify(r => r.CheckAltitude(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void PolarPlotter_AddCheckedCoordinate_Does_Not_Check_Positions_For_Sanity()
        {
            StandardInitialise();
            _Plotter.AddCheckedCoordinate(1, 12, 52.231, 1.234);
            _SanityChecker.Verify(r => r.CheckPosition(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }

        [TestMethod]
        public void PolarPlotter_AddCheckedCoordinate_Ignores_Positions_Further_Than_Radar_Range()
        {
            _Configuration.RawDecodingSettings.ReceiverRange = 650;
            StandardInitialise();
            double? latitude, longitude;
            GreatCircleMaths.Destination(_Plotter.Latitude, _Plotter.Longitude, 90, 650.001, out latitude, out longitude);

            _Plotter.AddCheckedCoordinate(1, 12, latitude.Value, longitude.Value);

            var slices = _Plotter.TakeSnapshot();
            Assert.AreEqual(0, slices.Count(r => r.PolarPlots.Count(i => i.Value.Distance != 0) > 0));
        }

        [TestMethod]
        public void PolarPlotter_AddCheckedCoordinate_Records_Plot_Correctly()
        {
            StandardInitialise();
            _Plotter.AddCheckedCoordinate(1, 12, 52.231, 1.234);

            var slices = _Plotter.TakeSnapshot();
            var plot = slices.Single(r => r.AltitudeLower == 10 && r.AltitudeHigher == 19).PolarPlots.Values.Single(r => r.Distance != 0);

            Assert.AreEqual(12, plot.Altitude);
            Assert.AreEqual(52.231, plot.Latitude);
            Assert.AreEqual(1.234, plot.Longitude);
            Assert.AreEqual(40, plot.Angle);
            Assert.AreEqual(186.4583, plot.Distance, 0.0001);
        }
        #endregion

        #region ClearPolarPlots
        [TestMethod]
        public void PolarPlotter_ClearPolarPlots_Resets_Polar_Plots_For_All_Slices()
        {
            StandardInitialise();
            _Plotter.AddCheckedCoordinate(1, 12, 52.231, 1.234);
            _Plotter.ClearPolarPlots();

            var slices = _Plotter.TakeSnapshot();
            Assert.AreNotEqual(0, slices.Count);
            Assert.IsFalse(slices.Any(r => r.PolarPlots.Any(i => i.Value.Distance != 0)));
        }

        [TestMethod]
        public void PolarPlotter_ClearPolarPlots_Adds_Initial_Bearings_For_All_Slices()
        {
            StandardInitialise();
            _Plotter.ClearPolarPlots();

            var slices = _Plotter.TakeSnapshot();
            Assert.IsFalse(slices.Any(r => r.PolarPlots.Count != (360 / 5)));
        }

        [TestMethod]
        public void PolarPlotter_ClearPolarPlots_Does_Nothing_If_Called_Before_Initialise()
        {
            _Plotter.ClearPolarPlots();

            var slices = _Plotter.TakeSnapshot();
            Assert.AreEqual(0, slices.Count);
        }

        [TestMethod]
        public void PolarPlotter_ClearPolarPlots_Does_Not_Reset_Location()
        {
            StandardInitialise();
            _Plotter.ClearPolarPlots();

            Assert.AreEqual(51.0, _Plotter.Latitude);
            Assert.AreEqual(-0.6, _Plotter.Longitude);
        }

        [TestMethod]
        public void PolarPlotter_ClearPolarPlots_Does_Not_Reset_Degrees()
        {
            StandardInitialise();
            _Plotter.ClearPolarPlots();

            Assert.AreEqual(5, _Plotter.RoundToDegrees);
        }
        #endregion

        #region LoadFrom
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PolarPlotter_LoadFrom_Throws_If_Passed_Null()
        {
            _Plotter.Initialise(1, 2);
            _Plotter.LoadFrom(null);
        }

        [TestMethod]
        public void PolarPlotter_LoadFrom_Overwrites_RoundToDegrees()
        {
            _Plotter.Initialise(1, 2);
            var roundToDegrees = _Plotter.RoundToDegrees + 1;
            _SavedPolarPlot.RoundToDegrees = roundToDegrees;

            _Plotter.LoadFrom(_SavedPolarPlot);

            Assert.AreEqual(roundToDegrees, _Plotter.RoundToDegrees);
        }

        [TestMethod]
        public void PolarPlotter_LoadFrom_Does_Not_Overwrite_Coordinates()
        {
            _Plotter.Initialise(1, 2);
            _SavedPolarPlot.Latitude = 5.1;
            _SavedPolarPlot.Longitude = 5.2;

            _Plotter.LoadFrom(_SavedPolarPlot);

            Assert.AreEqual(1.0, _Plotter.Latitude);
            Assert.AreEqual(2.0, _Plotter.Longitude);
        }

        [TestMethod]
        public void PolarPlotter_LoadFrom_Replaces_Slices()
        {
            _Plotter.Initialise(1, 2);
            _SavedPolarPlot.PolarPlotSlices.Add(new PolarPlotSlice() {
                AltitudeLower = 1000,
                AltitudeHigher = 2000,
                PolarPlots = {
                    { 20, new PolarPlot() { Altitude = 1100, Angle = 100, Distance = 30, Latitude = 5.1, Longitude = 5.2 } },
                },
            });

            _Plotter.LoadFrom(_SavedPolarPlot);

            var slices = _Plotter.TakeSnapshot();
            Assert.AreEqual(1, slices.Count);
            var slice = slices[0];
            Assert.AreEqual(1000, slice.AltitudeLower);
            Assert.AreEqual(2000, slice.AltitudeHigher);
            Assert.AreEqual(360, slice.PolarPlots.Count);
            var plot = slice.PolarPlots[20];
            Assert.AreEqual(1100, plot.Altitude);
            Assert.AreEqual(100, plot.Angle);
            Assert.AreEqual(30, plot.Distance);
            Assert.AreEqual(5.1, plot.Latitude);
            Assert.AreEqual(5.2, plot.Longitude);
        }
        #endregion
    }
}
