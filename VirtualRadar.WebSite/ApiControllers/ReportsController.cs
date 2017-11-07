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
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Interface.WebSite;
using SharedState = VirtualRadar.WebSite.ApiControllers.ReportsControllerSharedState;

namespace VirtualRadar.WebSite.ApiControllers
{
    /// <summary>
    /// Serves results of report requests.
    /// </summary>
    public class ReportsController : PipelineApiController
    {
        [HttpGet]
        [Route("ReportRows.json")]                      // V2 route
        public HttpResponseMessage ReportRowsV2()
        {
            HttpResponseMessage result = null;

            var config = SharedState.SharedConfiguration.Get();
            if(PipelineRequest.IsLocalOrLan || config.InternetClientSettings.CanRunReports) {
                ReportRowsJson jsonObj = null;
                var expectedJsonType = ExpectedJsonType();
                var startTime = SharedState.Clock.UtcNow;

                try {
                    var parameters = ExtractV2Parameters();
                    LimitDatesWhenNoStrongCriteriaPresent(parameters, PipelineRequest.IsInternet);
                    if(parameters?.Date?.UpperValue?.Year < 9999) {
                        parameters.Date.UpperValue = parameters.Date.UpperValue.Value.AddDays(1).AddMilliseconds(-1);
                    }

                    switch(parameters.ReportType) {
                        case "DATE":    jsonObj = CreateManyAircraftReport(parameters, config); break;
                        //case "ICAO":    jsonObj = CreateSingleAircraftReport(args, parameters, true); break;
                        //case "REG":     jsonObj = CreateSingleAircraftReport(args, parameters, false); break;
                        default:        throw new NotImplementedException();
                    }

                    if(jsonObj != null) {
                        jsonObj.GroupBy = parameters.SortField1 ?? parameters.SortField2 ?? "";
                    };
                } catch(Exception ex) {
                    var log = Factory.Singleton.Resolve<ILog>().Singleton;
                    log.WriteLine($"An exception was encountered during the processing of a report: {ex}");
                    jsonObj = EnsureJsonObjExists(jsonObj, expectedJsonType);
                    jsonObj.ErrorText = $"An exception was encounted during the processing of the report, see log for full details: {ex.Message}";
                }

                jsonObj = EnsureJsonObjExists(jsonObj, expectedJsonType);
                jsonObj.ProcessingTime = String.Format("{0:N3}", (SharedState.Clock.UtcNow - startTime).TotalSeconds);
                jsonObj.OperatorFlagsAvailable = ImagesFolderAvailable(SharedState.FileSystem, config.BaseStationSettings.OperatorFlagsFolder);
                jsonObj.SilhouettesAvailable = ImagesFolderAvailable(SharedState.FileSystem, config.BaseStationSettings.SilhouettesFolder);

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

        /// <summary>
        /// Returns the type of JSON that the report implies should be returned. The function tries
        /// not to throw any exceptions.
        /// </summary>
        /// <returns></returns>
        private Type ExpectedJsonType()
        {
            var result = typeof(AircraftReportJson);

            var reportType = PipelineRequest.Query["rep"];
            switch((reportType ?? "").ToUpper()) {
                case "DATE":    result = typeof(FlightReportJson); break;
            }

            return result;
        }

        /// <summary>
        /// Creates and returns a new instance corresponding to the type passed across if the instance passed in is null.
        /// </summary>
        /// <param name="jsonObj"></param>
        /// <param name="expectedJsonType"></param>
        /// <returns></returns>
        private ReportRowsJson EnsureJsonObjExists(ReportRowsJson jsonObj, Type expectedJsonType)
        {
            return jsonObj ?? (ReportRowsJson)Activator.CreateInstance(expectedJsonType);
        }

        /// <summary>
        /// Builds up rows for a report that wants information on flights for many aircraft simultaneously.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private FlightReportJson CreateManyAircraftReport(ReportParameters parameters, Configuration config)
        {
            var result = new FlightReportJson() {
                FromDate = FormatReportDate(parameters.Date?.LowerValue),
                ToDate = FormatReportDate(parameters.Date?.UpperValue),
            };

            var hasNonDatabaseCriteria = parameters.IsMilitary != null || parameters.WakeTurbulenceCategory != null || parameters.Species != null;
            var dbFlights = SharedState.BaseStationDatabase.GetFlights(
                parameters,
                hasNonDatabaseCriteria ? -1 : parameters.FromRow,
                hasNonDatabaseCriteria ? -1 : parameters.ToRow,
                parameters.SortField1,
                parameters.SortAscending1,
                parameters.SortField2,
                parameters.SortAscending2
            );

            if(!hasNonDatabaseCriteria) {
                result.CountRows = SharedState.BaseStationDatabase.GetCountOfFlights(parameters);
            } else {
                dbFlights = dbFlights.Where(f => {
                    var matches = f.Aircraft != null;
                    if(matches) {
                        if(parameters.IsMilitary != null) {
                            var codeBlock = SharedState.StandingDataManager.FindCodeBlock(f.Aircraft.ModeS);
                            matches = matches && codeBlock != null && parameters.IsMilitary.Passes(codeBlock.IsMilitary);
                        }
                        if(parameters.Species != null || parameters.WakeTurbulenceCategory != null) {
                            var aircraftType = SharedState.StandingDataManager.FindAircraftType(f.Aircraft.ICAOTypeCode);
                            if(parameters.Species != null) {
                                matches = matches && aircraftType != null && parameters.Species.Passes(aircraftType.Species);
                            }
                            if(parameters.WakeTurbulenceCategory != null) {
                                matches = matches && aircraftType != null && parameters.WakeTurbulenceCategory.Passes(aircraftType.WakeTurbulenceCategory);
                            }
                        }
                    }
                    return matches;
                }).ToList();

                result.CountRows = dbFlights.Count;

                var limit = parameters.ToRow == -1 || parameters.ToRow < parameters.FromRow ? int.MaxValue : (parameters.ToRow - Math.Max(0, parameters.FromRow)) + 1;
                var offset = parameters.FromRow < 0 ? 0 : parameters.FromRow;
                dbFlights = dbFlights.Skip(offset).Take(limit).ToList();
            }

            TranscribeDatabaseRecordsToJson(dbFlights, result.Flights, result.Aircraft, result.Airports, result.Routes, parameters, config);

            return result;
        }

        private void TranscribeDatabaseRecordsToJson(List<BaseStationFlight> dbFlights, List<ReportFlightJson> jsonFlights, List<ReportAircraftJson> jsonAircraft, List<ReportAirportJson> jsonAirports, List<ReportRouteJson> jsonRoutes, ReportParameters parameters, Configuration config)
        {
            var aircraftIdMap = new Dictionary<int, int>();
            var airportMap = new Dictionary<string, int>();
            var routeMap = new Dictionary<string, int>();

            var rowNumber = parameters.FromRow < 1 ? 1 : parameters.FromRow + 1;
            foreach(var dbFlight in dbFlights) {
                var jsonFlight = AddReportFlightJson(dbFlight, jsonFlights, ref rowNumber);

                if(jsonAircraft != null) {
                    var dbAircraft = dbFlight.Aircraft;
                    if(dbAircraft == null) {
                        jsonFlight.AircraftIndex = jsonAircraft.Count;
                        jsonAircraft.Add(new ReportAircraftJson() { IsUnknown = true });
                    } else {
                        if(!aircraftIdMap.TryGetValue(dbAircraft.AircraftID, out int aircraftIndex)) {
                            aircraftIndex = jsonAircraft.Count;
                            aircraftIdMap.Add(dbAircraft.AircraftID, aircraftIndex);
                            jsonAircraft.Add(CreateReportAircraftJson(dbAircraft, config));
                        }
                        jsonFlight.AircraftIndex = aircraftIndex;
                    }
                }

                var routeIndex = -1;
                if(!String.IsNullOrEmpty(dbFlight.Callsign) && !routeMap.TryGetValue(dbFlight.Callsign, out routeIndex)) {
                    var operatorCode = dbFlight.Aircraft?.OperatorFlagCode;
                    foreach(var routeCallsign in SharedState.CallsignParser.GetAllRouteCallsigns(dbFlight.Callsign, operatorCode)) {
                        var sdmRoute = SharedState.StandingDataManager.FindRoute(routeCallsign);
                        if(sdmRoute == null) routeIndex = -1;
                        else {
                            var jsonRoute = new ReportRouteJson() {
                                FromIndex = BuildAirportJson(sdmRoute.From, airportMap, jsonAirports, config.GoogleMapSettings.PreferIataAirportCodes),
                                ToIndex = BuildAirportJson(sdmRoute.To, airportMap, jsonAirports, config.GoogleMapSettings.PreferIataAirportCodes),
                            };
                            foreach(var stopover in sdmRoute.Stopovers) {
                                int index = BuildAirportJson(stopover, airportMap, jsonAirports, config.GoogleMapSettings.PreferIataAirportCodes);
                                if(index != -1) jsonRoute.StopoversIndex.Add(index);
                            }

                            routeIndex = jsonRoutes.Count;
                            jsonRoutes.Add(jsonRoute);
                            routeMap.Add(dbFlight.Callsign, routeIndex);

                            break;
                        }
                    }
                }
                jsonFlight.RouteIndex = routeIndex;
            }
        }

        /// <summary>
        /// Creates a JSON representation of the database flight and adds it to an existing list of flights.
        /// </summary>
        /// <param name="flight"></param>
        /// <param name="flightList"></param>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        private ReportFlightJson AddReportFlightJson(BaseStationFlight flight, List<ReportFlightJson> flightList, ref int rowNumber)
        {
            var result = new ReportFlightJson() {
                RowNumber =             rowNumber++,
                Callsign =              flight.Callsign,
                StartTime =             flight.StartTime,
                EndTime =               flight.EndTime.GetValueOrDefault(),
                FirstAltitude =         flight.FirstAltitude.GetValueOrDefault(),
                FirstGroundSpeed =      (int)flight.FirstGroundSpeed.GetValueOrDefault(),
                FirstIsOnGround =       flight.FirstIsOnGround,
                FirstLatitude =         flight.FirstLat.GetValueOrDefault(),
                FirstLongitude =        flight.FirstLon.GetValueOrDefault(),
                FirstSquawk =           flight.FirstSquawk.GetValueOrDefault(),
                FirstTrack =            flight.FirstTrack.GetValueOrDefault(),
                FirstVerticalRate =     flight.FirstVerticalRate.GetValueOrDefault(),
                HadAlert =              flight.HadAlert,
                HadEmergency =          flight.HadEmergency,
                HadSpi =                flight.HadSpi,
                LastAltitude =          flight.LastAltitude.GetValueOrDefault(),
                LastGroundSpeed =       (int)flight.LastGroundSpeed.GetValueOrDefault(),
                LastIsOnGround =        flight.LastIsOnGround,
                LastLatitude =          flight.LastLat.GetValueOrDefault(),
                LastLongitude =         flight.LastLon.GetValueOrDefault(),
                LastSquawk =            flight.LastSquawk.GetValueOrDefault(),
                LastTrack =             flight.LastTrack.GetValueOrDefault(),
                LastVerticalRate =      flight.LastVerticalRate.GetValueOrDefault(),
                NumADSBMsgRec =         flight.NumADSBMsgRec.GetValueOrDefault(),
                NumModeSMsgRec =        flight.NumModeSMsgRec.GetValueOrDefault(),
                NumPosMsgRec =          flight.NumPosMsgRec.GetValueOrDefault(),
            };
            flightList.Add(result);

            return result;
        }

        /// <summary>
        /// Creates the JSON representation of an aircraft.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        private ReportAircraftJson CreateReportAircraftJson(BaseStationAircraft aircraft, Configuration config)
        {
            var result = new ReportAircraftJson() {
                AircraftClass =     aircraft.AircraftClass,
                AircraftId =        aircraft.AircraftID,
                CofACategory =      aircraft.CofACategory,
                CofAExpiry =        aircraft.CofAExpiry,
                Country =           aircraft.Country,
                CurrentRegDate =    aircraft.CurrentRegDate,
                DeRegDate =         aircraft.DeRegDate,
                FirstRegDate =      aircraft.FirstRegDate,
                GenericName =       aircraft.GenericName,
                IcaoTypeCode =      aircraft.ICAOTypeCode,
                InfoUrl =           aircraft.InfoUrl,
                Interested =        aircraft.Interested,
                Manufacturer =      aircraft.Manufacturer,
                Icao =              aircraft.ModeS,
                ModeSCountry =      aircraft.ModeSCountry,
                MTOW =              aircraft.MTOW,
                OperatorFlagCode =  aircraft.OperatorFlagCode,
                OwnershipStatus =   aircraft.OwnershipStatus,
                PictureUrl1 =       aircraft.PictureUrl1,
                PictureUrl2 =       aircraft.PictureUrl2,
                PictureUrl3 =       aircraft.PictureUrl3,
                PopularName =       aircraft.PopularName,
                PreviousId =        aircraft.PreviousID,
                Registration =      aircraft.Registration,
                RegisteredOwners =  aircraft.RegisteredOwners,
                SerialNumber =      aircraft.SerialNo,
                Status =            aircraft.Status,
                TotalHours =        aircraft.TotalHours,
                Type =              aircraft.Type,
                Notes =             aircraft.UserNotes,
                YearBuilt =         aircraft.YearBuilt,
            };

            if(PipelineRequest.IsLocalOrLan || config.InternetClientSettings.CanShowPictures) {
                try {
                    var pictureDetails = SharedState.AircraftPictureManager.FindPicture(SharedState.PictureFolderCache, aircraft.ModeS, aircraft.Registration);
                    if(pictureDetails != null) {
                        result.HasPicture =     true;
                        result.PictureWidth =   pictureDetails.Width;
                        result.PictureHeight =  pictureDetails.Height;
                    }
                } catch(Exception ex) {
                    try {
                        var log = Factory.Singleton.Resolve<ILog>().Singleton;
                        log.WriteLine($"Caught exception when fetching picture for {aircraft.ModeS}/{aircraft.Registration} for a report: {ex.ToString()}");
                    } catch {
                    }
                }
            }

            var aircraftType = String.IsNullOrEmpty(aircraft.ICAOTypeCode) ? null : SharedState.StandingDataManager.FindAircraftType(aircraft.ICAOTypeCode);
            if(aircraftType != null) {
                result.WakeTurbulenceCategory = (int)aircraftType.WakeTurbulenceCategory;
                result.Engines =                aircraftType.Engines;
                result.EngineType =             (int)aircraftType.EngineType;
                result.EnginePlacement =        (int)aircraftType.EnginePlacement;
                result.Species =                (int)aircraftType.Species;
            }

            var codeBlock = String.IsNullOrEmpty(aircraft.ModeS) ? null : SharedState.StandingDataManager.FindCodeBlock(aircraft.ModeS);
            if(codeBlock != null) {
                result.Military = codeBlock.IsMilitary;
            }

            return result;
        }

        private int BuildAirportJson(Airport sdmAirport, Dictionary<string, int> airportMap, List<ReportAirportJson> jsonList, bool preferIataCodes)
        {
            var result = -1;

            if(sdmAirport != null) {
                var code = preferIataCodes ? String.IsNullOrEmpty(sdmAirport.IataCode) ? sdmAirport.IcaoCode : sdmAirport.IataCode
                                           : String.IsNullOrEmpty(sdmAirport.IcaoCode) ? sdmAirport.IataCode : sdmAirport.IcaoCode;
                if(!String.IsNullOrEmpty(code)) {
                    if(!airportMap.TryGetValue(code, out result)) {
                        result = jsonList.Count;
                        jsonList.Add(new ReportAirportJson() {
                            Code = code,
                            Name = Describe.Airport(sdmAirport, preferIataCodes, showCode: false, showName: true, showCountry: true)
                        });
                        airportMap.Add(code, result);
                    }
                }
            }

            return result;
        }

        private ReportParameters ExtractV2Parameters()
        {
            var result = new ReportParameters() {
                ReportType = QueryString("rep", toUpperCase: true),
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

                if(name.StartsWith("CALL-"))        result.Callsign =               DecodeStringFilter(name, value);
                else if(name.StartsWith("DATE-"))   result.Date =                   DecodeDateRangeFilter(result.Date, name, value);
                else if(name.StartsWith("ICAO-"))   result.Icao =                   DecodeStringFilter(name, value);
                else if(name.StartsWith("REG-"))    result.Registration =           DecodeStringFilter(name, value);
                else if(name.StartsWith("MIL-"))    result.IsMilitary =             DecodeBoolFilter(name, value);
                else if(name.StartsWith("WTC-"))    result.WakeTurbulenceCategory = DecodeEnumFilter<WakeTurbulenceCategory>(name, value);
                else if(name.StartsWith("SPC-"))    result.Species =                DecodeEnumFilter<Species>(name, value);
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

        private FilterBool DecodeBoolFilter(string name, string value)
        {
            FilterBool result = null;

            if(!String.IsNullOrEmpty(value)) {
                result = new FilterBool() {
                    Value = value != "0" && !value.Equals("false", StringComparison.OrdinalIgnoreCase)
                };
                DecodeFilter(result, name);
            }

            return result;
        }

        private FilterEnum<T> DecodeEnumFilter<T>(string name, string value)
            where T: struct, IComparable
        {
            var result = new FilterEnum<T>();
            DecodeFilter(result, name);

            var decoded = false;
            if(!String.IsNullOrEmpty(value)) {
                var number = QueryNInt(value);
                if(number != null && Enum.IsDefined(typeof(T), number)) {
                    result.Value = (T)((object)number.Value);
                    decoded = true;
                }
            }
            if(!decoded) {
                result = null;
            }

            return result;
        }

        private DateTime? QueryNDateTime(string text)
        {
            DateTime? result = null;
            if(!String.IsNullOrEmpty(text)) {
                if(DateTime.TryParseExact(text, "yyyy-M-d", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) {
                    result = date;
                }
            }

            return result;
        }

        protected int? QueryNInt(string text)
        {
            int? result = null;
            if(!String.IsNullOrEmpty(text)) {
                if(int.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out int value)) {
                    result = value;
                }
            }

            return result;
        }

        private void LimitDatesWhenNoStrongCriteriaPresent(SearchBaseStationCriteria criteria, bool isInternetRequest)
        {
            if(criteria.Callsign == null && criteria.Registration == null && criteria.Icao == null) {
                if(criteria.Date == null) {
                    criteria.Date = new FilterRange<DateTime>() { Condition = FilterCondition.Between };
                }

                const int defaultDayCount = 7;
                var now = SharedState.Clock.UtcNow;

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
