using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StateHistory;

namespace Test.VirtualRadar.Library.StateHistory
{
    [TestClass]
    public class StateHistoryManager_Tests
    {
        public TestContext TestContext { get; set; }

        private IStateHistoryManager    _Manager;
        private IClassFactory           _Snapshot;
        private MockSharedConfiguration _SharedConfig;
        private Configuration           _Configuration;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _Configuration = new Configuration();
            _SharedConfig = MockSharedConfiguration.TestInitialise(_Configuration);

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
    }
}
