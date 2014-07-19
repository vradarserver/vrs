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
            var result = new List<NameValue<T>>();

            foreach(T value in Enum.GetValues(typeof(T))) {
                if(filterValue != null && !filterValue(value)) continue;
                var name = describeValue == null ? value.ToString() : describeValue(value);
                result.Add(new NameValue<T>(value, name));
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
