// Copyright © 2013 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A user control that can show a list of <see cref="IFeed"/>s.
    /// </summary>
    public partial class FeedStatusControl : BaseUserControl
    {
        #region Private class - FeedDetail
        /// <summary>
        /// A private class that holds a copy of the information shown or recorded for a feed.
        /// </summary>
        class FeedDetail
        {
            public ListViewItem ListViewItem { get; set; }

            public int UniqueId { get; set; }

            public string Name { get; set; }

            public bool IsMergedFeed { get; set; }

            public bool HasPolarPlotter { get; set; }

            public ConnectionStatus ConnectionStatus { get; set; }

            public long TotalMessages { get; set; }

            public long TotalBadMessages { get; set; }

            public long AircraftCount { get; set; }
        }
        #endregion

        #region Private class - Sorter
        /// <summary>
        /// A private class that handles the sorting of the list view for us.
        /// </summary>
        class Sorter : AutoListViewSorter
        {
            private FeedStatusControl _Parent;

            public Sorter(FeedStatusControl parent) : base(parent.listView)
            {
                _Parent = parent;
            }

            public override IComparable GetRowValue(ListViewItem listViewItem)
            {
                var result = base.GetRowValue(listViewItem);
                var feedDetail = listViewItem.Tag as FeedDetail;
                if(feedDetail != null) {
                    var column = SortColumn ?? _Parent.columnHeaderName;
                    if(column == _Parent.columnHeaderAircraftCount)         result = feedDetail.AircraftCount;
                    else if(column == _Parent.columnHeaderConnectionStatus) result = (int)feedDetail.ConnectionStatus;
                    else if(column == _Parent.columnHeaderTotalBadMessages) result = feedDetail.TotalBadMessages;
                    else if(column == _Parent.columnHeaderTotalMessages)    result = feedDetail.TotalMessages;
                }

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that takes care of sorting the list view for us.
        /// </summary>
        private Sorter _Sorter;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the selected feeds.
        /// </summary>
        private FeedDetail[] SelectedFeeds
        {
            get { return GetAllSelectedListViewTag<FeedDetail>(listView); }
            set { SelectListViewItemsByTags(listView, value); }
        }
        #endregion

        #region Events exposed
        /// <summary>
        /// Raised when the user wants to reconnect a feed.
        /// </summary>
        public event EventHandler<FeedIdEventArgs> ReconnectFeedId;
        /// <summary>
        /// Raises <see cref="ReconnectFeedId"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnReconnectFeedId(FeedIdEventArgs args)
        {
            if(ReconnectFeedId != null) ReconnectFeedId(this, args);
        }

        /// <summary>
        /// Raised when the user wants to see statistics for a feed.
        /// </summary>
        public event EventHandler<FeedIdEventArgs> ShowFeedIdStatistics;
        /// <summary>
        /// Raises <see cref="ShowFeedIdStatistics"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnShowFeedIdStatistics(FeedIdEventArgs args)
        {
            if(ShowFeedIdStatistics != null) ShowFeedIdStatistics(this, args);
        }

        /// <summary>
        /// Raised when the user wants to reset polar plotters.
        /// </summary>
        public event EventHandler<FeedIdEventArgs> ResetPolarPlotter;
        /// <summary>
        /// Raises <see cref="ResetPolarPlotter"/>.
        /// </summary>
        /// <param name="args"></param>
        public virtual void OnResetPolarPlotter(FeedIdEventArgs args)
        {
            if(ResetPolarPlotter != null) ResetPolarPlotter(this, args);
        }

        /// <summary>
        /// Raised when the user wants to configure a feed.
        /// </summary>
        public event EventHandler<FeedIdEventArgs> ConfigureFeed;
        /// <summary>
        /// Raises <see cref="ConfigureFeed"/>.
        /// </summary>
        /// <param name="args"></param>
        public virtual void OnConfigureFeed(FeedIdEventArgs args)
        {
            if(ConfigureFeed != null) ConfigureFeed(this, args);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FeedStatusControl()
        {
            InitializeComponent();
            _Sorter = new Sorter(this);
            listView.ListViewItemSorter = _Sorter;
        }
        #endregion

        #region ShowFeeds, ShowFeed
        /// <summary>
        /// Updates the list of feeds.
        /// </summary>
        /// <param name="feeds"></param>
        public void ShowFeeds(IFeed[] feeds)
        {
            if(InvokeRequired) BeginInvoke(new MethodInvoker(() => { ShowFeeds(feeds); }));
            else {
                var feedDetails = GetFeedDetails();
                AddOrUpdateExistingFeeds(feeds, feedDetails);
                RemoveOldFeeds(feedDetails);
                listView.Sort();
            }
        }

        /// <summary>
        /// Updates the display for a single feed.
        /// </summary>
        /// <param name="feed"></param>
        public void ShowFeed(IFeed feed)
        {
            if(InvokeRequired) BeginInvoke(new MethodInvoker(() => { ShowFeed(feed); }));
            else {
                var feedDetails = GetFeedDetails();
                var feedDetail = feedDetails.FirstOrDefault(r => r.UniqueId == feed.UniqueId);
                if(feedDetail != null) {
                    UpdateFeedDisplay(feed, feedDetail, forceRefresh: false);
                    listView.Sort();
                }
            }
        }

        /// <summary>
        /// Updates all existing feeds in the list or adds new ones, removing existing ones from <paramref name="feedDetails"/>.
        /// </summary>
        /// <param name="feeds"></param>
        /// <param name="feedDetails"></param>
        private void AddOrUpdateExistingFeeds(IFeed[] feeds, List<FeedDetail> feedDetails)
        {
            foreach(var feed in feeds) {
                var aircraftList = feed.AircraftList;
                var listener = feed.Listener;

                if(aircraftList == null || listener == null) {
                    // They can be null if the feed is in the process of being disposed on another thread.
                    // These need to be left in feedDetails so that the caller knows they no longer exist.
                } else {
                    var feedDetail = feedDetails.FirstOrDefault(r => r.UniqueId == feed.UniqueId);
                    if(feedDetail != null) {
                        feedDetail.HasPolarPlotter = aircraftList.PolarPlotter != null;
                        feedDetails.Remove(feedDetail);
                        UpdateFeedDisplay(feed, feedDetail, forceRefresh: false);
                    } else {
                        feedDetail = new FeedDetail() {
                            UniqueId = feed.UniqueId,
                            IsMergedFeed = listener is IMergedFeedListener,
                            HasPolarPlotter = aircraftList.PolarPlotter != null,
                            ListViewItem = listView.Items.Add(new ListViewItem(new string[] { "", "", "", "", "" })),
                        };
                        feedDetail.ListViewItem.Tag = feedDetail;
                        UpdateFeedDisplay(feed, feedDetail, forceRefresh: true);
                    }
                }
            }
        }

        /// <summary>
        /// Removes feeds that are no longer in commission.
        /// </summary>
        /// <param name="feedDetails"></param>
        private void RemoveOldFeeds(List<FeedDetail> feedDetails)
        {
            foreach(var feedDetail in feedDetails) {
                listView.Items.Remove(feedDetail.ListViewItem);
                feedDetail.ListViewItem.Tag = null;
            }
        }
        #endregion

        #region GetFeedDetails, UpdateFeedDisplay
        /// <summary>
        /// Returns an array of feed details taken from the list view.
        /// </summary>
        /// <returns></returns>
        private List<FeedDetail> GetFeedDetails()
        {
            return listView.Items.OfType<ListViewItem>().Select(r => (FeedDetail)r.Tag).ToList();
        }

        /// <summary>
        /// Updates the display of a feed detail if anything has changed.
        /// </summary>
        /// <param name="feed"></param>
        /// <param name="feedDetail"></param>
        /// <param name="forceRefresh"></param>
        private void UpdateFeedDisplay(IFeed feed, FeedDetail feedDetail, bool forceRefresh)
        {
            var item = feedDetail.ListViewItem;

            // We can potentially get into a situation whereby the feed is disposed of while we're using it. If
            // that happens then the AircraftList and Listener properties are nulled out - so we need to take
            // separate copies of those.
            var feedAircraftList = feed.AircraftList;
            var feedListener = feed.Listener;

            if(forceRefresh || feed.Name != feedDetail.Name) {
                item.SubItems[0].Text = feed.Name;
                feedDetail.Name = feed.Name;
            }

            if(feedListener != null) {
                if(forceRefresh || feedListener.ConnectionStatus != feedDetail.ConnectionStatus) {
                    item.SubItems[1].Text = Describe.ConnectionStatus(feedListener.ConnectionStatus);
                    feedDetail.ConnectionStatus = feedListener.ConnectionStatus;
                }

                if(forceRefresh || feedListener.TotalMessages != feedDetail.TotalMessages) {
                    item.SubItems[2].Text = feedListener.TotalMessages.ToString("N0");
                    feedDetail.TotalMessages = feedListener.TotalMessages;
                }

                if(forceRefresh || feedListener.TotalBadMessages != feedDetail.TotalBadMessages) {
                    item.SubItems[3].Text = feedListener.TotalBadMessages.ToString("N0");
                    feedDetail.TotalBadMessages = feedListener.TotalBadMessages;
                }
            }

            if(feedAircraftList != null) {
                if(forceRefresh || feedAircraftList.Count != feedDetail.AircraftCount) {
                    item.SubItems[4].Text = feedAircraftList.Count.ToString("N0");
                    feedDetail.AircraftCount = feedAircraftList.Count;
                }
            }
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Called after the control has initialised but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Control(this);
                Localise.Control(contextMenuStrip);
                _Sorter.RefreshSortIndicators();
            }
        }

        /// <summary>
        /// Raised before the context menu is opened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void contextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            var selectedFeeds = SelectedFeeds;
            var hasReceiverFeed = selectedFeeds.Any(r => !r.IsMergedFeed);
            var hasPolarPlotter = selectedFeeds.Any(r => r.HasPolarPlotter);

            reconnectDataFeedToolStripMenuItem.Enabled = hasReceiverFeed;
            menuStatisticsToolStripMenuItem.Enabled = hasReceiverFeed;
            menuResetReceiverRangeToolStripMenuItem.Enabled = hasPolarPlotter;
        }

        private void RaiseEventForAllSelectedReceiverFeeds(Action<FeedIdEventArgs> eventCaller)
        {
            foreach(var feed in SelectedFeeds.Where(r => !r.IsMergedFeed)) {
                eventCaller(new FeedIdEventArgs(feed.UniqueId));
            }
        }

        private void RaiseEventForAllSelectedReceiverAndMergedFeeds(Action<FeedIdEventArgs> eventCaller)
        {
            foreach(var feed in SelectedFeeds) {
                eventCaller(new FeedIdEventArgs(feed.UniqueId));
            }
        }

        /// <summary>
        /// Raised when the user elects to reconnect to one or more data feeds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void reconnectDataFeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RaiseEventForAllSelectedReceiverFeeds(OnReconnectFeedId);
        }

        /// <summary>
        /// Raised when the user elects to view a data feed's statistics.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuStatisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RaiseEventForAllSelectedReceiverFeeds(OnShowFeedIdStatistics);
        }

        /// <summary>
        /// Raised when the user double-clicks an item in the list view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_DoubleClick(object sender, EventArgs e)
        {
            RaiseEventForAllSelectedReceiverFeeds(OnShowFeedIdStatistics);
        }

        /// <summary>
        /// Raised when the user clicks the context menu item to reset the polar plot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuResetReceiverRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RaiseEventForAllSelectedReceiverFeeds(OnResetPolarPlotter);
        }

        /// <summary>
        /// Raised when the user clicks the context menu item to configure feeds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuConfigureFeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedFeeds = SelectedFeeds;
            if(selectedFeeds.Length > 0) {
                OnConfigureFeed(new FeedIdEventArgs(selectedFeeds[0].UniqueId));
            }
        }
        #endregion
    }
}
