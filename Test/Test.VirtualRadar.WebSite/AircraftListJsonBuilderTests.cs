// Copyright © 2015 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite
{
    [TestClass]
    public class AircraftListJsonBuilderTests
    {
        #region Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IAircraftListJsonBuilder _Builder;
        private AircraftListJsonBuilderArgs _Args;
        private AircraftListJsonBuilderFilter _Filter;

        private IClassFactory _OriginalFactory;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private Configuration _Configuration;

        private MockFileSystemProvider _FileSystemProvider;
        private ClockMock _Clock;
        private List<Mock<IFeed>> _Feeds;
        private List<Mock<IBaseStationAircraftList>> _BaseStationAircraftLists;
        private List<List<IAircraft>> _AircraftLists;
        private Mock<IFeedManager> _ReceiverManager;
        private Mock<ISimpleAircraftList> _FlightSimulatorAircraftList;
        private List<IAircraft> _FlightSimulatorAircraft;

        [TestInitialize]
        public void TestInitialise()
        {
            _OriginalFactory = Factory.TakeSnapshot();

            _FileSystemProvider = new MockFileSystemProvider();
            Factory.Singleton.RegisterInstance<IFileSystemProvider>(_FileSystemProvider);

            _Clock = new ClockMock();
            Factory.Singleton.RegisterInstance<IClock>(_Clock.Object);

            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _Configuration = new Configuration();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 1;

            _Builder = Factory.Singleton.Resolve<IAircraftListJsonBuilder>();
            _Args = new AircraftListJsonBuilderArgs();
            _Filter = new AircraftListJsonBuilderFilter();

            _Feeds = new List<Mock<IFeed>>();
            _BaseStationAircraftLists = new List<Mock<IBaseStationAircraftList>>();
            _AircraftLists = new List<List<IAircraft>>();
            var useVisibleFeeds = true;
            _ReceiverManager = FeedHelper.CreateMockFeedManager(_Feeds, _BaseStationAircraftLists, _AircraftLists, useVisibleFeeds, 1, 2);

            _FlightSimulatorAircraftList = new Mock<ISimpleAircraftList>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _FlightSimulatorAircraft = new List<IAircraft>();
            long of1, of2;
            _FlightSimulatorAircraftList.Setup(m => m.TakeSnapshot(out of1, out of2)).Returns(_FlightSimulatorAircraft);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_OriginalFactory);
        }
        #endregion

        #region Helper methods
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

        #region Simple aircraft list
        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Empty_Json_When_BaseStationAircraftList_Is_Empty()
        {
            var json = _Builder.Build(_Args);
            Assert.AreEqual(0, json.Aircraft.Count);
            Assert.AreEqual(0, json.AvailableAircraft);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Empty_Json_When_There_Are_No_Feeds()
        {
            _ReceiverManager = FeedHelper.CreateMockFeedManager(new List<Mock<IFeed>>(), new List<Mock<IListener>>(), useVisibleFeeds: true);
            _Builder = Factory.Singleton.Resolve<IAircraftListJsonBuilder>();

            var json = _Builder.Build(_Args);

            Assert.AreEqual(0, json.Aircraft.Count);
            Assert.AreEqual(0, json.AvailableAircraft);
            Assert.AreEqual(-1, json.SourceFeedId);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Requested_SourceFeedId_When_There_Are_No_Feeds()
        {
            _ReceiverManager = FeedHelper.CreateMockFeedManager(new List<Mock<IFeed>>(), new List<Mock<IListener>>(), useVisibleFeeds: true);
            _Builder = Factory.Singleton.Resolve<IAircraftListJsonBuilder>();

            _Args.SourceFeedId = 9;
            var json = _Builder.Build(_Args);

            Assert.AreEqual(9, json.SourceFeedId);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Correct_Aircraft_List_When_Explicit_Feed_Requested()
        {
            AddBlankAircraft(1, listIndex: 1);
            _Args.SourceFeedId = 2;

            var json = _Builder.Build(_Args);
            Assert.AreEqual(1, json.Aircraft.Count);
            Assert.AreEqual(1, json.AvailableAircraft);
            Assert.AreEqual(2, json.SourceFeedId);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Default_Aircraft_List_When_No_Feed_Requested()
        {
            AddBlankAircraft(1, listIndex: 1);
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 2;

            var json = _Builder.Build(_Args);
            Assert.AreEqual(1, json.Aircraft.Count);
            Assert.AreEqual(1, json.AvailableAircraft);
            Assert.AreEqual(2, json.SourceFeedId);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Default_Aircraft_List_When_Invalid_Feed_Requested()
        {
            AddBlankAircraft(1, listIndex: 1);
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 2;

            var json = _Builder.Build(_Args);
            Assert.AreEqual(1, json.Aircraft.Count);
            Assert.AreEqual(1, json.AvailableAircraft);
            Assert.AreEqual(2, json.SourceFeedId);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Full_List_Of_Feeds()
        {
            foreach(var feedExists in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                _Args.SourceFeedId = feedExists ? 1 : 99;
                _Feeds[0].Setup(r => r.Name).Returns("Feed 1");
                _Feeds[1].Setup(r => r.Name).Returns("Feed 2");

                var json = _Builder.Build(_Args);

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
        public void AircraftListJsonBuilder_Does_Not_Returns_Feeds_If_FeedsNotRequired_Is_True()
        {
            _Args.FeedsNotRequired = true;
            var json = _Builder.Build(_Args);
            Assert.AreEqual(0, json.Feeds.Count);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Empty_AircraftListJson_When_AircraftList_Is_Null_And_FSX_List_Requested()
        {
            AddBlankAircraft(1);
            _Args.AircraftList = null;
            _Args.IsFlightSimulatorList = true;

            var json = _Builder.Build(_Args);

            Assert.AreEqual(0, json.Aircraft.Count);
            Assert.AreEqual(0, json.AvailableAircraft);
        }
        #endregion

        #region Configuration changes
        [TestMethod]
        public void AircraftListJsonBuilder_Picks_Up_Changes_To_Default_WebSite_Receiver_Id()
        {
            AddBlankAircraft(1, listIndex: 1);
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 1;

            var json = _Builder.Build(_Args);
            Assert.AreEqual(0, json.Aircraft.Count);

            _Configuration.GoogleMapSettings.WebSiteReceiverId = 2;

            json = _Builder.Build(_Args);
            Assert.AreEqual(1, json.Aircraft.Count);
        }
        #endregion

        #region List properties
        [TestMethod]
        public void AircraftListJsonBuilder_Sets_Flag_Dimensions_Correctly()
        {
            // These are currently non-configurable
            var json = _Builder.Build(_Args);
            Assert.AreEqual(85, json.FlagWidth);
            Assert.AreEqual(20, json.FlagHeight);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Build_Sets_LastDataVersion_Correctly()
        {
            long o1 = 0, o2 = 200;
            FeedHelper.SetupTakeSnapshot(_BaseStationAircraftLists, _AircraftLists, 0, o1, o2);
            var json = _Builder.Build(_Args);
            Assert.AreEqual("200", json.LastDataVersion);

            o2 = 573;
            FeedHelper.SetupTakeSnapshot(_BaseStationAircraftLists, _AircraftLists, 0, o1, o2);
            json = _Builder.Build(_Args);
            Assert.AreEqual("573", json.LastDataVersion);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Build_Sets_ServerTime_Correctly()
        {
            DateTime timestamp = new DateTime(2001, 12, 31, 14, 27, 32, 194);
            long o1 = timestamp.Ticks, o2 = 0;
            FeedHelper.SetupTakeSnapshot(_BaseStationAircraftLists, _AircraftLists, 0, o1, o2);

            var json = _Builder.Build(_Args);

            Assert.AreEqual(JavascriptHelper.ToJavascriptTicks(timestamp), json.ServerTime);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sets_ShowFlags_From_Configuration_Options()
        {
            _FileSystemProvider.AddFolder(@"c:\flags");

            _Configuration.BaseStationSettings.OperatorFlagsFolder = null;
            Assert.AreEqual(false, _Builder.Build(_Args).ShowFlags);

            _Configuration.BaseStationSettings.OperatorFlagsFolder = @"c:\flags";
            Assert.AreEqual(true, _Builder.Build(_Args).ShowFlags);

            _Configuration.BaseStationSettings.OperatorFlagsFolder = @"c:\no-flags";
            Assert.AreEqual(false, _Builder.Build(_Args).ShowFlags);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_In_FSX_Mode_Sets_ShowFlags_To_False_Regardless_Of_Configuration_Options()
        {
            _Args.IsFlightSimulatorList = true;

            _Configuration.BaseStationSettings.OperatorFlagsFolder = null;
            Assert.AreEqual(false, _Builder.Build(_Args).ShowFlags);

            _Configuration.BaseStationSettings.OperatorFlagsFolder = "EXISTS";
            Assert.AreEqual(false, _Builder.Build(_Args).ShowFlags);

            _Configuration.BaseStationSettings.OperatorFlagsFolder = "NOTEXISTS";
            Assert.AreEqual(false, _Builder.Build(_Args).ShowFlags);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sets_ShowPictures_From_Configuration_Options()
        {
            _FileSystemProvider.AddFolder(@"c:\pictures");

            _Configuration.BaseStationSettings.PicturesFolder = null;
            Assert.AreEqual(false, _Builder.Build(_Args).ShowPictures);

            _Configuration.BaseStationSettings.PicturesFolder = @"c:\pictures";
            Assert.AreEqual(true, _Builder.Build(_Args).ShowPictures);

            _Configuration.BaseStationSettings.PicturesFolder = @"c:\no-pictures";
            Assert.AreEqual(false, _Builder.Build(_Args).ShowPictures);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_In_FSX_Mode_Sets_ShowPictures_To_False_Regardless_Of_Configuration_Options()
        {
            _Args.IsFlightSimulatorList = true;

            _Configuration.BaseStationSettings.PicturesFolder = null;
            Assert.AreEqual(false, _Builder.Build(_Args).ShowPictures);

            _Configuration.BaseStationSettings.PicturesFolder = "EXISTS";
            Assert.AreEqual(false, _Builder.Build(_Args).ShowPictures);

            _Configuration.BaseStationSettings.PicturesFolder = "NOTEXISTS";
            Assert.AreEqual(false, _Builder.Build(_Args).ShowPictures);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Can_Hide_Pictures_From_Internet_Clients()
        {
            _Configuration.BaseStationSettings.PicturesFolder = @"c:\Pictures";
            _FileSystemProvider.AddFolder(_Configuration.BaseStationSettings.PicturesFolder);

            _Configuration.InternetClientSettings.CanShowPictures = true;

            _Args.IsInternetClient = true;
            Assert.AreEqual(true, _Builder.Build(_Args).ShowPictures);
            _Args.IsInternetClient = false;
            Assert.AreEqual(true, _Builder.Build(_Args).ShowPictures);

            _Configuration.InternetClientSettings.CanShowPictures = false;

            _Args.IsInternetClient = true;
            Assert.AreEqual(false, _Builder.Build(_Args).ShowPictures);
            _Args.IsInternetClient = false;
            Assert.AreEqual(true, _Builder.Build(_Args).ShowPictures);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sets_ShowSilhouettes_From_Configuration_Options()
        {
            _FileSystemProvider.AddFolder(@"c:\silhouettes");

            _Configuration.BaseStationSettings.SilhouettesFolder = null;
            Assert.AreEqual(false, _Builder.Build(_Args).ShowSilhouettes);

            _Configuration.BaseStationSettings.SilhouettesFolder = @"c:\silhouettes";
            Assert.AreEqual(true, _Builder.Build(_Args).ShowSilhouettes);

            _Configuration.BaseStationSettings.SilhouettesFolder = @"c:\no-silhouettes";
            Assert.AreEqual(false, _Builder.Build(_Args).ShowSilhouettes);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sets_ShowSilhouettes_To_False_Regardless_Of_Configuration_Options()
        {
            _Args.IsFlightSimulatorList = true;

            _Configuration.BaseStationSettings.SilhouettesFolder = null;
            Assert.AreEqual(false, _Builder.Build(_Args).ShowSilhouettes);

            _Configuration.BaseStationSettings.SilhouettesFolder = "EXISTS";
            Assert.AreEqual(false, _Builder.Build(_Args).ShowSilhouettes);

            _Configuration.BaseStationSettings.SilhouettesFolder = "NOTEXISTS";
            Assert.AreEqual(false, _Builder.Build(_Args).ShowSilhouettes);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sets_ShortTrailLength_From_Configuration_Options()
        {
            _Args.TrailType = TrailType.Short;

            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 10;
            Assert.AreEqual(10, _Builder.Build(_Args).ShortTrailLengthSeconds);

            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 20;
            Assert.AreEqual(20, _Builder.Build(_Args).ShortTrailLengthSeconds);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_ShortTrailLength_Sent_Even_If_Browser_Requested_Full_Trails()
        {
            _Args.TrailType = TrailType.Full;

            _Configuration.GoogleMapSettings.ShortTrailLengthSeconds = 10;
            Assert.AreEqual(10, _Builder.Build(_Args).ShortTrailLengthSeconds);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sets_Source_Correctly()
        {
            _BaseStationAircraftLists[0].Setup(m => m.Source).Returns(AircraftListSource.BaseStation);
            var json = _Builder.Build(_Args);
            Assert.AreEqual(1, json.Source);

            _BaseStationAircraftLists[0].Setup(m => m.Source).Returns(AircraftListSource.FlightSimulatorX);
            json = _Builder.Build(_Args);
            Assert.AreEqual(3, json.Source);
        }
        #endregion

        #region Aircraft list and properties
        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Aircraft_List()
        {
            AddBlankAircraft(2);

            var json = _Builder.Build(_Args);

            Assert.AreEqual(2, json.Aircraft.Count);
            Assert.IsTrue(json.Aircraft.Where(a => a.UniqueId == 0).Any());
            Assert.IsTrue(json.Aircraft.Where(a => a.UniqueId == 1).Any());
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Returns_Aircraft_List_In_FSX_Mode()
        {
            _Args.IsFlightSimulatorList = true;
            _Args.AircraftList = _FlightSimulatorAircraftList.Object;
            AddBlankFlightSimAircraft(2);

            var json = _Builder.Build(_Args);

            Assert.AreEqual(2, json.Aircraft.Count);
            Assert.IsTrue(json.Aircraft.Where(a => a.UniqueId == 0).Any());
            Assert.IsTrue(json.Aircraft.Where(a => a.UniqueId == 1).Any());
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftJson$")]
        public void AircraftListJsonBuilder_Correctly_Translates_IAircraft_Into_AircraftJson()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AircraftLists[0].Add(aircraft.Object);

            var aircraftProperty = typeof(IAircraft).GetProperty(worksheet.String("AircraftProperty"));
            var aircraftValue = TestUtilities.ChangeType(worksheet.EString("AircraftValue"), aircraftProperty.PropertyType, new CultureInfo("en-GB"));
            aircraftProperty.SetValue(aircraft.Object, aircraftValue, null);

            var json = _Builder.Build(_Args);
            Assert.AreEqual(1, json.Aircraft.Count);
            var aircraftJson = json.Aircraft[0];

            var jsonProperty = typeof(AircraftJson).GetProperty(worksheet.String("JsonProperty"));

            var expected = TestUtilities.ChangeType(worksheet.EString("JsonValue"), jsonProperty.PropertyType, new CultureInfo("en-GB"));
            var actual = jsonProperty.GetValue(aircraftJson, null);

            Assert.AreEqual(expected, actual, jsonProperty.Name);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Only_Copies_Values_If_They_Have_Changed()
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
                _Args.PreviousAircraft.Add(1);

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
                _Args.PreviousDataVersion = -1L;
                aircraftJson = _Builder.Build(_Args).Aircraft[0];
                Assert.IsNotNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                aircraft.Setup(lambda).Returns(10L);
                aircraftJson = _Builder.Build(_Args).Aircraft[0];
                Assert.IsNotNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                // If the browser version is prior to the list version then the property must be returned
                _Args.PreviousDataVersion = 9L;
                aircraftJson = _Builder.Build(_Args).Aircraft[0];
                Assert.IsNotNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                // If the browser version is the same as the list version then the property must not be returned
                _Args.PreviousDataVersion = 10L;
                aircraftJson = _Builder.Build(_Args).Aircraft[0];
                Assert.IsNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                // If the browser version is the after the list version then the property must not be returned
                _Args.PreviousDataVersion = 11L;
                aircraftJson = _Builder.Build(_Args).Aircraft[0];
                Assert.IsNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);

                // If the browser version is after the list version, but the aircraft has not been seen before, then the property must be returned
                _Args.PreviousAircraft.Clear();
                aircraftJson = _Builder.Build(_Args).Aircraft[0];
                Assert.IsNotNull(jsonProperty.GetValue(aircraftJson, null), aircraftProperty.Name);
            }
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Always_Copies_Icao_If_AlwaysCopyIcao_Is_Set()
        {
            AddBlankAircraft(1);
            _AircraftLists[0][0].Icao24 = "A";

            _Args.AlwaysShowIcao = true;
            _Args.PreviousAircraft.Add(0);
            _Args.PreviousDataVersion = _AircraftLists[0][0].Icao24Changed;

            var aircraftJson = _Builder.Build(_Args);
            Assert.AreEqual("A", aircraftJson.Aircraft[0].Icao24);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Copes_With_Multiple_Aircraft_Identifiers()
        {
            AddBlankAircraft(3);
            _AircraftLists[0][0].Callsign = "0";
            _AircraftLists[0][1].Callsign = "1";
            _AircraftLists[0][2].Callsign = "2";

            _Args.PreviousDataVersion = _AircraftLists[0][0].CallsignChanged + 1;

            _Args.PreviousAircraft.Add(0);
            _Args.PreviousAircraft.Add(2);

            var aircraftJson = _Builder.Build(_Args);

            // We have 3 aircraft and we've told the list builder that we know about the 1st and 3rd entries in the list and we already know the
            // callsigns. The second one is unknown to us, so it must send us everything it knows about it

            Assert.IsNull(aircraftJson.Aircraft.Where(a => a.UniqueId == 0).First().Callsign);
            Assert.AreEqual("1", aircraftJson.Aircraft.Where(a => a.UniqueId == 1).First().Callsign);
            Assert.IsNull(aircraftJson.Aircraft.Where(a => a.UniqueId == 2).First().Callsign);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListBearing$")]
        public void AircraftListJsonBuilder_Calculates_Bearing_From_Browser_To_Aircraft_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AircraftLists[0].Add(aircraft.Object);

            aircraft.Object.Latitude = worksheet.NFloat("AircraftLatitude");
            aircraft.Object.Longitude = worksheet.NFloat("AircraftLongitude");

            _Args.BrowserLatitude = worksheet.NDouble("BrowserLatitude");
            _Args.BrowserLongitude = worksheet.NDouble("BrowserLongitude");

            var list = _Builder.Build(_Args);
            Assert.AreEqual(1, list.Aircraft.Count);
            var aircraftJson = list.Aircraft[0];

            double? expected = worksheet.NDouble("Bearing");
            if(expected == null) Assert.IsNull(aircraftJson.BearingFromHere);
            else Assert.AreEqual((double)expected, (double)aircraftJson.BearingFromHere);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListDistance$")]
        public void AircraftListJsonBuilder_Calculates_Distances_From_Browser_To_Aircraft_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AircraftLists[0].Add(aircraft.Object);

            aircraft.Object.Latitude = worksheet.NFloat("AircraftLatitude");
            aircraft.Object.Longitude = worksheet.NFloat("AircraftLongitude");

            _Args.BrowserLatitude = worksheet.NDouble("BrowserLatitude");
            _Args.BrowserLongitude = worksheet.NDouble("BrowserLongitude");

            var list = _Builder.Build(_Args);
            Assert.AreEqual(1, list.Aircraft.Count);
            var aircraftJson = list.Aircraft[0];

            double? expected = worksheet.NDouble("Distance");
            if(expected == null) Assert.IsNull(aircraftJson.DistanceFromHere);
            else Assert.AreEqual((double)expected, (double)aircraftJson.DistanceFromHere);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListDistance$")]
        public void AircraftListJsonBuilder_Calculates_Distances_From_Browser_To_Aircraft_Correctly_When_Culture_Is_Not_UK()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            using(var switcher = new CultureSwitcher("de-DE")) {
                var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
                _AircraftLists[0].Add(aircraft.Object);

                aircraft.Object.Latitude = worksheet.NFloat("AircraftLatitude");
                aircraft.Object.Longitude = worksheet.NFloat("AircraftLongitude");

                _Args.BrowserLatitude = worksheet.NDouble("BrowserLatitude");
                _Args.BrowserLongitude = worksheet.NDouble("BrowserLongitude");

                var list = _Builder.Build(_Args);
                Assert.AreEqual(1, list.Aircraft.Count);
                var aircraftJson = list.Aircraft[0];

                double? expected = worksheet.NDouble("Distance");
                if(expected == null) Assert.IsNull(aircraftJson.DistanceFromHere);
                else Assert.AreEqual((double)expected, (double)aircraftJson.DistanceFromHere);
            }
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Does_Not_Calculate_Distances_For_FSX_List()
        {
            _Args.IsFlightSimulatorList = true;
            _Args.AircraftList = _FlightSimulatorAircraftList.Object;
            AddBlankFlightSimAircraft(1);
            var aircraft = _FlightSimulatorAircraft[0];

            aircraft.Latitude = aircraft.Longitude = 1f;

            _Args.BrowserLatitude = _Args.BrowserLongitude = 2.0;

            var list = _Builder.Build(_Args);
            Assert.IsNull(list.Aircraft[0].DistanceFromHere);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Copies_Stopovers_Array_From_IAircraft_To_AircraftJson()
        {
            var aircraft = new Mock<IAircraft>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            var stopovers = new List<string>();
            aircraft.Setup(r => r.Stopovers).Returns(stopovers);

            _AircraftLists[0].Add(aircraft.Object);

            var list = _Builder.Build(_Args);
            Assert.IsNull(list.Aircraft[0].Stopovers);

            stopovers.Add("Stop 1");
            stopovers.Add("Stop 2");
            list = _Builder.Build(_Args);
            Assert.AreEqual(2, list.Aircraft[0].Stopovers.Count);
            Assert.AreEqual("Stop 1", list.Aircraft[0].Stopovers[0]);
            Assert.AreEqual("Stop 2", list.Aircraft[0].Stopovers[1]);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListCoordinates$")]
        public void AircraftListJsonBuilder_Builds_Arrays_Of_Trail_Coordinates_Correctly()
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

            _Args.TrailType = worksheet.Bool("ArgsShort") ? TrailType.Short : TrailType.Full;
            _Args.PreviousDataVersion = worksheet.Long("ArgsPrevDV");
            if(worksheet.Bool("ArgsIsPrevAC")) _Args.PreviousAircraft.Add(0);
            _Args.ResendTrails = worksheet.Bool("ArgsResend");

            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

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
        public void AircraftListJsonBuilder_Sends_No_Trails_If_None_Are_Requested()
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

            _Args.TrailType = TrailType.None;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

            Assert.AreEqual(null, aircraftJson.TrailType);
            Assert.AreEqual(null, aircraftJson.FullCoordinates);
            Assert.AreEqual(null, aircraftJson.ShortCoordinates);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sends_ShortAltitude_Trails_If_Requested()
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

            _Args.TrailType = TrailType.ShortAltitude;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

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
        public void AircraftListJsonBuilder_Sends_ShortSpeed_Trails_If_Requested()
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

            _Args.TrailType = TrailType.ShortSpeed;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

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
        public void AircraftListJsonBuilder_Ignores_FullTrackCoordinates_Where_The_Only_Change_Is_In_Altitude()
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

            _Args.TrailType = TrailType.Full;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

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
        public void AircraftListJsonBuilder_Ignores_FullTrackCoordinates_Where_The_Only_Change_Is_In_Speed()
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

            _Args.TrailType = TrailType.Full;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

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
        public void AircraftListJsonBuilder_Sends_All_Altitude_FullTrackCoordinates_When_FullTrack_Altitude_Is_Requested()
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

            _Args.TrailType = TrailType.FullAltitude;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

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
        public void AircraftListJsonBuilder_Sends_All_Speed_FullTrackCoordinates_When_FullTrack_Speed_Is_Requested()
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

            _Args.TrailType = TrailType.FullSpeed;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

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
        public void AircraftListJsonBuilder_Skips_Speed_Changes_In_FullTrackCoordinates_When_FullTrack_Altitude_Is_Requested()
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

            _Args.TrailType = TrailType.FullAltitude;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

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
        public void AircraftListJsonBuilder_Skips_Altitude_Changes_In_FullTrackCoordinates_When_FullTrack_Speed_Is_Requested()
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

            _Args.TrailType = TrailType.FullSpeed;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

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
        public void  AircraftListJsonBuilder_Rounds_Altitude_When_Generating_Extra_Coordinates_For_FullTrackCoordinates()
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

            _Args.TrailType = TrailType.FullAltitude;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

            // We should get 2 coordinates and the altitude should have been rounded
            Assert.AreEqual(8, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual("a", aircraftJson.TrailType);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[5]);
            Assert.AreEqual(10, aircraftJson.FullCoordinates[6]);       // gets rounded
            Assert.AreEqual(10000, aircraftJson.FullCoordinates[7]);    // gets rounded
        }

        [TestMethod]
        public void  AircraftListJsonBuilder_Rounds_Speed_When_Generating_Extra_Coordinates_For_FullTrackCoordinates()
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

            _Args.TrailType = TrailType.FullSpeed;
            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

            // We should get 2 coordinates and the altitude should have been rounded
            Assert.AreEqual(8, aircraftJson.FullCoordinates.Count);
            Assert.AreEqual("s", aircraftJson.TrailType);

            Assert.AreEqual(4, aircraftJson.FullCoordinates[4]);
            Assert.AreEqual(5, aircraftJson.FullCoordinates[5]);
            Assert.AreEqual(10, aircraftJson.FullCoordinates[6]);     // gets rounded
            Assert.AreEqual(210, aircraftJson.FullCoordinates[7]);    // gets rounded
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Calculates_SecondsTracked_From_Server_Time_And_FirstSeen_Property()
        {
            AddBlankAircraft(1);
            var aircraft = _AircraftLists[0][0];
            Mock<IAircraft> mockAircraft = Mock.Get(aircraft);
            mockAircraft.Setup(a => a.FirstSeen).Returns(new DateTime(2001, 1, 1, 1, 2, 0));
            _Clock.Setup(p => p.UtcNow).Returns(new DateTime(2001, 1, 1, 1, 3, 17));

            var aircraftJson = _Builder.Build(_Args).Aircraft[0];

            Assert.AreEqual(77L, aircraftJson.SecondsTracked);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Only_Sends_Message_Fields_If_OnlySendMessageFields_Is_Set()
        {
            AddBlankAircraft(1);
            var aircraft = Mock.Get(_AircraftLists[0][0]);
            var properties = typeof(IAircraft).GetProperties().Where(r => !r.Name.EndsWith("Changed") && typeof(IAircraft).GetProperty(String.Format("{0}Changed", r.Name)) != null).ToArray();

            aircraft.Object.UniqueId = 7;
            aircraft.Object.DataVersion = 1;
            aircraft.Object.FirstSeen = new DateTime(2000, 1, 1);

            var dateTimeValue = DateTime.UtcNow;
            var altitudeTypeValue = AltitudeType.Geometric;
            var speedTypeValue = SpeedType.GroundSpeedReversing;
            var transponderType = TransponderType.Adsb2;
            foreach(var property in properties) {
                if(property.PropertyType == typeof(string))                         property.SetValue(aircraft.Object, "TEXT", null);
                else if(property.PropertyType == typeof(long))                      property.SetValue(aircraft.Object, 1L, null);
                else if(property.PropertyType == typeof(long?))                     property.SetValue(aircraft.Object, 1L, null);
                else if(property.PropertyType == typeof(int))                       property.SetValue(aircraft.Object, 1, null);
                else if(property.PropertyType == typeof(int?))                      property.SetValue(aircraft.Object, 1, null);
                else if(property.PropertyType == typeof(bool))                      property.SetValue(aircraft.Object, true, null);
                else if(property.PropertyType == typeof(bool?))                     property.SetValue(aircraft.Object, true, null);
                else if(property.PropertyType == typeof(double))                    property.SetValue(aircraft.Object, 1.0, null);
                else if(property.PropertyType == typeof(double?))                   property.SetValue(aircraft.Object, 1.0, null);
                else if(property.PropertyType == typeof(float))                     property.SetValue(aircraft.Object, 1F, null);
                else if(property.PropertyType == typeof(float?))                    property.SetValue(aircraft.Object, 1F, null);
                else if(property.PropertyType == typeof(DateTime))                  property.SetValue(aircraft.Object, dateTimeValue, null);
                else if(property.PropertyType == typeof(DateTime?))                 property.SetValue(aircraft.Object, dateTimeValue, null);
                else if(property.PropertyType == typeof(AltitudeType))              property.SetValue(aircraft.Object, altitudeTypeValue, null);
                else if(property.PropertyType == typeof(SpeedType))                 property.SetValue(aircraft.Object, speedTypeValue, null);
                else if(property.PropertyType == typeof(TransponderType))           property.SetValue(aircraft.Object, transponderType, null);
                else if(property.PropertyType == typeof(EnginePlacement))           property.SetValue(aircraft.Object, EnginePlacement.AftMounted, null);
                else if(property.PropertyType == typeof(EngineType))                property.SetValue(aircraft.Object, EngineType.Electric, null);
                else if(property.PropertyType == typeof(Species))                   property.SetValue(aircraft.Object, Species.TiltWing, null);
                else if(property.PropertyType == typeof(WakeTurbulenceCategory))    property.SetValue(aircraft.Object, WakeTurbulenceCategory.Light, null);
                else if(property.PropertyType == typeof(ICollection<string>))       aircraft.Object.Stopovers.Add("A");
                else throw new NotImplementedException($"Need to add code to set dummy values for {property.PropertyType.Name} properties");
            }
            aircraft.Object.LastSatcomUpdate = DateTime.UtcNow;

            _Args.OnlyIncludeMessageFields = true;
            var json = _Builder.Build(_Args).Aircraft[0];

            foreach(var property in typeof(AircraftJson).GetProperties()) {
                var value = property.GetValue(json, null);
                bool isEmpty = true;
                if(!Object.Equals(value, null)) {
                    var type = property.PropertyType;
                    if(property.PropertyType.IsEnum) type = Enum.GetUnderlyingType(type);
                    if(type == typeof(byte))            isEmpty = Object.Equals((byte)0, (byte)value);
                    else if(type == typeof(short))      isEmpty = Object.Equals((short)0, (short)value);
                    else if(type == typeof(int))        isEmpty = Object.Equals((int)0, (int)value);
                    else if(type == typeof(long))       isEmpty = Object.Equals((long)0, (long)value);
                    else if(type == typeof(bool))       isEmpty = Object.Equals(default(bool), (bool)value);
                    else if(type == typeof(DateTime))   isEmpty = Object.Equals(default(DateTime), (DateTime)value);
                    else if(type == typeof(string))     isEmpty = Object.Equals(null, (string)value);
                    else if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                        isEmpty = Object.Equals(null, value);
                    } else if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) {
                        isEmpty = Object.Equals(null, value);
                    } else {
                        throw new NotImplementedException($"Need test code for JSON property type {property.PropertyType.Name}");
                    }
                }

                switch(property.Name) {
                    // All of these are properties that we want filled in
                    case nameof(AircraftJson.AirPressureInHg):
                    case nameof(AircraftJson.Altitude):
                    case nameof(AircraftJson.AltitudeType):
                    case nameof(AircraftJson.Callsign):
                    case nameof(AircraftJson.CallsignIsSuspect):
                    case nameof(AircraftJson.Emergency):
                    case nameof(AircraftJson.GeometricAltitude):
                    case nameof(AircraftJson.GroundSpeed):
                    case nameof(AircraftJson.HasSignalLevel):
                    case nameof(AircraftJson.Icao24):
                    case nameof(AircraftJson.IdentActive):
                    case nameof(AircraftJson.IsSatcomFeed):
                    case nameof(AircraftJson.IsTisb):
                    case nameof(AircraftJson.Latitude):
                    case nameof(AircraftJson.Longitude):
                    case nameof(AircraftJson.OnGround):
                    case nameof(AircraftJson.PositionIsMlat):
                    case nameof(AircraftJson.SignalLevel):
                    case nameof(AircraftJson.SpeedType):
                    case nameof(AircraftJson.Squawk):
                    case nameof(AircraftJson.TargetAltitude):
                    case nameof(AircraftJson.TargetTrack):
                    case nameof(AircraftJson.Track):
                    case nameof(AircraftJson.TrackIsHeading):
                    case nameof(AircraftJson.TransponderType):
                    case nameof(AircraftJson.VerticalRate):
                    case nameof(AircraftJson.VerticalRateType):
                        Assert.IsFalse(isEmpty, property.Name);
                        break;
                    // These are properties that get filled in whether we like it or not
                    case nameof(AircraftJson.UniqueId):
                        break;
                    // And everything else should be null / default
                    default:
                        Assert.IsTrue(isEmpty, property.Name);
                        break;
                }
            }
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sends_All_Latitude_Longitude_And_PositionIsMlat_When_One_Changes_And_OnlySendMessageFields_Is_Set()
        {
            for(var i = 0;i < 3;++i) {
                TestCleanup();
                TestInitialise();

                AddBlankAircraft(1);
                var aircraft = Mock.Get(_AircraftLists[0][0]);
                aircraft.Object.UniqueId = 7;
                aircraft.Object.DataVersion = 1;
                aircraft.Object.FirstSeen = new DateTime(2000, 1, 1);
                aircraft.Object.Latitude = 10;
                aircraft.Object.Longitude = 20;
                aircraft.Object.PositionIsMlat = true;
                aircraft.Setup(r => r.LatitudeChanged).Returns(1);
                aircraft.Setup(r => r.LongitudeChanged).Returns(1);
                aircraft.Setup(r => r.PositionIsMlatChanged).Returns(1);

                aircraft.Object.DataVersion = 2;
                switch(i) {
                    case 0:     aircraft.Object.Latitude = 7; aircraft.Setup(r => r.LatitudeChanged).Returns(2); break;
                    case 1:     aircraft.Object.Longitude = 7; aircraft.Setup(r => r.LongitudeChanged).Returns(2); break;
                    case 2:     aircraft.Object.PositionIsMlat = false; aircraft.Setup(r => r.PositionIsMlatChanged).Returns(2); break;
                    default:    throw new NotImplementedException();
                }

                _Args.OnlyIncludeMessageFields = true;
                _Args.PreviousDataVersion = 1;
                _Args.PreviousAircraft.Add(7);
                var json = _Builder.Build(_Args).Aircraft[0];

                Assert.AreEqual(i == 0 ? 7 : 10, json.Latitude);
                Assert.AreEqual(i == 1 ? 7 : 20, json.Longitude);
                Assert.AreEqual(i == 2 ? false : true, json.PositionIsMlat);
            }
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sends_Altitude_GeometricAltitude_And_AltitudeType_As_A_Set()
        {
            AddBlankAircraft(1);
            var aircraft = Mock.Get(_AircraftLists[0][0]);
            aircraft.Object.UniqueId = 7;
            aircraft.Object.DataVersion = 1;
            _Args.OnlyIncludeMessageFields = true;
            _Args.PreviousAircraft.Add(7);

            aircraft.Object.Altitude = 100;
            aircraft.Object.GeometricAltitude = 200;
            aircraft.Object.AltitudeType = AltitudeType.Barometric;

            _Args.PreviousDataVersion = 1;
            aircraft.Object.DataVersion = 2;
            aircraft.SetupGet(r => r.AltitudeChanged).Returns(2);
            var json = _Builder.Build(_Args).Aircraft[0];
            Assert.IsNotNull(json.Altitude);
            Assert.IsNotNull(json.AltitudeType);
            Assert.IsNotNull(json.GeometricAltitude);

            _Args.PreviousDataVersion = 2;
            aircraft.Object.DataVersion = 3;
            aircraft.SetupGet(r => r.GeometricAltitudeChanged).Returns(3);
            json = _Builder.Build(_Args).Aircraft[0];
            Assert.IsNotNull(json.Altitude);
            Assert.IsNotNull(json.AltitudeType);
            Assert.IsNotNull(json.GeometricAltitude);

            _Args.PreviousDataVersion = 3;
            aircraft.Object.DataVersion = 4;
            aircraft.SetupGet(r => r.AltitudeTypeChanged).Returns(4);
            json = _Builder.Build(_Args).Aircraft[0];
            Assert.IsNotNull(json.Altitude);
            Assert.IsNotNull(json.AltitudeType);
            Assert.IsNotNull(json.GeometricAltitude);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sends_No_Coordinate_If_Nothing_Changes_And_OnlySendMessageFields_Is_Set()
        {
            for(var i = 0;i < 3;++i) {
                TestCleanup();
                TestInitialise();

                AddBlankAircraft(1);
                var aircraft = Mock.Get(_AircraftLists[0][0]);
                aircraft.Object.UniqueId = 7;
                aircraft.Object.DataVersion = 1;
                aircraft.Object.FirstSeen = new DateTime(2000, 1, 1);
                aircraft.Object.Latitude = 10;
                aircraft.Object.Longitude = 20;
                aircraft.Object.PositionIsMlat = true;
                aircraft.Setup(r => r.LatitudeChanged).Returns(1);
                aircraft.Setup(r => r.LongitudeChanged).Returns(1);
                aircraft.Setup(r => r.PositionIsMlatChanged).Returns(1);

                aircraft.Object.DataVersion = 2;

                _Args.OnlyIncludeMessageFields = true;
                _Args.PreviousDataVersion = 1;
                _Args.PreviousAircraft.Add(7);
                var json = _Builder.Build(_Args).Aircraft[0];

                Assert.IsNull(json.Latitude);
                Assert.IsNull(json.Longitude);
                Assert.IsNull(json.PositionIsMlat);
            }
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Does_Not_Include_Unchanged_Aircraft_If_IgnoreUnchanged_Is_Set()
        {
            AddBlankAircraft(2);
            var aircraft1 = Mock.Get(_AircraftLists[0][0]);
            var aircraft2 = Mock.Get(_AircraftLists[0][1]);

            aircraft1.Object.DataVersion = 100;
            aircraft2.Object.DataVersion = 101;

            _Args.PreviousDataVersion = 100;
            _Args.IgnoreUnchanged = true;
            var json = _Builder.Build(_Args);

            Assert.AreEqual(1, json.Aircraft.Count);
            Assert.AreEqual(aircraft2.Object.UniqueId, json.Aircraft[0].UniqueId);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Sets_PositionIsStale_To_Either_True_Or_Null()
        {
            // In 2.2 positions were marked as stale if the last position message was longer
            // than <display timeout> seconds ago. In 2.2.1 I have revised this so that it is
            // stale when the message is longer than:
            //   <display timeout seconds> + 60 seconds
            // This is to account for aircraft that are at an extreme range and a non-position
            // message comes in some time after the last position message. In those cases the
            // aircraft was removed from the map before it was removed from the list. 60 seconds
            // should hopefully be enough to cover that, and still be short enough to remove the
            // aircraft from the map when they were getting positions from an MLAT feed that
            // they're now out of range of.

            var now = DateTime.UtcNow;
            var boostSeconds = 60;

            foreach(var threshold in new string[] { "before", "on", "after" }) {
                TestCleanup();
                TestInitialise();

                _Clock.SetupGet(r => r.UtcNow).Returns(now);
                _Configuration.BaseStationSettings.DisplayTimeoutSeconds = 30;

                AddBlankAircraft(1);
                var aircraft = Mock.Get(_AircraftLists[0][0]);
                aircraft.SetupGet(r => r.Latitude).Returns(1.0);
                aircraft.SetupGet(r => r.Longitude).Returns(1.0);
                aircraft.SetupGet(r => r.PositionTime).Returns(now);

                now = now.AddSeconds(_Configuration.BaseStationSettings.DisplayTimeoutSeconds + boostSeconds);
                switch(threshold) {
                    case "before":  now = now.AddMilliseconds(-1); break;
                    case "on":      break;
                    case "after":   now = now.AddMilliseconds(1); break;
                }
                _Clock.SetupGet(r => r.UtcNow).Returns(now);

                var json = _Builder.Build(_Args);
                var jsonAircraft = json.Aircraft[0];

                switch(threshold) {
                    case "before":  Assert.AreEqual(null, jsonAircraft.PositionIsStale); break;
                    case "on":      Assert.AreEqual(null, jsonAircraft.PositionIsStale); break;
                    case "after":   Assert.AreEqual(true, jsonAircraft.PositionIsStale); break;
                }
            }
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Does_Not_Set_PositionIsStale_On_Satcom_Aircraft()
        {
            var now = DateTime.UtcNow;
            var boostSeconds = 60;

            _Clock.SetupGet(r => r.UtcNow).Returns(now);
            _Configuration.BaseStationSettings.DisplayTimeoutSeconds = 30;
            AddBlankAircraft(1);
            var aircraft = Mock.Get(_AircraftLists[0][0]);
            aircraft.SetupGet(r => r.Latitude).Returns(1.0);
            aircraft.SetupGet(r => r.Longitude).Returns(1.0);
            aircraft.SetupGet(r => r.PositionTime).Returns(now);
            aircraft.SetupGet(r => r.LastSatcomUpdate).Returns(DateTime.MinValue.AddMilliseconds(1));

            now = now.AddSeconds(_Configuration.BaseStationSettings.DisplayTimeoutSeconds + boostSeconds + 1);
            _Clock.SetupGet(r => r.UtcNow).Returns(now);

            var json = _Builder.Build(_Args);
            var jsonAircraft = json.Aircraft[0];

            // By this point the setup should be the same as for the "after" position in the
            // AircraftListJsonBuilder_Sets_PositionIsStale_To_Either_True_Or_Null test. The only difference
            // is the setting of a LastSatcomUpdate value, which indicates that the aircraft can't have its
            // display timed out.

            Assert.AreEqual(null, jsonAircraft.PositionIsStale);
        }

        [TestMethod]
        public void AircraftListJsonBuilder_In_FSX_Mode_Never_Sets_Stale_Position()
        {
            _Args.IsFlightSimulatorList = true;
            _Args.AircraftList = _FlightSimulatorAircraftList.Object;
            AddBlankFlightSimAircraft(1);
            _FlightSimulatorAircraft[0].PositionTime = new DateTime(2010, 1, 1);

            _Clock.SetupGet(r => r.UtcNow).Returns(new DateTime(2018, 1, 1));
            var json = _Builder.Build(_Args);

            Assert.IsNull(json.Aircraft[0].PositionIsStale);
        }
        #endregion

        #region Sorting of list
        [TestMethod]
        public void AircraftListJsonBuilder_Defaults_To_No_Sorting()
        {
            AddBlankAircraft(2);

            DateTime baseTime = new DateTime(2001, 1, 2, 0, 0, 0);
            DateTime loTime = baseTime.AddSeconds(1);
            DateTime hiTime = baseTime.AddSeconds(2);

            for(int order = 0;order < 2;++order) {
                _AircraftLists[0][0].FirstSeen = order == 0 ? hiTime : loTime;
                _AircraftLists[0][1].FirstSeen = order == 0 ? loTime : hiTime;

                var list = _Builder.Build(_Args).Aircraft;

                Assert.AreEqual(order == 0 ? hiTime : loTime, list[0].FirstSeen);
                Assert.AreEqual(order == 0 ? loTime : hiTime, list[1].FirstSeen);
            }
        }

        [TestMethod]
        public void AircraftListJsonBuilder_Will_Sort_List_By_Single_Column()
        {
            _Args.BrowserLatitude = _Args.BrowserLongitude = 0;

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

                            _Args.SortBy.Clear();
                            _Args.SortBy.Add(new KeyValuePair<string,bool>(sortColumn, sortDescending == 0));

                            _AircraftLists[0].Clear();
                            AddBlankAircraft(2);
                            var lhs = _AircraftLists[0][initialOrder == 0 ? 0 : 1];
                            var rhs = _AircraftLists[0][initialOrder == 0 ? 1 : 0];
                            if(sortColumn == AircraftComparerColumn.FirstSeen) lhs.FirstSeen = rhs.FirstSeen = DateTime.MinValue;

                            if(pass != 2) PrepareForSortTest(sortColumn, lhs, pass != 1);
                            if(pass != 3) PrepareForSortTest(sortColumn, rhs, pass != 0);

                            var list = _Builder.Build(_Args).Aircraft;

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
        public void AircraftListJsonBuilder_Will_Sort_List_By_Two_Columns()
        {
            _Args.SortBy.Add(new KeyValuePair<string,bool>(AircraftComparerColumn.Altitude, true));
            _Args.SortBy.Add(new KeyValuePair<string,bool>(AircraftComparerColumn.Registration, true));

            AddBlankAircraft(2);

            for(int order = 0;order < 2;++order) {
                _AircraftLists[0][0].Registration = order == 0 ? "A" : "B";
                _AircraftLists[0][1].Registration = order == 0 ? "B" : "A";

                var list = _Builder.Build(_Args).Aircraft;

                Assert.AreEqual("A", list[0].Registration);
                Assert.AreEqual("B", list[1].Registration);
            }
        }
        #endregion
    }
}
