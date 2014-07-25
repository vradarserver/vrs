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

        /// <summary>
        /// See base docs.
        /// </summary>
        public override string PageTitle { get { return Strings.Receivers; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image PageIcon { get { return Images.Radio16x16; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool PageUseFullHeight { get { return true; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageReceivers()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateChildPages()
        {
            base.AssociateChildPages();
            AssociateListWithChildPages(SettingsView.Configuration.Receivers, () => new PageReceiver());
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            comboBoxWebSiteReceiverId.DataSource =          CreateSortingBindingSource(SettingsView.CombinedFeed, r => r.Name);
            comboBoxClosestAircraftReceiverId.DataSource =  CreateSortingBindingSource(SettingsView.CombinedFeed, r => r.Name);
            comboBoxFsxReceiverId.DataSource =              CreateSortingBindingSource(SettingsView.CombinedFeed, r => r.Name);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, comboBoxWebSiteReceiverId,         r => r.Configuration.GoogleMapSettings.WebSiteReceiverId,           r => r.SelectedValue);
            AddBinding(SettingsView, comboBoxClosestAircraftReceiverId, r => r.Configuration.GoogleMapSettings.ClosestAircraftReceiverId,   r => r.SelectedValue);
            AddBinding(SettingsView, comboBoxFsxReceiverId,             r => r.Configuration.GoogleMapSettings.FlightSimulatorXReceiverId,  r => r.SelectedValue);

            _ListHelper = new RecordListHelper<Receiver,PageReceiver>(this, listReceivers, SettingsView.Configuration.Receivers);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
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

        /// <summary>
        /// See base docs.
        /// </summary>
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
            _ListHelper.CheckedChanged(e, (r, enabled) => r.Enabled = enabled);
        }
        #endregion
    }
}
