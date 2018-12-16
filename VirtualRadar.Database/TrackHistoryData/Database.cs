// Copyright © 2018 onwards, Andrew Whewell
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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using InterfaceFactory;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.SQLite;

namespace VirtualRadar.Database.TrackHistoryData
{
    /// <summary>
    /// Default implementation of <see cref="ITrackHistoryDatabase"/>.
    /// </summary>
    class Database : ITrackHistoryDatabaseSQLite
    {
        /// <summary>
        /// SQLite is single threaded and isn't always too happy about multi-threaded calls on the same database.
        /// This locks every operation to enforce single-threaded calls.
        /// </summary>
        private static object _SqlLiteSyncLock = new object();

        /// <summary>
        /// True if the schema is known to be up-to-date.
        /// </summary>
        private bool _SchemaUpdated;

        /// <summary>
        /// The object that helps when fulfilling the <see cref="ITransactionable"/> interface.
        /// </summary>
        private TransactionHelper _TransactionHelper = new TransactionHelper();

        /// <summary>
        /// The connection that's active when performing operations within a transaction. Null if no transaction is open.
        /// </summary>
        private IDbConnection _TransactionConnection;

        /// <summary>
        /// The transaction that's currently running, null if no transaction is in force.
        /// </summary>
        private IDbTransaction _Transaction;

        private string _FileName;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FileName
        {
            get { return _FileName; }
            set { _FileName = value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        public void Create(string fileName)
        {
            if(!String.IsNullOrEmpty(fileName)) {
                var folder = Path.GetDirectoryName(fileName);
                if(!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }

                lock(_SqlLiteSyncLock) {
                    using(var result = Factory.Resolve<ISQLiteConnectionProvider>().Create(BuildConnectionString(fileName))) {
                        result.Open();

                        UpdateSchema(result);
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool PerformInTransaction(Func<bool> action)
        {
            using(var wrapper = CreateOpenConnection()) {
                return _TransactionHelper.PerformInTransaction(
                    wrapper.Connection,
                    wrapper.Transaction != null,
                    allowNestedTransaction: true,
                    recordTransaction: r => {
                        wrapper.Transaction = r;
                        _Transaction = r;
                    },
                    action: action
                );
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public int DatabaseVersion_Get()
        {
            using(var connection = CreateOpenConnection()) {
                lock(_SqlLiteSyncLock) {
                    return connection.Connection?.Query<int>(@"
                        SELECT [Version] FROM [DatabaseVersion]
                    ", transaction: connection.Transaction).FirstOrDefault() ?? 0;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="version"></param>
        public void DatabaseVersion_Set(int version)
        {
            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        connection.Connection.Execute(@"
                            REPLACE INTO [DatabaseVersion] ([Version]) VALUES (@version)
                        ", new {
                            version,
                        }, transaction: connection.Transaction);
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryState TrackHistoryState_GetByID(long id)
        {
            TrackHistoryState result = null;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        result = connection.Connection.QueryFirstOrDefault<TrackHistoryState>(@"
                            SELECT *
                            FROM   [TrackHistoryState]
                            WHERE  [TrackHistoryStateID] = @id
                        ", new {
                            id
                        }, transaction: connection.Transaction);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="trackHistory"></param>
        /// <returns></returns>
        public IEnumerable<TrackHistoryState> TrackHistoryState_GetByTrackHistory(TrackHistory trackHistory)
        {
            TrackHistoryState[] result = null;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        result = connection.Connection.Query<TrackHistoryState>(@"
                            SELECT *
                            FROM   [TrackHistoryState]
                            WHERE  [TrackHistoryID] = @trackHistoryID
                        ", new {
                            trackHistoryID = trackHistory.TrackHistoryID
                        }, transaction: connection.Transaction)
                        .ToArray();
                    }
                }
            }

            return result ?? new TrackHistoryState[0];
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="trackHistoryState"></param>
        public void TrackHistoryState_Save(TrackHistoryState trackHistoryState)
        {
            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        if(trackHistoryState.TrackHistoryStateID == 0) {
                            trackHistoryState.TrackHistoryStateID = connection.Connection.ExecuteScalar<long>(
                                Commands.TrackHistoryState_Insert,
                                trackHistoryState,
                                connection.Transaction
                            );
                        } else {
                            connection.Connection.Execute(
                                Commands.TrackHistoryState_Update,
                                trackHistoryState,
                                connection.Transaction
                            );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="trackHistoryStates"></param>
        public void TrackHistoryState_SaveMany(IEnumerable<TrackHistoryState> trackHistoryStates)
        {
            foreach(var trackHistoryState in trackHistoryStates) {
                TrackHistoryState_Save(trackHistoryState);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryTruncateResult TrackHistory_Delete(long id)
        {
            TrackHistoryTruncateResult result = null;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        result = connection.Connection.QueryFirst<TrackHistoryTruncateResult>(
                            Commands.TrackHistory_Delete,
                            new {
                                trackHistoryID = id,
                            },
                            transaction: connection.Transaction
                        );
                    }
                }
            }

            return result ?? new TrackHistoryTruncateResult();
        }

        public TrackHistoryTruncateResult TrackHistory_DeleteMany(int daysBack)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TrackHistory> TrackHistory_GetByDateRange(DateTime? startTimeInclusive, DateTime? endTimeInclusive)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TrackHistory> TrackHistory_GetByIcao(string icao, DateTime? startTimeInclusive, DateTime? endTimeInclusive)
        {
            throw new NotImplementedException();
        }

        public TrackHistory TrackHistory_GetByID(long id)
        {
            throw new NotImplementedException();
        }

        public void TrackHistory_Save(TrackHistory trackHistory)
        {
            throw new NotImplementedException();
        }

        public TrackHistoryTruncateResult TrackHistory_Truncate(long id)
        {
            throw new NotImplementedException();
        }

        public TrackHistoryTruncateResult TrackHistory_TruncateMany(int daysBack)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an open connection to the database or null if the file does not exist, has not been set etc.
        /// </summary>
        /// <returns></returns>
        private ConnectionWrapper CreateOpenConnection()
        {
            IDbConnection connection = null;
            IDbTransaction transaction = null;

            var fileName = FileName;
            if(!String.IsNullOrEmpty(fileName) && File.Exists(fileName)) {
                lock(_SqlLiteSyncLock) {
                    connection = _TransactionConnection;
                    transaction = _Transaction;

                    if(connection == null) {
                        connection = Factory.Resolve<ISQLiteConnectionProvider>().Create(BuildConnectionString(FileName));
                        connection.Open();
                        UpdateSchema(connection);
                    }
                }
            }

            return new ConnectionWrapper(connection, transaction, disposeOfConnection: true);
        }

        /// <summary>
        /// Returns the connection string for the filename passed across.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string BuildConnectionString(string fileName)
        {
            var builder = Factory.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
            builder.DataSource = fileName;
            builder.DateTimeFormat = SQLiteDateFormats.ISO8601;
            builder.FailIfMissing = false;
            builder.ReadOnly = false;
            builder.JournalMode = SQLiteJournalModeEnum.Persist;

            return builder.ConnectionString;
        }

        /// <summary>
        /// Updates the schema.
        /// </summary>
        /// <param name="connection"></param>
        private void UpdateSchema(IDbConnection connection)
        {
            if(!_SchemaUpdated) {
                lock(_SqlLiteSyncLock) {
                    if(!_SchemaUpdated) {
                        _SchemaUpdated = true;
                        connection.Execute(Commands.UpdateSchema);

                        var currentVersion = 1;
                        var databaseVersion = connection.ExecuteScalar<long?>("SELECT [Version] FROM [DatabaseVersion]");
                        if(databaseVersion != currentVersion) {
                            connection.Execute("REPLACE INTO [DatabaseVersion] ([Version]) VALUES (@currentVersion)", new {
                                currentVersion,
                            });
                        }
                    }
                }
            }
        }
    }
}
