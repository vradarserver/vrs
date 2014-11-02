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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Lets the user view and change the web server settings.
    /// </summary>
    public partial class PageWebServer : Page
    {
        #region PageSummary
        /// <summary>
        /// The page summary object.
        /// </summary>
        public class Summary : PageSummary
        {
            /// <summary>
            /// See base docs.
            /// </summary>
            public override string PageTitle { get { return Strings.OptionsWebServerSheetTitle; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return Images.Server16x16; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageWebServer();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            protected override void AssociateChildPages()
            {
                base.AssociateChildPages();
                ChildPages.Add(new PageWebServerAuthentication.Summary());
            }
        }
        #endregion

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageWebServer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            var config = SettingsView.Configuration;

            AddControlBinder(new NumericIntBinder<WebServerSettings>        (config.WebServerSettings,      numericUPnpPort,                r => r.UPnpPort,        (r,v) => r.UPnpPort = v));
            AddControlBinder(new NumericIntBinder<InternetClientSettings>   (config.InternetClientSettings, numericInternetUserIdleTimeout, r => r.TimeoutMinutes,  (r,v) => r.TimeoutMinutes = v));

            AddControlBinder(new CheckBoxBoolBinder<WebServerSettings>      (config.WebServerSettings,      checkBoxEnableUPnp,                        r => r.EnableUPnp,                       (r,v) => r.EnableUPnp = v));
            AddControlBinder(new CheckBoxBoolBinder<WebServerSettings>      (config.WebServerSettings,      checkBoxResetPortAssignmentsOnStartup,     r => r.IsOnlyInternetServerOnLan,        (r,v) => r.IsOnlyInternetServerOnLan = v));
            AddControlBinder(new CheckBoxBoolBinder<WebServerSettings>      (config.WebServerSettings,      checkBoxAutoStartUPnP,                     r => r.AutoStartUPnP,                    (r,v) => r.AutoStartUPnP = v));
            AddControlBinder(new CheckBoxBoolBinder<InternetClientSettings> (config.InternetClientSettings, checkBoxInternetUsersCanRunReports,        r => r.CanRunReports,                    (r,v) => r.CanRunReports = v));
            AddControlBinder(new CheckBoxBoolBinder<InternetClientSettings> (config.InternetClientSettings, checkBoxInternetUsersCanListenToAudio,     r => r.CanPlayAudio,                     (r,v) => r.CanPlayAudio = v));
            AddControlBinder(new CheckBoxBoolBinder<InternetClientSettings> (config.InternetClientSettings, checkBoxInternetUsersCanViewPictures,      r => r.CanShowPictures,                  (r,v) => r.CanShowPictures = v));
            AddControlBinder(new CheckBoxBoolBinder<InternetClientSettings> (config.InternetClientSettings, checkBoxInternetUserCanSeeLabels,          r => r.CanShowPinText,                   (r,v) => r.CanShowPinText = v));
            AddControlBinder(new CheckBoxBoolBinder<InternetClientSettings> (config.InternetClientSettings, checkBoxInternetClientCanSubmitRoutes,     r => r.CanSubmitRoutes,                  (r,v) => r.CanSubmitRoutes = v));
            AddControlBinder(new CheckBoxBoolBinder<InternetClientSettings> (config.InternetClientSettings, checkBoxInternetClientCanShowPolarPlots,   r => r.CanShowPolarPlots,                (r,v) => r.CanShowPolarPlots = v));
            AddControlBinder(new CheckBoxBoolBinder<InternetClientSettings> (config.InternetClientSettings, checkBoxAllowInternetProximityGadgets,     r => r.AllowInternetProximityGadgets,    (r,v) => r.AllowInternetProximityGadgets = v));
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.UPnpPortNumber,           numericUPnpPort },
                { ValidationField.InternetUserIdleTimeout,  numericInternetUserIdleTimeout },
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxEnableUPnp,                       Strings.EnableUPnp,                     Strings.OptionsDescribeWebServerEnableUPnpFeatures);
            SetInlineHelp(checkBoxResetPortAssignmentsOnStartup,    Strings.ResetPortAssignmentsOnStartup,  Strings.OptionsDescribeWebServerIsOnlyVirtualRadarServerOnLan);
            SetInlineHelp(checkBoxAutoStartUPnP,                    Strings.AutoStartUPnP,                  Strings.OptionsDescribeWebServerAutoStartUPnp);
            SetInlineHelp(numericUPnpPort,                          Strings.UPnpPort,                       Strings.OptionsDescribeWebServerUPnpPort);

            SetInlineHelp(numericInternetUserIdleTimeout,           Strings.IdleTimeout,                        Strings.OptionsDescribeWebServerInternetClientTimeoutMinutes);
            SetInlineHelp(checkBoxInternetUsersCanRunReports,       Strings.InternetUsersCanRunReports,         Strings.OptionsDescribeWebServerInternetClientCanRunReports);
            SetInlineHelp(checkBoxInternetUsersCanListenToAudio,    Strings.InternetUsersCanListenToAudio,      Strings.OptionsDescribeWebServerInternetClientCanPlayAudio);
            SetInlineHelp(checkBoxInternetUsersCanViewPictures,     Strings.InternetUsersCanViewPictures,       Strings.OptionsDescribeWebServerInternetClientCanSeePictures);
            SetInlineHelp(checkBoxInternetUserCanSeeLabels,         Strings.InternetUserCanSeeLabels,           Strings.OptionsDescribeWebServerInternetClientCanSeeLabels);
            SetInlineHelp(checkBoxInternetClientCanSubmitRoutes,    Strings.InternetClientCanSubmitRoutes,      Strings.OptionsDescribeWebServerInternetClientCanSubmitRoutes);
            SetInlineHelp(checkBoxInternetClientCanShowPolarPlots,  Strings.InternetClientCanShowPolarPlots,    Strings.OptionsDescribeWebServerInternetClientCanShowPolarPlots);
            SetInlineHelp(checkBoxAllowInternetProximityGadgets,    Strings.AllowInternetProximityGadgets,      Strings.OptionsDescribeWebServerAllowInternetProximityGadgets);
        }
    }
}
