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
        private MockLog _Log;
        private EnvironmentOptions _EnvironmentOptions;
        private StandingDataManagerOptions _StandingDataManagerOptions;
        private IStandingDataManager _Implementation;
        private string _StandingDataSqbFullPath;
        private string _FakeModelCodesFullPath;
        private string _LocalAircraftFullPath;

        [TestInitialize]
        public void TestInitialise()
        {
            _EnvironmentOptions = new() {
                WorkingFolder = Path.GetTempPath(),
            };
            _StandingDataManagerOptions = new();

            _StandingDataSqbFullPath = Path.Combine(_EnvironmentOptions.WorkingFolder, "StandingData.sqb");
            File.WriteAllBytes(_StandingDataSqbFullPath, TestData.StandingData_sqb);

            _FakeModelCodesFullPath = Path.Combine(_EnvironmentOptions.WorkingFolder, "FakeModelCodes.txt");
            _LocalAircraftFullPath = Path.Combine(_EnvironmentOptions.WorkingFolder, "LocalAircraft.txt");

            _FileSystem = new();
            _FileSystem.AddFileContent(Path.Combine(_EnvironmentOptions.WorkingFolder, "StandingData.sqb"), Array.Empty<byte>());
            _FileSystem.AddFileContent(Path.Combine(_EnvironmentOptions.WorkingFolder, "FlightNumberCoverage.csv"), TestData.FlightNumberCoverage_csv);

            _Log = new();

#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)
            using(new CultureSwitcher("en-GB")) {
                _Implementation = new StandingDataManager(
                    new MockOptions<EnvironmentOptions>(_EnvironmentOptions),
                    new MockOptions<StandingDataManagerOptions>(_StandingDataManagerOptions),
                    _FileSystem,
                    _Log
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
        public void FindAircraftType_Can_Find_By_Type_Code_With_Multiple_Models()
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
        public void FindAircraftType_Returns_Correct_Record_For_Fake_Ground_Vehicle_Type()
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
        public void FindAircraftType_Returns_Correct_Record_For_Fake_Radio_Beacon_Type()
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

        [TestMethod]
        public void FindAircraftType_Fake_Ground_Vehicle_Can_Be_Overridden_By_Local_File()
        {
            _FileSystem.AddFileContent(_FakeModelCodesFullPath, new string[] {
                "#Comment",
                "[GroundVehicleCodes]",
                "FAKE1",
                "FAKE2 #Comment",
            });

            _Implementation.Load();

            Assert.AreEqual(Species.GroundVehicle, _Implementation.FindAircraftType("FAKE1").Species);
            Assert.AreEqual("FAKE1", _Implementation.FindAircraftType("FAKE1").Type);
            Assert.AreEqual(Species.GroundVehicle, _Implementation.FindAircraftType("FAKE2").Species);
            Assert.AreEqual(null, _Implementation.FindAircraftType("Fake1"));
            Assert.AreEqual(null, _Implementation.FindAircraftType("FAKE3"));
        }

        [TestMethod]
        public void FindAircraftType_Fake_Ground_Vehicle_Override_Does_Not_Replace_Standard_Code()
        {
            // When the local override was first written the -GND and -TWR codes were not in the main
            // database. That changed when support was added for submitting vehicles and towers with
            // fake codes on the SDM site. We need to show that the local override is in addition to,
            // and not instead of, the standard fake vehicle code

            _FileSystem.AddFileContent(_FakeModelCodesFullPath, new string[] {
                "[GroundVehicleCodes]",
                "FAKE1",
            });

            _Implementation.Load();

            Assert.IsNotNull(_Implementation.FindAircraftType("-GND"));
        }

        [TestMethod]
        public void FindAircraftType_Fake_Tower_Can_Be_Overridden_By_Local_File()
        {
            _FileSystem.AddFileContent(_FakeModelCodesFullPath, new string[] {
                "#Comment",
                "[TowerCodes]",
                "FAKE1",
                "FAKE2 #Comment",
            });

            _Implementation.Load();

            Assert.AreEqual(Species.Tower, _Implementation.FindAircraftType("FAKE1").Species);
            Assert.AreEqual("FAKE1", _Implementation.FindAircraftType("FAKE1").Type);
            Assert.AreEqual(Species.Tower, _Implementation.FindAircraftType("FAKE2").Species);
            Assert.AreEqual(null, _Implementation.FindAircraftType("Fake1"));
            Assert.AreEqual(null, _Implementation.FindAircraftType("FAKE3"));
        }

        [TestMethod]
        public void FindAircraftType_Fake_Tower_Override_Does_Not_Replace_Standard_Code()
        {
            // See notes against ground vehicle version

            _FileSystem.AddFileContent(_FakeModelCodesFullPath, new string[] {
                "[TowerCodes]",
                "FAKE1",
            });

            _Implementation.Load();

            Assert.IsNotNull(_Implementation.FindAircraftType("-TWR"));
        }

        [TestMethod]
        public void FindAirlinesForCode_Returns_Empty_Collection_If_Passed_Null()
        {
            _Implementation.Load();
            Assert.AreEqual(0, _Implementation.FindAirlinesForCode(null).ToList().Count);
        }

        [TestMethod]
        public void FindAirlinesForCode_Returns_Correct_Airline_For_Icao()
        {
            _Implementation.Load();

            var airlines = _Implementation.FindAirlinesForCode("DLH").ToList();
            Assert.AreEqual(1, airlines.Count);
            Assert.AreEqual("LH", airlines[0].IataCode);
            Assert.AreEqual("DLH", airlines[0].IcaoCode);
            Assert.AreEqual("Lufthansa", airlines[0].Name);
        }

        [TestMethod]
        public void FindAirlinesForCode_Returns_Correct_Airline_For_Iata()
        {
            _Implementation.Load();

            var airlines = _Implementation.FindAirlinesForCode("BA").ToList();
            Assert.AreEqual(1, airlines.Count);
            Assert.AreEqual("BA", airlines[0].IataCode);
            Assert.AreEqual("BAW", airlines[0].IcaoCode);
            Assert.AreEqual("British Airways", airlines[0].Name);
        }

        [TestMethod]
        public void FindAirlinesForCode_Returns_Multiple_Airlines()
        {
            _Implementation.Load();

            var airlines = _Implementation.FindAirlinesForCode("LH").ToList();

            Assert.AreEqual(2, airlines.Count);
            Assert.IsTrue(airlines.Any(r => r.IataCode == "LH" && r.IcaoCode == "DLH" && r.Name == "Lufthansa"));
            Assert.IsTrue(airlines.Any(r => r.IataCode == "LH" && r.IcaoCode == "GEC" && r.Name == "Lufthansa Cargo"));
        }

        [TestMethod]
        public void FindCodeBlock_Returns_Null_If_Passed_Null()
        {
            _Implementation.Load();
            Assert.IsNull(_Implementation.FindCodeBlock(null));
        }

        [TestMethod]
        public void FindCodeBlock_Returns_Null_If_Passed_Empty_String()
        {
            _Implementation.Load();
            Assert.IsNull(_Implementation.FindCodeBlock(""));
        }

        [TestMethod]
        public void FindCodeBlock_Returns_Null_If_Passed_Invalid_Icao()
        {
            _Implementation.Load();
            Assert.IsNull(_Implementation.FindCodeBlock("HELLO"));
        }

        [TestMethod]
        public void FindCodeBlock_Returns_Correct_Information_About_Civilian_Aircraft()
        {
            _Implementation.Load();

            var codeBlock = _Implementation.FindCodeBlock("4CA001");

            Assert.AreEqual("Ireland", codeBlock.Country);
            Assert.AreEqual(false, codeBlock.IsMilitary);
        }

        [TestMethod]
        public void FindCodeBlock_Returns_Correct_Information_About_Military_Aircraft()
        {
            _Implementation.Load();

            var codeBlock = _Implementation.FindCodeBlock("43C001");

            Assert.AreEqual("United Kingdom", codeBlock.Country);
            Assert.AreEqual(true, codeBlock.IsMilitary);
        }

        [TestMethod]
        public void FindCodeBlock_Returns_Most_Appropriate_Entry()
        {
            _Implementation.Load();

            var codeBlock1 = _Implementation.FindCodeBlock("ADF7C7");
            var codeBlock2 = _Implementation.FindCodeBlock("ADF7C8");

            Assert.AreEqual("United States", codeBlock1.Country);
            Assert.AreEqual(false, codeBlock1.IsMilitary);

            Assert.AreEqual("United States", codeBlock2.Country);
            Assert.AreEqual(true, codeBlock2.IsMilitary);
        }

        [TestMethod]
        public void FindCodeBlock_Works_With_Real_Life_Examples()
        {
            var foo = new SpreadsheetTestData(TestData.StandingDataTests, "CodeBlockRepository");
            foo.TestEveryRow(this, row => {
                _Implementation.Load();

                var codeBlock = _Implementation.FindCodeBlock(row.EString("ICAO24"));
                if(row.String("IsMilitary") == null) {
                    Assert.IsNull(codeBlock);
                } else {
                    Assert.AreEqual(row.EString("Country"), codeBlock.Country);
                    Assert.AreEqual(row.Bool("IsMilitary"), codeBlock.IsMilitary);
                }
            });
        }

        [TestMethod]
        public void FindCodeBlock_Can_Be_Overridden_By_Local_CodeBlock_File()
        {
            _FileSystem.AddFileContent(_LocalAircraftFullPath, new string[] {
                "[Made-up Country]",
                "ADF7C7 Mil",
                "ADF7C8\tCiv",
            });

            _Implementation.Load();

            var codeBlock1 = _Implementation.FindCodeBlock("ADF7C7");
            var codeBlock2 = _Implementation.FindCodeBlock("ADF7C8");

            Assert.AreEqual("Made-up Country", codeBlock1.Country);
            Assert.AreEqual(true, codeBlock1.IsMilitary);

            Assert.AreEqual("Made-up Country", codeBlock2.Country);
            Assert.AreEqual(false, codeBlock2.IsMilitary);

            Assert.AreEqual(0, _Log.Output.Count);
        }

        [TestMethod]
        public void FindCodeBlock_Local_CodeBlock_Override_Uses_Correct_Significant_Bitmask()
        {
            _FileSystem.AddFileContent(_LocalAircraftFullPath, new string[] {
                "[Hello]",
                "4CA000 Mil",
            });
            
            _Implementation.Load();

            var codeBlock = _Implementation.FindCodeBlock("4CA001");

            Assert.AreEqual("Ireland", codeBlock.Country);
            Assert.AreEqual(false, codeBlock.IsMilitary);
        }

        [TestMethod]
        public void FindCodeBlock_Override_Ignores_Entries_With_No_Country()
        {
            foreach(var firstLine in new string[] { "[Start but no finish", "Finish but no start]", "Gibberish", "", "   " }) {
                TestInitialise();

                _FileSystem.AddFileContent(_LocalAircraftFullPath, new string[] {
                    firstLine,
                    "ADF7C7 Mil",
                });

                _Implementation.Load();

                var codeBlock = _Implementation.FindCodeBlock("ADF7C7");
                Assert.AreEqual(false, codeBlock.IsMilitary, $"First line is \"{firstLine}\"");
                Assert.IsTrue(_Log.Output.Count >= 1, $"First line is \"{firstLine}\"");
            }
        }

        [TestMethod]
        public void FindCodeBlock_Override_Ignores_Entries_With_Bad_ICAO_Lines()
        {
            foreach(var icaoLine in new string[] { "NOTHEX Mil", "ADF7C7", "ADF7C7 Bla" }) {
                TestInitialise();

                _FileSystem.AddFileContent(_LocalAircraftFullPath, new string[] {
                    "[Country]",
                    icaoLine,
                });

                _Implementation.Load();

                var codeBlock = _Implementation.FindCodeBlock("ADF7C7");
                Assert.AreNotEqual("Country", codeBlock.Country, $"ICAO line is \"{icaoLine}\"");
                Assert.IsTrue(_Log.Output.Count >= 1, $"ICAO line is \"{icaoLine}\"");
            }
        }

        [TestMethod]
        public void FindCodeBlock_Override_Ignores_Lines_That_Start_With_Hash_Symbol()
        {
            _FileSystem.AddFileContent(_LocalAircraftFullPath, new string[] {
                "[Country]",
                "#[Different Country]",
                "AABBCC CIV # Gibberish",
            });

            _Implementation.Load();

            var codeBlock = _Implementation.FindCodeBlock("AABBCC");
            Assert.AreEqual("Country", codeBlock.Country);
            Assert.AreEqual(false, codeBlock.IsMilitary);
            Assert.AreEqual(0, _Log.Output.Count);
        }

        [TestMethod]
        public void FindAirportForCode_Returns_Null_If_Passed_Null()
        {
            _Implementation.Load();
            Assert.IsNull(_Implementation.FindAirportForCode(null));
        }

        [TestMethod]
        public void FindAirportForCode_Returns_Null_If_Passed_Unknown_Code()
        {
            _Implementation.Load();
            Assert.IsNull(_Implementation.FindAirportForCode("999"));
        }

        [TestMethod]
        public void FindAirportForCode_Returns_Airport_When_Passed_Known_Icao()
        {
            _Implementation.Load();
            var airport = _Implementation.FindAirportForCode("KJFK");

            Assert.AreEqual("KJFK",             airport.IcaoCode);
            Assert.AreEqual("JFK",              airport.IataCode);
            Assert.AreEqual("John F Kennedy",   airport.Name);
            Assert.AreEqual("United States",    airport.Country);
            Assert.AreEqual(40.639751,          (double)airport.Latitude,  0.001);
            Assert.AreEqual(-73.778925,         (double)airport.Longitude, 0.001);
            Assert.AreEqual(13,                 airport.AltitudeFeet);
        }

        [TestMethod]
        public void FindAirportForCode_Returns_Airport_When_Passed_Known_Iata()
        {
            _Implementation.Load();
            var airport = _Implementation.FindAirportForCode("JFK");

            Assert.AreEqual("KJFK",             airport.IcaoCode);
            Assert.AreEqual("JFK",              airport.IataCode);
            Assert.AreEqual("John F Kennedy",   airport.Name);
            Assert.AreEqual("United States",    airport.Country);
            Assert.AreEqual(40.639751,          (double)airport.Latitude,  0.001);
            Assert.AreEqual(-73.778925,         (double)airport.Longitude, 0.001);
            Assert.AreEqual(13,                 airport.AltitudeFeet);
        }
    }
}
