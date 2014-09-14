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
using VirtualRadar.WinForms.Controls;
using VirtualRadar.Interface;
using System.IO.Ports;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using InterfaceFactory;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// The options data entry page for a receiver.
    /// </summary>
    public partial class PageReceiver : Page
    {
        /// <summary>
        /// A list of all of the baud rates that we can show to the user.
        /// </summary>
        private static int[] _SupportedBaudRates = new int[] {
            110,
            300,
            1200,
            2400,
            4800,
            9600,
            19200,
            38400,
            57600,
            115200,
            230400,
            460800,
            921600,
            3000000,
        };

        /// <summary>
        /// A list of all of the data bit values that we can show to the user.
        /// </summary>
        private static int[] _SupportedDataBits = new int[] {
            5,
            6,
            7,
            8,
        };

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image PageIcon { get { return Images.Radio16x16; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public Receiver Receiver { get { return PageObject as Receiver; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageReceiver()
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
            var receiver = record as Receiver;
            return receiver != null && Receiver != null && receiver.UniqueId == Receiver.UniqueId;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            comboBoxDataSource.DataSource =         CreateSortingEnumSource<DataSource>(r => Describe.DataSource(r));
            comboBoxLocationId.DataSource =         CreateSortingBindingSource<ReceiverLocation>(SettingsView.Configuration.ReceiverLocations, r => r.Name);
            comboBoxConnectionType.DataSource =     CreateSortingEnumSource<ConnectionType>(r => Describe.ConnectionType(r));
            comboBoxDefaultAccess.DataSource =      CreateSortingEnumSource<DefaultAccess>(r => Describe.DefaultAccess(r));

            comboBoxSerialComPort.DataSource =      CreateNameValueSource<string>(SettingsView.GetSerialPortNames());
            comboBoxSerialBaudRate.DataSource =     CreateListBindingSource<int>(_SupportedBaudRates);
            comboBoxSerialDataBits.DataSource =     CreateListBindingSource<int>(_SupportedDataBits);
            comboBoxSerialStopBits.DataSource =     CreateSortingEnumSource<StopBits>(r => Describe.StopBits(r));
            comboBoxSerialParity.DataSource =       CreateSortingEnumSource<Parity>(r => Describe.Parity(r));
            comboBoxSerialHandshake.DataSource =    CreateSortingEnumSource<Handshake>(r => Describe.Handshake(r));

            SetPageTitleProperty<Receiver>(r => r.Name, () => Receiver.Name);
            SetPageEnabledProperty<Receiver>(r => r.Enabled, () => Receiver.Enabled);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(Receiver, checkBoxEnabled,                   r => r.Enabled,                 r => r.Checked);
            AddBinding(Receiver, textBoxName,                       r => r.Name,                    r => r.Text,            DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(Receiver, comboBoxDataSource,                r => r.DataSource,              r => r.SelectedValue);
            AddBinding(Receiver, comboBoxLocationId,                r => r.ReceiverLocationId,      r => r.SelectedValue);
            AddBinding(Receiver, comboBoxConnectionType,            r => r.ConnectionType,          r => r.SelectedValue,   DataSourceUpdateMode.OnPropertyChanged);

            AddBinding(Receiver, checkBoxIsPassive,     r => r.IsPassive,               r => r.Checked, DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(Receiver, textBoxAddress,        r => r.Address,                 r => r.Text);
            AddBinding(Receiver, numericPort,           r => r.Port,                    r => r.Value);
            AddBinding(Receiver, checkBoxUseKeepAlive,  r => r.UseKeepAlive,            r => r.Checked, DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(Receiver, numericIdleTimeout,    r => r.IdleTimeoutMilliseconds, r => r.Value,   format: MillisecondsToSeconds_Format, parse: MillisecondsToSeconds_Parse);

            AddBinding(Receiver.Access, comboBoxDefaultAccess, r => r.DefaultAccess, r => r.SelectedValue, DataSourceUpdateMode.OnPropertyChanged);
            bindingCidrList.DataSource = Receiver.Access.Addresses;

            AddBinding(Receiver, comboBoxSerialComPort,     r => r.ComPort,         r => r.SelectedValue);
            AddBinding(Receiver, comboBoxSerialBaudRate,    r => r.BaudRate,        r => r.SelectedItem);
            AddBinding(Receiver, comboBoxSerialDataBits,    r => r.DataBits,        r => r.SelectedItem);
            AddBinding(Receiver, comboBoxSerialStopBits,    r => r.StopBits,        r => r.SelectedValue);
            AddBinding(Receiver, comboBoxSerialParity,      r => r.Parity,          r => r.SelectedValue);
            AddBinding(Receiver, comboBoxSerialHandshake,   r => r.Handshake,       r => r.SelectedValue);
            AddBinding(Receiver, textBoxSerialStartupText,  r => r.StartupText,     r => r.Text);
            AddBinding(Receiver, textBoxSerialShutdownText, r => r.ShutdownText,    r => r.Text);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField, Control>() {
                { ValidationField.Enabled,              checkBoxEnabled },
                { ValidationField.Name,                 textBoxName },
                { ValidationField.Location,             comboBoxLocationId },

                { ValidationField.IsPassive,            checkBoxIsPassive },
                { ValidationField.BaseStationAddress,   textBoxAddress },
                { ValidationField.BaseStationPort,      numericPort },
                { ValidationField.UseKeepAlive,         checkBoxUseKeepAlive },
                { ValidationField.IdleTimeout,          numericIdleTimeout },

                { ValidationField.ComPort,              comboBoxSerialComPort },
                { ValidationField.BaudRate,             comboBoxSerialBaudRate },
                { ValidationField.DataBits,             comboBoxSerialDataBits },
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            SetInlineHelp(checkBoxEnabled,                  Strings.Enabled,                Strings.OptionsDescribeReceiverEnabled);
            SetInlineHelp(textBoxName,                      Strings.Name,                   Strings.OptionsDescribeReceiverName);
            SetInlineHelp(comboBoxLocationId,               Strings.ReceiverLocation,       Strings.OptionsDescribeRawFeedReceiverLocation);
            SetInlineHelp(comboBoxDataSource,               Strings.DataSource,             Strings.OptionsDescribeDataSourcesDataSource);
            SetInlineHelp(comboBoxConnectionType,           Strings.ConnectionType,         Strings.OptionsDescribeDataSourcesConnectionType);

            SetInlineHelp(checkBoxIsPassive,                Strings.PassiveReceiver,        Strings.OptionsDescribeDataSourcePassiveReceiver);
            SetInlineHelp(textBoxAddress,                   Strings.UNC,                    Strings.OptionsDescribeDataSourcesAddress);
            SetInlineHelp(numericPort,                      Strings.Port,                   Strings.OptionsDescribeDataSourcesPort);
            SetInlineHelp(checkBoxUseKeepAlive,             Strings.UseKeepAlive,           Strings.OptionsDescribeDataSourcesUseKeepAlive);
            SetInlineHelp(numericIdleTimeout,               Strings.IdleTimeout,            Strings.OptionsDescribeDataSourcesIdleTimeout);

            SetInlineHelp(comboBoxDefaultAccess,            Strings.DefaultAccess,          Strings.OptionsDescribeDefaultAccess);

            SetInlineHelp(comboBoxSerialComPort,            Strings.SerialComPort,          Strings.OptionsDescribeDataSourcesComPort);
            SetInlineHelp(comboBoxSerialBaudRate,           Strings.SerialBaudRate,         Strings.OptionsDescribeDataSourcesBaudRate);
            SetInlineHelp(comboBoxSerialDataBits,           Strings.SerialDataBits,         Strings.OptionsDescribeDataSourcesDataBits);
            SetInlineHelp(comboBoxSerialStopBits,           Strings.SerialStopBits,         Strings.OptionsDescribeDataSourcesStopBits);
            SetInlineHelp(comboBoxSerialParity,             Strings.SerialParity,           Strings.OptionsDescribeDataSourcesParity);
            SetInlineHelp(comboBoxSerialHandshake,          Strings.SerialHandshake,        Strings.OptionsDescribeDataSourcesHandshake);
            SetInlineHelp(textBoxSerialStartupText,         Strings.SerialStartupText,      Strings.OptionsDescribeDataSourcesStartupText);
            SetInlineHelp(textBoxSerialShutdownText,        Strings.SerialShutdownText,     Strings.OptionsDescribeDataSourcesShutdownText);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                buttonWizard.Image = Images.Wizard16x16;
                buttonTestConnection.Image = Images.Test16x16;

                groupBoxSerial.Location = Point.Empty;
                ShowHideConnectionTypePanels();
                EnableDisableControls();
            }
        }

        private void ShowHideConnectionTypePanels()
        {
            groupBoxNetwork.Visible = Receiver.ConnectionType == ConnectionType.TCP;
            groupBoxAccessControl.Visible = Receiver.ConnectionType == ConnectionType.TCP;
            groupBoxSerial.Visible = Receiver.ConnectionType == ConnectionType.COM;
            switch(Receiver.ConnectionType) {
                case ConnectionType.COM: Height = panelConnectionTypeSettings.Top + groupBoxSerial.Height; break;
                case ConnectionType.TCP: Height = panelConnectionTypeSettings.Top + groupBoxAccessControl.Top + groupBoxAccessControl.Height; break;
                default:                 throw new NotImplementedException();
            }
        }

        private void EnableDisableControls()
        {
            numericIdleTimeout.Enabled = !Receiver.UseKeepAlive;
            textBoxAddress.Enabled = !Receiver.IsPassive;
            groupBoxAccessControl.Enabled = Receiver.IsPassive;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="args"></param>
        internal override void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
            base.ConfigurationChanged(args);
            if(SettingsView != null && IsHandleCreated) {
                if(args.Record == Receiver) {
                    if(args.PropertyName == PropertyHelper.ExtractName<Receiver>(r => r.ConnectionType)) {
                        ShowHideConnectionTypePanels();
                    } else if(args.PropertyName == PropertyHelper.ExtractName<Receiver>(r => r.UseKeepAlive)) {
                        EnableDisableControls();
                    } else if(args.PropertyName == PropertyHelper.ExtractName<Receiver>(r => r.IsPassive)) {
                        EnableDisableControls();
                    }
                }
            }
        }

        private void buttonWizard_Click(object sender, EventArgs e)
        {
            using(var dialog = new ReceiverConfigurationWizard()) {
                if(dialog.ShowDialog() == DialogResult.OK) {
                    SettingsView.ApplyReceiverConfigurationWizard(dialog.Answers, Receiver);

                    // Under Mono the changes made by the presenter to this receiver will not show up,
                    // it ignores NotifyPropertyChanged events. I'm not changing how this works just
                    // for Mono so we're just going to rebind if we have the misfortune of running under
                    // it.
                    if(Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono) {
                        ClearBindings();
                        CreateBindings();
                    }
                }
            }
        }

        private void buttonClearLocationId_Click(object sender, EventArgs e)
        {
            Receiver.ReceiverLocationId = 0;

            // Under Mono that line doesn't work - Mono ignores the NotifyPropertyChanged event and continues
            // to show the old value. The workaround is to rebind the control.
            if(Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono) {
                ClearBindings();
                CreateBindings();
            }
        }

        private void buttonTestConnection_Click(object sender, EventArgs e)
        {
            SettingsView.RaiseTestConnectionClicked(new EventArgs<Receiver>(Receiver));
        }

        private void comboBoxDefaultAccess_SelectedIndexChanged(object sender, EventArgs e)
        {
            var access = Receiver.Access.DefaultAccess;
            bindingCidrList.Enabled = access != DefaultAccess.Unrestricted;
            labelCidrList.Text = String.Format("{0}:", access == DefaultAccess.Allow ? Strings.DenyTheseAddresses : Strings.AllowTheseAddresses);
        }
    }
}
