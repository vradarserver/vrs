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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// A common base for presenters of editor views.
    /// </summary>
    /// <typeparam name="TView"></typeparam>
    /// <typeparam name="TRecord"></typeparam>
    abstract class EditorPresenter<TView, TRecord> : Presenter<TView>
        where TView: IView, IValidateView
        where TRecord: class
    {
        /// <summary>
        /// When true the event handle for the ValueChanged event is not allowed to run.
        /// </summary>
        protected bool _SuppressValueChangedEventHandler;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="view"></param>
        public override void Initialise(TView view)
        {
            base.Initialise(view);
        }

        /// <summary>
        /// Copies the selected record to the edit fields.
        /// </summary>
        protected void CopyRecordToFields(TRecord record)
        {
            var currentSuppressSetting = _SuppressValueChangedEventHandler;
            try {
                _SuppressValueChangedEventHandler = true;

                DoCopyRecordToEditFields(record);
                _View.ShowValidationResults(new ValidationResult[] { });
            } finally {
                _SuppressValueChangedEventHandler = currentSuppressSetting;
            }
        }

        /// <summary>
        /// Copies the content of the editor fields to the record passed across.
        /// </summary>
        /// <param name="record"></param>
        protected bool CopyFieldsToRecord(TRecord record)
        {
            bool result = record != null;
            if(result) DoCopyEditFieldsToRecord(record);

            return result;
        }

        /// <summary>
        /// Validates the edit fields.
        /// </summary>
        /// <returns>True if the fields pass validation.</returns>
        public bool ValidateEditFields(TRecord currentRecord)
        {
            var results = new List<ValidationResult>();

            var record = DoCreateNewRecord();
            DoCopyEditFieldsToRecord(record);

            var isValid = DoValidation(results, record, currentRecord);
            _View.ShowValidationResults(results);

            return results.Count == 0 && (isValid == null ? true : isValid.Value);
        }

        /// <summary>
        /// When implemented by the derivee this creates a new record object. The new record must not be persisted.
        /// </summary>
        /// <returns></returns>
        protected abstract TRecord DoCreateNewRecord();

        /// <summary>
        /// When implemented by the derivee this copies the content of a record to the view's edit fields.
        /// </summary>
        /// <param name="record"></param>
        protected abstract void DoCopyRecordToEditFields(TRecord record);

        /// <summary>
        /// When implemented by the derivee this copies the content of the view's edit fields to the record.
        /// </summary>
        /// <param name="selectedRecord"></param>
        protected abstract void DoCopyEditFieldsToRecord(TRecord selectedRecord);

        /// <summary>
        /// When implemented by the derivee this validates the content of the record within the context of an
        /// optional original / current record.
        /// </summary>
        /// <param name="results"></param>
        /// <param name="record"></param>
        /// <param name="currentRecord"></param>
        /// <returns></returns>
        protected abstract bool? DoValidation(List<ValidationResult> results, TRecord record, TRecord currentRecord);
    }
}
