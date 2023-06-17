namespace VirtualRadar.Database
{
    class SortCondition
    {
        public string Column { get; }

        public bool Ascending { get; }

        public SortCondition(string column, bool ascending)
        {
            Column = column;
            Ascending = ascending;
        }

        public override string ToString() => $"{Column} {(Ascending ? "ASC" : "DESC")}";
    }
}
