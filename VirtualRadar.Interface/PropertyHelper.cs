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
            var lambdaExpression = (LambdaExpression)expression;

            MemberExpression memberExpression = lambdaExpression.Body.NodeType == ExpressionType.Convert ?
                                                ((UnaryExpression)lambdaExpression.Body).Operand as MemberExpression :
                                                lambdaExpression.Body as MemberExpression;
            var propertyInfo = memberExpression == null ? null : memberExpression.Member as PropertyInfo;

            return propertyInfo == null ? null : propertyInfo.Name;
        }
    }
}
