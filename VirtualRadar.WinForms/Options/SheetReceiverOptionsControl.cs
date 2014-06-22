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
using System.IO.Ports;
using VirtualRadar.Interface;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// Presents the options for a single receiver to the user.
    /// </summary>
    public partial class SheetReceiverOptionsControl : SheetControl
    {
        #region Sheet properties
        /// <summary>
        /// See base class docs.
        /// </summary>
        public override string SheetTitle { get { return RecordName ?? ""; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image Icon { get { return Images.iconmonstr_radio_3_icon; } }
        #endregion

        public bool RecordEnabled
        {
            get { return checkBoxEnabled.Checked; }
            set { checkBoxEnabled.Checked = value; }
        }

        public string RecordName
        {
            get { return textBoxName.Text.Trim(); }
            set { textBoxName.Text = value; }
        }

        public int ReceiverLocationId
        {
            get { return receiverLocationIdControl.SelectedLocationId; }
            set { receiverLocationIdControl.SelectedLocationId = value; }
        }

        public DataSource DataSource
        {
            get { return comboBoxDataSource.GetSelectedEnum<DataSource>(); }
            set { comboBoxDataSource.SelectedValue = value; }
        }

        public ConnectionType ConnectionType
        {
            get { return comboBoxConnectionType.GetSelectedEnum<ConnectionType>(); }
            set { comboBoxConnectionType.SelectedValue = value; }
        }

        public bool AutoReconnectAtStartup
        {
            get { return checkBoxAutoReconnectAtStartup.Checked; }
            set { checkBoxAutoReconnectAtStartup.Checked = value; }
        }

        public string Address
        {
            get { return textBoxAddress.Text.Trim(); }
            set { textBoxAddress.Text = value; }
        }

        public int Port
        {
            get { return (int)numericPort.Value; }
            set { numericPort.Value = value; }
        }

        public string ComPort
        {
            get { return comboBoxSerialComPort.GetSelectedObject<string>(); }
            set { comboBoxSerialComPort.SelectedValue = value; }
        }

        public int BaudRate
        {
            get { return comboBoxSerialBaudRate.GetSelectedObject<int>(); }
            set { comboBoxSerialBaudRate.SelectedValue = value; }
        }

        public int DataBits
        {
            get { return comboBoxSerialDataBits.GetSelectedObject<int>(); }
            set { comboBoxSerialDataBits.SelectedValue = value; }
        }

        public StopBits StopBits
        {
            get { return comboBoxSerialStopBits.GetSelectedEnum<StopBits>(); }
            set { comboBoxSerialStopBits.SelectedValue = value; }
        }

        public Parity Parity
        {
            get { return comboBoxSerialParity.GetSelectedEnum<Parity>(); }
            set { comboBoxSerialParity.SelectedValue = value; }
        }

        public Handshake Handshake
        {
            get { return comboBoxSerialHandshake.GetSelectedEnum<Handshake>(); }
            set { comboBoxSerialHandshake.SelectedValue = value; }
        }

        public string StartupText
        {
            get { return textBoxSerialStartupText.Text.Trim(); }
            set { textBoxSerialStartupText.Text = value; }
        }

        public string ShutdownText
        {
            get { return textBoxSerialShutdownText.Text.Trim(); }
            set { textBoxSerialShutdownText.Text = value; }
        }

        public SheetReceiverOptionsControl()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                // Work around anchors bug in designer...
                textBoxName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                groupBoxNetwork.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                groupBoxSerial.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                textBoxSerialStartupText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                textBoxSerialShutdownText.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                // Populate enum combo boxes
                comboBoxDataSource.PopulateWithEnums<DataSource>(Describe.DataSource);
                comboBoxConnectionType.PopulateWithEnums<ConnectionType>(Describe.ConnectionType);
                comboBoxSerialStopBits.PopulateWithEnums<StopBits>(Describe.StopBits);
                comboBoxSerialParity.PopulateWithEnums<Parity>(Describe.Parity);
                comboBoxSerialHandshake.PopulateWithEnums<Handshake>(Describe.Handshake);

                // Populate the combo boxes
                comboBoxSerialComPort.PopulateWithObjects<string>(SerialPort.GetPortNames().OrderBy(r => r), r => r);
                comboBoxSerialBaudRate.PopulateWithObjects<int>(new int[] {
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
                }, r => r.ToString());
                comboBoxSerialDataBits.PopulateWithObjects<int>(new int[] {
                    5,
                    6,
                    7,
                    8,
                }, r => r.ToString());

                // Attach localised descriptions
                AddLocalisedDescription(checkBoxEnabled,                Strings.Enabled,                Strings.OptionsDescribeReceiverEnabled);
                AddLocalisedDescription(textBoxName,                    Strings.Name,                   Strings.OptionsDescribeReceiverName);
                AddLocalisedDescription(receiverLocationIdControl,      Strings.ReceiverLocation,       Strings.OptionsDescribeRawFeedReceiverLocation);
                AddLocalisedDescription(comboBoxDataSource,             Strings.DataSource,             Strings.OptionsDescribeDataSourcesDataSource);
                AddLocalisedDescription(comboBoxConnectionType,         Strings.ConnectionType,         Strings.OptionsDescribeDataSourcesConnectionType);
                AddLocalisedDescription(checkBoxAutoReconnectAtStartup, Strings.AutoReconnectAtStartup, Strings.OptionsDescribeDataSourcesAutoReconnectAtStartup);
                AddLocalisedDescription(textBoxAddress,                 Strings.UNC,                    Strings.OptionsDescribeDataSourcesAddress);
                AddLocalisedDescription(numericPort,                    Strings.Port,                   Strings.OptionsDescribeDataSourcesPort);
                AddLocalisedDescription(comboBoxSerialComPort,          Strings.SerialComPort,          Strings.OptionsDescribeDataSourcesComPort);
                AddLocalisedDescription(comboBoxSerialBaudRate,         Strings.SerialBaudRate,         Strings.OptionsDescribeDataSourcesBaudRate);
                AddLocalisedDescription(comboBoxSerialDataBits,         Strings.SerialDataBits,         Strings.OptionsDescribeDataSourcesDataBits);
                AddLocalisedDescription(comboBoxSerialStopBits,         Strings.SerialStopBits,         Strings.OptionsDescribeDataSourcesStopBits);
                AddLocalisedDescription(comboBoxSerialParity,           Strings.SerialParity,           Strings.OptionsDescribeDataSourcesParity);
                AddLocalisedDescription(comboBoxSerialHandshake,        Strings.SerialHandshake,        Strings.OptionsDescribeDataSourcesHandshake);
                AddLocalisedDescription(textBoxSerialStartupText,       Strings.SerialStartupText,      Strings.OptionsDescribeDataSourcesStartupText);
                AddLocalisedDescription(textBoxSerialShutdownText,      Strings.SerialShutdownText,     Strings.OptionsDescribeDataSourcesShutdownText);

                // Set the value changed controls
                RaisesValueChanged(
                    textBoxName,
                    receiverLocationIdControl,
                    comboBoxDataSource,
                    comboBoxConnectionType,
                    textBoxAddress,
                    numericPort,
                    comboBoxSerialComPort,
                    comboBoxSerialBaudRate,
                    comboBoxSerialDataBits
                );

                // Attach validation field handlers
                _ValidationHelper.RegisterValidationField(ValidationField.Name, textBoxName);
                _ValidationHelper.RegisterValidationField(ValidationField.Location, receiverLocationIdControl);
                _ValidationHelper.RegisterValidationField(ValidationField.BaseStationAddress, textBoxAddress);
                _ValidationHelper.RegisterValidationField(ValidationField.BaseStationPort, numericPort);
                _ValidationHelper.RegisterValidationField(ValidationField.ComPort, comboBoxSerialComPort);
                _ValidationHelper.RegisterValidationField(ValidationField.BaudRate, comboBoxSerialBaudRate);
                _ValidationHelper.RegisterValidationField(ValidationField.DataBits, comboBoxSerialDataBits);
            }
        }
    }
}
