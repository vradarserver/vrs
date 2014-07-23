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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Lets the user edit the web site settings.
    /// </summary>
    public partial class PageWebSite : Page
    {
        public override string PageTitle { get { return Strings.OptionsWebSiteTitle; } }

        public override Image PageIcon { get { return Images.Site16x16; } }

        public PageWebSite()
        {
            InitializeComponent();
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            comboBoxInitialDistanceUnits.DataSource =   CreateSortingEnumSource<DistanceUnit>(r => Describe.DistanceUnit(r));
            comboBoxInitialHeightUnits.DataSource =     CreateSortingEnumSource<HeightUnit>(r => Describe.HeightUnit(r));
            comboBoxInitialSpeedUnits.DataSource =      CreateSortingEnumSource<SpeedUnit>(r => Describe.SpeedUnit(r));
            comboBoxProxyType.DataSource =              CreateSortingEnumSource<ProxyType>(r => Describe.ProxyType(r));
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, numericInitialRefresh,             r => r.Configuration.GoogleMapSettings.InitialRefreshSeconds,   r => r.Value);
            AddBinding(SettingsView, numericMinimumRefresh,             r => r.Configuration.GoogleMapSettings.MinimumRefreshSeconds,   r => r.Value);
            AddBinding(SettingsView, comboBoxInitialDistanceUnits,      r => r.Configuration.GoogleMapSettings.InitialDistanceUnit,     r => r.SelectedValue);
            AddBinding(SettingsView, comboBoxInitialHeightUnits,        r => r.Configuration.GoogleMapSettings.InitialHeightUnit,       r => r.SelectedValue);
            AddBinding(SettingsView, comboBoxInitialSpeedUnits,         r => r.Configuration.GoogleMapSettings.InitialSpeedUnit,        r => r.SelectedValue);
            AddBinding(SettingsView, checkBoxPreferIataAirportCodes,    r => r.Configuration.GoogleMapSettings.PreferIataAirportCodes,  r => r.Checked);

            AddBinding(SettingsView, comboBoxProxyType,                 r => r.Configuration.GoogleMapSettings.ProxyType,           r => r.SelectedValue);
            AddBinding(SettingsView, checkBoxEnableBundling,            r => r.Configuration.GoogleMapSettings.EnableBundling,      r => r.Checked);
            AddBinding(SettingsView, checkBoxEnableMinifying,           r => r.Configuration.GoogleMapSettings.EnableMinifying,     r => r.Checked);
            AddBinding(SettingsView, checkBoxEnableCompression,         r => r.Configuration.GoogleMapSettings.EnableCompression,   r => r.Checked);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.InitialGoogleMapRefreshSeconds, numericInitialRefresh },
                { ValidationField.MinimumGoogleMapRefreshSeconds, numericMinimumRefresh },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(numericInitialRefresh,            Strings.InitialRefresh,         Strings.OptionsDescribeWebSiteInitialGoogleMapRefreshSeconds);
            SetInlineHelp(numericMinimumRefresh,            Strings.MinimumRefresh,         Strings.OptionsDescribeWebSiteMinimumGoogleMapRefreshSeconds);
            SetInlineHelp(comboBoxInitialDistanceUnits,     Strings.InitialDistanceUnits,   Strings.OptionsDescribeWebSiteInitialDistanceUnit);
            SetInlineHelp(comboBoxInitialHeightUnits,       Strings.InitialHeightUnits,     Strings.OptionsDescribeWebSiteInitialHeightUnit);
            SetInlineHelp(comboBoxInitialSpeedUnits,        Strings.InitialSpeedUnits,      Strings.OptionsDescribeWebSiteInitialSpeedUnit);
            SetInlineHelp(checkBoxPreferIataAirportCodes,   Strings.PreferIataAirportCodes, Strings.OptionsDescribeWebSitePreferIataAirportCodes);

            SetInlineHelp(comboBoxProxyType,                Strings.ProxyType,          Strings.OptionsDescribeProxyType);
            SetInlineHelp(checkBoxEnableBundling,           Strings.EnableBundling,     Strings.OptionsDescribeEnableBundling);
            SetInlineHelp(checkBoxEnableMinifying,          Strings.EnableMinifying,    Strings.OptionsDescribeEnableMinifying);
            SetInlineHelp(checkBoxEnableCompression,        Strings.EnableCompression,  Strings.OptionsDescribeEnableCompression);
        }
    }
}
