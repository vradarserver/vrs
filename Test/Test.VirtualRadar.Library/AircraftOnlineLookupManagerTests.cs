// Copyright © 2015 onwards, Andrew Whewell
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

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class AircraftOnlineLookupManagerTests
    {
        #region DisposableCache
        class DisposableCache : IAircraftOnlineLookupCache, IDisposable
        {
            public int DisposeCallCount { get; private set; }
            public void Dispose()
            {
                ++DisposeCallCount;
            }

            public bool Enabled
            {
                get { return false; }
            }

            public bool IsWriteOnly { get; set; }

            public AircraftOnlineLookupDetail Load(string icao, BaseStationAircraft baseStationAircraft, bool searchedForBaseStationAircraft)
            {
                return null;
            }

            public Dictionary<string, AircraftOnlineLookupDetail> LoadMany(IEnumerable<string> icaos, IDictionary<string, BaseStationAircraft> baseStationAircraft)
            {
                return new Dictionary<string,AircraftOnlineLookupDetail>();
            }

            public void Save(AircraftOnlineLookupDetail lookupDetail)
            {
                ;
            }

            public void SaveMany(IEnumerable<AircraftOnlineLookupDetail> lookupDetails)
            {
                ;
            }

            public void RecordMissing(string icao)
            {
                ;
            }

            public void RecordManyMissing(IEnumerable<string> icaos)
            {
                ;
            }
        }
        #endregion

        #region Fields
        public TestContext TestContext { get; set; }

        private IClassFactory _Snapshot;
        private IAircraftOnlineLookupManager _Manager;
        private Mock<IAircraftOnlineLookup> _Lookup;
        private List<string> _LookupUsed;
        private List<string> _LookupIcaos;
        private bool _LookupResponds;
        private Mock<IAircraftOnlineLookupCache> _Cache1;
        private Mock<IAircraftOnlineLookupCache> _Cache2;
        private List<string> _Cache1Icaos;
        private List<string> _Cache2Icaos;
        private List<AircraftOnlineLookupDetail> _SavedDetails;
        private List<string> _RecordedMissingIcaos;
        private List<int> _LookupCacheUsed;
        private List<int> _SaveCacheUsed;
        private Action<AircraftOnlineLookupDetail> _CreateCacheRecordCallback;
        private Action _SaveCallback;
        private EventRecorder<AircraftOnlineLookupEventArgs> _FetchedRecorder;
        private ClockMock _Clock;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _FetchedRecorder = new EventRecorder<AircraftOnlineLookupEventArgs>();
            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _Lookup = TestUtilities.CreateMockSingleton<IAircraftOnlineLookup>();
            _LookupUsed = new List<string>();
            _LookupIcaos = new List<string>();
            _LookupResponds = true;
            _Lookup.Setup(r => r.Lookup(It.IsAny<string>())).Callback((string icao) => {
                _LookupUsed.Add(icao);
                if(_LookupResponds) {
                    var args = BuildAircraftOnlineLookupEventArgs(new string[] { icao }, _LookupIcaos);
                    _Lookup.Raise(r => r.AircraftFetched += null, args);
                }
            });
            _Lookup.Setup(r => r.LookupMany(It.IsAny<IEnumerable<string>>())).Callback((IEnumerable<string> icaos) => {
                _LookupUsed.AddRange(icaos);
                if(_LookupResponds) {
                    var args = BuildAircraftOnlineLookupEventArgs(icaos, _LookupIcaos);
                    _Lookup.Raise(r => r.AircraftFetched += null, args);
                }
            });

            _Cache1 = TestUtilities.CreateMockInstance<IAircraftOnlineLookupCache>();
            _Cache2 = TestUtilities.CreateMockInstance<IAircraftOnlineLookupCache>();
            _Cache1Icaos = new List<string>();
            _Cache2Icaos = new List<string>();
            _SavedDetails = new List<AircraftOnlineLookupDetail>();
            _RecordedMissingIcaos = new List<string>();
            _LookupCacheUsed = new List<int>();
            _SaveCacheUsed = new List<int>();
            _SaveCallback = null;
            _CreateCacheRecordCallback = null;
            SetupCache(1, _Cache1, _Cache1Icaos);
            SetupCache(2, _Cache2, _Cache2Icaos);

            _Manager = Factory.Singleton.Resolve<IAircraftOnlineLookupManager>();
            _Manager.RegisterCache(_Cache1.Object, 1, false);
            _Manager.RegisterCache(_Cache2.Object, 2, false);   // <-- this has a higher priority and should take precedence
        }

        private void SetupCache(int cacheNumber, Mock<IAircraftOnlineLookupCache> cache, List<string> knownIcaos)
        {
            cache.SetupGet(r => r.Enabled).Returns(true);

            cache.Setup(r => r.Load(It.IsAny<string>(), It.IsAny<BaseStationAircraft>(), It.IsAny<bool>())).Returns((string icao, BaseStationAircraft baseStationAircraft, bool searchedForBaseStationAircraft) => {
                _LookupCacheUsed.Add(cacheNumber);
                AircraftOnlineLookupDetail result = null;
                if(knownIcaos.Contains(icao)) result = CreateMockAircraftOnlineLookupDetail(icao);
                return result;
            });

            cache.Setup(r => r.LoadMany(It.IsAny<IEnumerable<string>>(), It.IsAny<IDictionary<string, BaseStationAircraft>>())).Returns((IEnumerable<string> icaos, IDictionary<string,BaseStationAircraft> databaseAircraft) => {
                _LookupCacheUsed.Add(cacheNumber);
                var result = new Dictionary<string, AircraftOnlineLookupDetail>();
                foreach(var icao in icaos) {
                    if(knownIcaos.Contains(icao)) result.Add(icao, CreateMockAircraftOnlineLookupDetail(icao));
                    else                          result.Add(icao, null);
                }
                return result;
            });

            cache.Setup(r => r.Save(It.IsAny<AircraftOnlineLookupDetail>())).Callback((AircraftOnlineLookupDetail detail) => {
                _SaveCacheUsed.Add(cacheNumber);
                _SavedDetails.Add(detail);
                if(_SaveCallback != null) _SaveCallback();
            });
            cache.Setup(r => r.SaveMany(It.IsAny<IEnumerable<AircraftOnlineLookupDetail>>())).Callback((IEnumerable<AircraftOnlineLookupDetail> details) => {
                _SaveCacheUsed.Add(cacheNumber);
                _SavedDetails.AddRange(details);
                if(_SaveCallback != null) _SaveCallback();
            });

            cache.Setup(r => r.RecordMissing(It.IsAny<string>())).Callback((string icao) => {
                _SaveCacheUsed.Add(cacheNumber);
                _RecordedMissingIcaos.Add(icao);
                if(_SaveCallback != null) _SaveCallback();
            });
            cache.Setup(r => r.RecordManyMissing(It.IsAny<IEnumerable<string>>())).Callback((IEnumerable<string> icaos) => {
                _SaveCacheUsed.Add(cacheNumber);
                _RecordedMissingIcaos.AddRange(icaos);
                if(_SaveCallback != null) _SaveCallback();
            });
        }

        private AircraftOnlineLookupDetail CreateMockAircraftOnlineLookupDetail(string icao)
        {
            var result = new AircraftOnlineLookupDetail() {
                Icao = icao,
                Registration = "A",
                CreatedUtc = _Clock.UtcNowValue,
                UpdatedUtc = _Clock.UtcNowValue,
            };
            if(_CreateCacheRecordCallback != null) _CreateCacheRecordCallback(result);

            return result;
        }

        private AircraftOnlineLookupEventArgs BuildAircraftOnlineLookupEventArgs(IEnumerable<string> icaos, List<string> knownIcaos)
        {
            var aircraftDetails = new List<AircraftOnlineLookupDetail>();
            var missingIcaos = new List<string>();
            foreach(var icao in icaos) {
                if(knownIcaos.Contains(icao)) {
                    aircraftDetails.Add(CreateMockAircraftOnlineLookupDetail(icao));
                } else {
                    missingIcaos.Add(icao);
                }
            }

            return new AircraftOnlineLookupEventArgs(aircraftDetails, missingIcaos);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }
        #endregion

        #region Ctor and properties
        [TestMethod]
        public void AircraftOnlineLookupManager_Singleton_Returns_Same_Instance()
        {
            var obj1 = Factory.Singleton.Resolve<IAircraftOnlineLookupManager>().Singleton;
            var obj2 = Factory.Singleton.Resolve<IAircraftOnlineLookupManager>().Singleton;

            Assert.AreSame(obj1, obj2);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void AircraftOnlineLookupManager_Dispose_Disposes_Of_Managed_Caches()
        {
            var manager = Factory.Singleton.Resolve<IAircraftOnlineLookupManager>();
            var disposableCache = new DisposableCache();
            manager.RegisterCache(disposableCache, 1, letManagerControlLifetime: true);

            manager.Dispose();

            Assert.AreEqual(1, disposableCache.DisposeCallCount);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Dispose_Does_Not_Dispose_Of_Unmanaged_Caches()
        {
            var manager = Factory.Singleton.Resolve<IAircraftOnlineLookupManager>();
            var disposableCache = new DisposableCache();
            manager.RegisterCache(disposableCache, 1, letManagerControlLifetime: false);

            manager.Dispose();

            Assert.AreEqual(0, disposableCache.DisposeCallCount);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Dispose_Does_Not_Call_Dispose_On_Managed_Caches_That_Are_Not_Disposable()
        {
            var manager = Factory.Singleton.Resolve<IAircraftOnlineLookupManager>();
            manager.RegisterCache(_Cache1.Object, 1, letManagerControlLifetime: true);

            manager.Dispose();
            // All we're checking here is that an exception is not thrown if the cache is not disposable
        }
        #endregion

        #region RegisterCache
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AircraftOnlineLookupManager_RegisterCache_Throws_If_Cache_Is_Null()
        {
            _Manager.RegisterCache(null, 1, letManagerControlLifetime: false);
        }
        #endregion

        #region DeregisterCache
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AircraftOnlineLookupManager_DeregisterCache_Throws_If_Cache_Is_Null()
        {
            _Manager.RegisterCache(null, 1, letManagerControlLifetime: false);
        }
        #endregion

        #region IsCacheObjectRegistered
        [TestMethod]
        public void AircraftOnlineLookupManager_IsCacheObjectRegistered_Returns_True_If_Cache_Is_Registered()
        {
            Assert.IsTrue(_Manager.IsCacheObjectRegistered(_Cache1.Object));
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_IsCacheObjectRegistered_Returns_False_If_Cache_Is_Not_Registered()
        {
            Assert.IsFalse(_Manager.IsCacheObjectRegistered(new DisposableCache()));
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_IsCacheObjectRegistered_Returns_False_If_Passed_Null()
        {
            Assert.IsFalse(_Manager.IsCacheObjectRegistered(null));
        }
        #endregion

        #region Lookup
        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Fetches_Icao_From_Cache()
        {
            _Cache2Icaos.Add("ABC123");

            var result = _Manager.Lookup("ABC123", null, false);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Passes_Through_BaseStationAircraft_To_Cache()
        {
            var aircraft = new BaseStationAircraft();

            _Manager.Lookup("ABC123", aircraft, true);
            _Manager.Lookup("XYZ987", aircraft, false);

            _Cache2.Verify(r => r.Load("ABC123", aircraft, true), Times.Once());
            _Cache2.Verify(r => r.Load("XYZ987", aircraft, false), Times.Once());
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Does_Not_Fetch_Icao_From_Second_Cache_If_Missing_From_First()
        {
            _Cache1Icaos.Add("ABC123");

            var result = _Manager.Lookup("ABC123", null, false);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Does_Not_Lookup_Aircraft_If_Found()
        {
            _Cache2Icaos.Add("ABC123");

            var result = _Manager.Lookup("ABC123", null, false);

            Assert.AreEqual(0, _LookupUsed.Count);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Refreshes_Aircraft_Marked_As_Missing_After_24_Hours()
        {
            _CreateCacheRecordCallback = (record) => {
                record.Registration = null;
                record.UpdatedUtc = _Clock.UtcNowValue.AddDays(-1).AddMilliseconds(1);
            };

            _Cache2Icaos.Add("ABC123");

            var result = _Manager.Lookup("ABC123", null, false);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, _LookupUsed.Count);

            _CreateCacheRecordCallback = (record) => {
                record.Registration = null;
                record.UpdatedUtc = _Clock.UtcNowValue.AddDays(-1);
            };

            result = _Manager.Lookup("ABC123", null, false);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, _LookupUsed.Count);
            Assert.AreEqual("ABC123", _LookupUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Refreshes_Aircraft_After_Four_Weeks()
        {
            _CreateCacheRecordCallback = (record) => {
                record.UpdatedUtc = _Clock.UtcNowValue.AddDays(-28).AddMilliseconds(1);
            };

            _Cache2Icaos.Add("ABC123");

            var result = _Manager.Lookup("ABC123", null, false);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, _LookupUsed.Count);

            _CreateCacheRecordCallback = (record) => {
                record.UpdatedUtc = _Clock.UtcNowValue.AddDays(-28);
            };

            result = _Manager.Lookup("ABC123", null, false);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, _LookupUsed.Count);
            Assert.AreEqual("ABC123", _LookupUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Looks_Up_Missing_Aircraft()
        {
            var result = _Manager.Lookup("ABC123", null, false);

            Assert.AreEqual(1, _LookupUsed.Count);
            Assert.AreEqual("ABC123", _LookupUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Saves_Results_Of_Successful_Lookup()
        {
            _LookupIcaos.Add("ABC123");
            var result = _Manager.Lookup("ABC123", null, false);

            Assert.AreEqual(1, _SavedDetails.Count);
            Assert.AreEqual(0, _RecordedMissingIcaos.Count);
            Assert.AreEqual("ABC123", _SavedDetails[0].Icao);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Saves_Results_Of_Missing_Lookup()
        {
            var result = _Manager.Lookup("ABC123", null, false);

            Assert.AreEqual(0, _SavedDetails.Count);
            Assert.AreEqual(1, _RecordedMissingIcaos.Count);
            Assert.AreEqual("ABC123", _RecordedMissingIcaos[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Does_Not_Fetch_From_Disabled_Caches()
        {
            _Cache1Icaos.Add("ABC123");
            _Cache2Icaos.Add("ABC123");
            _Cache1.SetupGet(r => r.Enabled).Returns(false);
            _Cache2.SetupGet(r => r.Enabled).Returns(false);

            Assert.IsNull(_Manager.Lookup("ABC123", null, false));
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Does_Not_Save_To_Disabled_Caches()
        {
            _Cache1.SetupGet(r => r.Enabled).Returns(false);
            _Cache2.SetupGet(r => r.Enabled).Returns(false);

            _Manager.Lookup("ABC123", null, false);

            Assert.AreEqual(0, _SaveCacheUsed.Count);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Searches_Higher_Priority_Caches_Before_Lower()
        {
            _Cache1Icaos.Add("ABC123");
            _Cache2Icaos.Add("ABC123");

            _Manager.Lookup("ABC123", null, false);

            Assert.AreEqual(1, _LookupCacheUsed.Count);
            Assert.AreEqual(2, _LookupCacheUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Saves_To_Higher_Priority_Caches_Before_Lower()
        {
            _Manager.Lookup("ABC123", null, false);

            Assert.AreEqual(1, _SaveCacheUsed.Count);
            Assert.AreEqual(2, _SaveCacheUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Raises_AircraftFetched_When_Aircraft_Looked_Up()
        {
            _Manager.AircraftFetched += _FetchedRecorder.Handler;
            _Manager.Lookup("ABC123", null, false);
            Assert.AreEqual(1, _FetchedRecorder.CallCount);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_Lookup_Raises_AircraftFetched_After_Updating_Cache()
        {
            var eventCountInSave = 0;
            _SaveCallback = () => { eventCountInSave = _FetchedRecorder.CallCount; };

            _Manager.AircraftFetched += _FetchedRecorder.Handler;
            _Manager.Lookup("ABC123", null, false);

            Assert.AreEqual(0, eventCountInSave);
        }
        #endregion

        #region LookupMany
        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Fetches_Icaos_From_Cache()
        {
            _Cache2Icaos.Add("ABC123");
            _Cache2Icaos.Add("123456");

            var result = _Manager.LookupMany(new string[] { "ABC123", "123456" }, null);
            Assert.IsTrue(result.ContainsKey("ABC123"));
            Assert.IsTrue(result.ContainsKey("123456"));
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Passes_DatabaseAircraft_To_Cache()
        {
            var dictionary = new Dictionary<string, BaseStationAircraft>();

            _Manager.LookupMany(new string[] { "ABC123" }, dictionary);

            _Cache2.Verify(r => r.LoadMany(It.IsAny<IEnumerable<string>>(), dictionary), Times.Once());
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Does_Not_Fetch_Icaos_From_Second_Cache_If_Missing_From_First()
        {
            _Cache1Icaos.Add("ABC123");
            _Cache1Icaos.Add("123456");

            var result = _Manager.LookupMany(new string[] { "ABC123", "123456" }, null);
            Assert.IsNull(result["ABC123"]);
            Assert.IsNull(result["123456"]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Only_Fetches_Icaos_From_One_Cache()
        {
            _Cache1Icaos.Add("ABC123");
            _Cache2Icaos.Add("123456");

            var result = _Manager.LookupMany(new string[] { "ABC123", "123456" }, null);
            Assert.IsTrue(result.ContainsKey("ABC123"));
            Assert.IsTrue(result.ContainsKey("123456"));
            Assert.IsNull(result["ABC123"]);
            Assert.IsNotNull(result["123456"]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Returns_Null_Entries_For_Missing_Icaos()
        {
            var result = _Manager.LookupMany(new string[] { "ABC123", }, null);
            Assert.IsTrue(result.ContainsKey("ABC123"));
            Assert.IsNull(result["ABC123"]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Does_Not_Lookup_Aircraft_If_Found()
        {
            _Cache2Icaos.Add("ABC123");

            var result = _Manager.LookupMany(new string[] { "ABC123" }, null);

            Assert.AreEqual(0, _LookupUsed.Count);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Refreshes_Aircraft_Marked_As_Missing_After_24_Hours()
        {
            _CreateCacheRecordCallback = (record) => {
                record.Registration = null;
                record.UpdatedUtc = _Clock.UtcNowValue.AddDays(-1).AddMilliseconds(1);
            };

            _Cache2Icaos.Add("ABC123");

            var result = _Manager.LookupMany(new string[] { "ABC123" }, null);
            Assert.IsTrue(result.ContainsKey("ABC123"));
            Assert.AreEqual(0, _LookupUsed.Count);

            _CreateCacheRecordCallback = (record) => {
                record.Registration = null;
                record.UpdatedUtc = _Clock.UtcNowValue.AddDays(-1);
            };

            result = _Manager.LookupMany(new string[] { "ABC123" }, null);
            Assert.IsTrue(result.ContainsKey("ABC123"));
            Assert.AreEqual(1, _LookupUsed.Count);
            Assert.AreEqual("ABC123", _LookupUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Refreshes_Aircraft_After_Four_Weeks()
        {
            _CreateCacheRecordCallback = (record) => {
                record.UpdatedUtc = _Clock.UtcNowValue.AddDays(-28).AddMilliseconds(1);
            };

            _Cache2Icaos.Add("ABC123");

            var result = _Manager.LookupMany(new string[] { "ABC123" }, null);
            Assert.IsTrue(result.ContainsKey("ABC123"));
            Assert.AreEqual(0, _LookupUsed.Count);

            _CreateCacheRecordCallback = (record) => {
                record.UpdatedUtc = _Clock.UtcNowValue.AddDays(-28);
            };

            result = _Manager.LookupMany(new string[] { "ABC123" }, null);
            Assert.IsTrue(result.ContainsKey("ABC123"));
            Assert.AreEqual(1, _LookupUsed.Count);
            Assert.AreEqual("ABC123", _LookupUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Looks_Up_Missing_Aircraft()
        {
            var result = _Manager.LookupMany(new string[] { "ABC123" }, null);

            Assert.AreEqual(1, _LookupUsed.Count);
            Assert.AreEqual("ABC123", _LookupUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Looks_Up_Missing_Aircraft_But_Not_Found_Aircraft()
        {
            _Cache2Icaos.Add("123456");
            var result = _Manager.LookupMany(new string[] { "ABC123", "123456" }, null);

            Assert.AreEqual(2, result.Count);
            Assert.IsNull(result["ABC123"]);
            Assert.IsNotNull(result["123456"]);

            Assert.AreEqual(1, _LookupUsed.Count);
            Assert.AreEqual("ABC123", _LookupUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Saves_Results_Of_Successful_Lookup()
        {
            _LookupIcaos.Add("ABC123");
            _Manager.LookupMany(new string[] { "ABC123" }, null);

            Assert.AreEqual(1, _SavedDetails.Count);
            Assert.AreEqual(0, _RecordedMissingIcaos.Count);
            Assert.AreEqual("ABC123", _SavedDetails[0].Icao);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Saves_Results_Of_Missing_Lookup()
        {
            _Manager.LookupMany(new string[] { "ABC123" }, null);

            Assert.AreEqual(0, _SavedDetails.Count);
            Assert.AreEqual(1, _RecordedMissingIcaos.Count);
            Assert.AreEqual("ABC123", _RecordedMissingIcaos[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Does_Not_Fetch_From_Disabled_Caches()
        {
            _Cache1Icaos.Add("ABC123");
            _Cache2Icaos.Add("ABC123");
            _Cache1.SetupGet(r => r.Enabled).Returns(false);
            _Cache2.SetupGet(r => r.Enabled).Returns(false);

            var result = _Manager.LookupMany(new string[] { "ABC123" }, null);
            Assert.AreEqual(1, result.Count);
            Assert.IsNull(result["ABC123"]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Does_Not_Save_To_Disabled_Caches()
        {
            _Cache1.SetupGet(r => r.Enabled).Returns(false);
            _Cache2.SetupGet(r => r.Enabled).Returns(false);

            _Manager.LookupMany(new string[] { "ABC123" }, null);

            Assert.AreEqual(0, _SaveCacheUsed.Count);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Searches_Higher_Priority_Caches_Before_Lower()
        {
            _Cache1Icaos.Add("ABC123");
            _Cache2Icaos.Add("ABC123");

            _Manager.LookupMany(new string[] { "ABC123" }, null);

            Assert.AreEqual(1, _LookupCacheUsed.Count);
            Assert.AreEqual(2, _LookupCacheUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Saves_To_Higher_Priority_Caches_Before_Lower()
        {
            _Manager.LookupMany(new string[] { "ABC123" }, null);

            Assert.AreEqual(1, _SaveCacheUsed.Count);
            Assert.AreEqual(2, _SaveCacheUsed[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Raises_AircraftFetched_When_Aircraft_Looked_Up()
        {
            _Manager.AircraftFetched += _FetchedRecorder.Handler;
            _Manager.LookupMany(new string[] { "ABC123" }, null);
            Assert.AreEqual(1, _FetchedRecorder.CallCount);
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_LookupMany_Raises_AircraftFetched_After_Updating_Cache()
        {
            var eventCountInSave = 0;
            _SaveCallback = () => { eventCountInSave = _FetchedRecorder.CallCount; };

            _Manager.AircraftFetched += _FetchedRecorder.Handler;
            _Manager.LookupMany(new string[] { "ABC123" }, null);

            Assert.AreEqual(0, eventCountInSave);
        }
        #endregion

        #region RecordNeedsRefresh
        class RecordNeedsRefresh
        {
            public string Registration      { get; set; }
            public DateTime LastUpdatedUtc  { get; set; }
            public bool ExpectedResult      { get; set; }

            public RecordNeedsRefresh(string registration, DateTime lastUpdatedUtc, bool expectedResult)
            {
                Registration = registration;
                LastUpdatedUtc = lastUpdatedUtc;
                ExpectedResult = expectedResult;
            }

            public override string ToString()
            {
                return String.Format("Registration = {0}, LastUpdatedUtc = {1}, ExpectedResult = {2}", Registration, LastUpdatedUtc, ExpectedResult);
            }
        }

        [TestMethod]
        public void AircraftOnlineLookupManager_RecordNeedsRefresh_Returns_Correct_Value()
        {
            foreach(var testParams in new RecordNeedsRefresh[] {
                // Missing aircraft details are refreshed after 24 hours have elapsed
                new RecordNeedsRefresh(null, _Clock.UtcNowValue, false),
                new RecordNeedsRefresh("",   _Clock.UtcNowValue, false),
                new RecordNeedsRefresh(null, _Clock.UtcNowValue.AddDays(-1).AddMilliseconds(1), false),
                new RecordNeedsRefresh("",   _Clock.UtcNowValue.AddDays(-1).AddMilliseconds(1), false),
                new RecordNeedsRefresh(null, _Clock.UtcNowValue.AddDays(-1).AddMilliseconds(-2), true),
                new RecordNeedsRefresh("",   _Clock.UtcNowValue.AddDays(-1).AddMilliseconds(-2), true),

                // Known aircraft details are refreshed after 28 days
                new RecordNeedsRefresh("A",  _Clock.UtcNowValue, false),
                new RecordNeedsRefresh("A",  _Clock.UtcNowValue.AddDays(-28).AddMilliseconds(1), false),
                new RecordNeedsRefresh("A",  _Clock.UtcNowValue.AddDays(-28).AddMilliseconds(-2), true),
            }) {
                var result = _Manager.RecordNeedsRefresh(testParams.Registration, testParams.LastUpdatedUtc);
                Assert.AreEqual(testParams.ExpectedResult, result, "Got {0} for {1}", result, testParams);
            }
        }
        #endregion
    }
}
