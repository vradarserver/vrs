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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for a singleton object that can configure and control a collection of <see cref="IRebroadcastServer"/>s.
    /// </summary>
    public interface IRebroadcastServerManager : ISingleton<IRebroadcastServerManager>, IBackgroundThreadExceptionCatcher, IDisposable
    {
        /// <summary>
        /// Gets the collection of rebroadcast servers that are being controlled by the manager.
        /// </summary>
        List<IRebroadcastServer> RebroadcastServers { get; }

        /// <summary>
        /// Gets or sets a flag indicating that the server is online.
        /// </summary>
        /// <remarks>
        /// Setting this to false does not disconnect the clients attached to the <see cref="IBroadcastProvider"/>,
        /// it just stops sending bytes to them. To disconnect the clients you need to dispose of the provider
        /// and create a new one after you go offline.
        /// </remarks>
        bool Online { get; set; }

        /// <summary>
        /// Raised when <see cref="Online"/> changes.
        /// </summary>
        event EventHandler OnlineChanged;

        /// <summary>
        /// Raised when a client connects to one of the servers.
        /// </summary>
        event EventHandler<ConnectionEventArgs> ClientConnected;

        /// <summary>
        /// Raised when a client disconnects from one of the servers.
        /// </summary>
        event EventHandler<ConnectionEventArgs> ClientDisconnected;

        /// <summary>
        /// Raised before some bytes are sent to a client.
        /// </summary>
        [Obsolete("Dropping support for this in IConnector - poll the connections instead")]
        event EventHandler<BroadcastEventArgs> BroadcastSending;

        /// <summary>
        /// Raised after some bytes have been sent to a client.
        /// </summary>
        [Obsolete("Dropping support for this in IConnector - poll the connections instead")]
        event EventHandler<BroadcastEventArgs> BroadcastSent;

        /// <summary>
        /// Creates the initial collection of rebroadcast servers from the configuration settings.
        /// </summary>
        void Initialise();

        /// <summary>
        /// Returns a list of objects describing all of the connections to the rebroadcast servers.
        /// </summary>
        List<RebroadcastServerConnection> GetConnections();
    }
}