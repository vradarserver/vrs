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

#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)
            using(new CultureSwitcher("en-GB")) {
                _SqliteImplementation = new BaseStationDatabase(
                    _FileSystem,
                    _SharedConfiguration.Object,
                    _MockClock.Object,
                    _StandingData.Object,
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

            for(var i = 0;i < 10;++i) {
                try {
                    if(File.Exists(_BaseStationSqbFullPath)) {
                        File.Delete(_BaseStationSqbFullPath);
                    }
                } catch(IOException) {
                    Thread.Sleep(50);
                }
            }
        }

        private IDbConnection CreateSqliteConnection(string fileName = null)
        {
            var builder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder() {
                DataSource = fileName ?? _BaseStationSqbFullPath,
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
    }
}
