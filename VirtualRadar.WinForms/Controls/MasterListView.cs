using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.View;
using VirtualRadar.Resources;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A list view that can be used to maintain a list of objects. It does not automatically
    /// bind to any lists, it needs to be told whenever there is a change in the list of
    /// objects.
    /// </summary>
    public partial class MasterListView : BaseUserControl, IValidateDelegate
    {
        #region Fields
        /// <summary>
        /// True when values are being copied from the list into the control. During this process we
        /// want to suppress changes to the checked state of the items, they are redundant and cause
        /// a bit of a firestorm.
        /// </summary>
        private bool _Populating;

        /// <summary>
        /// Some events are unsafe to hook before the control has loaded. This field is set to true
        /// once those events have been hooked.
        /// </summary>
        private bool _ListViewHooked;
        #endregion

        #region Record properties
        private IList _List;
        /// <summary>
        /// The list that we are displaying to the user.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList List
        {
            get { return _List; }
            set {
                if(_List != value) {
                    _List = value;
                    RefreshList();
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected record.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object SelectedRecord
        {
            get { var items = SelectedRecords.ToArray(); return items.Length == 0 ? null : items[0]; }
            set {
                SelectedRecords = value == null ? new object[]{} : new object[] { value };
                if(listView.SelectedItems.Count != 0) listView.SelectedItems[0].EnsureVisible();
            }
        }

        /// <summary>
        /// Returns a collection of every item currently displayed within the control. This may not
        /// correspond to <see cref="List"/> if the list has been modified since <see cref="RefreshList"/>
        /// was last called.
        /// </summary>
        public IEnumerable<object> AllRecords
        {
            get { return listView.Items.OfType<ListViewItem>().Select(r => r.Tag).ToArray(); }
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

        /// <summary>
        /// Gets or sets a collection of records that have been checked in the list view.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable<object> CheckedRecords
        {
            get { return GetAllCheckedListViewTag<object>(listView); }
            set { CheckListViewItemsByTags<object>(listView, value); }
        }
        #endregion

        #region Behavioural properties
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
        #endregion

        #region Underlying control properties
        /// <summary>
        /// Gets the underlying list view. Avoid using this unless absolutely necessary.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ListView ListView { get { return listView; } }

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
        #endregion

        #region Events exposed
        /// <summary>
        /// Raised to fetch the content of a row that represents a record.
        /// </summary>
        public event EventHandler<ListContentEventArgs> FetchRecordContent;

        /// <summary>
        /// Raises <see cref="FetchRecordContent"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnFetchRecordContent(ListContentEventArgs args)
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
        public event EventHandler<ListCheckedEventArgs> CheckedChanged;

        /// <summary>
        /// Raises <see cref="CheckedChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCheckedChanged(ListCheckedEventArgs args)
        {
            if(CheckedChanged != null) CheckedChanged(this, args);
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MasterListView()
        {
            InitializeComponent();
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
                buttonEdit.Left = 4;
                buttonAdd.Left = buttonEdit.Left + gap;
                buttonDelete.Left = buttonAdd.Left + gap;

                buttonEdit.Image = Images.Edit16x16;
                buttonAdd.Image = Images.Add16x16;
                buttonDelete.Image = Images.Cancel16x16;
                buttonAdd.Text = buttonDelete.Text = "";

                if(HideAllButList) {
                    labelErrorAnchor.Visible = false;
                    buttonEdit.Visible = false;
                    buttonAdd.Visible = false;
                    buttonDelete.Visible = false;
                    listView.Dock = DockStyle.Fill;
                }

                HookListView();

                EnableDisableControls();
            }
        }
        #endregion

        #region HookListView, UnhookListView
        /// <summary>
        /// Hooks the list view if it has not already been hooked.
        /// </summary>
        /// <remarks>
        /// Some listview events are hooked through the designer. However some events are unsafe
        /// to hook before the control has loaded (especially ItemChecked) and it is these that
        /// are hooked here.
        /// </remarks>
        protected void HookListView()
        {
            if(!_ListViewHooked) {
                listView.ItemChecked += listView_ItemChecked;
                _ListViewHooked = true;
            }
        }

        /// <summary>
        /// Unhooks the events hooked by <see cref="HookListView"/>.
        /// </summary>
        protected void UnhookListView()
        {
            if(_ListViewHooked) {
                listView.ItemChecked -= listView_ItemChecked;
                _ListViewHooked = false;
            }
        }
        #endregion

        #region EnableDisableControls
        /// <summary>
        /// Enables or disables controls based on the state of the object.
        /// </summary>
        private void EnableDisableControls()
        {
            buttonEdit.Enabled = AllowUpdate;
            buttonAdd.Enabled = AllowAdd;
            buttonDelete.Enabled = AllowDelete && SelectedRecord != null;
        }
        #endregion

        #region RefreshList, RefreshRecord
        /// <summary>
        /// Refreshes the content of the list, removing items that are no longer within
        /// <see cref="List"/> and adding new items. Applies sort after list refreshed.
        /// </summary>
        public virtual void RefreshList()
        {
            if(!_Populating) {
                _Populating = true;

                try {
                    foreach(var listViewItem in listView.Items.OfType<ListViewItem>().ToArray()) {
                        var record = listViewItem.Tag;
                        if(List == null || !List.Contains(record)) listView.Items.Remove(listViewItem);
                    }

                    if(List != null) {
                        foreach(var item in List) {
                            RefreshRecord(item, applySort: false);
                        }

                        if(listView.ListViewItemSorter != null) {
                            listView.Sort();
                        }
                    }
                } finally {
                    _Populating = false;
                }
            }
        }

        /// <summary>
        /// Refreshes the display of the record in the list. Applies sort after item refreshed.
        /// </summary>
        /// <param name="record"></param>
        public virtual void RefreshRecord(object record)
        {
            if(List != null && List.Contains(record)) {
                RefreshRecord(record, applySort: true);
            }
        }

        /// <summary>
        /// Refreshes the record, adding it if not already present. Can optionally sort the
        /// list view once the record is in place / refreshed. Does not check to make sure
        /// the record is in the list.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="applySort"></param>
        protected virtual void RefreshRecord(object record, bool applySort)
        {
            var listViewItem = listView.Items.OfType<ListViewItem>().FirstOrDefault(r => Object.Equals(r.Tag, record));
            var addListViewItem = listViewItem == null;
                    
            if(addListViewItem) {
                listViewItem = new ListViewItem() {
                    Tag = record,
                };
            }
            RefreshListViewItem(listViewItem);

            if(addListViewItem) listView.Items.Add(listViewItem);

            if(applySort && listView.ListViewItemSorter != null) {
                listView.Sort();
            }
        }

        /// <summary>
        /// Refreshes the content of the list view item. The list view item need not be contained
        /// within the list view itself, it can be detached.
        /// </summary>
        /// <param name="listViewItem"></param>
        protected virtual void RefreshListViewItem(ListViewItem listViewItem)
        {
            var recordContent = new ListContentEventArgs(listViewItem.Tag);
            OnFetchRecordContent(recordContent);

            if(CheckBoxes) FillAndCheckListViewItem<object>(listViewItem, r => recordContent.ColumnTexts.ToArray(), r => recordContent.Checked);
            else           FillListViewItem<object>(listViewItem, r => recordContent.ColumnTexts.ToArray());
        }
        #endregion

        #region SetRecordChecked
        /// <summary>
        /// Sets the listview item for a particular record to be checked or unchecked.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="setChecked"></param>
        public void SetRecordChecked(object record, bool setChecked)
        {
            if(List != null && List.Contains(record)) {
                var listViewItem = listView.Items.OfType<ListViewItem>().FirstOrDefault(r => Object.Equals(record, r.Tag));
                if(listViewItem != null) {
                    listViewItem.Checked = setChecked;
                }
            }
        }
        #endregion

        #region GetValidationDisplayControl
        /// <summary>
        /// Exposes the error anchor as the point where error providers are to be shown rather than the
        /// user control. If we are hiding all but the list then the error provider is anchored to the
        /// list.
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

        #region Events subscribed
        /// <summary>
        /// Called when the add button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            OnAddClicked(e);
        }

        /// <summary>
        /// Called when the delete button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            OnDeleteClicked(e);
        }

        /// <summary>
        /// Called when the edit button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEdit_Click(object sender, EventArgs e)
        {
            OnEditClicked(e);
        }

        /// <summary>
        /// Called when the user double-clicks an item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_DoubleClick(object sender, EventArgs e)
        {
            if(SelectedRecord != null) OnEditClicked(e);
        }

        /// <summary>
        /// Called when an item's checked state changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if(!_Populating) {
                OnCheckedChanged(new ListCheckedEventArgs(e.Item.Tag, e.Item.Checked));
            }
        }

        /// <summary>
        /// Called when the user presses a key with the list view in focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Called when the list view selection changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
            OnSelectedRecordChanged(e);
        }
        #endregion
    }
}
