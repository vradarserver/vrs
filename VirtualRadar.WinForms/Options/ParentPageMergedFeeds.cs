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

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The parent page that brings together the collection of merged feeds.
    /// </summary>
    public partial class ParentPageMergedFeeds : FeedParentPage
    {
        #region Fields
        private List<MergedFeed> _Records;
        #endregion

        #region Properties
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string PageTitle { get { return Strings.MergedFeeds; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ParentPageMergedFeeds() : base()
        {
            InitializeComponent();
        }
        #endregion

        #region Page methods - DoPopulate, DoSynchroniseValues etc.
        /// <summary>
        /// Populates the control.
        /// </summary>
        /// <param name="optionsView"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override int DoPopulate(OptionsPropertySheetView optionsView, List<ISheet> result)
        {
            _Records = optionsView.MergedFeeds;
            foreach(var record in _Records) {
                result.Add(CreateSheet(record));
            }

            return _Records.Count == 0 ? 1 : _Records.Max(r => r.UniqueId) + 1;
        }

        /// <summary>
        /// Creates a new sheet.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private ISheet CreateSheet(MergedFeed record)
        {
            var result = new SheetMergedFeedOptions() {
                Tag = record,

                Enabled =                       record.Enabled,
                IcaoTimeout =                   record.IcaoTimeout / 1000,
                IgnoreAircraftWithNoPosition =  record.IgnoreAircraftWithNoPosition,
                Name =                          record.Name,
                ReceiverIds =                   record.ReceiverIds,
            };

            return result;
        }

        /// <summary>
        /// Synchronises the values between the UI and the record.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected override void DoSynchroniseValues(ISheet ambiguousSheet)
        {
            var sheet = (SheetMergedFeedOptions)ambiguousSheet;
            var record = (MergedFeed)sheet.Tag;

            record.Enabled =                        sheet.Enabled;
            record.IcaoTimeout =                    sheet.IcaoTimeout * 1000;
            record.IgnoreAircraftWithNoPosition =   sheet.IgnoreAircraftWithNoPosition;
            record.Name =                           sheet.Name;
        }

        /// <summary>
        /// Removes the record associated with a sheet.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected override void DoRemoveRecordForSheet(ISheet ambiguousSheet)
        {
            var sheet = (SheetMergedFeedOptions)ambiguousSheet;
            var record = (MergedFeed)sheet.Tag;
            _Records.Remove(record);
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Raised when the new button is clicked - creates a new record and sheet.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNew_Click(object sender, EventArgs e)
        {
            var record = new MergedFeed() {
                Enabled = true,
                UniqueId = GenerateUniqueId(),
                Name = GenerateUniqueName(_Records, "Merged Feed", false, r => r.Name),
            };
            _Records.Add(record);
            ShowNewRecord(CreateSheet(record));
        }
        #endregion
    }
}
