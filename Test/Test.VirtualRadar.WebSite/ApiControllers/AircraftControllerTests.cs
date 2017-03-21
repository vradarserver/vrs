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
using VirtualRadar.Interface;

namespace Test.VirtualRadar.WebSite.ApiControllers
{
    [TestClass]
    public class AircraftControllerTests : ControllerTests
    {
        private Mock<IAirportDataDotCom> _AirportDataDotCom;

        protected override void ExtraInitialise()
        {
            _AirportDataDotCom = TestUtilities.CreateMockImplementation<IAirportDataDotCom>();
            _AirportDataDotCom.Setup(r => r.GetThumbnails(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns((WebRequestResult<AirportDataThumbnailsJson>)null);
        }

        private void SetupAirportDataDotCom(string icao, string registration, int maxThumbs, HttpStatusCode statusCode, AirportDataThumbnailsJson response)
        {
            var result = new WebRequestResult<AirportDataThumbnailsJson>(statusCode, response);
            _AirportDataDotCom.Setup(r => r.GetThumbnails((icao ?? "").ToUpper(), (registration ?? "").ToUpper(), maxThumbs)).Returns(result);
        }

        #region GetAircraftDataDotComThumbnails
        [TestMethod]
        public async Task AircraftController_GetAddcThumbnails_Returned_AirportDataApi_Json()
        {
            SetupAirportDataDotCom("4008F6", "G-VROS", 2, HttpStatusCode.OK, new AirportDataThumbnailsJson() {
                Error = "none",
                Status = 200,
                Thumbnails = {
                    new AirportDataThumbnailJson() { ImageUrl = "image1", LinkUrl = "link1", PhotographerName = "copyright1" },
                    new AirportDataThumbnailJson() { ImageUrl = "image2", LinkUrl = "link2", PhotographerName = "copyright2" },
                }
            });

            var response = await _Server.HttpClient.GetAsync("/api/1.00/aircraft/4008F6/airport-data-thumbnails?reg=G%2DVROS&numThumbs=2");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<AirportDataThumbnailsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(200, json.Status);
            Assert.AreEqual("none", json.Error);
            Assert.AreEqual(2, json.Thumbnails.Count);
            Assert.AreEqual("image1", json.Thumbnails[0].ImageUrl);
            Assert.AreEqual("link1", json.Thumbnails[0].LinkUrl);
            Assert.AreEqual("copyright1", json.Thumbnails[0].PhotographerName);
            Assert.AreEqual("image2", json.Thumbnails[1].ImageUrl);
            Assert.AreEqual("link2", json.Thumbnails[1].LinkUrl);
            Assert.AreEqual("copyright2", json.Thumbnails[1].PhotographerName);
        }

        [TestMethod]
        public async Task AircraftController_GetAddcThumbnails_Responds_To_Version_2_Route()
        {
            SetupAirportDataDotCom("4008F6", "G-VROS", 1, HttpStatusCode.OK, new AirportDataThumbnailsJson() {
                Error = "none",
                Status = 200,
                Thumbnails = {
                    new AirportDataThumbnailJson() { ImageUrl = "image1", LinkUrl = "link1", PhotographerName = "copyright1" },
                    new AirportDataThumbnailJson() { ImageUrl = "image2", LinkUrl = "link2", PhotographerName = "copyright2" },
                }
            });

            var response = await _Server.HttpClient.GetAsync("/AirportDataThumbnails.json?icao=4008F6&reg=G%2DVROS&numThumbs=1");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<AirportDataThumbnailsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("none", json.Error);
            Assert.AreEqual(2, json.Thumbnails.Count);
        }

        [TestMethod]
        public async Task AircraftController_GetAddcThumbnails_Converts_Icao_And_Reg_To_UpperCase()
        {
            SetupAirportDataDotCom("4008F6", "G-VROS", 1, HttpStatusCode.OK, new AirportDataThumbnailsJson() { Status = 200 });

            var response = await _Server.HttpClient.GetAsync("/api/1.00/aircraft/4008f6/airport-data-thumbnails?reg=g%2dvros&numThumbs=1");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<AirportDataThumbnailsJson>(content);

            Assert.AreEqual(200, json.Status);
        }

        [TestMethod]
        public async Task AircraftController_GetAddcThumbnails_Defaults_Registration_To_Null()
        {
            SetupAirportDataDotCom("4008F6", null, 1, HttpStatusCode.OK, new AirportDataThumbnailsJson() { Status = 200 });

            var response = await _Server.HttpClient.GetAsync("/api/1.00/aircraft/4008f6/airport-data-thumbnails?numThumbs=1");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<AirportDataThumbnailsJson>(content);

            Assert.AreEqual(200, json.Status);
        }

        [TestMethod]
        public async Task AircraftController_GetAddcThumbnails_Defaults_Number_Of_Thumbnails_To_One()
        {
            SetupAirportDataDotCom("4008F6", "G-VROS", 1, HttpStatusCode.OK, new AirportDataThumbnailsJson() { Status = 200 });

            var response = await _Server.HttpClient.GetAsync("/api/1.00/aircraft/4008f6/airport-data-thumbnails?reg=G%2DVROS");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<AirportDataThumbnailsJson>(content);

            Assert.AreEqual(200, json.Status);
        }

        [TestMethod]
        public async Task AircraftController_GetAddcThumbnails_Returns_Error_If_Icao_Is_Missing()
        {
            var response = await _Server.HttpClient.GetAsync("/AirportDataThumbnails.json?icao=&reg=G%2DVROS&numThumbs=1");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<AirportDataThumbnailsJson>(content);

            Assert.AreEqual(0, json.Status);
            Assert.AreEqual(0, json.Thumbnails.Count);
            Assert.AreEqual("Invalid ICAO", json.Error);
        }

        [TestMethod]
        public async Task AircraftController_GetAddcThumbnails_Returns_Error_If_Icao_Is_Garbage()
        {
            var response = await _Server.HttpClient.GetAsync("/api/1.00/aircraft/garbage/airport-data-thumbnails?reg=g%2dvros&numThumbs=1");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<AirportDataThumbnailsJson>(content);

            Assert.AreEqual(0, json.Status);
            Assert.AreEqual(0, json.Thumbnails.Count);
            Assert.AreEqual("Invalid ICAO", json.Error);
        }

        [TestMethod]
        public async Task AircraftController_GetAddcThumbnails_Passes_Through_Status_Code_If_AirportDataDotCom_Returns_Error()
        {
            SetupAirportDataDotCom("4008F6", "G-VROS", 1, HttpStatusCode.BadRequest, null);

            var response = await _Server.HttpClient.GetAsync("/api/1.00/aircraft/4008F6/airport-data-thumbnails?reg=G%2DVROS&numThumbs=1");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<AirportDataThumbnailsJson>(content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(400, json.Status);
            Assert.AreEqual("Could not retrieve thumbnails", json.Error);
            Assert.AreEqual(0, json.Thumbnails.Count);
        }

        [TestMethod]
        public async Task AircraftController_GetAddcThumbnails_Copes_If_AirportDataDotCom_Throws_Exception()
        {
            _AirportDataDotCom.Setup(r => r.GetThumbnails("4008F6", "G-VROS", 1)).Callback(() => {
                throw new InvalidOperationException();
            });

            var response = await _Server.HttpClient.GetAsync("/api/1.00/aircraft/4008F6/airport-data-thumbnails?reg=G%2DVROS&numThumbs=1");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonConvert.DeserializeObject<AirportDataThumbnailsJson>(content);

            Assert.AreEqual(500, json.Status);
            Assert.IsTrue(json.Error.StartsWith("Exception caught while fetching thumbnails:"));
        }
        #endregion
    }
}
