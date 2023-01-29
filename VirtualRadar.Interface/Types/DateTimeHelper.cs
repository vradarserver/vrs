namespace VirtualRadar.Interface.Types
{
    public static class DateTimeHelper
    {
        public static DateTime Min(DateTime lhs, DateTime rhs) => lhs < rhs ? lhs : rhs;

        public static DateTime? Min(DateTime? lhs, DateTime? rhs, bool nullIsSmallest = true)
        {
            if (!lhs.HasValue) {
                return nullIsSmallest
                    ? lhs
                    : rhs;
            }
            if (!rhs.HasValue) {
                return nullIsSmallest
                    ? rhs
                    : lhs;
            }
            return lhs < rhs ? lhs : rhs;
        }

        public static DateTime Max(DateTime lhs, DateTime rhs) => lhs > rhs ? lhs : rhs;

        public static DateTime? Max(DateTime? lhs, DateTime? rhs, bool nullIsSmallest = true)
        {
            if (!lhs.HasValue) {
                return nullIsSmallest
                    ? rhs
                    : lhs;
            }
            if (!rhs.HasValue) {
                return nullIsSmallest
                    ? lhs
                    : rhs;
            }
            return lhs > rhs ? lhs : rhs;
        }
    }
}
