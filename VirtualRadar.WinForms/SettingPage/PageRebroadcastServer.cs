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

            AddControlBinder(new CheckBoxBoolBinder<RebroadcastSettings>(RebroadcastSettings, checkBoxEnabled,          r => r.Enabled,         (r,v) => r.Enabled = v));
            AddControlBinder(new CheckBoxBoolBinder<RebroadcastSettings>(RebroadcastSettings, checkBoxIsTransmitter,    r => r.IsTransmitter,   (r,v) => r.IsTransmitter = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new CheckBoxBoolBinder<RebroadcastSettings>(RebroadcastSettings, checkBoxUseKeepAlive,     r => r.UseKeepAlive,    (r,v) => r.UseKeepAlive = v)  { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });


            AddControlBinder(new TextBoxStringBinder<RebroadcastSettings>(RebroadcastSettings, textBoxName,            r => r.Name,             (r,v) => r.Name = v)                { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new TextBoxStringBinder<RebroadcastSettings>(RebroadcastSettings, textBoxTransmitAddress, r => r.TransmitAddress,  (r,v) => r.TransmitAddress = v));
            AddControlBinder(new TextBoxStringBinder<RebroadcastSettings>(RebroadcastSettings, textBoxPassphrase,      r => r.Passphrase,       (r,v) => r.Passphrase = v));

            AddControlBinder(new NumericIntBinder<RebroadcastSettings>(RebroadcastSettings, numericPort,            r => r.Port,                            (r,v) => r.Port = v));
            AddControlBinder(new NumericIntBinder<RebroadcastSettings>(RebroadcastSettings, numericIdleTimeout,     r => r.IdleTimeoutMilliseconds / 1000,  (r,v) => r.IdleTimeoutMilliseconds = v * 1000) { ModelPropertyName = PropertyHelper.ExtractName<RebroadcastSettings>(r => r.IdleTimeoutMilliseconds) });
            AddControlBinder(new NumericIntBinder<RebroadcastSettings>(RebroadcastSettings, numericStaleSeconds,    r => r.StaleSeconds,                    (r,v) => r.StaleSeconds = v));

            AddControlBinder(new ComboBoxBinder<RebroadcastSettings, CombinedFeed, int>(RebroadcastSettings, comboBoxReceiver, SettingsView.CombinedFeed, r => r.ReceiverId, (r,v) => r.ReceiverId = v) { GetListItemDescription = r => r.Name, GetListItemValue = r => r.UniqueId });

            AddControlBinder(new ComboBoxEnumBinder<RebroadcastSettings, RebroadcastFormat> (RebroadcastSettings, comboBoxFormat, r => r.Format, (r,v) => r.Format = v, r => Describe.RebroadcastFormat(r)));

            AddControlBinder(new AccessControlBinder<RebroadcastSettings>(RebroadcastSettings, accessControl, r => r.Access));
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
        }

        /// <summary>
        /// Enables or disables controls.
        /// </summary>
        private void EnableDisableControls()
        {
            textBoxTransmitAddress.Enabled = RebroadcastSettings.IsTransmitter;
            accessControl.Enabled = !RebroadcastSettings.IsTransmitter;
            numericIdleTimeout.Enabled = !RebroadcastSettings.UseKeepAlive;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="args"></param>
        internal override void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
            base.ConfigurationChanged(args);

            if(SettingsView != null && this.IsHandleCreated) {
                if(args.Record == PageObject) {
                    if(args.PropertyName == PropertyHelper.ExtractName<RebroadcastSettings>(r => r.IsTransmitter) ||
                       args.PropertyName == PropertyHelper.ExtractName<Receiver>(r => r.UseKeepAlive)) {
                        EnableDisableControls();
                    }
                }
            }
        }
    }
}
