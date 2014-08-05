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
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Handles the display of a merged feed's settings to the user.
    /// </summary>
    public partial class PageMergedFeed : Page
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image PageIcon { get { return Images.MergedFeed16x16; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public MergedFeed MergedFeed { get { return PageObject as MergedFeed; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool PageUseFullHeight { get { return true; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageMergedFeed()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            listReceiverIds.ListView.ListViewItemSorter = new AutoListViewSorter(listReceiverIds.ListView);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(MergedFeed, checkBoxEnabled,                         r => r.Enabled,                         r => r.Checked);
            AddBinding(MergedFeed, textBoxName,                             r => r.Name,                            r => r.Text,    DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(MergedFeed, numericIcaoTimeout,                      r => r.IcaoTimeout,                     r => r.Value,   format: IcaoTimeout_Format, parse: IcaoTimeout_Parse);
            AddBinding(MergedFeed, checkBoxIgnoreAircraftWithNoPosition,    r => r.IgnoreAircraftWithNoPosition,    r => r.Checked);

            listReceiverIds.DataSource = CreateListBindingSource<Receiver>(SettingsView.Configuration.Receivers);
            listReceiverIds.CheckedSubset = MergedFeed.ReceiverIds;
            listReceiverIds.ExtractSubsetValue = (r) => ((Receiver)r).UniqueId;

            SetPageTitleProperty<MergedFeed>(r => r.Name, () => MergedFeed.Name);
            SetPageEnabledProperty<MergedFeed>(r => r.Enabled, () => MergedFeed.Enabled);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.Name,         textBoxName },
                { ValidationField.IcaoTimeout,  numericIcaoTimeout },
                { ValidationField.ReceiverIds,  listReceiverIds },
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxEnabled,                      Strings.Enabled,                        Strings.OptionsDescribeMergedFeedEnabled);
            SetInlineHelp(textBoxName,                          Strings.Name,                           Strings.OptionsDescribeMergedFeedName);
            SetInlineHelp(numericIcaoTimeout,                   Strings.IcaoTimeout,                    Strings.OptionsDescribeIcaoTimeout);
            SetInlineHelp(checkBoxIgnoreAircraftWithNoPosition, Strings.IgnoreAircraftWithNoPosition,   Strings.OptionsDescribeIgnoreAircraftWithNoPosition);
            SetInlineHelp(listReceiverIds,                      "",                                     "");
        }

        private void IcaoTimeout_Format(object sender, ConvertEventArgs args)
        {
            var value = (int)args.Value;
            args.Value = ((decimal)value) / 1000M;
        }

        private void IcaoTimeout_Parse(object sender, ConvertEventArgs args)
        {
            var value = (decimal)args.Value;
            args.Value = (int)(value * 1000M);
        }

        private void listReceiverIds_FetchRecordContent(object sender, BindingListView.RecordContentEventArgs args)
        {
            var receiver = args.Record as Receiver;
            if(receiver != null) {
                args.ColumnTexts.Add(receiver.Name);
                args.ColumnTexts.Add(receiver.Enabled ? Strings.Yes : Strings.No);
            }
        }
    }
}
