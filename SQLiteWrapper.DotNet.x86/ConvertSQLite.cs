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
    /// Converts values between the abstracted version and the provider's version of various enums, bitflag enums etc.
    /// </summary>
    static class ConvertSQLite
    {
        #region SQLiteDateFormats
        public static SQLiteDateFormats ToSQLiteDateFormats(VirtualRadar.Interface.SQLite.SQLiteDateFormats value)
        {
            switch(value) {
                case VirtualRadar.Interface.SQLite.SQLiteDateFormats.Ticks:             return SQLiteDateFormats.Ticks;
                case VirtualRadar.Interface.SQLite.SQLiteDateFormats.ISO8601:           return SQLiteDateFormats.ISO8601;
                case VirtualRadar.Interface.SQLite.SQLiteDateFormats.JulianDay:         return SQLiteDateFormats.JulianDay;
                #if DOTNET_BUILD
                case VirtualRadar.Interface.SQLite.SQLiteDateFormats.UnixEpoch:         return SQLiteDateFormats.UnixEpoch;
                case VirtualRadar.Interface.SQLite.SQLiteDateFormats.InvariantCulture:  return SQLiteDateFormats.InvariantCulture;
                case VirtualRadar.Interface.SQLite.SQLiteDateFormats.CurrentCulture:    return SQLiteDateFormats.CurrentCulture;
                #endif
                default:                                                                throw new NotImplementedException();
            }
        }

        public static VirtualRadar.Interface.SQLite.SQLiteDateFormats ToSQLiteDateFormats(SQLiteDateFormats value)
        {
            switch(value) {
                case SQLiteDateFormats.Ticks:               return VirtualRadar.Interface.SQLite.SQLiteDateFormats.Ticks;
                case SQLiteDateFormats.ISO8601:             return VirtualRadar.Interface.SQLite.SQLiteDateFormats.ISO8601;
                case SQLiteDateFormats.JulianDay:           return VirtualRadar.Interface.SQLite.SQLiteDateFormats.JulianDay;
                #if DOTNET_BUILD
                case SQLiteDateFormats.UnixEpoch:           return VirtualRadar.Interface.SQLite.SQLiteDateFormats.UnixEpoch;
                case SQLiteDateFormats.InvariantCulture:    return VirtualRadar.Interface.SQLite.SQLiteDateFormats.InvariantCulture;
                case SQLiteDateFormats.CurrentCulture:      return VirtualRadar.Interface.SQLite.SQLiteDateFormats.CurrentCulture;
                #endif
                default:                                    throw new NotImplementedException();
            }
        }
        #endregion

        #region SQLiteJournalModeEnum
        public static SQLiteJournalModeEnum ToSQLiteJournalModeEnum(VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum value)
        {
            switch(value) {
                case VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Delete:    return SQLiteJournalModeEnum.Delete;
                case VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Persist:   return SQLiteJournalModeEnum.Persist;
                case VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Off:       return SQLiteJournalModeEnum.Off;
                #if DOTNET_BUILD
                case VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Default:   return SQLiteJournalModeEnum.Default;
                case VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Truncate:  return SQLiteJournalModeEnum.Truncate;
                case VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Memory:    return SQLiteJournalModeEnum.Memory;
                case VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Wal:       return SQLiteJournalModeEnum.Wal;
                #else
                case VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Default:   return SQLiteJournalModeEnum.Delete;
                #endif
                default:                                                            throw new NotImplementedException();
            }
        }

        public static VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum ToSQLiteJournalModeEnum(SQLiteJournalModeEnum value)
        {
            switch(value) {
                case SQLiteJournalModeEnum.Delete:      return VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Delete;
                case SQLiteJournalModeEnum.Persist:     return VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Persist;
                case SQLiteJournalModeEnum.Off:         return VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Off;
                #if DOTNET_BUILD
                case SQLiteJournalModeEnum.Default:     return VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Default;
                case SQLiteJournalModeEnum.Truncate:    return VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Truncate;
                case SQLiteJournalModeEnum.Memory:      return VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Memory;
                case SQLiteJournalModeEnum.Wal:         return VirtualRadar.Interface.SQLite.SQLiteJournalModeEnum.Wal;
                #endif
                default:                                throw new NotImplementedException();
            }
        }
        #endregion

        #region SynchronizationModes
        internal static SynchronizationModes ToSynchronizationModes(VirtualRadar.Interface.SQLite.SynchronizationModes value)
        {
            switch(value) {
                case VirtualRadar.Interface.SQLite.SynchronizationModes.Normal: return SynchronizationModes.Normal;
                case VirtualRadar.Interface.SQLite.SynchronizationModes.Full:   return SynchronizationModes.Full;
                case VirtualRadar.Interface.SQLite.SynchronizationModes.Off:    return SynchronizationModes.Off;
                default:                                                        throw new NotImplementedException();
            }
        }

        internal static VirtualRadar.Interface.SQLite.SynchronizationModes ToSynchronizationModes(SynchronizationModes value)
        {
            switch(value) {
                case SynchronizationModes.Normal:   return VirtualRadar.Interface.SQLite.SynchronizationModes.Normal;
                case SynchronizationModes.Full:     return VirtualRadar.Interface.SQLite.SynchronizationModes.Full;
                case SynchronizationModes.Off:      return VirtualRadar.Interface.SQLite.SynchronizationModes.Off;
                default:                            throw new NotImplementedException();
            }
        }
        #endregion
    }
}
