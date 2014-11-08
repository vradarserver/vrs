// Copyright © 2014 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.SQLite
{
    /// <summary>
    /// An enumeration of the different error codes that can be exposed in an <see cref="ISQLiteException"/>.
    /// </summary>
    /// <remarks>
    /// These are copied verbatim from the SQLite source.
    /// </remarks>
    public enum SQLiteErrorCode
    {
        /// <summary>
        /// The error code is unknown.  This error code
        /// is only used by the managed wrapper itself.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Successful result
        /// </summary>
        Ok /* 0 */,

        /// <summary>
        /// SQL error or missing database
        /// </summary>
        Error /* 1 */,

        /// <summary>
        /// Internal logic error in SQLite
        /// </summary>
        Internal /* 2 */,

        /// <summary>
        /// Access permission denied
        /// </summary>
        Perm /* 3 */,

        /// <summary>
        /// Callback routine requested an abort
        /// </summary>
        Abort /* 4 */,

        /// <summary>
        /// The database file is locked
        /// </summary>
        Busy /* 5 */,

        /// <summary>
        /// A table in the database is locked
        /// </summary>
        Locked /* 6 */,

        /// <summary>
        /// A malloc() failed
        /// </summary>
        NoMem /* 7 */,

        /// <summary>
        /// Attempt to write a readonly database
        /// </summary>
        ReadOnly /* 8 */,

        /// <summary>
        /// Operation terminated by sqlite3_interrupt()
        /// </summary>
        Interrupt /* 9 */,

        /// <summary>
        /// Some kind of disk I/O error occurred
        /// </summary>
        IoErr /* 10 */,

        /// <summary>
        /// The database disk image is malformed
        /// </summary>
        Corrupt /* 11 */,

        /// <summary>
        /// Unknown opcode in sqlite3_file_control()
        /// </summary>
        NotFound /* 12 */,

        /// <summary>
        /// Insertion failed because database is full
        /// </summary>
        Full /* 13 */,

        /// <summary>
        /// Unable to open the database file
        /// </summary>
        CantOpen /* 14 */,

        /// <summary>
        /// Database lock protocol error
        /// </summary>
        Protocol /* 15 */,

        /// <summary>
        /// Database is empty
        /// </summary>
        Empty /* 16 */,

        /// <summary>
        /// The database schema changed
        /// </summary>
        Schema /* 17 */,

        /// <summary>
        /// String or BLOB exceeds size limit
        /// </summary>
        TooBig /* 18 */,

        /// <summary>
        /// Abort due to constraint violation
        /// </summary>
        Constraint /* 19 */,

        /// <summary>
        /// Data type mismatch
        /// </summary>
        Mismatch /* 20 */,

        /// <summary>
        /// Library used incorrectly
        /// </summary>
        Misuse /* 21 */,

        /// <summary>
        /// Uses OS features not supported on host
        /// </summary>
        NoLfs /* 22 */,

        /// <summary>
        /// Authorization denied
        /// </summary>
        Auth /* 23 */,

        /// <summary>
        /// Auxiliary database format error
        /// </summary>
        Format /* 24 */,

        /// <summary>
        /// 2nd parameter to sqlite3_bind out of range
        /// </summary>
        Range /* 25 */,

        /// <summary>
        /// File opened that is not a database file
        /// </summary>
        NotADb /* 26 */,

        /// <summary>
        /// Notifications from sqlite3_log()
        /// </summary>
        Notice /* 27 */,

        /// <summary>
        /// Warnings from sqlite3_log()
        /// </summary>
        Warning /* 28 */,

        /// <summary>
        /// sqlite3_step() has another row ready
        /// </summary>
        Row = 100,

        /// <summary>
        /// sqlite3_step() has finished executing
        /// </summary>
        Done, /* 101 */

        /// <summary>
        /// Used to mask off extended result codes
        /// </summary>
        NonExtendedMask = 0xFF
    }
}
