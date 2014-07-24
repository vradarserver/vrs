using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays the site's initial location to the user and lets them change it.
    /// </summary>
    public partial class PageWebSiteGoogleMaps : Page
    {
        public override string PageTitle { get { return Strings.GoogleMaps; } }

        public override Image PageIcon { get { return Images.Site16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageWebSiteGoogleMaps()
        {
            InitializeComponent();
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            comboBoxInitialMapType.DataSource = CreateNameValueSource<string>(new string[] {
                "HYBRID",
                "ROADMAP",
                "SATELLITE",
                "TERRAIN",
                "HIGHCONTRAST",
            }, sortList: false);
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, comboBoxInitialMapType,    r => r.Configuration.GoogleMapSettings.InitialMapType,      r => r.SelectedValue);
            AddBinding(SettingsView, numericInitialZoom,        r => r.Configuration.GoogleMapSettings.InitialMapZoom,      r => r.Value);
            AddBinding(SettingsView, numericInitialLatitude,    r => r.Configuration.GoogleMapSettings.InitialMapLatitude,  r => r.Value, DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(SettingsView, numericInitialLongitude,   r => r.Configuration.GoogleMapSettings.InitialMapLongitude, r => r.Value, DataSourceUpdateMode.OnPropertyChanged);

            var mapBindingSource = new BindingSource();
            mapBindingSource.DataSource = SettingsView.Configuration.GoogleMapSettings;
            bindingMap.DataSource = mapBindingSource;
            bindingMap.LatitudeMember = PropertyHelper.ExtractName<GoogleMapSettings>(r => r.InitialMapLatitude);
            bindingMap.LongitudeMember = PropertyHelper.ExtractName<GoogleMapSettings>(r => r.InitialMapLongitude);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.GoogleMapZoomLevel,   numericInitialZoom },
                { ValidationField.Latitude,             numericInitialLatitude },
                { ValidationField.Longitude,            numericInitialLongitude },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(comboBoxInitialMapType,   Strings.InitialMapType,     Strings.OptionsDescribeWebSiteInitialGoogleMapType);
            SetInlineHelp(numericInitialZoom,       Strings.InitialZoom,        Strings.OptionsDescribeWebSiteInitialGoogleMapZoom);
            SetInlineHelp(numericInitialLatitude,   Strings.InitialLatitude,    Strings.OptionsDescribeWebSiteInitialGoogleMapLatitude);
            SetInlineHelp(numericInitialLongitude,  Strings.InitialLongitude,   Strings.OptionsDescribeWebSiteInitialGoogleMapLongitude);
        }
    }
}
