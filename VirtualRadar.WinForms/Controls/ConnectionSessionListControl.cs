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
        /// Handles the sorting of the list view.
        /// </summary>
        class Sorter : AutoListViewSorter
        {
            private ConnectionSessionListControl _Parent;

            public Sorter(ConnectionSessionListControl parent) : base(parent.listView)
            {
                _Parent = parent;
            }

            public override IComparable GetRowValue(ListViewItem listViewItem)
            {
                var result = base.GetRowValue(listViewItem);
                var info = listViewItem.Tag as ClientAndSession;
                if(info != null) {
                    var column = SortColumn ?? _Parent.columnHeaderStart;
                    if(column == _Parent.columnHeaderAudioBytes)        result = info.Session.AudioBytesSent;
                    else if(column == _Parent.columnHeaderBytesSent)    result = info.Session.TotalBytesSent;
                    else if(column == _Parent.columnHeaderDuration)     result = info.Session.Duration;
                    else if(column == _Parent.columnHeaderHtmlBytes)    result = info.Session.HtmlBytesSent;
                    else if(column == _Parent.columnHeaderImageBytes)   result = info.Session.ImageBytesSent;
                    else if(column == _Parent.columnHeaderIpAddress)    result = new ByteArrayComparable(info.Client.Address);
                    else if(column == _Parent.columnHeaderJsonBytes)    result = info.Session.JsonBytesSent;
                    else if(column == _Parent.columnHeaderOtherBytes)   result = info.Session.OtherBytesSent;
                    else if(column == _Parent.columnHeaderRequests)     result = info.Session.CountRequests;
                    else if(column == _Parent.columnHeaderStart)        result = info.Session.StartTime;
                }

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that will sort the list view for us.
        /// </summary>
        private Sorter _Sorter;
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
            _Sorter = new Sorter(this);
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
                        subItems.Add(session.UserName ?? "");
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
        /// Called after the control has been fully constructed but is not yet on display.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(!DesignMode) {
                Localise.Control(this);
                _Sorter.RefreshSortIndicators();
            }
        }
        #endregion
    }
}
