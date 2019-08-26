// Copyright © 2014 onwards, Andrew Whewell
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
using VirtualRadar.Interface.StandingData;

namespace Test.VirtualRadar.Database
{
    [TestClass]
    public class CallsignRouteFetcherTests
    {
        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private const int AutoDeregisterInterval = 90000;

        private IClassFactory _OriginalFactory;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private ICallsignRouteFetcher _Fetcher;
        private EventRecorder<EventArgs<CallsignRouteDetail>> _FetchedHandler;
        private Mock<IAircraft> _Aircraft;
        private string _AircraftCallsign;
        private string _AircraftOperatorCode;
        private Mock<IHeartbeatService> _Heartbeat;
        private Mock<IStandingDataManager> _StandingDataManager;
        private Route _Route;
        private string _RouteCallsign;
        private ClockMock _Clock;
        private Mock<ICallsignParser> _CallsignParser;
        private Dictionary<string, List<string>> _RouteCallsigns;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsTest).Returns(true);

            _Clock = new ClockMock();
            Factory.RegisterInstance<IClock>(_Clock.Object);

            _Fetcher = Factory.ResolveNewInstance<ICallsignRouteFetcher>();
            _FetchedHandler = new EventRecorder<EventArgs<CallsignRouteDetail>>();
            _Fetcher.Fetched += _FetchedHandler.Handler;

            _Aircraft = TestUtilities.CreateMockInstance<IAircraft>();
            _AircraftCallsign = _RouteCallsign = "BAW1";
            _AircraftOperatorCode = null;
            _Aircraft.Setup(r => r.Icao24).Returns("ABC123");
            _Aircraft.Setup(r => r.Callsign).Returns(() => _AircraftCallsign);
            _Aircraft.Setup(r => r.OperatorIcao).Returns(() => _AircraftOperatorCode);

            _CallsignParser = TestUtilities.CreateMockImplementation<ICallsignParser>();
            _CallsignParser.Setup(r => r.GetAllRouteCallsigns(It.IsAny<string>(), It.IsAny<string>())).Returns((string callsign, string operatorCode) => {
                List<string> result;
                if(callsign == null || !_RouteCallsigns.TryGetValue(callsign, out result)) result = new List<string>();
                return result;
            });
            _RouteCallsigns = new Dictionary<string,List<string>>() {
                { "BAW1", new List<string>() { "BAW1" } }
            };

            // The fetcher uses a private heartbeat service to avoid slowing the GUI down but this
            // is hard to test with the singleton attribute, so under test environment it'll use the
            // singleton instead.
            _Heartbeat = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            Factory.RegisterInstance<IHeartbeatService>(_Heartbeat.Object);

            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _Route = null;
            _StandingDataManager.Setup(r => r.FindRoute(It.IsAny<string>())).Returns((string callsign) => {
                return callsign == _RouteCallsign ? _Route : null;
            });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }
        #endregion

        #region RegisterAircraft
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CallsignRouteFetcher_RegisterAircraft_Throws_If_Aircraft_Is_Null()
        {
            _Fetcher.RegisterAircraft(null);
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Ignores_Aircraft_With_Null_Callsign()
        {
            _AircraftCallsign = null;
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindRoute(null), Times.Never());
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Ignores_Aircraft_With_Empty_Callsign()
        {
            _AircraftCallsign = "";
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindRoute(""), Times.Never());
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Gets_List_Of_Callsigns_To_Search_On_Next_Fast_Tick()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _CallsignParser.Verify(r => r.GetAllRouteCallsigns("BAW1", null), Times.Once());
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Uses_Operator_Code_When_Searching_For_Callsigns()
        {
            _AircraftOperatorCode = "COD";
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _CallsignParser.Verify(r => r.GetAllRouteCallsigns("BAW1", "COD"), Times.Once());
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Fetches_Route_On_Next_Fast_Tick()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindRoute("BAW1"), Times.Once());
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Stops_Looking_When_Route_Found()
        {
            _RouteCallsigns["BAW1"] = new List<string>() { "FIRST", "BAW1", "NEVER" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Route = new Route();

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindRoute("FIRST"), Times.Once());
            _StandingDataManager.Verify(r => r.FindRoute("BAW1"), Times.Once());
            _StandingDataManager.Verify(r => r.FindRoute("NEVER"), Times.Never());
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Does_Not_Immediately_Fetch_Route()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _StandingDataManager.Verify(r => r.FindRoute(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Always_Returns_Null_On_First_Add()
        {
            _Route = new Route();
            Assert.IsNull(_Fetcher.RegisterAircraft(_Aircraft.Object));
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Only_Fetches_Route_On_Next_Fast_Tick()
        {
            _Route = new Route();
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindRoute("BAW1"), Times.Once());
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Returns_The_Correct_Details_If_The_Aircraft_Has_Already_Been_Registered()
        {
            var sameAircraftDifferentObject = TestUtilities.CreateMockInstance<IAircraft>();
            sameAircraftDifferentObject.Setup(r => r.Icao24).Returns("ABC123");
            sameAircraftDifferentObject.Setup(r => r.Callsign).Returns("BAW1");

            _Route = new Route();
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            var details = _Fetcher.RegisterAircraft(sameAircraftDifferentObject.Object);
            Assert.AreEqual("ABC123", details.Icao24);
            Assert.AreEqual("BAW1", details.Callsign);
            Assert.AreSame(_Route, details.Route);
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Does_Not_Raise_Multiple_Events_When_Same_Aircraft_Registered_Twice_Before_Initial_Lookup()
        {
            _Route = new Route();
            var sameAircraftDifferentObject = TestUtilities.CreateMockInstance<IAircraft>();
            sameAircraftDifferentObject.Setup(r => r.Icao24).Returns("ABC123");
            sameAircraftDifferentObject.Setup(r => r.Callsign).Returns("BAW1");

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Fetcher.RegisterAircraft(sameAircraftDifferentObject.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void CallsignRouteFetcher_RegisterAircraft_Returns_Correct_Route_When_Same_Aircraft_Registered_With_Different_Callsigns()
        {
            var sameAircraftDifferentObject = TestUtilities.CreateMockInstance<IAircraft>();
            sameAircraftDifferentObject.Setup(r => r.Icao24).Returns("ABC123");
            sameAircraftDifferentObject.Setup(r => r.Callsign).Returns("GIBBERISH");

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Fetcher.RegisterAircraft(sameAircraftDifferentObject.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
            Assert.IsTrue(_FetchedHandler.AllArgs.Count(r => r.Value.Icao24 == "ABC123") == 2);
            Assert.IsTrue(_FetchedHandler.AllArgs.Count(r => r.Value.Callsign == "BAW1") == 1);
            Assert.IsTrue(_FetchedHandler.AllArgs.Count(r => r.Value.Callsign == "GIBBERISH") == 1);
        }
        #endregion

        #region Automatic Deregistration
        [TestMethod]
        public void CallsignRouteFetcher_AutoDeregister_Stops_Route_Lookups_Once_Aircraft_Has_Not_Been_Registered_Within_Timeout()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(AutoDeregisterInterval);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindRoute("BAW1"), Times.Once());
        }

        [TestMethod]
        public void CallsignRouteFetcher_AutoDeregister_Interval_Refreshed_By_Registration()
        {
            var sameAircraftDifferentObject = TestUtilities.CreateMockInstance<IAircraft>();
            sameAircraftDifferentObject.Setup(r => r.Icao24).Returns("ABC123");
            sameAircraftDifferentObject.Setup(r => r.Callsign).Returns("BAW1");

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(AutoDeregisterInterval - 1);
            _Fetcher.RegisterAircraft(sameAircraftDifferentObject.Object);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(2);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindRoute("BAW1"), Times.Once());
        }
        #endregion

        #region Fetched
        [TestMethod]
        public void CallsignRouteFetcher_Fetched_Raised_If_Aircraft_Has_Route()
        {
            _Route = new Route();
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
            Assert.AreEqual("BAW1", _FetchedHandler.Args.Value.Callsign);
            Assert.AreEqual("ABC123", _FetchedHandler.Args.Value.Icao24);
            Assert.AreSame(_Route, _FetchedHandler.Args.Value.Route);
            Assert.AreEqual("BAW1", _FetchedHandler.Args.Value.UsedCallsign);
            Assert.AreSame(_Fetcher, _FetchedHandler.Sender);
        }

        [TestMethod]
        public void CallsignRouteFetcher_Fetched_Not_Raised_If_Details_Change_After_Initial_Fetch()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Route = new Route();
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(AutoDeregisterInterval - 1);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }
        #endregion

        #region LoadCompleted
        [TestMethod]
        public void CallsignRouteFetcher_LoadCompleted_Refetches_All_Routes()
        {
            _Route = new Route();
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Route = new Route();
            _StandingDataManager.Raise(r => r.LoadCompleted += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
            Assert.AreSame(_Route, _FetchedHandler.AllArgs[1].Value.Route);
        }
        #endregion
    }
}
