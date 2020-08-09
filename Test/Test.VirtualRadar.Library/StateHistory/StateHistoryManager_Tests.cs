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
            mockRepository
                .Setup(r => r.IsMissing)
                .Returns(false);

            mockDatabaseInstance.Setup(r => r.Initialise(It.IsAny<bool>(), It.IsAny<string>()))
                .Callback((bool writesEnabled, string nonStandardFolder) => {
                    mockDatabaseInstance
                        .SetupGet(r => r.WritesEnabled)
                        .Returns(writesEnabled);

                    mockDatabaseInstance
                        .SetupGet(r => r.NonStandardFolder)
                        .Returns(nonStandardFolder);

                    mockDatabaseInstance
                        .Setup(r => r.DoIfReadable(It.IsAny<Action<IStateHistoryRepository>>()))
                        .Returns((Action<IStateHistoryRepository> action) => {
                            var readable = !mockRepository.Object.IsMissing;
                            if(readable) {
                                action(mockRepository.Object);
                            }
                            return readable;
                        });

                    mockDatabaseInstance
                        .Setup(r => r.DoIfWriteable(It.IsAny<Action<IStateHistoryRepository>>()))
                        .Returns((Action<IStateHistoryRepository> action) => {
                            var readable = !mockRepository.Object.IsMissing;
                            var writable = writesEnabled;
                            if(readable && writable) {
                                action(mockRepository.Object);
                            }
                            return readable && writable;
                        });
                });
        }

        [TestMethod]
        public void Initialise_Loads_Configuration()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { WritesEnabled = true,   NonStandardFolder = "Xyz", },
                new { WritesEnabled = false,  NonStandardFolder = (string)null, },
                new { WritesEnabled = false,  NonStandardFolder = "", },
            }, row => {
                _Configuration.StateHistorySettings.WritesEnabled =     row.WritesEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.NonStandardFolder;

                _Manager.Initialise();

                Assert.AreEqual(row.WritesEnabled,      _Manager.WritesEnabled);
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
                _Configuration.StateHistorySettings.WritesEnabled =     row.WritesEnabled;
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
                    OldWritesEnabled = false,               NewWritesEnabled = true,
                    OldNonStandardFolder = (string)null,    NewNonStandardFolder = "Abc"
                },
            }, row => {
                _Configuration.StateHistorySettings.WritesEnabled =     row.OldWritesEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.OldNonStandardFolder;
                _Manager.Initialise();

                _Configuration.StateHistorySettings.WritesEnabled =     row.NewWritesEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.NewNonStandardFolder;
                _SharedConfig.RaiseConfigurationChanged();

                Assert.AreEqual(row.NewWritesEnabled,       _Manager.WritesEnabled);
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
                    case nameof(config.WritesEnabled):      config.WritesEnabled = !config.WritesEnabled; break;
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
                _Configuration.StateHistorySettings.WritesEnabled =     row.InitialEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.InitialFolder;
                _Manager.Initialise();

                var originalDatabaseInstance = _DatabaseInstance;
                var newDatabaseInstance = TestUtilities.CreateMockImplementation<IStateHistoryDatabaseInstance>();

                _Configuration.StateHistorySettings.WritesEnabled =     row.NewEnabled;
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
