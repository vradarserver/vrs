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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Shows the full list of merged feeds to the user.
    /// </summary>
    public partial class PageMergedFeeds : Page
    {
        private RecordListHelper<MergedFeed, PageMergedFeed> _ListHelper;

        /// <summary>
        /// See base docs.
        /// </summary>
        public override string PageTitle { get { return Strings.MergedFeeds; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image PageIcon { get { return Images.MergedFeed16x16; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool PageUseFullHeight { get { return true; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageMergedFeeds()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateChildPages()
        {
            base.AssociateChildPages();
            AssociateListWithChildPages(SettingsView.Configuration.MergedFeeds, () => new PageMergedFeed());
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            _ListHelper = new RecordListHelper<MergedFeed,PageMergedFeed>(this, listMergedFeeds, SettingsView.Configuration.MergedFeeds);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(listMergedFeeds, "", "");
        }

        #region Merged feed list handling
        private void listMergedFeeds_FetchRecordContent(object sender, Controls.BindingListView.RecordContentEventArgs e)
        {
            var record = (MergedFeed)e.Record;

            if(record != null) {
                e.Checked = record.Enabled;
                e.ColumnTexts.Add(record.Name);
                e.ColumnTexts.Add(record.ReceiverIds.Count.ToString());
                e.ColumnTexts.Add((((double)record.IcaoTimeout) / 1000.0).ToString("N2"));
                e.ColumnTexts.Add(record.IgnoreAircraftWithNoPosition ? Strings.Yes : Strings.No);
            }
        }

        private void listMergedFeeds_AddClicked(object sender, EventArgs e)
        {
            _ListHelper.AddClicked(() => SettingsView.CreateMergedFeed());
        }

        private void listMergedFeeds_DeleteClicked(object sender, EventArgs e)
        {
            _ListHelper.DeleteClicked();
        }

        private void listMergedFeeds_EditClicked(object sender, EventArgs e)
        {
            _ListHelper.EditClicked();
        }

        private void listMergedFeeds_CheckedChanged(object sender, BindingListView.RecordCheckedEventArgs e)
        {
            _ListHelper.CheckedChanged(e, (mergedFeed, enabled) => mergedFeed.Enabled = enabled);
        }
        #endregion
    }
}
