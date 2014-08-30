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
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interop;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The default implementation of the <see cref="IPActiveConnector"/>.
    /// </summary>
    class IPActiveConnector : Connector, IIPActiveConnector
    {
        #region Public fields
        /// <summary>
        /// The number of milliseconds the connector waits before abandoning an attempt to connect.
        /// </summary>
        public static readonly int ConnectTimeout = 10000;

        /// <summary>
        /// The number of milliseconds the connector waits before retrying the attempt to connect.
        /// </summary>
        public static readonly int RetryConnectTimeout = 5000;
        #endregion

        #region Fields
        /// <summary>
        /// The spin lock that gives us consistent access to the properties.
        /// </summary>
        private SpinLock _SpinLock = new SpinLock();

        /// <summary>
        /// The connection that the connector establishes.
        /// </summary>
        private SocketConnection _Connection;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UseKeepAlive { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int ResetConnectionTimeout { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public override bool IsPassive
        {
            get { return false; }
        }
        #endregion

        #region SetConnectionProperties
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="useKeepAlive"></param>
        /// <param name="resetConnectionTimeout"></param>
        public void SetConnectionProperties(string address, int port, bool useKeepAlive, int resetConnectionTimeout)
        {
            using(_SpinLock.AcquireLock()) {
                Address = address;
                Port = port;
                UseKeepAlive = _IsMono ? false : useKeepAlive;
                ResetConnectionTimeout = resetConnectionTimeout;
            }
        }
        #endregion

        #region DoEstablishConnection, GetConnection, CloseConnection
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoEstablishConnection()
        {
            Reconnect(ConnectionStatus.Connecting);
        }

        /// <summary>
        /// Keeps attempting to connect until a connection is established.
        /// </summary>
        private void Reconnect(ConnectionStatus status)
        {
            while(ConnectionStatus != ConnectionStatus.Connected) {
                try {
                    ConnectionStatus = status;
                    Connect();
                } catch(Exception ex) {
                    OnExceptionCaught(new EventArgs<Exception>(ex));
                }

                if(ConnectionStatus != ConnectionStatus.Connected) {
                    Thread.Sleep(RetryConnectTimeout);
                }
            }
        }

        /// <summary>
        /// Returns the connection that was current as-at the time the call is made.
        /// </summary>
        /// <returns></returns>
        private SocketConnection GetConnection()
        {
            _SpinLock.Lock();
            try {
                return _Connection;
            } finally {
                _SpinLock.Unlock();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public override void CloseConnection()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Connect, Disconnect, ConnectionAbandoned
        /// <summary>
        /// Connects to the remote machine.
        /// </summary>
        private void Connect()
        {
            Disconnect();

            string address = null;
            int port = 0;
            using(_SpinLock.AcquireLock()) {
                address = Address;
                port = Port;
            }

            var ipAddress = IPNetworkHelper.ResolveAddress(address);
            var endPoint = new IPEndPoint(ipAddress, port);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if(UseKeepAlive) {
                socket.IOControl(IOControlCode.KeepAliveValues, new TcpKeepAlive() {
                    OnOff = 1,
                    KeepAliveTime = 10 * 1000,
                    KeepAliveInterval = 1 * 1000
                }.BuildBuffer(), null);
            }

            var timeoutAction = new BackgroundThreadTimeout(ConnectTimeout, () => {
                socket.Connect(endPoint);
            });
            timeoutAction.PerformAction();

            SocketConnection connection;
            using(_SpinLock.AcquireLock()) {
                _Connection = connection = new SocketConnection(this, socket, ConnectionStatus.Disconnected);
            }

            RegisterConnection(connection, raiseConnectionEstablished: true, mirrorConnectionState: true);
            connection.ConnectionStatus = ConnectionStatus.Connected;
        }

        /// <summary>
        /// Disconnects the socket.
        /// </summary>
        private void Disconnect()
        {
            try {
                SocketConnection connection = GetConnection();
                if(connection != null) {
                    connection.Abandon();
                }
            } catch {
                ;
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="connection"></param>
        internal override void ConnectionAbandoned(IConnection connection)
        {
            var deregisterConnection = false;
            using(_SpinLock.AcquireLock()) {
                if(connection != null && connection == _Connection) {
                    deregisterConnection = true;
                    _Connection = null;
                }
            }

            if(deregisterConnection) {
                DeregisterConnection(connection, raiseConnectionClosed: true, stopMirroringConnectionState: true);
            }
        }
        #endregion
    }
}
