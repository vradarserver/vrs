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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Database;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class StandingDataManagerTests
    {
        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private const string DatabaseFileName = "STANDINGDATA.SQB";
        private const string FlightNumberCoverageFileName = "FLIGHTNUMBERCOVERAGE.CSV";
        private readonly string[] AllFileNames = new string[] {
            DatabaseFileName,
            FlightNumberCoverageFileName,
        };

        private IStandingDataManager _Implementation;
        private IClassFactory _ClassFactorySnapshot;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Configuration _Configuration;
        private Mock<IStandingDataManagerProvider> _Provider;
        private Dictionary<string, bool> _FileExists;
        private Dictionary<string, List<string>> _ReadAllLines;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private Mock<ILog> _Log;
        private List<string> _LogLines;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsMono).Returns(false);

            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _LogLines = new List<string>();
            _Log.Setup(r => r.WriteLine(It.IsAny<string>())).Callback((string line) => { _LogLines.Add(line); });
            _Log.Setup(r => r.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback((string format, object[] args) => { _LogLines.Add(String.Format(format, args)); });

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(r => r.Load()).Returns(_Configuration);
            _ConfigurationStorage.Setup(r => r.Folder).Returns(Path.Combine(TestContext.TestDeploymentDir, "StandingDataTest"));

            _Implementation = Factory.ResolveNewInstance<IStandingDataManager>();

            _Provider = new Mock<IStandingDataManagerProvider>();
            _Implementation.Provider = _Provider.Object;

            _FileExists = new Dictionary<string,bool>();
            _FileExists.Add(DatabaseFileName, true);
            _FileExists.Add(FlightNumberCoverageFileName, true);
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns((string fileName) => {
                var key = String.IsNullOrEmpty(fileName) ? null : Path.GetFileName(fileName).ToUpperInvariant();
                return key != null && _FileExists.ContainsKey(key) ? _FileExists[key] : false;
            });

            _ReadAllLines = new Dictionary<string,List<string>>();
            _ReadAllLines.Add(FlightNumberCoverageFileName, new List<string>());
            _Provider.Setup(p => p.ReadAllLines(It.IsAny<string>())).Returns((string fileName) => {
                return _ReadAllLines[Path.GetFileName(fileName).ToUpperInvariant()].ToArray();
            });

            _ReadAllLines[FlightNumberCoverageFileName].Add("A,B,C,D,E");
            _ReadAllLines[FlightNumberCoverageFileName].Add("2001-02-01,2002-12-31,1000,X,X");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void StandingDataManager_Constructor_Initialises_Provider()
        {
            var implementation = Factory.ResolveNewInstance<IStandingDataManager>();
            Assert.IsNotNull(implementation.Provider);
            TestUtilities.TestProperty(implementation, "Provider", implementation.Provider, _Provider.Object);
        }

        [TestMethod]
        public void StandingDataManager_Constructor_Exposes_Lock_Object()
        {
            Assert.IsNotNull(_Implementation.Lock);
        }
        #endregion

        #region Load / RouteStatus
        private const string RouteFilesMissingMessage = "Some route files are missing";
        [TestMethod]
        public void StandingDataManager_RouteStatus_Initialises_Correctly()
        {
            Assert.AreEqual("Not loaded", _Implementation.RouteStatus);
            Assert.AreEqual(false, _Implementation.CodeBlocksLoaded);
        }

        [TestMethod]
        public void StandingDataManager_RouteStatus_Reports_Missing_Files_After_Load()
        {
            CheckFileMissingRouteStatusMessage("STANDINGDATA.SQB");
            CheckFileMissingRouteStatusMessage("FLIGHTNUMBERCOVERAGE.CSV");

            foreach(var fileName in AllFileNames) {
                _FileExists[fileName] = true;
            }

            _Implementation.Load();
            Assert.AreNotEqual(RouteFilesMissingMessage, _Implementation.RouteStatus);
            Assert.AreEqual(true, _Implementation.CodeBlocksLoaded);
        }

        private void CheckFileMissingRouteStatusMessage(string missingFile)
        {
            missingFile = missingFile.ToUpperInvariant();
            foreach(var fileName in AllFileNames) {
                _FileExists[fileName] = fileName != missingFile;
            }

            _Implementation.Load();
            Assert.AreEqual(RouteFilesMissingMessage, _Implementation.RouteStatus);
            Assert.AreEqual(false, _Implementation.CodeBlocksLoaded);
        }

        [TestMethod]
        public void StandingDataManager_RouteStatus_Set_To_Content_Of_State_File_After_Load()
        {
            _Implementation.Load();

            var expectedMessage = String.Format("Loaded {0} routes for the period 01/02/2001 to 31/12/2002", 1000.ToString("N0"));
            Assert.AreEqual(expectedMessage, _Implementation.RouteStatus);
            Assert.AreEqual(true, _Implementation.CodeBlocksLoaded);
        }

        [TestMethod]
        public void StandingDataManager_Load_Raises_LoadCompleted()
        {
            var eventRecorder = new EventRecorder<EventArgs>();
            _Implementation.LoadCompleted += eventRecorder.Handler;

            var expectedMessage = String.Format("Loaded {0} routes for the period 01/02/2001 to 31/12/2002", 1000.ToString("N0"));
            eventRecorder.EventRaised += (sender, args) => {
                Assert.AreEqual(expectedMessage, _Implementation.RouteStatus);
                Assert.AreEqual(true, _Implementation.CodeBlocksLoaded);
            };

            _Implementation.Load();
            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreEqual(_Implementation, eventRecorder.Sender);
            Assert.IsNotNull(eventRecorder.Args);
        }
        #endregion

        #region FindRoute
        [TestMethod]
        public void StandingDataManager_FindRoute_Returns_Null_If_Passed_Null()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindRoute(null));
        }

        [TestMethod]
        public void StandingDataManager_FindRoute_Returns_Null_If_Passed_Empty_String()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindRoute(""));
        }

        [TestMethod]
        public void StandingDataManager_FindRoute_Returns_Correct_Information_For_Simple_Flight_Number()
        {
            _Implementation.Load();

            var route = _Implementation.FindRoute("VS1");
            Assert.AreEqual(0, route.Stopovers.Count);

            Assert.AreEqual("Heathrow, London", route.From.Name);
            Assert.AreEqual("United Kingdom", route.From.Country);
            Assert.AreEqual("LHR", route.From.IataCode);
            Assert.AreEqual("EGLL", route.From.IcaoCode);
            Assert.AreEqual(51.4775, route.From.Latitude);
            Assert.AreEqual(-0.461389, route.From.Longitude);
            Assert.AreEqual(83, route.From.AltitudeFeet);

            Assert.AreEqual("Newark Liberty Intl", route.To.Name);
            Assert.AreEqual("United States", route.To.Country);
            Assert.AreEqual("EWR", route.To.IataCode);
            Assert.AreEqual("KEWR", route.To.IcaoCode);
            Assert.AreEqual(40.6925, route.To.Latitude);
            Assert.AreEqual(-74.168667, route.To.Longitude);
            Assert.AreEqual(18, route.To.AltitudeFeet);
        }

        [TestMethod]
        public void StandingDataManager_FindRoute_Returns_Stopovers()
        {
            _Implementation.Load();

            var route = _Implementation.FindRoute("BA124");
            Assert.AreEqual(2, route.Stopovers.Count);

            Assert.AreEqual("OBBI", route.From.IcaoCode);
            Assert.AreEqual("EGLL", route.Stopovers[0].IcaoCode);
            Assert.AreEqual("OTBD", route.Stopovers[1].IcaoCode);
            Assert.AreEqual("OBBI", route.To.IcaoCode);

            Assert.AreEqual("Doha Intl", route.Stopovers[1].Name);
            Assert.AreEqual("Qatar", route.Stopovers[1].Country);
            Assert.AreEqual("DOH", route.Stopovers[1].IataCode);
            Assert.AreEqual(25.261125, route.Stopovers[1].Latitude);
            Assert.AreEqual(51.565056, route.Stopovers[1].Longitude);
            Assert.AreEqual(35, route.Stopovers[1].AltitudeFeet);
        }
        #endregion

        #region FindAircraftType
        [TestMethod]
        public void StandingDataManager_FindAircraftType_Returns_Null_If_Passed_Null()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindAircraftType(null));
        }

        [TestMethod]
        public void StandingDataManager_FindAircraftType_Returns_Null_If_Passed_Empty_String()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindAircraftType(""));
        }

        [TestMethod]
        public void StandingDataManager_FindAircraftType_Returns_Null_If_Passed_Unknown_Type_Code()
        {
            _Implementation.Load();
            Assert.AreEqual(null, _Implementation.FindAircraftType("UN"));
        }

        [TestMethod]
        public void StandingDataManager_FindAircraftType_Can_Find_By_Type_Code_With_Single_Model()
        {
            _Implementation.Load();

            var type = _Implementation.FindAircraftType("A30B");
            Assert.AreEqual(1, type.Models.Count);
            Assert.AreEqual(1, type.Manufacturers.Count);
            Assert.AreEqual("AIRBUS", type.Manufacturers[0]);
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
            Assert.AreEqual("FALCONAR", type.Manufacturers[0]);
            Assert.AreEqual("FALCONAR", type.Manufacturers[1]);
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

        [TestMethod]
        [DataSource("Data Source='DataTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftTypesCsv$")]
        public void StandingDataManager_FindAircraftType_Returns_Correct_Values()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);

            _Implementation.Load();

            var searchFor = worksheet.String("SearchFor");
            var aircraftType = _Implementation.FindAircraftType(searchFor);

            if(worksheet.Bool("IsNull")) Assert.AreEqual(null, aircraftType);
            else {
                List<string> manufacturers = new List<string>();
                List<string> models = new List<string>();
                BuildExpectedList(worksheet, manufacturers, "ManOut1");
                BuildExpectedList(worksheet, manufacturers, "ManOut2");
                BuildExpectedList(worksheet, models, "ModelOut1");
                BuildExpectedList(worksheet, models, "ModelOut2");

                Assert.AreEqual(manufacturers.Count, aircraftType.Manufacturers.Count);
                foreach(string manufacturer in manufacturers) Assert.IsTrue(aircraftType.Manufacturers.Contains(manufacturer));

                Assert.AreEqual(models.Count, aircraftType.Models.Count);
                foreach(string model in models) Assert.IsTrue(aircraftType.Models.Contains(model));

                Assert.AreEqual(worksheet.String("Type"), aircraftType.Type);
                Assert.AreEqual(worksheet.ParseEnum<WakeTurbulenceCategory>("Wake"), aircraftType.WakeTurbulenceCategory);
                Assert.AreEqual(worksheet.ParseEnum<EngineType>("EngineType"), aircraftType.EngineType);
                Assert.AreEqual(worksheet.String("Engines"), aircraftType.Engines);
                Assert.AreEqual(worksheet.ParseEnum<Species>("Species"), aircraftType.Species);
            }
        }

        private void BuildExpectedList(ExcelWorksheetData worksheet, List<string> list, string column)
        {
            string value = worksheet.EString(column);
            if(value != null) list.Add(value);
        }
        #endregion

        #region FindAircraftType and local overrides
        private string ConfigureForLocalOverrideOfModelCodes()
        {
            var folder = "ConfigurationFolder";
            var fileName = "FakeModelCodes.txt";
            var expectedPath = Path.Combine(folder, fileName);
            _ConfigurationStorage.Setup(r => r.Folder).Returns(folder);
            _Provider.Setup(r => r.FileExists(expectedPath)).Returns(true);

            return expectedPath;
        }

        [TestMethod]
        public void StandingDataManager_FindAircraftType_Fake_Ground_Vehicle_Can_Be_Overridden_By_Local_File()
        {
            var expectedPath = ConfigureForLocalOverrideOfModelCodes();
            _Provider.Setup(r => r.ReadAllLines(expectedPath)).Returns(new string[] {
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
            Assert.AreEqual(null, _Implementation.FindAircraftType("-GND"));
        }

        [TestMethod]
        public void StandingDataManager_FindAircraftType_Fake_Tower_Can_Be_Overridden_By_Local_File()
        {
            var expectedPath = ConfigureForLocalOverrideOfModelCodes();
            _Provider.Setup(r => r.ReadAllLines(expectedPath)).Returns(new string[] {
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
            Assert.AreEqual(null, _Implementation.FindAircraftType("-TWR"));
        }
        #endregion

        #region FindAirlinesForCode
        [TestMethod]
        public void StandingDataManager_FindAirlinesForCode_Returns_Empty_Collection_If_Passed_Null()
        {
            _Implementation.Load();
            Assert.AreEqual(0, _Implementation.FindAirlinesForCode(null).ToList().Count);
        }

        [TestMethod]
        public void StandingDataManager_FindAirlinesForCode_Returns_Correct_Airline_For_Icao()
        {
            _Implementation.Load();

            var airlines = _Implementation.FindAirlinesForCode("VIR").ToList();
            Assert.AreEqual(1, airlines.Count);
            Assert.AreEqual("VS", airlines[0].IataCode);
            Assert.AreEqual("VIR", airlines[0].IcaoCode);
            Assert.AreEqual("Virgin Atlantic Airways", airlines[0].Name);
        }

        [TestMethod]
        public void StandingDataManager_FindAirlinesForCode_Returns_Correct_Airline_For_Iata()
        {
            _Implementation.Load();

            var airlines = _Implementation.FindAirlinesForCode("TV").ToList();
            Assert.AreEqual(1, airlines.Count);
            Assert.AreEqual("TV", airlines[0].IataCode);
            Assert.AreEqual("VEX", airlines[0].IcaoCode);
            Assert.AreEqual("Virgin Express", airlines[0].Name);
        }

        [TestMethod]
        public void StandingDataManager_FindAirlinesForCode_Returns_Multiple_Airlines()
        {
            _Implementation.Load();

            var airlines = _Implementation.FindAirlinesForCode("LH").ToList();
            Assert.AreEqual(2, airlines.Count);
            Assert.IsTrue(airlines.Any(r => r.IataCode == "LH" && r.IcaoCode == "DLH" && r.Name == "Lufthansa"));
            Assert.IsTrue(airlines.Any(r => r.IataCode == "LH" && r.IcaoCode == "GEC" && r.Name == "Lufthansa Cargo"));
        }
        #endregion

        #region FindCodeBlock
        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Returns_Null_If_Passed_Null()
        {
            _Implementation.Load();
            Assert.IsNull(_Implementation.FindCodeBlock(null));
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Returns_Null_If_Passed_Empty_String()
        {
            _Implementation.Load();
            Assert.IsNull(_Implementation.FindCodeBlock(""));
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Returns_Null_If_Passed_Invalid_Icao()
        {
            _Implementation.Load();
            Assert.IsNull(_Implementation.FindCodeBlock("HELLO"));
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Does_Not_Care_If_File_Does_Not_Exist()
        {
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(false);
            _Implementation.Load();

            Assert.AreEqual(null, _Implementation.FindCodeBlock("123456"));
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Returns_Correct_Information_About_Civilian_Aircraft()
        {
            _Implementation.Load();

            var codeBlock = _Implementation.FindCodeBlock("4CA001");

            Assert.AreEqual("Ireland", codeBlock.Country);
            Assert.AreEqual(false, codeBlock.IsMilitary);
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Returns_Correct_Information_About_Military_Aircraft()
        {
            _Implementation.Load();

            var codeBlock = _Implementation.FindCodeBlock("43C001");

            Assert.AreEqual("United Kingdom", codeBlock.Country);
            Assert.AreEqual(true, codeBlock.IsMilitary);
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Returns_Most_Appropriate_Entry()
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
        [DataSource("Data Source='DataTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "CodeBlockRepository$")]
        public void StandingDataManager_FindCodeBlock_Works_With_Real_Life_Examples()
        {
            ExcelWorksheetData worksheet = new ExcelWorksheetData(TestContext);

            _Implementation.Load();

            var codeBlock = _Implementation.FindCodeBlock(worksheet.EString("ICAO24"));
            if(worksheet.String("IsMilitary") == null) Assert.IsNull(codeBlock);
            else {
                Assert.AreEqual(worksheet.EString("Country"), codeBlock.Country);
                Assert.AreEqual(worksheet.Bool("IsMilitary"), codeBlock.IsMilitary);
            }
        }
        #endregion

        #region FindCodeBlock and local overrides
        private string ConfigureForLocalOverride()
        {
            var folder = "ConfigurationFolder";
            var fileName = "LocalAircraft.txt";
            var expectedPath = Path.Combine(folder, fileName);
            _ConfigurationStorage.Setup(r => r.Folder).Returns(folder);
            _Provider.Setup(r => r.FileExists(expectedPath)).Returns(true);

            return expectedPath;
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Can_Be_Overridden_By_Local_CodeBlock_File()
        {
            var expectedPath = ConfigureForLocalOverride();
            _Provider.Setup(r => r.ReadAllLines(expectedPath)).Returns(new string[] {
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

            Assert.AreEqual(0, _LogLines.Count);
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Local_CodeBlock_Override_Uses_Correct_Significant_Bitmask()
        {
            var expectedPath = ConfigureForLocalOverride();
            _Provider.Setup(r => r.ReadAllLines(expectedPath)).Returns(new string[] {
                "[Hello]",
                "4CA000 Mil",
            });
            
            _Implementation.Load();

            var codeBlock = _Implementation.FindCodeBlock("4CA001");

            Assert.AreEqual("Ireland", codeBlock.Country);
            Assert.AreEqual(false, codeBlock.IsMilitary);
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Override_Ignores_Entries_With_No_Country()
        {
            foreach(var firstLine in new string[] { "[Start but no finish", "Finish but no start]", "Gibberish", "", "   " }) {
                TestCleanup();
                TestInitialise();

                var expectedPath = ConfigureForLocalOverride();
                _Provider.Setup(r => r.ReadAllLines(expectedPath)).Returns(new string[] {
                    firstLine,
                    "ADF7C7 Mil",
                });

                _Implementation.Load();

                var codeBlock = _Implementation.FindCodeBlock("ADF7C7");
                Assert.AreEqual(false, codeBlock.IsMilitary, "First line is \"{0}\"", firstLine);
                Assert.IsTrue(_LogLines.Count >= 1, "First line is \"{0}\"", firstLine);
            }
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Override_Ignores_Entries_With_Bad_ICAO_Lines()
        {
            foreach(var icaoLine in new string[] { "NOTHEX Mil", "ADF7C7", "ADF7C7 Bla" }) {
                TestCleanup();
                TestInitialise();

                var expectedPath = ConfigureForLocalOverride();
                _Provider.Setup(r => r.ReadAllLines(expectedPath)).Returns(new string[] {
                    "[Country]",
                    icaoLine,
                });

                _Implementation.Load();

                var codeBlock = _Implementation.FindCodeBlock("ADF7C7");
                Assert.AreNotEqual("Country", codeBlock.Country, "ICAO line is \"{0}\"", icaoLine);
                Assert.IsTrue(_LogLines.Count >= 1, "ICAO line is \"{0}\"", icaoLine);
            }
        }

        [TestMethod]
        public void StandingDataManager_FindCodeBlock_Override_Ignores_Lines_That_Start_With_Hash_Symbol()
        {
            var expectedPath = ConfigureForLocalOverride();
            _Provider.Setup(r => r.ReadAllLines(expectedPath)).Returns(new String[] {
                "[Country]",
                "#[Different Country]",
                "AABBCC CIV # Gibberish",
            });

            _Implementation.Load();

            var codeBlock = _Implementation.FindCodeBlock("AABBCC");
            Assert.AreEqual("Country", codeBlock.Country);
            Assert.AreEqual(false, codeBlock.IsMilitary);
            Assert.AreEqual(0, _LogLines.Count);
        }
        #endregion
    }
}
