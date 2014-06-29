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
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using System.Globalization;

namespace Test.VirtualRadar.Library.Presenter
{
    [TestClass]
    public class OptionsPresenterTests
    {
        #region Private class - ConfigurationProperty
        /// <summary>
        /// An internal class that simplifies the checking of large groups of configuration properties.
        /// </summary>
        /// <remarks>
        /// The main benefit of using this class is that it keeps the number of columns required in the spreadsheet data
        /// under control. The down-side is that we only check one property at a time, which means we're not checking for
        /// any side-effects.
        /// </remarks>
        class ConfigurationProperty
        {
            public string Name;
            public Func<ExcelWorksheetData, string, object> GetSpreadsheetConfig;
            public Func<ExcelWorksheetData, string, object> GetSpreadsheetView;
            public Func<Configuration, object>              GetConfig;
            public Action<Configuration, object>            SetConfig;
            public Func<IOptionsView, object>               GetView;
            public Action<IOptionsView, object>             SetView;

            public ConfigurationProperty()
            {
            }

            public ConfigurationProperty(string name, Func<ExcelWorksheetData, string, object> getSpreadsheet, Func<Configuration, object> getConfig, Action<Configuration, object> setConfig, Func<IOptionsView, object> getView, Action<IOptionsView, object> setView)
            {
                Name = name;
                GetSpreadsheetConfig = GetSpreadsheetView = getSpreadsheet;
                GetConfig = getConfig;
                SetConfig = setConfig;
                GetView = getView;
                SetView = setView;
            }

            public ConfigurationProperty(string name, Func<ExcelWorksheetData, string, object> getSpreadsheetConfig, Func<ExcelWorksheetData, string, object> getSpreadsheetView, Func<Configuration, object> getConfig, Action<Configuration, object> setConfig, Func<IOptionsView, object> getView, Action<IOptionsView, object> setView) : this(name, getSpreadsheetConfig, getConfig, setConfig, getView, setView)
            {
                GetSpreadsheetView = getSpreadsheetView;
            }

            public override string ToString()
            {
                return Name ?? base.ToString();
            }
        }
        #endregion

        #region TestContext, fields etc.
        public TestContext TestContext { get; set; }

        private IClassFactory _ClassFactorySnapshot;
        private IOptionsPresenter _Presenter;
        private Mock<IOptionsPresenterProvider> _Provider;
        private Mock<IOptionsView> _View;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Mock<IAutoConfigBaseStationDatabase> _AutoConfigBaseStationDatabase;
        private Mock<IBaseStationDatabase> _BaseStationDatabase;
        private List<MergedFeed> _ViewMergedFeeds;
        private List<ReceiverLocation> _ViewRawDecodingReceiverLocations;
        private List<RebroadcastSettings> _ViewRebroadcastSettings;
        private List<Receiver> _ViewReceivers;
        private List<string> _ViewWebServerUserIds;
        private Mock<IUserManager> _UserManager;
        private List<IUser> _UserManagerUsers;
        private List<IUser> _ViewUsers;

        [TestInitialize]
        public void TestInitialise()
        {
            _ClassFactorySnapshot = Factory.TakeSnapshot();

            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _Configuration = new Configuration();
            _Configuration.Receivers.Add(new Receiver() { Enabled = true, UniqueId = 1, Name = "DEFAULT" });
            _Configuration.Receivers.Add(new Receiver() { Enabled = true, UniqueId = 2, Name = "SECOND" });
            _Configuration.MergedFeeds.Add(new MergedFeed() { Enabled = true, UniqueId = 3, Name = "MERGED", ReceiverIds = { 1, 2 } });
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 1;
            _Configuration.GoogleMapSettings.ClosestAircraftReceiverId = 1;
            _Configuration.GoogleMapSettings.FlightSimulatorXReceiverId = 1;
            _ConfigurationStorage.Setup(c => c.Load()).Returns(_Configuration);

            _AutoConfigBaseStationDatabase = TestUtilities.CreateMockSingleton<IAutoConfigBaseStationDatabase>();
            _BaseStationDatabase = new Mock<IBaseStationDatabase>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _AutoConfigBaseStationDatabase.Setup(r => r.Database).Returns(_BaseStationDatabase.Object);

            _UserManager = TestUtilities.CreateMockSingleton<IUserManager>();
            _UserManagerUsers = new List<IUser>();
            _UserManager.Setup(r => r.GetUsers()).Returns(_UserManagerUsers);

            _Presenter = Factory.Singleton.Resolve<IOptionsPresenter>();
            _Provider = new Mock<IOptionsPresenterProvider>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(true);
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Returns(true);
            _Provider.Setup(p => p.TestNetworkConnection(It.IsAny<string>(), It.IsAny<int>())).Returns((Exception)null);
            _Provider.Setup(p => p.TestSerialConnection(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StopBits>(), It.IsAny<Parity>(), It.IsAny<Handshake>())).Returns((Exception)null);
            _Presenter.Provider = _Provider.Object;

            _ViewMergedFeeds = new List<MergedFeed>();
            _ViewRawDecodingReceiverLocations = new List<ReceiverLocation>();
            _ViewRebroadcastSettings = new List<RebroadcastSettings>();
            _ViewReceivers = new List<Receiver>();
            _ViewUsers = new List<IUser>();
            _ViewWebServerUserIds = new List<string>();

            _View = new Mock<IOptionsView>() { DefaultValue = DefaultValue.Mock }.SetupAllProperties();
            _View.Setup(r => r.RawDecodingReceiverLocations).Returns(_ViewRawDecodingReceiverLocations);
            _View.Setup(r => r.MergedFeeds).Returns(_ViewMergedFeeds);
            _View.Setup(r => r.RebroadcastSettings).Returns(_ViewRebroadcastSettings);
            _View.Setup(r => r.Receivers).Returns(_ViewReceivers);
            _View.Setup(r => r.Users).Returns(_ViewUsers);
            _View.Setup(r => r.WebServerUserIds).Returns(_ViewWebServerUserIds);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_ClassFactorySnapshot);
        }
        #endregion

        #region Configuration Property Declarations
        private readonly List<ConfigurationProperty> _BaseStationProperties = new List<ConfigurationProperty>() {
            new ConfigurationProperty("DatabaseFileName",               (w, c) => w.EString(c),                   r => r.BaseStationSettings.DatabaseFileName,         (r, v) => r.BaseStationSettings.DatabaseFileName = (string)v,       r => r.BaseStationDatabaseFileName,      (r, v) => r.BaseStationDatabaseFileName = (string)v),
            new ConfigurationProperty("OperatorFlagsFolder",            (w, c) => w.EString(c),                   r => r.BaseStationSettings.OperatorFlagsFolder,      (r, v) => r.BaseStationSettings.OperatorFlagsFolder = (string)v,    r => r.OperatorFlagsFolder,              (r, v) => r.OperatorFlagsFolder = (string)v),
            new ConfigurationProperty("SilhouettesFolder",              (w, c) => w.EString(c),                   r => r.BaseStationSettings.SilhouettesFolder,        (r, v) => r.BaseStationSettings.SilhouettesFolder = (string)v,      r => r.SilhouettesFolder,                (r, v) => r.SilhouettesFolder = (string)v),
            new ConfigurationProperty("PicturesFolder",                 (w, c) => w.EString(c),                   r => r.BaseStationSettings.PicturesFolder,           (r, v) => r.BaseStationSettings.PicturesFolder = (string)v,         r => r.PicturesFolder,                   (r, v) => r.PicturesFolder = (string)v),
            new ConfigurationProperty("SearchPictureSubFolders",        (w, c) => w.Bool(c),                      r => r.BaseStationSettings.SearchPictureSubFolders,  (r, v) => r.BaseStationSettings.SearchPictureSubFolders = (bool)v,  r => r.SearchPictureSubFolders,          (r, v) => r.SearchPictureSubFolders = (bool)v),
        };

        private readonly List<ConfigurationProperty> _AudioProperties = new List<ConfigurationProperty>() {
            new ConfigurationProperty("Enabled",    (w, c) => w.Bool(c),    r => r.AudioSettings.Enabled,   (r, v) => r.AudioSettings.Enabled = (bool)v,     r => r.AudioEnabled,      (r, v) => r.AudioEnabled = (bool)v),
            new ConfigurationProperty("VoiceName",  (w, c) => w.EString(c), r => r.AudioSettings.VoiceName, (r, v) => r.AudioSettings.VoiceName = (string)v, r => r.TextToSpeechVoice, (r, v) => r.TextToSpeechVoice = (string)v),
            new ConfigurationProperty("VoiceRate",  (w, c) => w.Int(c),     r => r.AudioSettings.VoiceRate, (r, v) => r.AudioSettings.VoiceRate = (int)v,    r => r.TextToSpeechSpeed, (r, v) => r.TextToSpeechSpeed = (int)v),
        };

        private readonly List<ConfigurationProperty> _GeneralProperties = new List<ConfigurationProperty>() {
            new ConfigurationProperty("CheckAutomatically",      (w, c) => w.Bool(c),   r => r.VersionCheckSettings.CheckAutomatically,     (r, v) => r.VersionCheckSettings.CheckAutomatically = (bool)v,      r => r.CheckForNewVersions,           (r, v) => r.CheckForNewVersions = (bool)v),
            new ConfigurationProperty("CheckPeriodDays",         (w, c) => w.Int(c),    r => r.VersionCheckSettings.CheckPeriodDays,        (r, v) => r.VersionCheckSettings.CheckPeriodDays = (int)v,          r => r.CheckForNewVersionsPeriodDays, (r, v) => r.CheckForNewVersionsPeriodDays = (int)v),
            new ConfigurationProperty("AutoUpdateEnabled",       (w, c) => w.Bool(c),   r => r.FlightRouteSettings.AutoUpdateEnabled,       (r, v) => r.FlightRouteSettings.AutoUpdateEnabled = (bool)v,        r => r.DownloadFlightRoutes,          (r, v) => r.DownloadFlightRoutes = (bool)v),
            new ConfigurationProperty("DisplayTimeoutSeconds",   (w, c) => w.Int(c),    r => r.BaseStationSettings.DisplayTimeoutSeconds,   (r, v) => r.BaseStationSettings.DisplayTimeoutSeconds = (int)v,     r => r.DisplayTimeoutSeconds,         (r, v) => r.DisplayTimeoutSeconds = (int)v),
            new ConfigurationProperty("TrackingTimeoutSeconds",  (w, c) => w.Int(c),    r => r.BaseStationSettings.TrackingTimeoutSeconds,  (r, v) => r.BaseStationSettings.TrackingTimeoutSeconds = (int)v,    r => r.TrackingTimeoutSeconds,        (r, v) => r.TrackingTimeoutSeconds = (int)v),
            new ConfigurationProperty("ShortTrailLengthSeconds", (w, c) => w.Int(c),    r => r.GoogleMapSettings.ShortTrailLengthSeconds,   (r, v) => r.GoogleMapSettings.ShortTrailLengthSeconds = (int)v,     r => r.ShortTrailLengthSeconds,       (r, v) => r.ShortTrailLengthSeconds = (int)v),
            new ConfigurationProperty("MinimiseToSystemTray",    (w, c) => w.Bool(c),   r => r.BaseStationSettings.MinimiseToSystemTray,    (r, v) => r.BaseStationSettings.MinimiseToSystemTray = (bool)v,     r => r.MinimiseToSystemTray,          (r, v) => r.MinimiseToSystemTray = (bool)v),
        };

        private readonly List<ConfigurationProperty> _GoogleMapOptions = new List<ConfigurationProperty>() {
            new ConfigurationProperty("InitialMapLatitude",         (w, c) => w.Double(c),                  r => r.GoogleMapSettings.InitialMapLatitude,            (r, v) => r.GoogleMapSettings.InitialMapLatitude = (double)v,        r => r.InitialGoogleMapLatitude,       (r, v) => r.InitialGoogleMapLatitude = (double)v),
            new ConfigurationProperty("InitialMapLongitude",        (w, c) => w.Double(c),                  r => r.GoogleMapSettings.InitialMapLongitude,           (r, v) => r.GoogleMapSettings.InitialMapLongitude = (double)v,       r => r.InitialGoogleMapLongitude,      (r, v) => r.InitialGoogleMapLongitude = (double)v),
            new ConfigurationProperty("InitialMapType",             (w, c) => w.EString(c),                 r => r.GoogleMapSettings.InitialMapType,                (r, v) => r.GoogleMapSettings.InitialMapType = (string)v,            r => r.InitialGoogleMapType,           (r, v) => r.InitialGoogleMapType = (string)v),
            new ConfigurationProperty("InitialMapZoom",             (w, c) => w.Int(c),                     r => r.GoogleMapSettings.InitialMapZoom,                (r, v) => r.GoogleMapSettings.InitialMapZoom = (int)v,               r => r.InitialGoogleMapZoom,           (r, v) => r.InitialGoogleMapZoom = (int)v),
            new ConfigurationProperty("InitialRefreshSeconds",      (w, c) => w.Int(c),                     r => r.GoogleMapSettings.InitialRefreshSeconds,         (r, v) => r.GoogleMapSettings.InitialRefreshSeconds = (int)v,        r => r.InitialGoogleMapRefreshSeconds, (r, v) => { r.InitialGoogleMapRefreshSeconds = r.MinimumGoogleMapRefreshSeconds = (int)v; }),
            new ConfigurationProperty("MinimumRefreshSeconds",      (w, c) => w.Int(c),                     r => r.GoogleMapSettings.MinimumRefreshSeconds,         (r, v) => r.GoogleMapSettings.MinimumRefreshSeconds = (int)v,        r => r.MinimumGoogleMapRefreshSeconds, (r, v) => { r.MinimumGoogleMapRefreshSeconds = r.InitialGoogleMapRefreshSeconds = (int)v; }),
            new ConfigurationProperty("InitialDistanceUnit",        (w, c) => w.ParseEnum<DistanceUnit>(c), r => r.GoogleMapSettings.InitialDistanceUnit,           (r, v) => r.GoogleMapSettings.InitialDistanceUnit = (DistanceUnit)v, r => r.InitialDistanceUnit,            (r, v) => r.InitialDistanceUnit = (DistanceUnit)v),
            new ConfigurationProperty("InitialHeightUnit",          (w, c) => w.ParseEnum<HeightUnit>(c),   r => r.GoogleMapSettings.InitialHeightUnit,             (r, v) => r.GoogleMapSettings.InitialHeightUnit = (HeightUnit)v,     r => r.InitialHeightUnit,              (r, v) => r.InitialHeightUnit = (HeightUnit)v),
            new ConfigurationProperty("InitialSpeedUnit",           (w, c) => w.ParseEnum<SpeedUnit>(c),    r => r.GoogleMapSettings.InitialSpeedUnit,              (r, v) => r.GoogleMapSettings.InitialSpeedUnit = (SpeedUnit)v,       r => r.InitialSpeedUnit,               (r, v) => r.InitialSpeedUnit = (SpeedUnit)v),
            new ConfigurationProperty("PreferIataAirportCodes",     (w, c) => w.Bool(c),                    r => r.GoogleMapSettings.PreferIataAirportCodes,        (r, v) => r.GoogleMapSettings.PreferIataAirportCodes = (bool)v,      r => r.PreferIataAirportCodes,         (r, v) => r.PreferIataAirportCodes = (bool)v),
            new ConfigurationProperty("EnableBundling",             (w, c) => w.Bool(c),                    r => r.GoogleMapSettings.EnableBundling,                (r, v) => r.GoogleMapSettings.EnableBundling = (bool)v,              r => r.EnableBundling,                 (r, v) => r.EnableBundling = (bool)v),
            new ConfigurationProperty("EnableMinifying",            (w, c) => w.Bool(c),                    r => r.GoogleMapSettings.EnableMinifying,               (r, v) => r.GoogleMapSettings.EnableMinifying = (bool)v,             r => r.EnableMinifying,                (r, v) => r.EnableMinifying = (bool)v),
            new ConfigurationProperty("EnableCompression",          (w, c) => w.Bool(c),                    r => r.GoogleMapSettings.EnableCompression,             (r, v) => r.GoogleMapSettings.EnableCompression = (bool)v,           r => r.EnableCompression,              (r, v) => r.EnableCompression = (bool)v),
            new ConfigurationProperty("ProxyType",                  (w, c) => w.ParseEnum<ProxyType>(c),    r => r.GoogleMapSettings.ProxyType,                     (r, v) => r.GoogleMapSettings.ProxyType = (ProxyType)v,              r => r.ProxyType,                      (r, v) => r.ProxyType = (ProxyType)v),
            new ConfigurationProperty("WebSiteReceiverId",          (w, c) => w.Int(c),                     r => r.GoogleMapSettings.WebSiteReceiverId,             (r, v) => r.GoogleMapSettings.WebSiteReceiverId = (int)v,            r => r.WebSiteReceiverId,              (r, v) => { r.WebSiteReceiverId = (int)v; r.Receivers[0].UniqueId = r.WebSiteReceiverId; }),
            new ConfigurationProperty("ClosestAircraftReceiverId",  (w, c) => w.Int(c),                     r => r.GoogleMapSettings.ClosestAircraftReceiverId,     (r, v) => r.GoogleMapSettings.ClosestAircraftReceiverId = (int)v,    r => r.ClosestAircraftReceiverId,      (r, v) => { r.ClosestAircraftReceiverId = (int)v; r.Receivers[0].UniqueId = r.ClosestAircraftReceiverId; }),
            new ConfigurationProperty("FlightSimulatorXReceiverId", (w, c) => w.Int(c),                     r => r.GoogleMapSettings.FlightSimulatorXReceiverId,    (r, v) => r.GoogleMapSettings.FlightSimulatorXReceiverId = (int)v,   r => r.FlightSimulatorXReceiverId,     (r, v) => { r.FlightSimulatorXReceiverId = (int)v; r.Receivers[0].UniqueId = r.FlightSimulatorXReceiverId; }),
        };

        private readonly List<ConfigurationProperty> _InternetClientOptions = new List<ConfigurationProperty>() {
            new ConfigurationProperty("CanRunReports",                  (w, c) => w.Bool(c),    r => r.InternetClientSettings.CanRunReports,                    (r, v) => r.InternetClientSettings.CanRunReports = (bool)v,                 r => r.InternetClientCanRunReports,     (r, v) => r.InternetClientCanRunReports = (bool)v),
            new ConfigurationProperty("CanPlayAudio",                   (w, c) => w.Bool(c),    r => r.InternetClientSettings.CanPlayAudio,                     (r, v) => r.InternetClientSettings.CanPlayAudio = (bool)v,                  r => r.InternetClientCanPlayAudio,      (r, v) => r.InternetClientCanPlayAudio = (bool)v),
            new ConfigurationProperty("CanShowPictures",                (w, c) => w.Bool(c),    r => r.InternetClientSettings.CanShowPictures,                  (r, v) => r.InternetClientSettings.CanShowPictures = (bool)v,               r => r.InternetClientCanSeePictures,    (r, v) => r.InternetClientCanSeePictures= (bool)v),
            new ConfigurationProperty("TimeoutMinutes",                 (w, c) => w.Int(c),     r => r.InternetClientSettings.TimeoutMinutes,                   (r, v) => r.InternetClientSettings.TimeoutMinutes = (int)v,                 r => r.InternetClientTimeoutMinutes,    (r, v) => r.InternetClientTimeoutMinutes= (int)v),
            new ConfigurationProperty("CanShowPinText",                 (w, c) => w.Bool(c),    r => r.InternetClientSettings.CanShowPinText,                   (r, v) => r.InternetClientSettings.CanShowPinText = (bool)v,                r => r.InternetClientCanSeeLabels,      (r, v) => r.InternetClientCanSeeLabels= (bool)v),
            new ConfigurationProperty("AllowInternetProximityGadgets",  (w, c) => w.Bool(c),    r => r.InternetClientSettings.AllowInternetProximityGadgets,    (r, v) => r.InternetClientSettings.AllowInternetProximityGadgets = (bool)v, r => r.AllowInternetProximityGadgets,   (r, v) => r.AllowInternetProximityGadgets= (bool)v),
            new ConfigurationProperty("CanSubmitRoutes",                (w, c) => w.Bool(c),    r => r.InternetClientSettings.CanSubmitRoutes,                  (r, v) => r.InternetClientSettings.CanSubmitRoutes = (bool)v,               r => r.InternetClientCanSubmitRoutes,   (r, v) => r.InternetClientCanSubmitRoutes = (bool)v),
            new ConfigurationProperty("CanShowPolarPlots",              (w, c) => w.Bool(c),    r => r.InternetClientSettings.CanShowPolarPlots,                (r, v) => r.InternetClientSettings.CanShowPolarPlots = (bool)v,             r => r.InternetClientCanShowPolarPlots, (r, v) => r.InternetClientCanShowPolarPlots = (bool)v),
        };

        private readonly List<ConfigurationProperty> _WebServerOptions = new List<ConfigurationProperty>() {
            new ConfigurationProperty("AuthenticationScheme",       (w, c) => w.ParseEnum<AuthenticationSchemes>(c), (w, c) => w.Bool(c), r => r.WebServerSettings.AuthenticationScheme,      (r, v) => r.WebServerSettings.AuthenticationScheme = (AuthenticationSchemes)v, r => r.WebServerUserMustAuthenticate,   (r, v) => r.WebServerUserMustAuthenticate = (bool)v),
            new ConfigurationProperty("EnableUPnp",                 (w, c) => w.Bool(c),                                                  r => r.WebServerSettings.EnableUPnp,                (r, v) => r.WebServerSettings.EnableUPnp = (bool)v,                            r => r.EnableUPnpFeatures,              (r, v) => r.EnableUPnpFeatures = (bool)v),
            new ConfigurationProperty("IsOnlyInternetServerOnLan",  (w, c) => w.Bool(c),                                                  r => r.WebServerSettings.IsOnlyInternetServerOnLan, (r, v) => r.WebServerSettings.IsOnlyInternetServerOnLan = (bool)v,             r => r.IsOnlyVirtualRadarServerOnLan,   (r, v) => r.IsOnlyVirtualRadarServerOnLan = (bool)v),
            new ConfigurationProperty("AutoStartUPnP",              (w, c) => w.Bool(c),                                                  r => r.WebServerSettings.AutoStartUPnP,             (r, v) => r.WebServerSettings.AutoStartUPnP = (bool)v,                         r => r.AutoStartUPnp,                   (r, v) => r.AutoStartUPnp = (bool)v),
            new ConfigurationProperty("UPnpPort",                   (w, c) => w.Int(c),                                                   r => r.WebServerSettings.UPnpPort,                  (r, v) => r.WebServerSettings.UPnpPort = (int)v,                               r => r.UPnpPort,                        (r, v) => r.UPnpPort = (int)v),
        };

        private readonly List<ConfigurationProperty> _RawDecodingOptions = new List<ConfigurationProperty>() {
            new ConfigurationProperty("ReceiverRange",                       (w, c) => w.Int(c),    r => r.RawDecodingSettings.ReceiverRange,                       (r, v) => r.RawDecodingSettings.ReceiverRange = (int)v,                          r => r.RawDecodingReceiverRange,                       (r, v) => r.RawDecodingReceiverRange = (int)v),
            new ConfigurationProperty("IgnoreMilitaryExtendedSquitter",      (w, c) => w.Bool(c),   r => r.RawDecodingSettings.IgnoreMilitaryExtendedSquitter,      (r, v) => r.RawDecodingSettings.IgnoreMilitaryExtendedSquitter = (bool)v,        r => r.RawDecodingIgnoreMilitaryExtendedSquitter,      (r, v) => r.RawDecodingIgnoreMilitaryExtendedSquitter = (bool)v),
            new ConfigurationProperty("AirborneGlobalPositionLimit",         (w, c) => w.Int(c),    r => r.RawDecodingSettings.AirborneGlobalPositionLimit,         (r, v) => r.RawDecodingSettings.AirborneGlobalPositionLimit = (int)v,            r => r.RawDecodingAirborneGlobalPositionLimit,         (r, v) => r.RawDecodingAirborneGlobalPositionLimit = (int)v),
            new ConfigurationProperty("FastSurfaceGlobalPositionLimit",      (w, c) => w.Int(c),    r => r.RawDecodingSettings.FastSurfaceGlobalPositionLimit,      (r, v) => r.RawDecodingSettings.FastSurfaceGlobalPositionLimit = (int)v,         r => r.RawDecodingFastSurfaceGlobalPositionLimit,      (r, v) => r.RawDecodingFastSurfaceGlobalPositionLimit = (int)v),
            new ConfigurationProperty("SlowSurfaceGlobalPositionLimit",      (w, c) => w.Int(c),    r => r.RawDecodingSettings.SlowSurfaceGlobalPositionLimit,      (r, v) => r.RawDecodingSettings.SlowSurfaceGlobalPositionLimit = (int)v,         r => r.RawDecodingSlowSurfaceGlobalPositionLimit,      (r, v) => r.RawDecodingSlowSurfaceGlobalPositionLimit = (int)v),
            new ConfigurationProperty("AcceptableAirborneSpeed",             (w, c) => w.Double(c), r => r.RawDecodingSettings.AcceptableAirborneSpeed,             (r, v) => r.RawDecodingSettings.AcceptableAirborneSpeed = (double)v,             r => r.RawDecodingAcceptableAirborneSpeed,             (r, v) => r.RawDecodingAcceptableAirborneSpeed = (double)v),
            new ConfigurationProperty("AcceptableAirSurfaceTransitionSpeed", (w, c) => w.Double(c), r => r.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed, (r, v) => r.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed = (double)v, r => r.RawDecodingAcceptableAirSurfaceTransitionSpeed, (r, v) => r.RawDecodingAcceptableAirSurfaceTransitionSpeed = (double)v),
            new ConfigurationProperty("AcceptableSurfaceSpeed",              (w, c) => w.Double(c), r => r.RawDecodingSettings.AcceptableSurfaceSpeed,              (r, v) => r.RawDecodingSettings.AcceptableSurfaceSpeed = (double)v,              r => r.RawDecodingAcceptableSurfaceSpeed,              (r, v) => r.RawDecodingAcceptableSurfaceSpeed = (double)v),
            new ConfigurationProperty("SuppressReceiverRangeCheck",          (w, c) => w.Bool(c),   r => r.RawDecodingSettings.SuppressReceiverRangeCheck,          (r, v) => r.RawDecodingSettings.SuppressReceiverRangeCheck = (bool)v,            r => r.RawDecodingSuppressReceiverRangeCheck,          (r, v) => r.RawDecodingSuppressReceiverRangeCheck = (bool)v),
            new ConfigurationProperty("UseLocalDecodeForInitialPosition",    (w, c) => w.Bool(c),   r => r.RawDecodingSettings.UseLocalDecodeForInitialPosition,    (r, v) => r.RawDecodingSettings.UseLocalDecodeForInitialPosition = (bool)v,      r => r.RawDecodingUseLocalDecodeForInitialPosition,    (r, v) => r.RawDecodingUseLocalDecodeForInitialPosition = (bool)v),
            new ConfigurationProperty("IgnoreCallsignsInBds20",              (w, c) => w.Bool(c),   r => r.RawDecodingSettings.IgnoreCallsignsInBds20,              (r, v) => r.RawDecodingSettings.IgnoreCallsignsInBds20 = (bool)v,                r => r.RawDecodingIgnoreCallsignsInBds20,              (r, v) => r.RawDecodingIgnoreCallsignsInBds20 = (bool)v),
            new ConfigurationProperty("AcceptIcaoInNonPICount",              (w, c) => w.Int(c),    r => r.RawDecodingSettings.AcceptIcaoInNonPICount,              (r, v) => r.RawDecodingSettings.AcceptIcaoInNonPICount = (int)v,                 r => r.AcceptIcaoInNonPICount,                         (r, v) => r.AcceptIcaoInNonPICount = (int)v),
            new ConfigurationProperty("AcceptIcaoInNonPISeconds",            (w, c) => w.Int(c),    r => r.RawDecodingSettings.AcceptIcaoInNonPISeconds,            (r, v) => r.RawDecodingSettings.AcceptIcaoInNonPISeconds = (int)v,               r => r.AcceptIcaoInNonPISeconds,                       (r, v) => r.AcceptIcaoInNonPISeconds = (int)v),
            new ConfigurationProperty("AcceptIcaoInPI0Count",                (w, c) => w.Int(c),    r => r.RawDecodingSettings.AcceptIcaoInPI0Count,                (r, v) => r.RawDecodingSettings.AcceptIcaoInPI0Count = (int)v,                   r => r.AcceptIcaoInPI0Count,                           (r, v) => r.AcceptIcaoInPI0Count = (int)v),
            new ConfigurationProperty("AcceptIcaoInPI0Seconds",              (w, c) => w.Int(c),    r => r.RawDecodingSettings.AcceptIcaoInPI0Seconds,              (r, v) => r.RawDecodingSettings.AcceptIcaoInPI0Seconds = (int)v,                 r => r.AcceptIcaoInPI0Seconds,                         (r, v) => r.AcceptIcaoInPI0Seconds = (int)v),
        };
        #endregion

        #region Check_ methods
        private void Check_Initialise_Copies_Values_From_Configuration_To_View(List<ConfigurationProperty> configurationProperties)
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            if(worksheet.String("Direction") != "UIToConfig") {
                var name = worksheet.String("Name");
                var configurationProperty = configurationProperties.Where(r => r.Name == name).Single();
                var configurationValue = configurationProperty.GetSpreadsheetConfig(worksheet, "ConfigValue");
                var uiValue = configurationProperty.GetSpreadsheetView(worksheet, "UIValue");

                configurationProperty.SetConfig(_Configuration, configurationValue);

                _Presenter.Initialise(_View.Object);

                Assert.AreEqual(uiValue, configurationProperty.GetView(_View.Object), configurationProperty.Name);
            }
        }

        private void Check_SaveClicked_Copies_Values_From_View_To_Configuration(List<ConfigurationProperty> configurationProperties)
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            if(worksheet.String("Direction") != "ConfigToUI") {
                var name = worksheet.String("Name");
                var configurationProperty = configurationProperties.Where(r => r.Name == name).Single();
                var configurationValue = configurationProperty.GetSpreadsheetConfig(worksheet, "ConfigValue");
                var uiValue = configurationProperty.GetSpreadsheetView(worksheet, "UIValue");

                _ConfigurationStorage.Setup(r => r.Save(_Configuration)).Callback(() => {
                    Assert.AreEqual(configurationValue, configurationProperty.GetConfig(_Configuration), name);
                });

                _Presenter.Initialise(_View.Object);
                configurationProperty.SetView(_View.Object, uiValue);

                _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

                _ConfigurationStorage.Verify(c => c.Save(_Configuration), Times.Once(), name);
            }
        }

        private void Check_ResetToDefaultClicked_Copies_Default_Configuration_To_UI(List<ConfigurationProperty> configurationProperties)
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            if(worksheet.String("Direction") != "ConfigToUI") {
                var name = worksheet.String("Name");
                var configurationProperty = configurationProperties.Where(r => r.Name == name).Single();
                var configurationValue = configurationProperty.GetSpreadsheetConfig(worksheet, "ConfigValue");

                var defaultConfig = new Configuration();
                _ConfigurationStorage.Setup(r => r.Save(_Configuration)).Callback(() => {
                    Assert.AreEqual(configurationProperty.GetConfig(defaultConfig), configurationProperty.GetConfig(_Configuration), name);
                });

                configurationProperty.SetConfig(_Configuration, configurationValue);
                _Presenter.Initialise(_View.Object);
                _View.Raise(v => v.ResetToDefaultsClicked += null, EventArgs.Empty);
                _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

                _ConfigurationStorage.Verify(c => c.Save(_Configuration), Times.Once(), name);
            }
        }

        private void SetupExpectedValidationFields(IEnumerable<ValidationResult> expectedResults)
        {
            _View.Setup(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>())).Callback((IEnumerable<ValidationResult> r) => {
                Assert.AreEqual(expectedResults.Count(), r.Count());
                foreach(var expectedResult in expectedResults) {
                    var matchingResult = r.Where(i => i.Field == expectedResult.Field && i.Record == expectedResult.Record).Single();
                    Assert.AreEqual(expectedResult.IsWarning, matchingResult.IsWarning);
                    Assert.AreEqual(expectedResult.Message, matchingResult.Message);
                }
            });
        }
        #endregion

        #region Constructor
        [TestMethod]
        public void OptionsPresenter_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var presenter = Factory.Singleton.Resolve<IOptionsPresenter>();

            Assert.IsNotNull(presenter.Provider);
            TestUtilities.TestProperty(presenter, "Provider", presenter.Provider, _Provider.Object);
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void OptionsPresenter_Dispose_Calls_Provider_Dispose()
        {
            _Presenter.Dispose();

            _Provider.Verify(p => p.Dispose(), Times.Once());
        }
        #endregion

        #region Initialise
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AudioOptions$")]
        public void OptionsPresenter_Initialise_Copies_Values_From_Configuration_To_Audio_Options_UI()
        {
            Check_Initialise_Copies_Values_From_Configuration_To_View(_AudioProperties);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "BaseStationOptions$")]
        public void OptionsPresenter_Initialise_Copies_Values_From_Configuration_To_BaseStation_Options_UI()
        {
            Check_Initialise_Copies_Values_From_Configuration_To_View(_BaseStationProperties);
        }

        [TestMethod]
        public void OptionsPresenter_Initialise_Copies_ReceiverLocations_From_Configuration_To_BaseStation_Options_UI()
        {
            var line1 = new ReceiverLocation() { UniqueId = 1, Name = "A", Latitude = 1.2, Longitude = 3.4 };
            var line2 = new ReceiverLocation() { UniqueId = 2, Name = "B", Latitude = 5.6, Longitude = 7.8 };
            _Configuration.ReceiverLocations.AddRange(new ReceiverLocation[] { line1, line2 });

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(2, _View.Object.RawDecodingReceiverLocations.Count);
            Assert.IsTrue(_View.Object.RawDecodingReceiverLocations.Contains(line1));
            Assert.IsTrue(_View.Object.RawDecodingReceiverLocations.Contains(line2));
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GeneralOptions$")]
        public void OptionsPresenter_Initialise_Copies_Values_From_Configuration_To_General_Options_UI()
        {
            Check_Initialise_Copies_Values_From_Configuration_To_View(_GeneralProperties);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GoogleMapOptions$")]
        public void OptionsPresenter_Initialise_Copies_Values_From_Configuration_To_GoogleMap_Options_UI()
        {
            Check_Initialise_Copies_Values_From_Configuration_To_View(_GoogleMapOptions);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "InternetClientOptions$")]
        public void OptionsPresenter_Initialise_Copies_Values_From_Configuration_To_InternetClient_Options_UI()
        {
            Check_Initialise_Copies_Values_From_Configuration_To_View(_InternetClientOptions);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "WebServerOptions$")]
        public void OptionsPresenter_Initialise_Copies_Values_From_Configuration_To_WebServer_Options_UI()
        {
            Check_Initialise_Copies_Values_From_Configuration_To_View(_WebServerOptions);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "RawDecodingOptions$")]
        public void OptionsPresenter_Initialise_Copies_Values_From_Configuration_To_RawDecoding_Options_UI()
        {
            Check_Initialise_Copies_Values_From_Configuration_To_View(_RawDecodingOptions);
        }

        [TestMethod]
        public void OptionsPresenter_Initialise_Populates_Audio_Options_Page_With_Voice_Names()
        {
            var names = new List<string>();
            _Provider.Setup(p => p.GetVoiceNames()).Returns(names);

            _Presenter.Initialise(_View.Object);

            _View.Verify(v => v.PopulateTextToSpeechVoices(names), Times.Once());
            _View.Verify(v => v.PopulateTextToSpeechVoices(It.IsAny<IEnumerable<string>>()), Times.Once());
        }

        [TestMethod]
        public void OptionsPresenter_Initialise_Copies_RebroadcastSettings_From_Configuration_To_General_Options_UI()
        {
            var line1 = new RebroadcastSettings() { Enabled = true, Name = "A", Port = 12, Format = RebroadcastFormat.Passthrough, StaleSeconds = 11, };
            var line2 = new RebroadcastSettings() { Enabled = true, Name = "B", Port = 17, Format = RebroadcastFormat.Port30003, StaleSeconds = 12, };

            _Configuration.RebroadcastSettings.AddRange(new RebroadcastSettings[] { line1, line2 });

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(2, _View.Object.RebroadcastSettings.Count);
            Assert.IsTrue(_View.Object.RebroadcastSettings.Contains(line1));
            Assert.IsTrue(_View.Object.RebroadcastSettings.Contains(line2));
        }

        [TestMethod]
        public void OptionsPresenter_Initialise_Copies_Receivers_From_Configuration_To_UI()
        {
            var line1 = new Receiver() { Name = "A" };
            var line2 = new Receiver() { Name = "B" };
            _Configuration.Receivers.Clear();
            _Configuration.Receivers.AddRange(new Receiver[] { line1, line2 });

            _Presenter.Initialise(_View.Object);
            
            Assert.AreEqual(2, _View.Object.Receivers.Count);
            Assert.AreSame(line1, _View.Object.Receivers[0]);
            Assert.AreSame(line2, _View.Object.Receivers[1]);
        }

        [TestMethod]
        public void OptionsPresenter_Initialise_Copies_MergedFeeds_From_Configuration_To_UI()
        {
            var line1 = new MergedFeed() { Name = "A" };
            var line2 = new MergedFeed() { Name = "B" };
            _Configuration.MergedFeeds.Clear();
            _Configuration.MergedFeeds.AddRange(new MergedFeed[] { line1, line2 });

            _Presenter.Initialise(_View.Object);
            
            Assert.AreEqual(2, _View.Object.MergedFeeds.Count);
            Assert.AreSame(line1, _View.Object.MergedFeeds[0]);
            Assert.AreSame(line2, _View.Object.MergedFeeds[1]);
        }

        [TestMethod]
        public void OptionsPresenter_Initialise_Copies_BasicAuthentication_Users_From_Configuration_To_UI()
        {
            _Configuration.WebServerSettings.BasicAuthenticationUserIds.Clear();
            _Configuration.WebServerSettings.BasicAuthenticationUserIds.Add("First");
            _Configuration.WebServerSettings.BasicAuthenticationUserIds.Add("Second");

            _Presenter.Initialise(_View.Object);
            
            Assert.AreEqual(2, _View.Object.WebServerUserIds.Count);
            Assert.AreEqual("First", _View.Object.WebServerUserIds[0]);
            Assert.AreEqual("Second", _View.Object.WebServerUserIds[1]);
        }

        [TestMethod]
        public void OptionsPresenter_Initialise_Copies_Users_From_Configuration_To_UI()
        {
            var user = TestUtilities.CreateMockInstance<IUser>();
            _UserManagerUsers.Add(user.Object);

            _Presenter.Initialise(_View.Object);

            Assert.AreEqual(1, _View.Object.Users.Count);
            Assert.AreSame(user.Object, _View.Object.Users[0]);
        }

        [TestMethod]
        public void OptionsPresenter_Initialise_Sets_UIPassword_When_Copying_Users_From_Configuration_To_UI()
        {
            var user = TestUtilities.CreateMockInstance<IUser>();
            _UserManagerUsers.Add(user.Object);

            _Presenter.Initialise(_View.Object);

            Assert.IsFalse(String.IsNullOrEmpty(user.Object.UIPassword));
        }
        #endregion

        #region SaveClicked
        #region Single value properties
        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ValidateOptionsView$")]
        public void OptionsPresenter_SaveClicked_Validates_UI_Before_Save()
        {
            DoValidationTest(() => { _View.Raise(v => v.SaveClicked += null, EventArgs.Empty); });
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ValidateOptionsView$")]
        public void OptionsPresenter_SaveClicked_Validates_When_Values_Changed()
        {
            DoValidationTest(() => { _View.Raise(v => v.ValueChanged += null, EventArgs.Empty); });
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ValidateOptionsView$")]
        public void OptionsPresenter_SaveClicked_Validation_Suppresses_FileName_Check_If_Already_Tested_FileName()
        {
            // Things were getting slow when the same filename keeps getting validated every time a value changes
            // so now the presenter is only expected to test a name if it has not already been tested.
            DoValidationTest(() => { _View.Raise(v => v.ValueChanged += null, EventArgs.Empty); }, true);
        }

        private void DoValidationTest(Action triggerValidation, bool doSuppressExcessiveFileSystemCheck = false)
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            List<ValidationResult> validationResults = new List<ValidationResult>();
            _View.Setup(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>())).Callback((IEnumerable<ValidationResult> results) => {
                foreach(var validationResult in results) validationResults.Add(validationResult);
            });

            int countFileExistsCalls = 0;
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(false);
            _Provider.Setup(p => p.FileExists(null)).Callback(() => {throw new NullReferenceException(); });
            _Provider.Setup(p => p.FileExists("FileExists")).Callback(() => countFileExistsCalls++).Returns(true);

            int countFolderExistsCalls = 0;
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Returns(false);
            _Provider.Setup(p => p.FolderExists(null)).Callback(() => {throw new NullReferenceException(); });
            _Provider.Setup(p => p.FolderExists("FolderExists")).Callback(() => countFolderExistsCalls++).Returns(true);

            _Presenter.Initialise(_View.Object);

            _View.Object.BaseStationDatabaseFileName = null;
            _View.Object.OperatorFlagsFolder = null;
            _View.Object.SilhouettesFolder = null;
            _View.Object.WebSiteReceiverId = 1;

            for(var i = 1;i <= 3;++i) {
                var uiFieldColumn = String.Format("UIField{0}", i);
                var valueColumn = String.Format("Value{0}", i);
                if(worksheet.String(uiFieldColumn) != null) {
                    switch(worksheet.String(uiFieldColumn)) {
                        case "AcceptableAirborneSpeed":             _View.Object.RawDecodingAcceptableAirborneSpeed = worksheet.Double(valueColumn); break;
                        case "AcceptableAirSurfaceTransitionSpeed": _View.Object.RawDecodingAcceptableAirSurfaceTransitionSpeed = worksheet.Double(valueColumn); break;
                        case "AcceptableSurfaceSpeed":              _View.Object.RawDecodingAcceptableSurfaceSpeed = worksheet.Double(valueColumn); break;
                        case "AcceptIcaoInNonPICount":              _View.Object.AcceptIcaoInNonPICount = worksheet.Int(valueColumn); break;
                        case "AcceptIcaoInNonPISeconds":            _View.Object.AcceptIcaoInNonPISeconds = worksheet.Int(valueColumn); break;
                        case "AcceptIcaoInPI0Count":                _View.Object.AcceptIcaoInPI0Count = worksheet.Int(valueColumn); break;
                        case "AcceptIcaoInPI0Seconds":              _View.Object.AcceptIcaoInPI0Seconds = worksheet.Int(valueColumn); break;
                        case "AirborneGlobalPositionLimit":         _View.Object.RawDecodingAirborneGlobalPositionLimit = worksheet.Int(valueColumn); break;
                        case "CheckForNewVersionsPeriodDays":       _View.Object.CheckForNewVersionsPeriodDays = worksheet.Int(valueColumn); break;
                        case "ClosestAircraftReceiverId":           _View.Object.ClosestAircraftReceiverId = worksheet.Int(valueColumn); break;
                        case "DatabaseFileName":                    _View.Object.BaseStationDatabaseFileName = worksheet.EString(valueColumn); break;
                        case "DisplayTimeoutSeconds":               _View.Object.DisplayTimeoutSeconds = worksheet.Int(valueColumn); break;
                        case "FastSurfaceGlobalPositionLimit":      _View.Object.RawDecodingFastSurfaceGlobalPositionLimit = worksheet.Int(valueColumn); break;
                        case "FlagsFolder":                         _View.Object.OperatorFlagsFolder = worksheet.EString(valueColumn); break;
                        case "FlightSimulatorXReceiverId":          _View.Object.FlightSimulatorXReceiverId = worksheet.Int(valueColumn); break;
                        case "InitialGoogleMapLatitude":            _View.Object.InitialGoogleMapLatitude = worksheet.Double(valueColumn); break;
                        case "InitialGoogleMapLongitude":           _View.Object.InitialGoogleMapLongitude = worksheet.Double(valueColumn); break;
                        case "InitialGoogleMapZoom":                _View.Object.InitialGoogleMapZoom = worksheet.Int(valueColumn); break;
                        case "InitialRefreshSeconds":               _View.Object.InitialGoogleMapRefreshSeconds = worksheet.Int(valueColumn); break;
                        case "InternetClientTimeoutMinutes":        _View.Object.InternetClientTimeoutMinutes = worksheet.Int(valueColumn); break;
                        case "MergedFeedId":                        _View.Object.MergedFeeds.Clear(); _View.Object.MergedFeeds.Add(new MergedFeed() { UniqueId = worksheet.Int(valueColumn), Name = "A", ReceiverIds = { 1, 2 } }); break;
                        case "MinimumRefreshSeconds":               _View.Object.MinimumGoogleMapRefreshSeconds = worksheet.Int(valueColumn); break;
                        case "PicturesFolder":                      _View.Object.PicturesFolder = worksheet.EString(valueColumn); break;
                        case "ReceiverId":                          _View.Object.Receivers.Clear(); _View.Object.Receivers.Add(new Receiver() { UniqueId = worksheet.Int(valueColumn), Name = "A" }); break;
                        case "ReceiverRange":                       _View.Object.RawDecodingReceiverRange = worksheet.Int(valueColumn); break;
                        case "ShortTrailLengthSeconds":             _View.Object.ShortTrailLengthSeconds = worksheet.Int(valueColumn); break;
                        case "SilhouettesFolder":                   _View.Object.SilhouettesFolder = worksheet.EString(valueColumn); break;
                        case "SlowSurfaceGlobalPositionLimit":      _View.Object.RawDecodingSlowSurfaceGlobalPositionLimit = worksheet.Int(valueColumn); break;
                        case "TextToSpeechSpeed":                   _View.Object.TextToSpeechSpeed = worksheet.Int(valueColumn); break;
                        case "TrackingTimeoutSeconds":              _View.Object.TrackingTimeoutSeconds = worksheet.Int(valueColumn); break;
                        case "UPnpPort":                            _View.Object.UPnpPort = worksheet.Int(valueColumn); break;
                        case "WebAuthenticateUser":                 _View.Object.WebServerUserMustAuthenticate = worksheet.Bool(valueColumn); break;
                        case "WebSiteReceiverId":                   _View.Object.WebSiteReceiverId = worksheet.Int(valueColumn); break;
                        default:                                    throw new NotImplementedException();
                    }
                }
            }

            triggerValidation();
            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Once());

            if(doSuppressExcessiveFileSystemCheck) {
                validationResults.Clear();

                triggerValidation();
                _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Exactly(2));

                Assert.IsTrue(countFileExistsCalls < 2);
                Assert.IsTrue(countFolderExistsCalls < 2);
            }

            var validationErrorSummary = new StringBuilder();
            foreach(var validationResult in validationResults) {
                if(validationErrorSummary.Length != 0) validationErrorSummary.Append("; ");
                validationErrorSummary.AppendFormat("{0}:{1}", validationResult.Field, validationResult.Message);
            }

            Assert.AreEqual(worksheet.Int("CountErrors"), validationResults.Count(), validationErrorSummary.ToString());
            if(validationResults.Count() > 0) {
                Assert.IsTrue(validationResults.Where(r => r.Field == worksheet.ParseEnum<ValidationField>("Field") &&
                                                           r.Message == worksheet.EString("Message") &&
                                                           r.IsWarning == worksheet.Bool("IsWarning")).Any(),
                              validationErrorSummary.ToString());
            }
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Will_Revalidate_FileSystem_Entries_If_They_Change()
        {
            int countCheckExistsCalls = 0;
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Callback(() => ++countCheckExistsCalls).Returns(false);
            _Provider.Setup(p => p.FolderExists(It.IsAny<string>())).Callback(() => ++countCheckExistsCalls).Returns(false);

            _Presenter.Initialise(_View.Object);
            _View.Object.BaseStationDatabaseFileName = null;
            _View.Object.OperatorFlagsFolder = null;
            _View.Object.SilhouettesFolder = null;
            _View.Object.PicturesFolder = null;

            foreach(var fillFileSystemValue in new Action<string>[] {
                (s) => { _View.Object.BaseStationDatabaseFileName = s; },
                (s) => { _View.Object.OperatorFlagsFolder = s; },
                (s) => { _View.Object.SilhouettesFolder = s; },
                (s) => { _View.Object.PicturesFolder = s; } }) {

                countCheckExistsCalls = 0;

                fillFileSystemValue("DNE1");
                _View.Raise(v => v.ValueChanged += null, EventArgs.Empty);
                Assert.AreEqual(1, countCheckExistsCalls);

                _View.Raise(v => v.ValueChanged += null, EventArgs.Empty);
                Assert.AreEqual(1, countCheckExistsCalls);

                fillFileSystemValue("DNE2");
                _View.Raise(v => v.ValueChanged += null, EventArgs.Empty);
                Assert.AreEqual(2, countCheckExistsCalls);

                fillFileSystemValue(null);
            }
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Failed_Validation_Prevents_Overwriting_Of_Configuration()
        {
            _Presenter.Initialise(_View.Object);
            _View.Object.Receivers.Clear();

            _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

            Assert.AreEqual(2, _Configuration.Receivers.Count);
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Failed_Validation_Prevents_Save_Of_Configuration()
        {
            _Presenter.Initialise(_View.Object);
            _View.Object.Receivers.Clear();

            _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

            _ConfigurationStorage.Verify(c => c.Save(It.IsAny<Configuration>()), Times.Never());
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Validation_With_Only_Warnings_Allows_Save_Of_Configuration()
        {
            _Presenter.Initialise(_View.Object);
            _View.Object.SilhouettesFolder = "Does not exist";
            _Provider.Setup(p => p.FileExists(It.IsAny<string>())).Returns(false);

            _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

            _ConfigurationStorage.Verify(c => c.Save(It.IsAny<Configuration>()), Times.Once());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "BaseStationOptions$")]
        public void OptionsPresenter_SaveClicked_Copies_Values_From_BaseStation_Options_UI_To_Configuration_Before_Save()
        {
            Check_SaveClicked_Copies_Values_From_View_To_Configuration(_BaseStationProperties);
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Copies_ReceiverLocations_From_BaseStation_Options_UI_To_Configuration_Before_Save()
        {
            _Configuration.ReceiverLocations.Add(new ReceiverLocation() { UniqueId = 3, Name = "Old garbage, should be cleared", Latitude = 1, Longitude = 2 });
            _Presenter.Initialise(_View.Object);

            var line1 = new ReceiverLocation() { UniqueId = 1, Name = "A", Latitude = 1.2, Longitude = 3.4 };
            var line2 = new ReceiverLocation() { UniqueId = 2, Name = "B", Latitude = 5.6, Longitude = 7.8 };
            _View.Object.RawDecodingReceiverLocations.Clear();
            _View.Object.RawDecodingReceiverLocations.AddRange(new ReceiverLocation[] { line1, line2 });

            _ConfigurationStorage.Setup(c => c.Save(_Configuration)).Callback(() => {
                Assert.AreEqual(2, _Configuration.ReceiverLocations.Count);
                Assert.IsTrue(_Configuration.ReceiverLocations.Contains(line1));
                Assert.IsTrue(_Configuration.ReceiverLocations.Contains(line2));
            });

            _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

            _ConfigurationStorage.Verify(c => c.Save(_Configuration), Times.Once());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AudioOptions$")]
        public void OptionsPresenter_SaveClicked_Copies_Values_From_Audio_Options_UI_To_Configuration_Before_Save()
        {
            Check_SaveClicked_Copies_Values_From_View_To_Configuration(_AudioProperties);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GeneralOptions$")]
        public void OptionsPresenter_SaveClicked_Copies_Values_From_General_Options_UI_To_Configuration_Before_Save()
        {
            Check_SaveClicked_Copies_Values_From_View_To_Configuration(_GeneralProperties);
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Copies_RebroadcastSettings_From_Options_UI_To_Configuration_Before_Save()
        {
            _Configuration.RebroadcastSettings.Add(new RebroadcastSettings() { Enabled = true, Name = "Will be deleted", Format = RebroadcastFormat.Passthrough, Port = 100, ReceiverId = 1 });
            _Presenter.Initialise(_View.Object);

            var line1 = new RebroadcastSettings() { Enabled = true, Name = "X1", Format = RebroadcastFormat.Passthrough, Port = 9000, ReceiverId = 1 };
            var line2 = new RebroadcastSettings() { Enabled = false, Name = "Y1", Format = RebroadcastFormat.Port30003, Port = 9001, ReceiverId = 1 };
            _View.Object.RebroadcastSettings.Clear();
            _View.Object.RebroadcastSettings.AddRange(new RebroadcastSettings[] { line1, line2 });

            _ConfigurationStorage.Setup(c => c.Save(_Configuration)).Callback(() => {
                Assert.AreEqual(2, _Configuration.RebroadcastSettings.Count);
                Assert.IsTrue(_Configuration.RebroadcastSettings.Contains(line1));
                Assert.IsTrue(_Configuration.RebroadcastSettings.Contains(line2));
            });

            _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

            _ConfigurationStorage.Verify(c => c.Save(_Configuration), Times.Once());
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Copies_Receivers_From_UI_To_Configuration_Before_Save()
        {
            _Configuration.Receivers.Add(new Receiver() { Name = "Will be deleted" });
            _Presenter.Initialise(_View.Object);

            var line1 = new Receiver() { Name = "X1" };
            var line2 = new Receiver() { Name = "Y1" };
            _View.Object.Receivers.Clear();
            _View.Object.Receivers.AddRange(new Receiver[] { line1, line2 });

            _ConfigurationStorage.Setup(c => c.Save(_Configuration)).Callback(() => {
                Assert.AreEqual(2, _Configuration.Receivers.Count);
                Assert.AreSame(line1, _Configuration.Receivers[0]);
                Assert.AreSame(line2, _Configuration.Receivers[1]);
            });

            _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

            _ConfigurationStorage.Verify(c => c.Save(_Configuration), Times.Once());
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Copies_MergedFeeds_From_UI_To_Configuration_Before_Save()
        {
            _Configuration.MergedFeeds.Add(new MergedFeed() { Name = "Will be deleted" });
            _Presenter.Initialise(_View.Object);

            var line1 = new MergedFeed() { Name = "X1", ReceiverIds = { 1, 2 } };
            var line2 = new MergedFeed() { Name = "Y1", ReceiverIds = { 1, 2 } };
            _View.Object.MergedFeeds.Clear();
            _View.Object.MergedFeeds.AddRange(new MergedFeed[] { line1, line2 });

            _ConfigurationStorage.Setup(c => c.Save(_Configuration)).Callback(() => {
                Assert.AreEqual(2, _Configuration.MergedFeeds.Count);
                Assert.AreSame(line1, _Configuration.MergedFeeds[0]);
                Assert.AreSame(line2, _Configuration.MergedFeeds[1]);
            });

            _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

            _ConfigurationStorage.Verify(c => c.Save(_Configuration), Times.Once());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GoogleMapOptions$")]
        public void OptionsPresenter_SaveClicked_Copies_Values_From_GoogleMap_Options_UI_To_Configuration_Before_Save()
        {
            Check_SaveClicked_Copies_Values_From_View_To_Configuration(_GoogleMapOptions);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "InternetClientOptions$")]
        public void OptionsPresenter_SaveClicked_Copies_Values_From_InternetClient_Options_UI_To_Configuration_Before_Save()
        {
            Check_SaveClicked_Copies_Values_From_View_To_Configuration(_InternetClientOptions);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "WebServerOptions$")]
        public void OptionsPresenter_SaveClicked_Copies_Values_From_WebServer_Options_UI_To_Configuration_Before_Save()
        {
            Check_SaveClicked_Copies_Values_From_View_To_Configuration(_WebServerOptions);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "RawDecodingOptions$")]
        public void OptionsPresenter_SaveClicked_Copies_Values_From_RawDecoding_Options_UI_To_Configuration_Before_Save()
        {
            Check_SaveClicked_Copies_Values_From_View_To_Configuration(_RawDecodingOptions);
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Copies_BasicAuthentication_Users_From_View_To_Configuration()
        {
            _Configuration.WebServerSettings.BasicAuthenticationUserIds.Clear();
            _Configuration.WebServerSettings.BasicAuthenticationUserIds.Add("Will be deleted");

            _Presenter.Initialise(_View.Object);

            _View.Object.WebServerUserIds.Clear();
            _View.Object.WebServerUserIds.Add("Added");

            _ConfigurationStorage.Setup(c => c.Save(_Configuration)).Callback(() => {
                Assert.AreEqual(1, _Configuration.WebServerSettings.BasicAuthenticationUserIds.Count);
                Assert.AreEqual("Added", _Configuration.WebServerSettings.BasicAuthenticationUserIds[0]);
            });

            _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

            _ConfigurationStorage.Verify(c => c.Save(_Configuration), Times.Once());
        }

        [TestMethod]
        public void OptionsPresenter_SaveClicked_Passes_Users_Through_UserManager_Validation()
        {
            _UserManager.Setup(r => r.CanEditUsers).Returns(true);
            var user = TestUtilities.CreateMockInstance<IUser>();
            _UserManagerUsers.Add(user.Object);

            _Presenter.Initialise(_View.Object);

            _View.Raise(v => v.SaveClicked += null, EventArgs.Empty);

            _UserManager.Verify(r => r.ValidateUser(It.IsAny<List<ValidationResult>>(), user.Object, user.Object, _ViewUsers), Times.Once());
        }
        #endregion

        #region Receivers
        private Receiver SetupReceiver()
        {
            var result = new Receiver() { Enabled = true, UniqueId = 1, Name = "RECEIVER" };
            return SetupReceiver(result);
        }

        private Receiver SetupReceiver(Receiver receiver)
        {
            var list = new List<Receiver>() { receiver };
            _View.Setup(v => v.Receivers).Returns(list);
            return receiver;
        }

        [TestMethod]
        public void OptionsPresenter_Receivers_SaveClicked_At_Least_One_Receiver_Must_Be_Present()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.Receivers.Clear();
            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(null, ValidationField.ReceiverIds, Strings.PleaseConfigureAtLeastOneReceiver)
            });

            _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.AtLeastOnce());
        }

        [TestMethod]
        public void OptionsPresenter_Receivers_SaveClicked_Insists_At_Least_One_Receiver_Is_Enabled()
        {
            _Presenter.Initialise(_View.Object);

            var line1 = new Receiver() { Enabled = false, UniqueId = 1, Name = "X" };
            var line2 = new Receiver() { Enabled = false, UniqueId = 2, Name = "Y" };
            SetupReceiver(line1);
            _View.Object.Receivers.Add(line2);

            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(null, ValidationField.ReceiverIds, Strings.PleaseEnableAtLeastOneReceiver)
            });

            _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.AtLeastOnce());
        }

        [TestMethod]
        public void OptionsPresenter_Receivers_SaveClicked_Checks_That_All_Names_Are_Unique()
        {
            var line1 = new Receiver() { UniqueId = 1, Name = "X" };
            var line2 = new Receiver() { UniqueId = 2, Name = "X" };
            SetupReceiver(line1);
            _View.Object.Receivers.Add(line2);

            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(line1, ValidationField.Name, Strings.NameMustBeUnique),
                new ValidationResult(line2, ValidationField.Name, Strings.NameMustBeUnique),
            });

            _Presenter.Initialise(_View.Object);
            _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.AtLeastOnce());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ValidateReceiver$")]
        public void OptionsPresenter_Receivers_SaveClicked_Validates_Content()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Presenter.Initialise(_View.Object);
            var receiver = _View.Object.Receivers[0];

            List<ValidationResult> validationResults = new List<ValidationResult>();
            _View.Setup(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>())).Callback((IEnumerable<ValidationResult> results) => {
                foreach(var validationResult in results) validationResults.Add(validationResult);
            });

            for(var i = 1;i <= 3;++i) {
                var uiFieldColumn = String.Format("UIField{0}", i);
                var valueColumn = String.Format("Value{0}", i);
                if(worksheet.String(uiFieldColumn) != null) {
                    switch(worksheet.String(uiFieldColumn)) {
                        case "Address":                 receiver.Address = worksheet.EString(valueColumn); break;
                        case "ConnectionType":          receiver.ConnectionType = worksheet.ParseEnum<ConnectionType>(valueColumn); break;
                        case "Port":                    receiver.Port = worksheet.Int(valueColumn); break;
                        case "BaudRate":                receiver.BaudRate = worksheet.Int(valueColumn); break;
                        case "ComPort":                 receiver.ComPort = worksheet.EString(valueColumn); break;
                        case "DataBits":                receiver.DataBits = worksheet.Int(valueColumn); break;
                        case "Name":                    receiver.Name = worksheet.EString(valueColumn); break;
                        default:                        throw new NotImplementedException();
                    }
                }
            }

            _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);
            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Once());

            var validationErrorSummary = new StringBuilder();
            foreach(var validationResult in validationResults) {
                if(validationErrorSummary.Length != 0) validationErrorSummary.Append("; ");
                validationErrorSummary.AppendFormat("{0}:{1}", validationResult.Field, validationResult.Message);
            }

            Assert.AreEqual(worksheet.Int("CountErrors"), validationResults.Count(), validationErrorSummary.ToString());
            if(validationResults.Count() > 0) {
                Assert.IsTrue(validationResults.Where(r => r.Record == receiver &&
                                                           r.Field == worksheet.ParseEnum<ValidationField>("Field") &&
                                                           r.Message == worksheet.EString("Message") &&
                                                           r.IsWarning == worksheet.Bool("IsWarning")).Any(),
                              validationErrorSummary.ToString());
            }
        }
        #endregion

        #region MergedFeeds
        private MergedFeed SetupMergedFeed()
        {
            var result = new MergedFeed() { Enabled = true, UniqueId = 1, Name = "MERGED FEED", ReceiverIds = { 1, 2 } };
            return SetupMergedFeed(result);
        }

        private MergedFeed SetupMergedFeed(MergedFeed mergedFeed)
        {
            var list = new List<MergedFeed>() { mergedFeed };
            _View.Setup(v => v.MergedFeeds).Returns(list);
            return mergedFeed;
        }

        [TestMethod]
        public void OptionsPresenter_MergedFeeds_SaveClicked_Checks_That_All_Names_Are_Unique()
        {
            var line1 = new MergedFeed() { UniqueId = 1, Name = "X", ReceiverIds = { 1, 2 } };
            var line2 = new MergedFeed() { UniqueId = 2, Name = "X", ReceiverIds = { 1, 2 } };
            SetupMergedFeed(line1);
            _View.Object.MergedFeeds.Add(line2);

            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(line1, ValidationField.Name, Strings.NameMustBeUnique),
                new ValidationResult(line2, ValidationField.Name, Strings.NameMustBeUnique),
            });

            _Presenter.Initialise(_View.Object);
            _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.AtLeastOnce());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "ValidateMergedFeed$")]
        public void OptionsPresenter_MergedFeeds_SaveClicked_Validates_Content()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            _Presenter.Initialise(_View.Object);
            var mergedFeed = _View.Object.MergedFeeds[0];

            List<ValidationResult> validationResults = new List<ValidationResult>();
            _View.Setup(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>())).Callback((IEnumerable<ValidationResult> results) => {
                foreach(var validationResult in results) validationResults.Add(validationResult);
            });

            for(var i = 1;i <= 3;++i) {
                var uiFieldColumn = String.Format("UIField{0}", i);
                var valueColumn = String.Format("Value{0}", i);
                if(worksheet.String(uiFieldColumn) != null) {
                    switch(worksheet.String(uiFieldColumn)) {
                        case "IcaoTimeout":             mergedFeed.IcaoTimeout = worksheet.Int(valueColumn); break;
                        case "Name":                    mergedFeed.Name = worksheet.EString(valueColumn); break;
                        case "ReceiverIds":
                            mergedFeed.ReceiverIds.Clear();
                            foreach(var idText in (worksheet.String(valueColumn) ?? "").Split(',')) {
                                int id = 0;
                                var text = idText.Trim();
                                if(text != "" && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out id)) {
                                    mergedFeed.ReceiverIds.Add(id);
                                }
                            }
                            break;
                        default:                        throw new NotImplementedException();
                    }
                }
            }

            _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);
            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Once());

            var validationErrorSummary = new StringBuilder();
            foreach(var validationResult in validationResults) {
                if(validationErrorSummary.Length != 0) validationErrorSummary.Append("; ");
                validationErrorSummary.AppendFormat("{0}:{1}", validationResult.Field, validationResult.Message);
            }

            Assert.AreEqual(worksheet.Int("CountErrors"), validationResults.Count(), validationErrorSummary.ToString());
            if(validationResults.Count() > 0) {
                Assert.IsTrue(validationResults.Where(r => r.Record == mergedFeed &&
                                                           r.Field == worksheet.ParseEnum<ValidationField>("Field") &&
                                                           r.Message == worksheet.EString("Message") &&
                                                           r.IsWarning == worksheet.Bool("IsWarning")).Any(),
                              validationErrorSummary.ToString());
            }
        }
        #endregion

        #region Combined Receivers and Merged feeds
        [TestMethod]
        public void OptionsPresenter_Feed_Names_Must_Be_Unique()
        {
            List<ValidationResult> validationResults = new List<ValidationResult>();
            _View.Setup(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>())).Callback((IEnumerable<ValidationResult> results) => {
                foreach(var validationResult in results) validationResults.Add(validationResult);
            });

            _Configuration.MergedFeeds[0].Name = _Configuration.Receivers[1].Name;
            _Presenter.Initialise(_View.Object);
            _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Once());
            Assert.AreEqual(1, validationResults.Count);
            Assert.AreEqual(Strings.ReceiversAndMergedFeedNamesMustBeUnique, validationResults[0].Message);
            Assert.AreEqual(false, validationResults[0].IsWarning);
        }
        #endregion

        #region ReceiverLocations
        private ReceiverLocation SetupReceiverLocation()
        {
            var result = new ReceiverLocation() { UniqueId = 1, Name = "SELECTED", Latitude = 1, Longitude = 2 };
            return SetupReceiverLocation(result);
        }

        private ReceiverLocation SetupReceiverLocation(ReceiverLocation location)
        {
            var list = new List<ReceiverLocation>() { location };
            _View.Setup(v => v.RawDecodingReceiverLocations).Returns(list);
            return location;
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_ValueChanged_Displays_Validation_Message_When_Any_Field_Is_Empty()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Any_Field_Is_Empty(() => 
                _View.Raise(v => v.ValueChanged += null, EventArgs.Empty)
            );
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_SaveClicked_Displays_Validation_Message_When_Any_Field_Is_Empty()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Any_Field_Is_Empty(() => _View.Raise(v => v.SaveClicked += null, EventArgs.Empty));
        }

        private void CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Any_Field_Is_Empty(Action trigger)
        {
            for(var emptyFieldNumber = 0;emptyFieldNumber < 1;++emptyFieldNumber) {
                TestCleanup();
                TestInitialise();

                var selectedLocation = SetupReceiverLocation(new ReceiverLocation() { Name = "A", Latitude = 7, Longitude = 8 });
                ValidationResult expectedValidationResult;
                switch(emptyFieldNumber) {
                    case 0:     expectedValidationResult = new ValidationResult(selectedLocation, ValidationField.Location, Strings.PleaseEnterNameForLocation); break;
                    default:    throw new NotImplementedException();
                }

                _Presenter.Initialise(_View.Object);
                SetupExpectedValidationFields(new ValidationResult[] { expectedValidationResult });

                selectedLocation.Name = emptyFieldNumber == 0 ? "" : "ABC";

                trigger();

                _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Once());
            }
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_ValueChanged_Displays_Validation_Message_When_Name_Duplicates_Existing_Name()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Name_Duplicates_Existing_Name(() => _View.Raise(v => v.ValueChanged += null, EventArgs.Empty));
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_Displays_Validation_Message_When_Name_Duplicates_Existing_Name()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Name_Duplicates_Existing_Name(() => _View.Raise(v => v.SaveClicked += null, EventArgs.Empty));
        }

        private void CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Name_Duplicates_Existing_Name(Action trigger)
        {
            var line1 = new ReceiverLocation() { UniqueId = 1, Name = "ABC", Latitude = 1, Longitude = 2 };
            var line2 = new ReceiverLocation() { UniqueId = 2, Name = "XYZ", Latitude = 1, Longitude = 2 };
            SetupReceiverLocation(line1);
            _View.Object.RawDecodingReceiverLocations.Add(line2);
            _Presenter.Initialise(_View.Object);

            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(line1, ValidationField.Location, Strings.PleaseEnterUniqueNameForLocation),
                new ValidationResult(line2, ValidationField.Location, Strings.PleaseEnterUniqueNameForLocation),
            });

            line2.Name = "ABC";
            trigger();

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Once());
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_ValueChanged_Displays_Validation_Message_When_Latitude_Is_Out_Of_Range()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Latitude_Is_Out_Of_Range(() => _View.Raise(v => v.ValueChanged += null, EventArgs.Empty));
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_SaveClicked_Displays_Validation_Message_When_Latitude_Is_Out_Of_Range()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Latitude_Is_Out_Of_Range(() => _View.Raise(v => v.SaveClicked += null, EventArgs.Empty));
        }

        private void CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Latitude_Is_Out_Of_Range(Action trigger)
        {
            foreach(var badLatitude in new double[] { -92.0, -91.0, -90.00001, 90.00001, 91.0, 92.0 }) {
                TestCleanup();
                TestInitialise();

                var line1 = new ReceiverLocation() { UniqueId = 1, Name = "ABC", Latitude = 1.0, Longitude = 2.0 };
                SetupReceiverLocation(line1);
                SetupExpectedValidationFields(new ValidationResult[] { new ValidationResult(line1, ValidationField.Latitude, Strings.LatitudeOutOfBounds) });
                _Presenter.Initialise(_View.Object);

                line1.Latitude = badLatitude;
                trigger();

                _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Once());
            }
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_ValueChanged_Displays_Validation_Message_When_Latitude_Is_Zero()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Latitude_Is_Zero(() => _View.Raise(v => v.ValueChanged += null, EventArgs.Empty));
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_SaveClicked_Displays_Validation_Message_When_Latitude_Is_Zero()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Latitude_Is_Zero(() => _View.Raise(v => v.SaveClicked += null, EventArgs.Empty));
        }

        private void CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Latitude_Is_Zero(Action trigger)
        {
            _Presenter.Initialise(_View.Object);

            var line1 = new ReceiverLocation() { UniqueId = 1, Name = "ABC", Latitude = 1.0, Longitude = 2.0 };
            SetupReceiverLocation(line1);
            SetupExpectedValidationFields(new ValidationResult[] { new ValidationResult(line1, ValidationField.Latitude, Strings.LatitudeCannotBeZero) });
            _Presenter.Initialise(_View.Object);

            line1.Latitude = 0.0;
            trigger();

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Exactly(2));
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_ValueChanged_Displays_Validation_Message_When_Longitude_Is_Out_Of_Range()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Longitude_Is_Out_Of_Range(() => _View.Raise(v => v.ValueChanged += null, EventArgs.Empty));
        }

        [TestMethod]
        public void OptionsPresenter_ReceiverLocations_SaveClicked_Displays_Validation_Message_When_Longitude_Is_Out_Of_Range()
        {
            CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Longitude_Is_Out_Of_Range(() => _View.Raise(v => v.SaveClicked += null, EventArgs.Empty));
        }

        private void CheckValidation_ReceiverLocations_Displays_Validation_Message_When_Longitude_Is_Out_Of_Range(Action trigger)
        {
            foreach(var badLongitude in new double[] { -182.0, -181.0, -180.00001, 180.00001, 181.0, 182.0 }) {
                TestCleanup();
                TestInitialise();

                var line1 = new ReceiverLocation() { UniqueId = 1, Name = "ABC", Latitude = 1.0, Longitude = 2.0 };
                SetupReceiverLocation(line1);
                SetupExpectedValidationFields(new ValidationResult[] { new ValidationResult(line1, ValidationField.Longitude, Strings.LongitudeOutOfBounds) });
                _Presenter.Initialise(_View.Object);

                line1.Longitude = badLongitude;
                trigger();

                _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Once());
            }
        }
        #endregion

        #region RebroadcastSettings
        private RebroadcastSettings SetupSelectedRebroadcastSettings()
        {
            var result = new RebroadcastSettings() { Enabled = true, Name = "Server", Port = 12001, Format = RebroadcastFormat.Port30003, ReceiverId = 1, StaleSeconds = 10 };
            return SetupSelectedRebroadcastSettings(result);
        }

        private RebroadcastSettings SetupSelectedRebroadcastSettings(RebroadcastSettings settings)
        {
            var list = new List<RebroadcastSettings>() { settings };
            _View.Setup(v => v.RebroadcastSettings).Returns(list);
            return settings;
        }

        [TestMethod]
        public void OptionsPresenter_RebroadcastSettings_ValueChanged_Displays_Validation_Message_When_Any_Field_Is_Empty()
        {
            for(var emptyFieldNumber = 0;emptyFieldNumber < 3;++emptyFieldNumber) {
                TestCleanup();
                TestInitialise();

                _Presenter.Initialise(_View.Object);
                var server = SetupSelectedRebroadcastSettings(new RebroadcastSettings() { Enabled = true, Name = "ABC", Format = RebroadcastFormat.Port30003, Port = 100, ReceiverId = 1, StaleSeconds = 10 });

                ValidationResult expectedValidationResult;
                switch(emptyFieldNumber) {
                    case 0:
                        server.Name = "";
                        expectedValidationResult = new ValidationResult(server, ValidationField.Name, Strings.NameRequired);
                        break;
                    case 1:
                        server.Format = RebroadcastFormat.None;
                        expectedValidationResult = new ValidationResult(server, ValidationField.Format, Strings.RebroadcastFormatRequired);
                        break;
                    case 2:
                        server.ReceiverId = 0;
                        expectedValidationResult = new ValidationResult(server, ValidationField.RebroadcastReceiver, Strings.ReceiverRequired);
                        break;
                    default:
                    throw new NotImplementedException();
                }
                SetupExpectedValidationFields(new ValidationResult[] { expectedValidationResult });

                _View.Raise(v => v.ValueChanged += null, EventArgs.Empty);

                _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.Once());
            }
        }

        [TestMethod]
        public void OptionsPresenter_RebroadcastSettings_ValueChanged_Displays_Validation_Message_When_Name_Duplicates_Existing_Name()
        {
            _Presenter.Initialise(_View.Object);

            var line1 = new RebroadcastSettings() { Name = "ABC", Format = RebroadcastFormat.Port30003, Port = 10001, ReceiverId = 1 };
            var line2 = new RebroadcastSettings() { Name = "XYZ", Format = RebroadcastFormat.Port30003, Port = 10002, ReceiverId = 1 };
            SetupSelectedRebroadcastSettings(line1);
            _View.Object.RebroadcastSettings.Add(line2);

            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(line1, ValidationField.Name, Strings.NameMustBeUnique),
                new ValidationResult(line2, ValidationField.Name, Strings.NameMustBeUnique),
            });

            line2.Name = "ABC";
            _View.Raise(v => v.ValueChanged += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.AtLeastOnce());
        }

        [TestMethod]
        public void OptionsPresenter_ValueChanged_Displays_Validation_Message_When_Port_Duplicates_Existing_Port()
        {
            _Presenter.Initialise(_View.Object);

            var line1 = new RebroadcastSettings() { Name = "ABC", Format = RebroadcastFormat.Port30003, Port = 10001, ReceiverId = 1 };
            var line2 = new RebroadcastSettings() { Name = "XYZ", Format = RebroadcastFormat.Port30003, Port = 10002, ReceiverId = 1 };
            SetupSelectedRebroadcastSettings(line1);
            _View.Object.RebroadcastSettings.Add(line2);

            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(line1, ValidationField.RebroadcastServerPort, Strings.PortMustBeUnique),
                new ValidationResult(line2, ValidationField.RebroadcastServerPort, Strings.PortMustBeUnique),
            });

            line2.Port = 10001;
            _View.Raise(v => v.ValueChanged += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.AtLeastOnce());
        }

        [TestMethod]
        public void OptionsPresenter_ValueChanged_Displays_Validation_Message_When_Port_Is_Too_Small()
        {
            _Presenter.Initialise(_View.Object);

            var line1 = new RebroadcastSettings() { Name = "ABC", Format = RebroadcastFormat.Port30003, Port = 0, ReceiverId = 1 };
            SetupSelectedRebroadcastSettings(line1);

            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(line1, ValidationField.RebroadcastServerPort, Strings.PortOutOfBounds),
            });
            _View.Raise(v => v.ValueChanged += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.AtLeastOnce());
        }

        [TestMethod]
        public void OptionsPresenter_ValueChanged_Displays_Validation_Message_When_Port_Is_Too_Large()
        {
            _Presenter.Initialise(_View.Object);

            var line1 = new RebroadcastSettings() { Name = "ABC", Format = RebroadcastFormat.Port30003, Port = 65536, ReceiverId = 1 };
            SetupSelectedRebroadcastSettings(line1);

            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(line1, ValidationField.RebroadcastServerPort, Strings.PortOutOfBounds),
            });
            _View.Raise(v => v.ValueChanged += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.AtLeastOnce());
        }

        [TestMethod]
        public void OptionsPresenter_ValueChanged_Displays_Validation_Message_When_StaleSeconds_Is_Too_Small()
        {
            _Presenter.Initialise(_View.Object);

            var line = new RebroadcastSettings() { Name = "A", Format = RebroadcastFormat.Port30003, Port = 10000, ReceiverId = 1, StaleSeconds = 0 };
            SetupSelectedRebroadcastSettings(line);

            SetupExpectedValidationFields(new ValidationResult[] {
                new ValidationResult(line, ValidationField.StaleSeconds, Strings.StaleSecondsOutOfBounds),
            });
            _View.Raise(v => v.ValueChanged += null, EventArgs.Empty);

            _View.Verify(v => v.ShowValidationResults(It.IsAny<IEnumerable<ValidationResult>>()), Times.AtLeastOnce());
        }
        #endregion

        #region Users
        [TestMethod]
        public void OptionsPresenter_Users_Changes_To_Existing_Users_Passed_To_UpdateUser()
        {
            foreach(var field in new string[] { "Enabled", "LoginName", "Name", "Password" }) {
                foreach(var makeChange in new bool[] { true, false }) {
                    TestCleanup();
                    TestInitialise();

                    _UserManager.Setup(r => r.CanEditUsers).Returns(true);
                    _UserManager.Setup(r => r.CanChangeEnabledState).Returns(true);
                    _UserManager.Setup(r => r.CanChangePassword).Returns(true);

                    var storeUser = TestUtilities.CreateMockInstance<IUser>();
                    var viewUser = TestUtilities.CreateMockInstance<IUser>();

                    storeUser.Setup(r => r.IsPersisted).Returns(true);
                    viewUser.Setup(r => r.IsPersisted).Returns(true);
                    storeUser.Object.UniqueId   =   viewUser.Object.UniqueId    = "1";
                    storeUser.Object.LoginName  =   viewUser.Object.LoginName   = "A";
                    storeUser.Object.Enabled    =   viewUser.Object.Enabled     = true;
                    storeUser.Object.Name       =   viewUser.Object.Name        = "B";

                    _UserManagerUsers.Add(viewUser.Object);
                    _Presenter.Initialise(_View.Object);

                    _UserManagerUsers.Clear();
                    _UserManagerUsers.Add(storeUser.Object);

                    string expectPassword = null;
                    if(makeChange) {
                        switch(field) {
                            case "Enabled":     viewUser.Object.Enabled = false; break;
                            case "LoginName":   viewUser.Object.LoginName = "X"; break;
                            case "Name":        viewUser.Object.Name = "Y"; break;
                            case "Password":    viewUser.Object.UIPassword = expectPassword = "Passw0rd"; break;
                            default:            throw new NotImplementedException();
                        }
                    }

                    _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);

                    if(makeChange)  _UserManager.Verify(r => r.UpdateUser(viewUser.Object, expectPassword), Times.Once());
                    else            _UserManager.Verify(r => r.UpdateUser(viewUser.Object, expectPassword), Times.Never());
                    _UserManager.Verify(r => r.CreateUser(viewUser.Object), Times.Never());
                    _UserManager.Verify(r => r.DeleteUser(viewUser.Object), Times.Never());
                }
            }
        }

        [TestMethod]
        public void OptionsPresenter_Users_New_Users_Passed_To_CreateUser()
        {
            _UserManager.Setup(r => r.CanCreateUsers).Returns(true);
            _UserManager.Setup(r => r.CanEditUsers).Returns(true);

            _Presenter.Initialise(_View.Object);

            var user = TestUtilities.CreateMockInstance<IUser>();
            _ViewUsers.Add(user.Object);
            user.Setup(r => r.IsPersisted).Returns(false);
            user.Object.Name = "A";
            user.Object.LoginName = "B";
            user.Object.Enabled = true;
            user.Object.UIPassword = "Abc123";

            _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);

            _UserManager.Verify(r => r.CreateUser(user.Object), Times.Once());
            _UserManager.Verify(r => r.UpdateUser(user.Object, It.IsAny<string>()), Times.Never());
            _UserManager.Verify(r => r.DeleteUser(user.Object), Times.Never());
        }

        [TestMethod]
        public void OptionsPresenter_Users_Deleted_Users_Passed_To_DeleteUser()
        {
            _UserManager.Setup(r => r.CanDeleteUsers).Returns(true);
            _UserManager.Setup(r => r.CanEditUsers).Returns(true);

            var user = TestUtilities.CreateMockInstance<IUser>();
            user.Object.Name = "A";
            user.Object.LoginName = "B";
            user.Object.Enabled = true;
            user.Object.UIPassword = "Abc123";
            _UserManagerUsers.Add(user.Object);

            _Presenter.Initialise(_View.Object);

            _ViewUsers.RemoveAt(0);
            _View.Raise(r => r.SaveClicked += null, EventArgs.Empty);

            _UserManager.Verify(r => r.DeleteUser(user.Object), Times.Once());
            _UserManager.Verify(r => r.CreateUser(user.Object), Times.Never());
            _UserManager.Verify(r => r.UpdateUser(user.Object, It.IsAny<string>()), Times.Never());
        }
        #endregion
        #endregion

        #region ResetToDefaultsClicked
        [TestMethod]
        public void OptionsPresenter_ResetToDefaultsClicked_Does_Not_Save_Default_Values()
        {
            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.ResetToDefaultsClicked += null, EventArgs.Empty);

            _ConfigurationStorage.Verify(c => c.Save(It.IsAny<Configuration>()), Times.Never());
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "BaseStationOptions$")]
        public void OptionsPresenter_ResetToDefaultsClicked_Copies_Default_BaseStation_Configuration_To_UI()
        {
            Check_ResetToDefaultClicked_Copies_Default_Configuration_To_UI(_BaseStationProperties);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AudioOptions$")]
        public void OptionsPresenter_ResetToDefaultsClicked_Copies_Default_Audio_Configuration_To_UI()
        {
            Check_ResetToDefaultClicked_Copies_Default_Configuration_To_UI(_AudioProperties);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GeneralOptions$")]
        public void OptionsPresenter_ResetToDefaultsClicked_Copies_Default_General_Configuration_To_UI()
        {
            Check_ResetToDefaultClicked_Copies_Default_Configuration_To_UI(_GeneralProperties);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "GoogleMapOptions$")]
        public void OptionsPresenter_ResetToDefaultsClicked_Copies_Default_GoogleMap_Configuration_To_UI()
        {
            Check_ResetToDefaultClicked_Copies_Default_Configuration_To_UI(_GoogleMapOptions);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "InternetClientOptions$")]
        public void OptionsPresenter_ResetToDefaultsClicked_Copies_Default_InternetClient_Configuration_To_UI()
        {
            Check_ResetToDefaultClicked_Copies_Default_Configuration_To_UI(_InternetClientOptions);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "WebServerOptions$")]
        public void OptionsPresenter_ResetToDefaultsClicked_Copies_Default_WebServer_Configuration_To_UI()
        {
            Check_ResetToDefaultClicked_Copies_Default_Configuration_To_UI(_WebServerOptions);
        }

        [TestMethod]
        [DataSource("Data Source='LibraryTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "RawDecodingOptions$")]
        public void OptionsPresenter_ResetToDefaultsClicked_Copies_Default_RawDecoding_Configuration_To_UI()
        {
            Check_ResetToDefaultClicked_Copies_Default_Configuration_To_UI(_RawDecodingOptions);
        }

        [TestMethod]
        public void OptionsPresenter_ResetToDefaultsClicked_Does_Not_Delete_ReceiverLocations()
        {
            // Personally I would be more than a little upset to enter half a dozen locations and then find that the button
            // had wiped them all out! :) We should preserve the receiver locations when the user clicks reset to default.
            var line1 = new ReceiverLocation() { UniqueId = 1, Name = "A", Latitude = 1.2, Longitude = 3.4 };
            var line2 = new ReceiverLocation() { UniqueId = 2, Name = "B", Latitude = 5.6, Longitude = 7.8 };
            _Configuration.ReceiverLocations.AddRange(new ReceiverLocation[] { line1, line2 });

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.ResetToDefaultsClicked += null, EventArgs.Empty);

            Assert.AreEqual(2, _View.Object.RawDecodingReceiverLocations.Count);
            Assert.IsTrue(_View.Object.RawDecodingReceiverLocations.Contains(line1));
            Assert.IsTrue(_View.Object.RawDecodingReceiverLocations.Contains(line2));
        }

        [TestMethod]
        public void OptionsPresenter_ResetToDefaultsClicked_Does_Not_Delete_Receivers()
        {
            _Configuration.Receivers.Clear();
            var line1 = new Receiver() { UniqueId = 1, Name = "A" };
            var line2 = new Receiver() { UniqueId = 2, Name = "B" };
            _Configuration.Receivers.AddRange(new Receiver[] { line1, line2 });

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.ResetToDefaultsClicked += null, EventArgs.Empty);

            Assert.AreEqual(2, _View.Object.Receivers.Count);
            Assert.IsTrue(_View.Object.Receivers.Contains(line1));
            Assert.IsTrue(_View.Object.Receivers.Contains(line2));
        }

        [TestMethod]
        public void OptionsPresenter_ResetToDefaultsClicked_Does_Not_Reset_WebSiteReceiverId()
        {
            _Configuration.GoogleMapSettings.WebSiteReceiverId = 1;

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.ResetToDefaultsClicked += null, EventArgs.Empty);

            Assert.AreEqual(1, _View.Object.WebSiteReceiverId);
        }

        [TestMethod]
        public void OptionsPresenter_ResetToDefaultsClicked_Does_Not_Reset_ClosestAircraftReceiverId()
        {
            _Configuration.GoogleMapSettings.ClosestAircraftReceiverId = 1;

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.ResetToDefaultsClicked += null, EventArgs.Empty);

            Assert.AreEqual(1, _View.Object.ClosestAircraftReceiverId);
        }

        [TestMethod]
        public void OptionsPresenter_ResetToDefaultsClicked_Does_Not_Reset_FlightSimulatorXReceiverId()
        {
            _Configuration.GoogleMapSettings.FlightSimulatorXReceiverId = 1;

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.ResetToDefaultsClicked += null, EventArgs.Empty);

            Assert.AreEqual(1, _View.Object.FlightSimulatorXReceiverId);
        }
        #endregion

        #region UseIcaoRawDecodingSettingsClicked
        [TestMethod]
        public void OptionsPresenter_UseIcaoRawDecodingSettingsClicked_Fills_View_With_Recommended_ICAO_Settings_For_Raw_Decoding()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.RawDecodingAcceptableAirborneSpeed = 9999;
            _View.Object.RawDecodingAcceptableAirSurfaceTransitionSpeed = 9999;
            _View.Object.RawDecodingAcceptableSurfaceSpeed = 9999;
            _View.Object.RawDecodingAirborneGlobalPositionLimit = 9999;
            _View.Object.RawDecodingFastSurfaceGlobalPositionLimit = 9999;
            _View.Object.RawDecodingSlowSurfaceGlobalPositionLimit = 9999;
            _View.Object.RawDecodingSuppressReceiverRangeCheck = true;
            _View.Object.RawDecodingUseLocalDecodeForInitialPosition = true;

            _View.Raise(v => v.UseIcaoRawDecodingSettingsClicked += null, EventArgs.Empty);

            Assert.AreEqual(11.112, _View.Object.RawDecodingAcceptableAirborneSpeed);
            Assert.AreEqual(4.63, _View.Object.RawDecodingAcceptableAirSurfaceTransitionSpeed);
            Assert.AreEqual(1.389, _View.Object.RawDecodingAcceptableSurfaceSpeed);
            Assert.AreEqual(10, _View.Object.RawDecodingAirborneGlobalPositionLimit);
            Assert.AreEqual(25, _View.Object.RawDecodingFastSurfaceGlobalPositionLimit);
            Assert.AreEqual(50, _View.Object.RawDecodingSlowSurfaceGlobalPositionLimit);
            Assert.AreEqual(false, _View.Object.RawDecodingSuppressReceiverRangeCheck);
            Assert.AreEqual(false, _View.Object.RawDecodingUseLocalDecodeForInitialPosition);
        }
        #endregion

        #region UseRecommendedRawDecodingSettingsClicked
        [TestMethod]
        public void OptionsPresenter_UseRecommendedRawDecodingSettingsClicked_Fills_View_With_Default_Settings_For_Raw_Decoding()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.RawDecodingAcceptableAirborneSpeed = 9999;
            _View.Object.RawDecodingAcceptableAirSurfaceTransitionSpeed = 9999;
            _View.Object.RawDecodingAcceptableSurfaceSpeed = 9999;
            _View.Object.RawDecodingAirborneGlobalPositionLimit = 9999;
            _View.Object.RawDecodingFastSurfaceGlobalPositionLimit = 9999;
            _View.Object.RawDecodingSlowSurfaceGlobalPositionLimit = 9999;
            _View.Object.RawDecodingSuppressReceiverRangeCheck = false;
            _View.Object.RawDecodingUseLocalDecodeForInitialPosition = true;

            _View.Raise(v => v.UseRecommendedRawDecodingSettingsClicked += null, EventArgs.Empty);

            var defaultValue = new RawDecodingSettings();

            Assert.AreEqual(defaultValue.AcceptableAirborneSpeed, _View.Object.RawDecodingAcceptableAirborneSpeed);
            Assert.AreEqual(defaultValue.AcceptableAirSurfaceTransitionSpeed, _View.Object.RawDecodingAcceptableAirSurfaceTransitionSpeed);
            Assert.AreEqual(defaultValue.AcceptableSurfaceSpeed, _View.Object.RawDecodingAcceptableSurfaceSpeed);
            Assert.AreEqual(defaultValue.AirborneGlobalPositionLimit, _View.Object.RawDecodingAirborneGlobalPositionLimit);
            Assert.AreEqual(defaultValue.FastSurfaceGlobalPositionLimit, _View.Object.RawDecodingFastSurfaceGlobalPositionLimit);
            Assert.AreEqual(defaultValue.SlowSurfaceGlobalPositionLimit, _View.Object.RawDecodingSlowSurfaceGlobalPositionLimit);
            Assert.AreEqual(true, _View.Object.RawDecodingSuppressReceiverRangeCheck);
            Assert.AreEqual(false, _View.Object.RawDecodingUseLocalDecodeForInitialPosition);
        }
        #endregion

        #region TestConnectionClicked
        [TestMethod]
        public void OptionsPresenter_TestConnectionClicked_Shows_Correct_Result_When_Network_Connection_Works()
        {
            _Presenter.Initialise(_View.Object);

            var receiver = new Receiver() {
                ConnectionType = ConnectionType.TCP,
                Address = "my address",
                Port = 100,
            };

            _View.Raise(v => v.TestConnectionClicked += null, new EventArgs<Receiver>(receiver));

            _Provider.Verify(p => p.TestNetworkConnection(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
            _Provider.Verify(p => p.TestNetworkConnection("my address", 100), Times.Once());

            _View.Verify(v => v.ShowTestConnectionResults(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            _View.Verify(v => v.ShowTestConnectionResults(Strings.CanConnectWithSettings, Strings.ConnectedSuccessfully), Times.Once());
        }

        [TestMethod]
        public void OptionsPresenter_TestConnectionClicked_Shows_Correct_Result_When_Serial_Connection_Works()
        {
            _Presenter.Initialise(_View.Object);

            var receiver = new Receiver() {
                ConnectionType = ConnectionType.COM,
                BaudRate = 2400,
                ComPort = "COM99",
                DataBits = 8,
                Handshake = Handshake.RequestToSendXOnXOff,
                Parity = Parity.Mark,
                StopBits = StopBits.One,
            };

            _View.Raise(v => v.TestConnectionClicked += null, new EventArgs<Receiver>(receiver));

            _Provider.Verify(p => p.TestSerialConnection(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StopBits>(), It.IsAny<Parity>(), It.IsAny<Handshake>()), Times.Once());
            _Provider.Verify(p => p.TestSerialConnection("COM99", 2400, 8, StopBits.One, Parity.Mark, Handshake.RequestToSendXOnXOff), Times.Once());

            _View.Verify(v => v.ShowTestConnectionResults(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            _View.Verify(v => v.ShowTestConnectionResults(Strings.CanConnectWithSettings, Strings.ConnectedSuccessfully), Times.Once());
        }

        [TestMethod]
        public void OptionsPresenter_TestConnectionClicked_Shows_Correct_Result_When_Network_Connection_Does_Not_Work()
        {
            _Presenter.Initialise(_View.Object);

            var receiver = new Receiver() {
                ConnectionType = ConnectionType.TCP,
                Address = "my address",
                Port = 100,
            };

            var exception = new InvalidOperationException("Exception text");
            _Provider.Setup(p => p.TestNetworkConnection(It.IsAny<string>(), It.IsAny<int>())).Returns(exception);

            _View.Raise(v => v.TestConnectionClicked += null, new EventArgs<Receiver>(receiver));

            _View.Verify(v => v.ShowTestConnectionResults(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            _View.Verify(v => v.ShowTestConnectionResults(String.Format("{0} {1}", Strings.CannotConnectWithSettings, exception.Message), Strings.CannotConnect), Times.Once());
        }

        [TestMethod]
        public void OptionsPresenter_TestConnectionClicked_Shows_Correct_Result_When_Serial_Connection_Does_Not_Work()
        {
            _Presenter.Initialise(_View.Object);

            var receiver = new Receiver() {
                ConnectionType = ConnectionType.COM,
                BaudRate = 2400,
                ComPort = "COM99",
                DataBits = 8,
                Handshake = Handshake.RequestToSendXOnXOff,
                Parity = Parity.Mark,
                StopBits = StopBits.One,
            };

            var exception = new InvalidOperationException("Exception text");
            _Provider.Setup(p => p.TestSerialConnection(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<StopBits>(), It.IsAny<Parity>(), It.IsAny<Handshake>())).Returns(exception);

            _View.Raise(v => v.TestConnectionClicked += null, new EventArgs<Receiver>(receiver));

            _View.Verify(v => v.ShowTestConnectionResults(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            _View.Verify(v => v.ShowTestConnectionResults(String.Format("{0} {1}", Strings.CannotConnectWithSettings, exception.Message), Strings.CannotConnect), Times.Once());
        }

        [TestMethod]
        public void OptionsPresenter_TestConnectionClicked_Shows_GUI_Is_Busy_While_Testing()
        {
            var previousState = new Object();
            _View.Setup(v => v.ShowBusy(It.IsAny<bool>(), It.IsAny<object>())).Returns((bool isBusy, object prev) => {
                return isBusy ? previousState : null;
            });

            _Provider.Setup(p => p.TestNetworkConnection(It.IsAny<string>(), It.IsAny<int>())).Callback(() => {
                _View.Verify(v => v.ShowBusy(true, null), Times.Once());
            });

            _View.Setup(v => v.ShowTestConnectionResults(It.IsAny<string>(), It.IsAny<string>())).Callback(() => {
                _View.Verify(v => v.ShowBusy(false, previousState), Times.Once());
            });

            _Presenter.Initialise(_View.Object);

            var receiver = new Receiver() {
                ConnectionType = ConnectionType.TCP,
            };

            _View.Raise(v => v.TestConnectionClicked += null, new EventArgs<Receiver>(receiver));

            _View.Verify(v => v.ShowBusy(It.IsAny<bool>(), It.IsAny<object>()), Times.Exactly(2));
        }
        #endregion

        #region TestTextToSpeechSettingsClicked
        [TestMethod]
        public void OptionsPresenter_TestTextToSpeechSettingsClicked_Runs_Text_To_Speech_Test()
        {
            _Presenter.Initialise(_View.Object);

            _View.Object.TextToSpeechVoice = "the voice";
            _View.Object.TextToSpeechSpeed = 90;
            _View.Raise(v => v.TestTextToSpeechSettingsClicked += null, EventArgs.Empty);

            _Provider.Verify(p => p.TestTextToSpeech("the voice", 90), Times.Once());
            _Provider.Verify(p => p.TestTextToSpeech(It.IsAny<string>(), It.IsAny<int>()), Times.Once());
        }
        #endregion

        #region UpdateReceiverLocationsFromBaseStationDatabaseClicked
        [TestMethod]
        public void OptionsPresenter_UpdateReceiverLocationsFromBaseStationDatabaseClicked_Adds_Entries_From_BaseStation_Database()
        {
            var location1 = new BaseStationLocation() { LocationName = "A", Latitude = 1.2, Longitude = 3.4 };
            var location2 = new BaseStationLocation() { LocationName = "B", Latitude = 5.6, Longitude = 7.8 };
            _BaseStationDatabase.Setup(r => r.GetLocations()).Returns(new BaseStationLocation[] { location1, location2 });

            var receiverLocations = new List<ReceiverLocation>();
            _View.Setup(r => r.MergeBaseStationDatabaseReceiverLocations(It.IsAny<IEnumerable<ReceiverLocation>>())).Callback((IEnumerable<ReceiverLocation> locations) => {
                receiverLocations.AddRange(locations);
            });

            _Presenter.Initialise(_View.Object);
            _View.Raise(v => v.UpdateReceiverLocationsFromBaseStationDatabaseClicked += null, EventArgs.Empty);

            _View.Verify(v => v.MergeBaseStationDatabaseReceiverLocations(It.IsAny<IEnumerable<ReceiverLocation>>()), Times.Once());
            Assert.AreEqual(2, receiverLocations.Count);
            var locationA = receiverLocations.Single(r => r.Name == "A");
            var locationB = receiverLocations.Single(r => r.Name == "B");
            Assert.AreEqual(1.2, locationA.Latitude);
            Assert.AreEqual(3.4, locationA.Longitude);
            Assert.AreEqual(5.6, locationB.Latitude);
            Assert.AreEqual(7.8, locationB.Longitude);
            Assert.IsTrue(locationA.IsBaseStationLocation);
            Assert.IsTrue(locationB.IsBaseStationLocation);
        }
        #endregion
    }
}
