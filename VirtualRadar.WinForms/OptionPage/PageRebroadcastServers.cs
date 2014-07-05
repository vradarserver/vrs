using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageRebroadcastServers : Page
    {
        private RecordListHelper<RebroadcastSettings, PageRebroadcastServer> _ListHelper;

        public override string PageTitle { get { return Strings.RebroadcastServersTitle; } }

        public override Image PageIcon { get { return Images.Rebroadcast16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public ObservableList<RebroadcastSettings> RebroadcastServers { get; private set; }

        public PageRebroadcastServers()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            RebroadcastServers = BindListProperty<RebroadcastSettings>(listRebroadcastServers);
            _ListHelper = new RecordListHelper<RebroadcastSettings,PageRebroadcastServer>(this, listRebroadcastServers, RebroadcastServers);
        }

        #region Receivers list handling
        private void listRebroadcastServers_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            var record = (RebroadcastSettings)e.Record;

            if(record != null) {
                var receiver = OptionsView.CombinedFeeds.Value.FirstOrDefault(r => r.UniqueId == record.ReceiverId);

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
            _ListHelper.AddClicked(() => new RebroadcastSettings() {
                UniqueId = GenerateUniqueId<RebroadcastSettings>(1, RebroadcastServers.Value, r => r.UniqueId),
                Enabled = true,
                Format = RebroadcastFormat.Port30003,
                Name = GenerateUniqueName(RebroadcastServers.Value, "New Server", false, r => r.Name),
                Port = GenerateUniquePort(RebroadcastServers.Value, 33001, r => r.Port),
            });
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
            _ListHelper.SetEnabledForListCheckedChanged(e, r => r.RecordEnabled);
        }

        protected override Page CreatePageForNewChildRecord(IObservableList observableList, object record)
        {
            return _ListHelper.CreatePageForNewChildRecord(observableList, record);
        }
        #endregion
    }
}
