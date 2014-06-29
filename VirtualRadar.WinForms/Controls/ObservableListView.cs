using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualRadar.WinForms.Binding;
using System.ComponentModel;
using System.Collections;

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// Sits on top of a <see cref="BindingListView"/>. The list view shows a full set of records
    /// and this exposes a subset of those which the user selects by ticking the desired records.
    /// Both the master list is an observable list and updates the list view automatically. The
    /// checked items list is not observable, you need to set a binder on it.
    /// </summary>
    public class ObservableListView : BindingListView
    {
        /// <summary>
        /// True if <see cref="MasterList"/> is hooked.
        /// </summary>
        private bool _HookedMasterList;

        private IObservableList _MasterList;
        /// <summary>
        /// Gets or sets the list of records to show to the user. The <see cref="CheckedItemsList"/>
        /// is either a subset of these records or the identifiers extracted from a subset of these
        /// records.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IObservableList MasterList
        {
            get { return _MasterList; }
            set {
                UnhookLists();

                _MasterList = value;
                Populate();

                HookLists();
            }
        }

        private IList _AssignedCheckedRecords;
        /// <summary>
        /// Gets or sets the list of checked items, where the list is a subset of the
        /// <see cref="MasterList"/>. The list could also be the unique identifiers of the
        /// selected <see cref="MasterList"/> records, but only if the mapping functions are
        /// supplied.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IList CheckedItemsList
        {
            get {
                var checkedRecords = CheckedRecords;
                return checkedRecords.Select(r => MapFromRecordToCheckedItem(r)).Where(r => r != null).ToList();
            }
            set {
                if(value != null && MasterList != null) {
                    UnhookLists();

                    var checkedRecords = new List<object>();
                    foreach(var checkedItemValue in value) {
                        var record = MapFromCheckedItemToRecord(checkedItemValue);
                        if(record != null) checkedRecords.Add(record);
                    }
                    CheckedRecords = checkedRecords;
                    _AssignedCheckedRecords = CheckedRecords.ToArray();

                    HookLists();

                    OnCheckedItemsListChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Maps from a record in <see cref="MasterList"/> to a value to place into
        /// <see cref="CheckedItemsList"/>. By default there is no mapping and the objects in
        /// <see cref="CheckedItemsList"/> are straight from <see cref="MasterList"/>.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<object, object> MapFromRecordToCheckedItem;

        /// <summary>
        /// Maps from an item in <see cref="CheckedItemsList"/> to a record in <see cref="MasterList"/>.
        /// Only supply this if you supply a function for <see cref="MapFromRecordToCheckedItem"/>.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Func<object, object> MapFromCheckedItemToRecord;

        // New versions of the underlying Allow properties - we don't want to support modifying the master
        // list, it might interfere with our own stuff.
        [DefaultValue(false)]
        public new bool AllowAdd
        {
            get { return base.AllowAdd; }
            set { base.AllowAdd = value; }
        }

        [DefaultValue(false)]
        public new bool AllowUpdate
        {
            get { return base.AllowUpdate; }
            set { base.AllowUpdate = value; }
        }

        [DefaultValue(false)]
        public new bool AllowDelete
        {
            get { return base.AllowDelete; }
            set { base.AllowDelete = value; }
        }

        [DefaultValue(true)]
        public new bool HideAllButList
        {
            get { return base.HideAllButList; }
            set { base.HideAllButList = value; }
        }

        [DefaultValue(true)]
        public new bool CheckBoxes
        {
            get { return base.CheckBoxes; }
            set { base.CheckBoxes = value; }
        }

        /// <summary>
        /// Raised when the checked items list is changed.
        /// </summary>
        public event EventHandler CheckedItemsListChanged;
        protected virtual void OnCheckedItemsListChanged(EventArgs args)
        {
            if(CheckedItemsListChanged != null) CheckedItemsListChanged(this, args);
        }

        /// <summary>
        ///  Creates a new object.
        /// </summary>
        public ObservableListView() : base()
        {
            AllowAdd = AllowDelete = AllowUpdate = false;
            HideAllButList = CheckBoxes = true;

            MapFromCheckedItemToRecord = r => r;
            MapFromRecordToCheckedItem = r => r;
        }

        /// <summary>
        /// Populates the list view with records from the master list.
        /// </summary>
        protected virtual void Populate()
        {
            PopulateWithList(MasterList.GetValue() as IList);
        }

        /// <summary>
        /// Refreshes the content of the list.
        /// </summary>
        public virtual void RefreshContent()
        {
            UnhookLists();

            Populate();
            SynchroniseTicks(suppressEvents: true); // The checkmarks shouldn't have changed, in principle we're just refreshing the column texts

            HookLists();
        }

        /// <summary>
        /// Ensures that the checkmarks on the list view items correspond to the check marks that we
        /// think should be set.
        /// </summary>
        /// <param name="suppressEvents"></param>
        protected void SynchroniseTicks(bool suppressEvents)
        {
            UnhookLists();

            // If we have an assigned list of checked items (this gets updated when the checked items list
            // is assigned or when the user ticks items) then we need to ensure those items are re-ticked
            // on the list. We cannot leave it up to the code that fills list view items to keep the ticks
            // up-to-date.
            if(_AssignedCheckedRecords != null && _MasterList != null) {
                var masterList = _MasterList.GetListValue().OfType<object>().ToArray();
                var checkedRecords = new List<object>();
                foreach(var assignedCheckedRecord in _AssignedCheckedRecords) {
                    var checkedRecord = masterList.FirstOrDefault(r => Object.Equals(r, assignedCheckedRecord));
                    if(checkedRecord != null) checkedRecords.Add(checkedRecord);
                }

                CheckedRecords = checkedRecords;
            }

            HookLists();

            if(!suppressEvents) OnCheckedItemsListChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Hooks the lists so that we can synchronise changes from the source lists to the control.
        /// </summary>
        private void HookLists()
        {
            if(!_HookedMasterList) {
                _HookedMasterList = true;

                if(MasterList != null) MasterList.Changed += MasterList_Changed;
                CheckedChanged += ListView_CheckedChanged;
            }
        }

        /// <summary>
        /// Reverses the hooking that <see cref="HookLists"/> performed.
        /// </summary>
        private void UnhookLists()
        {
            if(_HookedMasterList) {
                _HookedMasterList = false;

                if(MasterList != null) MasterList.Changed -= MasterList_Changed;
                CheckedChanged -= ListView_CheckedChanged;
            }
        }

        /// <summary>
        /// Called when the master list changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MasterList_Changed(object sender, EventArgs args)
        {
            SynchroniseTicks(suppressEvents: false);
        }

        /// <summary>
        /// Called when an item is checked in the list view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ListView_CheckedChanged(object sender, RecordCheckedEventArgs args)
        {
            _AssignedCheckedRecords = CheckedRecords.ToArray();
            OnCheckedItemsListChanged(EventArgs.Empty);
        }
    }
}
