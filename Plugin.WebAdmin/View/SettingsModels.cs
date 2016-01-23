using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Plugin.WebAdmin.View.Settings
{
    public class ViewModel
    {
        public ConfigurationModel Configuration { get; set; }

        public ViewModel(Configuration configuration)
        {
            Configuration = new ConfigurationModel(configuration);
        }
    }

    public class ConfigurationModel
    {
        public int DataVersion { get; set; }

        public BaseStationSettingsModel BaseStationSettingsModel { get; private set; }

        public ConfigurationModel()
        {
            BaseStationSettingsModel = new BaseStationSettingsModel();
        }

        public ConfigurationModel(Configuration configuration) : this()
        {
            RefreshFromConfiguration(configuration);
        }

        public void RefreshFromConfiguration(Configuration configuration)
        {
            DataVersion = configuration.DataVersion;

            BaseStationSettingsModel.RefreshFromSettings(configuration.BaseStationSettings);
        }

        public void CopyToConfiguration(Configuration configuration)
        {
            configuration.DataVersion = DataVersion;

            BaseStationSettingsModel.CopyToSettings(configuration.BaseStationSettings);
        }
    }

    public class BaseStationSettingsModel
    {
        public string DatabaseFileName { get; set; }

        public string OperatorFlagsFolder { get; set; }

        public string SilhouettesFolder { get; set; }

        public string OutlinesFolder { get; set; }

        public string PicturesFolder { get; set; }

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
            RefreshFromSettings(settings);
        }

        public void RefreshFromSettings(BaseStationSettings settings)
        {
            DatabaseFileName =              settings.DatabaseFileName;
            OperatorFlagsFolder =           settings.OperatorFlagsFolder;
            SilhouettesFolder =             settings.SilhouettesFolder;
            OutlinesFolder =                settings.OutlinesFolder;
            PicturesFolder =                settings.PicturesFolder;
            SearchPictureSubFolders =       settings.SearchPictureSubFolders;
            DisplayTimeoutSeconds =         settings.DisplayTimeoutSeconds;
            TrackingTimeoutSeconds =        settings.TrackingTimeoutSeconds;
            MinimiseToSystemTray =          settings.MinimiseToSystemTray;
            AutoSavePolarPlotsMinutes =     settings.AutoSavePolarPlotsMinutes;
            LookupAircraftDetailsOnline =   settings.LookupAircraftDetailsOnline;
        }

        public void CopyToSettings(BaseStationSettings settings)
        {
            settings.DatabaseFileName =             DatabaseFileName;
            settings.OperatorFlagsFolder =          OperatorFlagsFolder;
            settings.SilhouettesFolder =            SilhouettesFolder;
            settings.OutlinesFolder =               OutlinesFolder;
            settings.PicturesFolder =               PicturesFolder;
            settings.SearchPictureSubFolders =      SearchPictureSubFolders;
            settings.DisplayTimeoutSeconds =        DisplayTimeoutSeconds;
            settings.TrackingTimeoutSeconds =       TrackingTimeoutSeconds;
            settings.MinimiseToSystemTray =         MinimiseToSystemTray;
            settings.AutoSavePolarPlotsMinutes =    AutoSavePolarPlotsMinutes;
            settings.LookupAircraftDetailsOnline =  LookupAircraftDetailsOnline;
        }
    }
}
