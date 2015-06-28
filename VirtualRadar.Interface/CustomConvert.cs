// Copyright © 2015 onwards, Andrew Whewell
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
    /// Performs simple custom conversions.
    /// </summary>
    public static class CustomConvert
    {
        /// <summary>
        /// Converts a value from one type to another, as per the standard <see cref="Convert.ChangeType(object, Type)"/>,
        /// except that this version can cope with the conversion of <see cref="Nullable{T}"/> types.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static object ChangeType(object value, Type type, IFormatProvider provider)
        {
            object result = null;

            if(!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>)) {
                if(type.IsEnum) {
                    result = Enum.Parse(type, (string)Convert.ChangeType(value, typeof(string)));
                } else if(type == typeof(Guid)) {
                    var guidText = (string)Convert.ChangeType(value, typeof(string));
                    result = new Guid(guidText);
                } else if(type == typeof(IntPtr)) {
                    var intPtrText = (string)Convert.ChangeType(value, typeof(string));
                    var intPtrInt = int.Parse(intPtrText);
                    result = new IntPtr(intPtrInt);
                } else if(type == typeof(TimeSpan)) {
                    var timeSpanText = (string)Convert.ChangeType(value, typeof(string));
                    result = TimeSpan.Parse(timeSpanText);
                } else {
                    result = Convert.ChangeType(value, type, provider);
                }
            } else if(value != null) {
                var underlyingType = type.GetGenericArguments()[0];
                result = ChangeType(value, underlyingType, provider);
            }

            return result;
        }
    }
}
