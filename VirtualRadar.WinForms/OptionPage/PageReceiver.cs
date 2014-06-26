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
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Resources;
using VirtualRadar.Interface.Settings;
using System.IO.Ports;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// The options data entry page for a receiver.
    /// </summary>
    public partial class PageReceiver : Page
    {
        public override Image PageIcon { get { return Images.iconmonstr_radio_3_icon; } }

        public Receiver Receiver { get { return PageObject as Receiver; } }

        [PageEnabled]
        [LocalisedDisplayName("Enabled")]
        [LocalisedDescription("OptionsDescribeReceiverEnabled")]
        [ValidationField(ValidationField.Enabled)]
        public Observable<bool> RecordEnabled { get; private set; }

        [PageTitle]
        [LocalisedDisplayName("Name")]
        [LocalisedDescription("OptionsDescribeReceiverName")]
        [ValidationField(ValidationField.Name)]
        public Observable<string> RecordName { get; private set; }

        [LocalisedDisplayName("ReceiverLocation")]
        [LocalisedDescription("OptionsDescribeRawFeedReceiverLocation")]
        [ValidationField(ValidationField.Location)]
        public Observable<int> ReceiverLocationId { get; private set; }

        [LocalisedDisplayName("DataSource")]
        [LocalisedDescription("OptionsDescribeDataSourcesDataSource")]
        public Observable<DataSource> DataSource { get; private set; }

        [LocalisedDisplayName("ConnectionType")]
        [LocalisedDescription("OptionsDescribeDataSourcesConnectionType")]
        public Observable<ConnectionType> ConnectionType { get; private set; }

        [LocalisedDisplayName("AutoReconnectAtStartup")]
        [LocalisedDescription("OptionsDescribeDataSourcesAutoReconnectAtStartup")]
        public Observable<bool> AutoReconnectAtStartup { get; private set; }

        [LocalisedDisplayName("UNC")]
        [LocalisedDescription("OptionsDescribeDataSourcesAddress")]
        [ValidationField(ValidationField.BaseStationAddress)]
        public Observable<string> Address { get; private set; }

        [LocalisedDisplayName("Port")]
        [LocalisedDescription("OptionsDescribeDataSourcesPort")]
        [ValidationField(ValidationField.BaseStationPort)]
        public Observable<int> Port { get; private set; }

        [LocalisedDisplayName("SerialComPort")]
        [LocalisedDescription("OptionsDescribeDataSourcesComPort")]
        [ValidationField(ValidationField.ComPort)]
        public Observable<string> ComPort { get; private set; }

        [LocalisedDisplayName("SerialBaudRate")]
        [LocalisedDescription("OptionsDescribeDataSourcesBaudRate")]
        [ValidationField(ValidationField.BaudRate)]
        public Observable<int> BaudRate { get; private set; }

        [LocalisedDisplayName("SerialDataBits")]
        [LocalisedDescription("OptionsDescribeDataSourcesDataBits")]
        [ValidationField(ValidationField.DataBits)]
        public Observable<int> DataBits { get; private set; }

        [LocalisedDisplayName("SerialStopBits")]
        [LocalisedDescription("OptionsDescribeDataSourcesStopBits")]
        public Observable<StopBits> StopBits { get; private set; }

        [LocalisedDisplayName("SerialParity")]
        [LocalisedDescription("OptionsDescribeDataSourcesParity")]
        public Observable<Parity> Parity { get; private set; }

        [LocalisedDisplayName("SerialHandshake")]
        [LocalisedDescription("OptionsDescribeDataSourcesHandshake")]
        public Observable<Handshake> Handshake { get; private set; }

        [LocalisedDisplayName("SerialStartupText")]
        [LocalisedDescription("OptionsDescribeDataSourcesStartupText")]
        public Observable<string> StartupText { get; private set; }

        [LocalisedDisplayName("SerialShutdownText")]
        [LocalisedDescription("OptionsDescribeDataSourcesShutdownText")]
        public Observable<string> ShutdownText { get; set; }

        public PageReceiver()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            RecordEnabled =             BindProperty<bool>(checkBoxEnabled);
            RecordName =                BindProperty<string>(textBoxName);
            DataSource =                BindProperty<DataSource>(comboBoxDataSource);
            AutoReconnectAtStartup =    BindProperty<bool>(checkBoxAutoReconnectAtStartup);
            ReceiverLocationId =        BindProperty<int>(comboBoxLocationId);
            ConnectionType =            BindProperty<ConnectionType>(comboBoxConnectionType);

            Address =                   BindProperty<string>(textBoxAddress);
            Port =                      BindProperty<int>(numericPort);

            ComPort =                   BindProperty<string>(comboBoxSerialComPort);
            BaudRate =                  BindProperty<int>(comboBoxSerialBaudRate);
            DataBits =                  BindProperty<int>(comboBoxSerialDataBits);
            StopBits =                  BindProperty<StopBits>(comboBoxSerialStopBits);
            Parity =                    BindProperty<Parity>(comboBoxSerialParity);
            Handshake =                 BindProperty<Handshake>(comboBoxSerialHandshake);
            StartupText =               BindProperty<string>(textBoxSerialStartupText);
            ShutdownText =              BindProperty<string>(textBoxSerialShutdownText);
        }

        protected override void CopyRecordToObservables()
        {
            RecordEnabled.Value =           Receiver.Enabled;
            RecordName.Value =              Receiver.Name;
            DataSource.Value =              Receiver.DataSource;
            AutoReconnectAtStartup.Value =  Receiver.AutoReconnectAtStartup;
            ReceiverLocationId.Value =      Receiver.ReceiverLocationId;
            ConnectionType.Value =          Receiver.ConnectionType;

            Address.Value =                 Receiver.Address;
            Port.Value =                    Receiver.Port;

            ComPort.Value =                 Receiver.ComPort;
            BaudRate.Value =                Receiver.BaudRate;
            DataBits.Value =                Receiver.DataBits;
            StopBits.Value =                Receiver.StopBits;
            Parity.Value =                  Receiver.Parity;
            Handshake.Value =               Receiver.Handshake;
            StartupText.Value =             Receiver.StartupText;
            ShutdownText.Value =            Receiver.ShutdownText;
        }

        protected override void CopyObservablesToRecord()
        {
            Receiver.Enabled =                  RecordEnabled.Value;
            Receiver.Name =                     RecordName.Value;
            Receiver.DataSource =               DataSource.Value;
            Receiver.AutoReconnectAtStartup =   AutoReconnectAtStartup.Value;
            Receiver.ReceiverLocationId =       ReceiverLocationId.Value;
            Receiver.ConnectionType =           ConnectionType.Value;

            Receiver.Address =                  Address.Value;
            Receiver.Port =                     Port.Value;

            Receiver.ComPort =                  ComPort.Value;
            Receiver.BaudRate =                 BaudRate.Value;
            Receiver.DataBits =                 DataBits.Value;
            Receiver.StopBits =                 StopBits.Value;
            Receiver.Parity =                   Parity.Value;
            Receiver.Handshake =                Handshake.Value;
            Receiver.StartupText =              StartupText.Value;
            Receiver.ShutdownText =             ShutdownText.Value;
        }

        protected override void InitialiseControls()
        {
            comboBoxLocationId.ObservableList = OptionsView.PageReceiverLocations.ReceiverLocations;

            comboBoxDataSource.PopulateWithEnums<DataSource>(Describe.DataSource);
            comboBoxConnectionType.PopulateWithEnums<ConnectionType>(Describe.ConnectionType);
            comboBoxSerialStopBits.PopulateWithEnums<StopBits>(Describe.StopBits);
            comboBoxSerialParity.PopulateWithEnums<Parity>(Describe.Parity);
            comboBoxSerialHandshake.PopulateWithEnums<Handshake>(Describe.Handshake);
            comboBoxSerialComPort.PopulateWithCollection<string>(SerialPort.GetPortNames().OrderBy(r => r), r => r);
            comboBoxSerialBaudRate.PopulateWithCollection<int>(new int[] {
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
            comboBoxSerialDataBits.PopulateWithCollection<int>(new int[] {
                5,
                6,
                7,
                8,
            }, r => r.ToString());
        }

        private void buttonClearLocationId_Click(object sender, EventArgs e)
        {
            ReceiverLocationId.Value = 0;
        }

        private void comboBoxConnectionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            groupBoxNetwork.Enabled = ConnectionType.Value == Interface.Settings.ConnectionType.TCP;
            groupBoxSerial.Enabled = ConnectionType.Value == Interface.Settings.ConnectionType.COM;
        }

        private void buttonTestConnection_Click(object sender, EventArgs e)
        {
            CopyObservablesToRecord();
            OptionsView.RaiseTestConnectionClicked(new EventArgs<Receiver>(Receiver));
        }
    }
}
