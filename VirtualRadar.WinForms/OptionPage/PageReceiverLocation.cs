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
using VirtualRadar.Resources;
using VirtualRadar.Interface.Settings;
using VirtualRadar.WinForms.Binding;
using VirtualRadar.Localisation;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.OptionPage
{
    public partial class PageReceiverLocation : Page
    {
        public override Image PageIcon { get { return Images.iconmonstr_location_3_icon; } }

        public ReceiverLocation ReceiverLocation { get { return PageObject as ReceiverLocation; } }

        public override bool PageUseFullHeight { get { return true; } }

        [PageTitle]
        [LocalisedDisplayName("Name")]
        [LocalisedDescription("OptionsDescribeReceiverLocationName")]
        [ValidationField(ValidationField.Location)]
        public Observable<string> RecordName { get; private set; }

        [LocalisedDisplayName("Latitude")]
        [LocalisedDescription("OptionsDescribeReceiverLocationLatitude")]
        [ValidationField(ValidationField.Latitude)]
        public Observable<double> Latitude { get; private set; }

        [LocalisedDisplayName("Longitude")]
        [LocalisedDescription("OptionsDescribeReceiverLocationLongitude")]
        [ValidationField(ValidationField.Longitude)]
        public Observable<double> Longitude { get; private set; }

        public PageReceiverLocation()
        {
            InitializeComponent();
            InitialisePage();
        }

        protected override void CreateBindings()
        {
            RecordName = BindProperty<string>(textBoxName);
            Latitude = BindProperty<double>(locationMap, "Latitude");
            Longitude = BindProperty<double>(locationMap, "Longitude");
        }

        protected override void CopyRecordToObservables()
        {
            RecordName.Value = ReceiverLocation.Name;
            Latitude.Value = ReceiverLocation.Latitude;
            Longitude.Value = ReceiverLocation.Longitude;
        }

        protected override void CopyObservablesToRecord()
        {
            ReceiverLocation.Name = RecordName.Value;
            ReceiverLocation.Latitude = Latitude.Value;
            ReceiverLocation.Longitude = Longitude.Value;
        }
    }
}
