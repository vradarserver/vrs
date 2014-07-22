using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Shows the user all of the rebroadcast servers currently configured, lets them add new
    /// ones, remove existing ones etc.
    /// </summary>
    public partial class PageRebroadcastServers : Page
    {
        private RecordListHelper<RebroadcastSettings, PageRebroadcastServer> _ListHelper;

        public override string PageTitle { get { return Strings.RebroadcastServersTitle; } }

        public override Image PageIcon { get { return Images.Rebroadcast16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageRebroadcastServers()
        {
            InitializeComponent();
        }

        protected override void AssociateChildPages()
        {
            base.AssociateChildPages();
            AssociateListWithChildPages(SettingsView.Configuration.RebroadcastSettings, () => new PageRebroadcastServer());
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            _ListHelper = new RecordListHelper<RebroadcastSettings,PageRebroadcastServer>(this, listRebroadcastServers, SettingsView.Configuration.RebroadcastSettings);
        }

        #region Rebroadcast server list handling
        private void listRebroadcastServers_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            var record = (RebroadcastSettings)e.Record;

            if(record != null) {
                var receiver = SettingsView.CombinedFeed.FirstOrDefault(r => r.UniqueId == record.ReceiverId);

                e.Checked = record.Enabled;
                e.ColumnTexts.Add(record.Name);
                e.ColumnTexts.Add(receiver == null ? "" : receiver.Name ?? "");
                e.ColumnTexts.Add(Describe.RebroadcastFormat(record.Format));
                e.ColumnTexts.Add(record.Port.ToString());
                e.ColumnTexts.Add(record.StaleSeconds.ToString());
            }
        }

        private void listRebroadcastServers_AddClicked(object sender, EventArgs e)
        {
            _ListHelper.AddClicked(() => SettingsView.CreateRebroadcastServer());
        }

        private void listRebroadcastServers_DeleteClicked(object sender, EventArgs e)
        {
            _ListHelper.DeleteClicked();
        }

        private void listRebroadcastServers_EditClicked(object sender, EventArgs e)
        {
            _ListHelper.EditClicked();
        }

        private void listRebroadcastServers_CheckedChanged(object sender, BindingListView.RecordCheckedEventArgs e)
        {
            _ListHelper.SetEnabledForListCheckedChanged(e, (server, enabled) => server.Enabled = enabled);
        }
        #endregion
    }
}
