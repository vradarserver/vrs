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
        public static readonly int ConnectTimeout = 30000;

        /// <summary>
        /// The number of milliseconds the connector waits before retrying the attempt to connect.
        /// </summary>
        public static readonly int RetryConnectTimeout = 10000;

        /// <summary>
        /// The number of milliseconds of inactivity before the connection times out.
        /// </summary>
        public static readonly int DefaultIdleTimeout = 60000;
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

        /// <summary>
        /// True if the connector should automatically reconnect if the connection goes down.
        /// </summary>
        private bool _ReconnectAfterAbandon;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public override bool IsPassive
        {
            get { return false; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public override bool IsSingleConnection
        {
            get { return true; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IConnection Connection
        {
            get { return GetConnection(); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Port { get; set; }

        private bool _UseKeepAlive;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UseKeepAlive
        {
            get { return _UseKeepAlive; }
            set { _UseKeepAlive = _IsMono ? false : value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int IdleTimeout { get; set; }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public IPActiveConnector() : base()
        {
            UseKeepAlive = true;
            IdleTimeout = DefaultIdleTimeout;
        }
        #endregion

        #region GetConnection
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
        #endregion

        #region DoEstablishConnection, CloseConnection
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoEstablishConnection()
        {
            _ReconnectAfterAbandon = true;
            Reconnect(ConnectionStatus.Connecting, pauseBeforeConnect: false);
        }

        private static bool _InReconnect;
        /// <summary>
        /// Keeps attempting to connect until a connection is established.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="pauseBeforeConnect"></param>
        private void Reconnect(ConnectionStatus status, bool pauseBeforeConnect)
        {
            if(!_InReconnect) {
                _InReconnect = true;
                try {
                    while(ConnectionStatus != ConnectionStatus.Connected && _ReconnectAfterAbandon) {
                        if(pauseBeforeConnect) {
                            var countSleeps = Math.Max(1, RetryConnectTimeout / 100);
                            for(var i = 0;i < countSleeps && _ReconnectAfterAbandon;++i) {
                                Thread.Sleep(100);
                            }
                        }
                        pauseBeforeConnect = true;

                        try {
                            ConnectionStatus = status;
                            Connect();
                        } catch(Exception ex) {
                            OnExceptionCaught(new EventArgs<Exception>(ex));
                        }
                    }
                } finally {
                    _InReconnect = false;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public override void CloseConnection()
        {
            _ReconnectAfterAbandon = false;
            var connection = GetConnection();
            if(connection != null) {
                connection.Abandon();
            }
        }
        #endregion

        #region Connect, Disconnect, ConnectionAbandoned
        /// <summary>
        /// Connects to the remote machine.
        /// </summary>
        private void Connect()
        {
            var ipAddress = IPNetworkHelper.ResolveAddress(Address);
            var endPoint = new IPEndPoint(ipAddress, Port);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if(!UseKeepAlive) {
                socket.ReceiveTimeout = IdleTimeout;
                socket.SendTimeout =    IdleTimeout;
            } else {
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

            if(_ReconnectAfterAbandon) {
                Reconnect(ConnectionStatus.Reconnecting, pauseBeforeConnect: true);
            }
        }
        #endregion
    }
}
