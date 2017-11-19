// Copyright © 2016 onwards, Andrew Whewell
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
    /// A static class with methods that can help when dealing with collections.
    /// </summary>
    public static class CollectionHelper
    {
        /// <summary>
        /// Returns a copy of the list passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> ShallowCopy<T>(List<T> list)
        {
            return new List<T>(list);
        }

        /// <summary>
        /// Returns a copy of the dictionary passed across.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ShallowCopy<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            var result = new Dictionary<TKey, TValue>(dictionary.Comparer);
            foreach(var kvp in dictionary) {
                result.Add(kvp.Key, kvp.Value);
            }

            return result;
        }

        /// <summary>
        /// Returns a copy of the hashset passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hashSet"></param>
        /// <returns></returns>
        public static HashSet<T> ShallowCopy<T>(HashSet<T> hashSet)
        {
            var result = new HashSet<T>(hashSet.Comparer);
            foreach(var item in hashSet) {
                result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Generic (and not particularly efficient) method that takes two generic collections, where the generic types
        /// are related to each other in some way and each row has something that is unique about it, and applies add,
        /// update and delete operations to the destination so that the content matches the source.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="source">The source collection.</param>
        /// <param name="destination">The destination collection.</param>
        /// <param name="areIdentifiersEqual">Passed a source and destination record, returns true if they have the same identifier.</param>
        /// <param name="createDestinationRecord">Creates a new destination record from a source record.</param>
        /// <param name="overwriteDestinationRecord">Overwrites a destination record with values from a source record.</param>
        public static void ApplySourceToDestination<TSource, TDest>(ICollection<TSource> source, ICollection<TDest> destination, Func<TSource, TDest, bool> areIdentifiersEqual, Func<TSource, TDest> createDestinationRecord, Action<TSource, TDest> overwriteDestinationRecord)
        {
            var deleteRows = destination.Where(r => !source.Any(i => areIdentifiersEqual(i, r))).ToArray();
            foreach(var deleteRow in deleteRows) {
                destination.Remove(deleteRow);
            }

            foreach(var sourceRow in source) {
                var overwroteDest = false;
                foreach(var destRow in destination) {
                    if(areIdentifiersEqual(sourceRow, destRow)) {
                        overwriteDestinationRecord(sourceRow, destRow);
                        overwroteDest = true;
                        break;
                    }
                }

                if(!overwroteDest) {
                    var destRow = createDestinationRecord(sourceRow);
                    destination.Add(destRow);
                }
            }
        }

        /// <summary>
        /// A version of <see cref="ApplySourceToDestination"/> that is suitable for synchronising lists of values. There is no
        /// conversion between types and the source and destination must both implement IList. Values are not compared, the
        /// destination elements are just overwritten with the source.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        public static void OverwriteDestinationWithSource<T>(IList<T> source, IList<T> destination)
        {
            for(var i = 0;i < source.Count;++i) {
                if(destination.Count <= i) {
                    destination.Add(source[i]);
                } else {
                    destination[i] = source[i];
                }
            }

            while(destination.Count > source.Count) {
                destination.RemoveAt(source.Count);
            }
        }
    }
}
