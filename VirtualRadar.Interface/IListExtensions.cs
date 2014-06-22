using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        /// <returns></returns>
        public static bool HasSameContentAs(this IList list, IList other)
        {
            var result = Object.ReferenceEquals(list, other);
            if(!result) {
                result = list.Count == other.Count;
                if(result) {
                    for(var i = 0;i < list.Count;++i) {
                        result = Object.Equals(list[i], other[i]);
                        if(!result) break;
                    }
                }
            }

            return result;
        }
    }
}
