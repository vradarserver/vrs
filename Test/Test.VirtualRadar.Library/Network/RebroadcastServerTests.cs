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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Test.Framework;
using Test.VirtualRadar.Library.Network;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Library.Network
{
    [TestClass]
    public class RebroadcastServerTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;
        private IRebroadcastServer _Server;

        private Mock<IFeed> _Feed;
        private Mock<IListener> _Listener;
        private MockConnector<INetworkConnector, INetworkConnection> _Connector;
        private BaseStationMessage _Port30003Message;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtEvent;
        private EventRecorder<EventArgs> _OnlineChangedEvent;
        private Mock<IBaseStationMessageCompressor> _Compressor;
        private Mock<IAircraftListJsonBuilder> _AircraftListJsonBuilder;
        private ClockMock _Clock;
        private Mock<ITimer> _Timer;
        private AircraftListJson _AircraftListJson;
        private Mock<IWebSiteProvider> _WebsiteProvider;
        private AircraftListJsonBuilderArgs _AircraftListJsonBuilderArgs;
        private Mock<IBaseStationAircraftList> _AircraftList;
        private List<IAircraft> _SnapshotAircraft;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _Clock = new ClockMock();
            _Timer = TestUtilities.CreateMockImplementation<ITimer>();

            _Compressor = TestUtilities.CreateMockImplementation<IBaseStationMessageCompressor>();

            _Server = Factory.Resolve<IRebroadcastServer>();

            _Feed = TestUtilities.CreateMockInstance<IFeed>();
            _Listener = TestUtilities.CreateMockInstance<IListener>();
            _Feed.SetupGet(r => r.Listener).Returns(_Listener.Object);

            _Connector = new MockConnector<INetworkConnector,INetworkConnection>();

            _AircraftListJsonBuilder = TestUtilities.CreateMockImplementation<IAircraftListJsonBuilder>();
            _WebsiteProvider = TestUtilities.CreateMockImplementation<IWebSiteProvider>();
            _AircraftListJson = new AircraftListJson();
            _AircraftListJsonBuilderArgs = null;
            _AircraftListJsonBuilder.Setup(r => r.Build(It.IsAny<AircraftListJsonBuilderArgs>())).Callback((AircraftListJsonBuilderArgs args) => {
                _AircraftListJsonBuilderArgs = args;
            }).Returns(_AircraftListJson);

            _AircraftList = TestUtilities.CreateMockImplementation<IBaseStationAircraftList>();
            _Feed.SetupGet(r => r.AircraftList).Returns(_AircraftList.Object);
            _SnapshotAircraft = new List<IAircraft>();
            long of1, of2;
            _AircraftList.Setup(m => m.TakeSnapshot(out of1, out of2)).Returns(_SnapshotAircraft);

            _Server.UniqueId = 1;
            _Server.Name = "It's the code word";
            _Server.Format = RebroadcastFormat.Port30003;
            _Server.Feed = _Feed.Object;
            _Server.Connector = _Connector.Object;

            _ExceptionCaughtEvent = new EventRecorder<EventArgs<Exception>>();
            _ExceptionCaughtEvent.EventRaised += DefaultExceptionCaughtHandler;
            _OnlineChangedEvent = new EventRecorder<EventArgs>();
            _Server.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            _Server.OnlineChanged += _OnlineChangedEvent.Handler;

            _Port30003Message = new BaseStationMessage() { Icao24 = "313233" };
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
            _Server.Dispose();
        }
        #endregion

        #region DefaultExceptionCaughtHandler
        /// <summary>
        /// Default handler for exceptions raised on a background thread by the server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void DefaultExceptionCaughtHandler(object sender, EventArgs<Exception> args)
        {
            Assert.Fail("Exception caught and passed to ExceptionCaught: {0}", args.Value.ToString());
        }

        /// <summary>
        /// Removes the default exception caught handler.
        /// </summary>
        public void RemoveDefaultExceptionCaughtHandler()
        {
            _ExceptionCaughtEvent.EventRaised -= DefaultExceptionCaughtHandler;
        }
        #endregion

        #region ExpectedBytes
        /// <summary>
        /// Converts the message passed across to the bytes that are expected to be transmitted for it.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private byte[] ExpectedBytes(BaseStationMessage message)
        {
            return Encoding.ASCII.GetBytes(String.Concat(message.ToBaseStationString(), "\r\n"));
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void RebroadcastServer_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var server = Factory.Resolve<IRebroadcastServer>();
            TestUtilities.TestProperty(server, r => r.Connector, null, _Connector.Object);
            TestUtilities.TestProperty(server, r => r.Format, null, RebroadcastFormat.Port30003);
            TestUtilities.TestProperty(server, r => r.Feed, null, _Feed.Object);
            TestUtilities.TestProperty(server, r => r.Name, null, "Abc");
            TestUtilities.TestProperty(server, r => r.Online, false);
            TestUtilities.TestProperty(server, r => r.SendIntervalMilliseconds, 1000, 30000);
            TestUtilities.TestProperty(server, r => r.UniqueId, 0, 123);
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RebroadcastServer_Initialise_Throws_If_UniqueId_Is_Zero()
        {
            _Server.UniqueId = 0;
            _Server.Initialise();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RebroadcastServer_Initialise_Throws_If_Feed_Is_Null()
        {
            _Server.Feed = null;
            _Server.Initialise();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RebroadcastServer_Initialise_Throws_If_Connector_Is_Null()
        {
            _Server.Connector = null;
            _Server.Initialise();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void RebroadcastServer_Initialise_Throws_If_Format_Is_None()
        {
            _Server.Format = "";
            _Server.Initialise();
        }

        [TestMethod]
        public void RebroadcastServer_Initialise_Throws_If_Called_Twice()
        {
            foreach(var format in RebroadcastFormat.AllInternalFormats) {
                TestCleanup();
                TestInitialise();
                _Server.Format = format;

                bool seenException = false;
                try {
                    _Server.Initialise();
                    _Server.Initialise();
                } catch(InvalidOperationException) {
                    seenException = true;
                }

                Assert.IsTrue(seenException, format.ToString());
            }
        }

        [TestMethod]
        public void RebroadcastServer_Initialise_Sets_Name_On_Connector()
        {
            _Server.Initialise();

            Assert.AreEqual("It's the code word", _Connector.Object.Name);
        }

        [TestMethod]
        public void RebroadcastServer_Initialise_Begins_Listening_On_Connector()
        {
            _Server.Initialise();

            _Connector.Verify(r => r.EstablishConnection(), Times.Once());
        }
        #endregion

        #region OnlineChanged
        [TestMethod]
        public void RebroadcastServer_OnlineChanged_Raised_When_Online_Status_Changes_To_True()
        {
            _Server.Initialise();

            _OnlineChangedEvent.EventRaised += (s, a) => { Assert.AreEqual(true, _Server.Online); };

            Assert.AreEqual(0, _OnlineChangedEvent.CallCount);
            _Server.Online = true;
            Assert.AreEqual(1, _OnlineChangedEvent.CallCount);
            Assert.AreSame(_Server, _OnlineChangedEvent.Sender);
            Assert.IsNotNull(_OnlineChangedEvent.Args);
        }

        [TestMethod]
        public void RebroadcastServer_OnlineChanged_Raised_When_Online_Status_Changes_To_False()
        {
            _Server.Initialise();
            _Server.Online = true;

            _OnlineChangedEvent.EventRaised += (s, a) => { Assert.AreEqual(false, _Server.Online); };

            _Server.Online = false;
            Assert.AreEqual(2, _OnlineChangedEvent.CallCount);
            Assert.AreSame(_Server, _OnlineChangedEvent.Sender);
            Assert.IsNotNull(_OnlineChangedEvent.Args);
        }

        [TestMethod]
        public void RebroadcastServer_OnlineChanged_Not_Raised_If_Online_Not_Changed()
        {
            _Server.Initialise();

            _Server.Online = false;
            Assert.AreEqual(0, _OnlineChangedEvent.CallCount);

            _Server.Online = true;
            _Server.Online = true;
            Assert.AreEqual(1, _OnlineChangedEvent.CallCount);
        }
        #endregion

        #region Rebroadcasting of messages from the feed's Listener
        private void SetupForAvr()
        {
            _Server.Format = RebroadcastFormat.Avr;
            _Server.Initialise();
            _Server.Online = true;
            _Connector.SetupGet(r => r.HasConnection).Returns(true);
        }

        private void SetupForPort30003()
        {
            _Server.Format = RebroadcastFormat.Port30003;
            _Server.Initialise();
            _Server.Online = true;
            _Connector.SetupGet(r => r.HasConnection).Returns(true);
        }

        private void SetupForExtendedBaseStation()
        {
            _Server.Format = RebroadcastFormat.ExtendedBaseStation;
            _Server.Initialise();
            _Server.Online = true;
            _Connector.SetupGet(r => r.HasConnection).Returns(true);
        }

        private void SetupForCompressedVRS()
        {
            _Server.Format = RebroadcastFormat.CompressedVRS;
            _Server.Initialise();
            _Server.Online = true;
            _Connector.SetupGet(r => r.HasConnection).Returns(true);
        }

        private void SetupForPassthrough()
        {
            _Server.Format = RebroadcastFormat.Passthrough;
            _Server.Initialise();
            _Server.Online = true;
            _Connector.SetupGet(r => r.HasConnection).Returns(true);
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_Port30003_Messages_From_Feed()
        {
            SetupForPort30003();

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            var expectedBytes = ExpectedBytes(_Port30003Message);
            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.IsTrue(expectedBytes.SequenceEqual(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_ExtendedBaseStation_Messages_From_Feed()
        {
            SetupForExtendedBaseStation();

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            var expectedBytes = ExpectedBytes(_Port30003Message);
            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.IsTrue(expectedBytes.SequenceEqual(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_Compressed_Port30003_Messages_From_Feed()
        {
            var bytes = new byte[] { 0x01, 0xff };
            _Compressor.Setup(r => r.Compress(_Port30003Message)).Returns(bytes);
            SetupForCompressedVRS();

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.IsTrue(bytes.SequenceEqual(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Ignores_Compressed_Port30003_Messages_That_Will_Not_Compress()
        {
            foreach(var isNull in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Compressor.Setup(r => r.Compress(_Port30003Message)).Returns(isNull ? null : new byte[] { });
                SetupForCompressedVRS();

                _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

                Assert.AreEqual(0, _Connector.Written.Count);
            }
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_Raw_Bytes_From_Listener()
        {
            SetupForPassthrough();
            var bytes = new byte[] { 0x01, 0xff };

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(bytes));

            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.IsTrue(bytes.SequenceEqual(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_AVR_Colon_Messages_When_Listener_Transmits_ModeS_Bytes_With_Parity_Stripped()
        {
            var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54 };
            SetupForAvr();

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Length = bytes.Length, Format = ExtractedBytesFormat.ModeS, HasParity = false }));

            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.AreEqual(":0123456789ABCDEFFEDCBA987654;\r\n", Encoding.ASCII.GetString(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_AVR_Star_Messages_When_Listener_Transmits_ModeS_Bytes_With_Parity_Present()
        {
            var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54 };
            SetupForAvr();

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Length = bytes.Length, Format = ExtractedBytesFormat.ModeS, HasParity = true }));

            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.AreEqual("*0123456789ABCDEFFEDCBA987654;\r\n", Encoding.ASCII.GetString(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Honours_Offset_And_Length_For_ModeS_Bytes()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            SetupForAvr();

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Offset = 1, Length = bytes.Length - 1, Format = ExtractedBytesFormat.ModeS, HasParity = true }));

            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.AreEqual("*020304;\r\n", Encoding.ASCII.GetString(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Raw_Bytes_If_Port30003_Format_Specified()
        {
            SetupForPort30003();

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(new byte[] { 0x01, 0x02 }));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Raw_Bytes_If_ExtendedBaseStation_Format_Specified()
        {
            SetupForExtendedBaseStation();

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(new byte[] { 0x01, 0x02 }));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Raw_Bytes_If_Compressed_Format_Specified()
        {
            SetupForCompressedVRS();

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(new byte[] { 0x01, 0x02 }));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Port30003_Messages_If_Passthrough_Format_Specified()
        {
            var bytes = new byte[] { 0x01, 0xff };
            SetupForPassthrough();

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Port30003_Messages_When_Offline()
        {
            SetupForPort30003();
            _Server.Online = false;

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_ExtendedBaseStation_Messages_When_Offline()
        {
            SetupForExtendedBaseStation();
            _Server.Online = false;

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Port30003_Messages_When_There_Are_No_Connections()
        {
            SetupForPort30003();
            _Connector.SetupGet(r => r.HasConnection).Returns(false);

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_ExtendedBaseStation_Messages_When_There_Are_No_Connections()
        {
            SetupForExtendedBaseStation();
            _Connector.SetupGet(r => r.HasConnection).Returns(false);

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Port30003_Messages_Do_Not_Distinguish_Between_MLAT_And_Normal_Positions()
        {
            SetupForPort30003();

            _Port30003Message.Icao24 = "ABCDEF";
            _Port30003Message.MessageType = BaseStationMessageType.Transmission;
            _Port30003Message.Latitude = 1;
            _Port30003Message.Longitude = 2;
            _Port30003Message.IsMlat = true;

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            var writtenText = Encoding.ASCII.GetString(_Connector.Written[0]);
            Assert.IsTrue(writtenText.StartsWith("MSG,"));
        }

        [TestMethod]
        public void RebroadcastServer_ExtendedBaseStation_Messages_Distinguish_Between_MLAT_And_Normal_Positions()
        {
            SetupForExtendedBaseStation();

            _Port30003Message.Icao24 = "ABCDEF";
            _Port30003Message.MessageType = BaseStationMessageType.Transmission;
            _Port30003Message.Latitude = 1;
            _Port30003Message.Longitude = 2;
            _Port30003Message.IsMlat = true;

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            var writtenText = Encoding.ASCII.GetString(_Connector.Written[0]);
            Assert.IsTrue(writtenText.StartsWith("MLAT,"));
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Compressed_Messages_When_Offline()
        {
            var bytes = new byte[] { 0x01, 0xff };
            _Compressor.Setup(r => r.Compress(_Port30003Message)).Returns(bytes);
            SetupForCompressedVRS();
            _Server.Online = false;

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Compressed_Messages_When_There_Are_No_Connections()
        {
            var bytes = new byte[] { 0x01, 0xff };
            _Compressor.Setup(r => r.Compress(_Port30003Message)).Returns(bytes);
            SetupForCompressedVRS();
            _Connector.SetupGet(r => r.HasConnection).Returns(false);

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Raw_Bytes_When_Offline()
        {
            var bytes = new byte[] { 0x01, 0xff };
            SetupForPassthrough();
            _Server.Online = false;

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(bytes));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Raw_Bytes_When_There_Are_No_Connections()
        {
            var bytes = new byte[] { 0x01, 0xff };
            SetupForPassthrough();
            _Connector.SetupGet(r => r.HasConnection).Returns(false);

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(bytes));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_AVR_Format_When_Offline()
        {
            var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54 };
            SetupForAvr();
            _Server.Online = false;

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Length = bytes.Length, Format = ExtractedBytesFormat.ModeS, HasParity = false }));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_AVR_Format_When_There_Are_No_Connections()
        {
            var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54 };
            SetupForAvr();
            _Connector.SetupGet(r => r.HasConnection).Returns(false);

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Length = bytes.Length, Format = ExtractedBytesFormat.ModeS, HasParity = false }));

            Assert.AreEqual(0, _Connector.Written.Count);
        }
        #endregion

        #region Rebroadcasting of entire aircraft lists
        private void ConfigureForAircraftListJson()
        {
            _Server.Format = RebroadcastFormat.AircraftListJson;
            _Server.SendIntervalMilliseconds = 1000;
            _Server.Initialise();
            _Server.Online = true;

            _Connector.SetupGet(r => r.HasConnection).Returns(true);
        }

        private Mock<IAircraft> AddSnapshotAircraft(int uniqueId = -1)
        {
            var result = TestUtilities.CreateMockInstance<IAircraft>();
            result.Object.UniqueId = uniqueId == -1 ? _SnapshotAircraft.Count + 1 : uniqueId;
            _SnapshotAircraft.Add(result.Object);

            return result;
        }

        private void SetupAircraftJson(Func<IAircraft, bool> excludeAircraft = null)
        {
            _AircraftListJson.Aircraft.Clear();

            foreach(var snapshotAircraft in _SnapshotAircraft) {
                if(excludeAircraft == null || !excludeAircraft(snapshotAircraft)) {
                    _AircraftListJson.Aircraft.Add(new AircraftJson() {
                        UniqueId = snapshotAircraft.UniqueId,
                    });
                }
            }
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Initialises_Timer()
        {
            ConfigureForAircraftListJson();

            Assert.AreEqual(_Server.SendIntervalMilliseconds, _Timer.Object.Interval);
            Assert.AreEqual(true, _Timer.Object.Enabled);
            Assert.AreEqual(false, _Timer.Object.AutoReset);
            _Timer.Verify(r => r.Start(), Times.Once());
            _Timer.Verify(r => r.Stop(), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Sends_Aircraft_List_When_Timer_Elapses()
        {
            ConfigureForAircraftListJson();
            AddSnapshotAircraft();
            SetupAircraftJson();

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            Assert.AreEqual(1, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Passes_Correct_Arguments_To_AircraftListJsonBuilder()
        {
            ConfigureForAircraftListJson();

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            Assert.IsNotNull(_AircraftListJsonBuilderArgs);
            Assert.AreEqual(null, _AircraftListJsonBuilderArgs.AircraftList);
            Assert.AreEqual(true, _AircraftListJsonBuilderArgs.AlwaysShowIcao);
            Assert.AreEqual(null, _AircraftListJsonBuilderArgs.BrowserLatitude);
            Assert.AreEqual(null, _AircraftListJsonBuilderArgs.BrowserLongitude);
            Assert.AreEqual(true, _AircraftListJsonBuilderArgs.FeedsNotRequired);
            Assert.AreEqual(null, _AircraftListJsonBuilderArgs.Filter);
            Assert.AreEqual(true, _AircraftListJsonBuilderArgs.IgnoreUnchanged);
            Assert.AreEqual(false, _AircraftListJsonBuilderArgs.IsFlightSimulatorList);
            Assert.AreEqual(false, _AircraftListJsonBuilderArgs.IsInternetClient);
            Assert.AreEqual(true, _AircraftListJsonBuilderArgs.OnlyIncludeMessageFields);
            Assert.AreEqual(false, _AircraftListJsonBuilderArgs.ResendTrails);
            Assert.AreEqual(-1, _AircraftListJsonBuilderArgs.SelectedAircraftId);
            Assert.AreEqual(0, _AircraftListJsonBuilderArgs.SortBy.Count);
            Assert.AreEqual(_Feed.Object.UniqueId, _AircraftListJsonBuilderArgs.SourceFeedId);
            Assert.AreEqual(TrailType.None, _AircraftListJsonBuilderArgs.TrailType);

            // Data version can't be easily tested because Moq doesn't support setting out parameters.
            // However for the first call, which this is, the previous settings should all have default values.
            Assert.AreEqual(-1, _AircraftListJsonBuilderArgs.PreviousDataVersion);
            Assert.AreEqual(0, _AircraftListJsonBuilderArgs.PreviousAircraft.Count);
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Passes_Correct_Arguments_To_AircraftListJsonBuilder_For_Second_Call()
        {
            ConfigureForAircraftListJson();
            AddSnapshotAircraft(91);
            SetupAircraftJson();

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            _AircraftListJsonBuilder.Verify(r => r.Build(It.IsAny<AircraftListJsonBuilderArgs>()), Times.Exactly(2));
            Assert.AreEqual(null, _AircraftListJsonBuilderArgs.AircraftList);
            Assert.AreEqual(true, _AircraftListJsonBuilderArgs.AlwaysShowIcao);
            Assert.AreEqual(null, _AircraftListJsonBuilderArgs.BrowserLatitude);
            Assert.AreEqual(null, _AircraftListJsonBuilderArgs.BrowserLongitude);
            Assert.AreEqual(true, _AircraftListJsonBuilderArgs.FeedsNotRequired);
            Assert.AreEqual(null, _AircraftListJsonBuilderArgs.Filter);
            Assert.AreEqual(true, _AircraftListJsonBuilderArgs.IgnoreUnchanged);
            Assert.AreEqual(false, _AircraftListJsonBuilderArgs.IsFlightSimulatorList);
            Assert.AreEqual(false, _AircraftListJsonBuilderArgs.IsInternetClient);
            Assert.AreEqual(true, _AircraftListJsonBuilderArgs.OnlyIncludeMessageFields);
            Assert.AreEqual(false, _AircraftListJsonBuilderArgs.ResendTrails);
            Assert.AreEqual(-1, _AircraftListJsonBuilderArgs.SelectedAircraftId);
            Assert.AreEqual(0, _AircraftListJsonBuilderArgs.SortBy.Count);
            Assert.AreEqual(_Feed.Object.UniqueId, _AircraftListJsonBuilderArgs.SourceFeedId);
            Assert.AreEqual(TrailType.None, _AircraftListJsonBuilderArgs.TrailType);

            Assert.AreEqual(1, _AircraftListJsonBuilderArgs.PreviousAircraft.Count);
            Assert.AreEqual(91, _AircraftListJsonBuilderArgs.PreviousAircraft[0]);
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Starts_New_Timer_When_Old_Timer_Elapses()
        {
            ConfigureForAircraftListJson();

            _Server.SendIntervalMilliseconds = 30000;
            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            Assert.AreEqual(30000, _Timer.Object.Interval);
            Assert.AreEqual(true, _Timer.Object.Enabled);
            Assert.AreEqual(false, _Timer.Object.AutoReset);
            _Timer.Verify(r => r.Start(), Times.Exactly(2));
            _Timer.Verify(r => r.Stop(), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Nothing_Is_Sent_If_The_Json_Has_No_Aircraft()
        {
            ConfigureForAircraftListJson();
            _SnapshotAircraft.Clear();

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Sends_Full_Json_On_First_Connection()
        {
            ConfigureForAircraftListJson();
            AddSnapshotAircraft();
            SetupAircraftJson();

            var connection = TestUtilities.CreateMockInstance<IConnection>();
            var args = new ConnectionEventArgs(connection.Object);
            _Connector.Raise(r => r.AddingConnection += null, args);

            _AircraftListJsonBuilder.Verify(r => r.Build(It.IsAny<AircraftListJsonBuilderArgs>()), Times.Once());
            Assert.AreEqual(-1, _AircraftListJsonBuilderArgs.PreviousDataVersion);
            Assert.AreEqual(0, _AircraftListJsonBuilderArgs.PreviousAircraft.Count);
            connection.Verify(r => r.Write(It.IsAny<byte[]>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Does_Not_Use_PreviousAircraftList_On_First_Connection()
        {
            ConfigureForAircraftListJson();
            AddSnapshotAircraft();
            SetupAircraftJson();

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            var connection = TestUtilities.CreateMockInstance<IConnection>();
            var args = new ConnectionEventArgs(connection.Object);
            _Connector.Raise(r => r.AddingConnection += null, args);

            Assert.AreEqual(0, _AircraftListJsonBuilderArgs.PreviousAircraft.Count);
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Does_Not_Use_AircraftList_From_First_Connection()
        {
            ConfigureForAircraftListJson();
            AddSnapshotAircraft();
            SetupAircraftJson();

            var connection = TestUtilities.CreateMockInstance<IConnection>();
            var args = new ConnectionEventArgs(connection.Object);
            _Connector.Raise(r => r.AddingConnection += null, args);

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            Assert.AreEqual(0, _AircraftListJsonBuilderArgs.PreviousAircraft.Count);
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Does_Not_Restart_The_Timer_On_First_Connection()
        {
            ConfigureForAircraftListJson();

            var connection = TestUtilities.CreateMockInstance<IConnection>();
            var args = new ConnectionEventArgs(connection.Object);
            _Connector.Raise(r => r.AddingConnection += null, args);

            _Timer.Verify(r => r.Start(), Times.Once());        // 1 call from initialise, none from the connector getting added
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Does_Nothing_If_Connector_Has_No_Connections()
        {
            ConfigureForAircraftListJson();
            AddSnapshotAircraft();
            SetupAircraftJson();

            _Connector.SetupGet(r => r.HasConnection).Returns(false);

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            long of1, of2;
            _AircraftList.Verify(r => r.TakeSnapshot(out of1, out of2), Times.Never());
            _AircraftListJsonBuilder.Verify(r => r.Build(It.IsAny<AircraftListJsonBuilderArgs>()), Times.Never());
            Assert.AreEqual(0, _Connector.Written.Count);
            _Timer.Verify(r => r.Start(), Times.Exactly(2));
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Does_Nothing_When_Offline()
        {
            ConfigureForAircraftListJson();
            AddSnapshotAircraft();
            SetupAircraftJson();

            _Server.Online = false;

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            long of1, of2;
            _AircraftList.Verify(r => r.TakeSnapshot(out of1, out of2), Times.Never());
            _AircraftListJsonBuilder.Verify(r => r.Build(It.IsAny<AircraftListJsonBuilderArgs>()), Times.Never());
            Assert.AreEqual(0, _Connector.Written.Count);
            _Timer.Verify(r => r.Start(), Times.Exactly(2));
        }

        [TestMethod]
        public void RebroadcastServer_AircraftListJson_Sends_First_Time_Data_When_Connector_Has_No_Connections()
        {
            ConfigureForAircraftListJson();
            AddSnapshotAircraft();
            SetupAircraftJson();

            _Connector.SetupGet(r => r.HasConnection).Returns(false);

            var connection = TestUtilities.CreateMockInstance<IConnection>();
            var args = new ConnectionEventArgs(connection.Object);
            _Connector.Raise(r => r.AddingConnection += null, args);

            _AircraftListJsonBuilder.Verify(r => r.Build(It.IsAny<AircraftListJsonBuilderArgs>()), Times.Once());
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void RebroadcastServer_Dispose_Unhooks_From_Listener_Port30003_Events()
        {
            _Server.Format = RebroadcastFormat.Port30003;
            _Server.Initialise();
            _Server.Online = true;

            _Server.Dispose();

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Dispose_Can_Cope_If_Feed_Changes_Listener_Before_Dispose()
        {
            _Server.Format = RebroadcastFormat.Port30003;
            _Server.Initialise();
            _Server.Online = true;

            _Feed.SetupGet(r => r.Listener).Returns(TestUtilities.CreateMockInstance<IListener>().Object);

            _Server.Dispose();

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Dispose_Unhooks_From_Listener_Raw_Bytes_Events()
        {
            var bytes = new byte[] { 0x01, 0xff };
            _Server.Format = RebroadcastFormat.Passthrough;
            _Server.Initialise();
            _Server.Online = true;

            _Server.Dispose();

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(bytes));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Dispose_Unhooks_From_Listener_ModeS_Raw_Bytes_Events()
        {
            var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54 };
            _Server.Format = RebroadcastFormat.Avr;
            _Server.Initialise();
            _Server.Online = true;

            _Server.Dispose();

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Length = bytes.Length, Format = ExtractedBytesFormat.ModeS, HasParity = false }));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Dispose_Does_Not_Care_If_Feed_Is_Null()
        {
            _Server.Feed = null;
            _Server.Dispose();
        }

        [TestMethod]
        public void RebroadcastServer_Dispose_Does_Not_Care_If_Connector_Is_Null()
        {
            _Server.Connector = null;
            _Server.Dispose();
        }

        [TestMethod]
        public void RebroadcastServer_Dispose_Does_Not_Dispose_Of_Listener()
        {
            _Server.Initialise();
            _Server.Dispose();

            _Listener.Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServer_Dispose_Does_Not_Dispose_Of_BroadcastProvider()
        {
            _Server.Initialise();
            _Server.Dispose();

            _Connector.Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServer_Disposes_Of_Timer()
        {
            ConfigureForAircraftListJson();
            _Server.Dispose();
            _Timer.Verify(r => r.Dispose(), Times.Once());

            _Timer.Raise(r => r.Elapsed += null, EventArgs.Empty);

            _AircraftListJsonBuilder.Verify(r => r.Build(It.IsAny<AircraftListJsonBuilderArgs>()), Times.Never());
        }

        [TestMethod]
        public void RebroadcastServer_Unhooks_Connector()
        {
            ConfigureForAircraftListJson();
            _Server.Dispose();

            var connection = TestUtilities.CreateMockInstance<IConnection>();
            var args = new ConnectionEventArgs(connection.Object);
            _Connector.Raise(r => r.AddingConnection += null, args);

            _AircraftListJsonBuilder.Verify(r => r.Build(It.IsAny<AircraftListJsonBuilderArgs>()), Times.Never());
        }
        #endregion
    }
}
