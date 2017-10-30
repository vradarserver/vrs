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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Test.Framework;
using Test.VirtualRadar.WebSite.TestHelpers;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite.ApiControllers
{
    [TestClass]
    public class ReportsControllerTests : ControllerTests
    {
        private ReportRowsAddress _ReportRowsAddress;
        private Mock<IBaseStationDatabase> _BaseStationDatabase;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigBaseStationDatabase;

        protected override void ExtraInitialise()
        {
            _ReportRowsAddress = new ReportRowsAddress();

            _BaseStationDatabase = new Mock<IBaseStationDatabase>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AutoConfigBaseStationDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _AutoConfigBaseStationDatabase.Setup(a => a.Database).Returns(_BaseStationDatabase.Object);
        }

        /// <summary>
        /// Returns the type of the top-level JSON class associated with the report type passed across.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Type ReportJsonType(ReportJsonClass type)
        {
            switch(type) {
                case ReportJsonClass.Aircraft:  return typeof(AircraftReportJson);
                case ReportJsonClass.Flight:    return typeof(FlightReportJson);
                default:                        throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Requests a JSON file. The output is parsed into a JSON file of the type specified.
        /// </summary>
        /// <param name="jsonType"></param>
        /// <param name="pathAndFile"></param>
        /// <param name="isInternetClient"></param>
        /// <returns></returns>
        private object SendJsonRequest(Type jsonType, string pathAndFile, bool isInternetClient = false, Action<HttpResponseMessage> httpResponseAction = null)
        {
            _RemoteIpAddress = isInternetClient ? "1.2.3.4" : "127.0.0.1";

            var response = _Server.HttpClient.GetAsync(pathAndFile).Result;
            httpResponseAction?.Invoke(response);

            var content = response.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject(content, jsonType);
        }

        #region Date report tests
        [TestMethod]
        public void ReportRows_DateReport_Generates_Correct_JSON_When_No_Rows_Match()
        {
            Do_ReportRows_Report_Generates_Correct_JSON_When_No_Rows_Match("date", ReportJsonClass.Flight);
        }

        [TestMethod]
        public void ReportRows_DateReport_Adds_Correct_Cache_Control_Header()
        {
            Do_ReportRows_Report_Adds_Correct_Cache_Control_Header("date", ReportJsonClass.Flight);
        }

        [TestMethod]
        public void ReportRows_DateReport_Only_Returns_Json_If_Reports_Are_Permitted()
        {
            Do_ReportRows_Report_Only_Returns_Json_If_Reports_Are_Permitted("date", ReportJsonClass.Flight);
        }

        [TestMethod]
        public void ReportRows_DateReport_Returns_Count_Of_Rows_Matching_Criteria()
        {
            Do_WebSite_ReportRows_Report_Returns_Count_Of_Rows_Matching_Criteria("date", ReportJsonClass.Flight);
        }
        #endregion

        #region Tests shared between different report types
        private void Do_ReportRows_Report_Generates_Correct_JSON_When_No_Rows_Match(string report, ReportJsonClass reportClass)
        {
            _ReportRowsAddress.Report = report;

            dynamic json = SendJsonRequest(ReportJsonType(reportClass), _ReportRowsAddress.Address);

            Assert.AreEqual(0, json.Airports.Count);
            Assert.AreEqual(0, json.CountRows);
            Assert.AreEqual(null, json.ErrorText);
            Assert.AreEqual(0, json.Flights.Count);
            Assert.AreEqual("", json.GroupBy);
            Assert.AreEqual(0.ToString("0.000"), json.ProcessingTime);
            Assert.AreEqual(0, json.Routes.Count);

            switch(reportClass) {
                case ReportJsonClass.Aircraft:  Assert.AreEqual(true, json.Aircraft.IsUnknown); break;
                case ReportJsonClass.Flight:    Assert.AreEqual(0, json.Aircraft.Count); break;
                default:                        throw new NotImplementedException();
            }
        }

        private void Do_ReportRows_Report_Adds_Correct_Cache_Control_Header(string report, ReportJsonClass reportClass)
        {
            _ReportRowsAddress.Report = report;

            SendJsonRequest(ReportJsonType(reportClass), _ReportRowsAddress.Address, httpResponseAction: response => {
                var cacheControl = response.Headers.CacheControl;
                Assert.AreEqual(0, cacheControl.MaxAge.Value.TotalSeconds);
                Assert.IsTrue(cacheControl.NoCache);
                Assert.IsTrue(cacheControl.NoStore);
                Assert.IsTrue(cacheControl.MustRevalidate);
            });
        }

        private void Do_ReportRows_Report_Only_Returns_Json_If_Reports_Are_Permitted(string report, ReportJsonClass reportClass)
        {
            _ReportRowsAddress.Report = report;
            var jsonType = ReportJsonType(reportClass);

            _Configuration.InternetClientSettings.CanRunReports = false;
            Assert.IsNull(SendJsonRequest(jsonType, _ReportRowsAddress.Address, isInternetClient: true));

            _Configuration.InternetClientSettings.CanRunReports = true;
            Assert.IsNotNull(SendJsonRequest(jsonType, _ReportRowsAddress.Address, isInternetClient: true));

            _Configuration.InternetClientSettings.CanRunReports = false;
            Assert.IsNotNull(SendJsonRequest(jsonType, _ReportRowsAddress.Address, isInternetClient: false));

            _Configuration.InternetClientSettings.CanRunReports = true;
            Assert.IsNotNull(SendJsonRequest(jsonType, _ReportRowsAddress.Address, isInternetClient: false));
        }

        private void Do_WebSite_ReportRows_Report_Returns_Count_Of_Rows_Matching_Criteria(string report, ReportJsonClass reportClass)
        {
            _ReportRowsAddress.Report = report;

            switch(reportClass) {
                case ReportJsonClass.Aircraft:
                    _BaseStationDatabase.Setup(db => db.GetCountOfFlightsForAircraft(It.IsAny<BaseStationAircraft>(), It.IsAny<SearchBaseStationCriteria>())).Returns(12);
                    _ReportRowsAddress.Icao24 = _ReportRowsAddress.Registration = new StringFilter("A", FilterCondition.Equals, false);
                    break;
                case ReportJsonClass.Flight:
                    _BaseStationDatabase.Setup(db => db.GetCountOfFlights(It.IsAny<SearchBaseStationCriteria>())).Returns(12);
                    break;
                default:
                    throw new NotImplementedException();
            }

            _BaseStationDatabase.Setup(db => db.GetCountOfFlights(It.IsAny<SearchBaseStationCriteria>())).Returns(12);

            dynamic json = SendJsonRequest(ReportJsonType(reportClass), _ReportRowsAddress.Address);

            Assert.AreEqual(12, json.CountRows);
        }
        #endregion
    }
}
