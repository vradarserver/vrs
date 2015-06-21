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
using System.Timers;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that periodically raise an event.
    /// </summary>
    /// <remarks>
    /// This is just wrapping a standard System.Timers timer. It exists so that objects that rely on timers can
    /// be unit tested.
    /// </remarks>
    public interface ITimer : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating that the <see cref="Elapsed"/> event should be periodically raised.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Elapsed"/> event should be raised every time
        /// the interval elapses (true) or only once (false).
        /// </summary>
        bool AutoReset { get; set; }

        /// <summary>
        /// Gets or sets the interval at which to raise the <see cref="Elapsed"/> event.
        /// </summary>
        double Interval { get; set; }

        /// <summary>
        /// Raised when the interval elapses.
        /// </summary>
        event EventHandler Elapsed;

        /// <summary>
        /// Starts the timer.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the timer.
        /// </summary>
        /// <remarks>
        /// Note that the event can still fire while this function is running.
        /// </remarks>
        void Stop();
    }
}
