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
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Allows data entry for a single receiver location.
    /// </summary>
    public partial class PageReceiverLocation : Page
    {
        public override Image PageIcon { get { return Images.Location16x16; } }

        public ReceiverLocation ReceiverLocation { get { return PageObject as ReceiverLocation; } }

        public override bool PageUseFullHeight { get { return true; } }

        public PageReceiverLocation()
        {
            InitializeComponent();
        }

        protected override void InitialiseControls()
        {
            base.InitialiseControls();
            SetPageTitleProperty<ReceiverLocation>(r => r.Name, () => ReceiverLocation.Name);
        }

        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddBinding(ReceiverLocation, textBoxName,       r => r.Name,        r => r.Text,    DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(ReceiverLocation, numericLatitude,   r => r.Latitude,    r => r.Value,   DataSourceUpdateMode.OnPropertyChanged);
            AddBinding(ReceiverLocation, numericLongitude,  r => r.Longitude,   r => r.Value,   DataSourceUpdateMode.OnPropertyChanged);

            var mapBindingSource = new BindingSource();
            mapBindingSource.DataSource = ReceiverLocation;
            bindingMap.DataSource = mapBindingSource;
            bindingMap.LatitudeMember = PropertyHelper.ExtractName<ReceiverLocation>(r => r.Latitude);
            bindingMap.LongitudeMember = PropertyHelper.ExtractName<ReceiverLocation>(r => r.Longitude);
        }

        protected override void AssociateValidationFields()
        {
            base.AssociateValidationFields();
            SetValidationFields(new Dictionary<Interface.View.ValidationField,Control>() {
                { ValidationField.Location,     textBoxName },
                { ValidationField.Latitude,     numericLatitude },
                { ValidationField.Longitude,    numericLongitude },
            });
        }

        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(textBoxName,      Strings.Name,       Strings.OptionsDescribeReceiverLocationName);
            SetInlineHelp(numericLatitude,  Strings.Latitude,   Strings.OptionsDescribeReceiverLocationLatitude);
            SetInlineHelp(numericLongitude, Strings.Longitude,  Strings.OptionsDescribeReceiverLocationLongitude);
        }
    }
}
