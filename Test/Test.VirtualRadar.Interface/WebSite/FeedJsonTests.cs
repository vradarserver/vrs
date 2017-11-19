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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class FeedJsonTests
    {
        public TestContext TestContext { get; set; }
        private Mock<IFeed> _Feed;
        private Mock<IBaseStationAircraftList> _AircraftList;
        private Mock<IPolarPlotter> _PolarPlotter;

        [TestInitialize]
        public void TestInitialise()
        {
            _PolarPlotter = TestUtilities.CreateMockInstance<IPolarPlotter>();
            _AircraftList = TestUtilities.CreateMockInstance<IBaseStationAircraftList>();
            _Feed = TestUtilities.CreateMockInstance<IFeed>();
            _Feed.SetupGet(r => r.IsVisible).Returns(true);
            _Feed.SetupGet(r => r.AircraftList).Returns(_AircraftList.Object);
            _AircraftList.SetupGet(r => r.PolarPlotter).Returns(_PolarPlotter.Object);
        }

        [TestMethod]
        public void FeedJson_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var json = new FeedJson();

            TestUtilities.TestProperty(json, r => r.Name, null, "Abc");
            TestUtilities.TestProperty(json, r => r.UniqueId, 0, 123);
            TestUtilities.TestProperty(json, r => r.HasPolarPlot, false);
        }

        [TestMethod]
        public void FeedJson_ToModel_Returns_Null_If_Passed_Null()
        {
            Assert.IsNull(FeedJson.ToModel(null));
        }

        [TestMethod]
        public void FeedJson_ToModel_Returns_Null_If_Passed_Invisible_Feed()
        {
            _Feed.SetupGet(r => r.IsVisible).Returns(false);

            Assert.IsNull(FeedJson.ToModel(_Feed.Object));
        }

        [TestMethod]
        public void FeedJson_ToModel_Fills_Properties_Correctly()
        {
            _Feed.SetupGet(r => r.UniqueId).Returns(1);
            _Feed.SetupGet(r => r.Name).Returns("Feed Name");

            var model = FeedJson.ToModel(_Feed.Object);

            Assert.AreEqual(1, model.UniqueId);
            Assert.AreEqual("Feed Name", model.Name);
            Assert.IsTrue(model.HasPolarPlot);
        }

        [TestMethod]
        public void FeedJson_ToModel_Clears_HasPolarPlot_When_AircraftList_Is_Missing()
        {
            _Feed.SetupGet(r => r.AircraftList).Returns((IBaseStationAircraftList)null);

            var model = FeedJson.ToModel(_Feed.Object);

            Assert.IsFalse(model.HasPolarPlot);
        }

        [TestMethod]
        public void FeedJson_ToModel_Clears_HasPolarPlot_When_PolarPlotter_Is_Missing()
        {
            _AircraftList.SetupGet(r => r.PolarPlotter).Returns((IPolarPlotter)null);

            var model = FeedJson.ToModel(_Feed.Object);

            Assert.IsFalse(model.HasPolarPlot);
        }
    }
}
