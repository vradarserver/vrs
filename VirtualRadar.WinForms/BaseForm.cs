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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.ComponentModel;
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms.Controls;
using System.Linq.Expressions;
using System.Reflection;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The base for all forms.
    /// </summary>
    public class BaseForm : Form
    {
        #region Fields
        /// <summary>
        /// The object that helps display validation messages. The derivee is responsible for creating this.
        /// </summary>
        protected ValidationHelper _ValidationHelper;

        /// <summary>
        /// The object that helps hook value changed events on controls.
        /// </summary>
        protected ValueChangedHelper _ValueChangedHelper;

        /// <summary>
        /// The object that encapsulates behaviour shared between the base form and base controls.
        /// </summary>
        private CommonBaseBehaviour _CommonBaseBehaviour = new CommonBaseBehaviour();
        #endregion

        #region Properties
        private MonoAutoScaleMode _MonoAutoScaleMode;
        /// <summary>
        /// Gets or sets the autoscale mode.
        /// </summary>
        /// <remarks>
        /// Mono has a bit of a problem with AutoScale mode, it needs to be forced off when running under Mono but
        /// allowed to work as-per normal for .NET forms.
        /// </remarks>
        [DefaultValue(AutoScaleMode.Font)]
        public new AutoScaleMode AutoScaleMode
        {
            get { return _MonoAutoScaleMode.AutoScaleMode; }
            set { _MonoAutoScaleMode.AutoScaleMode = value; }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ValueChanged;

        /// <summary>
        /// Raises <see cref="ValueChanged"/>.
        /// </summary>
        /// <param name="sender">The control whose value has changed.</param>
        /// <param name="args"></param>
        protected virtual void OnValueChanged(object sender, EventArgs args)
        {
            if(ValueChanged != null) ValueChanged(sender, args);
        }

        /// <summary>
        /// Raises <see cref="ValueChanged"/>
        /// </summary>
        /// <param name="control"></param>
        /// <param name="args"></param>
        internal void RaiseValueChanged(Control control, EventArgs args)
        {
            OnValueChanged(control, args);
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BaseForm() : base()
        {
            _MonoAutoScaleMode = new MonoAutoScaleMode(this);

            _ValueChangedHelper = new ValueChangedHelper((sender, args) => OnValueChanged(sender, args));
        }
        #endregion

        #region GetAllChildControls
        /// <summary>
        /// Returns a recursive collection of every control on the form. Can optionally include the form itself
        /// as the first control.
        /// </summary>
        /// <param name="includeThis"></param>
        /// <returns></returns>
        public List<Control> GetAllChildControls(bool includeThis = false)
        {
            return _CommonBaseBehaviour.GetAllChildControls(this, includeThis);
        }
        #endregion

        #region AddBinding, GetAllDataBindings, GetAllDataBindingsForAttribute, GetPropertyInfoForBinding
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
        public System.Windows.Forms.Binding AddBinding<TControl, TModel>(TModel model, TControl control, Expression<Func<TModel, object>> modelProperty, Expression<Func<TControl, object>> controlProperty, DataSourceUpdateMode dataSourceUpdateMode = DataSourceUpdateMode.OnValidation)
            where TControl: Control
        {
            return _CommonBaseBehaviour.AddBinding<TControl, TModel>(model, control, modelProperty, controlProperty, dataSourceUpdateMode);
        }

        /// <summary>
        /// Returns all bindings for this form and all child controls.
        /// </summary>
        /// <param name="includeChildControls"></param>
        /// <returns></returns>
        public List<System.Windows.Forms.Binding> GetAllDataBindings(bool includeChildControls)
        {
            return _CommonBaseBehaviour.GetAllDataBindings(this, includeChildControls);
        }

        /// <summary>
        /// Returns all bindings on this form or any child that is bound to a property that is tagged
        /// with an attribute of the type passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public List<BindingTag<T>> GetAllDataBindingsForAttribute<T>(bool includeChildControls, bool inherit)
            where T: Attribute
        {
            return _CommonBaseBehaviour.GetAllDataBindingsForAttribute<T>(this, includeChildControls, inherit);
        }

        /// <summary>
        /// Returns the property info associated with a binding or null if it cannot be found.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        public PropertyInfo GetPropertyInfoForBinding(System.Windows.Forms.Binding binding)
        {
            return GetPropertyInfoForBinding(binding);
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

        #region GetSelectedTreeViewNodeTag, SelectTreeViewNodeByTag, AllTreeViewNodes
        /// <summary>
        /// Returns the Tag of the selected node case to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="treeView"></param>
        /// <returns></returns>
        protected T GetSelectedTreeViewNodeTag<T>(TreeView treeView)
            where T: class
        {
            return treeView.SelectedNode == null ? null : treeView.SelectedNode.Tag as T;
        }

        /// <summary>
        /// Sets the selected node of the tree view to the node whose Tag is <paramref name="tag"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="treeView"></param>
        /// <param name="tag"></param>
        protected void SelectTreeViewNodeByTag<T>(TreeView treeView, T tag)
            where T: class
        {
            var tagNode = AllTreeViewNodes(treeView).FirstOrDefault(r => r.Tag == tag);
            if(tagNode != null) {
                treeView.SelectedNode = tagNode;
                tagNode.EnsureVisible();
            }
        }

        /// <summary>
        /// Returns a collection of every node within a tree view.
        /// </summary>
        /// <param name="treeView"></param>
        /// <returns></returns>
        protected IEnumerable<TreeNode> AllTreeViewNodes(TreeView treeView)
        {
            var result = new List<TreeNode>();
            AddTreeNodes(result, treeView.Nodes);

            return result;
        }

        private void AddTreeNodes(List<TreeNode> result, TreeNodeCollection treeNodeCollection)
        {
            foreach(TreeNode node in treeNodeCollection) {
                result.Add(node);
                if(node.Nodes.Count > 0) AddTreeNodes(result, node.Nodes);
            }
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
        public virtual void ShowValidationResults(IEnumerable<ValidationResult> results)
        {
            if(_ValidationHelper != null) _ValidationHelper.ShowValidationResults(results);
        }

        /// <summary>
        /// Implements <see cref="IValidateView.ShowSingleFieldValidationResults"/>.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="validationField"></param>
        /// <param name="results"></param>
        public virtual void ShowSingleFieldValidationResults(object record, ValidationField validationField, IEnumerable<ValidationResult> results)
        {
            if(_ValidationHelper != null) _ValidationHelper.ShowSingleFieldValidationResults(record, validationField, results);
        }

        /// <summary>
        /// Implements <see cref="IBusyView.ShowBusy"/>.
        /// </summary>
        /// <param name="isBusy"></param>
        /// <param name="previousState"></param>
        /// <returns></returns>
        public virtual object ShowBusy(bool isBusy, object previousState)
        {
            return BusyViewHelper.ShowBusy(isBusy, previousState);
        }

        /// <summary>
        /// Implements <see cref="IConfirmView.ConfirmWithUser"/>.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="defaultYes"></param>
        /// <returns></returns>
        public virtual bool? ConfirmWithUser(string title, string message, bool defaultYes)
        {
            var result = MessageBox.Show(message, title, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, defaultYes ? MessageBoxDefaultButton.Button1 : MessageBoxDefaultButton.Button2);
            return result == DialogResult.Yes ? true : result == DialogResult.No ? false : (bool?)null;
        }
        #endregion
    }
}
