using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.PortableBinding;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebSite;
using VirtualRadar.Localisation;
using VirtualRadar.Plugin.WebAdmin.View.Settings;

namespace VirtualRadar.Plugin.WebAdmin.View
{
    class SettingsView : ISettingsView
    {
        private ISettingsPresenter _Presenter;
        private ValidationModelHelper _ValidationHelper;
        private ViewModel _ViewModel;
        private TestConnectionOutcomeModel _TestConnectionOutcome;
        private bool _FailedValidation;

        public Configuration Configuration { get; set; }

        public NotifyList<IUser> Users { get; private set; }

        public string UserManager { get; set; }

        public string OpenOnPageTitle { get; set; }

        public object OpenOnRecord { get; set; }

        public event EventHandler SaveClicked;
        private void OnSaveClicked(EventArgs args)
        {
            EventHelper.Raise(SaveClicked, this, args);
        }

        public event EventHandler UpdateReceiverLocationsFromBaseStationDatabaseClicked;
        private void OnUpdateReceiverLocationsFromBaseStationDatabaseClicked(EventArgs args)
        {
            EventHelper.Raise(UpdateReceiverLocationsFromBaseStationDatabaseClicked, this, args);
        }

        public event EventHandler<EventArgs<Receiver>> TestConnectionClicked;
        private void OnTestConnectionClicked(EventArgs<Receiver> args)
        {
            EventHelper.Raise(TestConnectionClicked, this, args);
        }

        public event EventHandler UseIcaoRawDecodingSettingsClicked;
        private void OnUseIcaoRawDecodingSettingsClicked(EventArgs args)
        {
            EventHelper.Raise(UseIcaoRawDecodingSettingsClicked, this, args);
        }

        public event EventHandler UseRecommendedRawDecodingSettingsClicked;
        private void OnUseRecommendedRawDecodingSettingsClicked(EventArgs args)
        {
            EventHelper.Raise(UseRecommendedRawDecodingSettingsClicked, this, args);
        }

        #pragma warning disable 0067
        public event EventHandler TestTextToSpeechSettingsClicked;
        public event EventHandler FlightSimulatorXOnlyClicked;
        public event EventHandler<EventArgs<string>> OpenUrlClicked;
        #pragma warning restore 0067

        public DialogResult ShowView()
        {
            _ViewModel = new ViewModel();
            _ValidationHelper = new ValidationModelHelper(_ViewModel.FindViewModelForRecord);
            Users = new NotifyList<IUser>();

            _Presenter = Factory.Resolve<ISettingsPresenter>();
            _Presenter.Initialise(this);

            _ViewModel.Configuration.RefreshFromConfiguration(Configuration, Users);
            _Presenter.ValidateView();

            _ViewModel.ComPortNames = _Presenter.GetSerialPortNames().ToArray();
            _ViewModel.VoiceNames = _Presenter.GetVoiceNames().ToArray();
            _ViewModel.TileServerSettingNames = _Presenter.GetTileServerSettingNames().ToArray();

            return DialogResult.OK;
        }

        public void Dispose()
        {
            _Presenter.Dispose();
        }

        public void PopulateTextToSpeechVoices(IEnumerable<string> voiceNames)
        {
            ;
        }

        public void ShowTestConnectionResults(string message, string title)
        {
            _TestConnectionOutcome = new TestConnectionOutcomeModel() {
                Title = title,
                Message = message,
            };
        }

        public void ShowAircraftDataLookupSettings(string dataSupplier, string supplierCredits, string supplierUrl)
        {
            _ViewModel.Configuration.OnlineLookupSupplierName = dataSupplier;
            _ViewModel.Configuration.OnlineLookupSupplierCredits = supplierCredits;
            _ViewModel.Configuration.OnlineLookupSupplierUrl = supplierUrl;
        }

        public object ShowBusy(bool isBusy, object previousState)
        {
            return null;
        }

        public void ShowValidationResults(ValidationResults results)
        {
            _ValidationHelper.ApplyValidationResults(results, _ViewModel);
            _FailedValidation = results.HasErrors;
        }

        [WebAdminMethod(User="user")]
        public ViewModel GetState(string user)
        {
            _ViewModel.CurrentUserName = user;
            return _ViewModel;
        }

        [WebAdminMethod(DeferExecution=true, User="user")]
        public ViewModel RaiseSaveClicked(ConfigurationModel configurationModel, string user)
        {
            return ApplyConfigurationAroundAction(configurationModel, user, () => {
                try {
                    OnSaveClicked(EventArgs.Empty);
                    _ViewModel.Outcome = _FailedValidation ? "FailedValidation" : "Saved";
                } catch(ConflictingUpdateException) {
                    var configurationStorage = Factory.Resolve<IConfigurationStorage>().Singleton;
                    var configuration = configurationStorage.Load();
                    _ViewModel.Configuration.RefreshFromConfiguration(configuration, Users);
                    ApplyConfigurationModelToView(_ViewModel.Configuration);

                    _ViewModel.Outcome = "ConflictingUpdate";
                }
            });
        }

        [WebAdminMethod(User="user")]
        public ViewModel RaiseUseIcaoRawDecodingSettingsClicked(ConfigurationModel configurationModel, string user)
        {
            return ApplyConfigurationAroundAction(configurationModel, user, () => {
                OnUseIcaoRawDecodingSettingsClicked(EventArgs.Empty);
            });
        }

        [WebAdminMethod(User="user")]
        public ViewModel RaiseUseRecommendedRawDecodingSettingsClicked(ConfigurationModel configurationModel, string user)
        {
            return ApplyConfigurationAroundAction(configurationModel, user, () => {
                OnUseRecommendedRawDecodingSettingsClicked(EventArgs.Empty);
            });
        }

        [WebAdminMethod(User="user")]
        public ViewModel CreateNewMergedFeed(ConfigurationModel configurationModel, string user)
        {
            return ApplyConfigurationAroundAction(configurationModel, user, () => {
                var newRecord = _Presenter.CreateMergedFeed();
                _ViewModel.NewMergedFeed = new MergedFeedModel(newRecord);
                ValidationModelHelper.CreateEmptyViewModelValidationFields(_ViewModel.NewMergedFeed);
            });
        }

        [WebAdminMethod(User="user")]
        public ViewModel CreateNewRebroadcastServer(ConfigurationModel configurationModel, string user)
        {
            return ApplyConfigurationAroundAction(configurationModel, user, () => {
                var newRecord = _Presenter.CreateRebroadcastServer();
                _ViewModel.NewRebroadcastServer = new RebroadcastServerModel(newRecord);
                ValidationModelHelper.CreateEmptyViewModelValidationFields(_ViewModel.NewRebroadcastServer);
            });
        }

        [WebAdminMethod(User="user")]
        public ViewModel CreateNewReceiver(ConfigurationModel configurationModel, string user)
        {
            return ApplyConfigurationAroundAction(configurationModel, user, () => {
                var newRecord = _Presenter.CreateReceiver();
                _ViewModel.NewReceiver = new ReceiverModel(newRecord);
                ValidationModelHelper.CreateEmptyViewModelValidationFields(_ViewModel.NewReceiver);
            });
        }

        [WebAdminMethod]
        public TestConnectionOutcomeModel TestConnection(ConfigurationModel configurationModel, int receiverId)
        {
            _TestConnectionOutcome = null;
            ApplyConfigurationModelToView(configurationModel);

            var receiver = Configuration.Receivers.FirstOrDefault(r => r.UniqueId == receiverId);
            if(receiver == null) {
                _TestConnectionOutcome = new TestConnectionOutcomeModel() {
                    Title = "Unknown Receiver ID",
                    Message = String.Format("There is no receiver with an ID of {0}", receiverId),
                };
            } else {
                var args = new EventArgs<Receiver>(receiver);
                try {
                    OnTestConnectionClicked(args);
                    if(_TestConnectionOutcome == null) {
                        _TestConnectionOutcome = new TestConnectionOutcomeModel() {
                            Title = "Presenter Failed",
                            Message = "The presenter did not supply a test connection outcome",
                        };
                    }
                } catch(Exception ex) {
                    _TestConnectionOutcome = new TestConnectionOutcomeModel() {
                        Title = Strings.Exception,
                        Message = ex.Message,
                    };
                }
            }

            return _TestConnectionOutcome;
        }

        [WebAdminMethod(User="user")]
        public ViewModel RaiseReceiverLocationsFromBaseStationDatabaseClicked(ConfigurationModel configurationModel, string user)
        {
            return ApplyConfigurationAroundAction(configurationModel, user, () => OnUpdateReceiverLocationsFromBaseStationDatabaseClicked(EventArgs.Empty));
        }

        [WebAdminMethod(User="user")]
        public ViewModel CreateNewReceiverLocation(ConfigurationModel configurationModel, string user)
        {
            return ApplyConfigurationAroundAction(configurationModel, user, () => {
                var newRecord = _Presenter.CreateReceiverLocation();
                _ViewModel.NewReceiverLocation = new ReceiverLocationModel(newRecord);
                ValidationModelHelper.CreateEmptyViewModelValidationFields(_ViewModel.NewReceiverLocation);
            });
        }

        [WebAdminMethod(User="user")]
        public ViewModel CreateNewUser(ConfigurationModel configurationModel, string user)
        {
            return ApplyConfigurationAroundAction(configurationModel, user, () => {
                var newRecord = _Presenter.CreateUser();
                _ViewModel.NewUser = new UserModel(newRecord);
                ValidationModelHelper.CreateEmptyViewModelValidationFields(_ViewModel.NewUser);
            });
        }

        private ViewModel ApplyConfigurationAroundAction(ConfigurationModel configurationModel, string user, Action action)
        {
            _ViewModel.CurrentUserName = user;
            _ViewModel.NewMergedFeed = null;
            _ViewModel.NewRebroadcastServer = null;
            _ViewModel.NewReceiver = null;
            _ViewModel.NewReceiverLocation = null;
            _ViewModel.NewUser = null;

            _ViewModel.Outcome = null;
            ApplyConfigurationModelToView(configurationModel);

            action();

            _ViewModel.Configuration.RefreshFromConfiguration(Configuration, Users);

            return _ViewModel;
        }

        private void ApplyConfigurationModelToView(ConfigurationModel configurationModel)
        {
            var originalConfiguration = _ViewModel.Configuration;

            _ViewModel.Configuration = configurationModel;
            _ViewModel.Configuration.CopyToConfiguration(Configuration, Users, _Presenter);

            _ViewModel.Configuration.OnlineLookupSupplierName = originalConfiguration.OnlineLookupSupplierName;
            _ViewModel.Configuration.OnlineLookupSupplierCredits = originalConfiguration.OnlineLookupSupplierCredits;
            _ViewModel.Configuration.OnlineLookupSupplierUrl = originalConfiguration.OnlineLookupSupplierUrl;
        }
    }
}
