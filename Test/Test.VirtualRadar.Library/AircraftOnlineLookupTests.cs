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
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class AircraftOnlineLookupTests
    {
        #region Fields
        public TestContext TestContext { get; set; }
        private IClassFactory _Snapshot;

        private IAircraftOnlineLookup _Lookup;
        private ClockMock _Clock;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private Configuration _Configuration;
        private Mock<IAircraftOnlineLookupProvider> _Provider;
        private EventRecorder<AircraftOnlineLookupEventArgs> _FetchedRecorder;
        private Mock<IHeartbeatService> _Heartbeat;
        private bool _DoLookupResult;
        private List<string> _ReportMissingIcaos;
        private string[] _RequestedIcaos;
        private Action _ProviderCallback;
        #endregion

        #region TestInitialise, TestCleanup
        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();
            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _RuntimeEnvironment.Setup(r => r.IsTest).Returns(true);
            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);
            _Heartbeat = TestUtilities.CreateMockSingleton<IHeartbeatService>();
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _Configuration = new Configuration();
            _Configuration.BaseStationSettings.LookupAircraftDetailsOnline = true;
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);

            _Provider = TestUtilities.CreateMockImplementation<IAircraftOnlineLookupProvider>();
            _Provider.SetupGet(r => r.MaxBatchSize).Returns(10);
            _Provider.SetupGet(r => r.MinSecondsBetweenRequests).Returns(1);
            _Provider.SetupGet(r => r.MaxSecondsAfterFailedRequest).Returns(5);

            _DoLookupResult = true;
            _ReportMissingIcaos = new List<string>();
            _RequestedIcaos = null;
            _ProviderCallback = null;
            _Provider.Setup(r => r.DoLookup(It.IsAny<string[]>(), It.IsAny<IList<AircraftOnlineLookupDetail>>(), It.IsAny<IList<string>>()))
                     .Returns((string[] icaos, IList<AircraftOnlineLookupDetail> fetched, IList<string> missing) => {
                        _RequestedIcaos = icaos;
                        if(icaos != null && fetched != null && missing != null) {
                            foreach(var icao in icaos) {
                                if(_ReportMissingIcaos.Contains(icao)) missing.Add(icao);
                                else                                   fetched.Add(new AircraftOnlineLookupDetail() { Icao = icao });
                            }
                        }
                        if(_ProviderCallback != null) _ProviderCallback();
                        return _DoLookupResult;
                     });

            _FetchedRecorder = new EventRecorder<AircraftOnlineLookupEventArgs>();

            _Lookup = Factory.Singleton.ResolveNewInstance<IAircraftOnlineLookup>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }
        #endregion

        #region Provider
        [TestMethod]
        public void AircraftOnlineLookup_Provider_Is_Initially_Null()
        {
            var lookup = Factory.Singleton.ResolveNewInstance<IAircraftOnlineLookup>();
            Assert.IsNull(lookup.Provider);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Provider_Filled_On_Call_To_Lookup()
        {
            _Lookup.Lookup("ABC123");
            Assert.IsNotNull(_Lookup.Provider);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Provider_Filled_On_Call_To_LookupMany()
        {
            _Lookup.LookupMany(new string[] { "ABC123", "123456" });
            Assert.IsNotNull(_Lookup.Provider);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AircraftOnlineLookup_Provider_SanityCheck_Rejects_Low_MaxBatchSize()
        {
            _Provider.SetupGet(r => r.MaxBatchSize).Returns(_Provider.Object.MaxBatchSize - 1);
            _Lookup.Lookup("ABC123");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AircraftOnlineLookup_Provider_SanityCheck_Rejects_Low_MinSecondsBetweenRequests()
        {
            _Provider.SetupGet(r => r.MinSecondsBetweenRequests).Returns(_Provider.Object.MinSecondsBetweenRequests - 1);
            _Lookup.Lookup("ABC123");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AircraftOnlineLookup_Provider_SanityCheck_Rejects_Low_MaxSecondsAfterFailedRequest()
        {
            _Provider.SetupGet(r => r.MaxSecondsAfterFailedRequest).Returns(_Provider.Object.MinSecondsBetweenRequests - 1); // Cannot be lower than minseconds
            _Lookup.Lookup("ABC123");
        }
        #endregion

        #region Lookup
        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Passes_Icao_To_Provider()
        {
            _Lookup.Lookup("ABC123");

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _RequestedIcaos.Length);
            Assert.AreEqual("ABC123", _RequestedIcaos[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Removes_ICAO_From_Lookup_After_Successful_Fetch()
        {
            _Lookup.Lookup("ABC123");

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _RequestedIcaos = null;

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Removes_Missing_Icaos_From_Queue()
        {
            _ReportMissingIcaos.Add("ABC123");
            _Lookup.Lookup("ABC123");

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _RequestedIcaos = null;

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Does_Not_Remove_ICAO_From_Lookup_After_Failed_Fetch()
        {
            _DoLookupResult = false;
            _Lookup.Lookup("ABC123");

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _RequestedIcaos = null;

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _RequestedIcaos.Length);
            Assert.AreEqual("ABC123", _RequestedIcaos[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Waits_For_MinSecondsBetweenRequests_Between_Requests()
        {
            _Lookup.Lookup("123321");
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _Lookup.Lookup("ABC123");
            _RequestedIcaos = null;

            _Clock.AddMilliseconds((_Provider.Object.MinSecondsBetweenRequests * 1000) - 1);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNull(_RequestedIcaos);

            _Clock.AddMilliseconds(2);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNotNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Failed_Fetch_Adds_MinSecondsBetweenRequests_Between_Requests()
        {
            _DoLookupResult = false;

            _Lookup.Lookup("123321");
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _RequestedIcaos = null;

            _Clock.AddMilliseconds((2 * _Provider.Object.MinSecondsBetweenRequests * 1000) - 1);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNull(_RequestedIcaos);

            _Clock.AddMilliseconds(2);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNotNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Failed_Fetch_Will_Not_Wait_Longer_Than_MaxSecondsAfterFailedRequest()
        {
            _DoLookupResult = false;
            _Lookup.Lookup("123321");
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            _RequestedIcaos = null;

            var minSeconds = _Provider.Object.MinSecondsBetweenRequests;
            var maxSeconds = _Provider.Object.MaxSecondsAfterFailedRequest;
            for(var pauseSeconds = minSeconds * 2;pauseSeconds <= maxSeconds;pauseSeconds += minSeconds) {
                _Clock.AddMilliseconds((pauseSeconds * 1000) + 1);
                _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
                Assert.IsNotNull(_RequestedIcaos);
                _RequestedIcaos = null;
            }

            _Clock.AddMilliseconds((maxSeconds * 1000) - 1);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNull(_RequestedIcaos);

            _Clock.AddMilliseconds(2);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNotNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Removes_ICAOs_From_Queue_If_Fetches_Continuously_Fail()
        {
            var interval = _Provider.Object.MinSecondsBetweenRequests;
            _DoLookupResult = false;
            var insertedTime = _Clock.UtcNowValue;
            _Lookup.Lookup("123456");

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);     // seconds between fetches = 2 * interval

            _RequestedIcaos = null;
            _Clock.AddMilliseconds((30 * 60000) - (3 * 1000 * interval));
            _Lookup.Lookup("ABC123");                                       // seconds between fetches = 3 * interval
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.AreEqual(2, _RequestedIcaos.Length);

            _Clock.AddMilliseconds(1 + (3 * 1000 * interval));
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.AreEqual(1, _RequestedIcaos.Length);
            Assert.AreEqual("ABC123", _RequestedIcaos[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Provider_Exception_Does_Not_Stop_Removal_Of_ICAO_From_Queue()
        {
            
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Successful_Lookup_Resets_Pause_Between_Requests()
        {
            var interval = _Provider.Object.MinSecondsBetweenRequests;

            _Lookup.Lookup("ABC123");

            _DoLookupResult = false;
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _DoLookupResult = true;
            _Clock.AddMilliseconds(1 + (2 * interval * 1000));
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _RequestedIcaos = null;

            _Lookup.Lookup("123123");
            _Clock.AddMilliseconds(1 + (interval * 1000));
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsNotNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_AircraftFetched_Raised_With_Lookup_Results()
        {
            _Lookup.AircraftFetched += _FetchedRecorder.Handler;
            _Lookup.Lookup("ABC123");

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedRecorder.CallCount);
            Assert.AreEqual("ABC123", _FetchedRecorder.Args.AircraftDetails[0].Icao);
            Assert.AreEqual(0, _FetchedRecorder.Args.MissingIcaos.Count);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_AircraftFetched_Raised_When_Icaos_Are_Missing()
        {
            _ReportMissingIcaos.Add("ABC123");
            _Lookup.AircraftFetched += _FetchedRecorder.Handler;
            _Lookup.Lookup("ABC123");

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(1, _FetchedRecorder.CallCount);
            Assert.AreEqual(0, _FetchedRecorder.Args.AircraftDetails.Count);
            Assert.AreEqual("ABC123", _FetchedRecorder.Args.MissingIcaos[0]);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_AircraftFetched_Raised_Before_Icaos_Are_Removed_From_Queue()
        {
            _Lookup.Lookup("ABC123");
            _Lookup.AircraftFetched += _FetchedRecorder.Handler;
            _FetchedRecorder.EventRaised += (sender, args) => {
                _Lookup.Lookup("ABC123");
            };

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            _RequestedIcaos = null;

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.IsNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_AircraftFetched_Event_Handler_Exception_Does_Not_Prevent_Queue_Removal()
        {
            _ProviderCallback = () => { throw new NotImplementedException(); };

            _Lookup.Lookup("ABC123");
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _ProviderCallback = null;
            _RequestedIcaos = null;

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_AircraftFetched_Event_Handler_Exception_Does_Not_Raise_Fetched_Event()
        {
            _ProviderCallback = () => { throw new NotImplementedException(); };
            _Lookup.AircraftFetched += _FetchedRecorder.Handler;

            _Lookup.Lookup("ABC123");
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(0, _FetchedRecorder.CallCount);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_AircraftFetched_Event_Handler_WebException_Prevents_Queue_Removal()
        {
            _ProviderCallback = () => { throw new System.Net.WebException(); };

            _Lookup.Lookup("ABC123");
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            _ProviderCallback = null;
            _RequestedIcaos = null;

            _Clock.AddMilliseconds(60000);
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNotNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_AircraftFetched_Event_Handler_WebException_Does_Not_Raise_Fetched_Event()
        {
            _ProviderCallback = () => { throw new System.Net.WebException(); };
            _Lookup.AircraftFetched += _FetchedRecorder.Handler;

            _Lookup.Lookup("ABC123");
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);

            Assert.AreEqual(0, _FetchedRecorder.CallCount);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Does_Nothing_If_Lookup_Disabled()
        {
            _Configuration.BaseStationSettings.LookupAircraftDetailsOnline = false;
            _Lookup.Lookup("ABC123");
            _Configuration.BaseStationSettings.LookupAircraftDetailsOnline = true;

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_LookupMany_Does_Nothing_If_Lookup_Disabled()
        {
            _Configuration.BaseStationSettings.LookupAircraftDetailsOnline = false;
            _Lookup.LookupMany(new string[] { "ABC123", "123456" });
            _Configuration.BaseStationSettings.LookupAircraftDetailsOnline = true;

            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNull(_RequestedIcaos);
        }

        [TestMethod]
        public void AircraftOnlineLookup_Lookup_Clears_Queue_On_Heartbeat_If_Lookup_Disabled()
        {
            _Lookup.Lookup("ABC123");

            _Configuration.BaseStationSettings.LookupAircraftDetailsOnline = false;
            _Heartbeat.Raise(r => r.FastTick += null, EventArgs.Empty);
            Assert.IsNull(_RequestedIcaos);
        }
        #endregion
    }
}
