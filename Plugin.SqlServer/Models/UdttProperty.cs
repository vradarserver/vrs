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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Plugin.SqlServer.Models
{
    /// <summary>
    /// Describes a property in a UDTT model and its position relative to other properties.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UdttProperty<T>
    {
        /// <summary>
        /// Gets or sets the property's ordinal number.
        /// </summary>
        public int? Ordinal { get; private set; }

        /// <summary>
        /// Gets or sets the PropertyInfo for the property.
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// Gets or sets a compiled function that, given an instance of T, can return the value for the property.
        /// </summary>
        public Func<T, object> GetFunc { get; private set; }

        /// <summary>
        /// Returns an array of UDTT properties for an object.
        /// </summary>
        /// <returns></returns>
        public static UdttProperty<T>[] GetUdttProperties()
        {
            var properties = typeof(T).GetProperties()
            .Select(r => {
                var ordinalAttribute = r.GetCustomAttribute<OrdinalAttribute>();
                return new UdttProperty<T> {
                    Ordinal  = ordinalAttribute == null ? (int?)null : ordinalAttribute.Number,
                    Property = r,
                };
            })
            .Where(r => r.Ordinal != null)
            .Select(r => {
                var parameter = Expression.Parameter(typeof(T), "r");
                Expression<Func<T, object>> getExpression;
                if(r.Property.PropertyType.IsValueType) {
                    getExpression = Expression.Lambda<Func<T, object>>(
                        Expression.TypeAs(Expression.Property(parameter, r.Property.Name), typeof(object))
                    , parameter);
                } else {
                    getExpression = Expression.Lambda<Func<T, object>>(
                        Expression.Property(parameter, r.Property.Name)
                    , parameter);
                }
                r.GetFunc = getExpression.Compile();
                return r;
            })
            .ToDictionary(r => r.Ordinal, r => r);

            return properties.OrderBy(r => r.Key).Select(r => r.Value).ToArray();
        }
    }
}
