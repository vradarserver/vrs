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
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that displays information about web server requests.
    /// </summary>
    public partial class WebServerUserListControl : BaseUserControl
    {
        #region Private class - Sorter
        /// <summary>
        /// Sorts the list view for us.
        /// </summary>
        class Sorter : AutoListViewSorter
        {
            WebServerUserListControl _Parent;

            public Sorter(WebServerUserListControl parent) : base(parent.listView)
            {
                _Parent = parent;
            }

            public override IComparable GetRowValue(ListViewItem listViewItem)
            {
                var result = base.GetRowValue(listViewItem);
                var details = listViewItem.Tag as ServerRequest;
                if(details != null) {
                    var column = SortColumn ?? _Parent.columnHeaderAddress;
                    if(column == _Parent.columnHeaderAddress) {
                        result = new ByteArrayComparable(details.RemoteEndPoint.Address);
                    }
                    else if(column == _Parent.columnHeaderBytesSent)    result = details.BytesSent;
                    else if(column == _Parent.columnHeaderLastRequest)  result = details.LastRequest;
                }

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that's sorting the list view for us.
        /// </summary>
        private Sorter _Sorter;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public WebServerUserListControl() : base()
        {
            InitializeComponent();
            _Sorter = new Sorter(this);
            listView.ListViewItemSorter = _Sorter;
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called after the control has loaded but before it's displayed.
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

        #region ShowServerRequests
        /// <summary>
        /// Shows the server requests to the user.
        /// </summary>
        /// <param name="serverRequests"></param>
        public void ShowServerRequests(ServerRequest[] serverRequests)
        {
            var unhandledRequests = new LinkedList<ServerRequest>(serverRequests.Select(r => r.Clone() as ServerRequest));
            Func<ServerRequest, LinkedListNode<ServerRequest>> findUnhandledRequestNode = r => {
                LinkedListNode<ServerRequest> findResult = null;
                for(var node = unhandledRequests.First;node != null;node = node.Next) {
                    if(node.Value.RemoteAddress == r.RemoteAddress) {
                        findResult = node;
                        break;
                    }
                }

                return findResult;
            };

            foreach(var listViewItem in listView.Items.OfType<ListViewItem>().ToArray()) {
                var listRequest = listViewItem.Tag as ServerRequest;
                if(listRequest != null) {
                    var unhandledNode = findUnhandledRequestNode(listRequest);
                    if(unhandledNode == null) {
                        listView.Items.Remove(listViewItem);
                    } else {
                        var request = unhandledNode.Value;
                        unhandledRequests.Remove(unhandledNode);
                        if(request.DataVersion != listRequest.DataVersion) {
                            listViewItem.Tag = request;
                            RefreshListViewItem(listViewItem);
                        }
                    }
                }
            }

            for(var newNode = unhandledRequests.First;newNode != null;newNode = newNode.Next) {
                var request = newNode.Value;
                var listViewItem = new ListViewItem() {
                    Tag = request,
                };
                RefreshListViewItem(listViewItem);

                listView.Items.Add(listViewItem);
            }

            listView.Sort();
        }

        private void RefreshListViewItem(ListViewItem listViewItem)
        {
            var request = listViewItem.Tag as ServerRequest;

            while(listViewItem.SubItems.Count < 5) listViewItem.SubItems.Add("");
            listViewItem.SubItems[0].Text = request.RemoteAddress;
            listViewItem.SubItems[1].Text = request.UserName;
            listViewItem.SubItems[2].Text = request.LastRequest.ToString("G");
            listViewItem.SubItems[3].Text = request.BytesSent.ToString("N0");
            listViewItem.SubItems[4].Text = request.LastUrl;
        }
        #endregion
    }
}
