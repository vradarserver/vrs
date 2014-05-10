// Copyright © 2010 onwards, Andrew Whewell
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
    /// Describes the result of the validation of a single field.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets the object that this result applies to.
        /// </summary>
        /// <remarks>
        /// This property is optional - it is only required if the validation result could apply to more than one object
        /// being validated.
        /// </remarks>
        public object Record { get; set; }

        /// <summary>
        /// Gets or sets the field that has failed validation.
        /// </summary>
        public ValidationField Field { get; set; }

        /// <summary>
        /// Gets or sets the message describing the problem with the field's content.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the validation result represents a warning to the user that they can
        /// ignore if they so wish, as opposed to an error that must be corrected before the input can be accepted.
        /// </summary>
        public bool IsWarning { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public ValidationResult() : this(null, ValidationField.None, null, false) { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="message"></param>
        public ValidationResult(ValidationField field, string message) : this(null, field, message, false) { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="field"></param>
        /// <param name="message"></param>
        /// <param name="isWarning"></param>
        public ValidationResult(ValidationField field, string message, bool isWarning) : this(null, field, message, isWarning) { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="field"></param>
        /// <param name="message"></param>
        public ValidationResult(object record, ValidationField field, string message) : this(record, field, message, false) { }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="field"></param>
        /// <param name="message"></param>
        /// <param name="isWarning"></param>
        public ValidationResult(object record, ValidationField field, string message, bool isWarning)
        {
            Record = record;
            Field = field;
            Message = message;
            IsWarning = isWarning;
        }

        /// <summary>
        /// Displays an English description of the validation result.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0}: {1}", Field, Message ?? "<null>");
        }
    }
}
