// Copyright © 2014 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface.Network
{
    /// <summary>
    /// The interface for objects that can establish a connection with a remote machine.
    /// </summary>
    /// <remarks><para>
    /// Connectors do the work of establishing a link with another machine or a piece of
    /// hardware. The intention is that the connector is created by another object that
    /// then either sends or receives a stream of bytes over the connection.
    /// </para><para>
    /// Connectors and their child connections have the capacity to generate a LOT of
    /// exceptions under the right conditions. All exceptions are caught and routed
    /// through the background thread exception event. The last exception raised is
    /// also recorded.
    /// </para><para>
    /// You cannot instantiate an <see cref="IConnector"/>. There are other interfaces
    /// that are based on this that you can instantiate.
    /// </para></remarks>
    /// <seealso cref="INetworkConnector"/>
    /// <seealso cref="ISerialConnector"/>
    public interface IConnector : IBackgroundThreadExceptionCatcher, IDisposable
    {
        /// <summary>
        /// Gets or sets the name of the connector.
        /// </summary>
        /// <remarks>
        /// This is for diagnostic purposes only, the code doesn't rely on anything about
        /// the name.
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connector waits for other things to
        /// connect to it or it actively connects to other things.
        /// </summary>
        /// <remarks><para>
        /// Attempting to change this after <see cref="EstablishConnection"/> has been called
        /// will throw an exception.
        /// </para><para>
        /// Passive connectors wait for incoming connections from another machine. They
        /// can accept one or many simultaneous connections.
        /// </para><para>
        /// By contrast active connectors attempt to create a connection with a remote
        /// machine or hardware and usually keep trying until they manage to establish the
        /// connection. They only create one connection.
        /// </para></remarks>
        bool IsPassive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the connector supports multiple connections
        /// or a single connection.
        /// </summary>
        /// <remarks>
        /// Attempting to change this after <see cref="EstablishConnection"/> has been called
        /// will throw an exception.
        /// </remarks>
        bool IsSingleConnection { get; set; }

        /// <summary>
        /// Gets a value indicating that the connector has established a connection with at
        /// least one end point.
        /// </summary>
        bool HasConnection { get; }

        /// <summary>
        /// Gets the first (or only) connection established by the connector.
        /// </summary>
        IConnection Connection { get; }

        /// <summary>
        /// Gets the last exception raised during the course of the connector's work.
        /// </summary>
        TimestampedException LastException { get; }

        /// <summary>
        /// Gets the connection status. This only reflects the status of the connector -
        /// individual connections being maintained by the connector have their own status.
        /// </summary>
        ConnectionStatus ConnectionStatus { get; }

        /// <summary>
        /// Raised when a connection has been established. This will usually be raised from
        /// a background thread.
        /// </summary>
        event EventHandler<ConnectionEventArgs> ConnectionEstablished;

        /// <summary>
        /// Raised when a connection's connection state has changed. This will usually be raised from
        /// a background thread. Sender will either be an <see cref="IConnection"/> or this
        /// <see cref="IConnector"/>.
        /// </summary>
        event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raised when a connection has been permanently closed. This will usually be raised from
        /// a background thread.
        /// </summary>
        event EventHandler<ConnectionEventArgs> ConnectionClosed;

        /// <summary>
        /// Tells the connector to establish a connection. This is a non-blocking call, the
        /// function will return immediately and establish the connection in the background.
        /// </summary>
        void EstablishConnection();

        /// <summary>
        /// Tells the connector to close all connections. This blocks until all connections
        /// have been shut down.
        /// </summary>
        /// <remarks>
        /// Connections can be re-opened by calling <see cref="EstablishConnection"/> once
        /// this call returns.
        /// </remarks>
        void CloseConnection();

        /// <summary>
        /// Closes the connection and then establishes the connection.
        /// </summary>
        void RestartConnection();

        /// <summary>
        /// Returns an array of established connections made by the connector.
        /// </summary>
        /// <returns></returns>
        IConnection[] GetConnections();
    }
}
