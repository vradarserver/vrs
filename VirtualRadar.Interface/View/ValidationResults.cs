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

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// Describes the results of the validation of an entire form or a single field.
    /// </summary>
    public class ValidationResults
    {
        /// <summary>
        /// Gets a collection of every error and warning found.
        /// </summary>
        public List<ValidationResult> Results { get; private set; }

        /// <summary>
        /// Gets a value indicating that validation was only run over a subset of all available fields.
        /// </summary>
        public bool IsPartialValidation { get; private set; }

        /// <summary>
        /// Gets a collection of validation results that indicate the records and fields that were
        /// examined in a partial validation. The content of this collection is undefined when
        /// <see cref="IsPartialValidation"/> is false.
        /// </summary>
        /// <remarks>
        /// Only the Record and Field properties are set on the validation results held here.
        /// </remarks>
        public List<ValidationResult> PartialValidationFields { get; private set; }

        /// <summary>
        /// Gets a value indicating whether <see cref="Results"/> contains any errors.
        /// </summary>
        public bool HasErrors { get { return Results.Any(r => !r.IsWarning); } }

        /// <summary>
        /// Gets an array of all of the errors from <see cref="Results"/>.
        /// </summary>
        public ValidationResult[] Errors { get { return Results.Where(r => !r.IsWarning).ToArray(); } }

        /// <summary>
        /// Gets a value indicating that <see cref="Results"/> contains at least one warning.
        /// </summary>
        public bool HasWarnings { get { return Results.Any(r => r.IsWarning); } }

        /// <summary>
        /// Gets an array of all of the warnings found in <see cref="Results"/>.
        /// </summary>
        public ValidationResult[] Warnings { get { return Results.Where(r => r.IsWarning).ToArray(); } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="isPartialValidation"></param>
        public ValidationResults(bool isPartialValidation)
        {
            Results = new List<ValidationResult>();
            IsPartialValidation = isPartialValidation;
            PartialValidationFields = new List<ValidationResult>();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}{1} error(s), {2} warning(s)",
                IsPartialValidation ? String.Format("[PARTIAL {0} FIELD(S)] ", PartialValidationFields.Count) : "",
                Errors.Length,
                Warnings.Length
            );
        }
    }
}
