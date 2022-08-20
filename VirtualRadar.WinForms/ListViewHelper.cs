// Copyright © 2022 onwards, Andrew Whewell
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
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// Common methods for working with list views where each row represents an object or value.
    /// </summary>
    /// <typeparam name="TAttachedValue">The type of object or value associated with each row. Cannot be a value type.</typeparam>
    /// <typeparam name="TOptionalID">An optional type of the single unique ID for the attached value. Set this to object if not required.</typeparam>
    public class ListViewHelper<TAttachedValue, TOptionalID>
        where TAttachedValue : class
    {
        // A map of FontStyle combinations to the font described by that combination. There is no entry for the list view's Font fontstyle.
        private Dictionary<FontStyle, Font> _Fonts = new Dictionary<FontStyle, Font>();

        // True if the ItemChecked event should be suppressed.
        private bool _SuppressItemChecked;

        /// <summary>
        /// Gets the list view that this is helping with.
        /// </summary>
        public ListView ListView { get; }

        /// <summary>
        /// Gets the callback that extracts column text out of an attached value.
        /// </summary>
        public Func<TAttachedValue, IEnumerable<string>> ExtractColumnText { get; }

        /// <summary>
        /// Gets the callback that will be used to decide whether two attached values are equal.
        /// </summary>
        public Func<TAttachedValue, TAttachedValue, bool> AreAttachedValuesEqual { get; }

        /// <summary>
        /// Gets the optional callback used to extract an ID from an attached value. This will be null if ID handling
        /// is not required or supported for the attached value type.
        /// </summary>
        public Func<TAttachedValue, TOptionalID> ExtractID { get; }

        /// <summary>
        /// Gets the optional callback used to decide on an font style for an attached value.
        /// </summary>
        public Func<TAttachedValue, FontStyle> GetFontStyle { get; }

        /// <summary>
        /// Gets a value indicating that the attached value for a ListViewItem is its Text property rather than its Tag.
        /// </summary>
        public bool AttachedValueIsItemText { get; }

        /// <summary>
        /// Gets a value indicating that if the list view only has one column then the column will be automatically resized
        /// to fill the available space while leaving room for a vertical scroll bar.
        /// </summary>
        public bool AutoResizeSingleColumnWidth { get; }

        /// <summary>
        /// Gets all values attached to ListView items.
        /// </summary>
        public TAttachedValue[] AllAttachedValues
        {
            get => GetAttachedValues(ListView.Items);
        }

        /// <summary>
        /// Gets all of the IDs for all values attached to ListView items. Only works if <see cref="ExtractID"/> was supplied.
        /// </summary>
        public TOptionalID[] AllAttachedValueIDs
        {
            get => GetAttachedValueIDs(ListView.Items);
        }

        /// <summary>
        /// Gets or sets all values attached to all selected ListView items.
        /// </summary>
        public IEnumerable<TAttachedValue> SelectedAttachedValues
        {
            get => GetAttachedValues(ListView.SelectedItems);
            set => SetAttachedValues(ListView.Items, value, (lvi, itemValue, isValueSelected) => lvi.Selected = isValueSelected);
        }

        /// <summary>
        /// Gets or sets all IDs extracted from all values attached to selected ListVIew items.
        /// </summary>
        public IEnumerable<TOptionalID> SelectedAttachedValueIDs
        {
            get => GetAttachedValueIDs(ListView.SelectedItems);
            set => SetAttachedValueIDs(ListView.Items, value, (lvi, itemValue, isValueSelected) => lvi.Selected = isValueSelected);
        }

        /// <summary>
        /// Gets or sets all values attached to all ticked ListView items.
        /// </summary>
        public IEnumerable<TAttachedValue> CheckedAttachedValues
        {
            get => GetAttachedValues(ListView.CheckedItems);
            set => SetAttachedValues(ListView.Items, value, (lvi, itemValue, isValueChecked) => lvi.Checked = isValueChecked);
        }

        /// <summary>
        /// Gets or sets all IDs extracted from values attached to all ticked ListView items.
        /// </summary>
        public IEnumerable<TOptionalID> CheckedAttachedValueIDs
        {
            get => GetAttachedValueIDs(ListView.CheckedItems);
            set => SetAttachedValueIDs(ListView.Items, value, (lvi, itemValue, isValueChecked) => lvi.Checked = isValueChecked);
        }

        /// <summary>
        /// Mirrors the list view's ItemChecked event.
        /// </summary>
        public event EventHandler<ItemCheckedEventArgs> ItemChecked;

        /// <summary>
        /// Raises <see cref="ItemChecked"/>.
        /// </summary>
        /// <param name="args"></param>
        public void RaiseItemChecked(ItemCheckedEventArgs args) => ItemChecked?.Invoke(ListView, args);

        /// <summary>
        /// Raised when one or more items are checked by <see cref="ToggleChecked"/> or <see cref="CheckAll"/>.
        /// </summary>
        public event EventHandler ManyItemsChecked;

        /// <summary>
        /// Raises <see cref="ManyItemsChecked"/>.
        /// </summary>
        /// <param name="args"></param>
        public void RaiseManyItemsChecked(EventArgs args) => ManyItemsChecked?.Invoke(ListView, args);

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="extractColumnText"></param>
        /// <param name="areAttachedValuesEqual"></param>
        /// <param name="attachedValueIsItemText"></param>
        /// <param name="extractID"></param>
        /// <param name="getFontStyle"></param>
        public ListViewHelper(
            ListView control,
            Func<TAttachedValue, IEnumerable<string>> extractColumnText,
            Func<TAttachedValue, TAttachedValue, bool> areAttachedValuesEqual = null,
            bool attachedValueIsItemText = false,
            Func<TAttachedValue, TOptionalID> extractID = null,
            Func<TAttachedValue, FontStyle> getFontStyle = null,
            bool autoResizeSingleColumnWidth = true
        )
        {
            ListView = control;
            ExtractColumnText = extractColumnText;
            AreAttachedValuesEqual = areAttachedValuesEqual ?? ((lhs, rhs) => Object.ReferenceEquals(lhs, rhs));
            AttachedValueIsItemText = attachedValueIsItemText;
            ExtractID = extractID;
            GetFontStyle = getFontStyle ?? (r => ListView.Font.Style);
            AutoResizeSingleColumnWidth = autoResizeSingleColumnWidth;

            ListView.Disposed += ListView_Disposed;
            ListView.ItemChecked += ListView_ItemChecked;
            ListView.Resize += ListView_Resize;

            AutoResizeColumns();
        }

        /// <summary>
        /// Rebuilds the listview by deleting all items and then re-adding them all.
        /// </summary>
        public void RebuildList(IEnumerable<TAttachedValue> sortedValues, bool scrollFirstSelectedIntoView = false)
        {
            var selectedValues = SelectedAttachedValues;
            var checkedValues = ListView.CheckBoxes ? CheckedAttachedValues : null;

            ListView.BeginUpdate();
            try {
                ListView.Items.Clear();
                foreach(var value in sortedValues) {
                    ListView.Items.Add(FormatListViewItemFromValue(new ListViewItem(), value));
                }

                if(ListView.CheckBoxes) {
                    CheckedAttachedValues = checkedValues;
                }
                SelectedAttachedValues = selectedValues;

                if(scrollFirstSelectedIntoView) {
                    ScrollToFirst(selectedValues);
                }
            } finally {
                ListView.EndUpdate();
            }
        }

        /// <summary>
        /// Refreshes the content of the list view without rebuilding from scratch. Each row is checked to see if it needs
        /// to be added, updated or deleted and the corresponding action taken. If there are no updates required then the
        /// list is not touched.
        /// </summary>
        /// <param name="sortedValues"></param>
        /// <param name="suppressItemChecked"></param>
        public void RefreshList(IEnumerable<TAttachedValue> sortedValues, bool suppressItemChecked = false)
        {
            var selectedValues = SelectedAttachedValues.ToArray();
            var checkedValues = ListView.CheckBoxes ? CheckedAttachedValues.ToArray() : new TAttachedValue[0];

            var oldSuppressItemChecked = _SuppressItemChecked;

            ListView.BeginUpdate();
            try {
                _SuppressItemChecked = suppressItemChecked;

                var pinnedValues = sortedValues?.ToArray() ?? new TAttachedValue[0];
                for(var i = 0;i < pinnedValues.Length;++i) {
                    var lvi = ListView.Items.Count > i ? ListView.Items[i] : null;
                    var addListViewItem = lvi == null;
                    if(addListViewItem) {
                        //lvi = new ListViewItem();     <-- this works but there is something messed up in ListView, if I later add the LVI it can crash with a bad index in SetItemState when calling Add()
                        //                              <-- note that SetItemState is internal to the listview, we don't pass the index when we call Add(). It's like the standalone LVI can't be added.
                        lvi = ListView.Items.Add("");
                    }

                    var value = pinnedValues[i];
                    FormatListViewItemFromValue(lvi, value);

                    //if(addListViewItem) {             <-- see notes above, we can't reliably add standalone LVIs. They *occasionally* trigger a weird exception - I was seeing it in a single column
                    //    ListView.Items.Add(lvi);      <-- listview with checkboxes switched on.
                    //}

                    var selectItem = selectedValues.Any(r => AreAttachedValuesEqual(r, value));
                    if(lvi.Selected != selectItem) {
                        lvi.Selected = selectItem;
                    }

                    var checkItem = checkedValues.Any(r => AreAttachedValuesEqual(r, value));
                    if(lvi.Checked != checkItem) {
                        lvi.Checked = checkItem;
                    }
                }

                while(pinnedValues.Length < ListView.Items.Count) {
                    ListView.Items.RemoveAt(ListView.Items.Count - 1);
                }
            } finally {
                _SuppressItemChecked = oldSuppressItemChecked;
                ListView.EndUpdate();
            }
        }

        /// <summary>
        /// Returns the first ListViewItem that has the supplied attached value or null if no such item exists.
        /// </summary>
        /// <param name="attachedValue"></param>
        /// <returns></returns>
        public ListViewItem FindListViewItemForAttachedValue(TAttachedValue attachedValue)
        {
            ListViewItem result = null;

            if(attachedValue != null) {
                result = ListView
                    .Items
                    .OfType<ListViewItem>()
                    .FirstOrDefault(r => GetAttachedValue(r) is TAttachedValue itemValue && AreAttachedValuesEqual(attachedValue, itemValue));
            }

            return result;
        }

        /// <summary>
        /// Returns the value attached to the list view item.
        /// </summary>
        /// <param name="lvi"></param>
        /// <returns></returns>
        public TAttachedValue GetAttachedValueFromListViewItem(ListViewItem lvi)
        {
            return lvi == null ? default(TAttachedValue) : GetAttachedValue(lvi) as TAttachedValue;
        }

        /// <summary>
        /// Returns the optional ID attached to the list view item.
        /// </summary>
        /// <param name="lvi"></param>
        /// <returns></returns>
        public TOptionalID GetAttachedValueIDFromListViewItem(ListViewItem lvi)
        {
            return lvi == null ? default(TOptionalID) : GetAttachedValueIDs((IEnumerable<ListViewItem>)(new ListViewItem[] { lvi })).FirstOrDefault();
        }

        /// <summary>
        /// Scrolls to the first ListView item that has the first attached value passed across.
        /// </summary>
        /// <param name="attachedValues"></param>
        public void ScrollToFirst(IEnumerable<TAttachedValue> attachedValues)
        {
            var lvi = FindListViewItemForAttachedValue(attachedValues?.FirstOrDefault());
            if(lvi != null) {
                lvi.EnsureVisible();
            }
        }

        public bool ToggleChecked(bool suppressItemChecked = false, bool suppressManyItemsChecked = false)
        {
            return MassCheckChanges(r => !r.Checked, suppressItemChecked, suppressManyItemsChecked);
        }

        public bool CheckAll(bool isChecked, bool suppressItemChecked = false, bool suppressManyItemsChecked = false)
        {
            return MassCheckChanges(r => isChecked, suppressItemChecked, suppressManyItemsChecked);
        }

        private bool MassCheckChanges(Func<ListViewItem, bool> newCheckedFunc, bool suppressItemChecked, bool suppressManyItemsChecked)
        {
            var anyChanged = false;

            var initialSuppress = _SuppressItemChecked;
            if(suppressItemChecked) {
                _SuppressItemChecked = true;
            }

            try {
                foreach(ListViewItem item in ListView.Items) {
                    var newCheckedValue = newCheckedFunc(item);
                    if(newCheckedValue != item.Checked) {
                        anyChanged = true;
                        item.Checked = newCheckedValue;
                    }
                }
            } finally {
                _SuppressItemChecked = initialSuppress;
            }

            if(anyChanged && !suppressManyItemsChecked) {
                RaiseManyItemsChecked(EventArgs.Empty);
            }

            return anyChanged;
        }

        public bool CheckAttachedValue(TAttachedValue attachedValue, bool isChecked, bool suppressItemChecked = false)
        {
            var changed = false;

            var item = FindListViewItemForAttachedValue(attachedValue);
            if(item != null) {
                var initialSuppress = _SuppressItemChecked;
                if(suppressItemChecked) {
                    _SuppressItemChecked = true;
                }

                try {
                    changed = isChecked != item.Checked;
                    if(changed) {
                        item.Checked = isChecked;
                    }
                } finally {
                    _SuppressItemChecked = initialSuppress;
                }
            }

            return changed;
        }

        /// <summary>
        /// Calls <paramref name="checkFunc"/> for each attached value, optionally suppressing events if the checked state changes.
        /// </summary>
        /// <param name="checkFunc">A func that returns the checked state to show against the value.</param>
        /// <param name="suppressItemChecked">False if ItemChecked is to be raised for every item whose checked state changes.</param>
        /// <param name="suppressManyItemsChecked">False if ManyItemsChecked is to be raised if any item's checked state changes.</param>
        /// <returns>True if any item's checked state changed.</returns>
        public bool ConditionallyCheckAttachedValues(Func<TAttachedValue, bool> checkFunc, bool suppressItemChecked = false, bool suppressManyItemsChecked = false)
        {
            return MassCheckChanges(lvi =>
                {
                    var result = lvi.Checked;
                    var attachedValue = GetAttachedValueFromListViewItem(lvi);
                    if(attachedValue != null) {
                        result = checkFunc(attachedValue);
                    }
                    return result;
                },
                suppressItemChecked,
                suppressManyItemsChecked
            );
        }

        /// <summary>
        /// Calls <paramref name="checkFunc"/> for each attached value, optionally suppressing events if the checked state changes.
        /// </summary>
        /// <param name="checkFunc">A func that returns the checked state to show against the value.</param>
        /// <param name="suppressItemChecked">False if ItemChecked is to be raised for every item whose checked state changes.</param>
        /// <param name="suppressManyItemsChecked">False if ManyItemsChecked is to be raised if any item's checked state changes.</param>
        /// <returns>True if any item's checked state changed.</returns>
        public bool ConditionallyCheckAttachedValueIDs(Func<TOptionalID, bool> checkFunc, bool suppressItemChecked = false, bool suppressManyItemsChecked = false)
        {
            return MassCheckChanges(lvi =>
                {
                    var result = lvi.Checked;
                    var optionalID = GetAttachedValueIDFromListViewItem(lvi);
                    result = checkFunc(optionalID);
                    return result;
                },
                suppressItemChecked,
                suppressManyItemsChecked
            );
        }

        public List<NameValueCollection> GetNameValueCollectionsForSelectedRows()
        {
            var result = new List<NameValueCollection>();

            var columnsInOrder = ListView.Columns.OfType<ColumnHeader>().OrderBy(r => r.DisplayIndex).ToArray();

            foreach(int selectedIdx in ListView.SelectedIndices) {
                var row = new NameValueCollection();
                result.Add(row);

                var selectedItem = ListView.Items[selectedIdx];
                for(var colNum = 0;colNum < columnsInOrder.Length;++colNum) {
                    var column = columnsInOrder[colNum];
                    var columnName = !String.IsNullOrEmpty(column.Text) ? column.Text : column.DisplayIndex.ToString();
                    if(row.AllKeys.Contains(columnName)) {
                        columnName = column.DisplayIndex.ToString();
                    }
                    if(row.AllKeys.Contains(columnName)) {
                        columnName = $"[Column Number {colNum + 1}]";
                    }

                    var subItem = selectedItem.SubItems.Count > colNum ? selectedItem.SubItems[colNum] : null;
                    row.Add(columnName, subItem?.Text ?? "");
                }
            }

            return result;
        }

        private ListViewItem FormatListViewItemFromValue(ListViewItem lvi, TAttachedValue value)
        {
            var fontStyle = GetFontStyle(value);
            var font = GetFontForFontStyle(fontStyle);
            if(font.Style != lvi.Font.Style) {
                lvi.Font = font;
            }

            var columnTexts = ExtractColumnText(value).ToArray();

            for(var i = 0;i < columnTexts.Length;++i) {
                var columnText = columnTexts[i] ?? "";

                if(lvi.SubItems.Count <= i) {
                    lvi.SubItems.Add(columnText);
                } else if(lvi.SubItems[i].Text != columnText) {
                    lvi.SubItems[i].Text = columnText;
                }
            }

            if(!AttachedValueIsItemText) {
                lvi.Tag = value;
            }

            return lvi;
        }

        private Font GetFontForFontStyle(FontStyle fontStyle)
        {
            var result = ListView.Font;

            if(fontStyle != ListView.Font.Style) {
                if(!_Fonts.TryGetValue(fontStyle, out result)) {
                    result = new Font(ListView.Font, fontStyle);
                    _Fonts.Add(fontStyle, result);
                }
            }

            return result;
        }

        private object GetAttachedValue(ListViewItem lvi)
        {
            return AttachedValueIsItemText ? lvi.Text : lvi.Tag;
        }

        private TAttachedValue[] GetAttachedValues(IEnumerable<ListViewItem> subsetItems)
        {
            return (subsetItems ?? new ListViewItem[0])
                .Select(r => GetAttachedValue(r) as TAttachedValue)
                .Where(r => r != null)
                .ToArray();
        }

        private TAttachedValue[] GetAttachedValues(ICollection untyped) => GetAttachedValues(untyped.OfType<ListViewItem>());

        private TOptionalID[] GetAttachedValueIDs(IEnumerable<ListViewItem> subsetItems)
        {
            return ExtractID == null ? new TOptionalID[0] : GetAttachedValues(subsetItems).Select(r => ExtractID(r)).ToArray();
        }

        private TOptionalID[] GetAttachedValueIDs(ICollection untyped) => GetAttachedValueIDs(untyped.OfType<ListViewItem>());

        private void SetAttachedValues(IEnumerable<ListViewItem> subsetItems, IEnumerable<TAttachedValue> subsetValues, Action<ListViewItem, TAttachedValue, bool> setCallback)
        {
            var valuesInSubset = subsetValues?.ToArray() ?? new TAttachedValue[0];

            foreach(var item in subsetItems) {
                var itemValue = GetAttachedValue(item) as TAttachedValue;
                var isValueInSubset = itemValue != null && valuesInSubset.Any(r => AreAttachedValuesEqual(r, itemValue));
                setCallback?.Invoke(item, itemValue, isValueInSubset);
            }
        }

        private void SetAttachedValues(ICollection untyped, IEnumerable<TAttachedValue> subsetValues, Action<ListViewItem, TAttachedValue, bool> setCallback) => SetAttachedValues(untyped.OfType<ListViewItem>(), subsetValues, setCallback);

        private void SetAttachedValueIDs(IEnumerable<ListViewItem> subsetItems, IEnumerable<TOptionalID> subsetValueIDs, Action<ListViewItem, TAttachedValue, bool> setCallback)
        {
            SetAttachedValues(subsetItems, FindAttachedValuesForIDs(subsetValueIDs), setCallback);
        }

        private void SetAttachedValueIDs(ICollection untyped, IEnumerable<TOptionalID> subsetValueIDs, Action<ListViewItem, TAttachedValue, bool> setCallback)
        {
            SetAttachedValueIDs(untyped.OfType<ListViewItem>(), subsetValueIDs, setCallback);
        }

        private List<TAttachedValue> FindAttachedValuesForIDs(IEnumerable<TOptionalID> ids)
        {
            var result = new List<TAttachedValue>();

            var normalisedIDs = ids?.ToArray() ?? new TOptionalID[0];
            if(normalisedIDs.Length > 0 && ExtractID != null) {
                foreach(ListViewItem lvi in ListView.Items) {
                    if(GetAttachedValue(lvi) is TAttachedValue itemValue) {
                        var valueID = ExtractID(itemValue);
                        if(normalisedIDs.Contains(valueID)) {
                            result.Add(itemValue);
                        }
                    }
                }
            }

            return result;
        }

        private void AutoResizeColumns()
        {
            if(ListView.Columns.Count == 1 && AutoResizeSingleColumnWidth) {
                ((ColumnHeader)ListView.Columns[0])
                    .Width = Math.Max(30, ListView.Width - 25);
            }
        }

        private void ListView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if(!_SuppressItemChecked) {
                RaiseItemChecked(e);
            }
        }

        private void ListView_Disposed(object sender, EventArgs args)
        {
            foreach(var font in _Fonts.Values) {
                font.Dispose();
            }
            _Fonts.Clear();
        }

        private void ListView_Resize(object sender, EventArgs e)
        {
            AutoResizeColumns();
        }
    }
}
