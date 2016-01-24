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
using VirtualRadar.Plugin.WebAdmin.View.Settings;

namespace VirtualRadar.Plugin.WebAdmin.View
{
    class SettingsView : ISettingsView
    {
        private ISettingsPresenter _Presenter;
        private ValidationModelHelper _ValidationHelper;
        private ViewModel _ViewModel;
        private bool _FailedValidation;

        private Configuration _Configuration;
        public Configuration Configuration
        {
            get { return _Configuration; }
            set {
                _Configuration = value;
                _ViewModel.Configuration.RefreshFromConfiguration(value);
            }
        }

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

        #pragma warning disable 0067
        public event EventHandler<EventArgs<Receiver>> TestConnectionClicked;
        public event EventHandler TestTextToSpeechSettingsClicked;
        public event EventHandler UseIcaoRawDecodingSettingsClicked;
        public event EventHandler UseRecommendedRawDecodingSettingsClicked;
        public event EventHandler FlightSimulatorXOnlyClicked;
        public event EventHandler<EventArgs<string>> OpenUrlClicked;
        #pragma warning restore 0067

        public DialogResult ShowView()
        {
            _ViewModel = new ViewModel();
            _ValidationHelper = new ValidationModelHelper(_ViewModel.FindViewModelForRecord);
            Users = new NotifyList<IUser>();

            _Presenter = Factory.Singleton.Resolve<ISettingsPresenter>();
            _Presenter.Initialise(this);
            _Presenter.ValidateView();

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
            ;
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

        [WebAdminMethod]
        public ViewModel GetState()
        {
            return _ViewModel;
        }

        [WebAdminMethod(DeferExecution=true)]
        public ViewModel RaiseSaveClicked(ConfigurationModel configurationModel)
        {
            return ApplyConfigurationAroundAction(configurationModel, () => {
                try {
                    OnSaveClicked(EventArgs.Empty);
                    _ViewModel.Outcome = _FailedValidation ? "FailedValidation" : "Saved";
                } catch(ConflictingUpdateException) {
                    var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                    var configuration = configurationStorage.Load();
                    _ViewModel.Configuration.RefreshFromConfiguration(configuration);
                    ApplyConfigurationModelToView(_ViewModel.Configuration);

                    _ViewModel.Outcome = "ConflictingUpdate";
                }
            });
        }

        [WebAdminMethod]
        public ViewModel RaiseReceiverLocationsFromBaseStationDatabaseClicked(ConfigurationModel configurationModel)
        {
            return ApplyConfigurationAroundAction(configurationModel, () => OnUpdateReceiverLocationsFromBaseStationDatabaseClicked(EventArgs.Empty));
        }

        [WebAdminMethod]
        public ViewModel CreateNewReceiverLocation(ConfigurationModel configurationModel)
        {
            return ApplyConfigurationAroundAction(configurationModel, () => {
                var newRecord = _Presenter.CreateReceiverLocation();
                ValidationModelHelper.CreateEmptyViewModelValidationFields(newRecord);
                _ViewModel.NewReceiverLocation = new ReceiverLocationModel(newRecord);
            });
        }

        private ViewModel ApplyConfigurationAroundAction(ConfigurationModel configurationModel, Action action)
        {
            _ViewModel.NewReceiverLocation = null;

            _ViewModel.Outcome = null;
            ApplyConfigurationModelToView(configurationModel);

            action();

            _ViewModel.Configuration.RefreshFromConfiguration(Configuration);

            return _ViewModel;
        }

        private void ApplyConfigurationModelToView(ConfigurationModel configurationModel)
        {
            var originalConfiguration = _ViewModel.Configuration;

            _ViewModel.Configuration = configurationModel;
            _ViewModel.Configuration.CopyToConfiguration(Configuration);

            _ViewModel.Configuration.OnlineLookupSupplierName = originalConfiguration.OnlineLookupSupplierName;
            _ViewModel.Configuration.OnlineLookupSupplierCredits = originalConfiguration.OnlineLookupSupplierCredits;
            _ViewModel.Configuration.OnlineLookupSupplierUrl = originalConfiguration.OnlineLookupSupplierUrl;
        }
    }
}
