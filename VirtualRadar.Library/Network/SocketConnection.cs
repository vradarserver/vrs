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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The default implementation of a connection that uses a plain socket.
    /// </summary>
    class SocketConnection : Connection
    {
        /// <summary>
        /// The spinlock that protects us from multi-threaded access.
        /// </summary>
        private SpinLock _SpinLock = new SpinLock();

        /// <summary>
        /// Gets the socket that the connection is using.
        /// </summary>
        public Socket Socket { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="socket"></param>
        /// <param name="initialStatus"></param>
        public SocketConnection(Connector connector, Socket socket, ConnectionStatus initialStatus) : base(connector, initialStatus)
        {
            Socket = socket;
        }

        /// <summary>
        /// Gets a reference to the socket that cannot be changed from other threads.
        /// </summary>
        /// <returns></returns>
        private Socket GetSocket()
        {
            _SpinLock.Lock();
            try {
                return Socket;
            } finally {
                _SpinLock.Unlock();
            }
        }

        /// <summary>
        /// See the base docs.
        /// </summary>
        protected override void AbandonConnection()
        {
            var closedSocket = false;

            using(_SpinLock.AcquireLock()) {
                if(Socket != null) {
                    closedSocket = true;
                    var socket = Socket;
                    Socket = null;

                    try {
                        socket.Close();
                        ((IDisposable)socket).Dispose();
                    } catch {
                    }
                }
            }

            if(closedSocket) {
                ConnectionStatus = ConnectionStatus.Disconnected;
                if(_Connector != null) _Connector.ConnectionAbandoned(this);
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="op"></param>
        protected override void DoRead(ReadWriteOperation op)
        {
            var socket = GetSocket();
            if(socket != null) {
                try {
                    op.BytesRead = socket.Receive(op.Buffer, op.Offset, op.Length, SocketFlags.None);
                    op.Abandon = op.BytesRead == 0;     // Other side has forcibly closed the connection
                } catch(Exception ex) {
                    if(_Connector != null) _Connector.RaiseConnectionException(this, ex);
                    op.Abandon = true;
                }
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="op"></param>
        protected override void DoWrite(ReadWriteOperation op)
        {
            var socket = GetSocket();
            if(socket != null) {
                try {
                    socket.Send(op.Buffer, op.Offset, op.Length, SocketFlags.None);
                } catch(Exception ex) {
                    if(_Connector != null) _Connector.RaiseConnectionException(this, ex);
                    op.Abandon = true;
                }
            }
        }
    }
}
