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
    /// Displays general / miscellaneous settings to the user for editing.
    /// </summary>
    public partial class PageGeneral : Page
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string PageTitle { get { return Strings.General; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image PageIcon { get { return Images.Gear16x16; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageGeneral()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            var voiceNames = SettingsView.GetVoiceNames().Where(r => r != null);
            comboBoxTextToSpeechVoice.DataSource = CreateNameValueSource<string>(voiceNames);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(SettingsView, checkBoxAutomaticallyDownloadNewRoutes,            r => r.Configuration.FlightRouteSettings.AutoUpdateEnabled,         r => r.Checked);
            AddBinding(SettingsView, checkBoxCheckForNewVersions,                       r => r.Configuration.VersionCheckSettings.CheckAutomatically,       r => r.Checked);
            AddBinding(SettingsView, numericDaysBetweenChecks,                          r => r.Configuration.VersionCheckSettings.CheckPeriodDays,          r => r.Value);
            AddBinding(SettingsView, numericDurationBeforeAircraftRemovedFromMap,       r => r.Configuration.BaseStationSettings.DisplayTimeoutSeconds,     r => r.Value);
            AddBinding(SettingsView, numericDurationBeforeAircraftRemovedFromTracking,  r => r.Configuration.BaseStationSettings.TrackingTimeoutSeconds,    r => r.Value);
            AddBinding(SettingsView, numericDurationOfShortTrails,                      r => r.Configuration.GoogleMapSettings.ShortTrailLengthSeconds,     r => r.Value);
            AddBinding(SettingsView, checkBoxMinimiseToSystemTray,                      r => r.Configuration.BaseStationSettings.MinimiseToSystemTray,      r => r.Checked);

            AddBinding(SettingsView, checkBoxAudioEnabled,                              r => r.Configuration.AudioSettings.Enabled,                         r => r.Checked);
            AddBinding(SettingsView, comboBoxTextToSpeechVoice,                         r => r.Configuration.AudioSettings.VoiceName,                       r => r.SelectedValue);
            AddBinding(SettingsView, numericReadingSpeed,                               r => r.Configuration.AudioSettings.VoiceRate,                       r => r.Value);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<ValidationField,Control>() {
                { ValidationField.CheckForNewVersions,  numericDaysBetweenChecks },
                { ValidationField.DisplayTimeout,       numericDurationBeforeAircraftRemovedFromMap },
                { ValidationField.TrackingTimeout,      numericDurationBeforeAircraftRemovedFromTracking },
                { ValidationField.ShortTrailLength,     numericDurationOfShortTrails },
                { ValidationField.TextToSpeechSpeed,    numericReadingSpeed },
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(checkBoxAutomaticallyDownloadNewRoutes,           Strings.CheckForNewVersions,            Strings.OptionsDescribeGeneralDownloadFlightRoutes);
            SetInlineHelp(checkBoxCheckForNewVersions,                      Strings.AutomaticallyDownloadNewRoutes, Strings.OptionsDescribeGeneralCheckForNewVersions);
            SetInlineHelp(numericDaysBetweenChecks,                         Strings.DaysBetweenChecks,              Strings.OptionsDescribeGeneralCheckForNewVersionsPeriodDays);
            SetInlineHelp(numericDurationBeforeAircraftRemovedFromMap,      Strings.RemoveFromDisplay,              Strings.OptionsDescribeGeneralDisplayTimeoutSeconds);
            SetInlineHelp(numericDurationBeforeAircraftRemovedFromTracking, Strings.RemoveFromTracking,             Strings.OptionsDescribeGeneralTrackingTimeoutSeconds);
            SetInlineHelp(numericDurationOfShortTrails,                     Strings.ShortTrailDuration,             Strings.OptionsDescribeGeneralShortTrailLengthSeconds);
            SetInlineHelp(checkBoxMinimiseToSystemTray,                     Strings.MinimiseToSystemTray,           Strings.OptionsDescribeMinimiseToSystemTray);

            SetInlineHelp(checkBoxAudioEnabled,                             Strings.Enabled,                        Strings.OptionsDescribeGeneralAudioEnabled);
            SetInlineHelp(comboBoxTextToSpeechVoice,                        Strings.TextToSpeechVoice,              Strings.OptionsDescribeGeneralTextToSpeechVoice);
            SetInlineHelp(numericReadingSpeed,                              Strings.ReadingSpeed,                   Strings.OptionsDescribeGeneralReadingSpeed);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(!DesignMode) {
                buttonTestAudio.Image = Images.Test16x16;
            }
        }

        private void buttonTestAudio_Click(object sender, EventArgs e)
        {
            SettingsView.RaiseTestTextToSpeechSettingsClicked(e);
        }

        private void linkLabelDefaultVoice_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SettingsView.Configuration.AudioSettings.VoiceName = null;
        }
    }
}
