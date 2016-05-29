// Copyright © 2010 onwards, Andrew Whewell
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
using VirtualRadar.Interface.WebServer;
using System.Net;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Interface;
using Test.Framework;
using Moq;
using System.Globalization;
using System.Linq.Expressions;
using System.Collections;
using System.IO;
using VirtualRadar.Interface.StandingData;
using System.Reflection;
using System.Threading;
using System.Web;

namespace Test.VirtualRadar.WebSite
{
    // This partial class contains all of the specific tests on the aircraft list JSON files.
    public partial class WebSiteTests
    {
        #region Pages
        private const string AircraftListPage = "/AircraftList.json";
        private const string FlightSimListPage = "/FlightSimList.json";
        #endregion

        #region Private Factory - AircraftListAddress
        /// <summary>
        /// A private class that simplifies the initialisation of a request for
        /// an aircraft list, typically from AircraftList.json
        /// </summary>
        class AircraftListAddress
        {
            private Mock<IRequest> _Request;

            public int? ReceiverId { get; set; }
            public string Page { get; set; }
            public IAircraftList AircraftList { get; set; }
            public double? BrowserLatitude { get; set; }
            public double? BrowserLongitude { get; set; }
            public List<int> PreviousAircraft { get; private set; }
            public long PreviousDataVersion { get; set; }
            public long ServerTimeJavaScriptTicks { get; set; }
            public bool ResendTrails { get; set; }
            public bool SendTrail { get; set; }
            public bool ShowShortTrail { get; set; }
            public bool ShowAltitudeTrail { get; set; }
            public bool ShowSpeedTrail { get; set; }
            public AircraftListFilter Filter { get; set; }
            public List<KeyValuePair<string, string>> SortBy { get; private set; }
            public string JsonpCallback { get; set; }
            public int? SelectedAircraftId { get; set; }

            public AircraftListAddress(Mock<IRequest> request)
            {
                _Request = request;

                Page = AircraftListPage;
                PreviousAircraft = new List<int>();
                PreviousDataVersion = -1L;
                ServerTimeJavaScriptTicks = -1L;
                SortBy = new List<KeyValuePair<string, string>>();
            }

            public string Address
            {
                get
                {
                    Dictionary<string, string> queryValues = new Dictionary<string,string>();

                    if(ReceiverId != null) queryValues.Add("feed", ReceiverId.Value.ToString(CultureInfo.InvariantCulture));
                    if(BrowserLatitude != null) queryValues.Add("lat", BrowserLatitude.Value.ToString(CultureInfo.InvariantCulture));
                    if(BrowserLongitude != null) queryValues.Add("lng", BrowserLongitude.Value.ToString(CultureInfo.InvariantCulture));
                    if(PreviousDataVersion > -1) queryValues.Add("ldv", PreviousDataVersion.ToString(CultureInfo.InvariantCulture));
                    if(ServerTimeJavaScriptTicks > -1) queryValues.Add("stm", ServerTimeJavaScriptTicks.ToString(CultureInfo.InvariantCulture));
                    if(ResendTrails) queryValues.Add("refreshTrails", "1");
                    if(SelectedAircraftId != null) queryValues.Add("selAc", SelectedAircraftId.ToString());

                    if(SendTrail) {
                        string trailCode;
                        if(ShowAltitudeTrail)   trailCode = ShowShortTrail ? "sa" : "fa";
                        else if(ShowSpeedTrail) trailCode = ShowShortTrail ? "ss" : "fs";
                        else                    trailCode = ShowShortTrail ? "s"  : "f";
                        queryValues.Add("trFmt", trailCode);
                    }

                    if(Filter != null) Filter.AddToQueryValuesMap(queryValues);

                    if(SortBy.Count > 0) AddSortToQueryValuesMap(queryValues);

                    if(JsonpCallback != null) queryValues.Add("callback", JsonpCallback);

                    var result = BuildUrl(Page, queryValues);

                    var headers = new System.Collections.Specialized.NameValueCollection();
                    _Request.Setup(p => p.Headers).Returns(headers);

                    var previousAircraft = new StringBuilder();
                    if(PreviousAircraft.Count > 0) {
                        for(int i = 0;i < PreviousAircraft.Count;++i) {
                            if(i > 0) previousAircraft.Append("%2C");
                            previousAircraft.Append(PreviousAircraft[i]);
                        }
                    }
                    headers.Add("X-VirtualRadarServer-AircraftIds", previousAircraft.ToString());

                    return result;
                }
            }

            private void AddSortToQueryValuesMap(Dictionary<string, string> queryValues)
            {
                for(int i = 0;i < Math.Min(2, SortBy.Count);++i) {
                    var sortBy = SortBy[i];
                    queryValues.Add(String.Format("sortBy{0}", i + 1), sortBy.Key);
                    queryValues.Add(String.Format("sortOrder{0}", i + 1), sortBy.Value);
                }
            }
        }
        #endregion

        #region PrivateClass - AircraftListFilter
        enum FilterCondition
        {
            Between,
            Equals,
            Contains,
            StartsWith,
            EndsWith,
        }

        /// <summary>
        /// The base for the aircraft filter helpers.
        /// </summary>
        abstract class Filter
        {
            public bool Reversed { get; set; }

            public Filter(bool reversed)
            {
                Reversed = reversed;
            }

            protected string FilterName(string filterName, char suffix = '\0')
            {
                var result = filterName;
                if(suffix != '\0') result = result + suffix;
                if(Reversed) result += 'N';

                return result;
            }

            public abstract void AddQueryValues(string filterName, Dictionary<string, string> queryValues);

            public abstract Type GetPropertyType();
        }

        /// <summary>
        /// A private class that describes the parameters for a numeric filter.
        /// </summary>
        class NumericFilter<T> : Filter
            where T: struct
        {
            public T? Lower { get; set; }
            public T? Upper { get; set; }

            public NumericFilter(T? lower, T? upper, bool reversed) : base(reversed)
            {
                Lower = lower;
                Upper = upper;
            }

            public override void AddQueryValues(string filterName, Dictionary<string, string> queryValues)
            {
                if(Lower != null) queryValues.Add(FilterName(filterName, 'L'), Convert.ToString(Lower.Value, CultureInfo.InvariantCulture));
                if(Upper != null) queryValues.Add(FilterName(filterName, 'U'), Convert.ToString(Upper.Value, CultureInfo.InvariantCulture));
            }

            public override Type GetPropertyType()
            {
                return typeof(T?);
            }
        }

        class DateFilter : Filter
        {
            public DateTime? Lower { get; set; }
            public DateTime? Upper { get; set; }

            public DateFilter(DateTime? lower, DateTime? upper, bool reversed) : base(reversed)
            {
                Lower = lower;
                Upper = upper;
            }

            public override void AddQueryValues(string filterName, Dictionary<string, string> queryValues)
            {
                if(Lower != null) queryValues.Add(FilterName(filterName, 'L'), Lower.Value.ToString("yyyy-MM-dd"));
                if(Upper != null) queryValues.Add(FilterName(filterName, 'U'), Upper.Value.ToString("yyyy-MM-dd"));
            }

            public override Type GetPropertyType()
            {
                return typeof(DateTime?);
            }
        }

        /// <summary>
        /// A private class that describes parameters for a boolean filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class BoolFilter<T> : Filter
        {
            public T Value { get; set; }

            public BoolFilter(T value, bool reversed) : base(reversed)
            {
                Value = value;
            }

            public override void AddQueryValues(string filterName, Dictionary<string, string> queryValues)
            {
                queryValues.Add(FilterName(filterName, 'Q'), Convert.ToInt32(Value, CultureInfo.InvariantCulture).ToString());
            }

            public override Type GetPropertyType()
            {
                return typeof(T);
            }
        }

        /// <summary>
        /// A private class that describes parameters for a string filter.
        /// </summary>
        class StringFilter : Filter
        {
            public string Value { get; set; }
            public FilterCondition Condition { get; set; }

            public StringFilter(string value, FilterCondition condition, bool reversed) : base(reversed)
            {
                Value = value;
                Condition = condition;
            }

            public override void AddQueryValues(string filterName, Dictionary<string, string> queryValues)
            {
                char conditionCharacter;
                switch(Condition) {
                    case FilterCondition.Between:       throw new InvalidOperationException("String filters cannot have the 'Between' condition");
                    case FilterCondition.Contains:      conditionCharacter = 'C'; break;
                    case FilterCondition.EndsWith:      conditionCharacter = 'E'; break;
                    case FilterCondition.Equals:        conditionCharacter = 'Q'; break;
                    case FilterCondition.StartsWith:    conditionCharacter = 'S'; break;
                    default:                            throw new NotImplementedException();
                }

                queryValues.Add(FilterName(filterName, conditionCharacter), Value);
            }

            public override Type GetPropertyType()
            {
                return typeof(string);
            }
        }

        /// <summary>
        /// A private class that simplifies the initialisation of a request for the filter portion of
        /// a request for an aircraft list JSON file - see <see cref="AircraftListAddress"/>.
        /// </summary>
        class AircraftListFilter
        {
            public StringFilter                         Airport { get; set; }
            public NumericFilter<int>                   Altitude { get; set; }
            public StringFilter                         Callsign { get; set; }
            public StringFilter                         Icao24Country { get; set; }
            public NumericFilter<double>                Distance { get; set; }
            public BoolFilter<EngineType>               EngineType { get; set; }
            public StringFilter                         Icao24 { get; set; }
            public BoolFilter<bool>                     IsMilitary { get; set; }
            public BoolFilter<bool>                     IsInteresting { get; set; }
            public BoolFilter<bool>                     MustTransmitPosition { get; set; }
            public StringFilter                         Operator { get; set; }
            public StringFilter                         OperatorIcao { get; set; }
            public StringFilter                         Registration { get; set; }
            public BoolFilter<Species>                  Species { get; set; }
            public NumericFilter<int>                   Squawk { get; set; }
            public StringFilter                         Type { get; set; }
            public BoolFilter<WakeTurbulenceCategory>   WakeTurbulenceCategory { get; set; }
            public StringFilter                         UserTag { get; set; }
            public Pair<Coordinate>                     PositionWithin { get; set; }

            public void AddToQueryValuesMap(Dictionary<string, string> queryValues)
            {
                if(Airport != null)                 Airport.AddQueryValues("fAir", queryValues);
                if(Altitude != null)                Altitude.AddQueryValues("fAlt", queryValues);
                if(Callsign != null)                Callsign.AddQueryValues("fCall", queryValues);
                if(Icao24Country != null)           Icao24Country.AddQueryValues("fCou", queryValues);
                if(Distance != null)                Distance.AddQueryValues("fDst", queryValues);
                if(EngineType != null)              EngineType.AddQueryValues("fEgt", queryValues);
                if(Icao24 != null)                  Icao24.AddQueryValues("fIco", queryValues);
                if(IsMilitary != null)              IsMilitary.AddQueryValues("fMil", queryValues);
                if(IsInteresting != null)           IsInteresting.AddQueryValues("fInt", queryValues);
                if(MustTransmitPosition != null)    MustTransmitPosition.AddQueryValues("fNoPos", queryValues);
                if(Operator != null)                Operator.AddQueryValues("fOp", queryValues);
                if(OperatorIcao != null)            OperatorIcao.AddQueryValues("fOpIcao", queryValues);
                if(Registration != null)            Registration.AddQueryValues("fReg", queryValues);
                if(Species != null)                 Species.AddQueryValues("fSpc", queryValues);
                if(Squawk != null)                  Squawk.AddQueryValues("fSqk", queryValues);
                if(Type != null)                    Type.AddQueryValues("fTyp", queryValues);
                if(UserTag != null)                 UserTag.AddQueryValues("fUt", queryValues);
                if(WakeTurbulenceCategory != null)  WakeTurbulenceCategory.AddQueryValues("fWtc", queryValues);

                if(PositionWithin != null) {
                    queryValues.Add("fNBnd", PositionWithin.First.Latitude.ToString(CultureInfo.InvariantCulture));
                    queryValues.Add("fWBnd", PositionWithin.First.Longitude.ToString(CultureInfo.InvariantCulture));
                    queryValues.Add("fSBnd", PositionWithin.Second.Latitude.ToString(CultureInfo.InvariantCulture));
                    queryValues.Add("fEBnd", PositionWithin.Second.Longitude.ToString(CultureInfo.InvariantCulture));
                }
            }

            public static bool IsNumericFilter(PropertyInfo filterProperty)
            {
                return filterProperty.PropertyType.IsGenericType && filterProperty.PropertyType.GetGenericTypeDefinition() == typeof(NumericFilter<>);
            }

            public static bool IsStringFilter(PropertyInfo filterProperty)
            {
                return typeof(StringFilter).IsAssignableFrom(filterProperty.PropertyType);
            }

            public static bool IsBoolFilter(PropertyInfo filterProperty)
            {
                return filterProperty.PropertyType.IsGenericType && filterProperty.PropertyType.GetGenericTypeDefinition() == typeof(BoolFilter<>);
            }

            public void SetStringProperty(PropertyInfo filterProperty, string value, FilterCondition condition = FilterCondition.Contains, bool reversed = false)
            {
                var filter = new StringFilter(value, condition, reversed);
                filterProperty.SetValue(this, filter, null);
            }

            public void SetValueProperty<T>(PropertyInfo filterProperty, T value, FilterCondition condition = FilterCondition.Equals, bool reversed = false)
            {
                Filter filter;
                if(IsBoolFilter(filterProperty)) {
                    var filterType = filterProperty.PropertyType.GetGenericArguments()[0];
                    filter = (Filter)Activator.CreateInstance(filterProperty.PropertyType, TestUtilities.ChangeType(value, filterType, CultureInfo.InvariantCulture), reversed);
                } else if(IsStringFilter(filterProperty)) {
                    filter = (Filter)Activator.CreateInstance(filterProperty.PropertyType, value.ToString(), condition, reversed);
                } else {
                    throw new InvalidOperationException("You can't set a numeric filter to a single value");
                }

                filterProperty.SetValue(this, filter, null);
            }

            public void SetRangeProperty<T>(PropertyInfo filterProperty, T? lower, T? upper, bool reversed = false)
                where T: struct
            {
                var filter = Activator.CreateInstance(filterProperty.PropertyType, lower, upper, reversed);
                filterProperty.SetValue(this, filter, null);
            }
        }
        #endregion

        #region Helper Methods
        private void AddBlankAircraft(int count, int listIndex = 0)
        {
            AddBlankAircraft(_AircraftLists[listIndex], count);
        }

        private void AddBlankFlightSimAircraft(int count)
        {
            AddBlankAircraft(_FlightSimulatorAircraft, count);
        }

        private void AddBlankAircraft(List<IAircraft> list, int count)
        {
            DateTime firstSeen = new DateTime(2010, 1, 1, 12, 0, 0);
            for(int i = 0;i < count;++i) {
                var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
                var stopovers = new List<string>();
                aircraft.Setup(r => r.Stopovers).Returns(stopovers);
                list.Add(aircraft.Object);
                list[i].UniqueId = i;
                list[i].FirstSeen = firstSeen.AddSeconds(-i);  // <-- if no sort order is specified then it should default to sorting by FirstSeen in descending order
            }
        }
        #endregion

        #region Basic aircraft list tests
        [TestMethod]
        public void WebSite_BaseStationAircraftList_Returns_Empty_AircraftListJson_When_BaseStationAircraftList_Is_Empty()
        {
            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(0, json.Aircraft.Count);
            Assert.AreEqual(0, json.AvailableAircraft);

            Assert.AreEqual(MimeType.Json, _Response.Object.MimeType);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Returns_Correct_Aircraft_List_When_Explicit_Feed_Requested()
        {
            AddBlankAircraft(1, listIndex: 1);
            _AircraftListAddress.ReceiverId = 2;

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(1, json.Aircraft.Count);
            Assert.AreEqual(1, json.AvailableAircraft);
            Assert.AreEqual(2, json.SourceFeedId);

            Assert.AreEqual(MimeType.Json, _Response.Object.MimeType);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Returns_Default_Aircraft_List_When_No_Feed_Requested()
        {
            AddBlankAircraft(1, listIndex: 1);
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 2;

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(1, json.Aircraft.Count);
            Assert.AreEqual(1, json.AvailableAircraft);
            Assert.AreEqual(2, json.SourceFeedId);

            Assert.AreEqual(MimeType.Json, _Response.Object.MimeType);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Returns_Default_AircraftListJson_When_Requested_Feed_Does_Not_Exist()
        {
            AddBlankAircraft(1, listIndex: 0);
            _AircraftListAddress.ReceiverId = 99;

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(1, json.Aircraft.Count);
            Assert.AreEqual(1, json.AvailableAircraft);
            Assert.AreEqual(1, json.SourceFeedId);

            Assert.AreEqual(MimeType.Json, _Response.Object.MimeType);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Returns_Full_List_Of_Feeds()
        {
            foreach(var feedExists in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _AircraftListAddress.ReceiverId = feedExists ? 1 : 99;
                _ReceiverPathways[0].Setup(r => r.Name).Returns("Feed 1");
                _ReceiverPathways[1].Setup(r => r.Name).Returns("Feed 2");

                var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

                Assert.AreEqual(2, json.Feeds.Count);

                var feed = json.Feeds[0];
                Assert.AreEqual(1, feed.UniqueId);
                Assert.AreEqual("Feed 1", feed.Name);

                feed = json.Feeds[1];
                Assert.AreEqual(2, feed.UniqueId);
                Assert.AreEqual("Feed 2", feed.Name);
            }
        }

        [TestMethod]
        public void WebSite_FlightSimAircraftList_Returns_Empty_AircraftListJson_When_FlightSimAircraftList_Is_Empty()
        {
            _AircraftListAddress.Page = FlightSimListPage;

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(0, json.Aircraft.Count);
            Assert.AreEqual(0, json.AvailableAircraft);

            Assert.AreEqual(MimeType.Json, _Response.Object.MimeType);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Responds_To_Request_For_JSONP_Correctly()
        {
            _AircraftListAddress.JsonpCallback = "jsonpfunc";

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address, false, "jsonpfunc");
            Assert.AreEqual(0, json.Aircraft.Count);
            Assert.AreEqual(0, json.AvailableAircraft);

            Assert.AreEqual(MimeType.Json, _Response.Object.MimeType);
        }

        [TestMethod]
        public void WebSite_FlightSimAircraftList_Responds_To_Request_For_JSONP_Correctly()
        {
            _AircraftListAddress.Page = FlightSimListPage;
            _AircraftListAddress.JsonpCallback = "jsonpfunc";

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address, false, "jsonpfunc");
            Assert.AreEqual(0, json.Aircraft.Count);
            Assert.AreEqual(0, json.AvailableAircraft);

            Assert.AreEqual(MimeType.Json, _Response.Object.MimeType);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Adds_Cache_Control_Header()
        {
            SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address, false);
            _Response.Verify(r => r.AddHeader("Cache-Control", "max-age=0, no-cache, no-store, must-revalidate"), Times.Once());
        }

        [TestMethod]
        public void WebSite_FlightSimAircraftList_Adds_Cache_Control_Header()
        {
            _AircraftListAddress.Page = FlightSimListPage;
            SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address, false);
            _Response.Verify(r => r.AddHeader("Cache-Control", "max-age=0, no-cache, no-store, must-revalidate"), Times.Once());
        }
        #endregion

        #region Configuration changes
        [TestMethod]
        public void WebSite_BaseStationAircraftList_Picks_Up_Changes_To_Default_WebSite_Receiver_Id()
        {
            AddBlankAircraft(1, listIndex: 1);
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 1;

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(0, json.Aircraft.Count);

            _Configuration.GoogleMapSettings.WebSiteReceiverId = 2;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(1, json.Aircraft.Count);
        }
        #endregion

        #region List properties
        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sets_Flag_Dimensions_Correctly()
        {
            // These are currently non-configurable
            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(85, json.FlagWidth);
            Assert.AreEqual(20, json.FlagHeight);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Build_Sets_LastDataVersion_Correctly()
        {
            long o1 = 0, o2 = 200;
            FeedHelper.SetupTakeSnapshot(_BaseStationAircraftLists, _AircraftLists, 0, o1, o2);
            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual("200", json.LastDataVersion);

            o2 = 573;
            FeedHelper.SetupTakeSnapshot(_BaseStationAircraftLists, _AircraftLists, 0, o1, o2);
            json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual("573", json.LastDataVersion);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Build_Sets_ServerTime_Correctly()
        {
            DateTime timestamp = new DateTime(2001, 12, 31, 14, 27, 32, 194);
            long o1 = timestamp.Ticks, o2 = 0;
            FeedHelper.SetupTakeSnapshot(_BaseStationAircraftLists, _AircraftLists, 0, o1, o2);

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            Assert.AreEqual(JavascriptHelper.ToJavascriptTicks(timestamp), json.ServerTime);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sets_ShowFlags_From_Configuration_Options()
        {
            _Configuration.BaseStationSettings.OperatorFlagsFolder = null;
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowFlags);

            _Configuration.BaseStationSettings.OperatorFlagsFolder = "EXISTS";
            Assert.AreEqual(true, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowFlags);

            _Configuration.BaseStationSettings.OperatorFlagsFolder = "NOTEXISTS";
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowFlags);
        }

        [TestMethod]
        public void WebSite_FlightSimAircraftList_Sets_ShowFlags_To_False_Regardless_Of_Configuration_Options()
        {
            _AircraftListAddress.Page = FlightSimListPage;

            _Configuration.BaseStationSettings.OperatorFlagsFolder = null;
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowFlags);

            _Configuration.BaseStationSettings.OperatorFlagsFolder = "EXISTS";
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowFlags);

            _Configuration.BaseStationSettings.OperatorFlagsFolder = "NOTEXISTS";
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowFlags);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sets_ShowPictures_From_Configuration_Options()
        {
            _Configuration.BaseStationSettings.PicturesFolder = null;
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowPictures);

            _Configuration.BaseStationSettings.PicturesFolder = "EXISTS";
            Assert.AreEqual(true, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowPictures);

            _Configuration.BaseStationSettings.PicturesFolder = "NOTEXISTS";
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowPictures);
        }

        [TestMethod]
        public void WebSite_FlightSimAircraftList_Sets_ShowPictures_To_False_Regardless_Of_Configuration_Options()
        {
            _AircraftListAddress.Page = FlightSimListPage;

            _Configuration.BaseStationSettings.PicturesFolder = null;
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowPictures);

            _Configuration.BaseStationSettings.PicturesFolder = "EXISTS";
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowPictures);

            _Configuration.BaseStationSettings.PicturesFolder = "NOTEXISTS";
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowPictures);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sets_ShowPictures_Can_Block_Internet_Clients()
        {
            _Configuration.BaseStationSettings.PicturesFolder = "EXISTS";

            _Configuration.InternetClientSettings.CanShowPictures = true;
            Assert.AreEqual(true, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address, false).ShowPictures);

            _Configuration.InternetClientSettings.CanShowPictures = true;
            Assert.AreEqual(true, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address, true).ShowPictures);

            _Configuration.InternetClientSettings.CanShowPictures = false;
            Assert.AreEqual(true, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address, false).ShowPictures);

            _Configuration.InternetClientSettings.CanShowPictures = false;
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address, true).ShowPictures);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sets_ShowSilhouettes_From_Configuration_Options()
        {
            _Configuration.BaseStationSettings.SilhouettesFolder = null;
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowSilhouettes);

            _Configuration.BaseStationSettings.SilhouettesFolder = "EXISTS";
            Assert.AreEqual(true, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowSilhouettes);

            _Configuration.BaseStationSettings.SilhouettesFolder = "NOTEXISTS";
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowSilhouettes);
        }

        [TestMethod]
        public void WebSite_FlightSimAircraftList_Sets_ShowSilhouettes_To_False_Regardless_Of_Configuration_Options()
        {
            _AircraftListAddress.Page = FlightSimListPage;

            _Configuration.BaseStationSettings.SilhouettesFolder = null;
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowSilhouettes);

            _Configuration.BaseStationSettings.SilhouettesFolder = "EXISTS";
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowSilhouettes);

            _Configuration.BaseStationSettings.SilhouettesFolder = "NOTEXISTS";
            Assert.AreEqual(false, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShowSilhouettes);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sets_ShortTrailLength_From_Configuration_Options()
        {
            _AircraftListAddress.ShowShortTrail = true;

            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 10;
            Assert.AreEqual(10, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShortTrailLengthSeconds);

            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 20;
            Assert.AreEqual(20, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShortTrailLengthSeconds);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_ShortTrailLength_Sent_Even_If_Browser_Requested_Full_Trails()
        {
            _AircraftListAddress.ShowShortTrail = false;

            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 10;
            _ConfigurationStorage.Raise(m => m.ConfigurationChanged += null, EventArgs.Empty);
            Assert.AreEqual(10, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).ShortTrailLengthSeconds);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sets_Source_Correctly()
        {
            _BaseStationAircraftLists[0].Setup(m => m.Source).Returns(AircraftListSource.BaseStation);
            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(1, json.Source);

            _BaseStationAircraftLists[0].Setup(m => m.Source).Returns(AircraftListSource.FlightSimulatorX);
            json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(3, json.Source);
        }
        #endregion

        #region Aircraft list and properties
        [TestMethod]
        public void WebSite_BaseStationAircraftList_Returns_Aircraft_List()
        {
            AddBlankAircraft(2);

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            Assert.AreEqual(2, json.Aircraft.Count);
            Assert.IsTrue(json.Aircraft.Where(a => a.UniqueId == 0).Any());
            Assert.IsTrue(json.Aircraft.Where(a => a.UniqueId == 1).Any());
        }

        [TestMethod]
        public void WebSite_FlightSimAircraftList_Returns_Aircraft_List()
        {
            _AircraftListAddress.Page = FlightSimListPage;
            AddBlankFlightSimAircraft(2);

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            Assert.AreEqual(2, json.Aircraft.Count);
            Assert.IsTrue(json.Aircraft.Where(a => a.UniqueId == 0).Any());
            Assert.IsTrue(json.Aircraft.Where(a => a.UniqueId == 1).Any());
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftJson$")]
        public void WebSite_BaseStationAircraftList_Correctly_Translates_IAircraft_Into_AircraftJson()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AircraftLists[0].Add(aircraft.Object);

            var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftProperty"));
            var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, new CultureInfo("en-GB"));
            aircraftProperty.SetValue(aircraft.Object, aircraftValue, null);

            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(1, json.Aircraft.Count);
            var aircraftJson = json.Aircraft[0];

            var jsonProperty = typeof(AircraftJson).GetProperty(worksheet.String("JsonProperty"));

            var expected = TestUtilities.ChangeType(worksheet.EString("JsonValue"), jsonProperty.PropertyType, new CultureInfo("en-GB"));
            var actual = jsonProperty.GetValue(aircraftJson, null);

            Assert.AreEqual(expected, actual, jsonProperty.Name);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Only_Copies_Values_If_They_Have_Changed()
        {
            var queryProperties = from p in typeof(IAircraft).GetProperties()
                                  where !p.Name.EndsWith("Changed") && typeof(IAircraft).GetProperty(String.Format("{0}Changed", p.Name)) != null
                                  select p;

            foreach(var aircraftProperty in queryProperties) {
                var changedProperty = typeof(IAircraft).GetProperty(String.Format("{0}Changed", aircraftProperty.Name));
                var jsonProperty = typeof(AircraftJson).GetProperty(aircraftProperty.Name);
                if(jsonProperty == null) {
                    switch(aircraftProperty.Name) {
                        case "FirstSeen":
                        case "Manufacturer":
                        case "PositionReceiverId":
                            continue;
                        case "PictureFileName":
                            jsonProperty = typeof(AircraftJson).GetProperty("HasPicture");
                            break;
                        default:
                            Assert.Fail("Need to add code to determine the JSON property for {0}", aircraftProperty.Name);
                            break;
                    }
                }

                var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
                var stopovers = new List<string>();
                aircraft.Setup(r => r.Stopovers).Returns(stopovers);
                _AircraftLists[0].Clear();
                _AircraftLists[0].Add(aircraft.Object);

                aircraft.Object.UniqueId = 1;
                _AircraftListAddress.PreviousAircraft.Add(1);

                object value = AircraftTestHelper.GenerateAircraftPropertyValue(aircraftProperty.PropertyType);
                var propertyAsList = aircraftProperty.GetValue(aircraft.Object, null) as IList;
                if(propertyAsList != null) propertyAsList.Add(value);
                else aircraftProperty.SetValue(aircraft.Object, value, null);

                Coordinate coordinate = value as Coordinate;
                if(coordinate != null) {
                    aircraft.Object.Latitude = coordinate.Latitude;
                    aircraft.Object.Longitude = coordinate.Longitude;
                }

                var parameter = Expression.Parameter(typeof(IAircraft));
                var body = Expression.Property(parameter, changedProperty.Name);
                var lambda = Expression.Lambda<Func<IAircraft, long>>(body, parameter);

                AircraftJson aircraftJson = null;

                // If the browser has never been sent a list before then the property must be returned
                aircraft.Setup(lambda).Returns(0L);
                _AircraftListAddress.PreviousDataVersion = -1L;
                aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];
                Assert.IsNotNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                aircraft.Setup(lambda).Returns(10L);
                aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];
                Assert.IsNotNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                // If the browser version is prior to the list version then the property must be returned
                _AircraftListAddress.PreviousDataVersion = 9L;
                aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];
                Assert.IsNotNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                // If the browser version is the same as the list version then the property must not be returned
                _AircraftListAddress.PreviousDataVersion = 10L;
                aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];
                Assert.IsNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                // If the browser version is the after the list version then the property must not be returned
                _AircraftListAddress.PreviousDataVersion = 11L;
                aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];
                Assert.IsNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                // If the browser version is after the list version, but the aircraft has not been seen before, then the property must be returned
                _AircraftListAddress.PreviousAircraft.Clear();
                aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];
                Assert.IsNotNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);
            }
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Copes_With_Multiple_Aircraft_Identifiers()
        {
            AddBlankAircraft(3);
            _AircraftLists[0][0].Callsign = "0";
            _AircraftLists[0][1].Callsign = "1";
            _AircraftLists[0][2].Callsign = "2";

            _AircraftListAddress.PreviousDataVersion = _AircraftLists[0][0].CallsignChanged + 1;

            _AircraftListAddress.PreviousAircraft.Add(0);
            _AircraftListAddress.PreviousAircraft.Add(2);

            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            // We have 3 aircraft and we've told the list builder that we know about the 1st and 3rd entries in the list and we already know the
            // callsigns. The second one is unknown to us, so it must send us everything it knows about it

            Assert.IsNull(aircraftJson.Aircraft.Where(a => a.UniqueId == 0).First().Callsign);
            Assert.AreEqual("1", aircraftJson.Aircraft.Where(a => a.UniqueId == 1).First().Callsign);
            Assert.IsNull(aircraftJson.Aircraft.Where(a => a.UniqueId == 2).First().Callsign);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListBearing$")]
        public void WebSite_BaseStationAircraftList_Calculates_Bearing_From_Browser_To_Aircraft_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AircraftLists[0].Add(aircraft.Object);

            aircraft.Object.Latitude = worksheet.NFloat("AircraftLatitude");
            aircraft.Object.Longitude = worksheet.NFloat("AircraftLongitude");

            _AircraftListAddress.BrowserLatitude = worksheet.NDouble("BrowserLatitude");
            _AircraftListAddress.BrowserLongitude = worksheet.NDouble("BrowserLongitude");

            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(1, list.Aircraft.Count);
            var aircraftJson = list.Aircraft[0];

            double? expected = worksheet.NDouble("Bearing");
            if(expected == null) Assert.IsNull(aircraftJson.BearingFromHere);
            else Assert.AreEqual((double)expected, (double)aircraftJson.BearingFromHere);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListDistance$")]
        public void WebSite_BaseStationAircraftList_Calculates_Distances_From_Browser_To_Aircraft_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AircraftLists[0].Add(aircraft.Object);

            aircraft.Object.Latitude = worksheet.NFloat("AircraftLatitude");
            aircraft.Object.Longitude = worksheet.NFloat("AircraftLongitude");

            _AircraftListAddress.BrowserLatitude = worksheet.NDouble("BrowserLatitude");
            _AircraftListAddress.BrowserLongitude = worksheet.NDouble("BrowserLongitude");

            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(1, list.Aircraft.Count);
            var aircraftJson = list.Aircraft[0];

            double? expected = worksheet.NDouble("Distance");
            if(expected == null) Assert.IsNull(aircraftJson.DistanceFromHere);
            else Assert.AreEqual((double)expected, (double)aircraftJson.DistanceFromHere);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListDistance$")]
        public void WebSite_BaseStationAircraftList_Calculates_Distances_From_Browser_To_Aircraft_Correctly_When_Culture_Is_Not_UK()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            using(var switcher = new CultureSwitcher("de-DE")) {
                var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
                _AircraftLists[0].Add(aircraft.Object);

                aircraft.Object.Latitude = worksheet.NFloat("AircraftLatitude");
                aircraft.Object.Longitude = worksheet.NFloat("AircraftLongitude");

                string address = String.Format("{0}?lat={1}&lng={2}", _AircraftListAddress.Page, worksheet.String("BrowserLatitude"), worksheet.String("BrowserLongitude"));

                var list = SendJsonRequest<AircraftListJson>(address);
                Assert.AreEqual(1, list.Aircraft.Count);
                var aircraftJson = list.Aircraft[0];

                double? expected = worksheet.NDouble("Distance");
                if(expected == null) Assert.IsNull(aircraftJson.DistanceFromHere);
                else Assert.AreEqual((double)expected, (double)aircraftJson.DistanceFromHere);
            }
        }

        [TestMethod]
        public void WebSite_FlightSimAircraftList_Does_Not_Calculate_Distances()
        {
            _AircraftListAddress.Page = FlightSimListPage;
            AddBlankFlightSimAircraft(1);
            var aircraft = _FlightSimulatorAircraft[0];

            aircraft.Latitude = aircraft.Longitude = 1f;

            _AircraftListAddress.BrowserLatitude = _AircraftListAddress.BrowserLongitude = 2.0;

            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.IsNull(list.Aircraft[0].DistanceFromHere);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Copies_Stopovers_Array_From_IAircraft_To_AircraftJson()
        {
            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var stopovers = new List<string>();
            aircraft.Setup(r => r.Stopovers).Returns(stopovers);

            _AircraftLists[0].Add(aircraft.Object);

            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.IsNull(list.Aircraft[0].Stopovers);

            stopovers.Add("Stop 1");
            stopovers.Add("Stop 2");
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(2, list.Aircraft[0].Stopovers.Count);
            Assert.AreEqual("Stop 1", list.Aircraft[0].Stopovers[0]);
            Assert.AreEqual("Stop 2", list.Aircraft[0].Stopovers[1]);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListCoordinates$")]
        public void WebSite_BaseStationAircraftList_Builds_Arrays_Of_Trail_Coordinates_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = worksheet.NFloat("ACLat");
            aircraft.Longitude = worksheet.NFloat("ACLng");
            aircraft.Track = worksheet.NFloat("ACTrk");
            aircraft.FirstCoordinateChanged = worksheet.Long("ACFirstCoCh");
            aircraft.LastCoordinateChanged = worksheet.Long("ACLastCoCh");
            aircraft.PositionTime = new DateTime(1970, 1, 1, 0, 0, 0, worksheet.Int("ACPosTimeCh"));
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(worksheet.Long("ACPosTimeCh"));

            for(int i = 1;i <= 2;++i) {
                var dataVersion = String.Format("Coord{0}DV", i);
                var tick = String.Format("Coord{0}Tick", i);
                var latitude = String.Format("Coord{0}Lat", i);
                var longitude = String.Format("Coord{0}Lng", i);
                var track = String.Format("Coord{0}Trk", i);
                if(worksheet.String(dataVersion) != null) {
                    DateTime dotNetDate = new DateTime(1970, 1, 1, 0, 0, 0, worksheet.Int(tick));
                    var coordinate = new Coordinate(worksheet.Long(dataVersion), dotNetDate.Ticks, worksheet.Float(latitude), worksheet.Float(longitude), worksheet.NFloat(track));
                    aircraft.FullCoordinates.Add(coordinate);
                    aircraft.ShortCoordinates.Add(coordinate);
                }
            }

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.PreviousDataVersion = worksheet.Long("ArgsPrevDV");
            if(worksheet.Bool("ArgsIsPrevAC")) _AircraftListAddress.PreviousAircraft.Add(0);
            _AircraftListAddress.ShowShortTrail = worksheet.Bool("ArgsShort");
            _AircraftListAddress.ResendTrails = worksheet.Bool("ArgsResend");

            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            var count = worksheet.Int("Count");
            if(count == 0) {
                Assert.IsNull(aircraftJson.ShortCoordinates);
                Assert.IsNull(aircraftJson.FullCoordinates);
            } else {
                var list = worksheet.Bool("IsShort") ? aircraftJson.ShortCoordinates : aircraftJson.FullCoordinates;
                Assert.AreEqual(count, list.Count);
                for(int i = 0;i < count;++i) {
                    var column = String.Format("R{0}", i);
                    Assert.AreEqual(worksheet.NDouble(column), list[i], "Element {0}", i);
                }
            }

            Assert.AreEqual(worksheet.Bool("ResetTrail"), aircraftJson.ResetTrail);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sends_No_Trails_If_None_Are_Requested()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 4;
            aircraft.Longitude = 5;
            aircraft.Track = 10f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 10000;
            aircraft.GroundSpeed = 200;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 9000, 200));
            aircraft.FullCoordinates.Add(new Coordinate(1001, 1001, 2, 3, 9, 9200, 200));
            aircraft.ShortCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 9000, 200));
            aircraft.ShortCoordinates.Add(new Coordinate(1001, 1001, 2, 3, 9, 9200, 200));

            _AircraftListAddress.SendTrail = false;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            Assert.AreEqual(null, aircraftJson.TrailType);
            Assert.AreEqual(null, aircraftJson.FullCoordinates);
            Assert.AreEqual(null, aircraftJson.ShortCoordinates);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sends_ShortAltitude_Trails_If_Requested()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 3;
            aircraft.Longitude = 4;
            aircraft.Track = 10f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 9200;
            aircraft.GroundSpeed = 200;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(900, 700000000000000000L, 1, 2, 8, 9000, 190));
            aircraft.FullCoordinates.Add(new Coordinate(901, 800000000000000000L, 3, 4, 9, 9200, 200));
            aircraft.ShortCoordinates.Add(new Coordinate(900, 700000000000000000L, 1, 2, 8, 9000, 190));
            aircraft.ShortCoordinates.Add(new Coordinate(901, 800000000000000000L, 3, 4, 9, 9200, 200));

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = true;
            _AircraftListAddress.ShowAltitudeTrail = true;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            Assert.AreEqual("a", aircraftJson.TrailType);
            Assert.AreEqual(null, aircraftJson.FullCoordinates);
            Assert.AreEqual(8, aircraftJson.ShortCoordinates.Count);

            Assert.AreEqual(1.0, aircraftJson.ShortCoordinates[0]);
            Assert.AreEqual(2.0, aircraftJson.ShortCoordinates[1]);
            Assert.AreEqual(7864403200000, aircraftJson.ShortCoordinates[2]);
            Assert.AreEqual(9000.0, aircraftJson.ShortCoordinates[3]);

            Assert.AreEqual(3.0, aircraftJson.ShortCoordinates[4]);
            Assert.AreEqual(4.0, aircraftJson.ShortCoordinates[5]);
            Assert.AreEqual(17864403200000, aircraftJson.ShortCoordinates[6]);
            Assert.AreEqual(9200.0, aircraftJson.ShortCoordinates[7]);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sends_ShortSpeed_Trails_If_Requested()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 3;
            aircraft.Longitude = 4;
            aircraft.Track = 10f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 9200;
            aircraft.GroundSpeed = 200;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(900, 700000000000000000L, 1, 2, 8, 9000, 190));
            aircraft.FullCoordinates.Add(new Coordinate(901, 800000000000000000L, 3, 4, 9, 9200, 200));
            aircraft.ShortCoordinates.Add(new Coordinate(900, 700000000000000000L, 1, 2, 8, 9000, 190));
            aircraft.ShortCoordinates.Add(new Coordinate(901, 800000000000000000L, 3, 4, 9, 9200, 200));

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = true;
            _AircraftListAddress.ShowSpeedTrail = true;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            Assert.AreEqual("s", aircraftJson.TrailType);
            Assert.AreEqual(null, aircraftJson.FullCoordinates);
            Assert.AreEqual(8, aircraftJson.ShortCoordinates.Count);

            Assert.AreEqual(1.0, aircraftJson.ShortCoordinates[0]);
            Assert.AreEqual(2.0, aircraftJson.ShortCoordinates[1]);
            Assert.AreEqual(7864403200000, aircraftJson.ShortCoordinates[2]);
            Assert.AreEqual(190.0, aircraftJson.ShortCoordinates[3]);

            Assert.AreEqual(3.0, aircraftJson.ShortCoordinates[4]);
            Assert.AreEqual(4.0, aircraftJson.ShortCoordinates[5]);
            Assert.AreEqual(17864403200000, aircraftJson.ShortCoordinates[6]);
            Assert.AreEqual(200.0, aircraftJson.ShortCoordinates[7]);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Ignores_FullTrackCoordinates_Where_The_Only_Change_Is_In_Altitude()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 4;
            aircraft.Longitude = 5;
            aircraft.Track = 9.4f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 10000;
            aircraft.GroundSpeed = 200;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 9000, 200));   // Initial coordinate
            aircraft.FullCoordinates.Add(new Coordinate(1001, 1001, 2, 3, 9, 9200, 200));   // Aircraft has moved but remains on same track. Altitude has changed, but that is coincidental.
            aircraft.FullCoordinates.Add(new Coordinate(1010, 1002, 4, 5, 9, 9300, 200));   // Same track as before - this exists purely because the altitude changed. When full trails are requested we should merge this.

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = false;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            // We should get 2 coordinates and the middle coordinate should be ignored because there was no change in track
            Assert.AreEqual(6, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual(null, aircraftJson.TrailType);

            Assert.AreEqual(1, aircraftJson.FullCoordinates[0]);
            Assert.AreEqual(2, aircraftJson.FullCoordinates[1]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[2]);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[3]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[5]);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Ignores_FullTrackCoordinates_Where_The_Only_Change_Is_In_Speed()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 4;
            aircraft.Longitude = 5;
            aircraft.Track = 9.4f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 10000;
            aircraft.GroundSpeed = 200;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 10000, 200));   // Initial coordinate
            aircraft.FullCoordinates.Add(new Coordinate(1001, 1001, 2, 3, 9, 10000, 250));   // Aircraft has moved but remains on same track. Speed has changed, but that is coincidental.
            aircraft.FullCoordinates.Add(new Coordinate(1010, 1002, 4, 5, 9, 10000, 300));   // Same track as before - this exists purely because the speed changed. When full trails are requested we should merge this.

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = false;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            // We should get 2 coordinates and the middle coordinate should be ignored because there was no change in track
            Assert.AreEqual(6, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual(null, aircraftJson.TrailType);

            Assert.AreEqual(1, aircraftJson.FullCoordinates[0]);
            Assert.AreEqual(2, aircraftJson.FullCoordinates[1]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[2]);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[3]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[5]);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sends_All_Altitude_FullTrackCoordinates_When_FullTrack_Altitude_Is_Requested()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 4;
            aircraft.Longitude = 5;
            aircraft.Track = 9.4f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 10000;
            aircraft.GroundSpeed = 200;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 9000, 200));   // Initial coordinate
            aircraft.FullCoordinates.Add(new Coordinate(1001, 1001, 2, 3, 9, 9200, 200));   // Aircraft has moved but remains on same track. Altitude has changed, but that is coincidental.
            aircraft.FullCoordinates.Add(new Coordinate(1010, 1002, 4, 5, 9, 9300, 200));   // Same track as before - this exists purely because the altitude changed. When full trails are requested we should merge this.

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = false;
            _AircraftListAddress.ShowAltitudeTrail = true;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            // We should get all 3 coordinates
            Assert.AreEqual(12, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual("a", aircraftJson.TrailType);

            Assert.AreEqual(1, aircraftJson.FullCoordinates[0]);
            Assert.AreEqual(2, aircraftJson.FullCoordinates[1]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[2]);
            Assert.AreEqual(9000, aircraftJson.FullCoordinates[3]);

            Assert.AreEqual(2, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(3, aircraftJson.FullCoordinates[5]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[6]);
            Assert.AreEqual(9200, aircraftJson.FullCoordinates[7]);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[8]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[9]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[10]);
            Assert.AreEqual(9300, aircraftJson.FullCoordinates[11]);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sends_All_Speed_FullTrackCoordinates_When_FullTrack_Speed_Is_Requested()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 4;
            aircraft.Longitude = 5;
            aircraft.Track = 9.4f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 10000;
            aircraft.GroundSpeed = 240;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 10000, 120));
            aircraft.FullCoordinates.Add(new Coordinate(1001, 1001, 2, 3, 9, 10000, 160));
            aircraft.FullCoordinates.Add(new Coordinate(1010, 1002, 4, 5, 9, 10000, 200));

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = false;
            _AircraftListAddress.ShowSpeedTrail = true;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            // We should get all 3 coordinates
            Assert.AreEqual(12, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual("s", aircraftJson.TrailType);

            Assert.AreEqual(1, aircraftJson.FullCoordinates[0]);
            Assert.AreEqual(2, aircraftJson.FullCoordinates[1]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[2]);
            Assert.AreEqual(120, aircraftJson.FullCoordinates[3]);

            Assert.AreEqual(2, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(3, aircraftJson.FullCoordinates[5]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[6]);
            Assert.AreEqual(160, aircraftJson.FullCoordinates[7]);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[8]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[9]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[10]);
            Assert.AreEqual(200, aircraftJson.FullCoordinates[11]);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Skips_Speed_Changes_In_FullTrackCoordinates_When_FullTrack_Altitude_Is_Requested()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 4;
            aircraft.Longitude = 5;
            aircraft.Track = 9.4f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 10000;
            aircraft.GroundSpeed = 200;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 9000, 200));   // Initial coordinate
            aircraft.FullCoordinates.Add(new Coordinate(1001, 1001, 2, 3, 9, 9000, 210));   // Aircraft has moved but remains on same track. Speed has changed, but that is coincidental.
            aircraft.FullCoordinates.Add(new Coordinate(1010, 1002, 4, 5, 9, 9000, 220));   // Same track as before - this exists purely because the speed changed. When full trails are requested we should merge this.

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = false;
            _AircraftListAddress.ShowAltitudeTrail = true;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            // We should get all 3 coordinates
            Assert.AreEqual(8, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual("a", aircraftJson.TrailType);

            Assert.AreEqual(1, aircraftJson.FullCoordinates[0]);
            Assert.AreEqual(2, aircraftJson.FullCoordinates[1]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[2]);
            Assert.AreEqual(9000, aircraftJson.FullCoordinates[3]);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[5]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[6]);
            Assert.AreEqual(9000, aircraftJson.FullCoordinates[7]);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Skips_Altitude_Changes_In_FullTrackCoordinates_When_FullTrack_Speed_Is_Requested()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 4;
            aircraft.Longitude = 5;
            aircraft.Track = 9.4f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 10000;
            aircraft.GroundSpeed = 200;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 8000,  200));
            aircraft.FullCoordinates.Add(new Coordinate(1001, 1001, 2, 3, 9, 9000,  200));
            aircraft.FullCoordinates.Add(new Coordinate(1010, 1002, 4, 5, 9, 10000, 200));

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = false;
            _AircraftListAddress.ShowSpeedTrail = true;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            // We should get all 3 coordinates
            Assert.AreEqual(8, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual("s", aircraftJson.TrailType);

            Assert.AreEqual(1, aircraftJson.FullCoordinates[0]);
            Assert.AreEqual(2, aircraftJson.FullCoordinates[1]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[2]);
            Assert.AreEqual(200, aircraftJson.FullCoordinates[3]);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[5]);
            Assert.AreEqual(9, aircraftJson.FullCoordinates[6]);
            Assert.AreEqual(200, aircraftJson.FullCoordinates[7]);
        }

        [TestMethod]
        public void  WebSite_BaseStationAircraftList_Rounds_Altitude_When_Generating_Extra_Coordinates_For_FullTrackCoordinates()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 4;
            aircraft.Longitude = 5;
            aircraft.Track = 9.4f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 9770;
            aircraft.GroundSpeed = 200;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 9000, 200));   // Initial coordinate

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = false;
            _AircraftListAddress.ShowAltitudeTrail = true;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            // We should get 2 coordinates and the altitude should have been rounded
            Assert.AreEqual(8, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual("a", aircraftJson.TrailType);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[5]);
            Assert.AreEqual(10, aircraftJson.FullCoordinates[6]);       // gets rounded
            Assert.AreEqual(10000, aircraftJson.FullCoordinates[7]);    // gets rounded
        }

        [TestMethod]
        public void  WebSite_BaseStationAircraftList_Rounds_Speed_When_Generating_Extra_Coordinates_For_FullTrackCoordinates()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);

            aircraft.Latitude = 4;
            aircraft.Longitude = 5;
            aircraft.Track = 9.4f;
            aircraft.FirstCoordinateChanged = 1000;
            aircraft.LastCoordinateChanged = 1010;
            aircraft.PositionTime = new DateTime(2013, 11, 24);
            aircraft.Altitude = 9770;
            aircraft.GroundSpeed = 207;
            mockAircraft.Setup(m => m.PositionTimeChanged).Returns(10);

            aircraft.FullCoordinates.Add(new Coordinate(1000, 1000, 1, 2, 9, 9000, 200));   // Initial coordinate

            _AircraftListAddress.SendTrail = true;
            _AircraftListAddress.ShowShortTrail = false;
            _AircraftListAddress.ShowSpeedTrail = true;
            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            // We should get 2 coordinates and the altitude should have been rounded
            Assert.AreEqual(8, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual("s", aircraftJson.TrailType);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[5]);
            Assert.AreEqual(10, aircraftJson.FullCoordinates[6]);     // gets rounded
            Assert.AreEqual(210, aircraftJson.FullCoordinates[7]);    // gets rounded
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Calculates_SecondsTracked_From_Server_Time_And_FirstSeen_Property()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);
            mockAircraft.Setup(a => a.FirstSeen).Returns(new DateTime(2001, 1, 1, 1, 2, 0));
            _Provider.Setup(p => p.UtcNow).Returns(new DateTime(2001, 1, 1, 1, 3, 17));

            var aircraftJson = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft[0];

            Assert.AreEqual(77L, aircraftJson.SecondsTracked);
        }
        #endregion

        #region ServerConfigurationChanged property
        [TestMethod]
        public void WebSite_BaseStationAircraftList_Does_Not_Set_ServerConfigurationChanged_For_First_Request_Of_Data()
        {
            var time = DateTime.UtcNow;
            _Provider.Setup(r => r.UtcNow).Returns(time);
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _AircraftListAddress.ServerTimeJavaScriptTicks = -1;
            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            Assert.AreEqual(false, json.ServerConfigChanged);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sets_ServerConfigurationChanged_If_Server_Config_Changed_After_Last_Fetch()
        {
            var time = DateTime.UtcNow;
            _Provider.Setup(r => r.UtcNow).Returns(time);
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _AircraftListAddress.ServerTimeJavaScriptTicks = JavascriptHelper.ToJavascriptTicks(time.Ticks) - 1;
            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            Assert.AreEqual(true, json.ServerConfigChanged);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Does_Not_Set_ServerConfigurationChanged_If_Server_Config_Changed_Before_Last_Fetch()
        {
            var time = DateTime.UtcNow;
            _Provider.Setup(r => r.UtcNow).Returns(time);
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _AircraftListAddress.ServerTimeJavaScriptTicks = JavascriptHelper.ToJavascriptTicks(time.Ticks) + 1;
            var json = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            Assert.AreEqual(false, json.ServerConfigChanged);
        }
        #endregion

        #region Filtering of list
        #region Individual filters - reflection tests
        [TestMethod]
        public void WebSite_BaseStationAircraftList_Applies_AircraftListFilters_When_Supplied()
        {
            foreach(var filterProperty in typeof(AircraftListFilter).GetProperties()) {
                ResetFilters();

                var isNumericRangeFilter = AircraftListFilter.IsNumericFilter(filterProperty);
                var isBoolFilter = AircraftListFilter.IsBoolFilter(filterProperty);
                var isStringFilter = AircraftListFilter.IsStringFilter(filterProperty);

                if(filterProperty.Name == "Distance") continue; // <-- we have a separate test for these
                else if(filterProperty.Name == "MustTransmitPosition") TestMustTransmitPositionFilter();
                else if(filterProperty.Name == "PositionWithin") continue; // <-- we have a separate spreadsheet test for these
                else if(isStringFilter) {
                    TestContainsFilter(filterProperty, false);      ResetFilters();
                    TestContainsFilter(filterProperty, true);       ResetFilters();
                    TestEqualsFilter(filterProperty, false);        ResetFilters();
                    TestEqualsFilter(filterProperty, true);         ResetFilters();
                    TestStartsWithFilter(filterProperty, false);    ResetFilters();
                    TestStartsWithFilter(filterProperty, true);     ResetFilters();
                    TestEndsWithFilter(filterProperty, false);      ResetFilters();
                    TestEndsWithFilter(filterProperty, true);
                } else if(isNumericRangeFilter) {
                    TestLowerFilter(filterProperty, false);         ResetFilters();
                    TestLowerFilter(filterProperty, true);          ResetFilters();
                    TestUpperFilter(filterProperty, false);         ResetFilters();
                    TestUpperFilter(filterProperty, true);
                } else if(isBoolFilter) {
                    TestEqualsFilter(filterProperty, false);        ResetFilters();
                    TestEqualsFilter(filterProperty, true);
                } else Assert.Fail("Need to add code to test the {0} filter", filterProperty.Name);
            }
        }

        private void ResetFilters()
        {
            _AircraftListFilter = new AircraftListFilter();
            _AircraftListAddress.Filter = _AircraftListFilter;
            _AircraftLists[0].Clear();
        }

        private void ExtractPropertiesFromFilterProperty(PropertyInfo filterProperty, out PropertyInfo aircraftProperty, out PropertyInfo jsonProperty)
        {
            var aircraftPropertyName = filterProperty.Name;
            if(aircraftPropertyName == "Airport") aircraftPropertyName = "Origin";
            aircraftProperty = typeof(IAircraft).GetProperty(aircraftPropertyName);
            Assert.IsNotNull(aircraftProperty, filterProperty.Name);

            var jsonPropertyName = aircraftProperty.Name;
            jsonProperty = typeof(AircraftJson).GetProperty(jsonPropertyName);
            Assert.IsNotNull(jsonProperty, filterProperty.Name);
        }

        private void TestContainsFilter(PropertyInfo filterProperty, bool reversed)
        {
            _AircraftLists[0].Clear();

            PropertyInfo aircraftProperty, jsonProperty;
            ExtractPropertiesFromFilterProperty(filterProperty, out aircraftProperty, out jsonProperty);

            AddBlankAircraft(3);
            aircraftProperty.SetValue(_AircraftLists[0][0], null, null);
            aircraftProperty.SetValue(_AircraftLists[0][1], "ABC", null);
            aircraftProperty.SetValue(_AircraftLists[0][2], "XYZ", null);

            _AircraftListFilter.SetStringProperty(filterProperty, null, FilterCondition.Contains, reversed);
            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetStringProperty(filterProperty, "", FilterCondition.Contains, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetStringProperty(filterProperty, "B", FilterCondition.Contains, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(1, list.Count, filterProperty.Name, FilterCondition.Contains, reversed);
                Assert.AreEqual("ABC", jsonProperty.GetValue(list[0], null), filterProperty.Name);
            } else {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                Assert.AreEqual(null, jsonProperty.GetValue(list[0], null), filterProperty.Name);
                Assert.AreEqual("XYZ", jsonProperty.GetValue(list[1], null), filterProperty.Name);
            }

            _AircraftListFilter.SetStringProperty(filterProperty, "b", FilterCondition.Contains, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                Assert.AreEqual("ABC", jsonProperty.GetValue(list[0], null), filterProperty.Name);
            } else {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                Assert.AreEqual(null, jsonProperty.GetValue(list[0], null), filterProperty.Name);
                Assert.AreEqual("XYZ", jsonProperty.GetValue(list[1], null), filterProperty.Name);
            }

            _AircraftListFilter.SetStringProperty(filterProperty, "W", FilterCondition.Contains, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) Assert.AreEqual(0, list.Count, filterProperty.Name);
            else          Assert.AreEqual(3, list.Count, filterProperty.Name);
        }

        private void TestEqualsFilter(PropertyInfo filterProperty, bool reversed)
        {
            // If the type is an Enum then this will only work if there are at least 2 entries in the enum and they are numbered 0, & 1. If this is
            // not the case then write an explicit test for the filter.

            _AircraftLists[0].Clear();
            PropertyInfo aircraftProperty, jsonProperty;
            ExtractPropertiesFromFilterProperty(filterProperty, out aircraftProperty, out jsonProperty);

            AddBlankAircraft(3);
            aircraftProperty.SetValue(_AircraftLists[0][0], null, null);
            aircraftProperty.SetValue(_AircraftLists[0][1], TestUtilities.ChangeType(0, aircraftProperty.PropertyType, CultureInfo.InvariantCulture), null);
            aircraftProperty.SetValue(_AircraftLists[0][2], TestUtilities.ChangeType(1, aircraftProperty.PropertyType, CultureInfo.InvariantCulture), null);

            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetValueProperty(filterProperty, 1, FilterCondition.Equals, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                Assert.AreEqual(TestUtilities.ChangeType(1, jsonProperty.PropertyType, CultureInfo.InvariantCulture), jsonProperty.GetValue(list[0], null));
            } else {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                foreach(var listItem in list) {
                    Assert.AreNotEqual(TestUtilities.ChangeType(1, jsonProperty.PropertyType, CultureInfo.InvariantCulture), jsonProperty.GetValue(listItem, null));
                }
            }
        }

        private void TestLowerFilter(PropertyInfo filterProperty, bool reversed)
        {
            PropertyInfo aircraftProperty, jsonProperty;
            ExtractPropertiesFromFilterProperty(filterProperty, out aircraftProperty, out jsonProperty);

            _AircraftLists[0].Clear();
            AddBlankAircraft(3);
            aircraftProperty.SetValue(_AircraftLists[0][0], null, null);
            aircraftProperty.SetValue(_AircraftLists[0][1], TestUtilities.ChangeType(99, aircraftProperty.PropertyType, CultureInfo.InvariantCulture), null);
            aircraftProperty.SetValue(_AircraftLists[0][2], TestUtilities.ChangeType(100, aircraftProperty.PropertyType, CultureInfo.InvariantCulture), null);

            _AircraftListFilter.SetRangeProperty(filterProperty, (int?)null, (int?)null, reversed);
            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetRangeProperty(filterProperty, 99, (int?)null, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                Assert.AreEqual(99, TestUtilities.ChangeType(jsonProperty.GetValue(list[0], null), typeof(int?), CultureInfo.InvariantCulture), filterProperty.Name);
                Assert.AreEqual(100, TestUtilities.ChangeType(jsonProperty.GetValue(list[1], null), typeof(int?), CultureInfo.InvariantCulture), filterProperty.Name);
            } else {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                var list0 = jsonProperty.GetValue(list[0], null);
                Assert.IsTrue(list0 == null || list0.ToString() == "" || list0.ToString() == "0");      // Squawks make this one a bit tricky, the aircraft stores them as ints but the output is string
            }

            _AircraftListFilter.SetRangeProperty(filterProperty, 100, (int?)null, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                Assert.AreEqual(100, TestUtilities.ChangeType(jsonProperty.GetValue(list[0], null), typeof(int?), CultureInfo.InvariantCulture), filterProperty.Name);
            } else {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                var list0 = jsonProperty.GetValue(list[0], null);
                Assert.IsTrue(list0 == null || list0.ToString() == "" || list0.ToString() == "0");      // Squawks make this one a bit tricky, the aircraft stores them as ints but the output is string
                Assert.AreEqual(99, TestUtilities.ChangeType(jsonProperty.GetValue(list[1], null), typeof(int?), CultureInfo.InvariantCulture), filterProperty.Name);
            }

            _AircraftListFilter.SetRangeProperty(filterProperty, 101, (int?)null, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) Assert.AreEqual(0, list.Count, filterProperty.Name);
            else          Assert.AreEqual(3, list.Count, filterProperty.Name);
        }

        private void TestUpperFilter(PropertyInfo filterProperty, bool reversed)
        {
            PropertyInfo aircraftProperty, jsonProperty;
            ExtractPropertiesFromFilterProperty(filterProperty, out aircraftProperty, out jsonProperty);

            _AircraftLists[0].Clear();
            AddBlankAircraft(3);
            aircraftProperty.SetValue(_AircraftLists[0][0], null, null);
            aircraftProperty.SetValue(_AircraftLists[0][1], TestUtilities.ChangeType(99, aircraftProperty.PropertyType, CultureInfo.InvariantCulture), null);
            aircraftProperty.SetValue(_AircraftLists[0][2], TestUtilities.ChangeType(100, aircraftProperty.PropertyType, CultureInfo.InvariantCulture), null);

            _AircraftListFilter.SetRangeProperty(filterProperty, (int?)null, (int?)null, reversed);
            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetRangeProperty(filterProperty, (int?)null, 100, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                Assert.AreEqual(99, TestUtilities.ChangeType(jsonProperty.GetValue(list[0], null), typeof(int), CultureInfo.InvariantCulture), filterProperty.Name);
                Assert.AreEqual(100, TestUtilities.ChangeType(jsonProperty.GetValue(list[1], null), typeof(int), CultureInfo.InvariantCulture), filterProperty.Name);
            } else {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                var list0 = jsonProperty.GetValue(list[0], null);
                Assert.IsTrue(list0 == null || list0.ToString() == "" || list0.ToString() == "0");      // Squawks make this one a bit tricky, the aircraft stores them as ints but the output is string
            }

            _AircraftListFilter.SetRangeProperty(filterProperty, (int?)null, 99, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                Assert.AreEqual(99, TestUtilities.ChangeType(jsonProperty.GetValue(list[0], null), typeof(int), CultureInfo.InvariantCulture), filterProperty.Name);
            } else {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                var list0 = jsonProperty.GetValue(list[0], null);
                Assert.IsTrue(list0 == null || list0.ToString() == "" || list0.ToString() == "0");      // Squawks make this one a bit tricky, the aircraft stores them as ints but the output is string
                Assert.AreEqual(100, TestUtilities.ChangeType(jsonProperty.GetValue(list[1], null), typeof(int?), CultureInfo.InvariantCulture), filterProperty.Name);
            }

            _AircraftListFilter.SetRangeProperty(filterProperty, (int?)null, 98, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) Assert.AreEqual(0, list.Count, filterProperty.Name);
            else          Assert.AreEqual(3, list.Count, filterProperty.Name);
        }

        private void TestMustTransmitPositionFilter()
        {
            _AircraftLists[0].Clear();
            AddBlankAircraft(4);
            _AircraftLists[0][0].Latitude = null;
            _AircraftLists[0][0].Longitude = null;

            _AircraftLists[0][1].Latitude = 1f;
            _AircraftLists[0][1].Longitude = null;

            _AircraftLists[0][2].Latitude = null;
            _AircraftLists[0][2].Longitude = 2f;

            _AircraftLists[0][3].Latitude = 3f;
            _AircraftLists[0][3].Longitude = 3f;

            _AircraftListFilter.MustTransmitPosition = new BoolFilter<bool>(true, false);

            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);
            Assert.AreEqual(1, list.Aircraft.Count, "MustTransmitPosition");
            Assert.AreEqual(3f, list.Aircraft[0].Latitude, "MustTransmitPosition");
        }

        private void TestStartsWithFilter(PropertyInfo filterProperty, bool reversed)
        {
            PropertyInfo aircraftProperty, jsonProperty;
            ExtractPropertiesFromFilterProperty(filterProperty, out aircraftProperty, out jsonProperty);

            _AircraftLists[0].Clear();
            AddBlankAircraft(3);
            aircraftProperty.SetValue(_AircraftLists[0][0], null, null);
            aircraftProperty.SetValue(_AircraftLists[0][1], "ABC", null);
            aircraftProperty.SetValue(_AircraftLists[0][2], "XYZ", null);

            _AircraftListFilter.SetStringProperty(filterProperty, null, FilterCondition.StartsWith, reversed);
            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetStringProperty(filterProperty, "", FilterCondition.StartsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetStringProperty(filterProperty, "A", FilterCondition.StartsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                Assert.AreEqual("ABC", jsonProperty.GetValue(list[0], null), filterProperty.Name);
            } else {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                Assert.AreEqual(null, jsonProperty.GetValue(list[0], null), filterProperty.Name);
                Assert.AreEqual("XYZ", jsonProperty.GetValue(list[1], null), filterProperty.Name);
            }

            _AircraftListFilter.SetStringProperty(filterProperty, "B", FilterCondition.StartsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) Assert.AreEqual(0, list.Count, filterProperty.Name);
            else          Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetStringProperty(filterProperty, "a", FilterCondition.StartsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                Assert.AreEqual("ABC", jsonProperty.GetValue(list[0], null), filterProperty.Name);
            } else {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                Assert.AreEqual(null, jsonProperty.GetValue(list[0], null), filterProperty.Name);
                Assert.AreEqual("XYZ", jsonProperty.GetValue(list[1], null), filterProperty.Name);
            }

            _AircraftListFilter.SetStringProperty(filterProperty, "W", FilterCondition.StartsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) Assert.AreEqual(0, list.Count, filterProperty.Name);
            else          Assert.AreEqual(3, list.Count, filterProperty.Name);
        }

        private void TestEndsWithFilter(PropertyInfo filterProperty, bool reversed)
        {
            PropertyInfo aircraftProperty, jsonProperty;
            ExtractPropertiesFromFilterProperty(filterProperty, out aircraftProperty, out jsonProperty);

            _AircraftLists[0].Clear();
            AddBlankAircraft(3);
            aircraftProperty.SetValue(_AircraftLists[0][0], null, null);
            aircraftProperty.SetValue(_AircraftLists[0][1], "ABC", null);
            aircraftProperty.SetValue(_AircraftLists[0][2], "XYZ", null);

            _AircraftListFilter.SetStringProperty(filterProperty, null, FilterCondition.EndsWith, reversed);
            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetStringProperty(filterProperty, "", FilterCondition.EndsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetStringProperty(filterProperty, "C", FilterCondition.EndsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                Assert.AreEqual("ABC", jsonProperty.GetValue(list[0], null), filterProperty.Name);
            } else {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                Assert.AreEqual(null, jsonProperty.GetValue(list[0], null), filterProperty.Name);
                Assert.AreEqual("XYZ", jsonProperty.GetValue(list[1], null), filterProperty.Name);
            }

            _AircraftListFilter.SetStringProperty(filterProperty, "B", FilterCondition.EndsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) Assert.AreEqual(0, list.Count, filterProperty.Name);
            else          Assert.AreEqual(3, list.Count, filterProperty.Name);

            _AircraftListFilter.SetStringProperty(filterProperty, "c", FilterCondition.EndsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) {
                Assert.AreEqual(1, list.Count, filterProperty.Name);
                Assert.AreEqual("ABC", jsonProperty.GetValue(list[0], null), filterProperty.Name);
            } else {
                Assert.AreEqual(2, list.Count, filterProperty.Name);
                Assert.AreEqual(null, jsonProperty.GetValue(list[0], null), filterProperty.Name);
                Assert.AreEqual("XYZ", jsonProperty.GetValue(list[1], null), filterProperty.Name);
            }

            _AircraftListFilter.SetStringProperty(filterProperty, "W", FilterCondition.EndsWith, reversed);
            list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;
            if(!reversed) Assert.AreEqual(0, list.Count, filterProperty.Name);
            else          Assert.AreEqual(3, list.Count, filterProperty.Name);
        }
        #endregion

        #region Individual filters - other tests
        [TestMethod]
        public void WebSite_BaseStationAircraftList_Filters_Are_Culture_Agnostic()
        {
            var cultureNames = new string[] { 
                "en-GB",
                "en-US",
                "de-DE",
                "fr-FR",
                "nn-NO",
                "el-GR",
                "ru-RU",
                "zh-CHT",
            };

            _AircraftListAddress.BrowserLatitude = _AircraftListAddress.BrowserLongitude = 0;

            var currentCulture = Thread.CurrentThread.CurrentCulture;
            foreach(var cultureName in cultureNames) {
                using(var switcher = new CultureSwitcher(cultureName)) {
                    for(var propertyNumber = 0;propertyNumber < 4;++propertyNumber) {
                        _AircraftLists[0].Clear();
                        AddBlankAircraft(2);
                        var aircraft0 = _AircraftLists[0][0];
                        var aircraft1 = _AircraftLists[0][1];

                        _AircraftListFilter = new AircraftListFilter();
                        _AircraftListAddress.Filter = _AircraftListFilter;

                        var expectedAircraftId = 1;
                        string property;
                        switch(propertyNumber) {
                            case 0:
                                property = "Altitude";
                                aircraft0.Altitude = -9; aircraft1.Altitude = 9;
                                _AircraftListFilter.Altitude = new NumericFilter<int>(-5, null, false);
                                break;
                            case 1:
                                property = "Distance";
                                aircraft0.Latitude = aircraft0.Longitude = 1; aircraft1.Latitude = aircraft1.Longitude = 2;
                                _AircraftListFilter.Distance = new NumericFilter<double>(200.25, null, false);
                                break;
                            case 2:
                                property = "Position";
                                aircraft0.Latitude = aircraft0.Longitude = -9.5f; aircraft1.Latitude = aircraft1.Longitude = 9.5f;
                                _AircraftListFilter.PositionWithin = new Pair<Coordinate>(new Coordinate(5.5f, -15.5f), new Coordinate(-15.5f, 5.5f));
                                expectedAircraftId = 0;
                                break;
                            case 3:
                                property = "Squawk";
                                aircraft0.Squawk = -9; aircraft1.Squawk = 9;
                                _AircraftListFilter.Squawk = new NumericFilter<int>(-5, null, false);
                                break;
                            default:
                                throw new NotImplementedException();
                        }

                        var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;

                        Assert.AreEqual(1, list.Count, switcher.CultureName, property);
                        Assert.AreEqual(expectedAircraftId, list[0].UniqueId, "{0} {1}", property, switcher.CultureName);
                    }
                }
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListDistanceFilter$")]
        public void WebSite_BaseStationAircraftList_AircraftListFilter_Distance_AircraftListFiltered_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _AircraftListAddress.Filter = _AircraftListFilter;
            _AircraftListAddress.BrowserLatitude = worksheet.Float("BrowserLatitude");
            _AircraftListAddress.BrowserLongitude = worksheet.Float("BrowserLongitude");

            AddBlankAircraft(1);
            _AircraftLists[0][0].Latitude = worksheet.NFloat("AircraftLatitude");
            _AircraftLists[0][0].Longitude = worksheet.NFloat("AircraftLongitude");

            _AircraftListFilter.Distance = new NumericFilter<double>(worksheet.NDouble("DistanceLower"), worksheet.NDouble("DistanceUpper"), worksheet.Bool("Reversed"));

            bool passed = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft.Count == 1;

            Assert.AreEqual(worksheet.Bool("Passes"), passed);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListPositionWithin$")]
        public void WebSite_BaseStationAircraftList_AircraftListFilter_PositionWithin_Works_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _AircraftListAddress.Filter = _AircraftListFilter;
            _AircraftListAddress.SelectedAircraftId = worksheet.NInt("SelectedId");

            AddBlankAircraft(1);
            _AircraftLists[0][0].UniqueId = worksheet.Int("AircraftId");
            _AircraftLists[0][0].Latitude = worksheet.NFloat("Latitude");
            _AircraftLists[0][0].Longitude = worksheet.NFloat("Longitude");

            _AircraftListFilter.PositionWithin = new Pair<Coordinate>(
                new Coordinate(worksheet.Float("Top"), worksheet.Float("Left")),
                new Coordinate(worksheet.Float("Bottom"), worksheet.Float("Right"))
            );

            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            Assert.AreEqual(worksheet.Bool("IsInBounds") ? 1 : 0, list.Aircraft.Count);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_AircraftListFilter_Icao24CountryContains_Works_With_Mixed_Case_Aircraft_Property()
        {
            _AircraftListAddress.Filter = _AircraftListFilter;
            _AircraftListFilter.Icao24Country = new StringFilter("B", FilterCondition.Contains, false);

            AddBlankAircraft(1);
            _AircraftLists[0][0].Icao24Country = "b";

            Assert.AreEqual(1, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft.Count);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_AircraftListFilter_OperatorContains_Works_With_Mixed_Case_Aircraft_Property()
        {
            _AircraftListAddress.Filter = _AircraftListFilter;
            _AircraftListFilter.Operator = new StringFilter("B", FilterCondition.Contains, false);

            AddBlankAircraft(1);
            _AircraftLists[0][0].Operator = "b";

            Assert.AreEqual(1, SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft.Count);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_AircraftListFilter_Sets_AvailableAircraft_Correctly()
        {
            _AircraftListAddress.Filter = _AircraftListFilter;
            _AircraftListFilter.Registration = new StringFilter("ABC", FilterCondition.Contains, false);

            AddBlankAircraft(2);
            _AircraftLists[0][0].Registration = "ABC";
            _AircraftLists[0][1].Registration = "XYZ";

            var result = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            Assert.AreEqual(1, result.Aircraft.Count);
            Assert.AreEqual(2, result.AvailableAircraft);
        }

        [TestMethod]
        public void WebSite_FlightSimAircraftList_Ignores_All_Filters()
        {
            _AircraftListAddress.Page = FlightSimListPage;
            _AircraftListAddress.Filter = _AircraftListFilter;

            _AircraftListFilter.Registration = new StringFilter("ABC", FilterCondition.Contains, false);

            AddBlankFlightSimAircraft(2);
            _FlightSimulatorAircraft[0].Registration = "ABC";
            _FlightSimulatorAircraft[1].Registration = "XYZ";

            var result = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address);

            Assert.AreEqual(2, result.Aircraft.Count);
        }
        #endregion

        #region Combinations of filters
        [TestMethod]
        public void WebSite_BaseStationAircraftList_Combinations_Of_AircraftListFilters_Work_Together()
        {
            foreach(var firstFilterProperty in typeof(AircraftListFilter).GetProperties()) {
                foreach(var secondFilterProperty in typeof(AircraftListFilter).GetProperties()) {
                    // Ignore combinations that are invalid or just a pain to automate
                    if(firstFilterProperty == secondFilterProperty) continue;
                    if(PropertiesAreCombinationOf(firstFilterProperty.Name, secondFilterProperty.Name, "Distance", "MustTransmitPosition")) continue;
                    if(PropertiesAreCombinationOf(firstFilterProperty.Name, secondFilterProperty.Name, "Distance", "PositionWithin")) continue;
                    if(PropertiesAreCombinationOf(firstFilterProperty.Name, secondFilterProperty.Name, "PositionWithin", "MustTransmitPosition")) continue;

                    // pass 0 = neither filter passes, pass 1 = first filter passes, pass 2 = second filter passes, pass 3 = both pass
                    // Only pass 3 should allow the aircraft to appear in the aircraft list
                    for(int pass = 0;pass < 4;++pass) {
                        TestCleanup();
                        TestInitialise();

                        _AircraftListAddress.Filter = _AircraftListFilter;

                        AddBlankAircraft(1);

                        PrepareForCombinationTest(firstFilterProperty, _AircraftLists[0][0], pass == 1 || pass == 3);
                        PrepareForCombinationTest(secondFilterProperty, _AircraftLists[0][0], pass >= 2);

                        bool present = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft.Count == 1;

                        Assert.AreEqual(pass == 3, present, "Filter on {0} and {1}, pass {2}", firstFilterProperty.Name, secondFilterProperty.Name, pass);
                    }
                }

                break; // Comparing one property against all the others is enough. If we loop through every combination it takes quite a while...
            }
        }

        /// <summary>
        /// Returns true if the two property names are the two names passed across.
        /// </summary>
        /// <param name="propertyName1"></param>
        /// <param name="propertyName2"></param>
        /// <param name="name1"></param>
        /// <param name="name2"></param>
        /// <returns></returns>
        private bool PropertiesAreCombinationOf(string propertyName1, string propertyName2, string name1, string name2)
        {
            return (propertyName1 == name1 || propertyName1 == name2) && (propertyName2 == name1 || propertyName2 == name2);
        }

        /// <summary>
        /// Sets up the filters, build arguments and aircraft for a test that confirms that combinations of filters behave correctly.
        /// </summary>
        /// <param name="filterProperty"></param>
        /// <param name="aircraft"></param>
        /// <param name="complies"></param>
        private void PrepareForCombinationTest(PropertyInfo filterProperty, IAircraft aircraft, bool complies)
        {
            switch(filterProperty.Name) {
                case "Airport":
                    _AircraftListFilter.Airport = new StringFilter("ABC", FilterCondition.Contains, false);
                    aircraft.Origin = complies ? "ABC123" : "XYZ789";
                    break;
                case "Altitude":
                    _AircraftListFilter.Altitude = new NumericFilter<int>(10000, 20000, false);
                    aircraft.Altitude = complies ? 15000 : 1000;
                    break;
                case "Callsign":
                    _AircraftListFilter.Callsign = new StringFilter("ABC", FilterCondition.Contains, false);
                    aircraft.Callsign = complies ? "ABC123" : "XYZ987";
                    break;
                case "Distance":
                    _AircraftListFilter.Distance = new NumericFilter<double>(15.0, 30.0, false);
                    _AircraftListAddress.BrowserLatitude = _AircraftListAddress.BrowserLongitude = 0;
                    aircraft.Latitude = aircraft.Longitude = complies ? 0.15F : 0.01F;
                    break;
                case "EngineType":
                    _AircraftListFilter.EngineType = new BoolFilter<EngineType>(EngineType.Piston, false);
                    aircraft.EngineType = complies ? EngineType.Piston : EngineType.Jet;
                    break;
                case "Icao24":
                    _AircraftListFilter.Icao24 = new StringFilter("44", FilterCondition.Contains, false);
                    aircraft.Icao24 = complies ? "114422" : "556677";
                    break;
                case "Icao24Country":
                    _AircraftListFilter.Icao24Country = new StringFilter("UNITED", FilterCondition.Contains, false);
                    aircraft.Icao24Country = complies ? "UNITED KINGDOM" : "BELGIUM";
                    break;
                case "IsInteresting":
                    _AircraftListFilter.IsInteresting = new BoolFilter<bool>(true, false);
                    aircraft.IsInteresting = complies ? true : false;
                    break;
                case "IsMilitary":
                    _AircraftListFilter.IsMilitary = new BoolFilter<bool>(true, false);
                    aircraft.IsMilitary = complies ? true : false;
                    break;
                case "MustTransmitPosition":
                    _AircraftListFilter.MustTransmitPosition = new BoolFilter<bool>(true, false);
                    aircraft.Latitude = aircraft.Longitude = complies ? 1F : (float?)null;
                    break;
                case "Operator":
                    _AircraftListFilter.Operator = new StringFilter("TRU", FilterCondition.Contains, false);
                    aircraft.Operator = complies ? "ERMENUTRUDE AIRLINES" : "DOOGAL INTERNATIONAL";
                    break;
                case "OperatorIcao":
                    _AircraftListFilter.OperatorIcao = new StringFilter("BA", FilterCondition.Contains, false);
                    aircraft.OperatorIcao = complies ? "BAW" : "VRS";
                    break;
                case "PositionWithin":
                    _AircraftListFilter.PositionWithin = new Pair<Coordinate>(new Coordinate(4F, 1F), new Coordinate(1F, 4F));
                    aircraft.Latitude = aircraft.Longitude = complies ? 3F : 6F;
                    break;
                case "Registration":
                    _AircraftListFilter.Registration = new StringFilter("GLU", FilterCondition.Contains, false);
                    aircraft.Registration = complies ? "G-GLUE" : "G-LUUU";
                    break;
                case "Species":
                    _AircraftListFilter.Species = new BoolFilter<Species>(Species.Helicopter, false);
                    aircraft.Species = complies ? Species.Helicopter : Species.Landplane;
                    break;
                case "Squawk":
                    _AircraftListFilter.Squawk = new NumericFilter<int>(2000, 4000, false);
                    aircraft.Squawk = complies ? 2654 : 1234;
                    break;
                case "Type":
                    _AircraftListFilter.Type = new StringFilter("A38", FilterCondition.StartsWith, false);
                    aircraft.Type = complies ? "A380" : "A340";
                    break;
                case "UserTag":
                    _AircraftListFilter.UserTag = new StringFilter("FORM", FilterCondition.Contains, false);
                    aircraft.UserTag = complies ? "Freeform text" : "Something else";
                    break;
                case "WakeTurbulenceCategory":
                    _AircraftListFilter.WakeTurbulenceCategory = new BoolFilter<WakeTurbulenceCategory>(WakeTurbulenceCategory.Heavy, false);
                    aircraft.WakeTurbulenceCategory = complies ? WakeTurbulenceCategory.Heavy : WakeTurbulenceCategory.Medium;
                    break;
                default:
                    Assert.Fail("Need to add code to prepare objects for {0} filter", filterProperty.Name);
                    break;
            }
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Combinations_Of_Upper_Lower_AircraftListFilters_Work_Together()
        {
            var foundOne = false;

            foreach(var lowerProperty in typeof(AircraftListFilter).GetProperties().Where(p => AircraftListFilter.IsNumericFilter(p))) {
                foundOne = true;

                // pass 0 = under lower limit, pass 1 = between lower and upper, pass 2 = above upper.
                // Only pass 1 should allow the aircraft to appear in the aircraft list
                for(int pass = 0;pass < 3;++pass) {
                    TestCleanup();
                    TestInitialise();

                    _AircraftListAddress.Filter = _AircraftListFilter;

                    AddBlankAircraft(1);
                    var aircraft = _AircraftLists[0][0];

                    switch(lowerProperty.Name) {
                        case "Altitude":
                            _AircraftListFilter.Altitude = new NumericFilter<int>(10000, 20000, false);
                            aircraft.Altitude = pass == 0 ? 5000 : pass == 1 ? 15000 : 25000;
                            break;
                        case "Distance":
                            _AircraftListFilter.Distance = new NumericFilter<double>(15.0, 30.0, false);
                            _AircraftListAddress.BrowserLatitude = _AircraftListAddress.BrowserLongitude = 0;
                            aircraft.Latitude = aircraft.Longitude = pass == 0 ? 0.01F : pass == 1 ? 0.15F : 0.2F;
                            break;
                        case "Squawk":
                            _AircraftListFilter.Squawk = new NumericFilter<int>(1000, 2000, false);
                            aircraft.Squawk = pass == 0 ? 500 : pass == 1 ? 1500 : 2500;
                            break;
                    }

                    bool present = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft.Count == 1;

                    Assert.AreEqual(pass == 1, present, "Filter on {0}, pass {1}", lowerProperty.Name, pass);
                }
            }

            Assert.IsTrue(foundOne);
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Airport_Filter_Works_On_All_Route_Fields()
        {
            for(var part = 0;part < 3;++part) {
                TestCleanup();
                TestInitialise();

                _AircraftListAddress.Filter = _AircraftListFilter;
                _AircraftListFilter.Airport = new StringFilter("LHR", FilterCondition.Equals, false);

                AddBlankAircraft(1);
                var aircraft = _AircraftLists[0][0];
                switch(part) {
                    case 0: aircraft.Origin = "LHR Whatever"; break;
                    case 1: aircraft.Destination = "LHR Whatever"; break;
                    case 2: aircraft.Stopovers.Add("LHR Whatever"); break;
                }

                var countPassed = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft.Count;
                Assert.AreEqual(1, countPassed, "Part is {0}", part);
            }
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Airport_Filter_Works_Correctly_When_Condition_Is_Reversed()
        {
            _AircraftListAddress.Filter = _AircraftListFilter;
            _AircraftListFilter.Airport = new StringFilter("LHR", FilterCondition.Equals, true);

            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];

            aircraft.Origin = "JFK Somewhere";
            aircraft.Destination = "LHR Somewhere else";

            var countPassed = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft.Count;
            Assert.AreEqual(0, countPassed);
        }
        #endregion
        #endregion

        #region Sorting of list
        [TestMethod]
        public void WebSite_BaseStationAircraftList_Defaults_To_Sorting_By_FirstSeen_Descending()
        {
            AddBlankAircraft(2);

            DateTime baseTime = new DateTime(2001, 1, 2, 0, 0, 0);
            DateTime loTime = baseTime.AddSeconds(1);
            DateTime hiTime = baseTime.AddSeconds(2);

            for(int order = 0;order < 2;++order) {
                _AircraftLists[0][0].FirstSeen = order == 0 ? hiTime : loTime;
                _AircraftLists[0][1].FirstSeen = order == 0 ? loTime : hiTime;

                var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;

                Assert.AreEqual(hiTime, list[0].FirstSeen);
                Assert.AreEqual(loTime, list[1].FirstSeen);
            }
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Will_Sort_List_By_Single_Column()
        {
            _AircraftListAddress.BrowserLatitude = _AircraftListAddress.BrowserLongitude = 0;

            foreach(var sortColumnField in typeof(AircraftComparerColumn).GetFields()) {
                var sortColumn = (string)sortColumnField.GetValue(null);

                for(int initialOrder = 0;initialOrder < 2;++initialOrder) {
                    for(int sortDescending = 0;sortDescending < 2;++sortDescending) {
                        // pass 0: LHS < RHS
                        // pass 1: LHS > RHS
                        // pass 2: LHS is default < RHS is not
                        // pass 3: LHS is not  > RHS is default
                        // Order is reversed if sortDescending == 1
                        for(int pass = 0;pass < 4;++pass) {
                            var failedMessage = String.Format("{0}, sortDescending = {1}, initialOrder = {2}, pass = {3}", sortColumn, sortDescending, initialOrder, pass);

                            _AircraftListAddress.SortBy.Clear();
                            _AircraftListAddress.SortBy.Add(new KeyValuePair<string,string>(sortColumn, sortDescending == 0 ? "ASC" : "DESC"));

                            _AircraftLists[0].Clear();
                            AddBlankAircraft(2);
                            var lhs = _AircraftLists[0][initialOrder == 0 ? 0 : 1];
                            var rhs = _AircraftLists[0][initialOrder == 0 ? 1 : 0];
                            if(sortColumn == AircraftComparerColumn.FirstSeen) lhs.FirstSeen = rhs.FirstSeen = DateTime.MinValue;

                            if(pass != 2) PrepareForSortTest(sortColumn, lhs, pass != 1);
                            if(pass != 3) PrepareForSortTest(sortColumn, rhs, pass != 0);

                            var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;

                            bool expectLhsFirst = pass == 0 || pass == 2;
                            if(sortDescending != 0) expectLhsFirst = !expectLhsFirst;

                            if(expectLhsFirst) {
                                Assert.AreEqual(lhs.UniqueId, list[0].UniqueId, failedMessage);
                                Assert.AreEqual(rhs.UniqueId, list[1].UniqueId, failedMessage);
                            } else {
                                Assert.AreEqual(rhs.UniqueId, list[0].UniqueId, failedMessage);
                                Assert.AreEqual(lhs.UniqueId, list[1].UniqueId, failedMessage);
                            }
                        }
                    }
                }
            }
        }

        private void PrepareForSortTest(string sortColumn, IAircraft aircraft, bool setLow)
        {
            switch(sortColumn) {
                case AircraftComparerColumn.Altitude:                   aircraft.Altitude = setLow ? 1 : 2; break;
                case AircraftComparerColumn.Callsign:                   aircraft.Callsign = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.Destination:                aircraft.Destination = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.DistanceFromHere:           aircraft.Latitude = aircraft.Longitude = setLow ? 1 : 2; break;
                case AircraftComparerColumn.FirstSeen:                  aircraft.FirstSeen = setLow ? new DateTime(2001, 1, 1, 0, 0, 0) : new DateTime(2001, 1, 1, 0, 0, 1); break;
                case AircraftComparerColumn.FlightsCount:               aircraft.FlightsCount = setLow ? 1 : 2; break;
                case AircraftComparerColumn.GroundSpeed:                aircraft.GroundSpeed = setLow ? 1 : 2; break;
                case AircraftComparerColumn.Icao24:                     aircraft.Icao24 = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.Icao24Country:              aircraft.Icao24Country = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.Model:                      aircraft.Model = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.NumberOfEngines:            aircraft.NumberOfEngines = setLow ? "1" : "2"; break;
                case AircraftComparerColumn.Operator:                   aircraft.Operator = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.OperatorIcao:               aircraft.OperatorIcao = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.Origin:                     aircraft.Origin = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.Registration:               aircraft.Registration = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.Species:                    aircraft.Species = setLow ? Species.Amphibian : Species.Helicopter; break;
                case AircraftComparerColumn.Squawk:                     aircraft.Squawk = setLow ? 1 : 2; break;
                case AircraftComparerColumn.Type:                       aircraft.Type = setLow ? "A" : "B"; break;
                case AircraftComparerColumn.VerticalRate:               aircraft.VerticalRate = setLow ? 1 : 2; break;
                case AircraftComparerColumn.WakeTurbulenceCategory:     aircraft.WakeTurbulenceCategory = setLow ? WakeTurbulenceCategory.Light : WakeTurbulenceCategory.Medium; break;
                default:                                                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Will_Sort_List_By_Two_Columns()
        {
            _AircraftListAddress.SortBy.Add(new KeyValuePair<string,string>(AircraftComparerColumn.Altitude, "ASC"));
            _AircraftListAddress.SortBy.Add(new KeyValuePair<string,string>(AircraftComparerColumn.Registration, "ASC"));

            AddBlankAircraft(2);

            for(int order = 0;order < 2;++order) {
                _AircraftLists[0][0].Registration = order == 0 ? "A" : "B";
                _AircraftLists[0][1].Registration = order == 0 ? "B" : "A";

                var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;

                Assert.AreEqual("A", list[0].Registration);
                Assert.AreEqual("B", list[1].Registration);
            }
        }

        [TestMethod]
        public void WebSite_BaseStationAircraftList_Sort_Column_Name_Is_Case_Insensitive()
        {
            AddBlankAircraft(2);

            for(int caseStyle = 0;caseStyle < 2;++caseStyle) {
                _AircraftListAddress.SortBy.Clear();
                _AircraftListAddress.SortBy.Add(new KeyValuePair<string,string>(
                    caseStyle == 0 ? AircraftComparerColumn.Registration.ToLower() : AircraftComparerColumn.Registration.ToUpper(),
                    caseStyle == 0 ? "asc" : "ASC"));

                for(int order = 0;order < 2;++order) {
                    _AircraftLists[0][0].Registration = order == 0 ? "A" : "B";
                    _AircraftLists[0][1].Registration = order == 0 ? "B" : "A";

                    var list = SendJsonRequest<AircraftListJson>(_AircraftListAddress.Address).Aircraft;

                    Assert.AreEqual("A", list[0].Registration);
                    Assert.AreEqual("B", list[1].Registration);
                }
            }
        }
        #endregion
    }
}
