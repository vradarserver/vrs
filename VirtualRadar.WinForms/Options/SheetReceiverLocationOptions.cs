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

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// A sheet class that can take input for one receiver location.
    /// </summary>
    class SheetReceiverLocationOptions : Sheet<SheetReceiverLocationOptions>
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string SheetTitle { get { return Location ?? ""; } }

        // The display order and number of categories on the sheet
        private const int ReceiverLocationCategory = 0;
        private const int TotalCategories = 1;

        [DisplayOrder(10)]
        [LocalisedDisplayName("Location")]
        [LocalisedCategory("ReceiverLocationsTitle", ReceiverLocationCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeReceiverLocationName")]
        [RaisesValuesChanged]
        public string Location { get; set; }
        public bool ShouldSerializeLocation() { return ValueHasChanged(r => Location); }

        [DisplayOrder(20)]
        [LocalisedDisplayName("Latitude")]
        [LocalisedCategory("ReceiverLocationsTitle", ReceiverLocationCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeReceiverLocationLatitude")]
        [RaisesValuesChanged]
        public double Latitude { get; set; }
        public bool ShouldSerializeLatitude() { return ValueHasChanged(r => Latitude); }

        [DisplayOrder(30)]
        [LocalisedDisplayName("Longitude")]
        [LocalisedCategory("ReceiverLocationsTitle", ReceiverLocationCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeReceiverLocationLongitude")]
        [RaisesValuesChanged]
        public double Longitude { get; set; }
        public bool ShouldSerializeLongitude() { return ValueHasChanged(r => Longitude); }
    }
}
