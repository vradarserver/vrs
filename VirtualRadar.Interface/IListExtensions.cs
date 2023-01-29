// Copyright © 2014 onwards, Andrew Whewell
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
    /// Extensions to IList&lt;T&gt; that add some functionality present on List&lt;T&gt; but
    /// missing on the interface.
    /// </summary>
    public static class IListExtensions
    {
        /// <summary>
        /// Appends one list to the end of the other.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="range"></param>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> range)
        {
            foreach(var item in range) {
                list.Add(item);
            }
        }

        /// <summary>
        /// Removes all matching entries from the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="match"></param>
        public static void RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            for(var i = list.Count - 1;i >= 0;--i) {
                if(match(list[i])) {
                    list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Returns true if two ILists have the same content.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="other"></param>
        /// <param name="orderMustMatch"></param>
        /// <returns></returns>
        public static bool HasSameContentAs<T>(this IList<T> list, IList<T> other, bool orderMustMatch)
        {
            var result = Object.ReferenceEquals(list, other);
            if(!result) {
                result = list.Count == other.Count;
                if(result) {
                    if(orderMustMatch) {
                        for(var i = 0;result && i < list.Count;++i) {
                            result = Object.Equals(list[i], other[i]);
                        }
                    } else {
                        var unmatched = new LinkedList<T>(other);

                        foreach(var thisItem in list) {
                            result = false;
                            for(var node = unmatched.First;!result && node != null;node = node.Next) {
                                result = Object.Equals(thisItem, node.Value);
                                if(result) {
                                    unmatched.Remove(node);
                                }
                            }
                            if(!result) {
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
