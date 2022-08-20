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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// Common methods for working with combo boxes where each item represents an object or value.
    /// </summary>
    /// <typeparam name="TItem">The type of object or value associated with each combo box item.</typeparam>
    /// <typeparam name="TOptionalID">An optional type of the single unique ID that can be extracted from an item. Set this to object if not required.</typeparam>
    public class ComboBoxHelper<TItem, TOptionalID>
    {
        class ComboBoxItem
        {
            public ComboBoxHelper<TItem, TOptionalID> Parent;
            public TItem Item;

            public override string ToString() => Parent.ExtractDescription(Item) ?? "";
        }

        /// <summary>
        /// Gets the combo box that this is helping with.
        /// </summary>
        public ComboBox ComboBox { get; }

        /// <summary>
        /// Gets the callback that can extract the item's description. This is what gets shown in the combo box for each item.
        /// </summary>
        public Func<TItem, string> ExtractDescription { get; }

        /// <summary>
        /// Gets the callback that will be used to decide whether two items are equal.
        /// </summary>
        public Func<TItem, TItem, bool> AreItemsEqual { get; }

        /// <summary>
        /// Gets the optional callback that can extract an ID from an item.
        /// </summary>
        public Func<TItem, TOptionalID> ExtractID { get; }

        /// <summary>
        /// Gets a value indicating that an entry should be added to the combo box for the null / default value.
        /// </summary>
        public bool AddDefaultValueItem { get; }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public TItem SelectedItem
        {
            get {
                var item = (ComboBoxItem)ComboBox.SelectedItem;
                return item == null ? default(TItem) : item.Item;
            }
            set {
                ComboBox.SelectedItem = FindFirstComboBoxItemForItem(value);
            }
        }

        /// <summary>
        /// Gets or sets the selected item by its ID.
        /// </summary>
        public TOptionalID SelectedItemID
        {
            get => ExtractID == null || ComboBox.SelectedIndex < 0 ? default(TOptionalID) : ExtractID(SelectedItem);
            set => ComboBox.SelectedItem = FindFirstComboBoxItemForID(value);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="control"></param>
        /// <param name="extractDescription"></param>
        /// <param name="areItemsEqual"></param>
        /// <param name="extractID"></param>
        /// <param name="addDefaultValue"></param>
        public ComboBoxHelper(
            ComboBox control,
            Func<TItem, string> extractDescription = null,
            Func<TItem, TItem, bool> areItemsEqual = null,
            Func<TItem, TOptionalID> extractID = null,
            bool addDefaultValue = false
        )
        {
            ComboBox = control;
            ExtractDescription = extractDescription ?? (r => r.ToString());
            AreItemsEqual = areItemsEqual ?? ((lhs, rhs) => Object.Equals(lhs, rhs));
            ExtractID = extractID;
            AddDefaultValueItem = addDefaultValue;
        }

        /// <summary>
        /// Rebuilds the combo box by deleting all items and then re-adding them all.
        /// </summary>
        /// <param name="sortedItems"></param>
        public void RebuildComboBox(IEnumerable<TItem> sortedItems)
        {
            var selectedItem = SelectedItem;

            ComboBox.BeginUpdate();
            try {
                ComboBox.Items.Clear();

                if(AddDefaultValueItem) {
                    ComboBox.Items.Add(
                        new ComboBoxItem() {
                            Parent = this,
                            Item =   default(TItem),
                        }
                    );
                }

                foreach(var item in sortedItems) {
                    ComboBox.Items.Add(
                        new ComboBoxItem() {
                            Parent = this,
                            Item =   item,
                        }
                    );
                }

                SelectedItem = selectedItem;
            } finally {
                ComboBox.EndUpdate();
            }
        }

        private ComboBoxItem FindFirstComboBoxItemForItem(TItem item)
        {
            return ComboBox
                .Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(r => AreItemsEqual(item, r.Item));
        }

        private ComboBoxItem FindFirstComboBoxItemForID(TOptionalID id)
        {
            return ExtractID == null
                ? null
                : ComboBox
                    .Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(r => Object.Equals(id, ExtractID(r.Item)));
        }
    }
}
