// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//
// Portions copyright © 2003 Ian Griffiths
// http://www.interact-sw.co.uk/utilities/bindablelistview/source/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Collections;
using VirtualRadar.Localisation;
using VirtualRadar.Resources;
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;
using System.Collections.Specialized;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A list view that deals with the common problems with using a list view to maintain a list
    /// of objects.
    /// </summary>
    [DefaultBindingProperty("Records")]
    public partial class BindingListView : BaseUserControl, IValidateDelegate
    {
        #region Nested class - ColumnTextEventArgs
        /// <summary>
        /// The event args for events that request the ListView content for a record.
        /// </summary>
        public class RecordContentEventArgs : EventArgs
        {
            public object Record { get; private set; }

            public List<string> ColumnTexts { get; private set; }

            public bool Checked { get; set; }

            public RecordContentEventArgs(object record)
            {
                Record = record;
                ColumnTexts = new List<string>();
            }
        }

        /// <summary>
        /// The event args for events that pass the checked state for a single record.
        /// </summary>
        public class RecordCheckedEventArgs : EventArgs
        {
            public object Record { get; private set; }

            public bool Checked { get; private set; }

            public RecordCheckedEventArgs(object record, bool checkedValue)
            {
                Record = record;
                Checked = checkedValue;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// True if the control is being populated. Some events are suppressed while this is happening.
        /// </summary>
        private bool _Populating;

        /// <summary>
        /// The currency manager that we're using.
        /// </summary>
        private CurrencyManager _CurrencyManager;

        /// <summary>
        /// The collection that we're bound to.
        /// </summary>
        private IBindingList _BindingList;

        /// <summary>
        /// True if the <see cref="CheckedSubset"/> list has been hooked. While it is hooked any changes
        /// to the subset list are reflected as changes in the Checked state of list view items.
        /// </summary>
        private bool _CheckedSubsetHooked;

        /// <summary>
        /// If true then changes in the checked state of list view items are not copied back to the
        /// <see cref="CheckedSubset"/> list.
        /// </summary>
        private bool _SuppressSyncListToCheckedSubset;
        #endregion

        #region Control Properties
        /// <summary>
        /// Gets or sets a value indicating that the user can add new records to the list.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowAdd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the user can update records in the list.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the user can remove records from the list.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowDelete { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating that all controls except the list are to be hidden.
        /// </summary>
        /// <remarks>
        /// In this mode the user control is set to be the error provider control, there's nowhere
        /// to display errors within the user control.
        /// </remarks>
        [DefaultValue(false)]
        public bool HideAllButList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the list should show check boxes.
        /// </summary>
        [DefaultValue(false)]
        public bool CheckBoxes
        {
            get { return listView.CheckBoxes; }
            set { listView.CheckBoxes = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the list should allow selects on any row.
        /// </summary>
        [DefaultValue(true)]
        public bool FullRowSelect
        {
            get { return listView.FullRowSelect; }
            set { listView.FullRowSelect = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating that the user can select more than one row at a time.
        /// </summary>
        [DefaultValue(true)]
        public bool MultiSelect
        {
            get { return listView.MultiSelect; }
            set { listView.MultiSelect = value; }
        }

        /// <summary>
        /// Gets the columns to show to the user.
        /// </summary>
        [Editor("System.Windows.Forms.Design.ColumnHeaderCollectionEditor", typeof(UITypeEditor))]
        [MergableProperty(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ListView.ColumnHeaderCollection Columns
        {
            get { return listView.Columns; }
        }

        /// <summary>
        /// Gets or sets the automatic sorting sort order.
        /// </summary>
        [DefaultValue(SortOrder.None)]
        public SortOrder Sorting
        {
            get { return listView.Sorting; }
            set { listView.Sorting = value; }
        }
        #endregion

        #region Binding Properties and hooks
        private object _DataSource;
        /// <summary>
        /// Gets or sets the list to bind the control to.
        /// </summary>
        public object DataSource
        {
            get { return _DataSource; }
            set
            {
                var list = value as IBindingList;
                if(value != null && list == null) throw new InvalidOperationException("Only IBindingList collections can be bound to BindingListViews");
                if(_DataSource != list) {
                    _DataSource = list;
                    SetDataBinding();
                    OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnBindingContextChanged(EventArgs e)
        {
            base.OnBindingContextChanged(e);
            SetDataBinding();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnParentBindingContextChanged(EventArgs e)
        {
            base.OnParentBindingContextChanged(e);
            SetDataBinding();
        }
        #endregion

        #region CheckedSubset properties
        private IBindingList _CheckedSubset;
        /// <summary>
        /// Gets or sets a binding list that is a subset of values that are derived from the list that we
        /// have bound the control to.
        /// </summary>
        /// <remarks>
        /// If this property is set then <see cref="ExtractSubsetValue"/> must also be supplied. The subset
        /// feature is ignored unless checkboxes are switched on for the list. Those items that are checked are
        /// added to the subset list, those items that are unchecked are removed. Items that are removed from
        /// the bound list are also removed from the subset. The Checked item in <see cref="FetchRecordContent"/>'s
        /// value is ignored when a subset is present because the Checked value is managed entirely by the
        /// control. To check an item you must add it to this list that CheckedSubset is pointing at.
        /// </remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IBindingList CheckedSubset
        {
            get { return _CheckedSubset; }
            set {
                UnhookCheckedSubset();
                if(_CheckedSubset != value) {
                    _CheckedSubset = value;
                    InitialiseCheckedSubset();
                }
            }
        }

        private Func<object, object> _ExtractSubsetValue;
        /// <summary>
        /// Gets or sets a delegate that can translate between the bound list values and a subset value. Only used
        /// when <see cref="CheckedSubset"/> has been set.
        /// </summary>
        /// <remarks>
        /// The value passed in is a value from the bound list, the value returned is the corresponding value in
        /// <see cref="CheckedSubset"/> (if any).
        /// </remarks>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<object, object> ExtractSubsetValue
        {
            get { return _ExtractSubsetValue; }
            set {
                if(_ExtractSubsetValue != value) {
                    _ExtractSubsetValue = value;
                    InitialiseCheckedSubset();
                }
            }
        }
        #endregion

        #region Other properties
        /// <summary>
        /// Gets or sets the selected record.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedRecord
        {
            get { var items = SelectedRecords.ToArray(); return items.Length == 0 ? null : items[0]; }
            set { SelectedRecords = value == null ? new object[]{} : new object[] { value }; }
        }

        /// <summary>
        /// Gets or sets the selected records.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<object> SelectedRecords
        {
            get { return GetAllSelectedListViewTag<object>(listView); }
            set { SelectListViewItemsByTags<object>(listView, value); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<object> CheckedRecords
        {
            get { return GetAllCheckedListViewTag<object>(listView); }
            set { CheckListViewItemsByTags<object>(listView, value); }
        }
        #endregion

        #region Events
        /// <summary>
        /// Raised when the <see cref="DataSource"/> is changed.
        /// </summary>
        public event EventHandler DataSourceChanged;

        /// <summary>
        /// Raises <see cref="DataSourceChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDataSourceChanged(EventArgs args)
        {
            if(DataSourceChanged != null) DataSourceChanged(this, args);
        }

        /// <summary>
        /// Raised to fetch the content of a row that represents a record.
        /// </summary>
        public event EventHandler<RecordContentEventArgs> FetchRecordContent;

        /// <summary>
        /// Raises <see cref="FetchRecordContent"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnFetchRecordContent(RecordContentEventArgs args)
        {
            if(FetchRecordContent != null) FetchRecordContent(this, args);
        }

        /// <summary>
        /// Raised when the selection changes.
        /// </summary>
        public event EventHandler SelectedRecordChanged;

        /// <summary>
        /// Raises <see cref="SelectedRecordChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnSelectedRecordChanged(EventArgs args)
        {
            if(SelectedRecordChanged != null) SelectedRecordChanged(this, args);
        }

        /// <summary>
        /// Raised when the add button is clicked.
        /// </summary>
        public event EventHandler AddClicked;

        /// <summary>
        /// Raises <see cref="AddClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnAddClicked(EventArgs args)
        {
            if(AddClicked != null && AllowAdd) AddClicked(this, args);
        }

        /// <summary>
        /// Raised when the delete button is clicked.
        /// </summary>
        public event EventHandler DeleteClicked;

        /// <summary>
        /// Raises <see cref="DeleteClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDeleteClicked(EventArgs args)
        {
            if(DeleteClicked != null && AllowDelete) DeleteClicked(this, args);
        }

        /// <summary>
        /// Raised when the user wants to edit a record.
        /// </summary>
        public event EventHandler EditClicked;

        /// <summary>
        /// Raises <see cref="EditClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnEditClicked(EventArgs args)
        {
            if(EditClicked != null && AllowUpdate) EditClicked(this, args);
        }

        /// <summary>
        /// Raised when the user changes the check state on an item.
        /// </summary>
        public event EventHandler<RecordCheckedEventArgs> CheckedChanged;

        /// <summary>
        /// Raises <see cref="CheckedChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCheckedChanged(RecordCheckedEventArgs args)
        {
            if(CheckedChanged != null) CheckedChanged(this, args);
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BindingListView()
        {
            InitializeComponent();
            AllowAdd = true;
            AllowUpdate = true;
            AllowDelete = true;
        }
        #endregion

        #region SetDataBinding
        /// <summary>
        /// Binds the control to the data source.
        /// </summary>
        /// <returns>True if the items were loaded by the method.</returns>
        private bool SetDataBinding()
        {
            var result = false;

            if(BindingContext != null) {
                var reloadItems = false;
                var currencyManager = DataSource == null ? null : BindingContext[DataSource] as CurrencyManager;
                var bindingList = currencyManager == null ? null : currencyManager.List as IBindingList;

                if(currencyManager != _CurrencyManager) {
                    if(_CurrencyManager != null) {
                        _CurrencyManager.MetaDataChanged -= CurrencyManager_MetaDataChanged;
                        _CurrencyManager.PositionChanged -= CurrencyManager_PositionChanged;
                        _CurrencyManager.ItemChanged -= CurrencyManager_ItemChanged;
                    }

                    _CurrencyManager = currencyManager;

                    if(_CurrencyManager != null) {
                        reloadItems = true;
                        _CurrencyManager.MetaDataChanged += CurrencyManager_MetaDataChanged;
                        _CurrencyManager.PositionChanged += CurrencyManager_PositionChanged;
                        _CurrencyManager.ItemChanged += CurrencyManager_ItemChanged;
                    }
                }

                if(bindingList != _BindingList) {
                    if(_BindingList != null) {
                        _BindingList.ListChanged -= BindingList_ListChanged;
                    }

                    _BindingList = bindingList;

                    if(_BindingList != null) {
                        reloadItems = true;
                        _BindingList.ListChanged += BindingList_ListChanged;
                    }
                }

                if(reloadItems) {
                    LoadItemsFromSource();
                    result = true;
                }
            }

            return result;
        }
        #endregion

        #region LoadItemsFromSource, CreateListViewItemForRecord, SetSelectedIndex
        /// <summary>
        /// Loads the items in the control from the bound list.
        /// </summary>
        private void LoadItemsFromSource()
        {
            var populating = _Populating;
            try {
                _Populating = true;

                listView.Items.Clear();

                var list = _CurrencyManager == null ? null : _CurrencyManager.List;
                if(list != null) {
                    for(var i = 0;i < list.Count;++i) {
                        listView.Items.Add(CreateListViewItemForRecord(list[i]));
                    }
                }

                if(_CurrencyManager != null && _CurrencyManager.Position != -1) {
                    SetSelectedIndex(_CurrencyManager.Position);
                }

                SynchroniseCheckedSubsetToList();
            } finally {
                _Populating = populating;
            }
        }

        /// <summary>
        /// Creates a ListViewItem for the record passed across.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private ListViewItem CreateListViewItemForRecord(object record)
        {
            var result = new ListViewItem() {
                Tag = record,
            };
            RefreshListViewItem(result);

            return result;
        }

        private bool _InSetSelectedIndex;
        /// <summary>
        /// Sets the selected row to the row at the index passed across.
        /// </summary>
        /// <param name="index"></param>
        private void SetSelectedIndex(int index)
        {
            if(!_InSetSelectedIndex) {
                _InSetSelectedIndex = true;
                try {
                    listView.SelectedItems.Clear();
                    var listViewItem = listView.Items.Count >= index ? null : listView.Items[index];
                    if(listViewItem != null) {
                        listViewItem.Selected = true;
                        listViewItem.EnsureVisible();
                    }
                } finally {
                    _InSetSelectedIndex = false;
                }
            }
        }

        /// <summary>
        /// Populates the list view item passed across.
        /// </summary>
        /// <param name="listViewItem"></param>
        protected void RefreshListViewItem(ListViewItem listViewItem)
        {
            var recordContent = new RecordContentEventArgs(listViewItem.Tag);
            OnFetchRecordContent(recordContent);

            var populating = _Populating;
            try {
                _Populating = true;

                if(CheckedSubset != null && ExtractSubsetValue != null) {
                    recordContent.Checked = CheckedSubsetContainsSubsetOfRecord(recordContent.Record);
                }

                if(CheckBoxes) FillAndCheckListViewItem<object>(listViewItem, r => recordContent.ColumnTexts.ToArray(), r => recordContent.Checked);
                else           FillListViewItem<object>(listViewItem, r => recordContent.ColumnTexts.ToArray());
            } finally {
                _Populating = populating;
            }
        }

        /// <summary>
        /// Checks or unchecks a record in the list view.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="ticked"></param>
        public void CheckRecord(object record, bool ticked)
        {
            var listViewItem = listView.Items.OfType<ListViewItem>().FirstOrDefault(r => Object.Equals(record, r.Tag));
            if(listViewItem != null) listViewItem.Checked = ticked;
        }
        #endregion

        #region EnableDisableControls
        /// <summary>
        /// Enables or disables controls based on the state of the object.
        /// </summary>
        private void EnableDisableControls()
        {
            buttonAdd.Enabled = AllowAdd;
            buttonDelete.Enabled = AllowDelete && SelectedRecord != null;
        }
        #endregion

        #region GetValidationDisplayControl
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="errorProvider"></param>
        /// <returns></returns>
        public Control GetValidationDisplayControl(ErrorProvider errorProvider)
        {
            var result = HideAllButList ? (Control)this : (Control)labelErrorAnchor;
            if(!HideAllButList) {
                errorProvider.SetIconAlignment(labelErrorAnchor, ErrorIconAlignment.BottomLeft);
            }

            return result;
        }
        #endregion

        #region CheckedSubset handling
        /// <summary>
        /// Hooks the CheckedSubset list if necessary and synchronises the checked state of the list
        /// items with the items in the subset.
        /// </summary>
        private void InitialiseCheckedSubset()
        {
            if(CheckedSubset != null && ExtractSubsetValue != null) {
                UnhookCheckedSubset();
                HookCheckedSubset();
                SynchroniseCheckedSubsetToList();
            }
        }

        /// <summary>
        /// Returns true if the <see cref="CheckedSubset"/> contains a value that
        /// corresponds to the record passed across.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private bool CheckedSubsetContainsSubsetOfRecord(object record)
        {
            var result = false;
            if(record != null && CheckedSubset != null && ExtractSubsetValue != null) {
                var subsetValue = ExtractSubsetValue(record);
                result = CheckedSubset.Contains(subsetValue);
            }

            return result;
        }

        /// <summary>
        /// Sets or clears the checked state of list view items to match the items present in
        /// the <see cref="CheckedSubset"/> list. Also removes any items from <see cref="CheckedSubset"/>
        /// that are not present in the list.
        /// </summary>
        private void SynchroniseCheckedSubsetToList()
        {
            if(CheckedSubset != null && ExtractSubsetValue != null) {
                var suppressSetting = _SuppressSyncListToCheckedSubset;
                _SuppressSyncListToCheckedSubset = true;
                UnhookCheckedSubset();
                try {
                    var notInMasterList = new LinkedList<object>();
                    if(_BindingList != null) {
                        foreach(var subsetValue in CheckedSubset) {
                            notInMasterList.AddLast(subsetValue);
                        }
                    }

                    foreach(var listViewItem in listView.Items.OfType<ListViewItem>().Where(r => r.Tag != null)) {
                        var boundItem = listViewItem.Tag;
                        var subsetValue = ExtractSubsetValue(boundItem);
                        var isInSubset = CheckedSubset.Contains(subsetValue);
                        listViewItem.Checked = CheckedSubset.Contains(subsetValue);

                        if(isInSubset) {
                            var node = notInMasterList.Find(subsetValue);
                            if(node != null) notInMasterList.Remove(node);
                        }
                    }

                    if(_BindingList != null) {
                        for(var node = notInMasterList.First;node != null;node = node.Next) {
                            CheckedSubset.Remove(node.Value);
                        }
                    }
                } finally {
                    _SuppressSyncListToCheckedSubset = suppressSetting;
                    HookCheckedSubset();
                }
            }
        }

        /// <summary>
        /// Hooks events on the <see cref="CheckedSubset"/> control.
        /// </summary>
        private void HookCheckedSubset()
        {
            if(_CheckedSubset != null && !_CheckedSubsetHooked) {
                _CheckedSubsetHooked = true;
                _CheckedSubset.ListChanged += checkedSubset_ListChanged;
            }
        }

        /// <summary>
        /// Unhooks events on the <see cref="CheckedSubset"/> control.
        /// </summary>
        private void UnhookCheckedSubset()
        {
            if(_CheckedSubset != null && _CheckedSubsetHooked) {
                _CheckedSubsetHooked = false;
                _CheckedSubset.ListChanged -= checkedSubset_ListChanged;
            }
        }
        #endregion

        #region OnLoad
        /// <summary>
        /// Called after the control has been created but before it is shown to the user.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                labelErrorAnchor.Text = "";
                labelErrorAnchor.Visible = true;
                labelErrorAnchor.BringToFront();

                var gap = buttonDelete.Left - buttonAdd.Left;
                buttonAdd.Left = 4;
                buttonDelete.Left = buttonAdd.Left + gap;

                buttonAdd.Image = Images.Add16x16;
                buttonDelete.Image = Images.Cancel16x16;
                buttonAdd.Text = buttonDelete.Text = "";

                if(HideAllButList) {
                    labelErrorAnchor.Visible = false;
                    buttonAdd.Visible = false;
                    buttonDelete.Visible = false;
                    listView.Dock = DockStyle.Fill;
                }

                EnableDisableControls();
            }
        }
        #endregion

        #region Events subscribed
        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
            OnSelectedRecordChanged(e);
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            OnAddClicked(e);
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            OnDeleteClicked(e);
        }

        private void checkedSubset_ListChanged(object sender, ListChangedEventArgs args)
        {
            SynchroniseCheckedSubsetToList();
        }

        private void listView_DoubleClick(object sender, EventArgs e)
        {
            if(SelectedRecord != null) OnEditClicked(e);
        }

        private void listView_KeyDown(object sender, KeyEventArgs e)
        {
            var haveSelectedRecord = SelectedRecord != null;

            if(e.Modifiers == Keys.None) {
                switch(e.KeyCode) {
                    case Keys.Insert:
                        OnAddClicked(e);
                        e.Handled = true;
                        break;
                    case Keys.Delete:
                        OnDeleteClicked(e);
                        e.Handled = true;
                        break;
                }
            }
        }

        private void listView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var record = e.Item == null ? null : e.Item.Tag;

            if(!_Populating) {
                if(record != null) {
                    var args = new RecordCheckedEventArgs(record, e.Item.Checked);
                    OnCheckedChanged(args);
                }

                if(!_SuppressSyncListToCheckedSubset && CheckedSubset != null && ExtractSubsetValue != null && record != null) {
                    var subsetValue = ExtractSubsetValue(record);
                    var isInSubset = CheckedSubset.Contains(subsetValue);
                    if(e.Item.Checked) {
                        if(!isInSubset) CheckedSubset.Add(subsetValue);
                    } else {
                        if(isInSubset)  CheckedSubset.Remove(subsetValue);
                    }
                }
            }
        }
        #endregion

        #region Binding events subscribed
        /// <summary>
        /// Called when the list we have bound to is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BindingList_ListChanged(object sender, ListChangedEventArgs args)
        {
            var populating = _Populating;
            try {
                _Populating = true;
                switch (args.ListChangedType) {
                    case ListChangedType.Reset:
                        LoadItemsFromSource();
                        break;
                    case ListChangedType.ItemChanged:
                        object changedRow = _CurrencyManager.List[args.NewIndex];
                        listView.BeginUpdate();
                        listView.Items[args.NewIndex] = CreateListViewItemForRecord(changedRow);
                        listView.EndUpdate();
                        break;
                    case ListChangedType.ItemAdded:
                        object newRow = _CurrencyManager.List[args.NewIndex];
                        var drv = newRow as DataRowView;
                        if(drv == null || !drv.IsNew) {
                            listView.BeginUpdate();
                            listView.Items.Insert(args.NewIndex, CreateListViewItemForRecord(newRow));
                            listView.EndUpdate();
                        }
                        break;
                    case ListChangedType.ItemDeleted:
                        if(args.NewIndex < listView.Items.Count) {
                            listView.Items.RemoveAt(args.NewIndex);
                        }
                        break;
                    case ListChangedType.ItemMoved:
                        listView.BeginUpdate();
                        var moving = listView.Items[args.OldIndex];
                        listView.Items.Insert(args.NewIndex, moving);
                        listView.EndUpdate();
                        break;
                    default:
                        LoadItemsFromSource();
                        break;
                }

                SynchroniseCheckedSubsetToList();
            } finally {
                _Populating = populating;
            }
        }

        /// <summary>
        /// Called when the metadata changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CurrencyManager_MetaDataChanged(object sender, EventArgs args)
        {
            LoadItemsFromSource();
        }

        /// <summary>
        /// Called when the user changes position within the source list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CurrencyManager_PositionChanged(object sender, EventArgs args)
        {
            if(_CurrencyManager != null) {
                SetSelectedIndex(_CurrencyManager.Position);
            }
        }

        /// <summary>
        /// Called when an item changes in the bound list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CurrencyManager_ItemChanged(object sender, ItemChangedEventArgs args)
        {
            // The code that I swiped was calling LoadItemsFromSource if the index was set to -1
            // but this causes problems. This event is called when the source list is a binding
            // list and the binding list had an item deleted. This event is raised after the 
            // delete with an index of -1, that causes the list to reload without the deleted
            // item and then the BindingList_ListChanged event is raised with the delete action,
            // which then deletes whatever item was at the index of the originally deleted item.
            //
            // I'm guessing he only ever bound to DataTables? Dunno. Either way, this is doing
            // the same thing as BindingList_ListChanged, only one of them should be allowed to
            // run... so I'm giving this one the chop.
            //if(args.Index == -1) {
            //    _BindingList.ListChanged -= new ListChangedEventHandler(BindingList_ListChanged);
            //    _BindingList = _CurrencyManager.List as IBindingList;
            //    if(_BindingList!= null) {
            //        _BindingList.ListChanged += new ListChangedEventHandler(BindingList_ListChanged);
            //    }
            //    LoadItemsFromSource();
            //}
        }
        #endregion
    }
}
