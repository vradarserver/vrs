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

namespace VirtualRadar.Interface.SQLite
{
    /// <summary>
    /// The interface that wrappers around a provider's SQLiteConnectionStringBuilder must implement.
    /// </summary>
    public interface ISQLiteConnectionStringBuilder
    {
        // The wrapped class uses constructors but we can't declare those in an interface. Instead
        // the caller has to make a call on Initialise before using the object.

        /// <summary>
        /// Constructs the object.
        /// </summary>
        /// <returns></returns>
        ISQLiteConnectionStringBuilder Initialise();

        /// <summary>
        /// Constructs the object.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        ISQLiteConnectionStringBuilder Initialise(string connectionString);

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        bool BinaryGUID { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        int CacheSize { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        string ConnectionString { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        string DataSource { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        SQLiteDateFormats DateTimeFormat { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        IsolationLevel DefaultIsolationLevel { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        int DefaultTimeout { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        bool Enlist { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        bool FailIfMissing { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        bool ForeignKeys { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        SQLiteJournalModeEnum JournalMode { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        bool LegacyFormat { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        int MaxPageCount { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        bool Pooling { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        bool ReadOnly { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        SynchronizationModes SyncMode { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        string Uri { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        bool UseUTF16Encoding { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        int Version { get; set; }

        /// <summary>
        /// See SQLite docs.
        /// </summary>
        bool TryGetValue(string keyword, out object value);
    }
}
