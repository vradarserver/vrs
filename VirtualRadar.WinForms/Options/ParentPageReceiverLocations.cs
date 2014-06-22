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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Resources;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The class that handles receiver location pages.
    /// </summary>
    public partial class ParentPageReceiverLocations : ParentPage
    {
        #region Fields
        private IList<ReceiverLocation> _Records;
        #endregion

        #region Properties
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string PageTitle { get { return Strings.ReceiverLocationsTitle; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image Icon { get { return Images.iconmonstr_location_3_icon; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ParentPageReceiverLocations()
        {
            InitializeComponent();
        }
        #endregion

        #region Page methods - DoPopulate, DoSynchroniseValues etc.
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected override int DoPopulate(List<ISheet> result)
        {
            _Records = OptionsView.RawDecodingReceiverLocations;
            foreach(var record in _Records) {
                result.Add(CreateSheet(record));
            }

            return _Records.Count != 0 ? _Records.Max(r => r.UniqueId) + 1 : 1;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private ISheet CreateSheet(ReceiverLocation record)
        {
            return new SheetReceiverLocationOptions() {
                Tag = record,
                Latitude = record.Latitude,
                Location = record.Name,
                Longitude = record.Longitude,
            };
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected override void DoSynchroniseValues(ISheet ambiguousSheet)
        {
            var sheet = (SheetReceiverLocationOptions)ambiguousSheet;
            var record = (ReceiverLocation)sheet.Tag;
            record.Latitude = sheet.Latitude;
            record.Longitude = sheet.Longitude;
            record.Name = sheet.Location;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="ambiguousSheet"></param>
        protected override void DoRemoveRecordForSheet(ISheet ambiguousSheet)
        {
            var sheet = (SheetReceiverLocationOptions)ambiguousSheet;
            var record = (ReceiverLocation)sheet.Tag;
            _Records.Remove(record);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoUpdateViewWithNewList()
        {
            foreach(var receiver in OptionsView.Receivers) {
                var rawDecodingUseStillExists = _Records.Any(r => r.UniqueId == receiver.ReceiverLocationId);
                if(!rawDecodingUseStillExists) receiver.ReceiverLocationId = 0;
            }
        }
        #endregion

        #region MergeBaseStationDatabaseReceiverLocations
        /// <summary>
        /// Merges reciever locations from a BaseStation database with those in the UI.
        /// </summary>
        /// <param name="receiverLocations"></param>
        internal void MergeBaseStationDatabaseReceiverLocations(IEnumerable<ReceiverLocation> receiverLocations)
        {
            var entriesPreviouslyCopiedFromBaseStationDatabase = _Records.Where(r => r.IsBaseStationLocation).ToList();
            _Records.RemoveAll(r => r.IsBaseStationLocation);

            foreach(var location in receiverLocations) {
                var previousEntry = entriesPreviouslyCopiedFromBaseStationDatabase.Where(r => r.Name == location.Name).FirstOrDefault();
                var newLocation = new ReceiverLocation() {
                    Name = GenerateUniqueName(_Records, location.Name, false, r => r.Name),
                    UniqueId = previousEntry != null ? previousEntry.UniqueId : GenerateUniqueId(_Records, r => r.UniqueId),
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    IsBaseStationLocation = true,
                };
                _Records.Add(newLocation);
            }
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
            var record = new ReceiverLocation() {
                UniqueId = GenerateUniqueId(_Records, r => r.UniqueId),
                Name = GenerateUniqueName(_Records, "New Location", false, r => r.Name),
                Latitude = 0.01,
            };
            _Records.Add(record);
            ShowNewRecord(CreateSheet(record));
        }

        private void linkLabelUpdateFromDatabase_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(OptionsView != null) OptionsView.OnUpdateReceiverLocationsFromBaseStationDatabaseClicked(e);
        }
        #endregion
    }
}
