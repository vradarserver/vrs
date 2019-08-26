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

namespace VirtualRadar.Library
{
    /// <summary>
    /// See interface docs.
    /// </summary>
    class AircraftOnlineLookupLog : IAircraftOnlineLookupLog
    {
        /// <summary>
        /// The collection of log entries that we're maintaining.
        /// </summary>
        private LinkedList<AircraftOnlineLookupLogEntry> _LogEntries = new LinkedList<AircraftOnlineLookupLogEntry>();

        /// <summary>
        /// The object that is used to lock <see cref="_LogEntries"/>.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// True if the object has been initialised.
        /// </summary>
        private bool _Initialised;

        private static IAircraftOnlineLookupLog _Singleton;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAircraftOnlineLookupLog Singleton
        {
            get {
                if(_Singleton == null) {
                    _Singleton = new AircraftOnlineLookupLog();
                }
                return _Singleton;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int LogLengthMinutes { get { return 30; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ResponsesChanged;

        /// <summary>
        /// Raises <see cref="ResponsesChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnResponsesChanged(EventArgs args)
        {
            EventHelper.Raise(ResponsesChanged, this, args);
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~AircraftOnlineLookupLog()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the object.
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
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_Initialised) {
                    Factory.ResolveSingleton<IAircraftOnlineLookup>().AircraftFetched -= AircraftOnlineLookup_AircraftFetched;
                    _LogEntries.Clear();
                    _Initialised = false;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(!_Initialised) {
                Factory.ResolveSingleton<IAircraftOnlineLookup>().AircraftFetched += AircraftOnlineLookup_AircraftFetched;
                _Initialised = true;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public AircraftOnlineLookupLogEntry[] GetResponses()
        {
            var hasChanged = false;
            AircraftOnlineLookupLogEntry[] result;

            lock(_SyncLock) {
                hasChanged = RemoveOldEntries();
                result = _LogEntries.ToArray();
            }

            if(hasChanged) {
                ThreadPool.QueueUserWorkItem(r => {
                    try {
                        OnResponsesChanged(EventArgs.Empty);
                    } catch {
                        ;
                    }
                });
            }

            return result;
        }

        /// <summary>
        /// Removes old entries from the log.
        /// </summary>
        /// <returns></returns>
        private bool RemoveOldEntries()
        {
            var result = false;

            var threshold = DateTime.UtcNow.AddMinutes(-LogLengthMinutes);
            for(var node = _LogEntries.First;node != null && node.Value.ResponseUtc <= threshold;node = _LogEntries.First) {
                _LogEntries.RemoveFirst();
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Called when an response is received from the online lookup.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AircraftOnlineLookup_AircraftFetched(object sender, AircraftOnlineLookupEventArgs args)
        {
            var hasChanged = false;

            lock(_SyncLock) {
                hasChanged = RemoveOldEntries();

                var now = DateTime.UtcNow;
                var startCount = _LogEntries.Count;

                foreach(var aircraftDetail in args.AircraftDetails) {
                    _LogEntries.AddLast(new AircraftOnlineLookupLogEntry() {
                            ResponseUtc =   now,
                            Icao =          aircraftDetail.Icao,
                            Detail =        aircraftDetail,
                    });
                }

                foreach(var missingIcao in args.MissingIcaos) {
                    _LogEntries.AddLast(new AircraftOnlineLookupLogEntry() {
                            ResponseUtc =   now,
                            Icao =          missingIcao,
                    });
                }

                if(_LogEntries.Count != startCount) hasChanged = true;
            }

            if(hasChanged) OnResponsesChanged(EventArgs.Empty);
        }
    }
}
