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
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VirtualRadar.Interface.WebServer;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Static methods that can help when dealing with <see cref="HttpContent"/>.
    /// </summary>
    public static class HttpContentHelper
    {
        /// <summary>
        /// Generates <see cref="FormUrlEncodedContent"/> content from a 2D array of string keys and values.
        /// </summary>
        /// <param name="unencodedKeyValues"></param>
        /// <returns></returns>
        public static FormUrlEncodedContent FormUrlEncoded(string[,] unencodedKeyValues)
        {
            if(unencodedKeyValues == null) {
                throw new ArgumentNullException(nameof(unencodedKeyValues));
            }
            if(unencodedKeyValues.GetLength(1) != 2) {
                throw new ArgumentOutOfRangeException($"You must pass a two dimensional array to {nameof(FormUrlEncoded)}");
            }

            var kvpList = new List<KeyValuePair<string, string>>();
            for(var i = 0;i < unencodedKeyValues.GetLength(0);++i) {
                kvpList.Add(new KeyValuePair<string, string>(unencodedKeyValues[i,0], unencodedKeyValues[i, 1]));
            }

            return new FormUrlEncodedContent(kvpList);
        }

        /// <summary>
        /// Generates <see cref="StringContent"/> content from an object that gets serialised into JSON.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static StringContent StringContentJson(object obj)
        {
            if(obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }

            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");
        }
    }
}
