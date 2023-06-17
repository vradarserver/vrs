using VirtualRadar.Interface;

namespace VirtualRadar.Database
{
    static class LinqDynamicQueryExtensions
    {
        /// <summary>
        /// Adds a where clause for the string filter passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="whereEqual"></param>
        /// <param name="whereNotEqual"></param>
        /// <param name="whereIsNull"></param>
        /// <param name="whereIsNotNull"></param>
        /// <param name="whereContains"></param>
        /// <param name="whereNotContains"></param>
        /// <param name="whereStartsWith"></param>
        /// <param name="whereNotStartsWith"></param>
        /// <param name="whereEndsWith"></param>
        /// <param name="whereNotEndsWith"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IQueryable<T> WhereStringFilter<T>(
            this IQueryable<T> query,
            FilterString filter,
            Func<IQueryable<T>, IQueryable<T>> whereEqual,
            Func<IQueryable<T>, IQueryable<T>> whereNotEqual,
            Func<IQueryable<T>, IQueryable<T>> whereIsNull,
            Func<IQueryable<T>, IQueryable<T>> whereIsNotNull,
            Func<IQueryable<T>, IQueryable<T>> whereContains,
            Func<IQueryable<T>, IQueryable<T>> whereNotContains,
            Func<IQueryable<T>, IQueryable<T>> whereStartsWith,
            Func<IQueryable<T>, IQueryable<T>> whereNotStartsWith,
            Func<IQueryable<T>, IQueryable<T>> whereEndsWith,
            Func<IQueryable<T>, IQueryable<T>> whereNotEndsWith
        )
        {
            if(filter != null) {
                switch(filter.Condition) {
                    case FilterCondition.Contains:
                        query = !filter.ReverseCondition
                            ? whereContains(query)
                            : whereNotContains(query);
                        break;
                    case FilterCondition.EndsWith:
                        query = !filter.ReverseCondition
                            ? whereEndsWith(query)
                            : whereNotEndsWith(query);
                        break;
                    case FilterCondition.StartsWith:
                        query = !filter.ReverseCondition
                            ? whereStartsWith(query)
                            : whereNotStartsWith(query);
                        break;
                    case FilterCondition.Equals:
                        var matchNull = String.IsNullOrEmpty(filter.Value);
                        if(!filter.ReverseCondition) {
                            query = matchNull
                                ? whereIsNull(query)
                                : whereEqual(query);
                        } else {
                            query = matchNull
                                ? whereIsNotNull(query)
                                : whereNotEqual(query);
                        }
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return query;
        }

        private static IQueryable<T> ApplyRangeFilter<T>(
            IQueryable<T> query,
            Filter filter,
            Func<bool> filterHasLowerValue,
            Func<bool> filterHasUpperValue,
            Func<IQueryable<T>, IQueryable<T>> whereLower,
            Func<IQueryable<T>, IQueryable<T>> whereUpper,
            Func<IQueryable<T>, IQueryable<T>> whereNotLower,
            Func<IQueryable<T>, IQueryable<T>> whereNotUpper,
            Func<IQueryable<T>, IQueryable<T>> whereNotLowerAndUpper
        )
        {
            if(filter != null) {
                var hasLower = filterHasLowerValue();
                var hasUpper = filterHasUpperValue();

                if(!filter.ReverseCondition) {
                    if(hasLower) {
                        query = whereLower(query);
                    }
                    if(hasUpper) {
                        query = whereUpper(query);
                    }
                } else {
                    if(hasLower && hasUpper) {
                        query = whereNotLowerAndUpper(query);
                    } else if(hasLower) {
                        query = whereNotLower(query);
                    } else if(hasUpper) {
                        query = whereNotUpper(query);
                    }
                }
            }

            return query;
        }

        /// <summary>
        /// Adds a where clause for the date range filter passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="whereLower"></param>
        /// <param name="whereUpper"></param>
        /// <param name="whereNotLower"></param>
        /// <param name="whereNotUpper"></param>
        /// <param name="whereNotLowerAndUpper"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereDateRangeFilter<T>(
            this IQueryable<T> query,
            FilterRange<DateTime> filter,
            Func<IQueryable<T>, IQueryable<T>> whereLower,
            Func<IQueryable<T>, IQueryable<T>> whereUpper,
            Func<IQueryable<T>, IQueryable<T>> whereNotLower,
            Func<IQueryable<T>, IQueryable<T>> whereNotUpper,
            Func<IQueryable<T>, IQueryable<T>> whereNotLowerAndUpper
        )
        {
            return ApplyRangeFilter(
                query,
                filter,
                () => filter.LowerValue != null && filter.LowerValue.Value.Year != DateTime.MinValue.Year,
                () => filter.UpperValue != null && filter.UpperValue.Value.Year != DateTime.MaxValue.Year,
                whereLower,
                whereUpper,
                whereNotLower,
                whereNotUpper,
                whereNotLowerAndUpper
            );
        }

        /// <summary>
        /// Adds a where clause for the integer range filter passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="whereLower"></param>
        /// <param name="whereUpper"></param>
        /// <param name="whereNotLower"></param>
        /// <param name="whereNotUpper"></param>
        /// <param name="whereNotLowerAndUpper"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereIntRangeFilter<T>(
            this IQueryable<T> query,
            FilterRange<int> filter,
            Func<IQueryable<T>, IQueryable<T>> whereLower,
            Func<IQueryable<T>, IQueryable<T>> whereUpper,
            Func<IQueryable<T>, IQueryable<T>> whereNotLower,
            Func<IQueryable<T>, IQueryable<T>> whereNotUpper,
            Func<IQueryable<T>, IQueryable<T>> whereNotLowerAndUpper
        )
        {
            return ApplyRangeFilter(
                query,
                filter,
                () => filter.LowerValue != null && filter.LowerValue.Value != int.MinValue,
                () => filter.UpperValue != null && filter.UpperValue.Value != int.MaxValue,
                whereLower,
                whereUpper,
                whereNotLower,
                whereNotUpper,
                whereNotLowerAndUpper
            );
        }

        /// <summary>
        /// Adds a where clause for a boolean filter.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="filter"></param>
        /// <param name="whereEqual"></param>
        /// <param name="whereNotEqual"></param>
        /// <returns></returns>
        public static IQueryable<T> WhereBoolFilter<T>(
            this IQueryable<T> query,
            FilterBool filter,
            Func<IQueryable<T>, IQueryable<T>> whereEqual,
            Func<IQueryable<T>, IQueryable<T>> whereNotEqual
        )
        {
            if(filter != null && filter.Condition == FilterCondition.Equals) {
                query = !filter.ReverseCondition
                    ? whereEqual(query)
                    : whereNotEqual(query);
            }

            return query;
        }

        /// <summary>
        /// Adds an arbitrary number of OrderBy / ThenBy clauses for the sort conditions
        /// passed across.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="sortConditions"></param>
        /// <param name="sortColumns"></param>
        /// <returns></returns>
        public static IQueryable<T> AddSortConditionSorting<T>(
            this IQueryable<T> query,
            IEnumerable<SortCondition> sortConditions,
            params SortColumnClauses<T>[] sortColumns
        )
        {
            IOrderedQueryable<T> orderedQuery = null;

            foreach(var sortCondition in sortConditions.Where(con => !String.IsNullOrWhiteSpace(con.Column))) {
                var sortColumn = sortColumns.FirstOrDefault(r => String.Equals(r.Column, sortCondition.Column, StringComparison.InvariantCultureIgnoreCase));
                if(sortColumn != default) {
                    if(orderedQuery == null) {
                        orderedQuery = sortCondition.Ascending
                            ? sortColumn.OrderBy(query)
                            : sortColumn.OrderByDescending(query);
                    } else {
                        orderedQuery = sortCondition.Ascending
                            ? sortColumn.ThenBy(orderedQuery)
                            : sortColumn.ThenByDescending(orderedQuery);
                    }
                    query = orderedQuery;
                }
            }

            return query;
        }

        /// <summary>
        /// Adds Skip and Take parameters when from and to rows indicate a subset is required.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="fromRow"></param>
        /// <param name="toRow"></param>
        /// <returns></returns>
        public static IQueryable<T> AddSkipAndTake<T>(this IQueryable<T> query, int fromRow, int toRow)
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
