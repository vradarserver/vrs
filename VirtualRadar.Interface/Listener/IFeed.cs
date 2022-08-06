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
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Interface.Listener
{
    /// <summary>
    /// The interface that ties together an aircraft list, which tracks the state of aircraft picked up by a
    /// receiver or merged feed, and a receiver or merged feed's identifier from the configuration details.
    /// </summary>
    public interface IFeed : IBackgroundThreadExceptionCatcher, IDisposable
    {
        /// <summary>
        /// Gets the unique identifier of the feed.
        /// </summary>
        int UniqueId { get; }

        /// <summary>
        /// Gets the name of the feed.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the aircraft list that the feed is supplying.
        /// </summary>
        IAircraftList AircraftList { get; }

        /// <summary>
        /// Gets a value indicating that the feed can be viewed from the web site.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Gets a value indicating whether the feed is connected to the source of aircraft message data.
        /// </summary>
        ConnectionStatus ConnectionStatus { get; }

        /// <summary>
        /// Raised when the feed connects or disconnects. Note that exceptions raised during parsing of
        /// messages will cause the object to automatically disconnect.
        /// </summary>
        event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Connects to the source of aircraft data.
        /// </summary>
        void Connect();

        /// <summary>
        /// Called implicitly by Dispose, disconnects from the source of aircraft data.
        /// </summary>
        void Disconnect();
    }
}
