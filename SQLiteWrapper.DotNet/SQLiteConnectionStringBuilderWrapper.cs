// Copyright © 2013 onwards, Andrew Whewell
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
using System.Data;
#if DOTNET_BUILD
    using System.Data.SQLite;
#else
    using Mono.Data.Sqlite;
#endif

namespace VirtualRadar.SQLiteWrapper
{
    /// <summary>
    /// The .NET and Mono implementations of <see cref="ISQLiteConnectionStringBuilder"/>.
    /// </summary>
    class SQLiteConnectionStringBuilderWrapper : VirtualRadar.Interface.SQLite.ISQLiteConnectionStringBuilder
    {
        #if DOTNET_BUILD
            private SQLiteConnectionStringBuilder _Wrapped;
        #else
            private SqliteConnectionStringBuilder _Wrapped;
        #endif

        public Interface.SQLite.ISQLiteConnectionStringBuilder Initialise()
        {
            #if DOTNET_BUILD
                _Wrapped = new SQLiteConnectionStringBuilder();
            #else
                _Wrapped = new SqliteConnectionStringBuilder();
            #endif
            return this;
        }

        public Interface.SQLite.ISQLiteConnectionStringBuilder Initialise(string connectionString)
        {
            #if DOTNET_BUILD
                _Wrapped = new SQLiteConnectionStringBuilder(connectionString);
            #else
                _Wrapped = new SqliteConnectionStringBuilder(connectionString);
            #endif
            return this;
        }

        public bool BinaryGUID
        {
            get { return _Wrapped.BinaryGUID; }
            set { _Wrapped.BinaryGUID = value; }
        }

        public int CacheSize
        {
            get { return _Wrapped.CacheSize; }
            set { _Wrapped.CacheSize = value; }
        }

        public string ConnectionString
        {
            get { return _Wrapped.ConnectionString; }
            set { _Wrapped.ConnectionString = value; }
        }

        public string DataSource
        {
            get { return _Wrapped.DataSource; }
            set { _Wrapped.DataSource = value; }
        }

        public Interface.SQLite.SQLiteDateFormats DateTimeFormat
        {
            get { return ConvertSQLite.ToSQLiteDateFormats(_Wrapped.DateTimeFormat); }
            set { _Wrapped.DateTimeFormat = ConvertSQLite.ToSQLiteDateFormats(value); }
        }

        public IsolationLevel DefaultIsolationLevel
        {
            get { return _Wrapped.DefaultIsolationLevel; }
            set { _Wrapped.DefaultIsolationLevel = value; }
        }

        public int DefaultTimeout
        {
            get { return _Wrapped.DefaultTimeout; }
            set { _Wrapped.DefaultTimeout = value; }
        }

        public bool Enlist
        {
            get { return _Wrapped.Enlist; }
            set { _Wrapped.Enlist = value; }
        }

        public bool FailIfMissing
        {
            get { return _Wrapped.FailIfMissing; }
            set { _Wrapped.FailIfMissing = value; }
        }

        #if DOTNET_BUILD
        public bool ForeignKeys
        {
            get { return _Wrapped.ForeignKeys; }
            set { _Wrapped.ForeignKeys = value; }
        }
        #else
        public bool ForeignKeys
        {
            get { return false; }
            set { ; }
        }
        #endif

        public Interface.SQLite.SQLiteJournalModeEnum JournalMode
        {
            get { return ConvertSQLite.ToSQLiteJournalModeEnum(_Wrapped.JournalMode); }
            set { _Wrapped.JournalMode = ConvertSQLite.ToSQLiteJournalModeEnum(value); }
        }

        public bool LegacyFormat
        {
            get { return _Wrapped.LegacyFormat; }
            set { _Wrapped.LegacyFormat = value; }
        }

        public int MaxPageCount
        {
            get { return _Wrapped.MaxPageCount; }
            set { _Wrapped.MaxPageCount = value; }
        }

        public int PageSize
        {
            get { return _Wrapped.PageSize; }
            set { _Wrapped.PageSize = value; }
        }

        public string Password
        {
            get { return _Wrapped.Password; }
            set { _Wrapped.Password = value; }
        }

        public bool Pooling
        {
            get { return _Wrapped.Pooling; }
            set { _Wrapped.Pooling = value; }
        }

        public bool ReadOnly
        {
            get { return _Wrapped.ReadOnly; }
            set { _Wrapped.ReadOnly = value; }
        }

        public Interface.SQLite.SynchronizationModes SyncMode
        {
            get { return ConvertSQLite.ToSynchronizationModes(_Wrapped.SyncMode); }
            set { _Wrapped.SyncMode = ConvertSQLite.ToSynchronizationModes(value); }
        }

        public string Uri
        {
            get { return _Wrapped.Uri; }
            set { _Wrapped.Uri = value; }
        }

        public bool UseUTF16Encoding
        {
            get { return _Wrapped.UseUTF16Encoding; }
            set { _Wrapped.UseUTF16Encoding = value; }
        }

        public int Version
        {
            get { return _Wrapped.Version; }
            set { _Wrapped.Version = value; }
        }

        public bool TryGetValue(string keyword, out object value)
        {
            return _Wrapped.TryGetValue(keyword, out value);
        }
    }
}
