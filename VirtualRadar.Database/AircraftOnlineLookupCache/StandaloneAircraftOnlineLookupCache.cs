// Copyright © 2015 onwards, Andrew Whewell
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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.SQLite;
using Dapper;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Database.AircraftOnlineLookupCache
{
    /// <summary>
    /// A standalone implementation of the aircraft online lookup cache. This is used if the user does
    /// not have the database writer plugin installed, or the database writer has been disabled, or the
    /// user has told the database writer not to cache records to BaseStation.sqb etc. etc. etc.
    /// </summary>
    class StandaloneAircraftOnlineLookupCache : IStandaloneAircraftOnlineLookupCache
    {
        /// <summary>
        /// The object used to ensure that all database access is single-threaded.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The full path and filename of the cache database file.
        /// </summary>
        private string _FileName;

        /// <summary>
        /// True if the schema has been updated.
        /// </summary>
        private bool _SchemaUpdated;

        /// <summary>
        /// See interface docs. The cache is always enabled.
        /// </summary>
        public bool Enabled { get { return true; } }

        /// <summary>
        /// See interface docs. The standalone cache always asks for refreshes of out-of-date aircraft.
        /// </summary>
        public bool RefreshOutOfDateAircraft { get { return true; } }

        /// <summary>
        /// Returns an open connection to the database.
        /// </summary>
        /// <returns></returns>
        private IDbConnection CreateOpenConnection()
        {
            var connectionStringBuilder = Factory.Singleton.Resolve<ISQLiteConnectionStringBuilder>().Initialise();
            connectionStringBuilder.DataSource = BuildFileName();
            connectionStringBuilder.DateTimeFormat = SQLiteDateFormats.ISO8601;
            connectionStringBuilder.FailIfMissing = false;
            connectionStringBuilder.ReadOnly = false;
            connectionStringBuilder.JournalMode = SQLiteJournalModeEnum.Persist;

            var result = Factory.Singleton.Resolve<ISQLiteConnectionProvider>().Create(connectionStringBuilder.ConnectionString);
            result.Open();

            UpdateSchema(result);

            return result;
        }

        /// <summary>
        /// Builds the filename if it hasn't already been built and returns it.
        /// </summary>
        /// <returns></returns>
        private string BuildFileName()
        {
            if(_FileName == null) {
                lock(_SyncLock) {
                    if(_FileName == null) {
                        var configurationStorage = Factory.Singleton.Resolve<IConfigurationStorage>().Singleton;
                        _FileName = Path.Combine(configurationStorage.Folder, "AircraftOnlineLookupCache.sqb");
                    }
                }
            }

            return _FileName;
        }

        /// <summary>
        /// Updates the schema.
        /// </summary>
        /// <param name="connection"></param>
        private void UpdateSchema(IDbConnection connection)
        {
            if(!_SchemaUpdated) {
                lock(_SyncLock) {
                    if(!_SchemaUpdated) {
                        _SchemaUpdated = true;
                        connection.Execute(Commands.UpdateSchema);

                        var currentVersion = 1;
                        var databaseVersion = connection.ExecuteScalar<long?>("SELECT [Version] FROM [DatabaseVersion]");
                        if(databaseVersion != currentVersion) {
                            connection.Execute("REPLACE INTO [DatabaseVersion] ([Version]) VALUES (@currentVersion)", new {
                                currentVersion = currentVersion,
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="baseStationAircraft">Unused.</param>
        /// <param name="searchedForBaseStationAircraft">Unused.</param>
        /// <returns></returns>
        public AircraftOnlineLookupDetail Load(string icao, BaseStationAircraft baseStationAircraft, bool searchedForBaseStationAircraft)
        {
            lock(_SyncLock) {
                using(var connection = CreateOpenConnection()) {
                    return connection.Query<AircraftOnlineLookupDetail>("SELECT * FROM [AircraftDetail] WHERE [Icao] = @icao", new {
                        icao = icao,
                    }).FirstOrDefault();
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        /// <param name="baseStationAircraft">Unused.</param>
        /// <returns></returns>
        public Dictionary<string, AircraftOnlineLookupDetail> LoadMany(IEnumerable<string> icaos, IDictionary<string, BaseStationAircraft> baseStationAircraft)
        {
            var filteredIcaos = icaos.Where(r => !String.IsNullOrEmpty(r)).Select(r => r.ToUpper().Trim()).Distinct().ToArray();

            var details = new List<AircraftOnlineLookupDetail>();
            lock(_SyncLock) {
                using(var connection = CreateOpenConnection()) {
                    const int batchSize = 200;
                    for(var i = 0;i < filteredIcaos.Length;i += batchSize) {
                        var batchIcaos = filteredIcaos.Skip(i).Take(batchSize).ToArray();
                        var batchDetails = connection.Query<AircraftOnlineLookupDetail>("SELECT * FROM [AircraftDetail] WHERE [Icao] IN @icaos", new {
                            @icaos = batchIcaos,
                        }).ToArray();
                        details.AddRange(batchDetails);
                    }
                }
            }

            var result = details.ToDictionary(r => r.Icao, r => r);
            foreach(var missingIcao in filteredIcaos.Except(details.Select(r => r.Icao.ToUpper()))) {
                result.Add(missingIcao, null);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="lookupDetail"></param>
        public void Save(AircraftOnlineLookupDetail lookupDetail)
        {
            lock(_SyncLock) {
                using(var connection = CreateOpenConnection()) {
                    UpsertFullAircraftDetail(connection, null, lookupDetail);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="lookupDetails"></param>
        public void SaveMany(IEnumerable<AircraftOnlineLookupDetail> lookupDetails)
        {
            lock(_SyncLock) {
                using(var connection = CreateOpenConnection()) {
                    using(var transaction = connection.BeginTransaction()) {
                        try {
                            foreach(var lookupDetail in lookupDetails) {
                                UpsertFullAircraftDetail(connection, transaction, lookupDetail);
                            }
                            transaction.Commit();
                        } catch {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        private void UpsertFullAircraftDetail(IDbConnection connection, IDbTransaction transaction, AircraftOnlineLookupDetail lookupDetail)
        {
            var now = DateTime.UtcNow;
            var existingId = GetAircraftDetailIDByIcao(connection, transaction, lookupDetail.Icao);
            if(existingId != null) {
                lookupDetail.UpdatedUtc = now;
                connection.Execute(Commands.AircraftDetail_Update, new {
                    @icao =             lookupDetail.Icao,
                    @registration =     lookupDetail.Registration,
                    @country =          lookupDetail.Country,
                    @manufacturer =     lookupDetail.Manufacturer,
                    @model =            lookupDetail.Model,
                    @modelIcao =        lookupDetail.ModelIcao,
                    @operator =         lookupDetail.Operator,
                    @operatorIcao =     lookupDetail.OperatorIcao,
                    @serial =           lookupDetail.Serial,
                    @yearBuilt =        lookupDetail.YearBuilt,
                    @updatedUtc =       lookupDetail.UpdatedUtc,
                    @aircraftDetailId = existingId.Value,
                }, transaction: transaction);
            } else {
                lookupDetail.CreatedUtc = now;
                lookupDetail.UpdatedUtc = now;
                lookupDetail.AircraftDetailId = connection.ExecuteScalar<long>(Commands.AircraftDetail_Insert, new {
                    @icao =             lookupDetail.Icao,
                    @registration =     lookupDetail.Registration,
                    @country =          lookupDetail.Country,
                    @manufacturer =     lookupDetail.Manufacturer,
                    @model =            lookupDetail.Model,
                    @modelIcao =        lookupDetail.ModelIcao,
                    @operator =         lookupDetail.Operator,
                    @operatorIcao =     lookupDetail.OperatorIcao,
                    @serial =           lookupDetail.Serial,
                    @yearBuilt =        lookupDetail.YearBuilt,
                    @createdUtc =       lookupDetail.CreatedUtc,
                    @updatedUtc =       lookupDetail.UpdatedUtc,
                });
            }
        }

        private long? GetAircraftDetailIDByIcao(IDbConnection connection, IDbTransaction transaction, string icao)
        {
            return connection.Query<long?>("SELECT [AircraftDetailId] FROM [AircraftDetail] WHERE [Icao] = @icao;", new {
                icao = icao
            }, transaction: transaction).FirstOrDefault();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        public void RecordMissing(string icao)
        {
            lock(_SyncLock) {
                using(var connection = CreateOpenConnection()) {
                    UpsertMissingAircraftDetail(connection, null, icao);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        public void RecordManyMissing(IEnumerable<string> icaos)
        {
            var filteredIcaos = icaos.Where(r => !String.IsNullOrEmpty(r)).Distinct().ToArray();
            lock(_SyncLock) {
                using(var connection = CreateOpenConnection()) {
                    using(var transaction = connection.BeginTransaction()) {
                        try {
                            foreach(var icao in filteredIcaos) {
                                UpsertMissingAircraftDetail(connection, transaction, icao);
                            }
                            transaction.Commit();
                        } catch {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
        }

        private void UpsertMissingAircraftDetail(IDbConnection connection, IDbTransaction transaction, string icao)
        {
            var now = DateTime.UtcNow;
            var existingId = GetAircraftDetailIDByIcao(connection, transaction, icao);
            if(existingId != null) {
                connection.Execute(Commands.AircraftDetail_UpdateMissing, new {
                    @updatedUtc = now,
                    @aircraftDetailId = existingId.Value,
                }, transaction: transaction);
            } else {
                connection.Execute(Commands.AircraftDetail_InsertMissing, new {
                    @icao =         icao,
                    @createdUtc =   now,
                    @updatedUtc =   now,
                });
            }
        }
    }
}
