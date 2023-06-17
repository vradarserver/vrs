namespace VirtualRadar.Database
{
    class SortColumnClauses<T>
    {
        public string Column { get; }

        public Func<IQueryable<T>, IOrderedQueryable<T>> OrderBy { get; }

        public Func<IQueryable<T>, IOrderedQueryable<T>> OrderByDescending { get; }

        public Func<IOrderedQueryable<T>, IOrderedQueryable<T>> ThenBy { get; }

        public Func<IOrderedQueryable<T>, IOrderedQueryable<T>> ThenByDescending { get; }

        public SortColumnClauses(
            string column,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderByDescending,
            Func<IOrderedQueryable<T>, IOrderedQueryable<T>> thenBy,
            Func<IOrderedQueryable<T>, IOrderedQueryable<T>> thenByDescending
        )
        {
            Column = column;
            OrderBy = orderBy;
            OrderByDescending = orderByDescending;
            ThenBy = thenBy;
            ThenByDescending = thenByDescending;
        }
    }
}
