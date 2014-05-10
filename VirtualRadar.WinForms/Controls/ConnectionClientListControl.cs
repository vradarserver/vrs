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

        #region Private class and enum - Sorter and SortColumn
        /// <summary>
        /// An enumeration of the different columns in the list view that can be used to control sorting.
        /// </summary>
        enum SortColumn
        {
            IpAddress,
            IsLocal,
            ReverseDns,
            FirstSeen,
            LastSeen,
            Sessions,
            Duration,
            BytesSent,
        }

        /// <summary>
        /// A private class that can be used to sort the list view.
        /// </summary>
        class Sorter : IComparer, IComparer<ListViewItem>
        {
            private IPAddressComparer _IPAddressComparer = new IPAddressComparer();

            public SortColumn SortColumn { get; set; }
            public bool Ascending { get; set; }

            public int Compare(ListViewItem lhs, ListViewItem rhs)
            {
                int result = Object.ReferenceEquals(lhs, rhs) ? 0 : -1;
                if(result != 0) {
                    var lhsClientAndSessions = (ClientAndSessions)lhs.Tag;
                    var rhsClientAndSessions = (ClientAndSessions)rhs.Tag;
                    var lhsClient = lhsClientAndSessions.Client;
                    var rhsClient = rhsClientAndSessions.Client;

                    switch(SortColumn) {
                        case SortColumn.BytesSent:          result = Compare(lhsClientAndSessions.TotalBytesSent, rhsClientAndSessions.TotalBytesSent); break;
                        case SortColumn.Duration:           result = TimeSpan.Compare(lhsClientAndSessions.TotalDuration, rhsClientAndSessions.TotalDuration); break;
                        case SortColumn.FirstSeen:          result = DateTime.Compare(lhsClientAndSessions.FirstSession, rhsClientAndSessions.FirstSession); break;
                        case SortColumn.IpAddress:          result = _IPAddressComparer.Compare(lhsClient.Address, rhsClient.Address); break;
                        case SortColumn.IsLocal:            result = Compare(lhsClient.IsLocal, rhsClient.IsLocal); break;
                        case SortColumn.LastSeen:           result = DateTime.Compare(lhsClientAndSessions.LastSession, rhsClientAndSessions.LastSession); break;
                        case SortColumn.ReverseDns:         result = String.Compare(lhsClient.ReverseDns, rhsClient.ReverseDns); break;
                        case SortColumn.Sessions:           result = lhsClientAndSessions.Sessions.Count - rhsClientAndSessions.Sessions.Count; break;
                        default: throw new NotImplementedException();
                    }
                }

                return Ascending ? result : -result;
            }

            private int Compare(long lhs, long rhs)
            {
                var difference = lhs - rhs;
                return difference < 0L ? -1 : difference > 0L ? 1 : 0;
            }

            private int Compare(bool lhs, bool rhs)
            {
                return lhs && !rhs ? -1 : !lhs && rhs ? 1 : 0;
            }

            public int Compare(object lhs, object rhs)
            {
                return Compare(lhs as ListViewItem, rhs as ListViewItem);
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that can sort rows in the list view.
        /// </summary>
        private Sorter _Sorter = new Sorter() { SortColumn = SortColumn.IpAddress, Ascending = true };
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
            if(SelectionChanged != null) SelectionChanged(this, args);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ConnectionClientListControl() : base()
        {
            InitializeComponent();
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
                Localise.Control(this);

                listView.ListViewItemSorter = _Sorter;
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

        /// <summary>
        /// Raised when the user clicks a column header.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortColumn sortColumn = _Sorter.SortColumn;
            ColumnHeader header = listView.Columns[e.Column];
            if(header == columnHeaderBytesSent) sortColumn = SortColumn.BytesSent;
            else if(header == columnHeaderCountSessions) sortColumn = SortColumn.Sessions;
            else if(header == columnHeaderDuration) sortColumn = SortColumn.Duration;
            else if(header == columnHeaderFirstSeen) sortColumn = SortColumn.FirstSeen;
            else if(header == columnHeaderIpAddress) sortColumn = SortColumn.IpAddress;
            else if(header == columnHeaderLastSeen) sortColumn = SortColumn.LastSeen;
            else if(header == columnHeaderSource) sortColumn = SortColumn.IsLocal;
            else if(header == columnHeaderReverseDns) sortColumn = SortColumn.ReverseDns;
            else throw new NotImplementedException();

            if(sortColumn == _Sorter.SortColumn) _Sorter.Ascending = !_Sorter.Ascending;
            else {
                _Sorter.SortColumn = sortColumn;
                _Sorter.Ascending = true;
            }

            listView.Sort();
        }
        #endregion
    }
}
