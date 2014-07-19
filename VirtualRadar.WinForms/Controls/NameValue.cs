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
