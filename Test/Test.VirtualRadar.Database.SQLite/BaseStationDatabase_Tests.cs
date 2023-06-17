using System.Data;
using System.IO;
using Test.Framework;
using VirtualRadar.Database.SQLite.KineticData;
using VirtualRadar.Interface;
using VirtualRadar.Interface.KineticData;
using VirtualRadar.Interface.Options;

namespace Test.VirtualRadar.Database.SQLite
{
    [TestClass]
    public class BaseStationDatabase_Tests : CommonBaseStationDatabaseTests
    {
        protected override string _SchemaPrefix => "";

        protected override bool _EngineTruncatesMilliseconds => false;

        protected override string _SqlReturnNewIdentity => "SELECT last_insert_rowid();";

        private MockFileSystem _FileSystem;
        private IBaseStationDatabaseSQLite _SqliteImplementation;
        private string _BaseStationSqbFullPath;

        [TestInitialize]
        public void TestInitialise()
        {
            base.CommonTestInitialise();

             _CreateConnection = () => CreateSqliteConnection(null);

            var randomFileNameElement = Guid.NewGuid().ToString();      // <-- need this to stop the O/S knacking tests while it deletes files in the background etc.
            var baseStationSqbFileName = $"BaseStation-{randomFileNameElement}.sqb";
            _BaseStationSqbFullPath = Path.Combine(_EnvironmentOptions.WorkingFolder, baseStationSqbFileName);
            _Configuration.BaseStationSettings.DatabaseFileName = _BaseStationSqbFullPath;
            File.WriteAllBytes(_BaseStationSqbFullPath, TestData.BaseStation_sqb);

            _FileSystem = new();
            _FileSystem.AddFileContent(_BaseStationSqbFullPath, new byte[] { 0 });

            // Uncomment this to see Entity Framework traces in the debug console:
            //_BaseStationDatabaseOptions.ShowDatabaseDiagnosticsInDebugConsole = true;

#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)
            using(new CultureSwitcher("en-GB")) {
                _SqliteImplementation = new BaseStationDatabase(
                    _FileSystem,
                    _SharedConfiguration.Object,
                    _Clock.Object,
                    _StandingData.Object,
                    _CallsignParser.Object,
                    new MockOptions<BaseStationDatabaseOptions>(_BaseStationDatabaseOptions)
                );
                _Implementation = _SqliteImplementation;
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _Implementation.Dispose();
            DeleteTestFile();
        }

        private void DeleteTestFile()
        {
            try {
                _FileSystem.DeleteFile(_BaseStationSqbFullPath);
            } catch(IOException) {
                // You might get this if it's been left in the mock file system, it
                // is completely benign.
            }

            if(File.Exists(_BaseStationSqbFullPath)) {
                File.Delete(_BaseStationSqbFullPath);
            }
        }

        private IDbConnection CreateSqliteConnection(string fileName = null)
        {
            var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder() {
                DataSource = fileName ?? _BaseStationSqbFullPath,
                Pooling = false,
            };
            var connection = new Microsoft.Data.Sqlite.SqliteConnection(builder.ConnectionString);

            return connection;
        }

        #region TestConnection
        [TestMethod]
        public void TestConnection_Returns_False_If_File_Could_Not_Be_Opened()
        {
            Assert.IsFalse(_SqliteImplementation.TestConnection(@"file-does-not-exist.sqb-not"));
        }

        [TestMethod]
        public void TestConnection_Returns_True_If_File_Could_Be_Opened()
        {
            var temporaryFileName = Path.GetTempFileName();
            try {
                File.WriteAllBytes(temporaryFileName, TestData.BaseStation_sqb);
                Assert.IsTrue(_SqliteImplementation.TestConnection(temporaryFileName));
            } finally {
                if(File.Exists(temporaryFileName)) {
                    for(var i = 0;i < 10;++i) {
                        try {
                            File.Delete(temporaryFileName);
                        } catch(IOException) {
                            Thread.Sleep(50);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TestConnection_Closes_SQLite_Connection()
        {
            Assert.IsTrue(_SqliteImplementation.TestConnection(_BaseStationSqbFullPath));
            DeleteTestFile();
        }

        [TestMethod]
        public void TestConnection_Returns_False_If_File_Exists_But_Is_Not_SQLite_File()
        {
            File.WriteAllText(_BaseStationSqbFullPath, "Hello");
            Assert.IsFalse(_SqliteImplementation.TestConnection(_BaseStationSqbFullPath));
        }

        [TestMethod]
        public void TestConnection_Can_Return_False_When_Connected()
        {
            _Implementation.GetAircraftByRegistration("G-ABCD");

            Assert.IsFalse(_SqliteImplementation.TestConnection("does-not-exist"));
        }

        [TestMethod]
        public void TestConnection_Does_Not_Affect_IsConnected_Property()
        {
            _Implementation.GetAircraftByRegistration("G-ABCD");

            _SqliteImplementation.TestConnection("does-not-exist");

            Assert.IsTrue(_Implementation.IsConnected);
        }
        #endregion

        #region Connection
        [TestMethod]
        public void Connection_Does_Not_Create_File_If_Missing()
        {
            DeleteTestFile();
            _Implementation.GetAircraftByRegistration("G-ABCD");
            Assert.IsFalse(File.Exists(_BaseStationSqbFullPath));
        }

        [TestMethod]
        public void Connection_Does_Not_Try_To_Use_Zero_Length_Files()
        {
            DeleteTestFile();
            File
                .Create(_BaseStationSqbFullPath)
                .Close();

            _Implementation.GetAircraftByRegistration("G-ABCD");

            Assert.AreEqual(false, _Implementation.IsConnected);
        }

        [TestMethod]
        public void Connection_Can_Work_With_ReadOnly_Access()
        {
            var fileInfo = new FileInfo(_BaseStationSqbFullPath);
            try {
                AddAircraft(CreateAircraft("ABCDEF", "G-AGWP"));
                fileInfo.IsReadOnly = true;
                Assert.IsNotNull(_Implementation.GetAircraftByRegistration("G-AGWP"));
            } finally {
                fileInfo.IsReadOnly = false;
            }
        }
        #endregion

        #region FileExists
        [TestMethod]
        public void FileExists_Returns_True_If_The_Configured_File_Exists()
        {
            Assert.IsTrue(_SqliteImplementation.FileExists());
        }

        [TestMethod]
        public void FileExists_Returns_False_If_The_Configured_File_Does_Not_Exist()
        {
            DeleteTestFile();
            Assert.IsFalse(_SqliteImplementation.FileExists());
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void FileExists_Does_Not_Test_Null_Or_Empty_FileName(string configuredFileName)
        {
            DeleteTestFile();
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;

            Assert.IsFalse(_SqliteImplementation.FileExists());
        }
        #endregion

        #region FileIsEmpty
        [TestMethod]
        public void FileIsEmpty_Returns_True_If_The_Configured_File_Is_Empty()
        {
            _FileSystem.WriteAllBytes(_BaseStationSqbFullPath, Array.Empty<byte>());

            Assert.IsTrue(_SqliteImplementation.FileIsEmpty());
        }

        [TestMethod]
        public void FileIsEmpty_Returns_False_If_The_FileName_Is_Not_Empty()
        {
            _FileSystem.WriteAllBytes(_BaseStationSqbFullPath, new byte[] { 0 });

            Assert.IsFalse(_SqliteImplementation.FileIsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void FileIsEmpty_Throws_Exception_If_File_Does_Not_Exist()
        {
            DeleteTestFile();
            _SqliteImplementation.FileIsEmpty();
        }
        #endregion

        #region GetAircraftByRegistration
        [TestMethod]
        public void GetAircraftByRegistration_Returns_Null_If_Passed_Null()
        {
            Common_GetAircraftByRegistration_Returns_Null_If_Passed_Null();
        }

        [TestMethod]
        public void GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Common_GetAircraftByRegistration_Returns_Null_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void GetAircraftByRegistration_Returns_Null_If_Database_File_Does_Not_Exist()
        {
            DeleteTestFile();
            Assert.IsNull(_Implementation.GetAircraftByRegistration("REG"));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetAircraftByRegistration_Returns_Null_If_Database_File_Not_Configured(string configuredFileName)
        {
            DeleteTestFile();
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;
            Assert.IsNull(_Implementation.GetAircraftByRegistration("REG"));
        }

        [TestMethod]
        public void GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration()
        {
            Common_GetAircraftByRegistration_Returns_Aircraft_Object_For_Registration();
        }
        #endregion

        #region GetAircraftByCode
        [TestMethod]
        public void GetAircraftByCode_Returns_Null_If_Passed_Null()
        {
            Common_GetAircraftByCode_Returns_Null_If_Passed_Null();
        }

        [TestMethod]
        public void GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Common_GetAircraftByCode_Returns_Null_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void GetAircraftByCode_Returns_Null_If_File_Does_Not_Exist()
        {
            DeleteTestFile();
            Assert.IsNull(_Implementation.GetAircraftByCode("ABC123"));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetAircraftByCode_Returns_Null_If_File_Not_Configured(string configuredFileName)
        {
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;
            Assert.IsNull(_Implementation.GetAircraftByCode("ABC123"));
        }

        [TestMethod]
        public void GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            Common_GetAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code();
        }
        #endregion

        #region GetManyAircraftByCode
        [TestMethod]
        public void GetManyAircraftByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            Common_GetManyAircraftByCode_Returns_Empty_Collection_If_Passed_Null();
        }

        [TestMethod]
        public void GetManyAircraftByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            Common_GetManyAircraftByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void GetManyAircraftByCode_Returns_Empty_Collection_If_File_Does_Not_Exist()
        {
            DeleteTestFile();
            Assert.AreEqual(0, _Implementation.GetManyAircraftByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetManyAircraftByCode_Returns_Empty_Collection_If_File_Not_Configured(string configuredFileName)
        {
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;
            Assert.AreEqual(0, _Implementation.GetManyAircraftByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        public void GetManyAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            Common_GetManyAircraftByCode_Returns_Aircraft_Object_For_ICAO24_Code();
        }

        [TestMethod]
        public void GetManyAircraftByCode_Can_Return_More_Than_One_Aircraft()
        {
            Common_GetManyAircraftByCode_Can_Return_More_Than_One_Aircraft();
        }

        [TestMethod]
        public void GetManyAircraftByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            Common_GetManyAircraftByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters();
        }
        #endregion

        #region GetManyAircraftAndFlightsCountByCode
        [TestMethod]
        public void GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Passed_Null()
        {
            Common_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Passed_Null();
        }

        [TestMethod]
        public void GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist()
        {
            Common_GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_File_Does_Not_Exist()
        {
            DeleteTestFile();
            Assert.AreEqual(0, _Implementation.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetManyAircraftAndFlightsCountByCode_Returns_Empty_Collection_If_File_Not_Configured(string configuredFileName)
        {
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;
            Assert.AreEqual(0, _Implementation.GetManyAircraftAndFlightsCountByCode(new string[] { "ABC123" }).Count);
        }

        [TestMethod]
        public void GetManyAircraftAndFlightsCountByCode_Returns_Aircraft_Object_For_ICAO24_Code()
        {
            Common_GetManyAircraftAndFlightsCountByCode_Returns_Aircraft_Object_For_ICAO24_Code();
        }

        [TestMethod]
        public void GetManyAircraftAndFlightsCountByCode_Can_Return_More_Than_One_Aircraft()
        {
            Common_GetManyAircraftAndFlightsCountByCode_Can_Return_More_Than_One_Aircraft();
        }

        [TestMethod]
        public void GetManyAircraftAndFlightsCountByCode_Returns_Counts_Of_Flights()
        {
            Common_GetManyAircraftAndFlightsCountByCode_Returns_Counts_Of_Flights();
        }

        [TestMethod]
        public void GetManyAircraftAndFlightsCountByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters()
        {
            Common_GetManyAircraftAndFlightsCountByCode_Transparently_Handles_Call_Splitting_When_Number_Of_Icaos_Exceeds_MaxParameters();
        }
        #endregion

        #region GetAircraftById
        [TestMethod]
        public void GetAircraftById_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Common_GetAircraftById_Returns_Null_If_Aircraft_Does_Not_Exist();
        }

        [TestMethod]
        public void GetAircraftById_Returns_Null_If_File_Does_Not_Exist()
        {
            DeleteTestFile();
            Assert.IsNull(_Implementation.GetAircraftById(1));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetAircraftById_Returns_Null_If_File_Not_Configured(string configuredFileName)
        {
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;
            Assert.IsNull(_Implementation.GetAircraftById(1));
        }

        [TestMethod]
        public void GetAircraftById_Returns_Aircraft_Object_For_Record_Identifier()
        {
            Common_GetAircraftById_Returns_Aircraft_Object_For_Record_Identifier();
        }
        #endregion

        #region InsertAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InsertAircraft_Throws_If_Writes_Disabled()
        {
            Common_InsertAircraft_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void InsertAircraft_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Implementation.WriteSupportEnabled = true;
            DeleteTestFile();

            _Implementation.InsertAircraft(new() { ModeS = "123456" });
            Assert.AreEqual(null, _Implementation.GetAircraftByCode("123456"));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void InsertAircraft_Does_Nothing_If_File_Not_Configured(string configuredFileName)
        {
            _Implementation.WriteSupportEnabled = true;
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;

            _SqliteImplementation.InsertAircraft(new() { ModeS = "X" });
            Assert.AreEqual(null, _Implementation.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void InsertAircraft_Does_Not_Truncate_Milliseconds_From_Date()
        {
            // In the .NET version there was some issue with the ADO.NET library not liking milliseconds
            // on dates... I can't remember. Anyway, it seems to be fine on .NET Core so for now I'm not
            // stripping off milliseconds... see how it goes :)

            _Implementation.WriteSupportEnabled = true;

            _Implementation.InsertAircraft(new() {
                ModeS = "X",
                FirstCreated = new DateTime(2001, 2, 3, 4, 5, 6, 789),
                LastModified = new DateTime(2009, 8, 7, 6, 5, 4, 321),
            });
            var readBack = _Implementation.GetAircraftByCode("X");

            Assert.AreEqual(new DateTime(2001, 2, 3, 4, 5, 6, 789), readBack.FirstCreated);
            Assert.AreEqual(new DateTime(2009, 8, 7, 6, 5, 4, 321), readBack.LastModified);
        }

        [TestMethod]
        public void InsertAircraft_Correctly_Inserts_Record()
        {
            Common_InsertAircraft_Correctly_Inserts_Record();
        }

        [TestMethod]
        public void InsertAircraft_Works_For_Different_Cultures()
        {
            Common_InsertAircraft_Works_For_Different_Cultures();
        }
        #endregion

        #region GetOrInsertAircraftByCode
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetOrInsertAircraftByCode_Throws_If_Writes_Disabled()
        {
            Common_GetOrInsertAircraftByCode_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void GetOrInsertAircraftByCode_Returns_Record_If_It_Exists()
        {
            Common_GetOrInsertAircraftByCode_Returns_Record_If_It_Exists();
        }

        [TestMethod]
        public void GetOrInsertAircraftByCode_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Implementation.WriteSupportEnabled = true;
            DeleteTestFile();

            _Implementation.GetOrInsertAircraftByCode("123456", out var created);
            Assert.AreEqual(null, _Implementation.GetAircraftByCode("123456"));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void GetOrInsertAircraftByCode_Does_Nothing_If_File_Not_Configured(string configuredFileName)
        {
            _Implementation.WriteSupportEnabled = true;
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;

            _Implementation.GetOrInsertAircraftByCode("123456", out var created);
            Assert.AreEqual(null, _Implementation.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void GetOrInsertAircraftByCode_Correctly_Inserts_Record()
        {
            Common_GetOrInsertAircraftByCode_Correctly_Inserts_Record();
        }

        [TestMethod]
        public void GetOrInsertAircraftByCode_Deals_With_Null_CodeBlock()
        {
            Common_GetOrInsertAircraftByCode_Deals_With_Null_CodeBlock();
        }

        [TestMethod]
        public void GetOrInsertAircraftByCode_Deals_With_Null_Country()
        {
            Common_GetOrInsertAircraftByCode_Deals_With_Null_Country();
        }

        [TestMethod]
        public void GetOrInsertAircraftByCode_Deals_With_Unknown_Country()
        {
            Common_GetOrInsertAircraftByCode_Deals_With_Unknown_Country();
        }

        [TestMethod]
        public void GetOrInsertAircraftByCode_Truncates_Milliseconds_From_Date()
        {
            Common_GetOrInsertAircraftByCode_Truncates_Milliseconds_From_Date();
        }
        #endregion

        #region UpdateAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateAircraft_Throws_If_Writes_Disabled()
        {
            Common_UpdateAircraft_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void UpdateAircraft_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Implementation.WriteSupportEnabled = true;
            DeleteTestFile();

            _Implementation.UpdateAircraft(new() { ModeS = "X" });
            Assert.AreEqual(null, _Implementation.GetAircraftByCode("X"));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void UpdateAircraft_Does_Nothing_If_File_Not_Configured(string configuredFileName)
        {
            _Implementation.WriteSupportEnabled = true;
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;

            _Implementation.UpdateAircraft(new() { ModeS = "X" });
            Assert.AreEqual(null, _Implementation.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void UpdateAircraft_Raises_AircraftUpdated()
        {
            Common_UpdateAircraft_Raises_AircraftUpdated();
        }

        [TestMethod]
        public void UpdateAircraft_Correctly_Updates_Record()
        {
            Common_UpdateAircraft_Correctly_Updates_Record();
        }

        [TestMethod]
        public void UpdateAircraft_Works_For_Different_Cultures()
        {
            Common_UpdateAircraft_Works_For_Different_Cultures();
        }
        #endregion

        #region UpdateAircraftModeSCountry
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateAircraftModeSCountry_Throws_If_Writes_Disabled()
        {
            Common_UpdateAircraftModeSCountry_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void UpdateAircraftModeSCountry_Does_Nothing_If_File_Not_Configured(string configuredFileName)
        {
            _Implementation.WriteSupportEnabled = true;
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;

            _Implementation.UpdateAircraftModeSCountry(1, "X");
            Assert.AreEqual(null, _Implementation.GetAircraftByCode("X"));
        }

        [TestMethod]
        public void UpdateAircraftModeSCountry_Updates_ModeSCountry_For_Existing_Record()
        {
            Common_UpdateAircraftModeSCountry_Updates_ModeSCountry_For_Existing_Record();
        }
        #endregion

        #region RecordMissingAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RecordMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            Common_RecordMissingAircraft_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void RecordMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            Common_RecordMissingAircraft_Creates_Almost_Empty_Aircraft_Record();
        }

        [TestMethod]
        public void RecordMissingAircraft_Updates_Existing_Empty_Records()
        {
            Common_RecordMissingAircraft_Updates_Existing_Empty_Records();
        }

        [TestMethod]
        public void RecordMissingAircraft_Updates_Existing_Empty_Records_With_Wrong_UserString1()
        {
            Common_RecordMissingAircraft_Updates_Existing_Empty_Records_With_Wrong_UserString1();
        }

        [TestMethod]
        public void RecordMissingAircraft_Only_Updates_Time_On_Existing_Records_With_Values()
        {
            Common_RecordMissingAircraft_Only_Updates_Time_On_Existing_Records_With_Values();
        }
        #endregion

        #region RecordManyMissingAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RecordManyMissingAircraft_Throws_Exception_If_Writes_Not_Enabled()
        {
            Common_RecordManyMissingAircraft_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void RecordManyMissingAircraft_Creates_Almost_Empty_Aircraft_Record()
        {
            Common_RecordManyMissingAircraft_Creates_Almost_Empty_Aircraft_Record();
        }

        [TestMethod]
        public void RecordManyMissingAircraft_Updates_Existing_Empty_Records()
        {
            Common_RecordManyMissingAircraft_Updates_Existing_Empty_Records();
        }

        [TestMethod]
        public void RecordManyMissingAircraft_Only_Updates_LastModified_Time_On_Existing_Records_With_Registrations()
        {
            Common_RecordManyMissingAircraft_Only_Updates_LastModified_Time_On_Existing_Records_With_Registrations();
        }
        #endregion

        #region UpsertAircraftLookup
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpsertAircraftLookup_Throws_Exception_If_Writes_Not_Enabled()
        {
            Common_UpsertAircraftLookup_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void UpsertAircraftLookup_Inserts_New_Lookups()
        {
            Common_UpsertAircraftLookup_Inserts_New_Lookups();
        }

        [TestMethod]
        public void UpsertAircraftLookup_Updates_Existing_Aircraft()
        {
            Common_UpsertAircraftLookup_Updates_Existing_Aircraft();
        }

        [TestMethod]
        public void UpsertAircraftLookup_Can_Ignore_Existing_Aircraft_When_Required()
        {
            Common_UpsertAircraftLookup_Can_Ignore_Existing_Aircraft_When_Required();
        }

        [TestMethod]
        public void UpsertAircraftLookup_Will_Overwrite_Missing_Aircraft_When_Required()
        {
            Common_UpsertAircraftLookup_Will_Overwrite_Missing_Aircraft_When_Required();
        }

        [TestMethod]
        public void UpsertAircraftLookup_Will_Not_Consider_Aircraft_Missing_If_They_Have_Registration()
        {
            Common_UpsertAircraftLookup_Will_Not_Consider_Aircraft_Missing_If_They_Have_Registration();
        }

        [TestMethod]
        public void UpsertAircraftLookup_Raises_AircraftUpdated_On_Update()
        {
            Common_UpsertAircraftLookup_Raises_AircraftUpdated_On_Update();
        }

        [TestMethod]
        public void UpsertAircraftLookup_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            Common_UpsertAircraftLookup_Does_Not_Raise_AircraftUpdated_On_Insert();
        }
        #endregion

        #region UpsertManyAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpsertManyAircraft_LookupVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            Common_UpsertManyAircraft_LookupVersion_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpsertManyAircraft_FullVersion_Throws_Exception_If_Writes_Not_Enabled()
        {
            Common_UpsertManyAircraft_FullVersion_Throws_Exception_If_Writes_Not_Enabled();
        }

        [TestMethod]
        public void UpsertManyAircraft_Inserts_New_Lookups()
        {
            Common_UpsertManyAircraft_Inserts_New_Lookups();
        }

        [TestMethod]
        public void UpsertManyAircraft_Inserts_New_Aircraft()
        {
            Common_UpsertManyAircraft_Inserts_New_Aircraft();
        }

        [TestMethod]
        public void UpsertManyAircraft_Updates_Existing_Lookups()
        {
            Common_UpsertManyAircraft_Updates_Existing_Lookups();
        }

        [TestMethod]
        public void UpsertManyAircraft_Updates_Existing_Aircraft()
        {
            Common_UpsertManyAircraft_Updates_Existing_Aircraft();
        }

        [TestMethod]
        public void UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Lookup()
        {
            Common_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Lookup();
        }

        [TestMethod]
        public void UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Aircraft()
        {
            Common_UpsertManyAircraft_Raises_AircraftUpdated_On_Update_Aircraft();
        }

        [TestMethod]
        public void UpsertManyAircraft_Does_Not_Raise_AircraftUpdated_On_Insert()
        {
            Common_UpsertManyAircraft_Does_Not_Raise_AircraftUpdated_On_Insert();
        }
        #endregion

        #region DeleteAircraft
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DeleteAircraft_Throws_If_Writes_Disabled()
        {
            Common_DeleteAircraft_Throws_If_Writes_Disabled();
        }

        [TestMethod]
        public void DeleteAircraft_Does_Nothing_If_File_Does_Not_Exist()
        {
            _Implementation.WriteSupportEnabled = true;
            DeleteTestFile();

            _Implementation.DeleteAircraft(new());
            Assert.IsFalse(File.Exists(_BaseStationSqbFullPath));
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        public void DeleteAircraft_Does_Nothing_If_File_Not_Configured(string configuredFileName)
        {
            _Implementation.WriteSupportEnabled = true;
            _Configuration.BaseStationSettings.DatabaseFileName = configuredFileName;

            _Implementation.DeleteAircraft(new());
        }

        [TestMethod]
        public void DeleteAircraft_Correctly_Deletes_Record()
        {
            Common_DeleteAircraft_Correctly_Deletes_Record();
        }
        #endregion

        #region GetFlights
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFlights_Throws_If_Criteria_Is_Null()
        {
            Common_GetFlights_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        public void GetFlights_Copies_Database_Record_To_Flight_Object()
        {
            Common_GetFlights_Copies_Database_Record_To_Flight_Object();
        }

        [TestMethod]
        public void GetFlights_Can_Return_List_Of_All_Flights()
        {
            Common_GetFlights_Can_Return_List_Of_All_Flights();
        }

        [TestMethod]
        public void GetFlights_Can_Filter_Flights_By_Equality_Criteria()
        {
            Common_GetFlights_Can_Filter_Flights_By_Equality_Criteria();
        }

        [TestMethod]
        public void GetFlights_Can_Filter_Flights_By_Contains_Criteria()
        {
            Common_GetFlights_Can_Filter_Flights_By_Contains_Criteria();
        }

        [TestMethod]
        public void GetFlights_Can_Filter_Flights_By_StartsWith_Criteria()
        {
            Common_GetFlights_Can_Filter_Flights_By_StartsWith_Criteria();
        }

        [TestMethod]
        public void GetFlights_Can_Filter_Flights_By_EndsWith_Criteria()
        {
            Common_GetFlights_Can_Filter_Flights_By_EndsWith_Criteria();
        }

        [TestMethod]
        public void GetFlights_Can_Filter_Flights_By_Range_Criteria()
        {
            Common_GetFlights_Can_Filter_Flights_By_Range_Criteria();
        }

        [TestMethod]
        public void GetFlights_String_Properties_Can_Match_Null_Or_Empty_Fields()
        {
            Common_GetFlights_String_Properties_Can_Match_Null_Or_Empty_Fields();
        }

        [TestMethod]
        public void GetFlights_Can_Search_For_All_Variations_Of_A_Callsign()
        {
            Common_GetFlights_Can_Search_For_All_Variations_Of_A_Callsign();
        }

        [TestMethod]
        public void GetFlights_Some_Criteria_Is_Case_Insensitive()
        {
            Common_GetFlights_Some_Criteria_Is_Case_Insensitive();
        }

        [TestMethod]
        public void GetFlights_Some_Criteria_Is_Case_Sensitive()
        {
            Common_GetFlights_Some_Criteria_Is_Case_Sensitive();
        }

        [TestMethod]
        public void GetFlights_Can_Return_Subset_Of_Rows()
        {
            Common_GetFlights_Can_Return_Subset_Of_Rows();
        }

        [TestMethod]
        public void GetFlights_Ignores_Unknown_Sort_Columns()
        {
            Common_GetFlights_Ignores_Unknown_Sort_Columns();
        }

        [TestMethod]
        public void GetFlights_Ignores_Case_On_Sort_Column_Names()
        {
            Common_GetFlights_Ignores_Case_On_Sort_Column_Names();
        }

        [TestMethod]
        public void GetFlights_Sorts_By_One_Column_Correctly()
        {
            Common_GetFlights_Sorts_By_One_Column_Correctly();
        }

        [TestMethod]
        public void GetFlights_Sorts_By_Two_Columns_Correctly()
        {
            Common_GetFlights_Sorts_By_Two_Columns_Correctly();
        }
        #endregion

        #region GetFlightsForAircraft
        [TestMethod]
        public void GetFlightsForAircraft_Returns_Empty_List_If_Aircraft_Is_Null()
        {
            Common_GetFlightsForAircraft_Returns_Empty_List_If_Aircraft_Is_Null();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetFlightsForAircraft_Throws_If_Criteria_Is_Null()
        {
            Common_GetFlightsForAircraft_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Restricts_Search_To_Single_Aircraft()
        {
            Common_GetFlightsForAircraft_Restricts_Search_To_Single_Aircraft();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Does_Not_Create_New_Aircraft_Objects()
        {
            Common_GetFlightsForAircraft_Does_Not_Create_New_Aircraft_Objects();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Copies_Database_Record_To_Flight_Object()
        {
            Common_GetFlightsForAircraft_Copies_Database_Record_To_Flight_Object();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Can_Filter_Flights_By_Equality_Criteria()
        {
            Common_GetFlightsForAircraft_Can_Filter_Flights_By_Equality_Criteria();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Can_Filter_Flights_By_Range_Criteria()
        {
            Common_GetFlightsForAircraft_Can_Filter_Flights_By_Range_Criteria();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Some_Criteria_Is_Case_Insensitive()
        {
            Common_GetFlightsForAircraft_Some_Criteria_Is_Case_Insensitive();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Some_Criteria_Is_Case_Sensitive()
        {
            Common_GetFlightsForAircraft_Some_Criteria_Is_Case_Sensitive();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Can_Return_Subset_Of_Rows()
        {
            Common_GetFlightsForAircraft_Can_Return_Subset_Of_Rows();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Ignores_Unknown_Sort_Columns()
        {
            Common_GetFlightsForAircraft_Ignores_Unknown_Sort_Columns();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Ignores_Case_On_Sort_Column_Names()
        {
            Common_GetFlightsForAircraft_Ignores_Case_On_Sort_Column_Names();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Sorts_By_One_Column_Correctly()
        {
            Common_GetFlightsForAircraft_Sorts_By_One_Column_Correctly();
        }

        [TestMethod]
        public void GetFlightsForAircraft_Sorts_By_Two_Columns_Correctly()
        {
            Common_GetFlightsForAircraft_Sorts_By_Two_Columns_Correctly();
        }
        #endregion

        #region GetCountOfFlights
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetCountOfFlights_Throws_If_Criteria_Is_Null()
        {
            Common_GetCountOfFlights_Throws_If_Criteria_Is_Null();
        }

        [TestMethod]
        public void GetCountOfFlights_Returns_Count_Of_Flights_Matching_Criteria()
        {
            Common_GetCountOfFlights_Returns_Count_Of_Flights_Matching_Criteria();
        }

        [TestMethod]
        public void GetCountOfFlights_Counts_Equality_Criteria()
        {
            Common_GetCountOfFlights_Counts_Equality_Criteria();
        }

        [TestMethod]
        public void GetCountOfFlights_Counts_Range_Criteria()
        {
            Common_GetCountOfFlights_Counts_Range_Criteria();
        }
        #endregion
    }
}
