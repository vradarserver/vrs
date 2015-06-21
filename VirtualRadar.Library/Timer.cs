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
    /// See base docs.
    /// </summary>
    class Timer : ITimer
    {
        /// <summary>
        /// The timer that this class wraps.
        /// </summary>
        private System.Threading.Timer _Timer;

        /// <summary>
        /// See base docs.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        public bool AutoReset { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        public double Interval { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        public event EventHandler Elapsed;

        /// <summary>
        /// Raises <see cref="Elapsed"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnElapsed(EventArgs args)
        {
            if(Elapsed != null) Elapsed(this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Timer()
        {
            _Timer = new System.Threading.Timer(TimerElapsed);
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Timer()
        {
            Dispose(false);
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        public void Start()
        {
            if(AutoReset) {
                _Timer.Change((long)Interval, (long)Interval);
            } else {
                _Timer.Change((long)Interval, System.Threading.Timeout.Infinite);
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        public void Stop()
        {
            _Timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        /// <summary>
        /// See base docs.
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
                Stop();
                _Timer.Dispose();
            }
        }

        /// <summary>
        /// Called when the timer has elapsed.
        /// </summary>
        /// <param name="state"></param>
        private void TimerElapsed(object state)
        {
            if(Enabled) {
                OnElapsed(EventArgs.Empty);
            }
        }
    }
}
