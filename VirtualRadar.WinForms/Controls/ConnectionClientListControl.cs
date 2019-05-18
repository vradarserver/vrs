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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.Database;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;
using InterfaceFactory;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that displays a list of clients to the user.
    /// </summary>
    public partial class ConnectionClientListControl : BaseUserControl
    {
        #region Private class - ClientAndSessions
        /// <summary>
        /// A private class that each entry in the list view is tagged with.
        /// </summary>
        class ClientAndSessions
        {
            public LogClient Client;

            public IList<LogSession> Sessions;

            public DateTime FirstSession    { get { return Sessions.Min(s => s.StartTime); } }

            public DateTime LastSession     { get { return Sessions.Max(s => s.EndTime); } }

            public TimeSpan TotalDuration   { get { return new TimeSpan((long)Sessions.Sum(s => s.Duration.Ticks)); } }

            public long TotalBytesSent      { get { return Sessions.Sum(s => s.TotalBytesSent); } }
        }
        #endregion

        #region Private class - Sorter
        /// <summary>
        /// A private class that can be used to sort the list view.
        /// </summary>
        class Sorter : AutoListViewSorter
        {
            ConnectionClientListControl _Parent;

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="parent"></param>
            public Sorter(ConnectionClientListControl parent) : base(parent.listView)
            {
                _Parent = parent;
            }

            /// <summary>
            /// Gets the column value to sort on.
            /// </summary>
            /// <param name="listViewItem"></param>
            /// <returns></returns>
            public override IComparable GetRowValue(ListViewItem listViewItem)
            {
                var result = base.GetRowValue(listViewItem);
                var info = listViewItem.Tag as ClientAndSessions;
                if(info != null) {
                    var column = SortColumn ?? _Parent.columnHeaderIpAddress;
                    if(column== _Parent.columnHeaderBytesSent)              result = info.TotalBytesSent;
                    else if(column == _Parent.columnHeaderCountSessions)    result = info.Sessions.Count;
                    else if(column == _Parent.columnHeaderDuration)         result = info.TotalDuration;
                    else if(column == _Parent.columnHeaderFirstSeen)        result = info.FirstSession;
                    else if(column == _Parent.columnHeaderLastSeen)         result = info.LastSession;
                    else if(column == _Parent.columnHeaderIpAddress)        result = new ByteArrayComparable(info.Client.Address);
                }

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that can sort rows in the list view.
        /// </summary>
        private Sorter _Sorter;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the currently selected client.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public LogClient SelectedClient
        {
            get { var result = GetSelectedListViewTag<ClientAndSessions>(listView); return result == null ? null : result.Client; }
        }

        /// <summary>
        /// Gets the sessions for the currently selected client.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList<LogSession> SelectedSessions
        {
            get { var result = GetSelectedListViewTag<ClientAndSessions>(listView); return result == null ? null : result.Sessions; }
        }
        #endregion

        #region Events exposed - SelectionChanged
        /// <summary>
        /// Raised when the user selects a client.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Raises <see cref="SelectionChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnSelectionChanged(EventArgs args)
        {
            EventHelper.Raise(SelectionChanged, this, args);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ConnectionClientListControl() : base()
        {
            InitializeComponent();
            _Sorter = new Sorter(this);
            listView.ListViewItemSorter = _Sorter;
        }
        #endregion

        #region FindItem
        /// <summary>
        /// Returns the list view row corresponding to the client passed across or null if no such row exists.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private ListViewItem FindItem(LogClient client)
        {
            ListViewItem result = null;
            foreach(ListViewItem item in listView.Items) {
                ClientAndSessions tag = (ClientAndSessions)item.Tag;
                if(tag.Client == client) {
                    result = item;
                    break;
                }
            }

            return result;
        }
        #endregion

        #region Populate
        /// <summary>
        /// Populates the control with a list of clients.
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="sessionMap"></param>
        public void Populate(IList<LogClient> clients, IDictionary<long, IList<LogSession>> sessionMap)
        {
            listView.Items.Clear();

            foreach(LogClient client in clients) {
                IList<LogSession> sessions;
                if(!sessionMap.TryGetValue(client.Id, out sessions)) sessions = new List<LogSession>();
                ClientAndSessions tag = new ClientAndSessions() { Client = client, Sessions = sessions };
                ListViewItem item = new ListViewItem(new string[] {
                    client.IpAddress,
                    client.IsLocal ? Strings.Local : Strings.Internet,
                    client.ReverseDns,
                    tag.FirstSession.ToLocalTime().ToString(),
                    tag.LastSession.ToLocalTime().ToString(),
                    String.Format("{0:N0}", sessions.Count),
                    Describe.TimeSpan(tag.TotalDuration),
                    Describe.Bytes(tag.TotalBytesSent)
                });
                item.Tag = tag;
                listView.Items.Add(item);
            }
        }

        /// <summary>
        /// Updates the list view to show the results of a reverse DNS lookup for the client.
        /// </summary>
        /// <param name="client"></param>
        public void RefreshClientReverseDnsDetails(LogClient client)
        {
            ListViewItem item = FindItem(client);
            if(item != null) item.SubItems[2].Text = client.ReverseDns;
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Called when the control has been loaded but is not yet on display.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                FormsLocalise.Control(this);
                _Sorter.RefreshSortIndicators();
            }
        }

        /// <summary>
        /// Raised when the user selects a row in the list view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            OnSelectionChanged(EventArgs.Empty);
        }
        #endregion
    }
}
