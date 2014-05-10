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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface;
using System.Net;

namespace Test.VirtualRadar.WebSite
{
    // This partial class contains all of the tests on the AirportDataThumbnails.json request.
    public partial class WebSiteTests
    {
        #region Pages
        private const string AirportDataThumbnailsPage = "/AirportDataThumbnails.json";
        #endregion

        #region Private class - AirportDataThumbnailsAddress
        class AirportDataThumbnailsAddress
        {
            private Mock<IRequest> _Request;

            public string Page { get; set; }
            public string Icao { get; set; }
            public string Registration { get; set; }
            public int CountThumbnails { get; set; }

            public AirportDataThumbnailsAddress(Mock<IRequest> request)
            {
                _Request = request;
                Page = AirportDataThumbnailsPage;
                CountThumbnails = -1;
            }

            public string Address
            {
                get
                {
                    Dictionary<string, string> queryValues = new Dictionary<string,string>();

                    if(Icao != null) queryValues.Add("icao", Icao);
                    if(Registration != null) queryValues.Add("reg", Registration);
                    if(CountThumbnails > -1) queryValues.Add("numThumbs", CountThumbnails.ToString(CultureInfo.InvariantCulture));

                    return BuildUrl(Page, queryValues);
                }
            }
        }
        #endregion

        #region Helpers - SetupAirportDataThumbnails
        private void SetupAirportDataThumbnails(int countThumbnails)
        {
            _AirportDataThumbnails.HttpStatusCode = HttpStatusCode.OK;
            _AirportDataThumbnails.Result = new AirportDataThumbnailsJson() {
                Status = 200,
            };

            for(var i = 0;i < countThumbnails;++i) {
                _AirportDataThumbnails.Result.Thumbnails.Add(new AirportDataThumbnailJson() {
                    ImageUrl = String.Format("Image{0}", i + 1),
                    LinkUrl = String.Format("Link{0}", i + 1),
                    PhotographerName = String.Format("Photographer{0}", i + 1),
                });
            }
        }
        #endregion

        #region AirportDataThumbnails tests
        [TestMethod]
        public void WebSite_AirportDataThumbnails_Calls_IAirportDataThumbnails_And_Returns_Response()
        {
            var request = new AirportDataThumbnailsAddress(_Request) {
                Icao = "ABC123",
                Registration = "G-VROS", 
                CountThumbnails = 2,
            };
            SetupAirportDataThumbnails(2);

            var json = SendJsonRequest<AirportDataThumbnailsJson>(request.Address);

            Assert.AreEqual("ABC123", _AirportDataThumbnailsIcao);
            Assert.AreEqual("G-VROS", _AirportDataThumbnailsRegistration);
            Assert.AreEqual(2, _AirportDataThumbnailsCountThumbnails);

            Assert.AreEqual(200, json.Status);
            Assert.AreEqual(null, json.Error);
            Assert.AreEqual(2, json.Thumbnails.Count);

            for(var i = 1;i <= 2;++i) {
                var thumbnail = json.Thumbnails[i - 1];
                Assert.AreEqual(String.Format("Image{0}", i), thumbnail.ImageUrl);
                Assert.AreEqual(String.Format("Link{0}", i), thumbnail.LinkUrl);
                Assert.AreEqual(String.Format("Photographer{0}", i), thumbnail.PhotographerName);
            }
        }

        [TestMethod]
        public void WebSite_AirportDataThumbnails_Converts_ICAO_To_UpperCase()
        {
            var request = new AirportDataThumbnailsAddress(_Request) {
                Icao = "abc123",
                CountThumbnails = 2,
            };
            SetupAirportDataThumbnails(2);

            var json = SendJsonRequest<AirportDataThumbnailsJson>(request.Address);

            Assert.AreEqual("ABC123", _AirportDataThumbnailsIcao);
        }

        [TestMethod]
        public void WebSite_AirportDataThumbnails_Returns_Error_Json_When_Icao_Is_Garbage()
        {
            foreach(var badIcao in new string[] { null, "", "      ", "ABCDE", "GGGGGG", "ABCDEF1" }) {
                TestCleanup();
                TestInitialise();

                var request = new AirportDataThumbnailsAddress(_Request) {
                    Icao = badIcao,
                    CountThumbnails = 2,
                };
                SetupAirportDataThumbnails(2);

                var json = SendJsonRequest<AirportDataThumbnailsJson>(request.Address);

                _AirportDataDotCom.Verify(r => r.GetThumbnails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never());
                Assert.AreEqual(0, json.Status);
                Assert.AreEqual("Invalid ICAO", json.Error);
                Assert.AreEqual(0, json.Thumbnails.Count);
            }
        }

        [TestMethod]
        public void WebSite_AirportDataThumbnals_Returns_Error_Json_When_AirportData_Returns_404()
        {
            var request = new AirportDataThumbnailsAddress(_Request) {
                Icao = "abc123",
                CountThumbnails = 2,
            };
            _AirportDataThumbnails.HttpStatusCode = HttpStatusCode.NotFound;

            var json = SendJsonRequest<AirportDataThumbnailsJson>(request.Address);

            Assert.AreEqual(404, json.Status);
            Assert.AreEqual("Could not retrieve thumbnails", json.Error);
            Assert.AreEqual(0, json.Thumbnails.Count);
        }

        [TestMethod]
        public void WebSite_AirportDataThumbnals_Returns_Error_Json_When_AirportData_Throws_Exception()
        {
            var request = new AirportDataThumbnailsAddress(_Request) {
                Icao = "abc123",
                CountThumbnails = 2,
            };

            var exception = new InvalidOperationException();
            _AirportDataDotCom.Setup(r => r.GetThumbnails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Callback((string icao, string reg, int count) => { throw exception; });

            var json = SendJsonRequest<AirportDataThumbnailsJson>(request.Address);

            Assert.AreEqual(500, json.Status);
            Assert.IsTrue(json.Error.Contains("Exception caught while fetching thumbnails: "));
            Assert.AreEqual(0, json.Thumbnails.Count);
        }
        #endregion
    }
}
