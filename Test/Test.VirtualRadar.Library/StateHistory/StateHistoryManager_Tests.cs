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

        private IStateHistoryManager            _Manager;
        private IClassFactory                   _Snapshot;
        private MockSharedConfiguration         _SharedConfig;
        private Configuration                   _Configuration;
        private Mock<IStateHistoryRepository>   _Repository;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Configuration = new Configuration();
            _SharedConfig = MockSharedConfiguration.TestInitialise(_Configuration);
            _Repository =       TestUtilities.CreateMockImplementation<IStateHistoryRepository>();

            _Manager = Factory.ResolveNewInstance<IStateHistoryManager>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
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
        public void Initialise_Updates_The_Schema_If_Enabled()
        {
            new InlineDataTest(this).TestAndAssert(new bool[] {
                true, false
            }, enabled => {
                _Configuration.StateHistorySettings.Enabled = enabled;
                _Manager.Initialise();

                _Repository.Verify(r => r.Schema_Update(), enabled
                    ? Times.Once()
                    : Times.Never()
                );
            });
        }

        [TestMethod]
        public void Initialise_Creates_DatabaseVersion_On_First_Run()
        {
            _Repository
                .Setup(r => r.DatabaseVersion_GetLatest())
                .Returns((DatabaseVersion)null);

            _Manager.Initialise();

            _Repository.Verify(r => r.DatabaseVersion_Save(It.IsAny<DatabaseVersion>()), Times.Once());
        }

        [TestMethod]
        public void Initialise_Ignores_DatabaseVersion_If_Already_Exists()
        {
            _Repository
                .Setup(r => r.DatabaseVersion_GetLatest())
                .Returns(new DatabaseVersion() {
                    DatabaseVersionID = 1,
                    CreatedUtc =        DateTime.UtcNow.AddDays(-1)
                });

            _Manager.Initialise();

            _Repository.Verify(r => r.DatabaseVersion_Save(It.IsAny<DatabaseVersion>()), Times.Never());
        }

        [TestMethod]
        public void Initialise_Creates_VrsSession()
        {
            VrsSession session = null;
            _Repository
                .Setup(r => r.VrsSession_Insert(It.IsAny<VrsSession>()))
                .Callback((VrsSession s) => session = s);

            _Manager.Initialise();

            _Repository.Verify(r => r.VrsSession_Insert(It.IsAny<VrsSession>()), Times.Once());
            Assert.AreNotEqual(0, session.DatabaseVersionID);
            Assert.IsTrue(session.CreatedUtc > DateTime.UtcNow.AddMinutes(-2));
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
        public void ConfigurationChange_Updates_Schema()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { InitialEnabled = false,   InitialFolder = "",     NewEnabled = false, NewFolder = "",     CountExpectedSchemaInit = 0 },
                new { InitialEnabled = false,   InitialFolder = "",     NewEnabled = false, NewFolder = "new",  CountExpectedSchemaInit = 0 },
                new { InitialEnabled = false,   InitialFolder = "",     NewEnabled = true,  NewFolder = "",     CountExpectedSchemaInit = 1 },
                new { InitialEnabled = true,    InitialFolder = "",     NewEnabled = false, NewFolder = "",     CountExpectedSchemaInit = 1 },
                new { InitialEnabled = true,    InitialFolder = "",     NewEnabled = true,  NewFolder = "",     CountExpectedSchemaInit = 1 },
                new { InitialEnabled = true,    InitialFolder = "",     NewEnabled = true,  NewFolder = "new",  CountExpectedSchemaInit = 2 },
                new { InitialEnabled = true,    InitialFolder = "old",  NewEnabled = true,  NewFolder = "",     CountExpectedSchemaInit = 2 },
            }, row => {
                _Configuration.StateHistorySettings.Enabled =           row.InitialEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.InitialFolder;
                _Manager.Initialise();

                _Configuration.StateHistorySettings.Enabled =           row.NewEnabled;
                _Configuration.StateHistorySettings.NonStandardFolder = row.NewFolder;
                _SharedConfig.RaiseConfigurationChanged();

                _Repository.Verify(r => r.Schema_Update(), Times.Exactly(row.CountExpectedSchemaInit));
            });
        }
    }
}
