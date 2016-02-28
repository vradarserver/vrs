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
using VirtualRadar.WinForms.PortableBinding;
using VirtualRadar.Interface.Listener;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// The options data entry page for a receiver.
    /// </summary>
    public partial class PageReceiver : Page
    {
        #region PageSummary
        /// <summary>
        /// The summary for receiver pages.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = Images.Radio16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return _PageIcon; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public Receiver Receiver { get { return PageObject as Receiver; } }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            public Summary() : base()
            {
                SetPageTitleProperty<Receiver>(r => r.Name, () => Receiver.Name);
                SetPageEnabledProperty<Receiver>(r => r.Enabled, () => Receiver.Enabled);
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageReceiver();
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
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageReceiver;
                SetValidationFields(new Dictionary<ValidationField, Control>() {
                    { ValidationField.Enabled,              page == null ? null : page.checkBoxEnabled },
                    { ValidationField.Name,                 page == null ? null : page.textBoxName },
                    { ValidationField.Location,             page == null ? null : page.comboBoxLocationId },
                    { ValidationField.Format,               page == null ? null : page.comboBoxDataSource },

                    { ValidationField.IsPassive,            page == null ? null : page.checkBoxIsPassive },
                    { ValidationField.BaseStationAddress,   page == null ? null : page.textBoxAddress },
                    { ValidationField.BaseStationPort,      page == null ? null : page.numericPort },
                    { ValidationField.UseKeepAlive,         page == null ? null : page.checkBoxUseKeepAlive },
                    { ValidationField.IdleTimeout,          page == null ? null : page.numericIdleTimeout },

                    { ValidationField.ComPort,              page == null ? null : page.comboBoxSerialComPort },
                    { ValidationField.BaudRate,             page == null ? null : page.comboBoxSerialBaudRate },
                    { ValidationField.DataBits,             page == null ? null : page.comboBoxSerialDataBits },
                });
            }
        }
        #endregion

        #region Fields
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
        #endregion

        /// <summary>
        /// See base docs.
        /// </summary>
        public Receiver Receiver { get { return ((Summary)PageSummary).Receiver; } }

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
        protected override void InitialiseControls()
        {
            radioButtonPanelUsage.MapButtonValues(r => {
                if(r == radioButtonUsageNormal)                 return ReceiverUsage.Normal;
                else if(r == radioButtonUsageHideFromWebSite)   return ReceiverUsage.HideFromWebSite;
                else if(r == radioButtonUsageMergeOnly)         return ReceiverUsage.MergeOnly;
                else                                            return null;
            });
            base.InitialiseControls();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();

            var receiverFormatManager = Factory.Singleton.Resolve<IReceiverFormatManager>().Singleton;
            var receiverFormats = receiverFormatManager.GetRegisteredFormats();

            AddControlBinder(new CheckBoxBoolBinder<Receiver>   (Receiver, checkBoxEnabled,         r => r.Enabled,         (r,v) => r.Enabled = v)         { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new CheckBoxBoolBinder<Receiver>   (Receiver, checkBoxIsPassive,       r => r.IsPassive,       (r,v) => r.IsPassive = v)       { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new CheckBoxBoolBinder<Receiver>   (Receiver, checkBoxUseKeepAlive,    r => r.UseKeepAlive,    (r,v) => r.UseKeepAlive = v)    { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new TextBoxStringBinder<Receiver>  (Receiver, textBoxName,                 r => r.Name,            (r,v) => r.Name = v)        { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new TextBoxStringBinder<Receiver>  (Receiver, textBoxAddress,              r => r.Address,         (r,v) => r.Address = v));
            AddControlBinder(new TextBoxStringBinder<Receiver>  (Receiver, textBoxPassphrase,           r => r.Passphrase,      (r,v) => r.Passphrase = v));
            AddControlBinder(new TextBoxStringBinder<Receiver>  (Receiver, textBoxSerialStartupText,    r => r.StartupText,     (r,v) => r.StartupText = v));
            AddControlBinder(new TextBoxStringBinder<Receiver>  (Receiver, textBoxSerialShutdownText,   r => r.ShutdownText,    (r,v) => r.ShutdownText = v));

            AddControlBinder(new NumericIntBinder<Receiver>     (Receiver, numericPort,             r => r.Port,                            (r,v) => r.Port = v));
            AddControlBinder(new NumericIntBinder<Receiver>     (Receiver, numericIdleTimeout,      r => r.IdleTimeoutMilliseconds / 1000,  (r,v) => r.IdleTimeoutMilliseconds = v * 1000) { ModelPropertyName = PropertyHelper.ExtractName<Receiver>(r => r.IdleTimeoutMilliseconds) });

            AddControlBinder(new ComboBoxBinder<Receiver, ReceiverLocation, int> (Receiver, comboBoxLocationId, SettingsView.Configuration.ReceiverLocations, r => r.ReceiverLocationId, (r,v) => r.ReceiverLocationId = v) { GetListItemDescription = r => r.Name, GetListItemValue = r => r.UniqueId, SortList = true, });
            AddControlBinder(new ComboBoxBinder<Receiver, ReceiverFormatName, string>(Receiver, comboBoxDataSource, receiverFormats,                              r => r.DataSource,         (r,v) => r.DataSource = v)         { GetListItemDescription = r => r.ShortName, GetListItemValue = r => r.UniqueId, SortList = true, });

            AddControlBinder(new ComboBoxEnumBinder<Receiver, ConnectionType>           (Receiver,          comboBoxConnectionType,         r => r.ConnectionType,          (r,v) => r.ConnectionType = v,          r => Describe.ConnectionType(r)) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new ComboBoxEnumBinder<Receiver, StopBits>                 (Receiver,          comboBoxSerialStopBits,         r => r.StopBits,                (r,v) => r.StopBits = v,                r => Describe.StopBits(r)));
            AddControlBinder(new ComboBoxEnumBinder<Receiver, Parity>                   (Receiver,          comboBoxSerialParity,           r => r.Parity,                  (r,v) => r.Parity = v,                  r => Describe.Parity(r)));
            AddControlBinder(new ComboBoxEnumBinder<Receiver, Handshake>                (Receiver,          comboBoxSerialHandshake,        r => r.Handshake,               (r,v) => r.Handshake = v,               r => Describe.Handshake(r)));

            AddControlBinder(new ComboBoxValueBinder<Receiver, string>  (Receiver, comboBoxSerialComPort,   SettingsView.GetSerialPortNames(),  r => r.ComPort,     (r,v) => r.ComPort = v));
            AddControlBinder(new ComboBoxValueBinder<Receiver, int>     (Receiver, comboBoxSerialBaudRate,  _SupportedBaudRates,                r => r.BaudRate,    (r,v) => r.BaudRate = v));
            AddControlBinder(new ComboBoxValueBinder<Receiver, int>     (Receiver, comboBoxSerialDataBits,  _SupportedDataBits,                 r => r.DataBits,    (r,v) => r.DataBits = v));

            AddControlBinder(new RadioButtonEnumBinder<Receiver, ReceiverUsage>(Receiver, radioButtonPanelUsage, r => r.ReceiverUsage, (r,v) => r.ReceiverUsage = v));

            AddControlBinder(new AccessControlBinder<Receiver>(Receiver, accessControl, r => r.Access));
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
            SetInlineHelp(radioButtonUsageNormal,           Strings.Normal,                 Strings.OptionDescribeReceiverUsageNormal);
            SetInlineHelp(radioButtonUsageHideFromWebSite,  Strings.HideFromWebSite,        Strings.OptionDescribeReceiverUsageHideFromWebSite);
            SetInlineHelp(radioButtonUsageMergeOnly,        Strings.MergeOnly,              Strings.OptionDescribeReceiverUsageMergeOnly);

            SetInlineHelp(checkBoxIsPassive,                Strings.PassiveReceiver,        Strings.OptionsDescribeDataSourcePassiveReceiver);
            SetInlineHelp(textBoxAddress,                   Strings.UNC,                    Strings.OptionsDescribeDataSourcesAddress);
            SetInlineHelp(numericPort,                      Strings.Port,                   Strings.OptionsDescribeDataSourcesPort);
            SetInlineHelp(textBoxPassphrase,                Strings.Passphrase,             Strings.OptionsDescribePassphrase);
            SetInlineHelp(checkBoxUseKeepAlive,             Strings.UseKeepAlive,           Strings.OptionsDescribeDataSourcesUseKeepAlive);
            SetInlineHelp(numericIdleTimeout,               Strings.IdleTimeout,            Strings.OptionsDescribeDataSourcesIdleTimeout);

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
            if(!DesignMode) {
                buttonWizard.Image = Images.Wizard16x16;
                buttonTestConnection.Image = Images.Test16x16;

                accessControl.AlignmentFieldLeftPosition = textBoxAddress.Left - 5;

                groupBoxSerial.Location = Point.Empty;
                ShowHideConnectionTypePanels();
                EnableDisableControls();
            }

            base.OnLoad(e);
        }

        private void ShowHideConnectionTypePanels()
        {
            groupBoxNetwork.Visible = Receiver.ConnectionType == ConnectionType.TCP;
            groupBoxAccessControl.Visible = Receiver.ConnectionType == ConnectionType.TCP;
            groupBoxSerial.Visible = Receiver.ConnectionType == ConnectionType.COM;
            switch(Receiver.ConnectionType) {
                case ConnectionType.COM: Height = panelConnectionTypeSettings.Top + groupBoxSerial.Height + 5; break;
                case ConnectionType.TCP: Height = panelConnectionTypeSettings.Top + groupBoxAccessControl.Top + groupBoxAccessControl.Height + 5; break;
                default:                 throw new NotImplementedException();
            }

            EnableDisableControls();
        }

        private void EnableDisableControls()
        {
            numericIdleTimeout.Enabled = !Receiver.UseKeepAlive;
            textBoxAddress.Enabled = !Receiver.IsPassive;
            accessControl.Enabled = Receiver.IsPassive;

            var access = Receiver.Access.DefaultAccess;
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
                }
            }
        }

        private void buttonClearLocationId_Click(object sender, EventArgs e)
        {
            Receiver.ReceiverLocationId = 0;
        }

        private void buttonTestConnection_Click(object sender, EventArgs e)
        {
            SettingsView.RaiseTestConnectionClicked(new EventArgs<Receiver>(Receiver));
        }
    }
}
