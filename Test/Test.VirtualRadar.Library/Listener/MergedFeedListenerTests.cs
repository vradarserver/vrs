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
using System.Linq;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class MergedFeedListenerTests
    {
        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalFactory;
        private IMergedFeedListener _MergedFeed;
        private ClockMock _Clock;
        private Mock<IListener> _Listener1;
        private Mock<IListener> _Listener2;
        private List<IListener> _Listeners;
        private Mock<IMergedFeedComponentListener> _Component1;
        private Mock<IMergedFeedComponentListener> _Component2;
        private List<IMergedFeedComponentListener> _Components;
        private Mock<IHeartbeatService> _HeartbeatService;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private EventRecorder<BaseStationMessageEventArgs> _BaseStationMessageEventRecorder;
        private EventRecorder<EventArgs<string>> _PositionResetRecorder;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtRecorder;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsTest).Returns(true);

            _MergedFeed = Factory.Singleton.Resolve<IMergedFeedListener>();

            _Listener1 = TestUtilities.CreateMockInstance<IListener>();
            _Listener1.Setup(r => r.ReceiverId).Returns(1);
            _Listener2 = TestUtilities.CreateMockInstance<IListener>();
            _Listener2.Setup(r => r.ReceiverId).Returns(2);
            _Listeners = new List<IListener>(new IListener[] { _Listener1.Object, _Listener2.Object });

            _Component1 = TestUtilities.CreateMockInstance<IMergedFeedComponentListener>();
            _Component1.Setup(r => r.Listener).Returns(_Listener1.Object);
            _Component2 = TestUtilities.CreateMockInstance<IMergedFeedComponentListener>();
            _Component2.Setup(r => r.Listener).Returns(_Listener2.Object);
            _Components = new List<IMergedFeedComponentListener>(new IMergedFeedComponentListener[] { _Component1.Object, _Component2.Object });

            _BaseStationMessageEventRecorder = new EventRecorder<BaseStationMessageEventArgs>();
            _PositionResetRecorder = new EventRecorder<EventArgs<string>>();
            _ExceptionCaughtRecorder = new EventRecorder<EventArgs<Exception>>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
            _MergedFeed.Dispose();
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void MergedFeedListener_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            CommonListenerTests.CommonListener_Constructor_Initialises_To_Known_State_And_Properties_Work(_MergedFeed);

            Assert.AreEqual(0, _MergedFeed.Listeners.Count);
            Assert.AreEqual(ConnectionStatus.Connected, _MergedFeed.ConnectionStatus);
            Assert.IsNull(_MergedFeed.Statistics);
            TestUtilities.TestProperty(_MergedFeed, r => r.IcaoTimeout, 5000, 10000);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void MergedFeedListener_Dispose_Does_Not_Dispose_Listeners()
        {
            _MergedFeed.SetListeners(_Components);

            _MergedFeed.Dispose();

            _Listener1.Verify(r => r.Dispose(), Times.Never());
            _Listener2.Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void MergedFeedListener_Dispose_Stops_Listening_To_Events()
        {
            _MergedFeed.Port30003MessageReceived += _BaseStationMessageEventRecorder.Handler;
            _MergedFeed.SetListeners(_Components);
            _MergedFeed.Dispose();
            _Listener1.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(new BaseStationMessage()));

            Assert.AreEqual(0, _BaseStationMessageEventRecorder.CallCount);
        }
        #endregion

        #region SetListeners
        [TestMethod]
        public void MergedFeedListener_SetListeners_Populates_Listeners()
        {
            _MergedFeed.SetListeners(_Components);

            Assert.AreEqual(2, _MergedFeed.Listeners.Count);
            Assert.IsTrue(_MergedFeed.Listeners.Contains(_Component1.Object));
            Assert.IsTrue(_MergedFeed.Listeners.Contains(_Component2.Object));
        }

        [TestMethod]
        public void MergedFeedListener_SetListener_BaseStationMessage_Events_On_Listeners_Passed_Through()
        {
            _MergedFeed.SetListeners(_Components);
            _MergedFeed.Port30003MessageReceived += _BaseStationMessageEventRecorder.Handler;

            var args = new BaseStationMessageEventArgs(new BaseStationMessage());
            _Listener1.Raise(r => r.Port30003MessageReceived += null, args);

            Assert.AreEqual(1, _BaseStationMessageEventRecorder.CallCount);
            Assert.AreSame(_MergedFeed, _BaseStationMessageEventRecorder.Sender);
            Assert.AreSame(args.Message, _BaseStationMessageEventRecorder.Args.Message);
        }

        [TestMethod]
        public void MergedFeedListener_SetListener_Passes_Through_Receiver_Id_On_BaseStation_Messages()
        {
            _MergedFeed.ReceiverId = 1234;
            _MergedFeed.SetListeners(_Components);
            _MergedFeed.Port30003MessageReceived += _BaseStationMessageEventRecorder.Handler;

            var args = new BaseStationMessageEventArgs(new BaseStationMessage() { ReceiverId = 998877 });
            _Listener1.Raise(r => r.Port30003MessageReceived += null, args);

            Assert.AreEqual(998877, _BaseStationMessageEventRecorder.Args.Message.ReceiverId);
        }

        [TestMethod]
        public void MergedFeedListener_SetListener_PositionReset_Events_On_Listeners_Passed_Through()
        {
            _MergedFeed.SetListeners(_Components);
            _MergedFeed.PositionReset += _PositionResetRecorder.Handler;

            var args = new EventArgs<string>("123123");
            _Listener1.Raise(r => r.PositionReset += null, args);

            Assert.AreEqual(1, _PositionResetRecorder.CallCount);
            Assert.AreSame(_MergedFeed, _PositionResetRecorder.Sender);
            Assert.AreSame(args, _PositionResetRecorder.Args);
        }

        [TestMethod]
        public void MergedFeedListener_SetListener_Does_Not_Double_Hook_Listeners_If_Same_Listener_Is_Passed_Twice()
        {
            _MergedFeed.SetListeners(_Components);
            _MergedFeed.SetListeners(_Components);
            _MergedFeed.Port30003MessageReceived += _BaseStationMessageEventRecorder.Handler;
            _MergedFeed.PositionReset += _PositionResetRecorder.Handler;

            _Listener1.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(new BaseStationMessage()));
            _Listener1.Raise(r => r.PositionReset += null, new EventArgs<string>("112233"));

            Assert.AreEqual(1, _BaseStationMessageEventRecorder.CallCount);
            Assert.AreEqual(1, _PositionResetRecorder.CallCount);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "MergedFeedIcaoFilter$")]
        public void MergedFeedListener_SetListener_Events_Are_Filtered_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Component1.Setup(r => r.IsMlatFeed).Returns(worksheet.Bool("L1MLAT"));
            _Component2.Setup(r => r.IsMlatFeed).Returns(worksheet.Bool("L2MLAT"));

            _MergedFeed.SetListeners(_Components);
            _MergedFeed.IcaoTimeout = worksheet.Int("IcaoTimeout");
            _MergedFeed.IgnoreAircraftWithNoPosition = worksheet.Bool("IgnoreNoPos");
            var startTime = new DateTime(2013, 10, 8, 14, 56, 21, 0);

            for(var i = 1;i <= 3;++i) {
                var listenerNumberName = String.Format("Listener{0}", i);
                var icaoName = String.Format("ICAO{0}", i);
                var msOffsetName = String.Format("MSOffset{0}", i);
                var hasPosnName = String.Format("HasPosn{0}", i);
                var isMlatName = String.Format("IsMlat{0}", i);
                var passesName = String.Format("Passes{0}", i);
                var isOutOfBandName = String.Format("OOB{0}", i);

                if(worksheet.String(listenerNumberName) != null) {
                    var listenerNumber = worksheet.Int(listenerNumberName);
                    var icao = worksheet.String(icaoName);
                    var msOffset = worksheet.Int(msOffsetName);
                    var hasZeroPosn = worksheet.String(hasPosnName) == "0";
                    var hasPosn = hasZeroPosn ? true : worksheet.Bool(hasPosnName);
                    var isMlat = worksheet.Bool(isMlatName);
                    var resetExpected = worksheet.String(passesName) == "True";
                    var messageExpected = resetExpected || worksheet.String(passesName) == "NoReset";
                    var isOutOfBand = worksheet.Bool(isOutOfBandName);

                    Mock<IListener> listener;
                    switch(listenerNumber) {
                        case 1:     listener = _Listener1; break;
                        case 2:     listener = _Listener2; break;
                        default:    throw new NotImplementedException();
                    }

                    _Clock.UtcNowValue = startTime.AddMilliseconds(msOffset);

                    var baseStationMessageEventArgs = new BaseStationMessageEventArgs(new BaseStationMessage() { Icao24 = icao });
                    if(hasZeroPosn)  baseStationMessageEventArgs.Message.Latitude = baseStationMessageEventArgs.Message.Longitude = 0.0;
                    else if(hasPosn) baseStationMessageEventArgs.Message.Latitude = baseStationMessageEventArgs.Message.Longitude = 1.0;
                    if(isMlat)       baseStationMessageEventArgs.Message.IsMlat = true;

                    var baseStationMessageEventRecorder = new EventRecorder<BaseStationMessageEventArgs>();
                    _MergedFeed.Port30003MessageReceived += baseStationMessageEventRecorder.Handler;

                    var positionResetEventArgs = new EventArgs<string>(icao);
                    var positionResetEventRecorder = new EventRecorder<EventArgs<string>>();
                    _MergedFeed.PositionReset += positionResetEventRecorder.Handler;

                    listener.Raise(r => r.Port30003MessageReceived += null, baseStationMessageEventArgs);
                    listener.Raise(r => r.PositionReset += null, positionResetEventArgs);

                    var failDetails = String.Format("Failed on message {0} {{0}}", i);
                    if(!messageExpected) {
                        Assert.AreEqual(0, baseStationMessageEventRecorder.CallCount, failDetails, "BaseStationMessage");
                        Assert.AreEqual(0, positionResetEventRecorder.CallCount, failDetails, "PostionReset");
                    } else {
                        Assert.AreEqual(1, baseStationMessageEventRecorder.CallCount, failDetails, "BaseStationMessage");
                        Assert.AreSame(_MergedFeed, baseStationMessageEventRecorder.Sender);

                        Assert.AreSame(baseStationMessageEventArgs.Message, baseStationMessageEventRecorder.Args.Message);
                        Assert.AreEqual(isOutOfBand, baseStationMessageEventRecorder.Args.IsOutOfBand);

                        if(!resetExpected) {
                            Assert.AreEqual(0, positionResetEventRecorder.CallCount, failDetails, "PostionReset");
                        } else {
                            Assert.AreEqual(1, positionResetEventRecorder.CallCount, failDetails, "PositionReset");
                            Assert.AreSame(_MergedFeed, positionResetEventRecorder.Sender);
                            Assert.AreSame(positionResetEventArgs, positionResetEventRecorder.Args);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void MergedFeedListener_SetListener_Not_Interferred_With_By_Background_ICAO_Cleanup()
        {
            _MergedFeed.SetListeners(_Components);
            _MergedFeed.Port30003MessageReceived += _BaseStationMessageEventRecorder.Handler;
            _MergedFeed.PositionReset += _PositionResetRecorder.Handler;
            var messageArgs = new BaseStationMessageEventArgs(new BaseStationMessage() { Icao24 = "123456" });
            var resetArgs = new EventArgs<string>("123456");

            _Listener1.Raise(r => r.Port30003MessageReceived += null, messageArgs);
            _Listener1.Raise(r => r.PositionReset += null, resetArgs);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(_MergedFeed.IcaoTimeout);
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _Listener2.Raise(r => r.Port30003MessageReceived += null, messageArgs);
            _Listener2.Raise(r => r.PositionReset += null, resetArgs);

            Assert.AreEqual(1, _BaseStationMessageEventRecorder.CallCount);
            Assert.AreEqual(1, _PositionResetRecorder.CallCount);
        }

        [TestMethod]
        public void MergedFeedListener_SetListener_Does_Not_Respond_To_Events_From_Old_Listeners()
        {
            _MergedFeed.SetListeners(_Components);
            _MergedFeed.Port30003MessageReceived += _BaseStationMessageEventRecorder.Handler;
            _MergedFeed.PositionReset += _PositionResetRecorder.Handler;
            var messageArgs = new BaseStationMessageEventArgs(new BaseStationMessage() { Icao24 = "123456" });
            var resetArgs = new EventArgs<string>("123456");

            _Components.Remove(_Component2.Object);
            _MergedFeed.SetListeners(_Components);
            _Listener2.Raise(r => r.Port30003MessageReceived += null, messageArgs);
            _Listener2.Raise(r => r.PositionReset += null, resetArgs);

            Assert.AreEqual(0, _BaseStationMessageEventRecorder.CallCount);
            Assert.AreEqual(0, _PositionResetRecorder.CallCount);
        }

        [TestMethod]
        public void MergedFeedListener_SetListener_Still_Considers_Old_Listeners_Offical_Sources_Of_Icaos_Until_They_Expire()
        {
            _MergedFeed.SetListeners(_Components);
            var messageArgs = new BaseStationMessageEventArgs(new BaseStationMessage() { Icao24 = "123456" });
            var resetArgs = new EventArgs<string>("123456");

            _Listener2.Raise(r => r.Port30003MessageReceived += null, messageArgs);
            _Listener2.Raise(r => r.PositionReset += null, resetArgs);
            _Listeners.Remove(_Listener2.Object);
            _MergedFeed.SetListeners(_Components);
            _MergedFeed.Port30003MessageReceived += _BaseStationMessageEventRecorder.Handler;
            _MergedFeed.PositionReset += _PositionResetRecorder.Handler;

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(_MergedFeed.IcaoTimeout);
            _Listener1.Raise(r => r.Port30003MessageReceived += null, messageArgs);
            _Listener1.Raise(r => r.PositionReset += null, resetArgs);
            Assert.AreEqual(0, _BaseStationMessageEventRecorder.CallCount);
            Assert.AreEqual(0, _PositionResetRecorder.CallCount);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(1);
            _Listener1.Raise(r => r.Port30003MessageReceived += null, messageArgs);
            _Listener1.Raise(r => r.PositionReset += null, resetArgs);
            Assert.AreEqual(1, _BaseStationMessageEventRecorder.CallCount);
            Assert.AreEqual(1, _PositionResetRecorder.CallCount);
        }
        #endregion

        #region ExceptionCaught
        [TestMethod]
        public void MergedFeedListener_ExceptionCaught_Raised_If_Exception_Thrown_By_BaseStation_Event_Handler()
        {
            _MergedFeed.ExceptionCaught += _ExceptionCaughtRecorder.Handler;
            _MergedFeed.Port30003MessageReceived += _BaseStationMessageEventRecorder.Handler;

            var exception = new InvalidCastException();
            _BaseStationMessageEventRecorder.EventRaised += (sender, args) => { throw exception; };

            _MergedFeed.SetListeners(_Components);
            _Listener1.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(new BaseStationMessage()));

            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_MergedFeed, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }
        #endregion

        #region ChangeSource
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MergedFeedListener_ChangeSource_Throws_Exception()
        {
            _MergedFeed.ChangeSource(
                TestUtilities.CreateMockInstance<IConnector>().Object,
                TestUtilities.CreateMockInstance<IMessageBytesExtractor>().Object,
                TestUtilities.CreateMockInstance<IRawMessageTranslator>().Object);
        }
        #endregion

        #region Connect
        [TestMethod]
        public void MergedFeedListener_Connect_Is_Inert()
        {
            var status = _MergedFeed.ConnectionStatus;
            _MergedFeed.Connect();
            Assert.AreEqual(status, _MergedFeed.ConnectionStatus);
        }
        #endregion

        #region Disconnect
        [TestMethod]
        public void MergedFeedListener_Disconnect_Is_Inert()
        {
            var status = _MergedFeed.ConnectionStatus;
            _MergedFeed.Disconnect();
            Assert.AreEqual(status, _MergedFeed.ConnectionStatus);
        }
        #endregion

        #region TotalMessages
        [TestMethod]
        public void MergedFeedListener_TotalMessages_Increments_When_Filter_Allows_Message_Through()
        {
            _MergedFeed.SetListeners(_Components);

            _Listener1.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(new BaseStationMessage()));

            Assert.AreEqual(1, _MergedFeed.TotalMessages);
        }

        [TestMethod]
        public void MergedFeedListener_TotalMessages_Not_Incremented_When_Filter_Blocks_Message()
        {
            _MergedFeed.SetListeners(_Components);

            _Listener1.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(new BaseStationMessage() { Icao24 = "AABBCC" }));
            _Listener2.Raise(r => r.Port30003MessageReceived += null, new BaseStationMessageEventArgs(new BaseStationMessage() { Icao24 = "AABBCC" }));

            Assert.AreEqual(1, _MergedFeed.TotalMessages);
        }
        #endregion
    }
}
