using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Records an exception and the time (at UTC) when it was thrown.
    /// </summary>
    public class TimestampedException
    {
        /// <summary>
        /// Gets the date and time at UTC when the exception was thrown.
        /// </summary>
        public DateTime TimeUtc { get; private set; }

        /// <summary>
        /// Gets the exception that was thrown.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="ex"></param>
        public TimestampedException(Exception exception) : this(DateTime.UtcNow, exception)
        {
        }

        /// <summary>
        /// Creates a new object
        /// </summary>
        /// <param name="timeUtc"></param>
        /// <param name="exception"></param>
        public TimestampedException(DateTime timeUtc, Exception exception)
        {
            TimeUtc = timeUtc;
            Exception = exception;
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0:HH:mm:ss.sss} {1}", TimeUtc, Exception == null ? null : Exception.Message);
        }
    }
}
