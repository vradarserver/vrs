// Copyright © 2013 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;
using VirtualRadar.Resources;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The parent page that controls the editing of receivers.
    /// </summary>
    public partial class ParentPageReceivers : FeedParentPage
    {
        #region Fields
        private List<Receiver> _Records;
        #endregion

        #region Properties
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string PageTitle { get { return Strings.Receivers; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image Icon { get { return Images.iconmonstr_radio_3_icon; } }

        public int WebSiteReceiverId
        {
            get { return feedWebSiteReceiverId.SelectedFeedId; }
            set { feedWebSiteReceiverId.SelectedFeedId = value; }
        }

        public int ClosestAircraftReceiverId
        {
            get { return feedClosestAircaftReceiverId.SelectedFeedId; }
            set { feedClosestAircaftReceiverId.SelectedFeedId = value; }
        }

        public int FlightSimulatorXReceiverId
        {
            get { return feedFlightSimulatorXReceiverId.SelectedFeedId; }
            set { feedFlightSimulatorXReceiverId.SelectedFeedId = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ParentPageReceivers() : base()
        {
            InitializeComponent();
        }
        #endregion

        #region Page methods - DoPopulate, DoSynchroniseValues etc.
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="optionsView"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override int DoPopulate(OptionsPropertySheetView optionsView, List<ISheet> result)
        {
            _Records = optionsView.Receivers;
            foreach(var record in _Records) {
                result.Add(CreateSheet(record));
            }

            return _Records.Count == 0 ? 1 : _Records.Max(r => r.UniqueId) + 1;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override void SetInitialValues()
        {
            ;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        protected override string DoGetSettingButtonText()
        {
            return Strings.TestConnection;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected override void DoSettingButtonClicked(ISheet ambiguousSheet)
        {
            var sheet = (SheetReceiverOptions)ambiguousSheet;
            var tag = (Receiver)sheet.Tag;
            OptionsView.OnTestBaseStationConnectionSettingsClicked(new EventArgs<Receiver>(tag));
        }

        private ISheet CreateSheet(Receiver record)
        {
            return new SheetReceiverOptions() {
                Tag = record,

                Address =                   record.Address,
                AutoReconnectAtStartup =    record.AutoReconnectAtStartup,
                BaudRate =                  record.BaudRate,
                ComPort =                   record.ComPort,
                ConnectionType =            record.ConnectionType,
                DataBits =                  record.DataBits,
                DataSource =                record.DataSource,
                Enabled =                   record.Enabled,
                Handshake =                 record.Handshake,
                Name =                      record.Name,
                Parity =                    record.Parity,
                Port =                      record.Port,
                ReceiverLocationId =        record.ReceiverLocationId,
                ShutdownText =              record.ShutdownText,
                StartupText =               record.StartupText,
                StopBits =                  record.StopBits,
            };
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected override void DoSynchroniseValues(ISheet ambiguousSheet)
        {
            var sheet = (SheetReceiverOptions)ambiguousSheet;
            var record = (Receiver)sheet.Tag;

            record.Address =                   sheet.Address;
            record.AutoReconnectAtStartup =    sheet.AutoReconnectAtStartup;
            record.BaudRate =                  sheet.BaudRate;
            record.ComPort =                   sheet.ComPort;
            record.ConnectionType =            sheet.ConnectionType;
            record.DataBits =                  sheet.DataBits;
            record.DataSource =                sheet.DataSource;
            record.Enabled =                   sheet.Enabled;
            record.Handshake =                 sheet.Handshake;
            record.Name =                      sheet.Name;
            record.Parity =                    sheet.Parity;
            record.Port =                      sheet.Port;
            record.ReceiverLocationId =        sheet.ReceiverLocationId;
            record.ShutdownText =              sheet.ShutdownText;
            record.StartupText =               sheet.StartupText;
            record.StopBits =                  sheet.StopBits;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected override void DoRemoveRecordForSheet(ISheet ambiguousSheet)
        {
            var sheet = (SheetReceiverOptions)ambiguousSheet;
            var record = (Receiver)sheet.Tag;
            _Records.Remove(record);
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                AddLocalisedDescription(feedWebSiteReceiverId,          Strings.WebSiteReceiverId,          Strings.OptionsDescribeWebSiteReceiverId);
                AddLocalisedDescription(feedClosestAircaftReceiverId,   Strings.ClosestAircraftReceiverId,  Strings.OptionsDescribeClosestAircraftReceiverId);
                AddLocalisedDescription(feedFlightSimulatorXReceiverId, Strings.FlightSimulatorXReceiverId, Strings.OptionsDescribeFlightSimulatorXReceiverId);

                RaisesValueChanged(
                    feedWebSiteReceiverId,
                    feedClosestAircaftReceiverId,
                    feedFlightSimulatorXReceiverId
                );

                _ValidationHelper.RegisterValidationField(ValidationField.WebSiteReceiver, feedWebSiteReceiverId);
                _ValidationHelper.RegisterValidationField(ValidationField.ClosestAircraftReceiver, feedClosestAircaftReceiverId);
                _ValidationHelper.RegisterValidationField(ValidationField.FlightSimulatorXReceiver, feedFlightSimulatorXReceiverId);
            }
        }

        /// <summary>
        /// Called when the new button is clicked - creates a new record and sheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNew_Click(object sender, EventArgs e)
        {
            var record = new Receiver() {
                Enabled = true,
                UniqueId = GenerateUniqueId(),
                Name = GenerateUniqueName(_Records, "Receiver", false, r => r.Name),
            };
            _Records.Add(record);
            ShowNewRecord(CreateSheet(record));
        }
        #endregion
    }
}
