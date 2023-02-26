// Copyright © 2014 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

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
        /// <param name="exception"></param>
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
