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

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// The options data entry page for a receiver.
    /// </summary>
    public partial class PageReceiver : Page
    {
        public override Image PageIcon { get { return Images.iconmonstr_radio_3_icon; } }

        [LocalisedDisplayName("Enabled")]
        [LocalisedDescription("OptionsDescribeReceiverEnabled")]
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
        public Observable<int> BaudRate { get; private set; }

        [LocalisedDisplayName("SerialDataBits")]
        [LocalisedDescription("OptionsDescribeDataSourcesDataBits")]
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
            RecordEnabled = BindProperty<bool>(checkBoxEnabled);
            RecordName = BindProperty<string>(textBoxName);
            ReceiverLocationId = BindProperty<int>(comboBoxLocationId);
        }

        protected override void CopyRecordToObservables()
        {
            var receiver = (Receiver)PageObject;
            RecordEnabled.Value =       receiver.Enabled;
            RecordName.Value =          receiver.Name;
            ReceiverLocationId.Value =  receiver.ReceiverLocationId;
        }

        protected override void CopyObservablesToRecord()
        {
            var receiver = (Receiver)PageObject;
            receiver.Enabled =              RecordEnabled.Value;
            receiver.Name =                 RecordName.Value;
            receiver.ReceiverLocationId =   ReceiverLocationId.Value;
        }

        protected override void InitialiseControls()
        {
            comboBoxLocationId.ObservableList = OptionsView.PageReceiverLocations.ReceiverLocations;
        }
    }
}
