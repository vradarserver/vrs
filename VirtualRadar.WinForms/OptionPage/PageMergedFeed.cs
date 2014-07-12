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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Resources;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// Displays and edits the settings for a merged feed.
    /// </summary>
    public partial class PageMergedFeed : Page
    {
        public override Image PageIcon { get { return Images.MergedFeed16x16; } }

        public MergedFeed MergedFeed { get { return PageObject as MergedFeed; } }

        public override bool PageUseFullHeight { get { return true; } }

        [PageEnabled]
        [LocalisedDisplayName("Enabled")]
        [LocalisedDescription("OptionsDescribeMergedFeedEnabled")]
        [ValidationField(ValidationField.Enabled)]
        public Observable<bool> RecordEnabled { get; private set; }

        [PageTitle]
        [LocalisedDisplayName("Name")]
        [LocalisedDescription("OptionsDescribeMergedFeedName")]
        [ValidationField(ValidationField.Name)]
        public Observable<string> RecordName { get; private set; }

        [LocalisedDisplayName("IcaoTimeout")]
        [LocalisedDescription("OptionsDescribeIcaoTimeout")]
        [ValidationField(ValidationField.IcaoTimeout)]
        public Observable<double> IcaoTimeout { get; private set; }

        [LocalisedDisplayName("IgnoreAircraftWithNoPosition")]
        [LocalisedDescription("OptionsDescribeIgnoreAircraftWithNoPosition")]
        public Observable<bool> IgnoreAircraftWithNoPosition { get; private set; }

        [LocalisedDisplayName("Receivers")]
        [LocalisedDescription("OptionsDescribeMergedFeedReceviers")]
        [ValidationField(ValidationField.ReceiverIds, IconAlignment=ErrorIconAlignment.TopLeft)]
        public ObservableList<int> ReceiverIds { get; private set; }

        public PageMergedFeed()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            RecordEnabled = BindProperty<bool>(checkBoxEnabled);
            RecordName = BindProperty<string>(textBoxName);
            IcaoTimeout = BindProperty<double>(numericIcaoTimeout);
            IgnoreAircraftWithNoPosition = BindProperty<bool>(checkBoxIgnoreAircraftWithNoPosition);
            ReceiverIds = BindListProperty<int>(listReceivers);
        }

        protected override void CopyRecordToObservables()
        {
            RecordEnabled.Value =                   MergedFeed.Enabled;
            RecordName.Value =                      MergedFeed.Name;
            IcaoTimeout.Value =                     ((double)MergedFeed.IcaoTimeout) / 1000.0;
            IgnoreAircraftWithNoPosition.Value =    MergedFeed.IgnoreAircraftWithNoPosition;
            ReceiverIds.SetListValue(MergedFeed.ReceiverIds);
        }

        protected override void CopyObservablesToRecord()
        {
            MergedFeed.Enabled =                        RecordEnabled.Value;
            MergedFeed.Name =                           RecordName.Value;
            MergedFeed.IcaoTimeout =                    (int)(IcaoTimeout.Value * 1000.0);
            MergedFeed.IgnoreAircraftWithNoPosition =   IgnoreAircraftWithNoPosition.Value;
            MergedFeed.ReceiverIds.Clear();
            MergedFeed.ReceiverIds.AddRange(ReceiverIds.Value);
        }

        protected override void InitialiseControls()
        {
          //  listReceivers.MasterList = OptionsView.PageReceivers.Receivers;
            listReceivers.MapFromCheckedItemToRecord = r => {
                return OptionsView.PageReceivers.Receivers.FirstOrDefault(i => i.UniqueId == (int)r);
            };
            listReceivers.MapFromRecordToCheckedItem = r => {
                return ((Receiver)r).UniqueId;
            };
        }

        public override void PageSelected()
        {
            listReceivers.RefreshContent();
        }

        private void listReceivers_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs e)
        {
            var receiver = (Receiver)e.Record;
            if(receiver != null) {
                e.ColumnTexts.Add(receiver.Name);
                e.ColumnTexts.Add(receiver.Enabled ? Strings.Yes : Strings.No);
            }
        }
    }
}
