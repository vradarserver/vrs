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
using System.Net.Sockets;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using Test.VirtualRadar.Library.Network;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class ListenerTests
    {
        #region Private Enum - TranslatorType
        /// <summary>
        /// An enumeration of the different translator types involved in each of the ExtractedBytesFormat decoding.
        /// </summary>
        enum TranslatorType
        {
            // Port30003 format translators
            Port30003,

            // ModeS format translators
            ModeS,
            Adsb,
            Raw,

            // Compressed format translators
            Compressed,

            // Aircraft list translators
            AircraftListJson,
        }
        #endregion

        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalClassFactory;

        private IListener _Listener;
        private MockConnector<IConnector, IConnection> _Connector;
        private ClockMock _Clock;
        private MockMessageBytesExtractor _BytesExtractor;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private Mock<IModeSParity> _ModeSParity;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtEvent;
        private EventRecorder<EventArgs> _ConnectionStateChangedEvent;
        private EventRecorder<ModeSMessageEventArgs> _ModeSMessageReceivedEvent;
        private EventRecorder<BaseStationMessageEventArgs> _Port30003MessageReceivedEvent;
        private EventRecorder<EventArgs> _SourceChangedEvent;
        private Mock<IBaseStationMessageTranslator> _Port30003Translator;
        private Mock<IModeSTranslator> _ModeSTranslator;
        private Mock<IAdsbTranslator> _AdsbTranslator;
        private Mock<IRawMessageTranslator> _RawMessageTranslator;
        private ModeSMessage _ModeSMessage;
        private AdsbMessage _AdsbMessage;
        private BaseStationMessage _Port30003Message;
        private Mock<IStatistics> _Statistics;
        private Mock<IBaseStationMessageCompressor> _Compressor;
        private Mock<IAircraftListJsonMessageConverter> _JsonConverter;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.TakeSnapshot();

            _Statistics = StatisticsHelper.CreateLockableStatistics();

            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsTest).Returns(true);

            _Port30003Translator = TestUtilities.CreateMockImplementation<IBaseStationMessageTranslator>();
            _ModeSTranslator = TestUtilities.CreateMockImplementation<IModeSTranslator>();
            _AdsbTranslator = TestUtilities.CreateMockImplementation<IAdsbTranslator>();
            _RawMessageTranslator = TestUtilities.CreateMockInstance<IRawMessageTranslator>();
            _ModeSParity = TestUtilities.CreateMockImplementation<IModeSParity>();
            _Compressor = TestUtilities.CreateMockImplementation<IBaseStationMessageCompressor>();
            _JsonConverter = TestUtilities.CreateMockImplementation<IAircraftListJsonMessageConverter>();

            _ModeSMessage = new ModeSMessage();
            _AdsbMessage = new AdsbMessage(_ModeSMessage);
            _Port30003Message = new BaseStationMessage();
            _Port30003Translator.Setup(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>())).Returns(_Port30003Message);
            _AdsbTranslator.Setup(r => r.Translate(It.IsAny<ModeSMessage>())).Returns(_AdsbMessage);
            _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>())).Returns(_ModeSMessage);
            _RawMessageTranslator.Setup(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>())).Returns(_Port30003Message);
            _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Returns(_Port30003Message);
            _JsonConverter.Setup(r => r.ConvertIntoBaseStationMessages(It.IsAny<AircraftListJson>())).Returns(new List<BaseStationMessage>() { _Port30003Message });

            _Listener = Factory.Singleton.Resolve<IListener>();
            _Connector = new MockConnector<IConnector, IConnection>();
            _BytesExtractor = new MockMessageBytesExtractor();

            _ExceptionCaughtEvent = new EventRecorder<EventArgs<Exception>>();
            _ConnectionStateChangedEvent = new EventRecorder<EventArgs>();
            _ModeSMessageReceivedEvent = new EventRecorder<ModeSMessageEventArgs>();
            _Port30003MessageReceivedEvent = new EventRecorder<BaseStationMessageEventArgs>();
            _SourceChangedEvent = new EventRecorder<EventArgs>();

            _Listener.ConnectionStateChanged += _ConnectionStateChangedEvent.Handler;
            _Listener.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            _Listener.ModeSMessageReceived += _ModeSMessageReceivedEvent.Handler;
            _Listener.Port30003MessageReceived += _Port30003MessageReceivedEvent.Handler;
            _Listener.SourceChanged += _SourceChangedEvent.Handler;

            _ExceptionCaughtEvent.EventRaised += DefaultExceptionCaughtHandler;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalClassFactory);

            _Listener.Dispose();
        }
        #endregion

        #region ChangeSourceAndConnect
        private void ChangeSourceAndConnect(bool reconnect = false, Mock<IConnector> connector = null, Mock<IMessageBytesExtractor> bytesExtractor = null, Mock<IRawMessageTranslator> translator = null)
        {
            _Listener.ChangeSource(connector == null ? _Connector.Object : connector.Object,
                                   bytesExtractor == null ? _BytesExtractor.Object : bytesExtractor.Object,
                                   translator == null ? _RawMessageTranslator.Object : translator.Object);
            _Listener.Connect();
        }
        #endregion

        #region DefaultExceptionCaughtHandler
        /// <summary>
        /// Default handler for exceptions raised on a background thread by the listener.
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

        #region DoForEveryFormatAndTranslator, MakeFormatTranslatorThrowException, MakeMessageReceivedHandlerThrowException
        private void DoForEveryFormat(Action<ExtractedBytesFormat, string> action)
        {
            foreach(ExtractedBytesFormat format in Enum.GetValues(typeof(ExtractedBytesFormat))) {
                TestCleanup();
                TestInitialise();

                action(format, String.Format("Format {0}", format));
            }
        }

        private void DoForEveryFormatAndTranslator(Action<ExtractedBytesFormat, TranslatorType, string> action)
        {
            foreach(ExtractedBytesFormat format in Enum.GetValues(typeof(ExtractedBytesFormat))) {
                TranslatorType[] translators;
                switch(format) {
                    case ExtractedBytesFormat.Port30003:        translators = new TranslatorType[] { TranslatorType.Port30003 }; break;
                    case ExtractedBytesFormat.ModeS:            translators = new TranslatorType[] { TranslatorType.ModeS, TranslatorType.Adsb, TranslatorType.Raw }; break;
                    case ExtractedBytesFormat.Compressed:       translators = new TranslatorType[] { TranslatorType.Compressed }; break;
                    case ExtractedBytesFormat.AircraftListJson: translators = new TranslatorType[] { TranslatorType.AircraftListJson }; break;
                    case ExtractedBytesFormat.None:             continue;
                    default:                                    throw new NotImplementedException();
                }

                foreach(var translatorType in translators) {
                    TestCleanup();
                    TestInitialise();

                    var failMessage = String.Format("Format {0}, Translator {1}", format, translatorType);
                    action(format, translatorType, failMessage);
                }
            }
        }

        private InvalidOperationException MakeFormatTranslatorThrowException(TranslatorType translatorType)
        {
            var exception = new InvalidOperationException();
            switch(translatorType) {
                case TranslatorType.Adsb:               _AdsbTranslator.Setup(r => r.Translate(It.IsAny<ModeSMessage>())).Throws(exception); break;
                case TranslatorType.ModeS:              _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>())).Throws(exception); break;
                case TranslatorType.Port30003:          _Port30003Translator.Setup(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>())).Throws(exception); break;
                case TranslatorType.Raw:                _RawMessageTranslator.Setup(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>())).Throws(exception); break;
                case TranslatorType.Compressed:         _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Throws(exception); break;
                case TranslatorType.AircraftListJson:   _JsonConverter.Setup(r => r.ConvertIntoBaseStationMessages(It.IsAny<AircraftListJson>())).Throws(exception); break;
                default:
                    throw new NotImplementedException();
            }

            return exception;
        }

        private InvalidOperationException MakeMessageReceivedHandlerThrowException(TranslatorType translatorType)
        {
            var exception = new InvalidOperationException();
            switch(translatorType) {
                case TranslatorType.Adsb:
                case TranslatorType.ModeS:      _ModeSMessageReceivedEvent.EventRaised += (s, a) => { throw exception; }; break;
                case TranslatorType.Port30003:
                case TranslatorType.Compressed:
                case TranslatorType.AircraftListJson:
                case TranslatorType.Raw:        _Port30003MessageReceivedEvent.EventRaised += (s, a) => { throw exception; }; break;
                default:
                    throw new NotImplementedException();
            }

            return exception;
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void Listener_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            CommonListenerTests.CommonListener_Constructor_Initialises_To_Known_State_And_Properties_Work(_Listener);
            Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus);

            Assert.AreSame(_Statistics.Object, _Listener.Statistics);
            _Statistics.Verify(r => r.Initialise(), Times.Once());

            Assert.AreSame(_Statistics.Object, _ModeSTranslator.Object.Statistics);
            Assert.AreSame(_Statistics.Object, _AdsbTranslator.Object.Statistics);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void Listener_Dispose_Calls_Connector_CloseConnection()
        {
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            _Listener.Dispose();
            _Connector.Verify(p => p.CloseConnection(), Times.Once());
        }

        [TestMethod]
        public void Listener_Dispose_Calls_Connector_Dispose()
        {
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            _Listener.Dispose();
            _Connector.Verify(p => p.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Listener_Dispose_Disposes_Of_RawMessageTranslator()
        {
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            _Listener.Dispose();
            _RawMessageTranslator.Verify(r => r.Dispose(), Times.Once());
        }
        #endregion

        #region ChangeSource
        [TestMethod]
        public void Listener_ChangeSource_Can_Change_Properties()
        {
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);

            Assert.AreSame(_Connector.Object, _Listener.Connector);
            Assert.AreSame(_Listener.BytesExtractor, _BytesExtractor.Object);
            Assert.AreSame(_RawMessageTranslator.Object, _Listener.RawMessageTranslator);
        }

        [TestMethod]
        public void Listener_ChangeSource_Raises_SourceChanged()
        {
            _SourceChangedEvent.EventRaised += (s, a) => {
                Assert.AreSame(_Connector.Object, _Listener.Connector);
                Assert.AreSame(_BytesExtractor.Object, _Listener.BytesExtractor);
                Assert.AreSame(_RawMessageTranslator.Object, _Listener.RawMessageTranslator);
            };

            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);

            Assert.AreEqual(1, _SourceChangedEvent.CallCount);
            Assert.AreSame(_Listener, _SourceChangedEvent.Sender);
            Assert.AreNotEqual(null, _SourceChangedEvent.Args);
        }

        [TestMethod]
        public void Listener_ChangeSource_Sets_Statistics_Property_On_RawMessageTranslator()
        {
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);

            Assert.AreSame(_Statistics.Object, _RawMessageTranslator.Object.Statistics);
        }

        [TestMethod]
        public void Listener_ChangeSource_Resets_TotalMessages_Counter()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("A");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003);

            ChangeSourceAndConnect();
            _Listener.ChangeSource(new MockConnector<IConnector, IConnection>().Object, new MockMessageBytesExtractor().Object, new Mock<IRawMessageTranslator>().Object);

            Assert.AreEqual(0, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_ChangeSource_Resets_TotalBadMessages_Counter()
        {
            _Listener.IgnoreBadMessages = false;
            RemoveDefaultExceptionCaughtHandler();
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("A");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003);
            MakeFormatTranslatorThrowException(TranslatorType.Port30003);

            ChangeSourceAndConnect();
            _Listener.ChangeSource(new MockConnector<IConnector, IConnection>().Object, new MockMessageBytesExtractor().Object, new Mock<IRawMessageTranslator>().Object);

            Assert.AreEqual(0, _Listener.TotalBadMessages);
        }

        [TestMethod]
        public void Listener_ChangeSource_Is_Not_Raised_If_Nothing_Changes()
        {
            var newConnector = new MockConnector<IConnector, IConnection>();
            var newExtractor = new MockMessageBytesExtractor();
            var newRawMessageTranslator = new Mock<IRawMessageTranslator>(MockBehavior.Default) { DefaultValue = DefaultValue.Mock };

            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            Assert.AreEqual(1, _SourceChangedEvent.CallCount);

            _Listener.ChangeSource(newConnector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            Assert.AreEqual(2, _SourceChangedEvent.CallCount);

            _Listener.ChangeSource(newConnector.Object, newExtractor.Object, _RawMessageTranslator.Object);
            Assert.AreEqual(3, _SourceChangedEvent.CallCount);

            _Listener.ChangeSource(newConnector.Object, newExtractor.Object, newRawMessageTranslator.Object);
            Assert.AreEqual(4, _SourceChangedEvent.CallCount);

            _Listener.ChangeSource(newConnector.Object, newExtractor.Object, newRawMessageTranslator.Object);
            Assert.AreEqual(4, _SourceChangedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_ChangeSource_Disposes_Of_Existing_RawMessageTranslator()
        {
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            _RawMessageTranslator.Verify(r => r.Dispose(), Times.Never());

            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, new Mock<IRawMessageTranslator>().Object);
            _RawMessageTranslator.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Listener_ChangeSource_Does_Not_Dispose_Of_Existing_RawMessageTranslator_If_It_Has_Not_Changed()
        {
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            _Listener.ChangeSource(new MockConnector<IConnector, IConnection>().Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            _RawMessageTranslator.Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void Listener_ChangeSource_Disposes_Of_Existing_Connector()
        {
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            _Connector.Verify(r => r.Dispose(), Times.Never());

            _Listener.ChangeSource(new MockConnector<IConnector, IConnection>().Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            _Connector.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Listener_ChangeSource_Does_Not_Dispose_Of_Existing_Connector_If_It_Has_Not_Changed()
        {
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, new Mock<IRawMessageTranslator>().Object);
            _Connector.Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void Listener_ChangeSource_Switches_Connector_To_Single_Connection_Mode()
        {
            _Connector.Object.IsSingleConnection = false;
            _Listener.ChangeSource(_Connector.Object, _BytesExtractor.Object, _RawMessageTranslator.Object);
            Assert.AreEqual(true, _Connector.Object.IsSingleConnection);
        }
        #endregion

        #region Connect - Basics
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Listener_Connect_Throws_If_ChangeSource_Never_Called()
        {
            _Listener.Connect();
        }

        [TestMethod]
        public void Listener_Connect_Calls_EstablishConnection()
        {
            ChangeSourceAndConnect();
            _Connector.Verify(p => p.EstablishConnection(), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Call_EstablishConnection_After_Disposed()
        {
            _Listener.Dispose();

            ChangeSourceAndConnect();
            _Connector.Verify(p => p.EstablishConnection(), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Sets_ConnectionStatus_To_Connecting_When_Calling_EstablishConnection()
        {
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.AreEqual(ConnectionStatus.Connecting, _Listener.ConnectionStatus);
            };

            ChangeSourceAndConnect(false, null, null, null);

            Assert.AreEqual(1, _ConnectionStateChangedEvent.CallCount);
            Assert.AreSame(_Listener, _ConnectionStateChangedEvent.Sender);
        }

        [TestMethod]
        public void Listener_Connect_Sets_ConnectionStatus_To_Connected_When_Connection_Established()
        {
            _Connector.ConfigureForConnect();

            int connectionChangedCount = 0;
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                if(++connectionChangedCount == 2) {
                    Assert.AreEqual(ConnectionStatus.Connected, _Listener.ConnectionStatus);
                }
            };

            ChangeSourceAndConnect();

            Assert.AreEqual(2, _ConnectionStateChangedEvent.CallCount);
            Assert.AreSame(_Listener, _ConnectionStateChangedEvent.Sender);
        }

        [TestMethod]
        public void Listener_Connect_Sets_ConnectionTime_In_Statistics_When_Connection_Established()
        {
            _Connector.ConfigureForConnect();

            DateTime now = new DateTime(2012, 11, 10, 9, 8, 7, 6);
            _Clock.UtcNowValue = now;

            int connectionChangedCount = 0;
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                switch(++connectionChangedCount) {
                    case 1: Assert.IsNull(_Statistics.Object.ConnectionTimeUtc); break;
                    case 2: Assert.AreEqual(now, _Statistics.Object.ConnectionTimeUtc); break;
                }
            };

            ChangeSourceAndConnect();

            Assert.AreEqual(2, _ConnectionStateChangedEvent.CallCount);
            Assert.AreSame(_Listener, _ConnectionStateChangedEvent.Sender);
        }

        [TestMethod]
        public void Listener_Connect_Begins_Reading_Content_From_Connection_When_Connection_Established()
        {
            _Connector.ConfigureForConnect();

            ChangeSourceAndConnect();

            _Connector.Verify(p => p.Read(It.IsAny<byte[]>(), It.IsAny<ConnectionReadDelegate>()), Times.AtLeastOnce());
        }
        #endregion

        #region Connect - General Message Processing
        [TestMethod]
        public void Listener_Connect_Raises_RawBytesReceived_When_Bytes_Are_Received()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream(bytes, 3);

            var eventRecorder = new EventRecorder<EventArgs<byte[]>>();
            _Listener.RawBytesReceived += eventRecorder.Handler;

            ChangeSourceAndConnect();

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreSame(_Listener, eventRecorder.Sender);
            Assert.IsTrue(new byte[] { 0x01, 0x02, 0x03}.SequenceEqual(eventRecorder.Args.Value));
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_Bytes_Are_Received()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream(new byte[] { 0x01, 0x02 }, subsequentReads: new List<IEnumerable<byte>>() { { new byte[] { 0x03, 0x04, 0x05 } } });
            _BytesExtractor.Setup(r => r.BufferSize).Returns(12);

            ChangeSourceAndConnect();

            Assert.AreEqual(5, _Statistics.Object.BytesReceived);
            Assert.AreEqual(12, _Statistics.Object.CurrentBufferSize);
        }

        [TestMethod]
        public void Listener_Connect_Raises_ModeSBytesReceived_When_BytesExtractor_Extracts_ModeS_Message()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            var extractedBytes = _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }, 1, 7, false, false);

            var eventRecorder = new EventRecorder<EventArgs<ExtractedBytes>>();
            _Listener.ModeSBytesReceived += eventRecorder.Handler;

            ChangeSourceAndConnect();

            Assert.AreEqual(1, eventRecorder.CallCount);
            Assert.AreSame(_Listener, eventRecorder.Sender);
            Assert.AreEqual(extractedBytes, eventRecorder.Args.Value);
            Assert.AreNotSame(extractedBytes, eventRecorder.Args.Value);  // must be a clone, not the original - the original can be reused
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Raise_ModeSBytesReceived_When_BytesExtractor_Extracts_Bad_Checksum_Message()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            var extractedBytes = _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }, 1, 7, true, false);

            var eventRecorder = new EventRecorder<EventArgs<ExtractedBytes>>();
            _Listener.ModeSBytesReceived += eventRecorder.Handler;

            ChangeSourceAndConnect();

            Assert.AreEqual(0, eventRecorder.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Raise_ModeSBytesReceived_When_BytesExtractor_Extracts_Port30003_Message()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            var extractedBytes = _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }, 1, 7, false, false);

            var eventRecorder = new EventRecorder<EventArgs<ExtractedBytes>>();
            _Listener.ModeSBytesReceived += eventRecorder.Handler;

            ChangeSourceAndConnect();

            Assert.AreEqual(0, eventRecorder.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Passes_Bytes_Received_To_Extractor()
        {
            var bytes = new List<byte>();
            for(var i = 0;i < 256;++i) {
                bytes.Add((byte)i);
            }

            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream(bytes);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _BytesExtractor.SourceByteArrays.Count);
            Assert.IsTrue(bytes.SequenceEqual(_BytesExtractor.SourceByteArrays[0]));
            _BytesExtractor.Verify(r => r.ExtractMessageBytes(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Passes_Count_Of_Bytes_Read_To_Message_Extractor()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream(new byte[] { 0xf0, 0xe8 }, 1);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _BytesExtractor.SourceByteArrays.Count);
            Assert.AreEqual(1, _BytesExtractor.SourceByteArrays[0].Length);
            Assert.AreEqual(0xf0, _BytesExtractor.SourceByteArrays[0][0]);
        }

        [TestMethod]
        public void Listener_Connect_Passes_Every_Block_Of_Bytes_Read_To_Message_Extractor()
        {
            var bytes1 = new byte[] { 0x01, 0xf0 };
            var bytes2 = new byte[] { 0xef, 0x02 };
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream(bytes1, subsequentReads: new List<IEnumerable<byte>>() { bytes2 });

            ChangeSourceAndConnect();

            Assert.AreEqual(2, _BytesExtractor.SourceByteArrays.Count);
            Assert.IsTrue(bytes1.SequenceEqual(_BytesExtractor.SourceByteArrays[0]));
            Assert.IsTrue(bytes2.SequenceEqual(_BytesExtractor.SourceByteArrays[1]));
        }
        #endregion

        #region Connect - Port30003 Message Processing
        [TestMethod]
        public void Listener_Connect_Passes_Port30003_Messages_To_Port30003_Message_Translator()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "ABC123");

            ChangeSourceAndConnect();

            _Port30003Translator.Verify(r => r.Translate("ABC123", It.IsAny<int?>()), Times.Once());
            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()), Times.Never());
            _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Never());
            _RawMessageTranslator.Verify(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>()), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Honours_Offset_And_Length_When_Translating_Extracted_Bytes_To_Port30003_Message_Strings_For_Translation()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "-ABC-", 1, 3, false, false);

            ChangeSourceAndConnect();

            _Port30003Translator.Verify(r => r.Translate("ABC", null), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Ignores_Extracted_Parity_On_Port30003_Messages()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "ABC123", false, true);

            ChangeSourceAndConnect();

            _Port30003Translator.Verify(r => r.Translate("ABC123", null), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Uses_Extracted_SignalLevel_On_Port30003_Messages()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "ABC123").SignalLevel = 123;

            ChangeSourceAndConnect();

            _Port30003Translator.Verify(r => r.Translate("ABC123", 123), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Raises_Port30003MessageReceived_With_Message_From_Translator()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreSame(_Listener, _Port30003MessageReceivedEvent.Sender);
            Assert.AreSame(_Port30003Message, _Port30003MessageReceivedEvent.Args.Message);

            Assert.AreEqual(0, _ModeSMessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Copies_ReceiverId_To_Port30003Messages()
        {
            _Listener.ReceiverId = 1234;
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");

            ChangeSourceAndConnect();

            Assert.AreEqual(1234, _Port30003MessageReceivedEvent.Args.Message.ReceiverId);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Raise_Port30003MessageReceived_When_Translator_Returns_Null()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");
            _Port30003Translator.Setup(r => r.Translate("A", null)).Returns((BaseStationMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreEqual(0, _ModeSMessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Raises_Port30003MessageReceived_For_Every_Extracted_Packet()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "B");

            ChangeSourceAndConnect();

            _Port30003Translator.Verify(r => r.Translate("A", null), Times.Once());
            _Port30003Translator.Verify(r => r.Translate("B", null), Times.Once());

            Assert.AreEqual(2, _Port30003MessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_When_Port30003_Message_Received()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_Port30003_Message_Received()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Statistics.Object.BaseStationMessagesReceived);
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_Bad_Port30003_Message_Received()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");

            var exception = MakeFormatTranslatorThrowException(TranslatorType.Port30003);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Statistics.Object.BaseStationMessagesReceived);
            Assert.AreEqual(1, _Statistics.Object.BaseStationBadFormatMessagesReceived);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_For_Every_Message_In_A_Received_Packet_Of_Port30003_Messages()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "B");

            ChangeSourceAndConnect();

            Assert.AreEqual(2, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Increment_TotalMessages_When_Port30003Translator_Returns_Null()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "B");
            _Port30003Translator.Setup(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>())).Returns((BaseStationMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Listener.TotalMessages);
        }
        #endregion

        #region Connect - ModeS Message Processing
        [TestMethod]
        public void Listener_Connect_Passes_ModeS_Messages_To_ModeS_Message_Translators()
        {
            var packetBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            var extractedBytes = _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, packetBytes, 1, 7, false, false);
            extractedBytes.SignalLevel = 123;
            extractedBytes.IsMlat = true;

            byte[] bytesPassedToTranslator = null;
            int offsetPassedToTranslator = 0;
            int? signalLevelPassedToTranslator = null;
            bool? isMlatPassedToTranslator = null;
            _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Callback((byte[] bytes, int offset, int? signalLevel, bool isMlat) => {
                bytesPassedToTranslator = bytes;
                offsetPassedToTranslator = offset;
                signalLevelPassedToTranslator = signalLevel;
                isMlatPassedToTranslator = isMlat;
            })
            .Returns(_ModeSMessage);

            ChangeSourceAndConnect();

            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()), Times.Once());
            Assert.AreSame(packetBytes, bytesPassedToTranslator);
            Assert.AreEqual(1, offsetPassedToTranslator);
            Assert.AreEqual(123, signalLevelPassedToTranslator);
            Assert.AreEqual(true, isMlatPassedToTranslator);

            _Port30003Translator.Verify(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>()), Times.Never());
            _AdsbTranslator.Verify(r => r.Translate(_ModeSMessage), Times.Once());
            _RawMessageTranslator.Verify(r => r.Translate(_Clock.UtcNowValue, _ModeSMessage, _AdsbMessage), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Pass_ModeS_Messages_To_ModeS_Translator_If_Length_Is_Anything_Other_Than_7_Or_14()
        {
            for(var length = 0;length < 20;++length) {
                TestCleanup();
                TestInitialise();

                var extractedBytes = new byte[length];
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("a");
                _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, extractedBytes);

                ChangeSourceAndConnect();

                string failMessage = String.Format("For length of {0}", length);
                if(length != 7 && length != 14) {
                    _Port30003Translator.Verify(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>()), Times.Never(), failMessage);
                    _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()), Times.Never(), failMessage);
                    _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Never(), failMessage);
                    _RawMessageTranslator.Verify(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>()), Times.Never(), failMessage);
                } else {
                    _Port30003Translator.Verify(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>()), Times.Never(), failMessage);
                    _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()), Times.Once(), failMessage);
                    _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Once(), failMessage);
                    _RawMessageTranslator.Verify(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>()), Times.Once(), failMessage);
                }
            }
        }

        [TestMethod]
        public void Listener_Connect_Strips_Parity_From_ModeSMessages_When_Indicated()
        {
            _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);

            var bytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, bytes, 1, 7, false, true);

            ChangeSourceAndConnect();

            _ModeSParity.Verify(r => r.StripParity(bytes, 1, 7), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Strip_Parity_From_ModeSMessages_When_Indicated()
        {
            _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);

            var bytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, bytes, 1, 7, false, false);

            ChangeSourceAndConnect();

            _ModeSParity.Verify(r => r.StripParity(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_If_ModeS_Message_Has_NonNull_PI()
        {
            var bytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };

            foreach(var piValue in new int?[] { null, 0, 99 }) {
                TestCleanup();
                TestInitialise();

                _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("a");
                _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, bytes, 1, 7, false, true);
                _ModeSMessage.ParityInterrogatorIdentifier = piValue;

                ChangeSourceAndConnect();

                long expected = piValue == null ? 0L : 1L;
                Assert.AreEqual(expected, _Statistics.Object.ModeSWithPIField);
            }
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_If_ModeS_Message_Has_NonZero_PI()
        {
            var bytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };

            foreach(var piValue in new int?[] { null, 0, 99 }) {
                TestCleanup();
                TestInitialise();

                _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("a");
                _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, bytes, 1, 7, false, true);
                _ModeSMessage.ParityInterrogatorIdentifier = piValue;

                ChangeSourceAndConnect();

                long expected = piValue == null || piValue == 0 ? 0L : 1L;
                Assert.AreEqual(expected, _Statistics.Object.ModeSWithBadParityPIField);
            }
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_If_ModeS_Message_Does_Not_Contain_Adsb_Payload()
        {
            var extractedBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            foreach(var hasAdsbPayload in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("a");
                _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, extractedBytes, 1, 7, false, false);
                if(!hasAdsbPayload) _AdsbTranslator.Setup(r => r.Translate(It.IsAny<ModeSMessage>())).Returns((AdsbMessage)null);

                ChangeSourceAndConnect();

                Assert.AreEqual(hasAdsbPayload ? 0L : 1L, _Statistics.Object.ModeSNotAdsbCount);
            }
        }

        [TestMethod]
        public void Listener_Connect_Raises_ModeSMessageReceived_When_ModeS_Message_Received()
        {
            _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _ModeSMessageReceivedEvent.CallCount);
            Assert.AreSame(_Listener, _ModeSMessageReceivedEvent.Sender);
            Assert.AreEqual(_Clock.UtcNowValue, _ModeSMessageReceivedEvent.Args.ReceivedUtc);
            Assert.AreSame(_ModeSMessage, _ModeSMessageReceivedEvent.Args.ModeSMessage);
            Assert.AreSame(_AdsbMessage, _ModeSMessageReceivedEvent.Args.AdsbMessage);
        }

        [TestMethod]
        public void Listener_Connect_Raises_Port30003MessageReceived_With_Translated_ModeS_Message()
        {
            _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);

            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreSame(_Listener, _Port30003MessageReceivedEvent.Sender);
            Assert.AreSame(_Port30003Message, _Port30003MessageReceivedEvent.Args.Message);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Translate_Null_ModeS_Messages()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>())).Returns((ModeSMessage)null);

            ChangeSourceAndConnect();

            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()), Times.Once());
            _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Never());
            _RawMessageTranslator.Verify(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>()), Times.Never());
            Assert.AreEqual(0, _ModeSMessageReceivedEvent.CallCount);
            Assert.AreEqual(0, _Port30003MessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Will_Translate_Null_Adsb_Messages()
        {
            _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);

            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _AdsbTranslator.Setup(r => r.Translate(It.IsAny<ModeSMessage>())).Returns((AdsbMessage)null);

            ChangeSourceAndConnect();

            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()), Times.Once());
            _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Once());
            _RawMessageTranslator.Verify(r => r.Translate(_Clock.UtcNowValue, _ModeSMessage, null), Times.Once());

            Assert.AreEqual(1, _ModeSMessageReceivedEvent.CallCount);
            Assert.AreEqual(_Clock.UtcNowValue, _ModeSMessageReceivedEvent.Args.ReceivedUtc);
            Assert.AreSame(_ModeSMessage, _ModeSMessageReceivedEvent.Args.ModeSMessage);
            Assert.IsNull(_ModeSMessageReceivedEvent.Args.AdsbMessage);

            Assert.AreEqual(1, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreSame(_Listener, _Port30003MessageReceivedEvent.Sender);
            Assert.AreSame(_Port30003Message, _Port30003MessageReceivedEvent.Args.Message);
        }

        [TestMethod]
        public void Listener_Connect_Will_Not_Raise_Port30003MessageReceived_When_Raw_Translator_Returns_Null()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _RawMessageTranslator.Setup(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>())).Returns((BaseStationMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Port30003MessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_When_ModeS_Message_Received()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_For_Every_Message_In_A_Received_Packet_Of_ModeS_Messages()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);

            ChangeSourceAndConnect();

            Assert.AreEqual(2, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Increment_TotalMessages_When_ModeSTranslator_Returns_Null()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>())).Returns((ModeSMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Increments_TotalMessages_When_AdsbTranslator_Returns_Null()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _AdsbTranslator.Setup(r => r.Translate(It.IsAny<ModeSMessage>())).Returns((AdsbMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Increments_TotalMessages_When_RawTranslator_Returns_Null()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _RawMessageTranslator.Setup(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>())).Returns((BaseStationMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }
        #endregion

        #region Connect - Compress Message Processing
        [TestMethod]
        public void Listener_Connect_Passes_Compressed_Messages_Through_BaseStationMesageCompressor()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            var payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, payload, 0, 6, false, false);

            ChangeSourceAndConnect();

            _Compressor.Verify(r => r.Decompress(payload), Times.Once());
            _Port30003Translator.Verify(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>()), Times.Never());
            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<bool>()), Times.Never());
            _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Never());
            _RawMessageTranslator.Verify(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>()), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Honours_Offset_And_Length_When_Translating_Compressed_Bytes()
        {
            var payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
            var expected = new byte[] { 0x02, 0x03, 0x04, 0x05 };
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, payload, 1, 4, false, false);

            ChangeSourceAndConnect();

            _Compressor.Verify(r => r.Decompress(expected), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Raises_Event_With_BaseStationMessage_Built_From_Compressed_Message()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreSame(_Listener, _Port30003MessageReceivedEvent.Sender);
            Assert.AreSame(_Port30003Message, _Port30003MessageReceivedEvent.Args.Message);

            Assert.AreEqual(0, _ModeSMessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Copies_ReceiverId_To_Compressed_Port30003Messages()
        {
            _Listener.ReceiverId = 1234;
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1234, _Port30003MessageReceivedEvent.Args.Message.ReceiverId);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Raise_Port30003MessageReceived_When_Compressor_Returns_Null()
        {
            _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Returns((BaseStationMessage)null);
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreEqual(0, _ModeSMessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Increment_TotalMessages_When_Compressor_Returns_Null()
        {
            _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Returns((BaseStationMessage)null);
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_When_Compressed_Message_Received()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_Compressed_Message_Received()
        {
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Statistics.Object.BaseStationMessagesReceived);
        }

        [TestMethod]
        public void Listener_Connect_Increments_TotalBadMessages_When_Bad_Compressed_Message_Received()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Callback(() => { throw new InvalidCastException(); });
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Listener.TotalMessages);
            Assert.AreEqual(1, _Listener.TotalBadMessages);
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_Bad_Compressed_Message_Received()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Callback(() => { throw new InvalidCastException(); });
            _Connector.ConfigureForConnect();
            _Connector.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Statistics.Object.BaseStationMessagesReceived);
            Assert.AreEqual(1, _Statistics.Object.BaseStationBadFormatMessagesReceived);
        }
        #endregion

        #region Connect - AircraftListJson Message Processing
        [TestMethod]
        public void Listener_Connect_Passes_AircraftListJson_Messages_Through_AircraftListJsonMessageConverter()
        {
            _Connector.ConfigureForConnect();
            var text = @"{""acList"":[]}";
            _Connector.ConfigureForReadStream(text);
            var payload = Encoding.UTF8.GetBytes(text);
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.AircraftListJson, payload, 0, payload.Length, false, false);

            ChangeSourceAndConnect();

            _JsonConverter.Verify(r => r.ConvertIntoBaseStationMessages(It.IsAny<AircraftListJson>()), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Raises_Event_With_BaseStationMessage_Built_From_AircraftListJson_Message()
        {
            _Connector.ConfigureForConnect();
            var text = @"{""acList"":[]}";
            _Connector.ConfigureForReadStream(text);
            var payload = Encoding.UTF8.GetBytes(text);
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.AircraftListJson, payload, 0, payload.Length, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreSame(_Listener, _Port30003MessageReceivedEvent.Sender);
            Assert.AreSame(_Port30003Message, _Port30003MessageReceivedEvent.Args.Message);

            Assert.AreEqual(0, _ModeSMessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Copies_ReceiverId_To_AircraftListJson_Port30003Messages()
        {
            _Listener.ReceiverId = 1234;
            _Connector.ConfigureForConnect();
            var text = @"{""acList"":[]}";
            _Connector.ConfigureForReadStream(text);
            var payload = Encoding.UTF8.GetBytes(text);
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.AircraftListJson, payload, 0, payload.Length, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1234, _Port30003MessageReceivedEvent.Args.Message.ReceiverId);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_When_AircraftListJson_Message_Received()
        {
            _Connector.ConfigureForConnect();
            var text = @"{""acList"":[]}";
            _Connector.ConfigureForReadStream(text);
            var payload = Encoding.UTF8.GetBytes(text);
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.AircraftListJson, payload, 0, payload.Length, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_AircraftListJson_Message_Received()
        {
            _Connector.ConfigureForConnect();
            var text = @"{""acList"":[]}";
            _Connector.ConfigureForReadStream(text);
            var payload = Encoding.UTF8.GetBytes(text);
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.AircraftListJson, payload, 0, payload.Length, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Statistics.Object.BaseStationMessagesReceived);
        }

        [TestMethod]
        public void Listener_Connect_Increments_TotalBadMessages_When_Bad_AircraftListJson_Message_Received()
        {
            RemoveDefaultExceptionCaughtHandler();
            _JsonConverter.Setup(r => r.ConvertIntoBaseStationMessages(It.IsAny<AircraftListJson>())).Callback(() => { throw new InvalidCastException(); });
            _Connector.ConfigureForConnect();
            var text = @"{""acList"":[]}";
            _Connector.ConfigureForReadStream(text);
            var payload = Encoding.UTF8.GetBytes(text);
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.AircraftListJson, payload, 0, payload.Length, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Listener.TotalMessages);
            Assert.AreEqual(1, _Listener.TotalBadMessages);
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_Bad_AircraftListJson_Message_Received()
        {
            RemoveDefaultExceptionCaughtHandler();
            _JsonConverter.Setup(r => r.ConvertIntoBaseStationMessages(It.IsAny<AircraftListJson>())).Callback(() => { throw new InvalidCastException(); });
            _Connector.ConfigureForConnect();
            var text = @"{""acList"":[]}";
            _Connector.ConfigureForReadStream(text);
            var payload = Encoding.UTF8.GetBytes(text);
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.AircraftListJson, payload, 0, payload.Length, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Statistics.Object.BaseStationMessagesReceived);
            Assert.AreEqual(1, _Statistics.Object.BaseStationBadFormatMessagesReceived);
        }
        #endregion

        #region Connect - Packet Checksum Handling
        [TestMethod]
        public void Listener_Connect_Does_Not_Increment_TotalMessages_When_BadChecksum_Packet_Received()
        {
            DoForEveryFormat((format, failMessage) => {
                if(format != ExtractedBytesFormat.None) {
                    RemoveDefaultExceptionCaughtHandler();

                    _Connector.ConfigureForConnect();
                    _Connector.ConfigureForReadStream("a");
                    _BytesExtractor.AddExtractedBytes(format, 7, true, false);

                    ChangeSourceAndConnect();

                    Assert.AreEqual(0, _Listener.TotalMessages, failMessage);
                }
            });
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_BadChecksum_Packet_Received()
        {
            DoForEveryFormat((format, failMessage) => {
                if(format != ExtractedBytesFormat.None) {
                    TestCleanup();
                    TestInitialise();

                    RemoveDefaultExceptionCaughtHandler();

                    _Connector.ConfigureForConnect();
                    _Connector.ConfigureForReadStream("a");
                    _BytesExtractor.AddExtractedBytes(format, 7, true, false);

                    ChangeSourceAndConnect();

                    Assert.AreEqual(1L, _Statistics.Object.FailedChecksumMessages);
                }
            });
        }

        [TestMethod]
        public void Listener_Connect_Increments_BadMessages_When_BadChecksum_Packet_Received_Irrespective_Of_IgnoreBadMessages_Setting()
        {
            DoForEveryFormat((format, failMessagePrefix) => {
                if(format != ExtractedBytesFormat.None) {
                    foreach(var ignoreBadMessages in new bool[] { true, false }) {
                        TestCleanup();
                        TestInitialise();

                        var failMessage = String.Format("{0} ignoreBadMessages {1}", failMessagePrefix, ignoreBadMessages);
                        RemoveDefaultExceptionCaughtHandler();

                        _Listener.IgnoreBadMessages = ignoreBadMessages;
                        _Connector.ConfigureForConnect();
                        _Connector.ConfigureForReadStream("a");
                        _BytesExtractor.AddExtractedBytes(format, 7, true, false);

                        ChangeSourceAndConnect();

                        Assert.AreEqual(1, _Listener.TotalBadMessages, failMessage);
                    }
                }
            });
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Throw_Exception_When_BadChecksum_Packet_Received_Irrespective_Of_IgnoreBadMessages_Setting()
        {
            DoForEveryFormat((format, failMessagePrefix) => {
                if(format != ExtractedBytesFormat.None) {
                    foreach(var ignoreBadMessages in new bool[] { true, false }) {
                        TestCleanup();
                        TestInitialise();

                        var failMessage = String.Format("{0} ignoreBadMessages {1}", failMessagePrefix, ignoreBadMessages);
                        RemoveDefaultExceptionCaughtHandler();

                        _Listener.IgnoreBadMessages = ignoreBadMessages;
                        _Connector.ConfigureForConnect();
                        _Connector.ConfigureForReadStream("a");
                        _BytesExtractor.AddExtractedBytes(format, 7, true, false);

                        ChangeSourceAndConnect();

                        Assert.AreEqual(0, _ExceptionCaughtEvent.CallCount, failMessage);
                        Assert.AreEqual(ConnectionStatus.Connected, _Listener.ConnectionStatus);
                    }
                }
            });
        }
        #endregion

        #region Connect - Exception Handling
        [TestMethod]
        public void Listener_Connect_Raises_ExceptionCaught_If_Translator_Throws_Exception_When_IgnoreBadMessages_Is_False()
        {
            DoForEveryFormatAndTranslator((format, translatorType, failMessage) => {
                _Listener.IgnoreBadMessages = false;
                RemoveDefaultExceptionCaughtHandler();
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("A");
                _BytesExtractor.AddExtractedBytes(format, 7);

                var exception = MakeFormatTranslatorThrowException(translatorType);

                ChangeSourceAndConnect();

                Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount, failMessage);
                Assert.AreSame(_Listener, _ExceptionCaughtEvent.Sender, failMessage);
                Assert.AreSame(exception, _ExceptionCaughtEvent.Args.Value, failMessage);
                Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus, failMessage);
            });
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Raise_ExceptionCaught_If_Translator_Throws_Exception_When_IgnoreBadMessages_Is_True()
        {
            DoForEveryFormatAndTranslator((format, translatorType, failMessage) => {
                _Listener.IgnoreBadMessages = true;
                RemoveDefaultExceptionCaughtHandler();
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("A");
                _BytesExtractor.AddExtractedBytes(format, 7);

                var exception = MakeFormatTranslatorThrowException(translatorType);

                ChangeSourceAndConnect();

                if(translatorType != TranslatorType.Raw) {
                    Assert.AreEqual(0, _ExceptionCaughtEvent.CallCount, failMessage);
                    Assert.AreEqual(ConnectionStatus.Connected, _Listener.ConnectionStatus, failMessage);
                } else {
                    // If the Mode-S and ADS-B messages decoded correctly then any exception in the raw message translator
                    // is something I always want to know about - there should never be any combination of Mode-S and ADS-B
                    // message that makes it throw an exception. So in this case I want to still stop the listener and report
                    // the exception.
                    Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount, failMessage);
                    Assert.AreSame(_Listener, _ExceptionCaughtEvent.Sender, failMessage);
                    Assert.AreSame(exception, _ExceptionCaughtEvent.Args.Value, failMessage);
                    Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus, failMessage);
                }
            });
        }

        [TestMethod]
        public void Listener_Connect_Raises_ExceptionCaught_If_MessageReceived_Event_Handler_Throws_Exception_When_IgnoreBadMessages_Is_True()
        {
            DoForEveryFormatAndTranslator((format, translatorType, failMessage) => {
                _Listener.IgnoreBadMessages = true;
                RemoveDefaultExceptionCaughtHandler();
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("A");
                _BytesExtractor.AddExtractedBytes(format, 7);

                var exception = MakeMessageReceivedHandlerThrowException(translatorType);

                ChangeSourceAndConnect();

                Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount, failMessage);
                Assert.AreSame(_Listener, _ExceptionCaughtEvent.Sender, failMessage);
                Assert.AreSame(exception, _ExceptionCaughtEvent.Args.Value, failMessage);
                Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus, failMessage);
            });
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Raise_MessageReceived_If_Translator_Throws_Exception_When_IgnoreBadMessages_Is_True()
        {
            DoForEveryFormatAndTranslator((format, translatorType, failMessage) => {
                _Listener.IgnoreBadMessages = true;
                RemoveDefaultExceptionCaughtHandler();
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("A");
                _BytesExtractor.AddExtractedBytes(format, 7);

                var exception = MakeFormatTranslatorThrowException(translatorType);

                ChangeSourceAndConnect();

                Assert.AreEqual(0, _Port30003MessageReceivedEvent.CallCount, failMessage);
                Assert.AreEqual(translatorType == TranslatorType.Raw ? 1 : 0, _ModeSMessageReceivedEvent.CallCount, failMessage); // The Mode-S message will have been raised before the raw translator runs
            });
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Increment_TotalMessages_If_Translator_Throws_Exception()
        {
            DoForEveryFormatAndTranslator((format, translatorType, failMessage) => {
                _Listener.IgnoreBadMessages = true;
                RemoveDefaultExceptionCaughtHandler();
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("A");
                _BytesExtractor.AddExtractedBytes(format, 7);

                var exception = MakeFormatTranslatorThrowException(translatorType);

                ChangeSourceAndConnect();

                Assert.AreEqual(translatorType == TranslatorType.Raw ? 1 : 0, _Listener.TotalMessages, failMessage); // If the message gets as far as the raw translator then it was actually a good message...
            });
        }

        [TestMethod]
        public void Listener_Connect_Increments_TotalBadMessages_If_Translator_Throws_Exception_When_IgnoreBadMessage_Is_True()
        {
            DoForEveryFormatAndTranslator((format, translatorType, failMessage) => {
                _Listener.IgnoreBadMessages = true;
                RemoveDefaultExceptionCaughtHandler();
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("A");
                _BytesExtractor.AddExtractedBytes(format, 7);

                var exception = MakeFormatTranslatorThrowException(translatorType);

                ChangeSourceAndConnect();

                Assert.AreEqual(translatorType == TranslatorType.Raw ? 0 : 1, _Listener.TotalBadMessages, failMessage);  // Exceptions in the raw translator are not an indication of a bad message, they're an indication of a bug
            });
        }

        [TestMethod]
        public void Listener_Connect_Increments_TotalBadMessages_If_Translator_Throws_Exception_When_IgnoreBadMessage_Is_False()
        {
            DoForEveryFormatAndTranslator((format, translatorType, failMessage) => {
                _Listener.IgnoreBadMessages = false;
                RemoveDefaultExceptionCaughtHandler();
                _Connector.ConfigureForConnect();
                _Connector.ConfigureForReadStream("A");
                _BytesExtractor.AddExtractedBytes(format, 7);

                var exception = MakeFormatTranslatorThrowException(translatorType);

                ChangeSourceAndConnect();

                Assert.AreEqual(translatorType == TranslatorType.Raw ? 0 : 1, _Listener.TotalBadMessages, failMessage);  // Exceptions in the raw translator are not an indication of a bad message, they're an indication of a bug
            });
        }
        #endregion

        #region Disconnect
        [TestMethod]
        public void Listener_Disconnect_Closes_Provider()
        {
            _Connector.ConfigureForConnect();

            ChangeSourceAndConnect();
            _Listener.Disconnect();

            _Connector.Verify(p => p.CloseConnection(), Times.Once());
        }

        [TestMethod]
        public void Listener_Disconnect_Sets_Connection_Status()
        {
            _Connector.ConfigureForConnect();
            int callCount = 0;
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                if(++callCount == 3) Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus);
            };

            ChangeSourceAndConnect();
            _Listener.Disconnect();

            Assert.AreEqual(3, _ConnectionStateChangedEvent.CallCount);
        }
        #endregion
    }
}
