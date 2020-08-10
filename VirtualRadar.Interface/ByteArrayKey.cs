// Copyright © 2020 onwards, Andrew Whewell
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
using System.Threading.Tasks;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Similar to <see cref="ByteArrayComparable"/> except this can be used as a key in dictionaries. Unlike
    /// <see cref="ByteArrayComparable"/> an empty array and a null array are not considered the same array.
    /// </summary>
    public class ByteArrayKey
    {
        /// <summary>
        /// Gets the byte array that is being used as a key.
        /// </summary>
        public byte[] Array { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="array"></param>
        public ByteArrayKey(byte[] array)
        {
            Array = array;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var result = Object.ReferenceEquals(this, obj);
            if(!result) {
                if(obj is ByteArrayKey other) {
                    result = Array == null && other.Array == null;
                    if(!result && Array != null && other.Array != null) {
                        result = Array.Length == other.Array.Length;
                        for(var i = 0;result && i < Array.Length;++i) {
                            result = Array[i] == other.Array[i];
                        }
                    }
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
            if(Array == null) {
                return int.MinValue;
            }

            var toHashCode = 0L;
            var len = Array.Length < 9 ? Array.Length : 8;
            for(var i = 0;i < len;++i) {
                toHashCode <<= 8;
                toHashCode |= Array[i];
            }

            return toHashCode.GetHashCode();
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Array == null ? "<null>" : String.Join("", Array.Select(r => r.ToString("x2")));
    }
}
