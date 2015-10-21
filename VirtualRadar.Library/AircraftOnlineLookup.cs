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
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library
{
    /// <summary>
    /// Default implementation of <see cref="IAircraftOnlineLookup"/>.
    /// </summary>
    sealed class AircraftOnlineLookup : IAircraftOnlineLookup
    {
        /// <summary>
        /// Represents an ICAO in the queue.
        /// </summary>
        class QueueEntry
        {
            /// <summary>
            /// The ICAO to lookup.
            /// </summary>
            public string Icao;

            /// <summary>
            /// The UTC date and time that the ICAO was placed in the queue.
            /// </summary>
            public DateTime QueueDate;

            /// <summary>
            /// Creates a new object.
            /// </summary>
            /// <param name="icao"></param>
            /// <param name="queueDate"></param>
            public QueueEntry(string icao, DateTime queueDate)
            {
                Icao = icao;
                QueueDate = queueDate;
            }
        }

        /// <summary>
        /// The longest period of time that we will try to fetch an ICAO before giving up. No event is ever raised for these,
        /// they just disappear into the ether.
        /// </summary>
        private static readonly int ExpireQueueEntryMinutes = 30;

        /// <summary>
        /// True once the object has been initialised.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// The shared configuration that we'll be using.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// The clock that the object will use.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The set of ICAOs that have been queued for lookup.
        /// </summary>
        private Dictionary<string, QueueEntry> _Queue = new Dictionary<string,QueueEntry>();

        /// <summary>
        /// The lock that forces single-threaded access to the queue.
        /// </summary>
        private object _QueueLock = new object();

        /// <summary>
        /// The time of the last lookup.
        /// </summary>
        private DateTime _LastLookup;

        /// <summary>
        /// The minimum number of seconds to wait until the next lookup.
        /// </summary>
        private int _SecondsToNextLookup;

        /// <summary>
        /// True if a lookup is in progress.
        /// </summary>
        private bool _LookupInProgress;

        private static IAircraftOnlineLookup _Singleton = new AircraftOnlineLookup();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftOnlineLookup Singleton { get { return _Singleton; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftOnlineLookupProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<AircraftOnlineLookupEventArgs> AircraftFetched;

        /// <summary>
        /// Raises <see cref="AircraftFetched"/>.
        /// </summary>
        /// <param name="argsBuilder"></param>
        private void OnAircraftFetched(Func<AircraftOnlineLookupEventArgs> argsBuilder)
        {
            EventHelper.Raise(AircraftFetched, this, argsBuilder);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        public void Lookup(string icao)
        {
            Initialise();

            if(_SharedConfiguration.Get().BaseStationSettings.LookupAircraftDetailsOnline) {
                var normalisedIcao = (icao ?? "").Trim().ToUpper();
                if(normalisedIcao.Length == 6) {
                    var queueEntry = new QueueEntry(normalisedIcao, _Clock.UtcNow);
                    lock(_Queue) {
                        if(!_Queue.ContainsKey(normalisedIcao)) {
                            _Queue.Add(normalisedIcao, queueEntry);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Initialises the object.
        /// </summary>
        private void Initialise()
        {
            if(!_Initialised) {
                lock(_QueueLock) {
                    if(_Initialised) {
                        _Initialised = true;
                        _SharedConfiguration = Factory.Singleton.Resolve<ISharedConfiguration>().Singleton;
                        _Clock = Factory.Singleton.Resolve<IClock>();

                        // Give plugins two ways to set the provider - either they can fetch the singleton for this
                        // object and set the provider to their own implementation or they can register their provider
                        // as the default implementation for IAircraftOnlineLookupProvider.
                        if(Provider == null) {
                            Provider = Factory.Singleton.Resolve<IAircraftOnlineLookupProvider>();
                            CheckProviderSanity();
                        }
                        _SecondsToNextLookup = Provider.MinSecondsBetweenRequests;

                        var heartbeatService = Factory.Singleton.Resolve<IHeartbeatService>().Singleton;
                        heartbeatService.FastTick += Heartbeat_FastTick;
                    }
                }
            }
        }

        /// <summary>
        /// Throws an exception if the provider is reporting insane values.
        /// </summary>
        private void CheckProviderSanity()
        {
            if(Provider.MaxBatchSize < 10) throw new InvalidOperationException("MaxBatchSize must be at least 10");
            if(Provider.MinSecondsBetweenRequests < 1) throw new InvalidOperationException("MinSecondsBetweenRequests cannot be less than 1");
            if(Provider.MaxSecondsAfterFailedRequest < Provider.MinSecondsBetweenRequests) throw new InvalidOperationException("MaxSecondsAfterFailedRequest cannot be less than MinSecondsBetweenRequests");
        }

        /// <summary>
        /// Raised roughly once a second on a background thread.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_FastTick(object sender, EventArgs args)
        {
            if(!_SharedConfiguration.Get().BaseStationSettings.LookupAircraftDetailsOnline) {
                if(_Queue.Count > 0) {
                    lock(_Queue) {
                        _Queue.Clear();
                    }
                }
            } else {
                if(_Queue.Count > 0 && !_LookupInProgress) {
                    var threshold = _LastLookup.AddSeconds(_SecondsToNextLookup);
                    if(_Clock.UtcNow > threshold) {
                        _LookupInProgress = true;
                        ThreadPool.QueueUserWorkItem(ThreadPool_LookupWorkItem);
                    }
                }
            }
        }

        /// <summary>
        /// Called on a threadpool thread, does the lookup.
        /// </summary>
        /// <param name="unusedState"></param>
        private void ThreadPool_LookupWorkItem(object unusedState)
        {
            try {
                CheckProviderSanity();
                var batchSize = Provider.MaxBatchSize;

                QueueEntry[] queueEntries;
                lock(_QueueLock) {
                    var removeThreshold = _Clock.UtcNow.AddMinutes(-ExpireQueueEntryMinutes);
                    var removeSet = _Queue.Where(r => r.Value.QueueDate <= removeThreshold).Select(r => r.Value.Icao).ToArray();
                    foreach(var removeKey in removeSet) {
                        _Queue.Remove(removeKey);
                    }

                    queueEntries = _Queue.Select(r => r.Value).OrderBy(r => r.QueueDate).Take(batchSize).ToArray();
                }
                if(queueEntries.Length > 0) {
                    var fetchedAircraft = new List<AircraftOnlineLookupDetail>();
                    var missingIcaos = new List<string>();
                    var removeIcaos = new List<string>();

                    var fetchFailed = true;
                    try {
                        fetchFailed = !Provider.DoLookup(queueEntries.Select(r => r.Icao).ToArray(), fetchedAircraft, missingIcaos);
                    } catch(System.Net.WebException) {
                        ;
                    } catch(ThreadAbortException) {
                        ;
                    } catch(Exception ex) {
                        var log = Factory.Singleton.Resolve<ILog>().Singleton;
                        log.WriteLine("AircraftOnlineLookup caught exception: {0}", ex.ToString());
                    }

                    _LastLookup = _Clock.UtcNow;
                    if(!fetchFailed) {
                        _SecondsToNextLookup = Provider.MinSecondsBetweenRequests;
                        OnAircraftFetched(() => new AircraftOnlineLookupEventArgs(fetchedAircraft, missingIcaos));
                    } else {
                        if(_SecondsToNextLookup + Provider.MinSecondsBetweenRequests < Provider.MaxSecondsAfterFailedRequest) {
                            _SecondsToNextLookup += Provider.MinSecondsBetweenRequests;
                        }
                        lock(_QueueLock) {
                            foreach(var icao in queueEntries.Select(r => r.Icao).Where(r => _Queue.ContainsKey(r))) {
                                _Queue.Remove(icao);
                            }
                        }
                    }
                }
            } catch(ThreadAbortException) {
                ;
            } catch {
                // Never let exceptions bubble up out of here
            } finally {
                _LookupInProgress = false;
            }
        }
    }
}
