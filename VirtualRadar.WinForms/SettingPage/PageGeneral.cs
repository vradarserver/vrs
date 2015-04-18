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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Displays general / miscellaneous settings to the user for editing.
    /// </summary>
    public partial class PageGeneral : Page
    {
        #region PageSummary
        /// <summary>
        /// The page summary object.
        /// </summary>
        public class Summary : PageSummary
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
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageGeneral();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            protected override void AssociateChildPages()
            {
                base.AssociateChildPages();

                var isMono = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono;
                if(isMono) {
                    ChildPages.Add(new PageMono.Summary());
                }
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageGeneral;
                SetValidationFields(new Dictionary<ValidationField,Control>() {
                    { ValidationField.CheckForNewVersions,  page == null ? null : page.numericDaysBetweenChecks },
                    { ValidationField.DisplayTimeout,       page == null ? null : page.numericDurationBeforeAircraftRemovedFromMap },
                    { ValidationField.TrackingTimeout,      page == null ? null : page.numericDurationBeforeAircraftRemovedFromTracking },
                    { ValidationField.ShortTrailLength,     page == null ? null : page.numericDurationOfShortTrails },
                    { ValidationField.TextToSpeechSpeed,    page == null ? null : page.numericReadingSpeed },
                    { ValidationField.AutoSavePolarPlots,   page == null ? null : page.numericAutoSavePolarPlots },
                });
            }
        }
        #endregion

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
        protected override void CreateBindings()
        {
            base.CreateBindings();

            var configuration = SettingsView.Configuration;
            var voiceNames = SettingsView.GetVoiceNames().Where(r => r != null).ToList();

            AddControlBinder(new CheckBoxBoolBinder<FlightRouteSettings>    (configuration.FlightRouteSettings,     checkBoxAutomaticallyDownloadNewRoutes, r => r.AutoUpdateEnabled,       (r,v) => r.AutoUpdateEnabled = v));
            AddControlBinder(new CheckBoxBoolBinder<VersionCheckSettings>   (configuration.VersionCheckSettings,    checkBoxCheckForNewVersions,            r => r.CheckAutomatically,      (r,v) => r.CheckAutomatically = v));
            AddControlBinder(new CheckBoxBoolBinder<BaseStationSettings>    (configuration.BaseStationSettings,     checkBoxMinimiseToSystemTray,           r => r.MinimiseToSystemTray,    (r,v) => r.MinimiseToSystemTray = v));
            AddControlBinder(new CheckBoxBoolBinder<AudioSettings>          (configuration.AudioSettings,           checkBoxAudioEnabled,                   r => r.Enabled,                 (r,v) => r.Enabled = v));

            AddControlBinder(new NumericIntBinder<VersionCheckSettings>     (configuration.VersionCheckSettings,    numericDaysBetweenChecks,                           r => r.CheckPeriodDays,             (r,v) => r.CheckPeriodDays = v));
            AddControlBinder(new NumericIntBinder<BaseStationSettings>      (configuration.BaseStationSettings,     numericDurationBeforeAircraftRemovedFromMap,        r => r.DisplayTimeoutSeconds,       (r,v) => r.DisplayTimeoutSeconds = v));
            AddControlBinder(new NumericIntBinder<BaseStationSettings>      (configuration.BaseStationSettings,     numericDurationBeforeAircraftRemovedFromTracking,   r => r.TrackingTimeoutSeconds,      (r,v) => r.TrackingTimeoutSeconds = v));
            AddControlBinder(new NumericIntBinder<GoogleMapSettings>        (configuration.GoogleMapSettings,       numericDurationOfShortTrails,                       r => r.ShortTrailLengthSeconds,     (r,v) => r.ShortTrailLengthSeconds = v));
            AddControlBinder(new NumericIntBinder<AudioSettings>            (configuration.AudioSettings,           numericReadingSpeed,                                r => r.VoiceRate,                   (r,v) => r.VoiceRate = v));
            AddControlBinder(new NumericIntBinder<BaseStationSettings>      (configuration.BaseStationSettings,     numericAutoSavePolarPlots,                          r => r.AutoSavePolarPlotsMinutes,   (r,v) => r.AutoSavePolarPlotsMinutes = v));

            AddControlBinder(new ComboBoxValueBinder<AudioSettings, string> (configuration.AudioSettings,   comboBoxTextToSpeechVoice,  voiceNames, r => r.VoiceName,   (r,v) => r.VoiceName = v));
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
            SetInlineHelp(numericAutoSavePolarPlots,                        Strings.AutoSavePolarPlots,             Strings.OptionsDescribeGeneralAutoSavePolarPlotsMinutes);
            SetInlineHelp(checkBoxMinimiseToSystemTray,                     Strings.MinimiseToSystemTray,           Strings.OptionsDescribeMinimiseToSystemTray);

            SetInlineHelp(checkBoxAudioEnabled,                             Strings.Enabled,                        Strings.OptionsDescribeGeneralAudioEnabled);
            SetInlineHelp(comboBoxTextToSpeechVoice,                        Strings.TextToSpeechVoice,              Strings.OptionsDescribeGeneralTextToSpeechVoice);
            SetInlineHelp(numericReadingSpeed,                              Strings.ReadingSpeed,                   Strings.OptionsDescribeGeneralReadingSpeed);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
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
