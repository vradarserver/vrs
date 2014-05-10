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
using System.Text;
using System.Windows.Forms;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// A common base for forms that present a list to the user and allow the
    /// user to create, delete and edit records from that list.
    /// </summary>
    public class ListDetailEditorForm : BaseForm
    {
        #region Fields
        protected bool _SuppressItemSelectedEventHandler;
        private GroupBox _ControlsGroupBox;
        private Func<object> _GetSelectedRecord;
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
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
        /// See interface docs.
        /// </summary>
        public event EventHandler ResetClicked;

        /// <summary>
        /// Raises <see cref="ResetClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnResetClicked(EventArgs args)
        {
            if(ResetClicked != null) ResetClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler NewRecordClicked;

        /// <summary>
        /// Raises <see cref="NewRecordClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnNewRecordClicked(EventArgs args)
        {
            if(NewRecordClicked != null) NewRecordClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler DeleteRecordClicked;

        /// <summary>
        /// Raises <see cref="DeleteRecordClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDeleteRecordClicked(EventArgs args)
        {
            if(DeleteRecordClicked != null) DeleteRecordClicked(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<CancelEventArgs> SaveClicked;

        /// <summary>
        /// Raises <see cref="SaveClicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnSaveClicked(CancelEventArgs args)
        {
            if(SaveClicked != null) SaveClicked(this, args);
        }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ListDetailEditorForm() : base()
        {
        }
        #endregion

        #region RegisterFormDetail, WireStandardEvents
        /// <summary>
        /// Records elements that are common to all list detail editor forms.
        /// </summary>
        /// <param name="controlsGroupBox"></param>
        /// <param name="getSelectedRecord"></param>
        protected void RegisterFormDetail(GroupBox controlsGroupBox, Func<object> getSelectedRecord)
        {
            _ControlsGroupBox = controlsGroupBox;
            _GetSelectedRecord = getSelectedRecord;

            EnableDisableControls();
        }

        /// <summary>
        /// Wires up common controls that we're interested in.
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="newButton"></param>
        /// <param name="deleteButton"></param>
        /// <param name="resetButton"></param>
        /// <param name="valueChangedControls"></param>
        /// <param name="okButton"></param>
        protected void WireStandardEvents(ListView listView, Button newButton, Button deleteButton, Button resetButton, object[] valueChangedControls, Button okButton)
        {
            listView.SelectedIndexChanged += listView_SelectedIndexChanged;
            newButton.Click += buttonNew_Click;
            deleteButton.Click += buttonDelete_Click;
            resetButton.Click += buttonReset_Click;
            okButton.Click += buttonOK_Click;

            _ValueChangedHelper.HookValueChanged(valueChangedControls);
        }
        #endregion

        #region PopulateRecordsListView, PopulateListViewItem
        /// <summary>
        /// When overridden by the derivee this populates the records list.
        /// </summary>
        protected virtual void PopulateRecords()
        {
        }

        /// <summary>
        /// Copies a list of records into the list view supplied.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="records"></param>
        /// <param name="selectedRecord"></param>
        /// <param name="selectRecord"></param>
        protected void PopulateRecordsListView<T>(ListView listView, List<T> records, T selectedRecord, Action<T> selectRecord)
        {
            var currentSuppressState = _SuppressItemSelectedEventHandler;

            try {
                _SuppressItemSelectedEventHandler = true;
                base.PopulateListView<T>(listView, records, selectedRecord, PopulateListViewItem, selectRecord);
            } finally {
                _SuppressItemSelectedEventHandler = currentSuppressState;
            }
        }

        /// <summary>
        /// When overridden by a derivee this fills a single list item's text with the values from the record.
        /// </summary>
        /// <param name="item"></param>
        protected virtual void PopulateListViewItem(ListViewItem item)
        {
        }
        #endregion

        #region RefreshRecords
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RefreshRecords()
        {
            PopulateRecords();
        }

        /// <summary>
        /// Refreshes the list view item for a single record in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="listView"></param>
        /// <param name="selectedRecord"></param>
        protected void DoRefreshSelectedRecord<T>(ListView listView, T selectedRecord)
            where T: class
        {
            var item = FindListViewItemForRecord(listView, selectedRecord);
            if(item != null) PopulateListViewItem(item);
        }
        #endregion

        #region EnableDisableControls
        /// <summary>
        /// Enables or disables controls.
        /// </summary>
        protected virtual void EnableDisableControls()
        {
            if(_ControlsGroupBox != null && _GetSelectedRecord != null) _ControlsGroupBox.Enabled = _GetSelectedRecord() != null;
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when a user selects an item in the list of servers.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableControls();
            if(!_SuppressItemSelectedEventHandler) OnSelectedRecordChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Raised when the new button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNew_Click(object sender, EventArgs e)
        {
            OnNewRecordClicked(EventArgs.Empty);
        }

        /// <summary>
        /// Raised when the delete button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonDelete_Click(object sender, EventArgs e)
        {
            OnDeleteRecordClicked(EventArgs.Empty);
        }

        /// <summary>
        /// Raised whenever the reset button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonReset_Click(object sender, EventArgs e)
        {
            OnResetClicked(EventArgs.Empty);
        }

        /// <summary>
        /// Raised when the OK button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            var args = new CancelEventArgs(false);
            OnSaveClicked(args);
            if(args.Cancel) DialogResult = DialogResult.None;
        }
        #endregion
    }
}
