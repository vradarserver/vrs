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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Library;

namespace Test.VirtualRadar.Library.BaseStation
{
    [TestClass]
    public class BaseStationAircraftListTests
    {
        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private const int MinutesBetweenDetailRefresh = 1;

        private IClassFactory _ClassFactorySnapshot;

        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private IBaseStationAircraftList _AircraftList;
        private ClockMock _Clock;
        private Mock<IListener> _Port30003Listener;
        private Mock<IAircraftDetailFetcher> _AircraftDetailFetcher;
        private AircraftDetail _AircraftDetail;
        private bool _ReturnNullAircraftDetail;
        private BaseStationMessage _BaseStationMessage;
        private BaseStationAircraft _BaseStationAircraft;
        private Mock<ICallsignRouteFetcher> _CallsignRouteFetcher;
        private CallsignRouteDetail _CallsignRouteDetail;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Route _Route;
        private Airport _Heathrow;
        private Airport _Helsinki;
        private Airport _JohnFKennedy;
        private Airport _Boston;
        private BaseStationMessageEventArgs _BaseStationMessageEventArgs;
        private BaseStationMessageEventArgs _OobBaseStationMessageEventArgs;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtEvent;
        private EventRecorder<EventArgs> _CountChangedEvent;
        private EventRecorder<EventArgs> _TrackingStateChangedEvent;
        private Exception _BackgroundException;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Mock<IHeartbeatService> _HeartbeatService;
        private PictureDetail _PictureDetails;
        private Mock<IPolarPlotter> _PolarPlotter;
        private Mock<IAircraftSanityChecker> _SanityChecker;
        private Certainty _SaneAltitude;
        private Certainty _SanePosition;
        private Mock<IAirPressureManager> _AirPressureManager;
        private Mock<IAirPressureLookup> _AirPressureLookup;
        private AirPressure _AirPressure;

        [TestInitialize]
        public void TestInitialise()
        {
            _BackgroundException = null;

            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsTest).Returns(true);

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _ConfigurationStorage.Setup(m => m.Load()).Returns(_Configuration);

            _PictureDetails = new PictureDetail();
            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();

            _Clock = new ClockMock();
            _Clock.UtcNowValue = new DateTime(99L);
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _Port30003Listener = new Mock<IListener>().SetupAllProperties();

            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _StandingDataManager.Setup(r => r.FindRoute(It.IsAny<string>())).Returns((Route)null);

            _BaseStationMessage = new BaseStationMessage();
            _BaseStationMessage.MessageType = BaseStationMessageType.Transmission;
            _BaseStationMessage.Icao24 = "4008F6";
            _BaseStationMessageEventArgs = new BaseStationMessageEventArgs(_BaseStationMessage);
            _OobBaseStationMessageEventArgs = new BaseStationMessageEventArgs(_BaseStationMessage, isOutOfBand: true);

            _CallsignRouteFetcher = TestUtilities.CreateMockSingleton<ICallsignRouteFetcher>();
            _CallsignRouteDetail = new CallsignRouteDetail();
            _CallsignRouteFetcher.Setup(r => r.RegisterAircraft(It.IsAny<IAircraft>())).Returns((IAircraft ac) => _CallsignRouteDetail);

            _AircraftDetailFetcher = TestUtilities.CreateMockSingleton<IAircraftDetailFetcher>();
            _BaseStationAircraft = new BaseStationAircraft() { ModeS = "4008F6" };
            _AircraftDetail = new AircraftDetail() {
                Icao24 = "4008F6",
                Aircraft = _BaseStationAircraft,
                Picture = _PictureDetails,
            };
            _ReturnNullAircraftDetail = false;
            _AircraftDetailFetcher.Setup(r => r.RegisterAircraft(It.IsAny<IAircraft>())).Returns((IAircraft aircraft) => {
                return _ReturnNullAircraftDetail ? null : _AircraftDetail;
            });

            _Heathrow = new Airport() { IcaoCode = "EGLL", IataCode = "LHR", Name = "Heathrow", Country = "UK", };
            _JohnFKennedy = new Airport() { IcaoCode = "KJFK", IataCode = "JFK", Country = "USA", };
            _Helsinki = new Airport() { IataCode = "HEL", };
            _Boston = new Airport() { IcaoCode = "KBOS", };
            _Route = new Route() { From = _Heathrow, To = _JohnFKennedy, Stopovers = { _Helsinki }, };

            _ExceptionCaughtEvent = new EventRecorder<EventArgs<Exception>>();
            _CountChangedEvent = new EventRecorder<EventArgs>();
            _TrackingStateChangedEvent = new EventRecorder<EventArgs>();

            _AircraftList = Factory.Singleton.Resolve<IBaseStationAircraftList>();
            _AircraftList.ExceptionCaught += AircraftListExceptionCaughtHandler;
            _AircraftList.Listener = _Port30003Listener.Object;
            _AircraftList.StandingDataManager = _StandingDataManager.Object;

            _PolarPlotter = TestUtilities.CreateMockInstance<IPolarPlotter>();

            _SanityChecker = TestUtilities.CreateMockImplementation<IAircraftSanityChecker>();
            _SanityChecker.Setup(r => r.IsGoodAircraftIcao(It.IsAny<string>())).Returns((string r) => {
                return !String.IsNullOrEmpty(r) && r != "000000";
            });
            _SaneAltitude = Certainty.ProbablyRight;
            _SanePosition = Certainty.ProbablyRight;
            _SanityChecker.Setup(r => r.CheckAltitude(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>())).Returns((int id, DateTime date, int alt) => {
                return _SaneAltitude;
            });
            _SanityChecker.Setup(r => r.CheckPosition(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>())).Returns((int id, DateTime date, double lat, double lng) => {
                return _SanePosition;
            });

            _AirPressure = new AirPressure();
            _AirPressureLookup = TestUtilities.CreateMockInstance<IAirPressureLookup>();
            _AirPressureLookup.Setup(r => r.FindClosest(It.IsAny<double>(), It.IsAny<double>())).Returns(_AirPressure);
            _AirPressureManager = TestUtilities.CreateMockSingleton<IAirPressureManager>();
            _AirPressureManager.SetupGet(r => r.Lookup).Returns(_AirPressureLookup.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);

            if(_AircraftList != null) _AircraftList.Dispose();
            _AircraftList = null;
            Assert.IsNull(_BackgroundException, _BackgroundException == null ? "" : _BackgroundException.ToString());
        }

        private void AircraftListExceptionCaughtHandler(object sender, EventArgs<Exception> args)
        {
            _BackgroundException = args.Value;
        }
        #endregion

        #region Constructor and Properties
        [TestMethod]
        public void BaseStationAircraftList_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            _AircraftList.Dispose();
            _AircraftList = Factory.Singleton.Resolve<IBaseStationAircraftList>();

            TestUtilities.TestProperty(_AircraftList, r => r.Listener, null, _Port30003Listener.Object);
            TestUtilities.TestProperty(_AircraftList, r => r.StandingDataManager, null, _StandingDataManager.Object);
            TestUtilities.TestProperty(_AircraftList, r => r.PolarPlotter, null, _PolarPlotter.Object);
            Assert.AreEqual(AircraftListSource.BaseStation, _AircraftList.Source);
            Assert.AreEqual(0, _AircraftList.Count);
            Assert.IsFalse(_AircraftList.IsTracking);
        }

        [TestMethod]
        public void BaseStationAircraftList_BaseStationMessageRelay_Stops_Picking_Up_Messages_When_MessageRelay_Changed()
        {
            _AircraftList.Start();
            _AircraftList.Listener = new Mock<IListener>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties().Object;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsNull(_AircraftList.FindAircraft(0x4008F6));
        }

        [TestMethod]
        public void BaseStationAircraftList_Count_Reflects_Number_Of_Aircraft_Being_Tracked()
        {
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(1, _AircraftList.Count);

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(1, _AircraftList.Count);

            _BaseStationMessage.Icao24 = "7";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(2, _AircraftList.Count);
        }
        #endregion

        #region ExceptionCaught
        [TestMethod]
        public void BaseStationAircraftList_ExceptionCaught_Is_Raised_When_Exception_Is_Raised_During_Message_Processing()
        {
            _AircraftList.ExceptionCaught -= AircraftListExceptionCaughtHandler;
            _AircraftList.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            InvalidOperationException exception = new InvalidOperationException();
            _StandingDataManager.Setup(m => m.FindCodeBlock(It.IsAny<string>())).Callback(() => { throw exception; });

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(1, _ExceptionCaughtEvent.CallCount);
            Assert.AreSame(_AircraftList, _ExceptionCaughtEvent.Sender);
            Assert.AreSame(exception, _ExceptionCaughtEvent.Args.Value);
        }

        [TestMethod]
        public void BaseStationAircraftList_ExceptionCaught_Is_Not_Raised_When_Background_Thread_Stops()
        {
            // A ThreadAbortExecption should just stop the background thread silently

            _AircraftList.ExceptionCaught -= AircraftListExceptionCaughtHandler;
            _AircraftList.ExceptionCaught += _ExceptionCaughtEvent.Handler;
            InvalidOperationException exception = new InvalidOperationException();

            _AircraftList.Dispose();
            _AircraftList = null;

            Assert.AreEqual(0, _ExceptionCaughtEvent.CallCount);
        }
        #endregion

        #region CountChanged
        [TestMethod]
        public void BaseStationAircraftList_CountChanged_Raised_When_Aircraft_First_Tracked()
        {
            _AircraftList.CountChanged += _CountChangedEvent.Handler;
            _CountChangedEvent.EventRaised += (s, a) => { Assert.AreEqual(1, _AircraftList.Count); };
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(1, _CountChangedEvent.CallCount);
            Assert.AreSame(_AircraftList, _CountChangedEvent.Sender);
            Assert.AreNotEqual(null, _CountChangedEvent.Args);
        }

        [TestMethod]
        public void BaseStationAircraftList_CountChanged_Not_Raised_If_Aircraft_Already_Tracked()
        {
            _AircraftList.CountChanged += _CountChangedEvent.Handler;
            _CountChangedEvent.EventRaised += (s, a) => { Assert.AreEqual(1, _AircraftList.Count); };
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(1, _CountChangedEvent.CallCount);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void BaseStationAircraftList_Dispose_Unhooks_BaseStationMessageRelay()
        {
            _AircraftList.Start();
            _AircraftList.Dispose();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsNull(_AircraftList.FindAircraft(0x4008F6));
        }
        #endregion

        #region Start
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationAircraftList_Start_Throws_If_BaseStationMessageRelay_Not_Set()
        {
            _AircraftList.Listener = null;
            _AircraftList.Start();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BaseStationAircraftList_Start_Throws_If_StandingDataManager_Not_Set()
        {
            _AircraftList.StandingDataManager = null;
            _AircraftList.Start();
        }

        [TestMethod]
        public void BaseStationAircraftList_Start_Sets_IsTracking()
        {
            _AircraftList.Start();
            Assert.IsTrue(_AircraftList.IsTracking);
        }

        [TestMethod]
        public void BaseStationAircraftList_Start_Raises_TrackingStateChanged()
        {
            _AircraftList.TrackingStateChanged += _TrackingStateChangedEvent.Handler;
            _AircraftList.Start();

            Assert.AreEqual(1, _TrackingStateChangedEvent.CallCount);
        }

        [TestMethod]
        public void BaseStationAircraftList_Start_Does_Not_Raise_TrackingStateChanged_If_Already_Tracking()
        {
            _AircraftList.Start();

            _AircraftList.TrackingStateChanged += _TrackingStateChangedEvent.Handler;
            _AircraftList.Start();
            Assert.AreEqual(0, _TrackingStateChangedEvent.CallCount);
        }

        [TestMethod]
        public void BaseStationAircraftList_Ignores_Aircraft_Messages_Until_Start_Is_Called()
        {
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(0, _AircraftList.Count);

            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(1, _AircraftList.Count);
        }
        #endregion

        #region Stop
        [TestMethod]
        public void BaseStationAircraftList_Stop_Clears_IsTracking()
        {
            _AircraftList.Start();
            _AircraftList.Stop();

            Assert.IsFalse(_AircraftList.IsTracking);
        }

        [TestMethod]
        public void BaseStationAircraftList_Stop_Raises_TrackingStateChanged()
        {
            _AircraftList.Start();

            _AircraftList.TrackingStateChanged += _TrackingStateChangedEvent.Handler;
            _AircraftList.Stop();

            Assert.AreEqual(1, _TrackingStateChangedEvent.CallCount);
        }

        [TestMethod]
        public void BaseStationAircraftList_Stop_Does_Not_Raise_TrackingStateChanged_If_Already_Stopped()
        {
            _AircraftList.Start();
            _AircraftList.Stop();

            _AircraftList.TrackingStateChanged += _TrackingStateChangedEvent.Handler;
            _AircraftList.Stop();

            Assert.AreEqual(0, _TrackingStateChangedEvent.CallCount);
        }

        [TestMethod]
        public void BaseStationAircraftList_Stop_Prevents_Further_Processing_Of_Messages()
        {
            _AircraftList.Start();
            _AircraftList.Stop();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(0, _AircraftList.Count);
        }

        [TestMethod]
        public void BaseStationAircraftList_Stop_Can_Be_Reversed_By_Calling_Start()
        {
            _AircraftList.Start();
            _AircraftList.Stop();
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(1, _AircraftList.Count);
        }

        [TestMethod]
        public void BaseStationAircraftList_Stop_Removes_All_Tracked_Aircraft()
        {
            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _AircraftList.Stop();

            Assert.AreEqual(0, _AircraftList.Count);
        }

        [TestMethod]
        public void BaseStationAircraftList_Stop_Raises_CountChanged()
        {
            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _AircraftList.CountChanged += _CountChangedEvent.Handler;
            _AircraftList.Stop();

            Assert.AreEqual(1, _CountChangedEvent.CallCount);
        }
        #endregion

        #region FindAircraft
        [TestMethod]
        public void BaseStationAircraftList_FindAircraft_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_AircraftList.FindAircraft(1));
        }

        [TestMethod]
        public void BaseStationAircraftList_FindAircraft_Returns_Aircraft_Matching_UniqueId()
        {
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsNotNull(_AircraftList.FindAircraft(0x4008F6));
        }

        [TestMethod]
        public void BaseStationAircraftList_FindAircraft_Returns_Clone()
        {
            // The object returned by FindAircraft must not be affected by any further messages arriving from the listener
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            IAircraft aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.IsNotNull(aircraft);

            _BaseStationMessage.Squawk = 1234;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsNull(aircraft.Squawk);
        }
        #endregion

        #region TakeSnapshot
        [TestMethod]
        public void BaseStationAircraftList_TakeSnapshot_Returns_Empty_List_When_No_Aircraft_Are_Visible()
        {
            var time = DateTime.UtcNow;
            _Clock.UtcNowValue = time;

            _AircraftList.Start();

            long timeStamp, dataVersion;
            var list = _AircraftList.TakeSnapshot(out timeStamp, out dataVersion);

            Assert.AreEqual(0, list.Count);
            Assert.AreEqual(time.Ticks, timeStamp);
            Assert.AreEqual(-1L, dataVersion);
        }

        [TestMethod]
        public void BaseStationAircraftList_TakeSnapshot_Returns_List_Of_Known_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "123456";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Icao24 = "ABCDEF";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            long o1, o2;
            var list = _AircraftList.TakeSnapshot(out o1, out o2);
            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.Where(ac => ac.Icao24 == "123456").Any());
            Assert.IsTrue(list.Where(ac => ac.Icao24 == "ABCDEF").Any());
        }

        [TestMethod]
        public void BaseStationAircraftList_TakeSnapshot_Fills_In_Current_Time_And_Latest_DataVersion()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Icao24 = "2";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var expectedDataVersion = _Clock.UtcNowValue.Ticks + 2;     // DataVersion was initialised to 99, 2 messages came in incrementing it twice

            long timeStamp, dataVersion;
            var list = _AircraftList.TakeSnapshot(out timeStamp, out dataVersion);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(_Clock.UtcNowValue.Ticks, timeStamp);
            Assert.AreEqual(expectedDataVersion, dataVersion);
        }

        [TestMethod]
        public void BaseStationAircraftList_TakeSnapshot_Returns_Aircraft_Clones()
        {
            // Messages that come in after the list has been established must not affect the aircraft in the snapshot

            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "123456";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            long o1, o2;
            List<IAircraft> list = _AircraftList.TakeSnapshot(out o1, out o2);
            Assert.AreEqual(1, list.Count);

            _BaseStationMessage.Squawk = 1234;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(null, list[0].Squawk);
        }

        [TestMethod]
        public void BaseStationAircraftList_TakeSnapshot_Does_Not_Show_Aircraft_Past_The_Hide_Threshold()
        {
            _Configuration.BaseStationSettings.DisplayTimeoutSeconds = 10;
            var time = DateTime.Now;

            _AircraftList.Start();

            _Clock.UtcNowValue = time;
            _BaseStationMessage.Icao24 = "1";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(10);
            long o1, o2;
            Assert.AreEqual(1, _AircraftList.TakeSnapshot(out o1, out o2).Count);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(1);
            Assert.AreEqual(0, _AircraftList.TakeSnapshot(out o1, out o2).Count);
        }

        [TestMethod]
        public void BaseStationAircraftList_TakeSnapshot_Takes_Account_Of_Changes_To_Hide_Threshold()
        {
            _Configuration.BaseStationSettings.DisplayTimeoutSeconds = 10;
            var time = DateTime.Now;

            _AircraftList.Start();

            _Clock.UtcNowValue = time;
            _BaseStationMessage.Icao24 = "1";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _Configuration.BaseStationSettings.DisplayTimeoutSeconds = 9;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _Clock.UtcNowValue = time.AddSeconds(9).AddMilliseconds(1);
            long o1, o2;
            Assert.AreEqual(0, _AircraftList.TakeSnapshot(out o1, out o2).Count);
        }

        [TestMethod]
        public void BaseStationAircraftList_TakeSnapshot_Does_Not_Delete_Aircraft_When_Hidden()
        {
            _Configuration.BaseStationSettings.DisplayTimeoutSeconds = 10;
            var time = DateTime.Now;

            _AircraftList.Start();

            _Clock.UtcNowValue = time;
            _BaseStationMessage.Icao24 = "1";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _Clock.UtcNowValue = time.AddSeconds(10).AddMilliseconds(1);
            long o1, o2;
            _AircraftList.TakeSnapshot(out o1, out o2);

            Assert.IsNotNull(_AircraftList.FindAircraft(1));
        }
        #endregion

        #region MessageReceived
        #region Basics
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Adds_Aircraft_To_List()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "7";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Icao24 = "5";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft1 = _AircraftList.FindAircraft(7);
            Assert.IsNotNull(aircraft1);
            Assert.AreEqual("7", aircraft1.Icao24);

            var aircraft2 = _AircraftList.FindAircraft(5);
            Assert.IsNotNull(aircraft2);
            Assert.AreEqual("5", aircraft2.Icao24);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Sets_ReceiverId_On_New_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";
            _BaseStationMessage.ReceiverId = 1234;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(1);
            Assert.AreEqual(1234, aircraft.ReceiverId);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Sets_ReceiverId_On_Existing_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";

            _BaseStationMessage.ReceiverId = 1234;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.ReceiverId = 9876;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(1);
            Assert.AreEqual(9876, aircraft.ReceiverId);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Sets_SignalLevel_On_New_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";
            _BaseStationMessage.SignalLevel = 1234;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(1);
            Assert.AreEqual(1234, aircraft.SignalLevel);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Sets_SignalLevel_On_Existing_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";

            _BaseStationMessage.SignalLevel = 1234;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.SignalLevel = 9876;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(1);
            Assert.AreEqual(9876, aircraft.SignalLevel);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Can_Overwrite_NonNull_SignalLevel_With_Null()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";

            _BaseStationMessage.SignalLevel = 1234;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.SignalLevel = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(1);
            Assert.AreEqual(null, aircraft.SignalLevel);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Sets_PositionIsMlat_When_Position_Is_MLAT()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";
            _BaseStationMessage.Latitude = 1;
            _BaseStationMessage.Longitude = 2;
            _BaseStationMessage.IsMlat = true;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(1);
            Assert.AreEqual(1.0, aircraft.Latitude);
            Assert.AreEqual(2.0, aircraft.Longitude);
            Assert.IsTrue(aircraft.PositionIsMlat.Value);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Leaves_PositionIsMlat_When_Position_Is_Not_Supplied()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";
            _BaseStationMessage.IsMlat = true;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(1);
            Assert.IsNull(aircraft.PositionIsMlat);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Clears_PositionIsMlat_When_Position_Is_No_Longer_MLAT()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";
            _BaseStationMessage.Latitude = 1;
            _BaseStationMessage.Longitude = 2;
            _BaseStationMessage.IsMlat = true;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.IsMlat = false;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(1);
            Assert.IsFalse(aircraft.PositionIsMlat.Value);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Increments_Count_Of_Messages()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "7";

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(1, _AircraftList.FindAircraft(7).CountMessagesReceived);

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(2, _AircraftList.FindAircraft(7).CountMessagesReceived);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Only_Responds_To_Transmission_Messages()
        {
            _AircraftList.Start();
            foreach(BaseStationMessageType messageType in Enum.GetValues(typeof(BaseStationMessageType))) {
                _BaseStationMessage.Icao24 = ((int)messageType).ToString();
                _BaseStationMessage.MessageType = messageType;
                _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            }

            long o1, o2;
            var list = _AircraftList.TakeSnapshot(out o1, out o2);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual((int)BaseStationMessageType.Transmission, list[0].UniqueId);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Ignored_If_AircraftList_Not_Yet_Started()
        {
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsNull(_AircraftList.FindAircraft(0x4008f6));
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_UniqueId_Is_Derived_From_Icao24()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "ABC123";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            long unused1, unused2;
            var list = _AircraftList.TakeSnapshot(out unused1, out unused2);
            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(0xABC123, list[0].UniqueId);
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "InvalidIcao$")]
        public void BaseStationAircraftList_MessageReceived_Ignores_Messages_With_Invalid_Icao24()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _AircraftList.Start();

            _BaseStationMessage.Icao24 = worksheet.EString("Icao");
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            bool expectValid = worksheet.Bool("IsValid");
            if(!expectValid) {
                long snapshotTime, snapshotDataVersion;
                Assert.AreEqual(0, _AircraftList.TakeSnapshot(out snapshotTime, out snapshotDataVersion).Count);
            } else {
                Assert.IsNotNull(_AircraftList.FindAircraft(worksheet.Int("UniqueId")));
            }
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Updates_LastUpdate_Time()
        {
            var messageTime1 = new DateTime(2001, 1, 1, 10, 20, 21);
            var messageTime2 = new DateTime(2001, 1, 1, 10, 20, 22);

            _AircraftList.Start();

            _Clock.UtcNowValue = messageTime1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(messageTime1, _AircraftList.FindAircraft(0x4008f6).LastUpdate);

            _Clock.UtcNowValue = messageTime2;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(messageTime2, _AircraftList.FindAircraft(0x4008f6).LastUpdate);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Sets_FirstSeen_On_First_Message_For_Aircraft()
        {
            var messageTime1 = new DateTime(2001, 1, 1, 10, 20, 21);
            var messageTime2 = new DateTime(2001, 1, 1, 10, 20, 22);

            _AircraftList.Start();

            _Clock.UtcNowValue = messageTime1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(messageTime1, _AircraftList.FindAircraft(0x4008f6).FirstSeen);

            _Clock.UtcNowValue = messageTime2;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(messageTime1, _AircraftList.FindAircraft(0x4008f6).FirstSeen);
        }
        #endregion

        #region Sanity checking
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Checks_Sanity_Of_Icao()
        {
            var now = DateTime.UtcNow;
            _Clock.UtcNowValue = now;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _SanityChecker.Verify(r => r.IsGoodAircraftIcao("4008F6"), Times.Once());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Checks_Sanity_Of_Altitudes()
        {
            var now = DateTime.UtcNow;
            _Clock.UtcNowValue = now;
            _BaseStationMessage.Altitude = 100;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _SanityChecker.Verify(r => r.CheckAltitude(0x4008f6, now, 100), Times.Once());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Does_Not_Sanity_Check_Altitude_When_Message_Has_No_Altitude()
        {
            _BaseStationMessage.Altitude = null;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _SanityChecker.Verify(r => r.CheckAltitude(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Does_Not_Sanity_Check_Altitude_When_Aircraft_Is_On_Ground()
        {
            _BaseStationMessage.Altitude = 100;
            _BaseStationMessage.OnGround = true;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _SanityChecker.Verify(r => r.CheckAltitude(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Ignores_Messages_With_CertainlyWrong_Altitudes()
        {
            _BaseStationMessage.Altitude = 100;
            _SaneAltitude = Certainty.CertainlyWrong;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsNull(_AircraftList.FindAircraft(0x4008f6));
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Accepts_Messages_With_Uncertain_Altitudes()
        {
            _BaseStationMessage.Altitude = 100;
            _SaneAltitude = Certainty.Uncertain;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsNotNull(_AircraftList.FindAircraft(0x4008f6));
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Checks_Sanity_Of_Positions()
        {
            var now = DateTime.UtcNow;
            _Clock.UtcNowValue = now;
            _BaseStationMessage.Latitude = 1;
            _BaseStationMessage.Longitude = 2;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _SanityChecker.Verify(r => r.CheckPosition(0x4008f6, now, 1.0, 2.0), Times.Once());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Does_Not_Sanity_Check_Position_When_Message_Has_No_Latitude()
        {
            _BaseStationMessage.Latitude = null;
            _BaseStationMessage.Longitude = 1.0;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _SanityChecker.Verify(r => r.CheckPosition(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Does_Not_Sanity_Check_Position_When_Message_Has_No_Longitude()
        {
            _BaseStationMessage.Latitude = 1.0;
            _BaseStationMessage.Longitude = null;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _SanityChecker.Verify(r => r.CheckPosition(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Does_Not_Sanity_Check_Position_When_Latitude_And_Longitude_Are_Zero()
        {
            _BaseStationMessage.Latitude = 0;
            _BaseStationMessage.Longitude = 0;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _SanityChecker.Verify(r => r.CheckPosition(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Doe_Sanity_Check_Position_When_Either_Latitude_Or_Longitude_Are_Not_Zero()
        {
            _BaseStationMessage.Latitude = 0.1;
            _BaseStationMessage.Longitude = 0;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _SanityChecker.Verify(r => r.CheckPosition(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<double>(), It.IsAny<double>()), Times.Once());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Ignores_Messages_With_CertainlyWrong_Positions()
        {
            _BaseStationMessage.Latitude = 1;
            _BaseStationMessage.Longitude = 1;
            _SanePosition = Certainty.CertainlyWrong;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsNull(_AircraftList.FindAircraft(0x4008f6));
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Accepts_Messages_With_Uncertain_Positions()
        {
            _BaseStationMessage.Latitude = 1;
            _BaseStationMessage.Longitude = 1;
            _SanePosition = Certainty.Uncertain;
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsNotNull(_AircraftList.FindAircraft(0x4008f6));
        }
        #endregion

        #region DataVersion handling
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_DataVersion_Updated()
        {
            _ReturnNullAircraftDetail = true;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008f6);
            Assert.AreNotEqual(0, aircraft.DataVersion);
            Assert.AreEqual(aircraft.DataVersion, aircraft.Icao24Changed);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_DataVersion_Increments_If_UtcNow_Is_Before_Current_DataVersion()
        {
            // This can happen if the clock is reset while the program is running. DataVersion must never go backwards.
            _AircraftList.Start();
            _Clock.UtcNowValue = new DateTime(100);
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            _Clock.UtcNowValue = new DateTime(90);
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008f6);
            Assert.IsTrue(aircraft.DataVersion > 100);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_DataVersion_Is_Maintained_Across_All_Aircraft()
        {
            // The dataversion needs to increment for each aircraft so that a single dataversion value can be sent to the browser
            // and then, when it's sent back to us by the browser, we know for certain what has changed since the last time the
            // browser was sent the aircraft list.
            _AircraftList.Start();
            _BaseStationMessageEventArgs.Message.Icao24 = "1";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            _BaseStationMessageEventArgs.Message.Icao24 = "2";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(100, _AircraftList.FindAircraft(1).DataVersion);
            Assert.AreEqual(101, _AircraftList.FindAircraft(2).DataVersion);
        }
        #endregion

        #region Message fields transcribed to aircraft correctly
        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "TranslateMessage$")]
        public void BaseStationAircraftList_MessageReceived_Translates_Message_Properties_Into_New_Aircraft_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _AircraftList.Start();

            object messageObject;
            PropertyInfo messageProperty;
            var messageColumn = worksheet.String("MessageColumn");
            if(!messageColumn.StartsWith("S:")) {
                messageObject = _BaseStationMessage;
                messageProperty = typeof(BaseStationMessage).GetProperty(messageColumn);
            } else {
                messageColumn = messageColumn.Substring(2);
                messageObject = _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage();
                messageProperty = typeof(BaseStationSupplementaryMessage).GetProperty(messageColumn);
            }

            var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftColumn"));

            var culture = new CultureInfo("en-GB");
            var messageValue = TestUtilities.ChangeType(worksheet.EString("MessageValue"), messageProperty.PropertyType, culture);
            var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, culture);

            messageProperty.SetValue(messageObject, messageValue, null);

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(aircraftValue, aircraftProperty.GetValue(aircraft, null));
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "TranslateMessage$")]
        public void BaseStationAircraftList_MessageReceived_Translates_Message_Properties_Into_Existing_Aircraft_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            object messageObject;
            PropertyInfo messageProperty;
            var messageColumn = worksheet.String("MessageColumn");
            if(!messageColumn.StartsWith("S:")) {
                messageObject = _BaseStationMessage;
                messageProperty = typeof(BaseStationMessage).GetProperty(messageColumn);
            } else {
                messageColumn = messageColumn.Substring(2);
                messageObject = _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage();
                messageProperty = typeof(BaseStationSupplementaryMessage).GetProperty(messageColumn);
            }

            var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftColumn"));

            var culture = new CultureInfo("en-GB");
            var messageValue = TestUtilities.ChangeType(worksheet.EString("MessageValue"), messageProperty.PropertyType, culture);
            var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, culture);

            messageProperty.SetValue(messageObject, messageValue, null);

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(aircraftValue, aircraftProperty.GetValue(aircraft, null));
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "TranslateMessage$")]
        public void BaseStationAircraftList_MessageReceived_Does_Not_Null_Out_Existing_Message_Values()
        {
            // We need to make sure that we cope when we get a message that sets a value and then another message with a null for the same value
            var worksheet = new ExcelWorksheetData(TestContext);

            _AircraftList.Start();

            object messageObject;
            PropertyInfo messageProperty;
            var messageColumn = worksheet.String("MessageColumn");
            if(!messageColumn.StartsWith("S:")) {
                messageObject = _BaseStationMessage;
                messageProperty = typeof(BaseStationMessage).GetProperty(messageColumn);
            } else {
                messageColumn = messageColumn.Substring(2);
                messageObject = _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage();
                messageProperty = typeof(BaseStationSupplementaryMessage).GetProperty(messageColumn);
            }

            // Up to and including version 2.1 all values being tested here were nullable. After 2.1 the IsTisb field
            // was added, and that's unconditionally set or cleared by either the message source or the raw message decoder.
            // We can ignore that field.
            var ignoreThisRow = false;
            switch(messageColumn) {
                case "IsTisb":
                    ignoreThisRow = true;
                    break;
            }

            if(!ignoreThisRow) {
                var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftColumn"));

                var culture = new CultureInfo("en-GB");
                var messageValue = TestUtilities.ChangeType(worksheet.EString("MessageValue"), messageProperty.PropertyType, culture);
                var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, culture);

                messageProperty.SetValue(messageObject, messageValue, null);
                _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

                var aircraft = _AircraftList.FindAircraft(0x4008F6);
                Assert.AreEqual(aircraftValue, aircraftProperty.GetValue(aircraft, null));

                messageProperty.SetValue(messageObject, null, null);
                _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

                var outerAircraft = _AircraftList.FindAircraft(0x4008F6);
                Assert.AreEqual(aircraftValue, aircraftProperty.GetValue(outerAircraft, null));
            }
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Sets_IsTransmittingTrack_Once_Track_Is_Seen()
        {
            _AircraftList.Start();

            _BaseStationMessage.Track = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008f6);
            Assert.IsFalse(aircraft.IsTransmittingTrack);

            _BaseStationMessage.Track = 100f;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            aircraft = _AircraftList.FindAircraft(0x4008f6);
            Assert.IsTrue(aircraft.IsTransmittingTrack);

            _BaseStationMessage.Track = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.IsTrue(_AircraftList.FindAircraft(0x4008f6).IsTransmittingTrack);
        }
        #endregion

        #region Emergency derived from squawk
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Message_Emergency_Is_Ignored()
        {
            // The reason it is ignored is because some feed aggregators would incorrectly set the Emergency flag even though
            // the squawk was clearly indicating that there was no emergency. This renders the flag unusable, we need to
            // figure the correct value out for ourselves. Luckily it isn't difficult :)

            for(int i = 0;i < 2;++i) {
                TestCleanup();
                TestInitialise();

                _BaseStationMessage.Emergency = i == 0;
                _AircraftList.Start();
                _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

                var aircraft = _AircraftList.FindAircraft(0x4008f6);
                Assert.IsNull(aircraft.Emergency);
            }
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Emergency_Is_Derived_From_Squawk()
        {
            _AircraftList.Start();

            // Run through every possible code. In real life the squawk is octal presented as decimal so some of these
            // values would never appear (0008, 0080 etc.) but C# doesn't do octal very well and the code doesn't really
            // care if they're valid octal or not
            for(int i = -1;i < 7777;++i) {
                _BaseStationMessage.Squawk = i == -1 ? (int?)null : i;

                _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

                bool? expected = i == -1 ? (bool?)null : i == 7500 || i == 7600 || i == 7700;
                var aircraft = _AircraftList.FindAircraft(0x4008f6);
                Assert.AreEqual(expected, aircraft.Emergency, i.ToString());
            }
        }
        #endregion

        #region Track calculation
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Calculates_Track_If_Position_Transmitted_Without_Track()
        {
            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Longitude = 7;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(89.6F, (float)_AircraftList.FindAircraft(0x4008f6).Track);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Calculates_Track_If_Position_Transmitted_With_Exclusively_Zero_Track()
        {
            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _BaseStationMessage.Track = 0f;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Longitude = 7;
            _BaseStationMessage.Track = 0f;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(89.6F, (float)_AircraftList.FindAircraft(0x4008f6).Track);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Continues_To_Use_Calculated_Track_Between_Position_Updates()
        {
            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _BaseStationMessage.Track = 0f;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Longitude = 7;
            _BaseStationMessage.Track = 0f;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(89.6F, (float)_AircraftList.FindAircraft(0x4008f6).Track);

            _BaseStationMessage.Latitude = null;
            _BaseStationMessage.Longitude = null;
            _BaseStationMessage.Track = 0f;
            _BaseStationMessage.Callsign = "Changed";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008f6);
            Assert.AreEqual("Changed", aircraft.Callsign);
            Assert.AreEqual(89.6F, (float)aircraft.Track);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Calculates_Track_Only_After_Aircraft_Has_Travelled_At_Least_250_Metres_When_Airborne()
        {
            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Longitude = 6.0035; // distance should be 244.9 metres
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(null, _AircraftList.FindAircraft(0x4008f6).Track);

            _BaseStationMessage.Longitude = 6.0036; // distance from first contact now 251 metres
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(90F, (float)_AircraftList.FindAircraft(0x4008f6).Track);

            _BaseStationMessage.Latitude = 51.002;
            _BaseStationMessage.Longitude = 6.005; // distance from last calculated track now 243 metres
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(90F, (float)_AircraftList.FindAircraft(0x4008f6).Track);  // <-- track shouldn't be recalculated until we're > 250 metres from last calculated position

            _BaseStationMessage.Latitude = 51.0021; // distance from last calculated track now 253 metres
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(22.8F, (float)_AircraftList.FindAircraft(0x4008f6).Track);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Calculates_Track_Only_After_Aircraft_Has_Travelled_At_Least_10_Metres_When_On_Ground()
        {
            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _BaseStationMessage.OnGround = true;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Longitude = 6.000141; // distance should be 9.9 metres at 90°
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(null, _AircraftList.FindAircraft(0x4008f6).Track);

            _BaseStationMessage.Longitude = 6.000157; // distance from first contact now 10.1 metres
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(90F, (float)_AircraftList.FindAircraft(0x4008f6).Track, 0.0001f);

            _BaseStationMessage.Latitude = 51.000089;
            _BaseStationMessage.Longitude = 6.000161; // distance from last calculated track now 9.9 metres at 1.6°
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(90F, (float)_AircraftList.FindAircraft(0x4008f6).Track, 0.0001f);  // <-- track shouldn't be recalculated until we're > 5 metres from last calculated position

            _BaseStationMessage.Latitude = 51.000099;
            _BaseStationMessage.Longitude = 6.000161; // distance from last calculated track now 10.1 metres
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(1.5F, (float)_AircraftList.FindAircraft(0x4008f6).Track);  // rounding errors force it to 1.5
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Does_Not_Calculate_Airborne_Track_If_Aircraft_Transmits_Track()
        {
            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _BaseStationMessage.Track = 35;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Latitude = 52;
            _BaseStationMessage.Track = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Latitude = 53;
            _BaseStationMessage.Track = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(35f, _AircraftList.FindAircraft(0x4008f6).Track); // If this is 0 then it erroneously calculated the track based on the two position updates with no track
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Calculates_Ground_Track_Even_If_Aircraft_Previously_Transmitted_Track()
        {
            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _BaseStationMessage.OnGround = true;
            _BaseStationMessage.Track = 35;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Longitude = 7;
            _BaseStationMessage.Track = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Longitude = 8;
            _BaseStationMessage.Track = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(89.6F, (float)_AircraftList.FindAircraft(0x4008f6).Track); // If this is 35 then it did not calculate the track based on the two position updates with no track
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Calculates_Ground_Track_If_Aircraft_Track_Has_Locked_On_Ground()
        {
            // 757s appear to have a problem with transmitting the correct track when the aircraft starts on the ground. The track
            // transmitted in SurfacePosition records remains as-at the heading it was pointing in when the aircraft started until
            // the aircraft becomes airborne. The reverse is not true - after a 757-200 lands it will transmit the correct track
            // in surface position messages (unless the speed drops below ~7.5 knots, when it stops transmitting track altogether).

            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _BaseStationMessage.OnGround = true;
            _BaseStationMessage.Track = 2.5F;
            _BaseStationMessage.OnGround = true;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // Move 90° for 10 metres
            _BaseStationMessage.Longitude = 6.000143;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(90F, (float)_AircraftList.FindAircraft(0x4008f6).Track);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Resets_Locks_Ground_Track_Timer_After_Thirty_Minutes()
        {
            // Another kludge for Fedex's 757 fleet. When they touch down at an airport the track will be correctly transmitted
            // during the taxi to the apron. The aircraft will then sit there for a few hours and continue transmissions. When it
            // starts up and taxis to take-off the track will be locked. Because the ground track was originally changing the lock
            // will not be detected and we get the same screwy track as before.
            //
            // The kludge is to record the time of the first ground track and then reset it every 30 minutes. That way the original
            // good track is forgotten about by the time the aircraft taxis to takeoff.

            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _BaseStationMessage.OnGround = true;
            _BaseStationMessage.Track = 2.5F;
            _BaseStationMessage.OnGround = true;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // Transmit a new track on the ground a minute later after moving 270° and 10 metres
            _BaseStationMessage.Track = 270.0F;
            _BaseStationMessage.Longitude = 5.999857;
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMinutes(1);
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // Wait 29 minutes from the original transmission (we've already added 1 minute so we add another 28)
            // and send another message 90° and 10 metres away from the original transmission
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMinutes(28);
            _BaseStationMessage.Longitude = 6.0;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // The track will still show 270 because the 30 minute timeout hasn't expired
            Assert.AreEqual(270F, (float)_AircraftList.FindAircraft(0x4008f6).Track);

            // Send another message 1 minute and 1 millisecond later, another 10 metres and 90° from the original
            // position. This should reset the detection of a locked track but because we don't have any points to
            // compare to we'll still be trusting the track from the aircraft.
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMinutes(1).AddMilliseconds(1);
            _BaseStationMessage.Longitude = 6.000143;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            Assert.AreEqual(270F, (float)_AircraftList.FindAircraft(0x4008f6).Track);

            // Finally if we send another message a second later then we should see the calculated track
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddSeconds(1);
            _BaseStationMessage.Longitude = 6.000286;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(90F, (float)_AircraftList.FindAircraft(0x4008f6).Track);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Reports_Previously_Calculated_Ground_Track_For_Surface_Aircraft_With_Locked_Tracks()
        {
            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _BaseStationMessage.OnGround = true;
            _BaseStationMessage.Track = 2.5F;
            _BaseStationMessage.OnGround = true;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // Move 90° for 10 metres
            _BaseStationMessage.Longitude = 6.000143;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // Move 100° for 9 metres. This is below the track calculation threshold.
            _BaseStationMessage.Latitude = 50.999986;
            _BaseStationMessage.Longitude = 6.000270;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(90F, (float)_AircraftList.FindAircraft(0x4008f6).Track);  // if this is 100 then the track was erroneously calculated. If it's 2.5 it didn't reuse the previously calculated track.
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Reports_Previously_Calculated_Ground_Track_For_Surface_Aircraft_That_Do_Not_Move()
        {
            _AircraftList.Start();

            _BaseStationMessage.Latitude = 51;
            _BaseStationMessage.Longitude = 6;
            _BaseStationMessage.OnGround = true;
            _BaseStationMessage.Track = 2.5F;
            _BaseStationMessage.OnGround = true;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // Move 90° for 10 metres
            _BaseStationMessage.Longitude = 6.000143;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // Now don't move at all
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(90F, (float)_AircraftList.FindAircraft(0x4008f6).Track);  // if this is 2.5 then the frozen track was used when the aircraft reported the same position twice
        }
        #endregion

        #region Trail lists
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Calls_UpdateCoordinates_On_Aircraft()
        {
            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 74;

            var aircraft = TestUtilities.CreateMockImplementation<IAircraft>();
            aircraft.Setup(a => a.UpdateCoordinates(It.IsAny<DateTime>(), It.IsAny<int>())).Callback(() => {
                Assert.AreEqual(1.0001, aircraft.Object.Latitude);
                Assert.AreEqual(1.0002, aircraft.Object.Longitude);
                Assert.AreEqual(1F, aircraft.Object.Track);
            });

            _AircraftList.Start();

            _BaseStationMessage.Latitude = 1.0001;
            _BaseStationMessage.Longitude = 1.0002;
            _BaseStationMessage.Track = 1F;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            aircraft.Verify(a => a.UpdateCoordinates(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once());
            aircraft.Verify(a => a.UpdateCoordinates(_Clock.UtcNowValue, 74), Times.Once());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Does_Not_Call_UpdateCoordinates_If_There_Is_No_Position_Altitude_Or_Speed_Is_In_Message()
        {
            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 74;

            var aircraft = TestUtilities.CreateMockImplementation<IAircraft>();

            _AircraftList.Start();

            _BaseStationMessage.Latitude = null;
            _BaseStationMessage.Longitude = null;
            _BaseStationMessage.Track = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            aircraft.Verify(a => a.UpdateCoordinates(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Does_Call_UpdateCoordinates_If_There_Is_Position_But_No_Altitude_Or_Speed()
        {
            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 74;

            var aircraft = TestUtilities.CreateMockImplementation<IAircraft>();

            _AircraftList.Start();

            _BaseStationMessage.Latitude = 1.2;
            _BaseStationMessage.Longitude = 2.3;
            _BaseStationMessage.Track = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            aircraft.Verify(a => a.UpdateCoordinates(It.IsAny<DateTime>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Picks_Up_Configuration_Changes_To_Short_Coordinates_Length()
        {
            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 74;

            var aircraft = TestUtilities.CreateMockImplementation<IAircraft>();

            _AircraftList.Start();

            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 92;
            _ConfigurationStorage.Raise(c => c.ConfigurationChanged += null, EventArgs.Empty);

            _BaseStationMessage.Latitude = 1.0001;
            _BaseStationMessage.Longitude = 1.0002;
            _BaseStationMessage.Track = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            aircraft.Verify(a => a.UpdateCoordinates(_Clock.UtcNowValue, 92), Times.Once());
        }
        #endregion

        #region Database lookup
        [TestMethod]
        public void BaseStationAircraftList_Registers_New_Aircraft_With_AircraftDetailFetcher()
        {
            _AircraftList.Start();

            int callCount = 0;
            IAircraft aircraft = null;
            _AircraftDetailFetcher.Setup(r => r.RegisterAircraft(It.IsAny<IAircraft>())).Callback((IAircraft ac) => {
                aircraft = ac;
                ++callCount;
            });

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(1, callCount);
            Assert.AreEqual("4008F6", aircraft.Icao24);
        }

        [TestMethod]
        public void BaseStationAircraftList_Registers_Existing_Aircraft_With_AircraftDetailFetcher()
        {
            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            int callCount = 0;
            IAircraft aircraft = null;
            _AircraftDetailFetcher.Setup(r => r.RegisterAircraft(It.IsAny<IAircraft>())).Callback((IAircraft ac) => {
                aircraft = ac;
                ++callCount;
            });

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            Assert.AreEqual(1, callCount);
            Assert.AreEqual("4008F6", aircraft.Icao24);
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "DatabaseFetch$")]
        public void BaseStationAircraftList_Applies_Result_From_AircraftDetailFetcher_RegisterAircraft()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _AircraftList.Start();
            
            var databaseProperty = typeof(BaseStationAircraft).GetProperty(worksheet.String("DatabaseColumn"));
            var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftColumn"));
            var culture = new CultureInfo("en-GB");
            var databaseValue = TestUtilities.ChangeType(worksheet.EString("DatabaseValue"), databaseProperty.PropertyType, culture);
            var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, culture);
            databaseProperty.SetValue(_BaseStationAircraft, databaseValue, null);

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(aircraftValue, aircraftProperty.GetValue(aircraft, null));
        }

        [TestMethod]
        public void BaseStationAircraftList_Applies_Flights_Count_From_AircraftDetailFetcher_RegisterAircraft()
        {
            _AircraftList.Start();
            
            _AircraftDetail.FlightsCount = 42;

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(42, aircraft.FlightsCount);
        }

        [TestMethod]
        public void BaseStationAircraftList_Does_Not_Crash_If_AircraftDetailFetcher_RegisterAircraft_Returns_Null()
        {
            _AircraftList.Start();
            
            _AircraftDetail = null;

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
        }

        [TestMethod]
        public void BaseStationAircraftList_Applies_Changes_If_AircraftDetailFetcher_Fetched_Raised_With_Null_Aircraft()
        {
            _AircraftList.Start();

            _AircraftDetail.Aircraft = _BaseStationAircraft;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _AircraftDetail.Aircraft = null;
            _AircraftDetailFetcher.Raise(r => r.Fetched += null, new EventArgs<AircraftDetail>(_AircraftDetail));

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.IsNull(aircraft.Registration);
        }

        [TestMethod]
        [DataSource("Data Source='AircraftTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "DatabaseFetch$")]
        public void BaseStationAircraftList_Applies_Database_Details_If_AircraftDetailFetcher_Fetched_Is_Raised()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            _AircraftList.Start();

            long originalDataVersion = 0;
            _AircraftDetailFetcher
                .Setup(r => r.RegisterAircraft(It.IsAny<IAircraft>()))
                .Callback((IAircraft ac) => originalDataVersion = ac.DataVersion)
                .Returns((AircraftDetail)null);
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationAircraft.ModeS = "4008F6";
            var databaseProperty = typeof(BaseStationAircraft).GetProperty(worksheet.String("DatabaseColumn"));
            var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftColumn"));
            var culture = new CultureInfo("en-GB");
            var databaseValue = TestUtilities.ChangeType(worksheet.EString("DatabaseValue"), databaseProperty.PropertyType, culture);
            var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, culture);
            databaseProperty.SetValue(_BaseStationAircraft, databaseValue, null);

            _AircraftDetailFetcher.Raise(r => r.Fetched += null, new EventArgs<AircraftDetail>(_AircraftDetail));

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.IsTrue(aircraft.DataVersion > originalDataVersion);
            Assert.AreEqual(aircraftValue, aircraftProperty.GetValue(aircraft, null));
        }

        [TestMethod]
        public void BaseStationAircraftList_Does_Not_Crash_If_AircraftDetailFetcher_Fetched_Is_Raised_For_Unknown_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationAircraft.ModeS = "4008F6";
            _AircraftDetailFetcher.Raise(r => r.Fetched += null, new EventArgs<AircraftDetail>(_AircraftDetail));
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Calls_RegisterAircraft_On_CallsignRouteFetcher_For_New_Aircraft()
        {
            _AircraftList.Start();

            IAircraft aircraft = null;
            _CallsignRouteFetcher.Setup(r => r.RegisterAircraft(It.IsAny<IAircraft>()))
                                 .Callback((IAircraft ac) => aircraft = ac)
                                 .Returns(_CallsignRouteDetail);

            _BaseStationMessage.Callsign = "ABC123";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _CallsignRouteFetcher.Verify(r => r.RegisterAircraft(It.IsAny<IAircraft>()), Times.Once());
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("ABC123", aircraft.Callsign);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Calls_RegisterAircraft_On_CallsignRouteFetcher_For_Existing_Aircraft()
        {
            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            IAircraft aircraft = null;
            _CallsignRouteFetcher.Setup(r => r.RegisterAircraft(It.IsAny<IAircraft>()))
                                 .Callback((IAircraft ac) => aircraft = ac)
                                 .Returns(_CallsignRouteDetail);

            _BaseStationMessage.Callsign = "ABC123";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _CallsignRouteFetcher.Verify(r => r.RegisterAircraft(It.IsAny<IAircraft>()), Times.Exactly(2));
            Assert.IsNotNull(aircraft);
            Assert.AreEqual("ABC123", aircraft.Callsign);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Copes_When_CallsignRouteFetcher_Returns_Null()
        {
            // This can happen if the callsign is null
            _AircraftList.Start();

            _CallsignRouteDetail = null;

            _BaseStationMessage.Callsign = "ABC123";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.IsNull(aircraft.Origin);
            Assert.IsNull(aircraft.Destination);
            Assert.AreEqual(0, aircraft.Stopovers.Count);
        }
        #endregion

        #region Route lookup
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Fetches_Route_For_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationMessage.Callsign = "CALL123";
            _CallsignRouteDetail.Route = _Route;

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual("EGLL Heathrow, UK", aircraft.Origin);
            Assert.AreEqual("KJFK, USA", aircraft.Destination);
            Assert.AreEqual(1, aircraft.Stopovers.Count);
            Assert.AreEqual("HEL", aircraft.Stopovers.First());
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Copes_With_Routes_With_No_Stopovers()
        {
            _AircraftList.Start();
            _Route.Stopovers.Clear();
            _BaseStationMessage.Callsign = "CALL123";
            _CallsignRouteDetail.Route = _Route;

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.IsNotNull(aircraft.Origin);
            Assert.AreEqual(0, aircraft.Stopovers.Count);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Copes_With_Routes_With_Many_Stopovers()
        {
            _AircraftList.Start();
            _Route.Stopovers.Add(_Boston);
            _BaseStationMessage.Callsign = "CALL123";
            _CallsignRouteDetail.Route = _Route;

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.IsNotNull(aircraft.Origin);
            Assert.AreEqual(2, aircraft.Stopovers.Count);
            Assert.AreEqual("HEL", aircraft.Stopovers.ElementAt(0));
            Assert.AreEqual("KBOS", aircraft.Stopovers.ElementAt(1));
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Updates_Route_When_Callsign_Changes()
        {
            _AircraftList.Start();

            var routeOut = new Route() { From = _Heathrow, To = _JohnFKennedy };
            var routeIn = new Route() { From = _JohnFKennedy, To = _Heathrow };

            _BaseStationMessage.Callsign = "VRS1";
            _CallsignRouteDetail.Route = routeOut;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Callsign = "VRS2";
            _CallsignRouteDetail.Route = routeIn;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual("EGLL Heathrow, UK", aircraft.Destination);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Updates_Route_When_Callsign_Changes_And_New_Callsign_Has_No_Route()
        {
            _AircraftList.Start();

            _BaseStationMessage.Callsign = "VRS1";
            _CallsignRouteDetail.Route = _Route;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Callsign = "VRS2";
            _CallsignRouteDetail.Route = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            aircraft = _AircraftList.FindAircraft(0x4008F6);

            Assert.AreEqual(null, aircraft.Origin);
            Assert.AreEqual(null, aircraft.Destination);
            Assert.AreEqual(0, aircraft.Stopovers.Count);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Honours_Configuration_When_Describing_Airport()
        {
            _AircraftList.Start();

            _BaseStationMessage.Callsign = "CALL123";
            _CallsignRouteDetail.Route = _Route;
            _Configuration.GoogleMapSettings.PreferIataAirportCodes = true;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual("LHR Heathrow, UK", aircraft.Origin);
            Assert.AreEqual("JFK, USA", aircraft.Destination);
            Assert.AreEqual(1, aircraft.Stopovers.Count);
            Assert.AreEqual("HEL", aircraft.Stopovers.First());
        }

        [TestMethod]
        public void BaseStationAircraftList_CallsignRouteFetcher_Fetched_Assigns_New_Route_To_Aircraft()
        {
            _AircraftList.Start();
            _BaseStationMessage.Callsign = "VS1";
            _CallsignRouteDetail.Callsign = "VS1";
            _CallsignRouteDetail.Icao24 = "4008F6";
            _CallsignRouteDetail.Route = _Route;
            _Port30003Listener.Raise(r => r.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _CallsignRouteDetail.Route = null;
            _CallsignRouteFetcher.Raise(r => r.Fetched += null, new EventArgs<CallsignRouteDetail>(_CallsignRouteDetail));

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(null, aircraft.Origin);
            Assert.AreEqual(null, aircraft.Destination);
            Assert.AreEqual(0, aircraft.Stopovers.Count);
        }

        [TestMethod]
        public void BaseStationAircraftList_CallsignRouteFetcher_Copes_When_Callsign_Assigns_Route_And_Operator_Code_Fetched_Later()
        {
            // We could get into a situation where we temporarily erase a valid route from an aircraft and put it back when:
            // 1. The callsign has a valid route for it. We assign the route to the aircraft.
            // 2. The database record fetcher raises Fetched and gives us a new operator code.
            // 3. The new operator code causes the route fetcher to fetch a new route, returning a null route for the aircraft.
            // 4. The list is fetched before callsign route fetcher raises Fetched with the assigned route for the callsign.

            _AircraftList.Start();
            _BaseStationMessage.Callsign = "BAW1";
            _CallsignRouteDetail.Callsign = "BAW1";
            _CallsignRouteDetail.Icao24 = "4008F6";
            _CallsignRouteDetail.Route = _Route;
            _AircraftDetail.Aircraft = null; // no aircraft details when the message is first assigned

            // Raise a message received. This assigns the route to the aircraft, the operator code will be null
            _Port30003Listener.Raise(r => r.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _CallsignRouteDetail.Route = null;
            _AircraftDetail.Aircraft = _BaseStationAircraft;
            _BaseStationAircraft.OperatorFlagCode = "BAW";

            // Raise an aircraft detail fetched. This will trigger a new fetch of route with a different operator code which will
            // have to be done on the background thread and satisfied via a Fetched event. This is where we could end up temporarily
            // wiping out the existing route on the aircraft.
            _AircraftDetailFetcher.Raise(r => r.Fetched += null, new EventArgs<AircraftDetail>(_AircraftDetail));

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.IsNotNull(aircraft.Origin);
            Assert.IsNotNull(aircraft.Destination);
            Assert.AreEqual(1, aircraft.Stopovers.Count);
        }
        #endregion

        #region Codeblock / Country / IsMilitary lookup
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Looks_Up_Codeblock_Details()
        {
            var codeblock1 = new CodeBlock() { Country = "UK", IsMilitary = false, };
            _StandingDataManager.Setup(m => m.FindCodeBlock("1")).Returns(codeblock1);

            var dbRecord1 = new BaseStationAircraft();
            _AircraftDetail.Aircraft = dbRecord1;
            dbRecord1.Country = "UNUSED";
            dbRecord1.ModeSCountry = "UNUSED";

            var codeBlock2 = new CodeBlock() { IsMilitary = true, };
            _StandingDataManager.Setup(m => m.FindCodeBlock("2")).Returns(codeBlock2);

            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "1";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Icao24 = "2";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft1 = _AircraftList.FindAircraft(1);
            Assert.AreEqual("UK", aircraft1.Icao24Country);
            Assert.AreEqual(false, aircraft1.IsMilitary);

            var aircraft2 = _AircraftList.FindAircraft(2);
            Assert.AreEqual(true, aircraft2.IsMilitary);
        }

        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Only_Looks_Up_Codeblock_Details_Once()
        {
            _AircraftList.Start();

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _StandingDataManager.Verify(m => m.FindCodeBlock("4008F6"), Times.Once());
        }
        #endregion

        #region ICAO8643 / Aircraft Type lookup
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Applies_Icao8643_Details_From_AircraftDetail()
        {
            _AircraftDetail.AircraftType = new AircraftType() {
                Engines = "C",
                EngineType = EngineType.Piston,
                EnginePlacement = EnginePlacement.AftMounted,
                Manufacturers = { "UNUSED 1" },
                Models = { "UNUSED 2" },
                Species = Species.Landplane,
                Type = "UNUSED",
                WakeTurbulenceCategory = WakeTurbulenceCategory.Medium,
            };

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual("C", aircraft.NumberOfEngines);
            Assert.AreEqual(EngineType.Piston, aircraft.EngineType);
            Assert.AreEqual(EnginePlacement.AftMounted, aircraft.EnginePlacement);
            Assert.AreEqual(Species.Landplane, aircraft.Species);
            Assert.AreEqual(WakeTurbulenceCategory.Medium, aircraft.WakeTurbulenceCategory);
            Assert.AreNotEqual("UNUSED", aircraft.Type);
            Assert.AreNotEqual("UNUSED 1", aircraft.Manufacturer);
            Assert.AreNotEqual("UNUSED 2", aircraft.Model);
        }

        [TestMethod]
        public void BaseStationAircraftList_Icao8643_Details_Refreshed_When_AircraftDetailFetcher_Fetched_Raised()
        {
            _AircraftList.Start();

            _AircraftDetail.Aircraft = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _AircraftDetail.Aircraft = _BaseStationAircraft;
            _AircraftDetail.AircraftType = new AircraftType() {
                Engines = "C",
                EngineType = EngineType.Piston,
                EnginePlacement = EnginePlacement.AftMounted,
                Species = Species.Landplane,
                WakeTurbulenceCategory = WakeTurbulenceCategory.Medium,
            };
            _AircraftDetailFetcher.Raise(r => r.Fetched += null, new EventArgs<AircraftDetail>(_AircraftDetail));

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual("C", aircraft.NumberOfEngines);
            Assert.AreEqual(EngineType.Piston, aircraft.EngineType);
            Assert.AreEqual(EnginePlacement.AftMounted, aircraft.EnginePlacement);
            Assert.AreEqual(Species.Landplane, aircraft.Species);
            Assert.AreEqual(WakeTurbulenceCategory.Medium, aircraft.WakeTurbulenceCategory);
        }
        #endregion

        #region Aircraft picture lookup
        [TestMethod]
        public void BaseStationAircraftList_MessageReceived_Uses_Aircraft_Pictures_From_AircraftDetailFetcher()
        {
            _BaseStationAircraft.Registration = "G-VROS";
            _PictureDetails.FileName = "Fullpath";
            _PictureDetails.Width = 640;
            _PictureDetails.Height = 480;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual("Fullpath", aircraft.PictureFileName);
            Assert.AreEqual(640, aircraft.PictureWidth);
            Assert.AreEqual(480, aircraft.PictureHeight);
        }

        [TestMethod]
        public void BaseStationAircraftList_AircraftDetailFetcher_Fetcher_Refreshes_Picture_Details()
        {
            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PictureDetails.FileName = "fileName";
            _PictureDetails.FileName = "Fullpath";
            _PictureDetails.Width = 640;
            _PictureDetails.Height = 480;
            _AircraftDetailFetcher.Raise(r => r.Fetched += null, new EventArgs<AircraftDetail>(_AircraftDetail));

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual("Fullpath", aircraft.PictureFileName);
            Assert.AreEqual(640, aircraft.PictureWidth);
            Assert.AreEqual(480, aircraft.PictureHeight);
        }
        #endregion

        #region Polar Plotter
        [TestMethod]
        public void BaseStationAircraftList_Passes_Coordinates_To_PolarPlotter()
        {
            _AircraftList.PolarPlotter = _PolarPlotter.Object;
            _BaseStationMessage.Altitude = 1000;
            _BaseStationMessage.Latitude = 1.23;
            _BaseStationMessage.Longitude = 4.56;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PolarPlotter.Verify(r => r.AddCheckedCoordinate(0x4008F6, 1000, 1.23, 4.56), Times.Once());
        }

        [TestMethod]
        public void BaseStationAircraftList_Does_Not_Pass_Coordinates_To_Null_PolarPlotter()
        {
            _AircraftList.PolarPlotter = _PolarPlotter.Object;
            _BaseStationMessage.Altitude = 1000;
            _BaseStationMessage.Latitude = 1.23;
            _BaseStationMessage.Longitude = 4.56;

            _AircraftList.Start();
            _AircraftList.PolarPlotter = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PolarPlotter.Verify(r => r.AddCheckedCoordinate(0x4008F6, 1000, 1.23, 4.56), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_Does_Not_Pass_Coordinates_When_Message_Does_Not_Contain_Position()
        {
            _AircraftList.PolarPlotter = _PolarPlotter.Object;
            _BaseStationMessage.Altitude = 1000;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PolarPlotter.Verify(r => r.AddCheckedCoordinate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_Does_Not_Pass_Coordinates_When_Message_Does_Not_Contain_Altitude()
        {
            // All extended squitter messages that carry position also carry altitude
            _AircraftList.PolarPlotter = _PolarPlotter.Object;
            _BaseStationMessage.Latitude = 1.23;
            _BaseStationMessage.Longitude = 4.56;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PolarPlotter.Verify(r => r.AddCheckedCoordinate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_Passes_OnGround_Coordinates_To_PolarPlotter_With_Altitude_Of_Zero()
        {
            _AircraftList.PolarPlotter = _PolarPlotter.Object;
            _BaseStationMessage.OnGround = true;
            _BaseStationMessage.Latitude = 1.23;
            _BaseStationMessage.Longitude = 4.56;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PolarPlotter.Verify(r => r.AddCheckedCoordinate(0x4008F6, 0, 1.23, 4.56), Times.Once());
        }

        [TestMethod]
        public void BaseStationAircraftList_Does_Not_Pass_Coordinates_When_SanityChecker_Reports_Uncertain_Altitude()
        {
            _AircraftList.PolarPlotter = _PolarPlotter.Object;
            _BaseStationMessage.Altitude = 1;
            _BaseStationMessage.Latitude = 1;
            _BaseStationMessage.Longitude = 1;
            _SaneAltitude = Certainty.Uncertain;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PolarPlotter.Verify(r => r.AddCheckedCoordinate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_Does_Not_Pass_Coordinates_When_SanityChecker_Reports_CertainlyWrong_Altitude()
        {
            _AircraftList.PolarPlotter = _PolarPlotter.Object;
            _BaseStationMessage.Altitude = 1;
            _BaseStationMessage.Latitude = 1;
            _BaseStationMessage.Longitude = 1;
            _SaneAltitude = Certainty.CertainlyWrong;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PolarPlotter.Verify(r => r.AddCheckedCoordinate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_Does_Not_Pass_Coordinates_When_SanityChecker_Reports_Uncertain_Position()
        {
            _AircraftList.PolarPlotter = _PolarPlotter.Object;
            _BaseStationMessage.Altitude = 1;
            _BaseStationMessage.Latitude = 1;
            _BaseStationMessage.Longitude = 1;
            _SanePosition = Certainty.Uncertain;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PolarPlotter.Verify(r => r.AddCheckedCoordinate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_Does_Not_Pass_Coordinates_When_SanityChecker_Reports_CertainlyWrong_Position()
        {
            _AircraftList.PolarPlotter = _PolarPlotter.Object;
            _BaseStationMessage.Altitude = 1;
            _BaseStationMessage.Latitude = 1;
            _BaseStationMessage.Longitude = 1;
            _SanePosition = Certainty.CertainlyWrong;

            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _PolarPlotter.Verify(r => r.AddCheckedCoordinate(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never());
        }
        #endregion

        #region TransponderType
        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Initialises_To_Mode_S()
        {
            _AircraftList.Start();
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.ModeS, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Set_To_ADSB_When_Speed_Is_Present()
        {
            _AircraftList.Start();
            _BaseStationMessage.GroundSpeed = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Set_To_ADSB_When_Vertical_Speed_Is_Present()
        {
            _AircraftList.Start();
            _BaseStationMessage.VerticalRate = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Set_To_ADSB_When_Latitude_Is_Present()
        {
            _AircraftList.Start();
            _BaseStationMessage.Latitude = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Set_To_ADSB_When_Longitude_Is_Present()
        {
            _AircraftList.Start();
            _BaseStationMessage.Longitude = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Set_To_ADSB_When_Track_Is_Present()
        {
            _AircraftList.Start();
            _BaseStationMessage.Track = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Set_To_ADSB0_When_Supplementary_ADSB_0_Is_Present()
        {
            _AircraftList.Start();
            _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage();
            _BaseStationMessage.Supplementary.TransponderType = TransponderType.Adsb0;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb0, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Set_To_ADSB1_When_Supplementary_ADSB_1_Is_Present()
        {
            _AircraftList.Start();
            _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage();
            _BaseStationMessage.Supplementary.TransponderType = TransponderType.Adsb1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb1, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Set_To_ADSB2_When_Supplementary_ADSB_2_Is_Present()
        {
            _AircraftList.Start();
            _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage();
            _BaseStationMessage.Supplementary.TransponderType = TransponderType.Adsb2;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb2, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Will_Not_Regress_From_ADSB2_To_ADSB1()
        {
            _AircraftList.Start();

            _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage();
            _BaseStationMessage.Supplementary.TransponderType = TransponderType.Adsb2;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Supplementary.TransponderType = TransponderType.Adsb1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb2, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Will_Not_Regress_From_ADSB1_To_ADSB0()
        {
            _AircraftList.Start();

            _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage();
            _BaseStationMessage.Supplementary.TransponderType = TransponderType.Adsb1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Supplementary.TransponderType = TransponderType.Adsb0;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb1, aircraft.TransponderType);
        }

        [TestMethod]
        public void BaseStationAircraftList_TransponderType_Will_Not_Regress_From_ADSB_To_ModeS()
        {
            _AircraftList.Start();

            _BaseStationMessage.GroundSpeed = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.Supplementary = null;
            _BaseStationMessage.GroundSpeed = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(TransponderType.Adsb, aircraft.TransponderType);
        }
        #endregion

        #region OutOfBand messages
        [TestMethod]
        public void BaseStationAircraftList_OutOfBand_Adds_MLAT_Position_To_No_Position_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationMessage.ReceiverId = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.ReceiverId = 2;
            _BaseStationMessage.Latitude = 10;
            _BaseStationMessage.Longitude = 20;
            _BaseStationMessage.Track = 30;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _OobBaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(10, aircraft.Latitude);
            Assert.AreEqual(20, aircraft.Longitude);
            Assert.AreEqual(30, aircraft.Track);
            Assert.IsTrue(aircraft.PositionIsMlat.GetValueOrDefault());
        }

        [TestMethod]
        public void BaseStationAircraftList_OutOfBand_Adds_MLAT_Position_To_MLAT_Position_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationMessage.ReceiverId = 1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.ReceiverId = 2;
            _BaseStationMessage.Latitude = 5;
            _BaseStationMessage.Longitude = 6;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _OobBaseStationMessageEventArgs);

            _BaseStationMessage.ReceiverId = 3;
            _BaseStationMessage.Latitude = 10;
            _BaseStationMessage.Longitude = 20;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _OobBaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(10, aircraft.Latitude);
            Assert.AreEqual(20, aircraft.Longitude);
            Assert.IsTrue(aircraft.PositionIsMlat.GetValueOrDefault());
        }

        [TestMethod]
        public void BaseStationAircraftList_OutOfBand_Does_Not_Change_Position_For_Normal_Position_Aircraft()
        {
            _AircraftList.Start();

            _BaseStationMessage.ReceiverId = 1;
            _BaseStationMessage.Latitude = 10;
            _BaseStationMessage.Longitude = 20;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.ReceiverId = 2;
            _BaseStationMessage.Latitude = 41;
            _BaseStationMessage.Longitude = 42;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _OobBaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(10, aircraft.Latitude);
            Assert.AreEqual(20, aircraft.Longitude);
            Assert.IsFalse(aircraft.PositionIsMlat.GetValueOrDefault());
        }

        [TestMethod]
        public void BaseStationAircraftList_OutOfBand_Changes_Position_For_Normal_Position_Aircraft_Set_By_Another_Receiver()
        {
            _AircraftList.Start();

            // This covers the situation where an MLAT feed with messages that are not marked as MLAT temporarily becomes the
            // nominated source in a merged feed. Its positions will be marked as non-MLAT in the aircraft list because they
            // just look like normal positions. In this case the non-MLAT positions will not have the same receiver ID as the
            // nominated receiver, so we overwrite them.

            // Nominated receiver has positions
            _BaseStationMessage.ReceiverId = 1;
            _BaseStationMessage.Latitude = -1;
            _BaseStationMessage.Longitude = -1;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // New nominated receiver does not have positions
            _BaseStationMessage.ReceiverId = 2;
            _BaseStationMessage.Latitude = null;
            _BaseStationMessage.Longitude = null;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            // OOB receiver can overwrite non-MLAT positions because position receiver ID will be 1 and the
            // aircraft will be branded as belonging (for now) to 2.
            _BaseStationMessage.ReceiverId = 3;
            _BaseStationMessage.Latitude = 10;
            _BaseStationMessage.Longitude = 20;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _OobBaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(10, aircraft.Latitude);
            Assert.AreEqual(20, aircraft.Longitude);
            Assert.IsTrue(aircraft.PositionIsMlat.GetValueOrDefault());
        }

        [TestMethod]
        public void BaseStationAircraftList_OutOfBand_Only_Sets_Position_And_Track()
        {
            _AircraftList.Start();

            _BaseStationMessage.ReceiverId = 1;
            _BaseStationMessage.Altitude = 1;
            _BaseStationMessage.Callsign = "1";
            _BaseStationMessage.Emergency = false;
            _BaseStationMessage.GroundSpeed = 1;
            _BaseStationMessage.OnGround = false;
            _BaseStationMessage.SignalLevel = 1;
            _BaseStationMessage.Squawk = 1;
            _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage() {
                AltitudeIsGeometric = false,
                CallsignIsSuspect = false,
                SpeedType = SpeedType.GroundSpeed,
                TargetAltitude = 1,
                TargetHeading = 1,
                TrackIsHeading = false,
                TransponderType = TransponderType.Adsb,
                VerticalRateIsGeometric = false,
            };
            _BaseStationMessage.Track = 1;
            _BaseStationMessage.VerticalRate = 1;

            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _BaseStationMessage.ReceiverId = 2;
            _BaseStationMessage.Latitude = 10;
            _BaseStationMessage.Longitude = 20;
            _BaseStationMessage.Altitude = 2;
            _BaseStationMessage.Callsign = "2";
            _BaseStationMessage.Emergency = true;
            _BaseStationMessage.GroundSpeed = 2;
            _BaseStationMessage.OnGround = true;
            _BaseStationMessage.SignalLevel = 2;
            _BaseStationMessage.Squawk = 2;
            _BaseStationMessage.Supplementary = new BaseStationSupplementaryMessage() {
                AltitudeIsGeometric = true,
                CallsignIsSuspect = true,
                SpeedType = SpeedType.GroundSpeedReversing,
                TargetAltitude = 2,
                TargetHeading = 2,
                TrackIsHeading = true,
                TransponderType = TransponderType.Adsb0,
                VerticalRateIsGeometric = true,
            };
            _BaseStationMessage.Track = 2;
            _BaseStationMessage.VerticalRate = 2;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _OobBaseStationMessageEventArgs);

            var aircraft = _AircraftList.FindAircraft(0x4008F6);
            Assert.AreEqual(1, aircraft.Altitude);
            Assert.AreEqual("1", aircraft.Callsign);
            Assert.AreEqual(false, aircraft.Emergency);
            Assert.AreEqual(1, aircraft.GroundSpeed);
            Assert.AreEqual(false, aircraft.OnGround);
            Assert.AreEqual(10, aircraft.Latitude);
            Assert.AreEqual(20, aircraft.Longitude);
            Assert.AreEqual(1, aircraft.SignalLevel);
            Assert.AreEqual(1, aircraft.Squawk);
            Assert.AreEqual(AltitudeType.Barometric, aircraft.AltitudeType);
            Assert.AreEqual(false, aircraft.CallsignIsSuspect);
            Assert.AreEqual(SpeedType.GroundSpeed, aircraft.SpeedType);
            Assert.AreEqual(1, aircraft.TargetAltitude);
            Assert.AreEqual(1, aircraft.TargetTrack);
            Assert.AreEqual(2, aircraft.Track);
            Assert.AreEqual(TransponderType.Adsb, aircraft.TransponderType);
            Assert.AreEqual(AltitudeType.Barometric, aircraft.VerticalRateType);
        }
        #endregion
        #endregion

        #region Listener.SourceChanged event
        [TestMethod]
        public void BaseStationAircraftList_SourceChanged_Clears_Aircraft_List()
        {
            _AircraftList.Start();

            _BaseStationMessage.Icao24 = "7";
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _Port30003Listener.Raise(b => b.SourceChanged += null, EventArgs.Empty);

            long out1, out2;
            Assert.AreEqual(0, _AircraftList.TakeSnapshot(out out1, out out2).Count);
        }

        [TestMethod]
        public void BaseStationAircraftList_SourceChanged_Raises_CountChanged()
        {
            _AircraftList.CountChanged += _CountChangedEvent.Handler;
            _AircraftList.Start();

            _Port30003Listener.Raise(b => b.SourceChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _CountChangedEvent.CallCount);
            Assert.AreSame(_AircraftList, _CountChangedEvent.Sender);
            Assert.AreNotEqual(null, _CountChangedEvent.Args);
        }
        #endregion

        #region Listener.PositionReset event
        [TestMethod]
        public void BaseStationAircraftList_PositionReset_Resets_Aircraft_Coordinate_List()
        {
            _AircraftList.Start();

            var aircraftMock = TestUtilities.CreateMockImplementation<IAircraft>();

            _BaseStationMessage.Icao24 = "ABC123";
            _BaseStationMessage.Latitude = 1.0;
            _BaseStationMessage.Longitude = 2.0;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _Port30003Listener.Raise(r => r.PositionReset += null, new EventArgs<string>("ABC123"));

            aircraftMock.Verify(r => r.ResetCoordinates(), Times.Once());
        }

        [TestMethod]
        public void BaseStationAircraftList_PositionReset_Ignores_Resets_On_Aircraft_Not_Being_Tracked()
        {
            _AircraftList.Start();

            var aircraftMock = TestUtilities.CreateMockImplementation<IAircraft>();

            _BaseStationMessage.Icao24 = "ABC123";
            _BaseStationMessage.Latitude = 1.0;
            _BaseStationMessage.Longitude = 2.0;
            _Port30003Listener.Raise(m => m.Port30003MessageReceived += null, _BaseStationMessageEventArgs);

            _Port30003Listener.Raise(r => r.PositionReset += null, new EventArgs<string>("123456"));

            aircraftMock.Verify(r => r.ResetCoordinates(), Times.Never());
        }

        [TestMethod]
        public void BaseStationAircraftList_PositionReset_Resets_Sanity_Checker()
        {
            _AircraftList.Start();

            _Port30003Listener.Raise(r => r.PositionReset += null, new EventArgs<string>("000001"));

            _SanityChecker.Verify(r => r.ResetAircraft(1), Times.Once());
        }
        #endregion

        #region StandingDataManager.LoadCompleted event
        [TestMethod]
        public void BaseStationAircraftList_StandingDataManager_LoadCompleted_Reloads_ModeS_Country_And_IsMilitary()
        {
            _AircraftList.Start();

            _Port30003Listener.Raise(r => r.Port30003MessageReceived += null, _BaseStationMessageEventArgs);
            _StandingDataManager.Verify(r => r.FindCodeBlock("4008F6"), Times.Once());

            IAircraft aircraft;
            long unused1, unused2;

            aircraft = _AircraftList.TakeSnapshot(out unused1, out unused2)[0];
            Assert.AreEqual(null, aircraft.Icao24Country);
            Assert.AreEqual(false, aircraft.IsMilitary);

            _StandingDataManager.Setup(r => r.FindCodeBlock("4008F6")).Returns(new CodeBlock() { Country = "UK", IsMilitary = true });
            _StandingDataManager.Raise(r => r.LoadCompleted += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindCodeBlock("4008F6"), Times.Exactly(2));
            aircraft = _AircraftList.TakeSnapshot(out unused1, out unused2)[0];
            Assert.AreEqual("UK", aircraft.Icao24Country);
            Assert.AreEqual(true, aircraft.IsMilitary);
        }
        #endregion
    }
}
