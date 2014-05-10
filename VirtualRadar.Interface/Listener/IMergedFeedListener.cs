// Copyright © 2013 onwards, Andrew Whewell
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface.Listener
{
    /// <summary>
    /// A listener that merges together messages picked up by two or more listeners.
    /// </summary>
    /// <remarks><para>
    /// Calls on the merged feed's version of methods like Connect etc. have no effect on the
    /// receivers that the feed is merging together - all the merged feed is doing is taking the messages from
    /// the receivers and merging them together. Many properties, events and methods on <see cref="IListener"/>
    /// are stubs on the merged feed. The only properties, events and methods that are fully operational are:
    /// </para><list type=">">
    /// <item>TotalMessages</item>
    /// <item>TotalBadMessages</item>
    /// <item>ExceptionCaught</item>
    /// <item>Port30003MessageReceived</item>
    /// <item>PositionReset</item>
    /// </list><para>
    /// Specifically, the connection is always on and cannot be turned off. To control the receivers that the feed
    /// is merging you must send messages to each receiver.
    /// </para></remarks>
    public interface IMergedFeedListener : IListener
    {
        /// <summary>
        /// Gets the list of listeners that will be merged into a single feed.
        /// </summary>
        ReadOnlyCollection<IListener> Listeners { get; }

        /// <summary>
        /// Gets or sets the number of milliseconds that must elapse after the last message from a given ICAO on a receiver before
        /// that receiver is no longer considered to be the only source of messages for that ICAO.
        /// </summary>
        int IcaoTimeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether messages from aircraft are ignored until they transmit a position.
        /// </summary>
        bool IgnoreAircraftWithNoPosition { get; set; }

        /// <summary>
        /// Sets the <see cref="Listeners"/> list.
        /// </summary>
        /// <param name="listeners"></param>
        void SetListeners(IEnumerable<IListener> listeners);
    }
}
