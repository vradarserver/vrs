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
                result.Add(null);

                try {
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

        #region Private class - DisableValidationOnViewChanged
        /// <summary>
        /// A private class that simplifies disabling validation on view changes.
        /// </summary>
        class DisableValidationOnViewChanged : IDisposable
        {
            private bool _RestoreSettingTo;
            private SettingsPresenter _Presenter;

            public DisableValidationOnViewChanged(SettingsPresenter presenter)
            {
                _Presenter = presenter;
                _RestoreSettingTo = _Presenter._ValueChangedValidationDisabled;
            }

            public void Dispose()
            {
                _Presenter._ValueChangedValidationDisabled = _RestoreSettingTo;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// True if validation on changes to values on the view is disabled. See
        /// <see cref="DisableValidationOnViewChanged"/>.
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
            }
        }
        #endregion

        #region Initialise
        /// <summary>
        /// Called by the view when it's ready to be initialised.
        /// </summary>
        /// <param name="view"></param>
        public override void Initialise(ISettingsView view)
        {
            base.Initialise(view);

            _View.SaveClicked += View_SaveClicked;
            _View.TestConnectionClicked += View_TestConnectionClicked;
            _View.UpdateReceiverLocationsFromBaseStationDatabaseClicked += View_UpdateReceiverLocationsFromBaseStationDatabaseClicked;
            _View.UseIcaoRawDecodingSettingsClicked += View_UseIcaoRawDecodingSettingsClicked;
            _View.UseRecommendedRawDecodingSettingsClicked += View_UseRecommendedRawDecodingSettingsClicked;

            var configStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
            var config = configStorage.Load();

            var highestReceiver = config.Receivers.OrderBy(r => r.UniqueId).LastOrDefault();
            var highestMergedFeed = config.MergedFeeds.OrderBy(r => r.UniqueId).LastOrDefault();
            _InitialMaxCombinedFeedId = Math.Max(highestReceiver == null ? 0 : highestReceiver.UniqueId, highestMergedFeed == null ? 0 : highestMergedFeed.UniqueId);

            _ConfigurationListener = Factory.Singleton.Resolve<IConfigurationListener>();
            _ConfigurationListener.PropertyChanged += ConfigurationListener_PropertyChanged;
            _ConfigurationListener.Initialise(config);

            CopyConfigurationToView(config);
        }

        /// <summary>
        /// Copies the configuration to the view.
        /// </summary>
        /// <param name="config"></param>
        private void CopyConfigurationToView(Configuration config)
        {
            using(new DisableValidationOnViewChanged(this)) {
                _View.Configuration = config;
            }
            ValidateForm();
        }
        #endregion

        #region Source Helpers - GetSerialPortNames
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetSerialPortNames()
        {
            return Provider.GetSerialPortNames();
        }
        #endregion

        #region Child object creation - CreateReceiver, CreateReceiverLocation, CreateMergedFeed, CreateRebroadcastServer
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

        #region UseIcaoRawDecodingSettings, UseRecommendedRawDecodingSettings
        /// <summary>
        /// Configures the view with the ICAO raw decoding settings.
        /// </summary>
        private void UseIcaoRawDecodingSettings()
        {
            _View.Configuration.RawDecodingSettings.AcceptableAirborneSpeed = 11.112;
            _View.Configuration.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed = 4.63;
            _View.Configuration.RawDecodingSettings.AcceptableSurfaceSpeed = 1.389;
            _View.Configuration.RawDecodingSettings.AirborneGlobalPositionLimit = 10;
            _View.Configuration.RawDecodingSettings.FastSurfaceGlobalPositionLimit = 25;
            _View.Configuration.RawDecodingSettings.SlowSurfaceGlobalPositionLimit = 50;
            _View.Configuration.RawDecodingSettings.SuppressReceiverRangeCheck = false;
            _View.Configuration.RawDecodingSettings.UseLocalDecodeForInitialPosition = false;
        }

        /// <summary>
        /// Configures the view with the default raw decoding settings.
        /// </summary>
        private void UseRecommendedRawDecodingSettings()
        {
            var defaults = new RawDecodingSettings();
            _View.Configuration.RawDecodingSettings.AcceptableAirborneSpeed = defaults.AcceptableAirborneSpeed;
            _View.Configuration.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed = defaults.AcceptableAirSurfaceTransitionSpeed;
            _View.Configuration.RawDecodingSettings.AcceptableSurfaceSpeed = defaults.AcceptableSurfaceSpeed;
            _View.Configuration.RawDecodingSettings.AirborneGlobalPositionLimit = defaults.AirborneGlobalPositionLimit;
            _View.Configuration.RawDecodingSettings.FastSurfaceGlobalPositionLimit = defaults.FastSurfaceGlobalPositionLimit;
            _View.Configuration.RawDecodingSettings.SlowSurfaceGlobalPositionLimit = defaults.SlowSurfaceGlobalPositionLimit;
            _View.Configuration.RawDecodingSettings.SuppressReceiverRangeCheck = true;
            _View.Configuration.RawDecodingSettings.UseLocalDecodeForInitialPosition = false;
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
                        case SdrDecoder.GrAirModes: receiver.Port = 30003; receiver.DataSource = DataSource.Port30003; break; // The source for gr-air-modes mentions a raw server but I couldn't see it being initialised? No idea what the format would be either :(
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
        /// Validates the form and returns the results, optionally constraining validation to a single record and field.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="valueChangedField"></param>
        /// <returns></returns>
        private List<ValidationResult> ValidateForm(object record = null, ValidationField valueChangedField = ValidationField.None)
        {
            List<ValidationResult> result = new List<ValidationResult>();

            ValidateDataSources(result, record, valueChangedField);
            ValidateGoogleMapSettings(result, record, valueChangedField);
            ValidateMergedFeeds(result, record, valueChangedField);
            ValidateRawFeedDecoding(result, record, valueChangedField);
            ValidateRebroadcastServers(result, record, valueChangedField);
            ValidateReceivers(result, record, valueChangedField);
            ValidateReceiverLocations(result, record, valueChangedField);

            return result;
        }

        private int[] CombinedFeedUniqueIds(object exceptCurrent)
        {
            var receiverIds = _View.Configuration.Receivers.Where(r => r != exceptCurrent).Select(r => r.UniqueId);
            var mergedFeedIds = _View.Configuration.MergedFeeds.Where(r => r != exceptCurrent).Select(r => r.UniqueId);

            return receiverIds.Concat(mergedFeedIds).ToArray();
        }

        private string[] CombinedFeedNamesUpperCase(object exceptCurrent)
        {
            var receiverNames = _View.Configuration.Receivers.Where(r => r != exceptCurrent).Select(r => (r.Name ?? "").ToUpper());
            var mergedFeedNames = _View.Configuration.MergedFeeds.Where(r => r != exceptCurrent).Select(r => (r.Name ?? "").ToUpper());

            return receiverNames.Concat(mergedFeedNames).ToArray();
        }

        #region DataSources
        /// <summary>
        /// Validates the data sources.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="valueChangedField"></param>
        private void ValidateDataSources(List<ValidationResult> results, object record, ValidationField valueChangedField)
        {
            if(record == null) {
                var settings = _View.Configuration.BaseStationSettings;

                FileExists(settings.DatabaseFileName, new ValidationParams(ValidationField.BaseStationDatabase, results, record, valueChangedField) {
                    IsWarning = true,
                });

                FolderExists(settings.OperatorFlagsFolder, new ValidationParams(ValidationField.FlagsFolder, results, record, valueChangedField) {
                    IsWarning = true,
                });

                FolderExists(settings.SilhouettesFolder, new ValidationParams(ValidationField.SilhouettesFolder, results, record, valueChangedField) {
                    IsWarning = true,
                });

                FolderExists(settings.PicturesFolder, new ValidationParams(ValidationField.PicturesFolder, results, record, valueChangedField) {
                    IsWarning = true,
                });
            }
        }
        #endregion

        #region GoogleMapSettings
        private void ValidateGoogleMapSettings(List<ValidationResult> results, object record, ValidationField valueChangedField)
        {
            if(record == null) {
                var settings = _View.Configuration.GoogleMapSettings;
                var allReceiverIds = _View.Configuration.Receivers.Select(r => r.UniqueId).Concat(_View.Configuration.MergedFeeds.Select(r => r.UniqueId)).ToArray();

                // Closest aircraft widget receiver has been filled in and is a valid ID
                if(ValueNotEqual(settings.ClosestAircraftReceiverId, 0, new ValidationParams(ValidationField.ClosestAircraftReceiver, results, record, valueChangedField) {
                    Message = Strings.ClosestAircraftReceiverRequired,
                })) {
                    ValueIsInList(settings.ClosestAircraftReceiverId, allReceiverIds, new ValidationParams(ValidationField.ClosestAircraftReceiver, results, record, valueChangedField) {
                        Message = Strings.ReceiverOrMergedFeedDoesNotExist,
                    });
                }

                // FSX receiver has been filled in and is a valid ID
                if(ValueNotEqual(settings.FlightSimulatorXReceiverId, 0, new ValidationParams(ValidationField.FlightSimulatorXReceiver, results, record, valueChangedField) {
                    Message = Strings.FlightSimulatorXReceiverRequired,
                })) {
                    ValueIsInList(settings.FlightSimulatorXReceiverId, allReceiverIds, new ValidationParams(ValidationField.FlightSimulatorXReceiver, results, record, valueChangedField) {
                        Message = Strings.ReceiverOrMergedFeedDoesNotExist,
                    });
                }

                // Default website receiver has been filled in and is a valid ID
                if(ValueNotEqual(settings.WebSiteReceiverId, 0, new ValidationParams(ValidationField.WebSiteReceiver, results, record, valueChangedField) {
                    Message = Strings.WebSiteReceiverRequired,
                })) {
                    ValueIsInList(settings.WebSiteReceiverId, allReceiverIds, new ValidationParams(ValidationField.WebSiteReceiver, results, record, valueChangedField) {
                        Message = Strings.ReceiverOrMergedFeedDoesNotExist,
                    });
                }
            }
        }
        #endregion

        #region MergedFeeds
        /// <summary>
        /// Validates the list of merged feeds.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="valueChangedField"></param>
        private void ValidateMergedFeeds(List<ValidationResult> results, object record, ValidationField valueChangedField)
        {
            var mergedFeed = record as MergedFeed;

            if(record == null) {
                // List of merged feeds. There's no validation on here, just on the child records
                if(valueChangedField == ValidationField.None) {
                    foreach(var child in _View.Configuration.MergedFeeds) {
                        ValidateMergedFeeds(results, child, valueChangedField);
                    }
                }
            } else if(mergedFeed != null) {
                // There has to be a name
                if(StringIsNotEmpty(mergedFeed.Name, new ValidationParams(ValidationField.Name, results, record, valueChangedField) {
                    Message = Strings.NameRequired,
                })) {
                    // The name cannot be the same as any other receiver or merged feed name
                    var receiverAndMergedFeedNames = CombinedFeedNamesUpperCase(mergedFeed);
                    ValueIsNotInList(mergedFeed.Name.ToUpper(), receiverAndMergedFeedNames, new ValidationParams(ValidationField.Name, results, record, valueChangedField) {
                        Message = Strings.ReceiversAndMergedFeedNamesMustBeUnique,
                    });

                    // The name cannot contain a comma
                    ConditionIsFalse(mergedFeed.Name, r => r.Contains(","), new ValidationParams(ValidationField.Name, results, record, valueChangedField) {
                        Message = Strings.NameCannotContainComma,
                    });
                }

                // The ICAO timeout has to be between 1000ms and 30000ms
                ValueIsInRange(mergedFeed.IcaoTimeout, 1000, 30000, new ValidationParams(ValidationField.IcaoTimeout, results, record, valueChangedField) {
                    Message = Strings.IcaoTimeoutOutOfBounds,
                });

                // There has to be at least two receivers in the merged feed (although we don't care if they're not enabled)
                ConditionIsFalse(mergedFeed.ReceiverIds, r => r.Count < 2, new ValidationParams(ValidationField.ReceiverIds, results, record, valueChangedField) {
                    Message = Strings.MergedFeedNeedsAtLeastTwoReceivers,
                });
            }
        }
        #endregion

        #region RawFeedDecoding
        /// <summary>
        /// Validates the raw feed decoding settings.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="valueChangedField"></param>
        private void ValidateRawFeedDecoding(List<ValidationResult> results, object record, ValidationField valueChangedField)
        {
            if(record == null) {
                var settings = _View.Configuration.RawDecodingSettings;

                ValueIsInRange(settings.ReceiverRange, 0, 99999, new ValidationParams(ValidationField.ReceiverRange, results, record, valueChangedField) {
                    Message = Strings.ReceiverRangeOutOfBounds,
                });

                ValueIsInRange(settings.AirborneGlobalPositionLimit, 1, 30, new ValidationParams(ValidationField.AirborneGlobalPositionLimit, results, record, valueChangedField) {
                    Message = Strings.AirborneGlobalPositionLimitOutOfBounds,
                });

                ValueIsInRange(settings.FastSurfaceGlobalPositionLimit, 1, 75, new ValidationParams(ValidationField.FastSurfaceGlobalPositionLimit, results, record, valueChangedField) {
                    Message = Strings.FastSurfaceGlobalPositionLimitOutOfBounds,
                });

                ValueIsInRange(settings.SlowSurfaceGlobalPositionLimit, 1, 150, new ValidationParams(ValidationField.SlowSurfaceGlobalPositionLimit, results, record, valueChangedField) {
                    Message = Strings.SlowSurfaceGlobalPositionLimitOutOfBounds,
                });

                ValueIsInRange(settings.AcceptableAirborneSpeed, 0.005, 45.0, new ValidationParams(ValidationField.AcceptableAirborneLocalPositionSpeed, results, record, valueChangedField) {
                    Message = Strings.AcceptableAirborneSpeedOutOfBounds,
                });

                ValueIsInRange(settings.AcceptableAirSurfaceTransitionSpeed, 0.003, 20.0, new ValidationParams(ValidationField.AcceptableTransitionLocalPositionSpeed, results, record, valueChangedField) {
                    Message = Strings.AcceptableAirSurfaceTransitionSpeedOutOfBounds,
                });

                ValueIsInRange(settings.AcceptableSurfaceSpeed, 0.001, 10.0, new ValidationParams(ValidationField.AcceptableSurfaceLocalPositionSpeed, results, record, valueChangedField) {
                    Message = Strings.AcceptableSurfaceSpeedOutOfBounds,
                });

                ValueIsInRange(settings.AcceptIcaoInNonPICount, 0, 100, new ValidationParams(ValidationField.AcceptIcaoInNonPICount, results, record, valueChangedField) {
                    Message = Strings.AcceptIcaoInNonPICountOutOfBounds,
                });

                ValueIsInRange(settings.AcceptIcaoInNonPISeconds, 1, 30, new ValidationParams(ValidationField.AcceptIcaoInNonPISeconds, results, record, valueChangedField) {
                    Message = Strings.AcceptIcaoInNonPISecondsOutOfBounds,
                });

                ValueIsInRange(settings.AcceptIcaoInPI0Count, 1, 10, new ValidationParams(ValidationField.AcceptIcaoInPI0Count, results, record, valueChangedField) {
                    Message = Strings.AcceptIcaoInPI0CountOutOfBounds,
                });

                ValueIsInRange(settings.AcceptIcaoInPI0Seconds, 1, 60, new ValidationParams(ValidationField.AcceptIcaoInPI0Seconds, results, record, valueChangedField) {
                    Message = Strings.AcceptIcaoInPI0SecondsOutOfBounds,
                });
            }
        }
        #endregion

        #region RebroadcastServers
        /// <summary>
        /// Handles the validation of the rebroadcast servers.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="valueChangedField"></param>
        private void ValidateRebroadcastServers(List<ValidationResult> results, object record, ValidationField valueChangedField)
        {
            var server = record as RebroadcastSettings;

            if(record == null) {
                if(valueChangedField == ValidationField.None) {
                    foreach(var child in _View.Configuration.RebroadcastSettings) {
                        ValidateRebroadcastServers(results, child, valueChangedField);
                    }
                }
            } else if(server != null) {
                // The name must be supplied
                if(StringIsNotEmpty(server.Name, new ValidationParams(ValidationField.Name, results, record, valueChangedField) {
                    Message = Strings.NameRequired,
                })) {
                    // Name must be unique
                    var names = _View.Configuration.RebroadcastSettings.Where(r => r != server).Select(r => (r.Name ?? "").ToUpper()).ToArray();
                    ValueIsNotInList(server.Name, names, new ValidationParams(ValidationField.Name, results, record, valueChangedField) {
                        Message = Strings.NameMustBeUnique,
                    });
                }

                // Port must be within range
                ValueIsInRange(server.Port, 1, 65535, new ValidationParams(ValidationField.RebroadcastServerPort, results, record, valueChangedField) {
                    Message = Strings.PortOutOfBounds,
                });

                // Port is unique
                var ports = _View.Configuration.RebroadcastSettings.Where(r => r != server).Select(r => r.Port).ToArray();
                ValueIsNotInList(server.Port, ports, new ValidationParams(ValidationField.RebroadcastServerPort, results, record, valueChangedField) {
                    Message = Strings.PortMustBeUnique,
                });

                // Format is correct
                ValueNotEqual(server.Format, RebroadcastFormat.None, new ValidationParams(ValidationField.Format, results, record, valueChangedField) {
                    Message = Strings.RebroadcastFormatRequired,
                });

                // Receiver has been supplied
                ValueIsInList(server.ReceiverId, CombinedFeedUniqueIds(null), new ValidationParams(ValidationField.RebroadcastReceiver, results, record, valueChangedField) {
                    Message = Strings.ReceiverRequired,
                });

                // Stale seconds is valid
                ConditionIsTrue(server.StaleSeconds, r => r > 0, new ValidationParams(ValidationField.StaleSeconds, results, record, valueChangedField) {
                    Message = Strings.StaleSecondsOutOfBounds,
                });
            }
        }
        #endregion

        #region Receivers
        /// <summary>
        /// Validates the list of receivers.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="valueChangedField"></param>
        private void ValidateReceivers(List<ValidationResult> results, object record, ValidationField valueChangedField)
        {
            var receiver = record as Receiver;

            if(record == null) {
                // LIST OF RECEIVERS
                var settings = _View.Configuration;

                // There must be at least one receiver
                if(CollectionIsNotEmpty(settings.Receivers, new ValidationParams(ValidationField.ReceiverIds, results, record, valueChangedField) {
                    Message = Strings.PleaseConfigureAtLeastOneReceiver,
                })) {
                    // At least one receiver must be enabled
                    ConditionIsTrue(settings.Receivers, r => r.Count(i => i.Enabled) != 0, new ValidationParams(ValidationField.ReceiverIds, results, record, valueChangedField) {
                        Message = Strings.PleaseEnableAtLeastOneReceiver,
                    });
                }

                // Check all of the child records
                if(valueChangedField == ValidationField.None) {
                    foreach(var child in settings.Receivers) {
                        ValidateReceivers(results, child, valueChangedField);
                    }
                }
            } else if(receiver != null) {
                // INDIVIDUAL RECEIVER

                // The receiver must have a name
                if(StringIsNotEmpty(receiver.Name, new ValidationParams(ValidationField.Name, results, record, valueChangedField) {
                    Message = Strings.NameRequired,
                })) {
                    // The name cannot be the same as any other receiver or merged feed name
                    var receiverAndMergedFeedNames = CombinedFeedNamesUpperCase(receiver);
                    ValueIsNotInList(receiver.Name.ToUpper(), receiverAndMergedFeedNames, new ValidationParams(ValidationField.Name, results, record, valueChangedField) {
                        Message = Strings.ReceiversAndMergedFeedNamesMustBeUnique,
                    });

                    // The name cannot contain a comma
                    ConditionIsFalse(receiver.Name, r => r.Contains(","), new ValidationParams(ValidationField.Name, results, record, valueChangedField) {
                        Message = Strings.NameCannotContainComma,
                    });
                }

                // If a receiver location has been supplied then it must exist
                if(receiver.ReceiverLocationId != 0) {
                    ValueIsInList(receiver.ReceiverLocationId, _View.Configuration.ReceiverLocations.Select(r => r.UniqueId), new ValidationParams(ValidationField.Location, results, record, valueChangedField) {
                        Message = Strings.LocationDoesNotExist,
                    });
                }

                // Test values that rely on the connection type
                switch(receiver.ConnectionType) {
                    case ConnectionType.TCP:
                        // The address must be supplied
                        if(StringIsNotEmpty(receiver.Address, new ValidationParams(ValidationField.BaseStationAddress, results, record, valueChangedField) {
                            Message = Strings.DataSourceNetworkAddressMissing,
                        })) {
                            // The address must resolve to a machine on the network
                            ConditionIsTrue(receiver.Address, (r) => {
                                try {
                                    Dns.GetHostAddresses(r);
                                    return true;
                                } catch {
                                    return false;
                                }
                            }, new ValidationParams(ValidationField.BaseStationAddress, results, record, valueChangedField) {
                                Format = Strings.CannotResolveAddress,
                                Args = new object[] { receiver.Address },
                            });
                        }

                        // The port must be within range
                        ValueIsInRange(receiver.Port, 1, 65535, new ValidationParams(ValidationField.BaseStationPort, results, record, valueChangedField) {
                            Message = Strings.PortOutOfBounds,
                        });
                        break;
                    case ConnectionType.COM:
                        // The COM port must be supplied
                        if(StringIsNotEmpty(receiver.ComPort, new ValidationParams(ValidationField.ComPort, results, record, valueChangedField) {
                            Message = Strings.SerialComPortMissing
                        })) {
                            // The COM port must be known to the system
                            ValueIsInList(receiver.ComPort, Provider.GetSerialPortNames(), new ValidationParams(ValidationField.ComPort, results, record, valueChangedField) {
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
                        }, new ValidationParams(ValidationField.BaudRate, results, record, valueChangedField) {
                            Message = Strings.SerialBaudRateInvalidValue,
                        });

                        // The data bits value must be a known good value
                        ValueIsInRange(receiver.DataBits, 5, 8, new ValidationParams(ValidationField.DataBits, results, record, valueChangedField) {
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
        /// Validates the list of receivers.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="valueChangedField"></param>
        private void ValidateReceiverLocations(List<ValidationResult> results, object record, ValidationField valueChangedField)
        {
            var receiverLocation = record as ReceiverLocation;

            if(record == null) {
                if(valueChangedField == ValidationField.None) {
                    foreach(var child in _View.Configuration.ReceiverLocations) {
                        ValidateReceiverLocations(results, child, valueChangedField);
                    }
                }
            } else if(receiverLocation != null) {
                // Name must not be empty and must be unique
                if(StringIsNotEmpty(receiverLocation.Name, new ValidationParams(ValidationField.Location, results, record, valueChangedField) {
                    Message = Strings.PleaseEnterNameForLocation,
                })) {
                    var names = _View.Configuration.ReceiverLocations.Where(r => r != receiverLocation).Select(r => (r.Name ?? "").ToUpper());
                    ValueIsNotInList(receiverLocation.Name.ToUpper(), names, new ValidationParams(ValidationField.Location, results, record, valueChangedField) {
                        Message = Strings.PleaseEnterUniqueNameForLocation,
                    });
                }

                // Latitude must be between -90 and 90
                ValueIsInRange(receiverLocation.Latitude, -90.0, 90.0, new ValidationParams(ValidationField.Latitude, results, record, valueChangedField) {
                    Message = Strings.LatitudeOutOfBounds,
                });

                // Longitude must be between -180 and 180
                ValueIsInRange(receiverLocation.Longitude, -180.0, 180.0, new ValidationParams(ValidationField.Longitude, results, record, valueChangedField) {
                    Message = Strings.LongitudeOutOfBounds,
                });

                // Can't have latitude set to zero - will be useless for ground position decoding
                ValueNotEqual(receiverLocation.Latitude, 0.0, new ValidationParams(ValidationField.Latitude, results, record, valueChangedField) {
                    Message = Strings.LatitudeCannotBeZero,
                });
            }
        }
        #endregion
        #endregion

        #region HandleConfigurationPropertyChanged
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
                case ConfigurationListenerGroup.BaseStation:            field = ConvertBaseStationPropertyToValidationField(args); break;
                case ConfigurationListenerGroup.Configuration:          field = ConvertConfigurationPropertyToValidationField(args); break;
                case ConfigurationListenerGroup.GoogleMapSettings:      field = ConvertGoogleMapPropertyToValidationFields(args); break;
                case ConfigurationListenerGroup.MergedFeed:             field = ConvertMergedFeedPropertyToValidationFields(args); break;
                case ConfigurationListenerGroup.RawDecodingSettings:    field = ConvertRawFeedDecodingToValidationFields(args); break;
                case ConfigurationListenerGroup.RebroadcastSetting:     field = ConvertRebroadcastServerToValidationFields(args); break;
                case ConfigurationListenerGroup.Receiver:               field = ConvertReceiverPropertyToValidationField(args); break;
                case ConfigurationListenerGroup.ReceiverLocation:       field = ConvertReceiverLocationPropertyToValidationField(args); break;
                default:                                            break;
            }

            if(field != ValidationField.None) {
                var results = ValidateForm(record, field);
                _View.ShowSingleFieldValidationResults(record, field, results);
            }
        }

        private ValidationField ValidationFieldForPropertyName<TModel>(ConfigurationListenerEventArgs args, Dictionary<ValidationField, Expression<Func<TModel, object>>> map)
        {
            var result = ValidationField.None;

            foreach(var kvp in map) {
                var propertyName = PropertyHelper.ExtractName<TModel>(kvp.Value);
                if(args.PropertyName == propertyName) {
                    result = kvp.Key;
                    break;
                }
            }

            return result;
        }

        private ValidationField ConvertBaseStationPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<BaseStationSettings>(args, new Dictionary<ValidationField,Expression<Func<BaseStationSettings,object>>>() {
                { ValidationField.BaseStationDatabase,  r => r.DatabaseFileName },
                { ValidationField.FlagsFolder,          r => r.OperatorFlagsFolder },
                { ValidationField.SilhouettesFolder,    r => r.SilhouettesFolder },
                { ValidationField.PicturesFolder,       r => r.PicturesFolder },
            });
            
            return result;
        }

        private ValidationField ConvertConfigurationPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<Configuration>(args, new Dictionary<ValidationField,Expression<Func<Configuration,object>>>() {
                { ValidationField.ReceiverIds,  r => r.Receivers },
            });

            return result;
        }

        private ValidationField ConvertGoogleMapPropertyToValidationFields(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<GoogleMapSettings>(args, new Dictionary<ValidationField,Expression<Func<GoogleMapSettings,object>>>() {
                { ValidationField.ClosestAircraftReceiver,  r => r.ClosestAircraftReceiverId },
                { ValidationField.FlightSimulatorXReceiver, r => r.FlightSimulatorXReceiverId },
                { ValidationField.WebSiteReceiver,          r => r.WebSiteReceiverId },
            });

            return result;
        }

        private ValidationField ConvertMergedFeedPropertyToValidationFields(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<MergedFeed>(args, new Dictionary<ValidationField,Expression<Func<MergedFeed,object>>>() {
                { ValidationField.Name,         r => r.Name },
                { ValidationField.IcaoTimeout,  r => r.IcaoTimeout },
                { ValidationField.ReceiverIds,  r => r.ReceiverIds },
            });

            return result;
        }

        private ValidationField ConvertRawFeedDecodingToValidationFields(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<RawDecodingSettings>(args, new Dictionary<ValidationField,Expression<Func<RawDecodingSettings,object>>>() {
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
            var result = ValidationFieldForPropertyName<RebroadcastSettings>(args, new Dictionary<ValidationField,Expression<Func<RebroadcastSettings,object>>>() {
                { ValidationField.Name,                     r => r.Name },
                { ValidationField.RebroadcastServerPort,    r => r.Port },
                { ValidationField.Format,                   r => r.Format },
                { ValidationField.RebroadcastReceiver,      r => r.ReceiverId },
                { ValidationField.StaleSeconds,             r => r.StaleSeconds },
            });

            return result;
        }

        private ValidationField ConvertReceiverPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<Receiver>(args, new Dictionary<ValidationField,Expression<Func<Receiver,object>>>() {
                { ValidationField.Enabled,              r => r.Enabled },
                { ValidationField.Name,                 r => r.Name },
                { ValidationField.Location,             r => r.ReceiverLocationId },
                { ValidationField.BaseStationAddress,   r => r.Address },
                { ValidationField.BaseStationPort,      r => r.Port },
                { ValidationField.ComPort,              r => r.ComPort },
                { ValidationField.BaudRate,             r => r.BaudRate },
                { ValidationField.DataBits,             r => r.DataBits },
            });

            return result;
        }

        private ValidationField ConvertReceiverLocationPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationFieldForPropertyName<ReceiverLocation>(args, new Dictionary<ValidationField,Expression<Func<ReceiverLocation,object>>>() {
                { ValidationField.Location,     r => r.Name },
                { ValidationField.Latitude,     r => r.Latitude },
                { ValidationField.Longitude,    r => r.Longitude },
            });

            return result;
        }
        #endregion


        private void SaveUsers()
        {
        }

        #region View events
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
        /// Raised when the user elects to save their changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_SaveClicked(object sender, EventArgs args)
        {
            var validationResults = ValidateForm();
            _View.ShowValidationResults(validationResults);

            if(validationResults.Where(r => r.IsWarning == false).Count() == 0) {
                SaveUsers();

                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                configurationStorage.Save(_View.Configuration);
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

        #region ConfigurationListener Event Handlers
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
        #endregion
    }
}
