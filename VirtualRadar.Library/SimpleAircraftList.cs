// Copyright © 2010 onwards, Andrew Whewell
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
using InterfaceFactory;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="ISimpleAircraftList"/>.
    /// </summary>
    class SimpleAircraftList : ISimpleAircraftList
    {
        #region Fields
        /// <summary>
        /// The object that manages the clock for us.
        /// </summary>
        private IClock _Clock;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public object ListSyncLock { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public AircraftListSource Source { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public List<IAircraft> Aircraft { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Count { get { return Aircraft.Count; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsTracking { get; private set; }
        #endregion

        #region Events
        #pragma warning disable 0067  // <-- suppress the warning about the events never being used
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <remarks>
        /// This implementation of aircraft list isn't multi-threaded so the event is redundant and never raised.
        /// </remarks>
        public event EventHandler<EventArgs<Exception>>  ExceptionCaught;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <remarks>The list is not observable so we don't bother with this event. Currently this is not a problem, if it is
        /// then it's not a big deal to retool <see cref="Aircraft"/> so that we can see modifications made to it and raise
        /// this event.</remarks>
        public event EventHandler CountChanged;
        #pragma warning restore 0067

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler TrackingStateChanged;

        /// <summary>
        /// Raises <see cref="TrackingStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnTrackingStateChanged(EventArgs args)
        {
            EventHelper.Raise(TrackingStateChanged, this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public SimpleAircraftList()
        {
            ListSyncLock = new object();
            Source = AircraftListSource.FakeAircraftList;
            Aircraft = new List<IAircraft>();
            _Clock = Factory.Resolve<IClock>();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~SimpleAircraftList()
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
            ;
        }
        #endregion

        #region Start, Stop, FindAircraft, TakeSnapshot
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            if(!IsTracking) {
                IsTracking = true;
                OnTrackingStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Stop()
        {
            if(IsTracking) {
                IsTracking = false;
                OnTrackingStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public IAircraft FindAircraft(int uniqueId)
        {
            IAircraft result = null;

            lock(ListSyncLock) {
                var aircraft = Aircraft.Where(a => a.UniqueId == uniqueId).FirstOrDefault();
                if(aircraft != null) {
                    lock(aircraft) {
                        result = (IAircraft)aircraft.Clone();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="snapshotTimeStamp"></param>
        /// <param name="snapshotDataVersion"></param>
        /// <returns></returns>
        public List<IAircraft> TakeSnapshot(out long snapshotTimeStamp, out long snapshotDataVersion)
        {
            snapshotTimeStamp = _Clock.UtcNow.Ticks;
            snapshotDataVersion = 0;

            var result = new List<IAircraft>();

            lock(ListSyncLock) {
                foreach(var aircraft in Aircraft) {
                    lock(aircraft) {
                        if(snapshotDataVersion < aircraft.DataVersion) snapshotDataVersion = aircraft.DataVersion;
                        result.Add((IAircraft)aircraft.Clone());
                    }
                }
            }

            return result;
        }
        #endregion
    }
}
