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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Presents the data sources values to the user.
    /// </summary>
    public partial class PageDataSources : Page
    {
        #region PageSummary
        /// <summary>
        /// The page summary object.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = ResourceImages.Notebook16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override string PageTitle { get { return Strings.OptionsDataSourcesSheetTitle; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return _PageIcon; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageDataSources();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageDataSources;
                SetValidationFields(new Dictionary<ValidationField, Control>() {
                    { ValidationField.BaseStationDatabase,  page == null ? null : page.fileDatabaseFileName },
                    { ValidationField.FlagsFolder,          page == null ? null : page.folderFlags },
                    { ValidationField.SilhouettesFolder,    page == null ? null : page.folderSilhouettes },
                    { ValidationField.PicturesFolder,       page == null ? null : page.folderPictures },
                });
            }
        }
        #endregion

        private ITileServerSettingsManager _TileServerSettingsManager;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageDataSources() : base()
        {
            _TileServerSettingsManager = Factory.ResolveSingleton<ITileServerSettingsManager>();
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            EnableDisableControls();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            var baseStationSettings = SettingsView.Configuration.BaseStationSettings;
            var mapSettings = SettingsView.Configuration.GoogleMapSettings;
            var tileServerSettingNames = _TileServerSettingsManager.GetAllTileServerSettings(MapProvider.Leaflet)
                .OrderBy(r => r.IsDefault && !r.IsCustom ? 0 : 1)
                .ThenBy(r => !r.IsCustom ? 0 : 1)
                .ThenBy(r => r.DisplayOrder)
                .ThenBy(r => (r.Name ?? "").ToLower())
                .Select(r => r.Name)
                .ToArray();

            AddControlBinder(new ComboBoxEnumBinder<GoogleMapSettings, MapProvider>(mapSettings,    comboBoxMapProvider,                                    r => r.MapProvider,             (r,v) => r.MapProvider = v, r => Describe.MapProvider(r)) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new ComboBoxValueBinder<GoogleMapSettings, string>(mapSettings,        comboBoxTileServerSettingsName, tileServerSettingNames, r => r.TileServerSettingName,   (r,v) => r.TileServerSettingName = v));
            AddControlBinder(new TextBoxStringBinder<GoogleMapSettings>(mapSettings,                textBoxGoogleMapsAPIKey,                                r => r.GoogleMapsApiKey,        (r,v) => r.GoogleMapsApiKey = v));

            AddControlBinder(new FileNameStringBinder<BaseStationSettings>(baseStationSettings, fileDatabaseFileName,    r => r.DatabaseFileName,   (r,v) => r.DatabaseFileName = v));

            AddControlBinder(new FolderStringBinder<BaseStationSettings>(baseStationSettings, folderFlags,          r => r.OperatorFlagsFolder,    (r,v) => r.OperatorFlagsFolder = v));
            AddControlBinder(new FolderStringBinder<BaseStationSettings>(baseStationSettings, folderSilhouettes,    r => r.SilhouettesFolder,      (r,v) => r.SilhouettesFolder = v));
            AddControlBinder(new FolderStringBinder<BaseStationSettings>(baseStationSettings, folderPictures,       r => r.PicturesFolder,         (r,v) => r.PicturesFolder = v));

            AddControlBinder(new CheckBoxBoolBinder<GoogleMapSettings>(mapSettings,           checkBoxUseGoogleMapsKeyWithLocalRequests,    r => r.UseGoogleMapsAPIKeyWithLocalRequests, (r,v) => r.UseGoogleMapsAPIKeyWithLocalRequests = v));
            AddControlBinder(new CheckBoxBoolBinder<BaseStationSettings>(baseStationSettings, checkBoxSearchPictureSubFolders,              r => r.SearchPictureSubFolders,              (r,v) => r.SearchPictureSubFolders = v));
            AddControlBinder(new CheckBoxBoolBinder<BaseStationSettings>(baseStationSettings, checkBoxLookupAircraftDetailsOnline,          r => r.LookupAircraftDetailsOnline,          (r,v) => r.LookupAircraftDetailsOnline = v));
            AddControlBinder(new CheckBoxBoolBinder<BaseStationSettings>(baseStationSettings, checkBoxDownloadWeather,                      r => r.DownloadGlobalAirPressureReadings,    (r,v) => r.DownloadGlobalAirPressureReadings = v));

            AddControlBinder(new LabelStringBinder<SettingsView>(SettingsView, labelAircraftLookupDataProvider,     r => r.AircraftOnlineLookupDataSupplier,        (r,v) => {;}));
            AddControlBinder(new LabelStringBinder<SettingsView>(SettingsView, labelAircraftLookupSupplierCredits,  r => r.AircraftOnlineLookupDataSupplierCredits, (r,v) => {;}));

            AddControlBinder(new LinkLabelStringBinder<SettingsView>(SettingsView, linkLabelAircraftLookupSupplierUrl, r => r.AircraftOnlineLookupDataSupplierUrl, (r,v) => {;}));
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(fileDatabaseFileName,                      Strings.DatabaseFileName,                   Strings.OptionsDescribeDataSourcesDatabaseFileName);
            SetInlineHelp(folderFlags,                               Strings.FlagsFolder,                        Strings.OptionsDescribeDataSourcesFlagsFolder);
            SetInlineHelp(folderSilhouettes,                         Strings.SilhouettesFolder,                  Strings.OptionsDescribeDataSourcesSilhouettesFolder);
            SetInlineHelp(folderPictures,                            Strings.PicturesFolder,                     Strings.OptionsDescribeDataSourcesPicturesFolder);
            SetInlineHelp(checkBoxSearchPictureSubFolders,           Strings.SearchPictureSubFolders,            Strings.OptionsDescribeDataSourcesSearchPictureSubFolders);
            SetInlineHelp(checkBoxLookupAircraftDetailsOnline,       Strings.LookupAircraftDetailsOnline,        Strings.OptionsDescribeDataSourcesLookupAircraftDetailsOnline);
            SetInlineHelp(checkBoxDownloadWeather,                   Strings.DownloadGlobalAirPressureReadings,  Strings.OptionsDescribeDownloadGlobalAirPressureReadings);
            SetInlineHelp(checkBoxUseGoogleMapsKeyWithLocalRequests, Strings.UseGoogleMapsKeyWithLocalRequests,  Strings.OptionsDescribeUseGoogleMapsKeyWithLocalRequests);
            SetInlineHelp(textBoxGoogleMapsAPIKey,                   Strings.GoogleMapsAPIKey,                   Strings.OptionsDescribeGoogleMapsAPIKey);
        }

        /// <summary>
        /// Called when the user clicks the aircraft online supplier URL.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelAircraftLookupSupplierUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SettingsView.RaiseOpenUrlClicked(new EventArgs<string>(linkLabelAircraftLookupSupplierUrl.Text));
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="args"></param>
        internal override void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
            base.ConfigurationChanged(args);
            if(SettingsView != null && IsHandleCreated) {
                if(args.Group == ConfigurationListenerGroup.GoogleMapSettings) {
                    EnableDisableControls();
                }
            }
        }

        private void EnableDisableControls()
        {
            textBoxGoogleMapsAPIKey.Enabled = SettingsView.Configuration.GoogleMapSettings.MapProvider == MapProvider.GoogleMaps;
            checkBoxUseGoogleMapsKeyWithLocalRequests.Enabled = SettingsView.Configuration.GoogleMapSettings.MapProvider == MapProvider.GoogleMaps;
            comboBoxTileServerSettingsName.Enabled = _TileServerSettingsManager.MapProviderUsesTileServers(SettingsView.Configuration.GoogleMapSettings.MapProvider);
        }
    }
}
