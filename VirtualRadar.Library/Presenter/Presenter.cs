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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        #region ValidationParams
        /// <summary>
        /// An inner class for use by any presenter to handle both single-field and whole-form validations.
        /// </summary>
        protected class Validation
        {
            /// <summary>
            /// Gets the validation results that this can potentially add to.
            /// </summary>
            public ValidationResults Results { get; private set; }

            /// <summary>
            /// Gets the validation field that represents the data-entry control that has a problem.
            /// </summary>
            public ValidationField Field { get; private set; }

            /// <summary>
            /// Gets or sets the child record that is being validated. Set to null if not validating a child record.
            /// </summary>
            public object Record { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that a validation failure is just a warning, it is not an outright fail.
            /// </summary>
            public bool IsWarning { get; set; }

            /// <summary>
            /// Gets or sets the message to show to the user when the validation fails. Is mutually exclusive with <see cref="Format"/>
            /// and <see cref="Args"/>.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the format string to use when formatting a message to show to the user.
            /// </summary>
            public string Format { get; set; }

            /// <summary>
            /// Gets or sets the arguments to the <see cref="Format"/> string when formatting a message to show to the user.
            /// </summary>
            public object[] Args { get; set; }

            /// <summary>
            /// Gets a value indicating that <see cref="Message"/> and <see cref="Format"/> have not been supplied.
            /// </summary>
            public bool HasNoMessage { get { return Message == null && Format == null; } }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="defaults"></param>
            public Validation(Validation defaults) : this(ValidationField.None, defaults.Results, defaults.Record)
            {
            }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="field"></param>
            /// <param name="defaults"></param>
            public Validation(ValidationField field, Validation defaults) : this(field, defaults.Results, defaults.Record)
            {
            }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="field"></param>
            /// <param name="results"></param>
            public Validation(ValidationField field, ValidationResults results) : this(field, results, null)
            {
            }

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="field"></param>
            /// <param name="results"></param>
            /// <param name="record"></param>
            public Validation(ValidationField field, ValidationResults results, object record)
            {
                Results = results;
                Field = field;
                Record = record;
            }

            /// <summary>
            /// Sets the <see cref="Message"/> property, but only if no message has yet been supplied.
            /// </summary>
            /// <param name="message"></param>
            public void DefaultMessage(string message)
            {
                if(HasNoMessage) Message = message;
            }

            /// <summary>
            /// Sets the <see cref="Message"/> property, but only if no message has yet been supplied.
            /// </summary>
            /// <param name="format"></param>
            /// <param name="args"></param>
            public void DefaultMessage(string format, params object[] args)
            {
                if(HasNoMessage) Message = String.Format(format, args);
            }

            /// <summary>
            /// Returns true if the validation passes, i.e. there is no problem with the value being tested.
            /// </summary>
            /// <param name="condition"></param>
            /// <returns></returns>
            public bool IsValid(Func<bool> condition)
            {
                if(Field == ValidationField.None) throw new InvalidOperationException("Cannot test the validity of a None field");

                var valid = false;
                try {
                    valid = condition();
                } catch(Exception ex) {
                    valid = false;
                    Message = null;
                    Format = Strings.ExceptionWhenCheckingValue;
                    Args = new object[] { ex.Message ?? "" };
                    IsWarning = false;
                }
                if(!valid) AddResult();

                if(Results.IsPartialValidation) {
                    AddPartialValidationField();
                }

                return valid;
            }

            /// <summary>
            /// Adds a record to the <see cref="Results"/> indicating that a validation test was carried out on the field.
            /// </summary>
            /// <returns>The result record describing the validation field or null if there was already an entry for this
            /// combination of field and record.</returns>
            public ValidationResult AddPartialValidationField()
            {
                return AddPartialValidationField(Field);
            }

            /// <summary>
            /// Adds a record to the <see cref="Results"/> indicating that a validation test was carried out on the field.
            /// </summary>
            /// <param name="field"></param>
            /// <returns>The result record describing the validation field or null if there was already an entry for this
            /// combination of field and record.</returns>
            public ValidationResult AddPartialValidationField(ValidationField field)
            {
                ValidationResult result = null;
                if(!Results.PartialValidationFields.Any(r => r.Record == Record && r.Field == field)) {
                    result = new ValidationResult(Record, field, null);
                    Results.PartialValidationFields.Add(result);
                }

                return result;
            }

            /// <summary>
            /// Adds a validation result to <see cref="Results"/> carrying the warning or error described by the properties.
            /// </summary>
            /// <returns></returns>
            public ValidationResult AddResult()
            {
                var result = new ValidationResult(Record, Field, Message ?? String.Format(Format, Args), IsWarning);
                Results.Results.Add(result);

                return result;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The view that the presenter is controlling.
        /// </summary>
        protected T _View;
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="view"></param>
        public virtual void Initialise(T view)
        {
            _View = view;
        }
        #endregion

        #region Validation methods
        /// <summary>
        /// Returns true if the file exists, false if it does not.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool FileExists(string fileName, Validation valParams)
        {
            valParams.DefaultMessage(Strings.SomethingDoesNotExist, fileName);
            return valParams.IsValid(() => FileExists(fileName));
        }

        /// <summary>
        /// Returns true if the file exists, false if it does not.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected bool FileExists(string fileName)
        {
            var result = true;
            if(!String.IsNullOrEmpty(fileName)) {
                result = File.Exists(fileName);
                if(result) {
                    // If we can't read the file then this should throw an exception
                    using(var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        ;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if the folder exists, false if it does not.
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool FolderExists(string folder, Validation valParams)
        {
            valParams.DefaultMessage(Strings.SomethingDoesNotExist, folder);
            return valParams.IsValid(() => FolderExists(folder));
        }

        /// <summary>
        /// Returns true if the folder exists, false if it does not.
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        protected bool FolderExists(string folder)
        {
            var result = true;
            if(!String.IsNullOrEmpty(folder)) {
                result = Directory.Exists(folder);
                if(result) {
                    // If we can't read the content of the folder then this should throw an exception
                    Directory.GetFileSystemEntries(folder);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if the domain address is valid.
        /// </summary>
        /// <param name="domainAddress"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool DomainAddressIsValid(string domainAddress, Validation valParams)
        {
            return valParams.IsValid(() => DomainAddressIsValid(domainAddress));
        }

        /// <summary>
        /// Returns true if the domain address is valid.
        /// </summary>
        /// <param name="domainAddress"></param>
        /// <returns></returns>
        protected bool DomainAddressIsValid(string domainAddress)
        {
            domainAddress = (domainAddress ?? "").Trim();
            var result = !String.IsNullOrEmpty(domainAddress);
            if(result) {
                try {
                    var addresses = Dns.GetHostAddresses(domainAddress);
                    result = addresses != null && addresses.Length != 0;
                } catch {
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the arbitrary condition is true.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="value"></param>
        /// <param name="condition"></param>
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool ConditionIsTrue<TValue>(TValue value, Func<TValue, bool> condition, Validation valParams)
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
        protected bool ConditionIsFalse<TValue>(TValue value, Func<TValue, bool> condition, Validation valParams)
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
        /// <param name="valParams"></param>
        /// <returns></returns>
        protected bool CollectionIsNotEmpty<TList>(IEnumerable<TList> collection, Validation valParams)
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
        protected bool StringIsNotEmpty(string value, Validation valParams)
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
        protected bool ValueNotEqual<TValue>(TValue value, TValue notEqualTo, Validation valParams)
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
        protected bool ValueIsInRange<TValue>(TValue value, TValue lowInclusive, TValue highInclusive, Validation valParams)
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
        protected bool ValueIsNotInList<TValue>(TValue value, IEnumerable<TValue> list, Validation valParams)
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
        protected bool ValueIsInList<TValue>(TValue value, IEnumerable<TValue> list, Validation valParams)
        {
            return valParams.IsValid(() => {
                return list.Contains(value);
            });
        }
        #endregion
    }
}
