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
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays the raw feed decoding settings to the user.
    /// </summary>
    public partial class PageRawFeedDecoding : Page
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string PageTitle { get { return Strings.OptionsRawFeedSheetTitle; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image PageIcon { get { return Images.Decoding16x16; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageRawFeedDecoding()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();

            var settings = SettingsView.Configuration.RawDecodingSettings;
            AddControlBinder(new NumericIntBinder<RawDecodingSettings>(settings, numericReceiverRange,                          r => r.ReceiverRange,                         (r,v) => r.ReceiverRange = v));
            AddControlBinder(new NumericIntBinder<RawDecodingSettings>(settings, numericAirborneGlobalPositionLimit,            r => r.AirborneGlobalPositionLimit,           (r,v) => r.AirborneGlobalPositionLimit = v));
            AddControlBinder(new NumericIntBinder<RawDecodingSettings>(settings, numericFastSurfaceGlobalPositionLimit,         r => r.FastSurfaceGlobalPositionLimit,        (r,v) => r.FastSurfaceGlobalPositionLimit = v));
            AddControlBinder(new NumericIntBinder<RawDecodingSettings>(settings, numericSlowSurfaceGlobalPositionLimit,         r => r.SlowSurfaceGlobalPositionLimit,        (r,v) => r.SlowSurfaceGlobalPositionLimit = v));
            AddControlBinder(new NumericIntBinder<RawDecodingSettings>(settings, numericAcceptIcaoInPI0Count,                   r => r.AcceptIcaoInPI0Count,                  (r,v) => r.AcceptIcaoInPI0Count = v));
            AddControlBinder(new NumericIntBinder<RawDecodingSettings>(settings, numericAcceptIcaoInPI0Seconds,                 r => r.AcceptIcaoInPI0Seconds,                (r,v) => r.AcceptIcaoInPI0Seconds = v));
            AddControlBinder(new NumericIntBinder<RawDecodingSettings>(settings, numericAcceptIcaoInNonPICount,                 r => r.AcceptIcaoInNonPICount,                (r,v) => r.AcceptIcaoInNonPICount = v));
            AddControlBinder(new NumericIntBinder<RawDecodingSettings>(settings, numericAcceptIcaoInNonPISeconds,               r => r.AcceptIcaoInNonPISeconds,              (r,v) => r.AcceptIcaoInNonPISeconds = v));

            AddControlBinder(new NumericDoubleBinder<RawDecodingSettings>(settings, numericAcceptableAirborneSpeed,             r => r.AcceptableAirborneSpeed,               (r,v) => r.AcceptableAirborneSpeed = v));
            AddControlBinder(new NumericDoubleBinder<RawDecodingSettings>(settings, numericAcceptableAirSurfaceTransitionSpeed, r => r.AcceptableAirSurfaceTransitionSpeed,   (r,v) => r.AcceptableAirSurfaceTransitionSpeed = v));
            AddControlBinder(new NumericDoubleBinder<RawDecodingSettings>(settings, numericAcceptableSurfaceSpeed,              r => r.AcceptableSurfaceSpeed,                (r,v) => r.AcceptableSurfaceSpeed = v));

            AddControlBinder(new CheckBoxBoolBinder<RawDecodingSettings>(settings, checkBoxSuppressReceiverRangeCheck,          r => r.SuppressReceiverRangeCheck,            (r,v) => r.SuppressReceiverRangeCheck = v));
            AddControlBinder(new CheckBoxBoolBinder<RawDecodingSettings>(settings, checkBoxIgnoreMilitaryExtendedSquitter,      r => r.IgnoreMilitaryExtendedSquitter,        (r,v) => r.IgnoreMilitaryExtendedSquitter = v));
            AddControlBinder(new CheckBoxBoolBinder<RawDecodingSettings>(settings, checkBoxUseLocalDecodeForInitialPosition,    r => r.UseLocalDecodeForInitialPosition,      (r,v) => r.UseLocalDecodeForInitialPosition = v));
            AddControlBinder(new CheckBoxBoolBinder<RawDecodingSettings>(settings, checkBoxIgnoreCallsignsInBds20,              r => r.IgnoreCallsignsInBds20,                (r,v) => r.IgnoreCallsignsInBds20 = v));
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.ReceiverRange,                            numericReceiverRange },
                { ValidationField.AirborneGlobalPositionLimit,              numericAirborneGlobalPositionLimit },
                { ValidationField.FastSurfaceGlobalPositionLimit,           numericFastSurfaceGlobalPositionLimit },
                { ValidationField.SlowSurfaceGlobalPositionLimit,           numericSlowSurfaceGlobalPositionLimit },
                { ValidationField.AcceptableAirborneLocalPositionSpeed,     numericAcceptableAirborneSpeed },
                { ValidationField.AcceptableTransitionLocalPositionSpeed,   numericAcceptableAirSurfaceTransitionSpeed },
                { ValidationField.AcceptableSurfaceLocalPositionSpeed,      numericAcceptableSurfaceSpeed },
                { ValidationField.AcceptIcaoInPI0Count,                     numericAcceptIcaoInPI0Count },
                { ValidationField.AcceptIcaoInPI0Seconds,                   numericAcceptIcaoInPI0Seconds },
                { ValidationField.AcceptIcaoInNonPICount,                   numericAcceptIcaoInNonPICount },
                { ValidationField.AcceptIcaoInNonPISeconds,                 numericAcceptIcaoInNonPISeconds },
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(numericReceiverRange,                          Strings.ReceiverRange,                         Strings.OptionsDescribeRawFeedReceiverRange);
            SetInlineHelp(checkBoxSuppressReceiverRangeCheck,            Strings.SuppressReceiverRangeCheck,            Strings.OptionsDescribeRawFeedSuppressReceiverRangeCheck);
            SetInlineHelp(checkBoxIgnoreMilitaryExtendedSquitter,        Strings.IgnoreMilitaryExtendedSquitter,        Strings.OptionsDescribeRawFeedIgnoreMilitaryExtendedSquitter);
            SetInlineHelp(checkBoxUseLocalDecodeForInitialPosition,      Strings.UseLocalDecodeForInitialPosition,      Strings.OptionsDescribeRawFeedUseLocalDecodeForInitialPosition);
            SetInlineHelp(numericAirborneGlobalPositionLimit,            Strings.AirborneGlobal,                        Strings.OptionsDescribeRawFeedAirborneGlobalPositionLimit);
            SetInlineHelp(numericFastSurfaceGlobalPositionLimit,         Strings.FastSurfaceGlobal,                     Strings.OptionsDescribeRawFeedFastSurfaceGlobalPositionLimit);
            SetInlineHelp(numericSlowSurfaceGlobalPositionLimit,         Strings.SlowSurfaceGlobal,                     Strings.OptionsDescribeRawFeedSlowSurfaceGlobalPositionLimit);
            SetInlineHelp(numericAcceptableAirborneSpeed,                Strings.MaxAirborneSpeed,                      Strings.OptionsDescribeRawFeedAcceptableAirborneSpeed);
            SetInlineHelp(numericAcceptableAirSurfaceTransitionSpeed,    Strings.MaxTransitionSpeed,                    Strings.OptionsDescribeRawFeedAcceptableAirSurfaceTransitionSpeed);
            SetInlineHelp(numericAcceptableSurfaceSpeed,                 Strings.MaxSurfaceSpeed,                       Strings.OptionsDescribeRawFeedAcceptableSurfaceSpeed);
            SetInlineHelp(checkBoxIgnoreCallsignsInBds20,                Strings.IgnoreCallsignsInBds20,                Strings.OptionsDescribeRawFeedIgnoreCallsignsInBds20);
            SetInlineHelp(numericAcceptIcaoInPI0Count,                   Strings.AcceptIcaoInPI0Count,                  Strings.OptionsDescribeRawFeedAcceptIcaoInPI0Count);
            SetInlineHelp(numericAcceptIcaoInPI0Seconds,                 Strings.AcceptIcaoInPI0Seconds,                Strings.OptionsDescribeRawFeedAcceptIcaoInPI0Seconds);
            SetInlineHelp(numericAcceptIcaoInNonPICount,                 Strings.AcceptIcaoInNonPICount,                Strings.OptionsDescribeRawFeedAcceptIcaoInNonPICount);
            SetInlineHelp(numericAcceptIcaoInNonPISeconds,               Strings.AcceptIcaoInNonPISeconds,              Strings.OptionsDescribeRawFeedAcceptIcaoInNonPISeconds);
        }

        private void linkLabelUseIcaoSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SettingsView.RaiseUseIcaoRawDecodingSettingsClicked(e);
        }

        private void linkLabelUseRecommendedSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SettingsView.RaiseUseRecommendedRawDecodingSettingsClicked(e);
        }
    }
}
