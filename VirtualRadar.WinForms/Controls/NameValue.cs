// Copyright © 2014 onwards, Andrew Whewell
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

namespace VirtualRadar.WinForms.Controls
{
    /// <summary>
    /// A class that holds a name and a value
    /// </summary>
    public class NameValue<T>
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        public NameValue(T value, string name)
        {
            Value = value;
            Name = name;
        }

        /// <summary>
        /// Returns a list of NameValues for every value in an enum type.
        /// </summary>
        /// <param name="describeValue"></param>
        /// <param name="filterValue"></param>
        /// <param name="sortList"></param>
        /// <returns></returns>
        public static List<NameValue<T>> EnumList(Func<T, string> describeValue, Func<T, bool> filterValue = null, bool sortList = true)
        {
            return CreateList(Enum.GetValues(typeof(T)).OfType<T>(), describeValue, filterValue, sortList);
        }

        /// <summary>
        /// Returns a list of NameValues for every value in the list.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="describeValue"></param>
        /// <param name="filterValue"></param>
        /// <param name="sortList"></param>
        /// <returns></returns>
        public static List<NameValue<T>> CreateList(IEnumerable<T> list, Func<T, string> describeValue = null, Func<T, bool> filterValue = null, bool sortList = true)
        {
            var result = new List<NameValue<T>>();

            foreach(var item in list) {
                if(filterValue != null && !filterValue(item)) continue;
                var name = describeValue == null ? ((object)item) == null ? "" : item.ToString() : describeValue(item) ?? "";
                result.Add(new NameValue<T>(item, name));
            }

            if(sortList) {
                result.Sort((lhs, rhs) => {
                    return String.Compare(lhs.Name, rhs.Name, StringComparison.CurrentCultureIgnoreCase);
                });
            }

            return result;
        }
    }
}
