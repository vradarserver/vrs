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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface.StateHistory;

namespace VirtualRadar.Library.StateHistory
{
    /// <summary>
    /// Default implementation of <see cref="IStateHistoryDatabaseInstance"/>.
    /// </summary>
    class StateHistoryDatabaseInstance : IStateHistoryDatabaseInstance
    {
        /// <summary>
        /// The database version ID for the current schema / recording methodology etc.
        /// </summary>
        private const long CurrentDatabaseVersionID = 1;

        /// <summary>
        /// The repository that this instance wraps.
        /// </summary>
        private IStateHistoryRepository _Repository;

        /// <summary>
        /// The cache of known objects indexed by fingerprint.
        /// </summary>
        private MemoryCache _Cache = new MemoryCache("StateHistoryDatabaseInstance cache");

        /// <summary>
        /// The policy to use when adding records to the cache.
        /// </summary>
        private CacheItemPolicy _CacheItemPolicy = new CacheItemPolicy() {
            SlidingExpiration = new TimeSpan(0, 10, 0),
        };

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool WritesEnabled { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string NonStandardFolder { get; private set; }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~StateHistoryDatabaseInstance()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                _Cache.Dispose();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="writesEnabled"></param>
        /// <param name="nonStandardFolder"></param>
        public void Initialise(bool writesEnabled, string nonStandardFolder)
        {
            WritesEnabled =     writesEnabled;
            NonStandardFolder = nonStandardFolder;
            _Repository =        Factory.Resolve<IStateHistoryRepository>();

            _Repository.Initialise(this);

            DoIfWriteable(repo => {
                repo.Schema_Update();

                if(repo.DatabaseVersion_GetLatest() == null) {
                    var databaseVersion = new DatabaseVersion() {
                        DatabaseVersionID = CurrentDatabaseVersionID,
                        CreatedUtc =        DateTime.UtcNow,
                    };
                    repo.DatabaseVersion_Save(databaseVersion);
                }

                repo.VrsSession_Insert(new VrsSession() {
                    DatabaseVersionID = CurrentDatabaseVersionID,
                    CreatedUtc =        DateTime.UtcNow,
                });
            });
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool DoIfReadable(Action<IStateHistoryRepository> action)
        {
            if(!_Repository.IsMissing) {
                action(_Repository);
            }

            return !_Repository.IsMissing;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool DoIfWriteable(Action<IStateHistoryRepository> action)
        {
            var result = _Repository.WritesEnabled;
            if(result) {
                result = DoIfReadable(action);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="countryName"></param>
        /// <returns></returns>
        public CountrySnapshot Country_GetOrCreate(string countryName)
        {
            return Snapshot_GetOrCreate(
                () => CountrySnapshot.TakeFingerprint(
                    countryName
                ),
                (repo, fingerprint, now) => repo.CountrySnapshot_GetOrCreate(
                    fingerprint,
                    now,
                    countryName
                )
            );
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="operatorName"></param>
        /// <returns></returns>
        public OperatorSnapshot Operator_GetOrCreate(string icao, string operatorName)
        {
            return Snapshot_GetOrCreate(
                () => OperatorSnapshot.TakeFingerprint(
                    icao,
                    operatorName
                ),
                (repo, fingerprint, now) => repo.OperatorSnapshot_GetOrCreate(
                    fingerprint,
                    now,
                    icao,
                    operatorName
                )
            );
        }

        private T Snapshot_GetOrCreate<T>(Func<byte[]> createFingerprint, Func<IStateHistoryRepository, byte[], DateTime, T> getOrCreate)
            where T: SnapshotRecord
        {
            T result = null;

            DoIfWriteable(repo => {
                var cache = _Cache;
                var fingerprint = createFingerprint();
                var key = Sha1Fingerprint.ConvertToString(fingerprint);
                result = cache.Get(key) as T;
                if(result == null) {
                    result = getOrCreate(repo, fingerprint, DateTime.UtcNow);
                    cache.Add(key, result, _CacheItemPolicy);
                }
            });

            return result;
        }
    }
}
