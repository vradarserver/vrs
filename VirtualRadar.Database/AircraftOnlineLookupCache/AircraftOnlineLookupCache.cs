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

namespace VirtualRadar.Database.AircraftOnlineLookupCache
{
    /// <summary>
    /// A standalone implementation of the aircraft online lookup cache. This is used if the user does
    /// not have the database writer plugin installed.
    /// </summary>
    class AircraftOnlineLookupCache : IAircraftOnlineLookupCache
    {
        /// <summary>
        /// The object used to ensure that all database access is single-threaded.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The object that handles the interactions with the aircraft detail table for us.
        /// </summary>
        private AircraftDetailTable _AircraftDetailTable = new AircraftDetailTable();

        /// <summary>
        /// The full path and filename of the cache database file.
        /// </summary>
        private string _FileName;

        /// <summary>
        /// True if the schema has been updated.
        /// </summary>
        private bool _SchemaUpdated;

        private static AircraftOnlineLookupCache _Singleton = new AircraftOnlineLookupCache();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftOnlineLookupCache Singleton { get { return _Singleton; } }

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
                        _AircraftDetailTable.CreateTable(connection);
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns></returns>
        public AircraftOnlineLookupDetail Load(string icao)
        {
            using(var connection = CreateOpenConnection()) {
                lock(_SyncLock) {
                    return _AircraftDetailTable.GetByIcao(connection, null, icao);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        /// <returns></returns>
        public Dictionary<string, AircraftOnlineLookupDetail> LoadMany(IEnumerable<string> icaos)
        {
            List<AircraftOnlineLookupDetail> details = null;
            var filteredIcaos = icaos.Where(r => !String.IsNullOrEmpty(r)).Select(r => r.ToUpper().Trim()).Distinct().ToArray();

            using(var connection = CreateOpenConnection()) {
                lock(_SyncLock) {
                    details = _AircraftDetailTable.GetManyByIcao(connection, null, filteredIcaos);
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
            using(var connection = CreateOpenConnection()) {
                lock(_SyncLock) {
                    if(lookupDetail.AircraftDetailId == 0) _AircraftDetailTable.Insert(connection, null, lookupDetail);
                    else                                   _AircraftDetailTable.Update(connection, null, lookupDetail);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="lookupDetails"></param>
        public void SaveMany(IEnumerable<AircraftOnlineLookupDetail> lookupDetails)
        {
            using(var connection = CreateOpenConnection()) {
                using(var transaction = connection.BeginTransaction()) {
                    lock(_SyncLock) {
                        foreach(var lookupDetail in lookupDetails) {
                            if(lookupDetail.AircraftDetailId == 0) _AircraftDetailTable.Insert(connection, transaction, lookupDetail);
                            else                                   _AircraftDetailTable.Update(connection, transaction, lookupDetail);
                        }
                    }
                }
            }
        }
    }
}
