// Copyright © 2017 onwards, Andrew Whewell
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
using System.Net;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Test.Framework;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite.ApiControllers
{
    [TestClass]
    public class FeedControllerTests : ControllerTests
    {
        private Mock<IFeedManager> _FeedManager;
        private List<Mock<IFeed>> _VisibleFeeds;

        protected override void ExtraInitialise()
        {
            _FeedManager = TestUtilities.CreateMockSingleton<IFeedManager>();
            _VisibleFeeds = new List<Mock<IFeed>>();
            _FeedManager.SetupGet(r => r.VisibleFeeds).Returns(() => {
                return _VisibleFeeds.Select(r => r.Object).ToArray();
            });
        }

        private Mock<IFeed> CreateFeed(int uniqueId = 1, string name = "My Feed", bool hasPlotter = true, bool isVisible = true)
        {
            var result = TestUtilities.CreateMockInstance<IFeed>();
            result.SetupGet(r => r.UniqueId).Returns(uniqueId);
            result.SetupGet(r => r.Name).Returns(name);
            result.SetupGet(r => r.IsVisible).Returns(isVisible);

            var aircraftList = TestUtilities.CreateMockInstance<IBaseStationAircraftList>();
            aircraftList.SetupGet(r => r.PolarPlotter).Returns(hasPlotter ? TestUtilities.CreateMockInstance<IPolarPlotter>().Object : null);
            result.SetupGet(r => r.AircraftList).Returns(aircraftList.Object);

            return result;
        }

        private Mock<IFeed> ConfigureGetFeedById(Mock<IFeed> feed)
        {
            _FeedManager.Setup(r => r.GetByUniqueId(feed.Object.UniqueId, true)).Returns(feed.Object);

            return feed;
        }

        [TestMethod]
        public async Task FeedController_GetFeeds_Returns_All_Visible_Feeds()
        {
            _VisibleFeeds.Add(CreateFeed(uniqueId: 1, name: "First", hasPlotter: true));
            _VisibleFeeds.Add(CreateFeed(uniqueId: 2, name: "Second", hasPlotter: false));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds");
            var content = await response.Content.ReadAsStringAsync();
            var feeds = JsonConvert.DeserializeObject<FeedJson[]>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(2, feeds.Length);

            var feed1 = feeds.Single(r => r.UniqueId == 1);
            Assert.AreEqual("First", feed1.Name);
            Assert.AreEqual(true, feed1.HasPolarPlot);

            var feed2 = feeds.Single(r => r.UniqueId == 2);
            Assert.AreEqual("Second", feed2.Name);
            Assert.AreEqual(false, feed2.HasPolarPlot);
        }

        [TestMethod]
        public async Task FeedController_GetFeeds_Returns_Empty_Array_If_All_Feeds_Are_Invisible()
        {
            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds");
            var content = await response.Content.ReadAsStringAsync();
            var feeds = JsonConvert.DeserializeObject<FeedJson[]>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, feeds.Length);
        }

        [TestMethod]
        public async Task FeedController_GetFeed_Returns_Feed_If_Known()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, name: "Feed", hasPlotter: true, isVisible: true));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feed/1");
            var content = await response.Content.ReadAsStringAsync();
            var feed = JsonConvert.DeserializeObject<FeedJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, feed.UniqueId);
            Assert.AreEqual("Feed", feed.Name);
            Assert.AreEqual(true, feed.HasPolarPlot);
        }

        [TestMethod]
        public async Task FeedController_GetFeed_Returns_Null_If_ID_Is_Unknown()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feed/2");
            var content = await response.Content.ReadAsStringAsync();
            var feed = JsonConvert.DeserializeObject<FeedJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(feed);
        }

        [TestMethod]
        public async Task FeedController_GetFeed_Returns_Null_If_Feed_Is_Invisible()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, isVisible: false));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feed/1");
            var content = await response.Content.ReadAsStringAsync();
            var feed = JsonConvert.DeserializeObject<FeedJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(feed);
        }
    }
}
