// Copyright © 2012 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using VirtualRadar.Localisation;
using System.Collections;

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// The base class for all configuration screen sheets.
    /// </summary>
    abstract class Sheet<T> : ISheet
    {
        #region Fields
        /// <summary>
        /// A dictionary of property names to the initial value of the property after the presenter filled them in.
        /// </summary>
        private Dictionary<string, object> _InitialValues = new Dictionary<string,object>();
        #endregion

        #region Properties
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<ParentPage> Pages { get; private set; }

        /// <summary>
        /// The title for the sheet in the list of sheets.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public abstract string SheetTitle { get; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public object Tag { get; set; }
        #endregion

        #region Static constructor
        static Sheet()
        {
            SetPropertyDisplayOrder();
        }
        #endregion

        #region Constructor
        public Sheet()
        {
            Pages = new List<ParentPage>();
        }
        #endregion

        #region ToString
        /// <summary>
        /// Returns a description of the sheet.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SheetTitle;
        }
        #endregion

        #region SetInitialValues, ValueHasChanged
        /// <summary>
        /// Records the initial values of all properties on the sheet.
        /// </summary>
        public void SetInitialValues()
        {
            _InitialValues.Clear();
            foreach(var property in GetType().GetProperties()) {
                var isBrowsable = true;
                var browsableAttribute = property.GetCustomAttributes(typeof(BrowsableAttribute), true).OfType<BrowsableAttribute>().FirstOrDefault();
                if(browsableAttribute != null) isBrowsable = browsableAttribute.Browsable;
                if(!isBrowsable) continue;

                var initialValue = property.GetValue(this, null);

                if(IsList(initialValue)) initialValue = CopyList((IList)initialValue);
                else {
                    var cloneable = initialValue as ICloneable;
                    if(cloneable != null) initialValue = cloneable.Clone();
                }

                _InitialValues.Add(property.Name, initialValue);
            }
        }

        /// <summary>
        /// Returns true if the current value of the object is not the same as its initial value.
        /// </summary>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        protected bool ValueHasChanged(Expression<Func<T, object>> propertyExpression)
        {
            var lambdaExpression = (LambdaExpression)propertyExpression;

            MemberExpression memberExpression = lambdaExpression.Body.NodeType == ExpressionType.Convert ?
                                                ((UnaryExpression)lambdaExpression.Body).Operand as MemberExpression :
                                                lambdaExpression.Body as MemberExpression;
            var propertyInfo = memberExpression == null ? null : memberExpression.Member as PropertyInfo;

            object initialValue;
            var result = _InitialValues.TryGetValue(propertyInfo.Name, out initialValue);
            if(result) {
                var currentValue = propertyInfo.GetValue(this, null);
                if(initialValue != null && currentValue != null) {
                    result = !IsList(initialValue) ? !initialValue.Equals(currentValue) : !CompareLists((IList)initialValue, (IList)currentValue);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the object is a list.
        /// </summary>
        /// <param name="initialValue"></param>
        /// <returns></returns>
        private static bool IsList(object initialValue)
        {
            return initialValue != null && typeof(IList).IsAssignableFrom(initialValue.GetType());
        }

        /// <summary>
        /// Returns a shallow copy of the list passed across.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private static IList CopyList(IList list)
        {
            var result = list == null ? null : (IList)Activator.CreateInstance(list.GetType());
            if(list != null) {
                foreach(var item in list) {
                    if(item == null) result.Add(item);
                    else {
                        var itemType = item.GetType();
                        if(itemType.IsValueType) result.Add(Convert.ChangeType(item, itemType));
                        else {
                            if(!typeof(ICloneable).IsAssignableFrom(itemType)) throw new InvalidOperationException("Items in options lists need to be deep-copy cloneable and they must override Equals");
                            result.Add(item == null ? null : ((ICloneable)item).Clone());
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the two lists contain the same number of items and every item in each list compares as equal,
        /// although they don't need to necessarily be in the same order.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        private static bool CompareLists(IList lhs, IList rhs)
        {
            bool result = lhs.Count == rhs.Count;
            if(result) {
                var rhsCopy = new ArrayList();
                rhsCopy.AddRange(rhs);

                foreach(var lhsItem in lhs) {
                    var foundMatch = false;
                    for(var i = 0;!foundMatch && i < rhsCopy.Count;++i) {
                        var rhsItem = rhsCopy[i];
                        foundMatch = Convert.Equals(lhsItem, rhsItem);
                        if(foundMatch) rhsCopy.RemoveAt(i);
                    }

                    if(!foundMatch) {
                        result = false;
                        break;
                    }
                }
            }

            return result;
        }
        #endregion

        #region SetPropertyDisplayOrder
        /// <summary>
        /// Forces the property grid to display the properties in the correct order.
        /// </summary>
        /// <remarks>
        /// Ordinarily you would set the display order using a type converter on the parent object and overriding
        /// GetProperties, but that doesn't work under Mono. It didn't appear to even be calling the converter. So
        /// that was a wash. This is take two - when in doubt, use brute force. The LocalisedDisplayName attribute
        /// that is providing the translated display names has been changed to add a variable number of tabs to the
        /// start of the text and the same kludge that was used to get the categories to appear in the correct order
        /// will now be used with the property names. The properties that should be displayed at the top of the grid
        /// will have the largest number of leading tabs added to them while the last property will have the fewest.
        /// The order of the properties will be determined by the DisplayOrder attribute.
        /// </remarks>
        public static void SetPropertyDisplayOrder()
        {
            var groupedProperties = typeof(T).GetProperties().Select(r => {
                var displayOrderAttribute = (DisplayOrderAttribute)r.GetCustomAttributes(typeof(DisplayOrderAttribute), true).FirstOrDefault();
                var categoryAttribute = (LocalisedCategoryAttribute)r.GetCustomAttributes(typeof(LocalisedCategoryAttribute), true).FirstOrDefault();
                var displayNameAttribute = (LocalisedDisplayNameAttribute)r.GetCustomAttributes(typeof(LocalisedDisplayNameAttribute), true).FirstOrDefault();

                return new {
                    DisplayOrder = displayOrderAttribute == null ? -1 : displayOrderAttribute.DisplayOrder,
                    DisplayNameAttribute = displayNameAttribute,
                    CategoryAttribute = categoryAttribute,
                    Property = r,
                };
            }).GroupBy(r => r.CategoryAttribute, r => r);

            foreach(var properties in groupedProperties) {
                var countPropertiesInCategory = properties.Count();
                int count = 0;
                foreach(var sortedProperty in properties.OrderBy(r => r.DisplayOrder)) {
                    var categoryPrefix = sortedProperty.CategoryAttribute == null ? "" : sortedProperty.CategoryAttribute.Prefix;
                    if(sortedProperty.DisplayNameAttribute != null) sortedProperty.DisplayNameAttribute.ForceMonoSortOrder(categoryPrefix, count++, countPropertiesInCategory);
                }
            }
        }
        #endregion

        #region GetChildPages
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public virtual List<ParentPage> GetParentPages()
        {
            return new List<ParentPage>();
        }
        #endregion
    }
}
