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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Adsb;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Interface.Settings;

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
        }
        #endregion

        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalClassFactory;

        private IListener _Listener;
        private MockListenerProvider _Provider;
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
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Mock<IHeartbeatService> _HeartbeatService;
        private Mock<ILog> _Log;
        private List<string> _LogMessages;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.TakeSnapshot();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();
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
            _ModeSMessage = new ModeSMessage();
            _AdsbMessage = new AdsbMessage(_ModeSMessage);
            _Port30003Message = new BaseStationMessage();
            _Port30003Translator.Setup(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>())).Returns(_Port30003Message);
            _AdsbTranslator.Setup(r => r.Translate(It.IsAny<ModeSMessage>())).Returns(_AdsbMessage);
            _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>())).Returns(_ModeSMessage);
            _RawMessageTranslator.Setup(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>())).Returns(_Port30003Message);
            _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Returns(_Port30003Message);

            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _LogMessages = new List<string>();
            _Log.Setup(r => r.WriteLine(It.IsAny<string>())).Callback((string message) => _LogMessages.Add(message));
            _Log.Setup(r => r.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback((string format, object[] args) => _LogMessages.Add(String.Format(format, args)));

            _Listener = Factory.Singleton.Resolve<IListener>();
            _Provider = new MockListenerProvider();
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
        private void ChangeSourceAndConnect(bool reconnect = false, Mock<IListenerProvider> provider = null, Mock<IMessageBytesExtractor> bytesExtractor = null, Mock<IRawMessageTranslator> translator = null, bool connectAutoReconnect = false)
        {
            _Listener.ChangeSource(provider == null ? _Provider.Object : provider.Object,
                                   bytesExtractor == null ? _BytesExtractor.Object : bytesExtractor.Object,
                                   translator == null ? _RawMessageTranslator.Object : translator.Object,
                                   reconnect);
            _Listener.Connect(connectAutoReconnect);
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
                    case ExtractedBytesFormat.Port30003:    translators = new TranslatorType[] { TranslatorType.Port30003 }; break;
                    case ExtractedBytesFormat.ModeS:        translators = new TranslatorType[] { TranslatorType.ModeS, TranslatorType.Adsb, TranslatorType.Raw }; break;
                    case ExtractedBytesFormat.Compressed:   translators = new TranslatorType[] { TranslatorType.Compressed }; break;
                    case ExtractedBytesFormat.None:         continue;
                    default:                                throw new NotImplementedException();
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
                case TranslatorType.Adsb:       _AdsbTranslator.Setup(r => r.Translate(It.IsAny<ModeSMessage>())).Throws(exception); break;
                case TranslatorType.ModeS:      _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>())).Throws(exception); break;
                case TranslatorType.Port30003:  _Port30003Translator.Setup(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>())).Throws(exception); break;
                case TranslatorType.Raw:        _RawMessageTranslator.Setup(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>())).Throws(exception); break;
                case TranslatorType.Compressed: _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Throws(exception); break;
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
        public void Listener_Dispose_Calls_Provider_Close()
        {
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);
            _Listener.Dispose();
            _Provider.Verify(p => p.Close(), Times.Once());
        }

        [TestMethod]
        public void Listener_Dispose_Disposes_Of_RawMessageTranslator()
        {
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);
            _Listener.Dispose();
            _RawMessageTranslator.Verify(r => r.Dispose(), Times.Once());
        }
        #endregion

        #region ChangeSource
        [TestMethod]
        public void Listener_ChangeSource_Can_Change_Properties()
        {
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);

            Assert.AreSame(_Provider.Object, _Listener.Provider);
            Assert.AreSame(_Listener.BytesExtractor, _BytesExtractor.Object);
            Assert.AreSame(_RawMessageTranslator.Object, _Listener.RawMessageTranslator);
        }

        [TestMethod]
        public void Listener_ChangeSource_Raises_SourceChanged()
        {
            _SourceChangedEvent.EventRaised += (s, a) => {
                Assert.AreSame(_Provider.Object, _Listener.Provider);
                Assert.AreSame(_BytesExtractor.Object, _Listener.BytesExtractor);
                Assert.AreSame(_RawMessageTranslator.Object, _Listener.RawMessageTranslator);
            };

            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);

            Assert.AreEqual(1, _SourceChangedEvent.CallCount);
            Assert.AreSame(_Listener, _SourceChangedEvent.Sender);
            Assert.AreNotEqual(null, _SourceChangedEvent.Args);
        }

        [TestMethod]
        public void Listener_ChangeSource_Sets_Statistics_Property_On_RawMessageTranslator()
        {
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);

            Assert.AreSame(_Statistics.Object, _RawMessageTranslator.Object.Statistics);
        }

        [TestMethod]
        public void Listener_ChangeSource_Resets_TotalMessages_Counter()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("A");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003);

            ChangeSourceAndConnect();
            _Listener.ChangeSource(new MockListenerProvider().Object, new MockMessageBytesExtractor().Object, new Mock<IRawMessageTranslator>().Object, false);

            Assert.AreEqual(0, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_ChangeSource_Resets_TotalBadMessages_Counter()
        {
            _Listener.IgnoreBadMessages = false;
            RemoveDefaultExceptionCaughtHandler();
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("A");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003);
            MakeFormatTranslatorThrowException(TranslatorType.Port30003);

            ChangeSourceAndConnect();
            _Listener.ChangeSource(new MockListenerProvider().Object, new MockMessageBytesExtractor().Object, new Mock<IRawMessageTranslator>().Object, false);

            Assert.AreEqual(0, _Listener.TotalBadMessages);
        }

        [TestMethod]
        public void Listener_ChangeSource_Is_Not_Raised_If_Nothing_Changes()
        {
            var newProvider = new MockListenerProvider();
            var newExtractor = new MockMessageBytesExtractor();
            var newRawMessageTranslator = new Mock<IRawMessageTranslator>(MockBehavior.Default) { DefaultValue = DefaultValue.Mock };

            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);
            Assert.AreEqual(1, _SourceChangedEvent.CallCount);

            _Listener.ChangeSource(newProvider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);
            Assert.AreEqual(2, _SourceChangedEvent.CallCount);

            _Listener.ChangeSource(newProvider.Object, newExtractor.Object, _RawMessageTranslator.Object, false);
            Assert.AreEqual(3, _SourceChangedEvent.CallCount);

            _Listener.ChangeSource(newProvider.Object, newExtractor.Object, newRawMessageTranslator.Object, false);
            Assert.AreEqual(4, _SourceChangedEvent.CallCount);

            _Listener.ChangeSource(newProvider.Object, newExtractor.Object, newRawMessageTranslator.Object, false);
            Assert.AreEqual(4, _SourceChangedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_ChangeSource_AutoReconnect_Reconnects_If_Set()
        {
            var firstProvider = new MockListenerProvider();
            var firstExtractor = new MockMessageBytesExtractor();
            firstProvider.Setup(p => p.Close()).Callback(() => {
                Assert.AreSame(firstProvider.Object, _Listener.Provider);
                Assert.AreSame(firstExtractor.Object, _Listener.BytesExtractor);
            });
            firstProvider.ConfigureForConnect();

            _Listener.ChangeSource(firstProvider.Object, firstExtractor.Object, _RawMessageTranslator.Object, false);
            _Listener.Connect(false);

            _Provider.ConfigureForConnect();
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, true);

            firstProvider.Verify(p => p.Close(), Times.Once());
            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void Listener_ChangeSource_AutoReconnect_Does_Not_Reconnect_If_Clear()
        {
            var firstProvider = new MockListenerProvider();
            var firstExtractor = new MockMessageBytesExtractor();
            firstProvider.ConfigureForConnect();

            _Listener.ChangeSource(firstProvider.Object, firstExtractor.Object, _RawMessageTranslator.Object, false);
            _Listener.Connect(false);

            _Provider.ConfigureForConnect();
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);

            firstProvider.Verify(p => p.Close(), Times.Once());
            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Never());
        }

        [TestMethod]
        public void Listener_ChangeSource_Disconnects_Existing_Provider_Even_If_Not_Connected()
        {
            var firstProvider = new MockListenerProvider();
            var firstExtractor = new MockMessageBytesExtractor();
            firstProvider.ConfigureForConnect();

            _Listener.ChangeSource(firstProvider.Object, firstExtractor.Object, _RawMessageTranslator.Object, false);

            _Provider.ConfigureForConnect();
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);

            firstProvider.Verify(p => p.Close(), Times.Once());
        }

        [TestMethod]
        public void Listener_ChangeSource_With_Reconnec_True_Connects_New_Provider_Even_If_Not_Originally_Connected()
        {
            var firstProvider = new MockListenerProvider();
            var firstExtractor = new MockMessageBytesExtractor();
            firstProvider.ConfigureForConnect();

            _Listener.ChangeSource(firstProvider.Object, firstExtractor.Object, _RawMessageTranslator.Object, false);

            _Provider.ConfigureForConnect();
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, true);

            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void Listener_ChangeSource_Disposes_Of_Existing_RawMessageTranslator()
        {
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);
            _RawMessageTranslator.Verify(r => r.Dispose(), Times.Never());

            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, new Mock<IRawMessageTranslator>().Object, false);
            _RawMessageTranslator.Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Listener_ChangeSource_Does_Not_Dispose_Of_Existing_RawMessageTranslator_If_It_Has_Not_Changed()
        {
            _Listener.ChangeSource(_Provider.Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);
            _Listener.ChangeSource(new MockListenerProvider().Object, _BytesExtractor.Object, _RawMessageTranslator.Object, false);
            _RawMessageTranslator.Verify(r => r.Dispose(), Times.Never());
        }
        #endregion

        #region Connect - Basics
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Listener_Connect_Without_AutoReconnect_Throws_If_ChangeSource_Never_Called()
        {
            _Listener.Connect(false);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Listener_Connect_With_AutoReconnect_Throws_If_ChangeSource_Never_Called()
        {
            _Listener.Connect(true);
        }

        [TestMethod]
        public void Listener_Connect_Calls_BeginConnect()
        {
            ChangeSourceAndConnect();
            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Call_BeginConnect_After_Disposed()
        {
            _Listener.Dispose();

            ChangeSourceAndConnect();
            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Pass_A_Null_AsyncCallback_To_Provider()
        {
            ChangeSourceAndConnect();
            _Provider.Verify(p => p.BeginConnect(null), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Calls_EndConnect_If_BeginConnect_Works()
        {
            var asyncResult = _Provider.ConfigureForConnect();

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.EndConnect(asyncResult), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Call_EndConnect_If_BeginConnect_Never_Completes()
        {
            ChangeSourceAndConnect();
            _Provider.Verify(p => p.EndConnect(It.IsAny<IAsyncResult>()), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Routes_All_Exceptions_From_BeginConnect_Through_ExceptionCaught()
        {
            // Connect could be called during a reconnect operation from a background thread so it can't let
            // exceptions just bubble up.
            var exception = new InvalidOperationException();
            _Provider.Setup(p => p.BeginConnect(It.IsAny<AsyncCallback>())).Callback(() => { throw exception; });
            RemoveDefaultExceptionCaughtHandler();

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            Assert.AreSame(_Listener, _ExceptionCaughtEvent.Sender);
            Assert.AreSame(exception, _ExceptionCaughtEvent.Args.Value);
        }

        [TestMethod]
        public void Listener_Connect_Disconnects_If_BeginConnect_Throws_Exception()
        {
            _Provider.Setup(p => p.BeginConnect(It.IsAny<AsyncCallback>())).Callback(() => { throw new InvalidOperationException(); });
            RemoveDefaultExceptionCaughtHandler();

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.Close(), Times.Once());
            Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus);
        }

        [TestMethod]
        public void Listener_Connect_Routes_All_Exceptions_From_EndConnect_Through_ExceptionCaught()
        {
            _Provider.ConfigureForConnect();

            var exception = new NotSupportedException();
            _Provider.Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Callback(() => { throw exception; });
            RemoveDefaultExceptionCaughtHandler();

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            Assert.AreSame(_Listener, _ExceptionCaughtEvent.Sender);
            Assert.AreSame(exception, _ExceptionCaughtEvent.Args.Value);
        }

        [TestMethod]
        public void Listener_Connect_Disconnects_After_EndConnect_Picks_Up_General_Exception()
        {
            _Provider.ConfigureForConnect();

            _Provider.Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Callback(() => { throw new NotSupportedException(); });
            RemoveDefaultExceptionCaughtHandler();

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.Close(), Times.Once());
            Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus);
        }

        [TestMethod]
        public void Listener_Connect_Ignores_Exceptions_From_EndConnect_About_The_Connection_Being_Disposed()
        {
            // These are just exception spam, we can safely ignore them

            _Provider.ConfigureForConnect();
            _Provider.Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Callback(() => { throw new ObjectDisposedException("whatever"); });
            RemoveDefaultExceptionCaughtHandler();

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _ExceptionCaughtEvent.CallCount);
            _Provider.Verify(p => p.Close(), Times.Never());
            Assert.AreEqual(ConnectionStatus.Connecting, _Listener.ConnectionStatus);
        }

        [TestMethod]
        public void Listener_Connect_Ignores_Exceptions_From_EndConnect_About_The_Connection_Being_Closed()
        {
            // These are just exception spam - unfortunately they use a fairly common exception class for these but we still need to ignore them

            _Provider.ConfigureForConnect();
            _Provider.Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Callback(() => { throw new InvalidOperationException(); });
            RemoveDefaultExceptionCaughtHandler();

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _ExceptionCaughtEvent.CallCount);
            _Provider.Verify(p => p.Close(), Times.Never());
            Assert.AreEqual(ConnectionStatus.Connecting, _Listener.ConnectionStatus);
        }

        [TestMethod]
        public void Listener_Connect_Sets_ConnectionStatus_To_Connecting_When_Calling_BeginConnect_With_AutoReconnect_False()
        {
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.AreEqual(ConnectionStatus.Connecting, _Listener.ConnectionStatus);
            };

            ChangeSourceAndConnect(false, null, null, null, false);

            Assert.AreEqual(1, _ConnectionStateChangedEvent.CallCount);
            Assert.AreSame(_Listener, _ConnectionStateChangedEvent.Sender);
        }

        [TestMethod]
        public void Listener_Connect_Sets_ConnectionStatus_To_Reconnecting_When_Calling_BeginConnect_With_AutoReconnect_True()
        {
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                Assert.AreEqual(ConnectionStatus.Reconnecting, _Listener.ConnectionStatus);
            };

            ChangeSourceAndConnect(false, null, null, null, true);

            Assert.AreEqual(1, _ConnectionStateChangedEvent.CallCount);
            Assert.AreSame(_Listener, _ConnectionStateChangedEvent.Sender);
        }

        [TestMethod]
        public void Listener_Connect_Sets_ConnectionStatus_To_Connected_When_BeginConnect_Calls_Back()
        {
            _Provider.ConfigureForConnect();

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
        public void Listener_Connect_Sets_ConnectionTime_In_Statistics_When_BeginConnect_Calls_Back()
        {
            _Provider.ConfigureForConnect();

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
        public void Listener_Connect_Does_Not_Set_Connection_Status_If_EndConnect_Reported_It_Was_For_An_Old_Connection()
        {
            _Provider.ConfigureForConnect();
            _Provider.Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Returns(false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _ConnectionStateChangedEvent.CallCount);
            Assert.AreEqual(ConnectionStatus.Connecting, _Listener.ConnectionStatus);
        }

        [TestMethod]
        public void Listener_Connect_Sets_ConnectionStatus_To_CannotConnect_If_EndConnect_Throws_SocketException()
        {
            _Provider.ConfigureForConnect();
            _Provider.Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Callback(() => { throw new SocketException(); } );

            ChangeSourceAndConnect();

            Assert.AreEqual(ConnectionStatus.CannotConnect, _Listener.ConnectionStatus);
            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Begins_Reading_Content_From_Connection_When_BeginConnect_Works()
        {
            _Provider.ConfigureForConnect();

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.BeginRead(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Read_Content_If_EndConnect_Reported_It_Was_For_An_Old_Connection()
        {
            _Provider.ConfigureForConnect();
            _Provider.Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Returns(false);

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.BeginRead(It.IsAny<AsyncCallback>()), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Read_Content_If_EndConnect_Throws_Exception()
        {
            _Provider.ConfigureForConnect();
            RemoveDefaultExceptionCaughtHandler();
            _Provider.Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Callback(() => { throw new NotImplementedException(); });

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            _Provider.Verify(p => p.BeginRead(It.IsAny<AsyncCallback>()), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Catches_Exceptions_In_EndRead_Calls()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("XYZ\n");
            RemoveDefaultExceptionCaughtHandler();

            var exception = new NotImplementedException();
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw exception; });

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            Assert.AreSame(_Listener, _ExceptionCaughtEvent.Sender);
            Assert.AreSame(exception, _ExceptionCaughtEvent.Args.Value);
        }

        [TestMethod]
        public void Listener_Connect_Disconnects_After_Catching_A_General_Exception_In_EndRead()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("XYZ\n");
            RemoveDefaultExceptionCaughtHandler();

            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new NotImplementedException(); });

            ChangeSourceAndConnect();

            Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus);
            _Provider.Verify(p => p.Close(), Times.Once());
            _Statistics.Verify(r => r.ResetConnectionStatistics(), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Start_New_Read_After_Catching_A_General_Exception_In_EndRead()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("XYZ\n");
            RemoveDefaultExceptionCaughtHandler();

            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new NotImplementedException(); });

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.BeginRead(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Ignores_ObjectDisposed_Exceptions_In_EndRead()
        {
            // These happen a lot, whenever the connection is disposed of on another thread. We need to ignore them.
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("XYZ\n");
            RemoveDefaultExceptionCaughtHandler();
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new ObjectDisposedException("x"); });

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _ExceptionCaughtEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Start_New_Read_After_Catching_An_Object_Disposed_Exception_In_EndRead()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("XYZ\n");
            RemoveDefaultExceptionCaughtHandler();
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new ObjectDisposedException("x"); });

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.BeginRead(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Will_Attempt_A_Reconnect_If_EndRead_Reports_SocketException()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("ABC\nXYZ\n");
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new SocketException(); });

            bool seenReconnectingConnectionStateEvent = false;
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                if(_Listener.ConnectionStatus == ConnectionStatus.Reconnecting) {
                    seenReconnectingConnectionStateEvent = true;
                }
            };

            ChangeSourceAndConnect();

            Assert.IsTrue(seenReconnectingConnectionStateEvent);
            Assert.AreEqual(ConnectionStatus.Reconnecting, _Listener.ConnectionStatus);
            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Exactly(2));
            _Statistics.Verify(r => r.ResetConnectionStatistics(), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Will_Attempt_A_Reconnect_If_EndRead_Returns_Zero()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("ABC\nXYZ\n");
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Returns(0);

            bool seenReconnectingConnectionStateEvent = false;
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                if(_Listener.ConnectionStatus == ConnectionStatus.Reconnecting) seenReconnectingConnectionStateEvent = true;
            };

            ChangeSourceAndConnect();

            Assert.IsTrue(seenReconnectingConnectionStateEvent);
            Assert.AreEqual(ConnectionStatus.Reconnecting, _Listener.ConnectionStatus);
            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Listener_Connect_Will_Not_Attempt_To_Read_Blocks_From_Dropped_Connection()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("ABC\nXYZ\n");
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new SocketException(); });

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.BeginRead(It.IsAny<AsyncCallback>()), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Will_Wait_At_Least_One_Second_Between_Each_Reconnect_Attempt()
        {
            var beginConnectTimes = new List<DateTime>();
            _Provider.ConfigureForConnect(2, () => { beginConnectTimes.Add(DateTime.UtcNow); });
            _Provider.ConfigureForReadStream("ABC\nXYZ\n");

            int endConnectCallCount = 0;
            _Provider.Setup(p => p.EndConnect(It.IsAny<IAsyncResult>())).Returns(true).Callback(() => {
                switch(endConnectCallCount++) {
                    case 0:     break;                       // first connect works, triggers read
                    case 1:     throw new SocketException(); // second connect (1st attempt at reconnect) fails 
                }
            });
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new SocketException(); });

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Exactly(3));
            Assert.AreEqual(ConnectionStatus.Reconnecting, _Listener.ConnectionStatus);

            var milliseconds = (beginConnectTimes[1] - beginConnectTimes[0]).TotalMilliseconds;
            Assert.IsTrue(milliseconds >= 900, milliseconds.ToString());
            milliseconds = (beginConnectTimes[2] - beginConnectTimes[1]).TotalMilliseconds;
            Assert.IsTrue(milliseconds >= 900, milliseconds.ToString());
        }

        [TestMethod]
        public void Listener_Connect_Will_Abandon_Reconnect_If_User_Manually_Connects_While_Pausing()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("ABC\nXYZ\n");
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new SocketException(); });
            _Provider.Setup(p => p.Sleep(It.IsAny<int>())).Callback(() => { _Listener.Connect(false); });

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Exactly(2));
            Assert.AreEqual(ConnectionStatus.Connecting, _Listener.ConnectionStatus);
        }

        [TestMethod]
        public void Listener_Connect_Will_Pass_Exceptions_From_BeginConnect_During_Reconnect_To_ExceptionCaught()
        {
            int connectCount = 0;
            var exception = new InvalidOperationException();
            _Provider.ConfigureForConnect(2, () => { if(++connectCount == 2) throw exception; });
            _Provider.ConfigureForReadStream("ABC\nXYZ\n");
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new SocketException(); });
            RemoveDefaultExceptionCaughtHandler();

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            Assert.AreSame(_Listener, _ExceptionCaughtEvent.Sender);
            Assert.AreSame(exception, _ExceptionCaughtEvent.Args.Value);
        }

        [TestMethod]
        public void Listener_Connect_Will_Disconnect_If_Reconnect_Throws_A_General_Exception()
        {
            int connectCount = 0;
            _Provider.ConfigureForConnect(2, () => { if(++connectCount == 2) throw new InvalidOperationException(); });
            _Provider.ConfigureForReadStream("ABC\nXYZ\n");
            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new SocketException(); });
            RemoveDefaultExceptionCaughtHandler();

            ChangeSourceAndConnect();

            _Provider.Verify(p => p.Close(), Times.Once());
            Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus);
        }
        #endregion

        #region Connect - General Message Processing
        [TestMethod]
        public void Listener_Connect_Raises_RawBytesReceived_When_Bytes_Are_Received()
        {
            var bytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream(bytes, 3);

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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream(new byte[] { 0x01, 0x02 }, int.MinValue, new List<IEnumerable<byte>>() { { new byte[] { 0x03, 0x04, 0x05 } } });
            _BytesExtractor.Setup(r => r.BufferSize).Returns(12);

            ChangeSourceAndConnect();

            Assert.AreEqual(5, _Statistics.Object.BytesReceived);
            Assert.AreEqual(12, _Statistics.Object.CurrentBufferSize);
        }

        [TestMethod]
        public void Listener_Connect_Raises_ModeSBytesReceived_When_BytesExtractor_Extracts_ModeS_Message()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            var extractedBytes = _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 }, 1, 7, true, false);

            var eventRecorder = new EventRecorder<EventArgs<ExtractedBytes>>();
            _Listener.ModeSBytesReceived += eventRecorder.Handler;

            ChangeSourceAndConnect();

            Assert.AreEqual(0, eventRecorder.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Raise_ModeSBytesReceived_When_BytesExtractor_Extracts_Port30003_Message()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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

            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream(bytes);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _BytesExtractor.SourceByteArrays.Count);
            Assert.IsTrue(bytes.SequenceEqual(_BytesExtractor.SourceByteArrays[0]));
            _BytesExtractor.Verify(r => r.ExtractMessageBytes(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Passes_Count_Of_Bytes_Read_To_Message_Extractor()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream(new byte[] { 0xf0, 0xe8 }, 1);

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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream(bytes1, int.MinValue, new List<IEnumerable<byte>>() { bytes2 });

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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "ABC123");

            ChangeSourceAndConnect();

            _Port30003Translator.Verify(r => r.Translate("ABC123", It.IsAny<int?>()), Times.Once());
            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Never());
            _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Never());
            _RawMessageTranslator.Verify(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>()), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Honours_Offset_And_Length_When_Translating_Extracted_Bytes_To_Port30003_Message_Strings_For_Translation()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "-ABC-", 1, 3, false, false);

            ChangeSourceAndConnect();

            _Port30003Translator.Verify(r => r.Translate("ABC", null), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Ignores_Extracted_Parity_On_Port30003_Messages()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "ABC123", false, true);

            ChangeSourceAndConnect();

            _Port30003Translator.Verify(r => r.Translate("ABC123", null), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Uses_Extracted_SignalLevel_On_Port30003_Messages()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "ABC123").SignalLevel = 123;

            ChangeSourceAndConnect();

            _Port30003Translator.Verify(r => r.Translate("ABC123", 123), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Raises_Port30003MessageReceived_With_Message_From_Translator()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");

            ChangeSourceAndConnect();

            Assert.AreEqual(1234, _Port30003MessageReceivedEvent.Args.Message.ReceiverId);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Raise_Port30003MessageReceived_When_Translator_Returns_Null()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");
            _Port30003Translator.Setup(r => r.Translate("A", null)).Returns((BaseStationMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreEqual(0, _ModeSMessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Raises_Port30003MessageReceived_For_Every_Extracted_Packet()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_Port30003_Message_Received()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Statistics.Object.BaseStationMessagesReceived);
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_Bad_Port30003_Message_Received()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");

            var exception = MakeFormatTranslatorThrowException(TranslatorType.Port30003);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Statistics.Object.BaseStationMessagesReceived);
            Assert.AreEqual(1, _Statistics.Object.BaseStationBadFormatMessagesReceived);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_For_Every_Message_In_A_Received_Packet_Of_Port30003_Messages()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "A");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Port30003, "B");

            ChangeSourceAndConnect();

            Assert.AreEqual(2, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Increment_TotalMessages_When_Port30003Translator_Returns_Null()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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
            var extractedBytes = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };

            _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, extractedBytes, 1, 7, false, false).SignalLevel = 123;

            byte[] bytesPassedToTranslator = null;
            int offsetPassedToTranslator = 0;
            int? signalLevelPassedToTranslator = null;
            _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()))
            .Callback((byte[] bytes, int offset, int? signalLevel) => {
                bytesPassedToTranslator = bytes;
                offsetPassedToTranslator = offset;
                signalLevelPassedToTranslator = signalLevel;
            })
            .Returns(_ModeSMessage);

            ChangeSourceAndConnect();

            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Once());
            Assert.AreSame(extractedBytes, bytesPassedToTranslator);
            Assert.AreEqual(1, offsetPassedToTranslator);
            Assert.AreEqual(123, signalLevelPassedToTranslator);

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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("a");
                _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, extractedBytes);

                ChangeSourceAndConnect();

                string failMessage = String.Format("For length of {0}", length);
                if(length != 7 && length != 14) {
                    _Port30003Translator.Verify(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>()), Times.Never(), failMessage);
                    _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Never(), failMessage);
                    _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Never(), failMessage);
                    _RawMessageTranslator.Verify(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>()), Times.Never(), failMessage);
                } else {
                    _Port30003Translator.Verify(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>()), Times.Never(), failMessage);
                    _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Once(), failMessage);
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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, bytes, 1, 7, false, true);

            ChangeSourceAndConnect();

            _ModeSParity.Verify(r => r.StripParity(bytes, 1, 7), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Strip_Parity_From_ModeSMessages_When_Indicated()
        {
            _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);

            var bytes = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("a");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("a");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("a");
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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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

            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreSame(_Listener, _Port30003MessageReceivedEvent.Sender);
            Assert.AreSame(_Port30003Message, _Port30003MessageReceivedEvent.Args.Message);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Translate_Null_ModeS_Messages()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>())).Returns((ModeSMessage)null);

            ChangeSourceAndConnect();

            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Once());
            _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Never());
            _RawMessageTranslator.Verify(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>()), Times.Never());
            Assert.AreEqual(0, _ModeSMessageReceivedEvent.CallCount);
            Assert.AreEqual(0, _Port30003MessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Will_Translate_Null_Adsb_Messages()
        {
            _Clock.UtcNowValue = new DateTime(2007, 8, 9, 10, 11, 12, 13);

            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _AdsbTranslator.Setup(r => r.Translate(It.IsAny<ModeSMessage>())).Returns((AdsbMessage)null);

            ChangeSourceAndConnect();

            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Once());
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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _RawMessageTranslator.Setup(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>())).Returns((BaseStationMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Port30003MessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_When_ModeS_Message_Received()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_For_Every_Message_In_A_Received_Packet_Of_ModeS_Messages()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);

            ChangeSourceAndConnect();

            Assert.AreEqual(2, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Increment_TotalMessages_When_ModeSTranslator_Returns_Null()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _ModeSTranslator.Setup(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>())).Returns((ModeSMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Increments_TotalMessages_When_AdsbTranslator_Returns_Null()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.ModeS, 7);
            _AdsbTranslator.Setup(r => r.Translate(It.IsAny<ModeSMessage>())).Returns((AdsbMessage)null);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Increments_TotalMessages_When_RawTranslator_Returns_Null()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            var payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, payload, 0, 6, false, false);

            ChangeSourceAndConnect();

            _Compressor.Verify(r => r.Decompress(payload), Times.Once());
            _Port30003Translator.Verify(r => r.Translate(It.IsAny<string>(), It.IsAny<int?>()), Times.Never());
            _ModeSTranslator.Verify(r => r.Translate(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Never());
            _AdsbTranslator.Verify(r => r.Translate(It.IsAny<ModeSMessage>()), Times.Never());
            _RawMessageTranslator.Verify(r => r.Translate(It.IsAny<DateTime>(), It.IsAny<ModeSMessage>(), It.IsAny<AdsbMessage>()), Times.Never());
        }

        [TestMethod]
        public void Listener_Connect_Honours_Offset_And_Length_When_Translating_Compressed_Bytes()
        {
            var payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
            var expected = new byte[] { 0x02, 0x03, 0x04, 0x05 };
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, payload, 1, 4, false, false);

            ChangeSourceAndConnect();

            _Compressor.Verify(r => r.Decompress(expected), Times.Once());
        }

        [TestMethod]
        public void Listener_Connect_Raises_Event_With_BaseStationMessage_Built_From_Compressed_Message()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1234, _Port30003MessageReceivedEvent.Args.Message.ReceiverId);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Raise_Port30003MessageReceived_When_Compressor_Returns_Null()
        {
            _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Returns((BaseStationMessage)null);
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Port30003MessageReceivedEvent.CallCount);
            Assert.AreEqual(0, _ModeSMessageReceivedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Increment_TotalMessages_When_Compressor_Returns_Null()
        {
            _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Returns((BaseStationMessage)null);
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Increments_Total_Messages_When_Compressed_Message_Received()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Listener.TotalMessages);
        }

        [TestMethod]
        public void Listener_Connect_Updates_Statistics_When_Compressed_Message_Received()
        {
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _Statistics.Object.BaseStationMessagesReceived);
        }

        [TestMethod]
        public void Listener_Connect_Increments_TotalBadMessages_When_Bad_Compressed_Message_Received()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Compressor.Setup(r => r.Decompress(It.IsAny<byte[]>())).Callback(() => { throw new InvalidCastException(); });
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
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
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a");
            _BytesExtractor.AddExtractedBytes(ExtractedBytesFormat.Compressed, 12, false, false);

            ChangeSourceAndConnect();

            Assert.AreEqual(0, _Statistics.Object.BaseStationMessagesReceived);
            Assert.AreEqual(1, _Statistics.Object.BaseStationBadFormatMessagesReceived);
        }
        #endregion

        #region Connect - Unknown Message Processing
        [TestMethod]
        public void Listener_Connect_Throws_Exception_When_ExtractedBytes_Are_In_Unknown_Format()
        {
            DoForEveryFormat((format, failMessage) => {
                RemoveDefaultExceptionCaughtHandler();

                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("a");
                _BytesExtractor.AddExtractedBytes(format);

                ChangeSourceAndConnect();

                switch(format) {
                    case ExtractedBytesFormat.None:
                        Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount, failMessage);
                        break;
                    case ExtractedBytesFormat.Compressed:
                    case ExtractedBytesFormat.Port30003:
                    case ExtractedBytesFormat.ModeS:
                        Assert.AreEqual(0, _ExceptionCaughtEvent.CallCount, failMessage);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            });
        }
        #endregion

        #region Connect - Packet Checksum Handling
        [TestMethod]
        public void Listener_Connect_Does_Not_Increment_TotalMessages_When_BadChecksum_Packet_Received()
        {
            DoForEveryFormat((format, failMessage) => {
                if(format != ExtractedBytesFormat.None) {
                    RemoveDefaultExceptionCaughtHandler();

                    _Provider.ConfigureForConnect();
                    _Provider.ConfigureForReadStream("a");
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

                    _Provider.ConfigureForConnect();
                    _Provider.ConfigureForReadStream("a");
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
                        _Provider.ConfigureForConnect();
                        _Provider.ConfigureForReadStream("a");
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
                        _Provider.ConfigureForConnect();
                        _Provider.ConfigureForReadStream("a");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("A");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("A");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("A");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("A");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("A");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("A");
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
                _Provider.ConfigureForConnect();
                _Provider.ConfigureForReadStream("A");
                _BytesExtractor.AddExtractedBytes(format, 7);

                var exception = MakeFormatTranslatorThrowException(translatorType);

                ChangeSourceAndConnect();

                Assert.AreEqual(translatorType == TranslatorType.Raw ? 0 : 1, _Listener.TotalBadMessages, failMessage);  // Exceptions in the raw translator are not an indication of a bad message, they're an indication of a bug
            });
        }
        #endregion

        #region Connect - CoarseListenerTimeout Handling
        void SetupCoarseListenerTimeout(int timeout)
        {
            _ConfigurationStorage.Object.CoarseListenerTimeout = timeout;
        }

        [TestMethod]
        public void Listener_CoarseListenerTimeout_Reconnects_If_Timeout_Expires_With_No_Messages()
        {
            SetupCoarseListenerTimeout(10);
            _Provider.ConfigureForConnect();
            ChangeSourceAndConnect();

            bool seenReconnectingConnectionStateEvent = false;
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                if(_Listener.ConnectionStatus == ConnectionStatus.Reconnecting) {
                    seenReconnectingConnectionStateEvent = true;
                }
            };

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(10).AddMilliseconds(1);
            _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsTrue(seenReconnectingConnectionStateEvent);
            Assert.AreEqual(ConnectionStatus.Reconnecting, _Listener.ConnectionStatus);
            _Provider.Verify(p => p.Close(), Times.Exactly(1));
            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Exactly(2));
        }

        [TestMethod]
        public void Listener_CoarseListenerTimeout_Does_Not_Reconnect_If_Not_Connected()
        {
            SetupCoarseListenerTimeout(10);
            _Provider.ConfigureForConnect();
            ChangeSourceAndConnect();

            bool seenReconnectingConnectionStateEvent = false;
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                if(_Listener.ConnectionStatus == ConnectionStatus.Reconnecting) {
                    seenReconnectingConnectionStateEvent = true;
                }
            };

            _Listener.Disconnect();
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(10).AddMilliseconds(1);
            _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsFalse(seenReconnectingConnectionStateEvent);
            Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus);
        }

        [TestMethod]
        public void Listener_CoarseListenerTimeout_Gives_The_Feed_A_Chance_To_Send_Something_Before_Reconnecting()
        {
            // See http://tracker.virtualradarserver.co.uk/youtrack/issue/VRS-64 - the problem was that the
            // if the feed doesn't send anything before the timer ticks and the test is performed against the
            // last receive time then the code disconnected and reconnected, even though the connection may have
            // been up for less than 10 seconds. The timeline of events is:
            //   00.000: Connection established.
            //   00.001: Timer ticks - is last receive time more than 10 seconds ago? Yes. Disconnect and reconnect.
            //           Everything freaks out.
            // Whereas it should be:
            //   00.000: Connection established.
            //   00.001: Timer ticks - is last receive time more than 10 seconds ago? No. It's only been up for 1ms.
            //           Do nothing.
            //   00.075: Message received - huzzah!
            //   01.000: Timer ticks - is last receive time more than 10 seconds ago? No.
            // etc.
            SetupCoarseListenerTimeout(10);
            _Provider.ConfigureForConnect();
            ChangeSourceAndConnect();

            bool seenReconnectingConnectionStateEvent = false;
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                if(_Listener.ConnectionStatus == ConnectionStatus.Reconnecting) {
                    seenReconnectingConnectionStateEvent = true;
                }
            };

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(10).AddMilliseconds(-1);
            _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsFalse(seenReconnectingConnectionStateEvent);
            Assert.AreEqual(ConnectionStatus.Connected, _Listener.ConnectionStatus);
            _Provider.Verify(p => p.Close(), Times.Never());
            _Provider.Verify(p => p.BeginConnect(It.IsAny<AsyncCallback>()), Times.Exactly(1));
        }
        #endregion

        #region Connect - Logging
        [TestMethod]
        public void Listener_Connect_Logs_Connection_Attempt()
        {
            ChangeSourceAndConnect();
            Assert.AreEqual(1, _LogMessages.Count(r => r.IndexOf("Connecting", StringComparison.OrdinalIgnoreCase) != -1));
        }

        [TestMethod]
        public void Listener_Connect_Logs_Failure_In_Provider_BeginConnect()
        {
            // This is unlikely to happen, but if it should then we want it logged
            RemoveDefaultExceptionCaughtHandler();
            _Provider.Setup(r => r.BeginConnect(It.IsAny<AsyncCallback>())).Callback((AsyncCallback ac) => { throw new InvalidOperationException(); });
            ChangeSourceAndConnect();
            Assert.AreEqual(1, _LogMessages.Count(r => r.IndexOf("Failed to connect", StringComparison.OrdinalIgnoreCase) != -1));
        }

        [TestMethod]
        public void Listener_Connect_Logs_Connection_Success()
        {
            _Provider.ConfigureForConnect();

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _LogMessages.Count(r => r.IndexOf("Connected", StringComparison.OrdinalIgnoreCase) != -1));
        }

        [TestMethod]
        public void Listener_Connect_Logs_EndConnect_Unknown_Exception()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Provider.ConfigureForConnect();
            _Provider.Setup(r => r.EndConnect(It.IsAny<IAsyncResult>())).Callback((IAsyncResult ar) => { throw new OverflowException(); });

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _LogMessages.Count(r => r.IndexOf("Failed to connect", StringComparison.OrdinalIgnoreCase) != -1));
        }

        [TestMethod]
        public void Listener_Connect_Logs_EndConnect_SocketException_When_Connecting()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Provider.ConfigureForConnect();
            _Provider.Setup(r => r.EndConnect(It.IsAny<IAsyncResult>())).Callback((IAsyncResult ar) => { throw new SocketException(); });

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _LogMessages.Count(r => r.IndexOf("Failed to connect", StringComparison.OrdinalIgnoreCase) != -1));
        }

        [TestMethod]
        public void Listener_Connect_Does_Not_Log_EndConnect_SocketException_When_Reconnecting()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Provider.ConfigureForConnect();
            _Provider.Setup(r => r.EndConnect(It.IsAny<IAsyncResult>())).Callback((IAsyncResult ar) => { throw new SocketException(); });

            ChangeSourceAndConnect(false, null, null, null, true);

            Assert.AreEqual(0, _LogMessages.Count(r => r.IndexOf("Failed", StringComparison.OrdinalIgnoreCase) != -1));
        }

        [TestMethod]
        public void Listener_Connect_Logs_Provider_SocketException_On_EndRead()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream("a\n");

            _Provider.Setup(p => p.EndRead(It.IsAny<IAsyncResult>())).Callback(() => { throw new SocketException(); });

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _LogMessages.Count(r => r.IndexOf("Caught", StringComparison.OrdinalIgnoreCase) != -1));
        }

        [TestMethod]
        public void Listener_Connect_Logs_Reconnect_On_EndRead()
        {
            RemoveDefaultExceptionCaughtHandler();
            _Provider.ConfigureForConnect();
            _Provider.ConfigureForReadStream(new byte[0]);

            ChangeSourceAndConnect();

            Assert.AreEqual(1, _LogMessages.Count(r => r.IndexOf("Reconnecting", StringComparison.OrdinalIgnoreCase) != -1));
        }
        #endregion

        #region Disconnect
        [TestMethod]
        public void Listener_Disconnect_Closes_Provider()
        {
            _Provider.ConfigureForConnect();

            ChangeSourceAndConnect();
            _Listener.Disconnect();

            _Provider.Verify(p => p.Close(), Times.Once());
        }

        [TestMethod]
        public void Listener_Disconnect_Sets_Connection_Status()
        {
            _Provider.ConfigureForConnect();
            int callCount = 0;
            _ConnectionStateChangedEvent.EventRaised += (object sender, EventArgs args) => {
                if(++callCount == 3) Assert.AreEqual(ConnectionStatus.Disconnected, _Listener.ConnectionStatus);
            };

            ChangeSourceAndConnect();
            _Listener.Disconnect();

            Assert.AreEqual(3, _ConnectionStateChangedEvent.CallCount);
        }

        [TestMethod]
        public void Listener_Disconnect_Logs_Disconnection()
        {
            _Provider.ConfigureForConnect();

            ChangeSourceAndConnect();
            _Listener.Disconnect();

            Assert.AreEqual(1, _LogMessages.Count(r => r.IndexOf("Disconnected", StringComparison.OrdinalIgnoreCase) != -1));
        }
        #endregion
    }
}
