// Copyright © 2012 onwards, Andrew Whewell
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
using System.Linq;
using System.Text;
using VirtualRadar.Localisation;
using System.ComponentModel;
using System.Drawing.Design;
using VirtualRadar.Interface.Settings;
using System.Drawing;
using VirtualRadar.Resources;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The property sheet for general options.
    /// </summary>
    class SheetGeneralOptions : Sheet<SheetGeneralOptions>
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string SheetTitle { get { return Strings.General; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image Icon { get { return Images.Gear16x16; } }

        // The display order and number of categories.
        private const int GeneralCategory = 0;
        private const int AudioCategory = 1;
        private const int RebroadcastCategory = 2;
        private const int TotalCategories = 3;

        [DisplayOrder(10)]
        [LocalisedDisplayName("CheckForNewVersions")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeGeneralCheckForNewVersions")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool CheckForNewVersions { get; set; }
        public bool ShouldSerializeCheckForNewVersions() { return ValueHasChanged(r => r.CheckForNewVersions); }

        [DisplayOrder(20)]
        [LocalisedDisplayName("DaysBetweenChecks")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeGeneralCheckForNewVersionsPeriodDays")]
        [RaisesValuesChanged]
        public int CheckForNewVersionsPeriodDays { get; set; }
        public bool ShouldSerializeCheckForNewVersionsPeriodDays() { return ValueHasChanged(r => r.CheckForNewVersionsPeriodDays); }

        [DisplayOrder(30)]
        [LocalisedDisplayName("AutomaticallyDownloadNewRoutes")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeGeneralDownloadFlightRoutes")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool DownloadFlightRoutes { get; set; }
        public bool ShouldSerializeDownloadFlightRoutes() { return ValueHasChanged(r => r.DownloadFlightRoutes); }

        [DisplayOrder(40)]
        [LocalisedDisplayName("DurationBeforeAircraftRemovedFromMap")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeGeneralDisplayTimeoutSeconds")]
        [RaisesValuesChanged]
        public int DisplayTimeoutSeconds { get; set; }
        public bool ShouldSerializeDisplayTimeoutSeconds() { return ValueHasChanged(r => r.DisplayTimeoutSeconds); }

        [DisplayOrder(50)]
        [LocalisedDisplayName("DurationBeforeAircraftRemovedFromTracking")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeGeneralTrackingTimeoutSeconds")]
        [RaisesValuesChanged]
        public int TrackingTimeoutSeconds { get; set; }
        public bool ShouldSerializeTrackingTimeoutSeconds() { return ValueHasChanged(r => r.TrackingTimeoutSeconds); }

        [DisplayOrder(60)]
        [LocalisedDisplayName("DurationOfShortTrails")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeGeneralShortTrailLengthSeconds")]
        [RaisesValuesChanged]
        public int ShortTrailLengthSeconds { get; set; }
        public bool ShouldSerializeShortTrailLengthSeconds() { return ValueHasChanged(r => r.ShortTrailLengthSeconds); }

        [DisplayOrder(70)]
        [LocalisedDisplayName("MinimiseToSystemTray")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeMinimiseToSystemTray")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool MinimiseToSystemTray { get; set; }
        public bool ShouldSerializeMinimiseToSystemTray() { return ValueHasChanged(r => r.MinimiseToSystemTray); }

        [DisplayOrder(80)]
        [LocalisedDisplayName("Enabled")]
        [LocalisedCategoryAttribute("Audio", AudioCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeGeneralAudioEnabled")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool AudioEnabled { get; set; }
        public bool ShouldSerializeAudioEnabled() { return ValueHasChanged(r => r.AudioEnabled); }

        [DisplayOrder(90)]
        [LocalisedDisplayName("TextToSpeechVoice")]
        [LocalisedCategoryAttribute("Audio", AudioCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeGeneralTextToSpeechVoice")]
        [TypeConverter(typeof(TextToSpeechVoiceTypeConverter))]
        public string TextToSpeechVoice { get; set; }
        public bool ShouldSerializeTextToSpeechVoice() { return ValueHasChanged(r => r.TextToSpeechVoice); }

        [DisplayOrder(100)]
        [LocalisedDisplayName("ReadingSpeed")]
        [LocalisedCategoryAttribute("Audio", AudioCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeGeneralReadingSpeed")]
        [RaisesValuesChanged]
        public int TextToSpeechSpeed { get; set; }
        public bool ShouldSerializeTextToSpeechSpeed() { return ValueHasChanged(r => r.TextToSpeechSpeed); }
    }
}
