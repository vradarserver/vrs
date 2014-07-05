using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// The page that supports the configuration of a single rebroadcast server.
    /// </summary>
    public partial class PageRebroadcastServer : Page
    {
        public override Image PageIcon { get { return Images.Rebroadcast16x16; } }

        public RebroadcastSettings RebroadcastServer { get { return PageObject as RebroadcastSettings; } }

        [PageEnabled]
        [LocalisedDisplayName("Enabled")]
        [LocalisedDescription("OptionsDescribeRebroadcastServerEnabled")]
        [ValidationField(ValidationField.Enabled)]
        public Observable<bool> RecordEnabled { get; private set; }

        [PageTitle]
        [LocalisedDisplayName("Name")]
        [LocalisedDescription("OptionsDescribeRebroadcastServerName")]
        [ValidationField(ValidationField.Name)]
        public Observable<string> RecordName { get; private set; }

        [LocalisedDisplayName("Receiver")]
        [LocalisedDescription("OptionsDescribeRebroadcastReceiver")]
        [ValidationField(ValidationField.RebroadcastReceiver)]
        public Observable<int> ReceiverId { get; private set; }

        [LocalisedDisplayName("Format")]
        [LocalisedDescription("OptionsDescribeRebroadcastServerFormat")]
        public Observable<RebroadcastFormat> Format { get; private set; }

        [LocalisedDisplayName("Port")]
        [LocalisedDescription("OptionsDescribeRebroadcastServerPort")]
        [ValidationField(ValidationField.RebroadcastServerPort)]
        public Observable<int> Port { get; private set; }

        [LocalisedDisplayName("StaleSeconds")]
        [LocalisedDescription("OptionsDescribeRebroadcastStaleSeconds")]
        [ValidationField(ValidationField.StaleSeconds)]
        public Observable<int> StaleSeconds { get; private set; }

        public PageRebroadcastServer()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            RecordEnabled = BindProperty<bool>(checkBoxEnabled);
            RecordName =    BindProperty<string>(textBoxName);
            ReceiverId =    BindProperty<int>(comboBoxReceiver);
            Format =        BindProperty<RebroadcastFormat>(comboBoxFormat);
            Port =          BindProperty<int>(numericPort);
            StaleSeconds =  BindProperty<int>(numericStaleSeconds);
        }

        protected override void CopyRecordToObservables()
        {
            RecordEnabled.Value =   RebroadcastServer.Enabled;
            RecordName.Value =      RebroadcastServer.Name;
            ReceiverId.Value =      RebroadcastServer.ReceiverId;
            Format.Value =          RebroadcastServer.Format;
            Port.Value =            RebroadcastServer.Port;
            StaleSeconds.Value =    RebroadcastServer.StaleSeconds;
        }

        protected override void CopyObservablesToRecord()
        {
            RebroadcastServer.Enabled =         RecordEnabled.Value;
            RebroadcastServer.Name =            RecordName.Value;
            RebroadcastServer.ReceiverId =      ReceiverId.Value;
            RebroadcastServer.Format =          Format.Value;
            RebroadcastServer.Port =            Port.Value;
            RebroadcastServer.StaleSeconds =    StaleSeconds.Value;
        }

        protected override void InitialiseControls()
        {
            comboBoxReceiver.ObservableList = OptionsView.CombinedFeeds;
            comboBoxFormat.PopulateWithEnums<RebroadcastFormat>(r => Describe.RebroadcastFormat(r));
        }

        public override void PageSelected()
        {
            base.PageSelected();
            comboBoxReceiver.RefreshContent();
        }
    }
}
