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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.View;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.WinForms.Binding;
using System.Collections;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.OptionPage
{
    /// <summary>
    /// The base for all option pages.
    /// </summary>
    public partial class Page : BaseUserControl
    {
        #region Private class - BinderTag
        class BinderTag<T>
        {
            public IBinder Binder;
            public T Tag;

            public BinderTag(IBinder binder, T tag)
            {
                Binder = binder;
                Tag = tag;
            }
        }
        #endregion

        #region Private class - InlineHelp
        class InlineHelp
        {
            public string Title;
            public string Text;
        }
        #endregion

        #region Fields
        /// <summary>
        /// All of the objects that are binding observables to controls.
        /// </summary>
        private List<IBinder> _Binders = new List<IBinder>();

        /// <summary>
        /// A list of binders to their associated validation field attribute.
        /// </summary>
        private List<BinderTag<ValidationFieldAttribute>> _ValidationFields;

        /// <summary>
        /// A list of binders and the inline help for the associated control.
        /// </summary>
        private List<BinderTag<InlineHelp>> _InlineHelpFields;

        /// <summary>
        /// A list of observables that have had their ValueChanged events hooked.
        /// </summary>
        private List<IObservable> _HookedObservableChangeds = new List<IObservable>();

        /// <summary>
        /// A list of controls that have had their Enter and Leave events hooked.
        /// </summary>
        private List<Control> _HookedInlineHelpControls = new List<Control>();

        /// <summary>
        /// A list of pages that have had their PagePropertyChanged events hooked.
        /// </summary>
        private List<Page> _HookedPagePropertyChangeds = new List<Page>();

        /// <summary>
        /// The observable whose value will be used as the page title.
        /// </summary>
        private IObservable _PageTitleObservable;

        /// <summary>
        /// The observable whose value will be used as the page enabled value.
        /// </summary>
        private IObservable _PageEnabledObservable;

        /// <summary>
        /// Set to true if the observables are being initialised from a PageObject.
        /// </summary>
        private bool _InitialisingObservables;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the title for the page.
        /// </summary>
        public virtual string PageTitle
        {
            get {
                var result = "";
                if(_PageTitleObservable == null) result = "Override PageTitle";
                else {
                    var title = _PageTitleObservable.GetValue();
                    result = title == null ? "" : title.ToString();
                }

                return result;
            }
        }
        
        /// <summary>
        /// Gets a value indicating that the page should be shown as enabled in the list of pages.
        /// </summary>
        public virtual bool PageEnabled
        {
            get {
                var result = true;
                if(_PageEnabledObservable != null) {
                    var enabled = _PageEnabledObservable.GetValue();
                    result = enabled == null ? true : Convert.ToBoolean(enabled);
                }

                return result;
            }
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

        /// <summary>
        /// Gets or sets the owning options view.
        /// </summary>
        public OptionsView OptionsView { get; set; }

        /// <summary>
        /// Gets a collection of every child page under this page.
        /// </summary>
        public List<Page> ChildPages { get; private set; }

        /// <summary>
        /// Gets or sets the tree node that represents this page in the owner view.
        /// </summary>
        public TreeNode TreeNode { get; set; }

        /// <summary>
        /// Gets a value indicating that the page is on display in the OptionsView.
        /// </summary>
        public bool IsInOptionView { get { return TreeNode != null; } }
        #endregion

        #region Events Exposed
        /// <summary>
        /// Raised whenever an observable changes value.
        /// </summary>
        public event EventHandler PropertyValueChanged;

        /// <summary>
        /// Raises <see cref="PropertyValueChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChangedValue(EventArgs args)
        {
            if(PropertyValueChanged != null) PropertyValueChanged(this, args);
        }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Page()
        {
            InitializeComponent();

            ChildPages = new List<Page>();
        }
        #endregion

        #region InitialisePage
        /// <summary>
        /// Performs the constructor initialisation. The derivee must call this in its
        /// constructor once the page is ready to initialise.
        /// </summary>
        protected virtual void InitialisePage()
        {
            CreateBindings();
            RecordPageTitleObservable();
            RecordPageEnabledObservable();
        }

        /// <summary>
        /// Records the observable property that has the PageTitle attribute.
        /// </summary>
        private void RecordPageTitleObservable()
        {
            foreach(var property in GetType().GetProperties()) {
                var titleAttribute = property.GetCustomAttributes(typeof(PageTitleAttribute), true).OfType<PageTitleAttribute>().FirstOrDefault();
                if(titleAttribute != null) {
                    _PageTitleObservable = property.GetValue(this, null) as IObservable;
                    break;
                }
            }
        }

        /// <summary>
        /// Returns the observable property that has the PageEnabled attribute.
        /// </summary>
        private void RecordPageEnabledObservable()
        {
            foreach(var property in GetType().GetProperties()) {
                var enabledAttribute = property.GetCustomAttributes(typeof(PageEnabledAttribute), true).OfType<PageEnabledAttribute>().FirstOrDefault();
                if(enabledAttribute != null) {
                    _PageEnabledObservable = property.GetValue(this, null) as IObservable;
                    break;
                }
            }
        }
        #endregion

        #region Binding - CreateBindings, BindProperty, BindListProperty, FindBinder***
        /// <summary>
        /// When overridden by the derivee this creates the bindings between the fields
        /// and the controls.
        /// </summary>
        protected virtual void CreateBindings()
        {
            ;
        }

        /// <summary>
        /// Creates an observable field, binds a control to it and returns the field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        /// <returns></returns>
        protected Observable<T> BindProperty<T>(Control control)
        {
            var result = new Observable<T>();
            AddBinder(result, control);

            return result;
        }

        /// <summary>
        /// Creates an observable list field, binds a control to it and returns the field.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="control"></param>
        /// <returns></returns>
        protected ObservableList<T> BindListProperty<T>(Control control)
        {
            var result = new ObservableList<T>();
            AddBinder(result, control);

            return result;
        }

        /// <summary>
        /// Applies common actions when binding a newly created observable to a control.
        /// </summary>
        /// <param name="observable"></param>
        /// <param name="control"></param>
        private void AddBinder(IObservable observable, Control control)
        {
            _Binders.Add(BindingFactory.CreateBinder(observable, control));
            HookObservableChanged(observable);
        }

        /// <summary>
        /// Finds the binder associated with the property passed across, if any.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected IBinder FindBinderForProperty(PropertyInfo property)
        {
            IBinder result = null;

            if(property != null && typeof(IObservable).IsAssignableFrom(property.PropertyType)) {
                var observable = property.GetValue(this, null);
                if(observable != null) result = FindBinderForObservable((IObservable)observable);
            }

            return result;
        }

        /// <summary>
        /// Finds the binder associated with the observable passed across, if any.
        /// </summary>
        /// <param name="observable"></param>
        /// <returns></returns>
        protected IBinder FindBinderForObservable(IObservable observable)
        {
            var result = _Binders.FirstOrDefault(r => r.Observable == observable);
            return result;
        }

        /// <summary>
        /// Finds the binder associated with the control passed across, if any.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        protected IBinder FindBinderForControl(Control control)
        {
            var result = _Binders.FirstOrDefault(r => r.Control == control);
            return result;
        }
        #endregion

        #region Event hookers
        /// <summary>
        /// Hooks an observable's ValueChanged event.
        /// </summary>
        /// <param name="observable"></param>
        private void HookObservableChanged(IObservable observable)
        {
            if(!_HookedObservableChangeds.Contains(observable)) {
                observable.Changed += Observable_Changed;
                _HookedObservableChangeds.Add(observable);
            }
        }

        /// <summary>
        /// Hooks a page's PropertyValueChanged event.
        /// </summary>
        /// <param name="page"></param>
        private void HookPropertyValueChanged(Page page)
        {
            if(!_HookedPagePropertyChangeds.Contains(page)) {
                page.PropertyValueChanged += Page_PropertyChangedValue;
                _HookedPagePropertyChangeds.Add(page);
            }
        }

        /// <summary>
        /// Unhooks a page's PropertyValueChanged event.
        /// </summary>
        /// <param name="page"></param>
        private void UnhookPropertyValueChanged(Page page)
        {
            if(_HookedPagePropertyChangeds.Contains(page)) {
                page.PropertyValueChanged -= Page_PropertyChangedValue;
                _HookedPagePropertyChangeds.Remove(page);
            }
        }
        #endregion

        #region Property helpers - GetAllObservableProperties
        /// <summary>
        /// Returns a collection of every property on the page that implements IObservable.
        /// </summary>
        /// <returns></returns>
        protected PropertyInfo[] GetAllObservableProperties()
        {
            var allProperties = GetType().GetProperties();
            var result = allProperties.Where(r => typeof(IObservable).IsAssignableFrom(r.PropertyType)).ToArray();

            return result;
        }
        #endregion

        #region Validation - GetControlForValidationField, BuildValidationFields
        /// <summary>
        /// Builds the validation field list.
        /// </summary>
        private void BuildValidationFields()
        {
            if(_ValidationFields == null) {
                _ValidationFields = new List<BinderTag<ValidationFieldAttribute>>();
                foreach(var property in GetAllObservableProperties()) {
                    var attribute = property.GetCustomAttributes(typeof(ValidationFieldAttribute), true).OfType<ValidationFieldAttribute>().FirstOrDefault();
                    if(attribute != null) {
                        var binder = FindBinderForProperty(property);
                        if(binder != null) {
                            _ValidationFields.Add(new BinderTag<ValidationFieldAttribute>(binder, attribute));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the control associated with the validation field and object passed across or null if no
        /// control claims ownership of this field.
        /// </summary>
        /// <param name="validationObject"></param>
        /// <param name="validationField"></param>
        /// <returns></returns>
        public Control GetControlForValidationField(object validationObject, ValidationField validationField)
        {
            Control result = null;

            if(validationObject == PageObject) {
                BuildValidationFields();
                result = _ValidationFields.Where(r => r.Tag.ValidationField == validationField).Select(r => r.Binder.Control).FirstOrDefault();
            }

            return result;
        }

        /// <summary>
        /// Returns the validation field attribute associated with a binder or null if there isn't one.
        /// </summary>
        /// <param name="binder"></param>
        /// <returns></returns>
        public ValidationFieldAttribute GetValidationFieldAttributeForBinder(IBinder binder)
        {
            ValidationFieldAttribute result = null;

            if(binder != null) {
                BuildValidationFields();
                var binderTag = _ValidationFields.FirstOrDefault(r => r.Binder == binder);
                result = binderTag == null ? null : binderTag.Tag;
            }

            return result;
        }
        #endregion

        #region InlineHelp - BuildInlineHelpFields
        /// <summary>
        /// Builds the inline help list.
        /// </summary>
        private void BuildInlineHelpFields()
        {
            if(_InlineHelpFields == null) {
                _InlineHelpFields = new List<BinderTag<InlineHelp>>();
                foreach(var property in GetAllObservableProperties()) {
                    var titleAttribute = property.GetCustomAttributes(typeof(LocalisedDisplayNameAttribute), true).OfType<LocalisedDisplayNameAttribute>().FirstOrDefault();
                    var textAttribute = titleAttribute == null ? null : property.GetCustomAttributes(typeof(LocalisedDescriptionAttribute), true).OfType<LocalisedDescriptionAttribute>().FirstOrDefault();
                    if(titleAttribute != null && textAttribute != null) {
                        var binder = FindBinderForProperty(property);
                        if(binder != null) {
                            var inlineHelp = new InlineHelp() {
                                Title = titleAttribute.DisplayName,
                                Text = textAttribute.Description,
                            };
                            _InlineHelpFields.Add(new BinderTag<InlineHelp>(binder, inlineHelp));
                        }
                    }
                }
            }
        }
        #endregion

        #region List handling - HandleListChanged
        /// <summary>
        /// Called when an observable list changes. Adds or removes sub-pages, where appropriate, to
        /// reflect the content of the list.
        /// </summary>
        /// <param name="observable"></param>
        private void HandleListChanged(IObservableList observable)
        {
            if(observable != null && ShowPagesForObservableList(observable)) {
                var list = (IList)observable.GetValue();

                // Add new pages
                foreach(var record in list) {
                    if(FindChildPageForRecord(record) == null) {
                        AddChildPageForRecord(observable, record);
                    }
                }

                // Remove dead pages
                foreach(var childPage in ChildPages.Where(r => !list.Contains(r.PageObject)).ToArray()) {
                    RemoveChildPage(childPage);
                }
            }
        }

        /// <summary>
        /// Finds all lists that contain the child page's record and refreshes their content in turn.
        /// </summary>
        /// <param name="childPage"></param>
        private void HandleListContentChanged(Page childPage)
        {
            if(childPage != null && childPage.PageObject != null) {
                foreach(var binder in _Binders) {
                    if(binder.Observable != null && typeof(IObservableList).IsAssignableFrom(binder.Observable.GetType())) {
                        var list = binder.Observable.GetValue() as IList;
                        if(list != null && list.Contains(childPage.PageObject)) {
                            binder.RefreshControl();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// When overridden by the derivee this determines whether an observable list has child
        /// pages created for it.
        /// </summary>
        /// <param name="observableList"></param>
        /// <returns></returns>
        protected virtual bool ShowPagesForObservableList(IObservableList observableList)
        {
            return true;
        }

        /// <summary>
        /// When overrridden by the derivee this creates a new page for a child record.
        /// </summary>
        /// <param name="observableList"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        protected virtual Page CreatePageForNewChildRecord(IObservableList observableList, object record)
        {
            return null;
        }

        /// <summary>
        /// When overridden by the derivee this fills the PageObject record with the content of the observables.
        /// </summary>
        protected virtual void CopyObservablesToRecord()
        {
            ;
        }

        /// <summary>
        /// Calls <see cref="CopyRecordToObservables"/> with a flag set that prevents calls to <see cref="CopyObservablesToRecord"/>
        /// while the observables are being initialised.
        /// </summary>
        public void RefreshPageFromPageObject()
        {
            var initialisingObservables = _InitialisingObservables;
            _InitialisingObservables = true;
            try {
                CopyRecordToObservables();
            } finally {
                _InitialisingObservables = initialisingObservables;
            }
        }

        /// <summary>
        /// When overridden by the derivee this sets the values of the observables to the content of the record.
        /// </summary>
        protected virtual void CopyRecordToObservables()
        {
            ;
        }

        /// <summary>
        /// Finds the child page that represents the record passed across.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private Page FindChildPageForRecord(object record)
        {
            var result = ChildPages.FirstOrDefault(r => r.PageObject == record);
            return result;
        }

        /// <summary>
        /// Creates a child page for a record.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="record"></param>
        private void AddChildPageForRecord(IObservableList list, object record)
        {
            var page = CreatePageForNewChildRecord(list, record);
            if(page != null) {
                page.PageObject = record;
                page.RefreshPageFromPageObject();
                HookPropertyValueChanged(page);

                ChildPages.Add(page);

                if(OptionsView != null && IsInOptionView) {
                    OptionsView.AddPage(page, this);
                }
            }
        }

        /// <summary>
        /// Removes a child page.
        /// </summary>
        /// <param name="childPage"></param>
        private void RemoveChildPage(Page childPage)
        {
            if(childPage != null && ChildPages.Contains(childPage)) {
                UnhookPropertyValueChanged(childPage);

                if(OptionsView != null && childPage.IsInOptionView) {
                    OptionsView.RemovePage(childPage, makeParentCurrent: true);
                }

                if(ChildPages.Contains(childPage)) {
                    ChildPages.Remove(childPage);
                }
            }
        }
        #endregion

        #region Child record creation helpers - GenerateUniqueId, GenerateUniqueName
        /// <summary>
        /// Creates a unique ID for a new record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nextUniqueId"></param>
        /// <param name="records"></param>
        /// <param name="getUniqueId"></param>
        /// <returns></returns>
        public static int GenerateUniqueId<T>(int nextUniqueId, IList<T> records, Func<T, int> getUniqueId)
        {
            var result = nextUniqueId;
            while(records.Any(r => getUniqueId(r) == result)) {
                ++result;
            }

            return result;
        }

        /// <summary>
        /// Creates a unique case-insensitive name for a new record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <param name="prefix"></param>
        /// <param name="alwaysAppendCounter"></param>
        /// <param name="getName"></param>
        /// <returns></returns>
        public static string GenerateUniqueName<T>(IList<T> records, string prefix, bool alwaysAppendCounter, Func<T, string> getName)
        {
            string result;
            int counter = 1;
            do {
                result = counter != 1 || alwaysAppendCounter ? String.Format("{0} {1}", prefix, counter) : prefix;
                ++counter;
            } while(records.Any(r => result.Equals(getName(r), StringComparison.OrdinalIgnoreCase)));

            return result;
        }
        #endregion

        #region OnLoad, InitialiseControls
        /// <summary>
        /// Called after the control has been loaded but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                Localise.Control(this);

                InitialiseControls();

                foreach(var binder in _Binders) {
                    binder.InitialiseControl();
                }
                var firstBinder = _Binders.FirstOrDefault();
                if(firstBinder != null) firstBinder.Control.Focus();

                BuildValidationFields();
                foreach(var binderTag in _ValidationFields) {
                    var control = binderTag.Binder.Control;
                    var observable = binderTag.Binder.Observable;
                    var attribute = binderTag.Tag;
                    OptionsView.SetControlErrorAlignment(control, attribute.IconAlignment);
                    
                    if(attribute.RaisesValueChanged) {
                        HookObservableChanged(observable);
                    }
                }

                BuildInlineHelpFields();
                foreach(var binderTag in _InlineHelpFields) {
                    var control = binderTag.Binder.Control;
                    var inlineHelp = binderTag.Tag;

                    if(!_HookedInlineHelpControls.Contains(control)) {
                        _HookedInlineHelpControls.Add(control);
                        control.Enter += Control_Enter;
                        control.Leave += Control_Leave;
                    }
                }
            }
        }

        /// <summary>
        /// When overridden by the derivee this initialises controls before they are
        /// bound to values.
        /// </summary>
        protected virtual void InitialiseControls()
        {
            ;
        }
        #endregion

        #region PageSelected
        /// <summary>
        /// Called whenever the page is selected.
        /// </summary>
        public virtual void PageSelected()
        {
        }
        #endregion

        #region Observable event handlers
        protected virtual void Observable_Changed(object sender, EventArgs args)
        {
            var binder = FindBinderForObservable((IObservable)sender);
            if(binder != null) {
                // If a list was changed then update the GUI to match
                HandleListChanged(binder.Observable as IObservableList);

                // If this has a page object then we need to copy values back to the object
                if(PageObject != null && !_InitialisingObservables) CopyObservablesToRecord();

                if(OptionsView != null) {
                    // If a page attribute has changed then tell the parent view
                    var titleChanged = _PageTitleObservable != null && sender == _PageTitleObservable;
                    var enabledChanged = _PageEnabledObservable != null && sender == _PageEnabledObservable;
                    if(titleChanged || enabledChanged) {
                        OptionsView.RefreshPageDescription(this);
                    }

                    // If the validation needs running then do so
                    var validationAttribute = GetValidationFieldAttributeForBinder(binder);
                    if(validationAttribute != null && validationAttribute.RaisesValueChanged) {
                        OptionsView.RaiseValueChanged(binder.Control, args);
                    }
                }

                OnPropertyChangedValue(args);
            }
        }
        #endregion

        #region Page event handlers
        /// <summary>
        /// Called when a property on another page is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Page_PropertyChangedValue(object sender, EventArgs args)
        {
            HandleListContentChanged(sender as Page);
        }
        #endregion

        #region Generic control event handlers
        protected virtual void Control_Enter(object sender, EventArgs args)
        {
            var binder = FindBinderForControl((Control)sender);
            var inlineHelp = binder == null ? null : _InlineHelpFields.FirstOrDefault(r => r.Binder == binder);
            if(inlineHelp != null) {
                OptionsView.InlineHelpTitle = inlineHelp.Tag.Title;
                OptionsView.InlineHelp = inlineHelp.Tag.Text;
            }
        }

        protected virtual void Control_Leave(object sender, EventArgs args)
        {
            var binder = FindBinderForControl((Control)sender);
            var inlineHelp = binder == null ? null : _InlineHelpFields.FirstOrDefault(r => r.Binder == binder);
            if(inlineHelp != null) {
                OptionsView.InlineHelpTitle = "";
                OptionsView.InlineHelp = "";
            }
        }
        #endregion
    }
}
