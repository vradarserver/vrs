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
using System.Timers;
using System.Threading;
using InterfaceFactory;
using System.Diagnostics;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IHeartbeatService"/>.
    /// </summary>
    sealed class HeartbeatService : IHeartbeatService
    {
        /// <summary>
        /// An enumeration of the different ticks that the Heartbeat service exposes.
        /// </summary>
        enum TickType
        {
            SlowTick,
            FastTick,
        }

        /// <summary>
        /// The timer that will be used to drive the <see cref="SlowTick"/> event.
        /// </summary>
        private System.Timers.Timer _SlowTickTimer;

        /// <summary>
        /// The timer that will be used to drive the <see cref="FastTick"/> event.
        /// </summary>
        private System.Timers.Timer _FastTickTimer;

        private static readonly IHeartbeatService _Singleton = new HeartbeatService();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IHeartbeatService Singleton { get { return _Singleton; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler SlowTick;

        /// <summary>
        /// Raises <see cref="SlowTick"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnSlowTick(EventArgs args)
        {
            if(SlowTick != null) {
                foreach(EventHandler eventHandler in SlowTick.GetInvocationList()) {
                    try {
                        eventHandler(this, EventArgs.Empty);
                    } catch(ThreadAbortException) {
                    } catch(Exception ex) {
                        ILog log = Factory.Singleton.Resolve<ILog>().Singleton;
                        log.WriteLine("Caught an exception on a slow tick heartbeat event: {0}", ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler FastTick;

        /// <summary>
        /// Raises <see cref="FastTick"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnFastTick(EventArgs args)
        {
            if(FastTick != null) {
                foreach(EventHandler eventHandler in FastTick.GetInvocationList()) {
                    try {
                        eventHandler(this, EventArgs.Empty);
                    } catch(ThreadAbortException) {
                    } catch(Exception ex) {
                        ILog log = Factory.Singleton.Resolve<ILog>().Singleton;
                        log.WriteLine("Caught an exception on a fast tick heartbeat event: {0}", ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~HeartbeatService()
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
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_SlowTickTimer != null) _SlowTickTimer.Dispose();
                if(_FastTickTimer != null) _FastTickTimer.Dispose();
                _SlowTickTimer = _FastTickTimer = null;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            if(_SlowTickTimer == null) {
                _SlowTickTimer = new System.Timers.Timer(10000) { AutoReset = false };
                _SlowTickTimer.Elapsed += SlowTickTimer_Elapsed;

                _SlowTickTimer.Start();
            }

            if(_FastTickTimer == null) {
                _FastTickTimer = new System.Timers.Timer(1000) { AutoReset = false };
                _FastTickTimer.Elapsed += FastTickTimer_Elapsed;

                _FastTickTimer.Start();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void SlowTickNow()
        {
            ThreadPool.QueueUserWorkItem(BackgroundThreadRaiseTick, TickType.SlowTick);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void FastTickNow()
        {
            ThreadPool.QueueUserWorkItem(BackgroundThreadRaiseTick, TickType.FastTick);
        }

        /// <summary>
        /// Called on a background thread, raises <see cref="SlowTick"/>.
        /// </summary>
        /// <param name="state"></param>
        private void BackgroundThreadRaiseTick(object state)
        {
            TickType tickType = (TickType)state;
            switch(tickType) {
                case TickType.SlowTick:     SlowTickTimer_Elapsed(this, null); break;
                case TickType.FastTick:     FastTickTimer_Elapsed(this, null); break;
                default:                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Raised every time the slow tick timer elapses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlowTickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnSlowTick(EventArgs.Empty);
            _SlowTickTimer.Start();
        }

        /// <summary>
        /// Raised every time the fast tick timer elapses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FastTickTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            OnFastTick(EventArgs.Empty);
            _FastTickTimer.Start();
        }
    }
}
