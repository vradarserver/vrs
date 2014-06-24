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

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A list view that deals with the common problems with using a list view to maintain a list
    /// of objects.
    /// </summary>
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

        // The X axis locations of the buttons when an error is being shown.
        public int _ButtonAddErrorX;
        public int _ButtonDeleteErrorX;
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

        #region Other properties
        /// <summary>
        /// Gets or sets the list of records in the list view.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList Records
        {
            get
            {
                return listView.Items.OfType<ListViewItem>().Select(r => r.Tag).ToList();
            }
            set
            {
                PopulateWithList(value);
            }
        }

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
        #endregion

        #region Events
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

        #region PopulateWithList
        /// <summary>
        /// Populates the control with the contents of a list. Attempts to preserve the
        /// selection across the population.
        /// </summary>
        /// <param name="list"></param>
        public void PopulateWithList(IList list)
        {
            var selectedRecords = SelectedRecords.ToArray();
            var selectIndex = selectedRecords.Length == 1 ? listView.SelectedIndices[0] : -1;

            var populating = _Populating;
            try {
                _Populating = true;

                listView.Items.Clear();
                foreach(var item in list) {
                    var listViewItem = new ListViewItem() {
                        Tag = item,
                    };
                    RefreshListViewItem(listViewItem);
                    listView.Items.Add(listViewItem);

                    if(selectedRecords.Contains(item)) {
                        listViewItem.Selected = true;
                    }
                }

                if(selectedRecords.Length == 1 && listView.SelectedIndices.Count == 0 && selectIndex > -1) {
                    if(selectIndex < listView.Items.Count) listView.Items[selectIndex].Selected = true;
                    else if(listView.Items.Count > 0) listView.Items[listView.Items.Count - 1].Selected = true;
                }
            } finally {
                _Populating = populating;
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

                if(CheckBoxes) FillAndCheckListViewItem<object>(listViewItem, r => recordContent.ColumnTexts.ToArray(), r => recordContent.Checked);
                else           FillListViewItem<object>(listViewItem, r => recordContent.ColumnTexts.ToArray());
            } finally {
                _Populating = populating;
            }
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
            errorProvider.SetIconAlignment(labelErrorAnchor, ErrorIconAlignment.BottomLeft);
            return labelErrorAnchor;
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
            if(!_Populating) {
                var record = e.Item == null ? null : e.Item.Tag;
                if(record != null) {
                    var args = new RecordCheckedEventArgs(record, e.Item.Checked);
                    OnCheckedChanged(args);
                }
            }
        }
        #endregion
    }
}
