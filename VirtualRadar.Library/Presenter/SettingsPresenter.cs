using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
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

        #region Child object creation - CreateReceiver
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

            return result;
        }

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
                case ConfigurationListenerGroup.BaseStation:    field = ConvertBaseStationPropertyToValidationField(args);  break;
                default:                                        break;
            }

            if(field != ValidationField.None) {
                var results = ValidateForm(record, field);
                _View.ShowSingleFieldValidationResults(record, field, results);
            }
        }

        private ValidationField ConvertBaseStationPropertyToValidationField(ConfigurationListenerEventArgs args)
        {
            var result = ValidationField.None;

            if(args.PropertyName == PropertyHelper.ExtractName<BaseStationSettings>(r => r.DatabaseFileName))           result = ValidationField.BaseStationDatabase;
            else if(args.PropertyName == PropertyHelper.ExtractName<BaseStationSettings>(r => r.OperatorFlagsFolder))   result = ValidationField.FlagsFolder;
            else if(args.PropertyName == PropertyHelper.ExtractName<BaseStationSettings>(r => r.SilhouettesFolder))     result = ValidationField.SilhouettesFolder;
            else if(args.PropertyName == PropertyHelper.ExtractName<BaseStationSettings>(r => r.PicturesFolder))        result = ValidationField.PicturesFolder;
            
            return result;
        }
        #endregion


        private void SaveUsers()
        {
        }

        #region View events
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
