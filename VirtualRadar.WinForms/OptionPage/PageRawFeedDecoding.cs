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
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms.Binding;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageRawFeedDecoding : Page
    {
        public override string PageTitle { get { return Strings.OptionsRawFeedSheetTitle; } }

        public override Image PageIcon { get { return Images.Decoding16x16; } }

        [LocalisedDisplayName("ReceiverRange")]
        [LocalisedDescription("OptionsDescribeRawFeedReceiverRange")]
        [ValidationField(ValidationField.ReceiverRange)]
        public Observable<int> ReceiverRange { get; private set; }

        [LocalisedDisplayName("SuppressReceiverRangeCheck")]
        [LocalisedDescription("OptionsDescribeRawFeedSuppressReceiverRangeCheck")]
        public Observable<bool> SuppressReceiverRangeCheck { get; private set; }

        [LocalisedDisplayName("IgnoreMilitaryExtendedSquitter")]
        [LocalisedDescription("OptionsDescribeRawFeedIgnoreMilitaryExtendedSquitter")]
        public Observable<bool> IgnoreMilitaryExtendedSquitter { get; private set; }

        [LocalisedDisplayName("UseLocalDecodeForInitialPosition")]
        [LocalisedDescription("OptionsDescribeRawFeedUseLocalDecodeForInitialPosition")]
        public Observable<bool> UseLocalDecodeForInitialPosition { get; private set; }

        [LocalisedDisplayName("AirborneGlobalPositionLimit")]
        [LocalisedDescription("OptionsDescribeRawFeedAirborneGlobalPositionLimit")]
        [ValidationField(ValidationField.AirborneGlobalPositionLimit)]
        public Observable<int> AirborneGlobalPositionLimit { get; private set; }

        [LocalisedDisplayName("FastSurfaceGlobalPositionLimit")]
        [LocalisedDescription("OptionsDescribeRawFeedFastSurfaceGlobalPositionLimit")]
        [ValidationField(ValidationField.FastSurfaceGlobalPositionLimit)]
        public Observable<int> FastSurfaceGlobalPositionLimit { get; private set; }

        [LocalisedDisplayName("SlowSurfaceGlobalPositionLimit")]
        [LocalisedDescription("OptionsDescribeRawFeedSlowSurfaceGlobalPositionLimit")]
        [ValidationField(ValidationField.SlowSurfaceGlobalPositionLimit)]
        public Observable<int> SlowSurfaceGlobalPositionLimit { get; private set; }

        [LocalisedDisplayName("AcceptableAirborneSpeed")]
        [LocalisedDescription("OptionsDescribeRawFeedAcceptableAirborneSpeed")]
        [ValidationField(ValidationField.AcceptableAirborneLocalPositionSpeed)]
        public Observable<double> AcceptableAirborneSpeed { get; private set; }

        [LocalisedDisplayName("AcceptableAirSurfaceTransitionSpeed")]
        [LocalisedDescription("OptionsDescribeRawFeedAcceptableAirSurfaceTransitionSpeed")]
        [ValidationField(ValidationField.AcceptableTransitionLocalPositionSpeed)]
        public Observable<double> AcceptableAirSurfaceTransitionSpeed { get; private set; }

        [LocalisedDisplayName("AcceptableSurfaceSpeed")]
        [LocalisedDescription("OptionsDescribeRawFeedAcceptableSurfaceSpeed")]
        [ValidationField(ValidationField.AcceptableSurfaceLocalPositionSpeed)]
        public Observable<double> AcceptableSurfaceSpeed { get; private set; }

        [LocalisedDisplayName("IgnoreCallsignsInBds20")]
        [LocalisedDescription("OptionsDescribeRawFeedIgnoreCallsignsInBds20")]
        public Observable<bool> IgnoreCallsignsInBds20 { get; private set; }

        [LocalisedDisplayName("AcceptIcaoInPI0Count")]
        [LocalisedDescription("OptionsDescribeRawFeedAcceptIcaoInPI0Count")]
        [ValidationField(ValidationField.AcceptIcaoInPI0Count)]
        public Observable<int> AcceptIcaoInPI0Count { get; private set; }

        [LocalisedDisplayName("AcceptIcaoInPI0Seconds")]
        [LocalisedDescription("OptionsDescribeRawFeedAcceptIcaoInPI0Seconds")]
        [ValidationField(ValidationField.AcceptIcaoInPI0Seconds)]
        public Observable<int> AcceptIcaoInPI0Seconds { get; private set; }

        [LocalisedDisplayName("AcceptIcaoInNonPICount")]
        [LocalisedDescription("OptionsDescribeRawFeedAcceptIcaoInNonPICount")]
        [ValidationField(ValidationField.AcceptIcaoInNonPICount)]
        public Observable<int> AcceptIcaoInNonPICount { get; private set; }

        [LocalisedDisplayName("AcceptIcaoInNonPISeconds")]
        [LocalisedDescription("OptionsDescribeRawFeedAcceptIcaoInNonPISeconds")]
        [ValidationField(ValidationField.AcceptIcaoInNonPISeconds)]
        public Observable<int> AcceptIcaoInNonPISeconds { get; private set; }

        public PageRawFeedDecoding()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            ReceiverRange = BindProperty<int>(numericReceiverRange);
            SuppressReceiverRangeCheck = BindProperty<bool>(checkBoxSuppressReceiverRangeCheck);

            IgnoreMilitaryExtendedSquitter = BindProperty<bool>(checkBoxIgnoreMilitaryExtendedSquitter);
            UseLocalDecodeForInitialPosition = BindProperty<bool>(checkBoxUseLocalDecodeForInitialPosition);
            AirborneGlobalPositionLimit = BindProperty<int>(numericAirborneGlobalPositionLimit);
            FastSurfaceGlobalPositionLimit = BindProperty<int>(numericFastSurfaceGlobalPositionLimit);
            SlowSurfaceGlobalPositionLimit = BindProperty<int>(numericSlowSurfaceGlobalPositionLimit);
            AcceptableAirborneSpeed = BindProperty<double>(numericAcceptableAirborneSpeed);
            AcceptableAirSurfaceTransitionSpeed = BindProperty<double>(numericAcceptableAirSurfaceTransitionSpeed);
            AcceptableSurfaceSpeed = BindProperty<double>(numericAcceptableSurfaceSpeed);
            IgnoreCallsignsInBds20 = BindProperty<bool>(checkBoxIgnoreCallsignsInBds20);

            AcceptIcaoInPI0Count = BindProperty<int>(numericAcceptIcaoInPI0Count);
            AcceptIcaoInPI0Seconds = BindProperty<int>(numericAcceptIcaoInPI0Seconds);
            AcceptIcaoInNonPICount = BindProperty<int>(numericAcceptIcaoInNonPICount);
            AcceptIcaoInNonPISeconds = BindProperty<int>(numericAcceptIcaoInNonPISeconds);
        }

        private void linkLabelUseIcaoSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OptionsView.RaiseUseIcaoRawDecodingSettingsClicked(e);
        }

        private void linkLabelUseRecommendedSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OptionsView.RaiseUseRecommendedRawDecodingSettingsClicked(e);
        }
    }
}
