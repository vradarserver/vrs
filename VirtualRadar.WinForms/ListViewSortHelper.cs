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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// A class that can help implement sorting of list views when their header columns are clicked.
    /// </summary>
    class ListViewSortHelper
    {
        /// <summary>
        /// The custom comparer that determines the relative sort order of rows in the list view.
        /// </summary>
        class ItemComparer : IComparer
        {
            ListViewSortHelper _SortHelper;

            public ItemComparer(ListViewSortHelper sortHelper)
            {
                _SortHelper = sortHelper;
            }

            public int Compare(object x, object y)
            {
                var result = 0;
                var lhs = x as ListViewItem;
                var rhs = y as ListViewItem;

                if(!Object.ReferenceEquals(lhs, rhs)) {
                    if(lhs == null && rhs != null)      result = -1;
                    else if(rhs == null && lhs != null) result = 1;
                    else if(lhs != null && rhs != null) {
                        var column = _SortHelper.SortColumn;
                        if(column >= 0 && column <= lhs.SubItems.Count) {
                            var lhsText = lhs.SubItems[column].Text;
                            var rhsText = rhs.SubItems[column].Text;
                            result = String.Compare((lhsText ?? "").Trim(), (rhsText ?? "").Trim(), StringComparison.CurrentCultureIgnoreCase);
                            if(_SortHelper.SortOrder == SortOrder.Descending) result = -result;
                        }
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// The comparer that we have attached to the list view.
        /// </summary>
        private ItemComparer _ItemComparer;

        /// <summary>
        /// Gets or sets the column that the list is currently sorting by.
        /// </summary>
        public int SortColumn { get; set; }

        /// <summary>
        /// Gets the direction in which the column is sorted.
        /// </summary>
        public SortOrder SortOrder { get; set; }

        /// <summary>
        /// Gets the list view that the helper is sorting.
        /// </summary>
        public ListView ListView { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="listView"></param>
        public ListViewSortHelper(ListView listView)
        {
            ListView = listView;
            SortColumn = ListView.Sorting == SortOrder.None ? -1 : 0;
            SortOrder = ListView.Sorting;

            _ItemComparer = new ItemComparer(this);

            ListView.Sorting = SortOrder.None;
            ListView.ListViewItemSorter = _ItemComparer;
            ListView.ColumnClick += ListView_ColumnClick;
        }

        /// <summary>
        /// Called when the user clicks on a column header.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ListView_ColumnClick(object sender, ColumnClickEventArgs args)
        {
            if(SortColumn == args.Column) {
                SortOrder = SortOrder == SortOrder.Ascending ? SortOrder.Descending : SortOrder.Ascending;
            } else {
                SortColumn = args.Column;
                SortOrder = SortOrder.Ascending;
            }

            ListView.Sort();
        }
    }
}
