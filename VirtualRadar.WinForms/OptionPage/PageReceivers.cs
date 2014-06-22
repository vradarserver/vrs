using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// The parent page for all receivers.
    /// </summary>
    public partial class PageReceivers : Page
    {
        public override string PageTitle { get { return Strings.Receivers; } }

        public override Image PageIcon { get { return Images.iconmonstr_radio_3_icon; } }

        [ValidationField(ValidationField.ReceiverIds)]
        public ObservableList<Receiver> Receivers { get; private set; }

        public PageReceivers()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            Receivers = BindListProperty<Receiver>(listReceivers);
        }

        #region Receivers list handling
        private void listReceivers_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            var receiver = (Receiver)e.Record;
            e.Checked = receiver.Enabled;

            if(receiver != null) {
                var location = OptionsView == null ? null : OptionsView.RawDecodingReceiverLocations.FirstOrDefault(r => r.UniqueId == receiver.ReceiverLocationId);

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
        #endregion

        #region Receiver sub-page handling
        protected override Page CreatePageForNewChildRecord(IObservableList observableList, object record)
        {
            Page result = null;

            if(observableList == Receivers) result = new PageReceiver();

            return result;
        }
        #endregion
    }
}
