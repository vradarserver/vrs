// Copyright © 2017 onwards, Andrew Whewell
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
    /// Extension methods for the stock StringBuilder class.
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Returns the index of the last occurance of <paramref name="value"/> or -1 if
        /// <paramref name="value"/> does not appear in the buffer.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int LastIndexOf(this StringBuilder builder, char value, int startIndex)
        {
            if(startIndex >= builder.Length) {
                throw new ArgumentOutOfRangeException($"A {nameof(startIndex)} of {startIndex} equals or exceeds the string length {builder.Length}");
            }

            var result = -1;

            for(var i = startIndex;i >= 0;--i) {
                if(builder[i] == value) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the index of the last occurance of <paramref name="value"/> or -1 if
        /// <paramref name="value"/> does not appear in the buffer.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int LastIndexOf(this StringBuilder builder, char value)
        {
            return LastIndexOf(builder, value, builder.Length - 1);
        }

        /// <summary>
        /// Returns the index of the first occurance of <paramref name="value"/> or -1 if
        /// <paramref name="value"/> does not appear in the buffer.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static int IndexOf(this StringBuilder builder, char value, int startIndex)
        {
            if(startIndex < 0) {
                throw new ArgumentOutOfRangeException($"{nameof(startIndex)} cannot be negative");
            }

            var result = -1;

            for(var i = startIndex;i < builder.Length;++i) {
                if(builder[i] == value) {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the index of the first occurance of <paramref name="value"/> or -1 if
        /// <paramref name="value"/> does not appear in the buffer.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int IndexOf(this StringBuilder builder, char value)
        {
            return IndexOf(builder, value, 0);
        }

        /// <summary>
        /// Returns true if the builder contains <paramref name="value"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool Contains(this StringBuilder builder, char value)
        {
            return IndexOf(builder, value) != -1;
        }
    }
}
