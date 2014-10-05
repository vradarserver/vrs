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
using System.Linq;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using Test.VirtualRadar.Library.Network;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Library.Network
{
    [TestClass]
    public class RebroadcastServerTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;
        private IRebroadcastServer _Server;
        private Mock<IListener> _Listener;
        private MockConnector<INetworkConnector, INetworkConnection> _Connector;
        private BaseStationMessage _Port30003Message;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtEvent;
        private EventRecorder<EventArgs> _OnlineChangedEvent;
        private Mock<IBaseStationMessageCompressor> _Compressor;
        private ClockMock _Clock;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _Clock = new ClockMock();

            _Compressor = TestUtilities.CreateMockImplementation<IBaseStationMessageCompressor>();

            _Server = Factory.Singleton.Resolve<IRebroadcastServer>();
            _Listener = new Mock<IListener>(MockBehavior.Default) { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            _Connector = new MockConnector<INetworkConnector,INetworkConnection>();

            _Server.UniqueId = 1;
            _Server.Name = "It's the code word";
            _Server.Format = RebroadcastFormat.Port30003;
            _Server.Listener = _Listener.Object;
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
            var server = Factory.Singleton.Resolve<IRebroadcastServer>();
            TestUtilities.TestProperty(server, r => r.Connector, null, _Connector.Object);
            TestUtilities.TestProperty(server, r => r.Format, RebroadcastFormat.None, RebroadcastFormat.Port30003);
            TestUtilities.TestProperty(server, r => r.Listener, null, _Listener.Object);
            TestUtilities.TestProperty(server, r => r.Name, null, "Abc");
            TestUtilities.TestProperty(server, r => r.Online, false);
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
        public void RebroadcastServer_Initialise_Throws_If_Listener_Is_Null()
        {
            _Server.Listener = null;
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
            _Server.Format = RebroadcastFormat.None;
            _Server.Initialise();
        }

        [TestMethod]
        public void RebroadcastServer_Initialise_Throws_If_Called_Twice()
        {
            foreach(RebroadcastFormat format in Enum.GetValues(typeof(RebroadcastFormat))) {
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

            Assert.AreEqual("Word Up!", _Connector.Object.Name);
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

        #region Rebroadcasting of messages
        [TestMethod]
        public void RebroadcastServer_Transmits_Port30003_Messages_From_Listener()
        {
            _Server.Format = RebroadcastFormat.Port30003;
            _Server.Initialise();
            _Server.Online = true;

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            var expectedBytes = ExpectedBytes(_Port30003Message);
            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.IsTrue(expectedBytes.SequenceEqual(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_Compressed_Port30003_Messages_From_Listener()
        {
            var bytes = new byte[] { 0x01, 0xff };
            _Compressor.Setup(r => r.Compress(_Port30003Message)).Returns(bytes);
            _Server.Format = RebroadcastFormat.CompressedVRS;
            _Server.Initialise();
            _Server.Online = true;

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
                _Server.Format = RebroadcastFormat.CompressedVRS;
                _Server.Initialise();
                _Server.Online = true;

                _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

                Assert.AreEqual(0, _Connector.Written.Count);
            }
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_Raw_Bytes_From_Listener()
        {
            var bytes = new byte[] { 0x01, 0xff };
            _Server.Format = RebroadcastFormat.Passthrough;
            _Server.Initialise();
            _Server.Online = true;

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(bytes));

            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.IsTrue(bytes.SequenceEqual(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_AVR_Colon_Messages_When_Listener_Transmits_ModeS_Bytes_With_Parity_Stripped()
        {
            var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54 };
            _Server.Format = RebroadcastFormat.Avr;
            _Server.Initialise();
            _Server.Online = true;

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Length = bytes.Length, Format = ExtractedBytesFormat.ModeS, HasParity = false }));

            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.AreEqual(":0123456789ABCDEFFEDCBA987654;\r\n", Encoding.ASCII.GetString(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Transmits_AVR_Star_Messages_When_Listener_Transmits_ModeS_Bytes_With_Parity_Present()
        {
            var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54 };
            _Server.Format = RebroadcastFormat.Avr;
            _Server.Initialise();
            _Server.Online = true;

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Length = bytes.Length, Format = ExtractedBytesFormat.ModeS, HasParity = true }));

            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.AreEqual("*0123456789ABCDEFFEDCBA987654;\r\n", Encoding.ASCII.GetString(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Honours_Offset_And_Length_For_ModeS_Bytes()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            _Server.Format = RebroadcastFormat.Avr;
            _Server.Initialise();
            _Server.Online = true;

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Offset = 1, Length = bytes.Length - 1, Format = ExtractedBytesFormat.ModeS, HasParity = true }));

            Assert.AreEqual(1, _Connector.Written.Count);
            Assert.AreEqual("*020304;\r\n", Encoding.ASCII.GetString(_Connector.Written[0]));
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Raw_Bytes_If_Port30003_Format_Specified()
        {
            _Server.Format = RebroadcastFormat.Port30003;
            _Server.Initialise();
            _Server.Online = true;

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(new byte[] { 0x01, 0x02 }));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Raw_Bytes_If_Compressed_Format_Specified()
        {
            _Server.Format = RebroadcastFormat.CompressedVRS;
            _Server.Initialise();
            _Server.Online = true;

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(new byte[] { 0x01, 0x02 }));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Port30003_Messages_If_Passthrough_Format_Specified()
        {
            var bytes = new byte[] { 0x01, 0xff };
            _Server.Format = RebroadcastFormat.Passthrough;
            _Server.Initialise();
            _Server.Online = true;

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Port30003_Messages_When_Offline()
        {
            _Server.Format = RebroadcastFormat.Port30003;
            _Server.Initialise();
            _Server.Online = false;

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Compressed_Messages_When_Offline()
        {
            var bytes = new byte[] { 0x01, 0xff };
            _Compressor.Setup(r => r.Compress(_Port30003Message)).Returns(bytes);
            _Server.Format = RebroadcastFormat.CompressedVRS;
            _Server.Initialise();
            _Server.Online = false;

            _Listener.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(_Port30003Message));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_Raw_Bytes_When_Offline()
        {
            var bytes = new byte[] { 0x01, 0xff };
            _Server.Format = RebroadcastFormat.Passthrough;
            _Server.Initialise();
            _Server.Online = false;

            _Listener.Raise(r => r.RawBytesReceived += null, new EventArgs<byte[]>(bytes));

            Assert.AreEqual(0, _Connector.Written.Count);
        }

        [TestMethod]
        public void RebroadcastServer_Does_Not_Transmit_AVR_Format_When_Offline()
        {
            var bytes = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0xfe, 0xdc, 0xba, 0x98, 0x76, 0x54 };
            _Server.Format = RebroadcastFormat.Avr;
            _Server.Initialise();
            _Server.Online = false;

            _Listener.Raise(r => r.ModeSBytesReceived += null, new EventArgs<ExtractedBytes>(new ExtractedBytes() { Bytes = bytes, Length = bytes.Length, Format = ExtractedBytesFormat.ModeS, HasParity = false }));

            Assert.AreEqual(0, _Connector.Written.Count);
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
        public void RebroadcastServer_Dispose_Does_Not_Care_If_Listener_Is_Null()
        {
            _Server.Listener = null;
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
        #endregion
    }
}
