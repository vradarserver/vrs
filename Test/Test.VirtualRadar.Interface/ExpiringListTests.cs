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
    public class ExpiringListTests
    {
        #region Fields, TestInitialise
        private IClassFactory _OriginalFactory;
        private ExpiringList<string> _List;
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

            _List = new ExpiringList<string>(200, 100);
            _CountChangedCallCount = 0;
            _List.CountChangedDelegate = CountChangedHandler;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
            _List.Dispose();
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
        public void ExpiringList_Constructor_Initialises_To_Known_Values()
        {
            using(var list = new ExpiringList<string>(1, 2)) {
                TestUtilities.TestProperty(list, r => r.ExpireMilliseconds, 1, 11);
                TestUtilities.TestProperty(list, r => r.MillisecondsBetweenChecks, 2, 12);
            }
        }

        [TestMethod]
        public void ExpiringList_Count_Returns_Count_Of_Items()
        {
            _List.Add("1");
            Assert.AreEqual(1, _List.Count);

            _List.Add("1");
            Assert.AreEqual(2, _List.Count);
        }

        [TestMethod]
        public void ExpiringList_Will_Not_Expire_Items_Before_ExpireMilliseconds_Has_Passed()
        {
            using(var list = new ExpiringList<string>(10, 1)) {
                list.Add("Hello");
                HeartbeatTick();

                _Clock.AddMilliseconds(9);
                HeartbeatTick();
                Assert.AreEqual(1, list.Count);

                _Clock.AddMilliseconds(1);
                HeartbeatTick();
                Assert.AreEqual(0, list.Count);
            }
        }

        [TestMethod]
        public void ExpiringList_Will_Expire_Items_After_ExpireMilliseconds_Has_Passed()
        {
            using(var list = new ExpiringList<string>(10, 1)) {
                list.Add("Hello");
                HeartbeatTick();

                _Clock.AddMilliseconds(11);
                HeartbeatTick();

                Assert.AreEqual(0, list.Count);
            }
        }

        [TestMethod]
        public void ExpiringList_Will_Not_Check_Items_For_Expiry_Until_MillisecondsBetweenChecks_Has_Passed()
        {
            using(var list = new ExpiringList<string>(1, 10)) {
                list.Add("Hello");
                HeartbeatTick();

                _Clock.AddMilliseconds(9);
                HeartbeatTick();
                Assert.AreEqual(1, list.Count);

                _Clock.AddMilliseconds(1);
                HeartbeatTick();
                Assert.AreEqual(0, list.Count);
            }
        }

        [TestMethod]
        public void ExpiringList_Will_Check_Items_For_Expiry_After_MillisecondsBetweenChecks_Has_Passed()
        {
            using(var list = new ExpiringList<string>(1, 10)) {
                list.Add("Hello");
                HeartbeatTick();

                _Clock.AddMilliseconds(11);
                HeartbeatTick();
                Assert.AreEqual(0, list.Count);
            }
        }
        #endregion

        #region Add
        [TestMethod]
        public void ExpiringList_Add_Performs_Without_Surprises()
        {
            _List.Add("Hello");
            Assert.AreEqual(1, _List.Count);
            Assert.AreEqual(1, _CountChangedCallCount);
            Assert.AreEqual(1, _LastCountChangedCounter);
        }

        [TestMethod]
        public void ExpiringList_Add_Can_Add_Same_Item_Twice()
        {
            _List.Add("Hello");
            _List.Add("Hello");
            Assert.AreEqual(2, _List.Count);
            Assert.AreEqual(2, _CountChangedCallCount);
            Assert.AreEqual(2, _LastCountChangedCounter);
        }

        [TestMethod]
        public void ExpiringList_AddOrRefresh_Updates_If_Adding_Same_Item_Twice()
        {
            _List.AddOrRefresh("Hello");
            _List.AddOrRefresh("Hello");
            Assert.AreEqual(1, _List.Count);
            Assert.AreEqual(1, _CountChangedCallCount);
            Assert.AreEqual(1, _LastCountChangedCounter);
        }

        [TestMethod]
        public void ExpiringList_AddOrRefresh_Refreshes_Existing_Items()
        {
            _List.AddOrRefresh("Hello");

            _Clock.AddMilliseconds(_List.ExpireMilliseconds);
            _List.AddOrRefresh("Hello");

            HeartbeatTick();
            Assert.AreEqual(1, _List.Count);
        }

        [TestMethod]
        public void ExpiringList_AddRange_Adds_A_Range_Of_Items()
        {
            _List.AddRange(new string[] { "o1", "p2" });
            Assert.AreEqual("o1", _List.Find(r => r == "o1"));
            Assert.AreEqual("p2", _List.Find(r => r == "p2"));
            Assert.AreEqual(2, _List.Count);
            Assert.AreEqual(1, _CountChangedCallCount);
            Assert.AreEqual(2, _LastCountChangedCounter);
        }

        [TestMethod]
        public void ExpiringList_AddRangeOrRefresh_Adds_Or_Refreshes_A_Range_Of_Items()
        {
            using(var list = new ExpiringList<string>(10, 10)) {
                list.AddRange(new string[] { "1", "2" });
                HeartbeatTick();

                _Clock.AddMilliseconds(10);
                list.AddRange(new string[] { "2", "3" });
                HeartbeatTick();

                Assert.AreEqual(2, list.Count);
                Assert.AreEqual(null, list.Find(r => r == "1"));
                Assert.AreEqual("2", list.Find(r => r == "2"));
                Assert.AreEqual("3", list.Find(r => r == "3"));
            }
        }
        #endregion

        #region Clear
        [TestMethod]
        public void ExpiringList_Clear_Removes_All_Items()
        {
            _List.Add("1");

            _List.Clear();

            Assert.AreEqual(0, _List.Count);
        }
        #endregion

        #region Find
        [TestMethod]
        public void ExpiringList_Find_Returns_Null_On_No_Match()
        {
            _List.Add("Hello");
            Assert.AreNotEqual("Hello", _List.Find(r => r == "Goodbye"));
        }

        [TestMethod]
        public void ExpiringList_Find_Returns_First_Matching_Item()
        {
            _List.Add("Hello");
            Assert.AreEqual("Hello", _List.Find(r => r == "Hello"));
        }

        [TestMethod]
        public void ExpiringList_Find_Does_Not_Refresh_Items()
        {
            using(var list = new ExpiringList<string>(10, 10)) {
                list.Add("1");

                _Clock.AddMilliseconds(10);
                Assert.AreEqual("1", list.Find(r => r == "1"));

                HeartbeatTick();
                Assert.AreEqual(0, list.Count);
            }
        }

        [TestMethod]
        public void ExpiringList_FindAndRefresh_Returns_First_Matching_Item_After_Refreshing_It()
        {
            using(var list = new ExpiringList<string>(10, 10)) {
                list.Add("1");

                _Clock.AddMilliseconds(10);
                Assert.AreEqual("1", list.FindAndRefresh(r => r == "1"));

                HeartbeatTick();
                Assert.AreEqual(1, list.Count);
            }
        }

        [TestMethod]
        public void ExpiringList_FindAll_Returns_All_Matching_Items()
        {
            using(var list = new ExpiringList<string>(10, 10)) {
                list.Add("1a");
                list.Add("1b");
                list.Add("2a");

                var matches = list.FindAll(r => r.StartsWith("1"));
                Assert.AreEqual(2, matches.Count);
                Assert.IsTrue(matches.Contains("1a"));
                Assert.IsTrue(matches.Contains("1b"));
            }
        }

        [TestMethod]
        public void ExpiringList_FindAll_Does_Not_Refresh_Matching_Items()
        {
            using(var list = new ExpiringList<string>(10, 10)) {
                list.Add("1");

                _Clock.AddMilliseconds(10);
                Assert.AreEqual(1, list.FindAll(r => r == "1").Count);

                HeartbeatTick();
                Assert.AreEqual(0, list.Count);
            }
        }

        [TestMethod]
        public void ExpiringList_FindAllAndRefresh_Refreshes_Matching_Items()
        {
            using(var list = new ExpiringList<string>(10, 10)) {
                list.Add("1a");
                list.Add("1b");
                list.Add("2a");

                _Clock.AddMilliseconds(10);
                var matches = list.FindAllAndRefresh(r => r.StartsWith("1"));
                Assert.AreEqual(2, matches.Count);
                Assert.IsTrue(matches.Contains("1a"));
                Assert.IsTrue(matches.Contains("1b"));

                HeartbeatTick();
                Assert.AreEqual(2, list.Count);
            }
        }
        #endregion

        #region RefreshAll
        [TestMethod]
        public void ExpiringList_RefreshAll_Refreshes_All_Items()
        {
            using(var list = new ExpiringList<string>(10, 10)) {
                list.Add("1");
                list.Add("2");

                _Clock.AddMilliseconds(10);
                list.RefreshAll();

                HeartbeatTick();
                Assert.AreEqual(2, list.Count);
            }
        }
        #endregion

        #region Remove
        [TestMethod]
        public void ExpiringList_Remove_Removes_Item()
        {
            _List.Add("1");
            _List.Remove("1");

            Assert.AreEqual(0, _List.Count);
            Assert.AreEqual(2, _CountChangedCallCount);
            Assert.AreEqual(0, _LastCountChangedCounter);
        }

        [TestMethod]
        public void ExpiringList_Remove_Ignores_Unknown_Items()
        {
            _List.Add("1");
            _List.Remove("2");

            Assert.AreEqual(1, _List.Count);
            Assert.AreEqual(1, _CountChangedCallCount);
        }

        [TestMethod]
        public void ExpiringList_RemoveRange_Removes_A_Range_Of_Items()
        {
            _List.Add("1");
            _List.Add("2");
            _List.Add("3");

            _List.RemoveRange(new string[] { "1", "3" });
            Assert.AreEqual(1, _List.Count);
            Assert.AreEqual("2", _List.Find(r => r == "2"));

            Assert.AreEqual(4, _CountChangedCallCount);
            Assert.AreEqual(1, _LastCountChangedCounter);
        }

        [TestMethod]
        public void ExpiringList_RemoveRange_Ignores_Unknown_Items()
        {
            _List.Add("1");
            _List.RemoveRange(new string[] { "Hello" });

            Assert.AreEqual(1, _List.Count);
            Assert.AreEqual(1, _CountChangedCallCount);
        }
        #endregion

        #region Snapshot
        [TestMethod]
        public void ExpiringList_Snapshot_Returns_Collection_Of_Items()
        {
            _List.Add("1");
            _List.Add("2");

            var snapshot = _List.Snapshot();

            _List.Clear();
            Assert.AreEqual(2, snapshot.Length);
            Assert.IsTrue(snapshot.Contains("1"));
            Assert.IsTrue(snapshot.Contains("2"));
        }

        [TestMethod]
        public void ExpiringList_Snapshot_Does_Not_Refresh_Items()
        {
            using(var list = new ExpiringList<string>(10, 1)) {
                list.Add("1");

                _Clock.AddMilliseconds(10);
                list.Snapshot();
                HeartbeatTick();

                Assert.AreEqual(0, list.Count);
            }
        }

        [TestMethod]
        public void ExpiringList_SnapshotAndRefresh_Refreshes_Items()
        {
            using(var list = new ExpiringList<string>(10, 1)) {
                list.Add("1");
                list.Add("2");

                _Clock.AddMilliseconds(10);
                var snapshot = list.SnapshotAndRefresh();
                HeartbeatTick();

                Assert.AreEqual(2, list.Count);

                Assert.AreEqual(2, snapshot.Length);
                Assert.IsTrue(snapshot.Contains("1"));
                Assert.IsTrue(snapshot.Contains("2"));
            }
        }
        #endregion
    }
}
