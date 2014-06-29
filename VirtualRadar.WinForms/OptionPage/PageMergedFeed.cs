using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Resources;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// Displays and edits the settings for a merged feed.
    /// </summary>
    public partial class PageMergedFeed : Page
    {
        public override Image PageIcon { get { return Images.MergedFeed16x16; } }

        public MergedFeed MergedFeed { get { return PageObject as MergedFeed; } }

        public override bool PageUseFullHeight { get { return true; } }

        [PageEnabled]
        [LocalisedDisplayName("Enabled")]
        [LocalisedDescription("OptionsDescribeMergedFeedEnabled")]
        [ValidationField(ValidationField.Enabled)]
        public Observable<bool> RecordEnabled { get; private set; }

        [PageTitle]
        [LocalisedDisplayName("Name")]
        [LocalisedDescription("OptionsDescribeMergedFeedName")]
        [ValidationField(ValidationField.Name)]
        public Observable<string> RecordName { get; private set; }

        [LocalisedDisplayName("IcaoTimeout")]
        [LocalisedDescription("OptionsDescribeIcaoTimeout")]
        [ValidationField(ValidationField.IcaoTimeout)]
        public Observable<double> IcaoTimeout { get; private set; }

        [LocalisedDisplayName("IgnoreAircraftWithNoPosition")]
        [LocalisedDescription("OptionsDescribeIgnoreAircraftWithNoPosition")]
        public Observable<bool> IgnoreAircraftWithNoPosition { get; private set; }

        [LocalisedDisplayName("Receivers")]
        [LocalisedDescription("OptionsDescribeMergedFeedReceviers")]
        [ValidationField(ValidationField.ReceiverIds, IconAlignment=ErrorIconAlignment.TopLeft)]
        public ObservableList<int> ReceiverIds { get; private set; }

        public PageMergedFeed()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            RecordEnabled = BindProperty<bool>(checkBoxEnabled);
            RecordName = BindProperty<string>(textBoxName);
            IcaoTimeout = BindProperty<double>(numericIcaoTimeout);
            IgnoreAircraftWithNoPosition = BindProperty<bool>(checkBoxIgnoreAircraftWithNoPosition);
            ReceiverIds = BindListProperty<int>(listReceivers);
        }

        protected override void CopyRecordToObservables()
        {
            RecordEnabled.Value =                   MergedFeed.Enabled;
            RecordName.Value =                      MergedFeed.Name;
            IcaoTimeout.Value =                     ((double)MergedFeed.IcaoTimeout) / 1000.0;
            IgnoreAircraftWithNoPosition.Value =    MergedFeed.IgnoreAircraftWithNoPosition;
            ReceiverIds.SetListValue(MergedFeed.ReceiverIds);
        }

        protected override void CopyObservablesToRecord()
        {
            MergedFeed.Enabled =                        RecordEnabled.Value;
            MergedFeed.Name =                           RecordName.Value;
            MergedFeed.IcaoTimeout =                    (int)(IcaoTimeout.Value * 1000.0);
            MergedFeed.IgnoreAircraftWithNoPosition =   IgnoreAircraftWithNoPosition.Value;
            MergedFeed.ReceiverIds.Clear();
            MergedFeed.ReceiverIds.AddRange(ReceiverIds.Value);
        }

        protected override void InitialiseControls()
        {
            listReceivers.MasterList = OptionsView.PageReceivers.Receivers;
            listReceivers.MapFromCheckedItemToRecord = r => {
                return OptionsView.PageReceivers.Receivers.Value.FirstOrDefault(i => i.UniqueId == (int)r);
            };
            listReceivers.MapFromRecordToCheckedItem = r => {
                return ((Receiver)r).UniqueId;
            };
        }

        public override void PageSelected()
        {
            listReceivers.RefreshContent();
        }

        private void listReceivers_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            var receiver = (Receiver)e.Record;
            if(receiver != null) {
                e.ColumnTexts.Add(receiver.Name);
                e.ColumnTexts.Add(receiver.Enabled ? Strings.Yes : Strings.No);
            }
        }
    }
}
