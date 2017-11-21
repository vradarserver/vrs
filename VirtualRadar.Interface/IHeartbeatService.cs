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
using InterfaceFactory;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A service that raises an event on a background thread every so-many minutes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The intention with this is to make it a bit easier to write tests for code that checks for
    /// program updates / new versions of files once every day or so. It can get complicated if the
    /// code that does the checking is using a real timer or is invoking code on a second thread.
    /// </para><para>
    /// If an object wants to periodically do some work then it hooks either <see cref="SlowTick"/>
    /// or <see cref="FastTick"/> on the object exposed by the Singleton property. These events are
    /// raised on a background thread so the object that hooks them does not have to get involved with
    /// maintaining a background thread or with periodically running a method on a background thread.
    /// However none of the Tick events are <em>guaranteed</em> to be raised exactly so-many seconds
    /// apart so the object that hooks them is expected to keep a track of the time that has elapsed
    /// since the last invocation of the event handler and make sure that they don't perform their
    /// periodic work before the requisite period of real time has elapsed.
    /// </para><para>
    /// Exceptions raised on the event handlers are currently logged but not pushed up to the GUI.
    /// </para><para>
    /// The Singleton version of the heartbeat service is started by the splash screen and is available
    /// to plugins in their Startup method. However if plugins want to maintain use their own instance
    /// of the heartbeat service they are welcome to do so - by doing this they can avoid blocking
    /// other objects that use the service. However if timely background processing is an issue for a
    /// plugin then perhaps the heartbeat service is not the best way of implementing it.
    /// </para></remarks>
    [Singleton]
    public interface IHeartbeatService : IDisposable
    {
        /// <summary>
        /// Raised once every ten seconds or so.
        /// </summary>
        event EventHandler SlowTick;

        /// <summary>
        /// Raised once every second or so.
        /// </summary>
        event EventHandler FastTick;

        /// <summary>
        /// Starts the timers.
        /// </summary>
        void Start();

        /// <summary>
        /// Raises <see cref="SlowTick"/> on a background thread.
        /// </summary>
        void SlowTickNow();

        /// <summary>
        /// Raises <see cref="FastTick"/> on a background thread.
        /// </summary>
        void FastTickNow();
    }
}
