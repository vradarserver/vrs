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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Lets the user edit the web site settings.
    /// </summary>
    public partial class PageWebSite : Page
    {
        #region PageSummary
        /// <summary>
        /// The page summary object.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = ResourceImages.Site16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override string PageTitle { get { return Strings.OptionsWebSiteTitle; } }

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
                return new PageWebSite();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            protected override void AssociateChildPages()
            {
                base.AssociateChildPages();
                ChildPages.Add(new PageWebSiteInitialSettings.Summary());
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageWebSite;
                SetValidationFields(new Dictionary<ValidationField,Control>() {
                    { ValidationField.InitialGoogleMapRefreshSeconds, page == null ? null : page.numericInitialRefresh },
                    { ValidationField.MinimumGoogleMapRefreshSeconds, page == null ? null : page.numericMinimumRefresh },
                    { ValidationField.AllowCorsDomains,               page == null ? null : page.textBoxAllowCorsDomains },
                });
            }
        }
        #endregion

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageWebSite()
        {
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
            var settings = SettingsView.Configuration.GoogleMapSettings;

            AddControlBinder(new CheckBoxBoolBinder<GoogleMapSettings>(settings, checkBoxPreferIataAirportCodes,            r => r.PreferIataAirportCodes,               (r,v) => r.PreferIataAirportCodes = v));
            AddControlBinder(new CheckBoxBoolBinder<GoogleMapSettings>(settings, checkBoxEnableBundling,                    r => r.EnableBundling,                       (r,v) => r.EnableBundling = v));
            AddControlBinder(new CheckBoxBoolBinder<GoogleMapSettings>(settings, checkBoxEnableMinifying,                   r => r.EnableMinifying,                      (r,v) => r.EnableMinifying = v));
            AddControlBinder(new CheckBoxBoolBinder<GoogleMapSettings>(settings, checkBoxEnableCompression,                 r => r.EnableCompression,                    (r,v) => r.EnableCompression = v));
            AddControlBinder(new CheckBoxBoolBinder<GoogleMapSettings>(settings, checkBoxEnableCorsSupport,                 r => r.EnableCorsSupport,                    (r,v) => r.EnableCorsSupport = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new TextBoxStringBinder<GoogleMapSettings>(settings, textBoxDirectoryEntryKey, r => r.DirectoryEntryKey, (r,v) => r.DirectoryEntryKey = v));
            AddControlBinder(new TextBoxStringBinder<GoogleMapSettings>(settings, textBoxAllowCorsDomains,  r => r.AllowCorsDomains,  (r,v) => r.AllowCorsDomains = v));

            AddControlBinder(new NumericIntBinder<GoogleMapSettings>(settings, numericInitialRefresh,   r => r.InitialRefreshSeconds,   (r,v) => r.InitialRefreshSeconds = v));
            AddControlBinder(new NumericIntBinder<GoogleMapSettings>(settings, numericMinimumRefresh,   r => r.MinimumRefreshSeconds,   (r,v) => r.MinimumRefreshSeconds = v));

            AddControlBinder(new ComboBoxEnumBinder<GoogleMapSettings, DistanceUnit>(settings, comboBoxInitialDistanceUnits, r => r.InitialDistanceUnit, (r,v) => r.InitialDistanceUnit = v,    r => Describe.DistanceUnit(r)));
            AddControlBinder(new ComboBoxEnumBinder<GoogleMapSettings, HeightUnit>  (settings, comboBoxInitialHeightUnits,   r => r.InitialHeightUnit,   (r,v) => r.InitialHeightUnit = v,      r => Describe.HeightUnit(r)));
            AddControlBinder(new ComboBoxEnumBinder<GoogleMapSettings, SpeedUnit>   (settings, comboBoxInitialSpeedUnits,    r => r.InitialSpeedUnit,    (r,v) => r.InitialSpeedUnit = v,       r => Describe.SpeedUnit(r)));
            AddControlBinder(new ComboBoxEnumBinder<GoogleMapSettings, ProxyType>   (settings, comboBoxProxyType,            r => r.ProxyType,           (r,v) => r.ProxyType = v,              r => Describe.ProxyType(r)));
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(numericInitialRefresh,                        Strings.InitialRefresh,                     Strings.OptionsDescribeWebSiteInitialGoogleMapRefreshSeconds);
            SetInlineHelp(numericMinimumRefresh,                        Strings.MinimumRefresh,                     Strings.OptionsDescribeWebSiteMinimumGoogleMapRefreshSeconds);
            SetInlineHelp(comboBoxInitialDistanceUnits,                 Strings.InitialDistanceUnits,               Strings.OptionsDescribeWebSiteInitialDistanceUnit);
            SetInlineHelp(comboBoxInitialHeightUnits,                   Strings.InitialHeightUnits,                 Strings.OptionsDescribeWebSiteInitialHeightUnit);
            SetInlineHelp(comboBoxInitialSpeedUnits,                    Strings.InitialSpeedUnits,                  Strings.OptionsDescribeWebSiteInitialSpeedUnit);
            SetInlineHelp(checkBoxPreferIataAirportCodes,               Strings.PreferIataAirportCodes,             Strings.OptionsDescribeWebSitePreferIataAirportCodes);
            SetInlineHelp(checkBoxEnableCorsSupport,                    Strings.EnableCorsSupport,                  Strings.OptionsDescribeWebSiteEnableCorsSupport);

            SetInlineHelp(comboBoxProxyType,                Strings.ProxyType,          Strings.OptionsDescribeProxyType);
            SetInlineHelp(checkBoxEnableBundling,           Strings.EnableBundling,     Strings.OptionsDescribeEnableBundling);
            SetInlineHelp(checkBoxEnableMinifying,          Strings.EnableMinifying,    Strings.OptionsDescribeEnableMinifying);
            SetInlineHelp(checkBoxEnableCompression,        Strings.EnableCompression,  Strings.OptionsDescribeEnableCompression);

            SetInlineHelp(textBoxDirectoryEntryKey,         Strings.DirectoryEntryKey,  Strings.OptionsDescribeDirectoryEntryKey);
            SetInlineHelp(textBoxAllowCorsDomains,          Strings.AllowedCorsDomains, Strings.OptionsDescribeWebSiteAllowCorsDomains);
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
                    if(args.PropertyName == PropertyHelper.ExtractName<GoogleMapSettings>(r => r.EnableCorsSupport)) {
                        EnableDisableControls();
                    }
                }
            }
        }

        /// <summary>
        /// Enables or disables controls on the form.
        /// </summary>
        private void EnableDisableControls()
        {
            textBoxAllowCorsDomains.Enabled = SettingsView.Configuration.GoogleMapSettings.EnableCorsSupport;
        }
    }
}
