using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.WebAdmin.View.Settings
{
    public class ViewModel
    {
        public ConfigurationModel Configuration { get; set; }

        public string Outcome { get; set; }

        public ReceiverLocationModel NewReceiverLocation { get; set; }

        public ViewModel()
        {
            Configuration = new ConfigurationModel();
        }

        public object FindViewModelForRecord(ValidationResult validationResult)
        {
            object result = null;

            if(validationResult.Record != null) {
                var receiverLocation = validationResult.Record as ReceiverLocation;
                if(receiverLocation != null) {
                    result = Configuration.ReceiverLocations.FirstOrDefault(r => r.UniqueId == receiverLocation.UniqueId);
                }
            }

            return result;
        }
    }

    public class ConfigurationModel
    {
        public int DataVersion { get; set; }

        public string OnlineLookupSupplierName { get; set; }

        public string OnlineLookupSupplierCredits { get; set; }

        public string OnlineLookupSupplierUrl { get; set; }

        public BaseStationSettingsModel BaseStationSettingsModel { get; private set; }

        public List<ReceiverLocationModel> ReceiverLocations { get; private set; }

        public ConfigurationModel()
        {
            BaseStationSettingsModel = new BaseStationSettingsModel();
            ReceiverLocations = new List<ReceiverLocationModel>();
        }

        public ConfigurationModel(Configuration configuration) : this()
        {
            RefreshFromConfiguration(configuration);
        }

        public void RefreshFromConfiguration(Configuration configuration)
        {
            DataVersion = configuration.DataVersion;

            BaseStationSettingsModel.RefreshFromSettings(configuration.BaseStationSettings);
            CollectionHelper.ApplySourceToDestination(configuration.ReceiverLocations, ReceiverLocations,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => new ReceiverLocationModel(source),
                (source, dest) => dest.RefreshFromSettings(source)
            );
        }

        public void CopyToConfiguration(Configuration configuration)
        {
            configuration.DataVersion = DataVersion;

            BaseStationSettingsModel.CopyToSettings(configuration.BaseStationSettings);
            CollectionHelper.ApplySourceToDestination(ReceiverLocations, configuration.ReceiverLocations,
                (source, dest) => source.UniqueId == dest.UniqueId,
                (source)       => source.CopyToSettings(new ReceiverLocation()),
                (source, dest) => source.CopyToSettings(dest)
            );
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

        public int TrackingTimeoutSeconds { get; set; }

        public bool MinimiseToSystemTray { get; set; }

        public int AutoSavePolarPlotsMinutes { get; set; }

        public bool LookupAircraftDetailsOnline { get; set; }

        public BaseStationSettingsModel()
        {
        }

        public BaseStationSettingsModel(BaseStationSettings settings)
        {
            ValidationModelHelper.CreateEmptyViewModelValidationFields(this);
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(BaseStationSettings settings)
        {
            DatabaseFileName =              settings.DatabaseFileName;
            OperatorFlagsFolder =           settings.OperatorFlagsFolder;
            SilhouettesFolder =             settings.SilhouettesFolder;
            PicturesFolder =                settings.PicturesFolder;
            SearchPictureSubFolders =       settings.SearchPictureSubFolders;
            DisplayTimeoutSeconds =         settings.DisplayTimeoutSeconds;
            TrackingTimeoutSeconds =        settings.TrackingTimeoutSeconds;
            MinimiseToSystemTray =          settings.MinimiseToSystemTray;
            AutoSavePolarPlotsMinutes =     settings.AutoSavePolarPlotsMinutes;
            LookupAircraftDetailsOnline =   settings.LookupAircraftDetailsOnline;
        }

        public BaseStationSettings CopyToSettings(BaseStationSettings settings)
        {
            settings.DatabaseFileName =             DatabaseFileName;
            settings.OperatorFlagsFolder =          OperatorFlagsFolder;
            settings.SilhouettesFolder =            SilhouettesFolder;
            settings.PicturesFolder =               PicturesFolder;
            settings.SearchPictureSubFolders =      SearchPictureSubFolders;
            settings.DisplayTimeoutSeconds =        DisplayTimeoutSeconds;
            settings.TrackingTimeoutSeconds =       TrackingTimeoutSeconds;
            settings.MinimiseToSystemTray =         MinimiseToSystemTray;
            settings.AutoSavePolarPlotsMinutes =    AutoSavePolarPlotsMinutes;
            settings.LookupAircraftDetailsOnline =  LookupAircraftDetailsOnline;

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

        public ReceiverLocationModel(ReceiverLocation settings)
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
}
