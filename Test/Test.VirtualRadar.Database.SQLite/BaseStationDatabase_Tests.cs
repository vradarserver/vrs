using System.IO;
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
            _FileSystem.AddFileContent(Path.Combine(_EnvironmentOptions.WorkingFolder, _BaseStationSqbFullPath), Array.Empty<byte>());

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
            if(File.Exists(_BaseStationSqbFullPath)) {
                File.Delete(_BaseStationSqbFullPath);
            }
        }

        [TestMethod]
        public void TestConnection_Returns_False_If_File_Could_Not_Be_Opened()
        {
            File.Delete(_BaseStationSqbFullPath);
            Assert.IsFalse(_Implementation.TestConnection());
        }
    }
}
