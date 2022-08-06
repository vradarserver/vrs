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
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class MergedFeedFeedTests
    {
        #region Private class - SettingsProperty, ConnectionProperty
        /// <summary>
        /// A class that describes a configuration setting that has some significance to the receiver pathway.
        /// </summary>
        class SettingsProperty
        {
            public string Name;                             // The name of the property
            public Action<Configuration> ChangeProperty;    // A delegate that changes the property to a non-default value

            public SettingsProperty()
            {
            }

            public SettingsProperty(string name, Action<Configuration> changeProperty) : this()
            {
                Name = name;
                ChangeProperty = changeProperty;
            }
        }

        /// <summary>
        /// A class that describes a configuration setting carrying a connection property.
        /// </summary>
        class ConnectionProperty : SettingsProperty
        {
            public List<ConnectionType> ConnectionTypes;    // The connection types that depend upon the property

            public ConnectionProperty() : this(default(ConnectionType), null, null)
            {
            }

            public ConnectionProperty(ConnectionType connectionType, string name, Action<Configuration> changeProperty) : base(name, changeProperty)
            {
                ConnectionTypes = new List<ConnectionType>();
                ConnectionTypes.Add(connectionType);
            }

            public bool MatchesConnectionType(ConnectionType connectionType)
            {
                return ConnectionTypes.Contains(connectionType);
            }
        }
        #endregion

        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;
        private IMergedFeedFeed _Feed;
        private Mock<IBaseStationAircraftList> _AircraftList;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Mock<IBaseStationDatabase> _BaseStationDatabase;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigBaseStationDatabase;
        private Configuration _Configuration;
        private Receiver _Receiver;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtRecorder;

        private Mock<IStatistics> _Statistics;

        private MergedFeed _MergedFeed;
        private Mock<IMergedFeedListener> _MergedFeedListener;
        private List<Mock<INetworkFeed>> _Feeds;
        private List<Mock<IListener>> _Listeners;
        private Mock<IFeedManager> _FeedManager;
        private List<INetworkFeed> _MergedFeedReceivers;
        private List<IMergedFeedComponentListener> _SetMergedFeedListeners;
        private Mock<IPolarPlotter> _PolarPlotter;

        private readonly List<ReceiverLocation> _ReceiverLocations = new List<ReceiverLocation>() {
            new ReceiverLocation() { UniqueId = 1, Name = "First", Latitude = 1.1, Longitude = 2.2 },
            new ReceiverLocation() { UniqueId = 2, Name = "Second", Latitude = 3.3, Longitude = 4.4 },
        };

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _Feed = Factory.Resolve<IMergedFeedFeed>();
            _Receiver = new Receiver() { UniqueId = 1, Name = "A", ReceiverLocationId = 1, IsSatcomFeed = true, };

            _Configuration = new Configuration();
            _Configuration.RawDecodingSettings.AssumeDF18CF1IsIcao = true;
            _Configuration.Receivers.Clear();
            _Configuration.Receivers.Add(_Receiver);
            _Configuration.ReceiverLocations.Clear();
            _Configuration.ReceiverLocations.AddRange(_ReceiverLocations);

            _AircraftList = TestUtilities.CreateMockImplementation<IBaseStationAircraftList>();
            _AircraftList.Object.PolarPlotter = null;
            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _BaseStationDatabase = TestUtilities.CreateMockInstance<IBaseStationDatabase>();
            _AutoConfigBaseStationDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _AutoConfigBaseStationDatabase.Setup(r => r.Database).Returns(_BaseStationDatabase.Object);
            _Statistics = StatisticsHelper.CreateLockableStatistics();
            _PolarPlotter = TestUtilities.CreateMockImplementation<IPolarPlotter>();
            _PolarPlotter.Setup(r => r.Initialise(It.IsAny<double>(), It.IsAny<double>()))
                         .Callback((double lat, double lng) => {
                _PolarPlotter.Setup(r => r.Latitude).Returns(lat);
                _PolarPlotter.Setup(r => r.Longitude).Returns(lng);
                _PolarPlotter.Setup(r => r.RoundToDegrees).Returns(1);
            });

            _SetMergedFeedListeners = new List<IMergedFeedComponentListener>();
            _MergedFeedListener = TestUtilities.CreateMockImplementation<IMergedFeedListener>();
            _MergedFeedListener.Setup(r => r.SetListeners(It.IsAny<IEnumerable<IMergedFeedComponentListener>>())).Callback((IEnumerable<IMergedFeedComponentListener> listeners) => {
                _SetMergedFeedListeners.Clear();
                _SetMergedFeedListeners.AddRange(listeners);
            });

            _ExceptionCaughtRecorder = new EventRecorder<EventArgs<Exception>>();

            _Feeds = new List<Mock<INetworkFeed>>();
            _Listeners = new List<Mock<IListener>>();
            var useVisibleFeeds = false;
            _FeedManager = FeedHelper.CreateMockFeedManager(_Feeds, _Listeners, useVisibleFeeds, 1, 2);
            _MergedFeedReceivers = FeedHelper.GetFeeds(_Feeds);
            _MergedFeed = new MergedFeed() { UniqueId = 3, Name = "M1", ReceiverIds = { 1, 2 } };
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
            _Feed.Dispose();
        }
        #endregion

        #region Constructors and Properties
        [TestMethod]
        public void Constructor_Initialises_To_Known_Value_And_Properties_Work()
        {
            _Feed.Dispose();
            _Feed = Factory.Resolve<IMergedFeedFeed>();

            Assert.IsNull(_Feed.AircraftList);
            Assert.IsFalse(_Feed.IsVisible);
            Assert.IsNull(_Feed.Listener);
            Assert.IsNull(_Feed.Name);
            Assert.AreEqual(0, _Feed.UniqueId);
        }

        [TestMethod]
        public void ConnectionStatus_Passes_Through_To_Listener()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _MergedFeedListener.SetupGet(r => r.ConnectionStatus).Returns(ConnectionStatus.Connected);
            Assert.AreEqual(ConnectionStatus.Connected, _Feed.ConnectionStatus);
        }
        #endregion

        #region Initialise - MergedFeed
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialise_Throws_If_Passed_Null_MergedFeed()
        {
            _Feed.Initialise((MergedFeed)null, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialise_Throws_If_Passed_Null_ReceiverPathways_List()
        {
            _Feed.Initialise(_MergedFeed, null);
        }

        [TestMethod]
        public void Initialise_Does_Not_Care_If_MergedFeed_Is_Not_In_Configuration()
        {
            var otherMergedFeed = new MergedFeed() { UniqueId = _MergedFeed.UniqueId + 1, Name = "Other mergedFeed", ReceiverIds = { 1, 2 } };
            _Feed.Initialise(otherMergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Initialise_Throws_If_Passed_Disabled_MergedFeed()
        {
            _MergedFeed.Enabled = false;
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Initialise_Throws_If_Called_Twice_With_MergedFeeds()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        public void Initialise_Does_Not_Throw_If_ReceiverId_Is_Not_In_ReceiverPathways()
        {
            _MergedFeed.ReceiverIds.Clear();
            _MergedFeed.ReceiverIds.Add(100);
            _MergedFeed.ReceiverIds.Add(101);
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        public void Initialise_Copies_MergedFeed_Details_To_Properties()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(3, _Feed.UniqueId);
            Assert.AreEqual("M1", _Feed.Name);
        }

        [TestMethod]
        public void Initialise_Creates_MergedFeed_Listener_And_AircraftList()
        {
            _MergedFeed.IcaoTimeout = 1234;
            _MergedFeed.UniqueId = 9988;
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            Assert.IsNotNull(_Feed.Listener);
            Assert.AreSame(_MergedFeedListener.Object, _Feed.Listener);
            Assert.IsNotNull(_Feed.AircraftList);
            Assert.IsTrue(_Feed.Listener.IgnoreBadMessages);
            Assert.AreEqual(1234, _MergedFeedListener.Object.IcaoTimeout);
            Assert.AreEqual(9988, _MergedFeedListener.Object.ReceiverId);
            Assert.AreEqual("M1", _MergedFeedListener.Object.ReceiverName);
            Assert.AreEqual(false, _MergedFeedListener.Object.IsSatcomFeed);
            Assert.AreEqual(false, _MergedFeedListener.Object.AssumeDF18CF1IsIcao);
        }

        [TestMethod]
        public void Initialise_Sets_IgnoreAircraftWithNoPosition_On_MergedFeed()
        {
            foreach(var ignoreFlag in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _MergedFeed.IgnoreAircraftWithNoPosition = ignoreFlag;
                _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

                Assert.AreEqual(ignoreFlag, _MergedFeedListener.Object.IgnoreAircraftWithNoPosition);
            }
        }

        [TestMethod]
        public void Initialise_Does_Not_Call_Change_Source_For_Merged_Feeds()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            _MergedFeedListener.Verify(r => r.ChangeSource(It.IsAny<IConnector>(), It.IsAny<IMessageBytesExtractor>(), It.IsAny<IRawMessageTranslator>()), Times.Never());
        }

        [TestMethod]
        public void Initialise_Calls_SetListeners()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(2, _SetMergedFeedListeners.Count);
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[0].Object)));
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[1].Object)));
        }

        [TestMethod]
        public void Initialise_Only_Calls_SetListeners_With_Listeners_For_The_Right_IDs()
        {
            FeedHelper.AddFeeds(_Feeds, _Listeners, 3, 4);
            _MergedFeedReceivers = FeedHelper.GetFeeds(_Feeds);

            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(2, _SetMergedFeedListeners.Count);
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[0].Object)));
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[1].Object)));
        }

        [TestMethod]
        public void Initialise_Calls_SetListeners_With_The_Correct_Feed_Types()
        {
        //MLATME
            _MergedFeed.ReceiverFlags.Add(new MergedFeedReceiver() {
                UniqueId = _Listeners[0].Object.ReceiverId,
                IsMlatFeed = false,
            });
            _MergedFeed.ReceiverFlags.Add(new MergedFeedReceiver() {
                UniqueId = _Listeners[1].Object.ReceiverId,
                IsMlatFeed = true,
            });
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            var component0 = _SetMergedFeedListeners.Single(r => Object.ReferenceEquals(r.Listener, _Listeners[0].Object));
            var component1 = _SetMergedFeedListeners.Single(r => Object.ReferenceEquals(r.Listener, _Listeners[1].Object));

            Assert.AreEqual(false, component0.IsMlatFeed);
            Assert.AreEqual(true, component1.IsMlatFeed);
        }

        [TestMethod]
        public void Initialise_Calls_SetListeners_With_No_Feed_Type_When_No_Flag_Is_Set()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            var component0 = _SetMergedFeedListeners.Single(r => Object.ReferenceEquals(r.Listener, _Listeners[0].Object));
            var component1 = _SetMergedFeedListeners.Single(r => Object.ReferenceEquals(r.Listener, _Listeners[1].Object));

            Assert.AreEqual(false, component0.IsMlatFeed);
            Assert.AreEqual(false, component1.IsMlatFeed);
        }

        [TestMethod]
        public void Initialise_Attaches_Merged_Feed_Listener_To_AircraftList()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            var baseStationAircraftList = (IBaseStationAircraftList)_Feed.AircraftList;
            Assert.AreSame(_MergedFeedListener.Object, baseStationAircraftList.Listener);
        }

        [TestMethod]
        public void Initialise_Attaches_StandingDataManager_To_MergedFeed_AircraftList()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            var baseStationAircraftList = (IBaseStationAircraftList)_Feed.AircraftList;
            Assert.AreSame(_StandingDataManager.Object, baseStationAircraftList.StandingDataManager);
        }

        [TestMethod]
        public void Initialise_Starts_MergedFeed_AircraftList()
        {
            _AircraftList.Setup(r => r.Start()).Callback(() => {
                Assert.IsNotNull(_AircraftList.Object.Listener);
                Assert.IsNotNull(_AircraftList.Object.StandingDataManager);
            });

            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            _AircraftList.Verify(r => r.Start(), Times.Once());
        }

        [TestMethod]
        public void Initialise_Sets_IsVisible_For_MergedFeeds()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            Assert.IsTrue(_Feed.IsVisible);
        }

        [TestMethod]
        public void Initialise_Hooks_MergedFeed_Listener_ExceptionCaught()
        {
            _Feed.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _MergedFeedListener.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Feed, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void Initialise_Hooks_MergedFeed_AircraftList_ExceptionCaught()
        {
            _Feed.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _AircraftList.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Feed, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void Initialise_Sets_MergedFeed_IsVisible_From_ReceiverUsage()
        {
            foreach(ReceiverUsage receiverUsage in Enum.GetValues(typeof(ReceiverUsage))) {
                TestCleanup();
                TestInitialise();

                _MergedFeed.ReceiverUsage = receiverUsage;
                _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

                var isVisible = receiverUsage == ReceiverUsage.Normal;
                Assert.AreEqual(isVisible, _Feed.IsVisible, receiverUsage.ToString());
            }
        }
        #endregion

        #region ApplyConfiguration - MergedFeed
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ApplyConfiguration_Throws_If_Called_Before_Initialise_For_MergedFeed()
        {
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApplyConfiguration_Throws_If_Passed_Null_MergedFeed()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.ApplyConfiguration((MergedFeed)null, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApplyConfiguration_Throws_If_Passed_Null_ReceiverPathways()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.ApplyConfiguration(_MergedFeed, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ApplyConfiguration_Throws_If_Passed_MergedFeed_With_Different_Unique_ID()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            ++_MergedFeed.UniqueId;
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        public void ApplyConfiguration_Picks_Up_Name_Change_For_MergedFeed()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _MergedFeed.Name = "My New Name";
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual("My New Name", _Feed.Name);
        }

        [TestMethod]
        public void ApplyConfiguration_Calls_SetListeners()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _SetMergedFeedListeners.Clear();
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(2, _SetMergedFeedListeners.Count);
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[0].Object)));
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[1].Object)));
        }

        [TestMethod]
        public void ApplyConfiguration_Sets_IcaoTimeout()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _SetMergedFeedListeners.Clear();

            _MergedFeed.IcaoTimeout = 9876;
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(9876, _MergedFeedListener.Object.IcaoTimeout);
        }

        [TestMethod]
        public void ApplyConfiguration_Sets_IgnoreAircraftWithNoPosition()
        {
            foreach(var ignoreFlag in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _MergedFeed.IgnoreAircraftWithNoPosition = !ignoreFlag;
                _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

                _MergedFeed.IgnoreAircraftWithNoPosition = ignoreFlag;
                _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);

                Assert.AreEqual(ignoreFlag, _MergedFeedListener.Object.IgnoreAircraftWithNoPosition);
            }
        }

        [TestMethod]
        public void ApplyConfiguration_Sets_IsVisible_On_MergedFeeds_As_Appropriate()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            foreach(ReceiverUsage receiverUsage in Enum.GetValues(typeof(ReceiverUsage))) {
                _MergedFeed.ReceiverUsage = receiverUsage;

                _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);

                var expected = receiverUsage == ReceiverUsage.Normal;
                Assert.AreEqual(expected, _Feed.IsVisible, receiverUsage.ToString());
            }
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void Dispose_Disposes_Of_AircraftList_First()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _AircraftList.Setup(r => r.Dispose()).Callback(() => {
                _MergedFeedListener.Verify(r => r.Dispose(), Times.Never());
            });

            _Feed.Dispose();
            _AircraftList.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Dispose_Disposes_Of_Listener()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.Dispose();
            _MergedFeedListener.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Dispose_Does_Not_Dispose_Of_BaseStationDatabase()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.Dispose();
            _BaseStationDatabase.Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void Dispose_Unhooks_Listener_ExceptionCaught()
        {
            _Feed.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.Dispose();
            _MergedFeedListener.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
        }

        [TestMethod]
        public void Dispose_Unhooks_AircraftList_ExceptionCaught()
        {
            _Feed.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.Dispose();
            _AircraftList.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
        }

        [TestMethod]
        public void Dispose_Can_Be_Called_Before_Initialise()
        {
            _Feed.Dispose();
        }

        [TestMethod]
        public void Dispose_Resets_Listener_And_AircraftList_Properties()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            _Feed.Dispose();

            Assert.IsNull(_Feed.AircraftList);
            Assert.IsNull(_Feed.Listener);
        }

        [TestMethod]
        public void Dispose_Can_Be_Called_Twice()
        {
            _Feed.Dispose();
            _Feed.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Dispose_Does_Not_Reset_Initialise_DoubleCall_Guard()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.Dispose();
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
        }
        #endregion

        #region ConnectionStateChanged
        [TestMethod]
        public void ConnectionStateChanged_Passes_Through_To_Listener()
        {
            var eventRecorder = new EventRecorder<EventArgs>();
            _Feed.ConnectionStateChanged += eventRecorder.Handler;

            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _MergedFeedListener.Raise(r => r.ConnectionStateChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreSame(_Feed, eventRecorder.Sender);
            Assert.IsNotNull(eventRecorder.Args);
        }
        #endregion

        #region Connect and Disconnect
        [TestMethod]
        public void Connect_Passes_Through_To_Listener()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.Connect();
            _MergedFeedListener.Verify(r => r.Connect(), Times.Once());
        }

        [TestMethod]
        public void Disconnect_Passes_Through_To_Listener()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.Disconnect();
            _MergedFeedListener.Verify(r => r.Disconnect(), Times.Once());
        }
        #endregion
    }
}
