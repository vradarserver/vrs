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
using System.Linq;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Library
{
    /// <summary>
    /// Default implementation of <see cref="IAircraftOnlineLookupManager"/>.
    /// </summary>
    sealed class AircraftOnlineLookupManager : IAircraftOnlineLookupManager
    {
        /// <summary>
        /// Private class that records information about a cache.
        /// </summary>
        class CacheEntry
        {
            public IAircraftOnlineLookupCache Cache;

            public int Priority;

            public bool ManageLifetime;
        }

        /// <summary>
        /// Used to lock changes to the cache. Does not get used to lock the cache during reads, so take a copy
        /// of the cache list before use.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The list of registered caches. Take a copy of this before doing anything with it. Do not add
        /// directly to this list, overwrite it inside a lock on _SyncLock.
        /// </summary>
        private List<CacheEntry> _CacheEntries = new List<CacheEntry>();

        /// <summary>
        /// The object that can fetch aircraft details for us.
        /// </summary>
        private IAircraftOnlineLookup _AircraftOnlineLookup;

        /// <summary>
        /// The object that fetches the time for us.
        /// </summary>
        private IClock _Clock;

        private static IAircraftOnlineLookupManager _Singleton = new AircraftOnlineLookupManager();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftOnlineLookupManager Singleton { get { return _Singleton; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<AircraftOnlineLookupEventArgs> AircraftFetched;

        /// <summary>
        /// Raises <see cref="AircraftFetched"/>. Note that the class is sealed, hence why this is private
        /// instead of virtual.
        /// </summary>
        /// <param name="args"></param>
        private void OnAircraftFetched(AircraftOnlineLookupEventArgs args)
        {
            EventHelper.Raise(AircraftFetched, this, args);
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~AircraftOnlineLookupManager()
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
        /// Disposes or finalises the object. Note that the class is sealed.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_AircraftOnlineLookup != null) {
                    _AircraftOnlineLookup.AircraftFetched -= AircraftOnlineLookup_AircraftFetched;
                }

                var cacheEntries = _CacheEntries;
                lock(_SyncLock) _CacheEntries = new List<CacheEntry>();

                foreach(var cacheEntry in cacheEntries) {
                    if(cacheEntry.ManageLifetime) {
                        var disposableCache = cacheEntry.Cache as IDisposable;
                        if(disposableCache != null) disposableCache.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="priority"></param>
        /// <param name="letManagerControlLifetime"></param>
        public void RegisterCache(IAircraftOnlineLookupCache cache, int priority, bool letManagerControlLifetime)
        {
            if(cache == null) throw new ArgumentNullException("cache");

            lock(_SyncLock) {
                if(!IsCacheObjectRegistered(cache)) {
                    var newCache = new List<CacheEntry>(_CacheEntries);
                    newCache.Add(new CacheEntry() {
                        Cache = cache,
                        Priority = priority,
                        ManageLifetime = letManagerControlLifetime,
                    });
                    newCache.Sort((lhs, rhs) => rhs.Priority - lhs.Priority);
                    _CacheEntries = newCache;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        public bool IsCacheObjectRegistered(IAircraftOnlineLookupCache cache)
        {
            var cacheEntries = _CacheEntries;
            return cacheEntries.Any(r => Object.ReferenceEquals(cache, r.Cache));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="cache"></param>
        public void DeregisterCache(IAircraftOnlineLookupCache cache)
        {
            if(cache == null) throw new ArgumentNullException("cache");

            lock(_SyncLock) {
                var newCache = new List<CacheEntry>();
                foreach(var cacheEntry in _CacheEntries) {
                    if(!Object.ReferenceEquals(cacheEntry.Cache, cache)) {
                        newCache.Add(cacheEntry);
                    }
                }
                _CacheEntries = newCache;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        private void Initialise()
        {
            if(_AircraftOnlineLookup == null) {
                lock(_SyncLock) {
                    if(_AircraftOnlineLookup == null) {
                        _Clock = Factory.Singleton.Resolve<IClock>();
                        _AircraftOnlineLookup = Factory.Singleton.Resolve<IAircraftOnlineLookup>().Singleton;
                        _AircraftOnlineLookup.AircraftFetched += AircraftOnlineLookup_AircraftFetched;
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="baseStationAircraft"></param>
        /// <param name="searchedForBaseStationAircraft"></param>
        /// <returns></returns>
        public AircraftOnlineLookupDetail Lookup(string icao, BaseStationAircraft baseStationAircraft, bool searchedForBaseStationAircraft)
        {
            Initialise();

            var result = FetchAircraftDetailsFromCache(icao, baseStationAircraft, searchedForBaseStationAircraft);
            if(RecordNeedsRefresh(result)) {
                _AircraftOnlineLookup.Lookup(icao);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        /// <param name="baseStationAircraft"></param>
        /// <returns></returns>
        public Dictionary<string, AircraftOnlineLookupDetail> LookupMany(IEnumerable<string> icaos, IDictionary<string, BaseStationAircraft> baseStationAircraft)
        {
            Initialise();

            var uniqueIcaos = icaos.Where(r => r != null && r.Length == 6).Select(r => r.ToUpper()).Distinct().ToArray();
            var result = FetchManyAircraftDetailsFromCache(uniqueIcaos, baseStationAircraft);

            var refreshIcaos = result.Where(r => RecordNeedsRefresh(r.Value)).Select(r => r.Key).ToArray();
            if(refreshIcaos.Length > 0) {
                _AircraftOnlineLookup.LookupMany(refreshIcaos);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="manufacturer"></param>
        /// <param name="model"></param>
        /// <param name="operatorName"></param>
        /// <param name="lastUpdatedUtc"></param>
        /// <returns></returns>
        public bool RecordNeedsRefresh(string registration, string manufacturer, string model, string operatorName, DateTime lastUpdatedUtc)
        {
            Initialise();

            var result = false;
            var cacheEntries = _CacheEntries;
            var cache = cacheEntries.FirstOrDefault(r => r.Cache.Enabled);
            if(cache != null) {
                var now = _Clock.UtcNow;
                var detailsPresent = !String.IsNullOrEmpty(registration) ||
                                     !String.IsNullOrEmpty(manufacturer) ||
                                     !String.IsNullOrEmpty(model) ||
                                     !String.IsNullOrEmpty(operatorName);
                result =              !detailsPresent && lastUpdatedUtc.AddDays(1) <= now;
                if(!result) result =   detailsPresent && cache.Cache.RefreshOutOfDateAircraft && lastUpdatedUtc.AddDays(28) <= now;
            }

            return result;
        }

        /// <summary>
        /// Fetches an aircraft from the cache.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="baseStationAircraft"></param>
        /// <param name="searchedForBaseStationAircraft"></param>
        /// <returns></returns>
        private AircraftOnlineLookupDetail FetchAircraftDetailsFromCache(string icao, BaseStationAircraft baseStationAircraft, bool searchedForBaseStationAircraft)
        {
            AircraftOnlineLookupDetail result = null;

            var cacheEntries = _CacheEntries;
            var cacheEntry = cacheEntries.FirstOrDefault(r => r.Cache.Enabled);
            if(cacheEntry != null) {
                result = cacheEntry.Cache.Load(icao, baseStationAircraft, searchedForBaseStationAircraft);
            }

            return result;
        }

        /// <summary>
        /// Fetches many aircraft from the cache.
        /// </summary>
        /// <param name="icaos"></param>
        /// <param name="baseStationAircraft"></param>
        /// <returns></returns>
        private Dictionary<string, AircraftOnlineLookupDetail> FetchManyAircraftDetailsFromCache(IEnumerable<string> icaos, IDictionary<string, BaseStationAircraft> baseStationAircraft)
        {
            var result = new Dictionary<string, AircraftOnlineLookupDetail>();

            var remainingIcaos = new LinkedList<string>();
            foreach(var icao in icaos) {
                remainingIcaos.AddLast(icao);
            }

            var cacheEntries = _CacheEntries;
            var cacheEntry = cacheEntries.FirstOrDefault(r => r.Cache.Enabled);
            if(cacheEntry != null) {
                var cacheResults = cacheEntry.Cache.LoadMany(remainingIcaos, baseStationAircraft);
                foreach(var kvp in cacheResults.Where(r => r.Value != null)) {
                    result.Add(kvp.Key, kvp.Value);
                    remainingIcaos.Remove(kvp.Key);
                }
            }

            for(var node = remainingIcaos.First;node != null;node = node.Next) {
                result.Add(node.Value, null);
            }

            return result;
        }

        /// <summary>
        /// Returns true if the cache record needs a refresh.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private bool RecordNeedsRefresh(AircraftOnlineLookupDetail record)
        {
            var result = record == null;

            if(!result && record.UpdatedUtc != null) {
                result = RecordNeedsRefresh(record.Registration, record.Manufacturer, record.Model, record.Operator, record.UpdatedUtc.Value);
            }

            return result;
        }

        /// <summary>
        /// Called when the online lookup service has finished fetching aircraft details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AircraftOnlineLookup_AircraftFetched(object sender, AircraftOnlineLookupEventArgs args)
        {
            try {
                var cacheEntries = _CacheEntries;
                var firstEnabledCache = cacheEntries.FirstOrDefault(r => r.Cache.Enabled);
                if(firstEnabledCache != null) {
                    if(args.AircraftDetails.Count > 0) {
                        firstEnabledCache.Cache.SaveMany(args.AircraftDetails);
                    }
                    if(args.MissingIcaos.Count > 0) {
                        firstEnabledCache.Cache.RecordManyMissing(args.MissingIcaos);
                    }
                }

                OnAircraftFetched(args);
            } catch(ThreadAbortException) {
            } catch(Exception ex) {
                var log = Factory.Singleton.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception in AircraftOnlineLookupManager during AircraftFetched: {0}", ex);
            }
        }
    }
}
