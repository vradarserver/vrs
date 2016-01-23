// Copyright © 2016 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// A representation of <see cref="ValidationResults"/> for abstractions of views that don't refer
    /// directly to internal objects (i.e. they can't use the Record field on the ValidationResult).
    /// </summary>
    public class ValidationResultsModel
    {
        /// <summary>
        /// An optional method that can convert a record into a name. Only required if the form uses multi-record validation results.
        /// </summary>
        private Func<object, string> RecordToString { get; set; }

        /// <summary>
        /// An optional method that can extract the string identifier from a record. Only required if the form uses multi-record validation results
        /// and the validation fields can apply to more than one record.
        /// </summary>
        private Func<object, string> RecordToId { get; set; }

        private List<ValidationResultModel> _Results = new List<ValidationResultModel>();
        /// <summary>
        /// Gets a collection of every error and warning found.
        /// </summary>
        public ValidationResultModel[] Results
        {
            get { return _Results.ToArray(); }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ValidationResultsModel() : this(null, null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="recordToString"></param>
        /// <param name="recordToId"></param>
        public ValidationResultsModel(Func<object, string> recordToString, Func<object, string> recordToId)
        {
            RecordToString = recordToString;
            RecordToId = recordToId;
        }

        /// <summary>
        /// Applies the latest set of validation results to the model.
        /// </summary>
        /// <param name="validationResults"></param>
        public void RefreshFromResults(ValidationResults validationResults)
        {
            if(!validationResults.IsPartialValidation) {
                _Results.Clear();
                _Results.AddRange(validationResults.Results.Where(r => !String.IsNullOrEmpty(r.Message)).Select(r => new ValidationResultModel(r, RecordToString, RecordToId)));
            } else {
                var results = validationResults.Results.Select(r => new ValidationResultModel(r, RecordToString, RecordToId)).ToDictionary(r => r, r => r);
                var partialResults = validationResults.Results.Select(r => new ValidationResultModel(r, RecordToString, RecordToId)).ToArray();

                foreach(var partialResult in partialResults) {
                    var existingResultIndex = _Results.IndexOf(partialResult);
                    if(existingResultIndex != -1) {
                        _Results.RemoveAt(existingResultIndex);
                    }

                    ValidationResultModel result;
                    if(results.TryGetValue(partialResult, out result)) {
                        if(!String.IsNullOrEmpty(result.Message)) {
                            _Results.Add(result);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// A representation of <see cref="ValidationResult"/> that does not refer directly to objects.
    /// </summary>
    public class ValidationResultModel
    {
        /// <summary>
        /// Gets the name of the object that this result applies to.
        /// </summary>
        /// <remarks>
        /// This property is optional - it is only required if the validation result could apply to more than one object
        /// being validated.
        /// </remarks>
        public string RecordName { get; private set; }

        /// <summary>
        /// Gets the identifier of the record that this result applies to.
        /// </summary>
        /// <remarks>
        /// This property is optional - it is only required if the validation result could apply to more than one object
        /// being validated.
        /// </remarks>
        public string RecordId { get; private set; }

        /// <summary>
        /// Gets the name of the field that has failed validation.
        /// </summary>
        public string FieldName { get; private set; }

        /// <summary>
        /// Gets the message describing the problem with the field's content.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets a value indicating that the validation result represents a warning to the user that they can
        /// ignore if they so wish, as opposed to an error that must be corrected before the input can be accepted.
        /// </summary>
        public bool IsWarning { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="validationResult"></param>
        /// <param name="recordToString"></param>
        /// <param name="recordToId"></param>
        public ValidationResultModel(ValidationResult validationResult, Func<object, string> recordToString, Func<object, string> recordToId)
        {
            RecordName = validationResult.Record == null || recordToString == null ? null : recordToString(validationResult.Record);
            RecordId = validationResult.Record == null || recordToId == null ? null : recordToId(validationResult.Record);
            FieldName = validationResult.Field.ToString();
            Message = validationResult.Message;
            IsWarning = validationResult.IsWarning;
        }

        /// <summary>
        /// Returns true if the <see cref="RecordName"/>, <see cref="RecordId"/> and <see cref="FieldName"/> match the other object's properties.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result) {
                var other = obj as ValidationResultModel;
                if(other != null) {
                    result = RecordName == other.RecordName &&
                             RecordId == other.RecordId &&
                             FieldName == other.FieldName;
                }
            }

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return FieldName.GetHashCode();
        }
    }
}
