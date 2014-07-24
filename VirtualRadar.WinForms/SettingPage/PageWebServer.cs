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

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Lets the user view and change the web server settings.
    /// </summary>
    public partial class PageWebServer : Page
    {
        public override string PageTitle { get { return Strings.OptionsWebServerSheetTitle; } }

        public override Image PageIcon { get { return Images.Server16x16; } }

        public PageWebServer()
        {
            InitializeComponent();
        }

        protected override void AssociateChildPages()
        {
            base.AssociateChildPages();
            ChildPages.Add(new PageWebServerAuthentication());
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, checkBoxEnableUPnp,                        r => r.Configuration.WebServerSettings.EnableUPnp,                  r => r.Checked);
            AddBinding(SettingsView, checkBoxResetPortAssignmentsOnStartup,     r => r.Configuration.WebServerSettings.IsOnlyInternetServerOnLan,   r => r.Checked);
            AddBinding(SettingsView, checkBoxAutoStartUPnP,                     r => r.Configuration.WebServerSettings.AutoStartUPnP,               r => r.Checked);
            AddBinding(SettingsView, numericUPnpPort,                           r => r.Configuration.WebServerSettings.UPnpPort,                    r => r.Value);

            AddBinding(SettingsView, numericInternetUserIdleTimeout,            r => r.Configuration.InternetClientSettings.TimeoutMinutes,                 r => r.Value);
            AddBinding(SettingsView, checkBoxInternetUsersCanRunReports,        r => r.Configuration.InternetClientSettings.CanRunReports,                  r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetUsersCanListenToAudio,     r => r.Configuration.InternetClientSettings.CanPlayAudio,                   r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetUsersCanViewPictures,      r => r.Configuration.InternetClientSettings.CanShowPictures,                r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetUserCanSeeLabels,          r => r.Configuration.InternetClientSettings.CanShowPinText,                 r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetClientCanSubmitRoutes,     r => r.Configuration.InternetClientSettings.CanSubmitRoutes,                r => r.Checked);
            AddBinding(SettingsView, checkBoxInternetClientCanShowPolarPlots,   r => r.Configuration.InternetClientSettings.CanShowPolarPlots,              r => r.Checked);
            AddBinding(SettingsView, checkBoxAllowInternetProximityGadgets,     r => r.Configuration.InternetClientSettings.AllowInternetProximityGadgets,  r => r.Checked);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.UPnpPortNumber,           numericUPnpPort },
                { ValidationField.InternetUserIdleTimeout,  numericInternetUserIdleTimeout },
            });
        }

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
