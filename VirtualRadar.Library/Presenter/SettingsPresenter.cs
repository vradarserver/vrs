// Copyright © 2014 onwards, Andrew Whewell
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
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using System.ComponentModel;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="ISettingsPresenter"/>.
    /// </summary>
    class SettingsPresenter : Presenter<ISettingsView>, ISettingsPresenter
    {
        #region Private class - DefaultProvider
        /// <summary>
        /// The default implementation of the provider.
        /// </summary>
        class DefaultProvider : ISettingsPresenterProvider
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

                try {
                    result.Add(null);
                    _DefaultVoiceName = _SpeechSynthesizer.DefaultVoiceName;

                    foreach(var installedVoiceName in _SpeechSynthesizer.GetInstalledVoiceNames()) {
                        result.Add(installedVoiceName);
                    }
                } catch {
                    ;
                }

                return result;
            }

            public IEnumerable<string> GetSerialPortNames()
            {
                string[] result = null;
                try {
                    result = SerialPort.GetPortNames();
                } catch {
                    ;
                }

                return result ?? new string[0];
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

        #region Fields
        /// <summary>
        /// True if validation on changes to values on the view is disabled.
        /// </summary>
        private bool _ValueChangedValidationDisabled;

        /// <summary>
        /// The object that will observe the configuration for changes.
        /// </summary>
        private IConfigurationListener _ConfigurationListener;

        /// <summary>
        /// The highest unique ID across both receivers and merged feed as-at the point that
        /// editing started. We do not create a unique ID that is at or below this value when
        /// we create new receivers and merged feeds.
        /// </summary>
        private int _InitialMaxCombinedFeedId;

        /// <summary>
        /// The user manager that the presenter is deferring all user handling to.
        /// </summary>
        private IUserManager _UserManager;

        /// <summary>
        /// The password that we use to indicate that a password has not been changed by the user.
        /// Needs to be something that is unlikely or impossible to have been entered via the UI.
        /// </summary>
        private string _DefaultPassword = "A\t\t\t\t\tA";
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ISettingsPresenterProvider Provider { get; set; }
        #endregion

        #region Ctors and Finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SettingsPresenter()
        {
            Provider = new DefaultProvider();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~SettingsPresenter()
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
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                if(Provider != null) Provider.Dispose();
                if(_ConfigurationListener != null) _ConfigurationListener.Dispose();
            }
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public override void Initialise(ISettingsView view)
        {
            base.Initialise(view);

            _View.FlightSimulatorXOnlyClicked += View_FlightSimulatorXOnlyClicked;
            _View.SaveClicked += View_SaveClicked;
            _View.TestConnectionClicked += View_TestConnectionClicked;
            _View.TestTextToSpeechSettingsClicked += View_TestTextToSpeechSettingsClicked;
            _View.UpdateReceiverLocationsFromBaseStationDatabaseClicked += View_UpdateReceiverLocationsFromBaseStationDatabaseClicked;
            _View.UseIcaoRawDecodingSettingsClicked += View_UseIcaoRawDecodingSettingsClicked;
            _View.UseRecommendedRawDecodingSettingsClicked += View_UseRecommendedRawDecodingSettingsClicked;

            var configStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
            var config = configStorage.Load();

            _UserManager = Factory.Singleton.Resolve<IUserManager>().Singleton;
            _View.UserManager = _UserManager.Name;

            var highestReceiver = config.Receivers.OrderBy(r => r.UniqueId).LastOrDefault();
            var highestMergedFeed = config.MergedFeeds.OrderBy(r => r.UniqueId).LastOrDefault();
            _InitialMaxCombinedFeedId = Math.Max(highestReceiver == null ? 0 : highestReceiver.UniqueId, highestMergedFeed == null ? 0 : highestMergedFeed.UniqueId);

            _ConfigurationListener = Factory.Singleton.Resolve<IConfigurationListener>();
            _ConfigurationListener.PropertyChanged += ConfigurationListener_PropertyChanged;
            _ConfigurationListener.Initialise(config);

            _View.Users.ListChanged += Users_ListChanged;

            CopyConfigurationToView(config);
        }

        /// <summary>
        /// Copies the configuration to the view.
        /// </summary>
        /// <param name="config"></param>
        private void CopyConfigurationToView(Configuration config)
        {
            var disableValidation = _ValueChangedValidationDisabled;
            _ValueChangedValidationDisabled = true;
            try {
                _View.Configuration = config;

                var allUsers = _UserManager.GetUsers().Select(r => { r.UIPassword = _DefaultPassword; return r; }).ToArray();
                _View.Users.Clear();
                _View.Users.AddRange(allUsers);
            } finally {
                _ValueChangedValidationDisabled = disableValidation;
            }
        }
        #endregion

        #region Source Helpers - GetSerialPortNames, GetVoiceNames
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSerialPortNames()
        {
            return Provider.GetSerialPortNames();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetVoiceNames()
        {
            return Provider.GetVoiceNames();
        }
        #endregion

        #region Child object creation - CreateReceiver, CreateReceiverLocation, CreateMergedFeed, CreateRebroadcastServer, CreateUser
        /// <summary>
        /// See interface docs.
        /// </summary>
        public Receiver CreateReceiver()
        {
            var result = new Receiver() {
                UniqueId = NextCombinedFeedUniqueId(),
                Name = NextCombinedFeedName("Receiver"),
                Enabled = true,
            };

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public ReceiverLocation CreateReceiverLocation()
        {
            return CreateReceiverLocation("Location");
        }

        private ReceiverLocation CreateReceiverLocation(string prefix)
        {
            var result = new ReceiverLocation() {
                UniqueId = NextUniqueId(_View.Configuration.ReceiverLocations.Select(r => r.UniqueId)),
                Name = NextName(_View.Configuration.ReceiverLocations.Select(r => r.Name), prefix),
                IsBaseStationLocation = false,
                Latitude = 0,
                Longitude = 0,
            };

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public MergedFeed CreateMergedFeed()
        {
            var result = new MergedFeed() {
                UniqueId = NextCombinedFeedUniqueId(),
                Name = NextCombinedFeedName("Merged Feed"),
                Enabled = true,
            };

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public RebroadcastSettings CreateRebroadcastServer()
        {
            var result = new RebroadcastSettings() {
                UniqueId = NextUniqueId(_View.Configuration.RebroadcastSettings.Select(r => r.UniqueId)),
                Name = NextName(_View.Configuration.RebroadcastSettings.Select(r => r.Name), "Rebroadcast Server"),
                Port = NextPort(_View.Configuration.RebroadcastSettings.Select(r => r.Port), 33001),
                Format = RebroadcastFormat.Port30003,
                Enabled = true,
            };

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IUser CreateUser()
        {
            var result = Factory.Singleton.Resolve<IUser>();
            result.Enabled = true;
            result.LoginName = "";
            result.Name = "";

            return result;
        }

        private int NextUniqueId(IEnumerable<int> existing)
        {
            int result = 1;

            if(existing.Count() > 0) {
                result = existing.Max(r => r) + 1;
            }

            return result;
        }

        private string NextName(IEnumerable<string> existingNames, string prefix)
        {
            var result = prefix;

            var counter = 2;
            while(existingNames.Any(r => result.Equals(r, StringComparison.CurrentCultureIgnoreCase))) {
                result = String.Format("{0} {1}", prefix, counter++);
            }

            return result;
        }

        private int NextCombinedFeedUniqueId()
        {
            var highestReceiver = _View.Configuration.Receivers.OrderBy(r => r.UniqueId).LastOrDefault();
            var highestMergedFeed = _View.Configuration.MergedFeeds.OrderBy(r => r.UniqueId).LastOrDefault();
            var highestCombinedFeedId = Math.Max(highestReceiver == null ? 0 : highestReceiver.UniqueId, highestMergedFeed == null ? 0 : highestMergedFeed.UniqueId);
            var startPoint = Math.Max(_InitialMaxCombinedFeedId, highestCombinedFeedId);

            return startPoint + 1;
        }

        private string NextCombinedFeedName(string prefix)
        {
            var existingNames = _View.Configuration.Receivers.Select(r => r.Name).Concat(_View.Configuration.MergedFeeds.Select(r => r.Name)).ToArray();

            var result = prefix;
            var counter = 2;
            while(existingNames.Any(r => result.Equals(r, StringComparison.CurrentCultureIgnoreCase))) {
                result = String.Format("{0} {1}", prefix, counter++);
            }

            return result;
        }

        private int NextPort(IEnumerable<int> existingPorts, int firstPort)
        {
            int result = firstPort;

            while(existingPorts.Contains(result)) {
                ++result;
            }

            return result;
        }
        #endregion

        #region UpdateReceiverLocationsFromBaseStationDatabase
        /// <summary>
        /// Updates the receiver locations from the BaseStation database.
        /// </summary>
        private void UpdateReceiverLocationsFromBaseStationDatabase()
        {
            var database = Factory.Singleton.Resolve<IAutoConfigBaseStationDatabase>().Singleton.Database;
            var databaseLocations = database.GetLocations().Select(r => new ReceiverLocation() {
                Name = r.LocationName,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                IsBaseStationLocation = true,
            });

            var viewList = _View.Configuration.ReceiverLocations;

            var updateList = viewList.Where(r => r.IsBaseStationLocation && databaseLocations.Any(i => i.Name == r.Name)).ToArray();
            var deleteList = viewList.Where(r => r.IsBaseStationLocation).Except(updateList).ToArray();
            var insertList = databaseLocations.Where(r => !updateList.Any(i => i.Name == r.Name)).ToArray();

            foreach(var location in updateList) {
                var refreshLocation = databaseLocations.First(r => r.Name == location.Name);
                location.Latitude = refreshLocation.Latitude;
                location.Longitude = refreshLocation.Longitude;
            }

            foreach(var deleteLocation in deleteList) {
                viewList.Remove(deleteLocation);
            }

            foreach(var location in insertList) {
                var newLocation = CreateReceiverLocation(location.Name);
                newLocation.Latitude = location.Latitude;
                newLocation.Longitude = location.Longitude;
                newLocation.IsBaseStationLocation = true;

                viewList.Add(newLocation);
            }
        }
        #endregion

        #region UseIcaoRawDecodingSettings, UseRecommendedRawDecodingSettings, UseFlightSimulatorXOnlySettings
        /// <summary>
        /// Configures the view with the ICAO raw decoding settings.
        /// </summary>
        private void UseIcaoRawDecodingSettings()
        {
            var settings = _View.Configuration.RawDecodingSettings;
            settings.AcceptableAirborneSpeed = 11.112;
            settings.AcceptableAirSurfaceTransitionSpeed = 4.63;
            settings.AcceptableSurfaceSpeed = 1.389;
            settings.AirborneGlobalPositionLimit = 10;
            settings.FastSurfaceGlobalPositionLimit = 25;
            settings.SlowSurfaceGlobalPositionLimit = 50;
            settings.SuppressReceiverRangeCheck = false;
            settings.UseLocalDecodeForInitialPosition = false;
        }

        /// <summary>
        /// Configures the view with the default raw decoding settings.
        /// </summary>
        private void UseRecommendedRawDecodingSettings()
        {
            var settings = _View.Configuration.RawDecodingSettings;
            var defaults = new RawDecodingSettings();
            settings.AcceptableAirborneSpeed = defaults.AcceptableAirborneSpeed;
            settings.AcceptableAirSurfaceTransitionSpeed = defaults.AcceptableAirSurfaceTransitionSpeed;
            settings.AcceptableSurfaceSpeed = defaults.AcceptableSurfaceSpeed;
            settings.AirborneGlobalPositionLimit = defaults.AirborneGlobalPositionLimit;
            settings.FastSurfaceGlobalPositionLimit = defaults.FastSurfaceGlobalPositionLimit;
            settings.SlowSurfaceGlobalPositionLimit = defaults.SlowSurfaceGlobalPositionLimit;
            settings.SuppressReceiverRangeCheck = true;
            settings.UseLocalDecodeForInitialPosition = false;
        }

        /// <summary>
        /// Configures the view with reasonable settings for use with FSX.
        /// </summary>
        private void UseFlightSimulatorXOnlySettings()
        {
            var settings = _View.Configuration;

            // Data source settings
            settings.BaseStationSettings.DatabaseFileName= "";
            settings.BaseStationSettings.SilhouettesFolder = "";
            settings.BaseStationSettings.OperatorFlagsFolder = "";
            settings.BaseStationSettings.PicturesFolder = "";
            settings.BaseStationSettings.SearchPictureSubFolders = false;

            // Receivers - ideally we'd want none but validation prevents that, and with good
            // reason. This should fail to connect to anything and if they do happen to have
            // a radio and it connects the format should cause it to fail to decode.
            var dummyReceiver = new Receiver() {
                UniqueId = 1,
                Name = "Dummy Receiver",
                Enabled = true,
                ConnectionType = ConnectionType.TCP,
                Address = "127.0.0.1",
                Port = 30003,
                DataSource = DataSource.Sbs3,
                AutoReconnectAtStartup = false,
            };
            settings.Receivers.Clear();
            settings.Receivers.Add(dummyReceiver);
            settings.GoogleMapSettings.WebSiteReceiverId = 1;
            settings.GoogleMapSettings.ClosestAircraftReceiverId = 1;
            settings.GoogleMapSettings.FlightSimulatorXReceiverId = 1;

            // Other lists that are meaningless for FSX-only operations
            settings.ReceiverLocations.Clear();
            settings.MergedFeeds.Clear();
            settings.RebroadcastSettings.Clear();

            // General settings
            settings.FlightRouteSettings.AutoUpdateEnabled = false;
        }
        #endregion

        #region Wizards - ApplyReceiverConfigurationWizard
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="receiver"></param>
        public void ApplyReceiverConfigurationWizard(IReceiverConfigurationWizardAnswers answers, Receiver receiver)
        {
            switch(answers.ReceiverClass) {
                case ReceiverClass.SoftwareDefinedRadio:
                    receiver.ConnectionType = ConnectionType.TCP;
                    receiver.DataSource = DataSource.Beast;
                    receiver.Address = answers.IsLoopback ? "127.0.0.1" : answers.NetworkAddress;
                    switch(answers.SdrDecoder) {
                        case SdrDecoder.AdsbSharp:  receiver.Port = 47806; break;
                        case SdrDecoder.Dump1090:   receiver.Port = 30002; break;
                        case SdrDecoder.GrAirModes: receiver.Port = 30003; receiver.DataSource = DataSource.Port30003; break; // The source for gr-air-modes mentions a raw server but I couldn't see it being initialised? No idea what the format would be either.
                        case SdrDecoder.Modesdeco:  receiver.Port = 30005; break;
                        case SdrDecoder.Rtl1090:    receiver.Port = 31001; break;
                        case SdrDecoder.Other:      receiver.Port = 30003; receiver.DataSource = DataSource.Port30003; break; // Most things support vanilla BaseStation
                        default:                    throw new NotImplementedException();
                    }
                    break;
                case ReceiverClass.DedicatedHardware:
                    switch(answers.DedicatedReceiver) {
                        case DedicatedReceiver.Beast:
                            receiver.DataSource = DataSource.Beast;
                            receiver.ConnectionType = answers.ConnectionType;
                            if(receiver.ConnectionType == ConnectionType.TCP) {
                                receiver.Address = answers.NetworkAddress;
                                receiver.Port = 30005;
                            }
                            break;
                        case DedicatedReceiver.KineticAvionicsAll:
                            receiver.DataSource = DataSource.Sbs3;
                            receiver.ConnectionType = ConnectionType.TCP;
                            receiver.Address = !answers.IsUsingBaseStation ? answers.NetworkAddress : answers.IsLoopback ? "127.0.0.1" : answers.NetworkAddress;
                            receiver.Port = answers.IsUsingBaseStation ? 30006 : 10001;
                            break;
                        case DedicatedReceiver.MicroAdsb:
                            receiver.ConnectionType = ConnectionType.COM;
                            receiver.DataSource = DataSource.Beast;
                            break;
                        case DedicatedReceiver.RadarBox:
                        case DedicatedReceiver.Other:
                            receiver.ConnectionType = ConnectionType.TCP;
                            receiver.DataSource = DataSource.Port30003;
                            receiver.Address = answers.NetworkAddress;
                            receiver.Port = 30003;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            if(receiver.ConnectionType == ConnectionType.COM) {
                if(receiver.BaudRate < 921600) receiver.BaudRate = 921600;
                receiver.DataBits = 8;
                receiver.StopBits = StopBits.One;
                receiver.Parity = Parity.None;
                receiver.Handshake = Handshake.None;
                receiver.StartupText = "#43-02\\r";
                receiver.ShutdownText = "#43-00\\r";
            }
        }
        #endregion

        #region Validation
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void ValidateView()
        {
            var validationResults = Validate();
            _View.ShowValidationResults(validationResults);
        }

        /// <summary>
        /// Validates the form and returns the results, optionally constraining validation to a single record and field.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="valueChangedField"></param>
        /// <returns></returns>
        private ValidationResults Validate(object record = null, ValidationField valueChangedField = ValidationField.None)
        {
            var result = new ValidationResults(isPartialValidation: valueChangedField != ValidationField.None);
            var defaults = new Validation(ValidationField.None, result, record, valueChangedField);

            ValidateAudioSettings(defaults);
            ValidateBaseStationSettings(defaults);
            ValidateGoogleMapSettings(defaults);
            ValidateInternetClient(defaults);
            ValidateMergedFeeds(defaults);
            ValidateRawFeedDecoding(defaults);
            ValidateRebroadcastServers(defaults);
            ValidateReceivers(defaults);
            ValidateReceiverLocations(defaults);
            ValidateUsers(defaults);
            ValidateVersionCheckSettings(defaults);
            ValidateWebServer(defaults);

            return result;
        }

        /// <summary>
        /// Validates a single field and reports the results to the view.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="field"></param>
        private void ValidateAndReportSingleField(object record, ValidationField field)
        {
            if(field != ValidationField.None) {
                var results = Validate(record, field);
                _View.ShowValidationResults(results);
            }
        }

        /// <summary>
        /// Returns an array of the combined unique IDs from receivers and merged feeds.
        /// </summary>
        /// <param name="exceptCurrent"></param>
        /// <returns></returns>
        private int[] CombinedFeedUniqueIds(object exceptCurrent)
        {
            var receiverIds = _View.Configuration.Receivers.Where(r => r != exceptCurrent).Select(r => r.UniqueId);
            var mergedFeedIds = _View.Configuration.MergedFeeds.Where(r => r != exceptCurrent).Select(r => r.UniqueId);

            return receiverIds.Concat(mergedFeedIds).ToArray();
        }

        /// <summary>
        /// Returns an array of the combined names from receivers and merged feeds.
        /// </summary>
        /// <param name="exceptCurrent"></param>
        /// <returns></returns>
        private string[] CombinedFeedNamesUpperCase(object exceptCurrent)
        {
            var receiverNames = _View.Configuration.Receivers.Where(r => r != exceptCurrent).Select(r => (r.Name ?? "").ToUpper());
            var mergedFeedNames = _View.Configuration.MergedFeeds.Where(r => r != exceptCurrent).Select(r => (r.Name ?? "").ToUpper());

            return receiverNames.Concat(mergedFeedNames).ToArray();
        }

        /// <summary>
        /// Returns an array of all ports on which the server is listening.
        /// </summary>
        /// <param name="exceptCurrent"></param>
        /// <returns></returns>
        private int[] ListeningPorts(object exceptCurrent)
        {
            var result = _View.Configuration.RebroadcastSettings
                                            .Where(r => r != exceptCurrent && !r.IsTransmitter)
                                            .Select(r => r.Port);

            return result.ToArray();
        }

        #region Audio
        /// <summary>
        /// Validates the audio settings.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateAudioSettings(Validation defaults)
        {
            if(defaults.Record == null) {
                var settings = _View.Configuration.AudioSettings;

                // Playback speed is within range
                ValueIsInRange(settings.VoiceRate, -10, 10, new Validation(ValidationField.TextToSpeechSpeed, defaults) {
                    Message = Strings.ReadingSpeedOutOfBounds,
                });
            }
        }
        #endregion

        #region BaseStation settings
        /// <summary>
        /// Validates the BaseStation settings object.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateBaseStationSettings(Validation defaults)
        {
            if(defaults.Record == null) {
                var settings = _View.Configuration.BaseStationSettings;

                // Files and folders
                FileExists(settings.DatabaseFileName, new Validation(ValidationField.BaseStationDatabase, defaults) {
                    IsWarning = true,
                });

                FolderExists(settings.OperatorFlagsFolder, new Validation(ValidationField.FlagsFolder, defaults) {
                    IsWarning = true,
                });

                FolderExists(settings.SilhouettesFolder, new Validation(ValidationField.SilhouettesFolder, defaults) {
                    IsWarning = true,
                });

                FolderExists(settings.PicturesFolder, new Validation(ValidationField.PicturesFolder, defaults) {
                    IsWarning = true,
                });

                // Tracking timeout is within range
                ValueIsInRange(settings.TrackingTimeoutSeconds, 1, 3600, new Validation(ValidationField.TrackingTimeout, defaults) {
                    Message = Strings.TrackingTimeoutOutOfBounds,
                });

                // Display timeout is within range
                ValueIsInRange(settings.DisplayTimeoutSeconds, 1, settings.TrackingTimeoutSeconds, new Validation(ValidationField.DisplayTimeout, defaults) {
                    Message = Strings.TrackingTimeoutLessThanDisplayTimeout,
                    RelatedFields = { ValidationField.TrackingTimeout },
                });
            }
        }
        #endregion

        #region GoogleMapSettings
        /// <summary>
        /// Validates the Google Map settings.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateGoogleMapSettings(Validation defaults)
        {
            if(defaults.Record == null) {
                var settings = _View.Configuration.GoogleMapSettings;
                var allReceiverIds = _View.Configuration.Receivers.Select(r => r.UniqueId).Concat(_View.Configuration.MergedFeeds.Select(r => r.UniqueId)).ToArray();

                // Closest aircraft widget receiver has been filled in and is a valid ID
                if(ValueNotEqual(settings.ClosestAircraftReceiverId, 0, new Validation(ValidationField.ClosestAircraftReceiver, defaults) {
                    Message = Strings.ClosestAircraftReceiverRequired,
                })) {
                    ValueIsInList(settings.ClosestAircraftReceiverId, allReceiverIds, new Validation(ValidationField.ClosestAircraftReceiver, defaults) {
                        Message = Strings.ReceiverOrMergedFeedDoesNotExist,
                    });
                }

                // FSX receiver has been filled in and is a valid ID
                if(ValueNotEqual(settings.FlightSimulatorXReceiverId, 0, new Validation(ValidationField.FlightSimulatorXReceiver, defaults) {
                    Message = Strings.FlightSimulatorXReceiverRequired,
                })) {
                    ValueIsInList(settings.FlightSimulatorXReceiverId, allReceiverIds, new Validation(ValidationField.FlightSimulatorXReceiver, defaults) {
                        Message = Strings.ReceiverOrMergedFeedDoesNotExist,
                    });
                }

                // Default website receiver has been filled in and is a valid ID
                if(ValueNotEqual(settings.WebSiteReceiverId, 0, new Validation(ValidationField.WebSiteReceiver, defaults) {
                    Message = Strings.WebSiteReceiverRequired,
                })) {
                    ValueIsInList(settings.WebSiteReceiverId, allReceiverIds, new Validation(ValidationField.WebSiteReceiver, defaults) {
                        Message = Strings.ReceiverOrMergedFeedDoesNotExist,
                    });
                }

                // Minimum refresh period is within range
                ValueIsInRange(settings.MinimumRefreshSeconds, 0, 3600, new Validation(ValidationField.MinimumGoogleMapRefreshSeconds, defaults) {
                    Message = Strings.MinimumRefreshOutOfBounds,
                });

                // Initial aircraft list refresh period is within range
                ValueIsInRange(settings.InitialRefreshSeconds, settings.MinimumRefreshSeconds, 3600, new Validation(ValidationField.InitialGoogleMapRefreshSeconds, defaults) {
                    Message = Strings.InitialRefreshLessThanMinimumRefresh,
                    RelatedFields = { ValidationField.MinimumGoogleMapRefreshSeconds },
                });

                // Initial zoom level is within range
                ValueIsInRange(settings.InitialMapZoom, 0, 19, new Validation(ValidationField.GoogleMapZoomLevel, defaults) {
                    Message = Strings.GoogleMapZoomOutOfBounds,
                });

                // Latitude is in range
                ValueIsInRange(settings.InitialMapLatitude, -90.0, 90.0, new Validation(ValidationField.Latitude, defaults) {
                    Message = Strings.LatitudeOutOfBounds,
                });

                // Longitude is in range
                ValueIsInRange(settings.InitialMapLongitude, -180.0, 180.0, new Validation(ValidationField.Longitude, defaults) {
                    Message = Strings.LongitudeOutOfBounds,
                });

                // Short trail length is in range
                ValueIsInRange(settings.ShortTrailLengthSeconds, 1, 1800, new Validation(ValidationField.ShortTrailLength, defaults) {
                    Message = Strings.DurationOfShortTrailsOutOfBounds,
                });
            }
        }
        #endregion

        #region InternetClientSettings
        /// <summary>
        /// Validates the Internet Client settings.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateInternetClient(Validation defaults)
        {
            if(defaults.Record == null) {
                var settings = _View.Configuration.InternetClientSettings;

                // Internet client timeout is within range
                ValueIsInRange(settings.TimeoutMinutes, 0, 1440, new Validation(ValidationField.InternetUserIdleTimeout, defaults) {
                    Message = Strings.InternetUserIdleTimeoutOutOfBounds,
                });
            }
        }
        #endregion

        #region MergedFeeds
        /// <summary>
        /// Validates the list of merged feeds and/or an individual merged feed.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateMergedFeeds(Validation defaults)
        {
            var mergedFeed = defaults.Record as MergedFeed;

            if(defaults.Record == null) {
                // List of merged feeds. There's no validation on here, just on the child records
                if(defaults.ValueChangedField == ValidationField.None) {
                    foreach(var child in _View.Configuration.MergedFeeds) {
                        var childDefaults = new Validation(defaults) { Record = child, };
                        ValidateMergedFeeds(childDefaults);
                    }
                }
            } else if(mergedFeed != null) {
                // There has to be a name
                if(StringIsNotEmpty(mergedFeed.Name, new Validation(ValidationField.Name, defaults) {
                    Message = Strings.NameRequired,
                })) {
                    // The name cannot be the same as any other receiver or merged feed name
                    var receiverAndMergedFeedNames = CombinedFeedNamesUpperCase(mergedFeed);
                    ValueIsNotInList(mergedFeed.Name.ToUpper(), receiverAndMergedFeedNames, new Validation(ValidationField.Name, defaults) {
                        Message = Strings.ReceiversAndMergedFeedNamesMustBeUnique,
                    });

                    // The name cannot contain a comma
                    ConditionIsFalse(mergedFeed.Name, r => r.Contains(","), new Validation(ValidationField.Name, defaults) {
                        Message = Strings.NameCannotContainComma,
                    });
                }

                // The ICAO timeout has to be between 1000ms and 30000ms
                ValueIsInRange(mergedFeed.IcaoTimeout, 1000, 30000, new Validation(ValidationField.IcaoTimeout, defaults) {
                    Message = Strings.IcaoTimeoutOutOfBounds,
                });

                // There has to be at least two receivers in the merged feed (although we don't care if they're not enabled)
                ConditionIsFalse(mergedFeed.ReceiverIds, r => r.Count < 2, new Validation(ValidationField.ReceiverIds, defaults) {
                    Message = Strings.MergedFeedNeedsAtLeastTwoReceivers,
                });
            }
        }
        #endregion

        #region RawFeedDecoding
        /// <summary>
        /// Validates the raw feed decoding settings.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateRawFeedDecoding(Validation defaults)
        {
            if(defaults.Record == null) {
                var settings = _View.Configuration.RawDecodingSettings;

                ValueIsInRange(settings.ReceiverRange, 0, 99999, new Validation(ValidationField.ReceiverRange, defaults) {
                    Message = Strings.ReceiverRangeOutOfBounds,
                });

                ValueIsInRange(settings.AirborneGlobalPositionLimit, 1, 30, new Validation(ValidationField.AirborneGlobalPositionLimit, defaults) {
                    Message = Strings.AirborneGlobalPositionLimitOutOfBounds,
                });

                ValueIsInRange(settings.FastSurfaceGlobalPositionLimit, 1, 75, new Validation(ValidationField.FastSurfaceGlobalPositionLimit, defaults) {
                    Message = Strings.FastSurfaceGlobalPositionLimitOutOfBounds,
                });

                ValueIsInRange(settings.SlowSurfaceGlobalPositionLimit, 1, 150, new Validation(ValidationField.SlowSurfaceGlobalPositionLimit, defaults) {
                    Message = Strings.SlowSurfaceGlobalPositionLimitOutOfBounds,
                });

                ValueIsInRange(settings.AcceptableAirborneSpeed, 0.005, 45.0, new Validation(ValidationField.AcceptableAirborneLocalPositionSpeed, defaults) {
                    Message = Strings.AcceptableAirborneSpeedOutOfBounds,
                });

                ValueIsInRange(settings.AcceptableAirSurfaceTransitionSpeed, 0.003, 20.0, new Validation(ValidationField.AcceptableTransitionLocalPositionSpeed, defaults) {
                    Message = Strings.AcceptableAirSurfaceTransitionSpeedOutOfBounds,
                });

                ValueIsInRange(settings.AcceptableSurfaceSpeed, 0.001, 10.0, new Validation(ValidationField.AcceptableSurfaceLocalPositionSpeed, defaults) {
                    Message = Strings.AcceptableSurfaceSpeedOutOfBounds,
                });

                ValueIsInRange(settings.AcceptIcaoInNonPICount, 0, 100, new Validation(ValidationField.AcceptIcaoInNonPICount, defaults) {
                    Message = Strings.AcceptIcaoInNonPICountOutOfBounds,
                });

                ValueIsInRange(settings.AcceptIcaoInNonPISeconds, 1, 30, new Validation(ValidationField.AcceptIcaoInNonPISeconds, defaults) {
                    Message = Strings.AcceptIcaoInNonPISecondsOutOfBounds,
                });

                ValueIsInRange(settings.AcceptIcaoInPI0Count, 1, 10, new Validation(ValidationField.AcceptIcaoInPI0Count, defaults) {
                    Message = Strings.AcceptIcaoInPI0CountOutOfBounds,
                });

                ValueIsInRange(settings.AcceptIcaoInPI0Seconds, 1, 60, new Validation(ValidationField.AcceptIcaoInPI0Seconds, defaults) {
                    Message = Strings.AcceptIcaoInPI0SecondsOutOfBounds,
                });
            }
        }
        #endregion

        #region RebroadcastServers
        /// <summary>
        /// Handles the validation of the rebroadcast servers.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateRebroadcastServers(Validation defaults)
        {
            var server = defaults.Record as RebroadcastSettings;

            if(defaults.Record == null) {
                if(defaults.ValueChangedField == ValidationField.None) {
                    foreach(var child in _View.Configuration.RebroadcastSettings) {
                        var childDefaults = new Validation(defaults) { Record = child };
                        ValidateRebroadcastServers(childDefaults);
                    }
                }
            } else if(server != null) {
                // The name must be supplied
                if(StringIsNotEmpty(server.Name, new Validation(ValidationField.Name, defaults) {
                    Message = Strings.NameRequired,
                })) {
                    // Name must be unique
                    var names = _View.Configuration.RebroadcastSettings.Where(r => r != server).Select(r => (r.Name ?? "").ToUpper()).ToArray();
                    ValueIsNotInList(server.Name, names, new Validation(ValidationField.Name, defaults) {
                        Message = Strings.NameMustBeUnique,
                    });
                }

                // Either transmit must be switched off or the transmit address should be a valid DNS address
                ConditionIsTrue(server, r => !r.IsTransmitter || DomainAddressIsValid(r.TransmitAddress),
                    new Validation(ValidationField.BaseStationAddress, defaults) {
                        Format = Strings.CannotResolveAddress,
                        Args = new object[] { server.TransmitAddress },
                        IsWarning = !String.IsNullOrEmpty((server.TransmitAddress ?? "").Trim()),
                        RelatedFields = { ValidationField.IsTransmitter },
                    }
                );

                // Port must be within range
                ValueIsInRange(server.Port, 1, 65535, new Validation(ValidationField.RebroadcastServerPort, defaults) {
                    Message = Strings.PortOutOfBounds,
                });

                // Either we're transmitting or the port must be unique
                var otherPorts = ListeningPorts(server);
                ConditionIsTrue(server, r => r.IsTransmitter || !otherPorts.Contains(r.Port), new Validation(ValidationField.RebroadcastServerPort, defaults) {
                    Message = Strings.PortMustBeUnique,
                    RelatedFields = { ValidationField.IsTransmitter },
                });

                // Port cannot clash with the web server port
                ConditionIsTrue(server, r => {
                    var autoConfigWebServer = Factory.Singleton.Resolve<IAutoConfigWebServer>().Singleton;
                    return r.IsTransmitter || r.Port != autoConfigWebServer.WebServer.Port;
                }, new Validation(ValidationField.RebroadcastServerPort, defaults) {
                    Format = Strings.PortIsUsedByWebServer,
                    Args = new object[] { server.Port },
                    RelatedFields = { ValidationField.IsTransmitter },
                });

                // The idle timeout must be between 5 seconds and int.MaxValue, but only if KeepAlive is switched off
                ConditionIsTrue(server, r => r.UseKeepAlive || r.IdleTimeoutMilliseconds >= 5000, new Validation(ValidationField.IdleTimeout, defaults) {
                    Message = Strings.RebroadcastServerIdleTimeoutOutOfBounds,
                    RelatedFields = {
                        ValidationField.UseKeepAlive,
                    },
                });

                // Format is present
                ValueNotEqual(server.Format, RebroadcastFormat.None, new Validation(ValidationField.Format, defaults) {
                    Message = Strings.RebroadcastFormatRequired,
                });

                // Format is valid for the receiver type
                ConditionIsTrue(server.Format, (format) => {
                    var formatIsOK = true;

                    var receiver = _View.Configuration.Receivers.FirstOrDefault(r => r.UniqueId == server.ReceiverId);
                    var mergedFeed = _View.Configuration.MergedFeeds.FirstOrDefault(r => r.UniqueId == server.ReceiverId);

                    if(mergedFeed != null) {
                        switch(format) {
                            case RebroadcastFormat.Avr:             formatIsOK = false; break;      // Cannot convert BaseStation to AVR
                            case RebroadcastFormat.CompressedVRS:   formatIsOK = true; break;
                            case RebroadcastFormat.Passthrough:     formatIsOK = false; break;      // As-of time of writing Passthrough not raised for merged feed listeners
                            case RebroadcastFormat.Port30003:       formatIsOK = true; break;
                            case RebroadcastFormat.None:            break;
                            default:                                throw new NotImplementedException();
                        }
                    } else if(receiver != null) {
                        var isCooked = false;
                        switch(receiver.DataSource) {
                            case DataSource.Beast:          break;
                            case DataSource.CompressedVRS:  isCooked = true; break;
                            case DataSource.Port30003:      isCooked = true; break;
                            case DataSource.Sbs3:           break;
                            default:                        throw new NotImplementedException();
                        }
                        switch(format) {
                            case RebroadcastFormat.Avr:             formatIsOK = !isCooked; break;
                            case RebroadcastFormat.CompressedVRS:   formatIsOK = true; break;
                            case RebroadcastFormat.None:            break;
                            case RebroadcastFormat.Passthrough:     formatIsOK = true; break;
                            case RebroadcastFormat.Port30003:       formatIsOK = true; break;
                            default:                                throw new NotImplementedException();
                        }
                    }

                    return formatIsOK;
                }, new Validation(ValidationField.Format, defaults) {
                    Message = Strings.FormatNotSupportedForReceiverType,
                    RelatedFields = {
                        ValidationField.RebroadcastReceiver,
                    }
                });

                // Receiver has been supplied
                ValueIsInList(server.ReceiverId, CombinedFeedUniqueIds(null), new Validation(ValidationField.RebroadcastReceiver, defaults) {
                    Message = Strings.ReceiverRequired,
                });

                // Stale seconds is valid
                ConditionIsTrue(server.StaleSeconds, r => r > 0, new Validation(ValidationField.StaleSeconds, defaults) {
                    Message = Strings.StaleSecondsOutOfBounds,
                });
            }
        }
        #endregion

        #region Receivers
        /// <summary>
        /// Validates the list of receivers and/or an individual receiver.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateReceivers(Validation defaults)
        {
            var receiver = defaults.Record as Receiver;

            // If we're doing a full or partial validation then we should check to see if the receivers list has at least
            // one enabled receiver in it
            if(defaults.Record == null || receiver != null) {
                var settings = _View.Configuration;

                // There must be at least one receiver
                if(CollectionIsNotEmpty(settings.Receivers, new Validation(ValidationField.ReceiverIds, defaults) {
                    Record = null,
                    Message = Strings.PleaseConfigureAtLeastOneReceiver,
                })) {
                    // At least one receiver must be enabled
                    ConditionIsTrue(settings.Receivers, r => r.Count(i => i.Enabled) != 0, new Validation(ValidationField.ReceiverIds, defaults) {
                        Message = Strings.PleaseEnableAtLeastOneReceiver,
                        Record = null,
                        RelatedFields = { ValidationField.Enabled },    // This only works because the record = null set of ValidationFields doesn't contain an Enabled...
                    });
                }
            }

            if(defaults.Record == null) {
                // Check all of the child records
                if(defaults.ValueChangedField == ValidationField.None) {
                    foreach(var child in _View.Configuration.Receivers) {
                        var childDefaults = new Validation(defaults) { Record = child };
                        ValidateReceivers(childDefaults);
                    }
                }
            } else if(receiver != null) {
                // The receiver must have a name
                if(StringIsNotEmpty(receiver.Name, new Validation(ValidationField.Name, defaults) {
                    Message = Strings.NameRequired,
                })) {
                    // The name cannot be the same as any other receiver or merged feed name
                    var allNames = CombinedFeedNamesUpperCase(receiver);
                    ValueIsNotInList(receiver.Name.ToUpper(), allNames, new Validation(ValidationField.Name, defaults) {
                        Message = Strings.ReceiversAndMergedFeedNamesMustBeUnique,
                    });

                    // The name cannot contain a comma (can't remember exactly why - easier parsing of the names at the browser?)
                    ConditionIsFalse(receiver.Name, r => r.Contains(","), new Validation(ValidationField.Name, defaults) {
                        Message = Strings.NameCannotContainComma,
                    });
                }

                // If a receiver location has been supplied then it must exist
                if(receiver.ReceiverLocationId != 0) {
                    ValueIsInList(receiver.ReceiverLocationId, _View.Configuration.ReceiverLocations.Select(r => r.UniqueId), new Validation(ValidationField.Location, defaults) {
                        Message = Strings.LocationDoesNotExist,
                    });
                }

                // Test values that rely on the connection type
                switch(receiver.ConnectionType) {
                    case ConnectionType.TCP:
                        // The address must be supplied
                        DomainAddressIsValid(receiver.Address, new Validation(ValidationField.BaseStationAddress, defaults) {
                            Format = Strings.CannotResolveAddress,
                            Args = new object[] { receiver.Address },
                            IsWarning = !String.IsNullOrEmpty((receiver.Address ?? "").Trim()),
                        });

                        // The port must be within range
                        ValueIsInRange(receiver.Port, 1, 65535, new Validation(ValidationField.BaseStationPort, defaults) {
                            Message = Strings.PortOutOfBounds,
                        });

                        // The idle timeout must be between 5 seconds and int.MaxValue, but only if KeepAlive is switched off
                        ConditionIsTrue(receiver, r => r.UseKeepAlive || r.IdleTimeoutMilliseconds >= 5000, new Validation(ValidationField.IdleTimeout, defaults) {
                            Message = Strings.ReceiverIdleTimeoutOutOfBounds,
                            RelatedFields = {
                                ValidationField.UseKeepAlive,
                            },
                        });
                        break;
                    case ConnectionType.COM:
                        // The COM port must be supplied
                        if(StringIsNotEmpty(receiver.ComPort, new Validation(ValidationField.ComPort, defaults) {
                            Message = Strings.SerialComPortMissing
                        })) {
                            // The COM port must be known to the system
                            ValueIsInList(receiver.ComPort, Provider.GetSerialPortNames(), new Validation(ValidationField.ComPort, defaults) {
                                Message = Strings.SerialComPortUnknown,
                            });
                        }

                        // The baud rate must be one of the known good baud rates
                        ConditionIsTrue(receiver.BaudRate, (r) => {
                            switch(r) {
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
                                    return true;
                                default:
                                    return false;
                            }
                        }, new Validation(ValidationField.BaudRate, defaults) {
                            Message = Strings.SerialBaudRateInvalidValue,
                        });

                        // The data bits value must be a known good value
                        ValueIsInRange(receiver.DataBits, 5, 8, new Validation(ValidationField.DataBits, defaults) {
                            Message = Strings.SerialDataBitsOutOfBounds,
                        });
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        #endregion

        #region ReceiverLocations
        /// <summary>
        /// Validates the list of receivers and/or an individual receiver.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateReceiverLocations(Validation defaults)
        {
            var receiverLocation = defaults.Record as ReceiverLocation;

            if(defaults.Record == null) {
                if(defaults.ValueChangedField == ValidationField.None) {
                    foreach(var child in _View.Configuration.ReceiverLocations) {
                        var childDefaults = new Validation(defaults) { Record = child };
                        ValidateReceiverLocations(childDefaults);
                    }
                }
            } else if(receiverLocation != null) {
                // Name must not be empty and must be unique
                if(StringIsNotEmpty(receiverLocation.Name, new Validation(ValidationField.Location, defaults) {
                    Message = Strings.PleaseEnterNameForLocation,
                })) {
                    var names = _View.Configuration.ReceiverLocations.Where(r => r != receiverLocation).Select(r => (r.Name ?? "").ToUpper());
                    ValueIsNotInList(receiverLocation.Name.ToUpper(), names, new Validation(ValidationField.Location, defaults) {
                        Message = Strings.PleaseEnterUniqueNameForLocation,
                    });
                }

                // Latitude must be between -90 and 90
                ValueIsInRange(receiverLocation.Latitude, -90.0, 90.0, new Validation(ValidationField.Latitude, defaults) {
                    Message = Strings.LatitudeOutOfBounds,
                });

                // Longitude must be between -180 and 180
                ValueIsInRange(receiverLocation.Longitude, -180.0, 180.0, new Validation(ValidationField.Longitude, defaults) {
                    Message = Strings.LongitudeOutOfBounds,
                });

                // Can't have latitude set to zero - will be useless for ground position decoding
                ValueNotEqual(receiverLocation.Latitude, 0.0, new Validation(ValidationField.Latitude, defaults) {
                    Message = Strings.LatitudeCannotBeZero,
                });
            }
        }
        #endregion

        #region Users
        /// <summary>
        /// Validates the list of users and/or an individual user.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateUsers(Validation defaults)
        {
            var user = defaults.Record as IUser;

            if(defaults.Record == null) {
                if(defaults.ValueChangedField == ValidationField.None) {
                    foreach(var child in _View.Users) {
                        var childDefaults = new Validation(defaults) { Record = child };
                        ValidateUsers(childDefaults);
                    }
                }
            } else if(user != null) {
                if(_UserManager.CanEditUsers) {
                    switch(defaults.ValueChangedField) {
                        case ValidationField.None:
                        case ValidationField.LoginName:
                        case ValidationField.Password:
                        case ValidationField.Name:
                            _UserManager.ValidateUser(defaults.Results.Results, user, user, _View.Users);
                            break;
                    }
                }
            }
        }
        #endregion

        #region VersionCheckSettings
        /// <summary>
        /// Validates the version check settings.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateVersionCheckSettings(Validation defaults)
        {
            if(defaults.Record == null) {
                var settings = _View.Configuration.VersionCheckSettings;

                // Version check period is within range
                ValueIsInRange(settings.CheckPeriodDays, 1, 365, new Validation(ValidationField.CheckForNewVersions, defaults) {
                    Message = Strings.DaysBetweenChecksOutOfBounds,
                });
            }
        }
        #endregion

        #region WebServer
        /// <summary>
        /// Validates the web server settings.
        /// </summary>
        /// <param name="defaults"></param>
        private void ValidateWebServer(Validation defaults)
        {
            if(defaults.Record == null) {
                var settings = _View.Configuration.WebServerSettings;

                // UPnP port has to be within range
                ValueIsInRange(settings.UPnpPort, 1, 65535, new Validation(ValidationField.UPnpPortNumber, defaults) {
                    Message = Strings.UPnpPortOutOfBounds,
                });
            }
        }
        #endregion
        #endregion

        #region HandleConfigurationPropertyChanged, HandleUsersPropertyChanged
        /// <summary>
        /// Converts from the object and property name to a ValidationField, validates the field and shows the validation
        /// results to the user.
        /// </summary>
        /// <param name="args"></param>
        private void HandleConfigurationPropertyChanged(ConfigurationListenerEventArgs args)
        {
            var record = args.IsListChild ? args.Record : null;

            var field = ValidationField.None;
            switch(args.Group) {
                case ConfigurationListenerGroup.Audio:                  field = ConvertAudioPropertyToValidationField(args); break;
                case ConfigurationListenerGroup.BaseStation:            field = ConvertBaseStationPropertyToValidationField(args); break;
                case ConfigurationListenerGroup.Configuration:          field = ConvertConfigurationPropertyToValidationField(args); break;
                case ConfigurationListenerGroup.GoogleMapSettings:      field = ConvertGoogleMapPropertyToValidationFields(args); break;
                case ConfigurationListenerGroup.InternetClientSettings: field = ConvertInternetClientPropertyToValidationFields(args); break;
                case ConfigurationListenerGroup.MergedFeed:             field = ConvertMergedFeedPropertyToValidationFields(args); break;
                case ConfigurationListenerGroup.RawDecodingSettings:    field = ConvertRawFeedDecodingToValidationFields(args); break;
                case ConfigurationListenerGroup.RebroadcastSetting:     field = ConvertRebroadcastServerToValidationFields(args); break;
                case ConfigurationListenerGroup.Receiver:               field = ConvertReceiverPropertyToValidationField(args); break;
                case ConfigurationListenerGroup.ReceiverLocation:       field = ConvertReceiverLocationPropertyToValidationField(args); break;
                case ConfigurationListenerGroup.VersionCheckSettings:   field = ConvertVersionPropertyToValidationField(args); break;
                case ConfigurationListenerGroup.WebServerSettings:      field = ConvertWebServerPropertyToValidationField(args); break;
                default:                                            break;
            }

            ValidateAndReportSingleField(record, field);
        }

        /// <summary>
        /// Converts from the IUser object and property name to a ValidationField, validates the field and shows the validation
        /// results to the user.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="propertyName"></param>
        private void HandleUsersPropertyChanged(IUser user, string propertyName)
        {
            if(user != null) {
                var field = ValidationFieldForPropertyName<IUser>(propertyName, new Dictionary<ValidationField,Expression<Func<IUser,object>>>() {
                    { ValidationField.LoginName,    r => r.LoginName },
                    { ValidationField.Password,     r => r.UIPassword },
                    { ValidationField.Name,         r => r.Name },
                });

                ValidateAndReportSingleField(user, field);
            }
        }


        private ValidationField ValidationFieldForPropertyName<TModel>(string propertyName, Dictionary<ValidationField, Expression<Func<TModel, object>>> map)
        {
            var result = ValidationField.None;

            foreach(var kvp in map) {
                var fieldPropertyName = PropertyHelper.ExtractName<TModel>(kvp.Value);
                if(propertyName == fieldPropertyName) {
                    result = kvp.Key;
                    break;
                }
            }

            return result;
        }

        private ValidationField ConvertAudioPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<AudioSettings>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<AudioSettings,object>>>() {
                { ValidationField.TextToSpeechSpeed, r => r.VoiceRate },
            });
            
            return result;
        }

        private ValidationField ConvertBaseStationPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<BaseStationSettings>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<BaseStationSettings,object>>>() {
                { ValidationField.BaseStationDatabase,  r => r.DatabaseFileName },
                { ValidationField.FlagsFolder,          r => r.OperatorFlagsFolder },
                { ValidationField.SilhouettesFolder,    r => r.SilhouettesFolder },
                { ValidationField.PicturesFolder,       r => r.PicturesFolder },

                { ValidationField.DisplayTimeout,       r => r.DisplayTimeoutSeconds },
                { ValidationField.TrackingTimeout,      r => r.TrackingTimeoutSeconds },
            });
            
            return result;
        }

        private ValidationField ConvertConfigurationPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<Configuration>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<Configuration,object>>>() {
                { ValidationField.ReceiverIds,  r => r.Receivers },
            });

            return result;
        }

        private ValidationField ConvertGoogleMapPropertyToValidationFields(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<GoogleMapSettings>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<GoogleMapSettings,object>>>() {
                { ValidationField.ClosestAircraftReceiver,          r => r.ClosestAircraftReceiverId },
                { ValidationField.FlightSimulatorXReceiver,         r => r.FlightSimulatorXReceiverId },
                { ValidationField.WebSiteReceiver,                  r => r.WebSiteReceiverId },

                { ValidationField.InitialGoogleMapRefreshSeconds,   r => r.InitialRefreshSeconds },
                { ValidationField.MinimumGoogleMapRefreshSeconds,   r => r.MinimumRefreshSeconds },

                { ValidationField.GoogleMapZoomLevel,               r => r.InitialMapZoom },
                { ValidationField.Latitude,                         r => r.InitialMapLatitude },
                { ValidationField.Longitude,                        r => r.InitialMapLongitude },

                { ValidationField.ShortTrailLength,                 r => r.ShortTrailLengthSeconds },
            });

            return result;
        }

        private ValidationField ConvertInternetClientPropertyToValidationFields(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<InternetClientSettings>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<InternetClientSettings,object>>>() {
                { ValidationField.InternetUserIdleTimeout, r => r.TimeoutMinutes },
            });

            return result;
        }

        private ValidationField ConvertMergedFeedPropertyToValidationFields(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<MergedFeed>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<MergedFeed,object>>>() {
                { ValidationField.Name,         r => r.Name },
                { ValidationField.IcaoTimeout,  r => r.IcaoTimeout },
                { ValidationField.ReceiverIds,  r => r.ReceiverIds },
            });

            return result;
        }

        private ValidationField ConvertRawFeedDecodingToValidationFields(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<RawDecodingSettings>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<RawDecodingSettings,object>>>() {
                { ValidationField.ReceiverRange,                            r => r.ReceiverRange },
                { ValidationField.AirborneGlobalPositionLimit,              r => r.AirborneGlobalPositionLimit },
                { ValidationField.FastSurfaceGlobalPositionLimit,           r => r.FastSurfaceGlobalPositionLimit },
                { ValidationField.SlowSurfaceGlobalPositionLimit,           r => r.SlowSurfaceGlobalPositionLimit },
                { ValidationField.AcceptableAirborneLocalPositionSpeed,     r => r.AcceptableAirborneSpeed },
                { ValidationField.AcceptableTransitionLocalPositionSpeed,   r => r.AcceptableAirSurfaceTransitionSpeed },
                { ValidationField.AcceptableSurfaceLocalPositionSpeed,      r => r.AcceptableSurfaceSpeed },
                { ValidationField.AcceptIcaoInPI0Count,                     r => r.AcceptIcaoInPI0Count },
                { ValidationField.AcceptIcaoInPI0Seconds,                   r => r.AcceptIcaoInPI0Seconds },
                { ValidationField.AcceptIcaoInNonPICount,                   r => r.AcceptIcaoInNonPICount },
                { ValidationField.AcceptIcaoInNonPISeconds,                 r => r.AcceptIcaoInNonPISeconds },
            });

            return result;
        }

        private ValidationField ConvertRebroadcastServerToValidationFields(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<RebroadcastSettings>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<RebroadcastSettings,object>>>() {
                { ValidationField.Name,                     r => r.Name },
                { ValidationField.IsTransmitter,            r => r.IsTransmitter },
                { ValidationField.BaseStationAddress,       r => r.TransmitAddress },
                { ValidationField.RebroadcastServerPort,    r => r.Port },
                { ValidationField.UseKeepAlive,             r => r.UseKeepAlive },
                { ValidationField.IdleTimeout,              r => r.IdleTimeoutMilliseconds },
                { ValidationField.Format,                   r => r.Format },
                { ValidationField.RebroadcastReceiver,      r => r.ReceiverId },
                { ValidationField.StaleSeconds,             r => r.StaleSeconds },
            });

            return result;
        }

        private ValidationField ConvertReceiverPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<Receiver>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<Receiver,object>>>() {
                { ValidationField.Enabled,              r => r.Enabled },
                { ValidationField.Name,                 r => r.Name },
                { ValidationField.Location,             r => r.ReceiverLocationId },
                { ValidationField.BaseStationAddress,   r => r.Address },
                { ValidationField.BaseStationPort,      r => r.Port },
                { ValidationField.UseKeepAlive,         r => r.UseKeepAlive },
                { ValidationField.IdleTimeout,          r => r.IdleTimeoutMilliseconds },
                { ValidationField.ComPort,              r => r.ComPort },
                { ValidationField.BaudRate,             r => r.BaudRate },
                { ValidationField.DataBits,             r => r.DataBits },
            });

            return result;
        }

        private ValidationField ConvertReceiverLocationPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<ReceiverLocation>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<ReceiverLocation,object>>>() {
                { ValidationField.Location,     r => r.Name },
                { ValidationField.Latitude,     r => r.Latitude },
                { ValidationField.Longitude,    r => r.Longitude },
            });

            return result;
        }

        private ValidationField ConvertVersionPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<VersionCheckSettings>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<VersionCheckSettings,object>>>() {
                { ValidationField.CheckForNewVersions,  r => r.CheckPeriodDays },
            });

            return result;
        }

        private ValidationField ConvertWebServerPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<WebServerSettings>(args.PropertyName, new Dictionary<ValidationField,Expression<Func<WebServerSettings,object>>>() {
                { ValidationField.UPnpPortNumber, r => r.UPnpPort },
            });

            return result;
        }
        #endregion

        #region Save
        /// <summary>
        /// Ensures that the configuration is in a valid state and then, if it is, it saves it.
        /// </summary>
        private void Save()
        {
            var validationResults = Validate();
            _View.ShowValidationResults(validationResults);

            if(!validationResults.HasErrors) {
                SaveUsers();

                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                configurationStorage.Save(_View.Configuration);
            }
        }

        /// <summary>
        /// Saves the users entered via the configuration screen.
        /// </summary>
        private void SaveUsers()
        {
            var existing = _UserManager.GetUsers();

            // Update existing records
            foreach(var record in _View.Users) {
                var current = existing.FirstOrDefault(r => r.UniqueId == record.UniqueId);
                if(current != null) {
                    if(!_UserManager.CanEditUsers) {
                        record.Name = current.Name;
                        record.LoginName = current.LoginName;
                    }
                    if(!_UserManager.CanChangeEnabledState) record.Enabled = current.Enabled;
                    if(!_UserManager.CanChangePassword) record.UIPassword = _DefaultPassword;

                    if(record.Enabled !=    current.Enabled ||
                       record.LoginName !=  current.LoginName ||
                       record.Name !=       current.Name ||
                       record.UIPassword != _DefaultPassword
                    ) {
                        _UserManager.UpdateUser(record, record.UIPassword == _DefaultPassword ? null : record.UIPassword);
                    }
                }
            }

            // Insert new records
            if(_UserManager.CanCreateUsers) {
                foreach(var record in _View.Users.Where(r => !r.IsPersisted)) {
                    if(record.UIPassword == _DefaultPassword) record.UIPassword = null;
                    _UserManager.CreateUser(record);
                    record.UIPassword = _DefaultPassword;
                }
            }

            // Delete missing records
            if(_UserManager.CanDeleteUsers) {
                foreach(var missing in existing.Where(r => !_View.Users.Any(i => i.IsPersisted && i.UniqueId == r.UniqueId))) {
                    _UserManager.DeleteUser(missing);
                }
            }
        }
        #endregion

        #region View events
        /// <summary>
        /// Raised when the user indicates that they don't have a radio, they only have FSX.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_FlightSimulatorXOnlyClicked(object sender, EventArgs args)
        {
            UseFlightSimulatorXOnlySettings();
        }

        /// <summary>
        /// Raised when the user elects to save their changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_SaveClicked(object sender, EventArgs args)
        {
            Save();
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
            Provider.TestTextToSpeech(_View.Configuration.AudioSettings.VoiceName, _View.Configuration.AudioSettings.VoiceRate);
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
        #endregion

        #region ConfigurationListener and User Event Handlers
        /// <summary>
        /// Raised when something changes the configuration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationListener_PropertyChanged(object sender, ConfigurationListenerEventArgs args)
        {
            if(!_ValueChangedValidationDisabled) {
                HandleConfigurationPropertyChanged(args);
            }
        }

        /// <summary>
        /// Raised when something changes the users list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Users_ListChanged(object sender, ListChangedEventArgs args)
        {
            if(args.ListChangedType == ListChangedType.ItemChanged) {
                HandleUsersPropertyChanged(_View.Users[args.NewIndex], args.PropertyDescriptor.Name);
            }
        }
        #endregion
    }
}
