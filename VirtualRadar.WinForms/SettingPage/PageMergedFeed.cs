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
using VirtualRadar.WinForms.PortableBinding;

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
        /// <param name="record"></param>
        /// <returns></returns>
        internal override bool IsForSameRecord(object record)
        {
            var mergedFeed = record as MergedFeed;
            return mergedFeed != null && MergedFeed != null && mergedFeed.UniqueId == MergedFeed.UniqueId;
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

            AddControlBinder(new CheckBoxBoolBinder<MergedFeed>(MergedFeed, checkBoxEnabled,                        r => r.Enabled,                         (r,v) => r.Enabled = v));
            AddControlBinder(new CheckBoxBoolBinder<MergedFeed>(MergedFeed, checkBoxIgnoreAircraftWithNoPosition,   r => r.IgnoreAircraftWithNoPosition,    (r,v) => r.IgnoreAircraftWithNoPosition = v));

            AddControlBinder(new TextBoxStringBinder<MergedFeed>(MergedFeed,    textBoxName,    r => r.Name,    (r,v) => r.Name = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new NumericIntBinder<MergedFeed>(MergedFeed,   numericIcaoTimeout, r => r.IcaoTimeout / 1000, (r,v) => r.IcaoTimeout = v * 1000) { ModelPropertyName = PropertyHelper.ExtractName<MergedFeed>(r => r.IcaoTimeout) });

            AddControlBinder(new MasterListToSubsetBinder<MergedFeed, Configuration, Receiver, int>(MergedFeed, listReceiverIds, SettingsView.Configuration, r => r.ReceiverIds, r => r.Receivers, r => r.UniqueId) {
                FetchColumns = (receiver, e) => {
                    e.ColumnTexts.Add(receiver.Name);
                    e.ColumnTexts.Add(receiver.Enabled ? Strings.Yes : Strings.No);
                },
            });

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
    }
}
