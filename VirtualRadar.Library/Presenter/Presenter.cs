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
using System.IO;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// A common base for presenters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class Presenter<T>
        where T: IView
    {
        /// <summary>
        /// The view that the presenter is controlling.
        /// </summary>
        protected T _View;

        /// <summary>
        /// Initialises the view.
        /// </summary>
        /// <param name="view"></param>
        public virtual void Initialise(T view)
        {
            _View = view;
        }

        /// <summary>
        /// Validates that a required field has been supplied. Returns true if the validation passes, false if it does not.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="results"></param>
        /// <param name="validationField"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool ValidateRequired(string field, List<ValidationResult> results, ValidationField validationField, string message)
        {
            bool valid = !String.IsNullOrEmpty(field);
            if(!valid) results.Add(new ValidationResult(validationField, message));

            return valid;
        }

        /// <summary>
        /// Validates that a required field has been supplied. Returns true if the validation passes, false if it does not.
        /// </summary>
        /// <typeparam name="TField"></typeparam>
        /// <param name="field"></param>
        /// <param name="results"></param>
        /// <param name="validationField"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool ValidateRequired<TField>(TField field, List<ValidationResult> results, ValidationField validationField, string message)
            where TField: class
        {
            bool valid = field != null;
            if(!valid) results.Add(new ValidationResult(validationField, message));

            return valid;
        }

        /// <summary>
        /// Validates that a record is unique. Returns true if the record is unique, false if it is not.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="field"></param>
        /// <param name="currentRecord"></param>
        /// <param name="records"></param>
        /// <param name="getField"></param>
        /// <param name="results"></param>
        /// <param name="validationField"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool ValidateMustBeUnique<TRecord>(string field, TRecord currentRecord, List<TRecord> records, Func<TRecord, string> getField, List<ValidationResult> results, ValidationField validationField, string message)
        {
            var valid = true;
            if(currentRecord != null) {
                valid = !records.Any(r => !Object.ReferenceEquals(r, currentRecord) && (field ?? "").Equals(getField(r), StringComparison.CurrentCultureIgnoreCase));
            }
            if(!valid) results.Add(new ValidationResult(validationField, message));

            return valid;
        }

        /// <summary>
        /// Validates that a record is unique. Returns true if the record is unique, false if it is not.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="field"></param>
        /// <param name="currentRecord"></param>
        /// <param name="records"></param>
        /// <param name="getField"></param>
        /// <param name="results"></param>
        /// <param name="validationField"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool ValidateMustBeUnique<TRecord, TValue>(TValue field, TRecord currentRecord, List<TRecord> records, Func<TRecord, TValue> getField, List<ValidationResult> results, ValidationField validationField, string message)
        {
            var valid = true;
            if(currentRecord != null) {
                valid = !records.Any(r => !Object.ReferenceEquals(r, currentRecord) && field.Equals(getField(r)));
            }
            if(!valid) results.Add(new ValidationResult(validationField, message));

            return valid;
        }

        /// <summary>
        /// Validates that a value is within a given range. Returns true if the value is within range, false if it is not.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="results"></param>
        /// <param name="validationField"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool ValidateInBounds(double value, double lowerBound, double upperBound, List<ValidationResult> results, ValidationField validationField, string message)
        {
            var valid = value >= lowerBound && value <= upperBound;
            if(!valid) results.Add(new ValidationResult(validationField, message));

            return valid;
        }

        /// <summary>
        /// Validates that a value is within a given range. Returns true if the value is within range, false if it is not.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lowerBound"></param>
        /// <param name="upperBound"></param>
        /// <param name="results"></param>
        /// <param name="validationField"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool ValidateInBounds(long value, long lowerBound, long upperBound, List<ValidationResult> results, ValidationField validationField, string message)
        {
            var valid = value >= lowerBound && value <= upperBound;
            if(!valid) results.Add(new ValidationResult(validationField, message));

            return valid;
        }

        /// <summary>
        /// Validates that a condition is true. Returns true if the condition is true, false if it is not.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="results"></param>
        /// <param name="validationField"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool ValidateIsTrue(bool condition, List<ValidationResult> results, ValidationField validationField, string message)
        {
            var valid = condition;
            if(!valid) results.Add(new ValidationResult(validationField, message));

            return valid;
        }

        /// <summary>
        /// Validates that a file exists. Returns true if the file exists, false if it does not.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="results"></param>
        /// <param name="validationField"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        protected bool ValidateFileExists(string field, List<ValidationResult> results, ValidationField validationField, string message)
        {
            var valid = File.Exists(field);
            if(!valid) results.Add(new ValidationResult(validationField, message));

            return valid;
        }
    }
}
