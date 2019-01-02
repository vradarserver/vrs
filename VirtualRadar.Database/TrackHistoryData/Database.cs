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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.SQLite;

namespace VirtualRadar.Database.TrackHistoryData
{
    /// <summary>
    /// Default implementation of <see cref="ITrackHistoryDatabase"/>.
    /// </summary>
    class Database : ITrackHistoryDatabaseSQLite
    {
        #region Fields, Properties and Events
        /// <summary>
        /// SQLite is single threaded and isn't always too happy about multi-threaded calls on the same database.
        /// This locks every operation to enforce single-threaded calls.
        /// </summary>
        private static object _SqlLiteSyncLock = new object();

        /// <summary>
        /// The object that supplies the time.
        /// </summary>
        private IClock _Clock;

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

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsDataSourceReadOnly { get => false; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool FileNameRequired { get => true; }

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
        /// See interface docs. Unused, the connection string is built from <see cref="FileName"/>.
        /// </summary>
        public string ConnectionString { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Database()
        {
            _Clock = Factory.Resolve<IClock>();
        }
        #endregion

        #region CreateOpenConnection, BuildConnectionString
        /// <summary>
        /// Returns an open connection to the database or null if the file does not exist, has not been set etc.
        /// </summary>
        /// <returns></returns>
        private ConnectionWrapper CreateOpenConnection()
        {
            IDbConnection connection = null;
            IDbTransaction transaction = null;

            var fileName = FileName;
            var disposeOfConnection = false;

            if(!String.IsNullOrEmpty(fileName) && File.Exists(fileName)) {
                lock(_SqlLiteSyncLock) {
                    connection = _TransactionConnection;
                    transaction = _Transaction;

                    if(connection == null) {
                        connection = Factory.Resolve<ISQLiteConnectionProvider>().Create(BuildConnectionString(FileName));
                        disposeOfConnection = true;
                        connection.Open();

                        // Foreign key constraints are disabled by default, we want them enabled
                        connection.Execute("PRAGMA foreign_keys = ON;");
                    }
                }
            }

            return new ConnectionWrapper(connection, transaction, disposeOfConnection);
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
        #endregion

        #region PerformInTransaction
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool PerformInTransaction(Func<bool> action)
        {
            var wrapper = CreateOpenConnection();
            try {
                if(wrapper.ConnectionWillBeDisposed) {
                    _TransactionConnection = wrapper.Connection;
                }
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
            } finally {
                if(wrapper.ConnectionWillBeDisposed) {
                    _TransactionConnection = null;
                }
                wrapper.Dispose();
            }
        }
        #endregion

        #region Create, CreateSchema
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="dataSource"></param>
        public void Create(string dataSource)
        {
            if(dataSource == null) {
                throw new ArgumentNullException(nameof(dataSource));
            } else if(dataSource == "") {
                throw new InvalidOperationException("Missing file name");
            } else if(File.Exists(dataSource)) {
                throw new InvalidOperationException($"{dataSource} already exists");
            }

            var folder = Path.GetDirectoryName(dataSource);
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            lock(_SqlLiteSyncLock) {
                using(var result = Factory.Resolve<ISQLiteConnectionProvider>().Create(BuildConnectionString(dataSource))) {
                    result.Open();

                    CreateSchema(result);
                }
            }
        }

        /// <summary>
        /// Creates the schema.
        /// </summary>
        /// <param name="connection"></param>
        private void CreateSchema(IDbConnection connection)
        {
            if(connection != null) {
                connection.Execute(Commands.CreateSchema);
            }
        }
        #endregion

        #region DatabaseVersion
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
                            UPDATE [DatabaseVersion]
                            SET    [Version] = @version
                            WHERE  [Version] <> @version;
                        ", new {
                            version,
                        }, transaction: connection.Transaction);
                    }
                }
            }
        }
        #endregion

        #region Receiver
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryReceiver Receiver_GetByID(long id)
        {
            TrackHistoryReceiver result = null;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        result = connection.Connection.QueryFirstOrDefault<TrackHistoryReceiver>(
                            "SELECT * FROM [Receiver] WHERE [ReceiverID] = @id",
                            new {
                                id,
                            },
                            transaction: connection.Transaction
                        );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryReceiver Receiver_GetByName(string name)
        {
            TrackHistoryReceiver result = null;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        result = Receiver_GetByName(connection, name);
                    }
                }
            }

            return result;
        }

        private TrackHistoryReceiver Receiver_GetByName(ConnectionWrapper connection, string name)
        {
            return connection.Connection.QueryFirstOrDefault<TrackHistoryReceiver>(
                "SELECT * FROM [Receiver] WHERE [Name] = @name",
                new {
                    name,
                },
                transaction: connection.Transaction
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryReceiver Receiver_GetOrCreateByName(string name)
        {
            TrackHistoryReceiver result = null;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        result = Receiver_GetByName(connection, name);

                        if(result == null) {
                            var now = _Clock.UtcNow;
                            result = new TrackHistoryReceiver() {
                                Name =       name,
                                CreatedUtc = now,
                                UpdatedUtc = now,
                            };
                            Receiver_Insert(connection, result);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receiver"></param>
        public void Receiver_Save(TrackHistoryReceiver receiver)
        {
            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        if(receiver.ReceiverID == 0) {
                            Receiver_Insert(connection, receiver);
                        } else {
                            receiver.CreatedUtc = connection.Connection.Query<DateTime>(
                                Commands.Receiver_Update,
                                receiver,
                                transaction: connection.Transaction
                            ).First();
                        }
                    }
                }
            }
        }

        private void Receiver_Insert(ConnectionWrapper connection, TrackHistoryReceiver receiver)
        {
            receiver.ReceiverID = connection.Connection.Query<int>(
                Commands.Receiver_Insert,
                receiver,
                transaction: connection.Transaction
            ).First();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receiver"></param>
        public void Receiver_Delete(TrackHistoryReceiver receiver)
        {
            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    connection.Connection.Execute(
                        "DELETE FROM [Receiver] WHERE [ReceiverID] = @ReceiverID",
                        new { receiver.ReceiverID },
                        transaction: connection.Transaction
                    );
                }
            }
        }
        #endregion

        #region TrackHistory
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="trackHistory"></param>
        /// <returns></returns>
        public TrackHistoryTruncateResult TrackHistory_Delete(TrackHistory trackHistory)
        {
            TrackHistoryTruncateResult result = null;

            // The TrackHistory_Delete script performs multiple delete operations, they need to
            // always be performed within a transaction.
            PerformInTransaction(() => {
                using(var connection = CreateOpenConnection()) {
                    if(connection.Connection != null) {
                        lock(_SqlLiteSyncLock) {
                            result = connection.Connection.QueryFirst<TrackHistoryTruncateResult>(
                                Commands.TrackHistory_Delete,
                                new {
                                    trackHistory.TrackHistoryID,
                                },
                                transaction: connection.Transaction
                            );
                        }
                    }
                }
                return true;
            });

            return result ?? new TrackHistoryTruncateResult();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="deleteUpToUtc"></param>
        /// <returns></returns>
        public TrackHistoryTruncateResult TrackHistory_DeleteExpired(DateTime deleteUpToUtc)
        {
            TrackHistoryTruncateResult result = null;

            // The TrackHistory_DeleteExpired script performs multiple delete operations, they need to
            // always be performed within a transaction.
            PerformInTransaction(() => {
                using(var connection = CreateOpenConnection()) {
                    if(connection.Connection != null) {
                        lock(_SqlLiteSyncLock) {
                            result = connection.Connection.QueryFirst<TrackHistoryTruncateResult>(
                                Commands.TrackHistory_DeleteExpired,
                                new {
                                    threshold = deleteUpToUtc,
                                },
                                transaction: connection.Transaction
                            );
                        }
                    }
                }
                return true;
            });

            return result ?? new TrackHistoryTruncateResult();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="startTimeInclusive"></param>
        /// <param name="endTimeInclusive"></param>
        /// <returns></returns>
        public IEnumerable<TrackHistory> TrackHistory_GetByDateRange(DateTime? startTimeInclusive, DateTime? endTimeInclusive)
        {
            TrackHistory[] result = null;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        result = connection.Connection.Query<TrackHistory>(
                            Commands.TrackHistory_GetByDateRange,
                            new {
                                startTimeInclusive,
                                endTimeInclusive,
                            },
                            transaction: connection.Transaction
                        ).ToArray();
                    }
                }
            }

            return result ?? new TrackHistory[0];
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="startTimeInclusive"></param>
        /// <param name="endTimeInclusive"></param>
        /// <returns></returns>
        public IEnumerable<TrackHistory> TrackHistory_GetByIcao(string icao, DateTime? startTimeInclusive, DateTime? endTimeInclusive)
        {
            TrackHistory[] result = null;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        result = connection.Connection.Query<TrackHistory>(
                            Commands.TrackHistory_GetByIcao,
                            new {
                                icao,
                                startTimeInclusive,
                                endTimeInclusive,
                            },
                            transaction: connection.Transaction
                        ).ToArray();
                    }
                }
            }

            return result ?? new TrackHistory[0];
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistory TrackHistory_GetByID(long id)
        {
            TrackHistory result = null;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        result = connection.Connection.QueryFirstOrDefault<TrackHistory>(
                            "SELECT * FROM [TrackHistory] WHERE [TrackHistoryID] = @trackHistoryID",
                            new {
                                trackHistoryID = id,
                            },
                            transaction: connection.Transaction
                        );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns all track histories meeting various criteria. For internal use.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="upToCreatedUtc"></param>
        /// <param name="includePreserved"></param>
        /// <param name="minimumHistoryStates"></param>
        /// <returns></returns>
        private IEnumerable<TrackHistory> TrackHistory_GetUpToCreatedUtc(ConnectionWrapper connection, DateTime upToCreatedUtc, bool includePreserved, int minimumHistoryStates)
        {
            return connection.Connection.Query<TrackHistory>(@"
                SELECT *
                FROM   [TrackHistory] AS [parent]
                WHERE  [CreatedUtc] <= @upToCreatedUtc
                AND    (@includePreserved = 1 OR [IsPreserved] = 0)
                AND    (SELECT COUNT(*) FROM [TrackHistoryState] WHERE [parent].[TrackHistoryID] = [TrackHistoryID] LIMIT (@minimumHistoryStates + 1)) > @minimumHistoryStates",
                new {
                    upToCreatedUtc,
                    includePreserved,
                    minimumHistoryStates
                },
                transaction: connection.Transaction
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="trackHistory"></param>
        public void TrackHistory_Save(TrackHistory trackHistory)
        {
            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        if(trackHistory.TrackHistoryID != 0) {
                            trackHistory.CreatedUtc = connection.Connection.Query<DateTime>(
                                Commands.TrackHistory_Update,
                                trackHistory,
                                transaction: connection.Transaction
                            ).First();
                        } else {
                            trackHistory.TrackHistoryID = connection.Connection.Query<long>(
                                Commands.TrackHistory_Insert,
                                trackHistory,
                                transaction: connection.Transaction
                            ).First();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="trackHistory"></param>
        /// <param name="newUpdatedUtc"></param>
        /// <returns></returns>
        public TrackHistoryTruncateResult TrackHistory_Truncate(TrackHistory trackHistory, DateTime newUpdatedUtc)
        {
            var result = new TrackHistoryTruncateResult();

            if(!(trackHistory?.IsPreserved ?? true)) {
                PerformInTransaction(() => {
                    using(var connection = CreateOpenConnection()) {
                        if(connection != null) {
                            lock(_SqlLiteSyncLock) {
                                TrackHistory_Truncate(connection, trackHistory, newUpdatedUtc, result);
                            }
                        }
                    }

                    return true;
                });
            }

            return result;
        }

        private void TrackHistory_Truncate(ConnectionWrapper connection, TrackHistory trackHistory, DateTime newUpdatedUtc, TrackHistoryTruncateResult truncateResult)
        {
            if(!(trackHistory?.IsPreserved ?? true)) {
                if(truncateResult.EarliestHistoryUtc == default(DateTime) || truncateResult.EarliestHistoryUtc > trackHistory.CreatedUtc) {
                    truncateResult.EarliestHistoryUtc = trackHistory.CreatedUtc;
                }
                if(truncateResult.LatestHistoryUtc < trackHistory.CreatedUtc) {
                    truncateResult.LatestHistoryUtc = trackHistory.CreatedUtc;
                }
                ++truncateResult.CountTrackHistories;

                var states = TrackHistoryState_GetByTrackHistory(connection, trackHistory);
                if(states.Length > 2) {
                    var originalCountStates = states.Length;
                    var mergedState = TrackHistoryState.MergeStates(states);
                    TrackHistoryState_DeleteMany(connection, states.Skip(1));
                    TrackHistoryState_Save(connection, mergedState);

                    truncateResult.CountTrackHistoryStates += originalCountStates - 2;
                }
            }
        }


        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="truncateUpToUtc"></param>
        /// <param name="newUpdatedUtc"></param>
        /// <returns></returns>
        public TrackHistoryTruncateResult TrackHistory_TruncateExpired(DateTime truncateUpToUtc, DateTime newUpdatedUtc)
        {
            var result = new TrackHistoryTruncateResult();

            PerformInTransaction(() => {
                using(var connection = CreateOpenConnection()) {
                    if(connection != null) {
                        lock(_SqlLiteSyncLock) {
                            foreach(var trackHistory in TrackHistory_GetUpToCreatedUtc(connection, truncateUpToUtc, includePreserved: false, minimumHistoryStates: 3)) {
                                TrackHistory_Truncate(connection, trackHistory, newUpdatedUtc, result);
                            }
                        }
                    }
                }

                return true;
            });

            return result;
        }
        #endregion

        #region TrackHistoryState
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
                        result = TrackHistoryState_GetByTrackHistory(connection, trackHistory);
                    }
                }
            }

            return result ?? new TrackHistoryState[0];
        }

        private TrackHistoryState[] TrackHistoryState_GetByTrackHistory(ConnectionWrapper connection, TrackHistory trackHistory)
        {
            return connection.Connection.Query<TrackHistoryState>(@"
                SELECT   *
                FROM     [TrackHistoryState]
                WHERE    [TrackHistoryID] = @trackHistoryID
                ORDER BY [SequenceNumber]
            ", new {
                trackHistoryID = trackHistory.TrackHistoryID,
            }, transaction: connection.Transaction)
            .ToArray();
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
                        TrackHistoryState_Save(connection, trackHistoryState);
                    }
                }
            }
        }

        private void TrackHistoryState_Save(ConnectionWrapper connection, TrackHistoryState trackHistoryState)
        {
            if(trackHistoryState.TrackHistoryID == 0) {
                throw new ArgumentOutOfRangeException($"{nameof(trackHistoryState.TrackHistoryID)} must be filled in before saving state");
            }
            if(trackHistoryState.SequenceNumber == 0) {
                throw new ArgumentOutOfRangeException($"{nameof(trackHistoryState.SequenceNumber)} must be filled in before saving state");
            }
            if(trackHistoryState.TimestampUtc == default(DateTime)) {
                throw new ArgumentOutOfRangeException($"{nameof(trackHistoryState.TimestampUtc)} must be filled in before saving state");
            }

            if(trackHistoryState.TrackHistoryStateID != 0) {
                connection.Connection.Execute(
                    Commands.TrackHistoryState_Update,
                    trackHistoryState,
                    connection.Transaction
                );
            } else {
                trackHistoryState.TrackHistoryStateID = connection.Connection.ExecuteScalar<long>(
                    Commands.TrackHistoryState_Insert,
                    trackHistoryState,
                    connection.Transaction
                );
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
        /// Deletes many state records. Assumes that the connection has been tested to be open and that a lock
        /// has been acquired. Runs faster if you also have a transaction open.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="trackHistoryStates"></param>
        private void TrackHistoryState_DeleteMany(ConnectionWrapper connection, IEnumerable<TrackHistoryState> trackHistoryStates)
        {
            foreach(var trackHistoryState in trackHistoryStates) {
                connection.Connection.Execute(
                    Commands.TrackHistoryState_Delete,
                    new {
                        trackHistoryState.TrackHistoryStateID,
                    },
                    transaction: connection.Transaction
                );
            }
        }
        #endregion
    }
}
