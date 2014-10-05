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
using VirtualRadar.Resources;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays the settings for an individual rebroadcast server.
    /// </summary>
    public partial class PageRebroadcastServer : Page
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image PageIcon { get { return Images.Rebroadcast16x16; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public RebroadcastSettings RebroadcastSettings { get { return PageObject as RebroadcastSettings; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool PageUseFullHeight { get { return true; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageRebroadcastServer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        internal override bool IsForSameRecord(object record)
        {
            var rebroadcastSettings = record as RebroadcastSettings;
            return rebroadcastSettings != null && RebroadcastSettings != null && rebroadcastSettings.UniqueId == RebroadcastSettings.UniqueId;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            comboBoxReceiver.DataSource =       CreateSortingBindingSource<CombinedFeed>(SettingsView.CombinedFeed, r => r.Name);
            comboBoxFormat.DataSource =         CreateSortingEnumSource<RebroadcastFormat>(r => Describe.RebroadcastFormat(r));
            comboBoxDefaultAccess.DataSource =  CreateSortingEnumSource<DefaultAccess>(r => Describe.DefaultAccess(r));

            SetPageTitleProperty<RebroadcastSettings>(r => r.Name, () => RebroadcastSettings.Name);
            SetPageEnabledProperty<RebroadcastSettings>(r => r.Enabled, () => RebroadcastSettings.Enabled);

            EnableDisableControls();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(RebroadcastSettings, checkBoxEnabled,        r => r.Enabled,                 r => r.Checked);
            AddBinding(RebroadcastSettings, textBoxName,            r => r.Name,                    r => r.Text,    DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(RebroadcastSettings, comboBoxReceiver,       r => r.ReceiverId,              r => r.SelectedValue);
            AddBinding(RebroadcastSettings, comboBoxFormat,         r => r.Format,                  r => r.SelectedValue);
            AddBinding(RebroadcastSettings, checkBoxIsTransmitter,  r => r.IsTransmitter,           r => r.Checked, DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(RebroadcastSettings, textBoxTransmitAddress, r => r.TransmitAddress,         r => r.Text);
            AddBinding(RebroadcastSettings, numericPort,            r => r.Port,                    r => r.Value);
            AddBinding(RebroadcastSettings, textBoxPassphrase,      r => r.Passphrase,              r => r.Text);
            AddBinding(RebroadcastSettings, checkBoxUseKeepAlive,   r => r.UseKeepAlive,            r => r.Checked, DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(RebroadcastSettings, numericIdleTimeout,     r => r.IdleTimeoutMilliseconds, r => r.Value, format: MillisecondsToSeconds_Format, parse: MillisecondsToSeconds_Parse);
            AddBinding(RebroadcastSettings, numericStaleSeconds,    r => r.StaleSeconds,            r => r.Value);

            AddBinding(RebroadcastSettings.Access, comboBoxDefaultAccess, r => r.DefaultAccess, r => r.SelectedValue, dataSourceUpdateMode: DataSourceUpdateMode.OnPropertyChanged);
            bindingCidrList.DataSource = RebroadcastSettings.Access.Addresses;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.Name,                     textBoxName },
                { ValidationField.IsTransmitter,            checkBoxIsTransmitter },
                { ValidationField.BaseStationAddress,       textBoxTransmitAddress },
                { ValidationField.RebroadcastServerPort,    numericPort },
                { ValidationField.UseKeepAlive,             checkBoxUseKeepAlive },
                { ValidationField.IdleTimeout,              numericIdleTimeout },
                { ValidationField.Format,                   comboBoxFormat },
                { ValidationField.RebroadcastReceiver,      comboBoxReceiver },
                { ValidationField.StaleSeconds,             numericStaleSeconds },
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxEnabled,          Strings.Enabled,        Strings.OptionsDescribeRebroadcastServerEnabled);
            SetInlineHelp(textBoxName,              Strings.Name,           Strings.OptionsDescribeRebroadcastServerName);
            SetInlineHelp(comboBoxReceiver,         Strings.Receiver,       Strings.OptionsDescribeRebroadcastReceiver);
            SetInlineHelp(comboBoxFormat,           Strings.Format,         Strings.OptionsDescribeRebroadcastServerFormat);
            SetInlineHelp(checkBoxIsTransmitter,    Strings.TransmitFeed,   Strings.OptionsDescribeRebroadcastIsTransmitter);
            SetInlineHelp(textBoxTransmitAddress,   Strings.UNC,            Strings.OptionsDescribeRebroadcastTransmitAddress);
            SetInlineHelp(numericPort,              Strings.Port,           Strings.OptionsDescribeRebroadcastServerPort);
            SetInlineHelp(textBoxPassphrase,        Strings.Passphrase,     Strings.OptionsDescribePassphrase);
            SetInlineHelp(checkBoxUseKeepAlive,     Strings.UseKeepAlive,   Strings.OptionsDescribeRebroadcastUseKeepAlive);
            SetInlineHelp(numericIdleTimeout,       Strings.IdleTimeout,    Strings.OptionsDescribeRebroadcastIdleTimeout);
            SetInlineHelp(numericStaleSeconds,      Strings.StaleSeconds,   Strings.OptionsDescribeRebroadcastStaleSeconds);
            SetInlineHelp(comboBoxDefaultAccess,    Strings.DefaultAccess,  Strings.OptionsDescribeDefaultAccess);
        }

        /// <summary>
        /// Called when the user changes the default access.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxDefaultAccess_SelectedIndexChanged(object sender, EventArgs e)
        {
            var access = RebroadcastSettings.Access.DefaultAccess;
            bindingCidrList.Enabled = access != DefaultAccess.Unrestricted;
            labelCidrList.Text = String.Format("{0}:", access == DefaultAccess.Allow ? Strings.DenyTheseAddresses : Strings.AllowTheseAddresses);
        }

        /// <summary>
        /// Enables or disables controls.
        /// </summary>
        private void EnableDisableControls()
        {
            textBoxTransmitAddress.Enabled = RebroadcastSettings.IsTransmitter;
            groupBoxAccessControl.Enabled = !RebroadcastSettings.IsTransmitter;
            numericIdleTimeout.Enabled = !RebroadcastSettings.UseKeepAlive;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="args"></param>
        internal override void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
            base.ConfigurationChanged(args);
            if(args.Record == PageObject && SettingsView != null && this.IsHandleCreated) {
                if(args.PropertyName == PropertyHelper.ExtractName<RebroadcastSettings>(r => r.IsTransmitter) ||
                   args.PropertyName == PropertyHelper.ExtractName<Receiver>(r => r.UseKeepAlive)) {
                    EnableDisableControls();
                }
            }
        }
    }
}
