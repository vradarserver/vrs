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
using VirtualRadar.Interface.View;

namespace VirtualRadar.Library.Presenter
{
    abstract class ListDetailEditorPresenter<TView, TRecord> : EditorPresenter<TView, TRecord>
        where TView: IListDetailEditorView<TRecord>, IView
        where TRecord: class
    {
        /// <summary>
        /// The original values for a row that the reset button will restore.
        /// </summary>
        private TRecord _OriginalValues;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public override void Initialise(TView view)
        {
            base.Initialise(view);

            _View.ResetClicked += View_ResetClicked;
            _View.SelectedRecordChanged += View_SelectedRecordChanged;
            _View.NewRecordClicked += View_NewRecordClicked;
            _View.DeleteRecordClicked += View_DeleteRecordClicked;
            _View.SaveClicked += View_SaveClicked;
            _View.ValueChanged += View_ValueChanged;

            var records = DoLoadRecords();      // bear in mind that for child forms _View.Records is prefilled by the parent view and the presenter just uses that
            _View.Records.Clear();
            _View.Records.AddRange(records);
            _View.RefreshRecords();

            if(_View.Records.Count > 0) _View.SelectedRecord = _View.Records[0];
        }

        /// <summary>
        /// Validates every record.
        /// </summary>
        /// <returns></returns>
        private bool ValidateAllRecords()
        {
            var results = new List<ValidationResult>();

            foreach(var record in _View.Records) {
                DoValidation(results, record, record);
                if(results.Count != 0) _View.SelectedRecord = record;
                _View.ShowValidationResults(results);
                if(results.Count != 0) break;
            }

            return results.Count == 0;
        }

        /// <summary>
        /// Called when the Reset button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_ResetClicked(object sender, EventArgs args)
        {
            if(_OriginalValues != null && _View.SelectedRecord != null) {
                CopyRecordToFields(_OriginalValues);
                CopyFieldsToRecord(_View.SelectedRecord);
                _View.RefreshSelectedRecord();
            }
        }

        /// <summary>
        /// Called when the view indicates that the selected record has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_SelectedRecordChanged(object sender, EventArgs args)
        {
            CopyRecordToFields(_View.SelectedRecord);

            if(_View.SelectedRecord == null) _OriginalValues = null;
            else {
                _OriginalValues = DoCreateNewRecord();
                CopyFieldsToRecord(_OriginalValues);
            }
        }

        /// <summary>
        /// Raised when the edit fields are changed on the view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_ValueChanged(object sender, EventArgs args)
        {
            if(!_SuppressValueChangedEventHandler) {
                var selectedRecord = _View.SelectedRecord;
                if(selectedRecord != null && ValidateEditFields(selectedRecord)) {
                    CopyFieldsToRecord(selectedRecord);
                    _View.RefreshSelectedRecord();
                }
            }
        }

        /// <summary>
        /// Raised when the user asks for a new record to be created.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_NewRecordClicked(object sender, EventArgs args)
        {
            var record = DoCreateNewRecord();
            _View.Records.Add(record);
            _View.RefreshRecords();
            _View.SelectedRecord = record;

            CopyRecordToFields(_View.SelectedRecord);
            _View.FocusOnEditFields();
        }

        /// <summary>
        /// Returns a unique name for a new entry.
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        protected string SelectUniqueName(string prefix, Func<TRecord, string> selectName)
        {
            string result = null;

            for(var nameSuffix = 0;nameSuffix < int.MaxValue;++nameSuffix) {
                var suffix = nameSuffix == 0 ? "" : String.Format("({0})", nameSuffix);
                var name = String.Format("{0}{1}", prefix, suffix);
                if(!_View.Records.Any(r => selectName(r) == name)) {
                    result = name;
                    break;
                }
            }
            if(result == null) throw new InvalidOperationException("Cannot determine a unique name for the new record");

            return result;
        }

        /// <summary>
        /// Raised when the user wants to delete the selected record.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_DeleteRecordClicked(object sender, EventArgs args)
        {
            var record = _View.SelectedRecord;
            if(record != null && DoConfirmDelete(record)) {
                var index = _View.Records.IndexOf(record);
                _View.Records.Remove(record);
                _View.RefreshRecords();

                if(index < _View.Records.Count) _View.SelectedRecord = _View.Records[index];
                else if(_View.Records.Count > 0) _View.SelectedRecord = _View.Records[_View.Records.Count - 1];
                else _View.SelectedRecord = null;

                CopyRecordToFields(_View.SelectedRecord);
            }
        }

        /// <summary>
        /// When overridden by the derivee this gets confirmation from the user that they
        /// want to delete the record passed across. The default is to always return true.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        protected virtual bool DoConfirmDelete(TRecord record)
        {
            return true;
        }

        /// <summary>
        /// Raised when the user wants to save their changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void View_SaveClicked(object sender, CancelEventArgs args)
        {
            args.Cancel = _View.SelectedRecord != null && !ValidateEditFields(_View.SelectedRecord);
            if(!args.Cancel) args.Cancel = !ValidateAllRecords();
            if(!args.Cancel) DoSaveRecords(_View.Records);
        }

        /// <summary>
        /// When overridden by the derivee this loads the list of records that will be shown to
        /// the user.
        /// </summary>
        /// <returns></returns>
        protected abstract List<TRecord> DoLoadRecords();

        /// <summary>
        /// When overridden by the derivee this persists the list of records passed in.
        /// </summary>
        /// <param name="records"></param>
        protected abstract void DoSaveRecords(List<TRecord> records);
    }
}
