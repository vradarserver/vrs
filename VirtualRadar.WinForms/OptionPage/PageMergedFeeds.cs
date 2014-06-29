using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageMergedFeeds : Page
    {
        public override string PageTitle { get { return Strings.MergedFeeds; } }

        public override Image PageIcon { get { return Images.MergedFeed16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public ObservableList<MergedFeed> MergedFeeds { get; private set; }

        public PageMergedFeeds()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            MergedFeeds = BindListProperty<MergedFeed>(listMergedFeeds);
        }

        protected override Page CreatePageForNewChildRecord(IObservableList observableList, object record)
        {
            Page result = null;
            if(observableList == MergedFeeds) result = new PageMergedFeed();

            return result;
        }

        private void listMergedFeeds_FetchRecordContent(object sender, Controls.BindingListView.RecordContentEventArgs e)
        {
            var record = (MergedFeed)e.Record;

            if(record != null) {
                e.Checked = record.Enabled;
                e.ColumnTexts.Add(record.Name);
                e.ColumnTexts.Add(record.ReceiverIds.Count.ToString());
                e.ColumnTexts.Add((((double)record.IcaoTimeout) / 1000.0).ToString("N1"));
                e.ColumnTexts.Add(record.IgnoreAircraftWithNoPosition ? Strings.No : Strings.Yes);
            }
        }

        private void listMergedFeeds_AddClicked(object sender, EventArgs e)
        {
            var record = new MergedFeed() {
                UniqueId = GenerateUniqueId(OptionsView.HighestConfiguredFeedId + 1, OptionsView.CombinedFeeds.Value, r => r.UniqueId),
                Name = GenerateUniqueName(MergedFeeds.Value, "Merged Feed", false, r => r.Name),
            };
            MergedFeeds.Value.Add(record);

            listMergedFeeds.SelectedRecord = record;
            OptionsView.DisplayPageForPageObject(record);
        }

        private void listMergedFeeds_DeleteClicked(object sender, EventArgs e)
        {
            var deleteRecords = listMergedFeeds.SelectedRecords.OfType<MergedFeed>().ToArray();
            foreach(var deleteRecord in deleteRecords) {
                MergedFeeds.Value.Remove(deleteRecord);
            }
        }

        private void listMergedFeeds_EditClicked(object sender, EventArgs e)
        {
            var record = listMergedFeeds.SelectedRecord as MergedFeed;
            if(record != null) OptionsView.DisplayPageForPageObject(record);
        }

        private void listMergedFeeds_CheckedChanged(object sender, Controls.BindingListView.RecordCheckedEventArgs e)
        {
            var page = OptionsView.FindPageForPageObject(e.Record) as PageMergedFeed;
            if(page != null && page.RecordEnabled.Value != e.Checked) {
                page.RecordEnabled.Value = e.Checked;
            }
        }
    }
}
