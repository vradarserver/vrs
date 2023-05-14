// Copyright © 2023 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Database.SQLite
{
    public enum SqliteErrorCode
    {
        SQLITE_OK           = 0,    /* Successful result */
        SQLITE_ERROR        = 1,    /* Generic error */
        SQLITE_INTERNAL     = 2,    /* Internal logic error in SQLite */
        SQLITE_PERM         = 3,    /* Access permission denied */
        SQLITE_ABORT        = 4,    /* Callback routine requested an abort */
        SQLITE_BUSY         = 5,    /* The database file is locked */
        SQLITE_LOCKED       = 6,    /* A table in the database is locked */
        SQLITE_NOMEM        = 7,    /* A malloc() failed */
        SQLITE_READONLY     = 8,    /* Attempt to write a readonly database */
        SQLITE_INTERRUPT    = 9,    /* Operation terminated by sqlite3_interrupt()*/
        SQLITE_IOERR        = 10,   /* Some kind of disk I/O error occurred */
        SQLITE_CORRUPT      = 11,   /* The database disk image is malformed */
        SQLITE_NOTFOUND     = 12,   /* Unknown opcode in sqlite3_file_control() */
        SQLITE_FULL         = 13,   /* Insertion failed because database is full */
        SQLITE_CANTOPEN     = 14,   /* Unable to open the database file */
        SQLITE_PROTOCOL     = 15,   /* Database lock protocol error */
        SQLITE_EMPTY        = 16,   /* Internal use only */
        SQLITE_SCHEMA       = 17,   /* The database schema changed */
        SQLITE_TOOBIG       = 18,   /* String or BLOB exceeds size limit */
        SQLITE_CONSTRAINT   = 19,   /* Abort due to constraint violation */
        SQLITE_MISMATCH     = 20,   /* Data type mismatch */
        SQLITE_MISUSE       = 21,   /* Library used incorrectly */
        SQLITE_NOLFS        = 22,   /* Uses OS features not supported on host */
        SQLITE_AUTH         = 23,   /* Authorization denied */
        SQLITE_FORMAT       = 24,   /* Not used */
        SQLITE_RANGE        = 25,   /* 2nd parameter to sqlite3_bind out of range */
        SQLITE_NOTADB       = 26,   /* File opened that is not a database file */
        SQLITE_NOTICE       = 27,   /* Notifications from sqlite3_log() */
        SQLITE_WARNING      = 28,   /* Warnings from sqlite3_log() */
        SQLITE_ROW          = 100,  /* sqlite3_step() has another row ready */
        SQLITE_DONE         = 101,  /* sqlite3_step() has finished executing */
    }
}
