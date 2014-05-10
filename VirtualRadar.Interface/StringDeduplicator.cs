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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A utility class that can be used to remove duplicates of strings.
    /// </summary>
    public class StringDeduplicator
    {
        private Dictionary<string, string> _UniqueStrings = new Dictionary<string,string>();

        /// <summary>
        /// Returns the text passed in. If the text has been seen before then the first instance of the string
        /// is returned in place of the instance passed across.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Deduplicate(string text)
        {
            string result = null;
            if(text != null && !_UniqueStrings.TryGetValue(text, out result)) {
                _UniqueStrings.Add(text, text);
                result = text;
            }

            return result;
        }

        /// <summary>
        /// Deduplicates every element in the array passed across.
        /// </summary>
        /// <param name="array"></param>
        public void Deduplicate(string[] array)
        {
            if(array != null) {
                for(int i = 0;i < array.Length;++i) {
                    array[i] = Deduplicate(array[i]);
                }
            }
        }

        /// <summary>
        /// Deduplicates every element in the list passed across.
        /// </summary>
        /// <param name="list"></param>
        public void Deduplicate(IList<string> list)
        {
            if(list != null) {
                for(int i = 0;i < list.Count;++i) {
                    list[i] = Deduplicate(list[i]);
                }
            }
        }
    }
}
