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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The base class for connections.
    /// </summary>
    abstract class Connection : IConnection
    {
        /// <summary>
        /// The spinlock that protects our internals.
        /// </summary>
        private SpinLock _SpinLock = new SpinLock();

        /// <summary>
        /// The connector that owns this connection.
        /// </summary>
        protected Connector _Connector;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IConnector Connector { get { return _Connector; } }

        private ConnectionStatus _ConnectionStatus;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ConnectionStatus ConnectionStatus
        {
            get {
                ConnectionStatus result;
                using(_SpinLock.AcquireLock()) {
                    result = _ConnectionStatus;
                }
                return result;
            }
            internal set {
                var raiseEvent = false;
                using(_SpinLock.AcquireLock()) {
                    if(_ConnectionStatus != value) {
                        _ConnectionStatus = value;
                        raiseEvent = true;
                    }
                }
                if(raiseEvent) {
                    OnConnectionStateChanged(EventArgs.Empty);
                    _Connector.RaiseConnectionStateChanged(this);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ConnectionStateChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectionStateChanged(EventArgs args)
        {
            if(ConnectionStateChanged != null) {
                ConnectionStateChanged(this, args);
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="initialStatus"></param>
        public Connection(Connector connector, ConnectionStatus initialStatus)
        {
            if(connector == null) throw new ArgumentNullException("connector");
            _Connector = connector;

            ConnectionStatus = initialStatus;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Connection()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the connection.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public abstract void Abandon();
    }
}
