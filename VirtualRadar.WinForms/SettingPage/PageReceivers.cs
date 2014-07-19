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
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// The parent page for all receivers.
    /// </summary>
    public partial class PageReceivers : Page
    {
        private RecordListHelper<Receiver, PageReceiver> _ListHelper;

        public override string PageTitle { get { return Strings.Receivers; } }

        public override Image PageIcon { get { return Images.Radio16x16; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageReceivers()
        {
            InitializeComponent();
        }

        protected override void AssociateChildPages()
        {
            base.AssociateChildPages();
            AssociateListWithChildPages(SettingsView.Configuration.Receivers, () => new PageReceiver());
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            comboBoxWebSiteReceiverId.DataSource =          CreateSortingBindingSource(SettingsView.CombinedFeed, r => r.Name);
            comboBoxClosestAircraftReceiverId.DataSource =  CreateSortingBindingSource(SettingsView.CombinedFeed, r => r.Name);
            comboBoxFsxReceiverId.DataSource =              CreateSortingBindingSource(SettingsView.CombinedFeed, r => r.Name);
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, comboBoxWebSiteReceiverId,         r => r.Configuration.GoogleMapSettings.WebSiteReceiverId,           r => r.SelectedValue);
            AddBinding(SettingsView, comboBoxClosestAircraftReceiverId, r => r.Configuration.GoogleMapSettings.ClosestAircraftReceiverId,   r => r.SelectedValue);
            AddBinding(SettingsView, comboBoxFsxReceiverId,             r => r.Configuration.GoogleMapSettings.FlightSimulatorXReceiverId,  r => r.SelectedValue);

            _ListHelper = new RecordListHelper<Receiver,PageReceiver>(this, listReceivers, SettingsView.Configuration.Receivers);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.WebSiteReceiver,          comboBoxWebSiteReceiverId },
                { ValidationField.ClosestAircraftReceiver,  comboBoxClosestAircraftReceiverId },
                { ValidationField.FlightSimulatorXReceiver, comboBoxFsxReceiverId },
                { ValidationField.ReceiverIds,              listReceivers },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(comboBoxWebSiteReceiverId,            Strings.WebSiteReceiverId,          Strings.OptionsDescribeWebSiteReceiverId);
            SetInlineHelp(comboBoxClosestAircraftReceiverId,    Strings.ClosestAircraftReceiverId,  Strings.OptionsDescribeClosestAircraftReceiverId);
            SetInlineHelp(comboBoxFsxReceiverId,                Strings.FlightSimulatorXReceiverId, Strings.OptionsDescribeFlightSimulatorXReceiverId);
            SetInlineHelp(listReceivers,                        "",                                 "");
        }

        #region Receivers list handling
        private void listReceivers_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            var receiver = (Receiver)e.Record;

            if(receiver != null) {
                var location = SettingsView == null ? null : SettingsView.Configuration.ReceiverLocations.FirstOrDefault(r => r.UniqueId == receiver.ReceiverLocationId);

                e.Checked = receiver.Enabled;
                e.ColumnTexts.Add(receiver.Name);
                e.ColumnTexts.Add(Describe.DataSource(receiver.DataSource));
                e.ColumnTexts.Add(location == null ? "" : location.Name);
                e.ColumnTexts.Add(Describe.ConnectionType(receiver.ConnectionType));
                e.ColumnTexts.Add(DescribeConnectionParameters(receiver));
            }
        }

        private string DescribeConnectionParameters(Receiver receiver)
        {
            var result = new StringBuilder();

            switch(receiver.ConnectionType) {
                case ConnectionType.COM:
                    result.AppendFormat("{0}, {1}, {2}/{3}, {4}, {5}, \"{6}\", \"{7}\"",
                        receiver.ComPort,
                        receiver.BaudRate,
                        receiver.DataBits,
                        Describe.StopBits(receiver.StopBits),
                        Describe.Parity(receiver.Parity),
                        Describe.Handshake(receiver.Handshake),
                        receiver.StartupText,
                        receiver.ShutdownText
                    );
                    break;
                case ConnectionType.TCP:
                    result.AppendFormat("{0}:{1}",
                        receiver.Address,
                        receiver.Port
                    );
                    break;
            }

            return result.ToString();
        }

        private void listReceivers_AddClicked(object sender, EventArgs e)
        {
            if(_ListHelper != null) {
                _ListHelper.AddClicked(() => SettingsView.CreateReceiver());
            }
        }

        private void listReceivers_DeleteClicked(object sender, EventArgs e)
        {
            _ListHelper.DeleteClicked();
        }

        private void listReceivers_EditClicked(object sender, EventArgs e)
        {
            _ListHelper.EditClicked();
        }

        private void listReceivers_CheckedChanged(object sender, BindingListView.RecordCheckedEventArgs e)
        {
            _ListHelper.SetEnabledForListCheckedChanged(e, (r, enabled) => r.Enabled = enabled);
        }
        #endregion
    }
}
