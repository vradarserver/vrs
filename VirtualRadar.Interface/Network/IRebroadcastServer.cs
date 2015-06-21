// Copyright © 2012 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface.Network
{
    /// <summary>
    /// The interface for objects that can rebroadcast messages or bytes received by an <see cref="IListener"/>.
    /// </summary>
    public interface IRebroadcastServer : IBackgroundThreadExceptionCatcher, IDisposable
    {
        /// <summary>
        /// Gets or sets the unique ID of the rebroadcast server.
        /// </summary>
        int UniqueId { get; set; }

        /// <summary>
        /// Gets or sets the name of the rebroadcast server.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the format that the server is going to send information in.
        /// </summary>
        RebroadcastFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the interval at which aircraft lists are rebroadcast.
        /// </summary>
        /// <remarks>
        /// Only used when the rebroadcast format involves sending the aircraft list instead of sending aircraft
        /// messages. Defaults to 1 second.
        /// </remarks>
        int SendListIntervalMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the feed to take messages and bytes from.
        /// </summary>
        IFeed Feed { get; set; }

        /// <summary>
        /// Gets or sets the object that establishes the connection for us.
        /// </summary>
        INetworkConnector Connector { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating that the server is online.
        /// </summary>
        /// <remarks>
        /// Setting this to false does not disconnect the clients attached to the <see cref="Connector"/>,
        /// it just stops sending bytes to them. To disconnect the clients you need to dispose of the provider
        /// and create a new one after you go offline.
        /// </remarks>
        bool Online { get; set; }

        /// <summary>
        /// Raised when <see cref="Online"/> changes.
        /// </summary>
        event EventHandler OnlineChanged;

        /// <summary>
        /// Initialises the listener and provider. After this has been called no changes should be made to any
        /// properties other than <see cref="Online"/>.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Returns a list of every connection to the rebroadcast server.
        /// </summary>
        /// <returns></returns>
        List<RebroadcastServerConnection> GetConnections();
    }
}
