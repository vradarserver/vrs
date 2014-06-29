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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageWebSite : Page
    {
        public override string PageTitle { get { return Strings.OptionsWebSiteTitle; } }

        public override Image PageIcon { get { return Images.Site16x16; } }

        public PageGoogleMaps PageGoogleMaps { get; private set; }

        [LocalisedDisplayName("InitialRefresh")]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapRefreshSeconds")]
        [ValidationField(ValidationField.InitialGoogleMapRefreshSeconds)]
        public Observable<int> InitialGoogleMapRefreshSeconds { get; private set; }

        [LocalisedDisplayName("MinimumRefresh")]
        [LocalisedDescription("OptionsDescribeWebSiteMinimumGoogleMapRefreshSeconds")]
        [ValidationField(ValidationField.MinimumGoogleMapRefreshSeconds)]
        public Observable<int> MinimumGoogleMapRefreshSeconds { get; private set; }

        [LocalisedDisplayName("InitialDistanceUnits")]
        [LocalisedDescription("OptionsDescribeWebSiteInitialDistanceUnit")]
        public Observable<DistanceUnit> InitialDistanceUnit { get; private set; }

        [LocalisedDisplayName("InitialHeightUnits")]
        [LocalisedDescription("OptionsDescribeWebSiteInitialHeightUnit")]
        public Observable<HeightUnit> InitialHeightUnit { get; private set; }

        [LocalisedDisplayName("InitialSpeedUnits")]
        [LocalisedDescription("OptionsDescribeWebSiteInitialSpeedUnit")]
        public Observable<SpeedUnit> InitialSpeedUnit { get; private set; }

        [LocalisedDisplayName("PreferIataAirportCodes")]
        [LocalisedDescription("OptionsDescribeWebSitePreferIataAirportCodes")]
        public Observable<bool> PreferIataAirportCodes { get; private set; }

        [LocalisedDisplayName("EnableBundling")]
        [LocalisedDescription("OptionsDescribeEnableBundling")]
        public Observable<bool> EnableBundling { get; private set; }

        [LocalisedDisplayName("EnableMinifying")]
        [LocalisedDescription("OptionsDescribeEnableMinifying")]
        public Observable<bool> EnableMinifying { get; private set; }

        [LocalisedDisplayName("EnableCompression")]
        [LocalisedDescription("OptionsDescribeEnableCompression")]
        public Observable<bool> EnableCompression { get; private set; }

        [LocalisedDisplayName("ProxyType")]
        [LocalisedDescription("OptionsDescribeProxyType")]
        public Observable<ProxyType> ProxyType { get; private set; }

        public PageWebSite()
        {
            PageGoogleMaps = new PageGoogleMaps();
            ChildPages.Add(PageGoogleMaps);

            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            InitialGoogleMapRefreshSeconds =    BindProperty<int>(numericInitialRefresh);
            MinimumGoogleMapRefreshSeconds =    BindProperty<int>(numericMinimumRefresh);
            InitialDistanceUnit =               BindProperty<DistanceUnit>(comboBoxInitialDistanceUnits);
            InitialHeightUnit =                 BindProperty<HeightUnit>(comboBoxInitialHeightUnits);
            InitialSpeedUnit =                  BindProperty<SpeedUnit>(comboBoxInitialSpeedUnits);
            PreferIataAirportCodes =            BindProperty<bool>(checkBoxPreferIataAirportCodes);

            EnableBundling =                    BindProperty<bool>(checkBoxEnableBundling);
            EnableMinifying =                   BindProperty<bool>(checkBoxEnableMinifying);
            EnableCompression =                 BindProperty<bool>(checkBoxEnableCompression);
            ProxyType =                         BindProperty<ProxyType>(comboBoxProxyType);
        }

        protected override void InitialiseControls()
        {
            comboBoxInitialDistanceUnits.PopulateWithEnums<DistanceUnit>(r => Describe.DistanceUnit(r));
            comboBoxInitialHeightUnits.PopulateWithEnums<HeightUnit>(r => Describe.HeightUnit(r));
            comboBoxInitialSpeedUnits.PopulateWithEnums<SpeedUnit>(r => Describe.SpeedUnit(r));
            comboBoxProxyType.PopulateWithEnums<ProxyType>(r => Describe.ProxyType(r));
        }
    }
}
