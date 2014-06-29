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
using System.Linq;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Library;
using System.Net;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class MainPresenterTests
    {
        #region TestContext, Fields, TestInitialise etc.
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;
        private IMainPresenter _Presenter;
        private Mock<IMainView> _View;
        private ClockMock _Clock;
        private Mock<IWebServer> _WebServer;
        private Mock<IAutoConfigWebServer> _AutoConfigWebServer;
        private Mock<IUniversalPlugAndPlayManager> _UPnpManager;
        private Mock<IHeartbeatService> _HeartbeatService;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Mock<INewVersionChecker> _NewVersionChecker;
        private EventRecorder<EventArgs> _HeartbeatTick;
        private Mock<ILog> _Log;
        private Mock<IBaseStationAircraftList> _BaseStationAircraftList;
        private Mock<IPluginManager> _PluginManager;
        private Mock<IRebroadcastServerManager> _RebroadcastServerManager;
        private Mock<IFeedManager> _FeedManager;
        private List<Mock<IFeed>> _Feeds;
        private List<Mock<IListener>> _Listeners;
        private Mock<IUserManager> _UserManager;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(cs => cs.Load()).Returns(_Configuration);

            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            _WebServer = new Mock<IWebServer>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AutoConfigWebServer = TestUtilities.CreateMockSingleton<IAutoConfigWebServer>();
            _AutoConfigWebServer.Setup(s => s.WebServer).Returns(_WebServer.Object);
            _UPnpManager = new Mock<IUniversalPlugAndPlayManager>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _NewVersionChecker = TestUtilities.CreateMockSingleton<INewVersionChecker>();
            _HeartbeatTick = new EventRecorder<EventArgs>();
            _BaseStationAircraftList = new Mock<IBaseStationAircraftList>();
            _PluginManager = TestUtilities.CreateMockSingleton<IPluginManager>();
            _RebroadcastServerManager = TestUtilities.CreateMockSingleton<IRebroadcastServerManager>();
            _UserManager = TestUtilities.CreateMockSingleton<IUserManager>();

            _Feeds = new List<Mock<IFeed>>();
            _Listeners = new List<Mock<IListener>>();
            _FeedManager = FeedHelper.CreateMockFeedManager(_Feeds, _Listeners, 1, 2);

            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _Presenter = Factory.Singleton.Resolve<IMainPresenter>();
            _View = new Mock<IMainView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void MainPresenter_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.IsNull(_Presenter.View);
            TestUtilities.TestProperty(_Presenter, r => r.UPnpManager, null, _UPnpManager.Object);
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MainPresenter_Initialise_Throws_If_View_IsNull()
        {
            _Presenter.Initialise(null);
        }

        [TestMethod]
        public void MainPresenter_Initialise_Sets_View_Property()
        {
            _Presenter.Initialise(_View.Object);
            Assert.AreSame(_View.Object, _Presenter.View);
        }

        [TestMethod]
        public void MainPresenter_Initialise_Sets_LogFileName_On_View()
        {
            string expectedFileName = @"c:\users\me\appsdata\local\vrs\log.txt";
            _Log.Setup(g => g.FileName).Returns(expectedFileName);

            _Presenter.Initialise(_View.Object);
            Assert.AreEqual(expectedFileName, _View.Object.LogFileName);
        }

        [TestMethod]
        public void MainPresenter_Initialise_Sets_InvalidPluginCount_On_View()
        {
            _PluginManager.Setup(p => p.IgnoredPlugins).Returns(new Dictionary<string, string>() { { "a", "a" }, { "b", "c" } });

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(2, _View.Object.InvalidPluginCount);
        }

        [TestMethod]
        public void MainPresenter_Initialise_Calls_GuiThreadStartup_On_All_Plugins()
        {
            var plugin = new Mock<IPlugin>();
            _PluginManager.Setup(p => p.LoadedPlugins).Returns(new List<IPlugin>() { plugin.Object });

            _Presenter.Initialise(_View.Object);

            plugin.Verify(p => p.GuiThreadStartup(), Times.Once());
        }
        #endregion

        #region Feeds
        [TestMethod]
        public void MainPresenter_Feed_Display_Initialised_When_View_Initialised()
        {
            IFeed[] feeds = null;
            _View.Setup(r => r.ShowFeeds(It.IsAny<IFeed[]>())).Callback((IFeed[] r) => {
                feeds = r;
            });

            _Presenter.Initialise(_View.Object);
            Assert.AreEqual(_Feeds.Count, feeds.Length);
            foreach(var feed in _Feeds) {
                Assert.IsTrue(feeds.Contains(feed.Object));
            }
        }

        [TestMethod]
        public void MainPresenter_Feed_Display_Initialised_When_Feeds_Changes()
        {
            _Presenter.Initialise(_View.Object);

            IFeed[] feeds = null;
            _View.Setup(r => r.ShowFeeds(It.IsAny<IFeed[]>())).Callback((IFeed[] r) => {
                feeds = r;
            });

            _FeedManager.Raise(r => r.FeedsChanged += null, EventArgs.Empty);

            Assert.AreEqual(_Feeds.Count, feeds.Length);
            foreach(var feed in _Feeds) {
                Assert.IsTrue(feeds.Contains(feed.Object));
            }
        }

        [TestMethod]
        public void MainPresenter_Feed_Counters_Refreshed_Periodically()
        {
            _Presenter.Initialise(_View.Object);

            IFeed[] feeds = null;
            _View.Setup(r => r.UpdateFeedCounters(It.IsAny<IFeed[]>())).Callback((IFeed[] r) => {
                feeds = r;
            });

            _HeartbeatService.Raise(s => s.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(_Feeds.Count, feeds.Length);
            foreach(var feed in _Feeds) {
                Assert.IsTrue(feeds.Contains(feed.Object));
            }
        }

        [TestMethod]
        public void MainPresenter_Feed_ConnectionStatus_Refreshed_When_Listener_Connection_State_Changes()
        {
            _Presenter.Initialise(_View.Object);

            var feed = _Feeds[1].Object;
            _FeedManager.Raise(r => r.ConnectionStateChanged += null, new EventArgs<IFeed>(feed));

            _View.Verify(r => r.ShowFeedConnectionStatus(feed), Times.Once());
        }
        #endregion

        #region GetReceiverFeeds
        [TestMethod]
        public void MainPresenter_GetReceiverFeeds_Returns_FeedManager_Feeds()
        {
            var feeds = _Presenter.GetReceiverFeeds();

            Assert.AreEqual(2, feeds.Length);
            Assert.IsTrue(feeds.Contains(_Feeds[0].Object));
            Assert.IsTrue(feeds.Contains(_Feeds[1].Object));
        }

        [TestMethod]
        public void MainPresenter_GetReceiverFeeds_Excludes_Merged_Feeds()
        {
            var mergedFeedListeners = new List<Mock<IMergedFeedListener>>();
            FeedHelper.AddMergedFeeds(_Feeds, mergedFeedListeners, 3);

            var feeds = _Presenter.GetReceiverFeeds();

            Assert.AreEqual(2, feeds.Length);
            Assert.IsTrue(feeds.Contains(_Feeds[0].Object));
            Assert.IsTrue(feeds.Contains(_Feeds[1].Object));
        }
        #endregion

        #region WebServer
        [TestMethod]
        public void MainPresenter_WebServer_Initial_Online_State_Is_Copied_To_View()
        {
            foreach(var state in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _WebServer.Object.Online = state;
                _Presenter.Initialise(_View.Object);

                Assert.AreEqual(state, _View.Object.WebServerIsOnline);
            }
        }

        [TestMethod]
        public void MainPresenter_WebServer_Local_Address_Is_Copied_To_View()
        {
            _WebServer.Setup(s => s.LocalAddress).Returns("This is local");
            _Presenter.Initialise(_View.Object);

            Assert.AreEqual("This is local", _View.Object.WebServerLocalAddress);
        }

        [TestMethod]
        public void MainPresenter_WebServer_Network_Address_Is_Copied_To_View()
        {
            _WebServer.Setup(s => s.NetworkAddress).Returns("This is network");
            _Presenter.Initialise(_View.Object);

            Assert.AreEqual("This is network", _View.Object.WebServerNetworkAddress);
        }

        [TestMethod]
        public void MainPresenter_WebServer_External_Address_Is_Copied_To_View()
        {
            _WebServer.Setup(s => s.ExternalAddress).Returns("This is external");
            _Presenter.Initialise(_View.Object);

            Assert.AreEqual("This is external", _View.Object.WebServerExternalAddress);
        }

        [TestMethod]
        public void MainPresenter_WebServer_External_Address_Is_Updated_If_Server_Reports_A_Change()
        {
            _WebServer.Setup(s => s.ExternalAddress).Returns("Original");
            _Presenter.Initialise(_View.Object);

            _WebServer.Setup(s => s.ExternalAddress).Returns("New");

            Assert.AreNotEqual("New", _View.Object.WebServerExternalAddress);
            _WebServer.Raise(s => s.ExternalAddressChanged += null, EventArgs.Empty);
            Assert.AreEqual("New", _View.Object.WebServerExternalAddress);
        }

        [TestMethod]
        public void MainPresenter_WebServer_Reflects_Changes_In_Online_State_In_View()
        {
            foreach(var initialState in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _WebServer.Object.Online = initialState;
                _Presenter.Initialise(_View.Object);

                _WebServer.Object.Online = !initialState;
                _WebServer.Raise(s => s.OnlineChanged += null, EventArgs.Empty);

                Assert.AreEqual(!initialState, _View.Object.WebServerIsOnline);
            }
        }

        [TestMethod]
        public void MainPresenter_WebServer_Toggles_WebServer_State_On_User_Request()
        {
            foreach(var initialState in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _WebServer.Object.Online = initialState;
                _Presenter.Initialise(_View.Object);

                _View.Raise(e => e.ToggleServerStatus += null, EventArgs.Empty);

                Assert.AreEqual(!initialState, _WebServer.Object.Online);
            }
        }

        [TestMethod]
        public void MainPresenter_WebServer_Updates_Display_With_Information_About_Serviced_Requests()
        {
            _Presenter.Initialise(_View.Object);

            var args = new ResponseSentEventArgs("url goes here", "192.168.0.44:58301", "127.0.3.4", 10203, ContentClassification.Image, null, 0, 0);
            _WebServer.Raise(s => s.ResponseSent += null, args);
            _View.Verify(v => v.ShowWebRequestHasBeenServiced("192.168.0.44:58301", "url goes here", 10203), Times.Once());
        }
        #endregion

        #region RebroadcastServerManager
        [TestMethod]
        public void MainPresenter_RebroadcastServerManager_Configuration_Shown_On_Initialise()
        {
            _Configuration.RebroadcastSettings.Add(new RebroadcastSettings() { Enabled = true });
            var connections = new List<RebroadcastServerConnection>();
            _RebroadcastServerManager.Setup(r => r.GetConnections()).Returns(connections);

            _Presenter.Initialise(_View.Object);

            _View.Verify(r => r.ShowRebroadcastServerStatus(connections), Times.Once());
        }

        [TestMethod]
        public void MainPresenter_RebroadcastServerManager_Refreshes_View_When_Display_Refreshes()
        {
            var connections = new List<RebroadcastServerConnection>();
            _RebroadcastServerManager.Setup(r => r.GetConnections()).Returns(connections);

            _Presenter.Initialise(_View.Object);

            _View.Raise(r => r.RefreshTimerTicked += null, EventArgs.Empty);

            _View.Verify(r => r.ShowRebroadcastServerStatus(connections), Times.Exactly(2));
        }

        [TestMethod]
        public void MainPresenter_RebroadcastServerManager_Configuration_Shown_On_Configuration_Update()
        {
            _Presenter.Initialise(_View.Object);

            _Configuration.RebroadcastSettings.Add(new RebroadcastSettings() { Enabled = true });
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(Describe.RebroadcastSettingsCollection(_Configuration.RebroadcastSettings), _View.Object.RebroadcastServersConfiguration);
        }
        #endregion

        #region Universal Plug & Play Manager
        [TestMethod]
        public void MainPresenter_UPnpManager_Initial_Port_Forwarding_State_Is_Copied_To_View()
        {
            foreach(var state in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Presenter.Initialise(_View.Object);

                _UPnpManager.Setup(m => m.PortForwardingPresent).Returns(state);
                _Presenter.UPnpManager = _UPnpManager.Object;
                Assert.AreEqual(state, _View.Object.UPnpPortForwardingActive);
            }
        }

        [TestMethod]
        public void MainPresenter_UPnpManager_Initial_Enabled_State_Is_Copied_To_View()
        {
            foreach(var state in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Presenter.Initialise(_View.Object);

                _UPnpManager.Setup(m => m.IsEnabled).Returns(state);
                _Presenter.UPnpManager = _UPnpManager.Object;
                Assert.AreEqual(state, _View.Object.UPnpEnabled);
            }
        }

        [TestMethod]
        public void MainPresenter_UPnpManager_Initial_Router_Present_State_Is_Copied_To_View()
        {
            foreach(var state in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Presenter.Initialise(_View.Object);

                _UPnpManager.Setup(m => m.IsRouterPresent).Returns(state);
                _Presenter.UPnpManager = _UPnpManager.Object;
                Assert.AreEqual(state, _View.Object.UPnpRouterPresent);
            }
        }

        [TestMethod]
        public void MainPresenter_UPnpManager_Port_Forwarding_State_Is_Updated_If_UPnpManager_Reports_Change()
        {
            _Presenter.Initialise(_View.Object);

            _UPnpManager.Setup(m => m.PortForwardingPresent).Returns(false);
            _Presenter.UPnpManager = _UPnpManager.Object;

            _UPnpManager.Setup(m => m.PortForwardingPresent).Returns(true);
            _UPnpManager.Raise(m => m.StateChanged += null, EventArgs.Empty);

            Assert.AreEqual(true, _View.Object.UPnpPortForwardingActive);
        }

        [TestMethod]
        public void MainPresenter_UPnpManager_Enabled_State_Is_Updated_If_UPnpManager_Reports_Change()
        {
            _Presenter.Initialise(_View.Object);

            _UPnpManager.Setup(m => m.IsEnabled).Returns(false);
            _Presenter.UPnpManager = _UPnpManager.Object;

            _UPnpManager.Setup(m => m.IsEnabled).Returns(true);
            _UPnpManager.Raise(m => m.StateChanged += null, EventArgs.Empty);

            Assert.AreEqual(true, _View.Object.UPnpEnabled);
        }

        [TestMethod]
        public void MainPresenter_UPnpManager_Router_Present_State_Is_Updated_If_UPnpManager_Reports_Change()
        {
            _Presenter.Initialise(_View.Object);

            _UPnpManager.Setup(m => m.IsRouterPresent).Returns(false);
            _Presenter.UPnpManager = _UPnpManager.Object;

            _UPnpManager.Setup(m => m.IsRouterPresent).Returns(true);
            _UPnpManager.Raise(m => m.StateChanged += null, EventArgs.Empty);

            Assert.AreEqual(true, _View.Object.UPnpRouterPresent);
        }

        [TestMethod]
        public void MainPresenter_UPnpManager_Toggles_Port_Forwarding_State_On_User_Request()
        {
            _UPnpManager.Setup(m => m.IsEnabled).Returns(true);
            _UPnpManager.Setup(m => m.IsRouterPresent).Returns(true);

            foreach(var initialState in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Presenter.Initialise(_View.Object);

                _UPnpManager.Setup(m => m.PortForwardingPresent).Returns(initialState);
                _Presenter.UPnpManager = _UPnpManager.Object;
                _View.Raise(e => e.ToggleUPnpStatus += null, EventArgs.Empty);

                _UPnpManager.Verify(m => m.PutServerOntoInternet(), initialState ? Times.Never() : Times.Once());
                _UPnpManager.Verify(m => m.TakeServerOffInternet(), initialState ? Times.Once() : Times.Never());
            }
        }
        #endregion

        #region Version Checker
        [TestMethod]
        public void MainPresenter_VersionChecker_Invoked_When_Heartbeat_Makes_First_Tick()
        {
            _Presenter.Initialise(_View.Object);

            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Never());
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Once());
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Invoked_On_First_Heartbeat_After_Check_Period_Has_Elapsed()
        {
            _Presenter.Initialise(_View.Object);

            _Configuration.VersionCheckSettings.CheckPeriodDays = 10;

            _Clock.UtcNowValue = new DateTime(2010, 1, 1);

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Once());

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddDays(9).AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Once());

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddDays(10);
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Exactly(2));
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Not_Invoked_If_CheckPeriodDays_Is_Too_Low()
        {
            _Presenter.Initialise(_View.Object);

            _Configuration.VersionCheckSettings.CheckPeriodDays = 0;

            _Clock.UtcNowValue = new DateTime(2010, 1, 1);

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Never());
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Not_Invoked_If_Automatic_Updating_Is_Prohibited()
        {
            _Presenter.Initialise(_View.Object);

            _Configuration.VersionCheckSettings.CheckAutomatically = false;

            _Clock.UtcNowValue = new DateTime(2010, 1, 1);

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Never());
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Not_Invoked_If_Automatic_Updating_Is_Prohibited_After_First_Tick()
        {
            _Presenter.Initialise(_View.Object);

            _Configuration.VersionCheckSettings.CheckPeriodDays = 10;

            _Clock.UtcNowValue = new DateTime(2010, 1, 1);

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Once());

            _Configuration.VersionCheckSettings.CheckAutomatically = false;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddDays(10);
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Once());
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Not_Invoked_If_Automatic_Updating_Is_Permitted_After_First_Tick()
        {
            _Presenter.Initialise(_View.Object);

            _Configuration.VersionCheckSettings.CheckAutomatically = false;
            _Configuration.VersionCheckSettings.CheckPeriodDays = 10;

            _Clock.UtcNowValue = new DateTime(2010, 1, 1);

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Never());

            _Configuration.VersionCheckSettings.CheckAutomatically = true;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddDays(10);
            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Once());
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Logs_Exceptions()
        {
            _Presenter.Initialise(_View.Object);

            var exception = new InvalidOperationException("Exception text here");
            _Log.Verify(o => o.WriteLine(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
            _NewVersionChecker.Setup(c => c.CheckForNewVersion()).Callback(() => { throw exception; });

            _HeartbeatService.Raise(h => h.SlowTick += null, EventArgs.Empty);

            _Log.Verify(o => o.WriteLine(It.IsAny<string>(), exception.ToString()), Times.Once());
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Sets_View_If_New_Version_Is_Available()
        {
            _Presenter.Initialise(_View.Object);

            Assert.IsFalse(_View.Object.NewVersionAvailable);
            _NewVersionChecker.Setup(c => c.IsNewVersionAvailable).Returns(true);
            _NewVersionChecker.Raise(c => c.NewVersionAvailable += null, EventArgs.Empty);
            Assert.IsTrue(_View.Object.NewVersionAvailable);
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Sets_DownloadUrl()
        {
            _Presenter.Initialise(_View.Object);

            Assert.IsNull(_View.Object.NewVersionDownloadUrl);
            _NewVersionChecker.Setup(c => c.DownloadUrl).Returns("My url");
            _NewVersionChecker.Raise(c => c.NewVersionAvailable += null, EventArgs.Empty);
            Assert.AreEqual("My url", _View.Object.NewVersionDownloadUrl);
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Can_Be_Invoked_Manually()
        {
            _Presenter.Initialise(_View.Object);

            _View.Raise(v => v.CheckForNewVersion += null, EventArgs.Empty);
            _NewVersionChecker.Verify(c => c.CheckForNewVersion(), Times.Once());
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Shows_Wait_Cursor_When_Invoked_Manually()
        {
            _Presenter.Initialise(_View.Object);

            object previousState = new object();
            _View.Setup(v => v.ShowBusy(true, null)).Returns(previousState);
            _NewVersionChecker.Setup(c => c.CheckForNewVersion()).Callback(() => _View.Verify(v => v.ShowBusy(true, null), Times.Once()));

            _View.Raise(v => v.CheckForNewVersion += null, EventArgs.Empty);
            _View.Verify(v => v.ShowBusy(true, null), Times.Once());
            _View.Verify(v => v.ShowBusy(false, previousState), Times.Once());
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Removes_Wait_Cursor_When_Invoked_Manually_Even_If_Exception_Occurs()
        {
            _Presenter.Initialise(_View.Object);

            object previousState = new object();
            _View.Setup(v => v.ShowBusy(true, null)).Returns(previousState);
            _NewVersionChecker.Setup(c => c.CheckForNewVersion()).Callback(() => { throw new InvalidOperationException(); });

            try {
                _View.Raise(v => v.CheckForNewVersion += null, EventArgs.Empty);
            } catch(InvalidOperationException) {
            }

            _View.Verify(v => v.ShowBusy(false, previousState), Times.Once());
        }

        [TestMethod]
        public void MainPresenter_VersionChecker_Shows_Result_Of_Manual_Check()
        {
            _Presenter.Initialise(_View.Object);

            _NewVersionChecker.Setup(c => c.CheckForNewVersion()).Returns(true);
            _View.Raise(v => v.CheckForNewVersion += null, EventArgs.Empty);
            _View.Verify(v => v.ShowManualVersionCheckResult(true), Times.Once());
            _View.Verify(v => v.ShowManualVersionCheckResult(false), Times.Never());

            _NewVersionChecker.Setup(c => c.CheckForNewVersion()).Returns(false);
            _View.Raise(v => v.CheckForNewVersion += null, EventArgs.Empty);
            _View.Verify(v => v.ShowManualVersionCheckResult(true), Times.Once());
            _View.Verify(v => v.ShowManualVersionCheckResult(false), Times.Once());
        }
        #endregion

        #region ReconnectFeed
        [TestMethod]
        public void MainPresenter_ReconnectToBaseStation_Disconnects_And_Then_Reconnects()
        {
            var feed = _Feeds[1];
            var listener = _Listeners[1];
            _Presenter.Initialise(_View.Object);

            _View.Raise(v => v.ReconnectFeed += null, new EventArgs<IFeed>(feed.Object));

            listener.Verify(v => v.Disconnect(), Times.Once());
            listener.Verify(v => v.Connect(false), Times.Once());
        }
        #endregion
    }
}
