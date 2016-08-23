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

namespace VirtualRadar.Interface.View
{
    /// <summary>
    /// Holds an enum value as an integer and a localised description of the enum value. Used to carry lists
    /// of enums (or other fixed values) to web interfaces.
    /// </summary>
    public class EnumModel
    {
        /// <summary>
        /// Gets or sets the enum value.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// Gets or sets the localised description of the enum.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public EnumModel() : this(0, null)
        {
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="description"></param>
        public EnumModel(int value, string description)
        {
            Value = value;
            Description = description;
        }

        /// <summary>
        /// Creates a sorted array of records for an enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getDescription"></param>
        /// <param name="sortByDescription"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static EnumModel[] CreateFromEnum<T>(Func<T, string> getDescription, bool sortByDescription = true, Func<T, bool> filter = null)
            where T: struct
        {
            var result = new List<EnumModel>();

            foreach(T value in Enum.GetValues(typeof(T))) {
                if(filter == null || filter(value)) {
                    result.Add(new EnumModel((int)Convert.ChangeType(value, typeof(Int32)), getDescription(value)));
                }
            }

            if(sortByDescription) {
                result.Sort((lhs, rhs) => String.Compare(lhs.Description, rhs.Description));
            }

            return result.ToArray();
        }

        /// <summary>
        /// Returns true if the value passed across is an valid value for the enum type parameter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInRange<T>(int value)
        {
            var result = false;

            foreach(T enumValue in Enum.GetValues(typeof(T))) {
                if((int)Convert.ChangeType(enumValue, typeof(Int32)) == value) {
                    result = true;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Casts an integer back to an enum value. Throws an InvalidCastException if the integer is not one of the enum's values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T CastFromInt<T>(int value)
        {
            if(!IsInRange<T>(value)) {
                throw new InvalidCastException($"Attempted to cast {value} to {typeof(T).Name}");
            }

            return (T)Enum.ToObject(typeof(T), value);
        }
    }
}
