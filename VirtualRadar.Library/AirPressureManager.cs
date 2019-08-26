// Copyright © 2016 onwards, Andrew Whewell
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
    /// The default implementation of <see cref="IAirPressureManager"/>.
    /// </summary>
    class AirPressureManager : IAirPressureManager
    {
        /// <summary>
        /// True if the class has been started.
        /// </summary>
        private bool _Started;

        /// <summary>
        /// The heartbeat service that has been hooked.
        /// </summary>
        private IHeartbeatService _HeartbeatService;

        /// <summary>
        /// The shared configuration that the object uses.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// The date and time of the last successful download at UTC.
        /// </summary>
        private DateTime _LastSuccessfulDownloadUtc;

        /// <summary>
        /// True if a download operation has been queued on the threadpool.
        /// </summary>
        private bool _DownloadQueued;

        /// <summary>
        /// The clock we're going to use for dates and times.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The object that will run lookups in the background for us.
        /// </summary>
        private IBackgroundWorker _BackgroundWorker;

        private static IAirPressureManager _Singleton;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAirPressureManager Singleton
        {
            get {
                if(_Singleton == null) {
                    _Singleton = new AirPressureManager();
                }
                return _Singleton;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Enabled { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAirPressureDownloader Downloader { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IAirPressureLookup Lookup { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler DownloadCompleted;

        /// <summary>
        /// Raises <see cref="DownloadCompleted"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnDownloadCompleted(EventArgs args)
        {
            EventHelper.Raise(DownloadCompleted, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            if(!_Started) {
                _Started = true;
                _Clock = Factory.Resolve<IClock>();
                Downloader = Factory.Resolve<IAirPressureDownloader>();
                Lookup = Factory.ResolveSingleton<IAirPressureLookup>();

                _SharedConfiguration = Factory.Resolve<ISharedConfiguration>().Singleton;
                Enabled = _SharedConfiguration.Get().BaseStationSettings.DownloadGlobalAirPressureReadings;
                _SharedConfiguration.ConfigurationChanged += SharedConfiguration_ConfigurationChanged;

                if(_BackgroundWorker == null) {
                    _BackgroundWorker = Factory.Resolve<IBackgroundWorker>();
                    _BackgroundWorker.DoWork += BackgroundWorker_DoWork;
                }

                _HeartbeatService = Factory.ResolveSingleton<IHeartbeatService>();
                _HeartbeatService.SlowTick += HeartbeatService_SlowTick;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Stop()
        {
            if(!_Started) {
                if(_HeartbeatService != null) {
                    _HeartbeatService.SlowTick -= HeartbeatService_SlowTick;
                }
                if(_SharedConfiguration != null) {
                    _SharedConfiguration.ConfigurationChanged -= SharedConfiguration_ConfigurationChanged;
                }

                _Started = false;
            }
        }

        /// <summary>
        /// Called on a threadpool background thread. Downloads a fresh set of air pressures.
        /// Once the method has finished it sets a flag to indicate that a download operation
        /// is no longer queued.
        /// </summary>
        /// <param name="state"></param>
        private void DownloadAirPressuresOnBackgroundThread(object state = null)
        {
            // Under no circumstances should an exception be allowed to bubble up out of this
            try {
                DownloadAirPressures();
            } catch(ThreadAbortException) {
            } catch(Exception ex) {
                try {
                    var log = Factory.ResolveSingleton<ILog>();
                    log.WriteLine("Caught exception in DownloadAirPressuresOnBackgroundThread: {0}", ex.ToString());
                } catch {
                }
            } finally {
                _DownloadQueued = false;
            }
        }

        /// <summary>
        /// Fetches air pressures, stores them in the lookup and raises the event to say that new
        /// pressures are available.
        /// </summary>
        private void DownloadAirPressures()
        {
            AirPressure[] airPressures = null;
            try {
                airPressures = Downloader.Fetch();
                if(airPressures.Length == 0) {
                    airPressures = null;
                } else {
                    _LastSuccessfulDownloadUtc = _Clock.UtcNow;
                }
            } catch(System.Net.WebException) {
                airPressures = null;
            }

            if(airPressures != null) {
                Lookup.LoadAirPressures(airPressures, _LastSuccessfulDownloadUtc);
                OnDownloadCompleted(EventArgs.Empty);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SharedConfiguration_ConfigurationChanged(object sender, EventArgs args)
        {
            Enabled = _SharedConfiguration.Get().BaseStationSettings.DownloadGlobalAirPressureReadings;
        }

        /// <summary>
        /// Called when the slow timer ticks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HeartbeatService_SlowTick(object sender, EventArgs args)
        {
            if(Enabled && !_DownloadQueued) {
                var threshold = _LastSuccessfulDownloadUtc.AddMinutes(Downloader.IntervalMinutes);
                if(_Clock.UtcNow >= threshold) {
                    _DownloadQueued = true;
                    _BackgroundWorker.StartWork(null);
                }
            }
        }

        /// <summary>
        /// Called when the background worker queue starts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BackgroundWorker_DoWork(object sender, EventArgs args)
        {
            DownloadAirPressuresOnBackgroundThread();
        }
    }
}
