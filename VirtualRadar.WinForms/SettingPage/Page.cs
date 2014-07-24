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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// The base class for all setting pages.
    /// </summary>
    public partial class Page : BaseUserControl
    {
        #region Private class - InlineHelp
        /// <summary>
        /// Describes an article of inline help.
        /// </summary>
        protected class InlineHelp
        {
            /// <summary>
            /// Gets or sets the help text's title.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// Gets or sets the help text.
            /// </summary>
            public string Help { get; set; }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="title"></param>
            /// <param name="help"></param>
            public InlineHelp(string title, string help)
            {
                Title = title;
                Help = help;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// A map of validation fields to the controls that they represent.
        /// </summary>
        protected Dictionary<ValidationField, Control> _ValidationFieldControlMap = new Dictionary<ValidationField,Control>();

        /// <summary>
        /// A map of controls to their inline help.
        /// </summary>
        protected Dictionary<Control, InlineHelp> _InlineHelpMap = new Dictionary<Control,InlineHelp>();

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
        private IBindingList _ChildPagesList;

        /// <summary>
        /// Creates a child page for an object.
        /// </summary>
        private Func<Page> _CreateChildPageDelegate;
        #endregion

        #region Properties
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

        /// <summary>
        /// If true then the page is stretched vertically to fill the available space.
        /// </summary>
        public virtual bool PageUseFullHeight { get { return false; } }

        private SettingsView _SettingsView;
        /// <summary>
        /// Gets or sets the owning settings view.
        /// </summary>
        public SettingsView SettingsView
        {
            get { return _SettingsView; }
            set {
                if(_SettingsView != value) {
                    _SettingsView = value;
                    DoCreateBindings();
                }
            }
        }

        /// <summary>
        /// Gets a collection of every child page under this page.
        /// </summary>
        public BindingList<Page> ChildPages { get; private set; }

        /// <summary>
        /// Gets or sets the tree node that represents this page in the owner view.
        /// </summary>
        public TreeNode TreeNode { get; set; }

        /// <summary>
        /// Gets a value indicating that the page is on display in the <see cref="SettingsView"/>.
        /// </summary>
        public bool IsInSettingsView { get { return TreeNode != null; } }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Page()
        {
            ChildPages = new BindingList<Page>();

            InitializeComponent();
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called after the page has been loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Control(this);
            }
        }
        #endregion

        #region CreateBindings
        /// <summary>
        /// Calls CreateBindings if the conditions are right.
        /// </summary>
        private void DoCreateBindings()
        {
            if(SettingsView == null) {
                ClearBindings();
            } else {
                AssociateChildPages();
                InitialiseControls();
                CreateBindings();
                AssociateValidationFields();
                AssociateInlineHelp();
            }
        }

        /// <summary>
        /// Gives the derived class the opportunity to synchronise its child pages list with something (or
        /// just create them if they're not associated with anything).
        /// </summary>
        protected virtual void AssociateChildPages()
        {
            ;
        }

        /// <summary>
        /// Called before anything is bound to the controls - gives the derivee the opportunity
        /// to initialise controls that need to be initialised.
        /// </summary>
        protected virtual void InitialiseControls()
        {
            ;
        }

        /// <summary>
        /// When overridden by the derivee this creates bindings between the page and objects
        /// on the owning <see cref="SettingsView"/>.
        /// </summary>
        protected virtual void CreateBindings()
        {
            ;
        }

        /// <summary>
        /// Removes all bindings from the page.
        /// </summary>
        protected virtual void ClearBindings()
        {
            foreach(var bindingControl in GetAllChildControls(includeThis: true).Where(r => r.DataBindings.Count > 0)) {
                bindingControl.DataBindings.Clear();
            }
        }

        /// <summary>
        /// Associates controls with validation fields.
        /// </summary>
        protected virtual void AssociateValidationFields()
        {
            ;
        }

        /// <summary>
        /// Associates controls with inline help.
        /// </summary>
        protected virtual void AssociateInlineHelp()
        {
            ;
        }
        #endregion

        #region CreateSortingBindingSource, CreateSortingEnumSource
        /// <summary>
        /// Creates a binding source that automatically sorts the list that it's attached to.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sortColumn"></param>
        /// <returns></returns>
        protected BindingSource CreateSortingBindingSource<T>(IList<T> list, Expression<Func<T, object>> sortColumn)
        {
            if(list == null) throw new ArgumentNullException("list");

            var castList = list as IList;
            if(castList == null) throw new ArgumentException("The list must be castable to IList");

            var bindingListView = new Equin.ApplicationFramework.BindingListView<T>(castList);
            var result = new BindingSource();
            result.DataSource = bindingListView;
            result.Sort = PropertyHelper.ExtractName<T>(sortColumn);

            return result;
        }

        /// <summary>
        /// Creates a simple unsorted binding source on a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        protected BindingSource CreateListBindingSource<T>(IList<T> list)
        {
            var result = new BindingSource();
            result.DataSource = list;

            return result;
        }

        /// <summary>
        /// Creates a binding source of enum values sorted by their description.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="describeEnumValue"></param>
        /// <returns></returns>
        protected BindingSource CreateSortingEnumSource<T>(Func<T, string> describeEnumValue)
        {
            var list = NameValue<T>.EnumList(describeEnumValue).OrderBy(r => r.Name).ToArray();
            var result = new BindingSource();
            result.DataSource = list;

            return result;
        }

        /// <summary>
        /// Creates a binding source of NameValue&lt;T&gt; values. These can be bound to Name and Value
        /// properties in ComboBoxes and are preferrable over CreateListBindingSource when the content
        /// of the list may change depending on the environment - if the current value is not present
        /// in the list it shows an empty combo box instead of the first value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="describeValue"></param>
        /// <param name="filterValue"></param>
        /// <param name="sortList"></param>
        /// <returns></returns>
        protected BindingSource CreateNameValueSource<T>(IEnumerable<T> list, Func<T, string> describeValue = null, Func<T, bool> filterValue = null, bool sortList = true)
        {
            var nameValueList = NameValue<T>.CreateList(list, describeValue, filterValue, sortList);
            var result = new BindingSource();
            result.DataSource = nameValueList;

            return result;
        }
        #endregion

        #region SetValidationFields, GetControlForValidationField
        /// <summary>
        /// Records which controls display which validation fields.
        /// </summary>
        /// <param name="fields"></param>
        protected void SetValidationFields(Dictionary<ValidationField, Control> fields)
        {
            foreach(var field in fields) {
                var validationField = field.Key;
                var control = field.Value;

                SettingsView.SetControlErrorAlignment(control, ErrorIconAlignment.MiddleLeft);

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
        /// <returns></returns>
        public Control GetControlForValidationField(object pageObject, ValidationField validationField)
        {
            Control result = null;

            if(pageObject == PageObject) {
                _ValidationFieldControlMap.TryGetValue(validationField, out result);
            }

            return result;
        }
        #endregion

        #region SetInlineHelp
        /// <summary>
        /// Sets the inline help up for the control passed across.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="title"></param>
        /// <param name="help"></param>
        protected virtual void SetInlineHelp(Control control, string title, string help)
        {
            if(_InlineHelpMap.ContainsKey(control)) {
                control.Enter -= Control_InlineHelp_Enter;
                _InlineHelpMap.Remove(control);
            }

            _InlineHelpMap.Add(control, new InlineHelp(title, help));
            control.Enter += Control_InlineHelp_Enter;
            control.Leave += Control_InlineHelp_Leave;
        }

        /// <summary>
        /// Displays the inline help associated with the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_InlineHelp_Enter(object sender, EventArgs args)
        {
            InlineHelp help;
            if(_InlineHelpMap.TryGetValue((Control)sender, out help)) {
                if(SettingsView != null) {
                    SettingsView.InlineHelpTitle = help.Title;
                    SettingsView.InlineHelp = help.Help;
                }
            }
        }

        /// <summary>
        /// Removes the inline help associated with the control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Control_InlineHelp_Leave(object sender, EventArgs args)
        {
            if(_InlineHelpMap.ContainsKey((Control)sender)) {
                if(SettingsView != null) {
                    SettingsView.InlineHelpTitle = "";
                    SettingsView.InlineHelp = "";
                }
            }
        }
        #endregion

        #region SetPageTitleProperty, SetPageEnabledProperty
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

            if(SettingsView != null) SettingsView.RefreshPageTreeNode(this);
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

            if(SettingsView != null) SettingsView.RefreshPageTreeNode(this);
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
                SettingsView.RefreshPageTreeNode(this);
            }
        }
        #endregion

        #region Child object to page handling
        /// <summary>
        /// Hooks up an observable collection with code that creates child pages for that collection's contents.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="createPageForChild"></param>
        internal void AssociateListWithChildPages<T>(BindingList<T> list, Func<Page> createPageForChild)
        {
            _ChildPagesList = list;
            _CreateChildPageDelegate = createPageForChild;
            _ChildPagesList.ListChanged += ChildPagesList_ListChanged;

            SynchroniseListToChildPages();
        }

        /// <summary>
        /// Synchronises an observable list's content to the child pages collection.
        /// </summary>
        private void SynchroniseListToChildPages()
        {
            if(_ChildPagesList != null && _CreateChildPageDelegate != null) {
                // Delete old pages
                foreach(var childPage in ChildPages.ToArray()) {
                    if(!((IList)_ChildPagesList).OfType<object>().Any(r => r == childPage.PageObject)) {
                        ChildPages.Remove(childPage);
                    }
                }

                // Add new pages
                foreach(var record in (IList)_ChildPagesList) {
                    if(!ChildPages.Any(r => r.PageObject == record)) {
                        var newChildPage = _CreateChildPageDelegate();
                        newChildPage.PageObject = record;
                        ChildPages.Add(newChildPage);
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
        #endregion

        #region PageSelected, ConfigurationChanged, PageDetached
        /// <summary>
        /// Called after the page is selected.
        /// </summary>
        internal virtual void PageSelected()
        {
            ;
        }

        /// <summary>
        /// Called when a configuration property somewhere has changed.
        /// </summary>
        /// <param name="args"></param>
        internal virtual void ConfigurationChanged(ConfigurationListenerEventArgs args)
        {
            RefreshTreeNodeForRecordProperty(args.Record, args.PropertyName);
        }

        /// <summary>
        /// Called when the users list, or any user in the list, changes.
        /// </summary>
        /// <param name="users"></param>
        /// <param name="args"></param>
        internal virtual void UsersChanged(IList<IUser> users, ListChangedEventArgs args)
        {
            if(args.ListChangedType == ListChangedType.ItemChanged) {
                RefreshTreeNodeForRecordProperty(users[args.NewIndex], args.PropertyDescriptor.Name);
            }
        }

        /// <summary>
        /// Called when the page is about to be removed from display while the parent form is still active.
        /// </summary>
        internal virtual void PageDetaching()
        {
            ;
        }

        /// <summary>
        /// Called when the page has been removed from display while the parent form is still active.
        /// </summary>
        internal virtual void PageDetached()
        {
            if(_ChildPagesList != null) {
                _ChildPagesList.ListChanged -= ChildPagesList_ListChanged;
            }
        }
        #endregion
    }
}
