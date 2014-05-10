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
using Moq;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.BaseStation;

namespace Test.Framework
{
    /// <summary>
    /// A set of helper methods to set up a feed manager with a bunch of feeds that each have a listener.
    /// </summary>
    public static class FeedHelper
    {
        #region CreateMockFeedManager
        /// <summary>
        /// Creates a mock <see cref="IFeedManager"/> singleton with a functioning Feeds property and methods, and populates the <see cref="IFeed"/>
        /// and <see cref="IListener"/> lists with mocks.
        /// </summary>
        /// <param name="feeds"></param>
        /// <param name="listeners"></param>
        /// <param name="feedIds"></param>
        /// <returns></returns>
        public static Mock<IFeedManager> CreateMockFeedManager(List<Mock<IFeed>> feeds, List<Mock<IListener>> listeners, params int[] feedIds)
        {
            var result = CreateMockFeedManager(feeds);
            AddFeeds(feeds, listeners, feedIds);

            return result;
        }

        /// <summary>
        /// Creates a mock <see cref="IFeedManager"/> singleton with a functioning Receivers property and methods, and populates the receiver pathway.
        /// and <see cref="IBaseStationAircraftList"/> lists with mocks.
        /// </summary>
        /// <param name="feeds"></param>
        /// <param name="baseStationAircraftLists"></param>
        /// <param name="aircraftLists"></param>
        /// <param name="feedIds"></param>
        /// <returns></returns>
        public static Mock<IFeedManager> CreateMockFeedManager(List<Mock<IFeed>> feeds, List<Mock<IBaseStationAircraftList>> baseStationAircraftLists, List<List<IAircraft>> aircraftLists, params int[] feedIds)
        {
            var result = CreateMockFeedManager(feeds);
            AddFeeds(feeds, baseStationAircraftLists, aircraftLists, feedIds);

            return result;
        }

        private static Mock<IFeedManager> CreateMockFeedManager(List<Mock<IFeed>> feeds)
        {
            var result = TestUtilities.CreateMockSingleton<IFeedManager>();
            result.Setup(r => r.Feeds).Returns(() => {
                return feeds.Select(r => r.Object).ToArray();
            });
            result.Setup(r => r.GetByUniqueId(It.IsAny<int>())).Returns((int id) => {
                return feeds.Select(r => r.Object).FirstOrDefault(r => r.UniqueId == id);
            });
            result.Setup(r => r.GetByName(It.IsAny<string>())).Returns((string name) => {
                return feeds.Select(r => r.Object).FirstOrDefault(r => String.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
            });
            return result;
        }
        #endregion

        #region AddFeeds
        /// <summary>
        /// Populates the <see cref="IFeed"/> and <see cref="IListener"/> lists with mocks for a mock <see cref="IFeedManager"/>.
        /// </summary>
        /// <param name="feeds"></param>
        /// <param name="listeners"></param>
        /// <param name="feedIds"></param>
        public static void AddFeeds(List<Mock<IFeed>> feeds, List<Mock<IListener>> listeners, params int[] feedIds)
        {
            DoAddFeeds(feeds, feedIds, feed => {
                var listener = TestUtilities.CreateMockInstance<IListener>();
                listener.Object.ReceiverId = feed.Object.UniqueId;
                listeners.Add(listener);
                feed.Setup(r => r.Listener).Returns(listener.Object);
            });
        }

        /// <summary>
        /// Populates the <see cref="IFeed"/> and <see cref="IListener"/> lists with mocks for a mock <see cref="IFeedManager"/>.
        /// </summary>
        /// <param name="feeds"></param>
        /// <param name="listeners"></param>
        /// <param name="feedIds"></param>
        public static void AddMergedFeeds(List<Mock<IFeed>> feeds, List<Mock<IMergedFeedListener>> listeners, params int[] feedIds)
        {
            DoAddFeeds(feeds, feedIds, feed => {
                var listener = TestUtilities.CreateMockInstance<IMergedFeedListener>();
                listener.Object.ReceiverId = feed.Object.UniqueId;
                listeners.Add(listener);
                feed.Setup(r => r.Listener).Returns(listener.Object);
            });
        }

        /// <summary>
        /// Populates the <see cref="IFeed"/> and <see cref="IBaseStationAircraftList"/> lists with mocks for a mock <see cref="IFeedManager"/>.
        /// </summary>
        /// <param name="feeds"></param>
        /// <param name="baseStationAircraftLists"></param>
        /// <param name="aircraftLists"></param>
        /// <param name="feedIds"></param>
        public static void AddFeeds(List<Mock<IFeed>> feeds, List<Mock<IBaseStationAircraftList>> baseStationAircraftLists, List<List<IAircraft>> aircraftLists, params int[] feedIds)
        {
            DoAddFeeds(feeds, feedIds, feed => {
                var index = baseStationAircraftLists.Count;

                var baseStationAircraftList = TestUtilities.CreateMockInstance<IBaseStationAircraftList>();
                baseStationAircraftLists.Add(baseStationAircraftList);

                var aircraftList = new List<IAircraft>();
                aircraftLists.Add(aircraftList);

                SetupTakeSnapshot(baseStationAircraftLists, aircraftLists, index, 0, 0);

                feed.Setup(r => r.AircraftList).Returns(baseStationAircraftList.Object);
            });
        }

        private static void DoAddFeeds(List<Mock<IFeed>> feeds, int[] feedIds, Action<Mock<IFeed>> setupFeed)
        {
            foreach(var feedId in feedIds) {
                if(!feeds.Any(r => r.Object.UniqueId == feedId)) {
                    var feed = TestUtilities.CreateMockInstance<IFeed>();
                    feed.Setup(r => r.UniqueId).Returns(feedId);
                    feeds.Add(feed);

                    setupFeed(feed);
                }
            }
        }
        #endregion

        #region RemoveFeed
        /// <summary>
        /// Removes <see cref="IFeed"/> and <see cref="IListener"/> mocks for a mock <see cref="IFeedManager"/>.
        /// </summary>
        /// <param name="feeds"></param>
        /// <param name="listeners"></param>
        /// <param name="feedIds"></param>
        public static void RemoveFeed(List<Mock<IFeed>> feeds, List<Mock<IListener>> listeners, params int[] feedIds)
        {
            DoRemoveFeed(feeds, feedIds, receiver => {
                var listener = listeners.Single(r => Object.ReferenceEquals(receiver.Object.Listener, r.Object));
                listeners.Remove(listener);
            });
        }

        /// <summary>
        /// Removes <see cref="IFeed"/> and <see cref="IBaseStationAircraftList"/> mocks for a mock <see cref="IFeedManager"/>.
        /// </summary>
        /// <param name="feeds"></param>
        /// <param name="baseStationAircraftLists"></param>
        /// <param name="aircraftLists"></param>
        /// <param name="feedIds"></param>
        public static void RemoveFeed(List<Mock<IFeed>> feeds, List<Mock<IBaseStationAircraftList>> baseStationAircraftLists, List<List<IAircraft>> aircraftLists, params int[] feedIds)
        {
            DoRemoveFeed(feeds, feedIds, receiver => {
                var baseStationAircraftList = baseStationAircraftLists.Single(r => Object.ReferenceEquals(receiver.Object.AircraftList, r.Object));
                var index = baseStationAircraftLists.IndexOf(baseStationAircraftList);

                baseStationAircraftLists.RemoveAt(index);
                aircraftLists.RemoveAt(index);
            });
        }

        private static void DoRemoveFeed(List<Mock<IFeed>> feeds, int[] feedIds, Action<Mock<IFeed>> removeForFeed)
        {
            foreach(var feedId in feedIds) {
                var feed = feeds.FirstOrDefault(r => r.Object.UniqueId == feedId);
                if(feed != null) {
                    feeds.Remove(feed);
                    removeForFeed(feed);
                }
            }
        }
        #endregion

        #region SetupTakeSnapshot
        /// <summary>
        /// A helper method to set up the <see cref="IBaseStationAircraftList"/> TakeSnaphot method for known values of out longs.
        /// </summary>
        /// <param name="baseStationAircraftLists"></param>
        /// <param name="aircraftLists"></param>
        /// <param name="index"></param>
        /// <param name="takeSnapshotOutValue1"></param>
        /// <param name="takeSnapshotOutValue2"></param>
        public static void SetupTakeSnapshot(List<Mock<IBaseStationAircraftList>> baseStationAircraftLists, List<List<IAircraft>> aircraftLists, int index, long takeSnapshotOutValue1, long takeSnapshotOutValue2)
        {
            var baseStationAircraftList = baseStationAircraftLists[index];
            var aircraftList = aircraftLists[index];

            long o1 = takeSnapshotOutValue1;
            long o2 = takeSnapshotOutValue2;

            baseStationAircraftList.Setup(m => m.TakeSnapshot(out o1, out o2)).Returns(aircraftList);
        }
        #endregion

        #region GetFeeds
        /// <summary>
        /// Returns a list of <see cref="IFeed"/> objects from a list of mock <see cref="IFeed"/> objects.
        /// </summary>
        /// <param name="feeds"></param>
        /// <returns></returns>
        public static List<IFeed> GetFeeds(List<Mock<IFeed>> feeds)
        {
            return feeds.Select(r => r.Object).ToList();
        }
        #endregion
    }
}
