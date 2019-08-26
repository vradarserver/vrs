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
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.StandingData;
using System.Threading;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class AircraftDetailFetcherTests
    {
        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private const int IntervalMilliseconds = 60000;
        private const int AutoDeregisterInterval = 90000;

        private IClassFactory _OriginalFactory;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private IAircraftDetailFetcher _Fetcher;
        private EventRecorder<EventArgs<AircraftDetail>> _FetchedHandler;
        private Mock<IAircraft> _Aircraft;
        private Mock<IHeartbeatService> _Heartbeat;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigDatabase;
        private Mock<IBaseStationDatabase> _Database;
        private BaseStationAircraftAndFlightsCount _DatabaseAircraftAndFlights;
        private BaseStationAircraft _DatabaseAircraft;
        private List<string> _GetManyAircraftAndFlightsByCodes;
        private List<string> _GetManyAircraftByCodes;
        private Mock<IAutoConfigPictureFolderCache> _AutoConfigPictureFolderCache;
        private Mock<IDirectoryCache> _PictureFolderCache;
        private Mock<IAircraftPictureManager> _AircraftPictureManager;
        private IDirectoryCache _PictureManagerCache;
        private string _PictureManagerIcao24;
        private string _PictureManagerReg;
        private bool _PictureManagerThrowException;
        private PictureDetail _PictureDetail;
        private Mock<IStandingDataManager> _StandingDataManager;
        private string _FindAircraftType;
        private AircraftType _AircraftType;
        private ClockMock _Clock;
        private Mock<ILog> _Log;
        private Mock<IAircraftOnlineLookupManager> _AircraftOnlineLookupManager;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsTest).Returns(true);

            _Clock = new ClockMock();
            Factory.RegisterInstance<IClock>(_Clock.Object);

            _Fetcher = Factory.ResolveNewInstance<IAircraftDetailFetcher>();
            _FetchedHandler = new EventRecorder<EventArgs<AircraftDetail>>();
            _Fetcher.Fetched += _FetchedHandler.Handler;

            _Aircraft = TestUtilities.CreateMockInstance<IAircraft>();
            _Aircraft.Setup(r => r.Icao24).Returns("ABC123");

            // The production code actually has a private heartbeat object but under test
            // it will use the singleton one to make life easier.
            _Heartbeat = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            Factory.RegisterInstance<IHeartbeatService>(_Heartbeat.Object);

            _AutoConfigDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _Database = TestUtilities.CreateMockInstance<IBaseStationDatabase>();
            _AutoConfigDatabase.Setup(r => r.Database).Returns(_Database.Object);
            _DatabaseAircraftAndFlights = null;
            _GetManyAircraftAndFlightsByCodes = new List<string>();
            _GetManyAircraftByCodes = new List<string>();
            _Database.Setup(r => r.GetManyAircraftByCode(It.IsAny<IEnumerable<string>>())).Returns((IEnumerable<string> icaos) => {
                var result = new Dictionary<string, BaseStationAircraft>();
                if(icaos != null && icaos.Count() == 1 && icaos.First() == "ABC123" && _DatabaseAircraft != null) result.Add("ABC123", _DatabaseAircraft);
                if(icaos != null) _GetManyAircraftByCodes.AddRange(icaos);
                return result;
            });
            _Database.Setup(r => r.GetManyAircraftAndFlightsCountByCode(It.IsAny<IEnumerable<string>>())).Returns((IEnumerable<string> icaos) => {
                var result = new Dictionary<string, BaseStationAircraftAndFlightsCount>();
                if(icaos != null && icaos.Count() == 1 && icaos.First() == "ABC123" && _DatabaseAircraftAndFlights != null) result.Add("ABC123", _DatabaseAircraftAndFlights);
                if(icaos != null) _GetManyAircraftAndFlightsByCodes.AddRange(icaos);
                return result;
            });

            _AutoConfigPictureFolderCache = TestUtilities.CreateMockSingleton<IAutoConfigPictureFolderCache>();
            _PictureFolderCache = TestUtilities.CreateMockInstance<IDirectoryCache>();
            _AutoConfigPictureFolderCache.Setup(r => r.DirectoryCache).Returns(() => _PictureFolderCache.Object);

            _AircraftPictureManager = TestUtilities.CreateMockSingleton<IAircraftPictureManager>();
            _PictureManagerCache = _PictureFolderCache.Object;
            _PictureManagerIcao24 = "INVALID";
            _PictureManagerReg = null;
            _PictureManagerThrowException = false;
            _PictureDetail = new PictureDetail();
            _AircraftPictureManager.Setup(r => r.FindPicture(It.IsAny<IDirectoryCache>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PictureDetail>())).Returns((IDirectoryCache cache, string icao24, string reg, PictureDetail existingPictureDetail) => {
                if(_PictureManagerThrowException) throw new InvalidOperationException();
                return cache == _PictureManagerCache && icao24 == _PictureManagerIcao24 && reg == _PictureManagerReg ? _PictureDetail : null;
            });

            _StandingDataManager = TestUtilities.CreateMockSingleton<IStandingDataManager>();
            _AircraftType = new AircraftType();
            _StandingDataManager.Setup(r => r.FindAircraftType(It.IsAny<string>())).Returns((string type) => {
                return type == _FindAircraftType ? _AircraftType : null;
            });

            _Log = TestUtilities.CreateMockSingleton<ILog>();
            _Log.Setup(r => r.WriteLine(It.IsAny<string>())).Callback((string x) => { throw new InvalidOperationException(x); });
            _Log.Setup(r => r.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback((string x, object[] args) => { throw new InvalidOperationException(String.Format(x, args)); });

            _AircraftOnlineLookupManager = TestUtilities.CreateMockSingleton<IAircraftOnlineLookupManager>();
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
        public void AircraftDetailFetcher_RegisterAircraft_Throws_If_Aircraft_Is_Null()
        {
            _Fetcher.RegisterAircraft(null);
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Ignores_Aircraft_With_Null_Icao24_Code()
        {
            _Aircraft.Setup(r => r.Icao24).Returns((string)null);
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Database.Verify(r => r.GetManyAircraftAndFlightsCountByCode(It.IsAny<IEnumerable<string>>()), Times.Never());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Ignores_Aircraft_With_Empty_Icao24_Code()
        {
            _Aircraft.Setup(r => r.Icao24).Returns("");
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Database.Verify(r => r.GetManyAircraftAndFlightsCountByCode(It.IsAny<IEnumerable<string>>()), Times.Never());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Does_Not_Immediately_Fetch_Aircraft_Record()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Database.Verify(r => r.GetManyAircraftAndFlightsCountByCode(It.IsAny<IEnumerable<string>>()), Times.Never());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Fetches_Aircraft_Record_On_Next_Fast_Tick()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual("ABC123", _GetManyAircraftAndFlightsByCodes.Single());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Does_Not_Immediately_Fetch_Aircraft_Picture()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _AircraftPictureManager.Verify(r => r.FindPicture(It.IsAny<IDirectoryCache>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PictureDetail>()), Times.Never());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Fetches_Aircraft_Picture_On_Next_Fast_Tick()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _AircraftPictureManager.Verify(r => r.FindPicture(It.IsAny<IDirectoryCache>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PictureDetail>()), Times.Once());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Always_Returns_Null_On_First_Add()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            Assert.IsNull(_Fetcher.RegisterAircraft(_Aircraft.Object));
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Fetches_Aircraft_Record_Only_On_First_Fast_Tick()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual("ABC123", _GetManyAircraftAndFlightsByCodes.Single());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Fetches_Aircraft_Picture_Only_On_First_Fast_Tick()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _AircraftPictureManager.Verify(r => r.FindPicture(It.IsAny<IDirectoryCache>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PictureDetail>()), Times.Once());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Does_Not_Make_Individual_Requests_For_Flight_Counts()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Database.Verify(r => r.GetCountOfFlightsForAircraft(It.IsAny<BaseStationAircraft>(), It.IsAny<SearchBaseStationCriteria>()), Times.Never());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Searches_For_AircraftType_Using_Database_Detail()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", ICAOTypeCode = "B747" };

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindAircraftType("B747"), Times.Once());
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Exposes_AircraftType_In_Results()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", ICAOTypeCode = "B747" };
            _FindAircraftType = "B747";

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreSame(_AircraftType, _FetchedHandler.Args.Value.AircraftType);
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Does_Not_Refetch_AircraftType_If_ModelIcao_Has_Not_Changed()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", ICAOTypeCode = "B747" };
            _FindAircraftType = "B747";

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC", ICAOTypeCode = "B747" };
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindAircraftType("B747"), Times.Once());
            Assert.AreEqual(2, _FetchedHandler.CallCount);
            Assert.AreSame(_AircraftType, _FetchedHandler.AllArgs[1].Value.AircraftType);
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Does_Refetch_AircraftType_If_ModelIcao_Has_Changed()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", ICAOTypeCode = "B747" };
            _FindAircraftType = "A380";

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _DatabaseAircraft = new BaseStationAircraft() { ModeS = "ABC123", ICAOTypeCode = "A380" };
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindAircraftType("B747"), Times.Once());
            _StandingDataManager.Verify(r => r.FindAircraftType("A380"), Times.Once());
            Assert.AreEqual(2, _FetchedHandler.CallCount);
            Assert.AreSame(_AircraftType, _FetchedHandler.AllArgs[1].Value.AircraftType);
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Returns_The_Correct_Details_If_The_Aircraft_Has_Already_Been_Registered()
        {
            var sameAircraftDifferentObject = TestUtilities.CreateMockInstance<IAircraft>();
            sameAircraftDifferentObject.Setup(r => r.Icao24).Returns("ABC123");
            sameAircraftDifferentObject.Setup(r => r.Registration).Returns("G-ABCD");
            _PictureManagerReg = "G-ABCD";
            _PictureManagerIcao24 = "ABC123";

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "G-ABCD", FlightsCount = 88 };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            var details = _Fetcher.RegisterAircraft(sameAircraftDifferentObject.Object);
            Assert.AreSame(_DatabaseAircraftAndFlights, details.Aircraft);
            Assert.AreEqual(88, details.FlightsCount);
            Assert.AreSame(_PictureDetail, details.Picture);
        }

        [TestMethod]
        public void AircraftDetailFetcher_RegisterAircraft_Does_Not_Raise_Multiple_Events_When_Same_Aircraft_Registered_Twice_Before_Initial_Lookup()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            var sameAircraftDifferentObject = TestUtilities.CreateMockInstance<IAircraft>();
            sameAircraftDifferentObject.Setup(r => r.Icao24).Returns("ABC123");

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Fetcher.RegisterAircraft(sameAircraftDifferentObject.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }
        #endregion

        #region Automatic Deregistration
        [TestMethod]
        public void AircraftDetailFetcher_AutoDeregister_Stops_Database_Lookups_Once_Aircraft_Has_Not_Been_Registered_Within_Timeout()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(AutoDeregisterInterval);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual("ABC123", _GetManyAircraftAndFlightsByCodes.Single());
        }

        [TestMethod]
        public void AircraftDetailFetcher_AutoDeregister_Interval_Refreshed_By_Registration()
        {
            var sameAircraftDifferentObject = TestUtilities.CreateMockInstance<IAircraft>();
            sameAircraftDifferentObject.Setup(r => r.Icao24).Returns("ABC123");

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(AutoDeregisterInterval - 1);
            _Fetcher.RegisterAircraft(sameAircraftDifferentObject.Object);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(2);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _GetManyAircraftAndFlightsByCodes.Count);
            Assert.AreEqual("ABC123", _GetManyAircraftAndFlightsByCodes[0]);
            Assert.AreEqual("ABC123", _GetManyAircraftAndFlightsByCodes[1]);
        }
        #endregion

        #region Fetched
        #region Aircraft Database Record
        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_If_Aircraft_Has_Database_Record()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", FlightsCount = 100 };
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
            Assert.AreSame(_DatabaseAircraftAndFlights, _FetchedHandler.Args.Value.Aircraft);
            Assert.AreEqual(100, _FetchedHandler.Args.Value.FlightsCount);
            Assert.AreEqual("ABC123", _FetchedHandler.Args.Value.Icao24);
            Assert.AreSame(_Fetcher, _FetchedHandler.Sender);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_If_Aircraft_Record_Added_After_Interval_Has_Elapsed()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_Aircraft_Record_Added_Before_Interval_Has_Elapsed()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds - 1);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_Aircraft_Record_Unchanged_After_Interval_Has_Elapsed()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraft = new BaseStationAircraft() { ModeS = "ABC123" };
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_If_Aircraft_Record_Changes()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", FlightsCount = 52 };

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraft = new BaseStationAircraft() { ModeS = "ABC123", Registration = "New Registration" };
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
            Assert.AreEqual("New Registration", _FetchedHandler.Args.Value.Aircraft.Registration);
            Assert.AreEqual(52, _FetchedHandler.Args.Value.FlightsCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_Count_Of_Flights_Changes()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", FlightsCount = 10 };

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraft = new BaseStationAircraft() { ModeS = "ABC123" };
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", FlightsCount = 42 };
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_If_Aircraft_Has_No_Record()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
            Assert.IsNull(_FetchedHandler.Args.Value.Aircraft);
            Assert.AreEqual("ABC123", _FetchedHandler.Args.Value.Icao24);
            Assert.AreEqual(0, _FetchedHandler.Args.Value.FlightsCount);
            Assert.AreSame(_Fetcher, _FetchedHandler.Sender);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Does_Not_Fetch_Flight_Count_If_DatabaseRecord_Does_Not_Exist_And_Picture_Does()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Database.Verify(r => r.GetCountOfFlightsForAircraft(It.IsAny<BaseStationAircraft>(), It.IsAny<SearchBaseStationCriteria>()), Times.Never());
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_With_Null_Aircraft_If_Aircraft_Record_Removed_After_Interval_Has_Elapsed()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraftAndFlights = null;
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
            Assert.IsNotNull(_FetchedHandler.AllArgs[0].Value.Aircraft);
            Assert.IsNull(_FetchedHandler.AllArgs[1].Value.Aircraft);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_Aircraft_Record_Remains_Missing_After_Interval_Elapses()
        {
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_Aircraft_Record_Remains_Deleted_After_Interval_Elapses_Twice()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);     // First raise of Fetched

            _DatabaseAircraftAndFlights = null;
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);     // Second raise of Fetched

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);     // Should not trigger a third

            Assert.AreEqual(2, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_AircraftType_Changes()
        {
            // Standing data manager will tell us if it's possible that the aircraft type has changed - we should
            // only check it if the code changes and we already have a test for that.

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", ICAOTypeCode = "B747" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _FindAircraftType = "B747";
            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_AircraftType_Does_Not_Change()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", ICAOTypeCode = "B747" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }
        #endregion

        #region Picture
        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_If_Picture_Exists()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC" };
            _PictureManagerReg = "ABC";
            _PictureManagerIcao24 = "ABC123";

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreSame(_PictureDetail, _FetchedHandler.Args.Value.Picture);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_If_Picture_Exists_And_Database_Record_Does_Not()
        {
            _PictureManagerIcao24 = "ABC123";
            _PictureManagerReg = null;

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreSame(_PictureDetail, _FetchedHandler.Args.Value.Picture);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_If_Picture_Found_But_Database_Record_Unchanged()
        {
            _DatabaseAircraft =  _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC" };
            _PictureManagerReg = null;
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _PictureManagerReg = "ABC";
            _PictureManagerIcao24 = "ABC123";
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
            Assert.AreSame(_PictureDetail, _FetchedHandler.Args.Value.Picture);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_Picture_Details_Are_Unchanged()
        {
            _DatabaseAircraft = new BaseStationAircraft() { ModeS = "ABC123", Registration = "ABC" };
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC" };

            _PictureManagerReg = "ABC";
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }
        #endregion
        #endregion

        #region BaseStationDatabase.AircraftUpdated
        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_If_BaseStationDatabase_AircraftUpdated_Raised_And_Details_Have_Changed()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "New" };
            _Database.Raise(r => r.AircraftUpdated += null, new EventArgs<BaseStationAircraft>(_DatabaseAircraftAndFlights));

            Assert.AreEqual(2, _FetchedHandler.CallCount);
            Assert.AreSame(_DatabaseAircraftAndFlights, _FetchedHandler.Args.Value.Aircraft);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Does_Not_Fetch_Aircraft_Record_If_BaseStationDatabase_AircraftUpdated_Raised()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "New" };
            _Database.Raise(r => r.AircraftUpdated += null, new EventArgs<BaseStationAircraft>(_DatabaseAircraftAndFlights));

            Assert.AreEqual("ABC123", _GetManyAircraftAndFlightsByCodes.Single());
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Does_Refetches_Aircraft_Picture_If_BaseStationDatabase_AircraftUpdated_Raised_With_New_Registration()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC" };
            _PictureManagerReg = "XYZ";
            _PictureManagerIcao24 = "ABC123";
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "XYZ" };
            _Database.Raise(r => r.AircraftUpdated += null, new EventArgs<BaseStationAircraft>(_DatabaseAircraftAndFlights));
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _AircraftPictureManager.Verify(r => r.FindPicture(_PictureFolderCache.Object, "ABC123", "ABC", It.IsAny<PictureDetail>()), Times.Once());
            _AircraftPictureManager.Verify(r => r.FindPicture(_PictureFolderCache.Object, "ABC123", "XYZ", It.IsAny<PictureDetail>()), Times.Once());
            Assert.AreSame(_PictureDetail, _FetchedHandler.AllArgs[1].Value.Picture);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Refetches_Aircraft_Picture_If_BaseStationDatabase_AircraftUpdated_Raised_With_Same_Registration()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC" };
            _PictureManagerReg = "XYZ";
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC" };
            _Database.Raise(r => r.AircraftUpdated += null, new EventArgs<BaseStationAircraft>(_DatabaseAircraftAndFlights));

            _AircraftPictureManager.Verify(r => r.FindPicture(_PictureFolderCache.Object, "ABC123", "ABC", It.IsAny<PictureDetail>()), Times.Exactly(2));
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Raised_If_BaseStationDatabase_AircraftUpdated_Raised_And_Details_Have_Not_Changed()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Database.Raise(r => r.AircraftUpdated += null, new EventArgs<BaseStationAircraft>(_DatabaseAircraftAndFlights));

            Assert.AreEqual(1, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_BaseStationDatabase_AircraftUpdated_Raised_For_Unknown_Aircraft()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "New" };
            _Database.Raise(r => r.AircraftUpdated += null, new EventArgs<BaseStationAircraft>(_DatabaseAircraftAndFlights));

            Assert.AreEqual(0, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_Fetched_Not_Raised_If_Aircraft_Unchanged_After_AircraftUpdated_Raised()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraft = new BaseStationAircraft() { ModeS = "ABC123", Registration = "New" };
            _Database.Raise(r => r.AircraftUpdated += null, new EventArgs<BaseStationAircraft>(_DatabaseAircraftAndFlights));

            _Clock.UtcNowValue = _Clock.UtcNowValue.AddMilliseconds(IntervalMilliseconds);
            _Heartbeat.Raise(r => r.SlowTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
        }

        [TestMethod]
        public void AircraftDetailFetcher_BaseStationDatabase_AircraftUpdated_Exceptions_Get_Logged_But_Do_Not_Bubble_Up()
        {
            var messageLogged = false;
            _Log.Setup(r => r.WriteLine(It.IsAny<string>())).Callback((string x) => { messageLogged = true; });
            _Log.Setup(r => r.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback((string x, object[] args) => { messageLogged = true; });

            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            // The database doesn't get interrogated when the AircraftUpdated event comes through, but it should try to
            // fetch the picture so throw an exception there
            _AircraftPictureManager.Setup(r => r.FindPicture(It.IsAny<IDirectoryCache>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PictureDetail>()))
                                   .Callback((IDirectoryCache a, string b, string c, PictureDetail d) => { throw new InvalidOperationException(); });

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "G-ABCD" };
            _Database.Raise(r => r.AircraftUpdated += null, new EventArgs<BaseStationAircraft>(_DatabaseAircraftAndFlights));

            Assert.IsTrue(messageLogged);
        }
        #endregion

        #region BaseStationDatabase.FileNameChanged
        [TestMethod]
        public void AircraftDetailFetcher_BaseStationDatabase_FileNameChanged_Forces_Refresh_Of_All_Database_Details_On_Next_Fast_Tick()
        {
            // If it performed the refetch immediately then it could cause the options screen to appear to hang as the event
            // is probably being raised on the GUI thread. Pushing it to the next fast heartbeat stops that, it'll happen on
            // our timer thread.

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "OLD" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DatabaseAircraft = new BaseStationAircraft() { ModeS = "ABC123", Registration = "NEW" };
            _Database.Raise(r => r.FileNameChanged += null, EventArgs.Empty);
            Assert.AreEqual(1, _FetchedHandler.CallCount);

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
            var lastArgs = _FetchedHandler.AllArgs[1].Value;
            Assert.AreSame(_DatabaseAircraft, lastArgs.Aircraft);
        }
        #endregion

        #region AutoConfigPictureFolderCache_CacheConfigurationChanged
        [TestMethod]
        public void AircraftDetailFetcher_AutoConfigPictureFolderCache_CacheConfigurationChanged_Refetches_All_Details()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC", FlightsCount = 42 };
            _PictureManagerReg = null;
            _PictureManagerIcao24 = "ABC123";
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _PictureManagerReg = "ABC";
            _AutoConfigPictureFolderCache.Raise(r => r.CacheConfigurationChanged += null, EventArgs.Empty);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
            var lastEvent = _FetchedHandler.AllArgs[1];
            Assert.AreSame(_DatabaseAircraftAndFlights, lastEvent.Value.Aircraft);
            Assert.AreEqual("ABC123", lastEvent.Value.Icao24);
            Assert.AreEqual(42, lastEvent.Value.FlightsCount);
            Assert.AreSame(_PictureDetail, lastEvent.Value.Picture);
        }

        [TestMethod]
        public void AircraftDetailFetcher_AutoConfigPictureFolderCache_CacheConfigurationChanged_Does_Not_Rerun_Database_Search()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC" };
            _PictureManagerReg = "ABC";
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _AutoConfigPictureFolderCache.Raise(r => r.CacheConfigurationChanged += null, EventArgs.Empty);

            _AircraftPictureManager.Verify(r => r.FindPicture(_PictureFolderCache.Object, "ABC123", "ABC", It.IsAny<PictureDetail>()), Times.Exactly(2));
            Assert.AreEqual("ABC123", _GetManyAircraftAndFlightsByCodes.Single());
        }

        [TestMethod]
        public void AircraftDetailFetcher_StandingDataManager_AutoConfigPictureFolderCache_CacheConfigurationChanged_Exceptions_Get_Logged_But_Do_Not_Bubble_Up()
        {
            var messageLogged = false;
            _PictureManagerIcao24 = "ABC123";
            _Log.Setup(r => r.WriteLine(It.IsAny<string>())).Callback((string x) => { messageLogged = true; });
            _Log.Setup(r => r.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback((string x, object[] args) => { messageLogged = true; });

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", Registration = "ABC" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _PictureManagerThrowException = true;

            _AutoConfigPictureFolderCache.Raise(r => r.CacheConfigurationChanged += null, EventArgs.Empty);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsTrue(messageLogged);
        }
        #endregion

        #region StandingDataManager_LoadCompleted
        [TestMethod]
        public void StandingDataManager_LoadCompleted_Forces_Refetch_Of_All_AircraftTypes()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", ICAOTypeCode = "B747", FlightsCount = 99 };
            _PictureManagerIcao24 = "ABC123";
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(2, _FetchedHandler.CallCount);
            _FindAircraftType = "B747";
            _StandingDataManager.Raise(r => r.LoadCompleted += null, EventArgs.Empty);

            Assert.AreEqual(3, _FetchedHandler.CallCount);
            var lastEventArgs = _FetchedHandler.Args.Value;
            Assert.AreSame(_DatabaseAircraftAndFlights, lastEventArgs.Aircraft);
            Assert.AreEqual("ABC123", lastEventArgs.Icao24);
            Assert.AreSame(_AircraftType, lastEventArgs.AircraftType);
            Assert.AreEqual(99, lastEventArgs.FlightsCount);
            Assert.AreSame(_PictureDetail, lastEventArgs.Picture);
        }

        [TestMethod]
        public void StandingDataManager_LoadCompleted_Does_Not_Rerun_A_Database_Search()
        {
            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", ICAOTypeCode = "B747" };
            _FindAircraftType = "B747";
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _StandingDataManager.Raise(r => r.LoadCompleted += null, EventArgs.Empty);

            _StandingDataManager.Verify(r => r.FindAircraftType("B747"), Times.Exactly(2));
            Assert.AreEqual("ABC123", _GetManyAircraftAndFlightsByCodes.Single());
        }

        [TestMethod]
        public void StandingDataManager_LoadCompleted_Exceptions_Get_Logged_But_Do_Not_Bubble_Up()
        {
            var messageLogged = false;
            _Log.Setup(r => r.WriteLine(It.IsAny<string>())).Callback((string x) => { messageLogged = true; });
            _Log.Setup(r => r.WriteLine(It.IsAny<string>(), It.IsAny<object[]>())).Callback((string x, object[] args) => { messageLogged = true; });

            _DatabaseAircraftAndFlights = new BaseStationAircraftAndFlightsCount() { ModeS = "ABC123", ICAOTypeCode = "B747" };
            _Fetcher.RegisterAircraft(_Aircraft.Object);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _StandingDataManager.Setup(r => r.FindAircraftType(It.IsAny<string>()))
                                .Callback((string code) => { throw new InvalidOperationException(); });

            _StandingDataManager.Raise(r => r.LoadCompleted += null, EventArgs.Empty);

            Assert.IsTrue(messageLogged);
        }
        #endregion
    }
}
