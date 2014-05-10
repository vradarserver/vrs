// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;
using Moq;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using InterfaceFactory;
using Test.Framework;
using System.Net;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class RebroadcastServerManagerTests
    {
        #region TestContext, fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalClassFactory;

        private IRebroadcastServerManager _Manager;
        private Mock<IRebroadcastServer> _Server;
        private List<Mock<IFeed>> _Feeds;
        private List<Mock<IListener>> _Listeners;
        private Mock<IFeedManager> _FeedManager;
        private Mock<IBroadcastProvider> _BroadcastProvider;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Configuration _Configuration;
        private RebroadcastSettings _RebroadcastSettings;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.TakeSnapshot();

            _Server = TestUtilities.CreateMockImplementation<IRebroadcastServer>();
            _BroadcastProvider = TestUtilities.CreateMockImplementation<IBroadcastProvider>();

            _Feeds = new List<Mock<IFeed>>();
            _Listeners = new List<Mock<IListener>>();
            _FeedManager = FeedHelper.CreateMockFeedManager(_Feeds, _Listeners, 1, 2);

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(r => r.Load()).Returns(_Configuration);
            _RebroadcastSettings = new RebroadcastSettings() { UniqueId = 22, Name = "A", Enabled = true, Port = 1000, Format = RebroadcastFormat.Passthrough, ReceiverId = 1, StaleSeconds = 3 };
            _Configuration.RebroadcastSettings.Add(_RebroadcastSettings);

            _Manager = Factory.Singleton.Resolve<IRebroadcastServerManager>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalClassFactory);
            _Manager.Dispose();
        }
        #endregion

        #region Constructors and Properties
        [TestMethod]
        public void RebroadcastServerManager_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var manager = Factory.Singleton.Resolve<IRebroadcastServerManager>();

            Assert.AreEqual(0, manager.RebroadcastServers.Count);
            TestUtilities.TestProperty(manager, r => r.Online, false);
        }

        [TestMethod]
        public void RebroadcastServerManager_Singleton_Returns_Same_Reference_For_All_Instances()
        {
            var instance1 = Factory.Singleton.Resolve<IRebroadcastServerManager>();
            var instance2 = Factory.Singleton.Resolve<IRebroadcastServerManager>();

            Assert.IsNotNull(instance1.Singleton);
            Assert.AreSame(instance1.Singleton, instance2.Singleton);
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RebroadcastServerManager_Initialise_Throws_If_Called_More_Than_Once()
        {
            _Manager.Initialise();
            _Manager.Initialise();
        }

        [TestMethod]
        public void RebroadcastServerManager_Initialise_Creates_Servers_For_Each_Configured_RebroadcastSettings()
        {
            _Manager.Initialise();

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreSame(_Server.Object, _Manager.RebroadcastServers[0]);

            Assert.AreEqual(22, _Server.Object.UniqueId);
            Assert.AreEqual("A", _Server.Object.Name);
            Assert.AreSame(_Listeners[0].Object, _Server.Object.Listener);
            Assert.AreSame(_BroadcastProvider.Object, _Server.Object.BroadcastProvider);
            Assert.AreEqual(1000, _BroadcastProvider.Object.Port);
            Assert.AreEqual(RebroadcastFormat.Passthrough, _Server.Object.Format);
        }

        [TestMethod]
        public void RebroadcastServerManager_Initialise_Does_Not_Create_Servers_For_Settings_That_Are_Disabled()
        {
            _RebroadcastSettings.Enabled = false;
            _Manager.Initialise();

            Assert.AreEqual(0, _Manager.RebroadcastServers.Count);
            _Server.Verify(r => r.Initialise(), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServerManager_Initialise_Calls_Initialise_On_Servers()
        {
            _Manager.Initialise();

            _Server.Verify(r => r.Initialise(), Times.Once());
        }

        [TestMethod]
        public void RebroadcastServerManager_Initialise_Does_Not_Put_New_Servers_Online()
        {
            _Manager.Initialise();

            Assert.AreEqual(false, _Manager.RebroadcastServers[0].Online);
            Assert.IsFalse(_Manager.Online);
        }
        #endregion

        #region ConfigurationChanged
        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Creates_Servers_Added_Since_Initialise_Was_Called()
        {
            _RebroadcastSettings.Enabled = false;
            _Manager.Initialise();

            _RebroadcastSettings.Enabled = true;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreSame(_Server.Object, _Manager.RebroadcastServers[0]);

            Assert.AreSame(_Listeners[0].Object, _Server.Object.Listener);
            Assert.AreSame(_BroadcastProvider.Object, _Server.Object.BroadcastProvider);
            Assert.AreEqual(1000, _BroadcastProvider.Object.Port);
            Assert.AreEqual(RebroadcastFormat.Passthrough, _Server.Object.Format);
            Assert.AreEqual(false, _Server.Object.Online);
            Assert.AreEqual(3, _BroadcastProvider.Object.StaleSeconds);
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Does_Not_Create_Servers_If_Feed_Does_Not_Exist()
        {
            _RebroadcastSettings.Enabled = false;
            _Manager.Initialise();

            _RebroadcastSettings.ReceiverId = 3;

            _RebroadcastSettings.Enabled = true;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, _Manager.RebroadcastServers.Count);
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Chooses_Correct_Feed()
        {
            _RebroadcastSettings.Enabled = false;
            _Manager.Initialise();

            _RebroadcastSettings.ReceiverId = 2;
            FeedHelper.AddFeeds(_Feeds, _Listeners, 2);

            _RebroadcastSettings.Enabled = true;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreSame(_Listeners[1].Object, _Manager.RebroadcastServers[0].Listener);
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Puts_New_Servers_Online_If_Manager_Is_Online()
        {
            _RebroadcastSettings.Enabled = false;
            _Manager.Initialise();

            _Manager.Online = true;
            _RebroadcastSettings.Enabled = true;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(true, _Server.Object.Online);
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Does_Nothing_Before_Initialise_Called()
        {
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _ConfigurationStorage.Verify(r => r.Load(), Times.Never());   // <-- if this fails then the manager is probably hooking configuration changed event in ctor instead of Initialise which means all dummy classes will be hooking it...
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Does_Not_Create_New_Server_If_Nothing_Changed()
        {
            _Manager.Initialise();

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            _Server.Verify(r => r.Dispose(), Times.Never());
            _BroadcastProvider.Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Copies_Change_Of_Name()
        {
            _Manager.Initialise();

            _RebroadcastSettings.Name = "B";
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _Server.Verify(r => r.Dispose(), Times.Never());
            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreEqual("B", _Manager.RebroadcastServers[0].Name);
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Disposes_Of_Server_If_Listener_Goes_Away()
        {
            _Manager.Initialise();

            FeedHelper.RemoveFeed(_Feeds, _Listeners, 1);
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, _Manager.RebroadcastServers.Count);
            _Server.Verify(r => r.Dispose(), Times.Once());
            _BroadcastProvider.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Disposes_Of_Old_And_Creates_New_Server_If_UniqueId_Changes()
        {
            _Manager.Initialise();

            _RebroadcastSettings.UniqueId = 99;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreEqual(99, _Server.Object.UniqueId);
            _Server.Verify(r => r.Dispose(), Times.Once());
            _BroadcastProvider.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Disposes_Of_Old_And_Creates_New_Server_If_Format_Changes()
        {
            _Manager.Initialise();

            _RebroadcastSettings.Format = RebroadcastFormat.Port30003;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreEqual(RebroadcastFormat.Port30003, _Server.Object.Format);
            _Server.Verify(r => r.Dispose(), Times.Once());
            _BroadcastProvider.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Disposes_Of_Old_Before_Creating_New_Server_If_Format_Changes()
        {
            _Manager.Initialise();

            var disposeCalled = false;
            _Server.Setup(r => r.Dispose()).Callback(() => { disposeCalled = true; });
            _Server.Setup(r => r.Initialise()).Callback(() => { Assert.IsTrue(disposeCalled); });

            _RebroadcastSettings.Format = RebroadcastFormat.Port30003;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _Server.Verify(r => r.Initialise(), Times.Exactly(2));
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Disposes_Of_Old_And_Creates_New_Server_If_ReceiverId_Changes()
        {
            _Manager.Initialise();

            _RebroadcastSettings.ReceiverId = 2;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreSame(_Listeners[1].Object, _Manager.RebroadcastServers[0].Listener);
            _Server.Verify(r => r.Dispose(), Times.Once());
            _BroadcastProvider.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Disposes_Of_Old_And_Creates_New_Server_If_Port_Changes()
        {
            _Manager.Initialise();

            _RebroadcastSettings.Port = 8080;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreEqual(8080, _BroadcastProvider.Object.Port);
            _Server.Verify(r => r.Dispose(), Times.Once());
            _BroadcastProvider.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Disposes_Of_Old_If_Enabled_Changes()
        {
            _Manager.Initialise();

            _RebroadcastSettings.Enabled = false;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, _Manager.RebroadcastServers.Count);
            _Server.Verify(r => r.Dispose(), Times.Once());
            _BroadcastProvider.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void RebroadcastServerManager_ConfigurationChanged_Keeps_Existing_And_Sets_Property_If_StaleSeconds_Changes()
        {
            _Manager.Initialise();

            _RebroadcastSettings.StaleSeconds = 10;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreEqual(10, _BroadcastProvider.Object.StaleSeconds);
            _Server.Verify(r => r.Dispose(), Times.Never());
            _BroadcastProvider.Verify(r => r.Dispose(), Times.Never());
        }
        #endregion

        #region FeedsChanged
        [TestMethod]
        public void RebroadcastServerManager_FeedsChanged_Creates_Servers_For_New_Feeds()
        {
            _RebroadcastSettings.ReceiverId = 99;
            _Manager.Initialise();
            Assert.AreEqual(0, _Manager.RebroadcastServers.Count);

            FeedHelper.AddFeeds(_Feeds, _Listeners, 99);
            _FeedManager.Raise(r => r.FeedsChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.RebroadcastServers.Count);
            Assert.AreSame(_Server.Object, _Manager.RebroadcastServers[0]);
            Assert.AreSame(_Listeners[2].Object, _Server.Object.Listener);
            Assert.AreSame(_BroadcastProvider.Object, _Server.Object.BroadcastProvider);
            Assert.AreEqual(1000, _BroadcastProvider.Object.Port);
            Assert.AreEqual(RebroadcastFormat.Passthrough, _Server.Object.Format);
            Assert.AreEqual(false, _Server.Object.Online);
        }

        [TestMethod]
        public void RebroadcastServerManager_FeedsChanged_Removes_Old_Feeds()
        {
            _Manager.Initialise();

            FeedHelper.RemoveFeed(_Feeds, _Listeners, 1);
            _FeedManager.Raise(r => r.FeedsChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, _Manager.RebroadcastServers.Count);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void RebroadcastServerManager_Dispose_Disposes_Of_Existing_Servers()
        {
            _Manager.Initialise();
            _Manager.Dispose();

            Assert.AreEqual(0, _Manager.RebroadcastServers.Count);
            _Server.Verify(r => r.Dispose(), Times.Once());
            _BroadcastProvider.Verify(r => r.Dispose(), Times.Once());
            _Feeds[0].Verify(r => r.Dispose(), Times.Never());
            _Listeners[0].Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServerManager_Dispose_Prevents_Configuration_Changes_From_Creating_New_Servers()
        {
            _RebroadcastSettings.Enabled = false;
            _Manager.Initialise();
            _Manager.Dispose();

            _RebroadcastSettings.Enabled = true;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, _Manager.RebroadcastServers.Count);
            _Server.Verify(r => r.Initialise(), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServerManager_Dispose_Prevents_FeedManager_Changes_From_Creating_New_Servers()
        {
            FeedHelper.RemoveFeed(_Feeds, _Listeners, 1);
            _Manager.Initialise();
            _Manager.Dispose();

            FeedHelper.AddFeeds(_Feeds, _Listeners, 1);
            _FeedManager.Raise(r => r.FeedsChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, _Manager.RebroadcastServers.Count);
            _Server.Verify(r => r.Initialise(), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServerManager_Dispose_Can_Be_Called_On_Uninitialised_Object()
        {
            _Manager.Dispose();
            // No assertion - this just has to not throw any exceptions
        }
        #endregion

        #region Online Property
        [TestMethod]
        public void RebroadcastServerManager_Online_True_Sets_All_Servers_Online()
        {
            _Manager.Initialise();

            _Manager.Online = true;

            Assert.IsTrue(_Server.Object.Online);
        }

        [TestMethod]
        public void RebroadcastServerManager_Online_False_Sets_All_Servers_Offline()
        {
            _Manager.Initialise();

            _Manager.Online = false;

            Assert.IsFalse(_Server.Object.Online);
        }

        [TestMethod]
        public void RebroadcastServerManager_Online_Only_Passed_To_Servers_If_Changed()
        {
            _Manager.Initialise();
            _Manager.Online = false;

            _Server.VerifySet(r => r.Online = false, Times.Never());

            _Manager.Online = true;
            _Manager.Online = true;
            _Server.VerifySet(r => r.Online = true, Times.Once());
        }
        #endregion

        #region OnlineChanged
        [TestMethod]
        public void RebroadcastServerManager_OnlineChanged_Raised_When_Online_Set_To_True()
        {
            var eventRecorder = new EventRecorder<EventArgs>();
            eventRecorder.EventRaised += (s, a) => { Assert.AreEqual(true, _Manager.Online); };
            _Manager.OnlineChanged += eventRecorder.Handler;

            _Manager.Online = true;

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreSame(_Manager, eventRecorder.Sender);
            Assert.IsNotNull(eventRecorder.Args);
        }

        [TestMethod]
        public void RebroadcastServerManager_OnlineChanged_Raised_When_Online_Set_To_False()
        {
            _Manager.Online = true;

            var eventRecorder = new EventRecorder<EventArgs>();
            eventRecorder.EventRaised += (s, a) => { Assert.AreEqual(false, _Manager.Online); };
            _Manager.OnlineChanged += eventRecorder.Handler;

            _Manager.Online = false;

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreSame(_Manager, eventRecorder.Sender);
            Assert.IsNotNull(eventRecorder.Args);
        }

        [TestMethod]
        public void RebroadcastServerManager_OnlineChanged_Is_Not_Passed_Through_From_Servers()
        {
            var eventRecorder = new EventRecorder<EventArgs>();
            _Manager.OnlineChanged += eventRecorder.Handler;

            _Manager.Initialise();
            _Server.Raise(r => r.OnlineChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, eventRecorder.CallCount);
        }
        #endregion

        #region ClientConnected
        [TestMethod]
        public void RebroadcastServerManager_ClientConnected_Passed_Through_From_Broadcast_Providers()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.ClientConnected += eventRecorder.Handler;
            var endPoint = new IPEndPoint(new IPAddress(0x2414188d), 900);

            _Manager.Initialise();
            _BroadcastProvider.Raise(r => r.ClientConnected += null, new BroadcastEventArgs(12, endPoint, 100, 8080));

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreEqual(12, eventRecorder.Args.RebroadcastServerId);
            Assert.AreSame(endPoint, eventRecorder.Args.EndPoint);
            Assert.AreEqual(100, eventRecorder.Args.BytesSent);
            Assert.AreEqual(8080, eventRecorder.Args.Port);
            Assert.AreSame(_Manager, eventRecorder.Sender);
        }

        [TestMethod]
        public void RebroadcastServerManager_ClientConnected_Not_Raised_For_Disposed_BroadcastProviders()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.ClientConnected += eventRecorder.Handler;

            _Manager.Initialise();
            _Manager.Dispose();
            _BroadcastProvider.Raise(r => r.ClientConnected += null, new BroadcastEventArgs(12, new IPEndPoint(new IPAddress(0x24252627), 8080), 100, 8080));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }

        [TestMethod]
        public void RebroadcastServerManager_ClientConnected_Not_Raised_For_BroadcastProviders_Removed_Due_To_Configuration_Change()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.ClientConnected += eventRecorder.Handler;

            _Manager.Initialise();
            _RebroadcastSettings.Enabled = false;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);
            _BroadcastProvider.Raise(r => r.ClientConnected += null, new BroadcastEventArgs(12, new IPEndPoint(new IPAddress(0x24252627), 8080), 100, 8080));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }
        #endregion

        #region BroadcastSending
        [TestMethod]
        public void RebroadcastServerManager_BroadcastSending_Passed_Through_From_Broadcast_Providers()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.BroadcastSending += eventRecorder.Handler;
            var endPoint = new IPEndPoint(new IPAddress(0x2414188d), 900);

            _Manager.Initialise();
            _BroadcastProvider.Raise(r => r.BroadcastSending += null, new BroadcastEventArgs(12, endPoint, 100, 8080));

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreEqual(12, eventRecorder.Args.RebroadcastServerId);
            Assert.AreSame(endPoint, eventRecorder.Args.EndPoint);
            Assert.AreEqual(100, eventRecorder.Args.BytesSent);
            Assert.AreEqual(8080, eventRecorder.Args.Port);
            Assert.AreSame(_Manager, eventRecorder.Sender);
        }

        [TestMethod]
        public void RebroadcastServerManager_BroadcastSending_Not_Raised_For_Disposed_BroadcastProviders()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.BroadcastSending += eventRecorder.Handler;

            _Manager.Initialise();
            _Manager.Dispose();
            _BroadcastProvider.Raise(r => r.BroadcastSending += null, new BroadcastEventArgs(12, new IPEndPoint(new IPAddress(0x24252627), 8080), 100, 8080));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }

        [TestMethod]
        public void RebroadcastServerManager_BroadcastSending_Not_Raised_For_BroadcastProviders_Removed_Due_To_Configuration_Change()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.BroadcastSending += eventRecorder.Handler;

            _Manager.Initialise();
            _RebroadcastSettings.Enabled = false;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);
            _BroadcastProvider.Raise(r => r.BroadcastSending += null, new BroadcastEventArgs(12, new IPEndPoint(new IPAddress(0x24252627), 8080), 100, 8080));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }
        #endregion

        #region BroadcastSent
        [TestMethod]
        public void RebroadcastServerManager_BroadcastSent_Passed_Through_From_Broadcast_Providers()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.BroadcastSent += eventRecorder.Handler;
            var endPoint = new IPEndPoint(new IPAddress(0x2414188d), 900);

            _Manager.Initialise();
            _BroadcastProvider.Raise(r => r.BroadcastSent += null, new BroadcastEventArgs(12, endPoint, 100, 8080));

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreEqual(12, eventRecorder.Args.RebroadcastServerId);
            Assert.AreSame(endPoint, eventRecorder.Args.EndPoint);
            Assert.AreEqual(100, eventRecorder.Args.BytesSent);
            Assert.AreEqual(8080, eventRecorder.Args.Port);
            Assert.AreSame(_Manager, eventRecorder.Sender);
        }

        [TestMethod]
        public void RebroadcastServerManager_BroadcastSent_Not_Raised_For_Disposed_BroadcastProviders()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.BroadcastSent += eventRecorder.Handler;

            _Manager.Initialise();
            _Manager.Dispose();
            _BroadcastProvider.Raise(r => r.BroadcastSent += null, new BroadcastEventArgs(12, new IPEndPoint(new IPAddress(0x24252627), 8080), 100, 8080));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }

        [TestMethod]
        public void RebroadcastServerManager_BroadcastSent_Not_Raised_For_BroadcastProviders_Removed_Due_To_Configuration_Change()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.BroadcastSent += eventRecorder.Handler;

            _Manager.Initialise();
            _RebroadcastSettings.Enabled = false;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);
            _BroadcastProvider.Raise(r => r.BroadcastSent += null, new BroadcastEventArgs(12, new IPEndPoint(new IPAddress(0x24252627), 8080), 100, 8080));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }
        #endregion

        #region ClientDisconnected
        [TestMethod]
        public void RebroadcastServerManager_ClientDisconnected_Passed_Through_From_Broadcast_Providers()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.ClientDisconnected += eventRecorder.Handler;
            var endPoint = new IPEndPoint(new IPAddress(0x2414188d), 900);

            _Manager.Initialise();
            _BroadcastProvider.Raise(r => r.ClientDisconnected += null, new BroadcastEventArgs(12, endPoint, 100, 8080));

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreEqual(12, eventRecorder.Args.RebroadcastServerId);
            Assert.AreSame(endPoint, eventRecorder.Args.EndPoint);
            Assert.AreEqual(100, eventRecorder.Args.BytesSent);
            Assert.AreEqual(8080, eventRecorder.Args.Port);
            Assert.AreSame(_Manager, eventRecorder.Sender);
        }

        [TestMethod]
        public void RebroadcastServerManager_ClientDisconnected_Not_Raised_For_Disposed_BroadcastProviders()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.ClientDisconnected += eventRecorder.Handler;

            _Manager.Initialise();
            _Manager.Dispose();
            _BroadcastProvider.Raise(r => r.ClientDisconnected += null, new BroadcastEventArgs(12, new IPEndPoint(new IPAddress(0x24252627), 8080), 100, 8080));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }

        [TestMethod]
        public void RebroadcastServerManager_ClientDisconnected_Not_Raised_For_BroadcastProviders_Removed_Due_To_Configuration_Change()
        {
            var eventRecorder = new EventRecorder<BroadcastEventArgs>();
            _Manager.ClientDisconnected += eventRecorder.Handler;

            _Manager.Initialise();
            _RebroadcastSettings.Enabled = false;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);
            _BroadcastProvider.Raise(r => r.ClientDisconnected += null, new BroadcastEventArgs(12, new IPEndPoint(new IPAddress(0x24252627), 8080), 100, 8080));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }
        #endregion

        #region ExceptionCaught
        [TestMethod]
        public void RebroadcastServerManager_ExceptionCaught_Passed_Through_From_Broadcast_Providers()
        {
            var eventRecorder = new EventRecorder<EventArgs<Exception>>();
            _Manager.ExceptionCaught += eventRecorder.Handler;
            var exception = new InvalidOperationException();

            _Manager.Initialise();
            _BroadcastProvider.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreSame(exception, eventRecorder.Args.Value);
            Assert.AreSame(_Manager, eventRecorder.Sender);
        }

        [TestMethod]
        public void RebroadcastServerManager_ExceptionCaught_Not_Raised_For_Disposed_BroadcastProviders()
        {
            var eventRecorder = new EventRecorder<EventArgs<Exception>>();
            _Manager.ExceptionCaught += eventRecorder.Handler;

            _Manager.Initialise();
            _Manager.Dispose();
            _BroadcastProvider.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(new InvalidOperationException()));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }

        [TestMethod]
        public void RebroadcastServerManager_ExceptionCaught_Not_Raised_For_BroadcastProviders_Removed_Due_To_Configuration_Change()
        {
            var eventRecorder = new EventRecorder<EventArgs<Exception>>();
            _Manager.ExceptionCaught += eventRecorder.Handler;

            _Manager.Initialise();
            _RebroadcastSettings.Enabled = false;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);
            _BroadcastProvider.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(new InvalidOperationException()));

            Assert.AreEqual(0, eventRecorder.CallCount);
        }
        #endregion
    }
}
