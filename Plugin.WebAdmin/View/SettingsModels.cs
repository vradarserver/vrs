// Copyright © 2016 onwards, Andrew Whewell
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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.PortableBinding;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Interface.WebSite.WebAdminModels;

namespace VirtualRadar.Plugin.WebAdmin.View.Settings
{
    public class ViewModel
    {
        public string CurrentUserName { get; set; }

        public ConfigurationModel Configuration { get; set; }

        public string Outcome { get; set; }

        public MergedFeedModel NewMergedFeed { get; set; }

        public RebroadcastServerModel NewRebroadcastServer { get; set; }

        public ReceiverModel NewReceiver { get; set; }

        public ReceiverLocationModel NewReceiverLocation { get; set; }

        public UserModel NewUser { get; set; }

        public EnumModel[] ConnectionTypes { get; private set; }

        public ReceiverFormatName[] DataSources { get; private set; }

        public EnumModel[] DefaultAccesses { get; private set; }

        public EnumModel[] DistanceUnits { get; private set; }

        public EnumModel[] Handshakes { get; private set; }

        public EnumModel[] HeightUnits { get; private set; }

        public EnumModel[] Parities { get; private set; }

        public EnumModel[] ProxyTypes { get; private set; }

        public RebroadcastFormatName[] RebroadcastFormats { get; private set; }

        public EnumModel[] ReceiverUsages { get; private set; }

        public EnumModel[] SpeedUnits { get; private set; }

        public EnumModel[] StopBits { get; private set; }

        public string[] ComPortNames { get; set; }

        public string[] VoiceNames { get; set; }

        public ViewModel()
        {
            Configuration = new ConfigurationModel();

            var receiverFormatManager = Factory.Singleton.ResolveSingleton<IReceiverFormatManager>();
            var rebroadcastFormatManager = Factory.Singleton.ResolveSingleton<IRebroadcastFormatManager>();

            ConnectionTypes =       EnumModel.CreateFromEnum<ConnectionType>(r => Describe.ConnectionType(r));
            DataSources =           receiverFormatManager.GetRegisteredFormats();
            DefaultAccesses =       EnumModel.CreateFromEnum<DefaultAccess>(r => Describe.DefaultAccess(r));
            DistanceUnits =         EnumModel.CreateFromEnum<DistanceUnit>(r => Describe.DistanceUnit(r));
            Handshakes =            EnumModel.CreateFromEnum<Handshake>(r => Describe.Handshake(r));
            HeightUnits =           EnumModel.CreateFromEnum<HeightUnit>(r => Describe.HeightUnit(r));
            Parities =              EnumModel.CreateFromEnum<Parity>(r => Describe.Parity(r));
            ProxyTypes =            EnumModel.CreateFromEnum<ProxyType>(r => Describe.ProxyType(r));
            RebroadcastFormats =    rebroadcastFormatManager.GetRegisteredFormats();
            ReceiverUsages =        EnumModel.CreateFromEnum<ReceiverUsage>(r => Describe.ReceiverUsage(r));
            SpeedUnits =            EnumModel.CreateFromEnum<SpeedUnit>(r => Describe.SpeedUnit(r));
            StopBits =              EnumModel.CreateFromEnum<StopBits>(r => Describe.StopBits(r));

            ComPortNames = new string[]{};
            VoiceNames = new string[]{};
        }

        public object FindViewModelForRecord(ValidationResult validationResult)
        {
            object result = null;

            if(validationResult.Record != null) {
                var mergedFeed = validationResult.Record as MergedFeed;
                if(mergedFeed != null) {
                    result = Configuration.MergedFeeds.FirstOrDefault(r => r.UniqueId == mergedFeed.UniqueId);
                }

                var rebroadcastSettings = validationResult.Record as RebroadcastSettings;
                if(rebroadcastSettings != null) {
                    result = Configuration.RebroadcastSettings.FirstOrDefault(r => r.UniqueId == rebroadcastSettings.UniqueId);
                }

                var receiver = validationResult.Record as Receiver;
                if(receiver != null) {
                    result = Configuration.Receivers.FirstOrDefault(r => r.UniqueId == receiver.UniqueId);
                }

                var receiverLocation = validationResult.Record as ReceiverLocation;
                if(receiverLocation != null) {
                    result = Configuration.ReceiverLocations.FirstOrDefault(r => r.UniqueId == receiverLocation.UniqueId);
                }

                var user = validationResult.Record as IUser;
                if(user != null) {
                    result = Configuration.Users.FirstOrDefault(r => r.UniqueId == user.UniqueId);
                }
            }

            return result;
        }
    }

    public class TestConnectionOutcomeModel
    {
        public string Title { get; set; }

        public string Message { get; set; }
    }

    public class ConfigurationModel
    {
        public int DataVersion { get; set; }

        public string OnlineLookupSupplierName { get; set; }

        public string OnlineLookupSupplierCredits { get; set; }

        public string OnlineLookupSupplierUrl { get; set; }

        public AudioSettingsModel AudioSettings { get; private set; }

        public BaseStationSettingsModel BaseStationSettings { get; private set; }

        public FlightRouteSettingsModel FlightRouteSettings { get; private set; }

        public GoogleMapSettingsModel GoogleMapSettings { get; private set; }

        public InternetClientSettingsModel InternetClientSettings { get; private set; }

        public RawDecodingSettingModel RawDecodingSettings { get; private set; }

        public VersionCheckSettingsModel VersionCheckSettings { get; private set; }

        public WebServerSettingsModel WebServerSettings { get; private set; }

        public List<MergedFeedModel> MergedFeeds { get; private set; }

        public List<RebroadcastServerModel> RebroadcastSettings { get; private set; }

        public List<ReceiverModel> Receivers { get; private set; }

        public List<ReceiverLocationModel> ReceiverLocations { get; private set; }

        public List<UserModel> Users { get; private set; }

        public ConfigurationModel()
        {
            AudioSettings = new AudioSettingsModel();
            BaseStationSettings = new BaseStationSettingsModel();
            FlightRouteSettings = new FlightRouteSettingsModel();
            GoogleMapSettings = new GoogleMapSettingsModel();
            InternetClientSettings = new InternetClientSettingsModel();
            MergedFeeds = new List<MergedFeedModel>();
            RawDecodingSettings = new RawDecodingSettingModel();
            RebroadcastSettings = new List<RebroadcastServerModel>();
            Receivers = new List<ReceiverModel>();
            ReceiverLocations = new List<ReceiverLocationModel>();
            Users = new List<UserModel>();
            VersionCheckSettings = new VersionCheckSettingsModel();
            WebServerSettings = new WebServerSettingsModel();
        }

        public ConfigurationModel(Configuration configuration, NotifyList<IUser> users) : this()
        {
            RefreshFromConfiguration(configuration, users);
        }

        public void RefreshFromConfiguration(Configuration configuration, NotifyList<IUser> users)
        {
            DataVersion = configuration.DataVersion;

            AudioSettings.RefreshFromSettings(configuration.AudioSettings);
            BaseStationSettings.RefreshFromSettings(configuration.BaseStationSettings);
            FlightRouteSettings.RefreshFromSettings(configuration.FlightRouteSettings);
            GoogleMapSettings.RefreshFromSettings(configuration.GoogleMapSettings);
            InternetClientSettings.RefreshFromSettings(configuration.InternetClientSettings);
            RawDecodingSettings.RefreshFromSettings(configuration.RawDecodingSettings);
            VersionCheckSettings.RefreshFromSettings(configuration.VersionCheckSettings);
            WebServerSettings.RefreshFromSettings(configuration.WebServerSettings);

            CollectionHelper.ApplySourceToDestination(configuration.MergedFeeds, MergedFeeds,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => new MergedFeedModel(source),
                (source, dest) => dest.RefreshFromSettings(source)
            );
            MergedFeeds.Sort((lhs, rhs) => String.Compare(lhs.Name, rhs.Name));

            CollectionHelper.ApplySourceToDestination(configuration.RebroadcastSettings, RebroadcastSettings,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => new RebroadcastServerModel(source),
                (source, dest) => dest.RefreshFromSettings(source)
            );
            RebroadcastSettings.Sort((lhs, rhs) => String.Compare(lhs.Name, rhs.Name));

            CollectionHelper.ApplySourceToDestination(configuration.Receivers, Receivers,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => new ReceiverModel(source),
                (source, dest) => dest.RefreshFromSettings(source)
            );
            Receivers.Sort((lhs, rhs) => String.Compare(lhs.Name, rhs.Name));

            CollectionHelper.ApplySourceToDestination(configuration.ReceiverLocations, ReceiverLocations,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => new ReceiverLocationModel(source),
                (source, dest) => dest.RefreshFromSettings(source)
            );
            ReceiverLocations.Sort((lhs, rhs) => String.Compare(lhs.Name, rhs.Name));

            CollectionHelper.ApplySourceToDestination(users, Users,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => new UserModel(source),
                (source, dest) => dest.RefreshFromSettings(source)
            );
            Users.Sort((lhs, rhs) => String.Compare(lhs.LoginName, rhs.LoginName));
        }

        public void CopyToConfiguration(Configuration configuration, NotifyList<IUser> users, ISettingsPresenter presenter)
        {
            configuration.DataVersion = DataVersion;

            AudioSettings.CopyToSettings(configuration.AudioSettings);
            BaseStationSettings.CopyToSettings(configuration.BaseStationSettings);
            FlightRouteSettings.CopyToSettings(configuration.FlightRouteSettings);
            GoogleMapSettings.CopyToSettings(configuration.GoogleMapSettings);
            InternetClientSettings.CopyToSettings(configuration.InternetClientSettings);
            RawDecodingSettings.CopyToSettings(configuration.RawDecodingSettings);
            VersionCheckSettings.CopyToSettings(configuration.VersionCheckSettings);
            WebServerSettings.CopyToSettings(configuration.WebServerSettings);

            CollectionHelper.ApplySourceToDestination(MergedFeeds, configuration.MergedFeeds,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => source.CopyToSettings(new MergedFeed()),
                (source, dest) => source.CopyToSettings(dest)
            );
            CollectionHelper.ApplySourceToDestination(RebroadcastSettings, configuration.RebroadcastSettings,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => source.CopyToSettings(new RebroadcastSettings()),
                (source, dest) => source.CopyToSettings(dest)
            );
            CollectionHelper.ApplySourceToDestination(Receivers, configuration.Receivers,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => source.CopyToSettings(new Receiver()),
                (source, dest) => source.CopyToSettings(dest)
            );
            CollectionHelper.ApplySourceToDestination(ReceiverLocations, configuration.ReceiverLocations,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => source.CopyToSettings(new ReceiverLocation()),
                (source, dest) => source.CopyToSettings(dest)
            );
            CollectionHelper.ApplySourceToDestination(Users, users,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => source.CopyToSettings(presenter.CreateUser()),
                (source, dest) => source.CopyToSettings(dest)
            );
        }
    }

    public class AudioSettingsModel
    {
        public bool Enabled { get; set; }

        public string VoiceName { get; set; }

        public int VoiceRate { get; set; }

        [ValidationModelField(ValidationField.TextToSpeechSpeed)]
        public ValidationModelField VoiceRateValidation { get; set; }

        public AudioSettingsModel()
        {
        }

        public AudioSettingsModel(AudioSettings settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(AudioSettings settings)
        {
            Enabled =   settings.Enabled;
            VoiceName = settings.VoiceName;
            VoiceRate = settings.VoiceRate;
        }

        public AudioSettings CopyToSettings(AudioSettings settings)
        {
            settings.Enabled =   Enabled;
            settings.VoiceName = VoiceName;
            settings.VoiceRate = VoiceRate;

            return settings;
        }
    }

    public class BaseStationSettingsModel
    {
        public string DatabaseFileName { get; set; }

        [ValidationModelField(ValidationField.BaseStationDatabase)]
        public ValidationModelField DatabaseFileNameValidation { get; set; }

        public string OperatorFlagsFolder { get; set; }

        [ValidationModelField(ValidationField.FlagsFolder)]
        public ValidationModelField OperatorFlagsFolderValidation { get; set; }

        public string SilhouettesFolder { get; set; }

        [ValidationModelField(ValidationField.SilhouettesFolder)]
        public ValidationModelField SilhouettesFolderValidation { get; set; }

        public string PicturesFolder { get; set; }

        [ValidationModelField(ValidationField.PicturesFolder)]
        public ValidationModelField PicturesFolderValidation { get; set; }

        public bool SearchPictureSubFolders { get; set; }

        public int DisplayTimeoutSeconds { get; set; }

        [ValidationModelField(ValidationField.DisplayTimeout)]
        public ValidationModelField DisplayTimeoutSecondsValidation { get; set; }

        public int TrackingTimeoutSeconds { get; set; }

        [ValidationModelField(ValidationField.TrackingTimeout)]
        public ValidationModelField TrackingTimeoutSecondsValidation { get; set; }

        public int SatcomDisplayTimeoutMinutes { get; set; }

        [ValidationModelField(ValidationField.SatcomDisplayTimeout)]
        public ValidationModelField SatcomDisplayTimeoutMinutesValidation { get; set; }

        public int SatcomTrackingTimeoutMinutes { get; set; }

        [ValidationModelField(ValidationField.SatcomTrackingTimeout)]
        public ValidationModelField SatcomTrackingTimeoutMinutesValidation { get; set; }

        public bool MinimiseToSystemTray { get; set; }

        public int AutoSavePolarPlotsMinutes { get; set; }

        [ValidationModelField(ValidationField.AutoSavePolarPlots)]
        public ValidationModelField AutoSavePolarPlotsMinutesValidation { get; set; }

        public bool LookupAircraftDetailsOnline { get; set; }

        public bool DownloadGlobalAirPressureReadings { get; set; }

        public BaseStationSettingsModel()
        {
        }

        public BaseStationSettingsModel(BaseStationSettings settings) : this()
        {
            ValidationModelHelper.CreateEmptyViewModelValidationFields(this);
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(BaseStationSettings settings)
        {
            DatabaseFileName =                  settings.DatabaseFileName;
            OperatorFlagsFolder =               settings.OperatorFlagsFolder;
            SilhouettesFolder =                 settings.SilhouettesFolder;
            PicturesFolder =                    settings.PicturesFolder;
            SearchPictureSubFolders =           settings.SearchPictureSubFolders;
            DisplayTimeoutSeconds =             settings.DisplayTimeoutSeconds;
            TrackingTimeoutSeconds =            settings.TrackingTimeoutSeconds;
            SatcomDisplayTimeoutMinutes =       settings.SatcomDisplayTimeoutMinutes;
            SatcomTrackingTimeoutMinutes =      settings.SatcomTrackingTimeoutMinutes;
            MinimiseToSystemTray =              settings.MinimiseToSystemTray;
            AutoSavePolarPlotsMinutes =         settings.AutoSavePolarPlotsMinutes;
            LookupAircraftDetailsOnline =       settings.LookupAircraftDetailsOnline;
            DownloadGlobalAirPressureReadings = settings.DownloadGlobalAirPressureReadings;
        }

        public BaseStationSettings CopyToSettings(BaseStationSettings settings)
        {
            settings.DatabaseFileName =                     DatabaseFileName;
            settings.OperatorFlagsFolder =                  OperatorFlagsFolder;
            settings.SilhouettesFolder =                    SilhouettesFolder;
            settings.PicturesFolder =                       PicturesFolder;
            settings.SearchPictureSubFolders =              SearchPictureSubFolders;
            settings.DisplayTimeoutSeconds =                DisplayTimeoutSeconds;
            settings.TrackingTimeoutSeconds =               TrackingTimeoutSeconds;
            settings.SatcomDisplayTimeoutMinutes =          SatcomDisplayTimeoutMinutes;
            settings.SatcomTrackingTimeoutMinutes =         SatcomTrackingTimeoutMinutes;
            settings.MinimiseToSystemTray =                 MinimiseToSystemTray;
            settings.AutoSavePolarPlotsMinutes =            AutoSavePolarPlotsMinutes;
            settings.LookupAircraftDetailsOnline =          LookupAircraftDetailsOnline;
            settings.DownloadGlobalAirPressureReadings =    DownloadGlobalAirPressureReadings;

            return settings;
        }
    }

    public class FlightRouteSettingsModel
    {
        public bool AutoUpdateEnabled { get; set; }

        public FlightRouteSettingsModel()
        {
        }

        public FlightRouteSettingsModel(FlightRouteSettings settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(FlightRouteSettings settings)
        {
            AutoUpdateEnabled = settings.AutoUpdateEnabled;
        }

        public FlightRouteSettings CopyToSettings(FlightRouteSettings settings)
        {
            settings.AutoUpdateEnabled = AutoUpdateEnabled;

            return settings;
        }
    }

    public class GoogleMapSettingsModel
    {
        public string InitialSettings { get; set; }

        [ValidationModelField(ValidationField.ExportedSettings)]
        public ValidationModelField InitialSettingsValidation { get; set; }

        public double InitialMapLatitude { get; set; }

        public double InitialMapLongitude { get; set; }

        public string InitialMapType { get; set; }

        public int InitialMapZoom { get; set; }

        public int InitialRefreshSeconds { get; set; }

        [ValidationModelField(ValidationField.InitialGoogleMapRefreshSeconds)]
        public ValidationModelField InitialRefreshSecondsValidation { get; set; }

        public int MinimumRefreshSeconds { get; set; }

        [ValidationModelField(ValidationField.MinimumGoogleMapRefreshSeconds)]
        public ValidationModelField MinimumRefreshSecondsValidation { get; set; }

        public int ShortTrailLengthSeconds { get; set; }

        [ValidationModelField(ValidationField.ShortTrailLength)]
        public ValidationModelField ShortTrailLengthSecondsValidation { get; set; }

        public int InitialDistanceUnit { get; set; }            // DistanceUnit

        public int InitialHeightUnit { get; set; }              // HeightUnit

        public int InitialSpeedUnit { get; set; }               // SpeedUnit

        public bool PreferIataAirportCodes { get; set; }

        public bool EnableBundling { get; set; }

        public bool EnableMinifying { get; set; }

        public bool EnableCompression { get; set; }

        public int WebSiteReceiverId { get; set; }

        [ValidationModelField(ValidationField.WebSiteReceiver)]
        public ValidationModelField WebSiteReceiverIdValidation { get; set; }

        public string DirectoryEntryKey { get; set; }

        public int ClosestAircraftReceiverId { get; set; }

        [ValidationModelField(ValidationField.ClosestAircraftReceiver)]
        public ValidationModelField ClosestAircraftReceiverIdValidation { get; set; }

        public int FlightSimulatorXReceiverId { get; set; }

        [ValidationModelField(ValidationField.FlightSimulatorXReceiver)]
        public ValidationModelField FlightSimulatorXReceiverIdValidation { get; set; }

        public int ProxyType { get; set; }                      // ProxyType

        public bool EnableCorsSupport { get; set; }

        public string AllowCorsDomains { get; set; }

        [ValidationModelField(ValidationField.AllowCorsDomains)]
        public ValidationModelField AllowCorsDomainsValidation { get; set; }

        public string GoogleMapsApiKey { get; set; }

        public bool UseGoogleMapsAPIKeyWithLocalRequests { get; set; }

        public bool UseSvgGraphicsOnDesktop { get; set; }

        public bool UseSvgGraphicsOnMobile { get; set; }

        public bool UseSvgGraphicsOnReports { get; set; }

        public GoogleMapSettingsModel()
        {
        }

        public GoogleMapSettingsModel(GoogleMapSettings settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(GoogleMapSettings settings)
        {
            InitialSettings =                       settings.InitialSettings;
            InitialMapLatitude =                    settings.InitialMapLatitude;
            InitialMapLongitude =                   settings.InitialMapLongitude;
            InitialMapType =                        settings.InitialMapType;
            InitialMapZoom =                        settings.InitialMapZoom;
            InitialRefreshSeconds =                 settings.InitialRefreshSeconds;
            MinimumRefreshSeconds =                 settings.MinimumRefreshSeconds;
            ShortTrailLengthSeconds =               settings.ShortTrailLengthSeconds;
            InitialDistanceUnit =                   (int)settings.InitialDistanceUnit;
            InitialHeightUnit =                     (int)settings.InitialHeightUnit;
            InitialSpeedUnit =                      (int)settings.InitialSpeedUnit;
            PreferIataAirportCodes =                settings.PreferIataAirportCodes;
            EnableBundling =                        settings.EnableBundling;
            EnableMinifying =                       settings.EnableMinifying;
            EnableCompression =                     settings.EnableCompression;
            WebSiteReceiverId =                     settings.WebSiteReceiverId;
            DirectoryEntryKey =                     settings.DirectoryEntryKey;
            ClosestAircraftReceiverId =             settings.ClosestAircraftReceiverId;
            FlightSimulatorXReceiverId =            settings.FlightSimulatorXReceiverId;
            ProxyType =                             (int)settings.ProxyType;
            EnableCorsSupport =                     settings.EnableCorsSupport;
            AllowCorsDomains =                      settings.AllowCorsDomains;
            GoogleMapsApiKey =                      settings.GoogleMapsApiKey;
            UseGoogleMapsAPIKeyWithLocalRequests =  settings.UseGoogleMapsAPIKeyWithLocalRequests;
            UseSvgGraphicsOnDesktop =               settings.UseSvgGraphicsOnDesktop;
            UseSvgGraphicsOnMobile =                settings.UseSvgGraphicsOnMobile;
            UseSvgGraphicsOnReports =               settings.UseSvgGraphicsOnReports;
        }

        public GoogleMapSettings CopyToSettings(GoogleMapSettings settings)
        {
            settings.InitialSettings =                      InitialSettings;
            settings.InitialMapLatitude =                   InitialMapLatitude;
            settings.InitialMapLongitude =                  InitialMapLongitude;
            settings.InitialMapType =                       InitialMapType;
            settings.InitialMapZoom =                       InitialMapZoom;
            settings.InitialRefreshSeconds =                InitialRefreshSeconds;
            settings.MinimumRefreshSeconds =                MinimumRefreshSeconds;
            settings.ShortTrailLengthSeconds =              ShortTrailLengthSeconds;
            settings.InitialDistanceUnit =                  EnumModel.CastFromInt<DistanceUnit>(InitialDistanceUnit);
            settings.InitialHeightUnit =                    EnumModel.CastFromInt<HeightUnit>(InitialHeightUnit);
            settings.InitialSpeedUnit =                     EnumModel.CastFromInt<SpeedUnit>(InitialSpeedUnit);
            settings.PreferIataAirportCodes =               PreferIataAirportCodes;
            settings.EnableBundling =                       EnableBundling;
            settings.EnableMinifying =                      EnableMinifying;
            settings.EnableCompression =                    EnableCompression;
            settings.WebSiteReceiverId =                    WebSiteReceiverId;
            settings.DirectoryEntryKey =                    DirectoryEntryKey;
            settings.ClosestAircraftReceiverId =            ClosestAircraftReceiverId;
            settings.FlightSimulatorXReceiverId =           FlightSimulatorXReceiverId;
            settings.ProxyType =                            EnumModel.CastFromInt<ProxyType>(ProxyType);
            settings.EnableCorsSupport =                    EnableCorsSupport;
            settings.AllowCorsDomains =                     AllowCorsDomains;
            settings.GoogleMapsApiKey =                     GoogleMapsApiKey;
            settings.UseGoogleMapsAPIKeyWithLocalRequests = UseGoogleMapsAPIKeyWithLocalRequests;
            settings.UseSvgGraphicsOnDesktop =              UseSvgGraphicsOnDesktop;
            settings.UseSvgGraphicsOnMobile =               UseSvgGraphicsOnMobile;
            settings.UseSvgGraphicsOnReports =              UseSvgGraphicsOnReports;

            return settings;
        }
    }

    public class InternetClientSettingsModel
    {
        public bool CanRunReports { get; set; }

        public bool CanShowPinText { get; set; }

        public bool CanPlayAudio { get; set; }

        public bool CanShowPictures { get; set; }

        public int TimeoutMinutes { get; set; }

        [ValidationModelField(ValidationField.InternetUserIdleTimeout)]
        public ValidationModelField TimeoutMinutesValidation { get; set; }

        public bool AllowInternetProximityGadgets { get; set; }

        public bool CanSubmitRoutes { get; set; }

        public bool CanShowPolarPlots { get; set; }

        public InternetClientSettingsModel()
        {
        }

        public InternetClientSettingsModel(InternetClientSettings settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(InternetClientSettings settings)
        {
            CanRunReports =                     settings.CanRunReports;
            CanShowPinText =                    settings.CanShowPinText;
            CanPlayAudio =                      settings.CanPlayAudio;
            CanShowPictures =                   settings.CanShowPictures;
            TimeoutMinutes =                    settings.TimeoutMinutes;
            AllowInternetProximityGadgets =     settings.AllowInternetProximityGadgets;
            CanSubmitRoutes =                   settings.CanSubmitRoutes;
            CanShowPolarPlots =                 settings.CanShowPolarPlots;
        }

        public InternetClientSettings CopyToSettings(InternetClientSettings settings)
        {
            settings.CanRunReports =                    CanRunReports;
            settings.CanShowPinText =                   CanShowPinText;
            settings.CanPlayAudio =                     CanPlayAudio;
            settings.CanShowPictures =                  CanShowPictures;
            settings.TimeoutMinutes =                   TimeoutMinutes;
            settings.AllowInternetProximityGadgets =    AllowInternetProximityGadgets;
            settings.CanSubmitRoutes =                  CanSubmitRoutes;
            settings.CanShowPolarPlots =                CanShowPolarPlots;

            return settings;
        }
    }

    public class MergedFeedModel
    {
        public bool Enabled { get; set; }

        public int UniqueId { get; set; }

        public string Name { get; set; }

        [ValidationModelField(ValidationField.Name)]
        public ValidationModelField NameValidation { get; set; }

        public List<int> ReceiverIds { get; private set; }

        [ValidationModelField(ValidationField.ReceiverIds)]
        public ValidationModelField ReceiverIdsValidation { get; set; }

        public List<MergedFeedReceiverModel> ReceiverFlags { get; private set; }

        public int IcaoTimeout { get; set; }

        [ValidationModelField(ValidationField.IcaoTimeout)]
        public ValidationModelField IcaoTimeoutValidation { get; set; }

        public bool IgnoreAircraftWithNoPosition { get; set; }

        public ReceiverUsage ReceiverUsage { get; set; }

        public MergedFeedModel()
        {
            ReceiverIds = new List<int>();
            ReceiverFlags = new List<MergedFeedReceiverModel>();
        }

        public MergedFeedModel(MergedFeed settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(MergedFeed settings)
        {
            Enabled =                       settings.Enabled;
            UniqueId =                      settings.UniqueId;
            Name =                          settings.Name;
            IcaoTimeout =                   settings.IcaoTimeout;
            IgnoreAircraftWithNoPosition =  settings.IgnoreAircraftWithNoPosition;
            ReceiverUsage =                 settings.ReceiverUsage;

            ReceiverFlags.Clear();
            ReceiverIds.Clear();
            ReceiverIds.AddRange(settings.ReceiverIds);
            ReceiverFlags.AddRange(settings.ReceiverFlags.Select(r => new MergedFeedReceiverModel(r)));
        }

        public MergedFeed CopyToSettings(MergedFeed settings)
        {
            settings.Enabled =                      Enabled;
            settings.UniqueId =                     UniqueId;
            settings.Name =                         Name;
            settings.IcaoTimeout =                  IcaoTimeout;
            settings.IgnoreAircraftWithNoPosition = IgnoreAircraftWithNoPosition;
            settings.ReceiverUsage =                ReceiverUsage;

            settings.ReceiverFlags.Clear();
            settings.ReceiverIds.Clear();
            settings.ReceiverIds.AddRange(ReceiverIds);
            settings.ReceiverFlags.AddRange(ReceiverFlags.Select(r => r.CopyToSettings(new MergedFeedReceiver())));

            return settings;
        }
    }

    public class MergedFeedReceiverModel
    {
        public int UniqueId { get; set; }

        public bool IsMlatFeed { get; set; }

        public MergedFeedReceiverModel()
        {
        }

        public MergedFeedReceiverModel(MergedFeedReceiver settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(MergedFeedReceiver settings)
        {
            UniqueId =      settings.UniqueId;
            IsMlatFeed =    settings.IsMlatFeed;
        }

        public MergedFeedReceiver CopyToSettings(MergedFeedReceiver settings)
        {
            settings.UniqueId =     UniqueId;
            settings.IsMlatFeed =   IsMlatFeed;

            return settings;
        }
    }

    public class RawDecodingSettingModel
    {
        public int ReceiverRange { get; set; }

        [ValidationModelField(ValidationField.ReceiverRange)]
        public ValidationModelField ReceiverRangeValidation { get; set; }

        public bool IgnoreMilitaryExtendedSquitter { get; set; }

        public bool SuppressReceiverRangeCheck { get; set; }

        public bool UseLocalDecodeForInitialPosition { get; set; }

        public int AirborneGlobalPositionLimit { get; set; }

        [ValidationModelField(ValidationField.AirborneGlobalPositionLimit)]
        public ValidationModelField AirborneGlobalPositionLimitValidation { get; set; }

        public int FastSurfaceGlobalPositionLimit { get; set; }

        [ValidationModelField(ValidationField.FastSurfaceGlobalPositionLimit)]
        public ValidationModelField FastSurfaceGlobalPositionLimitValidation { get; set; }

        public int SlowSurfaceGlobalPositionLimit { get; set; }

        [ValidationModelField(ValidationField.SlowSurfaceGlobalPositionLimit)]
        public ValidationModelField SlowSurfaceGlobalPositionLimitValidation { get; set; }

        public double AcceptableAirborneSpeed { get; set; }

        [ValidationModelField(ValidationField.AcceptableAirborneLocalPositionSpeed)]
        public ValidationModelField AcceptableAirborneSpeedValidation { get; set; }

        public double AcceptableAirSurfaceTransitionSpeed { get; set; }

        [ValidationModelField(ValidationField.AcceptableTransitionLocalPositionSpeed)]
        public ValidationModelField AcceptableAirSurfaceTransitionSpeedValidation { get; set; }

        public double AcceptableSurfaceSpeed { get; set; }

        [ValidationModelField(ValidationField.AcceptableSurfaceLocalPositionSpeed)]
        public ValidationModelField AcceptableSurfaceSpeedValidation { get; set; }

        public bool IgnoreCallsignsInBds20 { get; set; }

        public int AcceptIcaoInPI0Count { get; set; }

        [ValidationModelField(ValidationField.AcceptIcaoInPI0Count)]
        public ValidationModelField AcceptIcaoInPI0CountValidation { get; set; }

        public int AcceptIcaoInPI0Seconds { get; set; }

        [ValidationModelField(ValidationField.AcceptIcaoInPI0Seconds)]
        public ValidationModelField AcceptIcaoInPI0SecondsValidation { get; set; }

        public int AcceptIcaoInNonPICount { get; set; }

        [ValidationModelField(ValidationField.AcceptIcaoInNonPICount)]
        public ValidationModelField AcceptIcaoInNonPICountValidation { get; set; }

        public int AcceptIcaoInNonPISeconds { get; set; }

        [ValidationModelField(ValidationField.AcceptIcaoInNonPISeconds)]
        public ValidationModelField AcceptIcaoInNonPISecondsValidation { get; set; }

        public bool SuppressIcao0 { get; set; }

        public bool IgnoreInvalidCodeBlockInParityMessages { get; set; }

        public bool IgnoreInvalidCodeBlockInOtherMessages { get; set; }

        public bool SuppressTisbDecoding { get; set; }

        public RawDecodingSettingModel()
        {
        }

        public RawDecodingSettingModel(RawDecodingSettings settings)
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(RawDecodingSettings settings)
        {
            ReceiverRange =                             settings.ReceiverRange;
            IgnoreMilitaryExtendedSquitter =            settings.IgnoreMilitaryExtendedSquitter;
            SuppressReceiverRangeCheck =                settings.SuppressReceiverRangeCheck;
            UseLocalDecodeForInitialPosition =          settings.UseLocalDecodeForInitialPosition;
            AirborneGlobalPositionLimit =               settings.AirborneGlobalPositionLimit;
            FastSurfaceGlobalPositionLimit =            settings.FastSurfaceGlobalPositionLimit;
            SlowSurfaceGlobalPositionLimit =            settings.SlowSurfaceGlobalPositionLimit;
            AcceptableAirborneSpeed =                   settings.AcceptableAirborneSpeed;
            AcceptableAirSurfaceTransitionSpeed =       settings.AcceptableAirSurfaceTransitionSpeed;
            AcceptableSurfaceSpeed =                    settings.AcceptableSurfaceSpeed;
            IgnoreCallsignsInBds20 =                    settings.IgnoreCallsignsInBds20;
            AcceptIcaoInPI0Count =                      settings.AcceptIcaoInPI0Count;
            AcceptIcaoInPI0Seconds =                    settings.AcceptIcaoInPI0Seconds;
            AcceptIcaoInNonPICount =                    settings.AcceptIcaoInNonPICount;
            AcceptIcaoInNonPISeconds =                  settings.AcceptIcaoInNonPISeconds;
            SuppressIcao0 =                             settings.SuppressIcao0;
            IgnoreInvalidCodeBlockInParityMessages =    settings.IgnoreInvalidCodeBlockInParityMessages;
            IgnoreInvalidCodeBlockInOtherMessages =     settings.IgnoreInvalidCodeBlockInOtherMessages;
            SuppressTisbDecoding =                      settings.SuppressTisbDecoding;
        }

        public RawDecodingSettings CopyToSettings(RawDecodingSettings settings)
        {
            settings.ReceiverRange =                            ReceiverRange;
            settings.IgnoreMilitaryExtendedSquitter =           IgnoreMilitaryExtendedSquitter;
            settings.SuppressReceiverRangeCheck =               SuppressReceiverRangeCheck;
            settings.UseLocalDecodeForInitialPosition =         UseLocalDecodeForInitialPosition;
            settings.AirborneGlobalPositionLimit =              AirborneGlobalPositionLimit;
            settings.FastSurfaceGlobalPositionLimit =           FastSurfaceGlobalPositionLimit;
            settings.SlowSurfaceGlobalPositionLimit =           SlowSurfaceGlobalPositionLimit;
            settings.AcceptableAirborneSpeed =                  AcceptableAirborneSpeed;
            settings.AcceptableAirSurfaceTransitionSpeed =      AcceptableAirSurfaceTransitionSpeed;
            settings.AcceptableSurfaceSpeed =                   AcceptableSurfaceSpeed;
            settings.IgnoreCallsignsInBds20 =                   IgnoreCallsignsInBds20;
            settings.AcceptIcaoInPI0Count =                     AcceptIcaoInPI0Count;
            settings.AcceptIcaoInPI0Seconds =                   AcceptIcaoInPI0Seconds;
            settings.AcceptIcaoInNonPICount =                   AcceptIcaoInNonPICount;
            settings.AcceptIcaoInNonPISeconds =                 AcceptIcaoInNonPISeconds;
            settings.SuppressIcao0 =                            SuppressIcao0;
            settings.IgnoreInvalidCodeBlockInParityMessages =   IgnoreInvalidCodeBlockInParityMessages;
            settings.IgnoreInvalidCodeBlockInOtherMessages =    IgnoreInvalidCodeBlockInOtherMessages;
            settings.SuppressTisbDecoding =                     SuppressTisbDecoding;

            return settings;
        }
    }

    public class RebroadcastServerModel
    {
        public int UniqueId { get; set; }

        public string Name { get; set; }

        [ValidationModelField(ValidationField.Name)]
        public ValidationModelField NameValidation { get; set; }

        public bool Enabled { get; set; }

        public int ReceiverId { get; set; }

        [ValidationModelField(ValidationField.RebroadcastReceiver)]
        public ValidationModelField ReceiverIdValidation { get; set; }

        public string Format { get; set; }

        [ValidationModelField(ValidationField.Format)]
        public ValidationModelField FormatValidation { get; set; }

        public bool IsTransmitter { get; set; }

        [ValidationModelField(ValidationField.IsTransmitter)]
        public ValidationModelField IsTransmitterValidation { get; set; }

        public string TransmitAddress { get; set; }

        [ValidationModelField(ValidationField.BaseStationAddress)]
        public ValidationModelField TransmitAddressValidation { get; set; }

        public int Port { get; set; }

        [ValidationModelField(ValidationField.RebroadcastServerPort)]
        public ValidationModelField PortValidation { get; set; }

        public bool UseKeepAlive { get; set; }

        [ValidationModelField(ValidationField.UseKeepAlive)]
        public ValidationModelField UseKeepAliveValidation { get; set; }

        public int IdleTimeoutMilliseconds { get; set; }

        [ValidationModelField(ValidationField.IdleTimeout)]
        public ValidationModelField IdleTimeoutMillisecondsValidation { get; set; }

        public int StaleSeconds { get; set; }

        [ValidationModelField(ValidationField.StaleSeconds)]
        public ValidationModelField StaleSecondsValidation { get; set; }

        public AccessModel Access { get; set; }

        public string Passphrase { get; set; }

        public int SendIntervalMilliseconds { get; set; }

        [ValidationModelField(ValidationField.SendInterval)]
        public ValidationModelField SendIntervalMillisecondsValidation { get; set; }

        public RebroadcastServerModel()
        {
            Access = new AccessModel();
        }

        public RebroadcastServerModel(RebroadcastSettings settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(RebroadcastSettings settings)
        {
            UniqueId =                  settings.UniqueId;
            Name =                      settings.Name;
            Enabled =                   settings.Enabled;
            ReceiverId =                settings.ReceiverId;
            Format =                    settings.Format;
            IsTransmitter =             settings.IsTransmitter;
            TransmitAddress =           settings.TransmitAddress;
            Port =                      settings.Port;
            UseKeepAlive =              settings.UseKeepAlive;
            IdleTimeoutMilliseconds =   settings.IdleTimeoutMilliseconds;
            StaleSeconds =              settings.StaleSeconds;
            Passphrase =                settings.Passphrase;
            SendIntervalMilliseconds =  settings.SendIntervalMilliseconds;

            Access.RefreshFromSettings(settings.Access);
        }

        public RebroadcastSettings CopyToSettings(RebroadcastSettings settings)
        {
            settings.UniqueId =                  UniqueId;
            settings.Name =                      Name;
            settings.Enabled =                   Enabled;
            settings.ReceiverId =                ReceiverId;
            settings.Format =                    Format;
            settings.IsTransmitter =             IsTransmitter;
            settings.TransmitAddress =           TransmitAddress;
            settings.Port =                      Port;
            settings.UseKeepAlive =              UseKeepAlive;
            settings.IdleTimeoutMilliseconds =   IdleTimeoutMilliseconds;
            settings.StaleSeconds =              StaleSeconds;
            settings.Passphrase =                Passphrase;
            settings.SendIntervalMilliseconds =  SendIntervalMilliseconds;

            Access.CopyToSettings(settings.Access);

            return settings;
        }
    }

    public class ReceiverModel
    {
        public bool Enabled { get; set; }

        [ValidationModelField(ValidationField.Enabled)]
        public ValidationModelField EnabledValidation { get; set; }

        public int UniqueId { get; set; }

        public string Name { get; set; }

        [ValidationModelField(ValidationField.Name)]
        public ValidationModelField NameValidation { get; set; }

        public string DataSource { get; set; }

        [ValidationModelField(ValidationField.Format)]
        public ValidationModelField DataSourceValidation { get; set; }

        public bool IsSatcomFeed { get; set; }
        
        public int ConnectionType { get; set; }             // ConnectionType

        public bool AutoReconnectAtStartup { get; set; }

        public bool IsPassive { get; set; }

        [ValidationModelField(ValidationField.IsPassive)]
        public ValidationModelField IsPassiveValidation { get; set; }

        public AccessModel Access { get; private set; }

        public string Address { get; set; }

        [ValidationModelField(ValidationField.BaseStationAddress)]
        public ValidationModelField AddressValidation { get; set; }

        public int Port { get; set; }

        [ValidationModelField(ValidationField.BaseStationPort)]
        public ValidationModelField PortValidation { get; set; }

        public bool UseKeepAlive { get; set; }

        [ValidationModelField(ValidationField.UseKeepAlive)]
        public ValidationModelField UseKeepAliveValidation { get; set; }

        public int IdleTimeoutMilliseconds { get; set; }

        [ValidationModelField(ValidationField.IdleTimeout)]
        public ValidationModelField IdleTimeoutValidation { get; set; }

        public string Passphrase { get; set; }

        public string ComPort { get; set; }

        [ValidationModelField(ValidationField.ComPort)]
        public ValidationModelField ComPortValidation { get; set; }

        public int BaudRate { get; set; }

        [ValidationModelField(ValidationField.BaudRate)]
        public ValidationModelField BaudRateValidation { get; set; }

        public int DataBits { get; set; }

        [ValidationModelField(ValidationField.DataBits)]
        public ValidationModelField DataBitsValidation { get; set; }

        public int StopBits { get; set; }                   // StopBits

        public int Parity { get; set; }                     // Parity

        public int Handshake { get; set; }                  // Handshake

        public string StartupText { get; set; }

        public string ShutdownText { get; set; }

        public int ReceiverLocationId { get; set; }

        [ValidationModelField(ValidationField.Location)]
        public ValidationModelField ReceiverLocationIdValidation { get; set; }

        public int ReceiverUsage { get; set; }              // ReceiverUsage

        public ReceiverModel()
        {
            Access = new AccessModel();
        }

        public ReceiverModel(Receiver settings) : this()
        {
            ValidationModelHelper.CreateEmptyViewModelValidationFields(this);
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(Receiver settings)
        {
            Enabled =                       settings.Enabled;
            UniqueId =                      settings.UniqueId;
            Name =                          settings.Name;
            DataSource =                    settings.DataSource;
            IsSatcomFeed =                  settings.IsSatcomFeed;
            ConnectionType =                (int)settings.ConnectionType;
            AutoReconnectAtStartup =        settings.AutoReconnectAtStartup;
            IsPassive =                     settings.IsPassive;
            Address =                       settings.Address;
            Port =                          settings.Port;
            UseKeepAlive =                  settings.UseKeepAlive;
            IdleTimeoutMilliseconds =       settings.IdleTimeoutMilliseconds;
            Passphrase =                    settings.Passphrase;
            ComPort =                       settings.ComPort;
            BaudRate =                      settings.BaudRate;
            DataBits =                      settings.DataBits;
            StopBits =                      (int)settings.StopBits;
            Parity =                        (int)settings.Parity;
            Handshake =                     (int)settings.Handshake;
            StartupText =                   settings.StartupText;
            ShutdownText =                  settings.ShutdownText;
            ReceiverLocationId =            settings.ReceiverLocationId;
            ReceiverUsage =                 (int)settings.ReceiverUsage;

            Access.RefreshFromSettings(settings.Access);
        }

        public Receiver CopyToSettings(Receiver settings)
        {
            settings.Enabled =                  Enabled;
            settings.UniqueId =                 UniqueId;
            settings.Name =                     Name;
            settings.DataSource =               DataSource;
            settings.IsSatcomFeed =             IsSatcomFeed;
            settings.ConnectionType =           EnumModel.CastFromInt<ConnectionType>(ConnectionType);
            settings.AutoReconnectAtStartup =   AutoReconnectAtStartup;
            settings.IsPassive =                IsPassive;
            settings.Address =                  Address;
            settings.Port =                     Port;
            settings.UseKeepAlive =             UseKeepAlive;
            settings.IdleTimeoutMilliseconds =  IdleTimeoutMilliseconds;
            settings.Passphrase =               Passphrase;
            settings.ComPort =                  ComPort;
            settings.BaudRate =                 BaudRate;
            settings.DataBits =                 DataBits;
            settings.StopBits =                 EnumModel.CastFromInt<StopBits>(StopBits);
            settings.Parity =                   EnumModel.CastFromInt<Parity>(Parity);
            settings.Handshake =                EnumModel.CastFromInt<Handshake>(Handshake);
            settings.StartupText =              StartupText;
            settings.ShutdownText =             ShutdownText;
            settings.ReceiverLocationId =       ReceiverLocationId;
            settings.ReceiverUsage =            EnumModel.CastFromInt<ReceiverUsage>(ReceiverUsage);

            Access.CopyToSettings(settings.Access);

            return settings;
        }
    }

    public class ReceiverLocationModel
    {
        public int UniqueId { get; set; }

        public string Name { get; set; }

        [ValidationModelField(ValidationField.Location)]
        public ValidationModelField NameValidation { get; set; }

        public double Latitude { get; set; }

        [ValidationModelField(ValidationField.Latitude)]
        public ValidationModelField LatitudeValidation { get; set; }

        public double Longitude { get; set; }

        [ValidationModelField(ValidationField.Longitude)]
        public ValidationModelField LongitudeValidation { get; set; }

        public bool IsBaseStationLocation { get; set; }

        public ReceiverLocationModel()
        {
        }

        public ReceiverLocationModel(ReceiverLocation settings) : this()
        {
            ValidationModelHelper.CreateEmptyViewModelValidationFields(this);
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(ReceiverLocation settings)
        {
            UniqueId =              settings.UniqueId;
            Name =                  settings.Name;
            Latitude =              settings.Latitude;
            Longitude =             settings.Longitude;
            IsBaseStationLocation = settings.IsBaseStationLocation;
        }

        public ReceiverLocation CopyToSettings(ReceiverLocation settings)
        {
            settings.UniqueId =                 UniqueId;
            settings.Name =                     Name;
            settings.Latitude =                 Latitude;
            settings.Longitude =                Longitude;
            settings.IsBaseStationLocation =    IsBaseStationLocation;

            return settings;
        }
    }

    public class UserModel
    {
        public string UniqueId { get; set; }

        public bool Enabled { get; set; }

        public string LoginName { get; set; }

        [ValidationModelField(ValidationField.LoginName)]
        public ValidationModelField LoginNameValidation { get; set; }

        public string Name { get; set; }

        [ValidationModelField(ValidationField.Name)]
        public ValidationModelField NameValidation { get; set; }

        public string UIPassword { get; set; }

        [ValidationModelField(ValidationField.Password)]
        public ValidationModelField UIPasswordValidation { get; set; }

        public UserModel()
        {
        }

        public UserModel(IUser user) : this()
        {
            RefreshFromSettings(user);
        }

        public void RefreshFromSettings(IUser settings)
        {
            UniqueId =      settings.UniqueId;
            Enabled =       settings.Enabled;
            LoginName =     settings.LoginName;
            Name =          settings.Name;
            UIPassword =    settings.UIPassword;
        }

        public IUser CopyToSettings(IUser settings)
        {
            settings.UniqueId =     UniqueId;
            settings.Enabled =      Enabled;
            settings.LoginName =    LoginName;
            settings.Name =         Name;
            settings.UIPassword =   UIPassword;

            return settings;
        }
    }

    public class VersionCheckSettingsModel
    {
        public bool CheckAutomatically { get; set; }

        public int CheckPeriodDays { get; set; }

        [ValidationModelField(ValidationField.CheckForNewVersions)]
        public ValidationModelField CheckPeriodDaysValidation { get; set; }

        public VersionCheckSettingsModel()
        {
        }

        public VersionCheckSettingsModel(VersionCheckSettings settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(VersionCheckSettings settings)
        {
            CheckAutomatically =    settings.CheckAutomatically;
            CheckPeriodDays =       settings.CheckPeriodDays;
        }

        public VersionCheckSettings CopyToSettings(VersionCheckSettings settings)
        {
            settings.CheckAutomatically =   CheckAutomatically;
            settings.CheckPeriodDays =      CheckPeriodDays;

            return settings;
        }
    }

    public class WebServerSettingsModel
    {
        public bool UsersMustAuthenticate { get; set; }

        public List<string> BasicAuthenticationUserIds { get; private set; }

        public List<string> AdministratorUserIds { get; private set; }

        public bool EnableUPnp { get; set; }

        public int UPnpPort { get; set; }

        [ValidationModelField(ValidationField.UPnpPortNumber)]
        public ValidationModelField UPnpPortValidation { get; set; }

        public bool IsOnlyInternetServerOnLan { get; set; }

        public bool AutoStartUPnP { get; set; }

        public WebServerSettingsModel()
        {
            BasicAuthenticationUserIds = new List<string>();
            AdministratorUserIds = new List<string>();
        }

        public WebServerSettingsModel(WebServerSettings settings) : this()
        {
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(WebServerSettings settings)
        {
            UsersMustAuthenticate =         settings.AuthenticationScheme == AuthenticationSchemes.Basic;
            EnableUPnp =                    settings.EnableUPnp;
            UPnpPort =                      settings.UPnpPort;
            IsOnlyInternetServerOnLan =     settings.IsOnlyInternetServerOnLan;
            AutoStartUPnP =                 settings.AutoStartUPnP;

            CollectionHelper.OverwriteDestinationWithSource(settings.BasicAuthenticationUserIds, BasicAuthenticationUserIds);
            CollectionHelper.OverwriteDestinationWithSource(settings.AdministratorUserIds, AdministratorUserIds);
        }

        public WebServerSettings CopyToSettings(WebServerSettings settings)
        {
            settings.AuthenticationScheme =         UsersMustAuthenticate ? AuthenticationSchemes.Basic : AuthenticationSchemes.Anonymous;
            settings.EnableUPnp =                   EnableUPnp;
            settings.UPnpPort =                     UPnpPort;
            settings.IsOnlyInternetServerOnLan =    IsOnlyInternetServerOnLan;
            settings.AutoStartUPnP =                AutoStartUPnP;

            CollectionHelper.OverwriteDestinationWithSource(BasicAuthenticationUserIds, settings.BasicAuthenticationUserIds);
            CollectionHelper.OverwriteDestinationWithSource(AdministratorUserIds, settings.AdministratorUserIds);

            return settings;
        }
    }
}
