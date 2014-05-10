// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Database
{
    /// <summary>
    /// A collection of methods to help when working with date/times that are being written to a SQLite database.
    /// </summary>
    static class SQLiteDateHelper
    {
        /// <summary>
        /// Returns a new DateTime that has the milliseconds removed.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        /// <remarks><para>
        /// SQLite is perfectly capable of storing milliseconds but for whatever reason, I don't know why, when they are
        /// written by IQToolkit you get a 6 digit millisecond - like this:</para>
        /// <code>
        /// sqlite&gt; select * from sessions;
        /// 1|1|2012-01-24 10:42:20.527875|2012-01-24 10:42:29.465375
        /// </code>
        /// <para>
        /// This can cause .NET applications that aren't using IQToolkit to crash when they try to read the date as a string
        /// and then parse it. I tried setting the date format to ISO8601, which should truncate milliseconds entirely, but
        /// that didn't make any difference.
        /// </para>
        /// <para>This is no longer strictly needed as I've stopped using IQToolkit (it crashes under Mono) but I've left it in here for now.</para>
        /// </remarks>
        public static DateTime Truncate(DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
        }

        /// <summary>
        /// Returns a new DateTime that has the milliseconds removed.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        /// <remarks><para>
        /// SQLite is perfectly capable of storing milliseconds but for whatever reason, I don't know why, when they are
        /// written by IQToolkit you get a 6 digit millisecond - like this:</para>
        /// <code>
        /// sqlite&gt; select * from sessions;
        /// 1|1|2012-01-24 10:42:20.527875|2012-01-24 10:42:29.465375
        /// </code>
        /// <para>
        /// This can cause .NET applications that aren't using IQToolkit to crash when they try to read the date as a string
        /// and then parse it. I tried setting the date format to ISO8601, which should truncate milliseconds entirely, but
        /// that didn't make any difference.
        /// </para>
        /// <para>This is no longer strictly needed as I've stopped using IQToolkit (it crashes under Mono) but I've left it in here for now.</para>
        /// </remarks>
        public static DateTime? Truncate(DateTime? dateTime)
        {
            return dateTime == null ? (DateTime?)null : Truncate(dateTime.Value);
        }
    }
}
