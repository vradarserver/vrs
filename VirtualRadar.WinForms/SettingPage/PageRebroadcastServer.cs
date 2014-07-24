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
        public override Image PageIcon { get { return Images.Rebroadcast16x16; } }

        public RebroadcastSettings RebroadcastSettings { get { return PageObject as RebroadcastSettings; } }

        public PageRebroadcastServer()
        {
            InitializeComponent();
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            comboBoxReceiver.DataSource =   CreateSortingBindingSource<CombinedFeed>(SettingsView.CombinedFeed, r => r.Name);
            comboBoxFormat.DataSource =     CreateSortingEnumSource<RebroadcastFormat>(r => Describe.RebroadcastFormat(r));

            SetPageTitleProperty<RebroadcastSettings>(r => r.Name, () => RebroadcastSettings.Name);
            SetPageEnabledProperty<RebroadcastSettings>(r => r.Enabled, () => RebroadcastSettings.Enabled);
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(RebroadcastSettings, checkBoxEnabled,        r => r.Enabled,         r => r.Checked);
            AddBinding(RebroadcastSettings, textBoxName,            r => r.Name,            r => r.Text,    DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(RebroadcastSettings, comboBoxReceiver,       r => r.ReceiverId,      r => r.SelectedValue);
            AddBinding(RebroadcastSettings, comboBoxFormat,         r => r.Format,          r => r.SelectedValue);
            AddBinding(RebroadcastSettings, numericPort,            r => r.Port,            r => r.Value);
            AddBinding(RebroadcastSettings, numericStaleSeconds,    r => r.StaleSeconds,    r => r.Value);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.Name,                     textBoxName },
                { ValidationField.RebroadcastServerPort,    numericPort },
                { ValidationField.Format,                   comboBoxFormat },
                { ValidationField.RebroadcastReceiver,      comboBoxReceiver },
                { ValidationField.StaleSeconds,             numericStaleSeconds },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxEnabled,      Strings.Enabled,        Strings.OptionsDescribeRebroadcastServerEnabled);
            SetInlineHelp(textBoxName,          Strings.Name,           Strings.OptionsDescribeRebroadcastServerName);
            SetInlineHelp(comboBoxReceiver,     Strings.Receiver,       Strings.OptionsDescribeRebroadcastReceiver);
            SetInlineHelp(comboBoxFormat,       Strings.Format,         Strings.OptionsDescribeRebroadcastServerFormat);
            SetInlineHelp(numericPort,          Strings.Port,           Strings.OptionsDescribeRebroadcastServerPort);
            SetInlineHelp(numericStaleSeconds,  Strings.StaleSeconds,   Strings.OptionsDescribeRebroadcastStaleSeconds);
        }
    }
}
