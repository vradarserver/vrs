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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms.Controls;
using VirtualRadar.WinForms.PortableBinding;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The base for user controls.
    /// </summary>
    public class BaseUserControl : UserControl
    {
        #region Fields
        /// <summary>
        /// The object that hooks value changed events on controls.
        /// </summary>
        protected ValueChangedHelper _ValueChangedHelper;

        /// <summary>
        /// The object that encapsulates behaviour common to this base and the forms base.
        /// </summary>
        private CommonBaseBehaviour _CommonBaseBehaviour = new CommonBaseBehaviour();

        /// <summary>
        /// The object that handles the display of validation results for us. The derivee is responsible for creating this.
        /// </summary>
        protected ValidationHelper _ValidationHelper;
        #endregion

        #region Properties
        private MonoAutoScaleMode _MonoAutoScaleMode;
        /// <summary>
        /// Gets or sets the AutoScaleMode.
        /// </summary>
        /// <remarks>Works around Mono's weirdness over AutoScaleMode and anchoring / docking - see the comments against MonoAutoScaleMode.</remarks>
        [DefaultValue(AutoScaleMode.Font)]
        public new AutoScaleMode AutoScaleMode
        {
            get { return _MonoAutoScaleMode.AutoScaleMode; }
            set { _MonoAutoScaleMode.AutoScaleMode = value; }
        }
        #endregion

        #region Common events exposed
        /// <summary>
        /// Raised when a value changes.
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Raises <see cref="ValueChanged"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected virtual void OnValueChanged(object sender, EventArgs args)
        {
            if(ValueChanged != null) ValueChanged(sender, args);
        }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BaseUserControl() : base()
        {
            _MonoAutoScaleMode = new MonoAutoScaleMode(this);

            _ValueChangedHelper = new ValueChangedHelper((sender, args) => OnValueChanged(sender, args));
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            _CommonBaseBehaviour.DisposeControlBinders();
            base.Dispose(disposing);
        }
        #endregion

        #region GetAllChildControls
        /// <summary>
        /// Returns a recursive collection of every control on the user control. Can optionally include
        /// the user control itself as the first control.
        /// </summary>
        /// <param name="includeThis"></param>
        /// <returns></returns>
        public List<Control> GetAllChildControls(bool includeThis = false)
        {
            return _CommonBaseBehaviour.GetAllChildControls(this, includeThis);
        }
        #endregion

        #region Vanilla .NET data binding - AddBinding, Create****BindingSource
        /// <summary>
        /// A shorthand method for adding bindings with compiler-checked names.
        /// </summary>
        /// <typeparam name="TControl"></typeparam>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="modelProperty"></param>
        /// <param name="controlProperty"></param>
        /// <param name="dataSourceUpdateMode"></param>
        /// <param name="format"></param>
        /// <param name="parse"></param>
        public Binding AddBinding<TControl, TModel>(TModel model, TControl control, Expression<Func<TModel, object>> modelProperty, Expression<Func<TControl, object>> controlProperty, DataSourceUpdateMode dataSourceUpdateMode = DataSourceUpdateMode.OnValidation, ConvertEventHandler format = null, ConvertEventHandler parse = null)
            where TControl: Control
        {
            return _CommonBaseBehaviour.AddBinding<TControl, TModel>(model, control, modelProperty, controlProperty, dataSourceUpdateMode, format, parse);
        }

        /// <summary>
        /// Creates a binding source that automatically sorts the list that it's attached to.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sortColumn"></param>
        /// <returns></returns>
        protected BindingSource CreateSortingBindingSource<T>(IList<T> list, Expression<Func<T, object>> sortColumn)
        {
            return _CommonBaseBehaviour.CreateSortingBindingSource<T>(list, sortColumn);
        }

        /// <summary>
        /// Creates a simple unsorted binding source on a list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        protected BindingSource CreateListBindingSource<T>(IList<T> list)
        {
            return _CommonBaseBehaviour.CreateListBindingSource<T>(list);
        }

        /// <summary>
        /// Creates a binding source of enum values sorted by their description.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="describeEnumValue"></param>
        /// <returns></returns>
        protected BindingSource CreateSortingEnumSource<T>(Func<T, string> describeEnumValue)
        {
            return _CommonBaseBehaviour.CreateSortingEnumSource<T>(describeEnumValue);
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
            return _CommonBaseBehaviour.CreateNameValueSource<T>(list, describeValue, filterValue, sortList);
        }
        #endregion

        #region Portable binding - AddControlBinder, GetControlBinders
        /// <summary>
        /// Adds a control binder.
        /// </summary>
        /// <param name="binder"></param>
        protected void AddControlBinder(ControlBinder binder)
        {
            _CommonBaseBehaviour.AddControlBinder(binder);
        }

        /// <summary>
        /// Returns an array of all control binders.
        /// </summary>
        /// <returns></returns>
        protected ControlBinder[] GetControlBinders()
        {
            return _CommonBaseBehaviour.GetControlBinders();
        }

        /// <summary>
        /// Initialises the registered control binders.
        /// </summary>
        protected void InitialiseControlBinders()
        {
            _CommonBaseBehaviour.InitialiseControlBinders();
        }
        #endregion

        #region GetSelectedListViewTag, GetAllSelectedListViewTag, SelectListViewItemByTag, SelectListViewItemsByTags
        /// <summary>
        /// Gets the tag associated with the selected row in a list view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <returns></returns>
        protected T GetSelectedListViewTag<T>(ListView listView)
            where T: class
        {
            return _CommonBaseBehaviour.GetSelectedListViewTag<T>(listView);
        }

        /// <summary>
        /// Gets the tags associated with all selected items in a multi-select list view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <returns></returns>
        protected T[] GetAllSelectedListViewTag<T>(ListView listView)
            where T: class
        {
            return _CommonBaseBehaviour.GetAllSelectedListViewTag<T>(listView);
        }

        /// <summary>
        /// Gets the tags associated with all checked items in a list view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <returns></returns>
        protected T[] GetAllCheckedListViewTag<T>(ListView listView)
            where T: class
        {
            return _CommonBaseBehaviour.GetAllCheckedListViewTag<T>(listView);
        }

        /// <summary>
        /// Selects the list view item associated with the tag value passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="value"></param>
        protected void SelectListViewItemByTag<T>(ListView listView, T value)
            where T: class
        {
            _CommonBaseBehaviour.SelectListViewItemByTag<T>(listView, value);
        }

        /// <summary>
        /// Selects many list view items associated with the tag values passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="values"></param>
        /// <param name="ensureVisible"></param>
        protected void SelectListViewItemsByTags<T>(ListView listView, IEnumerable<T> values, T ensureVisible = null)
            where T: class
        {
            _CommonBaseBehaviour.SelectListViewItemsByTags<T>(listView, values, ensureVisible);
        }

        /// <summary>
        /// Sets the checked state of every list view item.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="checkedValues"></param>
        protected void CheckListViewItemsByTags<T>(ListView listView, IEnumerable<T> checkedValues)
            where T: class
        {
            _CommonBaseBehaviour.CheckListViewItemsByTags<T>(listView, checkedValues);
        }
        #endregion

        #region FillDropDownWithEnumValues, FillDropDownWithValues, GetSelectedComboBoxValue, SelectComboBoxItemByValue
        /// <summary>
        /// Fills the dropdown list for a combo box with enum values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        /// <param name="converter"></param>
        protected void FillDropDownWithEnumValues<T>(ComboBox comboBox, TypeConverter converter)
        {
            _CommonBaseBehaviour.FillDropDownWithEnumValues<T>(comboBox, converter);
        }

        /// <summary>
        /// Fills the dropdown list for a combo box with values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        /// <param name="values"></param>
        /// <param name="getDescription"></param>
        protected void FillDropDownWithValues<T>(ComboBox comboBox, IEnumerable<T> values, Func<T, string> getDescription)
        {
            _CommonBaseBehaviour.FillDropDownWithValues<T>(comboBox, values, getDescription);
        }

        /// <summary>
        /// Returns the value associated with the selected item in a combo box.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected T GetSelectedComboBoxValue<T>(ComboBox comboBox, T defaultValue = default(T))
        {
            return _CommonBaseBehaviour.GetSelectedComboBoxValue<T>(comboBox, defaultValue);
        }

        /// <summary>
        /// Selects the item in a combo box associated with the value passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        /// <param name="value"></param>
        protected void SelectComboBoxItemByValue<T>(ComboBox comboBox, T value)
        {
            _CommonBaseBehaviour.SelectComboBoxItemByValue<T>(comboBox, value);
        }

        /// <summary>
        /// Returns a collection of all of the items in the combo box.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        /// <returns></returns>
        protected IEnumerable<T> GetComboBoxValues<T>(ComboBox comboBox)
        {
            return _CommonBaseBehaviour.GetComboBoxValues<T>(comboBox);
        }
        #endregion

        #region PopulateListView, FillListViewItem, FindListViewItemForRecord
        /// <summary>
        /// Populates a list view, assigning the record to Tag and reselecting the currently selected record if applicable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="records"></param>
        /// <param name="selectedRecord"></param>
        /// <param name="populateListViewItem"></param>
        /// <param name="selectRecord"></param>
        protected virtual void PopulateListView<T>(ListView listView, IEnumerable<T> records, T selectedRecord, Action<ListViewItem> populateListViewItem, Action<T> selectRecord)
        {
            _CommonBaseBehaviour.PopulateListView<T>(listView, records, selectedRecord, populateListViewItem, selectRecord);
        }

        /// <summary>
        /// Fills a list view item with text columns on the assumption that the item's tag contains a reference to a record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="extractColumnText"></param>
        protected void FillListViewItem<T>(ListViewItem item, Func<T, string[]> extractColumnText)
            where T: class
        {
            _CommonBaseBehaviour.FillListViewItem<T>(item, extractColumnText);
        }

        /// <summary>
        /// Fills a list view item with text columns and sets the Checked state on the assumption that the item's tag contains a reference to a record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="extractColumnText"></param>
        /// <param name="extractChecked"></param>
        protected void FillAndCheckListViewItem<T>(ListViewItem item, Func<T, string[]> extractColumnText, Func<T, bool> extractChecked)
            where T: class
        {
            _CommonBaseBehaviour.FillAndCheckListViewItem<T>(item, extractColumnText, extractChecked);
        }

        /// <summary>
        /// Returns the list view item whose tag is the same as the record passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        protected ListViewItem FindListViewItemForRecord<T>(ListView listView, T record)
            where T: class
        {
            return _CommonBaseBehaviour.FindListViewItemForRecord<T>(listView, record);
        }
        #endregion

        #region View interface helper methods
        /// <summary>
        /// Implements <see cref="IValidateView.ShowValidationResults"/>.
        /// </summary>
        /// <param name="results"></param>
        public virtual void ShowValidationResults(ValidationResults results)
        {
            if(_ValidationHelper != null) _ValidationHelper.ShowValidationResults(results);
        }
        #endregion
    }
}
