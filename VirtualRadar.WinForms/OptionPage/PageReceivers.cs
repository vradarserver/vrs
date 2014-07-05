// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
using System.IO.Ports;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// The parent page for all receivers.
    /// </summary>
    public partial class PageReceivers : Page
    {
        private RecordListHelper<Receiver, PageReceiver> _ListHelper;

        public override string PageTitle { get { return Strings.Receivers; } }

        public override Image PageIcon { get { return Images.iconmonstr_radio_3_icon; } }

        public override bool PageUseFullHeight { get { return true; } }

        [ValidationField(ValidationField.WebSiteReceiver)]
        [LocalisedDisplayName("WebSiteReceiverId")]
        [LocalisedDescription("OptionsDescribeWebSiteReceiverId")]
        public Observable<int> WebSiteReceiverId { get; private set; }

        [ValidationField(ValidationField.ClosestAircraftReceiver)]
        [LocalisedDisplayName("ClosestAircraftReceiverId")]
        [LocalisedDescription("OptionsDescribeClosestAircraftReceiverId")]
        public Observable<int> ClosestAircraftReceiverId { get; private set; }

        [ValidationField(ValidationField.FlightSimulatorXReceiver)]
        [LocalisedDisplayName("FlightSimulatorXReceiverId")]
        [LocalisedDescription("OptionsDescribeFlightSimulatorXReceiverId")]
        public Observable<int> FlightSimulatorXReceiverId { get; private set; }

        [ValidationField(ValidationField.ReceiverIds)]
        public ObservableList<Receiver> Receivers { get; private set; }

        public PageReceivers()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            WebSiteReceiverId = BindProperty<int>(comboBoxWebSiteReceiverId);
            ClosestAircraftReceiverId = BindProperty<int>(comboBoxClosestAircraftReceiverId);
            FlightSimulatorXReceiverId = BindProperty<int>(comboBoxFsxReceiverId);
            Receivers = BindListProperty<Receiver>(listReceivers);

            _ListHelper = new RecordListHelper<Receiver,PageReceiver>(this, listReceivers, Receivers);
        }

        protected override void InitialiseControls()
        {
            comboBoxWebSiteReceiverId.ObservableList =          OptionsView.CombinedFeeds;
            comboBoxClosestAircraftReceiverId.ObservableList =  OptionsView.CombinedFeeds;
            comboBoxFsxReceiverId.ObservableList =              OptionsView.CombinedFeeds;
        }

        public override void PageSelected()
        {
            base.PageSelected();

            comboBoxWebSiteReceiverId.RefreshContent();
            comboBoxClosestAircraftReceiverId.RefreshContent();
            comboBoxFsxReceiverId.RefreshContent();
        }

        #region Receivers list handling
        private void listReceivers_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            var receiver = (Receiver)e.Record;

            if(receiver != null) {
                var location = OptionsView == null ? null : OptionsView.RawDecodingReceiverLocations.FirstOrDefault(r => r.UniqueId == receiver.ReceiverLocationId);

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
            _ListHelper.AddClicked(() => new Receiver() {
                Enabled = true,
                UniqueId = GenerateUniqueId(OptionsView.HighestConfiguredFeedId + 1, OptionsView.CombinedFeeds.Value, r => r.UniqueId),
                Name = GenerateUniqueName(Receivers.Value, "Receiver", false, r => r.Name),
            });
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
            _ListHelper.SetEnabledForListCheckedChanged(e, r => r.RecordEnabled);
        }

        protected override Page CreatePageForNewChildRecord(IObservableList observableList, object record)
        {
            return _ListHelper.CreatePageForNewChildRecord(observableList, record);
        }
        #endregion
    }
}
