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
using InterfaceFactory;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Brings together the online lookup and the online lookup cache. If you want something from
    /// the online aircraft lookup then talk to this object.
    /// </summary>
    [Singleton]
    public interface IAircraftOnlineLookupManager : ISingleton<IAircraftOnlineLookupManager>, IDisposable
    {
        /// <summary>
        /// Raised when aircraft details have been looked up or fetched from the cache.
        /// </summary>
        event EventHandler<AircraftOnlineLookupEventArgs> AircraftFetched;

        /// <summary>
        /// Register a cache with the manager.
        /// </summary>
        /// <param name="cache">The cache to register.</param>
        /// <param name="priority">The relative priority of the cache. When the manager wants to save a
        /// record in the cache it uses higher value priority caches over lower values.</param>
        /// <param name="letManagerControlLifetime">True if the manager should dispose of the cache when
        /// it shuts down, false if something else is looking after the cache's lifetime. Has no effect
        /// if the cache does not implement IDisposable.</param>
        /// <remarks><para>
        /// More than one cache can be registered with the manager. By default VRS registers a standalone
        /// cache with a priority of zero. This caches aircraft records in the configuration folder. If the
        /// database writer plugin is installed then that also registers a cache, this time with a priority
        /// of 100 so that it takes precedence over the standalone cache. This means that BaseStation.sqb is
        /// used as a cache in preferance to the standalone cache, but if the user does not enable the
        /// caching feature of the database writer (which is off by default) then the manager falls back to
        /// the standalone cache.
        /// </para><para>
        /// If you are writing a plugin that supplies a cache, and you want the cache to take precedence over
        /// the standard VRS caches, then use a priority that is higher than 100. If you want to only be used
        /// when the fallback cache is switched off then use a priority lower than 0.
        /// </para><para>
        /// The priority cannot be changed once the cache has been registered.
        /// </para><para>
        /// <paramref name="letManagerControlLifetime"/> determines whether the Dispose method on the cache
        /// is called when the manager shuts down, usually when the program is closing. If the cache does not
        /// implement IDisposable then it has no effect.
        /// </para><para>
        /// This call does nothing if the cache has already been registered.
        /// </para></remarks>
        void RegisterCache(IAircraftOnlineLookupCache cache, int priority, bool letManagerControlLifetime);

        /// <summary>
        /// Returns true if the cache object passed across has already been registered.
        /// </summary>
        /// <param name="cache"></param>
        /// <returns></returns>
        bool IsCacheObjectRegistered(IAircraftOnlineLookupCache cache);

        /// <summary>
        /// Removes the cache passed across from the manager. Does nothing if the cache has not been registered.
        /// </summary>
        /// <param name="cache"></param>
        void DeregisterCache(IAircraftOnlineLookupCache cache);

        /// <summary>
        /// Requests a lookup. The details are either fetched from the cache and returned straight away or they
        /// are queued for lookup by the online service and <see cref="AircraftFetched"/> raised once the results
        /// are in.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="baseStationAircraft">Passed through to cache. Not used by the manager.</param>
        /// <param name="searchedForBaseStationAircraft">Passed through to cache. Not used by the manager.</param>
        /// <returns></returns>
        AircraftOnlineLookupDetail Lookup(string icao, BaseStationAircraft baseStationAircraft, bool searchedForBaseStationAircraft);

        /// <summary>
        /// Requests the lookup of many ICAOs. All ICAOs that are in the cache are returned immediately, the rest
        /// are passed to the online lookup service and returned via the <see cref="AircraftFetched"/> event.
        /// </summary>
        /// <param name="icaos"></param>
        /// <param name="baseStationAircraft">Passed through to cache. Not used by the manager.</param>
        /// <returns></returns>
        Dictionary<string, AircraftOnlineLookupDetail> LookupMany(IEnumerable<string> icaos, IDictionary<string, BaseStationAircraft> baseStationAircraft);

        /// <summary>
        /// Returns true if the details passed across indicate that the aircraft that they belong to needs refreshing.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="manufacturer"></param>
        /// <param name="model"></param>
        /// <param name="operatorName"></param>
        /// <param name="lastUpdatedUtc"></param>
        /// <returns></returns>
        bool RecordNeedsRefresh(string registration, string manufacturer, string model, string operatorName, DateTime lastUpdatedUtc);
    }
}
