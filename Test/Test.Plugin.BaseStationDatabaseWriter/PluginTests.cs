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
using System.Globalization;
using System.Linq;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using PluginNS = VirtualRadar.Plugin.BaseStationDatabaseWriter;
using VirtualRadar.Plugin.BaseStationDatabaseWriter;
using VirtualRadar.Interface.SQLite;
using Newtonsoft.Json;

namespace Test.VirtualRadar.Plugin.BaseStationDatabaseWriter
{
    [TestClass]
    public class PluginTests
    {
        #region Fields, TestInitialise etc.
        public TestContext TestContext { get; set; }

        private const double MinutesBeforeFlightClosed = 25;

        private IClassFactory _ClassFactorySnapshot;
        private PluginNS.Plugin _Plugin;
        private Mock<PluginNS.IPluginProvider> _Provider;
        private Mock<IPluginSettingsStorage> _PluginSettingsStorage;
        private PluginSettings _PluginSettings;
        private PluginStartupParameters _StartupParameters;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Mock<IBaseStationDatabase> _BaseStationDatabase;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigBaseStationDatabase;
        private Mock<PluginNS.IOptionsView> _OptionsView;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Mock<IHeartbeatService> _HeartbeatService;
        private EventRecorder<EventArgs> _StatusChangedEvent;
        private Mock<ILog> _Log;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private List<Mock<IFeed>> _Feeds;
        private List<Mock<IListener>> _Listeners;
        private Mock<IFeedManager> _FeedManger;
        private Mock<IListener> _Listener;
        private Mock<ISQLiteException> _SqliteException;
        private Mock<IOnlineLookupCache> _OnlineLookupCache;
        private Options _Options;
        private Mock<IAircraftOnlineLookupManager> _AircraftOnlineLookupManager;

        [TestInitialize]
        public void TestInitialise()
        {
            _StartupParameters = new PluginStartupParameters(null, null, null, null);
            _ClassFactorySnapshot = Factory.TakeSnapshot();
            _PluginSettingsStorage = TestUtilities.CreateMockSingleton<IPluginSettingsStorage>();
            _PluginSettings = new PluginSettings();
            _PluginSettingsStorage.Setup(s => s.Load()).Returns(_PluginSettings);
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsTest).Returns(true);
            _Options = new Options();

            _Feeds = new List<Mock<IFeed>>();
            _Listeners = new List<Mock<IListener>>();
            var useVisibleFeeds = false;
            _FeedManger = FeedHelper.CreateMockFeedManager(_Feeds, _Listeners, useVisibleFeeds, 1, 2);

            _BaseStationDatabase = new Mock<IBaseStationDatabase>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _BaseStationDatabase.Object.FileName = "fn";
            _AutoConfigBaseStationDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _AutoConfigBaseStationDatabase.Setup(a => a.Database).Returns(_BaseStationDatabase.Object);

            _BaseStationDatabase.Setup(r => r.InsertAircraft(It.IsAny<BaseStationAircraft>())).Callback((BaseStationAircraft r) => r.AircraftID = 100);
            _BaseStationDatabase.Setup(r => r.InsertFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight r) => r.FlightID = 200);

            _BaseStationDatabase.Setup(d => d.GetLocations()).Returns(new List<BaseStationLocation>() { new BaseStationLocation() { LocationID = 9821 } } );
            SetDBHistory(true);

            _HeartbeatService = TestUtilities.CreateMockInstance<IHeartbeatService>();
            Factory.Singleton.RegisterInstance(typeof(IHeartbeatService), _HeartbeatService.Object);

            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _SqliteException = TestUtilities.CreateMockImplementation<ISQLiteException>();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(c => c.Load()).Returns(_Configuration);

            _StatusChangedEvent = new EventRecorder<EventArgs>();

            _AircraftOnlineLookupManager = TestUtilities.CreateMockSingleton<IAircraftOnlineLookupManager>();

            _Plugin = new PluginNS.Plugin();
            _Provider = new Mock<PluginNS.IPluginProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(true);
            _Provider.Setup(p => p.LocalNow).Returns(new DateTime(2001, 2, 3, 4, 5, 6));
            _Provider.Setup(p => p.FileSize(It.IsAny<string>())).Returns(1000000L);
            _OptionsView = new Mock<PluginNS.IOptionsView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _OptionsView.Setup(r => r.DisplayView()).Returns(true);
            _Provider.Setup(p => p.CreateOptionsView()).Returns(_OptionsView.Object);
            _OnlineLookupCache = TestUtilities.CreateMockImplementation<IOnlineLookupCache>();
            _Provider.Setup(r => r.CreateOnlineLookupCache()).Returns(_OnlineLookupCache.Object);
            _Plugin.Provider = _Provider.Object;

            SetReceiverIdOption(1);
            _Listener = _Listeners[0];
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }

        void SetEnabledOption(bool enabled)                     { _Options.Enabled = enabled; RecordSettings(); }
        void SetAllowUpdateOfOtherDatabasesOption(bool value)   { _Options.AllowUpdateOfOtherDatabases = value; RecordSettings(); }
        void SetReceiverIdOption(int value)                     { _Options.ReceiverId = value; RecordSettings(); }
        void SetOnlineCacheEnabled(bool enabled)                { _Options.SaveDownloadedAircraftDetails = enabled; RecordSettings(); }
        void SetRefreshOutOfDateAircraft(bool refresh)          { _Options.RefreshOutOfDateAircraft = refresh; RecordSettings(); }

        void RecordSettings()
        {
            // Record the old settings in _PluginSettings
            _PluginSettings.Write(_Plugin, "Enabled",                       _Options.Enabled);
            _PluginSettings.Write(_Plugin, "AllowUpdateOfOtherDatabases",   _Options.AllowUpdateOfOtherDatabases);
            _PluginSettings.Write(_Plugin, "ReceiverId",                    _Options.ReceiverId);

            // Record the new method of storing settings in _PluginSettings
            var jsonText = JsonConvert.SerializeObject(_Options);
            _PluginSettings.Write(_Plugin, "Options", jsonText);
        }

        void SetConfigurationBaseStationDatabaseFileName(string fileName)
        {
            _Configuration.BaseStationSettings.DatabaseFileName = fileName;
            _ConfigurationStorage.Raise(v => v.ConfigurationChanged += null, EventArgs.Empty);
        }

        DateTime SetProviderTimes(DateTime localNow, DateTime? utcNow = null)
        {
            _Provider.Setup(p => p.LocalNow).Returns(localNow);
            _Provider.Setup(p => p.UtcNow).Returns(utcNow == null ? localNow : utcNow.Value);

            return localNow;
        }

        void SetDBHistory(bool createdByVrs)
        {
            _BaseStationDatabase.Setup(d => d.GetDatabaseHistory()).Returns(new List<BaseStationDBHistory>() { new BaseStationDBHistory() { Description = createdByVrs ? "Database autocreated by Virtual Radar Server" : "Database autocreated by Snoopy" } } );
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void Plugin_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _Plugin = new PluginNS.Plugin();
            Assert.IsNotNull(_Plugin.Provider);
            TestUtilities.TestProperty(_Plugin, r => r.Provider, _Plugin.Provider, _Provider.Object);

            Assert.AreEqual("VirtualRadar.Plugin.BaseStationDatabaseWriter", _Plugin.Id);
            Assert.IsFalse(String.IsNullOrEmpty(_Plugin.Name));
            Assert.IsFalse(String.IsNullOrEmpty(_Plugin.Version));
            Assert.AreEqual("Disabled", _Plugin.Status);
            Assert.AreEqual(null, _Plugin.StatusDescription);
            Assert.IsTrue(_Plugin.HasOptions);
        }
        #endregion

        #region RegisterImplementations
        [TestMethod]
        public void Plugin_RegisterImplementations_Does_Nothing()
        {
            var classFactory = new Mock<IClassFactory>(MockBehavior.Strict).Object;

            _Plugin.RegisterImplementations(classFactory);
        }
        #endregion

        #region Startup
        [TestMethod]
        public void Plugin_Startup_Loads_Options()
        {
            _Plugin.Startup(_StartupParameters);
            _PluginSettingsStorage.Verify(s => s.Load(), Times.Once());
        }

        [TestMethod]
        public void Plugin_Startup_Enables_Write_Support_On_Database_If_Option_Enabled()
        {
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(true, _BaseStationDatabase.Object.WriteSupportEnabled);
        }

        [TestMethod]
        public void Plugin_Startup_Enables_Online_Cache_If_Option_Enabled()
        {
            SetEnabledOption(true);
            SetOnlineCacheEnabled(true);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(true, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Registers_Online_Cache()
        {
            _Plugin.Startup(_StartupParameters);

            _AircraftOnlineLookupManager.Verify(r => r.RegisterCache(_OnlineLookupCache.Object, 100, false), Times.Once());
        }

        [TestMethod]
        public void Plugin_Startup_Sets_Database_On_Online_Cache()
        {
            _OnlineLookupCache.Object.Database = null;

            _Plugin.Startup(_StartupParameters);

            Assert.IsNotNull(_OnlineLookupCache.Object.Database);
        }

        [TestMethod]
        public void Plugin_Startup_Sets_RefreshOutOfDateAircraft_On_Online_Cache()
        {
            SetRefreshOutOfDateAircraft(true);

            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(true, _OnlineLookupCache.Object.RefreshOutOfDateAircraft);
        }

        [TestMethod]
        public void Plugin_Startup_Clears_RefreshOutOfDateAircraft_On_Online_Cache()
        {
            SetRefreshOutOfDateAircraft(false);

            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.RefreshOutOfDateAircraft);
        }

        [TestMethod]
        public void Plugin_Startup_Does_Not_Enable_Write_Support_On_Database_If_Option_Disabled()
        {
            SetEnabledOption(false);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _BaseStationDatabase.Object.WriteSupportEnabled);
        }

        [TestMethod]
        public void Plugin_Startup_Sets_Correct_Status_If_Plugin_Disabled()
        {
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;

            SetEnabledOption(false);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(PluginStrings.Disabled, _Plugin.Status);
            Assert.AreEqual(null, _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Startup_Disables_Online_Cache_If_Plugin_Disabled()
        {
            SetEnabledOption(false);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Disables_Online_Cache_If_Caching_Is_Disabled()
        {
            SetEnabledOption(true);
            SetOnlineCacheEnabled(false);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Sets_Correct_Status_If_ReceiverId_Not_Specified()
        {
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;
            _StatusChangedEvent.EventRaised += (s, a) => {
                Assert.AreEqual(PluginStrings.EnabledNoReceiver, _Plugin.Status);
                Assert.AreEqual(null, _Plugin.StatusDescription);
            };

            SetEnabledOption(true);
            SetReceiverIdOption(0);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(1, _StatusChangedEvent.CallCount);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
        }

        [TestMethod]
        public void Plugin_Startup_Disables_Online_Cache_If_ReceiverId_Not_Specified()
        {
            SetEnabledOption(true);
            SetReceiverIdOption(0);
            SetOnlineCacheEnabled(true);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Sets_Correct_Status_If_ReceiverId_Is_Invalid()
        {
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;
            _StatusChangedEvent.EventRaised += (s, a) => {
                Assert.AreEqual(PluginStrings.EnabledBadReceiver, _Plugin.Status);
                Assert.AreEqual(null, _Plugin.StatusDescription);
            };

            SetEnabledOption(true);
            SetReceiverIdOption(100);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(1, _StatusChangedEvent.CallCount);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
        }

        [TestMethod]
        public void Plugin_Startup_Disables_Online_Cache_If_ReceiverId_Is_Invalid()
        {
            SetEnabledOption(true);
            SetReceiverIdOption(100);
            SetOnlineCacheEnabled(true);
            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Creates_A_Session_Record_If_Option_Enabled()
        {
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession s) => {
                Assert.AreEqual(null, s.EndTime);
                Assert.AreEqual(9821, s.LocationID);
                Assert.AreEqual(_Provider.Object.LocalNow, s.StartTime);
            });

            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Startup_Sets_Status_If_Option_Enabled()
        {
            SetEnabledOption(true);
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;
            _StatusChangedEvent.EventRaised += (s, a) => {
                Assert.AreEqual(@"Enabled, updating c:\folder\db.sqb", _Plugin.Status);
            };
            _BaseStationDatabase.Object.FileName = @"c:\folder\db.sqb";

            _Plugin.Startup(_StartupParameters);

            Assert.AreNotEqual(0, _StatusChangedEvent.CallCount);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
        }

        [TestMethod]
        public void Plugin_Startup_Sets_Status_If_Database_Is_Null()
        {
            SetEnabledOption(true);
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;
            _BaseStationDatabase.Object.FileName = null;

            _Plugin.Startup(_StartupParameters);

            Assert.AreNotEqual(0, _StatusChangedEvent.CallCount);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            Assert.AreEqual("Enabled, database not specified", _Plugin.Status);
            Assert.AreEqual(null, _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Startup_Disables_Cache_If_Database_Is_Null()
        {
            SetEnabledOption(true);
            SetOnlineCacheEnabled(true);
            _BaseStationDatabase.Object.FileName = null;

            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Sets_Status_If_Database_Does_Not_Exist()
        {
            SetEnabledOption(true);
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;
            _BaseStationDatabase.Object.FileName = @"c:\folder\database.sqb";
            _Provider.Setup(p => p.FileExists(@"c:\folder\database.sqb")).Returns(false);

            _Plugin.Startup(_StartupParameters);

            Assert.AreNotEqual(0, _StatusChangedEvent.CallCount);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            Assert.AreEqual("Enabled but not updating database", _Plugin.Status);
            Assert.AreEqual(@"'c:\folder\database.sqb' does not exist", _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Startup_Disables_Cache_If_Database_Does_Not_Exist()
        {
            SetEnabledOption(true);
            SetOnlineCacheEnabled(true);
            _BaseStationDatabase.Object.FileName = @"c:\folder\database.sqb";
            _Provider.Setup(p => p.FileExists(@"c:\folder\database.sqb")).Returns(false);

            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Sets_Status_If_Database_Exists_But_Is_Zero_Length()
        {
            SetEnabledOption(true);
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;
            _BaseStationDatabase.Object.FileName = @"c:\folder\database.sqb";
            _Provider.Setup(p => p.FileExists(@"c:\folder\database.sqb")).Returns(true);
            _Provider.Setup(p => p.FileSize(@"c:\folder\database.sqb")).Returns(0L);

            _Plugin.Startup(_StartupParameters);

            Assert.AreNotEqual(0, _StatusChangedEvent.CallCount);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            Assert.AreEqual("Enabled but not updating database", _Plugin.Status);
            Assert.AreEqual(@"'c:\folder\database.sqb' is zero length", _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Startup_Disables_Cache_If_Database_Does_Not_Exist_But_Is_Zero_Length()
        {
            SetEnabledOption(true);
            SetOnlineCacheEnabled(true);
            _BaseStationDatabase.Object.FileName = @"c:\folder\database.sqb";
            _Provider.Setup(p => p.FileExists(@"c:\folder\database.sqb")).Returns(true);
            _Provider.Setup(p => p.FileSize(@"c:\folder\database.sqb")).Returns(0L);

            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Sets_Status_If_Failed_To_Start_Session()
        {
            var exception = new InvalidOperationException();
            SetEnabledOption(true);
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback(() => { throw exception; });

            _Plugin.Startup(_StartupParameters);

            Assert.IsTrue(_StatusChangedEvent.CallCount > 0);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            Assert.AreEqual("Enabled but not updating database", _Plugin.Status);
            Assert.AreEqual(String.Format("Exception caught when starting session: {0}", exception.Message), _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Startup_Disables_Cache_If_Failed_To_Start_Session()
        {
            SetEnabledOption(true);
            SetOnlineCacheEnabled(true);
            var exception = new InvalidOperationException();
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback(() => { throw exception; });

            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Records_Exception_In_Log_If_Session_Start_Throws()
        {
            var exception = new InvalidOperationException();
            SetEnabledOption(true);
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback(() => { throw exception; });

            _Plugin.Startup(_StartupParameters);

            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), exception.ToString()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Startup_Copes_If_No_Locations_Are_Specified()
        {
            _BaseStationDatabase.Setup(d => d.GetLocations()).Returns(new List<BaseStationLocation>());
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession s) => {
                Assert.AreEqual(0, s.LocationID);
            });

            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Startup_Does_Not_Start_Session_If_Database_Not_Created_By_Plugin()
        {
            SetDBHistory(false);
            SetEnabledOption(true);

            _Plugin.Startup(_StartupParameters);

            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Startup_Writes_Correct_Status_If_Database_Not_Created_By_Plugin()
        {
            SetDBHistory(false);
            SetEnabledOption(true);

            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual("Enabled but not updating database", _Plugin.Status);
            Assert.AreEqual("Settings prevent update of databases not created by plugin", _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Startup_Disables_Cache_If_Database_Not_Created_By_Plugin()
        {
            SetDBHistory(false);
            SetEnabledOption(true);
            SetOnlineCacheEnabled(true);

            _Plugin.Startup(_StartupParameters);

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Startup_Assumes_That_Databases_With_No_History_Were_Not_Created_By_Plugin()
        {
            _BaseStationDatabase.Setup(d => d.GetDatabaseHistory()).Returns(new List<BaseStationDBHistory>());
            SetEnabledOption(true);

            _Plugin.Startup(_StartupParameters);

            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Startup_Searches_History_For_VRS_Plugin_Marker()
        {
            var history = new List<BaseStationDBHistory>() {
                new BaseStationDBHistory() { Description = "Something" },
                new BaseStationDBHistory() { Description = "Database autocreated by Virtual Radar Server" },
                new BaseStationDBHistory() { Description = "Database autocreated by Snoopy" },
            };
            _BaseStationDatabase.Setup(d => d.GetDatabaseHistory()).Returns(history);
            SetEnabledOption(true);

            _Plugin.Startup(_StartupParameters);

            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Startup_Initialises_Private_Heartbeat_Service()
        {
            _Plugin.Startup(_StartupParameters);

            _HeartbeatService.Verify(r => r.Start(), Times.Once());
        }
        #endregion

        #region Shutdown
        [TestMethod]
        public void Plugin_Shutdown_Records_Time_Of_Shutdown_In_Session()
        {
            var startTime = new DateTime(2001, 2, 3, 4, 5, 6);
            var endTime = startTime.AddHours(12);

            BaseStationSession session = null;
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession s) => { session = s; });

            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession s) => {
                Assert.AreSame(session, s);
                Assert.AreEqual(startTime, s.StartTime);
                Assert.AreEqual(endTime, s.EndTime);
                Assert.AreEqual(9821, s.LocationID);
            });

            SetEnabledOption(true);

            _Provider.Setup(p => p.LocalNow).Returns(startTime);
            _Plugin.Startup(_StartupParameters);

            _Provider.Setup(p => p.LocalNow).Returns(endTime);
            _Plugin.Shutdown();

            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Shutdown_Flushes_All_Tracked_Flights_To_Disk()
        {
            SetEnabledOption(true);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir, Icao24 = "Y" };
            _Listener.Raise(m => m.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight flight) => {
                Assert.AreEqual(_Provider.Object.LocalNow, flight.EndTime);
                Assert.AreEqual(1, flight.NumModeSMsgRec);
            });

            _Plugin.Shutdown();

            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Shutdown_Disables_Online_Cache()
        {
            SetEnabledOption(true);
            SetOnlineCacheEnabled(true);

            _Plugin.Startup(_StartupParameters);
            _Plugin.Shutdown();

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Shutdown_Sets_Status()
        {
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;
            _StatusChangedEvent.EventRaised += (s, a) => {
                Assert.AreEqual("Not updating database", _Plugin.Status);
                Assert.AreEqual(null, _Plugin.StatusDescription);
            };

            _Plugin.Shutdown();

            Assert.AreEqual(1, _StatusChangedEvent.CallCount);
            Assert.AreNotEqual(null, _StatusChangedEvent.Args);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
        }

        [TestMethod]
        public void Plugin_Shutdown_Sets_Status_Correctly_If_Exception_Raised_During_Session_Shutdown()
        {
            var exception = new InvalidOperationException();
            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback(() => { throw exception; });

            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);
            _Plugin.StatusChanged += _StatusChangedEvent.Handler;

            _Plugin.Shutdown();

            Assert.IsTrue(_StatusChangedEvent.CallCount > 0);
            Assert.AreNotEqual(null, _StatusChangedEvent.Args);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            Assert.AreEqual("Not updating database", _Plugin.Status);
            Assert.AreEqual(String.Format("Exception caught when closing session: {0}", exception.Message), _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Shutdown_Logs_Exceptions_Raised_During_Session_Shutdown()
        {
            var exception = new InvalidOperationException();
            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback(() => { throw exception; });

            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);
            _Plugin.Shutdown();

            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), exception.ToString()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Shutdown_Exceptions_Raised_During_Session_Shutdown_Still_Close_Session()
        {
            // If an exception is raised during shutdown then the internal state must still go to "no session open" - a further attempt to
            // shutdown the object shouldn't make another attempt to close the session.

            var exception = new InvalidOperationException();
            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback(() => { throw exception; });

            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);
            _Plugin.Shutdown();
            _Plugin.Shutdown();

            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Shutdown_Exceptions_Raised_During_Session_Shutdown_Still_Disable_Online_Cache()
        {
            var exception = new InvalidOperationException();
            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback(() => { throw exception; });

            SetOnlineCacheEnabled(true);
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);
            _Plugin.Shutdown();
            _Plugin.Shutdown();

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_Shutdown_Does_Nothing_If_Plugin_Disabled()
        {
            SetEnabledOption(false);

            _Plugin.Startup(_StartupParameters);
            _Plugin.Shutdown();

            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Never());
        }
        #endregion

        #region ShowWinFormsOptionsUI
        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Displays_View_To_User()
        {
            _Plugin.Startup(_StartupParameters);
            _Plugin.ShowWinFormsOptionsUI();

            _OptionsView.Verify(v => v.DisplayView(), Times.Once());
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Disposes_Of_View_After_Use()
        {
            _OptionsView.Setup(v => v.DisplayView()).Callback(() => {
                _OptionsView.Verify(v => v.Dispose(), Times.Never());
            });

            _Plugin.ShowWinFormsOptionsUI();

            _OptionsView.Verify(v => v.Dispose(), Times.Once());
        }

        [TestMethod]
        [DataSource("Data Source='PluginBaseStationDatabaseWriterTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "OptionsView$")]
        public void Plugin_ShowWinFormsOptionsUI_Sets_Options_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            SetEnabledOption(worksheet.Bool("Enabled"));
            SetAllowUpdateOfOtherDatabasesOption(worksheet.Bool("AllowUpdateOfOtherDatabases"));
            SetReceiverIdOption(worksheet.Int("ReceiverId"));
            SetOnlineCacheEnabled(worksheet.Bool("SaveDownloadedAircraftDetails"));
            SetRefreshOutOfDateAircraft(worksheet.Bool("RefreshOutOfDateAircraft"));
            _Configuration.BaseStationSettings.DatabaseFileName = worksheet.EString("DatabaseFileName");

            _OptionsView.Setup(v => v.DisplayView()).Callback(() => {
                _OptionsView.VerifySet(v => v.PluginEnabled = worksheet.Bool("Enabled"), Times.Once());
                _OptionsView.VerifySet(v => v.AllowUpdateOfOtherDatabases = worksheet.Bool("AllowUpdateOfOtherDatabases"), Times.Once());
                _OptionsView.VerifySet(v => v.DatabaseFileName = worksheet.EString("DatabaseFileName"), Times.Once());
                _OptionsView.VerifySet(v => v.ReceiverId = worksheet.Int("ReceiverId"), Times.Once());
                _OptionsView.VerifySet(v => v.SaveDownloadedAircraftDetails = worksheet.Bool("SaveDownloadedAircraftDetails"), Times.Once());
                _OptionsView.VerifySet(v => v.RefreshOutOfDateAircraft = worksheet.Bool("RefreshOutOfDateAircraft"), Times.Once());
            });

            _Plugin.Startup(_StartupParameters);
            _Plugin.ShowWinFormsOptionsUI();
        }

        [TestMethod]
        [DataSource("Data Source='PluginBaseStationDatabaseWriterTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "OptionsView$")]
        public void Plugin_ShowWinFormsOptionsUI_Save_Changes()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Plugin.Startup(_StartupParameters);

            _OptionsView.Setup(v => v.PluginEnabled).Returns(worksheet.Bool("Enabled"));
            _OptionsView.Setup(v => v.AllowUpdateOfOtherDatabases).Returns(worksheet.Bool("AllowUpdateOfOtherDatabases"));
            _OptionsView.Setup(v => v.DatabaseFileName).Returns(worksheet.EString("DatabaseFileName"));
            _OptionsView.Setup(v => v.ReceiverId).Returns(worksheet.Int("ReceiverId"));
            _OptionsView.Setup(v => v.SaveDownloadedAircraftDetails).Returns(worksheet.Bool("SaveDownloadedAircraftDetails"));
            _OptionsView.Setup(v => v.RefreshOutOfDateAircraft).Returns(worksheet.Bool("RefreshOutOfDateAircraft"));

            _PluginSettingsStorage.Setup(s => s.Save(It.IsAny<PluginSettings>())).Callback((PluginSettings settings) => {
                var jsonText = JsonConvert.SerializeObject(new Options() {
                    Enabled =                       worksheet.Bool("Enabled"),
                    AllowUpdateOfOtherDatabases =   worksheet.Bool("AllowUpdateOfOtherDatabases"),
                    ReceiverId =                    worksheet.Int("ReceiverId"),
                    SaveDownloadedAircraftDetails = worksheet.Bool("SaveDownloadedAircraftDetails"),
                    RefreshOutOfDateAircraft =      worksheet.Bool("RefreshOutOfDateAircraft"),
                });
                Assert.AreEqual(jsonText, settings.ReadString(_Plugin, "Options"));
            });

            _ConfigurationStorage.Setup(s => s.Save(It.IsAny<Configuration>())).Callback((Configuration configuration) => {
                Assert.AreEqual(worksheet.EString("DatabaseFileName"), configuration.BaseStationSettings.DatabaseFileName);
            });

            _Plugin.ShowWinFormsOptionsUI();

            _PluginSettingsStorage.Verify(s => s.Save(It.IsAny<PluginSettings>()), Times.Once());
            _ConfigurationStorage.Verify(s => s.Save(It.IsAny<Configuration>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Does_Not_Save_Changes_If_User_Cancels()
        {
            _OptionsView.Setup(v => v.DisplayView()).Returns(false);

            _Plugin.ShowWinFormsOptionsUI();

            _PluginSettingsStorage.Verify(s => s.Save(It.IsAny<PluginSettings>()), Times.Never());
            _ConfigurationStorage.Verify(s => s.Save(It.IsAny<Configuration>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Disabling_Plugin_Closes_Session()
        {
            _OptionsView.Setup(v => v.PluginEnabled).Returns(false);

            BaseStationSession session = null;
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession s) => { session = s; });
            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession s) => {
                Assert.AreSame(session, s);
                Assert.AreEqual(_Provider.Object.LocalNow, s.EndTime);
            });

            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Once());

            _Plugin.Shutdown();
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Disabling_Plugin_Disables_Online_Cache()
        {
            _OptionsView.Setup(v => v.PluginEnabled).Returns(false);

            SetOnlineCacheEnabled(true);
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Sets_RefreshOutOfDateAircraft_On_Online_Cache()
        {
            _OptionsView.Setup(v => v.RefreshOutOfDateAircraft).Returns(true);

            SetRefreshOutOfDateAircraft(false);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();

            Assert.AreEqual(true, _OnlineLookupCache.Object.RefreshOutOfDateAircraft);
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Clears_RefreshOutOfDateAircraft_On_Online_Cache()
        {
            _OptionsView.Setup(v => v.RefreshOutOfDateAircraft).Returns(false);

            SetRefreshOutOfDateAircraft(true);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();

            Assert.AreEqual(false, _OnlineLookupCache.Object.RefreshOutOfDateAircraft);
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Disabling_Plugin_Flushes_All_Tracked_Flights_To_Disk()
        {
            SetEnabledOption(true);
            _OptionsView.Setup(v => v.PluginEnabled).Returns(false);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir, Icao24 = "Y" };
            _Listener.Raise(m => m.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight flight) => {
                Assert.AreEqual(_Provider.Object.LocalNow, flight.EndTime);
                Assert.AreEqual(1, flight.NumModeSMsgRec);
            });

            _Plugin.ShowWinFormsOptionsUI();

            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Leaving_Plugin_Disabled_Does_Not_Close_Session()
        {
            _OptionsView.Setup(v => v.PluginEnabled).Returns(false);

            SetEnabledOption(false);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Leaving_Plugin_Enabled_Does_Not_Close_Session()
        {
            _OptionsView.Setup(v => v.PluginEnabled).Returns(true);

            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Enabling_Plugin_And_Disabling_Cache_Disables_Cache()
        {
            _OptionsView.Setup(v => v.SaveDownloadedAircraftDetails).Returns(false);

            SetOnlineCacheEnabled(true);
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();

            Assert.AreEqual(false, _OnlineLookupCache.Object.Enabled);
        }

        [TestMethod]
        public void Plugin_ShowWinFormsOptionsUI_Enabling_Cache_Enables_Cache()
        {
            _OptionsView.Setup(v => v.SaveDownloadedAircraftDetails).Returns(true);

            SetOnlineCacheEnabled(false);
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();

            Assert.AreEqual(true, _OnlineLookupCache.Object.Enabled);
        }
        #endregion

        #region Feed Change Handling
        [TestMethod]
        public void Plugin_Hooks_Feed_If_Changed_In_Options()
        {
            BaseStationAircraft aircraft = new BaseStationAircraft() { AircraftID = 5832, ModeS = "X" };
            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns(aircraft);

            SetEnabledOption(true);
            _OptionsView.Setup(v => v.ReceiverId).Returns(2);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();

            _Listener = _Listeners[1];

            var message = new BaseStationMessage() { MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir, Icao24 = "X" };
            _Listener.Raise(m => m.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Unhooks_Old_Feed_If_Changed_In_Options()
        {
            BaseStationAircraft aircraft = new BaseStationAircraft() { AircraftID = 5832, ModeS = "X" };
            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns(aircraft);

            SetEnabledOption(true);
            _OptionsView.Setup(v => v.ReceiverId).Returns(2);
            _Plugin.Startup(_StartupParameters);

            _Plugin.ShowWinFormsOptionsUI();

            var message = new BaseStationMessage() { MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir, Icao24 = "X" };
            _Listener.Raise(m => m.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Unhooks_Old_Feed_If_Disabled_In_Configuration_Changes()
        {
            SetEnabledOption(true);
            BaseStationAircraft aircraft = new BaseStationAircraft() { AircraftID = 5832, ModeS = "X" };
            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns(aircraft);
            _Plugin.Startup(_StartupParameters);

            FeedHelper.RemoveFeed(_Feeds, _Listeners, 1);
            _FeedManger.Raise(r => r.FeedsChanged += null, EventArgs.Empty);

            var message = new BaseStationMessage() { MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir, Icao24 = "X" };
            _Listener.Raise(m => m.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Unhooks_New_Feed_After_Configuration_Change_If_Originally_Disabled()
        {
            SetReceiverIdOption(3);
            SetEnabledOption(true);
            BaseStationAircraft aircraft = new BaseStationAircraft() { AircraftID = 5832, ModeS = "X" };
            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns(aircraft);
            _Plugin.Startup(_StartupParameters);

            FeedHelper.AddFeeds(_Feeds, _Listeners, 3);
            _Listener = _Listeners[2];
            _FeedManger.Raise(r => r.FeedsChanged += null, EventArgs.Empty);

            var message = new BaseStationMessage() { MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir, Icao24 = "X" };
            _Listener.Raise(m => m.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Once());
        }
        #endregion

        #region IBaseStationDatabase.FileNameChanging event
        [TestMethod]
        public void Plugin_DatabaseFileNameChanging_Closes_Current_Session()
        {
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession session) => {
                Assert.IsNotNull(session.EndTime);
            });

            _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_DatabaseFileNameChanging_Flushes_All_Tracked_Flights_To_Disk()
        {
            SetEnabledOption(true);
            _OptionsView.Setup(v => v.PluginEnabled).Returns(false);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir, Icao24 = "Y" };
            _Listener.Raise(m => m.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight flight) => {
                Assert.AreEqual(_Provider.Object.LocalNow, flight.EndTime);
                Assert.AreEqual(1, flight.NumModeSMsgRec);
            });

            _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_DatabaseFileNameChanging_Closes_Current_Session_And_Prevents_Double_Close_Of_Session()
        {
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Once());

            _Plugin.Shutdown();
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_DatabaseFileNameChanging_Does_Not_Close_Session_If_Not_Already_Open()
        {
            SetEnabledOption(false);
            _Plugin.Startup(_StartupParameters);

            _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Never());
        }
        #endregion

        #region IBaseStationDatabase.FileNameChanged event
        [TestMethod]
        public void Plugin_DatabaseFileNameChanged_Creates_New_Session_If_Plugin_Is_Enabled()
        {
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);                                            // <-- creates a session
            _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);   // <-- closes the session

            var timeStamp = new DateTime(1999, 8, 7, 6, 5, 4);
            _Provider.Setup(p => p.LocalNow).Returns(timeStamp);

            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession session) => {
                Assert.AreEqual(timeStamp, session.StartTime);
                Assert.AreEqual(null, session.EndTime);
                Assert.AreEqual(9821, session.LocationID);
            });

            _BaseStationDatabase.Raise(d => d.FileNameChanged += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Plugin_DatabaseFileNameChanged_Creates_New_Session_That_Shutdown_Can_Close()
        {
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);                                            // <-- creates a session
            _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);   // <-- closes the session

            _BaseStationDatabase.Raise(d => d.FileNameChanged += null, EventArgs.Empty);
            _Plugin.Shutdown();

            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Plugin_DatabaseFileNameChanged_Does_Not_Create_Session_If_Plugin_Disabled()
        {
            SetEnabledOption(false);
            _Plugin.Startup(_StartupParameters);                                            // <-- does not create a session, plugin disabled
            _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);   // <-- does not close session

            _BaseStationDatabase.Raise(d => d.FileNameChanged += null, EventArgs.Empty);    // <-- should not create a new session!

            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_DatabaseFileNameChanged_Will_Start_Session_As_Result_Of_Name_Change_In_Options()
        {
            _BaseStationDatabase.Setup(d => d.FileName).Returns("OLD");
            SetEnabledOption(false);
            _Plugin.Startup(_StartupParameters);

            _OptionsView.Setup(v => v.PluginEnabled).Returns(true);
            _OptionsView.Setup(v => v.DatabaseFileName).Returns("NEW");
            _ConfigurationStorage.Setup(c => c.Save(_Configuration)).Callback(() => {
                _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);
                Assert.AreEqual(false, _BaseStationDatabase.Object.WriteSupportEnabled);
                _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Never());
                _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Never());

                _BaseStationDatabase.Setup(d => d.FileName).Returns("NEW");

                _BaseStationDatabase.Raise(d => d.FileNameChanged += null, EventArgs.Empty);
                Assert.AreEqual(true, _BaseStationDatabase.Object.WriteSupportEnabled);
                _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Once());
            });

            _Plugin.ShowWinFormsOptionsUI();

            _ConfigurationStorage.Verify(c => c.Save(_Configuration), Times.Once());
        }
        #endregion

        #region IListener.SourceChanged event
        [TestMethod]
        public void Plugin_Listener_SourceChanged_Closes_Current_Session()
        {
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession session) => {
                Assert.IsNotNull(session.EndTime);
            });

            _Listener.Raise(m => m.SourceChanged += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Listener_SourceChanged_Creates_New_Session()
        {
            SetEnabledOption(true);
            _Plugin.Startup(_StartupParameters);

            var timeStamp = new DateTime(1999, 8, 7, 6, 5, 4);
            _Provider.Setup(p => p.LocalNow).Returns(timeStamp);

            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession session) => {
                Assert.AreEqual(timeStamp, session.StartTime);
                Assert.AreEqual(null, session.EndTime);
                Assert.AreEqual(9821, session.LocationID);
            });

            _Listener.Raise(m => m.SourceChanged += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Exactly(2));
        }
        #endregion

        #region Session handling over lifetime of plugin
        [TestMethod]
        [DataSource("Data Source='PluginBaseStationDatabaseWriterTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "StartupSessionState$")]
        public void Plugin_Sessions_Are_Created_And_Destroyed_Correctly_From_Startup_Over_Config_Change_To_Shutdown()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            // Most of the testing takes place when InsertSession (create new session) and UpdateSession (close existing session) are
            // called. The test basically updates the following few parameters and then the Insert / Update calls check them
            int expectInsertCallCount = 0, expectUpdateCallCount = 0;
            string expectInsertFileName = null, expectUpdateFileName = null;
            DateTime localNow = SetProviderTimes(DateTime.Now);
            BaseStationSession lastSessionInserted = null;
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession session) => {
                lastSessionInserted = session;
                Assert.IsTrue(_BaseStationDatabase.Object.WriteSupportEnabled);
                Assert.AreEqual(expectInsertFileName, _BaseStationDatabase.Object.FileName);
                Assert.AreEqual(null, session.EndTime);
                Assert.AreEqual(localNow, session.StartTime);
                Assert.AreEqual(9821, session.LocationID);
            });
            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession session) => {
                Assert.IsTrue(_BaseStationDatabase.Object.WriteSupportEnabled);
                Assert.AreEqual(expectUpdateFileName, _BaseStationDatabase.Object.FileName);
                Assert.AreSame(lastSessionInserted, session);
                Assert.AreEqual(localNow, session.EndTime);
            });

            // Setup ConfigurationStorage so that if the configuration changes the database filename we raise the right events
            _ConfigurationStorage.Setup(c => c.Save(It.IsAny<Configuration>())).Callback((Configuration c) => {
                var configFileName = c.BaseStationSettings.DatabaseFileName;
                bool isNewFileName = configFileName != _BaseStationDatabase.Object.FileName;
                if(isNewFileName) _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);
                _BaseStationDatabase.Setup(d => d.FileName).Returns(configFileName);
                if(isNewFileName) _BaseStationDatabase.Raise(d => d.FileNameChanged += null, EventArgs.Empty);
            });

            // Create the conditions present at plugin startup
            SetEnabledOption(worksheet.Bool("InitialEnabled"));
            SetAllowUpdateOfOtherDatabasesOption(worksheet.Bool("InitialAllowForeignUpdate"));
            SetConfigurationBaseStationDatabaseFileName(worksheet.EString("InitialDB"));
            expectInsertFileName = worksheet.EString("InitialDB");
            _BaseStationDatabase.Setup(d => d.FileName).Returns(expectInsertFileName);
            SetDBHistory(worksheet.Bool("InitialDBIsVRS"));

            // Start the plugin
            _Plugin.Startup(_StartupParameters);

            // Check that the session was created, or not
            if(worksheet.Bool("ExpectStartOnStartup")) ++expectInsertCallCount;
            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectInsertCallCount));
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectUpdateCallCount));

            // Change the configuration
            expectUpdateFileName = expectInsertFileName;
            expectInsertFileName = worksheet.EString("ConfigDB");
            _OptionsView.Setup(v => v.PluginEnabled).Returns(worksheet.Bool("ConfigEnabled"));
            _OptionsView.Setup(v => v.AllowUpdateOfOtherDatabases).Returns(worksheet.Bool("ConfigAllowForeignUpdate"));
            _OptionsView.Setup(v => v.DatabaseFileName).Returns(expectInsertFileName);
            SetDBHistory(worksheet.Bool("ConfigDBIsVRS"));

            _Plugin.ShowWinFormsOptionsUI();

            if(worksheet.Bool("ExpectCloseOnConfig")) ++expectUpdateCallCount;
            if(worksheet.Bool("ExpectStartOnConfig")) ++expectInsertCallCount;
            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectInsertCallCount));
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectUpdateCallCount));

            // Shutdown the plugin and make sure sessions are correctly closed etc.
            expectUpdateFileName = expectInsertFileName;

            _Plugin.Shutdown();

            if(worksheet.Bool("ExpectCloseOnShutdown")) ++expectUpdateCallCount;
            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectInsertCallCount));
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectUpdateCallCount));
        }

        [TestMethod]
        [DataSource("Data Source='PluginBaseStationDatabaseWriterTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GlobalConfigSessionState$")]
        public void Plugin_Sessions_Are_Created_And_Destroyed_Correctly_From_Startup_Over_Global_Config_Change_To_Shutdown()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            // Most of the testing takes place when InsertSession (create new session) and UpdateSession (close existing session) are
            // called. The test basically updates the following few parameters and then the Insert / Update calls check them
            int expectInsertCallCount = 0, expectUpdateCallCount = 0;
            string expectInsertFileName = null, expectUpdateFileName = null;
            DateTime localNow = SetProviderTimes(DateTime.Now);
            BaseStationSession lastSessionInserted = null;
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession session) => {
                lastSessionInserted = session;
                Assert.IsTrue(_BaseStationDatabase.Object.WriteSupportEnabled);
                Assert.AreEqual(expectInsertFileName, _BaseStationDatabase.Object.FileName);
                Assert.AreEqual(null, session.EndTime);
                Assert.AreEqual(localNow, session.StartTime);
                Assert.AreEqual(9821, session.LocationID);
            });
            _BaseStationDatabase.Setup(d => d.UpdateSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession session) => {
                Assert.IsTrue(_BaseStationDatabase.Object.WriteSupportEnabled);
                Assert.AreEqual(expectUpdateFileName, _BaseStationDatabase.Object.FileName);
                Assert.AreSame(lastSessionInserted, session);
                Assert.AreEqual(localNow, session.EndTime);
            });

            // Create the conditions present at plugin startup
            SetEnabledOption(worksheet.Bool("InitialEnabled"));
            SetAllowUpdateOfOtherDatabasesOption(worksheet.Bool("InitialAllowForeignUpdate"));
            SetConfigurationBaseStationDatabaseFileName(worksheet.EString("InitialDB"));
            expectInsertFileName = worksheet.EString("InitialDB");
            _BaseStationDatabase.Setup(d => d.FileName).Returns(expectInsertFileName);
            SetDBHistory(worksheet.Bool("InitialDBIsVRS"));

            // Start the plugin
            _Plugin.Startup(_StartupParameters);

            // Check that the session was created, or not
            if(worksheet.Bool("ExpectStartOnStartup")) ++expectInsertCallCount;
            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectInsertCallCount));
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectUpdateCallCount));

            // Change the global configuration
            expectUpdateFileName = expectInsertFileName;
            expectInsertFileName = worksheet.EString("GlobalDB");
            _Configuration.BaseStationSettings.DatabaseFileName = expectInsertFileName;
            SetDBHistory(worksheet.Bool("GlobalDBIsVRS"));

            // Raise the events that we expect to see when the filename changes
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);
            if(expectUpdateFileName != expectInsertFileName) {
                _BaseStationDatabase.Raise(d => d.FileNameChanging += null, EventArgs.Empty);
                _BaseStationDatabase.Setup(d => d.FileName).Returns(expectInsertFileName);
                _BaseStationDatabase.Raise(d => d.FileNameChanged += null, EventArgs.Empty);
            }

            if(worksheet.Bool("ExpectCloseOnConfig")) ++expectUpdateCallCount;
            if(worksheet.Bool("ExpectStartOnConfig")) ++expectInsertCallCount;
            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectInsertCallCount));
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectUpdateCallCount));

            // Shutdown the plugin and make sure sessions are correctly closed etc.
            expectUpdateFileName = expectInsertFileName;

            _Plugin.Shutdown();

            if(worksheet.Bool("ExpectCloseOnShutdown")) ++expectUpdateCallCount;
            _BaseStationDatabase.Verify(d => d.InsertSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectInsertCallCount));
            _BaseStationDatabase.Verify(d => d.UpdateSession(It.IsAny<BaseStationSession>()), Times.Exactly(expectUpdateCallCount));
        }
        #endregion

        #region Database Aircraft and Flight record creation
        [TestMethod]
        public void Plugin_Aircraft_Record_Written_When_First_Message_From_Aircraft_Received()
        {
            SetEnabledOption(true);

            BaseStationAircraft aircraft = null;
            _BaseStationDatabase.Setup(d => d.GetOrInsertAircraftByCode("X", It.IsAny<Func<string, BaseStationAircraft>>()))
                                .Returns((string icao, Func<string, BaseStationAircraft> callback) => {
                                    aircraft = callback(icao);
                                    return aircraft;
                                });

            var codeBlock = new CodeBlock() { Country = "Elbonia" };
            _StandingDataManager.Setup(m => m.FindCodeBlock("X")).Returns(codeBlock);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.GetOrInsertAircraftByCode(It.IsAny<string>(), It.IsAny<Func<string, BaseStationAircraft>>()), Times.Once());
            Assert.AreEqual(0, aircraft.AircraftID);
            Assert.AreEqual("X", aircraft.ModeS);
            Assert.AreEqual("Elbonia", aircraft.ModeSCountry);
            Assert.AreEqual(_Provider.Object.LocalNow, aircraft.FirstCreated);
            Assert.AreEqual(_Provider.Object.LocalNow, aircraft.LastModified);
            Assert.AreEqual(null, aircraft.AircraftClass);
            Assert.AreEqual(null, aircraft.CofACategory);
            Assert.AreEqual(null, aircraft.CofAExpiry);
            Assert.AreEqual(null, aircraft.Country);
            Assert.AreEqual(null, aircraft.CurrentRegDate);
            Assert.AreEqual(null, aircraft.DeRegDate);
            Assert.AreEqual(null, aircraft.Engines);
            Assert.AreEqual(null, aircraft.FirstRegDate);
            Assert.AreEqual(null, aircraft.GenericName);
            Assert.AreEqual(null, aircraft.ICAOTypeCode);
            Assert.AreEqual(null, aircraft.InfoUrl);
            Assert.AreEqual(false, aircraft.Interested);
            Assert.AreEqual(null, aircraft.Manufacturer);
            Assert.AreEqual(null, aircraft.MTOW);
            Assert.AreEqual(null, aircraft.OperatorFlagCode);
            Assert.AreEqual(null, aircraft.OwnershipStatus);
            Assert.AreEqual(null, aircraft.PictureUrl1);
            Assert.AreEqual(null, aircraft.PictureUrl2);
            Assert.AreEqual(null, aircraft.PictureUrl3);
            Assert.AreEqual(null, aircraft.PopularName);
            Assert.AreEqual(null, aircraft.PreviousID);
            Assert.AreEqual(null, aircraft.RegisteredOwners);
            Assert.AreEqual(null, aircraft.Registration);
            Assert.AreEqual(null, aircraft.SerialNo);
            Assert.AreEqual(null, aircraft.Status);
            Assert.AreEqual(null, aircraft.TotalHours);
            Assert.AreEqual(null, aircraft.Type);
            Assert.AreEqual(null, aircraft.UserNotes);
            Assert.AreEqual(null, aircraft.YearBuilt);
        }

        [TestMethod]
        public void Plugin_Aircraft_ModeSCountry_Filled_Correctly_When_Country_Is_Unknown()
        {
            SetEnabledOption(true);

            bool sawInsertOfNull = false;
            _StandingDataManager.Setup(m => m.FindCodeBlock("X")).Returns((CodeBlock)null);
            _BaseStationDatabase.Setup(d => d.GetOrInsertAircraftByCode("X", It.IsAny<Func<string, BaseStationAircraft>>()))
                                .Returns((string icao, Func<string, BaseStationAircraft> callback) => {
                                    var result = callback(icao);
                                    sawInsertOfNull = result.ModeSCountry == null;
                                    return result;
                                });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.IsTrue(sawInsertOfNull);
        }

        [TestMethod]
        public void Plugin_Aircraft_ModeSCountry_Filled_Correctly_When_Country_Name_Starts_With_Unknown_Space()
        {
            SetEnabledOption(true);

            bool sawInsertOfNull = false;
            _StandingDataManager.Setup(m => m.FindCodeBlock("X")).Returns(new CodeBlock() {
                Country = "Unknown or unassigned country",
            });
            _BaseStationDatabase.Setup(d => d.GetOrInsertAircraftByCode("X", It.IsAny<Func<string, BaseStationAircraft>>()))
                                .Returns((string icao, Func<string, BaseStationAircraft> callback) => {
                                    var result = callback(icao);
                                    sawInsertOfNull = result.ModeSCountry == null;
                                    return result;
                                });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.IsTrue(sawInsertOfNull);
        }

        [TestMethod]
        public void Plugin_Aircraft_Missing_ModeSCountry_Updated_If_StandingData_Reloaded()
        {
            SetEnabledOption(true);

            _StandingDataManager.Setup(m => m.FindCodeBlock("X")).Returns((CodeBlock)null);
            _BaseStationDatabase.Setup(d => d.GetOrInsertAircraftByCode("X", It.IsAny<Func<string, BaseStationAircraft>>()))
                                .Returns(new BaseStationAircraft() { AircraftID = 100, ModeS = "X", });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _StandingDataManager.Setup(m => m.FindCodeBlock("X")).Returns(new CodeBlock() { Country = "Y" });
            _StandingDataManager.Raise(r => r.LoadCompleted += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.UpdateAircraftModeSCountry(100, "Y"), Times.Once());
        }

        [TestMethod]
        public void Plugin_Aircraft_Missing_ModeSCountry_Not_Updated_If_CodeBlock_Cannot_Be_Found()
        {
            SetEnabledOption(true);

            _StandingDataManager.Setup(m => m.FindCodeBlock("X")).Returns((CodeBlock)null);
            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns((BaseStationAircraft)null);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _StandingDataManager.Setup(m => m.FindCodeBlock("X")).Returns((CodeBlock)null);
            _StandingDataManager.Raise(r => r.LoadCompleted += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.UpdateAircraftModeSCountry(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Aircraft_Missing_ModeSCountry_Not_Updated_If_CodeBlock_Has_Not_Changed()
        {
            SetEnabledOption(true);

            _StandingDataManager.Setup(m => m.FindCodeBlock("X")).Returns(new CodeBlock() { Country = "UK" });
            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns((BaseStationAircraft)null);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _StandingDataManager.Raise(r => r.LoadCompleted += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.UpdateAircraftModeSCountry(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Flight_Record_Written_When_First_Message_From_Aircraft_Received()
        {
            SetEnabledOption(true);

            _BaseStationDatabase.Setup(d => d.InsertFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight flight) => {
                Assert.AreEqual(0, flight.FlightID);
                Assert.AreEqual(5483, flight.AircraftID);
                Assert.AreEqual(42, flight.SessionID);
                Assert.AreEqual(null, flight.Aircraft);
                Assert.AreEqual("ABC123", flight.Callsign);
                Assert.AreEqual(_Provider.Object.LocalNow, flight.StartTime);
                Assert.AreEqual(null, flight.EndTime);

                Assert.AreEqual(null, flight.FirstAltitude);
                Assert.AreEqual(null, flight.FirstGroundSpeed);
                Assert.AreEqual(false, flight.FirstIsOnGround);
                Assert.AreEqual(null, flight.FirstLat);
                Assert.AreEqual(null, flight.FirstLon);
                Assert.AreEqual(null, flight.FirstSquawk);
                Assert.AreEqual(null, flight.FirstTrack);
                Assert.AreEqual(null, flight.FirstVerticalRate);
                Assert.AreEqual(false, flight.HadAlert);
                Assert.AreEqual(false, flight.HadEmergency);
                Assert.AreEqual(false, flight.HadSpi);
                Assert.AreEqual(null, flight.LastAltitude);
                Assert.AreEqual(null, flight.LastGroundSpeed);
                Assert.AreEqual(false, flight.LastIsOnGround);
                Assert.AreEqual(null, flight.LastLat);
                Assert.AreEqual(null, flight.LastLon);
                Assert.AreEqual(null, flight.LastSquawk);
                Assert.AreEqual(null, flight.LastTrack);
                Assert.AreEqual(null, flight.LastVerticalRate);
                Assert.AreEqual(null, flight.NumADSBMsgRec);
                Assert.AreEqual(null, flight.NumModeSMsgRec);
                Assert.AreEqual(null, flight.NumSurPosMsgRec);
                Assert.AreEqual(null, flight.NumAirPosMsgRec);
                Assert.AreEqual(null, flight.NumAirVelMsgRec);
                Assert.AreEqual(null, flight.NumSurAltMsgRec);
                Assert.AreEqual(null, flight.NumSurIDMsgRec);
                Assert.AreEqual(null, flight.NumAirToAirMsgRec);
                Assert.AreEqual(null, flight.NumAirCallRepMsgRec);
                Assert.AreEqual(null, flight.NumPosMsgRec);
            });
            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns((BaseStationAircraft)null);
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession s) => { s.SessionID = 42; });
            _BaseStationDatabase.Setup(d => d.InsertAircraft(It.IsAny<BaseStationAircraft>())).Callback((BaseStationAircraft a) => { a.AircraftID = 5483; });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = "ABC123", Altitude = 31, Emergency = true,
                GroundSpeed = 289, IdentActive = true, Latitude = 31.1F, Longitude = 123.1F, MessageGenerated = DateTime.Now, MessageLogged = DateTime.Now, MessageNumber = 12389, MessageType = BaseStationMessageType.Transmission,
                OnGround = true, Squawk = 1293, SquawkHasChanged = true, StatusCode = BaseStationStatusCode.None, Track = 12.4F, TransmissionType = BaseStationTransmissionType.AirToAir, VerticalRate = 18 };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Only_Takes_Notice_Of_Transmission_Messages()
        {
            SetEnabledOption(true);

            _Plugin.Startup(_StartupParameters);

            int expectedInserts = 0;
            int counter = 0;

            var messageTypes =
                from        m in Enum.GetValues(typeof(BaseStationMessageType)).OfType<BaseStationMessageType>()
                from        t in Enum.GetValues(typeof(BaseStationTransmissionType)).OfType<BaseStationTransmissionType>()
                from        s in Enum.GetValues(typeof(BaseStationStatusCode)).OfType<BaseStationStatusCode>()
                select      new { MessageType = m, TransmissionType = t, StatusCode = s };
            foreach(var messageType in messageTypes) {
                var message = new BaseStationMessage() {
                    MessageType = messageType.MessageType,
                    TransmissionType = messageType.TransmissionType,
                    StatusCode = messageType.StatusCode,
                    Icao24 = counter++.ToString(),
                };

                if(message.MessageType == BaseStationMessageType.Transmission && message.TransmissionType != BaseStationTransmissionType.None) ++expectedInserts;

                _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
                _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

                _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Exactly(expectedInserts), String.Format("{0}/{1}/{2}", messageType.MessageType, messageType.StatusCode, messageType.TransmissionType));
            }
        }

        [TestMethod]
        public void Plugin_Aircraft_And_Flight_Records_Inserted_Within_Transaction()
        {
            SetEnabledOption(true);
            _BaseStationDatabase.Setup(d => d.GetOrInsertAircraftByCode("Z", It.IsAny<Func<string, BaseStationAircraft>>()))
                                .Callback(() => {
                                    _BaseStationDatabase.Verify(d => d.StartTransaction(), Times.AtLeastOnce());
                                })
                                .Returns(new BaseStationAircraft());

            var countStartTransactions = 0;
            var countEndTransactions = 0;
            _BaseStationDatabase.Setup(d => d.StartTransaction()).Callback(() => {
                ++countStartTransactions;
            });
            _BaseStationDatabase.Setup(d => d.EndTransaction()).Callback(() => {
                ++countEndTransactions;
                _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Once());
            });
            _BaseStationDatabase.Setup(d => d.InsertFlight(It.IsAny<BaseStationFlight>())).Callback(() => {
                _BaseStationDatabase.Verify(d => d.GetOrInsertAircraftByCode("Z", It.IsAny<Func<string, BaseStationAircraft>>()), Times.Once());
            });

            _Plugin.Startup(_StartupParameters);

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(new BaseStationMessage() { Icao24 = "Z", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir }));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.EndTransaction(), Times.AtLeastOnce());
            Assert.AreEqual(countStartTransactions, countEndTransactions);
        }

        [TestMethod]
        public void Plugin_Aircraft_Record_Not_Touched_If_Aircraft_Record_Already_Exists_For_Transmitting_Aircraft()
        {
            SetEnabledOption(true);

            BaseStationAircraft aircraft = new BaseStationAircraft();
            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns(aircraft);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X" };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _BaseStationDatabase.Verify(d => d.InsertAircraft(It.IsAny<BaseStationAircraft>()), Times.Never());
            _BaseStationDatabase.Verify(d => d.UpdateAircraft(It.IsAny<BaseStationAircraft>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Flight_Record_Still_Inserted_If_Aircraft_Record_Already_Exists_For_Transmitting_Aircraft()
        {
            SetEnabledOption(true);

            BaseStationFlight flight = null;
            BaseStationAircraft aircraft = new BaseStationAircraft() { AircraftID = 5832, ModeS = "X" };
            _BaseStationDatabase.Setup(d => d.GetOrInsertAircraftByCode("X", It.IsAny<Func<string, BaseStationAircraft>>())).Returns(aircraft);
            _BaseStationDatabase.Setup(d => d.InsertFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Once());
            Assert.AreEqual(5832, flight.AircraftID);
        }

        [TestMethod]
        public void Plugin_Aircraft_Database_Not_Repeatedly_Polled_For_Same_Aircraft()
        {
            SetEnabledOption(true);

            BaseStationAircraft aircraft = new BaseStationAircraft();
            _BaseStationDatabase.Setup(d => d.GetOrInsertAircraftByCode("X", It.IsAny<Func<string, BaseStationAircraft>>())).Returns(aircraft);

            _Plugin.Startup(_StartupParameters);

            var messageX = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            var messageY = new BaseStationMessage() { AircraftId = 42, Icao24 = "Y", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(messageX));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(messageX));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(messageY));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.GetOrInsertAircraftByCode("X", It.IsAny<Func<string, BaseStationAircraft>>()), Times.Once());
            _BaseStationDatabase.Verify(d => d.GetOrInsertAircraftByCode("Y", It.IsAny<Func<string, BaseStationAircraft>>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Flight_Record_Not_Repeatedly_Inserted_For_Same_Aircraft()
        {
            SetEnabledOption(true);

            BaseStationAircraft aircraft = new BaseStationAircraft() { AircraftID = 5832, ModeS = "X" };
            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns(aircraft);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Once());
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Database_Not_Touched_If_Plugin_Disabled()
        {
            SetEnabledOption(false);

            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns((BaseStationAircraft)null);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X" };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _BaseStationDatabase.Verify(d => d.InsertAircraft(It.IsAny<BaseStationAircraft>()), Times.Never());
            _BaseStationDatabase.Verify(d => d.UpdateAircraft(It.IsAny<BaseStationAircraft>()), Times.Never());

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Never());
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Database_Not_Touched_If_Writes_Disallowed()
        {
            SetEnabledOption(true);
            SetDBHistory(false);

            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("X")).Returns((BaseStationAircraft)null);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X" };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _BaseStationDatabase.Verify(d => d.InsertAircraft(It.IsAny<BaseStationAircraft>()), Times.Never());
            _BaseStationDatabase.Verify(d => d.UpdateAircraft(It.IsAny<BaseStationAircraft>()), Times.Never());

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Never());
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Never());
        }

        [TestMethod]
        public void Plugin_Copes_If_Aircraft_Icao24_Is_Missing_From_Message()
        {
            // This will never happen from a compliant feed of messages but we just need to know that if it ever
            // did arise we would handle it gracefully
            SetEnabledOption(true);

            _Plugin.Startup(_StartupParameters);

            var messageX = new BaseStationMessage() { AircraftId = 99, Icao24 = null };
            var messageY = new BaseStationMessage() { AircraftId = 42, Icao24 = "" };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(messageX));
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(messageY));

            _BaseStationDatabase.Verify(d => d.GetAircraftByCode(null), Times.Never());
            _BaseStationDatabase.Verify(d => d.GetAircraftByCode(""), Times.Never());
        }

        [TestMethod]
        public void Plugin_Reports_Exceptions_During_Aircraft_Creation()
        {
            var exception = new InvalidOperationException();
            SetEnabledOption(true);
            _BaseStationDatabase.Setup(d => d.GetOrInsertAircraftByCode("X", It.IsAny<Func<string, BaseStationAircraft>>()))
                                .Callback(() => {  throw exception; });

            _Plugin.Startup(_StartupParameters);

            _Plugin.StatusChanged += _StatusChangedEvent.Handler;

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.IsTrue(_StatusChangedEvent.CallCount > 0);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), exception.ToString()), Times.Once());
            Assert.AreEqual(String.Format("Exception caught: {0}", exception.Message), _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Reports_Exceptions_During_Flight_Creation()
        {
            var exception = new InvalidOperationException();
            SetEnabledOption(true);
            _BaseStationDatabase.Setup(d => d.InsertFlight(It.IsAny<BaseStationFlight>())).Callback(() => { throw exception; });

            _Plugin.Startup(_StartupParameters);

            _Plugin.StatusChanged += _StatusChangedEvent.Handler;

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.IsTrue(_StatusChangedEvent.CallCount > 0);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), exception.ToString()), Times.Once());
            Assert.AreEqual(String.Format("Exception caught: {0}", exception.Message), _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Reports_Exceptions_During_Database_StartTransaction()
        {
            var exception = new InvalidOperationException();
            SetEnabledOption(true);
            _BaseStationDatabase.Setup(d => d.StartTransaction()).Callback(() => { throw exception; });

            _Plugin.Startup(_StartupParameters);

            _Plugin.StatusChanged += _StatusChangedEvent.Handler;

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.IsTrue(_StatusChangedEvent.CallCount > 0);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), exception.ToString()), Times.Once());
            Assert.AreEqual(String.Format("Exception caught: {0}", exception.Message), _Plugin.StatusDescription);
        }

        [TestMethod]
        public void Plugin_Reports_Exceptions_During_Database_EndTransaction()
        {
            var exception = new InvalidOperationException();
            SetEnabledOption(true);
            _BaseStationDatabase.Setup(d => d.EndTransaction()).Callback(() => { throw exception; });

            _Plugin.Startup(_StartupParameters);

            _Plugin.StatusChanged += _StatusChangedEvent.Handler;

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.IsTrue(_StatusChangedEvent.CallCount > 0);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), exception.ToString()), Times.Once());
            Assert.AreEqual(String.Format("Exception caught: {0}", exception.Message), _Plugin.StatusDescription);
        }
        #endregion

        #region Database Flight record updates
        [TestMethod]
        public void Plugin_Flight_Leaves_Callsign_Empty_If_Missing_From_Initial_Message()
        {
            SetEnabledOption(true);

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.InsertFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = null, MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Once());
            Assert.AreEqual("", flight.Callsign);
        }

        [TestMethod]
        public void Plugin_Flight_Updated_After_No_Message_Received_For_Many_Minutes()
        {
            SetEnabledOption(true);

            var startTime = SetProviderTimes(new DateTime(2012, 3, 4, 5, 6, 7));

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = "ABC123", Altitude = 31, Emergency = true,
                GroundSpeed = 289, IdentActive = true, Latitude = 31.1F, Longitude = 123.1F, MessageGenerated = DateTime.Now, MessageLogged = DateTime.Now, MessageNumber = 12389, MessageType = BaseStationMessageType.Transmission,
                OnGround = true, Squawk = 1293, SquawkHasChanged = true, StatusCode = BaseStationStatusCode.None, Track = 12.4F, TransmissionType = BaseStationTransmissionType.AirToAir, VerticalRate = 18 };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            var endTime = SetProviderTimes(startTime.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());
            Assert.AreEqual(startTime, flight.StartTime);
            Assert.AreEqual(startTime, flight.EndTime);
        }

        [TestMethod]
        public void Plugin_Flight_Updated_Correctly_When_Clocks_Go_Forwards_During_Flight()
        {
            SetEnabledOption(true);

            // First message is received when both local and UTC are the same value
            var startTime = SetProviderTimes(new DateTime(2012, 3, 4, 5, 6, 7));

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = "ABC123", Altitude = 31, Emergency = true,
                GroundSpeed = 289, IdentActive = true, Latitude = 31.1F, Longitude = 123.1F, MessageGenerated = DateTime.Now, MessageLogged = DateTime.Now, MessageNumber = 12389, MessageType = BaseStationMessageType.Transmission,
                OnGround = true, Squawk = 1293, SquawkHasChanged = true, StatusCode = BaseStationStatusCode.None, Track = 12.4F, TransmissionType = BaseStationTransmissionType.AirToAir, VerticalRate = 18 };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            // Now daylight saving time is in effect and local is localNow one hour ahead of UTC, so local is 61 ahead of original start time.
            // The plugin must recognise that only one minute has elapsed and NOT update the flight record on the database.
            var localEndTime = startTime.AddMinutes(61);
            var utcEndTime = startTime.AddMinutes(1);
            SetProviderTimes(localEndTime, utcEndTime);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Never());

            // Time moves on and the wait period has elapsed in both local and UTC - the database should localNow be updated
            localEndTime = localEndTime.AddMinutes(MinutesBeforeFlightClosed);
            utcEndTime = utcEndTime.AddMinutes(MinutesBeforeFlightClosed);
            SetProviderTimes(localEndTime, utcEndTime);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());

            Assert.AreEqual(startTime, flight.StartTime);
            Assert.AreEqual(startTime, flight.EndTime);
        }

        [TestMethod]
        public void Plugin_Flight_Updated_Correctly_When_Clocks_Go_Backwards_During_Flight()
        {
            SetEnabledOption(true);

            // First message is received when both local and UTC are the same value
            var startTime = SetProviderTimes(new DateTime(2012, 3, 4, 5, 6, 7));

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = "ABC123", Altitude = 31, Emergency = true,
                GroundSpeed = 289, IdentActive = true, Latitude = 31.1F, Longitude = 123.1F, MessageGenerated = DateTime.Now, MessageLogged = DateTime.Now, MessageNumber = 12389, MessageType = BaseStationMessageType.Transmission,
                OnGround = true, Squawk = 1293, SquawkHasChanged = true, StatusCode = BaseStationStatusCode.None, Track = 12.4F, TransmissionType = BaseStationTransmissionType.AirToAir, VerticalRate = 18 };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            // Local is localNow one hour behind UTC, so local is 59 minutes behind original start time.
            // The plugin must recognise that only one minute has elapsed and NOT update the flight record on the database.
            var localEndTime = startTime.AddMinutes(-59);
            var utcEndTime = startTime.AddMinutes(1);
            SetProviderTimes(localEndTime, utcEndTime);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Never());

            // Time moves on and the wait period has elapsed in both local and UTC - the database should localNow be updated
            localEndTime = localEndTime.AddMinutes(MinutesBeforeFlightClosed);
            utcEndTime = utcEndTime.AddMinutes(MinutesBeforeFlightClosed);
            SetProviderTimes(localEndTime, utcEndTime);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());

            Assert.AreEqual(startTime, flight.StartTime);
            Assert.AreEqual(startTime, flight.EndTime);
        }

        [TestMethod]
        public void Plugin_Flight_Updated_Correctly_When_Transmissions_Received_After_Clocks_Go_Forwards_During_Flight()
        {
            SetEnabledOption(true);

            // First message is received when both local and UTC are the same value
            var startTime = SetProviderTimes(new DateTime(2012, 3, 4, 5, 6, 7));

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = "ABC123", Altitude = 31, Emergency = true,
                GroundSpeed = 289, IdentActive = true, Latitude = 31.1F, Longitude = 123.1F, MessageGenerated = DateTime.Now, MessageLogged = DateTime.Now, MessageNumber = 12389, MessageType = BaseStationMessageType.Transmission,
                OnGround = true, Squawk = 1293, SquawkHasChanged = true, StatusCode = BaseStationStatusCode.None, Track = 12.4F, TransmissionType = BaseStationTransmissionType.AirToAir, VerticalRate = 18 };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            // Next two messages come in after one minute each, but after local time is one hour ahead of UTC.
            var localEndTime = startTime.AddMinutes(61);
            var utcEndTime = startTime.AddMinutes(1);
            SetProviderTimes(localEndTime, utcEndTime);
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            localEndTime = localEndTime.AddMinutes(1);
            utcEndTime = utcEndTime.AddMinutes(1);
            SetProviderTimes(localEndTime, utcEndTime);
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            // No more messages - no update should be performed 25 minutes after start time because 2 messages came in after that...
            localEndTime = startTime.AddMinutes(60 + MinutesBeforeFlightClosed);
            utcEndTime = startTime.AddMinutes(MinutesBeforeFlightClosed);
            SetProviderTimes(localEndTime, utcEndTime);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Never());

            // But 25 minutes after the last message it should close the flight off
            localEndTime = startTime.AddMinutes(60 + 2 + MinutesBeforeFlightClosed);
            utcEndTime = startTime.AddMinutes(2 + MinutesBeforeFlightClosed);
            SetProviderTimes(localEndTime, utcEndTime);
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());

            Assert.AreEqual(startTime, flight.StartTime);
            Assert.AreEqual(startTime.AddMinutes(62), flight.EndTime);
        }

        [TestMethod]
        public void Plugin_Reports_Exceptions_During_Flight_Update()
        {
            var exception = new InvalidOperationException();
            SetEnabledOption(true);
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback(() => { throw exception; });

            var startTime = SetProviderTimes(DateTime.Now);
            _Plugin.Startup(_StartupParameters);

            _Plugin.StatusChanged += _StatusChangedEvent.Handler;
            _StatusChangedEvent.EventRaised += (s, a) => {
                Assert.AreEqual(String.Format("Exception caught: {0}", exception.Message), _Plugin.StatusDescription);
            };

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            var endTime = SetProviderTimes(startTime.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _StatusChangedEvent.CallCount);
            Assert.AreSame(_Plugin, _StatusChangedEvent.Sender);
            _Log.Verify(g => g.WriteLine(It.IsAny<string>(), exception.ToString()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Flight_Update_Keeps_Callsign_If_Originally_Missing()
        {
            SetEnabledOption(true);

            var startTime = SetProviderTimes(DateTime.Now);

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = null, Altitude = 31, Emergency = true,
                GroundSpeed = 289, IdentActive = true, Latitude = 31.1F, Longitude = 123.1F, MessageGenerated = DateTime.Now, MessageLogged = DateTime.Now, MessageNumber = 12389, MessageType = BaseStationMessageType.Transmission,
                OnGround = true, Squawk = 1293, SquawkHasChanged = true, StatusCode = BaseStationStatusCode.None, Track = 12.4F, TransmissionType = BaseStationTransmissionType.AirToAir, VerticalRate = 18 };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            message.Callsign = "ABC123";
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight flight) => {
                Assert.AreEqual("ABC123", flight.Callsign);
            });

            var endTime = SetProviderTimes(startTime.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());
        }

        [TestMethod]
        public void Plugin_Flight_EndTime_Reflects_Time_Of_Last_Message_From_Aircraft()
        {
            SetEnabledOption(true);

            _Plugin.Startup(_StartupParameters);

            var startTime = SetProviderTimes(new DateTime(2012, 3, 4, 5, 6, 7));
            var message = new BaseStationMessage() { Icao24 = "123456", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            SetProviderTimes(startTime.AddMinutes(1));
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            var endTime = SetProviderTimes(startTime.AddMinutes(7));
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight flight) => {
                Assert.AreEqual(startTime, flight.StartTime);
                Assert.AreEqual(endTime, flight.EndTime);
            });

            SetProviderTimes(startTime.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Never());

            SetProviderTimes(endTime.AddMinutes(MinutesBeforeFlightClosed).AddMilliseconds(-1));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Never());

            SetProviderTimes(endTime.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());
        }

        [TestMethod]
        [DataSource("Data Source='PluginBaseStationDatabaseWriterTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "UpdateFirstLast$")]
        public void Plugin_Flight_Updates_First_Last_Values_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            SetEnabledOption(true);

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            _Plugin.Startup(_StartupParameters);

            var messageProperty = typeof(BaseStationMessage).GetProperty(worksheet.String("MsgProperty"));
            var isMlat = worksheet.Bool("IsMlat");

            for(int i = 0;i < worksheet.Int("Count");++i) {
                var message = new BaseStationMessage() { Icao24 = "Z", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };
                var args = new BaseStationMessageEventArgs(message, isOutOfBand: isMlat);

                var valueText = worksheet.EString(String.Format("MsgValue{0}", i + 1));
                messageProperty.SetValue(message, TestUtilities.ChangeType(valueText, messageProperty.PropertyType, CultureInfo.InvariantCulture), null);

                _Listener.Raise(r => r.Port30003MessageReceived += null, args);
            }

            SetProviderTimes(_Provider.Object.LocalNow.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            var flightPropertyName = worksheet.String("FlightProperty");
            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());

            foreach(var prefix in new string[] { "First", "Last" }) {
                var propertyName = String.Format("{0}{1}", prefix, flightPropertyName);
                var flightProperty = typeof(BaseStationFlight).GetProperty(propertyName);
                var valueText = worksheet.EString(String.Format("{0}Value", prefix));

                var expectedValue = TestUtilities.ChangeType(valueText, flightProperty.PropertyType, CultureInfo.InvariantCulture);
                var actualValue = flightProperty.GetValue(flight, null);

                Assert.AreEqual(expectedValue, actualValue, propertyName);
            }
        }

        [TestMethod]
        [DataSource("Data Source='PluginBaseStationDatabaseWriterTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "UpdateStatusBools$")]
        public void Plugin_Flight_Updates_Status_Bool_Values_Correctly()
        {
            // These are the Flight record fields that start with "Had".
            var worksheet = new ExcelWorksheetData(TestContext);

            SetEnabledOption(true);

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            _Plugin.Startup(_StartupParameters);

            var messageProperty = typeof(BaseStationMessage).GetProperty(worksheet.String("MsgProperty"));
            for(int i = 0;i < worksheet.Int("Count");++i) {
                var message = new BaseStationMessage() { Icao24 = "Z", MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir };

                var valueText = worksheet.EString(String.Format("MsgValue{0}", i + 1));
                messageProperty.SetValue(message, TestUtilities.ChangeType(valueText, messageProperty.PropertyType, CultureInfo.InvariantCulture), null);

                _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            }

            SetProviderTimes(_Provider.Object.LocalNow.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());

            var propertyName = worksheet.String("FlightProperty");
            var flightProperty = typeof(BaseStationFlight).GetProperty(propertyName);
            var flightValue = worksheet.EString("FlightValue");

            var expectedValue = TestUtilities.ChangeType(flightValue, flightProperty.PropertyType, CultureInfo.InvariantCulture);
            var actualValue = flightProperty.GetValue(flight, null);

            Assert.AreEqual(expectedValue, actualValue, propertyName);
        }

        [TestMethod]
        [DataSource("Data Source='PluginBaseStationDatabaseWriterTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "UpdateMessageCounters$")]
        public void Plugin_Flight_Updates_Message_Counters_Correctly()
        {
            // These are the nullable integers which start with "Num"
            var worksheet = new ExcelWorksheetData(TestContext);

            SetEnabledOption(true);

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            _Plugin.Startup(_StartupParameters);

            var message = new BaseStationMessage() {
                Icao24 = "Z",
                MessageType = worksheet.ParseEnum<BaseStationMessageType>("MessageType"),
                TransmissionType = worksheet.ParseEnum<BaseStationTransmissionType>("TransmissionType"),
                StatusCode = worksheet.ParseEnum<BaseStationStatusCode>("StatusCode"),
                GroundSpeed = worksheet.NFloat("GroundSpeed"),
                Track = worksheet.NFloat("Track"),
                Latitude = worksheet.NFloat("Latitude"),
                Longitude = worksheet.NFloat("Longitude"),
                VerticalRate = worksheet.NInt("VerticalRate"),
            };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            SetProviderTimes(_Provider.Object.LocalNow.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(s => s.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.UpdateFlight(It.IsAny<BaseStationFlight>()), Times.Once());

            // Note that the totals are not null after the final update even if no messages were received in a particular category
            Assert.AreEqual(worksheet.Int("NumPosMsgRec"), flight.NumPosMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumADSBMsgRec"), flight.NumADSBMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumModeSMsgRec"), flight.NumModeSMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumIDMsgRec"), flight.NumIDMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumSurPosMsgRec"), flight.NumSurPosMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumAirPosMsgRec"), flight.NumAirPosMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumAirVelMsgRec"), flight.NumAirVelMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumSurAltMsgRec"), flight.NumSurAltMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumSurIDMsgRec"), flight.NumSurIDMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumAirToAirMsgRec"), flight.NumAirToAirMsgRec.Value);
            Assert.AreEqual(worksheet.Int("NumAirCallRepMsgRec"), flight.NumAirCallRepMsgRec.Value);
        }

        [TestMethod]
        public void Plugin_Writes_New_Flight_Record_For_Aircraft_If_Message_Received_After_Final_Update_Of_Previous_Flight()
        {
            SetEnabledOption(true);

            _BaseStationDatabase.Setup(d => d.GetAircraftByCode("Z")).Returns(new BaseStationAircraft() { AircraftID = 4722 });
            _BaseStationDatabase.Setup(d => d.InsertSession(It.IsAny<BaseStationSession>())).Callback((BaseStationSession session) => { session.SessionID = 318; });

            var message = new BaseStationMessage() { MessageType = BaseStationMessageType.Transmission, TransmissionType = BaseStationTransmissionType.AirToAir, AircraftId = 2372211, SessionId = -1, Icao24 = "Z" };

            _Plugin.Startup(_StartupParameters);

            var time = SetProviderTimes(DateTime.Now);
            _Listener.Raise(m => m.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            time = SetProviderTimes(time.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Setup(d => d.InsertFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight flight) => {
                Assert.AreEqual(0, flight.FlightID);
                Assert.AreEqual(4722, flight.AircraftID);
                Assert.AreEqual(318, flight.SessionID);
                Assert.AreEqual(null, flight.Aircraft);
                Assert.AreEqual("", flight.Callsign);
                Assert.AreEqual(time, flight.StartTime);
                Assert.AreEqual(null, flight.EndTime);

                Assert.AreEqual(null, flight.FirstAltitude);
                Assert.AreEqual(null, flight.FirstGroundSpeed);
                Assert.AreEqual(false, flight.FirstIsOnGround);
                Assert.AreEqual(null, flight.FirstLat);
                Assert.AreEqual(null, flight.FirstLon);
                Assert.AreEqual(null, flight.FirstSquawk);
                Assert.AreEqual(null, flight.FirstTrack);
                Assert.AreEqual(null, flight.FirstVerticalRate);
                Assert.AreEqual(false, flight.HadAlert);
                Assert.AreEqual(false, flight.HadEmergency);
                Assert.AreEqual(false, flight.HadSpi);
                Assert.AreEqual(null, flight.LastAltitude);
                Assert.AreEqual(null, flight.LastGroundSpeed);
                Assert.AreEqual(false, flight.LastIsOnGround);
                Assert.AreEqual(null, flight.LastLat);
                Assert.AreEqual(null, flight.LastLon);
                Assert.AreEqual(null, flight.LastSquawk);
                Assert.AreEqual(null, flight.LastTrack);
                Assert.AreEqual(null, flight.LastVerticalRate);
                Assert.AreEqual(null, flight.NumADSBMsgRec);
                Assert.AreEqual(null, flight.NumModeSMsgRec);
                Assert.AreEqual(null, flight.NumSurPosMsgRec);
                Assert.AreEqual(null, flight.NumAirPosMsgRec);
                Assert.AreEqual(null, flight.NumAirVelMsgRec);
                Assert.AreEqual(null, flight.NumSurAltMsgRec);
                Assert.AreEqual(null, flight.NumSurIDMsgRec);
                Assert.AreEqual(null, flight.NumAirToAirMsgRec);
                Assert.AreEqual(null, flight.NumAirCallRepMsgRec);
                Assert.AreEqual(null, flight.NumPosMsgRec);
            });

            time = SetProviderTimes(time.AddMilliseconds(1));
            _Listener.Raise(m => m.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _BaseStationDatabase.Verify(d => d.InsertFlight(It.IsAny<BaseStationFlight>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Plugin_Flight_Does_Not_Record_Coordinates_Of_Zero_Zero()
        {
            SetEnabledOption(true);
            var startTime = SetProviderTimes(new DateTime(2012, 3, 4, 5, 6, 7));
            _Plugin.Startup(_StartupParameters);

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = "ABC123", Altitude = 31, Emergency = true,
                GroundSpeed = 289, IdentActive = true, Latitude = 0F, Longitude = 0F, MessageGenerated = DateTime.Now, MessageLogged = DateTime.Now, MessageNumber = 12389, MessageType = BaseStationMessageType.Transmission,
                OnGround = true, Squawk = 1293, SquawkHasChanged = true, StatusCode = BaseStationStatusCode.None, Track = 12.4F, TransmissionType = BaseStationTransmissionType.AirToAir, VerticalRate = 18 };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            SetProviderTimes(startTime.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(null, flight.FirstLat);
            Assert.AreEqual(null, flight.FirstLon);
            Assert.AreEqual(null, flight.LastLat);
            Assert.AreEqual(null, flight.LastLon);
        }

        [TestMethod]
        public void Plugin_Flight_Does_Record_Latitude_Of_Zero_Longitude_Of_NonZero()
        {
            SetEnabledOption(true);
            var startTime = SetProviderTimes(new DateTime(2012, 3, 4, 5, 6, 7));
            _Plugin.Startup(_StartupParameters);

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = "ABC123", Altitude = 31, Emergency = true,
                GroundSpeed = 289, IdentActive = true, Latitude = 0F, Longitude = 0.1F, MessageGenerated = DateTime.Now, MessageLogged = DateTime.Now, MessageNumber = 12389, MessageType = BaseStationMessageType.Transmission,
                OnGround = true, Squawk = 1293, SquawkHasChanged = true, StatusCode = BaseStationStatusCode.None, Track = 12.4F, TransmissionType = BaseStationTransmissionType.AirToAir, VerticalRate = 18 };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            SetProviderTimes(startTime.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(0F, flight.FirstLat);
            Assert.AreEqual(0.1F, flight.FirstLon);
            Assert.AreEqual(0F, flight.LastLat);
            Assert.AreEqual(0.1F, flight.LastLon);
        }

        [TestMethod]
        public void Plugin_Flight_Does_Record_Latitude_Of_NonZero_Longitude_Of_Zero()
        {
            SetEnabledOption(true);
            var startTime = SetProviderTimes(new DateTime(2012, 3, 4, 5, 6, 7));
            _Plugin.Startup(_StartupParameters);

            BaseStationFlight flight = null;
            _BaseStationDatabase.Setup(d => d.UpdateFlight(It.IsAny<BaseStationFlight>())).Callback((BaseStationFlight f) => { flight = f; });

            var message = new BaseStationMessage() { AircraftId = 99, Icao24 = "X", FlightId = 429, SessionId = 1239, Callsign = "ABC123", Altitude = 31, Emergency = true,
                GroundSpeed = 289, IdentActive = true, Latitude = 0.1F, Longitude = 0F, MessageGenerated = DateTime.Now, MessageLogged = DateTime.Now, MessageNumber = 12389, MessageType = BaseStationMessageType.Transmission,
                OnGround = true, Squawk = 1293, SquawkHasChanged = true, StatusCode = BaseStationStatusCode.None, Track = 12.4F, TransmissionType = BaseStationTransmissionType.AirToAir, VerticalRate = 18 };
            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(message));

            SetProviderTimes(startTime.AddMinutes(MinutesBeforeFlightClosed));
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(0.1F, flight.FirstLat);
            Assert.AreEqual(0F, flight.FirstLon);
            Assert.AreEqual(0.1F, flight.LastLat);
            Assert.AreEqual(0F, flight.LastLon);
        }
        #endregion
    }
}
