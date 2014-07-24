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

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays the raw feed decoding settings to the user.
    /// </summary>
    public partial class PageRawFeedDecoding : Page
    {
        public override string PageTitle { get { return Strings.OptionsRawFeedSheetTitle; } }

        public override Image PageIcon { get { return Images.Decoding16x16; } }

        public PageRawFeedDecoding()
        {
            InitializeComponent();
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, numericReceiverRange,                          r => r.Configuration.RawDecodingSettings.ReceiverRange,                         r => r.Value);
            AddBinding(SettingsView, checkBoxSuppressReceiverRangeCheck,            r => r.Configuration.RawDecodingSettings.SuppressReceiverRangeCheck,            r => r.Checked);
            AddBinding(SettingsView, checkBoxIgnoreMilitaryExtendedSquitter,        r => r.Configuration.RawDecodingSettings.IgnoreMilitaryExtendedSquitter,        r => r.Checked);
            AddBinding(SettingsView, checkBoxUseLocalDecodeForInitialPosition,      r => r.Configuration.RawDecodingSettings.UseLocalDecodeForInitialPosition,      r => r.Checked);
            AddBinding(SettingsView, numericAirborneGlobalPositionLimit,            r => r.Configuration.RawDecodingSettings.AirborneGlobalPositionLimit,           r => r.Value);
            AddBinding(SettingsView, numericFastSurfaceGlobalPositionLimit,         r => r.Configuration.RawDecodingSettings.FastSurfaceGlobalPositionLimit,        r => r.Value);
            AddBinding(SettingsView, numericSlowSurfaceGlobalPositionLimit,         r => r.Configuration.RawDecodingSettings.SlowSurfaceGlobalPositionLimit,        r => r.Value);
            AddBinding(SettingsView, numericAcceptableAirborneSpeed,                r => r.Configuration.RawDecodingSettings.AcceptableAirborneSpeed,               r => r.Value);
            AddBinding(SettingsView, numericAcceptableAirSurfaceTransitionSpeed,    r => r.Configuration.RawDecodingSettings.AcceptableAirSurfaceTransitionSpeed,   r => r.Value);
            AddBinding(SettingsView, numericAcceptableSurfaceSpeed,                 r => r.Configuration.RawDecodingSettings.AcceptableSurfaceSpeed,                r => r.Value);
            AddBinding(SettingsView, checkBoxIgnoreCallsignsInBds20,                r => r.Configuration.RawDecodingSettings.IgnoreCallsignsInBds20,                r => r.Checked);
            AddBinding(SettingsView, numericAcceptIcaoInPI0Count,                   r => r.Configuration.RawDecodingSettings.AcceptIcaoInPI0Count,                  r => r.Value);
            AddBinding(SettingsView, numericAcceptIcaoInPI0Seconds,                 r => r.Configuration.RawDecodingSettings.AcceptIcaoInPI0Seconds,                r => r.Value);
            AddBinding(SettingsView, numericAcceptIcaoInNonPICount,                 r => r.Configuration.RawDecodingSettings.AcceptIcaoInNonPICount,                r => r.Value);
            AddBinding(SettingsView, numericAcceptIcaoInNonPISeconds,               r => r.Configuration.RawDecodingSettings.AcceptIcaoInNonPISeconds,              r => r.Value);
        }

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
