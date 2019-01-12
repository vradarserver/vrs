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
using VirtualRadar.Interface.StandingData;

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

        #region Helper Methods
        /// <summary>
        /// Acquires a connection (either a new one or the current transaction's connection) and acquires the
        /// lock before calling the action. If the database has not been configured then the action is not called.
        /// </summary>
        /// <param name="action"></param>
        /// <returns>True if the connection was acquired and the action called, false if no connection could be made.</returns>
        private bool RunWithinConnection(Action<ConnectionWrapper> action)
        {
            var actionPerformed = false;

            using(var connection = CreateOpenConnection()) {
                if(connection.Connection != null) {
                    lock(_SqlLiteSyncLock) {
                        action(connection);
                        actionPerformed = true;
                    }
                }
            }

            return actionPerformed;
        }

        /// <summary>
        /// Opens a connection or co-opts the transaction's connection and runs the SQL. Returns all rows.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="useConnection"></param>
        /// <returns></returns>
        private IEnumerable<T> Query<T>(string sql, object parameters, ConnectionWrapper useConnection = null)
        {
            var result = default(IEnumerable<T>);

            if(useConnection != null) {
                result = useConnection.Connection.Query<T>(
                    sql,
                    parameters,
                    transaction: useConnection.Transaction
                );
            } else {
                RunWithinConnection((connection) => {
                    result = connection.Connection.Query<T>(
                        sql,
                        parameters,
                        transaction: connection.Transaction
                    );
                });
            }

            return result ?? new T[0];
        }

        private T QueryFirstOrDefault<T>(string sql, object parameters, ConnectionWrapper useConnection = null)
        {
            return Query<T>(sql, parameters, useConnection).FirstOrDefault();
        }

        private T QueryFirst<T>(string sql, object parameters, ConnectionWrapper useConnection = null)
        {
            return Query<T>(sql, parameters, useConnection).First();
        }

        /// <summary>
        /// Opens a connection or co-opts the transaction's connection and executes the SQL on it.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="useConnection"></param>
        private void Execute(string sql, object parameters, ConnectionWrapper useConnection = null)
        {
            if(useConnection != null) {
                useConnection.Connection.Execute(
                    sql,
                    parameters,
                    transaction: useConnection.Transaction
                );
            } else {
                RunWithinConnection((connection) => {
                    connection.Connection.Execute(
                        sql,
                        parameters,
                        transaction: connection.Transaction
                    );
                });
            }
        }

        /// <summary>
        /// Opens a connection or co-opts the transaction's connection, executes the SQL on it and returns the
        /// single scalar result.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <param name="useConnection"></param>
        private T ExecuteScalar<T>(string sql, object parameters, ConnectionWrapper useConnection = null)
        {
            var result = default(T);

            if(useConnection != null) {
                result = useConnection.Connection.ExecuteScalar<T>(
                    sql,
                    parameters,
                    transaction: useConnection.Transaction
                );
            } else {
                RunWithinConnection((connection) => {
                    result = connection.Connection.ExecuteScalar<T>(
                        sql,
                        parameters,
                        transaction: connection.Transaction
                    );
                });
            }

            return result;
        }

        /// <summary>
        /// Performs a standard save operation - if the ID is unassigned then calls the <paramref name="insertAction"/>, otherwise calls the <paramref name="updateAction"/>.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="record"></param>
        /// <param name="hasID"></param>
        /// <param name="insertAction"></param>
        /// <param name="updateAction"></param>
        /// <param name="useConnection"></param>
        private void Save<TRecord>(TRecord record, Func<bool> hasID, Action<ConnectionWrapper, TRecord> insertAction, Action<ConnectionWrapper, TRecord> updateAction, ConnectionWrapper useConnection = null)
        {
            if(useConnection != null) {
                if(hasID()) {
                    updateAction(useConnection, record);
                } else {
                    insertAction(useConnection, record);
                }
            } else {
                RunWithinConnection((connection) => {
                    if(hasID()) {
                        updateAction(connection, record);
                    } else {
                        insertAction(connection, record);
                    }
                });
            }
        }

        /// <summary>
        /// Performs a standard insert operation. The SQL must return the ID as a single column in a single row, this is assigned to the object via the
        /// <paramref name="assignID"/> callback.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="connection"></param>
        /// <param name="record"></param>
        /// <param name="sql"></param>
        /// <param name="assignID"></param>
        /// <param name="parameters"></param>
        private void Insert<TRecord, TKey>(ConnectionWrapper connection, TRecord record, string sql, Action<TKey> assignID, object parameters = null)
        {
            var id = connection.Connection.Query<TKey>(
                sql,
                parameters ?? record,
                transaction: connection.Transaction
            ).First();

            assignID(id);
        }

        /// <summary>
        /// Performs a standard update operation for a timestamped record. The SQL must not change the Created timestamp, instead it must return it
        /// and it must be forced onto the record that was updated via <paramref name="assignCreatedUtc"/>.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="connection"></param>
        /// <param name="record"></param>
        /// <param name="sql"></param>
        /// <param name="assignCreatedUtc"></param>
        /// <param name="parameters"></param>
        private void Update<TRecord>(ConnectionWrapper connection, TRecord record, string sql, Action<DateTime> assignCreatedUtc, object parameters = null)
        {
            var unchangedCreatedUtc = connection.Connection.Query<DateTime>(
                sql,
                parameters ?? record,
                transaction: connection.Transaction
            ).First();

            assignCreatedUtc(unchangedCreatedUtc);
        }

        /// <summary>
        /// Wraps the standard procedure for fetching a record by a unique key field and creating it if it does not already exist.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="key"></param>
        /// <param name="getByKey"></param>
        /// <param name="insertAction"></param>
        /// <param name="buildNewRecord"></param>
        /// <param name="useConnection"></param>
        /// <returns></returns>
        private TResult GetOrCreateByKey<TResult, TKey>(TKey key, Func<ConnectionWrapper, TKey, TResult> getByKey, Action<ConnectionWrapper, TResult> insertAction, Func<DateTime, TResult> buildNewRecord, ConnectionWrapper useConnection = null)
            where TResult : class
        {
            return GetOrCreateByCompositeKey<TResult>(
                connection => getByKey(connection, key),
                insertAction,
                buildNewRecord,
                useConnection
            );
        }

        /// <summary>
        /// Wraps the standard procedure for fetching a record by a unique set of composite key fields and creating it if it does not already exist.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="getByKey"></param>
        /// <param name="insertAction"></param>
        /// <param name="buildNewRecord"></param>
        /// <param name="useConnection"></param>
        /// <returns></returns>
        private TResult GetOrCreateByCompositeKey<TResult>(Func<ConnectionWrapper, TResult> getByKey, Action<ConnectionWrapper, TResult> insertAction, Func<DateTime, TResult> buildNewRecord, ConnectionWrapper useConnection = null)
            where TResult : class
        {
            TResult result = null;

            if(useConnection != null) {
                result = getByKey(useConnection);

                if(result == null) {
                    result = buildNewRecord(_Clock.UtcNow);
                    insertAction(useConnection, result);
                }
            } else {
                RunWithinConnection((connection) => {
                    result = getByKey(connection);

                    if(result == null) {
                        result = buildNewRecord(_Clock.UtcNow);
                        insertAction(connection, result);
                    }
                });
            }

            return result;
        }
        #endregion

        #region DatabaseVersion
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public int DatabaseVersion_Get()
        {
            return QueryFirst<int>(
                "SELECT [Version] FROM [DatabaseVersion]",
                null
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="version"></param>
        public void DatabaseVersion_Set(int version)
        {
            Execute(@"
                UPDATE [DatabaseVersion]
                SET    [Version] = @version
                WHERE  [Version] <> @version;
            ", new {
                version,
            });
        }
        #endregion

        #region Aircraft
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryAircraft Aircraft_GetByID(long id)
        {
            return QueryFirstOrDefault<TrackHistoryAircraft>(
                "SELECT * FROM [Aircraft] WHERE [AircraftID] = @id",
                new { id }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        public TrackHistoryAircraft Aircraft_GetByIcao(string icao)
        {
            return QueryFirstOrDefault<TrackHistoryAircraft>(
                "SELECT * FROM [Aircraft] WHERE [Icao] = @icao",
                new { icao }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void Aircraft_Save(TrackHistoryAircraft aircraft)
        {
            Save(aircraft, () => aircraft.AircraftID != 0, Aircraft_Insert, Aircraft_Update);
        }

        private void Aircraft_Insert(ConnectionWrapper connection, TrackHistoryAircraft aircraft)
        {
            Insert<TrackHistoryAircraft, long>(connection, aircraft, Commands.Aircraft_Insert, id => aircraft.AircraftID = id);
        }

        private void Aircraft_Update(ConnectionWrapper connection, TrackHistoryAircraft aircraft)
        {
            Update<TrackHistoryAircraft>(connection, aircraft, Commands.Aircraft_Update, createdUtc => aircraft.CreatedUtc = createdUtc);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        public void Aircraft_Delete(TrackHistoryAircraft aircraft)
        {
            Execute(
                "DELETE FROM [Aircraft] WHERE [AircraftID] = @AircraftID",
                new { aircraft.AircraftID }
            );
        }
        #endregion

        #region AircraftType
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryAircraftType AircraftType_GetByID(int id)
        {
            return QueryFirstOrDefault<TrackHistoryAircraftType>(
                "SELECT * FROM [AircraftType] WHERE [AircraftTypeID] = @id",
                new { id }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftType"></param>
        public void AircraftType_Save(TrackHistoryAircraftType aircraftType)
        {
            Save(aircraftType, () => aircraftType.AircraftTypeID != 0, AircraftType_Insert, AircraftType_Update);
        }

        private void AircraftType_Insert(ConnectionWrapper connection, TrackHistoryAircraftType aircraftType)
        {
            Insert<TrackHistoryAircraftType, int>(connection, aircraftType, Commands.AircraftType_Insert, id => aircraftType.AircraftTypeID = id);
        }

        private void AircraftType_Update(ConnectionWrapper connection, TrackHistoryAircraftType aircraftType)
        {
            Update<TrackHistoryAircraftType>(connection, aircraftType, Commands.AircraftType_Update, created => aircraftType.CreatedUtc = created);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftType"></param>
        public void AircraftType_Delete(TrackHistoryAircraftType aircraftType)
        {
            Execute(
                "DELETE FROM [AircraftType] WHERE [AircraftTypeID] = @AircraftTypeID",
                new { aircraftType.AircraftTypeID }
            );
        }
        #endregion

        #region Airport
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryAirport Airport_GetByID(int id)
        {
            return QueryFirstOrDefault<TrackHistoryAirport>(
                "SELECT * FROM [Airport] WHERE [AirportID] = @id",
                new { id }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="airport"></param>
        public void Airport_Save(TrackHistoryAirport airport)
        {
            Save<TrackHistoryAirport>(airport, () => airport.AirportID != 0, Airport_Insert, Airport_Update);
        }

        private void Airport_Insert(ConnectionWrapper connection, TrackHistoryAirport airport)
        {
            Insert<TrackHistoryAirport, int>(connection, airport, Commands.Airport_Insert, id => airport.AirportID = id);
        }

        private void Airport_Update(ConnectionWrapper connection, TrackHistoryAirport airport)
        {
            Update<TrackHistoryAirport>(connection, airport, Commands.Airport_Update, created => airport.CreatedUtc = created);
        }
        #endregion

        #region Country
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryCountry Country_GetByID(int id)
        {
            return QueryFirstOrDefault<TrackHistoryCountry>(
                "SELECT * FROM [Country] WHERE [CountryID] = @id",
                new { id, }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryCountry Country_GetByName(string name)
        {
            return Country_GetByName(null, name);
        }

        private TrackHistoryCountry Country_GetByName(ConnectionWrapper connection, string name)
        {
            return QueryFirstOrDefault<TrackHistoryCountry>(
                "SELECT * FROM [Country] WHERE [Name] = @name",
                new { name, },
                connection
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="country"></param>
        public void Country_Save(TrackHistoryCountry country)
        {
            Save(country, () => country.CountryID != 0, Country_Insert, Country_Update);
        }

        private void Country_Insert(ConnectionWrapper connection, TrackHistoryCountry country)
        {
            Insert<TrackHistoryCountry, int>(connection, country, Commands.Country_Insert, id => country.CountryID = id);
        }

        private void Country_Update(ConnectionWrapper connection, TrackHistoryCountry country)
        {
            Update<TrackHistoryCountry>(connection, country, Commands.Country_Update, created => country.CreatedUtc = created);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryCountry Country_GetOrCreateByName(string name)
        {
            return GetOrCreateByKey<TrackHistoryCountry, string>(
                name,
                Country_GetByName,
                Country_Insert,
                now => new TrackHistoryCountry() {
                    Name = name,
                    CreatedUtc = now,
                    UpdatedUtc = now,
                }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="country"></param>
        public void Country_Delete(TrackHistoryCountry country)
        {
            Execute(
                "DELETE FROM [Country] WHERE [CountryID] = @CountryID",
                new { country.CountryID }
            );
        }
        #endregion

        #region EnginePlacement
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="enginePlacement"></param>
        /// <returns></returns>
        public TrackHistoryEnginePlacement EnginePlacement_GetByID(EnginePlacement enginePlacement)
        {
            return QueryFirstOrDefault<TrackHistoryEnginePlacement>(
                "SELECT * FROM [EnginePlacement] WHERE [EnginePlacementID] = @EnginePlacementID",
                new { EnginePlacementID = enginePlacement }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TrackHistoryEnginePlacement> EnginePlacement_GetAll()
        {
            return Query<TrackHistoryEnginePlacement>(
                "SELECT * FROM [EnginePlacement]",
                null
            ).ToArray();
        }
        #endregion

        #region EngineType
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="engineType"></param>
        /// <returns></returns>
        public TrackHistoryEngineType EngineType_GetByID(EngineType engineType)
        {
            return QueryFirstOrDefault<TrackHistoryEngineType>(
                "SELECT * FROM [EngineType] WHERE [EngineTypeID] = @EngineTypeID",
                new { EngineTypeID = engineType }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TrackHistoryEngineType> EngineType_GetAll()
        {
            return Query<TrackHistoryEngineType>(
                "SELECT * FROM [EngineType]",
                null
            ).ToArray();
        }
        #endregion

        #region Manufacturer
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryManufacturer Manufacturer_GetByID(int id)
        {
            return QueryFirstOrDefault<TrackHistoryManufacturer>(
                "SELECT * FROM [Manufacturer] WHERE [ManufacturerID] = @id",
                new { id, }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryManufacturer Manufacturer_GetByName(string name)
        {
            return Manufacturer_GetByName(null, name);
        }

        private TrackHistoryManufacturer Manufacturer_GetByName(ConnectionWrapper connection, string name)
        {
            return QueryFirstOrDefault<TrackHistoryManufacturer>(
                "SELECT * FROM [Manufacturer] WHERE [Name] = @name",
                new { name, },
                connection
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="manufacturer"></param>
        public void Manufacturer_Save(TrackHistoryManufacturer manufacturer)
        {
            Save(manufacturer, () => manufacturer.ManufacturerID != 0, Manufacturer_Insert, Manufacturer_Update);
        }

        private void Manufacturer_Insert(ConnectionWrapper connection, TrackHistoryManufacturer manufacturer)
        {
            Insert<TrackHistoryManufacturer, int>(connection, manufacturer, Commands.Manufacturer_Insert, id => manufacturer.ManufacturerID = id);
        }

        private void Manufacturer_Update(ConnectionWrapper connection, TrackHistoryManufacturer manufacturer)
        {
            Update<TrackHistoryManufacturer>(connection, manufacturer, Commands.Manufacturer_Update, created => manufacturer.CreatedUtc = created);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryManufacturer Manufacturer_GetOrCreateByName(string name)
        {
            return GetOrCreateByKey<TrackHistoryManufacturer, string>(
                name,
                Manufacturer_GetByName,
                Manufacturer_Insert,
                now => new TrackHistoryManufacturer() {
                    Name = name,
                    CreatedUtc = now,
                    UpdatedUtc = now,
                }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="manufacturer"></param>
        public void Manufacturer_Delete(TrackHistoryManufacturer manufacturer)
        {
            Execute(
                "DELETE FROM [Manufacturer] WHERE [ManufacturerID] = @ManufacturerID",
                new { manufacturer.ManufacturerID }
            );
        }
        #endregion

        #region Model
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryModel Model_GetByID(int id)
        {
            return QueryFirstOrDefault<TrackHistoryModel>(
                "SELECT * FROM [Model] WHERE [ModelID] = @id",
                new { id, }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryModel Model_GetByName(string name)
        {
            return Model_GetByName(null, name);
        }

        private TrackHistoryModel Model_GetByName(ConnectionWrapper connection, string name)
        {
            return QueryFirstOrDefault<TrackHistoryModel>(
                "SELECT * FROM [Model] WHERE [Name] = @name",
                new { name, },
                connection
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="model"></param>
        public void Model_Save(TrackHistoryModel model)
        {
            Save(model, () => model.ModelID != 0, Model_Insert, Model_Update);
        }

        private void Model_Insert(ConnectionWrapper connection, TrackHistoryModel model)
        {
            Insert<TrackHistoryModel, int>(connection, model, Commands.Model_Insert, id => model.ModelID = id);
        }

        private void Model_Update(ConnectionWrapper connection, TrackHistoryModel model)
        {
            Update<TrackHistoryModel>(connection, model, Commands.Model_Update, created => model.CreatedUtc = created);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryModel Model_GetOrCreateByName(string name)
        {
            return GetOrCreateByKey<TrackHistoryModel, string>(
                name,
                Model_GetByName,
                Model_Insert,
                now => new TrackHistoryModel() {
                    Name = name,
                    CreatedUtc = now,
                    UpdatedUtc = now,
                }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="model"></param>
        public void Model_Delete(TrackHistoryModel model)
        {
            Execute(
                "DELETE FROM [Model] WHERE [ModelID] = @ModelID",
                new { model.ModelID }
            );
        }
        #endregion

        #region Operator
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryOperator Operator_GetByID(int id)
        {
            return QueryFirstOrDefault<TrackHistoryOperator>(
                "SELECT * FROM [Operator] WHERE [OperatorID] = @id",
                new { id }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryOperator Operator_GetByKey(string icao, string name)
        {
            return Operator_GetByKey(null, icao, name);
        }

        public TrackHistoryOperator Operator_GetByKey(ConnectionWrapper connection, string icao, string name)
        {
            return QueryFirstOrDefault<TrackHistoryOperator>(
                "SELECT * FROM [Operator] WHERE [Icao] = @icao AND [Name] = @name",
                new { icao, name },
                connection
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryOperator Operator_GetOrCreateByKey(string icao, string name)
        {
            return GetOrCreateByCompositeKey<TrackHistoryOperator>(
                connection => Operator_GetByKey(connection, icao, name),
                Operator_Insert,
                now => new TrackHistoryOperator() {
                    Icao =          icao,
                    Name =          name,
                    CreatedUtc =    now,
                    UpdatedUtc =    now,
                }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="acOperator"></param>
        public void Operator_Save(TrackHistoryOperator acOperator)
        {
            Save<TrackHistoryOperator>(acOperator, () => acOperator.OperatorID != 0, Operator_Insert, Operator_Update);
        }

        private void Operator_Insert(ConnectionWrapper connection, TrackHistoryOperator acOperator)
        {
            Insert<TrackHistoryOperator, int>(connection, acOperator, Commands.Operator_Insert, id => acOperator.OperatorID = id);
        }

        private void Operator_Update(ConnectionWrapper connection, TrackHistoryOperator acOperator)
        {
            Update<TrackHistoryOperator>(connection, acOperator, Commands.Operator_Update, created => acOperator.CreatedUtc = created);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="acOperator"></param>
        public void Operator_Delete(TrackHistoryOperator acOperator)
        {
            Execute(
                "DELETE FROM [Operator] WHERE [OperatorID] = @OperatorID",
                new { acOperator.OperatorID }
            );
        }
        #endregion

        #region Receiver
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TrackHistoryReceiver Receiver_GetByID(int id)
        {
            return QueryFirstOrDefault<TrackHistoryReceiver>(
                "SELECT * FROM [Receiver] WHERE [ReceiverID] = @id",
                new { id, }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryReceiver Receiver_GetByName(string name)
        {
            return Receiver_GetByName(null, name);
        }

        private TrackHistoryReceiver Receiver_GetByName(ConnectionWrapper connection, string name)
        {
            return QueryFirstOrDefault<TrackHistoryReceiver>(
                "SELECT * FROM [Receiver] WHERE [Name] = @name",
                new { name, },
                connection
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TrackHistoryReceiver Receiver_GetOrCreateByName(string name)
        {
            return GetOrCreateByKey<TrackHistoryReceiver, string>(
                name,
                Receiver_GetByName,
                Receiver_Insert,
                now => new TrackHistoryReceiver() {
                    Name = name,
                    CreatedUtc = now,
                    UpdatedUtc = now,
                }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receiver"></param>
        public void Receiver_Save(TrackHistoryReceiver receiver)
        {
            RunWithinConnection((connection) => {
                if(receiver.ReceiverID == 0) {
                    Receiver_Insert(connection, receiver);
                } else {
                    receiver.CreatedUtc = connection.Connection.Query<DateTime>(
                        Commands.Receiver_Update,
                        receiver,
                        transaction: connection.Transaction
                    ).First();
                }
            });
        }

        private void Receiver_Insert(ConnectionWrapper connection, TrackHistoryReceiver receiver)
        {
            Insert<TrackHistoryReceiver, int>(connection, receiver, Commands.Receiver_Insert, id => receiver.ReceiverID = id);
        }

        private void Receiver_Update(ConnectionWrapper connection, TrackHistoryReceiver receiver)
        {
            Update<TrackHistoryReceiver>(connection, receiver, Commands.Receiver_Insert, created => receiver.CreatedUtc = created);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receiver"></param>
        public void Receiver_Delete(TrackHistoryReceiver receiver)
        {
            Execute(
                "DELETE FROM [Receiver] WHERE [ReceiverID] = @ReceiverID",
                new { receiver.ReceiverID }
            );
        }
        #endregion

        #region Species
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="species"></param>
        /// <returns></returns>
        public TrackHistorySpecies Species_GetByID(Species species)
        {
            return QueryFirstOrDefault<TrackHistorySpecies>(
                "SELECT * FROM [Species] WHERE [SpeciesID] = @SpeciesID",
                new { SpeciesID = species }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TrackHistorySpecies> Species_GetAll()
        {
            return Query<TrackHistorySpecies>(
                "SELECT * FROM [Species]",
                null
            ).ToArray();
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
                result = QueryFirst<TrackHistoryTruncateResult>(
                    Commands.TrackHistory_Delete,
                    new { trackHistory.TrackHistoryID, }
                );

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
                result = QueryFirst<TrackHistoryTruncateResult>(
                    Commands.TrackHistory_DeleteExpired,
                    new { threshold = deleteUpToUtc, }
                );

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

            RunWithinConnection((connection) => {
                result = Query<TrackHistory>(
                    Commands.TrackHistory_GetByDateRange,
                    new {
                        startTimeInclusive,
                        endTimeInclusive,
                    }
                ).ToArray();
            });

            return result ?? new TrackHistory[0];
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraftID"></param>
        /// <param name="startTimeInclusive"></param>
        /// <param name="endTimeInclusive"></param>
        /// <returns></returns>
        public IEnumerable<TrackHistory> TrackHistory_GetByAircraftID(long aircraftID, DateTime? startTimeInclusive, DateTime? endTimeInclusive)
        {
            TrackHistory[] result = null;

            RunWithinConnection((connection) => {
                result = Query<TrackHistory>(
                    Commands.TrackHistory_GetByAircraftID,
                    new {
                        aircraftID,
                        startTimeInclusive,
                        endTimeInclusive,
                    }
                ).ToArray();
            });

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

            RunWithinConnection((connection) => {
                result = QueryFirstOrDefault<TrackHistory>(
                    "SELECT * FROM [TrackHistory] WHERE [TrackHistoryID] = @trackHistoryID",
                    new { trackHistoryID = id, }
                );
            });

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
            return Query<TrackHistory>(@"
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
                connection
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="trackHistory"></param>
        public void TrackHistory_Save(TrackHistory trackHistory)
        {
            Save<TrackHistory>(trackHistory, () => trackHistory.TrackHistoryID != 0, TrackHistory_Insert, TrackHistory_Update);
        }

        private void TrackHistory_Insert(ConnectionWrapper connection, TrackHistory trackHistory)
        {
            Insert<TrackHistory, long>(connection, trackHistory, Commands.TrackHistory_Insert, id => trackHistory.TrackHistoryID = id);
        }

        private void TrackHistory_Update(ConnectionWrapper connection, TrackHistory trackHistory)
        {
            Update<TrackHistory>(connection, trackHistory, Commands.TrackHistory_Update, created => trackHistory.CreatedUtc = created);
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
                    RunWithinConnection((connection) => {
                        TrackHistory_Truncate(connection, trackHistory, newUpdatedUtc, result);
                    });

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
                RunWithinConnection((connection) => {
                    foreach(var trackHistory in TrackHistory_GetUpToCreatedUtc(connection, truncateUpToUtc, includePreserved: false, minimumHistoryStates: 3)) {
                        TrackHistory_Truncate(connection, trackHistory, newUpdatedUtc, result);
                    }
                });

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
            return QueryFirstOrDefault<TrackHistoryState>(
                "SELECT * FROM [TrackHistoryState] WHERE [TrackHistoryStateID] = @id",
                new { id }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="trackHistory"></param>
        /// <returns></returns>
        public IEnumerable<TrackHistoryState> TrackHistoryState_GetByTrackHistory(TrackHistory trackHistory)
        {
            return TrackHistoryState_GetByTrackHistory(null, trackHistory);
        }

        private TrackHistoryState[] TrackHistoryState_GetByTrackHistory(ConnectionWrapper connection, TrackHistory trackHistory)
        {
            return Query<TrackHistoryState>(@"
                    SELECT   *
                    FROM     [TrackHistoryState]
                    WHERE    [TrackHistoryID] = @trackHistoryID
                    ORDER BY [SequenceNumber]
                ", new {
                    trackHistoryID = trackHistory.TrackHistoryID,
                },
                connection
            ).ToArray();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="trackHistoryState"></param>
        public void TrackHistoryState_Save(TrackHistoryState trackHistoryState)
        {
            TrackHistoryState_Save(null, trackHistoryState);
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
                Execute(
                    Commands.TrackHistoryState_Update,
                    trackHistoryState,
                    connection
                );
            } else {
                trackHistoryState.TrackHistoryStateID = ExecuteScalar<long>(
                    Commands.TrackHistoryState_Insert,
                    trackHistoryState,
                    connection
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
                Execute(
                    "DELETE FROM [TrackHistoryState] WHERE [TrackHistoryStateID] = @TrackHistoryStateID",
                    new { trackHistoryState.TrackHistoryStateID, },
                    connection
                );
            }
        }
        #endregion

        #region WakeTurbulenceCategory
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="wakeTurbulenceCategory"></param>
        /// <returns></returns>
        public TrackHistoryWakeTurbulenceCategory WakeTurbulenceCategory_GetByID(WakeTurbulenceCategory wakeTurbulenceCategory)
        {
            return QueryFirstOrDefault<TrackHistoryWakeTurbulenceCategory>(
                "SELECT * FROM [WakeTurbulenceCategory] WHERE [WakeTurbulenceCategoryID] = @WakeTurbulenceCategoryID",
                new { WakeTurbulenceCategoryID = wakeTurbulenceCategory }
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TrackHistoryWakeTurbulenceCategory> WakeTurbulenceCategory_GetAll()
        {
            return Query<TrackHistoryWakeTurbulenceCategory>(
                "SELECT * FROM [WakeTurbulenceCategory]",
                null
            ).ToArray();
        }
        #endregion
    }
}
