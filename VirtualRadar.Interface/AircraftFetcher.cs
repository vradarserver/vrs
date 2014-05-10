// Copyright © 2014 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A base class for fetcher implementations that register an aircraft and then periodically
    /// fetch records related to that aircraft.
    /// </summary>
    /// <typeparam name="TKey">The type of key to use when indexing the lookup details.</typeparam>
    /// <typeparam name="TDetail">The type of the DTO returned by the fetcher.</typeparam>
    /// <remarks>
    /// There are no unit tests on this class, it is an implementation detail for fetchers that use
    /// it. The unit tests must be on the fetchers themselves.
    /// </remarks>
    public abstract class AircraftFetcher<TKey, TDetail> : IDisposable
        where TDetail: class
    {
        #region Private class - FetchedDetail
        /// <summary>
        /// The fetched detail for an aircraft.
        /// </summary>
        protected class FetchedDetail
        {
            /// <summary>
            /// Gets or sets the key for the fetched detail record.
            /// </summary>
            public TKey Key { get; set; }

            /// <summary>
            /// Gets or sets the aircraft associated with the key, if known.
            /// </summary>
            public IAircraft Aircraft { get; set; }

            /// <summary>
            /// Gets or sets the detail associated with the key.
            /// </summary>
            public TDetail Detail { get; set; }

            /// <summary>
            /// Gets or sets the time that the key was last used in a registration call.
            /// </summary>
            public DateTime LastRegisteredUtc { get; set; }

            /// <summary>
            /// Gets or sets the time that the details were last fetched.
            /// </summary>
            public DateTime LastCheckedUtc { get; set; }

            /// <summary>
            /// Gets a value indicating that the aircraft's details have not yet been fetched.
            /// </summary>
            public bool IsFirstFetch { get { return LastCheckedUtc == default(DateTime); } }
        }
        #endregion

        #region Fields
        /// <summary>
        /// Our own heartbeat service - using our own timers avoids blocking other services that use
        /// the global heartbeat.
        /// </summary>
        private IHeartbeatService _PrivateHeartbeat;

        /// <summary>
        /// The spin lock that locks the queues and maps. Care must be taken that a function does
        /// not call another function protected by the same spin lock, otherwise it will deadlock.
        /// </summary>
        private SpinLock _QueueLock = new SpinLock();

        /// <summary>
        /// The spin lock that protects <see cref="FetchAircraft"/> from simultaneous calls. Care
        /// must be taken not to call FetchAircraft recursively otherwise it will deadlock.
        /// </summary>
        private SpinLock _FetchLock = new SpinLock();

        /// <summary>
        /// True once the object has been initialised.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// The clock that the object uses.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The queue of items that are to be immediately fetched.
        /// </summary>
        private Dictionary<TKey, FetchedDetail> _LookupQueue = new Dictionary<TKey,FetchedDetail>();

        /// <summary>
        /// The map of fetched details against their keys.
        /// </summary>
        private Dictionary<TKey, FetchedDetail> _FetchedDetailMap = new Dictionary<TKey,FetchedDetail>();
        #endregion

        #region Properties
        /// <summary>
        /// Gets the number of milliseconds between automatic refetches of the aircraft data. 0 indicates
        /// that automatic refetching is not required.
        /// </summary>
        protected abstract int AutomaticRecheckIntervalMilliseconds { get; }

        /// <summary>
        /// Gets the number of milliseconds that have to pass after the last registration before the aircraft
        /// is automatically deregistered.
        /// </summary>
        protected abstract int AutomaticDeregisterIntervalMilliseconds { get; }

        /// <summary>
        /// Gets a value indicating that the object has been disposed.
        /// </summary>
        protected bool Disposed { get; private set; }

        /// <summary>
        /// Gets or sets a value that, when set, will force a refresh of all aircraft on the next fast tick.
        /// </summary>
        protected bool ForceRefetchOnFastTick { get; set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public AircraftFetcher()
        {
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~AircraftFetcher()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
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
                if(!Disposed) {
                    if(_PrivateHeartbeat != null) {
                        _PrivateHeartbeat.FastTick -= Heartbeat_FastTimerTicked;
                        _PrivateHeartbeat.SlowTick -= Heartbeat_SlowTimerTicked;
                        _PrivateHeartbeat.Dispose();
                    }
                    Disposed = true;
                }
            }
        }
        #endregion

        #region Initialise
        /// <summary>
        /// Initialises the object.
        /// </summary>
        protected void Initialise()
        {
            if(!_Initialised) {
                _QueueLock.Lock();
                try {
                    if(!_Initialised) {
                        DoInitialise();
                        _Initialised = true;
                    }
                } finally {
                    _QueueLock.Unlock();
                }
            }
        }

        /// <summary>
        /// Initialises the object. This is called within a lock, do not call Initialise from
        /// within this.
        /// </summary>
        protected virtual void DoInitialise()
        {
            _Clock = Factory.Singleton.Resolve<IClock>();
            _PrivateHeartbeat = Factory.Singleton.Resolve<IHeartbeatService>();
            _PrivateHeartbeat.FastTick += Heartbeat_FastTimerTicked;
            _PrivateHeartbeat.SlowTick += Heartbeat_SlowTimerTicked;
            _PrivateHeartbeat.Start();
        }
        #endregion

        #region DoRegisterAircraft
        /// <summary>
        /// Registers or refreshes the entry for the key passed across.
        /// </summary>
        /// <param name="key">The key to register. This must be usable within a dictionary.</param>
        /// <param name="aircraft">The aircraft to associate with the key, if any. Pass null if the aircraft is unknown.</param>
        /// <returns></returns>
        protected virtual TDetail DoRegisterAircraft(TKey key, IAircraft aircraft)
        {
            TDetail result = null;
            Initialise();

            _QueueLock.Lock();
            try {
                FetchedDetail fetchedDetail;
                if(!_FetchedDetailMap.TryGetValue(key, out fetchedDetail)) {
                    if(!_LookupQueue.TryGetValue(key, out fetchedDetail)) {
                        fetchedDetail = new FetchedDetail() {
                            Aircraft = aircraft,
                            Key = key,
                        };
                        _LookupQueue.Add(key, fetchedDetail);
                    }
                }
                fetchedDetail.LastRegisteredUtc = _Clock.UtcNow;
                result = fetchedDetail.Detail;
            } finally {
                _QueueLock.Unlock();
            }

            return result;
        }
        #endregion

        #region FetchAircraft, FetchAllAircraft
        /// <summary>
        /// Fetches many aircraft simultaneously. 
        /// </summary>
        /// <param name="fetchedDetails"></param>
        /// <returns></returns>
        private bool FetchAllAircraft(IEnumerable<FetchedDetail> fetchedDetails)
        {
            _FetchLock.Lock();
            try {
                var result = DoFetchManyAircraft(fetchedDetails);
                if(result) {
                    var now = _Clock.UtcNow;
                    foreach(var fetchedDetail in fetchedDetails) {
                        fetchedDetail.LastCheckedUtc = now;
                    }
                }

                return result;
            } finally {
                _FetchLock.Unlock();
            }
        }

        /// <summary>
        /// Fetches the aircraft's details.
        /// </summary>
        /// <param name="fetchedDetail"></param>
        private void FetchAircraft(FetchedDetail fetchedDetail)
        {
            _FetchLock.Lock();
            try {
                var isFirstFetch = fetchedDetail.LastCheckedUtc == default(DateTime);
                fetchedDetail.LastCheckedUtc = _Clock.UtcNow;
                fetchedDetail.Detail = DoFetchAircraft(fetchedDetail);
            } finally {
                _FetchLock.Unlock();
            }
        }

        /// <summary>
        /// Returns the fetched detail for a key. The lock is acquired and released while fetching the detail.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected FetchedDetail GetFetchedDetailUnderLock(TKey key)
        {
            FetchedDetail result;
            _QueueLock.Lock();
            try {
                _FetchedDetailMap.TryGetValue(key, out result);
            } finally {
                _QueueLock.Unlock();
            }

            return result;
        }

        /// <summary>
        /// Fakes an aircraft fetch. Some derivees may hook events that pre-fetch the interesting data and they don't want to fetch
        /// it again with a call to FetchAircraft, so this applies the same lock as FetchAircraft and calls the action passed across.
        /// Do not call <see cref="FetchAircraft"/> from within this, it will deadlock.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="fakeFetchAircraft"></param>
        protected void FauxFetchAircraft(TKey key, Func<TKey, TDetail, bool, IAircraft, TDetail> fakeFetchAircraft)
        {
            FetchedDetail fetchedDetail;
            _QueueLock.Lock();
            try {
                _FetchedDetailMap.TryGetValue(key, out fetchedDetail);
            } finally {
                _QueueLock.Unlock();
            }

            if(fetchedDetail != null) {
                _FetchLock.Lock();
                try {
                    var isFirstFetch = fetchedDetail.LastCheckedUtc == default(DateTime);
                    fetchedDetail.LastCheckedUtc = _Clock.UtcNow;
                    fetchedDetail.Detail = fakeFetchAircraft(fetchedDetail.Key, fetchedDetail.Detail, isFirstFetch, fetchedDetail.Aircraft);
                } finally {
                    _FetchLock.Unlock();
                }
            }
        }

        /// <summary>
        /// Refetches all expired records, optionally forcing a refetch of records even if they have not expired.
        /// </summary>
        /// <param name="forceRefetch">True to fetch records that have already been recently fetched, false to only
        /// fetch those records that are past the <see cref="AutomaticRecheckIntervalMilliseconds"/> interval.</param>
        protected void RecheckAll(bool forceRefetch)
        {
            var now = _Clock.UtcNow;
            var recheckMilliseconds = AutomaticRecheckIntervalMilliseconds;

            var lookupList = new List<FetchedDetail>();
            _QueueLock.Lock();
            try {
                lookupList.AddRange(_FetchedDetailMap.Values.Where(r => forceRefetch || (r.LastCheckedUtc.Year > 1 && r.LastCheckedUtc.AddMilliseconds(recheckMilliseconds) <= now)));
            } finally {
                _QueueLock.Unlock();
            }

            if(!FetchAllAircraft(lookupList)) {
                foreach(var fetchedDetail in lookupList) {
                    FetchAircraft(fetchedDetail);
                }
            }
        }

        /// <summary>
        /// Fetches the detail for the aircraft, raising any events necessary when a change has been detected.
        /// </summary>
        /// <param name="fetchedDetail"></param>
        /// <returns></returns>
        protected abstract TDetail DoFetchAircraft(FetchedDetail fetchedDetail);

        /// <summary>
        /// When overridden by a derivee this fetches many aircraft details simultaneously.
        /// </summary>
        /// <param name="fetchedDetails">The aircraft to fetch.</param>
        /// <returns>False if the derivee does not support fetching many details simultaneously, or if the derivee could not
        /// fetch multiple aircraft, in which case each aircraft is fetched individually. True if the derivee fetched all
        /// aircraft successfully.</returns>
        protected virtual bool DoFetchManyAircraft(IEnumerable<FetchedDetail> fetchedDetails)
        {
            return false;
        }
        #endregion

        #region AutoDeregisterEntries
        /// <summary>
        /// Deregisters records that haven't been registered in a while.
        /// </summary>
        private void AutoDeregisterEntries()
        {
            var now = _Clock.UtcNow;
            var interval = AutomaticDeregisterIntervalMilliseconds;

            _QueueLock.Lock();
            try {
                var oldKeys = new List<TKey>();
                foreach(var kvp in _FetchedDetailMap) {
                    var key = kvp.Key;
                    var fetchedDetail = kvp.Value;
                    if(fetchedDetail.LastRegisteredUtc.AddMilliseconds(interval) <= now) oldKeys.Add(key);
                }

                foreach(var oldKey in oldKeys) {
                    _FetchedDetailMap.Remove(oldKey);
                }
            } finally {
                _QueueLock.Unlock();
            }
        }
        #endregion

        #region GetRegisteredAircraft
        /// <summary>
        /// Returns a copy of the dictionary that holds all of the registered aircraft for which
        /// at least one fetch has already been made.
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<TKey, TDetail> GetRegisteredAircraft()
        {
            var result = new Dictionary<TKey,TDetail>();

            _QueueLock.Lock();
            try {
                foreach(var kvp in _FetchedDetailMap) {
                    result.Add(kvp.Key, kvp.Value.Detail);
                }
            } finally {
                _QueueLock.Unlock();
            }

            return result;
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called roughly once a second on a background thread. By the time this has been called
        /// the object is guaranteed to have been initialised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_FastTimerTicked(object sender, EventArgs args)
        {
            if(ForceRefetchOnFastTick) {
                ForceRefetchOnFastTick = false;
                RecheckAll(forceRefetch: true);
            }

            var lookupList = new List<FetchedDetail>();
            _QueueLock.Lock();
            try {
                foreach(var newFetchedDetail in _LookupQueue.Values) {
                    lookupList.Add(newFetchedDetail);
                    _FetchedDetailMap.Add(newFetchedDetail.Key, newFetchedDetail);
                }
                _LookupQueue.Clear();
            } finally {
                _QueueLock.Unlock();
            }

            if(!FetchAllAircraft(lookupList)) {
                foreach(var newFetchedDetail in lookupList) {
                    FetchAircraft(newFetchedDetail);
                }
            }

            DoExtraFastTimerTickWork();
        }

        /// <summary>
        /// Called after the basic fast timer tick work has been completed.
        /// </summary>
        protected virtual void DoExtraFastTimerTickWork()
        {
            ;
        }

        /// <summary>
        /// Called when the slow timer has ticked on the heartbeat service, roughly once every
        /// 10 seconds. By the time this has been called the object is guaranteed to have been
        /// initialised.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_SlowTimerTicked(object sender, EventArgs args)
        {
            AutoDeregisterEntries();
            if(AutomaticRecheckIntervalMilliseconds > 0) RecheckAll(forceRefetch: false);
            DoExtraSlowTimerTickWork();
        }

        /// <summary>
        /// Called after the basic slow timer tick work has been completed.
        /// </summary>
        protected virtual void DoExtraSlowTimerTickWork()
        {
            ;
        }
        #endregion
    }
}
