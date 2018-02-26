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
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// Shows the user all of the rebroadcast servers currently configured, lets them add new
    /// ones, remove existing ones etc.
    /// </summary>
    public partial class PageRebroadcastServers : Page
    {
        #region PageSummary
        /// <summary>
        /// The page summary object.
        /// </summary>
        public class Summary : PageSummary
        {
            private static Image _PageIcon = Images.Rebroadcast16x16;

            /// <summary>
            /// See base docs.
            /// </summary>
            public override string PageTitle { get { return Strings.RebroadcastServersTitle; } }

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
                return new PageRebroadcastServers();
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            protected override void AssociateChildPages()
            {
                base.AssociateChildPages();
                AssociateListWithChildPages(SettingsView.Configuration.RebroadcastSettings, () => new PageRebroadcastServer.Summary());
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
        public PageRebroadcastServers()
        {
            InitializeComponent();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void CreateBindings()
        {
            base.CreateBindings();

            var rebroadcastFormatManager = Factory.ResolveSingleton<IRebroadcastFormatManager>();

            AddControlBinder(new MasterListToListBinder<Configuration, RebroadcastSettings>(SettingsView.Configuration, listRebroadcastServers, r => r.RebroadcastSettings) {
                FetchColumns = (rebroadcastServer, e) => {
                    var receiver = SettingsView.CombinedFeed.FirstOrDefault(r => r.UniqueId == rebroadcastServer.ReceiverId);
                    var portDescription = !rebroadcastServer.IsTransmitter ? String.Format("::{0}", rebroadcastServer.Port) : String.Format("{0}:{1}", rebroadcastServer.TransmitAddress, rebroadcastServer.Port);

                    e.Checked = rebroadcastServer.Enabled;
                    e.ColumnTexts.Add(rebroadcastServer.Name);
                    e.ColumnTexts.Add(receiver == null ? "" : receiver.Name ?? "");
                    e.ColumnTexts.Add(rebroadcastFormatManager.ShortName(rebroadcastServer.Format));
                    e.ColumnTexts.Add(portDescription);
                    e.ColumnTexts.Add(Describe.DefaultAccess(rebroadcastServer.Access.DefaultAccess));
                },
                GetSortValue = (rebroadcastServer, header, defaultValue) => {
                    IComparable result = defaultValue;
                    if(header == columnHeaderUNC) {
                        if(!rebroadcastServer.IsTransmitter) result = String.Format("_{0:00000}", rebroadcastServer.Port);
                        else                                 result = String.Format("{0}:{1:00000}", rebroadcastServer.TransmitAddress, rebroadcastServer.Port);
                    }

                    return result;
                },
                AddHandler = () => SettingsView.CreateRebroadcastServer(),
                AutoDeleteEnabled = true,
                EditHandler = (rebroadcastServer) => SettingsView.DisplayPageForPageObject(rebroadcastServer),
                CheckedChangedHandler = (rebroadcastServer, isChecked) => rebroadcastServer.Enabled = isChecked,
            });
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="args"></param>
        /// <remarks>
        /// The list view doesn't recognise changes in the <see cref="Access"/> child object so we need
        /// to pick those up manually.
        /// </remarks>
        internal override void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
            base.ConfigurationChanged(args);

            if(args.Group == ConfigurationListenerGroup.Access) {
                listRebroadcastServers.ResetBindings();
            }
        }
    }
}
