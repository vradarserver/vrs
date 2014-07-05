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
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// The page for the general options.
    /// </summary>
    public partial class PageGeneral : Page
    {
        private string _DefaultVoiceName;

        public override string PageTitle { get { return Strings.General; } }

        public override Image PageIcon { get { return Images.Gear16x16; } }

        [LocalisedDisplayName("CheckForNewVersions")]
        [LocalisedDescription("OptionsDescribeGeneralCheckForNewVersions")]
        public Observable<bool> CheckForNewVersions { get; private set; }

        [LocalisedDisplayName("DaysBetweenChecks")]
        [LocalisedDescription("OptionsDescribeGeneralCheckForNewVersionsPeriodDays")]
        [ValidationField(ValidationField.CheckForNewVersions)]
        public Observable<int> CheckForNewVersionsPeriodDays { get; private set; }

        [LocalisedDisplayName("AutomaticallyDownloadNewRoutes")]
        [LocalisedDescription("OptionsDescribeGeneralDownloadFlightRoutes")]
        public Observable<bool> DownloadFlightRoutes { get; private set; }

        [LocalisedDisplayName("DurationBeforeAircraftRemovedFromMap")]
        [LocalisedDescription("OptionsDescribeGeneralDisplayTimeoutSeconds")]
        [ValidationField(ValidationField.DisplayTimeout)]
        public Observable<int> DisplayTimeoutSeconds { get; private set; }

        [LocalisedDisplayName("DurationBeforeAircraftRemovedFromTracking")]
        [LocalisedDescription("OptionsDescribeGeneralTrackingTimeoutSeconds")]
        [ValidationField(ValidationField.TrackingTimeout)]
        public Observable<int> TrackingTimeoutSeconds { get; private set; }

        [LocalisedDisplayName("DurationOfShortTrails")]
        [LocalisedDescription("OptionsDescribeGeneralShortTrailLengthSeconds")]
        [ValidationField(ValidationField.ShortTrailLength)]
        public Observable<int> ShortTrailLengthSeconds { get; private set; }

        [LocalisedDisplayName("MinimiseToSystemTray")]
        [LocalisedDescription("OptionsDescribeMinimiseToSystemTray")]
        public Observable<bool> MinimiseToSystemTray { get; private set; }

        [LocalisedDisplayName("Enabled")]
        [LocalisedDescription("OptionsDescribeGeneralAudioEnabled")]
        public Observable<bool> AudioEnabled { get; private set; }

        [LocalisedDisplayName("TextToSpeechVoice")]
        [LocalisedDescription("OptionsDescribeGeneralTextToSpeechVoice")]
        public Observable<string> TextToSpeechVoice { get; private set; }

        [LocalisedDisplayName("ReadingSpeed")]
        [LocalisedDescription("OptionsDescribeGeneralReadingSpeed")]
        [ValidationField(ValidationField.TextToSpeechSpeed)]
        public Observable<int> TextToSpeechSpeed { get; private set; }

        public PageGeneral()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            CheckForNewVersions =           BindProperty<bool>(checkBoxCheckForNewVersions);
            CheckForNewVersionsPeriodDays = BindProperty<int>(numericDaysBetweenChecks);
            DownloadFlightRoutes =          BindProperty<bool>(checkBoxAutomaticallyDownloadNewRoutes);
            DisplayTimeoutSeconds =         BindProperty<int>(numericDurationBeforeAircraftRemovedFromMap);
            TrackingTimeoutSeconds =        BindProperty<int>(numericDurationBeforeAircraftRemovedFromTracking);
            ShortTrailLengthSeconds =       BindProperty<int>(numericDurationOfShortTrails);
            MinimiseToSystemTray =          BindProperty<bool>(checkBoxMinimiseToSystemTray);

            AudioEnabled =                  BindProperty<bool>(checkBoxAudioEnabled);
            TextToSpeechVoice =             BindProperty<string>(comboBoxTextToSpeechVoice);
            TextToSpeechSpeed =             BindProperty<int>(numericReadingSpeed);
        }

        public void PopulateTextToSpeechVoices(IEnumerable<string> voiceNames)
        {
            _DefaultVoiceName = String.Format("[{0}]", Strings.DefaultVoice);
            comboBoxTextToSpeechVoice.PopulateWithCollection(
                voiceNames.OrderBy(r => r),
                r => r ?? _DefaultVoiceName
            );
        }

        private void buttonTestAudio_Click(object sender, EventArgs e)
        {
            OptionsView.RaiseTestTextToSpeechSettingsClicked(e);
        }
    }
}
