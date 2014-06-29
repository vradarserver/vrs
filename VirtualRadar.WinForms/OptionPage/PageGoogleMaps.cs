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
using VirtualRadar.WinForms.Binding;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageGoogleMaps : Page
    {
        public override string PageTitle { get { return Strings.GoogleMaps; } }

        public override Image PageIcon { get { return Images.Site16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        [LocalisedDisplayName("InitialLatitude")]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapLatitude")]
        [ValidationField(ValidationField.Latitude)]
        public Observable<double> InitialGoogleMapLatitude { get; private set; }

        [LocalisedDisplayName("InitialLongitude")]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapLongitude")]
        [ValidationField(ValidationField.Longitude)]
        public Observable<double> InitialGoogleMapLongitude { get; private set; }

        [LocalisedDisplayName("InitialMapType")]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapType")]
        public Observable<string> InitialGoogleMapType { get; private set; }

        [LocalisedDisplayName("InitialZoom")]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapZoom")]
        [ValidationField(ValidationField.GoogleMapZoomLevel)]
        public Observable<int> InitialGoogleMapZoom { get; private set; }

        public PageGoogleMaps()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            InitialGoogleMapLatitude = BindProperty<double>(locationMap, "Latitude");
            InitialGoogleMapLongitude = BindProperty<double>(locationMap, "Longitude");
            InitialGoogleMapType = BindProperty<string>(comboBoxInitialMapType);
            InitialGoogleMapZoom = BindProperty<int>(numericInitialZoom);
        }

        protected override void InitialiseControls()
        {
            comboBoxInitialMapType.PopulateWithCollection<string>(new string[] {
                "HYBRID",
                "ROADMAP",
                "SATELLITE",
                "TERRAIN",
                "HIGHCONTRAST"
            }, r => r);
        }
    }
}
