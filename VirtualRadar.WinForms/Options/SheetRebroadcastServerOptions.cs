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
using VirtualRadar.Interface.Settings;
using System.Drawing;
using VirtualRadar.Resources;

namespace VirtualRadar.WinForms.Options
{
    class SheetRebroadcastServerOptions : Sheet<SheetRebroadcastServerOptions>
    {
        /// <summary>
        /// See base docs.
        /// </summary>
        public override string SheetTitle { get { return Name ?? ""; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override Image Icon { get { return Images.Rebroadcast16x16; } }

        // The display order and number of categories on the sheet
        private const int RebroadcastServerCategory = 0;
        private const int TotalCategories = 1;

        [DisplayOrder(10)]
        [LocalisedDisplayName("Enabled")]
        [LocalisedCategory("RebroadcastServersTitle", RebroadcastServerCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeRebroadcastServerEnabled")]
        [TypeConverter(typeof(YesNoConverter))]
        public bool Enabled { get; set; }
        public bool ShouldSerializeEnabled() { return ValueHasChanged(r => r.Enabled); }

        [DisplayOrder(20)]
        [LocalisedDisplayName("Name")]
        [LocalisedCategory("RebroadcastServersTitle", RebroadcastServerCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeRebroadcastServerName")]
        [RaisesValuesChanged]
        public string Name { get; set; }
        public bool ShouldSerializeName() { return ValueHasChanged(r => Name); }

        [DisplayOrder(30)]
        [LocalisedDisplayName("Receiver")]
        [LocalisedCategory("RebroadcastServersTitle", RebroadcastServerCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeRebroadcastReceiver")]
        [TypeConverter(typeof(FeedTypeConverter))]
        [RaisesValuesChanged]
        public int ReceiverId { get; set; }
        public bool ShouldSerializeReceiverId() { return ValueHasChanged(r => ReceiverId); }

        [DisplayOrder(40)]
        [LocalisedDisplayName("Format")]
        [LocalisedCategory("RebroadcastServersTitle", RebroadcastServerCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeRebroadcastServerFormat")]
        [TypeConverter(typeof(RebroadcastFormatEnumConverter))]
        public RebroadcastFormat Format { get; set; }
        public bool ShouldSerializeFormat() { return ValueHasChanged(r => r.Format); }

        [DisplayOrder(50)]
        [LocalisedDisplayName("Port")]
        [LocalisedCategory("RebroadcastServersTitle", RebroadcastServerCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeRebroadcastServerPort")]
        [RaisesValuesChanged]
        public int Port { get; set; }
        public bool ShouldSerializePort() { return ValueHasChanged(r => r.Port); }

        [DisplayOrder(60)]
        [LocalisedDisplayName("StaleSeconds")]
        [LocalisedCategory("RebroadcastServersTitle", RebroadcastServerCategory, TotalCategories)]
        [LocalisedDescription("OptionsDescribeRebroadcastStaleSeconds")]
        [RaisesValuesChanged]
        public int StaleSeconds { get; set; }
        public bool ShouldSerializeStaleSeconds() { return ValueHasChanged(r => r.StaleSeconds); }
    }
}
