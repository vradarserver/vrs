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

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class ExpiringDictionaryTests
    {
        #region Fields, TestInitialise
        private IClassFactory _OriginalFactory;
        private ExpiringDictionary<int, string> _Map;
        private Mock<IHeartbeatService> _HeartbeatService;
        private ClockMock _Clock;
        private int _CountChangedCallCount;
        private int _LastCountChangedCounter;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();
            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _HeartbeatService = TestUtilities.CreateMockSingleton<IHeartbeatService>();

            _Map = new ExpiringDictionary<int, string>(200, 100);
            _CountChangedCallCount = 0;
            _Map.CountChangedDelegate = CountChangedHandler;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
            _Map.Dispose();
        }
        #endregion

        #region Utility methods - CountChangedHandler, HeartbeatTick
        private void CountChangedHandler(int newCount)
        {
            ++_CountChangedCallCount;
            _LastCountChangedCounter = newCount;
        }

        private void HeartbeatTick()
        {
            _HeartbeatService.Raise(r => r.SlowTick += null, EventArgs.Empty);
            _HeartbeatService.Raise(r => r.FastTick += null, EventArgs.Empty);
        }
        #endregion

        #region Ctors and Properties
        [TestMethod]
        public void ExpiringDictionary_Constructor_Initialises_To_Known_Values()
        {
            using(var map = new ExpiringDictionary<int, string>(1, 2)) {
                TestUtilities.TestProperty(map, r => r.ExpireMilliseconds, 1, 11);
                TestUtilities.TestProperty(map, r => r.MillisecondsBetweenChecks, 2, 12);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_Count_Returns_Count_Of_Items()
        {
            _Map.Add(1, "1");
            Assert.AreEqual(1, _Map.Count);

            _Map.Add(2, "1");
            Assert.AreEqual(2, _Map.Count);
        }

        [TestMethod]
        public void ExpiringDictionary_Will_Not_Expire_Items_Before_ExpireMilliseconds_Has_Passed()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "Hello");
                HeartbeatTick();

                _Clock.AddMilliseconds(9);
                HeartbeatTick();
                Assert.AreEqual(1, map.Count);

                _Clock.AddMilliseconds(1);
                HeartbeatTick();
                Assert.AreEqual(0, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_Will_Expire_Items_After_ExpireMilliseconds_Has_Passed()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "Hello");
                HeartbeatTick();

                _Clock.AddMilliseconds(11);
                HeartbeatTick();

                Assert.AreEqual(0, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_Will_Not_Check_Items_For_Expiry_Until_MillisecondsBetweenChecks_Has_Passed()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "Hello");
                HeartbeatTick();

                _Clock.AddMilliseconds(9);
                HeartbeatTick();
                Assert.AreEqual(1, map.Count);

                _Clock.AddMilliseconds(1);
                HeartbeatTick();
                Assert.AreEqual(0, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_Will_Check_Items_For_Expiry_After_MillisecondsBetweenChecks_Has_Passed()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "Hello");
                HeartbeatTick();

                _Clock.AddMilliseconds(11);
                HeartbeatTick();
                Assert.AreEqual(0, map.Count);
            }
        }
        #endregion

        #region Add
        [TestMethod]
        public void ExpiringDictionary_Add_Adds_Item_To_Map()
        {
            _Map.Add(1, "Hello");
            Assert.AreEqual("Hello", _Map.GetForKey(1));
            Assert.AreEqual(1, _Map.Count);
            Assert.AreEqual(1, _CountChangedCallCount);
            Assert.AreEqual(1, _LastCountChangedCounter);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ExpiringDictionary_Add_Throws_If_Adding_Duplicate_Key()
        {
            _Map.Add(1, "Hello");
            _Map.Add(1, "Same key");
        }
        #endregion

        #region Clear
        [TestMethod]
        public void ExpiringDictionary_Clear_Removes_All_Entries_From_Map()
        {
            _Map.Add(1, "!");
            _Map.Add(2, "<");

            _Map.Clear();

            Assert.AreEqual(0, _Map.Count);
        }
        #endregion

        #region Find
        [TestMethod]
        public void ExpiringDictionary_Find_Returns_Null_On_No_Match()
        {
            _Map.Add(1, "Hello");
            Assert.AreNotEqual("Hello", _Map.Find(r => r == "Goodbye"));
        }

        [TestMethod]
        public void ExpiringDictionary_Find_Returns_First_Matching_Item()
        {
            _Map.Add(1, "Hello");
            Assert.AreEqual("Hello", _Map.Find(r => r == "Hello"));
        }

        [TestMethod]
        public void ExpiringDictionary_Find_Does_Not_Refresh_Items()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 10)) {
                map.Add(100, "1");

                _Clock.AddMilliseconds(10);
                Assert.AreEqual("1", map.Find(r => r == "1"));

                HeartbeatTick();
                Assert.AreEqual(0, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_FindAndRefresh_Returns_First_Matching_Item_After_Refreshing_It()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 10)) {
                map.Add(8, "1");

                _Clock.AddMilliseconds(10);
                Assert.AreEqual("1", map.FindAndRefresh(r => r == "1"));

                HeartbeatTick();
                Assert.AreEqual(1, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_FindAll_Returns_All_Matching_Items()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 10)) {
                map.Add(1, "1a");
                map.Add(7, "1b");
                map.Add(3, "2a");

                var matches = map.FindAll(r => r.StartsWith("1"));
                Assert.AreEqual(2, matches.Count);
                Assert.IsTrue(matches.Contains("1a"));
                Assert.IsTrue(matches.Contains("1b"));
            }
        }

        [TestMethod]
        public void ExpiringDictionary_FindAll_Does_Not_Refresh_Matching_Items()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 10)) {
                map.Add(8, "1");

                _Clock.AddMilliseconds(10);
                Assert.AreEqual(1, map.FindAll(r => r == "1").Count);

                HeartbeatTick();
                Assert.AreEqual(0, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_FindAllAndRefresh_Refreshes_Matching_Items()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 10)) {
                map.Add(8, "1a");
                map.Add(7, "1b");
                map.Add(6, "2a");

                _Clock.AddMilliseconds(10);
                var matches = map.FindAllAndRefresh(r => r.StartsWith("1"));
                Assert.AreEqual(2, matches.Count);
                Assert.IsTrue(matches.Contains("1a"));
                Assert.IsTrue(matches.Contains("1b"));

                HeartbeatTick();
                Assert.AreEqual(2, map.Count);
            }
        }
        #endregion

        #region GetForKey
        [TestMethod]
        public void ExpiringDictionary_GetForKey_Returns_Null_On_No_Match()
        {
            Assert.IsNull(_Map.GetForKey(1));
        }

        [TestMethod]
        public void ExpiringDictionary_GetForKey_Returns_Value_On_Match()
        {
            _Map.Add(9, "Hello");
            Assert.AreEqual("Hello", _Map.GetForKey(9));
        }

        [TestMethod]
        public void ExpiringDictionary_GetForKey_Does_Not_Refresh()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "X");
                HeartbeatTick();

                _Clock.AddMilliseconds(10);
                map.GetForKey(1);

                HeartbeatTick();
                Assert.AreEqual(0, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_GetForKeyAndRefresh_Refreshes()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "X");
                HeartbeatTick();

                _Clock.AddMilliseconds(10);
                Assert.AreEqual("X", map.GetForKeyAndRefresh(1));

                HeartbeatTick();
                Assert.AreEqual(1, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_GetOrCreate_Creates_If_Key_Missing()
        {
            var result = _Map.GetOrCreate(7, (key) => {
                Assert.AreEqual(7, key);
                return "Y";
            });
            Assert.AreEqual("Y", result);
            Assert.AreEqual("Y", _Map.GetForKey(7));
        }

        [TestMethod]
        public void ExpiringDictionary_GetOrCreate_Does_Not_Call_Create_If_Key_Exists()
        {
            _Map.Add(-1, "Z");
            var result = _Map.GetOrCreate(-1, (unused) => {
                throw new InvalidOperationException("Create must only be called if a new item is required");
            });

            Assert.AreEqual("Z", result);
        }

        [TestMethod]
        public void ExpiringDictionary_GetAndRefreshOrCreate_Refreshes_If_Key_Exists()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(2, "kj");
                HeartbeatTick();

                _Clock.AddMilliseconds(10);
                Assert.AreEqual("kj", map.GetAndRefreshOrCreate(2, null));

                HeartbeatTick();
                Assert.AreEqual(1, map.Count);
            }
        }
        #endregion

        #region RefreshAll
        [TestMethod]
        public void ExpiringDictionary_RefreshAll_Refreshes_All_Items()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 10)) {
                map.Add(22, "1");
                map.Add(23, "2");

                _Clock.AddMilliseconds(10);
                map.RefreshAll();

                HeartbeatTick();
                Assert.AreEqual(2, map.Count);
            }
        }
        #endregion

        #region RemoveIfExists
        [TestMethod]
        public void ExpiringDictionary_RemoveIfExists_Removes_Items_By_Key()
        {
            _Map.Add(8, "K");
            _Map.RemoveIfExists(8);
            Assert.AreEqual(0, _Map.Count);
        }

        [TestMethod]
        public void ExpiringDictionary_RemoveIfExists_Does_Not_Throw_If_Key_Does_Not_Exist()
        {
            _Map.RemoveIfExists(9);
        }
        #endregion

        #region Snapshot
        [TestMethod]
        public void ExpiringDictionary_Snapshot_Returns_Collection_Of_KeyValuePairs()
        {
            _Map.Add(1, "A");
            _Map.Add(2, "B");

            var snapshot = _Map.Snapshot();

            _Map.Clear();
            Assert.AreEqual(2, snapshot.Length);
            Assert.IsTrue(snapshot.Contains(new KeyValuePair<int, string>(1, "A")));
            Assert.IsTrue(snapshot.Contains(new KeyValuePair<int, string>(2, "B")));
        }

        [TestMethod]
        public void ExpiringDictionary_Snapshot_Does_Not_Refresh_Items()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "1");

                _Clock.AddMilliseconds(10);
                map.Snapshot();
                HeartbeatTick();

                Assert.AreEqual(0, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_SnapshotAndRefresh_Refreshes_Items()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "A");
                map.Add(2, "B");

                _Clock.AddMilliseconds(10);
                var snapshot = map.SnapshotAndRefresh();
                HeartbeatTick();

                Assert.AreEqual(2, map.Count);

                Assert.AreEqual(2, snapshot.Length);
                Assert.IsTrue(snapshot.Contains(new KeyValuePair<int, string>(1, "A")));
                Assert.IsTrue(snapshot.Contains(new KeyValuePair<int, string>(2, "B")));
            }
        }
        #endregion

        #region Upsert
        [TestMethod]
        public void ExpiringDictionary_Upsert_Adds_Item_If_Missing()
        {
            _Map.Upsert(1, "A");
            Assert.AreEqual(1, _Map.Count);
            Assert.AreEqual("A", _Map.GetForKey(1));
        }

        [TestMethod]
        public void ExpiringDictionary_Upsert_Overwrites_Item_If_Present()
        {
            _Map.Add(1, "A");
            _Map.Upsert(1, "B");
            Assert.AreEqual(1, _Map.Count);
            Assert.AreEqual("B", _Map.GetForKey(1));
        }

        [TestMethod]
        public void ExpiringDictionary_Upsert_Does_Not_Refresh()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "A");
                HeartbeatTick();

                _Clock.AddMilliseconds(10);
                map.Upsert(1, "B");
                HeartbeatTick();

                Assert.AreEqual(0, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_UpsertAndRefresh_Does_Refresh()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "A");
                HeartbeatTick();

                _Clock.AddMilliseconds(10);
                map.UpsertAndRefresh(1, "B");
                HeartbeatTick();

                Assert.AreEqual(1, map.Count);
                Assert.AreEqual("B", map.GetForKey(1));
            }
        }

        [TestMethod]
        public void ExpiringDictionary_UpsertRange_Adds_Range()
        {
            _Map.Add(1, "A");
            _Map.UpsertRange(new string[] { "1", "2" }, (val) => int.Parse(val));

            Assert.AreEqual(2, _Map.Count);
            Assert.AreEqual("1", _Map.GetForKey(1));
            Assert.AreEqual("2", _Map.GetForKey(2));
        }

        [TestMethod]
        public void ExpiringDictionary_UpsertRange_Does_Not_Refresh()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "B");
                HeartbeatTick();

                _Clock.AddMilliseconds(10);
                map.UpsertRange(new string[] { "N" }, (unused) => 1);
                HeartbeatTick();

                Assert.AreEqual(0, map.Count);
            }
        }

        [TestMethod]
        public void ExpiringDictionary_UpsertRangeAndRefresh_Does_Refresh()
        {
            using(var map = new ExpiringDictionary<int, string>(10, 1)) {
                map.Add(1, "B");
                HeartbeatTick();

                _Clock.AddMilliseconds(10);
                map.UpsertRangeAndRefresh(new string[] { "1", "2" }, (val) => int.Parse(val));
                HeartbeatTick();

                Assert.AreEqual(2, map.Count);
                Assert.AreEqual("1", map.GetForKey(1));
                Assert.AreEqual("2", map.GetForKey(2));
            }
        }
        #endregion
    }
}
