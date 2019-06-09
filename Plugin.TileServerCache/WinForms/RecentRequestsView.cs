// Copyright © 2019 onwards, Andrew Whewell
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

namespace VirtualRadar.Plugin.TileServerCache.WinForms
{
    public partial class RecentRequestsView : Form
    {
        public RecentRequestsView()
        {
            InitializeComponent();
        }

        private void RefreshDisplay()
        {
            if(InvokeRequired) {
                BeginInvoke(new MethodInvoker(() => RefreshDisplay()));
            } else {
                var controller = new RecentRequestsController();
                RefreshList(controller.GetRecentRequestOutcomes());
            }
        }

        private void RefreshList(RequestOutcome[] outcomes)
        {
            lvwRecentRequests.BeginUpdate();

            try {
                for(var i = 0;i < outcomes.Length;++i) {
                    var lvi = lvwRecentRequests.Items.Count > i ? lvwRecentRequests.Items[i] : null;
                    if(lvi == null) {
                        lvi = lvwRecentRequests.Items.Add("");
                    }

                    RefreshListViewItem(outcomes[i], lvi);
                }

                while(lvwRecentRequests.Items.Count > outcomes.Length) {
                    lvwRecentRequests.Items.RemoveAt(lvwRecentRequests.Items.Count - 1);
                }
            } finally {
                lvwRecentRequests.EndUpdate();
            }
        }

        private void RefreshListViewItem(RequestOutcome requestOutcome, ListViewItem lvi)
        {
            if(!(lvi.Tag is RequestOutcome taggedOutcome) || !taggedOutcome.Equals(requestOutcome)) {
                while(lvi.SubItems.Count < 8) {
                    lvi.SubItems.Add("");
                }
                lvi.Tag = requestOutcome;

                lvi.SubItems[0].Text = requestOutcome.ReceivedUtc.ToString("HH:mm:ss.fff");
                lvi.SubItems[1].Text = requestOutcome.CompletedUtc == null ? "" : requestOutcome.DurationMs.Value.ToString("N0");
                lvi.SubItems[2].Text = requestOutcome.TileServerName ?? "";
                lvi.SubItems[3].Text = requestOutcome.Zoom ?? "";
                lvi.SubItems[4].Text = requestOutcome.X ?? "";
                lvi.SubItems[5].Text = requestOutcome.Y ?? "";
                lvi.SubItems[6].Text = requestOutcome.Retina ?? "";
                lvi.SubItems[7].Text = requestOutcome.Outcome ?? "";
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                PluginLocalise.Form(this);
                RefreshDisplay();
            }
        }

        private void TimerRefresh_Tick(object sender, EventArgs e)
        {
            RefreshDisplay();
        }
    }
}
