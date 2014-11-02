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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interface.PortableBinding;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// A class that exposes the summary information for a <see cref="Page"/>.
    /// </summary>
    /// <remarks><para>
    /// In beta testing of the new options screens it was found that the options screen
    /// for installations with a large number of feeds were taking a very long time to
    /// open. This was because each page is a user control, and all of the user controls
    /// had to be created before the screen was shown.
    /// </para><para>
    /// Having pages as user controls is quite attractive for a number of reasons, but a
    /// way had to be found to defer the creation of them until the user actually wants
    /// them. To this end we have this object. It carries the information that the options
    /// screen tree view needs from a page and it also carries the page itself. The page
    /// summaries are created by the options screen and associated with tree view nodes.
    /// When the user clicks a node, or causes a node to be selected, the code creates the
    /// page (if it doesn't already exist) and adds it to the options screen. The options
    /// screen opens faster but the drawback is that switching to a new page has some
    /// overhead while the page is created.
    /// </para></remarks>
    public abstract class PageSummary
    {
        #region Fields
        /// <summary>
        /// True once the summary has been initialised.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// The name of the property that is to be used for the page's title.
        /// </summary>
        private string _PageTitleProperty;

        /// <summary>
        /// A delegate that can fetch the page's title.
        /// </summary>
        private Func<string> _PageTitleFetcher;

        /// <summary>
        /// The name of the property that is used for the page's enabled state.
        /// </summary>
        private string _PageEnabledProperty;

        /// <summary>
        /// A delegate that can fetch the page's enabled state.
        /// </summary>
        private Func<bool> _PageEnabledFetcher;

        /// <summary>
        /// The list that is driving the content of the <see cref="ChildPages"/>, if any.
        /// </summary>
        private IBindingList _ChildPageSummariesList;

        /// <summary>
        /// Creates a child page for an object.
        /// </summary>
        private Func<PageSummary> _CreateChildPageSummaryDelegate;

        /// <summary>
        /// A map of validation fields to the controls that they represent.
        /// </summary>
        protected Dictionary<ValidationField, Control> _ValidationFieldControlMap = new Dictionary<ValidationField,Control>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the page associated with the summary.
        /// </summary>
        public Page Page { get; set; }

        /// <summary>
        /// Gets the title for the page.
        /// </summary>
        public virtual string PageTitle
        {
            get { return _PageTitleFetcher != null ? _PageTitleFetcher() : ""; }
        }
        
        /// <summary>
        /// Gets a value indicating that the page should be shown as enabled in the list of pages.
        /// </summary>
        public virtual bool PageEnabled
        {
            get { return _PageEnabledFetcher != null ? _PageEnabledFetcher() : true; }
        }

        /// <summary>
        /// Gets the icon for the page.
        /// </summary>
        public virtual Image PageIcon { get { return null; } }

        /// <summary>
        /// Gets the settings object being modified by this page.
        /// </summary>
        public virtual object PageObject { get; protected set; }

        private SettingsView _SettingsView;
        /// <summary>
        /// Gets or sets the view that owns this summary.
        /// </summary>
        public SettingsView SettingsView
        {
            get { return _SettingsView; }
            set {
                if(_SettingsView != value) {
                    _SettingsView = value;
                    Initialise();
                }
            }
        }

        /// <summary>
        /// Gets or sets the tree node that represents this summary.
        /// </summary>
        public TreeNode TreeNode { get; set; }

        /// <summary>
        /// Gets a value indicating that child pages are to be shown in alphabetical order of
        /// <see cref="PageTitle"/>. False by default unless the child pages are associated with a list.
        /// </summary>
        public virtual bool ShowChildPagesInAlphabeticalOrder
        {
            get { return _ChildPageSummariesList != null; }
        }

        /// <summary>
        /// Gets a list of all child summaries.
        /// </summary>
        public NotifyList<PageSummary> ChildPages { get; private set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PageSummary()
        {
            ChildPages = new NotifyList<PageSummary>();
        }
        #endregion

        #region CreatePage
        /// <summary>
        /// Creates the page for display.
        /// </summary>
        public void CreatePage()
        {
            if(Page == null) {
                Page = DoCreatePage();
                Page.PageSummary = this;
            }
        }

        /// <summary>
        /// Creates the page.
        /// </summary>
        /// <returns></returns>
        protected abstract Page DoCreatePage();
        #endregion

        #region Initialise
        /// <summary>
        /// Initialises the summary.
        /// </summary>
        private void Initialise()
        {
            if(!_Initialised && SettingsView != null) {
                _Initialised = true;
                AssociateValidationFields(Page);
                AssociateChildPages();
            }
        }
        #endregion

        #region Child object to page handling
        /// <summary>
        /// Gives the derived class the opportunity to synchronise its child pages list with something (or
        /// just create them if they're not associated with anything).
        /// </summary>
        protected virtual void AssociateChildPages()
        {
            ;
        }

        /// <summary>
        /// Hooks up an observable collection with code that creates child pages for that collection's contents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="createPageSummaryForChild"></param>
        internal void AssociateListWithChildPages<T>(NotifyList<T> list, Func<PageSummary> createPageSummaryForChild)
        {
            _ChildPageSummariesList = list;
            _CreateChildPageSummaryDelegate = createPageSummaryForChild;
            _ChildPageSummariesList.ListChanged += ChildPagesList_ListChanged;

            SynchroniseListToChildPages();
        }

        /// <summary>
        /// Synchronises an observable list's content to the child pages collection.
        /// </summary>
        private void SynchroniseListToChildPages()
        {
            if(_ChildPageSummariesList != null && _CreateChildPageSummaryDelegate != null) {
                // Delete old summaries
                foreach(var childPageSummary in ChildPages.ToArray()) {
                    if(!((IList)_ChildPageSummariesList).OfType<object>().Any(r => r == childPageSummary.PageObject)) {
                        ChildPages.Remove(childPageSummary);
                    }
                }

                // Add new summaries
                foreach(var record in (IList)_ChildPageSummariesList) {
                    if(!ChildPages.Any(r => r.PageObject == record)) {
                        var newChildPageSummary = _CreateChildPageSummaryDelegate();
                        newChildPageSummary.PageObject = record;
                        ChildPages.Add(newChildPageSummary);
                    }
                }
            }
        }

        /// <summary>
        /// Called whenever the observable list that is driving the creation of child pages is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ChildPagesList_ListChanged(object sender, ListChangedEventArgs args)
        {
            SynchroniseListToChildPages();
        }

        /// <summary>
        /// Returns true if the summary is exposing a record with the same properties as the record passed
        /// across. Note that the record will *NOT* have the same object reference as any records being
        /// edited by the pages, its properties need to be compared.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        internal virtual bool IsForSameRecord(object record)
        {
            return false;
        }
        #endregion

        #region Tree view handling - SetPageTitleProperty, SetPageEnabledProperty
        /// <summary>
        /// Sets up the automatic page title fetcher.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageTitleProperty">A LINQ expression that gives the name of the page title property. Set to null if the page title is a composite of many properties.</param>
        /// <param name="pageTitleFetcher">A function that returns the current value for the page title.</param>
        protected void SetPageTitleProperty<T>(Expression<Func<T, object>> pageTitleProperty, Func<string> pageTitleFetcher)
        {
            _PageTitleProperty = pageTitleProperty == null ? null : PropertyHelper.ExtractName<T>(pageTitleProperty);
            _PageTitleFetcher = pageTitleFetcher;

            if(SettingsView != null) SettingsView.RefreshPageSummaryTreeNode(this);
        }

        /// <summary>
        /// Sets up the automatic page enabled fetcher.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageEnabledProperty">A LINQ expression that gives the name of the page enabled property. Set to null if the page enabled state is not held by a single property.</param>
        /// <param name="pageEnabledFetcher">A function that returns the current value for the page enabled state.</param>
        protected void SetPageEnabledProperty<T>(Expression<Func<T, object>> pageEnabledProperty, Func<bool> pageEnabledFetcher)
        {
            _PageEnabledProperty = pageEnabledProperty == null ? null : PropertyHelper.ExtractName<T>(pageEnabledProperty);
            _PageEnabledFetcher = pageEnabledFetcher;

            if(SettingsView != null) SettingsView.RefreshPageSummaryTreeNode(this);
        }

        /// <summary>
        /// Refreshes the tree nodes for pages that have set up PageEnabled or PageTitle properties.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="propertyName"></param>
        private void RefreshTreeNodeForRecordProperty(object record, string propertyName)
        {
            var refreshPageTreeNode = false;

            if(record == PageObject && SettingsView != null) {
                if(_PageTitleFetcher != null) {
                    if(_PageTitleProperty == null || propertyName == _PageTitleProperty) {
                        refreshPageTreeNode = true;
                    }
                }
                if(_PageEnabledFetcher != null) {
                    if(_PageEnabledProperty == null || propertyName == _PageEnabledProperty) {
                        refreshPageTreeNode = true;
                    }
                }
            }

            if(refreshPageTreeNode) {
                SettingsView.RefreshPageSummaryTreeNode(this);
            }
        }
        #endregion

        #region AssociateValidationFields, SetValidationFields, GetControlForValidationField
        /// <summary>
        /// Associates controls with validation fields.
        /// </summary>
        /// <param name="genericPage"></param>
        public virtual void AssociateValidationFields(Page genericPage)
        {
        }

        /// <summary>
        /// Records which controls display which validation fields.
        /// </summary>
        /// <param name="fields"></param>
        public void SetValidationFields(Dictionary<ValidationField, Control> fields)
        {
            foreach(var field in fields) {
                var validationField = field.Key;
                var control = field.Value;

                if(control != null) {
                    SettingsView.SetControlErrorAlignment(control, ErrorIconAlignment.MiddleLeft);
                }

                if(!_ValidationFieldControlMap.ContainsKey(field.Key)) {
                    _ValidationFieldControlMap.Add(validationField, control);
                } else {
                    _ValidationFieldControlMap[validationField] = control;
                }
            }
        }

        /// <summary>
        /// Returns the control associated with the field passed across. Returns null if the pageObject passed
        /// across does not match the pageObject held by the page.
        /// </summary>
        /// <param name="pageObject"></param>
        /// <param name="validationField"></param>
        /// <param name="isClearMessage"></param>
        /// <returns></returns>
        public Control GetControlForValidationField(object pageObject, ValidationField validationField, bool isClearMessage)
        {
            Control result = null;

            if(pageObject == PageObject) {
                _ValidationFieldControlMap.TryGetValue(validationField, out result);

                if(result == null && !isClearMessage && _ValidationFieldControlMap.ContainsKey(validationField)) {
                    // The page hasn't been created and it has a validation error on it - create the page and try again
                    SettingsView.CreatePage(this);
                    _ValidationFieldControlMap.TryGetValue(validationField, out result);
                }
            }

            return result;
        }
        #endregion

        #region ConfigurationChanged, UsersChanged, PageDetaching, PageDetached
        /// <summary>
        /// Called when a configuration property somewhere has changed.
        /// </summary>
        /// <param name="args"></param>
        internal virtual void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
            RefreshTreeNodeForRecordProperty(args.Record, args.PropertyName);
            if(Page != null) Page.ConfigurationChanged(args);
        }

        /// <summary>
        /// Called when the users list, or any user in the list, changes.
        /// </summary>
        /// <param name="users"></param>
        /// <param name="args"></param>
        internal virtual void UsersChanged(IList<IUser> users, ListChangedEventArgs args)
        {
            if(args.PropertyDescriptor != null && args.ListChangedType == ListChangedType.ItemChanged) {
                RefreshTreeNodeForRecordProperty(users[args.NewIndex], args.PropertyDescriptor.Name);
            }

            if(Page != null) Page.UsersChanged(users, args);
        }

        /// <summary>
        /// Called when the page is about to be removed from display while the parent form is still active.
        /// </summary>
        internal virtual void PageDetaching()
        {
            if(Page != null) Page.PageDetaching();
        }

        /// <summary>
        /// Called when the page has been removed from display while the parent form is still active.
        /// </summary>
        internal virtual void PageDetached()
        {
            if(_ChildPageSummariesList != null) {
                _ChildPageSummariesList.ListChanged -= ChildPagesList_ListChanged;
            }

            if(Page != null) Page.PageDetached();
        }
        #endregion
    }
}
