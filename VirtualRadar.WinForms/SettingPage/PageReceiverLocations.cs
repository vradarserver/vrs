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
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// The parent page for all receiver locations.
    /// </summary>
    public partial class PageReceiverLocations : Page
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
            public override string PageTitle { get { return Strings.ReceiverLocationsTitle; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return Images.Location16x16; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageReceiverLocations();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            protected override void AssociateChildPages()
            {
                base.AssociateChildPages();
                AssociateListWithChildPages(SettingsView.Configuration.ReceiverLocations, () => new PageReceiverLocation.Summary());
            }
        }
        #endregion

        /// <summary>
        /// See base docs.
        /// </summary>
        public override bool PageUseFullHeight { get { return true; } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageReceiverLocations()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();

            AddControlBinder(new MasterListToListBinder<Configuration, ReceiverLocation>(SettingsView.Configuration, listReceiverLocations, r => r.ReceiverLocations) {
                FetchColumns = (receiverLocation, e) => {
                    e.ColumnTexts.Add(receiverLocation.Name);
                    e.ColumnTexts.Add(receiverLocation.Latitude.ToString("N6"));
                    e.ColumnTexts.Add(receiverLocation.Longitude.ToString("N6"));
                },
                GetSortValue = (receiverLocation, header, defaultValue) => {
                    IComparable result = defaultValue;
                    if(header == columnHeaderLatitude)          result = receiverLocation.Latitude;
                    else if(header == columnHeaderLongitude)    result = receiverLocation.Longitude;

                    return result;
                },
                AddHandler = () => SettingsView.CreateReceiverLocation(),
                AutoDeleteEnabled = true,
                EditHandler = (receiverLocation) => SettingsView.DisplayPageForPageObject(receiverLocation),
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(listReceiverLocations, "", "");
        }

        /// <summary>
        /// Called when the user clicks the link to copy receiver locations from BaseStation.sqb.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void linkLabelUpdateFromBaseStationDatabase_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SettingsView.RaiseUpdateReceiverLocationsFromBaseStationDatabaseClicked(e);
        }
    }
}
