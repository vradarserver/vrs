// Copyright © 2014 onwards, Andrew Whewell
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Interface;
using VirtualRadar.WinForms.SettingPage;
using VirtualRadar.Resources;
using System.Collections.Specialized;
using System.Collections;
using System.Linq.Expressions;
using VirtualRadar.Interface.PortableBinding;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="ISettingsView"/>.
    /// </summary>
    public partial class SettingsView : BaseForm, ISettingsView, INotifyPropertyChanged
    {
        #region TreeViewSorter
        /// <summary>
        /// The comparer that keeps the items in the tree view in the correct order.
        /// </summary>
        /// <remarks>
        /// The top-level pages are always shown in the order in which they were added to
        /// the tree view, they are not sorted. Child pages for parents whose
        /// <see cref="Page.ShowChildPagesInAlphabeticalOrder"/> is true are shown in
        /// alphabetical order of page title, otherwise they are shown in the order in
        /// which they were added.
        /// </remarks>
        class TreeViewSorter : IComparer
        {
            private SettingsView _Parent;

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="parent"></param>
            public TreeViewSorter(SettingsView parent)
            {
                _Parent = parent;
            }

            /// <summary>
            /// See interface docs.
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public int Compare(object x, object y)
            {
                int result = 0;
                var lhsNode = x as TreeNode;
                var rhsNode = y as TreeNode;
                if(lhsNode != null && rhsNode != null && lhsNode.Parent == rhsNode.Parent) {
                    var parentNode = lhsNode.Parent;
                    var lhsPageSummary = lhsNode.Tag as PageSummary;
                    var rhsPageSummary = rhsNode.Tag as PageSummary;
                    var parentPageSummary = parentNode == null ? null : parentNode.Tag as PageSummary;

                    if(parentPageSummary != null && parentPageSummary.ShowChildPagesInAlphabeticalOrder) {
                        result = String.Compare(lhsPageSummary == null ? null : lhsPageSummary.PageTitle, rhsPageSummary == null ? null : rhsPageSummary.PageTitle, StringComparison.CurrentCultureIgnoreCase);
                    } else {
                        var pages = parentPageSummary == null ? (IList<PageSummary>)_Parent._TopLevelPageSummaries : parentPageSummary.ChildPages;
                        var lhsIndex = lhsPageSummary == null ? -1 : pages.IndexOf(lhsPageSummary);
                        var rhsIndex = rhsPageSummary == null ? -1 : pages.IndexOf(rhsPageSummary);
                        if(lhsIndex == -1) lhsIndex = int.MaxValue;
                        if(rhsIndex == -1) rhsIndex = int.MaxValue;

                        result = lhsIndex - rhsIndex;
                    }
                }

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The object that holds all of the business logic for the form.
        /// </summary>
        private ISettingsPresenter _Presenter;

        /// <summary>
        /// The object that manages the tree view's image list for us.
        /// </summary>
        private DynamicImageList _ImageList = new DynamicImageList();

        /// <summary>
        /// The object that sorts the tree view for us.
        /// </summary>
        private TreeViewSorter _TreeViewSorter;

        /// <summary>
        /// The object that is listening to the configuration for changes.
        /// </summary>
        private IConfigurationListener _ConfigurationListener;

        /// <summary>
        /// A list of all page summaries that have had their events hooked.
        /// </summary>
        private List<PageSummary> _HookedPageSummaries = new List<PageSummary>();

        /// <summary>
        /// True if a save event handler is running, false otherwise.
        /// </summary>
        private bool _IsSaving;

        /// <summary>
        /// True if the last full-form validation results indicated a failed validation.
        /// </summary>
        private bool _LastValidationFailed;

        // All of the top-level pages in the order in which they are shown to the user.
        private PageSummary[] _TopLevelPageSummaries = new PageSummary[] {
            new PageDataSources.Summary(),
            new PageReceivers.Summary(),
            new PageReceiverLocations.Summary(),
            new PageMergedFeeds.Summary(),
            new PageRebroadcastServers.Summary(),
            new PageUsers.Summary(),
            new PageRawFeedDecoding.Summary(),
            new PageWebServer.Summary(),
            new PageWebSite.Summary(),
            new PageGeneral.Summary(),
        };
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the currently selected page summary in the tree view.
        /// </summary>
        public PageSummary CurrentTreePageSummary
        {
            get { return treeViewPagePicker.SelectedNode == null ? null : treeViewPagePicker.SelectedNode.Tag as PageSummary; }
            set {
                if(value == null) treeViewPagePicker.SelectedNode = null;
                else              treeViewPagePicker.SelectedNode = value.TreeNode;
            }
        }

        /// <summary>
        /// Gets or sets the summary for the currently selected page in the panel.
        /// </summary>
        public PageSummary CurrentPanelPageSummary
        {
            get { return panelPageContent.Tag as PageSummary; }
            set { DisplayPage(value); }
        }

        /// <summary>
        /// Gets or sets the inline help title.
        /// </summary>
        public string InlineHelpTitle
        {
            get { return labelInlineHelpTitle.Text; }
            set { labelInlineHelpTitle.Text = value; }
        }

        /// <summary>
        /// Gets or sets the inline help text.
        /// </summary>
        public string InlineHelp
        {
            get { return labelInlineHelp.Text; }
            set { labelInlineHelp.Text = value; }
        }

        private Configuration _Configuration;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public Configuration Configuration
        {
            get { return _Configuration; }
            set {
                _ConfigurationListener.Initialise(value);
                SetField(ref _Configuration, value, () => Configuration);
            }
        }

        private NotifyList<IUser> _Users = new NotifyList<IUser>();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public NotifyList<IUser> Users { get { return _Users; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserManager { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string OpenOnPageTitle { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public object OpenOnRecord { get; set; }

        private NotifyList<CombinedFeed> _CombinedFeed = new NotifyList<CombinedFeed>();
        /// <summary>
        /// Gets a combined collection of receivers and merged feeds.
        /// </summary>
        public NotifyList<CombinedFeed> CombinedFeed
        {
            get { return _CombinedFeed; }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if(PropertyChanged != null) PropertyChanged(this, args);
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="selectorExpression"></param>
        /// <returns></returns>
        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if(EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;

            if(selectorExpression == null) throw new ArgumentNullException("selectorExpression");
            MemberExpression body = selectorExpression.Body as MemberExpression;
            if(body == null) throw new ArgumentException("The body must be a member expression");
            OnPropertyChanged(new PropertyChangedEventArgs(body.Member.Name));

            return true;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler SaveClicked;

        /// <summary>
        /// Raises <see cref="SaveClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnSaveClicked(EventArgs args)
        {
            if(SaveClicked != null) SaveClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Receiver>> TestConnectionClicked;

        /// <summary>
        /// Raises <see cref="TestConnectionClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        public void RaiseTestConnectionClicked(EventArgs<Receiver> args)
        {
            if(TestConnectionClicked != null) TestConnectionClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler TestTextToSpeechSettingsClicked;

        /// <summary>
        /// Raises <see cref="TestTextToSpeechSettingsClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        public void RaiseTestTextToSpeechSettingsClicked(EventArgs args)
        {
            if(TestTextToSpeechSettingsClicked != null) TestTextToSpeechSettingsClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler UpdateReceiverLocationsFromBaseStationDatabaseClicked;

        /// <summary>
        /// Raises <see cref="UpdateReceiverLocationsFromBaseStationDatabaseClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        public void RaiseUpdateReceiverLocationsFromBaseStationDatabaseClicked(EventArgs args)
        {
            if(UpdateReceiverLocationsFromBaseStationDatabaseClicked != null) UpdateReceiverLocationsFromBaseStationDatabaseClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler UseIcaoRawDecodingSettingsClicked;

        /// <summary>
        /// Raises <see cref="UseIcaoRawDecodingSettingsClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        public void RaiseUseIcaoRawDecodingSettingsClicked(EventArgs args)
        {
            if(UseIcaoRawDecodingSettingsClicked != null) UseIcaoRawDecodingSettingsClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler UseRecommendedRawDecodingSettingsClicked;
        
        /// <summary>
        /// Raises <see cref="UseRecommendedRawDecodingSettingsClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        public void RaiseUseRecommendedRawDecodingSettingsClicked(EventArgs args)
        {
            if(UseRecommendedRawDecodingSettingsClicked != null) UseRecommendedRawDecodingSettingsClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler FlightSimulatorXOnlyClicked;

        /// <summary>
        /// Raises <see cref="FlightSimulatorXOnlyClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnFlightSimulatorXOnlyClicked(EventArgs args)
        {
            if(FlightSimulatorXOnlyClicked != null) FlightSimulatorXOnlyClicked(this, args);
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SettingsView()
        {
            _ConfigurationListener = Factory.Singleton.Resolve<IConfigurationListener>();
            _ConfigurationListener.PropertyChanged += ConfigurationListener_PropertyChanged;
            _Users.ListChanged += Users_ListChanged;

            InitializeComponent();

            _TreeViewSorter = new TreeViewSorter(this);
            treeViewPagePicker.TreeViewNodeSorter = _TreeViewSorter;
        }
        #endregion

        #region OnLoad, DisplayInitialPage
        /// <summary>
        /// Called after the form has been loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Form(this);
                treeViewPagePicker.ImageList = _ImageList.ImageList;

                InlineHelp = "";
                InlineHelpTitle = "";

                _Presenter = Factory.Singleton.Resolve<ISettingsPresenter>();
                _Presenter.Initialise(this);

                InitialiseCombinedFeed();

                foreach(var pageSummary in _TopLevelPageSummaries) {
                    AddPageSummary(pageSummary, null);
                }
                treeViewPagePicker.ExpandAll();

                DisplayInitialPage();

                _Presenter.ValidateView();
            }
        }

        /// <summary>
        /// Gets the initial page on display.
        /// </summary>
        private void DisplayInitialPage()
        {
            PageSummary initialPageSummary = null;

            if(OpenOnRecord != null) {
                foreach(var pageSummary in GetAllPageSummaries()) {
                    if(pageSummary.IsForSameRecord(OpenOnRecord)) {
                        initialPageSummary = pageSummary;
                        break;
                    }
                }
            }

            if(initialPageSummary == null && OpenOnPageTitle != null) {
                foreach(var pageSummary in GetAllPageSummaries()) {
                    if(pageSummary.PageTitle == OpenOnPageTitle) {
                        initialPageSummary = pageSummary;
                        break;
                    }
                }
            }

            if(initialPageSummary == null) initialPageSummary = _TopLevelPageSummaries.First();

            DisplayPage(initialPageSummary);
        }
        #endregion

        #region CombinedFeed handling
        /// <summary>
        /// One-time setup of the <see cref="CombinedFeed"/> collection, including
        /// the hooking of the constituent lists.
        /// </summary>
        private void InitialiseCombinedFeed()
        {
            Configuration.Receivers.ListChanged += CombinedFeed_Source_ListChanged;
            Configuration.MergedFeeds.ListChanged += CombinedFeed_Source_ListChanged;

            SyncCombinedFeed();
        }

        /// <summary>
        /// Ensures that the combined feed contains only those records that are present in the source lists.
        /// </summary>
        private void SyncCombinedFeed()
        {
            // Sync receivers
            var cfReceivers = CombinedFeed.Where(r => r.Receiver != null).ToArray();
            var deletedCfReceivers = cfReceivers.Where(r => !Configuration.Receivers.Any(i => i == r.Receiver)).ToArray();
            var newCfReceivers = Configuration.Receivers.Except(cfReceivers.Select(r => r.Receiver)).ToArray();
            foreach(var deleteCombinedFeed in deletedCfReceivers) {
                CombinedFeed.Remove(deleteCombinedFeed);
                deleteCombinedFeed.Dispose();
            }
            foreach(var newReceiver in newCfReceivers) {
                CombinedFeed.Add(new CombinedFeed(newReceiver));
            }

            // Sync merged feeds
            var cfMergedFeeds = CombinedFeed.Where(r => r.MergedFeed != null).ToArray();
            var deletedCfMergedFeeds = cfMergedFeeds.Where(r => !Configuration.MergedFeeds.Any(i => i == r.MergedFeed)).ToArray();
            var newCfMergedFeeds = Configuration.MergedFeeds.Except(cfMergedFeeds.Select(r => r.MergedFeed)).ToArray();
            foreach(var deleteCombinedFeed in deletedCfMergedFeeds) {
                CombinedFeed.Remove(deleteCombinedFeed);
                deleteCombinedFeed.Dispose();
            }
            foreach(var newMergedFeed in newCfMergedFeeds) {
                CombinedFeed.Add(new CombinedFeed(newMergedFeed));
            }
        }

        /// <summary>
        /// Called whenever a source list for the combined feed changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CombinedFeed_Source_ListChanged(object sender, ListChangedEventArgs args)
        {
            SyncCombinedFeed();
        }
        #endregion

        #region Page handling
        /// <summary>
        /// Adds a page summary to the tree-view and to the owner's ChildPages collection. If the
        /// owner is null then the page is added to the top-level of the pages display.
        /// </summary>
        /// <param name="pageSummary"></param>
        /// <param name="owner"></param>
        public void AddPageSummary(PageSummary pageSummary, PageSummary owner)
        {
            var parentNodes = owner == null ? treeViewPagePicker.Nodes : owner.TreeNode.Nodes;

            if(pageSummary.TreeNode == null) {
                pageSummary.TreeNode = CreatePageSummaryTreeNode(pageSummary);
                RefreshPageSummaryTreeNode(pageSummary);
                parentNodes.Add(pageSummary.TreeNode);
            }

            if(pageSummary.SettingsView == null) {
                pageSummary.SettingsView = this;
            }

            foreach(var childPageSummary in pageSummary.ChildPages) {
                AddPageSummary(childPageSummary, pageSummary);
            }

            if(!_HookedPageSummaries.Contains(pageSummary)) {
                pageSummary.ChildPages.ListChanged += PageSummary_ChildPages_ListChanged;
            }
        }

        /// <summary>
        /// Removes the page from display.
        /// </summary>
        /// <param name="pageSummary"></param>
        /// <param name="makeParentCurrent"></param>
        public void RemovePageSummary(PageSummary pageSummary, bool makeParentCurrent)
        {
            if(pageSummary != null) {
                pageSummary.PageDetaching();
                
                if(_HookedPageSummaries.Contains(pageSummary)) {
                    pageSummary.ChildPages.ListChanged -= PageSummary_ChildPages_ListChanged;
                }

                foreach(var subPage in pageSummary.ChildPages) {
                    RemovePageSummary(subPage, makeParentCurrent);
                }

                if(pageSummary.SettingsView != null) {
                    pageSummary.SettingsView = null;
                }

                if(pageSummary.TreeNode != null) {
                    pageSummary.TreeNode.Parent.Nodes.Remove(pageSummary.TreeNode);
                    pageSummary.TreeNode = null;
                }

                if(pageSummary.Page != null) {
                    if(panelPageContent.Controls.Contains(pageSummary.Page)) {
                        panelPageContent.Controls.Remove(pageSummary.Page);
                        pageSummary.Page.Dispose();
                        pageSummary.Page = null;
                    }
                }


                var allSummaries = GetAllPageSummariesInTreeNodeOrder();
                var pageIndex = allSummaries.IndexOf(pageSummary);
                if(pageIndex != -1) {
                    allSummaries.RemoveAt(pageIndex);

                    var parentSummary = FindParentPageSummary(pageSummary);
                    if(CurrentTreePageSummary == pageSummary) {
                        CurrentTreePageSummary = null;
                        if(makeParentCurrent && parentSummary != null) {
                            DisplayPage(parentSummary);
                        } else {
                            if(pageIndex < allSummaries.Count) {
                                DisplayPage(allSummaries[pageIndex]);
                            } else if(allSummaries.Count > 0) {
                                DisplayPage(allSummaries[allSummaries.Count - 1]);
                            } else if(parentSummary != null) {
                                DisplayPage(parentSummary);
                            }
                        }
                    }
                }

                pageSummary.PageDetached();
            }
        }

        /// <summary>
        /// Displays the page for the summary passed across, creating the summary if necessary.
        /// </summary>
        /// <param name="pageSummary"></param>
        public void DisplayPage(PageSummary pageSummary)
        {
            if(pageSummary != null && pageSummary != CurrentPanelPageSummary && GetAllPageSummaries().Contains(pageSummary)) {
                panelPageContent.SuspendLayout();
                try {
                    if(pageSummary.Page == null) {
                        pageSummary.CreatePage();
                        var page = pageSummary.Page;

                        var panelContainsPage = panelPageContent.Controls.Contains(page);
                        if(!panelContainsPage) {
                            page.Visible = false;
                            page.Width = panelPageContent.Width - 8;
                            page.Location = new Point(4, 4);
                            page.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                            if(page.PageUseFullHeight) {
                                page.Height = panelPageContent.Height - 8;
                                page.Anchor |= AnchorStyles.Bottom;
                            }

                            panelPageContent.Controls.Add(page);
                        }
                    }

                    var hideSummary = CurrentPanelPageSummary;
                    panelPageContent.Tag = pageSummary;
                    CurrentPanelPageSummary.Page.Visible = true;
                    CurrentPanelPageSummary.Page.Initialise();
                    if(hideSummary != null) hideSummary.Page.Visible = false;
                } finally {
                    panelPageContent.ResumeLayout();
                }

                if(CurrentPanelPageSummary != pageSummary) {
                    CurrentPanelPageSummary = pageSummary;
                }

                pageSummary.Page.PageSelected();
            }
        }

        /// <summary>
        /// Displays the page for the page object passed across.
        /// </summary>
        /// <param name="pageObject"></param>
        public void DisplayPageForPageObject(object pageObject)
        {
            var pageSummary = FindPageSummaryForPageObject(pageObject);
            if(pageSummary != null) DisplayPage(pageSummary);
        }

        /// <summary>
        /// Returns a flattened collection of every page summary in a random order.
        /// </summary>
        /// <returns></returns>
        private List<PageSummary> GetAllPageSummaries()
        {
            return _TopLevelPageSummaries.Concat(_TopLevelPageSummaries.SelectMany(r => r.ChildPages)).ToList();
        }

        /// <summary>
        /// Returns a flattened collection of page summaries in the order of their tree node.
        /// </summary>
        /// <returns></returns>
        private List<PageSummary> GetAllPageSummariesInTreeNodeOrder()
        {
            var result = new List<PageSummary>();
            var allSummaries = GetAllPageSummaries();

            AddPageSummaries(treeViewPagePicker.Nodes, allSummaries, result);

            return result;
        }

        /// <summary>
        /// Recursively adds page summaries to the tree.
        /// </summary>
        /// <param name="treeNodes"></param>
        /// <param name="allSummaries"></param>
        /// <param name="sortedPageSummaries"></param>
        private void AddPageSummaries(TreeNodeCollection treeNodes, List<PageSummary> allSummaries, List<PageSummary> sortedPageSummaries)
        {
            foreach(TreeNode treeNode in treeNodes) {
                var pageSummary = allSummaries.FirstOrDefault(r => r.TreeNode == treeNode);
                if(pageSummary != null) sortedPageSummaries.Add(pageSummary);
                AddPageSummaries(treeNode.Nodes, allSummaries, sortedPageSummaries);
            }
        }

        /// <summary>
        /// Finds the summary that is the parent of the one passed across. Returns null if there is no
        /// parent summary for it.
        /// </summary>
        /// <param name="pageSummary"></param>
        /// <returns></returns>
        public PageSummary FindParentPageSummary(PageSummary pageSummary)
        {
            PageSummary result = null;

            if(pageSummary != null) {
                result = GetAllPageSummaries().FirstOrDefault(r => r.ChildPages.Contains(pageSummary));
            }

            return result;
        }

        /// <summary>
        /// Finds the page summary that contains the page object passed across. Returns null if no summary could
        /// be found.
        /// </summary>
        /// <param name="pageObject"></param>
        /// <returns></returns>
        public PageSummary FindPageSummaryForPageObject(object pageObject)
        {
            PageSummary result = null;

            if(pageObject != null) {
                result = GetAllPageSummaries().FirstOrDefault(r => r.PageObject == pageObject);
            }

            return result;
        }
        #endregion

        #region TreeView handling
        /// <summary>
        /// Creates a TreeNode for the page summary passed across.
        /// </summary>
        /// <param name="pageSummary"></param>
        /// <returns></returns>
        private TreeNode CreatePageSummaryTreeNode(PageSummary pageSummary)
        {
            if(pageSummary.TreeNode == null) {
                pageSummary.TreeNode = new TreeNode();
                RefreshPageSummaryTreeNode(pageSummary);
            }

            return pageSummary.TreeNode;
        }

        /// <summary>
        /// Refreshes the treeview description for a page summary.
        /// </summary>
        /// <param name="pageSummary"></param>
        public void RefreshPageSummaryTreeNode(PageSummary pageSummary)
        {
            if(pageSummary.TreeNode != null) {
                var title =     pageSummary.PageTitle ?? "";
                var icon =      pageSummary.PageIcon ?? Images.Transparent_16x16;
                var iconIndex = _ImageList.AddImage(icon);
                var colour =    pageSummary.PageEnabled ? treeViewPagePicker.ForeColor : SystemColors.GrayText;

                if(pageSummary.TreeNode.Tag != pageSummary)                 pageSummary.TreeNode.Tag = pageSummary;
                if(pageSummary.TreeNode.Text != title)                      pageSummary.TreeNode.Text = title;
                if(pageSummary.TreeNode.ForeColor != colour)                pageSummary.TreeNode.ForeColor = colour;
                if(pageSummary.TreeNode.ImageIndex != iconIndex)            pageSummary.TreeNode.ImageIndex = iconIndex;
                if(pageSummary.TreeNode.SelectedImageIndex != iconIndex)    pageSummary.TreeNode.SelectedImageIndex = iconIndex;

                if(pageSummary.TreeNode.TreeView == treeViewPagePicker) {
                    treeViewPagePicker.Sort();
                }
            }
        }

        /// <summary>
        /// Synchronises the tree view to match the list of child pages for the page summary.
        /// </summary>
        /// <param name="pageSummary"></param>
        public void SynchroniseTreeViewToChildPages(PageSummary pageSummary)
        {
            if(pageSummary != null && pageSummary.TreeNode != null) {
                treeViewPagePicker.BeginUpdate();
                try {
                    var treePageSummaries = pageSummary.TreeNode.Nodes.OfType<TreeNode>().Select(r => r.Tag).OfType<PageSummary>().Where(r => r != null).ToArray();
                    var realPageSummaries = pageSummary.ChildPages.ToArray();

                    foreach(var deletedSummary in treePageSummaries.Except(realPageSummaries).ToArray()) {
                        RemovePageSummary(deletedSummary, false);
                    }

                    foreach(var newSummary in realPageSummaries.Except(treePageSummaries).ToArray()) {
                        AddPageSummary(newSummary, pageSummary);
                    }

                    if(realPageSummaries.Length != pageSummary.TreeNode.Nodes.Count) {
                        throw new InvalidOperationException(String.Format("Assertion failed - there are {0} child pages and {1} tree nodes", realPageSummaries.Length, pageSummary.TreeNode.Nodes.Count));
                    }

                    treeViewPagePicker.Sort();
                } finally {
                    treeViewPagePicker.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Returns the page summary associated with the tree node or null if no such summary exists.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        private PageSummary FindPageSummaryForTreeNode(TreeNode treeNode)
        {
            var result = treeNode == null ? null : treeNode.Tag as PageSummary;

            return result;
        }
        #endregion

        #region PopulateTextToSpeechVoices, ShowTestConnectionResults
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="voiceNames"></param>
        public void PopulateTextToSpeechVoices(IEnumerable<string> voiceNames)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        public void ShowTestConnectionResults(string message, string title)
        {
            MessageBox.Show(message, title);
        }
        #endregion

        #region Child object creation - CreateReceiver etc.
        /// <summary>
        /// Creates a new receiver.
        /// </summary>
        /// <returns></returns>
        internal Receiver CreateReceiver()
        {
            return _Presenter.CreateReceiver();
        }

        /// <summary>
        /// Creates a new receiver location.
        /// </summary>
        /// <returns></returns>
        internal ReceiverLocation CreateReceiverLocation()
        {
            return _Presenter.CreateReceiverLocation();
        }

        /// <summary>
        /// Creates a new merged feed.
        /// </summary>
        /// <returns></returns>
        internal MergedFeed CreateMergedFeed()
        {
            return _Presenter.CreateMergedFeed();
        }

        /// <summary>
        /// Creates a new rebroadcast server.
        /// </summary>
        /// <returns></returns>
        internal RebroadcastSettings CreateRebroadcastServer()
        {
            return _Presenter.CreateRebroadcastServer();
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <returns></returns>
        internal IUser CreateUser()
        {
            return _Presenter.CreateUser();
        }
        #endregion

        #region Source helpers - GetSerialPortNames, GetVoiceNames
        /// <summary>
        /// Returns a collection of serial port names.
        /// </summary>
        /// <returns></returns>
        public IList<string> GetSerialPortNames()
        {
            return _Presenter.GetSerialPortNames().ToList();
        }

        /// <summary>
        /// Returns a collection of voice names. A voice name of null indicates the presence of a default voice.
        /// </summary>
        /// <returns></returns>
        public IList<string> GetVoiceNames()
        {
            return _Presenter.GetVoiceNames().ToList();
        }
        #endregion

        #region Wizard helpers - ApplyReceiverConfigurationWizard
        /// <summary>
        /// Applies the answers from a receiver configuration wizard to a receiver.
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="receiver"></param>
        public void ApplyReceiverConfigurationWizard(IReceiverConfigurationWizardAnswers answers, Receiver receiver)
        {
            _Presenter.ApplyReceiverConfigurationWizard(answers, receiver);
        }
        #endregion

        #region Validation - ShowValidationResults, SetControlErrorAlignment
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="results"></param>
        public override void ShowValidationResults(ValidationResults results)
        {
            if(!results.IsPartialValidation) {
                errorProvider.ClearErrors();
                warningProvider.ClearErrors();
            } else {
                foreach(var pageSummary in GetAllPageSummaries()) {
                    foreach(var fieldChecked in results.PartialValidationFields) {
                        var fieldControl = pageSummary.GetControlForValidationField(fieldChecked.Record, fieldChecked.Field);
                        var warningControl = fieldControl;
                        var errorControl = fieldControl;
                        if(fieldControl != null) {
                            var validateDelegate = fieldControl as IValidateDelegate;
                            if(validateDelegate != null) {
                                warningControl = validateDelegate.GetValidationDisplayControl(warningProvider);
                                errorControl = validateDelegate.GetValidationDisplayControl(errorProvider);
                            }
                            if(warningControl != null) warningProvider.SetClearableError(warningControl, null);
                            if(errorControl != null)   errorProvider.SetClearableError(errorControl, null);
                        }
                    }
                }
            }

            // Normally here we'd set DialogResult to None if we had any errors. Unfortunately Mono's
            // implementation of DialogResult is broken, we can't use it. Instead we need to set a flag
            // that the OK button click handler can use to control whether the form closes or not.
            _LastValidationFailed = results.HasErrors;

            var showPageSummary = ShowValidationResultsAgainstControls(results);
            if(showPageSummary != null) DisplayPage(showPageSummary);
        }

        /// <summary>
        /// Displays the validation results passed across and returns the page that should be shown to
        /// view the first error.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private PageSummary ShowValidationResultsAgainstControls(ValidationResults results)
        {
            PageSummary result = null;

            var allSummaries = GetAllPageSummariesInTreeNodeOrder();
            foreach(var validationResult in results.Results) {
                PageSummary fieldPageSummary = null;
                Control fieldControl = null;

                foreach(var pageSummary in allSummaries) {
                    fieldControl = pageSummary.GetControlForValidationField(validationResult.Record, validationResult.Field);
                    if(fieldControl != null) {
                        fieldPageSummary = pageSummary;
                        break;
                    }
                }
                if(fieldPageSummary == null || fieldControl == null) {
// TODO: put this back :) Need to figure out a better way of doing validation first though...

                  //  throw new InvalidOperationException(String.Format("Cannot find a page and control for {0} on {1}. The validation {2} message was {3}", validationResult.Field, validationResult.Record, validationResult.IsWarning ? "warning" : "error", validationResult.Message));
                }

                var validateDelegate = fieldControl as IValidateDelegate;
                if(validateDelegate != null) {
                    fieldControl = validateDelegate.GetValidationDisplayControl(validationResult.IsWarning ? warningProvider : errorProvider);
                }

                if(fieldControl != null) {
                    if(validationResult.IsWarning) {
                        warningProvider.SetClearableError(fieldControl, validationResult.Message);
                    } else {
                        errorProvider.SetClearableError(fieldControl, validationResult.Message);
                        if(_IsSaving) {
                            if(result == null || allSummaries.IndexOf(result) > allSummaries.IndexOf(fieldPageSummary)) {
                                result = fieldPageSummary;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the alignment of error icons for a control.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="alignment"></param>
        public void SetControlErrorAlignment(Control control, ErrorIconAlignment alignment)
        {
            errorProvider.SetIconAlignment(control, alignment);
            warningProvider.SetIconAlignment(control, alignment);
        }
        #endregion

        #region Events subscribed - ConfigurationListener, Users
        /// <summary>
        /// Called when the configuration changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationListener_PropertyChanged(object sender, ConfigurationListenerEventArgs args)
        {
            foreach(var pageSummary in GetAllPageSummaries()) {
                pageSummary.ConfigurationChanged(args);
            }
        }

        /// <summary>
        /// Called when the users change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Users_ListChanged(object sender, ListChangedEventArgs args)
        {
            foreach(var pageSummary in GetAllPageSummaries()) {
                pageSummary.UsersChanged(_Users, args);
            }
        }
        #endregion

        #region Events subscribed - Page
        /// <summary>
        /// Called when a page's child collection is changed after it has been added to the tree view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void PageSummary_ChildPages_ListChanged(object sender, ListChangedEventArgs args)
        {
            var childPageSummaries = (NotifyList<PageSummary>)sender;
            var parentPageSummary = GetAllPageSummaries().FirstOrDefault(r => r.ChildPages == childPageSummaries);
            if(parentPageSummary != null) {
                SynchroniseTreeViewToChildPages(parentPageSummary);
            }
        }
        #endregion

        #region Events subscribed - TreeView
        private void treeViewPagePicker_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var pageSummary = FindPageSummaryForTreeNode(treeViewPagePicker.SelectedNode);
            DisplayPage(pageSummary);
        }
        #endregion

        #region Events subscribed - buttons
        private void buttonOK_Click(object sender, EventArgs e)
        {
            var isSaving = _IsSaving;
            _IsSaving = true;
            try {
                OnSaveClicked(e);
            } finally {
                _IsSaving = isSaving;
            }

            // We had to set DialogResult to None instead of OK to work around a problem with Mono, whereby the
            // setting of DialogResult immediately calls the FormClosing events and you can't stop the form from
            // closing by just setting DialogResult to None. As a workaround I've added _LastValidationFailed
            // - if that's set to false then we can set DialogResult to OK here.
            if(!_LastValidationFailed) DialogResult = DialogResult.OK;
        }
        #endregion

        #region Events subscribed - menu
        /// <summary>
        /// Called when the user clicks the menu to configure for FSX without a radio.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void justFlightSimulatorXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnFlightSimulatorXOnlyClicked(e);
        }
        #endregion
    }
}
