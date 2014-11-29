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
using VirtualRadar.WinForms.Controls;

namespace VirtualRadar.WinForms.PortableBinding
{
    /// <summary>
    /// A binder that binds between a master list and a subset of the master list's
    /// rows. Rows are distinguished by an identifier.
    /// </summary>
    /// <typeparam name="TSubsetModel">The type of object that holds the subset list, the list of identifiers.</typeparam>
    /// <typeparam name="TMasterModel">The type of object that holds the master list.</typeparam>
    /// <typeparam name="TListModel">The type of object in the master list.</typeparam>
    /// <typeparam name="TIdentifier">The type of identifier.</typeparam>
    public class MasterListToSubsetBinder<TSubsetModel, TMasterModel, TListModel, TIdentifier> : MasterListToListBinder<TMasterModel, TListModel>
        where TListModel: class
    {
        #region Fields
        /// <summary>
        /// The base method to fetch columns. We chain onto this.
        /// </summary>
        protected Action<TListModel, ListContentEventArgs> _BaseFetchColumns;

        /// <summary>
        /// True if we have hooked the list subset identifiers.
        /// </summary>
        private bool _HookedSubsetList;
        #endregion

        #region Properties
        private TSubsetModel _SubsetModel;
        /// <summary>
        /// Gets or sets the object that holds the subset list of identifiers.
        /// </summary>
        public TSubsetModel SubsetModel
        {
            get { return _SubsetModel; }
            set { if(!Initialised) _SubsetModel = value; }
        }

        private Expression<Func<TSubsetModel, IList<TIdentifier>>> _GetSubsetListExpression;
        private Func<TSubsetModel, IList<TIdentifier>> _GetSubsetListAction;
        /// <summary>
        /// Gets or sets the action that retrieves the subset list from the subset model.
        /// </summary>
        public Func<TSubsetModel, IList<TIdentifier>> GetSubsetAction
        {
            get { return _GetSubsetListAction; }
            set { if(!Initialised) _GetSubsetListAction = value; }
        }

        private Expression<Func<TListModel, TIdentifier>> _GetListIdentifierExpression;
        private Func<TListModel, TIdentifier> _GetListIdentifierAction;
        /// <summary>
        /// Gets or sets the action that retrieves the identifier from a row list item.
        /// </summary>
        public Func<TListModel, TIdentifier> GetListIdentifierAction
        {
            get { return _GetListIdentifierAction; }
            set { if(!Initialised) _GetListIdentifierAction = value; }
        }

        /// <summary>
        /// See base docs. Throws exception if you try to set it.
        /// </summary>
        public override Action<TListModel, bool> CheckedChangedHandler
        {
            get { return base.CheckedChangedHandler; }
            set { throw new InvalidOperationException("You cannot set the CheckedChangedHandler on the subset binder"); }
        }

        /// <summary>
        /// Gets the list of subset identifiers.
        /// </summary>
        public IList<TIdentifier> ModelSubsetList
        {
            get { return _GetSubsetListAction(SubsetModel); }
        }

        /// <summary>
        /// Gets the list of subset identitifers as an IBindingList.
        /// </summary>
        public IBindingList ModelSubsetBindingList
        {
            get { return ModelSubsetList as IBindingList; }
        }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="subsetModel">The object that holds the subset list.</param>
        /// <param name="control">The <see cref="MasterListView"/> control that we'll be binding to.</param>
        /// <param name="masterModel">The object that holds the master list.</param>
        /// <param name="getMasterList">A delegate that returns the master list.</param>
        /// <param name="getSubsetList">A delegate that returns the subset list.</param>
        /// <param name="getListIdentifier">A delegate that returns the identifier from a list item.</param>
        public MasterListToSubsetBinder(TSubsetModel subsetModel, MasterListView control, TMasterModel masterModel, 
            Expression<Func<TSubsetModel, IList<TIdentifier>>> getSubsetList,
            Expression<Func<TMasterModel, IList<TListModel>>> getMasterList,
            Expression<Func<TListModel, TIdentifier>> getListIdentifier)
            : base(masterModel, control, getMasterList)
        {
            _SubsetModel = subsetModel;
            _GetSubsetListExpression = getSubsetList;
            _GetListIdentifierExpression = getListIdentifier;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See base class docs.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if(disposing) {
                UnhookSubsetList();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Initialise, HookSubsetList, UnhookSubsetList
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialising()
        {
            _GetSubsetListAction = _GetSubsetListExpression.Compile();
            _GetListIdentifierAction = _GetListIdentifierExpression.Compile();

            _BaseFetchColumns = _FetchColumns;
            _FetchColumns = Chain_FetchColumns;
            _CheckedChangedHandler = Base_CheckedChangedHandler;

            HookSubsetList();

            base.DoInitialising();
        }

        /// <summary>
        /// Hooks the subset list.
        /// </summary>
        private void HookSubsetList()
        {
            if(!_HookedSubsetList && ModelSubsetBindingList != null) {
                _HookedSubsetList = true;
                ModelSubsetBindingList.ListChanged += SubsetList_ListChanged;
            }
        }

        /// <summary>
        /// Unhooks the subset list.
        /// </summary>
        private void UnhookSubsetList()
        {
            if(_HookedSubsetList && ModelSubsetBindingList != null) {
                _HookedSubsetList = false;
                ModelSubsetBindingList.ListChanged -= SubsetList_ListChanged;
            }
        }
        #endregion

        #region CopyModelToControl, CopyModelSubsetToControl
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCopyModelToControl()
        {
            base.DoCopyModelToControl();

            CopyModelSubsetToControl();
        }

        /// <summary>
        /// Sets the checkmarks on the control to mirror the checkmarks on the model list.
        /// </summary>
        protected void CopyModelSubsetToControl()
        {
            var subsetList = ModelSubsetList;

            var allIdentifiers = new List<TIdentifier>();
            var allRowsInControl = Control.AllRecords.OfType<TListModel>().ToArray();
            foreach(var row in allRowsInControl) {
                var identifier = GetListIdentifierAction(row);
                allIdentifiers.Add(identifier);

                var needsTick = subsetList.Contains(identifier);
                Control.SetRecordChecked(row, needsTick);
            }

            var invalidSubset = subsetList.Except(allIdentifiers).ToArray();
            foreach(var invalidIdentifier in invalidSubset) {
                subsetList.Remove(invalidIdentifier);
            }
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when the columns need to be populated.
        /// </summary>
        /// <param name="listModel"></param>
        /// <param name="e"></param>
        private void Chain_FetchColumns(TListModel listModel, ListContentEventArgs e)
        {
            _BaseFetchColumns(listModel, e);

            var identifier = GetListIdentifierAction(listModel);
            var inSubset = ModelSubsetList.Contains(identifier);
            e.Checked = inSubset;
        }

        /// <summary>
        /// Called when the subset list has been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubsetList_ListChanged(object sender, ListChangedEventArgs e)
        {
            CopyModelSubsetToControl();
        }

        /// <summary>
        /// Called when the checked state is changed in the list.
        /// </summary>
        /// <param name="listModel"></param>
        /// <param name="isChecked"></param>
        private void Base_CheckedChangedHandler(TListModel listModel, bool isChecked)
        {
            var identifier = GetListIdentifierAction(listModel);
            if(isChecked && !ModelSubsetList.Contains(identifier)) ModelSubsetList.Add(identifier);
            if(!isChecked && ModelSubsetList.Contains(identifier)) ModelSubsetList.Remove(identifier);
        }
        #endregion
    }
}
