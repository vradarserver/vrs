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
        #region PageSummary
        /// <summary>
        /// The page summary object.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = ResourceImages.MergedFeed16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return _PageIcon; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public MergedFeed MergedFeed { get { return PageObject as MergedFeed; } }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            public Summary() : base()
            {
                SetPageTitleProperty<MergedFeed>(r => r.Name, () => MergedFeed.Name);
                SetPageEnabledProperty<MergedFeed>(r => r.Enabled, () => MergedFeed.Enabled);
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageMergedFeed();
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
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageMergedFeed;
                SetValidationFields(new Dictionary<ValidationField,Control>() {
                    { ValidationField.Name,         page == null ? null : page.textBoxName },
                    { ValidationField.IcaoTimeout,  page == null ? null : page.numericIcaoTimeout },
                    { ValidationField.ReceiverIds,  page == null ? null : page.listReceiverIds },
                });
            }
        }
        #endregion

        /// <summary>
        /// See base docs.
        /// </summary>
        public MergedFeed MergedFeed { get { return ((Summary)PageSummary).MergedFeed; } }

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
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();

            AddControlBinder(new CheckBoxBoolBinder<MergedFeed>(MergedFeed, checkBoxEnabled,                        r => r.Enabled,                                         (r,v) => r.Enabled = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new CheckBoxBoolBinder<MergedFeed>(MergedFeed, checkBoxIgnoreAircraftWithNoPosition,   r => r.IgnoreAircraftWithNoPosition,                    (r,v) => r.IgnoreAircraftWithNoPosition = v));
            AddControlBinder(new CheckBoxBoolBinder<MergedFeed>(MergedFeed, checkBoxHideFromWebSite,                r => r.ReceiverUsage == ReceiverUsage.HideFromWebSite,  (r,v) => r.ReceiverUsage = v ? ReceiverUsage.HideFromWebSite : ReceiverUsage.Normal) { ModelPropertyName = nameof(MergedFeed.ReceiverUsage) });

            AddControlBinder(new CheckBoxBoolBinder<MergedFeedReceiver>(null, checkBoxIsMlatFeed, r => r.IsMlatFeed, (r,v) => r.IsMlatFeed = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new TextBoxStringBinder<MergedFeed>(MergedFeed,    textBoxName,    r => r.Name,    (r,v) => r.Name = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new NumericIntBinder<MergedFeed>(MergedFeed,   numericIcaoTimeout, r => r.IcaoTimeout / 1000, (r,v) => r.IcaoTimeout = v * 1000) { ModelPropertyName = nameof(MergedFeed.IcaoTimeout) });

            AddControlBinder(new MasterListToSubsetBinder<MergedFeed, Configuration, Receiver, int>(MergedFeed, listReceiverIds, SettingsView.Configuration, r => r.ReceiverIds, r => r.Receivers, r => r.UniqueId) {
                FetchColumns = (receiver, e) => {
                    var mergedFeedReceiver = MergedFeed.ReceiverFlags.FirstOrDefault(r => r.UniqueId == receiver.UniqueId);

                    e.ColumnTexts.Add(receiver.Name);
                    e.ColumnTexts.Add(receiver.Enabled ? Strings.Yes : Strings.No);
                    e.ColumnTexts.Add(mergedFeedReceiver == null || !mergedFeedReceiver.IsMlatFeed ? Strings.No : Strings.Yes);
                },
            });

            BindSelectedMergedFeedReceiver();
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
            SetInlineHelp(checkBoxIsMlatFeed,                   Strings.MLAT,                           Strings.OptionsDescribeIsMlatFeed);
        }

        private void SynchroniseReceiverIdsToFlags()
        {
            var addFlags = MergedFeed.ReceiverIds.Where(r => !MergedFeed.ReceiverFlags.Any(i => i.UniqueId == r)).ToArray();
            var delFlags = MergedFeed.ReceiverFlags.Where(r => !MergedFeed.ReceiverIds.Any(i => r.UniqueId == i)).ToArray();

            foreach(var addFlag in addFlags) {
                MergedFeed.ReceiverFlags.Add(new MergedFeedReceiver() {
                    UniqueId = addFlag,
                });
            }

            foreach(var delFlag in delFlags) {
                MergedFeed.ReceiverFlags.Remove(delFlag);
            }

            if(delFlags.Length > 0) {
                listReceiverIds.RefreshList();
            }
        }

        private void BindSelectedMergedFeedReceiver()
        {
            var binder = (CheckBoxBoolBinder<MergedFeedReceiver>)GetControlBinders().Single(r => r.ControlObject == checkBoxIsMlatFeed);
            MergedFeedReceiver model = null;

            var selectedRecords = listReceiverIds.SelectedRecords.ToArray();
            if(selectedRecords.Length == 1) {
                SynchroniseReceiverIdsToFlags();

                var receiver = (Receiver)selectedRecords[0];
                model = MergedFeed.ReceiverFlags.FirstOrDefault(r => r.UniqueId == receiver.UniqueId);
            }

            binder.Model = model;
            checkBoxIsMlatFeed.Enabled = model != null;
            if(model == null) checkBoxIsMlatFeed.Checked = false;
        }

        private void listReceiverIds_SelectedRecordChanged(object sender, EventArgs e)
        {
            BindSelectedMergedFeedReceiver();
        }

        internal override void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
            base.ConfigurationChanged(args);

            if(args.Record == MergedFeed) {
                if(args.PropertyName == nameof(MergedFeed.ReceiverIds)) {
                    SynchroniseReceiverIdsToFlags();
                    BindSelectedMergedFeedReceiver();
                }
            } else {
                var mergedFeedReceiver = args.Record as MergedFeedReceiver;
                if(MergedFeed.ReceiverFlags.Contains(mergedFeedReceiver)) {
                    listReceiverIds.RefreshList();
                }
            }
        }
    }
}
