// Copyright © 2012 onwards, Andrew Whewell
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
using System.Drawing.Design;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.Settings;
using System.Drawing;
using VirtualRadar.Resources;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// A class that holds the web site options for the options view.
    /// </summary>
    class SheetWebSiteOptions : Sheet<SheetWebSiteOptions>
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string SheetTitle { get { return Strings.OptionsWebSiteTitle; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image Icon { get { return Images.Site16x16; } }

        // Category display orders and totals
        private const int GoogleMapCategory = 0;
        private const int WebSiteSettingsCategory = 1;
        private const int WebSiteCustomisationCategory = 2;
        private const int TotalCategories = 3;

        [DisplayOrder(10)]
        [LocalisedDisplayName("InitialLatitude")]
        [LocalisedCategory("GoogleMaps", GoogleMapCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapLatitude")]
        [RaisesValuesChanged]
        public double InitialGoogleMapLatitude { get; set; }
        public bool ShouldSerializeInitialGoogleMapLatitude() { return ValueHasChanged(r => r.InitialGoogleMapLatitude); }

        [DisplayOrder(20)]
        [LocalisedDisplayName("InitialLongitude")]
        [LocalisedCategory("GoogleMaps", GoogleMapCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapLongitude")]
        [RaisesValuesChanged]
        public double InitialGoogleMapLongitude { get; set; }
        public bool ShouldSerializeInitialGoogleMapLongitude() { return ValueHasChanged(r => r.InitialGoogleMapLongitude); }

        [DisplayOrder(30)]
        [LocalisedDisplayName("InitialMapType")]
        [LocalisedCategory("GoogleMaps", GoogleMapCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapType")]
        [TypeConverter(typeof(GoogleMapStyleTypeConverter))]
        public string InitialGoogleMapType { get; set; }
        public bool ShouldSerializeInitialGoogleMapType() { return ValueHasChanged(r => r.InitialGoogleMapType); }

        [DisplayOrder(40)]
        [LocalisedDisplayName("InitialZoom")]
        [LocalisedCategory("GoogleMaps", GoogleMapCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapZoom")]
        [RaisesValuesChanged]
        public int InitialGoogleMapZoom { get; set; }
        public bool ShouldSerializeInitialGoogleMapZoom() { return ValueHasChanged(r => r.InitialGoogleMapZoom); }

        [DisplayOrder(50)]
        [LocalisedDisplayName("InitialRefresh")]
        [LocalisedCategory("OptionsWebSiteSettingsCategory", WebSiteSettingsCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSiteInitialGoogleMapRefreshSeconds")]
        [RaisesValuesChanged]
        public int InitialGoogleMapRefreshSeconds { get; set; }
        public bool ShouldSerializeInitialGoogleMapRefreshSeconds() { return ValueHasChanged(r => r.InitialGoogleMapRefreshSeconds); }

        [DisplayOrder(60)]
        [LocalisedDisplayName("MinimumRefresh")]
        [LocalisedCategory("OptionsWebSiteSettingsCategory", WebSiteSettingsCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSiteMinimumGoogleMapRefreshSeconds")]
        [RaisesValuesChanged]
        public int MinimumGoogleMapRefreshSeconds { get; set; }
        public bool ShouldSerializeMinimumGoogleMapRefreshSeconds() { return ValueHasChanged(r => r.MinimumGoogleMapRefreshSeconds); }

        [DisplayOrder(70)]
        [LocalisedDisplayName("InitialDistanceUnits")]
        [LocalisedCategory("OptionsWebSiteSettingsCategory", WebSiteSettingsCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSiteInitialDistanceUnit")]
        [TypeConverter(typeof(DistanceUnitEnumConverter))]
        public DistanceUnit InitialDistanceUnit { get; set; }
        public bool ShouldSerializeInitialDistanceUnit() { return ValueHasChanged(r => r.InitialDistanceUnit); }

        [DisplayOrder(80)]
        [LocalisedDisplayName("InitialHeightUnits")]
        [LocalisedCategory("OptionsWebSiteSettingsCategory", WebSiteSettingsCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSiteInitialHeightUnit")]
        [TypeConverter(typeof(HeightUnitEnumConverter))]
        public HeightUnit InitialHeightUnit { get; set; }
        public bool ShouldSerializeInitialHeightUnit() { return ValueHasChanged(r => r.InitialHeightUnit); }

        [DisplayOrder(90)]
        [LocalisedDisplayName("InitialSpeedUnits")]
        [LocalisedCategory("OptionsWebSiteSettingsCategory", WebSiteSettingsCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSiteInitialSpeedUnit")]
        [TypeConverter(typeof(SpeedUnitEnumConverter))]
        public SpeedUnit InitialSpeedUnit { get; set; }
        public bool ShouldSerializeInitialSpeedUnit() { return ValueHasChanged(r => r.InitialSpeedUnit); }

        [DisplayOrder(100)]
        [LocalisedDisplayName("PreferIataAirportCodes")]
        [LocalisedCategory("OptionsWebSiteSettingsCategory", WebSiteSettingsCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeWebSitePreferIataAirportCodes")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool PreferIataAirportCodes { get; set; }
        public bool ShouldSerializePreferIataAirportCodes() { return ValueHasChanged(r => r.PreferIataAirportCodes); }

        [DisplayOrder(110)]
        [LocalisedDisplayName("EnableBundling")]
        [LocalisedCategory("OptionsWebSiteCustomisationCategory", WebSiteCustomisationCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeEnableBundling")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool EnableBundling { get; set; }
        public bool ShouldSerializeEnableBundling() { return ValueHasChanged(r => r.EnableBundling); }

        [DisplayOrder(120)]
        [LocalisedDisplayName("EnableMinifying")]
        [LocalisedCategory("OptionsWebSiteCustomisationCategory", WebSiteCustomisationCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeEnableMinifying")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool EnableMinifying { get; set; }
        public bool ShouldSerializeEnableMinifying() { return ValueHasChanged(r => r.EnableMinifying); }

        [DisplayOrder(120)]
        [LocalisedDisplayName("EnableCompression")]
        [LocalisedCategory("OptionsWebSiteCustomisationCategory", WebSiteCustomisationCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeEnableCompression")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool EnableCompression { get; set; }
        public bool ShouldSerializeEnableCompression() { return ValueHasChanged(r => r.EnableCompression); }

        [DisplayOrder(130)]
        [LocalisedDisplayName("ProxyType")]
        [LocalisedCategory("OptionsWebSiteCustomisationCategory", WebSiteCustomisationCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeProxyType")]
        [TypeConverter(typeof(ProxyTypeEnumConverter))]
        public ProxyType ProxyType { get; set; }
        public bool ShouldSerializeProxyType() { return ValueHasChanged(r => r.ProxyType); }
    }
}
