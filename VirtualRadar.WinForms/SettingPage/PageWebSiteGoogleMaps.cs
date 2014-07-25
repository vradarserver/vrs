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
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string PageTitle { get { return Strings.GoogleMaps; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image PageIcon { get { return Images.Site16x16; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool PageUseFullHeight { get { return true; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageWebSiteGoogleMaps()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
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

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, comboBoxInitialMapType,    r => r.Configuration.GoogleMapSettings.InitialMapType,      r => r.SelectedValue,   DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(SettingsView, numericInitialZoom,        r => r.Configuration.GoogleMapSettings.InitialMapZoom,      r => r.Value,           DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(SettingsView, numericInitialLatitude,    r => r.Configuration.GoogleMapSettings.InitialMapLatitude,  r => r.Value,           DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(SettingsView, numericInitialLongitude,   r => r.Configuration.GoogleMapSettings.InitialMapLongitude, r => r.Value,           DataSourceUpdateMode.OnPropertyChanged);

            var mapBindingSource = new BindingSource();
            mapBindingSource.DataSource = SettingsView.Configuration.GoogleMapSettings;
            bindingMap.DataSource = mapBindingSource;
            bindingMap.MapTypeMember = PropertyHelper.ExtractName<GoogleMapSettings>(r => r.InitialMapType);
            bindingMap.ZoomLevelMember = PropertyHelper.ExtractName<GoogleMapSettings>(r => r.InitialMapZoom);
            bindingMap.LatitudeMember = PropertyHelper.ExtractName<GoogleMapSettings>(r => r.InitialMapLatitude);
            bindingMap.LongitudeMember = PropertyHelper.ExtractName<GoogleMapSettings>(r => r.InitialMapLongitude);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.GoogleMapZoomLevel,   numericInitialZoom },
                { ValidationField.Latitude,             numericInitialLatitude },
                { ValidationField.Longitude,            numericInitialLongitude },
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
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
