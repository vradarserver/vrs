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
    public class FeedsControllerTests : ControllerTests
    {
        private Mock<IFeedManager> _FeedManager;
        private List<Mock<IFeed>> _VisibleFeeds;
        private Mock<IBaseStationAircraftList> _AircraftList;
        private Mock<IPolarPlotter> _PolarPlotter;
        private List<PolarPlotSlice> _Slices;

        protected override void ExtraInitialise()
        {
            _Configuration.InternetClientSettings.CanShowPolarPlots = true;

            _Slices = new List<PolarPlotSlice>();

            _PolarPlotter = TestUtilities.CreateMockInstance<IPolarPlotter>();
            _PolarPlotter.Setup(r => r.TakeSnapshot()).Returns(_Slices);

            _AircraftList = TestUtilities.CreateMockInstance<IBaseStationAircraftList>();
            _AircraftList.SetupGet(r => r.PolarPlotter).Returns(_PolarPlotter.Object);

            _FeedManager = TestUtilities.CreateMockSingleton<IFeedManager>();
            _VisibleFeeds = new List<Mock<IFeed>>();
            _FeedManager.Setup(r => r.GetByUniqueId(It.IsAny<int>(), It.IsAny<bool>())).Returns((IFeed)null);
            _FeedManager.SetupGet(r => r.VisibleFeeds).Returns(() => {
                return _VisibleFeeds.Select(r => r.Object).ToArray();
            });
        }

        private Mock<IFeed> CreateFeed(int uniqueId = 1, string name = "My Feed", bool hasPlotter = true, bool isVisible = true, bool hasDistinctAircraftList = false)
        {
            var result = TestUtilities.CreateMockInstance<IFeed>();
            result.SetupGet(r => r.UniqueId).Returns(uniqueId);
            result.SetupGet(r => r.Name).Returns(name);
            result.SetupGet(r => r.IsVisible).Returns(isVisible);

            if(hasDistinctAircraftList) {
                var aircraftList = TestUtilities.CreateMockInstance<IBaseStationAircraftList>();
                aircraftList.SetupGet(r => r.PolarPlotter).Returns(hasPlotter ? TestUtilities.CreateMockInstance<IPolarPlotter>().Object : null);
                result.SetupGet(r => r.AircraftList).Returns(aircraftList.Object);
            } else {
                if(!hasPlotter) {
                    _AircraftList.SetupGet(r => r.PolarPlotter).Returns((IPolarPlotter)null);
                }
                result.SetupGet(r => r.AircraftList).Returns(_AircraftList.Object);
            }

            return result;
        }

        private PolarPlotSlice CreatePolarPlotSlice(int lowAltitude, int highAltitude, params PolarPlot[] points)
        {
            var result = new PolarPlotSlice() {
                AltitudeLower =     lowAltitude,
                AltitudeHigher =    highAltitude,
            };
            foreach(var point in points) {
                result.PolarPlots.Add(point.Angle, point);
            }

            return result;
        }

        private Mock<IFeed> ConfigureGetFeedById(Mock<IFeed> feed)
        {
            _FeedManager.Setup(r => r.GetByUniqueId(feed.Object.UniqueId, true)).Returns(feed.Object);

            return feed;
        }

        #region GetFeeds
        [TestMethod]
        public async Task FeedsController_GetFeeds_Returns_All_Visible_Feeds()
        {
            _VisibleFeeds.Add(CreateFeed(uniqueId: 1, name: "First", hasPlotter: true, hasDistinctAircraftList: true));
            _VisibleFeeds.Add(CreateFeed(uniqueId: 2, name: "Second", hasPlotter: false, hasDistinctAircraftList: true));

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
        public async Task FeedsController_GetFeeds_Returns_Empty_Array_If_All_Feeds_Are_Invisible()
        {
            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds");
            var content = await response.Content.ReadAsStringAsync();
            var feeds = JsonConvert.DeserializeObject<FeedJson[]>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(0, feeds.Length);
        }
        #endregion

        #region GetFeed
        [TestMethod]
        public async Task FeedsController_GetFeed_Returns_Feed_If_Known()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, name: "Feed", hasPlotter: true, isVisible: true));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/1");
            var content = await response.Content.ReadAsStringAsync();
            var feed = JsonConvert.DeserializeObject<FeedJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, feed.UniqueId);
            Assert.AreEqual("Feed", feed.Name);
            Assert.AreEqual(true, feed.HasPolarPlot);
        }

        [TestMethod]
        public async Task FeedsController_GetFeed_Returns_Null_If_ID_Is_Unknown()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/2");
            var content = await response.Content.ReadAsStringAsync();
            var feed = JsonConvert.DeserializeObject<FeedJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(feed);
        }

        [TestMethod]
        public async Task FeedsController_GetFeed_Returns_Null_If_Feed_Is_Invisible()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, isVisible: false));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/1");
            var content = await response.Content.ReadAsStringAsync();
            var feed = JsonConvert.DeserializeObject<FeedJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(feed);
        }
        #endregion

        #region GetPolarPlot
        [TestMethod]
        public async Task FeedsController_GetPolarPlot_Returns_Object()
        {
            ConfigureGetFeedById(CreateFeed());

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/1/polar-plot");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PolarPlotsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual(1, json.FeedId);
            Assert.IsNotNull(json.Slices);
        }

        [TestMethod]
        public async Task FeedsController_GetPolarPlot_Returns_Object_For_Version_2_Route()
        {
            ConfigureGetFeedById(CreateFeed());

            var response = await _Server.HttpClient.GetAsync("/PolarPlot.json?feedId=1");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PolarPlotsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual(1, json.FeedId);
            Assert.IsNotNull(json.Slices);
        }

        [TestMethod]
        public async Task FeedsController_GetPolarPlot_Returns_Slices()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 2));
            _Slices.Add(CreatePolarPlotSlice(100, 199, new PolarPlot() { Angle = 1, Altitude = 150, Distance = 7, Latitude = 10.1, Longitude = 11.2 }));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/2/polar-plot");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PolarPlotsJson>(content);

            Assert.AreEqual(2, json.FeedId);
            Assert.AreEqual(1, json.Slices.Count);

            var slice = json.Slices[0];
            Assert.AreEqual(100, slice.StartAltitude);
            Assert.AreEqual(199, slice.FinishAltitude);
            Assert.AreEqual(1, slice.Plots.Count);
            Assert.AreEqual(10.1F, slice.Plots[0].Latitude);
            Assert.AreEqual(11.2F, slice.Plots[0].Longitude);
        }

        [TestMethod]
        public async Task FeedsController_GetPolarPlot_Returns_No_Slices_If_Invalid_ID_Supplied()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 2));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/1/polar-plot");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PolarPlotsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public async Task FeedsController_GetPolarPlot_Returns_No_Slices_If_Feed_Is_Invisible()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, isVisible: false));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/1/polar-plot");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PolarPlotsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public async Task FeedsController_GetPolarPlot_Returns_No_Slices_If_Feed_Has_No_Aircraft_List()
        {
            var feed = CreateFeed(uniqueId: 1);
            feed.SetupGet(r => r.AircraftList).Returns((IBaseStationAircraftList)null);
            ConfigureGetFeedById(feed);

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/1/polar-plot");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PolarPlotsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public async Task FeedsController_GetPolarPlot_Returns_No_Slices_If_Feed_Has_No_Plotter()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, hasPlotter: false));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/1/polar-plot");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PolarPlotsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public async Task FeedsController_GetPolarPlot_Returns_No_Slices_If_Prohibited_By_Configuration()
        {
            _Configuration.InternetClientSettings.CanShowPolarPlots = false;
            _RemoteIpAddress = "1.2.3.4";

            ConfigureGetFeedById(CreateFeed(uniqueId: 1));
            _Slices.Add(CreatePolarPlotSlice(100, 199, new PolarPlot() { Angle = 1, Altitude = 150, Distance = 7, Latitude = 10.1, Longitude = 11.2 }));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/1/polar-plot");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PolarPlotsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public async Task FeedsController_GetPolarPlot_Returns_Slices_If_Prohibited_By_Configuration_But_Accessed_Locally()
        {
            _Configuration.InternetClientSettings.CanShowPolarPlots = false;
            _RemoteIpAddress = "192.168.0.1";

            ConfigureGetFeedById(CreateFeed(uniqueId: 1));
            _Slices.Add(CreatePolarPlotSlice(100, 199, new PolarPlot() { Angle = 1, Altitude = 150, Distance = 7, Latitude = 10.1, Longitude = 11.2 }));

            var response = await _Server.HttpClient.GetAsync("/api/1.00/feeds/1/polar-plot");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<PolarPlotsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(1, json.Slices.Count);
        }
        #endregion
    }
}
