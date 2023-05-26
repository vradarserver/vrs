using System.Data;
using System.IO;
using System.Text;
using Dapper;
using Test.Framework;
using VirtualRadar.Database.SQLite.KineticData;
using VirtualRadar.Interface;
using VirtualRadar.Interface.KineticData;
using VirtualRadar.Interface.Options;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Database.SQLite
{
    [TestClass]
    public class BaseStationDatabase_Tests
    {
        // Eventually these should go into the shared BaseStation database tests base
        public const string SchemaPrefix = "";
        Func<IDbConnection> _CreateConnection;
        private string _SqlReturnNewIdentity = "SELECT last_insert_rowid();";

        private MockFileSystem _FileSystem;
        private EnvironmentOptions _EnvironmentOptions;
        private BaseStationDatabaseOptions _BaseStationDatabaseOptions;
        private IBaseStationDatabaseSQLite _Implementation;
        private MockSharedConfiguration _SharedConfiguration;
        private Configuration _Configuration;
        private MockClock _MockClock;
        private MockStandingDataManager _StandingData;
        private string _BaseStationSqbFullPath;

        [TestInitialize]
        public void TestInitialise()
        {
            // These can be removed once we have the common base
             _CreateConnection = () => CreateSqliteConnection(null);

            _SharedConfiguration = new();
            _Configuration = _SharedConfiguration.Configuration;

            _EnvironmentOptions = new() {
                WorkingFolder = Path.GetTempPath(),
            };
            _BaseStationDatabaseOptions = new();

            var randomFileNameElement = Guid.NewGuid().ToString();      // <-- need this to stop the O/S knacking tests while it deletes files in the background etc.
            var baseStationSqbFileName = $"BaseStation-{randomFileNameElement}.sqb";
            _BaseStationSqbFullPath = Path.Combine(_EnvironmentOptions.WorkingFolder, baseStationSqbFileName);
            _Configuration.BaseStationSettings.DatabaseFileName = _BaseStationSqbFullPath;
            File.WriteAllBytes(_BaseStationSqbFullPath, TestData.BaseStation_sqb);

            _FileSystem = new();
            _FileSystem.AddFileContent(_BaseStationSqbFullPath, new byte[] { 0 });

            _MockClock = new();

            _StandingData = new();

#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)
            using(new CultureSwitcher("en-GB")) {
                _Implementation = new BaseStationDatabase(
                    _FileSystem,
                    _SharedConfiguration.Object,
                    _MockClock.Object,
                    _StandingData.Object,
                    new MockOptions<BaseStationDatabaseOptions>(_BaseStationDatabaseOptions)
                );
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

        protected long AddAircraft(KineticAircraft aircraft)
        {
            long result = 0;

            using(var connection = _CreateConnection()) {
                connection.Open();

                var dynamicParameters = new Dapper.DynamicParameters();
                var fieldNames = new StringBuilder();
                var parameters = new StringBuilder();

                foreach(var property in typeof(KineticAircraft).GetProperties()) {
                    var fieldName = property.Name;
                    if(fieldName == nameof(KineticAircraft.AircraftID)) {
                        continue;
                    }

                    if(fieldNames.Length > 0) {
                        fieldNames.Append(',');
                    }
                    if(parameters.Length > 0) {
                        parameters.Append(',');
                    }

                    fieldNames.Append($"[{fieldName}]");
                    parameters.Append($"@{fieldName}");

                    dynamicParameters.Add(fieldName, property.GetValue(aircraft, null));
                }

                result = connection.ExecuteScalar<long>($"INSERT INTO {SchemaPrefix}[Aircraft] ({fieldNames}) VALUES ({parameters}); {_SqlReturnNewIdentity}", dynamicParameters);
                aircraft.AircraftID = (int)result;
            }

            return result;
        }

        protected static KineticAircraft CreateAircraft(string icao24 = "123456", string registration = "G-VRST")
        {
            return new KineticAircraft() {
                ModeS = icao24,
                Registration = registration,
            };
        }

        #region TestConnection
        [TestMethod]
        public void TestConnection_Returns_False_If_File_Could_Not_Be_Opened()
        {
            Assert.IsFalse(_Implementation.TestConnection(@"file-does-not-exist.sqb-not"));
        }

        [TestMethod]
        public void TestConnection_Returns_True_If_File_Could_Be_Opened()
        {
            var temporaryFileName = Path.GetTempFileName();
            try {
                File.WriteAllBytes(temporaryFileName, TestData.BaseStation_sqb);
                Assert.IsTrue(_Implementation.TestConnection(temporaryFileName));
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
            Assert.IsTrue(_Implementation.TestConnection(_BaseStationSqbFullPath));
            DeleteTestFile();
        }

        [TestMethod]
        public void TestConnection_Returns_False_If_File_Exists_But_Is_Not_SQLite_File()
        {
            File.WriteAllText(_BaseStationSqbFullPath, "Hello");
            Assert.IsFalse(_Implementation.TestConnection(_BaseStationSqbFullPath));
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
        public void SQLite_Connection_Does_Not_Try_To_Use_Zero_Length_Files()
        {
            DeleteTestFile();
            File
                .Create(_BaseStationSqbFullPath)
                .Close();

            _Implementation.GetAircraftByRegistration("G-ABCD");

            Assert.AreEqual(false, _Implementation.IsConnected);
        }

        [TestMethod]
        public void SQLite_Connection_Can_Work_With_ReadOnly_Access()
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
    }
}
