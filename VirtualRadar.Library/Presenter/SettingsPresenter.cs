using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using InterfaceFactory;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// The default implementation of <see cref="ISettingsPresenter"/>.
    /// </summary>
    class SettingsPresenter : Presenter<ISettingsView>, ISettingsPresenter
    {
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

            var configStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
            var config = configStorage.Load();

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

            ValidateDataSources(result);

            return result;
        }

        /// <summary>
        /// Validates the data sources.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="valueChangedField"></param>
        private void ValidateDataSources(List<ValidationResult> results, object record = null, ValidationField valueChangedField = ValidationField.None)
        {
            if(record == null) {
                var settings = _View.Configuration.BaseStationSettings;

                FileExists(settings.DatabaseFileName, new ValidationParams(results, ValidationField.BaseStationDatabase) {
                    IsWarning = true,
                });

                FolderExists(settings.OperatorFlagsFolder, new ValidationParams(results, ValidationField.FlagsFolder) {
                    IsWarning = true,
                });

                FolderExists(settings.SilhouettesFolder, new ValidationParams(results, ValidationField.SilhouettesFolder) {
                    IsWarning = true,
                });

                FolderExists(settings.PicturesFolder, new ValidationParams(results, ValidationField.PicturesFolder) {
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


        private void CopyUIToConfiguration(Configuration configuration)
        {
        }

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
                var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                var configuration = configurationStorage.Load();

                SaveUsers();
                CopyUIToConfiguration(configuration);

                configurationStorage.Save(configuration);
            }
        }

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
