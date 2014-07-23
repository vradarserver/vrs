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
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Presenter
{
    /// <summary>
    /// A common base for presenters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class Presenter<T>
        where T: IView
    {
        protected class ValidationParams
        {
            public List<ValidationResult> Results { get; private set; }

            public ValidationField Field { get; private set; }

            public ValidationField ValueChangedField { get; set; }

            public bool FieldMatches { get { return ValueChangedField == ValidationField.None || Field == ValueChangedField; } }

            public object Record { get; set; }

            public bool IsWarning { get; set; }

            public string Message { get; set; }

            public string Format { get; set; }

            public object[] Args { get; set; }

            public bool HasNoMessage { get { return Message == null && Format == null; } }

            public ValidationParams(ValidationField field, List<ValidationResult> results) : this(field, results, null, ValidationField.None)
            {
            }

            public ValidationParams(ValidationField field, List<ValidationResult> results, object record) : this(field, results, record, ValidationField.None)
            {
            }

            public ValidationParams(ValidationField field, List<ValidationResult> results, object record, ValidationField valueChangedField)
            {
                Results = results;
                Field = field;
                Record = record;
                ValueChangedField = valueChangedField;
            }

            public void DefaultMessage(string message)
            {
                if(HasNoMessage) Message = message;
            }

            public void DefaultMessage(string format, params object[] args)
            {
                if(HasNoMessage) Message = String.Format(format, args);
            }

            public bool IsValid(Func<bool> condition)
            {
                var fieldMatches = FieldMatches;
                var valid = !fieldMatches;
                if(!valid) {
                    try {
                        valid = condition();
                    } catch(Exception ex) {
                        valid = false;
                        Message = null;
                        Format = Strings.ExceptionWhenCheckingValue;
                        Args = new object[] { ex.Message ?? "" };
                        IsWarning = false;
                    }
                }
                if(!valid) AddResult();

                return valid;
            }

            public ValidationResult AddResult()
            {
                var result = new ValidationResult(Record, Field, Message ?? String.Format(Format, Args), IsWarning);
                Results.Add(result);

                return result;
            }
        }

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

        #region Legcay Validation Methods - once nothing is using these, delete them
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
        #endregion

        #region Validation methods - FileExists, FolderExists
        /// <summary>
        /// Returns true if the file exists, false if it does not.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool FileExists(string fileName, ValidationParams valParams)
        {
            valParams.DefaultMessage(Strings.SomethingDoesNotExist, fileName);
            return valParams.IsValid(() => {
                var result = true;
                if(!String.IsNullOrEmpty(fileName)) {
                    result = File.Exists(fileName);
                    if(result) {
                        // If we can't read the file then this should throw an exception
                        using(var stream = File.OpenRead(fileName)) {
                            ;
                        }
                    }
                }
                return result;
            });
        }

        /// <summary>
        /// Returns true if the folder exists, false if it does not.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool FolderExists(string folder, ValidationParams valParams)
        {
            valParams.DefaultMessage(Strings.SomethingDoesNotExist, folder);
            return valParams.IsValid(() => {
                var result = true;
                if(!String.IsNullOrEmpty(folder)) {
                    result = Directory.Exists(folder);
                    if(result) {
                        // If we can't read the content of the folder then this should throw an exception
                        Directory.GetFileSystemEntries(folder);
                    }
                }
                return result;
            });
        }

        /// <summary>
        /// Returns true if the arbitrary condition is true.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="condition"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool ConditionIsTrue<TValue>(TValue value, Func<TValue, bool> condition, ValidationParams valParams)
        {
            return valParams.IsValid(() => {
                return condition(value);
            });
        }

        /// <summary>
        /// Returns true if the arbitrary condition is false.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="condition"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool ConditionIsFalse<TValue>(TValue value, Func<TValue, bool> condition, ValidationParams valParams)
        {
            return valParams.IsValid(() => {
                return !condition(value);
            });
        }

        /// <summary>
        /// Returns true if the collection is not empty.
        /// </summary>
        /// <typeparam name="TList"></typeparam>
        /// <param name="collection"></param>
        /// <param name="collectionIsValid"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool CollectionIsNotEmpty<TList>(IEnumerable<TList> collection, ValidationParams valParams)
        {
            return valParams.IsValid(() => {
                return collection.Count() != 0;
            });
        }

        /// <summary>
        /// Returns true if the string is not null or empty.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool StringIsNotEmpty(string value, ValidationParams valParams)
        {
            return valParams.IsValid(() => {
                return !String.IsNullOrEmpty((value ?? "").Trim());
            });
        }

        /// <summary>
        /// Returns true if the value is not equal to the value passed across.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="notEqualTo"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool ValueNotEqual<TValue>(TValue value, TValue notEqualTo, ValidationParams valParams)
            where TValue: IComparable
        {
            return valParams.IsValid(() => {
                return value.CompareTo(notEqualTo) != 0;
            });
        }

        /// <summary>
        /// Returns true if the value is within the inclusive range.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="lowInclusive"></param>
        /// <param name="highInclusive"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool ValueIsInRange<TValue>(TValue value, TValue lowInclusive, TValue highInclusive, ValidationParams valParams)
            where TValue: IComparable
        {
            return valParams.IsValid(() => {
                return lowInclusive.CompareTo(value) <= 0 && highInclusive.CompareTo(value) >= 0;
            });
        }

        /// <summary>
        /// Returns true if the value is not within the list of values passed across.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="list"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool ValueIsNotInList<TValue>(TValue value, IEnumerable<TValue> list, ValidationParams valParams)
        {
            return valParams.IsValid(() => {
                return !list.Contains(value);
            });
        }

        /// <summary>
        /// Returns true if the value is in the list of values passed across.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="list"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool ValueIsInList<TValue>(TValue value, IEnumerable<TValue> list, ValidationParams valParams)
        {
            return valParams.IsValid(() => {
                return list.Contains(value);
            });
        }
        #endregion
    }
}
