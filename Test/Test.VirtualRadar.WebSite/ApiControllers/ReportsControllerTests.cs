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
using VRSInterface = VirtualRadar.Interface;
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
        private Mock<VRSInterface.ILog> _Log;
        private ClockMock _Clock;
        private MockFileSystemProvider _FileSystemProvider;

        protected override void ExtraInitialise()
        {
            _ReportRowsAddress = new ReportRowsAddress();

            _BaseStationDatabase = TestUtilities.CreateMockInstance<IBaseStationDatabase>();
            _AutoConfigBaseStationDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _AutoConfigBaseStationDatabase.Setup(a => a.Database).Returns(_BaseStationDatabase.Object);

            _Log = TestUtilities.CreateMockSingleton<VRSInterface.ILog>();

            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<VRSInterface.IClock>(_Clock.Object);

            _FileSystemProvider = new MockFileSystemProvider();
            Factory.Singleton.RegisterInstance<VRSInterface.IFileSystemProvider>(_FileSystemProvider);
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

        private void InterceptGetCountOfFlights(Func<SearchBaseStationCriteria, int> func)
        {
            _BaseStationDatabase.Setup(
                db => db.GetCountOfFlights(It.IsAny<SearchBaseStationCriteria>())
            ).Returns(
                (SearchBaseStationCriteria c) => { return func(c); }
            );
        }

        private void InterceptGetFlights(Func<SearchBaseStationCriteria, int, int, string, bool, string, bool, List<BaseStationFlight>> func)
        {
            _BaseStationDatabase.Setup(
                r => r.GetFlights(It.IsAny<SearchBaseStationCriteria>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>())
            ).Returns(
                (SearchBaseStationCriteria c, int from, int to, string sort1, bool sort1Asc, string sort2, bool sort2Asc) =>
                {
                    return func(c, from, to, sort1, sort1Asc, sort2, sort2Asc) ?? new List<BaseStationFlight>();
                }
            );
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

        private T SendJsonRequest<T>(string pathAndFile, bool isInternetClient = false, Action<HttpResponseMessage> httpResponseAction = null)
        {
            _RemoteIpAddress = isInternetClient ? "1.2.3.4" : "127.0.0.1";

            var response = _Server.HttpClient.GetAsync(pathAndFile).Result;
            httpResponseAction?.Invoke(response);

            var content = response.Content.ReadAsStringAsync().Result;

            return JsonConvert.DeserializeObject<T>(content);
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
            Do_ReportRows_Report_Returns_Count_Of_Rows_Matching_Criteria("date", ReportJsonClass.Flight);
        }

        [TestMethod]
        public void ReportRows_DateReport_Returns_Details_Of_Exceptions_Raised_During_Report_Generation()
        {
            Do_ReportRows_Report_Returns_Details_Of_Exceptions_Raised_During_Report_Generation("date", ReportJsonClass.Flight);
        }

        [TestMethod]
        public void ReportRows_DateReport_Logs_Exceptions_Raised_During_Report_Generation()
        {
            Do_ReportRows_Report_Logs_Exceptions_Raised_During_Report_Generation("date", ReportJsonClass.Flight);
        }

        [TestMethod]
        public void ReportRows_DateReport_Returns_Processing_Time()
        {
            Do_ReportRows_Report_Returns_Processing_Time("date", ReportJsonClass.Flight);
        }

        [TestMethod]
        public void ReportRows_DateReport_Returns_Images_Available_Flags()
        {
            Do_ReportRows_Report_Returns_Images_Available_Flags("date", ReportJsonClass.Flight);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "FlightsReportDateLimits$")]
        public void ReportRows_DateReport_Returns_Date_Ranges_Used()
        {
            _Configuration.InternetClientSettings.CanRunReports = true;

            var worksheet = new ExcelWorksheetData(TestContext);

            _ReportRowsAddress.Report = "date";
            _ReportRowsAddress.Date = new DateFilter(worksheet.NDateTime("RequestStart"), worksheet.NDateTime("RequestEnd"), false);
            _ReportRowsAddress.Callsign =       StringFilter.CreateIfNotNull(worksheet.String("Callsign"));
            _ReportRowsAddress.Registration =   StringFilter.CreateIfNotNull(worksheet.String("Registration"));
            _ReportRowsAddress.Icao24 =         StringFilter.CreateIfNotNull(worksheet.String("Icao24"));

            _Clock.Setup(p => p.UtcNow).Returns(worksheet.DateTime("Today"));

            var json = SendJsonRequest<FlightReportJson>(_ReportRowsAddress.Address, worksheet.Bool("IsInternetClient"));

            var actualStart = worksheet.NDateTime("ActualStart");
            var actualEnd = worksheet.NDateTime("ActualEnd");
            Assert.AreEqual(actualStart.GetValueOrDefault().Year == 1 ? null : actualStart.Value.Date.ToString("yyyy-MM-dd"), json.FromDate);
            Assert.AreEqual(actualEnd.GetValueOrDefault().Year == 1 ? null : actualEnd.Value.Date.ToString("yyyy-MM-dd"), json.ToDate);
        }

        [TestMethod]
        public void ReportRows_DateReport_Passes_Same_Criteria_To_CountRows_And_FetchRows()
        {
            _ReportRowsAddress.Report = "date";

            SearchBaseStationCriteria searchCriteria = null;
            InterceptGetCountOfFlights(c => { searchCriteria = c; return 1; });
            SendJsonRequest<FlightReportJson>(_ReportRowsAddress.Address);

            _BaseStationDatabase.Verify(db => db.GetFlights(searchCriteria, -1, -1, null, It.IsAny<bool>(), null, It.IsAny<bool>()), Times.Once());
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "FlightsReportDateLimits$")]
        public void ReportRows_DateReport_Sets_Correct_Limits_On_Date_Ranges()
        {
            _Configuration.InternetClientSettings.CanRunReports = true;

            var worksheet = new ExcelWorksheetData(TestContext);

            _ReportRowsAddress.Report = "date";
            _ReportRowsAddress.Date = new DateFilter(worksheet.NDateTime("RequestStart"), worksheet.NDateTime("RequestEnd"), false);
            _ReportRowsAddress.Callsign =       StringFilter.CreateIfNotNull(worksheet.String("Callsign"));
            _ReportRowsAddress.Registration =   StringFilter.CreateIfNotNull(worksheet.String("Registration"));
            _ReportRowsAddress.Icao24 =         StringFilter.CreateIfNotNull(worksheet.String("Icao24"));

            _Clock.Setup(p => p.UtcNow).Returns(worksheet.DateTime("Today"));

            SearchBaseStationCriteria searchCriteria = null;
            InterceptGetCountOfFlights(c => { searchCriteria = c; return 1; });
            var json = SendJsonRequest<FlightReportJson>(_ReportRowsAddress.Address, worksheet.Bool("IsInternetClient"));

            var actualStart = worksheet.NDateTime("ActualStart");
            var actualEnd = worksheet.NDateTime("ActualEnd");

            if(actualStart == null) Assert.IsTrue(searchCriteria.Date == null || searchCriteria.Date.LowerValue == null);
            else                    Assert.AreEqual(actualStart.Value, searchCriteria.Date.LowerValue);

            if(actualEnd == null)   Assert.IsTrue(searchCriteria.Date == null || searchCriteria.Date.UpperValue == null);
            else                    Assert.AreEqual(actualEnd.Value, searchCriteria.Date.UpperValue);
        }

        [TestMethod]
        public void ReportRows_DateReport_Passes_Range_And_Sort_Criteria_To_FetchRows()
        {
            _ReportRowsAddress.Report = "date";

            _ReportRowsAddress.FromRow = 10;
            _ReportRowsAddress.ToRow = 11;
            _ReportRowsAddress.SortField1 = "Ff1";
            _ReportRowsAddress.SortField2 = "Ff2";
            _ReportRowsAddress.SortAscending1 = true;
            _ReportRowsAddress.SortAscending2 = false;
            SendJsonRequest<FlightReportJson>(_ReportRowsAddress.Address);

            _BaseStationDatabase.Verify(db => db.GetFlights(It.IsAny<SearchBaseStationCriteria>(), 10, 11, "Ff1", true, "Ff2", false), Times.Once());

            _ReportRowsAddress.SortAscending1 = false;
            _ReportRowsAddress.SortAscending2 = true;
            SendJsonRequest<FlightReportJson>(_ReportRowsAddress.Address);

            _BaseStationDatabase.Verify(db => db.GetFlights(It.IsAny<SearchBaseStationCriteria>(), 10, 11, "Ff1", false, "Ff2", true), Times.Once());
        }

        [TestMethod]
        public void ReportRows_DateReport_Passes_UseAlternativeCallsigns_Criteria_To_FetchRows()
        {
            _ReportRowsAddress.Report = "date";

            SearchBaseStationCriteria criteria = null;
            InterceptGetFlights((c, u1, u2, u3, u4, u5, u6) => {
                criteria = c;
                return null;
            });

            _ReportRowsAddress.UseAlternativeCallsigns = true;
            SendJsonRequest<FlightReportJson>(_ReportRowsAddress.Address);
            Assert.IsTrue(criteria.UseAlternateCallsigns);

            _ReportRowsAddress.UseAlternativeCallsigns = false;
            SendJsonRequest<FlightReportJson>(_ReportRowsAddress.Address);
            Assert.IsFalse(criteria.UseAlternateCallsigns);
        }

        [TestMethod]
        public void ReportRows_DateReport_Informs_Caller_Of_Primary_Sort_Column_Used()
        {
            Do_ReportRows_Report_Informs_Caller_Of_Primary_Sort_Column_Used("date", ReportJsonClass.Flight);
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

        private void Do_ReportRows_Report_Returns_Count_Of_Rows_Matching_Criteria(string report, ReportJsonClass reportClass)
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

        private void Do_ReportRows_Report_Returns_Details_Of_Exceptions_Raised_During_Report_Generation(string report, ReportJsonClass reportClass)
        {
            _ReportRowsAddress.Report = report;

            switch(reportClass) {
                case ReportJsonClass.Aircraft:
                    _BaseStationDatabase.Setup(db => db.GetCountOfFlightsForAircraft(It.IsAny<BaseStationAircraft>(), It.IsAny<SearchBaseStationCriteria>())).Callback(() => { throw new InvalidOperationException("Text of message"); } );
                    _ReportRowsAddress.Icao24 = _ReportRowsAddress.Registration = new StringFilter("A", FilterCondition.Equals, false);
                    break;
                case ReportJsonClass.Flight:
                    _BaseStationDatabase.Setup(db => db.GetCountOfFlights(It.IsAny<SearchBaseStationCriteria>())).Callback(() => { throw new InvalidOperationException("Text of message"); } );
                    break;
                default:
                    throw new NotImplementedException();
            }

            dynamic json = SendJsonRequest(ReportJsonType(reportClass), _ReportRowsAddress.Address);

            Assert.IsNotNull(json.ErrorText);
            Assert.IsTrue(json.ErrorText.Contains("Text of message"));
        }

        private void Do_ReportRows_Report_Logs_Exceptions_Raised_During_Report_Generation(string report, ReportJsonClass reportClass)
        {
            _ReportRowsAddress.Report = report;

            switch(reportClass) {
                case ReportJsonClass.Aircraft:
                    _BaseStationDatabase.Setup(db => db.GetCountOfFlightsForAircraft(It.IsAny<BaseStationAircraft>(), It.IsAny<SearchBaseStationCriteria>())).Callback(() => { throw new InvalidOperationException("Message goes here"); } );
                    _ReportRowsAddress.Icao24 = _ReportRowsAddress.Registration = new StringFilter("A", FilterCondition.Equals, false);
                    break;
                case ReportJsonClass.Flight:
                    _BaseStationDatabase.Setup(db => db.GetCountOfFlights(It.IsAny<SearchBaseStationCriteria>())).Callback(() => { throw new InvalidOperationException("Message goes here"); } );
                    break;
                default:
                    throw new NotImplementedException();
            }

            dynamic json = SendJsonRequest(ReportJsonType(reportClass), _ReportRowsAddress.Address);

            _Log.Verify(p => p.WriteLine(It.IsAny<string>()), Times.Once());
        }

        private void Do_ReportRows_Report_Returns_Processing_Time(string report, ReportJsonClass reportClass)
        {
            _ReportRowsAddress.Report = report;

            var startTime = new DateTime(2001, 2, 3, 4, 5, 6);
            var endTime = startTime.AddMilliseconds(71234);

            // TODO: This relies on implementation details. Maybe use a stopwatch object instead?
            var callCount = 0;
            _Clock.SetupGet(r => r.UtcNow).Returns(() => callCount++ < 1 ? startTime : endTime);

            dynamic json = SendJsonRequest(ReportJsonType(reportClass), _ReportRowsAddress.Address);
            Assert.AreEqual((71.234M).ToString(), json.ProcessingTime);
        }

        private void Do_ReportRows_Report_Returns_Images_Available_Flags(string report, ReportJsonClass reportClass)
        {
            _ReportRowsAddress.Report = report;

            const string silhouettesFolder = @"c:\silhouettes";
            const string flagsFolder = @"c:\flags";

            foreach(var silhouettesFolderPresent in new bool[] { false, true }) {
                foreach(var flagsFolderPresent in new bool[] { false, true }) {
                    foreach(var showSilhouettes in new bool[] { false, true }) {
                        foreach(var showFlags in new bool[] { false, true }) {
                            _FileSystemProvider.Reset();

                            _Configuration.BaseStationSettings.SilhouettesFolder = showSilhouettes ? silhouettesFolder : null;
                            _Configuration.BaseStationSettings.OperatorFlagsFolder = showFlags ? flagsFolder : null;

                            if(silhouettesFolderPresent) {
                                _FileSystemProvider.AddFolder(silhouettesFolder);
                            }
                            if(flagsFolderPresent) {
                                _FileSystemProvider.AddFolder(flagsFolder);
                            }

                            dynamic json = SendJsonRequest(ReportJsonType(reportClass), _ReportRowsAddress.Address);

                            Assert.AreEqual(showSilhouettes && silhouettesFolderPresent, json.SilhouettesAvailable, $"show={showSilhouettes}, folder={silhouettesFolderPresent}");
                            Assert.AreEqual(showFlags && flagsFolderPresent, json.OperatorFlagsAvailable, $"show={showFlags}, folder={flagsFolderPresent}");
                        }
                    }
                }
            }
        }

        private void Do_ReportRows_Report_Informs_Caller_Of_Primary_Sort_Column_Used(string report, ReportJsonClass reportClass)
        {
            _ReportRowsAddress.Report = report;
            _ReportRowsAddress.Icao24 = _ReportRowsAddress.Registration = new StringFilter("A", FilterCondition.Equals, false);

            var jsonType = ReportJsonType(reportClass);

            dynamic json = SendJsonRequest(jsonType, _ReportRowsAddress.Address);
            Assert.AreEqual("", json.GroupBy);

            _ReportRowsAddress.SortField1 = "ABC";
            json = SendJsonRequest(jsonType, _ReportRowsAddress.Address);
            Assert.AreEqual("ABC", json.GroupBy);

            _ReportRowsAddress.SortField2 = "XYZ";
            json = SendJsonRequest(jsonType, _ReportRowsAddress.Address);
            Assert.AreEqual("ABC", json.GroupBy);

            _ReportRowsAddress.SortField1 = null;
            json = SendJsonRequest(jsonType, _ReportRowsAddress.Address);
            Assert.AreEqual("XYZ", json.GroupBy);
        }
        #endregion
    }
}
