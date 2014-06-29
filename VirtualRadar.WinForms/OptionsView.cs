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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.OptionPage;
using VirtualRadar.Interface.Presenter;
using InterfaceFactory;
using VirtualRadar.WinForms.Binding;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The default WinForms implementation of <see cref="IOptionsView"/>.
    /// </summary>
    public partial class OptionsView : BaseForm, IOptionsView
    {
        #region Fields
        /// <summary>
        /// The presenter that handles the business logic for us.
        /// </summary>
        private IOptionsPresenter _Presenter;

        /// <summary>
        /// True if a save event handler is running, false otherwise.
        /// </summary>
        private bool _IsSaving;

        /// <summary>
        /// A list of every top-level page.
        /// </summary>
        private List<Page> _TopLevelPages = new List<Page>();

        /// <summary>
        /// The object that manages the tree view's image list for us.
        /// </summary>
        private DynamicImageList _ImageList = new DynamicImageList();
        #endregion

        #region Top-level Page Properties
        public PageDataSources          PageDataSources         { get; private set; }
        public PageReceivers            PageReceivers           { get; private set; }
        public PageReceiverLocations    PageReceiverLocations   { get; private set; }
        public PageMergedFeeds          PageMergedFeeds         { get; private set; }
        public PageRawFeedDecoding      PageRawFeedDecoding     { get; private set; }
        public PageUsers                PageUsers               { get; private set; }
        #endregion

        #region Options Properties

        #region Data Sources
        public string BaseStationDatabaseFileName
        {
            get { return PageDataSources.DatabaseFileName.Value; }
            set { PageDataSources.DatabaseFileName.Value = value; }
        }

        public string OperatorFlagsFolder
        {
            get { return PageDataSources.FlagsFolder.Value; }
            set { PageDataSources.FlagsFolder.Value = value; }
        }

        public string SilhouettesFolder
        {
            get { return PageDataSources.SilhouettesFolder.Value; }
            set { PageDataSources.SilhouettesFolder.Value = value; }
        }

        public string PicturesFolder
        {
            get { return PageDataSources.PicturesFolder.Value; }
            set { PageDataSources.PicturesFolder.Value = value; }
        }

        public bool SearchPictureSubFolders
        {
            get { return PageDataSources.SearchPictureSubFolders.Value; }
            set { PageDataSources.SearchPictureSubFolders.Value = value; }
        }
        #endregion

        #region Receivers
        public int WebSiteReceiverId
        {
            get { return PageReceivers.WebSiteReceiverId.Value; }
            set { PageReceivers.WebSiteReceiverId.Value = value; }
        }

        public int ClosestAircraftReceiverId
        {
            get { return PageReceivers.ClosestAircraftReceiverId.Value; }
            set { PageReceivers.ClosestAircraftReceiverId.Value = value; }
        }

        public int FlightSimulatorXReceiverId
        {
            get { return PageReceivers.FlightSimulatorXReceiverId.Value; }
            set { PageReceivers.FlightSimulatorXReceiverId.Value = value; }
        }

        public IList<Receiver> Receivers { get { return PageReceivers.Receivers.Value; } }
        #endregion

        #region ReceiverLocations
        public IList<ReceiverLocation> RawDecodingReceiverLocations { get { return PageReceiverLocations.ReceiverLocations.Value; } }
        #endregion

        #region MergedFeeds
        public IList<MergedFeed> MergedFeeds { get { return PageMergedFeeds.MergedFeeds.Value; } }
        #endregion

        #region Users
        public IList<IUser> Users { get { return PageUsers.Users.Value; } }
        #endregion

        #region RawFeedDecoding
        public int RawDecodingReceiverRange
        {
            get { return PageRawFeedDecoding.ReceiverRange.Value; }
            set { PageRawFeedDecoding.ReceiverRange.Value = value; }
        }

        public bool RawDecodingIgnoreMilitaryExtendedSquitter
        {
            get { return PageRawFeedDecoding.IgnoreMilitaryExtendedSquitter.Value; }
            set { PageRawFeedDecoding.IgnoreMilitaryExtendedSquitter.Value = value; }
        }

        public bool RawDecodingSuppressReceiverRangeCheck
        {
            get { return PageRawFeedDecoding.SuppressReceiverRangeCheck.Value; }
            set { PageRawFeedDecoding.SuppressReceiverRangeCheck.Value = value; }
        }

        public bool RawDecodingUseLocalDecodeForInitialPosition
        {
            get { return PageRawFeedDecoding.UseLocalDecodeForInitialPosition.Value; }
            set { PageRawFeedDecoding.UseLocalDecodeForInitialPosition.Value = value; }
        }

        public int RawDecodingAirborneGlobalPositionLimit
        {
            get { return PageRawFeedDecoding.AirborneGlobalPositionLimit.Value; }
            set { PageRawFeedDecoding.AirborneGlobalPositionLimit.Value = value; }
        }

        public int RawDecodingFastSurfaceGlobalPositionLimit
        {
            get { return PageRawFeedDecoding.FastSurfaceGlobalPositionLimit.Value; }
            set { PageRawFeedDecoding.FastSurfaceGlobalPositionLimit.Value = value; }
        }

        public int RawDecodingSlowSurfaceGlobalPositionLimit
        {
            get { return PageRawFeedDecoding.SlowSurfaceGlobalPositionLimit.Value; }
            set { PageRawFeedDecoding.SlowSurfaceGlobalPositionLimit.Value = value; }
        }

        public double RawDecodingAcceptableAirborneSpeed
        {
            get { return PageRawFeedDecoding.AcceptableAirborneSpeed.Value; }
            set { PageRawFeedDecoding.AcceptableAirborneSpeed.Value = value; }
        }

        public double RawDecodingAcceptableAirSurfaceTransitionSpeed
        {
            get { return PageRawFeedDecoding.AcceptableAirSurfaceTransitionSpeed.Value; }
            set { PageRawFeedDecoding.AcceptableAirSurfaceTransitionSpeed.Value = value; }
        }

        public double RawDecodingAcceptableSurfaceSpeed
        {
            get { return PageRawFeedDecoding.AcceptableSurfaceSpeed.Value; }
            set { PageRawFeedDecoding.AcceptableSurfaceSpeed.Value = value; }
        }

        public bool RawDecodingIgnoreCallsignsInBds20
        {
            get { return PageRawFeedDecoding.IgnoreCallsignsInBds20.Value; }
            set { PageRawFeedDecoding.IgnoreCallsignsInBds20.Value = value; }
        }

        public int AcceptIcaoInNonPICount
        {
            get { return PageRawFeedDecoding.AcceptIcaoInNonPICount.Value; }
            set { PageRawFeedDecoding.AcceptIcaoInNonPICount.Value = value; }
        }

        public int AcceptIcaoInNonPISeconds
        {
            get { return PageRawFeedDecoding.AcceptIcaoInNonPISeconds.Value; }
            set { PageRawFeedDecoding.AcceptIcaoInNonPISeconds.Value = value; }
        }

        public int AcceptIcaoInPI0Count
        {
            get { return PageRawFeedDecoding.AcceptIcaoInPI0Count.Value; }
            set { PageRawFeedDecoding.AcceptIcaoInPI0Count.Value = value; }
        }

        public int AcceptIcaoInPI0Seconds
        {
            get { return PageRawFeedDecoding.AcceptIcaoInPI0Seconds.Value; }
            set { PageRawFeedDecoding.AcceptIcaoInPI0Seconds.Value = value; }
        }
        #endregion

        public bool CheckForNewVersions { get; set; }

        public int CheckForNewVersionsPeriodDays { get; set; }

        public bool DownloadFlightRoutes { get; set; }

        public int DisplayTimeoutSeconds { get; set; }

        public int TrackingTimeoutSeconds { get; set; }

        public int ShortTrailLengthSeconds { get; set; }

        public bool MinimiseToSystemTray { get; set; }

        private List<RebroadcastSettings> _RebroadcastSettings = new List<RebroadcastSettings>();
        public IList<RebroadcastSettings> RebroadcastSettings
        {
            get { return _RebroadcastSettings; }
        }

        public bool WebServerUserMustAuthenticate { get; set; }

        private List<string> _WebServerUserIds = new List<string>();
        public IList<string> WebServerUserIds { get { return _WebServerUserIds; } }

        public bool EnableUPnpFeatures { get; set; }

        public bool IsOnlyVirtualRadarServerOnLan { get; set; }

        public bool AutoStartUPnp { get; set; }

        public int UPnpPort { get; set; }

        public double InitialGoogleMapLatitude { get; set; }

        public double InitialGoogleMapLongitude { get; set; }

        public string InitialGoogleMapType { get; set; }

        public int InitialGoogleMapZoom { get; set; }

        public int InitialGoogleMapRefreshSeconds { get; set; }

        public int MinimumGoogleMapRefreshSeconds { get; set; }

        public DistanceUnit InitialDistanceUnit { get; set; }

        public HeightUnit InitialHeightUnit { get; set; }

        public SpeedUnit InitialSpeedUnit { get; set; }

        public bool PreferIataAirportCodes { get; set; }

        public bool EnableBundling { get; set; }

        public bool EnableMinifying { get; set; }

        public bool EnableCompression { get; set; }

        public ProxyType ProxyType { get; set; }

        public bool InternetClientCanRunReports { get; set; }

        public bool InternetClientCanPlayAudio { get; set; }

        public bool InternetClientCanSeePictures { get; set; }

        public int InternetClientTimeoutMinutes { get; set; }

        public bool InternetClientCanSeeLabels { get; set; }

        public bool AllowInternetProximityGadgets { get; set; }

        public bool InternetClientCanSubmitRoutes { get; set; }

        public bool InternetClientCanShowPolarPlots { get; set; }

        public bool AudioEnabled { get; set; }

        public string TextToSpeechVoice { get; set; }

        public int TextToSpeechSpeed { get; set; }
        #endregion

        #region Form properties
        public string InlineHelpTitle
        {
            get { return labelInlineHelpTitle.Text; }
            set { labelInlineHelpTitle.Text = value; }
        }

        public string InlineHelp
        {
            get { return labelInlineHelp.Text; }
            set { labelInlineHelp.Text = value; }
        }

        private Page _CurrentPage;
        public Page CurrentPage
        {
            get { return _CurrentPage; }
            set { DisplayPage(value); }
        }

        /// <summary>
        /// Gets the highest unique ID for any receiver or merged feed as-at the point
        /// where configuration began. No new feed IDs may be lower than this value, we
        /// do not recycle feed IDs.
        /// </summary>
        public int HighestConfiguredFeedId { get; private set; }

        /// <summary>
        /// A combined list of receivers and merged feeds.
        /// </summary>
        public ObservableList<CombinedFeed> CombinedFeeds { get; private set; }
        #endregion

        #region Events exposed
        public event EventHandler ResetToDefaultsClicked;

        public event EventHandler SaveClicked;
        protected virtual void OnSaveClicked(EventArgs args)
        {
            if(SaveClicked != null) SaveClicked(this, args);
        }

        public event EventHandler<EventArgs<Receiver>> TestConnectionClicked;
        public void RaiseTestConnectionClicked(EventArgs<Receiver> args)
        {
            if(TestConnectionClicked != null) TestConnectionClicked(this, args);
        }

        public event EventHandler TestTextToSpeechSettingsClicked;

        public event EventHandler UpdateReceiverLocationsFromBaseStationDatabaseClicked;
        public void RaiseUpdateReceiverLocationsFromBaseStationDatabaseClicked(EventArgs args)
        {
            if(UpdateReceiverLocationsFromBaseStationDatabaseClicked != null) UpdateReceiverLocationsFromBaseStationDatabaseClicked(this, args);
        }

        public event EventHandler UseIcaoRawDecodingSettingsClicked;
        public void RaiseUseIcaoRawDecodingSettingsClicked(EventArgs args)
        {
            if(UseIcaoRawDecodingSettingsClicked != null) UseIcaoRawDecodingSettingsClicked(this, args);
        }

        public event EventHandler UseRecommendedRawDecodingSettingsClicked;
        public void RaiseUseRecommendedRawDecodingSettingsClicked(EventArgs args)
        {
            if(UseRecommendedRawDecodingSettingsClicked != null) UseRecommendedRawDecodingSettingsClicked(this, args);
        }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OptionsView()
        {
            PageDataSources = new PageDataSources();
            PageReceivers = new PageReceivers();
            PageReceiverLocations = new PageReceiverLocations();
            PageMergedFeeds = new PageMergedFeeds();
            PageRawFeedDecoding = new PageRawFeedDecoding();
            PageUsers = new PageUsers();

            InitializeComponent();

            CombinedFeeds = new ObservableList<CombinedFeed>();
        }
        #endregion

        #region Validation - ShowValidationResults
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="results"></param>
        public override void ShowValidationResults(IEnumerable<ValidationResult> results)
        {
            errorProvider.ClearErrors();
            warningProvider.ClearErrors();

            var hasWarnings = results.Any(r => r.IsWarning);
            var hasErrors = results.Any(r => !r.IsWarning);

            var allPages = GetAllPagesInTreeNodeOrder();
            Page showPage = null;
            foreach(var result in results) {
                Page fieldPage = null;
                Control fieldControl = null;

                foreach(var page in allPages) {
                    fieldControl = page.GetControlForValidationField(result.Record, result.Field);
                    if(fieldControl != null) {
                        fieldPage = page;
                        break;
                    }
                }
                if(fieldPage == null || fieldControl == null) throw new InvalidOperationException(String.Format("Cannot find a page and control for {0} on {1}. The validation {2} message was {3}", result.Field, result.Record, result.IsWarning ? "warning" : "error", result.Message));

                var validateDelegate = fieldControl as IValidateDelegate;
                if(validateDelegate != null) fieldControl = validateDelegate.GetValidationDisplayControl(result.IsWarning ? warningProvider : errorProvider);

                if(result.IsWarning) {
                    warningProvider.SetClearableError(fieldControl, result.Message);
                } else {
                    errorProvider.SetClearableError(fieldControl, result.Message);
                    if(_IsSaving) {
                        if(showPage == null || allPages.IndexOf(showPage) > allPages.IndexOf(fieldPage)) {
                            showPage = fieldPage;
                        }
                    }
                }
            }

            if(hasErrors) {
                DialogResult = DialogResult.None;
            }

            if(showPage != null) DisplayPage(showPage);
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

        #region IOptionsView methods - PopulateTextToSpeechVoices, MergeBaseStationDatabaseReceiverLocations, ShowTestConnectionResults
        public void PopulateTextToSpeechVoices(IEnumerable<string> voiceNames)
        {
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receiverLocations"></param>
        public void MergeBaseStationDatabaseReceiverLocations(IEnumerable<ReceiverLocation> receiverLocations)
        {
            var viewList = PageReceiverLocations.ReceiverLocations.Value;

            var updateList = viewList.Where(r => r.IsBaseStationLocation && receiverLocations.Any(i => i.Name == r.Name));
            var deleteList = viewList.Where(r => r.IsBaseStationLocation).Except(updateList);
            var insertList = receiverLocations.Where(r => !updateList.Any(i => i.Name == r.Name));

            foreach(var location in updateList) {
                var newLocation = receiverLocations.First(r => r.Name == location.Name);
                location.Latitude = newLocation.Latitude;
                location.Longitude = newLocation.Longitude;

                var page = FindPageForPageObject(location);
                if(page != null) page.RefreshPageFromPageObject();
            }

            foreach(var deleteLocation in deleteList) {
                viewList.Remove(deleteLocation);
            }

            foreach(var location in insertList) {
                var newLocation = new ReceiverLocation() {
                    Name = Page.GenerateUniqueName(viewList, location.Name, false, r => r.Name),
                    UniqueId = Page.GenerateUniqueId(1, viewList, r => r.UniqueId),
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    IsBaseStationLocation = true,
                };
                viewList.Add(newLocation);
            }
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

        #region Pages - AddPage, RemovePage, DisplayPage, GetAllPages
        /// <summary>
        /// Adds a top-level page.
        /// </summary>
        /// <param name="page"></param>
        public void AddPage(Page page)
        {
            AddPage(page, null);
        }

        /// <summary>
        /// Adds a page to the tree-view and to the owner's ChildPages collection. If the
        /// owner is null then the page is added to the top-level of the pages display.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="owner"></param>
        public void AddPage(Page page, Page owner)
        {
            var pages = owner == null ? _TopLevelPages : owner.ChildPages;
            var parentNodes = owner == null ? treeViewPagePicker.Nodes : owner.TreeNode.Nodes;

            page.OptionsView = this;
            if(!pages.Contains(page)) pages.Add(page);

            page.TreeNode = new TreeNode();
            RefreshPageDescription(page);
            parentNodes.Add(page.TreeNode);

            if(!panelPageContent.Controls.Contains(page)) {
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

            foreach(var subPage in page.ChildPages) {
                AddPage(subPage, page);
            }
        }

        /// <summary>
        /// Removes the page from display.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="makeParentCurrent"></param>
        public void RemovePage(Page page, bool makeParentCurrent)
        {
            if(page != null) {
                foreach(var subPage in page.ChildPages) {
                    RemovePage(subPage, makeParentCurrent);
                }

                if(page.TreeNode != null) {
                    page.TreeNode.Parent.Nodes.Remove(page.TreeNode);
                    page.TreeNode = null;
                }

                var parentPage = FindParentPage(page);
                var pages = parentPage != null ? parentPage.ChildPages : _TopLevelPages;
                if(pages.Contains(page)) {
                    pages.Remove(page);
                }

                if(panelPageContent.Controls.Contains(page)) {
                    panelPageContent.Controls.Remove(page);
                }

                var allPages = GetAllPagesInTreeNodeOrder();
                var pageIndex = allPages.IndexOf(page);
                if(pageIndex != -1) {
                    allPages.RemoveAt(pageIndex);

                    if(_CurrentPage == page) {
                        _CurrentPage = null;
                        if(makeParentCurrent && parentPage != null) {
                            DisplayPage(parentPage);
                        } else {
                            if(pageIndex < allPages.Count) {
                                DisplayPage(allPages[pageIndex]);
                            } else if(allPages.Count > 0) {
                                DisplayPage(allPages[allPages.Count - 1]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Displays the page passed across.
        /// </summary>
        /// <param name="page"></param>
        public void DisplayPage(Page page)
        {
            if(page != null && page != _CurrentPage && GetAllPages().Contains(page)) {
                if(_CurrentPage != null) _CurrentPage.Visible = false;
                _CurrentPage = page;
                _CurrentPage.Visible = true;

                if(treeViewPagePicker.SelectedNode != page.TreeNode) {
                    treeViewPagePicker.SelectedNode = page.TreeNode;
                }

                page.PageSelected();
            }
        }

        /// <summary>
        /// Displays the page for the page object passed across.
        /// </summary>
        /// <param name="pageObject"></param>
        public void DisplayPageForPageObject(object pageObject)
        {
            var page = FindPageForPageObject(pageObject);
            if(page != null) DisplayPage(page);
        }

        /// <summary>
        /// Returns a flattened collection of every page in a random order.
        /// </summary>
        /// <returns></returns>
        private List<Page> GetAllPages()
        {
            return _TopLevelPages.Concat(_TopLevelPages.SelectMany(r => r.ChildPages)).ToList();
        }

        /// <summary>
        /// Returns a flattened collection of page in the order of their tree node.
        /// </summary>
        /// <returns></returns>
        private List<Page> GetAllPagesInTreeNodeOrder()
        {
            var result = new List<Page>();
            var allPages = GetAllPages();

            AddPages(treeViewPagePicker.Nodes, allPages, result);

            return result;
        }

        private void AddPages(TreeNodeCollection treeNodes, List<Page> allPages, List<Page> sortedPages)
        {
            foreach(TreeNode treeNode in treeNodes) {
                var page = allPages.FirstOrDefault(r => r.TreeNode == treeNode);
                if(page != null) sortedPages.Add(page);
                AddPages(treeNode.Nodes, allPages, sortedPages);
            }
        }

        /// <summary>
        /// Finds the page that is the parent of the one passed across. Returns null if there is no
        /// parent page for it.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public Page FindParentPage(Page page)
        {
            Page result = null;

            if(page != null) {
                result = GetAllPages().FirstOrDefault(r => r.ChildPages.Contains(page));
            }

            return result;
        }

        /// <summary>
        /// Finds the page that contains the page object passed across. Returns null if no page could
        /// be found.
        /// </summary>
        /// <param name="pageObject"></param>
        /// <returns></returns>
        public Page FindPageForPageObject(object pageObject)
        {
            Page result = null;

            if(pageObject != null) {
                result = GetAllPages().FirstOrDefault(r => r.PageObject == pageObject);
            }

            return result;
        }
        #endregion

        #region TreeView - RefreshPageDescription, FindPageForNode
        /// <summary>
        /// Refreshes the description for a page.
        /// </summary>
        /// <param name="page"></param>
        public void RefreshPageDescription(Page page)
        {
            if(page.TreeNode != null) {
                var title = page.PageTitle ?? "";
                var icon = page.PageIcon ?? Images.Transparent_16x16;
                var iconIndex = _ImageList.AddImage(icon);
                var colour = page.PageEnabled ? treeViewPagePicker.ForeColor : SystemColors.GrayText;

                if(page.TreeNode.Text != title) page.TreeNode.Text = title;
                if(page.TreeNode.ForeColor != colour) page.TreeNode.ForeColor = colour;
                if(page.TreeNode.ImageIndex != iconIndex) page.TreeNode.ImageIndex = iconIndex;
                if(page.TreeNode.SelectedImageIndex != iconIndex) page.TreeNode.SelectedImageIndex = iconIndex;
            }
        }

        /// <summary>
        /// Returns the page associated with the tree node or null if no such page exists.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        private Page FindPageForNode(TreeNode treeNode)
        {
            Page result = null;

            if(treeNode != null) {
                var allPages = GetAllPages();
                result = allPages.FirstOrDefault(r => r.TreeNode == treeNode);
            }

            return result;
        }
        #endregion

        #region CombinedFeed - RefreshCombinedFeed
        /// <summary>
        /// Refreshes the content of <see cref="CombinedFeeds"/>.
        /// </summary>
        private void RefreshCombinedFeeds()
        {
            var newList = Receivers.Select(r => new CombinedFeed(r)).Concat(MergedFeeds.Select(r => new CombinedFeed(r)));
            CombinedFeeds.ReplaceContent(newList);
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called after the form has been loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                InlineHelp = InlineHelpTitle = "";

                Localise.Form(this);
                treeViewPagePicker.ImageList = _ImageList.ImageList;

                _Presenter = Factory.Singleton.Resolve<IOptionsPresenter>();
                _Presenter.Initialise(this);

                var maxReceiverId = Receivers.Count > 0 ? Receivers.Max(r => r.UniqueId) : 0;
                var maxMergedFeedId = MergedFeeds.Count > 0 ? MergedFeeds.Max(r => r.UniqueId) : 0;
                HighestConfiguredFeedId = Math.Max(maxReceiverId, maxMergedFeedId);

                RefreshCombinedFeeds();
                PageReceivers.Receivers.Changed += (s, a) => RefreshCombinedFeeds();
                PageMergedFeeds.MergedFeeds.Changed += (s, a) => RefreshCombinedFeeds();

                AddPage(PageDataSources);
                AddPage(PageReceivers);
                AddPage(PageReceiverLocations);
                AddPage(PageMergedFeeds);
                AddPage(PageUsers);
                AddPage(PageRawFeedDecoding);

                treeViewPagePicker.ExpandAll();
                var firstNode = treeViewPagePicker.Nodes[0];
                treeViewPagePicker.SelectedNode = firstNode;

                // Trigger a validation of the content now that we're all set up
                OnValueChanged(this, EventArgs.Empty);
            }
        }
        #endregion

        #region TreeView events subscribed
        private void treeViewPagePicker_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var page = FindPageForNode(treeViewPagePicker.SelectedNode);
            DisplayPage(page);
        }
        #endregion

        #region Button events subscribed
        private void buttonOK_Click(object sender, EventArgs e)
        {
            var isSaving = _IsSaving;
            _IsSaving = true;
            try {
                OnSaveClicked(e);
            } finally {
                _IsSaving = isSaving;
            }
        }
        #endregion
    }
}
