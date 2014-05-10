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
using VirtualRadar.Interface;
using System.Net;
using System.Collections;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that displays a list of browser sessions.
    /// </summary>
    public partial class ConnectionSessionListControl : BaseUserControl
    {
        #region Private class - ClientAndSession
        /// <summary>
        /// The class for the object that is held as the tag against each ListViewItem.
        /// </summary>
        class ClientAndSession
        {
            public LogClient Client;
            public LogSession Session;
        }
        #endregion

        #region Private class and enum - Sorter and SortColumn
        /// <summary>
        /// An enumeration of the different columns that the list can be sorted on
        /// </summary>
        enum SortColumn
        {
            StartTime,
            IpAddress,
            IsLocal,
            Duration,
            Requests,
            BytesSent,
            HtmlBytesSent,
            JsonBytesSent,
            ImageBytesSent,
            AudioBytesSent,
            OtherBytesSent,
        }

        /// <summary>
        /// A private class that can manage the sorting of the ListView.
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
                    ClientAndSession lhsClientAndSession = (ClientAndSession)lhs.Tag;
                    ClientAndSession rhsClientAndSession = (ClientAndSession)rhs.Tag;
                    LogClient lhsClient = lhsClientAndSession.Client;
                    LogClient rhsClient = rhsClientAndSession.Client;
                    LogSession lhsSession = lhsClientAndSession.Session;
                    LogSession rhsSession = rhsClientAndSession.Session;

                    switch(SortColumn) {
                        case SortColumn.AudioBytesSent:     result = Compare(lhsSession.AudioBytesSent, rhsSession.AudioBytesSent); break;
                        case SortColumn.BytesSent:          result = Compare(lhsSession.TotalBytesSent, rhsSession.TotalBytesSent); break;
                        case SortColumn.Duration:           result = TimeSpan.Compare(lhsSession.Duration, rhsSession.Duration); break;
                        case SortColumn.HtmlBytesSent:      result = Compare(lhsSession.HtmlBytesSent, rhsSession.HtmlBytesSent); break;
                        case SortColumn.ImageBytesSent:     result = Compare(lhsSession.ImageBytesSent, rhsSession.ImageBytesSent); break;
                        case SortColumn.IpAddress:          result = _IPAddressComparer.Compare(lhsClient.Address, rhsClient.Address); break;
                        case SortColumn.IsLocal:            result = Compare(lhsClient.IsLocal, rhsClient.IsLocal); break;
                        case SortColumn.JsonBytesSent:      result = Compare(lhsSession.JsonBytesSent, rhsSession.JsonBytesSent); break;
                        case SortColumn.OtherBytesSent:     result = Compare(lhsSession.OtherBytesSent, rhsSession.OtherBytesSent); break;
                        case SortColumn.Requests:           result = Compare(lhsSession.CountRequests, rhsSession.CountRequests); break;
                        case SortColumn.StartTime:          result = DateTime.Compare(lhsSession.StartTime, rhsSession.StartTime); break;
                        default:                            throw new NotImplementedException();
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
        /// The object that will sort the list view for us.
        /// </summary>
        private Sorter _Sorter = new Sorter() { SortColumn = SortColumn.StartTime, Ascending = false };
        #endregion

        #region Properties
        private bool _ShowClientDetails = true;
        /// <summary>
        /// Gets or sets a value that indicates that the client information should be shown. Set this to false if all
        /// of the sessions are for the same client.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowClientDetails
        {
            get { return _ShowClientDetails; }
            set
            {
                _ShowClientDetails = value;
                if(!DesignMode) {
                    if(_ShowClientDetails && columnHeaderIpAddress.ListView == null) {
                        listView.Columns.Add(columnHeaderIpAddress);
                        listView.Columns.Add(columnHeaderSource);
                    } else if(!_ShowClientDetails && columnHeaderIpAddress.ListView != null) {
                        listView.Columns.Remove(columnHeaderIpAddress);
                        listView.Columns.Remove(columnHeaderSource);
                    }
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ConnectionSessionListControl() : base()
        {
            InitializeComponent();
            listView.ListViewItemSorter = _Sorter;
        }
        #endregion

        #region Populate
        /// <summary>
        /// Displays the clients and sessions passed across.
        /// </summary>
        /// <param name="clients"></param>
        /// <param name="sessions"></param>
        public void Populate(IEnumerable<LogClient> clients, IEnumerable<LogSession> sessions)
        {
            listView.Items.Clear();
            if(sessions != null) {
                foreach(LogSession session in sessions) {
                    LogClient client = clients.Where(c => c.Id == session.ClientId).FirstOrDefault();
                    if(client != null) {
                        List<string> subItems = new List<string>();
                        subItems.Add(session.StartTime.ToLocalTime().ToString());
                        if(_ShowClientDetails) {
                            subItems.Add(client.IpAddress);
                            subItems.Add(client.IsLocal ? Strings.Local : Strings.Internet);
                        }
                        subItems.Add(Describe.TimeSpan(session.Duration));
                        subItems.Add(String.Format("{0:N0}", session.CountRequests));
                        subItems.Add(Describe.Bytes(session.TotalBytesSent));
                        subItems.Add(Describe.Bytes(session.HtmlBytesSent));
                        subItems.Add(Describe.Bytes(session.JsonBytesSent));
                        subItems.Add(Describe.Bytes(session.ImageBytesSent));
                        subItems.Add(Describe.Bytes(session.AudioBytesSent));
                        subItems.Add(Describe.Bytes(session.OtherBytesSent));

                        ListViewItem item = new ListViewItem(subItems.ToArray());
                        item.Tag = new ClientAndSession() { Client = client, Session = session };
                        listView.Items.Add(item);
                    }
                }
            }
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Raised when the user clicks a column header.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            SortColumn sortColumn = _Sorter.SortColumn;
            ColumnHeader header = listView.Columns[e.Column];
            if(header == columnHeaderAudioBytes) sortColumn = SortColumn.AudioBytesSent;
            else if(header == columnHeaderBytesSent) sortColumn = SortColumn.BytesSent;
            else if(header == columnHeaderDuration) sortColumn = SortColumn.Duration;
            else if(header == columnHeaderHtmlBytes) sortColumn = SortColumn.HtmlBytesSent;
            else if(header == columnHeaderImageBytes) sortColumn = SortColumn.ImageBytesSent;
            else if(header == columnHeaderIpAddress) sortColumn = SortColumn.IpAddress;
            else if(header == columnHeaderJsonBytes) sortColumn = SortColumn.JsonBytesSent;
            else if(header == columnHeaderOtherBytes) sortColumn = SortColumn.OtherBytesSent;
            else if(header == columnHeaderRequests) sortColumn = SortColumn.Requests;
            else if(header == columnHeaderSource) sortColumn = SortColumn.IsLocal;
            else if(header == columnHeaderStart) sortColumn = SortColumn.StartTime;
            else throw new NotImplementedException();

            if(sortColumn == _Sorter.SortColumn) _Sorter.Ascending = !_Sorter.Ascending;
            else {
                _Sorter.SortColumn = sortColumn;
                _Sorter.Ascending = true;
            }
            listView.Sort();
        }

        /// <summary>
        /// Called after the control has been fully constructed but is not yet on display.
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
