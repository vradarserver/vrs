// Copyright © 2020 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using InterfaceFactory;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.SQLite;
using VirtualRadar.Interface.StateHistory;

namespace VirtualRadar.Database.StateHistory
{
    /// <summary>
    /// The SQLite implementation of <see cref="IStateHistoryRepository"/>.
    /// </summary>
    class StateHistoryRepository_SQLite : IStateHistoryRepository_SQLite
    {
        /// <summary>
        /// The connection string that the repository will always use.
        /// </summary>
        private string _ConnectionString;

        /// <summary>
        /// The database instance that holds configuration variables etc. for us.
        /// </summary>
        private IStateHistoryDatabaseInstance _DatabaseInstance;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsMissing { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool WritesEnabled => _DatabaseInstance?.WritesEnabled ?? false;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="databaseInstance"></param>
        public void Initialise(IStateHistoryDatabaseInstance databaseInstance)
        {
            if(_DatabaseInstance != null) {
                throw new InvalidOperationException($"You cannot initialise a {nameof(IStateHistoryRepository)} twice");
            }
            _DatabaseInstance = databaseInstance ?? throw new ArgumentNullException(nameof(databaseInstance));
            _ConnectionString = BuildConnectionString();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public DatabaseVersion DatabaseVersion_GetLatest()
        {
            using(var connection = OpenConnection(forWrite: false)) {
                return connection.Query<DatabaseVersion>(Scripts.DatabaseVersion_GetLatest)
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="record"></param>
        public void DatabaseVersion_Save(DatabaseVersion record)
        {
            using(var connection = OpenConnection(forWrite: true)) {
                connection.Execute(Scripts.DatabaseVersion_Save, record);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fingerprint"></param>
        /// <param name="createdUtc"></param>
        /// <param name="countryName"></param>
        /// <returns></returns>
        public CountrySnapshot CountrySnapshot_GetOrCreate(byte[] fingerprint, DateTime createdUtc, string countryName)
        {
            using(var connection = OpenConnection(forWrite: true)) {
                return connection.Query<CountrySnapshot>(Scripts.CountrySnapshot_GetOrCreate, new {
                    createdUtc,
                    fingerprint,
                    countryName,
                })
                .FirstOrDefault();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fingerprint"></param>
        /// <param name="createdUtc"></param>
        /// <param name="icao"></param>
        /// <param name="operatorName"></param>
        /// <returns></returns>
        public OperatorSnapshot OperatorSnapshot_GetOrCreate(byte[] fingerprint, DateTime createdUtc, string icao, string operatorName)
        {
            using(var connection = OpenConnection(forWrite: true)) {
                return connection.Query<OperatorSnapshot>(Scripts.OperatorSnapshot_GetOrCreate, new {
                    createdUtc,
                    fingerprint,
                    icao,
                    operatorName,
                })
                .FirstOrDefault();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Schema_Update()
        {
            using(var connection = OpenConnection(forWrite: true)) {
                connection.Execute(Scripts.Schema);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="session"></param>
        public void VrsSession_Insert(VrsSession session)
        {
            using(var connection = OpenConnection(forWrite: true)) {
                session.VrsSessionID = connection.Query<int>(Scripts.VrsSession_Insert, new {
                    session.DatabaseVersionID,
                    session.CreatedUtc,
                }).Single();
            }
        }

        /// <summary>
        /// Creates an open connection to the state history database.
        /// </summary>
        /// <param name="forWrite"></param>
        /// <returns></returns>
        protected IDbConnection OpenConnection(bool forWrite)
        {
            if(IsMissing) {
                throw new InvalidOperationException($"The {nameof(IStateHistoryRepository)} cannot be called, the database is missing");
            }
            if(!WritesEnabled && forWrite) {
                throw new InvalidOperationException($"Write operations on {nameof(IStateHistoryRepository)} are invalid, writes have been disabled");
            }

            var result = Factory.Resolve<ISQLiteConnectionProvider>()
                .Create(_ConnectionString);
            result.Open();

            return result;
        }

        private string BuildConnectionString()
        {
            var folder = _DatabaseInstance.NonStandardFolder;
            if(String.IsNullOrEmpty(folder)) {
                folder = Factory.ResolveSingleton<IConfigurationStorage>()
                    .Folder;
            }
            var fullPath = Path.Combine(folder, "StateHistory.sqb");

            if(_DatabaseInstance.WritesEnabled) {
                if(!Directory.Exists(folder)) {
                    Directory.CreateDirectory(folder);
                }
                if(!File.Exists(fullPath)) {
                    File
                        .Create(fullPath)
                        .Dispose();
                }
            }

            IsMissing = !File.Exists(fullPath);

            var result = "";
            if(!IsMissing) {
                var builder = Factory.Resolve<ISQLiteConnectionStringBuilder>()
                        .Initialise();
                builder.DataSource = fullPath;
                builder.DateTimeFormat = SQLiteDateFormats.ISO8601; // <-- not the most efficient but having different date formats in different SQLite files blows the ADO.NET adapter's mind
                builder.FailIfMissing = true;
                builder.JournalMode = SQLiteJournalModeEnum.Default;
                builder.ReadOnly = !_DatabaseInstance.WritesEnabled;

                result = builder.ConnectionString;
            }

            return result;
        }
    }
}
