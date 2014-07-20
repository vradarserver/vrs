using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Shows the full list of merged feeds to the user.
    /// </summary>
    public partial class PageMergedFeeds : Page
    {
        private RecordListHelper<MergedFeed, PageMergedFeed> _ListHelper;

        public override string PageTitle { get { return Strings.MergedFeeds; } }

        public override Image PageIcon { get { return Images.MergedFeed16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageMergedFeeds()
        {
            InitializeComponent();
        }

        protected override void AssociateChildPages()
        {
            base.AssociateChildPages();
            AssociateListWithChildPages(SettingsView.Configuration.MergedFeeds, () => new PageMergedFeed());
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            _ListHelper = new RecordListHelper<MergedFeed,PageMergedFeed>(this, listMergedFeeds, SettingsView.Configuration.MergedFeeds);
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(listMergedFeeds, "", "");
        }

        #region Merged feed list handling
        private void listMergedFeeds_FetchRecordContent(object sender, Controls.BindingListView.RecordContentEventArgs e)
        {
            var record = (MergedFeed)e.Record;

            if(record != null) {
                e.Checked = record.Enabled;
                e.ColumnTexts.Add(record.Name);
                e.ColumnTexts.Add(record.ReceiverIds.Count.ToString());
                e.ColumnTexts.Add((((double)record.IcaoTimeout) / 1000.0).ToString("N2"));
                e.ColumnTexts.Add(record.IgnoreAircraftWithNoPosition ? Strings.Yes : Strings.No);
            }
        }

        private void listMergedFeeds_AddClicked(object sender, EventArgs e)
        {
            _ListHelper.AddClicked(() => SettingsView.CreateMergedFeed());
        }

        private void listMergedFeeds_DeleteClicked(object sender, EventArgs e)
        {
            _ListHelper.DeleteClicked();
        }

        private void listMergedFeeds_EditClicked(object sender, EventArgs e)
        {
            _ListHelper.EditClicked();
        }

        private void listMergedFeeds_CheckedChanged(object sender, BindingListView.RecordCheckedEventArgs e)
        {
            _ListHelper.SetEnabledForListCheckedChanged(e, (mergedFeed, enabled) => mergedFeed.Enabled = enabled);
        }
        #endregion
    }
}
