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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface;
using VirtualRadar.Library;
using Test.Framework;
using Moq;
using InterfaceFactory;

namespace Test.VirtualRadar.Library
{
    [TestClass]
    public class SimpleAircraftListTests
    {
        #region Fields, TestInitialise etc.
        public TestContext TestContext { get; set; }

        private IClassFactory _OriginalClassFactory;
        private ISimpleAircraftList _AircraftList;
        private Mock<IAircraft> _Aircraft;
        private ClockMock _Clock;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalClassFactory = Factory.TakeSnapshot();

            _Clock = new ClockMock();
            Factory.RegisterInstance<IClock>(_Clock.Object);

            _AircraftList = Factory.Resolve<ISimpleAircraftList>();
            _Aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();

            _Aircraft.Setup(m => m.Clone()).Returns(() => {
                var result = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
                result.Object.UniqueId = _Aircraft.Object.UniqueId;
                return result.Object;
            });
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalClassFactory);
        }
        #endregion

        #region Constructor and properties
        [TestMethod]
        public void SimpleAircraftList_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            Assert.IsNotNull(_AircraftList.ListSyncLock);
            Assert.AreEqual(0, _AircraftList.Aircraft.Count);

            TestUtilities.TestProperty(_AircraftList, "Source", AircraftListSource.FakeAircraftList, AircraftListSource.FlightSimulatorX);

            Assert.AreEqual(AircraftListSource.FlightSimulatorX, ((IAircraftList)_AircraftList).Source);
            Assert.AreEqual(0, _AircraftList.Count);
        }

        [TestMethod]
        public void SimpleAircraftList_Count_Returns_Count_Of_Aircraft()
        {
            Assert.AreEqual(0, _AircraftList.Count);

            _AircraftList.Aircraft.Add(_Aircraft.Object);
            Assert.AreEqual(1, _AircraftList.Count);

            _AircraftList.Aircraft.Add(_Aircraft.Object);
            Assert.AreEqual(2, _AircraftList.Count);

            _AircraftList.Aircraft.Clear();
            Assert.AreEqual(0, _AircraftList.Count);
        }
        #endregion

        #region FindAircraft
        [TestMethod]
        public void SimpleAircraftList_FindAircraft_Returns_Null_If_Aircraft_Does_Not_Exist()
        {
            Assert.IsNull(_AircraftList.FindAircraft(100));
        }

        [TestMethod]
        public void SimpleAircraftList_FindAircraft_Returns_Aircraft_If_Exists()
        {
            _Aircraft.Object.UniqueId = 100;
            _AircraftList.Aircraft.Add(_Aircraft.Object);

            Assert.AreEqual(100, _AircraftList.FindAircraft(100).UniqueId);
        }

        [TestMethod]
        public void SimpleAircraftList_FindAircraft_Returns_Aircraft_Clone()
        {
            _Aircraft.Object.UniqueId = 100;
            _AircraftList.Aircraft.Add(_Aircraft.Object);

            var aircraft = _AircraftList.FindAircraft(100);

            _Aircraft.Verify(m => m.Clone(), Times.Once());
            Assert.AreNotSame(aircraft, _Aircraft);
        }
        #endregion

        #region TakeSnapshot
        [TestMethod]
        public void SimpleAircraftList_TakeSnapshot_Returns_Copy_Of_Aircraft_List()
        {
            long l1, l2;
            var list = _AircraftList.TakeSnapshot(out l1, out l2);

            Assert.IsNotNull(list);
            Assert.AreEqual(0, list.Count);
            Assert.AreNotSame(_AircraftList.Aircraft, list);
        }

        [TestMethod]
        public void SimpleAircraftList_TakeSnapshot_Returns_Clones_Of_Aircraft()
        {
            _AircraftList.Aircraft.Add(_Aircraft.Object);

            long l1, l2;
            var list = _AircraftList.TakeSnapshot(out l1, out l2);

            Assert.AreEqual(1, list.Count);
            Assert.AreNotSame(_Aircraft.Object, list[0]);
            _Aircraft.Verify(m => m.Clone(), Times.Once());
        }

        [TestMethod]
        public void SimpleAircraftList_TakeSnapshot_Fills_DataVersion_With_Latest_DataVersion_From_Aircraft()
        {
            var aircraft1 = _Aircraft;
            var aircraft2 = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            aircraft2.Setup(m => m.Clone()).Returns(aircraft2.Object);

            aircraft1.Object.DataVersion = 100;
            aircraft2.Object.DataVersion = 101;

            _AircraftList.Aircraft.Add(aircraft1.Object);
            _AircraftList.Aircraft.Add(aircraft2.Object);

            long l1, dataVersion;
            var list = _AircraftList.TakeSnapshot(out l1, out dataVersion);

            Assert.AreEqual(101, dataVersion);
        }

        [TestMethod]
        public void SimpleAircraftList_TakeSnapshot_Fills_Timestamp_With_Current_Time()
        {
            long timestamp, l2;
            _AircraftList.TakeSnapshot(out timestamp, out l2);

            Assert.AreEqual(_Clock.UtcNowValue.Ticks, timestamp);
        }
        #endregion
    }
}
