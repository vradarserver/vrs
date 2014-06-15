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
using VirtualRadar.Resources;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The parent page that controls the editing of a list of rebroadcast servers.
    /// </summary>
    public partial class ParentPageRebroadcastServers : ParentPage
    {
        #region Fields
        private List<RebroadcastSettings> _Records;
        #endregion

        #region Properties
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string PageTitle { get { return Strings.RebroadcastServersTitle; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image Icon { get { return Images.Rebroadcast16x16; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ParentPageRebroadcastServers() : base()
        {
            InitializeComponent();
        }
        #endregion

        public override void SetInitialValues()
        {
            base.SetInitialValues();
        }

        #region Page methods - DoPopulate, DoSynchroniseValues etc.
        /// <summary>
        /// Populates the control.
        /// </summary>
        /// <param name="optionsView"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override int DoPopulate(OptionsPropertySheetView optionsView, List<ISheet> result)
        {
            _Records = optionsView.RebroadcastSettings;
            foreach(var record in _Records) {
                result.Add(CreateSheet(record));
            }

            return _Records.Count == 0 ? 1 : _Records.Max(r => r.UniqueId) + 1;
        }

        /// <summary>
        /// Creates a new sheet for a record.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private ISheet CreateSheet(RebroadcastSettings record)
        {
            return new SheetRebroadcastServerOptions() {
                Tag = record,
                Enabled = record.Enabled,
                Name = record.Name,
                Format = record.Format,
                Port = record.Port,
                ReceiverId = record.ReceiverId,
                StaleSeconds = record.StaleSeconds,
            };
        }

        /// <summary>
        /// Synchronises the values between a sheet and record.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected override void DoSynchroniseValues(ISheet ambiguousSheet)
        {
            var sheet = (SheetRebroadcastServerOptions)ambiguousSheet;
            var record = (RebroadcastSettings)sheet.Tag;
            record.Enabled = sheet.Enabled;
            record.Name = sheet.Name;
            record.Format = sheet.Format;
            record.Port = sheet.Port;
            record.ReceiverId = sheet.ReceiverId;
            record.StaleSeconds = sheet.StaleSeconds;
        }

        /// <summary>
        /// Removes the record for a sheet.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected override void DoRemoveRecordForSheet(ISheet ambiguousSheet)
        {
            var sheet = (SheetRebroadcastServerOptions)ambiguousSheet;
            var record = (RebroadcastSettings)sheet.Tag;
            _Records.Remove(record);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoUpdateViewWithNewList()
        {
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when the new button is clicked - creates a new record and sheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNew_Click(object sender, EventArgs e)
        {
            var record = new RebroadcastSettings() {
                UniqueId = GenerateUniqueId(_Records, r => r.UniqueId),
                Enabled = true,
                Format = RebroadcastFormat.Port30003,
                Name = GenerateUniqueName(_Records, "New Server", false, r => r.Name),
                Port = GenerateUniquePort(_Records, 33001),
            };
            _Records.Add(record);
            ShowNewRecord(CreateSheet(record));
        }

        private int GenerateUniquePort(List<RebroadcastSettings> records, int startPort)
        {
            int result = startPort;
            for(;result < 65536 && records.Any(r => r.Port == result);++result) ;

            return result;
        }
        #endregion
    }
}
