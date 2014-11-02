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
    /// Displays the site's initial location to the user and lets them change it.
    /// </summary>
    public partial class PageWebSiteGoogleMaps : Page
    {
        #region PageSummary
        public class Summary : PageSummary
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
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageWebSiteGoogleMaps();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageWebSiteGoogleMaps;
                SetValidationFields(new Dictionary<ValidationField,Control>() {
                    { ValidationField.GoogleMapZoomLevel,   page == null ? null : page.numericInitialZoom },
                    { ValidationField.Latitude,             page == null ? null : page.numericInitialLatitude },
                    { ValidationField.Longitude,            page == null ? null : page.numericInitialLongitude },
                });
            }
        }
        #endregion

        // List of allowable map types
        private static readonly string[] _MapTypes = new string[] {
            "HYBRID",
            "ROADMAP",
            "SATELLITE",
            "TERRAIN",
            "HIGHCONTRAST",
        };

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
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            var settings = SettingsView.Configuration.GoogleMapSettings;

            AddControlBinder(new NumericIntBinder<GoogleMapSettings>(settings, numericInitialZoom, r => r.InitialMapZoom, (r,v) => r.InitialMapZoom = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new NumericDoubleBinder<GoogleMapSettings>(settings, numericInitialLatitude,   r => r.InitialMapLatitude,  (r,v) => r.InitialMapLatitude = v)  { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new NumericDoubleBinder<GoogleMapSettings>(settings, numericInitialLongitude,  r => r.InitialMapLongitude, (r,v) => r.InitialMapLongitude = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new ComboBoxValueBinder<GoogleMapSettings, string>(settings, comboBoxInitialMapType, _MapTypes, r => r.InitialMapType, (r,v) => r.InitialMapType = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new MapValuesBinder<GoogleMapSettings>(settings, mapControl,
                r => r.InitialMapLatitude,  (r,v) => r.InitialMapLatitude = v,
                r => r.InitialMapLongitude, (r,v) => r.InitialMapLongitude = v,
                r => r.InitialMapType,      (r,v) => r.InitialMapType = v,
                r => r.InitialMapZoom,      (r,v) => r.InitialMapZoom = v
            ));
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
