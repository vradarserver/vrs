using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StateHistory;

namespace Test.VirtualRadar.Library.StateHistory
{
    [TestClass]
    public class StateHistoryManager_Tests
    {
        public TestContext TestContext { get; set; }

        private IStateHistoryManager                _Manager;
        private IClassFactory                       _Snapshot;
        private MockSharedConfiguration             _SharedConfig;
        private Configuration                       _Configuration;
        private Mock<IStateHistoryRepository>       _Repository;
        private Mock<IStateHistoryDatabaseInstance> _DatabaseInstance;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Configuration =    new Configuration();
            _SharedConfig =     MockSharedConfiguration.TestInitialise(_Configuration);
            _Repository =       TestUtilities.CreateMockImplementation<IStateHistoryRepository>();
            _DatabaseInstance = TestUtilities.CreateMockImplementation<IStateHistoryDatabaseInstance>();

            SetupDatabaseInstance(_DatabaseInstance, _Repository);

            _Manager = Factory.ResolveNewInstance<IStateHistoryManager>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        private void SetupDatabaseInstance(Mock<IStateHistoryDatabaseInstance> mockDatabaseInstance, Mock<IStateHistoryRepository> mockRepository)
        {
            mockDatabaseInstance.Setup(r => r.Initialise(It.IsAny<bool>(), It.IsAny<string>()))
                .Callback((bool writesEnabled, string nonStandardFolder) => {
                    mockDatabaseInstance
                        .SetupGet(r => r.WritesEnabled)
                        .Returns(writesEnabled);

                    mockDatabaseInstance
                        .SetupGet(r => r.NonStandardFolder)
                        .Returns(nonStandardFolder);

                    mockDatabaseInstance
                        .SetupGet(r => r.Repository)
                        .Returns(mockRepository.Object);
                });
        }

        [TestMethod]
        public void Initialise_Loads_Configuration()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Enabled = true,   NonStandardFolder = "Xyz", },
                new { Enabled = false,  NonStandardFolder = (string)null, },
                new { Enabled = false,  NonStandardFolder = "", },
            }, row => {
                _Configuration.StateHistorySettings.Enabled =           row.Enabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.NonStandardFolder;

                _Manager.Initialise();

                Assert.AreEqual(row.Enabled,            _Manager.Enabled);
                Assert.AreEqual(row.NonStandardFolder,  _Manager.NonStandardFolder);
            });
        }

        [TestMethod]
        public void Initialise_Does_Not_Raise_ConfigurationLoaded()
        {
            var eventRec = new EventRecorder<EventArgs>();
            _Manager.ConfigurationLoaded += eventRec.Handler;

            _Manager.Initialise();

            Assert.AreEqual(0, eventRec.CallCount);
        }

        [TestMethod]
        public void Initialise_Creates_DatabaseInstance()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { WritesEnabled = true,     NonStandardFolder = (string)null },
                new { WritesEnabled = true,     NonStandardFolder = "" },
                new { WritesEnabled = false,    NonStandardFolder = "Abc" },
            }, row => {
                _Configuration.StateHistorySettings.Enabled =           row.WritesEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.NonStandardFolder;
                _Manager.Initialise();

                _DatabaseInstance.Verify(r => r.Initialise(row.WritesEnabled, row.NonStandardFolder), Times.Once());
            });
        }

        [TestMethod]
        public void Configuration_Change_Automatically_Picked_Up()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new {
                    OldEnabled = false,                     NewEnabled = true,
                    OldNonStandardFolder = (string)null,    NewNonStandardFolder = "Abc"
                },
            }, row => {
                _Configuration.StateHistorySettings.Enabled =           row.OldEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.OldNonStandardFolder;
                _Manager.Initialise();

                _Configuration.StateHistorySettings.Enabled =           row.NewEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.NewNonStandardFolder;
                _SharedConfig.RaiseConfigurationChanged();

                Assert.AreEqual(row.NewEnabled,             _Manager.Enabled);
                Assert.AreEqual(row.NewNonStandardFolder,   _Manager.NonStandardFolder);
            });
        }

        [TestMethod]
        public void Configuration_Change_Raises_ConfigurationLoaded()
        {
            new InlineDataTest(this).TestAndAssert(
                typeof(StateHistorySettings).GetProperties()
            , configProperty => {
                _Manager.Initialise();

                var config = _Configuration.StateHistorySettings;
                switch(configProperty.Name) {
                    case nameof(config.Enabled):            config.Enabled = !config.Enabled; break;
                    case nameof(config.NonStandardFolder):  config.NonStandardFolder = (config.NonStandardFolder ?? "") + "a"; break;
                    default:                                throw new NotImplementedException($"Need code to change {configProperty.Name}");
                }

                var eventRec = new EventRecorder<EventArgs>();
                _Manager.ConfigurationLoaded += eventRec.Handler;

                _SharedConfig.RaiseConfigurationChanged();

                Assert.AreEqual(1, eventRec.CallCount);
                Assert.AreSame(_Manager, eventRec.Sender);
                Assert.IsNotNull(eventRec.Args);
            });
        }

        [TestMethod]
        public void Configuration_Change_Does_Not_Raise_ConfigurationLoaded_If_No_StateHistory_Settings_Changed()
        {
            _Manager.Initialise();

            var eventRec = new EventRecorder<EventArgs>();
            _Manager.ConfigurationLoaded += eventRec.Handler;

            _SharedConfig.RaiseConfigurationChanged();

            Assert.AreEqual(0, eventRec.CallCount);
        }

        [TestMethod]
        public void Configuration_Change_Replaces_DatabaseInstance_When_Database_Settings_Change()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { InitialEnabled = false,   InitialFolder = "",     NewEnabled = false, NewFolder = "",     ExpectNewDatabaseInstance = false, },
                new { InitialEnabled = false,   InitialFolder = "",     NewEnabled = false, NewFolder = "new",  ExpectNewDatabaseInstance = true, },
                new { InitialEnabled = false,   InitialFolder = "",     NewEnabled = true,  NewFolder = "",     ExpectNewDatabaseInstance = true, },
                new { InitialEnabled = true,    InitialFolder = "",     NewEnabled = false, NewFolder = "",     ExpectNewDatabaseInstance = true, },
                new { InitialEnabled = true,    InitialFolder = "",     NewEnabled = true,  NewFolder = "",     ExpectNewDatabaseInstance = false, },
                new { InitialEnabled = true,    InitialFolder = "",     NewEnabled = true,  NewFolder = "new",  ExpectNewDatabaseInstance = true, },
                new { InitialEnabled = true,    InitialFolder = "old",  NewEnabled = true,  NewFolder = "",     ExpectNewDatabaseInstance = true, },
            }, row => {
                _Configuration.StateHistorySettings.Enabled =           row.InitialEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.InitialFolder;
                _Manager.Initialise();

                var originalDatabaseInstance = _DatabaseInstance;
                var newDatabaseInstance = TestUtilities.CreateMockImplementation<IStateHistoryDatabaseInstance>();

                _Configuration.StateHistorySettings.Enabled =           row.NewEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.NewFolder;
                _SharedConfig.RaiseConfigurationChanged();

                newDatabaseInstance.Verify(r => r.Initialise(row.NewEnabled, row.NewFolder), row.ExpectNewDatabaseInstance
                    ? Times.Once()
                    : Times.Never()
                );
            });
        }
    }
}
