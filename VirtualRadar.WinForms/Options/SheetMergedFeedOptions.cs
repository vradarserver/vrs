// Copyright © 2013 onwards, Andrew Whewell
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

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The sheet that displays the options for a single merged feed.
    /// </summary>
    class SheetMergedFeedOptions : Sheet<SheetMergedFeedOptions>
    {
        /// <summary>
        /// See base class docs.
        /// </summary>
        public override string SheetTitle { get { return Name ?? ""; } }

        // The number and display order of categories on the sheet
        private const int GeneralCategory = 0;
        private const int TotalCategories = 1;

        [DisplayOrder(10)]
        [LocalisedDisplayName("Enabled")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeMergedFeedEnabled")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool Enabled { get; set; }
        public bool ShouldSerializeEnabled() { return ValueHasChanged(r => r.Enabled); }

        [DisplayOrder(20)]
        [LocalisedDisplayName("Name")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeMergedFeedName")]
        [RaisesValuesChanged]
        public string Name { get; set; }
        public bool ShouldSerializeName() { return ValueHasChanged(r => Name); }

        [DisplayOrder(30)]
        [LocalisedDisplayName("Receivers")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeMergedFeedReceviers")]
        [TypeConverter(typeof(ReceiverIdCollectionTypeConverter))]
        [Editor(typeof(ReceiverIdCollectionUITypeEditor), typeof(UITypeEditor))]
        [RaisesValuesChanged]
        public List<int> ReceiverIds { get; set; }
        public bool ShouldSerializeReceiverIds() { return ValueHasChanged(r => r.ReceiverIds); }

        [DisplayOrder(40)]
        [LocalisedDisplayName("IcaoTimeout")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeIcaoTimeout")]
        [RaisesValuesChanged]
        public int IcaoTimeout { get; set; }
        public bool ShouldSerializeIcaoTimeout() { return ValueHasChanged(r => IcaoTimeout); }

        [DisplayOrder(50)]
        [LocalisedDisplayName("IgnoreAircraftWithNoPosition")]
        [LocalisedCategory("OptionsGeneralCategory", GeneralCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeIgnoreAircraftWithNoPosition")]
        [RaisesValuesChanged]
        public bool IgnoreAircraftWithNoPosition { get; set; }
        public bool ShouldSerializeIgnoreAircraftWithNoPosition() { return ValueHasChanged(r => IgnoreAircraftWithNoPosition); }

        public SheetMergedFeedOptions()
        {
        }
    }
}
