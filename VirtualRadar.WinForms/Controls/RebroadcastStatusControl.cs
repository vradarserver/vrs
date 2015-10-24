// Copyright © 2012 onwards, Andrew Whewell
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
using System.Net;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that can display information about connections to one or more rebroadcast servers.
    /// </summary>
    public partial class RebroadcastStatusControl : BaseUserControl
    {
        #region Private class - Sorter
        /// <summary>
        /// Manages the sorting of the list view for us.
        /// </summary>
        class Sorter : AutoListViewSorter
        {
            private RebroadcastStatusControl _Parent;

            public Sorter(RebroadcastStatusControl parent) : base(parent.listView)
            {
                _Parent = parent;
            }

            public override IComparable GetRowValue(ListViewItem listViewItem)
            {
                var result = base.GetRowValue(listViewItem);
                var connection = listViewItem.Tag as RebroadcastServerConnection;
                if(connection != null) {
                    var column = SortColumn ?? _Parent.columnHeaderName;
                    if(column == _Parent.columnHeaderBytesBuffered)     result = connection.BytesBuffered;
                    else if(column == _Parent.columnHeaderBytesSent)    result = connection.BytesWritten;
                    else if(column == _Parent.columnHeaderBytesStale)   result = connection.StaleBytesDiscarded;
                    else if(column == _Parent.columnHeaderIPAddress)    result = new ByteArrayComparable(connection.EndpointIPAddress);
                    else if(column == _Parent.columnHeaderPort)         result = connection.EndpointPort;
                }

                return result;
            }
        }
        #endregion

        /// <summary>
        /// The object that sorts the list view for us.
        /// </summary>
        private Sorter _Sorter;

        /// <summary>
        /// Gets or sets a description of the configuration.
        /// </summary>
        public string Configuration
        {
            get { return labelDescribeConfiguration.Text; }
            set { labelDescribeConfiguration.Text = value; }
        }

        /// <summary>
        /// Raised when the user indicates that they want to see the configuration GUI for
        /// rebroadcast servers.
        /// </summary>
        public event EventHandler ShowRebroadcastServersConfigurationClicked;

        /// <summary>
        /// Raises <see cref="ShowRebroadcastServersConfigurationClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnShowRebroadcastServersConfigurationClicked(EventArgs args)
        {
            EventHelper.Raise(ShowRebroadcastServersConfigurationClicked, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public RebroadcastStatusControl() : base()
        {
            InitializeComponent();
            _Sorter = new Sorter(this);
            listView.ListViewItemSorter = _Sorter;
        }

        /// <summary>
        /// Updates, adds or removes items in the list view to match up with the connections passed across.
        /// </summary>
        /// <param name="connections"></param>
        public void DisplayRebroadcastServerConnections(IList<RebroadcastServerConnection> connections)
        {
            // Update and delete existing entries
            var listItems = listView.Items.OfType<ListViewItem>().ToList();
            foreach(var listItem in listItems) {
                var existingConnection = listItem.Tag as RebroadcastServerConnection;
                var connection = existingConnection == null ? null : connections.FirstOrDefault(r => r.IsSameConnection(existingConnection));
                if(connection != null) UpdateListViewItem(listItem, connection);
                else {
                    listView.Items.Remove(listItem);
                    listItem.Tag = null;
                }                   
            }

            // Add new items
            foreach(var connection in connections) {
                if(!listItems.Any(r => { var existingConnection = r.Tag as RebroadcastServerConnection; return existingConnection != null && existingConnection.IsSameConnection(connection); })) {
                    var listItem = new ListViewItem();
                    UpdateListViewItem(listItem, connection);
                    listView.Items.Add(listItem);
                }
            }

            listView.Sort();
        }

        /// <summary>
        /// Updates the ListView item passed across to reflect the content of the connection.
        /// </summary>
        /// <param name="listItem"></param>
        /// <param name="connection"></param>
        private void UpdateListViewItem(ListViewItem listItem, RebroadcastServerConnection connection)
        {
            listItem.Tag = connection;
            while(listItem.SubItems.Count < 6) {
                listItem.SubItems.Add("");
            }

            for(var i = 0;i < listItem.SubItems.Count;++i) {
                var subItem = listItem.SubItems[i];
                switch(i) {
                    case 0:     subItem.Text = connection.Name; break;
                    case 1:     subItem.Text = connection.EndpointIPAddress == null ? "" : String.Format("{0}:{1}", connection.EndpointIPAddress, connection.EndpointPort); break;
                    case 2:     subItem.Text = connection.LocalPort.ToString(); break;
                    case 3:     subItem.Text = connection.BytesBuffered.ToString("N0"); break;
                    case 4:     subItem.Text = connection.BytesWritten.ToString("N0"); break;
                    case 5:     subItem.Text = connection.StaleBytesDiscarded.ToString("N0"); break;
                }
            }
        }

        /// <summary>
        /// Called after the control has been loaded but before anything is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if(!DesignMode) {
                _Sorter.RefreshSortIndicators();
            }
        }

        /// <summary>
        /// Called when the user clicks the configuration description label.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void labelDescribeConfiguration_Click(object sender, EventArgs e)
        {
            OnShowRebroadcastServersConfigurationClicked(EventArgs.Empty);
        }
    }
}
