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

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="ISettingsView"/>.
    /// </summary>
    public partial class SettingsView : BaseForm, ISettingsView, INotifyPropertyChanged
    {
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
        /// The object that is listening to the configuration for changes.
        /// </summary>
        private IConfigurationListener _ConfigurationListener;

        /// <summary>
        /// A list of all pages that have had their events hooked.
        /// </summary>
        private List<Page> _HookedPages = new List<Page>();

        /// <summary>
        /// True if a save event handler is running, false otherwise.
        /// </summary>
        private bool _IsSaving;

        // All of the top-level pages in the order in which they are shown to the user.
        private Page[] _TopLevelPages = new Page[] {
            new PageDataSources(),
            new PageReceivers(),
            new PageReceiverLocations(),
            new PageMergedFeeds(),
            new PageRebroadcastServers(),
            new PageUsers(),
            new PageRawFeedDecoding(),
            new PageWebServer(),
            new PageWebSite(),
            new PageGeneral(),
        };
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the currently selected page in the tree view.
        /// </summary>
        public Page CurrentTreePage
        {
            get { return treeViewPagePicker.SelectedNode == null ? null : treeViewPagePicker.SelectedNode.Tag as Page; }
            set {
                if(value == null) treeViewPagePicker.SelectedNode = null;
                else              treeViewPagePicker.SelectedNode = value.TreeNode;
            }
        }

        /// <summary>
        /// Gets or sets the currently selected page in the panel.
        /// </summary>
        public Page CurrentPanelPage
        {
            get { return panelPageContent.Tag as Page; }
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

        private BindingList<IUser> _Users = new BindingList<IUser>();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public BindingList<IUser> Users { get { return _Users; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string UserManager { get; set; }

        private BindingList<CombinedFeed> _CombinedFeed = new BindingList<CombinedFeed>();
        /// <summary>
        /// Gets a combined collection of receivers and merged feeds.
        /// </summary>
        public BindingList<CombinedFeed> CombinedFeed
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
                Localise.Form(this);
                treeViewPagePicker.ImageList = _ImageList.ImageList;

                InlineHelp = "";
                InlineHelpTitle = "";

                _Presenter = Factory.Singleton.Resolve<ISettingsPresenter>();
                _Presenter.Initialise(this);

                InitialiseCombinedFeed();

                foreach(var page in _TopLevelPages) {
                    AddPage(page, null);
                }
                treeViewPagePicker.ExpandAll();

                DisplayPage(_TopLevelPages.First());

                _Presenter.ValidateView();
            }
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
        /// Adds a page to the tree-view and to the owner's ChildPages collection. If the
        /// owner is null then the page is added to the top-level of the pages display.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="owner"></param>
        public void AddPage(Page page, Page owner)
        {
            var parentNodes = owner == null ? treeViewPagePicker.Nodes : owner.TreeNode.Nodes;

            if(page.TreeNode == null) {
                page.TreeNode = CreatePageTreeNode(page);
                RefreshPageTreeNode(page);
                parentNodes.Add(page.TreeNode);
            }

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

            if(page.SettingsView == null) {
                page.SettingsView = this;
            }

            foreach(var subPage in page.ChildPages) {
                AddPage(subPage, page);
            }

            if(!_HookedPages.Contains(page)) {
                page.ChildPages.ListChanged += Page_ChildPages_ListChanged;
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
                page.PageDetaching();
                
                if(_HookedPages.Contains(page)) {
                    page.ChildPages.ListChanged -= Page_ChildPages_ListChanged;
                }

                foreach(var subPage in page.ChildPages) {
                    RemovePage(subPage, makeParentCurrent);
                }

                if(page.SettingsView != null) {
                    page.SettingsView = null;
                }

                if(page.TreeNode != null) {
                    page.TreeNode.Parent.Nodes.Remove(page.TreeNode);
                    page.TreeNode = null;
                }

                if(panelPageContent.Controls.Contains(page)) {
                    panelPageContent.Controls.Remove(page);
                }

                var allPages = GetAllPagesInTreeNodeOrder();
                var pageIndex = allPages.IndexOf(page);
                if(pageIndex != -1) {
                    allPages.RemoveAt(pageIndex);

                    var parentPage = FindParentPage(page);
                    if(CurrentTreePage == page) {
                        CurrentTreePage = null;
                        if(makeParentCurrent && parentPage != null) {
                            DisplayPage(parentPage);
                        } else {
                            if(pageIndex < allPages.Count) {
                                DisplayPage(allPages[pageIndex]);
                            } else if(allPages.Count > 0) {
                                DisplayPage(allPages[allPages.Count - 1]);
                            } else if(parentPage != null) {
                                DisplayPage(parentPage);
                            }
                        }
                    }
                }

                page.PageDetached();
            }
        }

        /// <summary>
        /// Displays the page passed across.
        /// </summary>
        /// <param name="page"></param>
        public void DisplayPage(Page page)
        {
            if(page != null && page != CurrentPanelPage && GetAllPages().Contains(page)) {
                if(CurrentPanelPage != null) CurrentPanelPage.Visible = false;

                panelPageContent.Tag = page;
                CurrentPanelPage.Visible = true;

                if(CurrentTreePage != page) {
                    CurrentTreePage = page;
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

        #region TreeView handling
        /// <summary>
        /// Creates a TreeNode for the page passed across.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private TreeNode CreatePageTreeNode(Page page)
        {
            if(page.TreeNode == null) {
                page.TreeNode = new TreeNode();
                RefreshPageTreeNode(page);
            }

            return page.TreeNode;
        }

        /// <summary>
        /// Refreshes the treeview description for a page.
        /// </summary>
        /// <param name="page"></param>
        public void RefreshPageTreeNode(Page page)
        {
            if(page.TreeNode != null) {
                var title = page.PageTitle ?? "";
                var icon = page.PageIcon ?? Images.Transparent_16x16;
                var iconIndex = _ImageList.AddImage(icon);
                var colour = page.PageEnabled ? treeViewPagePicker.ForeColor : SystemColors.GrayText;

                if(page.TreeNode.Tag != page) page.TreeNode.Tag = page;
                if(page.TreeNode.Text != title) page.TreeNode.Text = title;
                if(page.TreeNode.ForeColor != colour) page.TreeNode.ForeColor = colour;
                if(page.TreeNode.ImageIndex != iconIndex) page.TreeNode.ImageIndex = iconIndex;
                if(page.TreeNode.SelectedImageIndex != iconIndex) page.TreeNode.SelectedImageIndex = iconIndex;
            }
        }

        /// <summary>
        /// Synchronises the tree view to match the list of child pages for the page.
        /// </summary>
        /// <param name="page"></param>
        public void SynchroniseTreeViewToChildPages(Page page)
        {
            if(page != null && page.TreeNode != null) {
                treeViewPagePicker.BeginUpdate();
                try {
                    var treePages = page.TreeNode.Nodes.OfType<TreeNode>().Select(r => r.Tag).OfType<Page>().Where(r => r != null).ToArray();
                    var realPages = page.ChildPages.ToArray();

                    foreach(var deletedPage in treePages.Except(realPages).ToArray()) {
                        RemovePage(deletedPage, false);
                    }

                    foreach(var newPage in realPages.Except(treePages).ToArray()) {
                        AddPage(newPage, page);
                    }

                    if(realPages.Length != page.TreeNode.Nodes.Count) {
                        throw new InvalidOperationException(String.Format("Assertion failed - there are {0} child pages and {1} tree nodes", realPages.Length, page.TreeNode.Nodes.Count));
                    }

                    for(var i = 0;i < realPages.Length;++i) {
                        var childPage = realPages[i];
                        if(childPage.TreeNode != page.TreeNode.Nodes[i]) {
                            var moveNode = page.TreeNode.Nodes.OfType<TreeNode>().Single(r => r.Tag == childPage);
                            moveNode.Remove();
                            page.TreeNode.Nodes.Insert(i, moveNode);
                        }
                    }
                } finally {
                    treeViewPagePicker.EndUpdate();
                }
            }
        }

        /// <summary>
        /// Returns the page associated with the tree node or null if no such page exists.
        /// </summary>
        /// <param name="treeNode"></param>
        /// <returns></returns>
        private Page FindPageForTreeNode(TreeNode treeNode)
        {
            Page result = treeNode == null ? null : treeNode.Tag as Page;

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
        public IEnumerable<string> GetSerialPortNames()
        {
            return _Presenter.GetSerialPortNames();
        }

        /// <summary>
        /// Returns a collection of voice names. A voice name of null indicates the presence of a default voice.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetVoiceNames()
        {
            return _Presenter.GetVoiceNames();
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
        public override void ShowValidationResults(IEnumerable<ValidationResult> results)
        {
            errorProvider.ClearErrors();
            warningProvider.ClearErrors();

            var hasWarnings = results.Any(r => r.IsWarning);
            var hasErrors = results.Any(r => !r.IsWarning);

            Page showPage = ShowValidationResultsAgainstControls(results);

            if(hasErrors) {
                DialogResult = DialogResult.None;
            }

            if(showPage != null) DisplayPage(showPage);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="validationField"></param>
        /// <param name="results"></param>
        public override void ShowSingleFieldValidationResults(object record, ValidationField validationField, IEnumerable<ValidationResult> results)
        {
            var allPages = GetAllPagesInTreeNodeOrder();
            foreach(var page in allPages) {
                var fieldControl = page.GetControlForValidationField(record, validationField);
                var warningControl = fieldControl;
                var errorControl = fieldControl;
                if(fieldControl != null) {
                    var validateDelegate = fieldControl as IValidateDelegate;
                    if(validateDelegate != null) {
                        warningControl = validateDelegate.GetValidationDisplayControl(warningProvider);
                        errorControl = validateDelegate.GetValidationDisplayControl(errorProvider);
                    }
                }

                if(warningControl != null) warningProvider.SetClearableError(warningControl, null);
                if(errorControl != null)   errorProvider.SetClearableError(errorControl, null);
            }

            ShowValidationResultsAgainstControls(results);
        }

        /// <summary>
        /// Displays the validation results passed across and returns the page that should be shown to
        /// view the first error.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        private Page ShowValidationResultsAgainstControls(IEnumerable<ValidationResult> results)
        {
            Page result = null;

            var allPages = GetAllPagesInTreeNodeOrder();
            foreach(var validationResult in results) {
                Page fieldPage = null;
                Control fieldControl = null;

                foreach(var page in allPages) {
                    fieldControl = page.GetControlForValidationField(validationResult.Record, validationResult.Field);
                    if(fieldControl != null) {
                        fieldPage = page;
                        break;
                    }
                }
                if(fieldPage == null || fieldControl == null) {
                    throw new InvalidOperationException(String.Format("Cannot find a page and control for {0} on {1}. The validation {2} message was {3}", validationResult.Field, validationResult.Record, validationResult.IsWarning ? "warning" : "error", validationResult.Message));
                }

                var validateDelegate = fieldControl as IValidateDelegate;
                if(validateDelegate != null) {
                    fieldControl = validateDelegate.GetValidationDisplayControl(validationResult.IsWarning ? warningProvider : errorProvider);
                }

                if(validationResult.IsWarning) {
                    warningProvider.SetClearableError(fieldControl, validationResult.Message);
                } else {
                    errorProvider.SetClearableError(fieldControl, validationResult.Message);
                    if(_IsSaving) {
                        if(result == null || allPages.IndexOf(result) > allPages.IndexOf(fieldPage)) {
                            result = fieldPage;
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
            foreach(var page in GetAllPages()) {
                page.ConfigurationChanged(args);
            }
        }

        /// <summary>
        /// Called when the users change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Users_ListChanged(object sender, ListChangedEventArgs args)
        {
            foreach(var page in GetAllPages()) {
                page.UsersChanged(_Users, args);
            }
        }
        #endregion

        #region Events subscribed - Page
        /// <summary>
        /// Called when a page's child collection is changed after it has been added to the tree view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Page_ChildPages_ListChanged(object sender, ListChangedEventArgs args)
        {
            var childPages = (BindingList<Page>)sender;
            var parentPage = GetAllPages().FirstOrDefault(r => r.ChildPages == childPages);
            if(parentPage != null) {
                SynchroniseTreeViewToChildPages(parentPage);
            }
        }
        #endregion

        #region Events subscribed - TreeView
        private void treeViewPagePicker_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var page = FindPageForTreeNode(treeViewPagePicker.SelectedNode);
            DisplayPage(page);
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
        }
        #endregion

        #region Events subscribed - menu
        private void justFlightSimulatorXToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnFlightSimulatorXOnlyClicked(e);
        }
        #endregion
    }
}
