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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Caches the results of online aircraft detail lookups.
    /// </summary>
    /// <remarks>
    /// Once you have created an instance of your cache you should register it with the singleton instance of the
    /// <see cref="IAircraftOnlineLookupManager"/>. Your cache will not be called until it has been registered.
    /// </remarks>
    public interface IAircraftOnlineLookupCache
    {
        /// <summary>
        /// Gets a flag indicating that the cache is enabled.
        /// </summary>
        /// <remarks>
        /// <see cref="IAircraftOnlineLookupManager"/> checks this before it asks the cache to do something. However, given
        /// the multi-threaded nature of things you might be asked to load or save objects after you have disabled yourself.
        /// When this happens your cache should behave graciously, even if it does not honour the request.
        /// </remarks>
        bool Enabled { get; }

        /// <summary>
        /// Fetches an aircraft's details from the cache.
        /// </summary>
        /// <param name="icao"></param>
        /// <returns>Null if there are no details for the ICAO in the cache, otherwise returns the cached details.</returns>
        AircraftOnlineLookupDetail Load(string icao);

        /// <summary>
        /// Loads multiple records from the cache simultaneously.
        /// </summary>
        /// <param name="icaos"></param>
        /// <returns>A dictionary of ICAOs to cached records. If there is no record for a particular ICAO in the cache
        /// then the dictionary value for the ICAO is null.</returns>
        Dictionary<string, AircraftOnlineLookupDetail> LoadMany(IEnumerable<string> icaos);

        /// <summary>
        /// Saves an aircraft's details in the cache.
        /// </summary>
        /// <param name="lookupDetail"></param>
        void Save(AircraftOnlineLookupDetail lookupDetail);

        /// <summary>
        /// Saves multiple records to the cache simultaneously.
        /// </summary>
        /// <param name="lookupDetails"></param>
        void SaveMany(IEnumerable<AircraftOnlineLookupDetail> lookupDetails);

        /// <summary>
        /// Records an ICAO that has no online details.
        /// </summary>
        /// <param name="icao"></param>
        void RecordMissing(string icao);

        /// <summary>
        /// Records many missing ICAOs.
        /// </summary>
        /// <param name="icaos"></param>
        void RecordManyMissing(IEnumerable<string> icaos);
    }
}
