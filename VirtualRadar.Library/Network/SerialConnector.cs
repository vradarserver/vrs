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
using System.IO.Ports;
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The default implementation of <see cref="ISerialConnector"/>.
    /// </summary>
    class SerialConnector : Connector, ISerialConnector
    {
        #region Public fields
        /// <summary>
        /// The number of milliseconds the connector waits before abandoning an attempt to connect.
        /// </summary>
        public static readonly int ConnectTimeout = 10000;
        #endregion

        #region Fields
        /// <summary>
        /// The spin lock that gives us consistent access to the properties.
        /// </summary>
        private SpinLock _SpinLock = new SpinLock();

        /// <summary>
        /// The connection that the connector establishes.
        /// </summary>
        private SerialConnection _Connection;
        #endregion

        #region Behavioural properties
        protected override bool PassiveModeSupported        { get { return false; } }
        protected override bool ActiveModeSupported         { get { return true; } }
        protected override bool SingleConnectionSupported   { get { return true; } }
        protected override bool MultiConnectionSupported    { get { return false; } }
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ComPort { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int DataBits { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public StopBits StopBits { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Parity Parity { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Handshake Handshake { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string StartupText { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string ShutdownText { get; set; }
        #endregion

        #region GetConnection
        /// <summary>
        /// Returns the connection that was current as-at the time the call is made.
        /// </summary>
        /// <returns></returns>
        private SerialConnection GetConnection()
        {
            _SpinLock.Lock();
            try {
                return _Connection;
            } finally {
                _SpinLock.Unlock();
            }
        }
        #endregion

        #region DoEstablishConnection, DoCloseConnection, ConnectionAbandoned
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoEstablishConnection()
        {
            if(GetConnection() == null) {
                ConnectionStatus = ConnectionStatus.Connecting;

                SerialPort serialPort = null;
                var timeoutAction = new BackgroundThreadTimeout(ConnectTimeout, () => {
                    serialPort = new SerialPort(ComPort, BaudRate, Parity, DataBits, StopBits) {
                        ReadTimeout = 1000,
                        WriteTimeout = 1000,
                        Handshake = Handshake,
                        ReadBufferSize = 8192,
                    };
                    serialPort.Open();
                });
                timeoutAction.PerformAction();

                SerialConnection connection;
                using(_SpinLock.AcquireLock()) {
                    _Connection = connection = new SerialConnection(this, ConnectionStatus.Disconnected, serialPort, ShutdownText);
                }
                connection.SendText(StartupText);

                RegisterConnection(connection, raiseConnectionEstablished: true, mirrorConnectionState: true);
                connection.ConnectionStatus = ConnectionStatus.Connected;
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCloseConnection()
        {
            var connection = GetConnection();
            if(connection != null) {
                connection.Abandon();
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
