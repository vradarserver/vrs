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
            buttonWizard.Image = Images.Wizard16x16;
            buttonTestConnection.Image = Images.Test16x16;
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            comboBoxDataSource.DataSource =         CreateSortingEnumSource<DataSource>(r => Describe.DataSource(r));
            comboBoxLocationId.DataSource =         CreateSortingBindingSource<ReceiverLocation>(SettingsView.Configuration.ReceiverLocations, r => r.Name);
            comboBoxConnectionType.DataSource =     CreateSortingEnumSource<ConnectionType>(r => Describe.ConnectionType(r));

            comboBoxSerialComPort.DataSource =      CreateNameValueSource<string>(SettingsView.GetSerialPortNames());
            comboBoxSerialBaudRate.DataSource =     CreateListBindingSource<int>(_SupportedBaudRates);
            comboBoxSerialDataBits.DataSource =     CreateListBindingSource<int>(_SupportedDataBits);
            comboBoxSerialStopBits.DataSource =     CreateSortingEnumSource<StopBits>(r => Describe.StopBits(r));
            comboBoxSerialParity.DataSource =       CreateSortingEnumSource<Parity>(r => Describe.Parity(r));
            comboBoxSerialHandshake.DataSource =    CreateSortingEnumSource<Handshake>(r => Describe.Handshake(r));

            SetPageTitleProperty<Receiver>(r => r.Name, () => Receiver.Name);
            SetPageEnabledProperty<Receiver>(r => r.Enabled, () => Receiver.Enabled);
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(Receiver, checkBoxEnabled,                   r => r.Enabled,                 r => r.Checked);
            AddBinding(Receiver, textBoxName,                       r => r.Name,                    r => r.Text,            DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(Receiver, comboBoxDataSource,                r => r.DataSource,              r => r.SelectedValue);
            AddBinding(Receiver, checkBoxAutoReconnectAtStartup,    r => r.AutoReconnectAtStartup,  r => r.Checked);
            AddBinding(Receiver, comboBoxLocationId,                r => r.ReceiverLocationId,      r => r.SelectedValue);
            AddBinding(Receiver, comboBoxConnectionType,            r => r.ConnectionType,          r => r.SelectedValue,   DataSourceUpdateMode.OnPropertyChanged);

            AddBinding(Receiver, textBoxAddress,    r => r.Address,     r => r.Text);
            AddBinding(Receiver, numericPort,       r => r.Port,        r => r.Value);

            AddBinding(Receiver, comboBoxSerialComPort,     r => r.ComPort,         r => r.SelectedValue);
            AddBinding(Receiver, comboBoxSerialBaudRate,    r => r.BaudRate,        r => r.SelectedItem);
            AddBinding(Receiver, comboBoxSerialDataBits,    r => r.DataBits,        r => r.SelectedItem);
            AddBinding(Receiver, comboBoxSerialStopBits,    r => r.StopBits,        r => r.SelectedValue);
            AddBinding(Receiver, comboBoxSerialParity,      r => r.Parity,          r => r.SelectedValue);
            AddBinding(Receiver, comboBoxSerialHandshake,   r => r.Handshake,       r => r.SelectedValue);
            AddBinding(Receiver, textBoxSerialStartupText,  r => r.StartupText,     r => r.Text);
            AddBinding(Receiver, textBoxSerialShutdownText, r => r.ShutdownText,    r => r.Text);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField, Control>() {
                { ValidationField.Enabled,              checkBoxEnabled },
                { ValidationField.Name,                 textBoxName },
                { ValidationField.Location,             comboBoxLocationId },

                { ValidationField.BaseStationAddress,   textBoxAddress },
                { ValidationField.BaseStationPort,      numericPort },

                { ValidationField.ComPort,              comboBoxSerialComPort },
                { ValidationField.BaudRate,             comboBoxSerialBaudRate },
                { ValidationField.DataBits,             comboBoxSerialDataBits },
            });
        }

        protected override void AssociateInlineHelp()
        {
            SetInlineHelp(checkBoxEnabled,                  Strings.Enabled,                Strings.OptionsDescribeReceiverEnabled);
            SetInlineHelp(textBoxName,                      Strings.Name,                   Strings.OptionsDescribeReceiverName);
            SetInlineHelp(comboBoxLocationId,               Strings.ReceiverLocation,       Strings.OptionsDescribeRawFeedReceiverLocation);
            SetInlineHelp(comboBoxDataSource,               Strings.DataSource,             Strings.OptionsDescribeDataSourcesDataSource);
            SetInlineHelp(comboBoxConnectionType,           Strings.ConnectionType,         Strings.OptionsDescribeDataSourcesConnectionType);
            SetInlineHelp(checkBoxAutoReconnectAtStartup,   Strings.AutoReconnectAtStartup, Strings.OptionsDescribeDataSourcesAutoReconnectAtStartup);

            SetInlineHelp(textBoxAddress,                   Strings.UNC,                    Strings.OptionsDescribeDataSourcesAddress);
            SetInlineHelp(numericPort,                      Strings.Port,                   Strings.OptionsDescribeDataSourcesPort);

            SetInlineHelp(comboBoxSerialComPort,            Strings.SerialComPort,          Strings.OptionsDescribeDataSourcesComPort);
            SetInlineHelp(comboBoxSerialBaudRate,           Strings.SerialBaudRate,         Strings.OptionsDescribeDataSourcesBaudRate);
            SetInlineHelp(comboBoxSerialDataBits,           Strings.SerialDataBits,         Strings.OptionsDescribeDataSourcesDataBits);
            SetInlineHelp(comboBoxSerialStopBits,           Strings.SerialStopBits,         Strings.OptionsDescribeDataSourcesStopBits);
            SetInlineHelp(comboBoxSerialParity,             Strings.SerialParity,           Strings.OptionsDescribeDataSourcesParity);
            SetInlineHelp(comboBoxSerialHandshake,          Strings.SerialHandshake,        Strings.OptionsDescribeDataSourcesHandshake);
            SetInlineHelp(textBoxSerialStartupText,         Strings.SerialStartupText,      Strings.OptionsDescribeDataSourcesStartupText);
            SetInlineHelp(textBoxSerialShutdownText,        Strings.SerialShutdownText,     Strings.OptionsDescribeDataSourcesShutdownText);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                groupBoxSerial.Location = groupBoxNetwork.Location = Point.Empty;
                ShowHideConnectionTypePanels();
            }
        }

        private void ShowHideConnectionTypePanels()
        {
            groupBoxNetwork.Visible = Receiver.ConnectionType == ConnectionType.TCP;
            groupBoxSerial.Visible = Receiver.ConnectionType == ConnectionType.COM;
            switch(Receiver.ConnectionType) {
                case ConnectionType.COM: Height = panelConnectionTypeSettings.Top + groupBoxSerial.Height; break;
                case ConnectionType.TCP: Height = panelConnectionTypeSettings.Top + groupBoxNetwork.Height; break;
                default:                 throw new NotImplementedException();
            }
        }

        internal override void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
            base.ConfigurationChanged(args);
            if(args.Record == PageObject && SettingsView != null && this.IsHandleCreated) {
                if(args.PropertyName == PropertyHelper.ExtractName<Receiver>(r => r.ConnectionType)) {
                    ShowHideConnectionTypePanels();
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
