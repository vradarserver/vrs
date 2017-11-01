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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite.ApiControllers
{
    /// <summary>
    /// Serves results of report requests.
    /// </summary>
    public class ReportsController : PipelineApiController
    {
        private ISharedConfiguration _SharedConfiguration;

        private ISharedConfiguration InitialiseSharedConfiguration()
        {
            var result = _SharedConfiguration;
            if(result == null) {
                result = Factory.Singleton.Resolve<ISharedConfiguration>().Singleton;
                _SharedConfiguration = result;
            }

            return result;
        }

        [HttpGet]
        [Route("ReportRows.json")]                      // V2 route
        public HttpResponseMessage ReportRowsV2()
        {
            HttpResponseMessage result = null;

            var config = InitialiseSharedConfiguration().Get();
            if(PipelineRequest.IsLocalOrLan || config.InternetClientSettings.CanRunReports) {
                var clock = Factory.Singleton.Resolve<IClock>();
                var startTime = clock.UtcNow;

                var parameters = ExtractV2Parameters();
                LimitDatesWhenNoStrongCriteriaPresent(clock, parameters, PipelineRequest.IsInternet);
                if(parameters?.Date?.UpperValue?.Year < 9999) {
                    parameters.Date.UpperValue = parameters.Date.UpperValue.Value.AddDays(1).AddMilliseconds(-1);
                }

                var jsonObj = new FlightReportJson() {
                    GroupBy = "",
                };

                try {
                    var baseStation = Factory.Singleton.Resolve<IAutoConfigBaseStationDatabase>().Singleton.Database;
                    jsonObj.CountRows = baseStation.GetCountOfFlights(parameters);
                    baseStation.GetFlights(
                        parameters,
                        parameters.FromRow,
                        parameters.ToRow,
                        parameters.SortField1,
                        parameters.SortAscending1,
                        parameters.SortField2,
                        parameters.SortAscending2
                    );
                } catch(Exception ex) {
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine($"An exception was encountered during the processing of a report: {ex}");
                    jsonObj.ErrorText = $"An exception was encounted during the processing of the report, see log for full details: {ex.Message}";
                }

                var fileSystemProvider = Factory.Singleton.Resolve<IFileSystemProvider>();
                jsonObj.ProcessingTime = String.Format("{0:N3}", (clock.UtcNow - startTime).TotalSeconds);
                jsonObj.OperatorFlagsAvailable = ImagesFolderAvailable(fileSystemProvider, config.BaseStationSettings.OperatorFlagsFolder);
                jsonObj.SilhouettesAvailable = ImagesFolderAvailable(fileSystemProvider, config.BaseStationSettings.SilhouettesFolder);
                jsonObj.FromDate = FormatReportDate(parameters.Date?.LowerValue);
                jsonObj.ToDate = FormatReportDate(parameters.Date?.UpperValue);

                var jsonText = JsonConvert.SerializeObject(jsonObj);
                result = new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(jsonText, Encoding.UTF8, MimeType.Json),
                };
                result.Headers.CacheControl = new CacheControlHeaderValue() {
                    MaxAge = TimeSpan.Zero,
                    NoCache = true,
                    NoStore = true,
                    MustRevalidate = true,
                };
            }

            return result ?? new HttpResponseMessage(HttpStatusCode.Forbidden);
        }

        private ReportParameters ExtractV2Parameters()
        {
            var result = new ReportParameters() {
                FromRow = QueryInt("fromrow", -1),
                ToRow = QueryInt("torow", -1),
                SortField1 = QueryString("sort1"),
                SortField2 = QueryString("sort2"),
                SortAscending1 = QueryString("sort1dir", toUpperCase: true) != "DESC",
                SortAscending2 = QueryString("sort2dir", toUpperCase: true) != "DESC",
                UseAlternateCallsigns = QueryBool("altCall", false),
            };

            foreach(var kvp in PipelineContext.Request.Query) {
                var name = (kvp.Key ?? "").ToUpper();
                var value = kvp.Value == null || kvp.Value.Length < 1 ? "" : kvp.Value[0] ?? "";

                if(name.StartsWith("CALL-"))        result.Callsign =       DecodeStringFilter(name, value);
                else if(name.StartsWith("DATE-"))   result.Date =           DecodeDateRangeFilter(result.Date, name, value);
                else if(name.StartsWith("ICAO-"))   result.Icao =           DecodeStringFilter(name, value);
                else if(name.StartsWith("REG-"))    result.Registration =   DecodeStringFilter(name, value);
            }
            if(result.Date != null) {
                result.Date.NormaliseRange();
            }

            return result;
        }

        private bool QueryBool(string key, bool defaultValue)
        {
            return QueryInt(key, defaultValue ? 1 : 0) != 0;
        }

        private int QueryInt(string key, int defaultValue)
        {
            if(!int.TryParse(PipelineRequest.Query[key], out int result)) {
                result = defaultValue;
            }

            return result;
        }

        private string QueryString(string key, bool toUpperCase = false)
        {
            var result = PipelineRequest.Query[key];
            if(toUpperCase) {
                result = result?.ToUpper();
            }

            return result;
        }

        private char DecodeFilter<T>(T filter, string name)
            where T: Filter
        {
            var result = '\0';

            for(var i = name.Length - 2;i < name.Length;++i) {
                var ch = name[i];
                switch(name[i]) {
                    case 'L':
                    case 'U':   filter.Condition = FilterCondition.Between;     result = ch; break;
                    case 'S':   filter.Condition = FilterCondition.StartsWith;  result = ch; break;
                    case 'E':   filter.Condition = FilterCondition.EndsWith;    result = ch; break;
                    case 'C':   filter.Condition = FilterCondition.Contains;    result = ch; break;
                    case 'Q':   filter.Condition = FilterCondition.Equals;      result = ch; break;
                    case 'N':   filter.ReverseCondition = true; break;
                }
            }

            return result;
        }

        private FilterString DecodeStringFilter(string name, string value)
        {
            var result = new FilterString();
            DecodeFilter(result, name);
            result.Value = value;

            return result;
        }

        private FilterRange<DateTime> DecodeDateRangeFilter(FilterRange<DateTime> filterRange, string name, string value)
        {
            if(filterRange == null) filterRange = new FilterRange<DateTime>();
            var conditionChar = DecodeFilter(filterRange, name);
            switch(conditionChar) {
                case 'L':   filterRange.LowerValue = QueryNDateTime(value); break;
                case 'U':   filterRange.UpperValue = QueryNDateTime(value); break;
                default:    filterRange.Condition = FilterCondition.Missing; break;
            }

            return filterRange;
        }

        private DateTime? QueryNDateTime(string text)
        {
            DateTime? result = null;
            if(!String.IsNullOrEmpty(text)) {
                if(DateTime.TryParseExact(text, "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) {
                    result = date;
                }
            }

            return result;
        }

        private void LimitDatesWhenNoStrongCriteriaPresent(IClock clock, SearchBaseStationCriteria criteria, bool isInternetRequest)
        {
            if(criteria.Callsign == null && criteria.Registration == null && criteria.Icao == null) {
                if(criteria.Date == null) {
                    criteria.Date = new FilterRange<DateTime>() { Condition = FilterCondition.Between };
                }

                const int defaultDayCount = 7;
                var now = clock.UtcNow;

                var fromIsMissing = criteria.Date.LowerValue == null;
                var toIsMissing = criteria.Date.UpperValue == null;

                if(fromIsMissing && toIsMissing) {
                    criteria.Date.UpperValue = now.Date;
                    toIsMissing = false;
                }

                if(fromIsMissing) {
                    criteria.Date.LowerValue = criteria.Date.UpperValue.Value.AddDays(-defaultDayCount);
                } else if(toIsMissing) {
                    criteria.Date.UpperValue = criteria.Date.LowerValue.Value.AddDays(defaultDayCount);
                } else if(isInternetRequest && (criteria.Date.UpperValue.Value - criteria.Date.LowerValue.Value).TotalDays > defaultDayCount) {
                    criteria.Date.UpperValue = criteria.Date.LowerValue.Value.AddDays(defaultDayCount);
                }
            }
        }

        private string FormatReportDate(DateTime? date)
        {
            string result = null;
            if(date != null && date.Value.Year != DateTime.MinValue.Year && date.Value.Year != DateTime.MaxValue.Year) {
                result = date.Value.Date.ToString("yyyy-MM-dd");
            }

            return result;
        }

        private bool ImagesFolderAvailable(IFileSystemProvider fileSystemProvider, string configFolder)
        {
            return !String.IsNullOrEmpty(configFolder) && fileSystemProvider.DirectoryExists(configFolder);
        }
    }
}
