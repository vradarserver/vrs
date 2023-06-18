namespace VirtualRadar.Interface.Types
{
    /// <summary>
    /// Various utility methods that act on date times.
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// Returns the smaller of two date times.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static DateTime Min(DateTime lhs, DateTime rhs) => lhs < rhs ? lhs : rhs;

        /// <summary>
        /// Returns the smaller of two nullable date times.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="nullIsSmallest"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the larger of two date times.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static DateTime Max(DateTime lhs, DateTime rhs) => lhs > rhs ? lhs : rhs;

        /// <summary>
        /// Returns the larger of two nullable date times.
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <param name="nullIsSmallest"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the date time passed across with the milliseconds component set to zero.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime TruncateMilliseconds(this DateTime dateTime)
        {
            return dateTime.Millisecond == 0
                ? dateTime
                : new DateTime(
                    dateTime.Year, dateTime.Month,  dateTime.Day,
                    dateTime.Hour, dateTime.Minute, dateTime.Second
                );
        }

        /// <summary>
        /// Returns the date time passed across with the milliseconds component set to zero.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime? TruncateMilliseconds(this DateTime? dateTime)
        {
            return dateTime == null
                ? dateTime
                : dateTime.Value.TruncateMilliseconds();
        }
    }
}
