using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Database
{
    static class LinqDynamicQueryExtensions
    {
        /// <summary>
        /// Adds Skip and Take parameters when from and to rows indicate a subset is required.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <returns></returns>
        public static IQueryable<T> SkipAndTake<T>(this IQueryable<T> query, int fromRow, int toRow)
        {
            fromRow = Math.Max(0, fromRow);
            if(fromRow > 0) {
                query = query.Skip(fromRow);
            }
            if(toRow >= fromRow) {
                query = query.Take((toRow - fromRow) + 1);
            }

            return query;
        }
    }
}
