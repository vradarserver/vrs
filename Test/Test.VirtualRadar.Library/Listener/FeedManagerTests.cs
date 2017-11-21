// Copyright © 2013 onwards, Andrew Whewell
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
using System.IO.Ports;
using System.Linq;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.Listener;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class FeedManagerTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _SnapshotFactory;
        private IFeedManager _Manager;
        private List<Mock<IFeed>> _CreatedFeeds;
        private List<Mock<IListener>> _CreatedListeners;
        private Dictionary<MergedFeed, List<IFeed>> _MergedFeedFeeds;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Receiver _Receiver1;
        private Receiver _Receiver2;
        private Receiver _Receiver3;
        private Receiver _Receiver4;
        private MergedFeed _MergedFeed1;
        private MergedFeed _MergedFeed2;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtRecorder;
        private EventRecorder<EventArgs> _FeedsChangedRecorder;
        private EventRecorder<EventArgs<IFeed>> _ConnectionStateChangedRecorder;

        [TestInitialize]
        public void TestInitialise()
        {
            _SnapshotFactory = Factory.TakeSnapshot();

            _Manager = Factory.Singleton.ResolveNewInstance<IFeedManager>();

            _Receiver1 = new Receiver() { UniqueId = 1, Name = "First", DataSource = DataSource.Port30003, ConnectionType = ConnectionType.TCP, Address = "127.0.0.1", Port = 30003 };
            _Receiver2 = new Receiver() { UniqueId = 2, Name = "Second", DataSource = DataSource.Beast, ConnectionType = ConnectionType.COM, ComPort = "COM1", BaudRate = 19200, DataBits = 8, StopBits = StopBits.One };
            _Receiver3 = new Receiver() { UniqueId = 3, Name = "Third", ReceiverUsage = ReceiverUsage.HideFromWebSite, };
            _Receiver4 = new Receiver() { UniqueId = 4, Name = "Fourth", ReceiverUsage = ReceiverUsage.MergeOnly, };
            _MergedFeed1 = new MergedFeed() { UniqueId = 5, Name = "M1", ReceiverIds = { 1, 2 }, ReceiverUsage = ReceiverUsage.Normal, };
            _MergedFeed2 = new MergedFeed() { UniqueId = 6, Name = "M2", ReceiverIds = { 3, 4 }, ReceiverUsage = ReceiverUsage.HideFromWebSite, };
            _Configuration = new Configuration() {
                Receivers = { _Receiver1, _Receiver2, _Receiver3, _Receiver4 },
                MergedFeeds = { _MergedFeed1, _MergedFeed2 },
            };
            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _ConfigurationStorage.Setup(r => r.Load()).Returns(_Configuration);

            _CreatedListeners = new List<Mock<IListener>>();
            _CreatedFeeds = new List<Mock<IFeed>>();
            _MergedFeedFeeds = new Dictionary<MergedFeed,List<IFeed>>();
            Factory.Singleton.Register<IFeed>(() => {
                var feed = TestUtilities.CreateMockInstance<IFeed>();
                var listener = TestUtilities.CreateMockInstance<IListener>();
                _CreatedListeners.Add(listener);

                feed.Setup(r => r.Initialise(It.IsAny<Receiver>(), It.IsAny<Configuration>())).Callback((Receiver rcvr, Configuration conf) => {
                    feed.Setup(i => i.UniqueId).Returns(rcvr.UniqueId);
                    feed.Setup(i => i.Name).Returns(rcvr.Name);
                    feed.Setup(i => i.Listener).Returns(listener.Object);
                    feed.Setup(i => i.IsVisible).Returns(rcvr.ReceiverUsage == ReceiverUsage.Normal);
                });

                feed.Setup(r => r.Initialise(It.IsAny<MergedFeed>(), It.IsAny<IEnumerable<IFeed>>())).Callback((MergedFeed mfeed, IEnumerable<IFeed> feeds) => {
                    feed.Setup(i => i.UniqueId).Returns(mfeed.UniqueId);
                    feed.Setup(i => i.Name).Returns(mfeed.Name);
                    feed.Setup(i => i.Listener).Returns(listener.Object);
                    feed.Setup(i => i.IsVisible).Returns(mfeed.ReceiverUsage == ReceiverUsage.Normal);

                    if(_MergedFeedFeeds.ContainsKey(mfeed)) _MergedFeedFeeds[mfeed] = feeds.ToList();
                    else                                    _MergedFeedFeeds.Add(mfeed, feeds.ToList());
                });

                feed.Setup(r => r.ApplyConfiguration(It.IsAny<MergedFeed>(), It.IsAny<IEnumerable<IFeed>>())).Callback((MergedFeed mfeed, IEnumerable<IFeed> feeds) => {
                    if(_MergedFeedFeeds.ContainsKey(mfeed)) _MergedFeedFeeds[mfeed] = feeds.ToList();
                    else                                    _MergedFeedFeeds.Add(mfeed, feeds.ToList());
                });

                _CreatedFeeds.Add(feed);
                return feed.Object;
            });

            _ExceptionCaughtRecorder = new EventRecorder<EventArgs<Exception>>();
            _FeedsChangedRecorder = new EventRecorder<EventArgs>();
            _ConnectionStateChangedRecorder = new EventRecorder<EventArgs<IFeed>>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_SnapshotFactory);
            _Manager.Dispose();
        }

        void OnlyUseTwoReceiversAndNoMergedFeed()
        {
            _Configuration.Receivers.Remove(_Receiver3);
            _Configuration.Receivers.Remove(_Receiver4);
            _Configuration.MergedFeeds.Clear();

            _Receiver3 = _Receiver4 = null;
            _MergedFeed1 = _MergedFeed2 = null;
        }
        #endregion

        #region Constructors and Properties
        [TestMethod]
        public void FeedManager_Constructor_Initialises_To_Known_Value_And_Properties_Work()
        {
            _Manager.Dispose();
            _Manager = Factory.Singleton.ResolveNewInstance<IFeedManager>();

            Assert.AreEqual(0, _Manager.Feeds.Length);
            Assert.AreEqual(0, _Manager.VisibleFeeds.Length);
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void FeedManager_Initialise_Throws_If_Called_Twice()
        {
            _Manager.Initialise();
            _Manager.Initialise();
        }

        [TestMethod]
        public void FeedManager_Initialise_Causes_Manager_To_Create_And_Initialise_ReceiverPathways_For_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Manager.Initialise();

            Assert.AreEqual(2, _CreatedFeeds.Count);
            _CreatedFeeds[0].Verify(r => r.Initialise(_Receiver1, _Configuration), Times.Once());
            _CreatedFeeds[1].Verify(r => r.Initialise(_Receiver2, _Configuration), Times.Once());
        }

        [TestMethod]
        public void FeedManager_Initialise_Causes_Manager_To_Create_And_Initialise_ReceiverPathways_For_MergedFeeds()
        {
            _Manager.Initialise();

            Assert.AreEqual(6, _CreatedFeeds.Count);
            _CreatedFeeds[4].Verify(r => r.Initialise(_MergedFeed1, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
            _CreatedFeeds[5].Verify(r => r.Initialise(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Once());

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed1].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed1].Contains(_CreatedFeeds[0].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed1].Contains(_CreatedFeeds[1].Object));

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedFeeds[2].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedFeeds[3].Object));
        }

        [TestMethod]
        public void FeedManager_Initialise_Does_Not_Create_Pathways_For_Disabled_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Receiver1.Enabled = false;
            _Manager.Initialise();

            Assert.AreEqual(1, _CreatedFeeds.Count);
            _CreatedFeeds[0].Verify(r => r.Initialise(_Receiver2, _Configuration), Times.Once());
        }

        [TestMethod]
        public void FeedManager_Initialise_Does_Not_Create_Pathways_For_Disabled_MergedFeeds()
        {
            _MergedFeed1.Enabled = false;
            _Manager.Initialise();

            Assert.AreEqual(5, _CreatedFeeds.Count);
            _CreatedFeeds[4].Verify(r => r.Initialise(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
        }

        [TestMethod]
        public void FeedManager_Initialise_Exposes_Created_Feeds_In_Feeds_Property()
        {
            _Manager.Initialise();

            Assert.AreEqual(6, _Manager.Feeds.Length);
            Assert.AreSame(_CreatedFeeds[0].Object, _Manager.Feeds[0]);
            Assert.AreSame(_CreatedFeeds[1].Object, _Manager.Feeds[1]);
            Assert.AreSame(_CreatedFeeds[2].Object, _Manager.Feeds[2]);
            Assert.AreSame(_CreatedFeeds[3].Object, _Manager.Feeds[3]);
            Assert.AreSame(_CreatedFeeds[4].Object, _Manager.Feeds[4]);
            Assert.AreSame(_CreatedFeeds[5].Object, _Manager.Feeds[5]);
        }

        [TestMethod]
        public void FeedManager_Initialise_Exposes_Visible_Feeds_In_VisibleFeeds_Property()
        {
            // Receivers 3 & 4 should not be visible, neither should merged feed 2
            _Manager.Initialise();

            Assert.AreEqual(3, _Manager.VisibleFeeds.Length);
            Assert.IsFalse(_Manager.VisibleFeeds.Any(r => r.UniqueId == _Receiver3.UniqueId));
            Assert.IsFalse(_Manager.VisibleFeeds.Any(r => r.UniqueId == _Receiver4.UniqueId));
            Assert.IsFalse(_Manager.VisibleFeeds.Any(r => r.UniqueId == _MergedFeed2.UniqueId));
        }

        [TestMethod]
        public void FeedManager_Initialise_Hooks_ExceptionCaught_On_Pathways()
        {
            _Manager.Initialise();
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();

            // Pathway created from receiver
            _CreatedFeeds[0].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));
            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Manager, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);

            // Pathway created from merged feed
            _CreatedFeeds[4].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));
            Assert.AreEqual(2, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Manager, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void FeedManager_Initialise_Raises_FeedsChanged()
        {
            _Manager.FeedsChanged += _FeedsChangedRecorder.Handler;
            _Manager.Initialise();

            Assert.AreEqual(1, _FeedsChangedRecorder.CallCount);
            Assert.AreSame(_Manager, _FeedsChangedRecorder.Sender);
        }

        [TestMethod]
        public void FeedManager_Initialise_Connects_Listeners()
        {
            _Receiver1.AutoReconnectAtStartup = true;
            _Receiver2.AutoReconnectAtStartup = false;

            _Manager.Initialise();

            _CreatedListeners[0].Verify(r => r.Connect(), Times.Once());
            _CreatedListeners[1].Verify(r => r.Connect(), Times.Once());

            // Merged feeds are not connected - they don't have any connection state, they always report connected
            _CreatedListeners[4].Verify(r => r.Connect(), Times.Never());
            _CreatedListeners[5].Verify(r => r.Connect(), Times.Never());
        }
        #endregion

        #region ConfigurationChanged
        [TestMethod]
        public void FeedManager_ConfigurationChanged_Updates_Existing_ReceiverPathways_For_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Manager.Initialise();

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[0].Verify(r => r.ApplyConfiguration(_Receiver1, _Configuration), Times.Once());
            _CreatedFeeds[1].Verify(r => r.ApplyConfiguration(_Receiver2, _Configuration), Times.Once());
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Updates_Existing_ReceiverPathways_For_Merged_Feeds()
        {
            _Manager.Initialise();
            _MergedFeedFeeds.Clear();

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[4].Verify(r => r.ApplyConfiguration(_MergedFeed1, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
            _CreatedFeeds[5].Verify(r => r.ApplyConfiguration(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Once());

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed1].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed1].Contains(_CreatedFeeds[0].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed1].Contains(_CreatedFeeds[1].Object));

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedFeeds[2].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedFeeds[3].Object));
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Determines_Existing_ReceiverPathways_By_UniqueId_For_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Manager.Initialise();
            _Receiver1.Name = "New Name";

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[0].Verify(r => r.ApplyConfiguration(_Receiver1, _Configuration), Times.Once());
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Determines_Existing_ReceiverPathways_By_UniqueId_For_MergedFeeds()
        {
            _Manager.Initialise();
            _MergedFeed1.Name = "New Name";

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[4].Verify(r => r.ApplyConfiguration(_MergedFeed1, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Disposes_Of_Old_ReceiverPathways()
        {
            _Manager.Initialise();
            _Configuration.Receivers.RemoveAt(0);
            _Configuration.MergedFeeds.RemoveAt(0);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[0].Verify(r => r.Dispose(), Times.Once());
            _CreatedFeeds[4].Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Identifies_Old_ReceiverPathways_By_UniqueId()
        {
            _Manager.Initialise();
            _Receiver1.Name = "New Name";
            _MergedFeed1.Name = "Another New Name";

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[0].Verify(r => r.Dispose(), Times.Never());
            _CreatedFeeds[4].Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Unhooks_Disposed_ReceiverPathways()
        {
            _Manager.Initialise();
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;
            _Configuration.Receivers.RemoveAt(0);
            _Configuration.MergedFeeds.RemoveAt(0);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[0].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(new Exception()));
            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);

            _CreatedFeeds[4].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(new Exception()));
            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Disposes_Of_ReceiverPathways_That_Have_Been_Disabled()
        {
            _Manager.Initialise();
            _Receiver1.Enabled = false;
            _MergedFeed1.Enabled = false;

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[0].Verify(r => r.Dispose(), Times.Once());
            _CreatedFeeds[4].Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Creates_New_Pathways_For_New_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Configuration.Receivers.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.Receivers.Add(_Receiver2);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[1].Verify(r => r.Initialise(_Receiver2, _Configuration), Times.Once());
            _CreatedFeeds[1].Verify(r => r.ApplyConfiguration(_Receiver2, _Configuration), Times.Never());
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Connects_New_Receivers()
        {
            foreach(var reconnect in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                OnlyUseTwoReceiversAndNoMergedFeed();
                _Configuration.Receivers.RemoveAt(1);
                _Manager.Initialise();
                _Receiver2.AutoReconnectAtStartup = reconnect;
                _Configuration.Receivers.Add(_Receiver2);

                _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

                _CreatedListeners[1].Verify(r => r.Connect(), Times.Once());
            }
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Does_Not_Create_New_Feeds_For_Disabled_New_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Configuration.Receivers.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.Receivers.Add(_Receiver2);
            _Receiver2.Enabled = false;

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.Feeds.Length);
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Creates_New_Feeds_For_New_MergedFeeds()
        {
            _Configuration.MergedFeeds.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.MergedFeeds.Add(_MergedFeed2);

            _MergedFeedFeeds.Clear();
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedFeeds[5].Verify(r => r.Initialise(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
            _CreatedFeeds[5].Verify(r => r.ApplyConfiguration(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Never());

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedFeeds[2].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedFeeds[3].Object));
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Creates_New_Feeds_For_Disabled_New_MergedFeeds()
        {
            _Configuration.MergedFeeds.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.MergedFeeds.Add(_MergedFeed2);
            _MergedFeed2.Enabled = false;

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(5, _Manager.Feeds.Length);
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Creates_New_Merged_Feeds_After_All_Receivers_Have_Been_Created()
        {
            _Configuration.Receivers.Remove(_Receiver3);
            _Configuration.Receivers.Remove(_Receiver4);
            _Configuration.MergedFeeds.Clear();
            _Manager.Initialise();

            _Configuration.Receivers.Add(_Receiver3);
            _Configuration.Receivers.Add(_Receiver4);
            _Configuration.MergedFeeds.Add(_MergedFeed2);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedFeeds[2].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedFeeds[3].Object));
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Creates_New_Merged_Feeds_After_All_Receivers_Have_Been_Deleted()
        {
            _Configuration.MergedFeeds.Clear();
            _Manager.Initialise();

            _Configuration.Receivers.Remove(_Receiver4);
            _Configuration.MergedFeeds.Add(_MergedFeed2);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.AreSame(_CreatedFeeds[2].Object, _MergedFeedFeeds[_MergedFeed2][0]);
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Hooks_ExceptionCaught_On_New_Receivers()
        {
            _Configuration.Receivers.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.Receivers.Add(_Receiver2);
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            var exception = new InvalidOperationException();
            _CreatedFeeds[1].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Manager, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Hooks_ExceptionCaught_On_New_Merged_Feeds()
        {
            _Configuration.MergedFeeds.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.MergedFeeds.Add(_MergedFeed2);
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            var exception = new InvalidOperationException();
            _CreatedFeeds[5].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Manager, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Reflects_Changes_In_Feeds_Property()
        {
            _Manager.Initialise();

            _Receiver1.Enabled = false;
            _MergedFeed1.Enabled = false;
            _Configuration.Receivers.Add(new Receiver() { UniqueId = 100, Name = "New Receiver", Port = 10001 });
            _Configuration.MergedFeeds.Add(new MergedFeed() { UniqueId = 101, Name = "New Merged Feed", ReceiverIds = { 3, 4, 100 } });
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(6, _Manager.Feeds.Length);
            for(var i = 0;i < 8;++i) {
                var stillExists = i != 0 /* Receiver1 */ && i != 4 /* MergedFeed1 */;
                Assert.AreEqual(stillExists, _Manager.Feeds.Contains(_CreatedFeeds[i].Object));
            }
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Reflects_Changes_In_VisibleFeeds_Property()
        {
            _Manager.Initialise();

            _Configuration.Receivers.Add(new Receiver() { UniqueId = 100, Name = "New Receiver", Port = 10001 });

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(4, _Manager.VisibleFeeds.Length);
        }

        [TestMethod]
        public void FeedManager_ConfigurationChanged_Raises_FeedsChanged()
        {
            _Manager.Initialise();
            _Manager.FeedsChanged += _FeedsChangedRecorder.Handler;

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _FeedsChangedRecorder.CallCount);
            Assert.AreSame(_Manager, _FeedsChangedRecorder.Sender);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void FeedManager_Dispose_Disposes_Of_All_Pathways()
        {
            _Manager.Initialise();

            _Manager.Dispose();

            _CreatedFeeds[0].Verify(r => r.Dispose(), Times.Once());
            _CreatedFeeds[1].Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void FeedManager_Dispose_Unhooks_All_Pathways()
        {
            _Manager.Initialise();
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            _Manager.Dispose();
            var args = new EventArgs<Exception>(new Exception());
            _CreatedFeeds[0].Raise(r => r.ExceptionCaught += null, args);
            _CreatedFeeds[1].Raise(r => r.ExceptionCaught += null, args);

            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
        }

        [TestMethod]
        public void FeedManager_Dispose_Unhooks_ConfigurationManager()
        {
            _Manager.Initialise();
            _Manager.Dispose();

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, _Manager.Feeds.Length);
        }

        [TestMethod]
        public void FeedManager_Dispose_Clears_Down_Feeds_Property()
        {
            _Manager.Initialise();
            _Manager.Dispose();
            Assert.AreEqual(0, _Manager.Feeds.Length);
        }

        [TestMethod]
        public void FeedManager_Dispose_Clears_Down_VisibleFeeds_Property()
        {
            _Manager.Initialise();
            _Manager.Dispose();
            Assert.AreEqual(0, _Manager.VisibleFeeds.Length);
        }

        [TestMethod]
        public void FeedManager_Dispose_Can_Be_Called_Before_Initialise()
        {
            _Manager.Dispose();
        }

        [TestMethod]
        public void FeedManager_Dispose_Can_Be_Called_Twice()
        {
            _Manager.Dispose();
            _Manager.Dispose();
        }
        #endregion

        #region ConnectionStateChanged
        [TestMethod]
        public void FeedManager_ConnectionStateChanged_Raised_When_Listener_Raises_Event()
        {
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;
            _Manager.Initialise();

            _CreatedListeners[1].Raise(r => r.ConnectionStateChanged += null, EventArgs.Empty);
            Assert.AreEqual(1, _ConnectionStateChangedRecorder.CallCount);
            Assert.AreSame(_Manager, _ConnectionStateChangedRecorder.Sender);
            Assert.AreSame(_CreatedFeeds[1].Object, _ConnectionStateChangedRecorder.Args.Value);
        }

        [TestMethod]
        public void FeedManager_ConnectionStateChanged_Raised_When_Listener_Raises_Event_After_Being_Created_By_Configuration_Change()
        {
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;

            _Configuration.Receivers.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.Receivers.Add(_Receiver2);
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedListeners[1].Raise(r => r.ConnectionStateChanged += null, EventArgs.Empty);
            Assert.AreEqual(1, _ConnectionStateChangedRecorder.CallCount);
            Assert.AreSame(_Manager, _ConnectionStateChangedRecorder.Sender);
            Assert.AreSame(_CreatedFeeds[1].Object, _ConnectionStateChangedRecorder.Args.Value);
        }

        [TestMethod]
        public void FeedManager_ConnectionStateChanged_Not_Raised_When_Listener_Raises_Event_After_Removal_By_Configuration_Change()
        {
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;
            _Manager.Initialise();
            _Configuration.Receivers.RemoveAt(1);
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedListeners[1].Raise(r => r.ConnectionStateChanged += null, EventArgs.Empty);
            Assert.AreEqual(0, _ConnectionStateChangedRecorder.CallCount);
        }
        #endregion

        #region GetByName
        [TestMethod]
        public void FeedManager_GetByName_Returns_Null_If_Passed_Null()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByName(null, false));
        }

        [TestMethod]
        public void FeedManager_GetByName_Returns_Feed_If_Passed_Matching_Name()
        {
            _Manager.Initialise();
            Assert.AreSame(_CreatedFeeds[0].Object, _Manager.GetByName(_Receiver1.Name, false));
        }

        [TestMethod]
        public void FeedManager_GetByName_Can_Ignore_Invisible_Feeds()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByName(_Receiver3.Name, ignoreInvisibleFeeds: true));
        }

        [TestMethod]
        public void FeedManager_GetByName_Can_Return_Invisible_Feeds()
        {
            _Manager.Initialise();
            Assert.IsNotNull(_Manager.GetByName(_Receiver3.Name, ignoreInvisibleFeeds: false));
        }

        [TestMethod]
        public void FeedManager_GetByName_Is_Case_Insensitive()
        {
            _Manager.Initialise();
            Assert.AreSame(_CreatedFeeds[0].Object, _Manager.GetByName(_Receiver1.Name.ToLower(), false));
            Assert.AreSame(_CreatedFeeds[0].Object, _Manager.GetByName(_Receiver1.Name.ToUpper(), false));
        }

        [TestMethod]
        public void FeedManager_GetByName_Returns_Null_If_Not_Found()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByName("DOES NOT EXIST", false));
        }
        #endregion

        #region GetByUniqueId
        [TestMethod]
        public void FeedManager_GetByUniqueId_Returns_Pathway_With_Matching_UniqueId()
        {
            _Manager.Initialise();
            Assert.AreSame(_CreatedFeeds[0].Object, _Manager.GetByUniqueId(_Receiver1.UniqueId, false));
        }

        [TestMethod]
        public void FeedManager_GetByUniqueId_Returns_Null_If_No_Record_Matches()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByUniqueId(_Manager.Feeds.Select(r => r.UniqueId).Max() + 1, false));
        }

        [TestMethod]
        public void FeedManager_GetByUniqueId_Can_Ignore_Invisible_Feeds()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByUniqueId(_Receiver3.UniqueId, ignoreInvisibleFeeds: true));
        }

        [TestMethod]
        public void FeedManager_GetByUniqueId_Can_Return_Invisible_Feeds()
        {
            _Manager.Initialise();
            Assert.IsNotNull(_Manager.GetByUniqueId(_Receiver3.UniqueId, ignoreInvisibleFeeds: false));
        }
        #endregion

        #region Connect
        [TestMethod]
        public void FeedManager_Connect_Passes_The_Call_Through_To_Listeners()
        {
            _Manager.Initialise();

            // Can't use Verify as that will count the connect from Initialise as well and it gets a bit messy
            bool seenConnectCall = false;
            _CreatedListeners[0].Setup(r => r.Connect()).Callback(() => seenConnectCall = true);

            _Manager.Connect();

            Assert.IsTrue(seenConnectCall);
        }
        #endregion

        #region Disconnect
        [TestMethod]
        public void FeedManager_Disconnect_Passes_The_Call_Through_To_Listeners()
        {
            _Manager.Initialise();
            _Manager.Disconnect();

            _CreatedListeners[0].Verify(r => r.Disconnect(), Times.Once());
            _CreatedListeners[1].Verify(r => r.Disconnect(), Times.Once());
        }
        #endregion
    }
}
