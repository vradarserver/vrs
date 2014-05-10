// Copyright © 2010 onwards, Andrew Whewell
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
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that displays information about web server requests.
    /// </summary>
    public partial class WebServerUserListControl : BaseUserControl
    {
        #region Private class - Details
        /// <summary>
        /// The information held against an entry on display
        /// </summary>
        class Details
        {
            public ListViewItem ListViewItem;
            public string Address;
            public DateTime LastRequest;
            public long BytesSent;
            public string LastUrl;
            public bool Changed;
        }
        #endregion

        #region Fields
        /// <summary>
        /// A list of every entry on display.
        /// </summary>
        private List<Details> _Entries = new List<Details>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets and sets a value indicating that the port number should be shown.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowPortNumber { get; set; }

        /// <summary>
        /// Gets and sets the number of milliseconds an entry can be shown for before it is removed (0 indicates
        /// that no automatic deletion of entries is to be performed).
        /// </summary>
        public int MillisecondsBeforeDelete { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebServerUserListControl() : base()
        {
            InitializeComponent();
            ShowPortNumber = true;
        }
        #endregion

        #region UpdateEntry
        /// <summary>
        /// Tells the control about a web request that needs to be displayed / updated.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="requestTime"></param>
        /// <param name="url"></param>
        /// <param name="bytesSent"></param>
        /// <remarks>
        /// This does not update the GUI, it just records information about the request. The GUI is updated on a timer.
        /// </remarks>
        public void UpdateEntry(string address, DateTime requestTime, string url, long bytesSent)
        {
            if(bytesSent > 0) {
                if(!ShowPortNumber) {
                    int portStart = address.LastIndexOf(':');
                    if(portStart != -1) address = address.Remove(portStart);
                }

                lock(_Entries) {
                    Details details = _Entries.Find(delegate(Details listDetails) { return String.Compare(listDetails.Address, address, StringComparison.InvariantCultureIgnoreCase) == 0; });
                    if(details == null) {
                        details = new Details() { Address = address };
                        _Entries.Add(details);
                    }
                    details.LastRequest = requestTime;
                    details.BytesSent += bytesSent;
                    details.LastUrl = url;
                    details.Changed = true;
                }
            }
        }

        /// <summary>
        /// Deletes entries that have not been updated within so-many milliseconds.
        /// </summary>
        /// <param name="milliseconds"></param>
        private void RemoveOldEntries(int milliseconds)
        {
            lock(_Entries) {
                List<int> removeList = null;
                DateTime threshold = DateTime.Now.AddMilliseconds(-milliseconds);

                for(int c = _Entries.Count - 1;c >= 0;c--) {
                    if(_Entries[c].LastRequest <= threshold) {
                        if(removeList == null) removeList = new List<int>();
                        removeList.Add(c);
                    }
                }

                if(removeList != null) {
                    foreach(int index in removeList) {
                        Details details = _Entries[index];
                        if(details.ListViewItem != null) listView.Items.Remove(details.ListViewItem);
                        _Entries.RemoveAt(index);
                    }
                }
            }
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Raised when the timer has elapsed. Updates the display.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            timerRefresh.Stop();

            if(MillisecondsBeforeDelete != 0) RemoveOldEntries(MillisecondsBeforeDelete);

            lock(_Entries) {
                foreach(Details details in _Entries) {
                    if(details.ListViewItem == null) {
                        details.ListViewItem = new ListViewItem(new string[] { details.Address, "", "", "" });
                        if(listView.Items.Count > 0) listView.Items.Insert(0, details.ListViewItem);
                        else                         listView.Items.Add(details.ListViewItem);
                    }

                    if(details.Changed) {
                        details.ListViewItem.SubItems[1].Text = details.LastRequest.ToString("G");
                        details.ListViewItem.SubItems[2].Text = details.BytesSent.ToString("N0");
                        details.ListViewItem.SubItems[3].Text = details.LastUrl;
                        details.Changed = false;
                    }
                }
            }

            timerRefresh.Start();
        }

        /// <summary>
        /// Called after the control has loaded but before it's displayed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(!DesignMode) Localise.Control(this);
        }
        #endregion
    }
}
