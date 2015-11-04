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
using VirtualRadar.Interface;

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

        private static IAircraftOnlineLookupManager _Singleton = new AircraftOnlineLookupManager();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftOnlineLookupManager Singleton { get { return _Singleton; } }

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
                var cacheEntries = _CacheEntries;
                lock(_SyncLock) _CacheEntries = new List<CacheEntry>();

                foreach(var cacheEntry in cacheEntries) {
                    if(cacheEntry.ManageLifetime) {
                        var disposableCache = cacheEntry as IDisposable;
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
    }
}
