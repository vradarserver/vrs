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
using System.Globalization;
using System.Linq;
using System.Text;
using Moq;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.BaseStation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Interface.Listener;

namespace Test.VirtualRadar.WebSite
{
    // This partial class contains all of the tests on the PolarPlot.json request.
    public partial class WebSiteTests
    {
        #region Pages
        private const string PolarPlotPage = "/PolarPlot.json";
        #endregion

        #region Private class - PolarPlotAddress
        class PolarPlotAddress
        {
            private Mock<IRequest> _Request;

            public string Page { get; set; }
            public int FeedId { get; set; }

            public PolarPlotAddress(Mock<IRequest> request)
            {
                _Request = request;
                Page = PolarPlotPage;
                FeedId = -1;
            }

            public string Address
            {
                get
                {
                    Dictionary<string, string> queryValues = new Dictionary<string,string>();

                    if(FeedId > -1) queryValues.Add("feedId", FeedId.ToString(CultureInfo.InvariantCulture));

                    return BuildUrl(Page, queryValues);
                }
            }
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Configures the mocks such that a BaseStationAircraftList exists with a plotter on it.
        /// </summary>
        /// <param name="feedId"></param>
        /// <returns></returns>
        private Mock<IBaseStationAircraftList> SetupPolarPlotterForFeed(int feedId)
        {
            Mock<IBaseStationAircraftList> result = null;

            var feed = _ReceiverPathways.FirstOrDefault(r => r.Object.UniqueId == feedId);
            if(feed != null) {
                result = _BaseStationAircraftLists.FirstOrDefault(r => r.Object == feed.Object.AircraftList);
                if(result != null) {
                    result.Setup(r => r.PolarPlotter).Returns(_PolarPlotter.Object);
                }
            }

            return result;
        }
        #endregion

        #region PolarPlot
        [TestMethod]
        public void WebSite_PolarPlotJson_Returns_Correct_Json_For_Request_For_Polar_Plots()
        {
            var request = new PolarPlotAddress(_Request) {
                FeedId = 1,
            };

            _PolarPlotSlices.Add(new PolarPlotSlice() {
                AltitudeLower = 101,
                AltitudeHigher = 200,
                PolarPlots = {
                    { 21, new PolarPlot() { Altitude = 110, Latitude = 1.234, Longitude = 5.678, Angle = 21, Distance = 42, } },
                    { 90, new PolarPlot() { Altitude = 120, Latitude = -43.2, Longitude = -32.1, Angle = 90, Distance = 56, } },
                }
            });
            _PolarPlotSlices.Add(new PolarPlotSlice() {
                AltitudeLower = 201,
                AltitudeHigher = 300,
            });

            SetupPolarPlotterForFeed(1);
            var json = SendJsonRequest<PolarPlotsJson>(request.Address);

            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(2, json.Slices.Count);

            var slice = json.Slices[0];
            Assert.AreEqual(101, slice.StartAltitude);
            Assert.AreEqual(200, slice.FinishAltitude);
            Assert.AreEqual(2, slice.Plots.Count);

            var plot = slice.Plots[0];
            Assert.AreEqual(1.234f, plot.Latitude, 0.001f);
            Assert.AreEqual(5.678f, plot.Longitude, 0.001f);

            plot = slice.Plots[1];
            Assert.AreEqual(-43.2f, plot.Latitude, 0.001f);
            Assert.AreEqual(-32.1f, plot.Longitude, 0.001f);

            slice = json.Slices[1];
            Assert.AreEqual(201, slice.StartAltitude);
            Assert.AreEqual(300, slice.FinishAltitude);
            Assert.AreEqual(0, slice.Plots.Count);
        }

        [TestMethod]
        public void WebSite_PolarPlotJson_Returns_Correct_Json_For_Request_For_Polar_Plots_From_Internet_When_Permission_Denied()
        {
            var request = new PolarPlotAddress(_Request) {
                FeedId = 1,
            };
            _PolarPlotSlices.Add(new PolarPlotSlice() {
                AltitudeLower = 101,
                AltitudeHigher = 200,
                PolarPlots = {
                    { 21, new PolarPlot() { Altitude = 110, Latitude = 1.234, Longitude = 5.678, Angle = 21, Distance = 42, } },
                    { 90, new PolarPlot() { Altitude = 120, Latitude = -43.2, Longitude = -32.1, Angle = 90, Distance = 56, } },
                }
            });
            SetupPolarPlotterForFeed(1);
            
            var json = SendJsonRequest<PolarPlotsJson>(request.Address, isInternetClient: true);

            Assert.AreEqual(-1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public void WebSite_PolarPlotJson_Returns_Correct_Json_For_Request_For_Polar_Plots_From_Internet_When_Permission_Granted()
        {
            _Configuration.InternetClientSettings.CanShowPolarPlots = true;
            var request = new PolarPlotAddress(_Request) {
                FeedId = 1,
            };
            _PolarPlotSlices.Add(new PolarPlotSlice() {
                AltitudeLower = 101,
                AltitudeHigher = 200,
                PolarPlots = {
                    { 21, new PolarPlot() { Altitude = 110, Latitude = 1.234, Longitude = 5.678, Angle = 21, Distance = 42, } },
                    { 90, new PolarPlot() { Altitude = 120, Latitude = -43.2, Longitude = -32.1, Angle = 90, Distance = 56, } },
                }
            });
            SetupPolarPlotterForFeed(1);
            
            var json = SendJsonRequest<PolarPlotsJson>(request.Address, isInternetClient: true);

            Assert.AreEqual(1, json.FeedId);
            Assert.AreNotEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public void WebSite_PolarPlotJson_Returns_Correct_Json_When_Feed_Is_Missing()
        {
            var request = new PolarPlotAddress(_Request) {
            };

            SetupPolarPlotterForFeed(1);
            var json = SendJsonRequest<PolarPlotsJson>(request.Address);

            Assert.AreEqual(-1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public void WebSite_PolarPlotJson_Returns_Correct_Json_When_Feed_Is_Invalid()
        {
            var request = new PolarPlotAddress(_Request) {
                FeedId = 42,
            };

            SetupPolarPlotterForFeed(1);
            var json = SendJsonRequest<PolarPlotsJson>(request.Address);

            Assert.AreEqual(42, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public void WebSite_PolarPlotJson_Returns_Correct_Json_When_Feed_Has_No_Polar_Plot()
        {
            var request = new PolarPlotAddress(_Request) {
                FeedId = 1,
            };
            var aircraftList = SetupPolarPlotterForFeed(1);
            aircraftList.Setup(r => r.PolarPlotter).Returns((IPolarPlotter)null);

            var json = SendJsonRequest<PolarPlotsJson>(request.Address);

            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }
        #endregion
    }
}
