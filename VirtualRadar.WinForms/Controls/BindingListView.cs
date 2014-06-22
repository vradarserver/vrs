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

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A list view that deals with the common problems with using a list view to maintain a list
    /// of objects.
    /// </summary>
    public partial class BindingListView : BaseUserControl
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
            var selectedRecords = SelectedRecords;

            listView.Items.Clear();
            foreach(var item in list) {
                var listViewItem = new ListViewItem() {
                    Tag = item,
                };
                RefreshListViewItem(listViewItem);
                listView.Items.Add(listViewItem);
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

            if(CheckBoxes) FillAndCheckListViewItem<object>(listViewItem, r => recordContent.ColumnTexts.ToArray(), r => recordContent.Checked);
            else           FillListViewItem<object>(listViewItem, r => recordContent.ColumnTexts.ToArray());
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
            }
        }
        #endregion
    }
}
