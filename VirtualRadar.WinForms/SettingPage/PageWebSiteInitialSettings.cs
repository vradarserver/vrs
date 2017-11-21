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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays the initial settings for the web site to the user and lets them change them.
    /// </summary>
    public partial class PageWebSiteInitialSettings : Page
    {
        #region PageSummary
        /// <summary>
        /// Describes the content of the initial site settings page.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = Images.Site16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override string PageTitle { get { return Strings.InitialSettingsTitle; } }

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
                return new PageWebSiteInitialSettings();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageWebSiteInitialSettings;
                SetValidationFields(new Dictionary<ValidationField,Control>() {
                    { ValidationField.ExportedSettings, page == null ? null : page.textBoxInitialSettings },
                });
            }
        }
        #endregion

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool PageUseFullHeight { get { return true; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageWebSiteInitialSettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void InitialiseControls()
        {
            base.InitialiseControls();

            var webServer = Factory.Singleton.ResolveSingleton<IAutoConfigWebServer>().WebServer;
            linkLabelDesktopSite.Text = FormatAddress(webServer.LocalAddress, "desktop.html");
            linkLabelMobileSite.Text = FormatAddress(webServer.LocalAddress, "mobile.html");
            linkLabelSettingsPage.Text = FormatAddress(webServer.LocalAddress, "settings.html");
        }

        /// <summary>
        /// Returns a formatted URL to a page on the site.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        private string FormatAddress(string address, string page)
        {
            return String.Format("{0}{1}{2}", address, address.EndsWith("/") ? "" : "/", page);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            var settings = SettingsView.Configuration.GoogleMapSettings;

            AddControlBinder(new TextBoxStringBinder<GoogleMapSettings>(settings, textBoxInitialSettings, r => r.InitialSettings, (r,v) => r.InitialSettings = v));
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(textBoxInitialSettings, Strings.ExportedSettings, Strings.OptionsDescribeWebSiteInitialGoogleMapSettings);
        }

        /// <summary>
        /// Called whent the user clicks one of the site links.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var linkLabel = (LinkLabel)sender;
            Process.Start(linkLabel.Text);
        }

        /// <summary>
        /// Called when the user clicks the Copy from Clipboard link.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelCopyFromClipboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try {
                var text = Clipboard.ContainsText(TextDataFormat.UnicodeText) ? Clipboard.GetText(TextDataFormat.UnicodeText) : "";
                SettingsView.Configuration.GoogleMapSettings.InitialSettings = text;
            } catch {
            }
        }

        /// <summary>
        /// Called when the user clicks the Copy to Clipboard link.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelCopyToClipboard_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try {
                Clipboard.SetText(SettingsView.Configuration.GoogleMapSettings.InitialSettings);
            } catch {
            }
        }
    }
}
