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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// The parent page for all receivers.
    /// </summary>
    public partial class PageReceivers : Page
    {
        #region PageSummary
        /// <summary>
        /// The page summary object.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = Images.Radio16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override string PageTitle { get { return Strings.Receivers; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            public override Image PageIcon { get { return _PageIcon; } }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <returns></returns>
            protected override Page DoCreatePage()
            {
                return new PageReceivers();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            protected override void AssociateChildPages()
            {
                base.AssociateChildPages();
                AssociateListWithChildPages(SettingsView.Configuration.Receivers, () => new PageReceiver.Summary());
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="genericPage"></param>
            public override void AssociateValidationFields(Page genericPage)
            {
                var page = genericPage as PageReceivers;
                SetValidationFields(new Dictionary<ValidationField,Control>() {
                    { ValidationField.WebSiteReceiver,          page == null ? null : page.comboBoxWebSiteReceiverId },
                    { ValidationField.ClosestAircraftReceiver,  page == null ? null : page.comboBoxClosestAircraftReceiverId },
                    { ValidationField.FlightSimulatorXReceiver, page == null ? null : page.comboBoxFsxReceiverId },
                    { ValidationField.ReceiverIds,              page == null ? null : page.listReceivers },
                });
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
        public PageReceivers()
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

            var combinedFeeds = SettingsView.CombinedFeed;
            var receiverFormatManager = Factory.ResolveSingleton<IReceiverFormatManager>();
            var settings = SettingsView.Configuration.GoogleMapSettings;
            AddControlBinder(new ComboBoxBinder<GoogleMapSettings, CombinedFeed, int>(settings, comboBoxWebSiteReceiverId,         combinedFeeds,   r => r.WebSiteReceiverId,           (r,v) => r.WebSiteReceiverId = v)           { GetListItemDescription = r => r.Name, GetListItemValue = r => r.UniqueId, SortList = true, });
            AddControlBinder(new ComboBoxBinder<GoogleMapSettings, CombinedFeed, int>(settings, comboBoxClosestAircraftReceiverId, combinedFeeds,   r => r.ClosestAircraftReceiverId,   (r,v) => r.ClosestAircraftReceiverId = v)   { GetListItemDescription = r => r.Name, GetListItemValue = r => r.UniqueId, SortList = true, });
            AddControlBinder(new ComboBoxBinder<GoogleMapSettings, CombinedFeed, int>(settings, comboBoxFsxReceiverId,             combinedFeeds,   r => r.FlightSimulatorXReceiverId,  (r,v) => r.FlightSimulatorXReceiverId = v)  { GetListItemDescription = r => r.Name, GetListItemValue = r => r.UniqueId, SortList = true, });

            AddControlBinder(new MasterListToListBinder<Configuration, Receiver>(SettingsView.Configuration, listReceivers, r => r.Receivers) {
                FetchColumns = (receiver, e) => {
                    var location = SettingsView == null ? null : SettingsView.Configuration.ReceiverLocations.FirstOrDefault(r => r.UniqueId == receiver.ReceiverLocationId);

                    e.Checked = receiver.Enabled;
                    e.ColumnTexts.Add(receiver.Name);
                    e.ColumnTexts.Add(receiverFormatManager.ShortName(receiver.DataSource));
                    e.ColumnTexts.Add(Describe.ReceiverUsage(receiver.ReceiverUsage));
                    e.ColumnTexts.Add(location == null ? "" : location.Name);
                    e.ColumnTexts.Add(Describe.ConnectionType(receiver.ConnectionType));
                    e.ColumnTexts.Add(DescribeConnectionParameters(receiver));
                },
                AddHandler =                () => SettingsView.CreateReceiver(),
                DeleteHandler =             (r) => SettingsView.RemoveReceivers(r),
                EditHandler =               (receiver) => SettingsView.DisplayPageForPageObject(receiver),
                CheckedChangedHandler =     (receiver, isChecked) => receiver.Enabled = isChecked,
            });
        }

        private string DescribeConnectionParameters(Receiver receiver)
        {
            var result = new StringBuilder();

            switch(receiver.ConnectionType) {
                case ConnectionType.COM:
                    result.AppendFormat("{0}, {1}, {2}/{3}, {4}, {5}, \"{6}\", \"{7}\"",
                        receiver.ComPort,
                        receiver.BaudRate,
                        receiver.DataBits,
                        Describe.StopBits(receiver.StopBits),
                        Describe.Parity(receiver.Parity),
                        Describe.Handshake(receiver.Handshake),
                        receiver.StartupText,
                        receiver.ShutdownText
                    );
                    break;
                case ConnectionType.TCP:
                    result.AppendFormat("{0}:{1}",
                        receiver.Address,
                        receiver.Port
                    );
                    break;
            }

            return result.ToString();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void AssociateInlineHelp()
        {
            base.AssociateInlineHelp();
            SetInlineHelp(comboBoxWebSiteReceiverId,            Strings.WebSiteReceiverId,          Strings.OptionsDescribeWebSiteReceiverId);
            SetInlineHelp(comboBoxClosestAircraftReceiverId,    Strings.ClosestAircraftReceiverId,  Strings.OptionsDescribeClosestAircraftReceiverId);
            SetInlineHelp(comboBoxFsxReceiverId,                Strings.FlightSimulatorXReceiverId, Strings.OptionsDescribeFlightSimulatorXReceiverId);
            SetInlineHelp(listReceivers,                        "",                                 "");
        }
    }
}
