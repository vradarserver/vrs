// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A string filter to apply to a value.
    /// </summary>
    public class FilterString : Filter
    {
        /// <summary>
        /// Gets or sets the text that the value will be filtered against.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FilterString() : base()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="value"></param>
        public FilterString(string value) : this()
        {
            Value = value;
            Condition = FilterCondition.Equals;
        }

        /// <summary>
        /// Converts the value to upper-case.
        /// </summary>
        public void ToUpperInvariant()
        {
            if(Value != null) {
                Value = Value.ToUpperInvariant();
            }
        }

        /// <summary>
        /// Returns true if the value passes the filter. Note that all comparisons are case insensitive.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Passes(string value)
        {
            var result = true;

            if(String.IsNullOrEmpty(Value)) {
                if(Condition == FilterCondition.Equals) {
                    result = String.IsNullOrEmpty(value);
                    if(ReverseCondition) {
                        result = !result;
                    }
                }
            } else {
                var validCondition = true;
                switch(Condition) {
                    case FilterCondition.Equals:        result = !String.IsNullOrEmpty(value) && value.Equals(Value, StringComparison.OrdinalIgnoreCase); break;
                    case FilterCondition.Contains:      result = !String.IsNullOrEmpty(value) && value.IndexOf(Value, StringComparison.OrdinalIgnoreCase) != -1; break;
                    case FilterCondition.StartsWith:    result = !String.IsNullOrEmpty(value) && value.StartsWith(Value, StringComparison.OrdinalIgnoreCase); break;
                    case FilterCondition.EndsWith:      result = !String.IsNullOrEmpty(value) && value.EndsWith(Value, StringComparison.OrdinalIgnoreCase); break;
                    default:                            validCondition = false; break;
                }
                if(validCondition && ReverseCondition) {
                    result = !result;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if any of the values pass the filter. Note that all comparisons are case insensitive.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Passes(IEnumerable<string> value)
        {
            var result = true;

            if(String.IsNullOrEmpty(Value)) {
                if(Condition == FilterCondition.Equals) {
                    result = value == null || value.Any(r => String.IsNullOrEmpty(r));
                    if(ReverseCondition) {
                        result = !result;
                    }
                }
            } else {
                var validCondition = true;
                switch(Condition) {
                    case FilterCondition.Equals:        result = value != null && value.Any(r => !String.IsNullOrEmpty(r) && r.Equals(Value, StringComparison.OrdinalIgnoreCase)); break;
                    case FilterCondition.Contains:      result = value != null && value.Any(r => !String.IsNullOrEmpty(r) && r.IndexOf(Value, StringComparison.OrdinalIgnoreCase) != -1); break;
                    case FilterCondition.StartsWith:    result = value != null && value.Any(r => !String.IsNullOrEmpty(r) && r.StartsWith(Value, StringComparison.OrdinalIgnoreCase)); break;
                    case FilterCondition.EndsWith:      result = value != null && value.Any(r => !String.IsNullOrEmpty(r) && r.EndsWith(Value, StringComparison.OrdinalIgnoreCase)); break;
                    default:                            validCondition = false; break;
                }
                if(validCondition && ReverseCondition) {
                    result = !result;
                }
            }

            return result;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = base.Equals(obj);
            if(result) {
                result = false;
                if(obj is FilterString other) {
                    result = Object.Equals(Value, other.Value);
                }
            }

            return result;
        }

        /// <summary>
        /// See base docs. Do not use these objects as keys!
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked {
                return (base.GetHashCode() << 16) | Value.GetHashCode();
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{base.ToString()} {Value}";
    }
}
