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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.SettingPage
{
    /// <summary>
    /// A class that helps when presenting lists of records where the records are
    /// shown in sub-pages.
    /// </summary>
    class RecordListHelper<TRecord, TPage>
        where TRecord: class
        where TPage: Page, new()
    {
        private Page _ListPage;
        private BindingListView _Control;
        private IList _List;
        private Func<object, ColumnHeader, IComparable, IComparable> _GetSortValue;
        private Comparer _Comparer;

        /// <summary>
        /// An implementation of <see cref="AutoListViewSorter"/> that adds automatic
        /// sorting to the list.
        /// </summary>
        class Comparer : AutoListViewSorter
        {
            private RecordListHelper<TRecord, TPage> _Helper;

            public Comparer(RecordListHelper<TRecord, TPage> helper) : base(helper._Control.ListView)
            {
                _Helper = helper;
            }

            public override IComparable GetRowValue(ListViewItem listViewItem)
            {
                IComparable result = base.GetRowValue(listViewItem);

                if(_Helper._GetSortValue != null) {
                    result = _Helper._GetSortValue(listViewItem.Tag, SortColumn, result);
                }

                return result;
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="listPage"></param>
        /// <param name="listView"></param>
        /// <param name="list"></param>
        public RecordListHelper(Page listPage, BindingListView listView, IList list) : this(listPage, listView, list, null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="listPage"></param>
        /// <param name="listView"></param>
        /// <param name="list"></param>
        /// <param name="getSortValue"></param>
        public RecordListHelper(Page listPage, BindingListView listView, IList list, Func<object, ColumnHeader, IComparable, IComparable> getSortValue)
        {
            _ListPage = listPage;
            _Control = listView;
            _List = list;
            _GetSortValue = getSortValue;

            _Comparer = new Comparer(this);

            listView.ListView.ListViewItemSorter = _Comparer;
            listView.DataSource = list;
        }

        /// <summary>
        /// Handles the Add event.
        /// </summary>
        /// <param name="createNewRecord"></param>
        public void AddClicked(Func<TRecord> createNewRecord)
        {
            var record = createNewRecord();
            _List.Add(record);

            _Control.SelectedRecord = record;
            _ListPage.SettingsView.DisplayPageForPageObject(record);
        }

        /// <summary>
        /// Handles the Delete event.
        /// </summary>
        public void DeleteClicked()
        {
            var deleteRecords = _Control.SelectedRecords.OfType<TRecord>().ToArray();
            foreach(var deleteRecord in deleteRecords) {
                _List.Remove(deleteRecord);
            }
        }

        /// <summary>
        /// Handles the Edit event.
        /// </summary>
        public void EditClicked()
        {
            var record = _Control.SelectedRecord as TRecord;
            if(record != null) _ListPage.SettingsView.DisplayPageForPageObject(record);
        }

        /// <summary>
        /// Handles the CheckedChanged event.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="setEnabled"></param>
        public void CheckedChanged(BindingListView.RecordCheckedEventArgs args, Action<TRecord, bool> setEnabled)
        {
            var page = _ListPage.SettingsView.FindPageForPageObject(args.Record) as TPage;
            if(page != null) {
                var record = page.PageObject as TRecord;
                if(record != null) {
                    setEnabled(record, args.Checked);
                }
            }
        }
    }
}
