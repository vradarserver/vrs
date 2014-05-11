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
using System.ComponentModel;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// A utility class encapsulating base behaviour common to both forms and user controls.
    /// </summary>
    class CommonBaseBehaviour
    {
        #region GetSelectedListViewTag, GetAllSelectedListViewTag, SelectListViewItemByTag, SelectListViewItemsByTags
        /// <summary>
        /// Gets the tag associated with the selected row in a list view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <returns></returns>
        public T GetSelectedListViewTag<T>(ListView listView)
            where T: class
        {
            return listView.SelectedItems.Count == 0 ? null : listView.SelectedItems[0].Tag as T;
        }

        /// <summary>
        /// Gets the tags associated with all selected items in a multi-select list view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <returns></returns>
        public T[] GetAllSelectedListViewTag<T>(ListView listView)
            where T: class
        {
            return listView.SelectedItems.OfType<ListViewItem>().Select(r => r.Tag as T).Where(r => r != null).ToArray();
        }

        /// <summary>
        /// Gets the tags associated with all checked items in a list view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <returns></returns>
        public T[] GetAllCheckedListViewTag<T>(ListView listView)
            where T: class
        {
            return listView.CheckedItems.OfType<ListViewItem>().Select(r => r.Tag as T).Where(r => r != null).ToArray();
        }

        /// <summary>
        /// Selects the list view item associated with the tag value passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="value"></param>
        public void SelectListViewItemByTag<T>(ListView listView, T value)
            where T: class
        {
            listView.SelectedIndices.Clear();
            var item = listView.Items.OfType<ListViewItem>().Where(r => r.Tag == value).FirstOrDefault();
            if(item != null) {
                item.Selected = true;
                item.EnsureVisible();
            }
        }

        /// <summary>
        /// Selects many list view items associated with the tag values passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="values"></param>
        /// <param name="ensureVisible"></param>
        public void SelectListViewItemsByTags<T>(ListView listView, IEnumerable<T> values, T ensureVisible = null)
            where T: class
        {
            listView.SelectedIndices.Clear();
            var tags = values.ToArray();
            if(tags.Length > 0) {
                foreach(ListViewItem item in listView.Items) {
                    var tag = item.Tag as T;
                    item.Selected = tags.Contains(tag);
                    if(tag != null && tag == ensureVisible) item.EnsureVisible();
                }
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
        /// <param name="orderByDescription"></param>
        public void FillDropDownWithEnumValues<T>(ComboBox comboBox, TypeConverter converter, bool orderByDescription = true)
        {
            var items = Enum.GetValues(typeof(T))
                            .OfType<T>()
                            .Select(r => new ValueDescription<T>(r, converter.ConvertToString(r)))
                            .ToList();
            if(orderByDescription) items.Sort((lhs, rhs) => String.Compare(lhs.Description, rhs.Description));

            FillDropDownList<T>(comboBox, items);
        }

        /// <summary>
        /// Fills the dropdown list for a combo box with values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        /// <param name="values"></param>
        /// <param name="getDescription"></param>
        public void FillDropDownWithValues<T>(ComboBox comboBox, IEnumerable<T> values, Func<T, string> getDescription)
        {
            var items = values.Select(r => new ValueDescription<T>(r, getDescription(r)))
                              .ToList();
            FillDropDownList<T>(comboBox, items);
        }

        private static void FillDropDownList<T>(ComboBox comboBox, List<ValueDescription<T>> items)
        {
            comboBox.Items.Clear();
            comboBox.DisplayMember = "Description";
            comboBox.ValueMember = "Value";
            foreach (var item in items) {
                comboBox.Items.Add(item);
            }
        }

        /// <summary>
        /// Returns the value associated with the selected item in a combo box.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetSelectedComboBoxValue<T>(ComboBox comboBox, T defaultValue = default(T))
        {
            T result = defaultValue;

            if(comboBox.SelectedIndex != -1) result = ((ValueDescription<T>)comboBox.SelectedItem).Value;

            return result;
        }

        /// <summary>
        /// Selects the item in a combo box associated with the value passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comboBox"></param>
        /// <param name="value"></param>
        public void SelectComboBoxItemByValue<T>(ComboBox comboBox, T value)
        {
            var selectItem = comboBox.Items.OfType<ValueDescription<T>>().FirstOrDefault(r => r.Value.Equals(value));
            comboBox.SelectedItem = selectItem;
        }

        /// <summary>
        /// Returns a collection of all of the combo box values.
        /// </summary>
        /// <param name="comboBox"></param>
        /// <returns></returns>
        public IEnumerable<T> GetComboBoxValues<T>(ComboBox comboBox)
        {
            return comboBox.Items.OfType<ValueDescription<T>>().Select(r => r.Value);
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
        public void PopulateListView<T>(ListView listView, IEnumerable<T> records, T selectedRecord, Action<ListViewItem> populateListViewItem, Action<T> selectRecord)
        {
            listView.Items.Clear();
            foreach(var record in records) {
                var item = new ListViewItem() { Tag = record };
                populateListViewItem(item);
                listView.Items.Add(item);
            }

            if(selectRecord != null && records.Contains(selectedRecord)) selectRecord(selectedRecord);
        }

        /// <summary>
        /// Fills a list view item with text columns on the assumption that the item's tag contains a reference to a record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="extractColumnText"></param>
        public void FillListViewItem<T>(ListViewItem item, Func<T, string[]> extractColumnText)
            where T: class
        {
            FillAndCheckListViewItem<T>(item, extractColumnText, null);
        }

        /// <summary>
        /// Fills a list view item with text columns and sets the Checked state on the assumption that the item's tag contains a reference to a record.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="extractColumnText"></param>
        /// <param name="extractChecked"></param>
        public void FillAndCheckListViewItem<T>(ListViewItem item, Func<T, string[]> extractColumnText, Func<T, bool> extractChecked)
            where T: class
        {
            var record = (T)item.Tag;
            var columnText = extractColumnText(record);
            var ticked = extractChecked == null ? false : extractChecked(record);

            while(item.SubItems.Count < columnText.Length) item.SubItems.Add("");
            for(var i = 0;i < columnText.Length;++i) {
                item.SubItems[i].Text = columnText[i];
            }

            if(extractChecked != null) item.Checked = ticked;
        }

        /// <summary>
        /// Returns the list view item whose tag is the same as the record passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        public ListViewItem FindListViewItemForRecord<T>(ListView listView, T record)
            where T: class
        {
            return record == null ? null : listView.Items.OfType<ListViewItem>().Where(r => r.Tag == record).FirstOrDefault();
        }
        #endregion

        #region ParseNInt
        /// <summary>
        /// Parses a nullable integer, returning null if the text is unparseable.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public int? ParseNInt(string text)
        {
            int result;
            return int.TryParse(text, out result) ? result : (int?)null;
        }
        #endregion
    }
}
