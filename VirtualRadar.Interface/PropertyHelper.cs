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
using System.Linq.Expressions;
using System.Reflection;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Utility methods for working with properties.
    /// </summary>
    public static class PropertyHelper
    {
        /// <summary>
        /// Retrieves the name of a property from the lambda expression passed across. Returns null
        /// if the expression refers to anything other than a simple property.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string ExtractName(Expression expression)
        {
            String result;

            var lambdaExpression = (LambdaExpression)expression;
            var parameter = lambdaExpression.Parameters.FirstOrDefault();

            MemberExpression memberExpression = lambdaExpression.Body.NodeType == ExpressionType.Convert ?
                                                ((UnaryExpression)lambdaExpression.Body).Operand as MemberExpression :
                                                lambdaExpression.Body as MemberExpression;
            if(parameter != null) {
                result = memberExpression.ToString().Substring(parameter.Name.Length + 1);
            } else {
                var propertyInfo = memberExpression == null ? null : memberExpression.Member as PropertyInfo;
                result = propertyInfo == null ? null : propertyInfo.Name;
            }

            return result;
        }

        /// <summary>
        /// Retrieves the name of a property from the lambda expression passed across. Returns null
        /// if the expression refers to anything other than a simple property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="selectorExpression"></param>
        /// <returns></returns>
        public static string ExtractName<T>(Expression<Func<T, object>> selectorExpression)
        {
            return ExtractName((Expression)selectorExpression);
        }

        /// <summary>
        /// Retrieves the name of a property from the lambda expression passed across. Returns null
        /// if the expression refers to anything other than a simple property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="unused"></param>
        /// <param name="selectorExpression"></param>
        /// <returns></returns>
        public static string ExtractName<T>(T unused, Expression<Func<T, object>> selectorExpression)
        {
            return ExtractName((Expression)selectorExpression);
        }
    }
}
