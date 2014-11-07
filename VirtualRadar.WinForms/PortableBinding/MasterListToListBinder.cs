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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Forms;
using VirtualRadar.Interface.PortableBinding;
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// A binder between a master list control and a list of objects.
    /// </summary>
    public class MasterListToListBinder<TModel, TListModel> : ControlBinder
        where TListModel: class
    {
        #region Private Class - Sorter
        /// <summary>
        /// The class that handles the sorting of the list view.
        /// </summary>
        class Sorter : AutoListViewSorter
        {
            /// <summary>
            /// The sort method. If this is null then no special sorting is required.
            /// </summary>
            private Func<TListModel, ColumnHeader, IComparable, IComparable> _GetSortValue;

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="control"></param>
            /// <param name="sortDelegate"></param>
            public Sorter(MasterListView control, Func<TListModel, ColumnHeader, IComparable, IComparable> sortDelegate) : base(control.ListView, showNativeSortIndicators: true)
            {
                _GetSortValue = sortDelegate;
            }

            /// <summary>
            /// See base docs.
            /// </summary>
            /// <param name="listViewItem"></param>
            /// <returns></returns>
            public override IComparable GetRowValue(ListViewItem listViewItem)
            {
                IComparable result = base.GetRowValue(listViewItem);

                if(_GetSortValue != null) {
                    var listModel = listViewItem.Tag as TListModel;
                    if(listModel != null) {
                        result = _GetSortValue(listModel, SortColumn, result);
                    }
                }

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The wrapper around the list.
        /// </summary>
        private GenericListWrapper<TListModel> _IList;

        /// <summary>
        /// True if we are not to copy changes to the master list to the control.
        /// </summary>
        private bool _MasterListUpdatesLocked;

        /// <summary>
        /// True if the model list has had its list changed event hooked.
        /// </summary>
        private bool _ModelListHooked;

        /// <summary>
        /// True if the control has been hooked.
        /// </summary>
        private bool _ControlHooked;

        /// <summary>
        /// The object that handles the sorting of rows.
        /// </summary>
        private Sorter _Sorter;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the model that owns the list.
        /// </summary>
        public TModel Model { get { return (TModel)ModelObject; } }

        /// <summary>
        /// Gets the control that is showing the list.
        /// </summary>
        public MasterListView Control { get { return (MasterListView)ControlObject; } }

        private Expression<Func<TModel, IList<TListModel>>> _GetModelListExpression;
        private Func<TModel, IList<TListModel>> _GetModelList;
        /// <summary>
        /// Gets the list held by the model.
        /// </summary>
        public IList<TListModel> ModelList
        {
            get { return _GetModelList(Model); }
        }

        /// <summary>
        /// Gets the <see cref="ModelList"/> as an IBindingList.
        /// </summary>
        public IBindingList ModelListBindingList
        {
            get { return ModelList as IBindingList; }
        }

        protected Action<TListModel, ListContentEventArgs> _FetchColumns;
        /// <summary>
        /// Gets or sets the method that handles the fetching of columns. Can only be set before initialisation.
        /// </summary>
        public Action<TListModel, ListContentEventArgs> FetchColumns
        {
            get { return _FetchColumns; }
            set { if(!Initialised) _FetchColumns = value; }
        }

        private Func<TListModel> _AddHandler;
        /// <summary>
        /// Gets or sets the method that handles the adding of new items. Can only be set before initialisation.
        /// </summary>
        /// <remarks>
        /// The method should return a new object to add to the list. If it returns null then no add is performed.
        /// If you want to insert the new object anywhere but the end of the list then insert it yourself in the
        /// handler and return null.
        /// </remarks>
        public Func<TListModel> AddHandler
        {
            get { return _AddHandler; }
            set { if(!Initialised) _AddHandler = value; }
        }

        private bool _AutoAddEnabled;
        /// <summary>
        /// Gets or sets a value indicating that if <see cref="AddHandler"/> is null then an automatic add handler should be used.
        /// </summary>
        /// <remarks>
        /// The auto-add handler creates a new object using the default ctor and adds it to the list.
        /// </remarks>
        public bool AutoAddEnabled
        {
            get { return _AutoAddEnabled; }
            set { if(!Initialised) _AutoAddEnabled = value; }
        }

        private Action<IList<TListModel>> _DeleteHandler;
        /// <summary>
        /// Gets or sets the method that handles the removal of existing items. Can only be set before initialisation.
        /// </summary>
        public Action<IList<TListModel>> DeleteHandler
        {
            get { return _DeleteHandler; }
            set { if(!Initialised) _DeleteHandler = value; }
        }

        private bool _AutoDeleteEnabled;
        /// <summary>
        /// Gets or sets a value indicating that if <see cref="DeleteHandler"/> is null then an automatic delete handler should be used.
        /// </summary>
        /// <remarks>
        /// The auto-delete handler simply removes the model from the list.
        /// </remarks>
        public bool AutoDeleteEnabled
        {
            get { return _AutoDeleteEnabled; }
            set { if(!Initialised) _AutoDeleteEnabled = value; }
        }

        private Action<TListModel> _EditHandler;
        /// <summary>
        /// Gets or sets the method that handles the editing of an existing item. Can only be set before initialisation.
        /// </summary>
        public Action<TListModel> EditHandler
        {
            get { return _EditHandler; }
            set { if(!Initialised) _EditHandler = value; }
        }

        protected Action<TListModel, bool> _CheckedChangedHandler;
        /// <summary>
        /// Gets or sets the method that handles the change in checked state for an existing item. Can only be set before initialisation.
        /// </summary>
        public virtual Action<TListModel, bool> CheckedChangedHandler
        {
            get { return _CheckedChangedHandler; }
            set { if(!Initialised) _CheckedChangedHandler = value; }
        }

        private bool _EnableSorting = true;
        /// <summary>
        /// Gets or sets a value indicating that the rows are to be sorted on display. Can only be set before initialisation.
        /// </summary>
        public bool EnableSorting
        {
            get { return _EnableSorting; }
            set { if(!Initialised) _EnableSorting = value; }
        }

        private Func<TListModel, ColumnHeader, IComparable, IComparable> _GetSortValue;
        /// <summary>
        /// Gets or sets a delegate that returns a comparable value from the list model for a single column.
        /// </summary>
        /// <remarks>
        /// The parameters are list model, followed by the header for the column that we're sorting on,
        /// followed by the default value for the list model (usually the result of ToString). The method
        /// should return a comparable. If it doesn't have any special interest in the column then return
        /// the comparable that was passed in.
        /// </remarks>
        public Func<TListModel, ColumnHeader, IComparable, IComparable> GetSortValue
        {
            get { return _GetSortValue; }
            set { if(!Initialised) _GetSortValue = value; }
        }
        #endregion

        #region ControlBinder Properties - ModelValueObject, ControlValueObject
        /// <summary>
        /// See base docs.
        /// </summary>
        public override object ModelValueObject
        {
            get             { return ModelList; }
            protected set   { throw new InvalidOperationException("Attempt made to set the ModelValueObject for a list"); }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        public override object ControlValueObject
        {
            get             { return Control.AllRecords; }
            protected set   { throw new InvalidOperationException("Attempt made to set the ControlValue for a list"); }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="control"></param>
        /// <param name="getModelList"></param>
        public MasterListToListBinder(TModel model, MasterListView control, Expression<Func<TModel, IList<TListModel>>> getModelList)
            : base(model, control)
        {
            _GetModelListExpression = getModelList;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                if(_Sorter != null) {
                    _Sorter.Dispose();
                    Control.ListView.ListViewItemSorter = null;
                }

                if(_ControlHooked) {
                    Control.CheckedChanged -= Control_CheckedChanged;
                    Control.EditClicked -= Control_EditClicked;
                    Control.DeleteClicked -= Control_DeleteClicked;
                    Control.AddClicked -= Control_AddClicked;
                    Control.FetchRecordContent -= Control_FetchRecordContent;
                    _ControlHooked = false;
                }

                if(_ModelListHooked) {
                    ModelListBindingList.ListChanged -= ModelListBindingList_ListChanged;
                    _ModelListHooked = false;
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialising()
        {
            if(FetchColumns == null) throw new InvalidOperationException("No FetchColumns method specified");

            _GetModelList = _GetModelListExpression.Compile();

            if(ModelListBindingList != null) {
                ModelListBindingList.ListChanged += ModelListBindingList_ListChanged;
                _ModelListHooked = true;
            }

            if(AutoAddEnabled && AddHandler == null) AddHandler = () => Activator.CreateInstance<TListModel>();
            if(AddHandler != null) Control.AllowAdd = true;

            if(AutoDeleteEnabled && DeleteHandler == null) DeleteHandler = (selectedModels) => {
                foreach(var selectedModel in selectedModels) {
                    ModelList.Remove(selectedModel);
                }
            };
            if(DeleteHandler != null) Control.AllowDelete = true;

            if(EditHandler != null) Control.AllowUpdate = true;
            if(CheckedChangedHandler != null) Control.CheckBoxes = true;

            Control.FetchRecordContent += Control_FetchRecordContent;
            Control.AddClicked += Control_AddClicked;
            Control.DeleteClicked += Control_DeleteClicked;
            Control.EditClicked += Control_EditClicked;
            Control.CheckedChanged += Control_CheckedChanged;
            _ControlHooked = true;

            if(EnableSorting) {
                _Sorter = new Sorter(Control, _GetSortValue);
                Control.ListView.ListViewItemSorter = _Sorter;
            }

            base.DoInitialising();
        }
        #endregion

        #region CopyModelToControl, CopyControlToModel
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyModelToControl()
        {
            if(!_MasterListUpdatesLocked) {
                _MasterListUpdatesLocked = true;
                try {
                    var modelList = ModelList;
                    if(_IList == null || !Object.ReferenceEquals(modelList, _IList.GenericList)) {
                        _IList = new GenericListWrapper<TListModel>(modelList);
                    }
                    Control.List = _IList;
                    Control.RefreshList();
                } finally {
                    _MasterListUpdatesLocked = false;
                }
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyControlToModel()
        {
            // This is a one-way binder from model to control. There is no copying
            // from the control back to the model.
        }
        #endregion

        #region DoHook/UnhookControlPropertyChanged
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="eventHandler"></param>
        protected override void DoHookControlPropertyChanged(EventHandler eventHandler)
        {
            ;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoUnhookControlPropertyChanged(EventHandler eventHandler)
        {
            ;
        }
        #endregion

        #region AddListModel, DeleteListModel, EditListModel
        /// <summary>
        /// Calls the add handler, returns the list model that it creates.
        /// </summary>
        /// <returns></returns>
        protected TListModel AddListModel()
        {
            TListModel result = null;

            if(AddHandler != null) {
                result = AddHandler();
                if(result != null) {
                    ModelList.Add(result);
                    Control.SelectedRecord = result;
                }
            }

            return result;
        }

        /// <summary>
        /// Calls the delete handler with the list model passed across.
        /// </summary>
        /// <param name="listModels"></param>
        protected void DeleteListModel(IList<TListModel> listModels)
        {
            if(DeleteHandler != null && listModels != null && listModels.Count > 0) {
                DeleteHandler(listModels);
            }
        }

        /// <summary>
        /// Calls the edit handler with the list model passed across.
        /// </summary>
        /// <param name="listModel"></param>
        protected void EditListModel(TListModel listModel)
        {
            if(EditHandler != null && listModel != null) {
                EditHandler(listModel);
            }
        }
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when the model list changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ModelListBindingList_ListChanged(object sender, ListChangedEventArgs args)
        {
            CopyModelToControl();
        }

        /// <summary>
        /// Called whent the control wants text for the columns for a record.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_FetchRecordContent(object sender, ListContentEventArgs e)
        {
            var listModel = e.Record as TListModel;
            if(listModel != null) {
                FetchColumns(listModel, e);
            }
        }

        /// <summary>
        /// Called when the user clicks the Add button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_AddClicked(object sender, EventArgs e)
        {
            var newModel = AddListModel();
            EditListModel(newModel);
        }

        /// <summary>
        /// Called when the user changes the checked state of an item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_CheckedChanged(object sender, ListCheckedEventArgs e)
        {
            var listModel = e.Record as TListModel;
            if(CheckedChangedHandler != null && listModel != null) {
                CheckedChangedHandler(listModel, e.Checked);
            }
        }

        /// <summary>
        /// Called when the user clicks the Delete button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Control_DeleteClicked(object sender, EventArgs e)
        {
            DeleteListModel(Control.SelectedRecords.OfType<TListModel>().ToList());
        }

        /// <summary>
        /// Called when the user clicks the Edit button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_EditClicked(object sender, EventArgs e)
        {
            EditListModel(Control.SelectedRecord as TListModel);
        }
        #endregion
    }
}
