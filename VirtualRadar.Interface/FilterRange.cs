// Copyright © 2013 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Describes a range filter to apply to a value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FilterRange<T> : Filter
        where T: struct, IComparable
    {
        /// <summary>
        /// Gets or sets the lower value in the range. A null indicates that there is no lower limit.
        /// </summary>
        public T? LowerValue { get; set; }

        /// <summary>
        /// Gets or sets the upper value in the range. A null indicates that there is no upper limit.
        /// </summary>
        public T? UpperValue { get; set; }

        /// <summary>
        /// Gets a value indicating whether the filter appears to be correctly configured with a valid condition and at least an upper or lower value.
        /// </summary>
        public bool IsValid { get { return Condition == FilterCondition.Between && (LowerValue != null || UpperValue != null); } }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FilterRange() : base()
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="lowerValue"></param>
        /// <param name="upperValue"></param>
        public FilterRange(T? lowerValue, T? upperValue) : this()
        {
            LowerValue = lowerValue;
            UpperValue = upperValue;
            Condition = FilterCondition.Between;
        }

        /// <summary>
        /// If the lower value is larger than the upper value then this swaps them around.
        /// </summary>
        public void NormaliseRange()
        {
            if(LowerValue != null && UpperValue != null && LowerValue.Value.CompareTo(UpperValue.Value) > 0) {
                var lowerValue = LowerValue;
                LowerValue = UpperValue;
                UpperValue = lowerValue;
            }
        }

        /// <summary>
        /// Returns true if the value is within the bounds set by the filter.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Passes(T? value)
        {
            var result = true;
            if(IsValid) {
                result = value != null;
                if(result && LowerValue != null) result = value.Value.CompareTo(LowerValue.Value) >= 0;
                if(result && UpperValue != null) result = value.Value.CompareTo(UpperValue.Value) <= 0;

                if(ReverseCondition) result = !result;
            }

            return result;
        }
    }
}
