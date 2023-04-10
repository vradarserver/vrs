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
    /// Describes a filter against an enum value.
    /// </summary>
    public class FilterEnum<T> : Filter
        where T: struct, IComparable
    {
        /// <summary>
        /// Gets or sets the enum value to compare against a value.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FilterEnum() : base()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="value"></param>
        public FilterEnum(T value) : this()
        {
            Value = value;
            Condition = FilterCondition.Equals;
        }

        /// <summary>
        /// Returns true if the enum value passed across passes the filter.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Passes(T? value)
        {
            var result = true;
            
            if(Condition == FilterCondition.Equals) {
                result = value != null;
                if(result) {
                    result = value.Value.CompareTo(Value) == 0;
                }
                if(ReverseCondition) {
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
                if(obj is FilterEnum<T> other) {
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
