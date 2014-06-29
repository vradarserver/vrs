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
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="IOptionsPresenter"/>.
    /// </summary>
    sealed class OptionsPresenter : IOptionsPresenter
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of the provider.
        /// </summary>
        class DefaultProvider : IOptionsPresenterProvider
        {
            private ISpeechSynthesizerWrapper _SpeechSynthesizer;

            private string _DefaultVoiceName;

            public DefaultProvider()
            {
                _SpeechSynthesizer = Factory.Singleton.Resolve<ISpeechSynthesizerWrapper>();
            }

            public void Dispose()
            {
                if(_SpeechSynthesizer != null) {
                    _SpeechSynthesizer.Dispose();
                    _SpeechSynthesizer = null;
                }
            }

            public bool FileExists(string fileName)     { return File.Exists(fileName); }

            public bool FolderExists(string folder)     { return Directory.Exists(folder); }

            public Exception TestNetworkConnection(string address, int port)
            {
                Exception result = null;
                try {
                    UriBuilder builder = new UriBuilder("telnet", address, port);
                    Uri uri = builder.Uri;
                    using(TcpClient client = new TcpClient()) {
                        client.Connect(address, port);
                    }
                } catch(Exception ex) {
                    result = ex;
                }

                return result;
            }

            public Exception TestSerialConnection(string comPort, int baudRate, int dataBits, StopBits stopBits, Parity parity, Handshake handShake)
            {
                Exception result = null;
                try {
                    using(var serialPort = new SerialPort(comPort, baudRate, parity, dataBits, stopBits)) {
                        serialPort.Handshake = handShake;
                        serialPort.ReadTimeout = 250;
                        serialPort.WriteTimeout = 250;
                        serialPort.Open();
                        byte[] buffer = new byte[128];
                        serialPort.Read(buffer, 0, buffer.Length);
                        serialPort.Write(new byte[1] { 0x10 }, 0, 1);
                    }
                } catch(TimeoutException) {
                } catch(Exception ex) {
                    result = ex;
                }

                return result;
            }

            public IEnumerable<string> GetVoiceNames()
            {
                List<string> result = new List<string>();
                result.Add(null);

                _DefaultVoiceName = _SpeechSynthesizer.DefaultVoiceName;

                foreach(var installedVoiceName in _SpeechSynthesizer.GetInstalledVoiceNames()) {
                    result.Add(installedVoiceName);
                }

                return result;
            }

            public void TestTextToSpeech(string name, int rate)
            {
                _SpeechSynthesizer.Dispose();
                _SpeechSynthesizer = Factory.Singleton.Resolve<ISpeechSynthesizerWrapper>();

                _SpeechSynthesizer.SelectVoice(name ?? _DefaultVoiceName);
                _SpeechSynthesizer.Rate = rate;
                _SpeechSynthesizer.SetOutputToDefaultAudioDevice();
                _SpeechSynthesizer.SpeakAsync("From L. F. P. G., Paris to E. D. D. T., Berlin");
            }
        }
        #endregion

        #region Private class - CachedFileSystemResult
        /// <summary>
        /// The cached result of a check against a file system entity.
        /// </summary>
        class CachedFileSystemResult
        {
            public object ValidationRecord;
            public ValidationField ValidationField;
            public string FileSystemEntityName;
            public ValidationResult ValidationResult;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The GUI object that we're controlling.
        /// </summary>
        private IOptionsView _View;

        /// <summary>
        /// The cache of file system results.
        /// </summary>
        private List<CachedFileSystemResult> _CachedFileSystemResults = new List<CachedFileSystemResult>();

        /// <summary>
        /// The password that we use to indicate that the password has not been changed by the user.
        /// </summary>
        private string _DefaultPassword = "A\t\t\t\t\tA";
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IOptionsPresenterProvider Provider { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OptionsPresenter()
        {
            Provider = new DefaultProvider();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~OptionsPresenter()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) Provider.Dispose();
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public void Initialise(IOptionsView view)
        {
            _View = view;
            _View.ResetToDefaultsClicked += View_ResetToDefaultsClicked;
            _View.SaveClicked += View_SaveClicked;
            _View.TestConnectionClicked += View_TestConnectionClicked;
            _View.TestTextToSpeechSettingsClicked += View_TestTextToSpeechSettingsClicked;
            _View.UpdateReceiverLocationsFromBaseStationDatabaseClicked += View_UpdateReceiverLocationsFromBaseStationDatabaseClicked;
            _View.UseIcaoRawDecodingSettingsClicked += View_UseIcaoRawDecodingSettingsClicked;
            _View.UseRecommendedRawDecodingSettingsClicked += View_UseRecommendedRawDecodingSettingsClicked;
            _View.ValueChanged += View_ValueChanged;

            _View.PopulateTextToSpeechVoices(Provider.GetVoiceNames());

            var configuration = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton.Load();
            CopyConfigurationToUI(configuration);
        }
        #endregion

        #region CopyConfigurationToUI, CopyUIToConfiguration
        /// <summary>
        /// Loads the configuration and copies it to the user interface.
        /// </summary>
        /// <param name="configuration"></param>
        private void CopyConfigurationToUI(Configuration configuration)
        {
            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            var allUsers = userManager.GetUsers().Select(r => { r.UIPassword = _DefaultPassword; return r; }).ToArray();
            _View.Users.AddRange(allUsers);

            _View.AudioEnabled = configuration.AudioSettings.Enabled;
            _View.TextToSpeechSpeed = configuration.AudioSettings.VoiceRate;
            _View.TextToSpeechVoice = String.IsNullOrEmpty(configuration.AudioSettings.VoiceName) ? null : configuration.AudioSettings.VoiceName;
            _View.RebroadcastSettings.AddRange(configuration.RebroadcastSettings);
            _View.Receivers.AddRange(configuration.Receivers);
            _View.MergedFeeds.AddRange(configuration.MergedFeeds);

            _View.BaseStationDatabaseFileName = configuration.BaseStationSettings.DatabaseFileName;
            _View.DisplayTimeoutSeconds = configuration.BaseStationSettings.DisplayTimeoutSeconds;
            _View.TrackingTimeoutSeconds = configuration.BaseStationSettings.TrackingTimeoutSeconds;
            _View.OperatorFlagsFolder = configuration.BaseStationSettings.OperatorFlagsFolder;
            _View.PicturesFolder = configuration.BaseStationSettings.PicturesFolder;
            _View.SearchPictureSubFolders = configuration.BaseStationSettings.SearchPictureSubFolders;
            _View.SilhouettesFolder = configuration.BaseStationSettings.SilhouettesFolder;
            _View.MinimiseToSystemTray = configuration.BaseStationSettings.MinimiseToSystemTray;

            _View.RawDecodingReceiverLocations.AddRange(configuration.ReceiverLocations);

            _View.DownloadFlightRoutes = configuration.FlightRouteSettings.AutoUpdateEnabled;

            _View.InitialGoogleMapLatitude = configuration.GoogleMapSettings.InitialMapLatitude;
            _View.InitialGoogleMapLongitude = configuration.GoogleMapSettings.InitialMapLongitude;
            _View.InitialGoogleMapType = configuration.GoogleMapSettings.InitialMapType;
            _View.InitialGoogleMapZoom = configuration.GoogleMapSettings.InitialMapZoom;
            _View.InitialGoogleMapRefreshSeconds = configuration.GoogleMapSettings.InitialRefreshSeconds;
            _View.MinimumGoogleMapRefreshSeconds = configuration.GoogleMapSettings.MinimumRefreshSeconds;
            _View.ShortTrailLengthSeconds = configuration.GoogleMapSettings.ShortTrailLengthSeconds;
            _View.InitialDistanceUnit = configuration.GoogleMapSettings.InitialDistanceUnit;
            _View.InitialHeightUnit = configuration.GoogleMapSettings.InitialHeightUnit;
            _View.InitialSpeedUnit = configuration.GoogleMapSettings.InitialSpeedUnit;
            _View.PreferIataAirportCodes = configuration.GoogleMapSettings.PreferIataAirportCodes;
            _View.EnableBundling = configuration.GoogleMapSettings.EnableBundling;
            _View.EnableMinifying = configuration.GoogleMapSettings.EnableMinifying;
            _View.EnableCompression = configuration.GoogleMapSettings.EnableCompression;
            _View.ProxyType = configuration.GoogleMapSettings.ProxyType;
            _View.WebSiteReceiverId = configuration.GoogleMapSettings.WebSiteReceiverId;
            _View.ClosestAircraftReceiverId = configuration.GoogleMapSettings.ClosestAircraftReceiverId;
            _View.FlightSimulatorXReceiverId = configuration.GoogleMapSettings.FlightSimulatorXReceiverId;

            _View.InternetClientCanPlayAudio = configuration.InternetClientSettings.CanPlayAudio;
            _View.InternetClientCanRunReports = configuration.InternetClientSettings.CanRunReports;
            _View.InternetClientCanSeeLabels = configuration.InternetClientSettings.CanShowPinText;
            _View.InternetClientCanSeePictures = configuration.InternetClientSettings.CanShowPictures;
            _View.InternetClientTimeoutMinutes = configuration.InternetClientSettings.TimeoutMinutes;
            _View.AllowInternetProximityGadgets = configuration.InternetClientSettings.AllowInternetProximityGadgets;
            _View.InternetClientCanSubmitRoutes = configuration.InternetClientSettings.CanSubmitRoutes;
            _View.InternetClientCanShowPolarPlots = configuration.InternetClientSettings.CanShowPolarPlots;

            _View.CheckForNewVersions = configuration.VersionCheckSettings.CheckAutomatically;
            _View.CheckForNewVersionsPeriodDays = configuration.VersionCheckSettings.CheckPeriodDays;

            _View.WebServerUserMustAuthenticate = configuration.WebServerSettings.AuthenticationScheme == AuthenticationSchemes.Basic;
            _View.WebServerUsers.AddRange(GetSubsetOfUsers(allUsers, configuration.WebServerSettings.BasicAuthenticationUserIds));
            _View.EnableUPnpFeatures = configuration.WebServerSettings.EnableUPnp;
            _View.IsOnlyVirtualRadarServerOnLan = configuration.WebServerSettings.IsOnlyInternetServerOnLan;
            _View.AutoStartUPnp = configuration.WebServerSettings.AutoStartUPnP;
            _View.UPnpPort = configuration.WebServerSettings.UPnpPort;

            _View.RawDecodingAcceptableAirborneSpeed = configuration.RawDecodingSettings.AcceptableAirborneSpeed;
            _View.RawDecodingAcceptableAirSurfaceTransitionSpeed = configuration.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed;
            _View.RawDecodingAcceptableSurfaceSpeed = configuration.RawDecodingSettings.AcceptableSurfaceSpeed;
            _View.RawDecodingAirborneGlobalPositionLimit = configuration.RawDecodingSettings.AirborneGlobalPositionLimit;
            _View.RawDecodingFastSurfaceGlobalPositionLimit = configuration.RawDecodingSettings.FastSurfaceGlobalPositionLimit;
            _View.RawDecodingIgnoreCallsignsInBds20 = configuration.RawDecodingSettings.IgnoreCallsignsInBds20;
            _View.RawDecodingIgnoreMilitaryExtendedSquitter = configuration.RawDecodingSettings.IgnoreMilitaryExtendedSquitter;
            _View.RawDecodingReceiverRange = configuration.RawDecodingSettings.ReceiverRange;
            _View.RawDecodingSlowSurfaceGlobalPositionLimit = configuration.RawDecodingSettings.SlowSurfaceGlobalPositionLimit;
            _View.RawDecodingSuppressReceiverRangeCheck = configuration.RawDecodingSettings.SuppressReceiverRangeCheck;
            _View.RawDecodingUseLocalDecodeForInitialPosition = configuration.RawDecodingSettings.UseLocalDecodeForInitialPosition;
            _View.AcceptIcaoInNonPICount = configuration.RawDecodingSettings.AcceptIcaoInNonPICount;
            _View.AcceptIcaoInNonPISeconds = configuration.RawDecodingSettings.AcceptIcaoInNonPISeconds;
            _View.AcceptIcaoInPI0Count = configuration.RawDecodingSettings.AcceptIcaoInPI0Count;
            _View.AcceptIcaoInPI0Seconds = configuration.RawDecodingSettings.AcceptIcaoInPI0Seconds;
        }

        private IUser[] GetSubsetOfUsers(IUser[] allUsers, IEnumerable<string> selectedUserIds)
        {
            var result = selectedUserIds.Select(r => allUsers.FirstOrDefault(i => i.UniqueId == r)).Where(r => r != null).ToArray();
            return result;
        }

        /// <summary>
        /// Copies the configuration settings from the UI to the user interface.
        /// </summary>
        /// <param name="configuration"></param>
        private void CopyUIToConfiguration(Configuration configuration)
        {
            // By this point we should have saved changes to the users
            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            var allUsers = userManager.GetUsers().Select(r => { r.UIPassword = _DefaultPassword; return r; }).ToArray();

            configuration.AudioSettings.Enabled = _View.AudioEnabled;
            configuration.AudioSettings.VoiceName = _View.TextToSpeechVoice;
            configuration.AudioSettings.VoiceRate = _View.TextToSpeechSpeed;
            configuration.RebroadcastSettings.Clear();
            configuration.RebroadcastSettings.AddRange(_View.RebroadcastSettings);
            configuration.Receivers.Clear();
            configuration.Receivers.AddRange(_View.Receivers);
            configuration.MergedFeeds.Clear();
            configuration.MergedFeeds.AddRange(_View.MergedFeeds);

            configuration.BaseStationSettings.DatabaseFileName = _View.BaseStationDatabaseFileName;
            configuration.BaseStationSettings.DisplayTimeoutSeconds = _View.DisplayTimeoutSeconds;
            configuration.BaseStationSettings.TrackingTimeoutSeconds = _View.TrackingTimeoutSeconds;
            configuration.BaseStationSettings.PicturesFolder = _View.PicturesFolder;
            configuration.BaseStationSettings.SearchPictureSubFolders = _View.SearchPictureSubFolders;
            configuration.BaseStationSettings.OperatorFlagsFolder = _View.OperatorFlagsFolder;
            configuration.BaseStationSettings.SilhouettesFolder = _View.SilhouettesFolder;
            configuration.BaseStationSettings.MinimiseToSystemTray = _View.MinimiseToSystemTray;

            configuration.ReceiverLocations.Clear();
            configuration.ReceiverLocations.AddRange(_View.RawDecodingReceiverLocations);

            configuration.FlightRouteSettings.AutoUpdateEnabled = _View.DownloadFlightRoutes;

            configuration.GoogleMapSettings.InitialMapLatitude = _View.InitialGoogleMapLatitude;
            configuration.GoogleMapSettings.InitialMapLongitude = _View.InitialGoogleMapLongitude;
            configuration.GoogleMapSettings.InitialMapType = _View.InitialGoogleMapType;
            configuration.GoogleMapSettings.InitialMapZoom = _View.InitialGoogleMapZoom;
            configuration.GoogleMapSettings.InitialRefreshSeconds = _View.InitialGoogleMapRefreshSeconds;
            configuration.GoogleMapSettings.MinimumRefreshSeconds = _View.MinimumGoogleMapRefreshSeconds;
            configuration.GoogleMapSettings.ShortTrailLengthSeconds = _View.ShortTrailLengthSeconds;
            configuration.GoogleMapSettings.InitialDistanceUnit = _View.InitialDistanceUnit;
            configuration.GoogleMapSettings.InitialHeightUnit = _View.InitialHeightUnit;
            configuration.GoogleMapSettings.InitialSpeedUnit = _View.InitialSpeedUnit;
            configuration.GoogleMapSettings.PreferIataAirportCodes = _View.PreferIataAirportCodes;
            configuration.GoogleMapSettings.EnableBundling = _View.EnableBundling;
            configuration.GoogleMapSettings.EnableMinifying = _View.EnableMinifying;
            configuration.GoogleMapSettings.EnableCompression = _View.EnableCompression;
            configuration.GoogleMapSettings.ProxyType = _View.ProxyType;
            configuration.GoogleMapSettings.WebSiteReceiverId = _View.WebSiteReceiverId;
            configuration.GoogleMapSettings.ClosestAircraftReceiverId = _View.ClosestAircraftReceiverId;
            configuration.GoogleMapSettings.FlightSimulatorXReceiverId = _View.FlightSimulatorXReceiverId;

            configuration.InternetClientSettings.AllowInternetProximityGadgets = _View.AllowInternetProximityGadgets;
            configuration.InternetClientSettings.CanPlayAudio = _View.InternetClientCanPlayAudio;
            configuration.InternetClientSettings.CanRunReports = _View.InternetClientCanRunReports;
            configuration.InternetClientSettings.CanShowPictures = _View.InternetClientCanSeePictures;
            configuration.InternetClientSettings.CanShowPinText = _View.InternetClientCanSeeLabels;
            configuration.InternetClientSettings.TimeoutMinutes = _View.InternetClientTimeoutMinutes;
            configuration.InternetClientSettings.CanSubmitRoutes = _View.InternetClientCanSubmitRoutes;
            configuration.InternetClientSettings.CanShowPolarPlots = _View.InternetClientCanShowPolarPlots;

            configuration.VersionCheckSettings.CheckAutomatically = _View.CheckForNewVersions;
            configuration.VersionCheckSettings.CheckPeriodDays = _View.CheckForNewVersionsPeriodDays;

            configuration.WebServerSettings.AuthenticationScheme = _View.WebServerUserMustAuthenticate ? AuthenticationSchemes.Basic : AuthenticationSchemes.Anonymous;
            configuration.WebServerSettings.BasicAuthenticationUserIds.Clear();
            configuration.WebServerSettings.BasicAuthenticationUserIds.AddRange(GetUniqueIdsForSubsetOfAllUsers(allUsers, _View.WebServerUsers));
            configuration.WebServerSettings.EnableUPnp = _View.EnableUPnpFeatures;
            configuration.WebServerSettings.IsOnlyInternetServerOnLan = _View.IsOnlyVirtualRadarServerOnLan;
            configuration.WebServerSettings.AutoStartUPnP = _View.AutoStartUPnp;
            configuration.WebServerSettings.UPnpPort = _View.UPnpPort;

            configuration.RawDecodingSettings.AcceptableAirborneSpeed = _View.RawDecodingAcceptableAirborneSpeed;
            configuration.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed = _View.RawDecodingAcceptableAirSurfaceTransitionSpeed;
            configuration.RawDecodingSettings.AcceptableSurfaceSpeed = _View.RawDecodingAcceptableSurfaceSpeed;
            configuration.RawDecodingSettings.AirborneGlobalPositionLimit = _View.RawDecodingAirborneGlobalPositionLimit;
            configuration.RawDecodingSettings.FastSurfaceGlobalPositionLimit = _View.RawDecodingFastSurfaceGlobalPositionLimit;
            configuration.RawDecodingSettings.IgnoreCallsignsInBds20 = _View.RawDecodingIgnoreCallsignsInBds20;
            configuration.RawDecodingSettings.IgnoreMilitaryExtendedSquitter = _View.RawDecodingIgnoreMilitaryExtendedSquitter;
            configuration.RawDecodingSettings.ReceiverRange = _View.RawDecodingReceiverRange;
            configuration.RawDecodingSettings.SlowSurfaceGlobalPositionLimit = _View.RawDecodingSlowSurfaceGlobalPositionLimit;
            configuration.RawDecodingSettings.SuppressReceiverRangeCheck = _View.RawDecodingSuppressReceiverRangeCheck;
            configuration.RawDecodingSettings.UseLocalDecodeForInitialPosition = _View.RawDecodingUseLocalDecodeForInitialPosition;
            configuration.RawDecodingSettings.AcceptIcaoInNonPICount = _View.AcceptIcaoInNonPICount;
            configuration.RawDecodingSettings.AcceptIcaoInNonPISeconds = _View.AcceptIcaoInNonPISeconds;
            configuration.RawDecodingSettings.AcceptIcaoInPI0Count = _View.AcceptIcaoInPI0Count;
            configuration.RawDecodingSettings.AcceptIcaoInPI0Seconds = _View.AcceptIcaoInPI0Seconds;
        }

        string[] GetUniqueIdsForSubsetOfAllUsers(IUser[] allUsers, IEnumerable<IUser> subset)
        {
            var result = subset.Where(r => allUsers.Any(i => i.UniqueId == r.UniqueId)).Select(r => r.UniqueId).ToArray();
            return result;
        }
        #endregion

        #region UseIcaoRawDecodingSettings, UseRecommendedRawDecodingSettings
        /// <summary>
        /// Configures the view with the ICAO raw decoding settings.
        /// </summary>
        private void UseIcaoRawDecodingSettings()
        {
            _View.RawDecodingAcceptableAirborneSpeed = 11.112;
            _View.RawDecodingAcceptableAirSurfaceTransitionSpeed = 4.63;
            _View.RawDecodingAcceptableSurfaceSpeed = 1.389;
            _View.RawDecodingAirborneGlobalPositionLimit = 10;
            _View.RawDecodingFastSurfaceGlobalPositionLimit = 25;
            _View.RawDecodingSlowSurfaceGlobalPositionLimit = 50;
            _View.RawDecodingSuppressReceiverRangeCheck = false;
            _View.RawDecodingUseLocalDecodeForInitialPosition = false;
        }

        /// <summary>
        /// Configures the view with the default raw decoding settings.
        /// </summary>
        private void UseRecommendedRawDecodingSettings()
        {
            var defaults = new RawDecodingSettings();

            _View.RawDecodingAcceptableAirborneSpeed = defaults.AcceptableAirborneSpeed;
            _View.RawDecodingAcceptableAirSurfaceTransitionSpeed = defaults.AcceptableAirSurfaceTransitionSpeed;
            _View.RawDecodingAcceptableSurfaceSpeed = defaults.AcceptableSurfaceSpeed;
            _View.RawDecodingAirborneGlobalPositionLimit = defaults.AirborneGlobalPositionLimit;
            _View.RawDecodingFastSurfaceGlobalPositionLimit = defaults.FastSurfaceGlobalPositionLimit;
            _View.RawDecodingSlowSurfaceGlobalPositionLimit = defaults.SlowSurfaceGlobalPositionLimit;
            _View.RawDecodingSuppressReceiverRangeCheck = true;
            _View.RawDecodingUseLocalDecodeForInitialPosition = false;
        }
        #endregion

        #region ValidateForm
        /// <summary>
        /// Validates the content of the form, returning a list of errors and warnings.
        /// </summary>
        /// <returns></returns>
        private List<ValidationResult> ValidateForm()
        {
            List<ValidationResult> result = new List<ValidationResult>();

            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            if(userManager.CanEditUsers) {
                foreach(var user in _View.Users) {
                    userManager.ValidateUser(result, user, user, _View.Users);
                }
            }

            ValidateFileExists(ValidationField.BaseStationDatabase, _View.BaseStationDatabaseFileName, result);
            ValidateFolderExists(ValidationField.FlagsFolder, _View.OperatorFlagsFolder, result);
            ValidateFolderExists(ValidationField.PicturesFolder, _View.PicturesFolder, result);
            ValidateFolderExists(ValidationField.SilhouettesFolder, _View.SilhouettesFolder, result);

            ValidateWithinBounds(ValidationField.ReceiverRange, _View.RawDecodingReceiverRange, 0, 99999, Strings.ReceiverRangeOutOfBounds, result);
            ValidateWithinBounds(ValidationField.AirborneGlobalPositionLimit, _View.RawDecodingAirborneGlobalPositionLimit, 1, 30, Strings.AirborneGlobalPositionLimitOutOfBounds, result);
            ValidateWithinBounds(ValidationField.FastSurfaceGlobalPositionLimit, _View.RawDecodingFastSurfaceGlobalPositionLimit, 1, 75, Strings.FastSurfaceGlobalPositionLimitOutOfBounds, result);
            ValidateWithinBounds(ValidationField.SlowSurfaceGlobalPositionLimit, _View.RawDecodingSlowSurfaceGlobalPositionLimit, 1, 150, Strings.SlowSurfaceGlobalPositionLimitOutOfBounds, result);
            ValidateWithinBounds(ValidationField.AcceptableAirborneLocalPositionSpeed, _View.RawDecodingAcceptableAirborneSpeed, 0.005, 45.0, Strings.AcceptableAirborneSpeedOutOfBounds, result);
            ValidateWithinBounds(ValidationField.AcceptableTransitionLocalPositionSpeed, _View.RawDecodingAcceptableAirSurfaceTransitionSpeed, 0.003, 20.0, Strings.AcceptableAirSurfaceTransitionSpeedOutOfBounds, result);
            ValidateWithinBounds(ValidationField.AcceptableSurfaceLocalPositionSpeed, _View.RawDecodingAcceptableSurfaceSpeed, 0.001, 10.0, Strings.AcceptableSurfaceSpeedOutOfBounds, result);
            ValidateWithinBounds(ValidationField.AcceptIcaoInNonPICount, _View.AcceptIcaoInNonPICount, 0, 100, Strings.AcceptIcaoInNonPICountOutOfBounds, result);
            ValidateWithinBounds(ValidationField.AcceptIcaoInNonPISeconds, _View.AcceptIcaoInNonPISeconds, 1, 30, Strings.AcceptIcaoInNonPISecondsOutOfBounds, result);
            ValidateWithinBounds(ValidationField.AcceptIcaoInPI0Count, _View.AcceptIcaoInPI0Count, 1, 10, Strings.AcceptIcaoInPI0CountOutOfBounds, result);
            ValidateWithinBounds(ValidationField.AcceptIcaoInPI0Seconds, _View.AcceptIcaoInPI0Seconds, 1, 60, Strings.AcceptIcaoInPI0SecondsOutOfBounds, result);

            ValidateWithinBounds(ValidationField.UPnpPortNumber, _View.UPnpPort, 1, 65535, Strings.UPnpPortOutOfBounds, result);
            ValidateWithinBounds(ValidationField.InternetUserIdleTimeout, _View.InternetClientTimeoutMinutes, 0, 1440, Strings.InternetUserIdleTimeoutOutOfBounds, result);

            ValidateWithinBounds(ValidationField.Latitude, _View.InitialGoogleMapLatitude, -90.0, 90.0, Strings.LatitudeOutOfBounds, result);
            ValidateWithinBounds(ValidationField.Longitude, _View.InitialGoogleMapLongitude, -180.0, 180.0, Strings.LongitudeOutOfBounds, result);
            ValidateWithinBounds(ValidationField.GoogleMapZoomLevel, _View.InitialGoogleMapZoom, 0, 19, Strings.GoogleMapZoomOutOfBounds, result);
            ValidateWithinBounds(ValidationField.MinimumGoogleMapRefreshSeconds, _View.MinimumGoogleMapRefreshSeconds, 0, 3600, Strings.MinimumRefreshOutOfBounds, result);
            if(_View.InitialGoogleMapRefreshSeconds > 3600) result.Add(new ValidationResult(ValidationField.InitialGoogleMapRefreshSeconds, Strings.InitialRefreshOutOfBounds));
            if(_View.InitialGoogleMapRefreshSeconds < _View.MinimumGoogleMapRefreshSeconds) result.Add(new ValidationResult(ValidationField.InitialGoogleMapRefreshSeconds, Strings.InitialRefreshLessThanMinimumRefresh));
            if(_View.WebSiteReceiverId == 0) result.Add(new ValidationResult(ValidationField.WebSiteReceiver, Strings.WebSiteReceiverRequired));
            if(_View.ClosestAircraftReceiverId == 0) result.Add(new ValidationResult(ValidationField.ClosestAircraftReceiver, Strings.ClosestAircraftReceiverRequired));
            if(_View.FlightSimulatorXReceiverId == 0) result.Add(new ValidationResult(ValidationField.FlightSimulatorXReceiver, Strings.FlightSimulatorXReceiverRequired));

            ValidateWithinBounds(ValidationField.CheckForNewVersions, _View.CheckForNewVersionsPeriodDays, 1, 365, Strings.DaysBetweenChecksOutOfBounds, result);
            ValidateWithinBounds(ValidationField.DisplayTimeout, _View.DisplayTimeoutSeconds, 5, 540, Strings.DurationBeforeAircraftRemovedFromMapOutOfBounds, result);
            ValidateWithinBounds(ValidationField.ShortTrailLength, _View.ShortTrailLengthSeconds, 1, 1800, Strings.DurationOfShortTrailsOutOfBounds, result);
            ValidateWithinBounds(ValidationField.TextToSpeechSpeed, _View.TextToSpeechSpeed, -10, 10, Strings.ReadingSpeedOutOfBounds, result);
            if(_View.TrackingTimeoutSeconds < _View.DisplayTimeoutSeconds) result.Add(new ValidationResult(ValidationField.TrackingTimeout, Strings.TrackingTimeoutLessThanDisplayTimeout));
            else if(_View.TrackingTimeoutSeconds > 3600) result.Add(new ValidationResult(ValidationField.TrackingTimeout, Strings.TrackingTimeoutOutOfBounds));

            if(_View.Receivers.Count == 0) result.Add(new ValidationResult(ValidationField.ReceiverIds, Strings.PleaseConfigureAtLeastOneReceiver));
            else {
                if(!_View.Receivers.Any(r => r.Enabled)) result.Add(new ValidationResult(ValidationField.ReceiverIds, Strings.PleaseEnableAtLeastOneReceiver));
                foreach(var record in _View.Receivers) {
                    ValidateNotEmpty(ValidationField.Name, record.Name, Strings.NameRequired, result, record);
                    if(record.Name != null && record.Name.Contains(',')) result.Add(new ValidationResult(record, ValidationField.Name, Strings.NameCannotContainComma, false));
                    ValidateIsUnique(ValidationField.Name, Strings.NameMustBeUnique, record, _View.Receivers, r => r.Name, false, result);
                    switch(record.ConnectionType) {
                        case ConnectionType.TCP:
                            if(String.IsNullOrEmpty(record.Address)) result.Add(new ValidationResult(record, ValidationField.BaseStationAddress, Strings.DataSourceNetworkAddressMissing));
                            else {
                                try {
                                    Dns.GetHostAddresses(record.Address);
                                } catch(Exception) {
                                    result.Add(new ValidationResult(record, ValidationField.BaseStationAddress, String.Format(Strings.CannotResolveAddress, record.Address)));
                                }
                            }
                            break;
                        case ConnectionType.COM:
                            if(String.IsNullOrEmpty(record.ComPort)) result.Add(new ValidationResult(record, ValidationField.ComPort, Strings.SerialComPortMissing));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    ValidateWithinBounds(ValidationField.BaseStationPort, record.Port, 1, 65535, Strings.PortOutOfBounds, result, record);
                    switch(record.BaudRate) {
                        case 110:
                        case 300:
                        case 1200:
                        case 2400:
                        case 4800:
                        case 9600:
                        case 19200:
                        case 38400:
                        case 57600:
                        case 115200:
                        case 230400:
                        case 460800:
                        case 921600:
                        case 3000000:
                            break;
                        default:
                            result.Add(new ValidationResult(record, ValidationField.BaudRate, Strings.SerialBaudRateInvalidValue));
                            break;
                    }
                    ValidateWithinBounds(ValidationField.DataBits, record.DataBits, 5, 8, Strings.SerialDataBitsOutOfBounds, result, record);
                }
            }

            foreach(var record in _View.MergedFeeds) {
                ValidateNotEmpty(ValidationField.Name, record.Name, Strings.NameRequired, result, record);
                if(record.Name != null && record.Name.Contains(',')) result.Add(new ValidationResult(record, ValidationField.Name, Strings.NameCannotContainComma, false));
                ValidateIsUnique(ValidationField.Name, Strings.NameMustBeUnique, record, _View.MergedFeeds, r => r.Name, false, result);
                ValidateWithinBounds(ValidationField.IcaoTimeout, record.IcaoTimeout, 1000, 30000, Strings.IcaoTimeoutOutOfBounds, result, record);
                if(record.ReceiverIds.Count < 2) result.Add(new ValidationResult(record, ValidationField.ReceiverIds, Strings.MergedFeedNeedsAtLeastTwoReceivers, false));
                if(_View.Receivers.Any(r => String.Equals(r.Name, record.Name, StringComparison.CurrentCultureIgnoreCase))) {
                    result.Add(new ValidationResult(record, ValidationField.Name, Strings.ReceiversAndMergedFeedNamesMustBeUnique, false));
                }
            }

            foreach(var record in _View.RawDecodingReceiverLocations) {
                ValidateNotEmpty(ValidationField.Location, record.Name, Strings.PleaseEnterNameForLocation, result, record);
                ValidateIsUnique(ValidationField.Location, Strings.PleaseEnterUniqueNameForLocation, record, _View.RawDecodingReceiverLocations, r => r.Name, false, result);
                ValidateWithinBounds(ValidationField.Latitude, record.Latitude, -90.0, 90.0, Strings.LatitudeOutOfBounds, result, record);
                ValidateWithinBounds(ValidationField.Longitude, record.Longitude, -180.0, 180.0, Strings.LongitudeOutOfBounds, result, record);
                if(record.Latitude == 0.0) result.Add(new ValidationResult(record, ValidationField.Latitude, Strings.LatitudeCannotBeZero));
            }

            foreach(var record in _View.RebroadcastSettings) {
                ValidateNotEmpty(ValidationField.Name, record.Name, Strings.NameRequired, result, record);
                ValidateIsUnique(ValidationField.Name, Strings.NameMustBeUnique, record, _View.RebroadcastSettings, r => r.Name, false, result);
                ValidateWithinBounds(ValidationField.RebroadcastServerPort, record.Port, 1, 65535, Strings.PortOutOfBounds, result, record);
                ValidateIsUnique(ValidationField.RebroadcastServerPort, Strings.PortMustBeUnique, record, _View.RebroadcastSettings, r => r.Port, false, result);
                if(record.Format == RebroadcastFormat.None) result.Add(new ValidationResult(record, ValidationField.Format, Strings.RebroadcastFormatRequired));
                if(record.ReceiverId == 0) result.Add(new ValidationResult(record, ValidationField.RebroadcastReceiver, Strings.ReceiverRequired));
                if(record.StaleSeconds <= 0) result.Add(new ValidationResult(record, ValidationField.StaleSeconds, Strings.StaleSecondsOutOfBounds));
            }

            return result;
        }

        private void ValidateNotEmpty(ValidationField field, string value, string message, List<ValidationResult> results, object record = null)
        {
            if(String.IsNullOrEmpty(value)) results.Add(new ValidationResult(record, field, message));
        }

        private void ValidateIsUnique<TRecord, TField>(ValidationField field, string message, TRecord record, IEnumerable<TRecord> records, Func<TRecord, TField> getFieldValue, bool caseSensitive, List<ValidationResult> results)
            where TRecord: class
        {
            foreach(var otherRecord in records.Where(r => !Object.ReferenceEquals(r, record))) {
                var lhsValue = getFieldValue(record);
                var rhsValue = getFieldValue(otherRecord);
                var areEqual = typeof(TField) == typeof(String) ? String.Equals(lhsValue as string, rhsValue as string, caseSensitive ? StringComparison.CurrentCulture : StringComparison.CurrentCultureIgnoreCase)
                                                                : Object.Equals(lhsValue, rhsValue);
                if(areEqual) {
                    results.Add(new ValidationResult(record, field, message));
                }
            }
        }

        private void ValidateWithinBounds(ValidationField field, int value, int lowerInclusive, int upperInclusive, string message, List<ValidationResult> results, object record = null)
        {
            if(value < lowerInclusive || value > upperInclusive) results.Add(new ValidationResult(record, field, message));
        }

        private void ValidateWithinBounds(ValidationField field, double value, double lowerInclusive, double upperInclusive, string message, List<ValidationResult> results, object record = null)
        {
            if(value < lowerInclusive || value > upperInclusive) results.Add(new ValidationResult(record, field, message));
        }

        private void ValidateFileExists(ValidationField field, string fileName, List<ValidationResult> results, object record = null)
        {
            ValidateFileSystemEntityExists(field, fileName, results, true, record);
        }

        private void ValidateFolderExists(ValidationField field, string folder, List<ValidationResult> results, object record = null)
        {
            ValidateFileSystemEntityExists(field, folder, results, false, record);
        }

        private void ValidateFileSystemEntityExists(ValidationField field, string entityName, List<ValidationResult> results, bool isFile, object record)
        {
            ValidationResult result = null;

            if(!String.IsNullOrEmpty(entityName)) {
                var cachedResult = _CachedFileSystemResults.Where(r => r.ValidationRecord == record && r.ValidationField == field).FirstOrDefault();
                if(cachedResult != null && entityName == cachedResult.FileSystemEntityName) result = cachedResult.ValidationResult;
                else {
                    if(cachedResult != null) _CachedFileSystemResults.Remove(cachedResult);
                    cachedResult = new CachedFileSystemResult() { ValidationRecord = record, ValidationField = field, FileSystemEntityName = entityName };

                    bool entityExists = isFile ? Provider.FileExists(entityName) : Provider.FolderExists(entityName);
                    if(!entityExists) cachedResult.ValidationResult = result = new ValidationResult(record, field, String.Format(Strings.SomethingDoesNotExist, entityName), true);

                    _CachedFileSystemResults.Add(cachedResult);
                }

                if(result != null) results.Add(result);
            }
        }
        #endregion

        #region UpdateReceiverLocationsFromBaseStationDatabase
        /// <summary>
        /// Updates the receiver locations from the BaseStation database.
        /// </summary>
        private void UpdateReceiverLocationsFromBaseStationDatabase()
        {
            var database = Factory.Singleton.Resolve<IAutoConfigBaseStationDatabase>().Singleton.Database;
            _View.MergeBaseStationDatabaseReceiverLocations(database.GetLocations().Select(r => new ReceiverLocation() {
                Name = r.LocationName,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                IsBaseStationLocation = true,
            }));
        }
        #endregion

        #region SaveUsers
        /// <summary>
        /// Saves changes to the users to the database.
        /// </summary>
        private void SaveUsers()
        {
            var userManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            var existing = userManager.GetUsers();

            // Update existing records
            foreach(var record in _View.Users) {
                var current = existing.FirstOrDefault(r => r.UniqueId == record.UniqueId);
                if(current != null) {
                    if(!userManager.CanEditUsers) {
                        record.Name = current.Name;
                        record.LoginName = current.LoginName;
                    }
                    if(!userManager.CanChangeEnabledState) record.Enabled = current.Enabled;
                    if(!userManager.CanChangePassword) record.UIPassword = _DefaultPassword;

                    if(record.Enabled !=    current.Enabled ||
                       record.LoginName !=  current.LoginName ||
                       record.Name !=       current.Name ||
                       record.UIPassword != _DefaultPassword
                    ) {
                        userManager.UpdateUser(record, record.UIPassword == _DefaultPassword ? null : record.UIPassword);
                    }
                }
            }

            // Insert new records
            if(userManager.CanCreateUsers) {
                foreach(var record in _View.Users.Where(r => !r.IsPersisted)) {
                    if(record.UIPassword == _DefaultPassword) record.UIPassword = null;
                    userManager.CreateUser(record);
                    record.UIPassword = _DefaultPassword;
                }
            }

            // Delete missing records
            if(userManager.CanDeleteUsers) {
                foreach(var missing in existing.Where(r => !_View.Users.Any(i => i.IsPersisted && i.UniqueId == r.UniqueId))) {
                    userManager.DeleteUser(missing);
                }
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Raised when the user wants to reset the view to default values.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_ResetToDefaultsClicked(object sender, EventArgs args)
        {
            var configuration = new Configuration();
            configuration.GoogleMapSettings.WebSiteReceiverId = _View.WebSiteReceiverId;
            configuration.GoogleMapSettings.ClosestAircraftReceiverId = _View.ClosestAircraftReceiverId;
            configuration.GoogleMapSettings.FlightSimulatorXReceiverId = _View.FlightSimulatorXReceiverId;

            CopyConfigurationToUI(configuration);
        }

        /// <summary>
        /// Raised when the user elects to save their changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_SaveClicked(object sender, EventArgs args)
        {
            var validationResults = ValidateForm();
            _View.ShowValidationResults(validationResults);

            if(validationResults.Where(r => r.IsWarning == false).Count() == 0) {
                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                var configuration = configurationStorage.Load();

                SaveUsers();
                CopyUIToConfiguration(configuration);

                configurationStorage.Save(configuration);
            }
        }

        /// <summary>
        /// Raised when the user indicates that they want to test the connection settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_TestConnectionClicked(object sender, EventArgs<Receiver> args)
        {
            var receiver = args.Value;

            var previousBusyState = _View.ShowBusy(true, null);
            Exception exception = null;
            try {
                switch(receiver.ConnectionType) {
                    case ConnectionType.TCP:
                        exception = Provider.TestNetworkConnection(receiver.Address, receiver.Port);
                        break;
                    case ConnectionType.COM:
                        exception = Provider.TestSerialConnection(receiver.ComPort, receiver.BaudRate, receiver.DataBits, receiver.StopBits, receiver.Parity, receiver.Handshake);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            } finally {
                _View.ShowBusy(false, previousBusyState);
            }

            if(exception == null)   _View.ShowTestConnectionResults(Strings.CanConnectWithSettings, Strings.ConnectedSuccessfully);
            else                    _View.ShowTestConnectionResults(String.Format("{0} {1}", Strings.CannotConnectWithSettings, exception.Message), Strings.CannotConnect);
        }

        /// <summary>
        /// Raised when the user requests a test of the text-to-speech settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_TestTextToSpeechSettingsClicked(object sender, EventArgs args)
        {
            Provider.TestTextToSpeech(_View.TextToSpeechVoice, _View.TextToSpeechSpeed);
        }

        /// <summary>
        /// Raised when the user wants to import receiver locations from the BaseStation database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_UpdateReceiverLocationsFromBaseStationDatabaseClicked(object sender, EventArgs args)
        {
            UpdateReceiverLocationsFromBaseStationDatabase();
        }

        /// <summary>
        /// Raised when the user wants to use the ICAO recommended raw decoder settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_UseIcaoRawDecodingSettingsClicked(object sender, EventArgs args)
        {
            UseIcaoRawDecodingSettings();
        }

        /// <summary>
        /// Raised when the user wants to use the default raw decoder settings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_UseRecommendedRawDecodingSettingsClicked(object sender, EventArgs args)
        {
            UseRecommendedRawDecodingSettings();
        }

        /// <summary>
        /// Raised after control values may have been changed by the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_ValueChanged(object sender, EventArgs args)
        {
            _View.ShowValidationResults(ValidateForm());
        }
        #endregion
    }
}
