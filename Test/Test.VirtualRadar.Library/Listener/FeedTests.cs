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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.BaseStation;
using Test.Framework;
using Moq;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using System.IO.Ports;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Network;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class FeedTests
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
        private IFeed _Feed;
        private Mock<IListener> _Listener;
        private Mock<IBaseStationAircraftList> _AircraftList;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Mock<IBaseStationDatabase> _BaseStationDatabase;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigBaseStationDatabase;
        private Configuration _Configuration;
        private Receiver _Receiver;
        private RawDecodingSettings _RawDecodingSettings;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtRecorder;

        private Mock<INetworkConnector> _IPActiveConnector;
        private Mock<ISerialConnector> _SerialActiveConnector;
        private Mock<IPassphraseAuthentication> _PassphraseAuthentication;
        private Mock<IPort30003MessageBytesExtractor> _Port30003Extractor;
        private Mock<ISbs3MessageBytesExtractor> _Sbs3MessageBytesExtractor;
        private Mock<IBeastMessageBytesExtractor> _BeastMessageBytesExtractor;
        private Mock<ICompressedMessageBytesExtractor> _CompressedMessageBytesExtractor;
        private Mock<IAircraftListJsonMessageBytesExtractor> _AircraftListJsonMessageBytesExtractor;
        private Mock<IPlaneFinderMessageBytesExtractor> _PlaneFinderMessageBytesExtractor;
        private Mock<IRawMessageTranslator> _RawMessageTranslator;
        private Mock<ISavedPolarPlotStorage> _SavedPolarPlotStorage;
        private SavedPolarPlot _SavedPolarPlot;

        private Mock<IStatistics> _Statistics;

        private MergedFeed _MergedFeed;
        private Mock<IMergedFeedListener> _MergedFeedListener;
        private List<Mock<IFeed>> _Feeds;
        private List<Mock<IListener>> _Listeners;
        private Mock<IFeedManager> _FeedManager;
        private List<IFeed> _MergedFeedReceivers;
        private List<IMergedFeedComponentListener> _SetMergedFeedListeners;
        private Mock<IPolarPlotter> _PolarPlotter;

        private readonly List<ConnectionProperty> _ConnectionProperties = new List<ConnectionProperty>() {
            new ConnectionProperty(ConnectionType.TCP, "Address",                   c => c.Receivers[0].Address = "9.8.7.6"),
            new ConnectionProperty(ConnectionType.TCP, "Port",                      c => c.Receivers[0].Port = 77),
            new ConnectionProperty(ConnectionType.TCP, "UseKeepAlive",              c => c.Receivers[0].UseKeepAlive = false),
            new ConnectionProperty(ConnectionType.TCP, "IdleTimeoutMilliseconds",   c => c.Receivers[0].IdleTimeoutMilliseconds = 20000),

            new ConnectionProperty(ConnectionType.COM, "ComPort",                   c => c.Receivers[0].ComPort = "COM99"),
            new ConnectionProperty(ConnectionType.COM, "BaudRate",                  c => c.Receivers[0].BaudRate = 9600),
            new ConnectionProperty(ConnectionType.COM, "DataBits",                  c => c.Receivers[0].DataBits = 7),
            new ConnectionProperty(ConnectionType.COM, "StopBits",                  c => c.Receivers[0].StopBits = StopBits.OnePointFive),
            new ConnectionProperty(ConnectionType.COM, "Parity",                    c => c.Receivers[0].Parity = Parity.Odd),
            new ConnectionProperty(ConnectionType.COM, "Handshake",                 c => c.Receivers[0].Handshake = Handshake.XOnXOff),
            new ConnectionProperty(ConnectionType.COM, "StartupText",               c => c.Receivers[0].StartupText = "UP"),
            new ConnectionProperty(ConnectionType.COM, "ShutdownText",              c => c.Receivers[0].ShutdownText = "DOWN"),
        };

        private readonly List<SettingsProperty> _RawMessageTranslatorProperties = new List<SettingsProperty>() {
            new SettingsProperty("AirborneGlobalPositionLimit",         s => s.RawDecodingSettings.AirborneGlobalPositionLimit = 999),
            new SettingsProperty("FastSurfaceGlobalPositionLimit",      s => s.RawDecodingSettings.FastSurfaceGlobalPositionLimit = 998),
            new SettingsProperty("SlowSurfaceGlobalPositionLimit",      s => s.RawDecodingSettings.SlowSurfaceGlobalPositionLimit = 997),
            new SettingsProperty("AcceptableAirborneSpeed",             s => s.RawDecodingSettings.AcceptableAirborneSpeed = 996),
            new SettingsProperty("AcceptableSurfaceSpeed",              s => s.RawDecodingSettings.AcceptableSurfaceSpeed = 995),
            new SettingsProperty("AcceptableAirSurfaceTransitionSpeed", s => s.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed = 994),
            new SettingsProperty("ReceiverRange",                       s => s.RawDecodingSettings.ReceiverRange = 993),
            new SettingsProperty("IgnoreMilitaryExtendedSquitter",      s => s.RawDecodingSettings.IgnoreMilitaryExtendedSquitter = true),
            new SettingsProperty("ReceiverLocation",                    s => s.Receivers[0].ReceiverLocationId = 2),
            new SettingsProperty("TrackingTimeoutSeconds",              s => s.BaseStationSettings.TrackingTimeoutSeconds = 100),
        };

        private readonly List<ReceiverLocation> _ReceiverLocations = new List<ReceiverLocation>() {
            new ReceiverLocation() { UniqueId = 1, Name = "First", Latitude = 1.1, Longitude = 2.2 },
            new ReceiverLocation() { UniqueId = 2, Name = "Second", Latitude = 3.3, Longitude = 4.4 },
        };

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _Feed = Factory.Resolve<IFeed>();
            _Receiver = new Receiver() { UniqueId = 1, Name = "A", ReceiverLocationId = 1, IsSatcomFeed = true, };
            _RawDecodingSettings = new RawDecodingSettings();

            _Configuration = new Configuration();
            _Configuration.Receivers.Clear();
            _Configuration.Receivers.Add(_Receiver);
            _Configuration.ReceiverLocations.Clear();
            _Configuration.ReceiverLocations.AddRange(_ReceiverLocations);

            _SavedPolarPlot = new SavedPolarPlot();
            _SavedPolarPlotStorage = TestUtilities.CreateMockSingleton<ISavedPolarPlotStorage>();
            _SavedPolarPlotStorage.Setup(r => r.Load(It.IsAny<IFeed>())).Returns(_SavedPolarPlot);

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

            _Listener = TestUtilities.CreateMockImplementation<IListener>();
            _Listener.Setup(r => r.Connector).Returns((IConnector)null);
            _Listener.Setup(r => r.BytesExtractor).Returns((IMessageBytesExtractor)null);
            _Listener.Setup(r => r.RawMessageTranslator).Returns((IRawMessageTranslator)null);
            _Listener.Setup(r => r.Statistics).Returns(_Statistics.Object);
            _Listener.Setup(r => r.ChangeSource(It.IsAny<IConnector>(), It.IsAny<IMessageBytesExtractor>(), It.IsAny<IRawMessageTranslator>()))
                     .Callback((IConnector connector, IMessageBytesExtractor extractor, IRawMessageTranslator translator) => {
                _Listener.Setup(r => r.Connector).Returns(connector);
                _Listener.Setup(r => r.BytesExtractor).Returns(extractor);
                _Listener.Setup(r => r.RawMessageTranslator).Returns(translator);
            });

            _SetMergedFeedListeners = new List<IMergedFeedComponentListener>();
            _MergedFeedListener = TestUtilities.CreateMockImplementation<IMergedFeedListener>();
            _MergedFeedListener.Setup(r => r.SetListeners(It.IsAny<IEnumerable<IMergedFeedComponentListener>>())).Callback((IEnumerable<IMergedFeedComponentListener> listeners) => {
                _SetMergedFeedListeners.Clear();
                _SetMergedFeedListeners.AddRange(listeners);
            });

            _ExceptionCaughtRecorder = new EventRecorder<EventArgs<Exception>>();

            _Feeds = new List<Mock<IFeed>>();
            _Listeners = new List<Mock<IListener>>();
            var useVisibleFeeds = false;
            _FeedManager = FeedHelper.CreateMockFeedManager(_Feeds, _Listeners, useVisibleFeeds, 1, 2);
            _MergedFeedReceivers = FeedHelper.GetFeeds(_Feeds);
            _MergedFeed = new MergedFeed() { UniqueId = 3, Name = "M1", ReceiverIds = { 1, 2 } };

            CreateNewListenerChildObjectInstances();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
            _Feed.Dispose();
        }
        #endregion

        #region Utility methods
        private void DoForAllSourcesAndConnectionTypes(Action<string, ConnectionType, string> action)
        {
            foreach(var dataSource in DataSource.AllInternalDataSources) {
                foreach(ConnectionType connectionType in Enum.GetValues(typeof(ConnectionType))) {
                    TestCleanup();
                    TestInitialise();

                    _Receiver.DataSource = dataSource;
                    _Receiver.ConnectionType = connectionType;

                    action(dataSource, connectionType, String.Format("DataSource {0} ConnectionType {1}", dataSource, connectionType));
                }
            }
        }

        private void CreateNewListenerChildObjectInstances()
        {
            // The TestInitialise method sets up the different listener providers, byte extractors and raw message translators
            // so that every time the code asks for one they'll get the same instance back. This is fine as far as it goes but
            // it makes it hard to test that new instances are created when appropriate. This method creates a new set of objects
            // and registers them as the default object.
            _IPActiveConnector = TestUtilities.CreateMockImplementation<INetworkConnector>();
            _SerialActiveConnector = TestUtilities.CreateMockImplementation<ISerialConnector>();
            _IPActiveConnector.Object.Authentication = null;
            _SerialActiveConnector.Object.Authentication = null;

            _Port30003Extractor = TestUtilities.CreateMockImplementation<IPort30003MessageBytesExtractor>();
            _Sbs3MessageBytesExtractor = TestUtilities.CreateMockImplementation<ISbs3MessageBytesExtractor>();
            _BeastMessageBytesExtractor = TestUtilities.CreateMockImplementation<IBeastMessageBytesExtractor>();
            _CompressedMessageBytesExtractor = TestUtilities.CreateMockImplementation<ICompressedMessageBytesExtractor>();
            _AircraftListJsonMessageBytesExtractor = TestUtilities.CreateMockImplementation<IAircraftListJsonMessageBytesExtractor>();
            _PlaneFinderMessageBytesExtractor = TestUtilities.CreateMockImplementation<IPlaneFinderMessageBytesExtractor>();

            _RawMessageTranslator = TestUtilities.CreateMockImplementation<IRawMessageTranslator>();
            _RawMessageTranslator.Object.ReceiverLocation = null;

            _PassphraseAuthentication = TestUtilities.CreateMockImplementation<IPassphraseAuthentication>();
        }
        #endregion

        #region Constructors and Properties
        [TestMethod]
        public void Feed_Constructor_Initialises_To_Known_Value_And_Properties_Work()
        {
            _Feed.Dispose();
            _Feed = Factory.Resolve<IFeed>();

            Assert.IsNull(_Feed.AircraftList);
            Assert.IsFalse(_Feed.IsVisible);
            Assert.IsNull(_Feed.Listener);
            Assert.IsNull(_Feed.Name);
            Assert.AreEqual(0, _Feed.UniqueId);
        }
        #endregion

        #region Initialise - Receiver
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Feed_Initialise_Throws_If_Passed_Null_Receiver()
        {
            _Feed.Initialise((Receiver)null, _Configuration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Feed_Initialise_Throws_If_Passed_Null_Configuration()
        {
            _Feed.Initialise(_Receiver, null);
        }

        [TestMethod]
        public void Feed_Initialise_Does_Not_Care_If_Receiver_Is_Not_In_Configuration()
        {
            var otherReceiver = new Receiver() { UniqueId = _Receiver.UniqueId + 1, Name = "Other receiver", Port = _Receiver.Port + 1 };
            _Feed.Initialise(otherReceiver, _Configuration);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_Initialise_Throws_If_Passed_Disabled_Receiver()
        {
            _Receiver.Enabled = false;
            _Feed.Initialise(_Receiver, _Configuration);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_Initialise_Throws_If_Called_Twice()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.Initialise(_Receiver, _Configuration);
        }

        [TestMethod]
        public void Feed_Initialise_Does_Not_Throw_If_ReceiverLocationId_Is_Not_In_ReceiverLocations()
        {
            _Receiver.ReceiverLocationId = _ReceiverLocations.Max(r => r.UniqueId) + 1;
            _Feed.Initialise(_Receiver, _Configuration);
        }

        [TestMethod]
        public void Feed_Initialise_Copies_Receiver_Details_To_Properties()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            Assert.AreEqual(1, _Feed.UniqueId);
            Assert.AreEqual("A", _Feed.Name);
        }

        [TestMethod]
        public void Feed_Initialise_Creates_Listener_And_AircraftList()
        {
            _Receiver.UniqueId = 1234;
            _Feed.Initialise(_Receiver, _Configuration);

            Assert.IsNotNull(_Feed.Listener);
            Assert.IsNull(_Feed.Listener as IMergedFeedListener);
            Assert.IsNotNull(_Feed.AircraftList);
            Assert.AreEqual(1234, _Feed.Listener.ReceiverId);
            Assert.IsTrue(_Feed.Listener.IgnoreBadMessages);
            Assert.AreEqual("A", _Feed.Listener.ReceiverName);
            Assert.AreEqual(true, _Feed.Listener.IsSatcomFeed);
        }

        [TestMethod]
        public void Feed_Initialise_Creates_PolarPlotter_If_ReceiverLocation_Is_Present()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            Assert.AreSame(_PolarPlotter.Object, _Feed.AircraftList.PolarPlotter);
            _PolarPlotter.Verify(r => r.Initialise(1.1, 2.2), Times.Once());
        }

        [TestMethod]
        public void Feed_Initialise_Does_Not_Create_PolarPlotter_If_ReceiverLocation_Missing()
        {
            _Configuration.ReceiverLocations.Clear();

            _Feed.Initialise(_Receiver, _Configuration);

            Assert.AreNotSame(_PolarPlotter.Object, _Feed.AircraftList.PolarPlotter);
            _PolarPlotter.Verify(r => r.Initialise(It.IsAny<double>(), It.IsAny<double>()), Times.Never());
            _PolarPlotter.Verify(r => r.Initialise(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void Feed_Initialise_Calls_Listener_ChangeSource_With_Correct_Parameters_For_DataSource_And_ConnectionType()
        {
            DoForAllSourcesAndConnectionTypes((dataSource, connectionType, failMessage) => {
                _Feed.Initialise(_Receiver, _Configuration);

                if(dataSource == DataSource.Beast) {
                     Assert.AreSame(_BeastMessageBytesExtractor.Object, _Listener.Object.BytesExtractor);
                } else if(dataSource == DataSource.Port30003) {
                    Assert.AreSame(_Port30003Extractor.Object, _Listener.Object.BytesExtractor);
                } else if(dataSource == DataSource.Sbs3) {
                    Assert.AreSame(_Sbs3MessageBytesExtractor.Object, _Listener.Object.BytesExtractor);
                } else if(dataSource == DataSource.CompressedVRS) {
                    Assert.AreSame(_CompressedMessageBytesExtractor.Object, _Listener.Object.BytesExtractor);
                } else if(dataSource == DataSource.AircraftListJson) {
                    Assert.AreSame(_AircraftListJsonMessageBytesExtractor.Object, _Listener.Object.BytesExtractor);
                } else if(dataSource == DataSource.PlaneFinder) {
                    Assert.AreSame(_PlaneFinderMessageBytesExtractor.Object, _Listener.Object.BytesExtractor);
                } else {
                    throw new NotImplementedException();
                }

                switch(connectionType) {
                    case ConnectionType.COM:        Assert.AreSame(_SerialActiveConnector.Object, _Listener.Object.Connector); break;
                    case ConnectionType.TCP:        Assert.AreSame(_IPActiveConnector.Object, _Listener.Object.Connector); break;
                    default:                        throw new NotImplementedException();
                }

                Assert.AreSame(_RawMessageTranslator.Object, _Listener.Object.RawMessageTranslator);
            });
        }

        [TestMethod]
        public void Feed_Initialise_Applies_Configuration_Settings_For_Connection_Type()
        {
            Do_Check_Configuration_Changes_Are_Applied(false, () => { _Feed.Initialise(_Receiver, _Configuration); });
        }

        private void Do_Check_Configuration_Changes_Are_Applied(bool initialiseFirst, Action action)
        {
            foreach(ConnectionType connectionType in Enum.GetValues(typeof(ConnectionType))) {
                TestCleanup();
                TestInitialise();

                if(initialiseFirst) _Feed.Initialise(_Receiver, _Configuration);

                _Receiver.ConnectionType = connectionType;

                _Receiver.Address = "TCP Address";
                _Receiver.Port = 12345;
                _Receiver.UseKeepAlive = true;
                _Receiver.IdleTimeoutMilliseconds = 30000;

                _Receiver.ComPort = "Serial COM Port";
                _Receiver.BaudRate = 10;
                _Receiver.DataBits = 9;
                _Receiver.StopBits = StopBits.Two;
                _Receiver.Parity = Parity.Mark;
                _Receiver.Handshake = Handshake.XOnXOff;
                _Receiver.StartupText = "Up";
                _Receiver.ShutdownText = "Down";

                action();

                Assert.AreEqual(true, _Listener.Object.IgnoreBadMessages);

                switch(connectionType) {
                    case ConnectionType.COM:
                        Assert.AreEqual("Serial COM Port", _SerialActiveConnector.Object.ComPort);
                        Assert.AreEqual(10, _SerialActiveConnector.Object.BaudRate);
                        Assert.AreEqual(9, _SerialActiveConnector.Object.DataBits);
                        Assert.AreEqual(StopBits.Two, _SerialActiveConnector.Object.StopBits);
                        Assert.AreEqual(Parity.Mark, _SerialActiveConnector.Object.Parity);
                        Assert.AreEqual(Handshake.XOnXOff, _SerialActiveConnector.Object.Handshake);
                        Assert.AreEqual("Up", _SerialActiveConnector.Object.StartupText);
                        Assert.AreEqual("Down", _SerialActiveConnector.Object.ShutdownText);
                        break;
                    case ConnectionType.TCP:
                        Assert.AreEqual("TCP Address", _IPActiveConnector.Object.Address);
                        Assert.AreEqual(12345, _IPActiveConnector.Object.Port);
                        Assert.AreEqual(true, _IPActiveConnector.Object.UseKeepAlive);
                        Assert.AreEqual(30000, _IPActiveConnector.Object.IdleTimeout);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void Feed_Initialise_Applies_Correct_Network_Settings_To_Connector()
        {
            Do_Check_Correct_Network_Settings_Applied_To_Connector(false, () => { _Feed.Initialise(_Receiver, _Configuration); });
        }

        private void Do_Check_Correct_Network_Settings_Applied_To_Connector(bool initialiseFirst, Action action)
        {
            foreach(var isPassive in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                if(initialiseFirst) _Feed.Initialise(_Receiver, _Configuration);

                _Receiver.ConnectionType = ConnectionType.TCP;
                _Receiver.IsPassive = isPassive;
                _Receiver.Address = "Address";
                _Receiver.Port = 12345;
                _Receiver.UseKeepAlive = false;
                _Receiver.IdleTimeoutMilliseconds = 10000;

                CreateNewListenerChildObjectInstances();
                action();

                Assert.AreEqual(12345, _IPActiveConnector.Object.Port);
                Assert.AreEqual(false, _IPActiveConnector.Object.UseKeepAlive);
                Assert.AreEqual(10000, _IPActiveConnector.Object.IdleTimeout);

                if(isPassive) {
                    Assert.AreEqual(true, _IPActiveConnector.Object.IsPassive);
                    _IPActiveConnector.VerifySet(r => r.Address = It.IsAny<string>(), Times.Never());
                    Assert.AreSame(_Receiver.Access, _IPActiveConnector.Object.Access);
                } else {
                    Assert.AreEqual(false, _IPActiveConnector.Object.IsPassive);
                    Assert.AreEqual("Address", _IPActiveConnector.Object.Address);
                    _IPActiveConnector.VerifySet(r => r.Access = It.IsAny<Access>(), Times.Never());
                }
            }
        }

        [TestMethod]
        public void Feed_Initialise_Copies_Configuration_To_RawTranslator()
        {
            Do_Check_Configuration_Changes_Copied_To_RawTranslator(false, () => _Feed.Initialise(_Receiver, _Configuration));
        }

        private void Do_Check_Configuration_Changes_Copied_To_RawTranslator(bool initialiseFirst, Action triggerAction)
        {
            if(initialiseFirst) _Feed.Initialise(_Receiver, _Configuration);

            _Receiver.DataSource = DataSource.Sbs3;
            _Receiver.ReceiverLocationId = 99;
            _Configuration.ReceiverLocations.Add(new ReceiverLocation() { UniqueId = 99, Latitude = 1.2345, Longitude = -17.89 });

            _Configuration.RawDecodingSettings.AcceptableAirborneSpeed = 999.123;
            _Configuration.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed = 888.456;
            _Configuration.RawDecodingSettings.AcceptableSurfaceSpeed = 777.789;
            _Configuration.RawDecodingSettings.AirborneGlobalPositionLimit = 99;
            _Configuration.RawDecodingSettings.FastSurfaceGlobalPositionLimit = 88;
            _Configuration.RawDecodingSettings.SlowSurfaceGlobalPositionLimit = 77;
            _Configuration.RawDecodingSettings.IgnoreCallsignsInBds20 = true;
            _Configuration.RawDecodingSettings.IgnoreMilitaryExtendedSquitter = true;
            _Configuration.RawDecodingSettings.ReceiverLocationId = 12;
            _Configuration.RawDecodingSettings.ReceiverRange = 1234;
            _Configuration.RawDecodingSettings.SuppressReceiverRangeCheck = true;
            _Configuration.RawDecodingSettings.SuppressTisbDecoding = true;
            _Configuration.RawDecodingSettings.UseLocalDecodeForInitialPosition = true;
            _Configuration.BaseStationSettings.TrackingTimeoutSeconds = 100;
            _Configuration.RawDecodingSettings.AcceptIcaoInNonPICount = 8;
            _Configuration.RawDecodingSettings.AcceptIcaoInNonPISeconds = 16;
            _Configuration.RawDecodingSettings.AcceptIcaoInPI0Count = 24;
            _Configuration.RawDecodingSettings.AcceptIcaoInPI0Seconds = 32;
            _Configuration.RawDecodingSettings.IgnoreInvalidCodeBlockInOtherMessages = false;
            _Configuration.RawDecodingSettings.IgnoreInvalidCodeBlockInParityMessages = true;

            triggerAction();

            foreach(var property in typeof(IRawMessageTranslator).GetProperties()) {
                switch(property.Name) {
                    case "AcceptIcaoInNonPICount":                          Assert.AreEqual(8, _RawMessageTranslator.Object.AcceptIcaoInNonPICount); break;
                    case "AcceptIcaoInNonPIMilliseconds":                   Assert.AreEqual(16000, _RawMessageTranslator.Object.AcceptIcaoInNonPIMilliseconds); break;
                    case "AcceptIcaoInPI0Count":                            Assert.AreEqual(24, _RawMessageTranslator.Object.AcceptIcaoInPI0Count); break;
                    case "AcceptIcaoInPI0Milliseconds":                     Assert.AreEqual(32000, _RawMessageTranslator.Object.AcceptIcaoInPI0Milliseconds); break;
                    case "GlobalDecodeAirborneThresholdMilliseconds":       Assert.AreEqual(99000, _RawMessageTranslator.Object.GlobalDecodeAirborneThresholdMilliseconds); break;
                    case "GlobalDecodeFastSurfaceThresholdMilliseconds":    Assert.AreEqual(88000, _RawMessageTranslator.Object.GlobalDecodeFastSurfaceThresholdMilliseconds); break;
                    case "GlobalDecodeSlowSurfaceThresholdMilliseconds":    Assert.AreEqual(77000, _RawMessageTranslator.Object.GlobalDecodeSlowSurfaceThresholdMilliseconds); break;
                    case "IgnoreInvalidCodeBlockInOtherMessages":           Assert.AreEqual(false, _RawMessageTranslator.Object.IgnoreInvalidCodeBlockInOtherMessages); break;
                    case "IgnoreInvalidCodeBlockInParityMessages":          Assert.AreEqual(true, _RawMessageTranslator.Object.IgnoreInvalidCodeBlockInParityMessages); break;
                    case "IgnoreMilitaryExtendedSquitter":                  Assert.AreEqual(true, _RawMessageTranslator.Object.IgnoreMilitaryExtendedSquitter); break;
                    case "LocalDecodeMaxSpeedAirborne":                     Assert.AreEqual(999.123, _RawMessageTranslator.Object.LocalDecodeMaxSpeedAirborne); break;
                    case "LocalDecodeMaxSpeedSurface":                      Assert.AreEqual(777.789, _RawMessageTranslator.Object.LocalDecodeMaxSpeedSurface); break;
                    case "LocalDecodeMaxSpeedTransition":                   Assert.AreEqual(888.456, _RawMessageTranslator.Object.LocalDecodeMaxSpeedTransition); break;
                    case "ReceiverRangeKilometres":                         Assert.AreEqual(1234, _RawMessageTranslator.Object.ReceiverRangeKilometres); break;
                    case "SuppressCallsignsFromBds20":                      Assert.AreEqual(true, _RawMessageTranslator.Object.SuppressCallsignsFromBds20); break;
                    case "SuppressTisbDecoding":                            Assert.AreEqual(true, _RawMessageTranslator.Object.SuppressTisbDecoding); break;
                    case "SuppressReceiverRangeCheck":                      Assert.AreEqual(true, _RawMessageTranslator.Object.SuppressReceiverRangeCheck); break;
                    case "TrackingTimeoutSeconds":                          Assert.AreEqual(100, _RawMessageTranslator.Object.TrackingTimeoutSeconds); break;
                    case "UseLocalDecodeForInitialPosition":                Assert.AreEqual(true, _RawMessageTranslator.Object.UseLocalDecodeForInitialPosition); break;
                    case "ReceiverLocation":
                        Assert.AreEqual(1.2345, _RawMessageTranslator.Object.ReceiverLocation.Latitude);
                        Assert.AreEqual(-17.89, _RawMessageTranslator.Object.ReceiverLocation.Longitude);
                        break;
                    case "Statistics":
                        break;
                    default:
                        throw new NotImplementedException(String.Format("Need to implement test for {0}", property.Name));
                }
            }
        }

        [TestMethod]
        public void Feed_Initialise_Does_Not_Connect_Listener()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            _Listener.Verify(r => r.ChangeSource(It.IsAny<IConnector>(), It.IsAny<IMessageBytesExtractor>(), It.IsAny<IRawMessageTranslator>()), Times.Once());
            _Listener.Verify(r => r.Connect(), Times.Never());
        }

        [TestMethod]
        public void Feed_Initialise_Attaches_Listener_To_AircraftList()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            Assert.AreSame(_Listener.Object, _Feed.AircraftList.Listener);
        }

        [TestMethod]
        public void Feed_Initialise_Attaches_StandingDataManager_To_AircraftList()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            Assert.AreSame(_StandingDataManager.Object, _Feed.AircraftList.StandingDataManager);
        }

        [TestMethod]
        public void Feed_Initialise_Starts_AircraftList_If_Required()
        {
            foreach(ReceiverUsage receiverUsage in Enum.GetValues(typeof(ReceiverUsage))) {
                TestCleanup();
                TestInitialise();

                _Receiver.ReceiverUsage = receiverUsage;
                _Feed.Initialise(_Receiver, _Configuration);

                switch(receiverUsage) {
                    case ReceiverUsage.Normal:
                    case ReceiverUsage.HideFromWebSite:
                        _AircraftList.Verify(r => r.Start(), Times.Once());
                        break;
                    case ReceiverUsage.MergeOnly:
                        _AircraftList.Verify(r => r.Start(), Times.Never());
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        [TestMethod]
        public void Feed_Initialise_Sets_Receiver_IsVisible_From_ReceiverUsage()
        {
            foreach(ReceiverUsage receiverUsage in Enum.GetValues(typeof(ReceiverUsage))) {
                TestCleanup();
                TestInitialise();

                _Receiver.ReceiverUsage = receiverUsage;
                _Feed.Initialise(_Receiver, _Configuration);

                var isVisible = receiverUsage == ReceiverUsage.Normal;
                Assert.AreEqual(isVisible, _Feed.IsVisible, receiverUsage.ToString());
            }
        }

        [TestMethod]
        public void Feed_Initialise_Hooks_Listener_ExceptionCaught()
        {
            _Feed.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();
            _Feed.Initialise(_Receiver, _Configuration);
            _Listener.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Feed, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void Feed_Initialise_Hooks_AircraftList_ExceptionCaught()
        {
            _Feed.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();
            _Feed.Initialise(_Receiver, _Configuration);
            _AircraftList.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Feed, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void Feed_Initialise_Sets_Authentication_On_Network_Connector_If_Passphrase_Supplied()
        {
            _Receiver.Passphrase = "A";

            _Feed.Initialise(_Receiver, _Configuration);

            Assert.AreSame(_PassphraseAuthentication.Object, _Feed.Listener.Connector.Authentication);
            Assert.AreEqual("A", _PassphraseAuthentication.Object.Passphrase);
        }

        [TestMethod]
        public void Feed_Initialise_Does_Not_Set_Authentication_On_Network_Connector_If_Passphrase_Is_Null()
        {
            _Receiver.Passphrase = null;

            _Feed.Initialise(_Receiver, _Configuration);

            Assert.IsNull(_Feed.Listener.Connector.Authentication);
        }
        #endregion

        #region Initialise - MergedFeed
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Feed_Initialise_Throws_If_Passed_Null_MergedFeed()
        {
            _Feed.Initialise((MergedFeed)null, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Feed_Initialise_Throws_If_Passed_Null_ReceiverPathways_List()
        {
            _Feed.Initialise(_MergedFeed, null);
        }

        [TestMethod]
        public void Feed_Initialise_Does_Not_Care_If_MergedFeed_Is_Not_In_Configuration()
        {
            var otherMergedFeed = new MergedFeed() { UniqueId = _MergedFeed.UniqueId + 1, Name = "Other mergedFeed", ReceiverIds = { 1, 2 } };
            _Feed.Initialise(otherMergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_Initialise_Throws_If_Passed_Disabled_MergedFeed()
        {
            _MergedFeed.Enabled = false;
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_Initialise_Throws_If_Called_Twice_With_MergedFeeds()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_Initialise_Throws_If_Called_Twice_With_Combination_Of_Receiver_And_MergedFeeds()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        public void Feed_Initialise_Does_Not_Throw_If_ReceiverId_Is_Not_In_ReceiverPathways()
        {
            _MergedFeed.ReceiverIds.Clear();
            _MergedFeed.ReceiverIds.Add(100);
            _MergedFeed.ReceiverIds.Add(101);
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        public void Feed_Initialise_Copies_MergedFeed_Details_To_Properties()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(3, _Feed.UniqueId);
            Assert.AreEqual("M1", _Feed.Name);
        }

        [TestMethod]
        public void Feed_Initialise_Creates_MergedFeed_Listener_And_AircraftList()
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
        }

        [TestMethod]
        public void Feed_Initialise_Sets_IgnoreAircraftWithNoPosition_On_MergedFeed()
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
        public void Feed_Initialise_Does_Not_Call_Change_Source_For_Merged_Feeds()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            _MergedFeedListener.Verify(r => r.ChangeSource(It.IsAny<IConnector>(), It.IsAny<IMessageBytesExtractor>(), It.IsAny<IRawMessageTranslator>()), Times.Never());
        }

        [TestMethod]
        public void Feed_Initialise_Calls_SetListeners()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(2, _SetMergedFeedListeners.Count);
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[0].Object)));
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[1].Object)));
        }

        [TestMethod]
        public void Feed_Initialise_Only_Calls_SetListeners_With_Listeners_For_The_Right_IDs()
        {
            FeedHelper.AddFeeds(_Feeds, _Listeners, 3, 4);
            _MergedFeedReceivers = FeedHelper.GetFeeds(_Feeds);

            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(2, _SetMergedFeedListeners.Count);
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[0].Object)));
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[1].Object)));
        }

        [TestMethod]
        public void Feed_Initialise_Calls_SetListeners_With_The_Correct_Feed_Types()
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
        public void Feed_Initialise_Calls_SetListeners_With_No_Feed_Type_When_No_Flag_Is_Set()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            var component0 = _SetMergedFeedListeners.Single(r => Object.ReferenceEquals(r.Listener, _Listeners[0].Object));
            var component1 = _SetMergedFeedListeners.Single(r => Object.ReferenceEquals(r.Listener, _Listeners[1].Object));

            Assert.AreEqual(false, component0.IsMlatFeed);
            Assert.AreEqual(false, component1.IsMlatFeed);
        }

        [TestMethod]
        public void Feed_Initialise_Attaches_Merged_Feed_Listener_To_AircraftList()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            Assert.AreSame(_MergedFeedListener.Object, _Feed.AircraftList.Listener);
        }

        [TestMethod]
        public void Feed_Initialise_Attaches_StandingDataManager_To_MergedFeed_AircraftList()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            Assert.AreSame(_StandingDataManager.Object, _Feed.AircraftList.StandingDataManager);
        }

        [TestMethod]
        public void Feed_Initialise_Starts_MergedFeed_AircraftList()
        {
            _AircraftList.Setup(r => r.Start()).Callback(() => {
                Assert.IsNotNull(_AircraftList.Object.Listener);
                Assert.IsNotNull(_AircraftList.Object.StandingDataManager);
            });

            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);

            _AircraftList.Verify(r => r.Start(), Times.Once());
        }

        [TestMethod]
        public void Feed_Initialise_Sets_IsVisible_For_MergedFeeds()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            Assert.IsTrue(_Feed.IsVisible);
        }

        [TestMethod]
        public void Feed_Initialise_Hooks_MergedFeed_Listener_ExceptionCaught()
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
        public void Feed_Initialise_Hooks_MergedFeed_AircraftList_ExceptionCaught()
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
        public void Feed_Initialise_Sets_MergedFeed_IsVisible_From_ReceiverUsage()
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

        #region ApplyConfiguration - Receiver
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_ApplyConfiguration_Throws_If_Called_Before_Initialise()
        {
            _Feed.ApplyConfiguration(_Receiver, _Configuration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Feed_ApplyConfiguration_Throws_If_Passed_Null_Receiver()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.ApplyConfiguration((Receiver)null, _Configuration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Feed_ApplyConfiguration_Throws_If_Passed_Null_Configuration()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.ApplyConfiguration(_Receiver, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_ApplyConfiguration_Throws_If_Passed_Receiver_With_Different_Unique_ID()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            ++_Receiver.UniqueId;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Picks_Up_Name_Change()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Receiver.Name = "My New Name";
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.AreEqual("My New Name", _Feed.Name);
            Assert.AreEqual("My New Name", _Feed.Listener.ReceiverName);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Picks_Up_IsSatcomFeed_Change()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Receiver.IsSatcomFeed = false;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.AreEqual(false, _Feed.Listener.IsSatcomFeed);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Saves_PolarPlot_Before_Change_Of_Name()
        {
            var savedFeedName = "";
            _SavedPolarPlotStorage.Setup(r => r.Save(It.IsAny<IFeed>())).Callback((IFeed feed) => {
                savedFeedName = feed.Name;
            });

            _Receiver.Name = "OldName";
            _Feed.Initialise(_Receiver, _Configuration);
            _Receiver.Name = "NewName";
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.AreEqual("OldName", savedFeedName);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Does_Not_Save_PolarPlot_If_Name_Does_Not_Change()
        {
            _Receiver.Name = "OldName";
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            _SavedPolarPlotStorage.Verify(r => r.Save(It.IsAny<IFeed>()), Times.Never());
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Copies_Changes_To_Listener()
        {
            Do_Check_Configuration_Changes_Are_Applied(true, () => _Feed.ApplyConfiguration(_Receiver, _Configuration) );
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Copies_Changes_To_RawTranslator()
        {
            Do_Check_Configuration_Changes_Copied_To_RawTranslator(true, () => _Feed.ApplyConfiguration(_Receiver, _Configuration) );
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Applies_Correct_Settings_To_Network_Connector()
        {
            Do_Check_Correct_Network_Settings_Applied_To_Connector(true, () => _Feed.ApplyConfiguration(_Receiver, _Configuration) );
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Creates_PolarPlotter_If_ReceiverLocation_Is_Added()
        {
            _Configuration.ReceiverLocations[0].UniqueId = 1000;
            _Feed.Initialise(_Receiver, _Configuration);

            _Configuration.ReceiverLocations[0].UniqueId = 1;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.AreSame(_PolarPlotter.Object, _Feed.AircraftList.PolarPlotter);
            _PolarPlotter.Verify(r => r.Initialise(1.1, 2.2), Times.Once());
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Loads_PolarPlotter_If_ReceiverLocation_Is_Added()
        {
            _Configuration.ReceiverLocations[0].UniqueId = 1000;
            _Feed.Initialise(_Receiver, _Configuration);

            _Configuration.ReceiverLocations[0].UniqueId = 1;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            _SavedPolarPlotStorage.Verify(r => r.Load(_Feed), Times.Once());
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Loads_PolarPlotter_If_Name_Is_Changed()
        {
            _Receiver.Name = "OldName";
            _Feed.Initialise(_Receiver, _Configuration);

            _Receiver.Name = "NewName";
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            _SavedPolarPlotStorage.Verify(r => r.Load(_Feed), Times.Exactly(2));       // Initial load and then reload after name change
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Reinitialises_PolarPlotter_If_ReceiverLocation_Latitude_Is_Changed()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            // We want to make sure that it doesn't create a new plotter in this case
            TestUtilities.CreateMockImplementation<IPolarPlotter>();

            _Configuration.ReceiverLocations[0].Latitude = 9.9;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.AreSame(_PolarPlotter.Object, _Feed.AircraftList.PolarPlotter);
            _PolarPlotter.Verify(r => r.Initialise(9.9, 2.2), Times.Once());
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Does_Not_Load_PolarPlotter_If_ReceiverLocation_Latitude_Is_Changed()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            TestUtilities.CreateMockImplementation<IPolarPlotter>();
            _Configuration.ReceiverLocations[0].Latitude = 9.9;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            _SavedPolarPlotStorage.Verify(r => r.Load(_Feed), Times.Once());        // We always get an initial load
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Reinitialises_PolarPlotter_If_ReceiverLocation_Longitude_Is_Changed()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            // We want to make sure that it doesn't create a new plotter in this case
            TestUtilities.CreateMockImplementation<IPolarPlotter>();

            _Configuration.ReceiverLocations[0].Longitude = 9.9;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.AreSame(_PolarPlotter.Object, _Feed.AircraftList.PolarPlotter);
            _PolarPlotter.Verify(r => r.Initialise(1.1, 9.9), Times.Once());
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Leaves_PolarPlotter_Alone_If_Nothing_Changed()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            // We want to make sure that it doesn't create a new plotter in this case
            TestUtilities.CreateMockImplementation<IPolarPlotter>();
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.AreSame(_PolarPlotter.Object, _Feed.AircraftList.PolarPlotter);
            _PolarPlotter.Verify(r => r.Initialise(1.1, 2.2), Times.Once());
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Nulls_PolarPlotter_If_ReceiverLocation_Goes_Away()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            _Configuration.ReceiverLocations.Clear();
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.AreNotSame(_PolarPlotter.Object, _Feed.AircraftList.PolarPlotter);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Only_Creates_New_BytesExtractor_When_DataSource_Changes()
        {
            foreach(var initialDataSource in DataSource.AllInternalDataSources) {
                foreach(var newDataSource in DataSource.AllInternalDataSources) {
                    TestCleanup();
                    TestInitialise();

                    _Receiver.DataSource = initialDataSource;
                    _Feed.Initialise(_Receiver, _Configuration);
                    var initialBytesExtractor = _Listener.Object.BytesExtractor;

                    CreateNewListenerChildObjectInstances();

                    _Receiver.DataSource = newDataSource;
                    _Feed.ApplyConfiguration(_Receiver, _Configuration);

                    var failMessage = String.Format("Initial datasource is {0}, new datasource is {1}", initialDataSource, newDataSource);
                    if(initialDataSource == newDataSource) {
                        Assert.AreSame(initialBytesExtractor, _Listener.Object.BytesExtractor, failMessage);
                    } else {
                        Assert.AreNotSame(initialBytesExtractor, _Listener.Object.BytesExtractor, failMessage);
                    }
                }
            }
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Resets_Statistics_When_DataSource_Changes()
        {
            foreach(var initialDataSource in DataSource.AllInternalDataSources) {
                foreach(var newDataSource in DataSource.AllInternalDataSources) {
                    TestCleanup();
                    TestInitialise();

                    _Receiver.DataSource = initialDataSource;
                    _Feed.Initialise(_Receiver, _Configuration);

                    CreateNewListenerChildObjectInstances();

                    _Receiver.DataSource = newDataSource;
                    _Feed.ApplyConfiguration(_Receiver, _Configuration);

                    if(initialDataSource == newDataSource) {
                        _Statistics.Verify(r => r.ResetMessageCounters(), Times.Once());
                    } else {
                        _Statistics.Verify(r => r.ResetMessageCounters(), Times.Exactly(2));
                    }
                }
            }
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Only_Creates_New_Provider_When_Connection_Properties_Change()
        {
            foreach(ConnectionType initialConnectionType in Enum.GetValues(typeof(ConnectionType))) {
                foreach(ConnectionType newConnectionType in Enum.GetValues(typeof(ConnectionType))) {
                    foreach(var connectionProperty in _ConnectionProperties) {
                        TestCleanup();
                        TestInitialise();

                        _Receiver.ConnectionType = initialConnectionType;
                        _Feed.Initialise(_Receiver, _Configuration);
                        var initialConnector = _Listener.Object.Connector;

                        CreateNewListenerChildObjectInstances();

                        _Receiver.ConnectionType = newConnectionType;
                        connectionProperty.ChangeProperty(_Configuration);

                        _Feed.ApplyConfiguration(_Receiver, _Configuration);

                        var failMessage = String.Format("Initial connectionType is {0}, new connectionType is {1}, changed property {2}", initialConnectionType, newConnectionType, connectionProperty.Name);
                        if(initialConnectionType == newConnectionType && !connectionProperty.MatchesConnectionType(newConnectionType)) {
                            Assert.AreSame(initialConnector, _Listener.Object.Connector, failMessage);
                        } else {
                            Assert.AreNotSame(initialConnector, _Listener.Object.Connector, failMessage);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Resets_Statistics_When_Connection_Properties_Change()
        {
            foreach(ConnectionType initialConnectionType in Enum.GetValues(typeof(ConnectionType))) {
                foreach(ConnectionType newConnectionType in Enum.GetValues(typeof(ConnectionType))) {
                    foreach(var connectionProperty in _ConnectionProperties) {
                        TestCleanup();
                        TestInitialise();

                        _Receiver.ConnectionType = initialConnectionType;
                        _Feed.Initialise(_Receiver, _Configuration);

                        CreateNewListenerChildObjectInstances();

                        _Receiver.ConnectionType = newConnectionType;
                        connectionProperty.ChangeProperty(_Configuration);

                        _Feed.ApplyConfiguration(_Receiver, _Configuration);

                        if(initialConnectionType == newConnectionType && !connectionProperty.MatchesConnectionType(newConnectionType)) {
                            _Statistics.Verify(r => r.ResetMessageCounters(), Times.Once());
                        } else {
                            _Statistics.Verify(r => r.ResetMessageCounters(), Times.Exactly(2));
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Only_Creates_New_RawMessageTranslator_When_ConnectionType_Or_DataSource_Changes()
        {
            var connectionTypes = new ConnectionType[] { ConnectionType.COM, ConnectionType.TCP };
            var dataSources = new string[] { DataSource.Sbs3, DataSource.Beast };
            foreach(ConnectionType initialConnectionType in connectionTypes) {
                foreach(ConnectionType newConnectionType in connectionTypes) {
                    foreach(var initialDataSource in dataSources) {
                        foreach(var newDataSource in dataSources) {
                            foreach(var settingProperty in _RawMessageTranslatorProperties) {
                                TestCleanup();
                                TestInitialise();

                                _Receiver.ConnectionType = initialConnectionType;
                                _Receiver.DataSource = initialDataSource;
                                _Feed.Initialise(_Receiver, _Configuration);
                                var initialTranslator = _Listener.Object.RawMessageTranslator;

                                CreateNewListenerChildObjectInstances();

                                _Receiver.ConnectionType = newConnectionType;
                                _Receiver.DataSource = newDataSource;
                                settingProperty.ChangeProperty(_Configuration);

                                _Feed.ApplyConfiguration(_Receiver, _Configuration);

                                var failMessage = String.Format("ConnectionType: from {0} to {1}, DataSource: from {2} to {3}, Changed Property: {4}", initialConnectionType, newConnectionType, initialDataSource, newDataSource, settingProperty.Name);
                                if(initialConnectionType == newConnectionType && initialDataSource == newDataSource) {
                                    Assert.AreSame(initialTranslator, _Listener.Object.RawMessageTranslator, failMessage);
                                } else {
                                    Assert.AreNotSame(initialTranslator, _Listener.Object.RawMessageTranslator, failMessage);
                                }
                            }
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Does_Not_Call_ChangeSource_If_DataSource_Or_ConnectionType_Has_Not_Changed()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Listener.Verify(r => r.ChangeSource(It.IsAny<IConnector>(), It.IsAny<IMessageBytesExtractor>(), It.IsAny<IRawMessageTranslator>()), Times.Once());

            _Feed.ApplyConfiguration(_Receiver, _Configuration);
            _Listener.Verify(r => r.ChangeSource(It.IsAny<IConnector>(), It.IsAny<IMessageBytesExtractor>(), It.IsAny<IRawMessageTranslator>()), Times.Once());

            _Receiver.DataSource = DataSource.Sbs3;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);
            _Listener.Verify(r => r.ChangeSource(It.IsAny<IConnector>(), It.IsAny<IMessageBytesExtractor>(), It.IsAny<IRawMessageTranslator>()), Times.Exactly(2));

            _Receiver.ConnectionType = ConnectionType.COM;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);
            _Listener.Verify(r => r.ChangeSource(It.IsAny<IConnector>(), It.IsAny<IMessageBytesExtractor>(), It.IsAny<IRawMessageTranslator>()), Times.Exactly(3));

            _Configuration.RawDecodingSettings.ReceiverRange = 700;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);
            _Listener.Verify(r => r.ChangeSource(It.IsAny<IConnector>(), It.IsAny<IMessageBytesExtractor>(), It.IsAny<IRawMessageTranslator>()), Times.Exactly(3));
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Applies_Passphrase_If_Supplied()
        {
            _Receiver.Passphrase = null;
            _Feed.Initialise(_Receiver, _Configuration);

            _Receiver.Passphrase = "A";
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.AreSame(_PassphraseAuthentication.Object, _Feed.Listener.Connector.Authentication);
            Assert.AreEqual("A", _PassphraseAuthentication.Object.Passphrase);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Removes_Passphrase_If_Not_Supplied()
        {
            _Receiver.Passphrase = "A";
            _Feed.Initialise(_Receiver, _Configuration);

            _Receiver.Passphrase = null;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);

            Assert.IsNull(_Feed.Listener.Connector.Authentication);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Starts_Or_Stops_AircraftList_As_Appropriate()
        {
            foreach(ReceiverUsage initialReceiverUsage in Enum.GetValues(typeof(ReceiverUsage))) {
                foreach(ReceiverUsage newReceiverUsage in Enum.GetValues(typeof(ReceiverUsage))) {
                    var wasStarted = initialReceiverUsage != ReceiverUsage.MergeOnly;
                    var shouldStart = newReceiverUsage != ReceiverUsage.MergeOnly;

                    TestCleanup();
                    TestInitialise();

                    _Receiver.ReceiverUsage = initialReceiverUsage;
                    _Feed.Initialise(_Receiver, _Configuration);

                    _Receiver.ReceiverUsage = newReceiverUsage;
                    _Feed.ApplyConfiguration(_Receiver, _Configuration);

                    var expectedStarts = (wasStarted ? 1 : 0) + (shouldStart ? 1 : 0);
                    var expectedStops = !shouldStart ? 1 : 0;
                    var message = String.Format("initial: {0}, new: {1}", initialReceiverUsage, newReceiverUsage);

                    _AircraftList.Verify(r => r.Start(), Times.Exactly(expectedStarts), message);
                    _AircraftList.Verify(r => r.Stop(), Times.Exactly(expectedStops), message);
                }
            }
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Sets_IsVisible_As_Appropriate()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            foreach(ReceiverUsage receiverUsage in Enum.GetValues(typeof(ReceiverUsage))) {
                _Receiver.ReceiverUsage = receiverUsage;

                _Feed.ApplyConfiguration(_Receiver, _Configuration);

                var expected = receiverUsage == ReceiverUsage.Normal;
                Assert.AreEqual(expected, _Feed.IsVisible, receiverUsage.ToString());
            }
        }
        #endregion

        #region ApplyConfiguration - MergedFeed
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_ApplyConfiguration_Throws_If_Called_Before_Initialise_For_MergedFeed()
        {
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Feed_ApplyConfiguration_Throws_If_Passed_Null_MergedFeed()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.ApplyConfiguration((MergedFeed)null, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Feed_ApplyConfiguration_Throws_If_Passed_Null_ReceiverPathways()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Feed.ApplyConfiguration(_MergedFeed, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_ApplyConfiguration_Throws_If_Passed_MergedFeed_With_Different_Unique_ID()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            ++_MergedFeed.UniqueId;
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_ApplyConfiguration_Throws_If_Initialised_With_MergedFeed_But_Updated_With_Receiver()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _Receiver.UniqueId = _MergedFeed.UniqueId;
            _Feed.ApplyConfiguration(_Receiver, _Configuration);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_ApplyConfiguration_Throws_If_Initialised_With_Receiver_But_Updated_With_MergedFeed()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _MergedFeed.UniqueId = _Receiver.UniqueId;
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Picks_Up_Name_Change_For_MergedFeed()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _MergedFeed.Name = "My New Name";
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual("My New Name", _Feed.Name);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Calls_SetListeners()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _SetMergedFeedListeners.Clear();
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(2, _SetMergedFeedListeners.Count);
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[0].Object)));
            Assert.IsTrue(_SetMergedFeedListeners.Any(r => Object.ReferenceEquals(r.Listener, _Listeners[1].Object)));
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Sets_IcaoTimeout()
        {
            _Feed.Initialise(_MergedFeed, _MergedFeedReceivers);
            _SetMergedFeedListeners.Clear();

            _MergedFeed.IcaoTimeout = 9876;
            _Feed.ApplyConfiguration(_MergedFeed, _MergedFeedReceivers);

            Assert.AreEqual(9876, _MergedFeedListener.Object.IcaoTimeout);
        }

        [TestMethod]
        public void Feed_ApplyConfiguration_Sets_IgnoreAircraftWithNoPosition()
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
        public void Feed_ApplyConfiguration_Sets_IsVisible_On_MergedFeeds_As_Appropriate()
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
        public void Feed_Dispose_Disposes_Of_AircraftList_First()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _AircraftList.Setup(r => r.Dispose()).Callback(() => {
                _Listener.Verify(r => r.Dispose(), Times.Never());
            });

            _Feed.Dispose();
            _AircraftList.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Feed_Dispose_Disposes_Of_Listener()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.Dispose();
            _Listener.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Feed_Dispose_Does_Not_Dispose_Of_BaseStationDatabase()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.Dispose();
            _BaseStationDatabase.Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void Feed_Dispose_Unhooks_Listener_ExceptionCaught()
        {
            _Feed.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.Dispose();
            _Listener.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
        }

        [TestMethod]
        public void Feed_Dispose_Unhooks_AircraftList_ExceptionCaught()
        {
            _Feed.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.Dispose();
            _AircraftList.Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
        }

        [TestMethod]
        public void Feed_Dispose_Can_Be_Called_Before_Initialise()
        {
            _Feed.Dispose();
        }

        [TestMethod]
        public void Feed_Dispose_Resets_Listener_And_AircraftList_Properties()
        {
            _Feed.Initialise(_Receiver, _Configuration);

            _Feed.Dispose();

            Assert.IsNull(_Feed.AircraftList);
            Assert.IsNull(_Feed.Listener);
        }

        [TestMethod]
        public void Feed_Dispose_Can_Be_Called_Twice()
        {
            _Feed.Dispose();
            _Feed.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Feed_Dispose_Does_Not_Reset_Initialise_DoubleCall_Guard()
        {
            _Feed.Initialise(_Receiver, _Configuration);
            _Feed.Dispose();
            _Feed.Initialise(_Receiver, _Configuration);
        }
        #endregion
    }
}
