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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite.ApiControllers
{
    [TestClass]
    public class DirectoryEntryControllerTests : ControllerTests
    {
        private Mock<IApplicationInformation> _ApplicationInformation;
        private Mock<IFeedManager> _FeedManager;

        protected override void ExtraInitialise()
        {
            _ApplicationInformation = TestUtilities.CreateMockImplementation<IApplicationInformation>();
            _FeedManager = TestUtilities.CreateMockSingleton<IFeedManager>();
            _FeedManager.SetupGet(r => r.VisibleFeeds).Returns(new IFeed[] {});
        }

        private void SetVersionNumber(string version)
        {
            _ApplicationInformation.SetupGet(r => r.ShortVersion).Returns(version);
        }

        private Mock<IFeed>[] SetCountVisibleFeeds(int countFeeds)
        {
            var feeds = new Mock<IFeed>[countFeeds];
            for(var i = 0;i < feeds.Length;++i) {
                var feed = TestUtilities.CreateMockInstance<IFeed>();
                feeds[i] = feed;

                var aircraftList = TestUtilities.CreateMockInstance<IBaseStationAircraftList>();
                feed.SetupGet(r => r.AircraftList).Returns(aircraftList.Object);

                var snapshotList = new List<IAircraft>(new IAircraft[0]);
                long unused1, unused2;
                aircraftList.Setup(r => r.TakeSnapshot(out unused1, out unused2)).Returns(snapshotList);
            }

            _FeedManager.SetupGet(r => r.VisibleFeeds).Returns(feeds.Select(r => r.Object).ToArray());

            return feeds;
        }

        private void SetAircraftListCounts(params int[] counts)
        {
            var feeds = new IFeed[counts.Length];
            for(var i = 0;i < feeds.Length;++i) {
                var feed = TestUtilities.CreateMockInstance<IFeed>();
                var aircraftList = TestUtilities.CreateMockInstance<IBaseStationAircraftList>();
                feed.SetupGet(r => r.AircraftList).Returns(aircraftList.Object);

                var snapshotList = new List<IAircraft>(new IAircraft[counts[i]]);
                long unused1, unused2;
                aircraftList.Setup(r => r.TakeSnapshot(out unused1, out unused2)).Returns(snapshotList);

                feeds[i] = feed.Object;
            }

            _FeedManager.SetupGet(r => r.VisibleFeeds).Returns(feeds);
        }

        private void SetKey(Guid key, bool ensureUppercase = true)
        {
            var text = key.ToString();
            text = ensureUppercase ? text.ToUpper() : text.ToLower();

            _Configuration.GoogleMapSettings.DirectoryEntryKey = text;
        }

        [TestMethod]
        public void DirectoryEntryController_GetDirectoryEntry_Responds_With_Entry_Information()
        {
            var key = Guid.NewGuid();
            SetKey(key);

            var json = Get($"/api/3.00/directory-entry/{key}").Json<DirectoryEntryJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void DirectoryEntryController_GetDirectoryEntry_Responds_To_Legacy_Path()
        {
            var key = Guid.NewGuid();
            SetKey(key);

            var json = Get($"/DirectoryEntry.json?key={key}").Json<DirectoryEntryJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void DirectoryEntryController_GetDirectoryEntry_Is_Not_Case_Sensitive()
        {
            var key = Guid.NewGuid();
            SetKey(key, ensureUppercase: true);

            var json = Get($"/api/3.00/directory-entry/{key.ToString().ToLower()}").Json<DirectoryEntryJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void DirectoryEntryController_GetDirectoryEntry_Shows_Short_Version_Number()
        {
            var key = Guid.NewGuid();
            SetKey(key);
            SetVersionNumber("1.2.3");

            var json = Get($"/api/3.00/directory-entry/{key}").Json<DirectoryEntryJson>();

            Assert.AreEqual("1.2.3", json.Version);
        }

        [TestMethod]
        public void DirectoryEntryController_GetDirectoryEntry_Shows_Number_Of_Visible_Feeds()
        {
            var key = Guid.NewGuid();
            SetKey(key);
            SetCountVisibleFeeds(2);

            var json = Get($"/api/3.00/directory-entry/{key}").Json<DirectoryEntryJson>();

            Assert.AreEqual(2, json.NumberOfFeeds);
        }

        [TestMethod]
        public void DirectoryEntryController_GetDirectoryEntry_Does_Not_Include_Visible_Feeds_With_No_Aircraft_List()
        {
            var key = Guid.NewGuid();
            SetKey(key);
            var feedMocks = SetCountVisibleFeeds(4);

            feedMocks[1].SetupGet(r => r.AircraftList).Returns((IBaseStationAircraftList)null);

            var json = Get($"/api/3.00/directory-entry/{key}").Json<DirectoryEntryJson>();

            Assert.AreEqual(3, json.NumberOfFeeds);
        }

        [TestMethod]
        public void DirectoryEntryController_GetDirectoryEntry_Shows_Number_Of_Aircraft_On_Display_On_Largest_Feed()
        {
            var key = Guid.NewGuid();
            SetKey(key);
            SetAircraftListCounts(7, 42, 28);

            var json = Get($"/api/3.00/directory-entry/{key}").Json<DirectoryEntryJson>();

            Assert.AreEqual(42, json.NumberOfAircraft);
        }

        [TestMethod]
        public void DirectoryEntryController_GetDirectoryEntry_Responds_With_404_When_Key_Missing()
        {
            SetKey(Guid.NewGuid());

            Get("/api/3.00/directory-entry");
            Assert.AreEqual(HttpStatusCode.NotFound, _Context.ResponseHttpStatusCode);
            //Assert.AreEqual(0, content.Length);       web api returns a route not found message in the body because {key} is part of the route

            Get("/api/3.00/directory-entry/");
            Assert.AreEqual(HttpStatusCode.NotFound, _Context.ResponseHttpStatusCode);
            //Assert.AreEqual(0, content.Length);       web api returns a route not found message in the body because {key} is part of the route

            Get("/DirectoryEntry.json");
            Assert.AreEqual(HttpStatusCode.NotFound, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(0, _Context.ResponseBody.Length);

            Get("/DirectoryEntry.json?key=");
            Assert.AreEqual(HttpStatusCode.NotFound, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(0, _Context.ResponseBody.Length);
        }

        [TestMethod]
        public void DirectoryEntryController_GetDirectoryEntry_Responds_With_404_When_Key_Is_Wrong()
        {
            SetKey(Guid.NewGuid());
        
            Get($"/api/3.00/directory-entry/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(0, _Context.ResponseBody.Length);
        }
    }
}
