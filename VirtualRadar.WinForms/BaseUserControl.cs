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
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.View;
using VirtualRadar.WinForms.Controls;

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
            _ValidationHelper.ShowValidationResults(results);
        }
        #endregion
    }
}
