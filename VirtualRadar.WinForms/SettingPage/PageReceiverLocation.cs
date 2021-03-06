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
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Allows data entry for a single receiver location.
    /// </summary>
    public partial class PageReceiverLocation : Page
    {
        #region PageSummary
        /// <summary>
        /// The summary for receiver location pages.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = ResourceImages.Location16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return _PageIcon; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public ReceiverLocation ReceiverLocation { get { return PageObject as ReceiverLocation; } }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            public Summary() : base()
            {
                SetPageTitleProperty<ReceiverLocation>(r => r.Name, () => ReceiverLocation.Name);
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageReceiverLocation();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="record"></param>
            /// <returns></returns>
            internal override bool IsForSameRecord(object record)
            {
                var receiverLocation = record as ReceiverLocation;
                return receiverLocation != null && ReceiverLocation != null && receiverLocation.UniqueId == ReceiverLocation.UniqueId;
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageReceiverLocation;
                SetValidationFields(new Dictionary<Interface.View.ValidationField,Control>() {
                    { ValidationField.Location,     page == null ? null : page.textBoxName },
                    { ValidationField.Latitude,     page == null ? null : page.numericLatitude },
                    { ValidationField.Longitude,    page == null ? null : page.numericLongitude },
                });
            }
        }
        #endregion

        /// <summary>
        /// See base docs.
        /// </summary>
        public ReceiverLocation ReceiverLocation { get { return ((Summary)PageSummary).ReceiverLocation; } }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool PageUseFullHeight { get { return true; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageReceiverLocation()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void InitialiseControls()
        {
            base.InitialiseControls();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();
            AddControlBinder(new TextBoxStringBinder<ReceiverLocation>(ReceiverLocation, textBoxName, r => r.Name, (r,v) => r.Name = v) { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });

            AddControlBinder(new NumericDoubleBinder<ReceiverLocation>(ReceiverLocation, numericLatitude,   r => r.Latitude,    (r,v) => r.Latitude = v)    { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
            AddControlBinder(new NumericDoubleBinder<ReceiverLocation>(ReceiverLocation, numericLongitude,  r => r.Longitude,   (r,v) => r.Longitude = v)   { UpdateMode = DataSourceUpdateMode.OnPropertyChanged });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(textBoxName,      Strings.Name,       Strings.OptionsDescribeReceiverLocationName);
            SetInlineHelp(numericLatitude,  Strings.Latitude,   Strings.OptionsDescribeReceiverLocationLatitude);
            SetInlineHelp(numericLongitude, Strings.Longitude,  Strings.OptionsDescribeReceiverLocationLongitude);
        }
    }
}
