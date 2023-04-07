// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;
using System.Text.RegularExpressions;
using Moq;
using Test.Framework;
using VirtualRadar.Database.SQLite.StandingData;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Database.SQLite
{
    [TestClass]
    public class StandingDataManager_Tests
    {
        private MockFileSystem _FileSystem;
        private Mock<ILog> _Log;
        private EnvironmentOptions _EnvironmentOptions;
        private StandingDataManagerOptions _StandingDataManagerOptions;
        private IStandingDataManager _Implementation;
        private string _StandingDataSqbFullPath;

        [TestInitialize]
        public void TestInitialise()
        {
            _EnvironmentOptions = new() {
                WorkingFolder = Path.GetTempPath(),
            };
            _StandingDataManagerOptions = new();

            _StandingDataSqbFullPath = Path.Combine(_EnvironmentOptions.WorkingFolder, "StandingData.sqb");
            File.WriteAllBytes(_StandingDataSqbFullPath, TestData.StandingData_sqb);

            _FileSystem = new();
            _FileSystem.AddFileContent(Path.Combine(_EnvironmentOptions.WorkingFolder, "StandingData.sqb"), Array.Empty<byte>());
            _FileSystem.AddFileContent(Path.Combine(_EnvironmentOptions.WorkingFolder, "FlightNumberCoverage.csv"), TestData.FlightNumberCoverage_csv);

            _Log = MockHelper.CreateMock<ILog>();

#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)
            using(new CultureSwitcher("en-GB")) {
                _Implementation = new StandingDataManager(
                    new MockOptions<EnvironmentOptions>(_EnvironmentOptions),
                    new MockOptions<StandingDataManagerOptions>(_StandingDataManagerOptions),
                    _FileSystem,
                    _Log.Object
                );
            }
        }

        [TestMethod]
        public void RouteStatus_Initialises_Correctly()
        {
            Assert.AreEqual("Not loaded", _Implementation.RouteStatus);
            Assert.AreEqual(false, _Implementation.CodeBlocksLoaded);
        }

        [TestMethod]
        [DataRow("StandingData.sqb")]
        [DataRow("FlightNumberCoverage.csv")]
        public void RouteStatus_Reports_Missing_Files_After_Load(string fileName)
        {
            _FileSystem.RemoveFile(Path.Combine(_EnvironmentOptions.WorkingFolder, fileName));

            using(new CultureSwitcher("en-GB")) {
                _Implementation.Load();
            }

            Assert.AreEqual("Some route files are missing", _Implementation.RouteStatus);
            Assert.AreEqual(false, _Implementation.CodeBlocksLoaded);
        }

        [TestMethod]
        public void RouteStatus_Set_To_Content_Of_State_File_After_Load()
        {
            using(new CultureSwitcher("en-GB")) {
                _Implementation.Load();
            }

            var isMatch = Regex.IsMatch(_Implementation.RouteStatus, @"Loaded .* routes for the period .* to .*");
            Assert.IsTrue(isMatch);
            Assert.IsTrue(_Implementation.CodeBlocksLoaded);
        }

        [TestMethod]
        public void Load_Raises_LoadCompleted()
        {
            var eventRecorder = new EventRecorder<EventArgs>();
            _Implementation.LoadCompleted += eventRecorder.Handler;

            eventRecorder.EventRaised += (sender, args) => {
                Assert.AreEqual(true, _Implementation.CodeBlocksLoaded);
            };

            _Implementation.Load();

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreEqual(_Implementation, eventRecorder.Sender);
            Assert.IsNotNull(eventRecorder.Args);
        }

        [TestMethod]
        public void FindRoute_Returns_Null_If_Passed_Null()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindRoute(null));
        }

        [TestMethod]
        public void FindRoute_Returns_Null_If_Passed_Empty_String()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindRoute(""));
        }

        [TestMethod]
        public void FindRoute_Returns_Correct_Information_For_Simple_Flight_Number()
        {
            _Implementation.Load();

            var route = _Implementation.FindRoute("DLH400");
            Assert.AreEqual(0, route.Stopovers.Count);

            Assert.IsFalse(String.IsNullOrWhiteSpace(route.From.Name));
            Assert.IsFalse(String.IsNullOrWhiteSpace(route.From.Country));
            Assert.AreEqual("FRA", route.From.IataCode);
            Assert.AreEqual("EDDF", route.From.IcaoCode);
            Assert.AreEqual(50.026, route.From.Latitude.Value, 0.001);
            Assert.AreEqual(8.543, route.From.Longitude.Value, 0.001);
            Assert.AreEqual(364, route.From.AltitudeFeet);

            Assert.IsFalse(String.IsNullOrWhiteSpace(route.To.Name));
            Assert.IsFalse(String.IsNullOrWhiteSpace(route.To.Country));
            Assert.AreEqual("JFK", route.To.IataCode);
            Assert.AreEqual("KJFK", route.To.IcaoCode);
            Assert.AreEqual(40.639, route.To.Latitude.Value, 0.001);
            Assert.AreEqual(-73.778, route.To.Longitude.Value, 0.001);
            Assert.AreEqual(13, route.To.AltitudeFeet);
        }

        [TestMethod]
        public void FindRoute_Returns_Stopovers()
        {
            _Implementation.Load();

            var route = _Implementation.FindRoute("DLH8208");
            Assert.AreEqual(1, route.Stopovers.Count);

            Assert.AreEqual("EDDF", route.From.IcaoCode);
            Assert.AreEqual("KORD", route.Stopovers[0].IcaoCode);
            Assert.AreEqual("KATL", route.To.IcaoCode);
        }

        [TestMethod]
        public void FindAircraftType_Returns_Null_If_Passed_Null()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindAircraftType(null));
        }

        [TestMethod]
        public void FindAircraftType_Returns_Null_If_Passed_Empty_String()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindAircraftType(""));
        }

        [TestMethod]
        public void FindAircraftType_Returns_Null_If_Passed_Unknown_Type_Code()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindAircraftType("UNKN"));
        }

        [TestMethod]
        public void FindAircraftType_Can_Find_By_Type_Code_With_Single_Model()
        {
            _Implementation.Load();

            var type = _Implementation.FindAircraftType("A30B");
            Assert.AreEqual(1, type.Models.Count);
            Assert.AreEqual(1, type.Manufacturers.Count);
            Assert.AreEqual("Airbus", type.Manufacturers[0], ignoreCase: true);
            Assert.AreEqual("A-300B2", type.Models[0]);
            Assert.AreEqual("A30B", type.Type);
            Assert.AreEqual(WakeTurbulenceCategory.Heavy, type.WakeTurbulenceCategory);
            Assert.AreEqual(Species.Landplane, type.Species);
            Assert.AreEqual(EngineType.Jet, type.EngineType);
            Assert.AreEqual("2", type.Engines);
        }

        [TestMethod]
        public void StandingDataManager_FindAircraftType_Can_Find_By_Type_Code_With_Multiple_Models()
        {
            _Implementation.Load();

            var type = _Implementation.FindAircraftType("D11");
            Assert.AreEqual(2, type.Models.Count);
            Assert.AreEqual(2, type.Manufacturers.Count);
            Assert.AreEqual("FALCONAR", type.Manufacturers[0], ignoreCase: true);
            Assert.AreEqual("FALCONAR", type.Manufacturers[1], ignoreCase: true);
            Assert.IsTrue(type.Models.Contains("Cruiser"));
            Assert.IsTrue(type.Models.Contains("F-11"));
            Assert.AreEqual("D11", type.Type);
            Assert.AreEqual(WakeTurbulenceCategory.Light, type.WakeTurbulenceCategory);
            Assert.AreEqual(Species.Landplane, type.Species);
            Assert.AreEqual(EngineType.Piston, type.EngineType);
            Assert.AreEqual("1", type.Engines);
        }

        [TestMethod]
        public void StandingDataManager_FindAircraftType_Returns_Correct_Record_For_Fake_Ground_Vehicle_Type()
        {
            _Implementation.Load();

            var type = _Implementation.FindAircraftType("-GND");
            Assert.IsNotNull(type);
            Assert.AreEqual(0, type.Models.Count);
            Assert.AreEqual(0, type.Manufacturers.Count);
            Assert.AreEqual("-GND", type.Type);
            Assert.AreEqual(WakeTurbulenceCategory.None, type.WakeTurbulenceCategory);
            Assert.AreEqual(Species.GroundVehicle, type.Species);
            Assert.AreEqual(EngineType.None, type.EngineType);
            Assert.AreEqual(null, type.Engines);
        }

        [TestMethod]
        public void StandingDataManager_FindAircraftType_Returns_Correct_Record_For_Fake_Radio_Beacon_Type()
        {
            _Implementation.Load();

            var type = _Implementation.FindAircraftType("-TWR");
            Assert.IsNotNull(type);
            Assert.AreEqual(0, type.Models.Count);
            Assert.AreEqual(0, type.Manufacturers.Count);
            Assert.AreEqual("-TWR", type.Type);
            Assert.AreEqual(WakeTurbulenceCategory.None, type.WakeTurbulenceCategory);
            Assert.AreEqual(Species.Tower, type.Species);
            Assert.AreEqual(EngineType.None, type.EngineType);
            Assert.AreEqual(null, type.Engines);
        }
    }
}
