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
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Database;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that can collect together a <see cref="IFeed"/> for each receiver and merged feed configured in settings.
    /// </summary>
    public interface IFeedManager : ISingleton<IFeedManager>, IBackgroundThreadExceptionCatcher, IDisposable
    {
        /// <summary>
        /// Gets a collection of active and enabled feeds.
        /// </summary>
        /// <remarks>
        /// If a feed has been disabled in the configuration then it is not included in this collection.
        /// </remarks>
        IFeed[] Feeds { get; }

        /// <summary>
        /// Raised when the collection of feeds managed by the object is changed.
        /// </summary>
        event EventHandler FeedsChanged;

        /// <summary>
        /// Raised when the listener attached to a feed changes its connection state.
        /// </summary>
        event EventHandler<EventArgs<IFeed>> ConnectionStateChanged;

        /// <summary>
        /// Initialises the manager.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Causes every feed to establish a connection.
        /// </summary>
        /// <param name="autoReconnect">True if the feed should keep trying to connect if at first it fails.</param>
        void Connect(bool autoReconnect);

        /// <summary>
        /// Causes every feed to disconnect from the data source.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Returns the feed with a given name or null if no such feed exists. Only enabled feed are returned.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IFeed GetByName(string name);

        /// <summary>
        /// Returns the feed with the given identifier or null if no such feed exists. Only enabled feeds are
        /// returned.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IFeed GetByUniqueId(int id);
    }
}
