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
        public override string PageTitle { get { return Strings.General; } }

        public override Image PageIcon { get { return Images.Gear16x16; } }

        public PageGeneral()
        {
            InitializeComponent();
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            var voiceNames = SettingsView.GetVoiceNames().Where(r => r != null);
            comboBoxTextToSpeechVoice.DataSource = CreateNameValueSource<string>(voiceNames);
        }

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
