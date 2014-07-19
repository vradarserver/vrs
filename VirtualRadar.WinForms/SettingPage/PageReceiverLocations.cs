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
    /// The parent page for all receiver locations.
    /// </summary>
    public partial class PageReceiverLocations : Page
    {
        private RecordListHelper<ReceiverLocation, PageReceiverLocation> _ListHelper;

        public override string PageTitle { get { return Strings.ReceiverLocationsTitle; } }

        public override Image PageIcon { get { return Images.Location16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageReceiverLocations()
        {
            InitializeComponent();
        }

        protected override void AssociateChildPages()
        {
            base.AssociateChildPages();
            AssociateListWithChildPages(SettingsView.Configuration.ReceiverLocations, () => new PageReceiverLocation());
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            _ListHelper = new RecordListHelper<ReceiverLocation,PageReceiverLocation>(this, listReceiverLocations, SettingsView.Configuration.ReceiverLocations);
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(listReceiverLocations, "", "");
        }

        #region ReceiverLocation list handling
        private void listReceiverLocations_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            var record = (ReceiverLocation)e.Record;

            if(record != null) {
                e.ColumnTexts.Add(record.Name);
                e.ColumnTexts.Add(record.Latitude.ToString("N6"));
                e.ColumnTexts.Add(record.Longitude.ToString("N6"));
            }
        }

        private void listReceiverLocations_AddClicked(object sender, EventArgs e)
        {
            _ListHelper.AddClicked(() => SettingsView.CreateReceiverLocation());
        }

        private void listReceiverLocations_DeleteClicked(object sender, EventArgs e)
        {
            _ListHelper.DeleteClicked();
        }

        private void listReceiverLocations_EditClicked(object sender, EventArgs e)
        {
            _ListHelper.EditClicked();
        }
        #endregion

        #region Event handlers
        private void linkLabelUpdateFromBaseStationDatabase_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SettingsView.RaiseUpdateReceiverLocationsFromBaseStationDatabaseClicked(e);
        }
        #endregion
    }
}
