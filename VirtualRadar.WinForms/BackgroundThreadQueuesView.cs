// Copyright © 2015 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="IBackgroundThreadQueuesView"/>.
    /// </summary>
    public partial class BackgroundThreadQueuesView : BaseForm, IBackgroundThreadQueuesView
    {
        /// <summary>
        /// The detail associated with each queue on display.
        /// </summary>
        class QueueDetail
        {
            public ListViewItem ListViewItem;

            public IQueue Queue;

            public string Name;

            public int Count;

            public int Peak;
        }

        /// <summary>
        /// A private class that handles the sorting of the list view for us.
        /// </summary>
        class Sorter : AutoListViewSorter
        {
            private BackgroundThreadQueuesView _Parent;

            public Sorter(BackgroundThreadQueuesView parent) : base(parent.listView)
            {
                _Parent = parent;
            }

            public override IComparable GetRowValue(ListViewItem listViewItem)
            {
                var result = base.GetRowValue(listViewItem);
                var queueDetail = listViewItem.Tag as QueueDetail;
                if(queueDetail != null) {
                    var column = SortColumn ?? _Parent.columnHeaderName;
                    if(column == _Parent.columnHeaderCount) result = queueDetail.Count;
                    else if(column == _Parent.columnHeaderPeakCount) result = queueDetail.Peak;
                }

                return result;
            }
        }

        /// <summary>
        /// The object that controls this form.
        /// </summary>
        private IBackgroundThreadQueuesPresenter _Presenter;

        /// <summary>
        /// The object that takes care of sorting the list view for us.
        /// </summary>
        private Sorter _Sorter;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BackgroundThreadQueuesView()
        {
            InitializeComponent();

            _Sorter = new Sorter(this);
            listView.ListViewItemSorter = _Sorter;
        }

        /// <summary>
        /// Called after the form has loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);

                _Presenter = Factory.Singleton.Resolve<IBackgroundThreadQueuesPresenter>();
                _Presenter.Initialise(this);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="queues"></param>
        public void RefreshDisplay(IQueue[] queues)
        {
            if(InvokeRequired) {
                try {
                    BeginInvoke(new MethodInvoker(() => { RefreshDisplay(queues); }));
                } catch(InvalidOperationException) {
                    ; // Required for mono
                }
            } else {
                var queueDetails = GetQueueDetails();
                AddOrUpdateExistingQueues(queues, queueDetails);
                RemoveOldQueues(queueDetails);
                listView.Sort();
            }
        }

        /// <summary>
        /// Returns an array of queue details taken from the list view.
        /// </summary>
        /// <returns></returns>
        private List<QueueDetail> GetQueueDetails()
        {
            return listView.Items.OfType<ListViewItem>().Select(r => (QueueDetail)r.Tag).ToList();
        }

        /// <summary>
        /// Updates all existing queues in the list or adds new ones, removing existing ones from <paramref name="queueDetails"/>.
        /// </summary>
        /// <param name="queues"></param>
        /// <param name="queueDetails"></param>
        private void AddOrUpdateExistingQueues(IQueue[] queues, List<QueueDetail> queueDetails)
        {
            foreach(var queue in queues) {
                var queueDetail = queueDetails.FirstOrDefault(r => Object.ReferenceEquals(r.Queue, queue));
                if(queueDetail != null) {
                    queueDetails.Remove(queueDetail);
                    UpdateQueueDisplay(queue, queueDetail, forceRefresh: false);
                } else {
                    queueDetail = new QueueDetail() {
                        Queue = queue,
                        ListViewItem = new ListViewItem(new string[] { "", "", "", }),
                    };
                    queueDetail.ListViewItem.Tag = queueDetail;
                    UpdateQueueDisplay(queue, queueDetail, forceRefresh: true);

                    listView.Items.Add(queueDetail.ListViewItem);
                }
            }
        }

        /// <summary>
        /// Removes queues that are no longer in commission.
        /// </summary>
        /// <param name="queueDetails"></param>
        private void RemoveOldQueues(List<QueueDetail> queueDetails)
        {
            foreach(var queueDetail in queueDetails) {
                listView.Items.Remove(queueDetail.ListViewItem);
                queueDetail.ListViewItem.Tag = null;
            }
        }

        /// <summary>
        /// Updates the display of a queue detail if anything has changed.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="queueDetail"></param>
        /// <param name="forceRefresh"></param>
        private void UpdateQueueDisplay(IQueue queue, QueueDetail queueDetail, bool forceRefresh)
        {
            var item = queueDetail.ListViewItem;

            var countQueuedItems = queue.CountQueuedItems;
            var peakQueuedItems = queue.PeakQueuedItems;

            if(forceRefresh || queue.Name != queueDetail.Name) {
                item.SubItems[0].Text = queue.Name;
                queueDetail.Name = queue.Name;
            }

            if(forceRefresh || countQueuedItems != queueDetail.Count) {
                item.SubItems[1].Text = countQueuedItems.ToString("N0");
                queueDetail.Count = countQueuedItems;
            }

            if(forceRefresh || peakQueuedItems != queueDetail.Peak) {
                item.SubItems[2].Text = peakQueuedItems.ToString("N0");
                queueDetail.Peak = peakQueuedItems;
            }
        }
    }
}
