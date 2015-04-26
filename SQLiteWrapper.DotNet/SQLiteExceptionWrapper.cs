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
using VirtualRadar.Interface.SQLite;
#if DOTNET_BUILD
    using System.Data.SQLite;
#else
    using Mono.Data.Sqlite;
#endif

namespace VirtualRadar.SQLiteWrapper
{
    /// <summary>
    /// See interface docs.
    /// </summary>
    class SQLiteExceptionWrapper : ISQLiteException
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsSQLiteException
        {
            get { return Exception != null; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public VirtualRadar.Interface.SQLite.SQLiteErrorCode ErrorCode
        {
            get { 
                return _Exception == null ? VirtualRadar.Interface.SQLite.SQLiteErrorCode.Ok
                                          : (VirtualRadar.Interface.SQLite.SQLiteErrorCode)((int)_Exception.ErrorCode);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsLocked
        {
            get { return ErrorCode == Interface.SQLite.SQLiteErrorCode.Busy || ErrorCode == Interface.SQLite.SQLiteErrorCode.Locked; }
        }

        #if DOTNET_BUILD
            System.Data.SQLite.SQLiteException _Exception;
        #else
            Mono.Data.Sqlite.SqliteException _Exception;
        #endif
        /// <summary>
        /// See interface docs.
        /// </summary>
        public Exception Exception
        {
            get { return _Exception; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="ex"></param>
        public void Initialise(Exception ex)
        {
            #if DOTNET_BUILD
                _Exception = ex as System.Data.SQLite.SQLiteException;
            #else
                _Exception = ex as Mono.Data.Sqlite.SqliteException;
            #endif
        }
    }
}
