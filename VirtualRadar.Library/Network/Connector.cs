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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The base for all connectors.
    /// </summary>
    abstract class Connector : IConnector
    {
        #region Fields
        /// <summary>
        /// The spin lock around the list of connections.
        /// </summary>
        private SpinLock _SpinLock = new SpinLock();

        /// <summary>
        /// The list of established connections.
        /// </summary>
        private List<IConnection> _Connections = new List<IConnection>();

        /// <summary>
        /// True if the application is running under Mono.
        /// </summary>
        protected bool _IsMono;

        /// <summary>
        /// The thread created when <see cref="EstablishConnection"/> is called. This thread is
        /// closed when <see cref="CloseConnection"/> is called.
        /// </summary>
        private Thread _ConnectThread;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public abstract bool IsPassive { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public abstract bool IsSingleConnection { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public TimestampedException LastException { get; protected set; }

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
            protected set {
                var raiseEvent = false;
                using(_SpinLock.AcquireLock()) {
                    if(_ConnectionStatus != value) {
                        _ConnectionStatus = value;
                        raiseEvent = true;
                    }
                }
                if(raiseEvent) {
                    OnConnectionStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public virtual bool HasConnection
        {
            get {
                using(_SpinLock.AcquireLock()) {
                    return _Connections.Count > 0;
                }
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnExceptionCaught(EventArgs<Exception> args)
        {
            if(!(args.Value is ThreadAbortException)) {
                LastException = new TimestampedException(args.Value);
                if(ExceptionCaught != null) {
                    try {
                        ExceptionCaught(this, args);
                    } catch {
                        ;
                    }
                }
            }
        }

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/> on behalf of an <see cref="IConnection"/>.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="ex"></param>
        internal virtual void RaiseConnectionException(IConnection connection, Exception ex)
        {
            if(ex != null && !(ex is ThreadAbortException)) {
                try {
                    LastException = new TimestampedException(ex);
                    if(ExceptionCaught != null) {
                        ExceptionCaught(connection, new EventArgs<Exception>(ex));
                    }
                } catch {
                    ;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionEstablished;

        /// <summary>
        /// Raises <see cref="ConnectionEstablished"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectionEstablished(ConnectionEventArgs args)
        {
            if(ConnectionEstablished != null) ConnectionEstablished(this, args);
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
            if(ConnectionStateChanged != null) ConnectionStateChanged(this, args);
        }

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/> on behalf of the connection.
        /// </summary>
        /// <param name="connection"></param>
        internal void RaiseConnectionStateChanged(IConnection connection)
        {
            if(ConnectionStateChanged != null && connection != null) {
                ConnectionStateChanged(connection, EventArgs.Empty);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<ConnectionEventArgs> ConnectionClosed;

        /// <summary>
        /// Raises <see cref="ConnectionClosed"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectionClosed(ConnectionEventArgs args)
        {
            if(ConnectionClosed != null) ConnectionClosed(this, args);
        }
        #endregion

        #region Ctors, Finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Connector()
        {
            _IsMono = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Connector()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                CloseConnection();

                // The CloseConnection call should stop the derivee from accepting or attempting to make
                // any more connections. It should also close all existing connections. This block just
                // cleans up anything that is left lying around. The SpinLock is for the benefit of
                // other threads that might still be in an event handler or something when this kicks off.
                _SpinLock.Lock();
                try {
                    foreach(var connection in _Connections) {
                        connection.Dispose();
                    }
                    _Connections.Clear();
                } finally {
                    _SpinLock.Unlock();
                }
            }
        }
        #endregion

        #region EstablishConnection, CloseConnection, RestartConnection, GetConnections
        /// <summary>
        /// See interface docs.
        /// </summary>
        public virtual void EstablishConnection()
        {
            using(_SpinLock.AcquireLock()) {
                if(_ConnectThread == null) {
                    _ConnectThread = new Thread(BackgroundEstablishConnection);
                    _ConnectThread.Start();
                }
            }
        }

        /// <summary>
        /// Establishes the connection on a background thread.
        /// </summary>
        protected virtual void BackgroundEstablishConnection()
        {
            try {
                DoEstablishConnection();
            } catch(Exception ex) {
                OnExceptionCaught(new EventArgs<Exception>(ex));
                _ConnectThread = null;
            }

            using(_SpinLock.AcquireLock()) {
                _ConnectThread = null;
            }
        }

        /// <summary>
        /// Does the work of establishing a connection.
        /// </summary>
        protected abstract void DoEstablishConnection();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public abstract void CloseConnection();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RestartConnection()
        {
            CloseConnection();
            EstablishConnection();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public virtual IConnection[] GetConnections()
        {
            _SpinLock.Lock();
            try {
                return _Connections.ToArray();
            } finally {
                _SpinLock.Unlock();
            }
        }
        #endregion

        #region RegisterConnection, DeregisterConnection, ConnectionAbandoned
        /// <summary>
        /// Adds a connection to the list of established connections.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="raiseConnectionEstablished"></param>
        /// <param name="mirrorConnectionState"></param>
        protected virtual void RegisterConnection(IConnection connection, bool raiseConnectionEstablished, bool mirrorConnectionState)
        {
            if(connection != null) {
                _SpinLock.Lock();
                try {
                    _Connections.Add(connection);
                } finally {
                    _SpinLock.Unlock();
                }

                if(raiseConnectionEstablished) {
                    OnConnectionEstablished(new ConnectionEventArgs(connection));
                }

                if(mirrorConnectionState) {
                    connection.ConnectionStateChanged += Connection_ConnectionStateChanged;
                }
            }
        }

        /// <summary>
        /// Removes a connection from the list of established connections.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="raiseConnectionClosed"></param>
        /// <param name="stopMirroringConnectionState"></param>
        protected virtual void DeregisterConnection(IConnection connection, bool raiseConnectionClosed, bool stopMirroringConnectionState)
        {
            if(connection != null) {
                using(_SpinLock.AcquireLock()) {
                    var index = _Connections.IndexOf(connection);
                    if(index != -1) {
                        _Connections.RemoveAt(index);
                    }
                }

                if(stopMirroringConnectionState) {
                    connection.ConnectionStateChanged -= Connection_ConnectionStateChanged;
                }

                if(raiseConnectionClosed) {
                    OnConnectionClosed(new ConnectionEventArgs(connection));
                }
            }
        }

        /// <summary>
        /// Called by a connection when it is abandoned.
        /// </summary>
        /// <param name="connection"></param>
        internal abstract void ConnectionAbandoned(IConnection connection);
        #endregion

        #region Events subscribed
        /// <summary>
        /// Called when a connection whose connection state is being mirrored raises
        /// an event indicating a change in state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connection_ConnectionStateChanged(object sender, EventArgs e)
        {
            var connection = (IConnection)sender;
            ConnectionStatus = connection.ConnectionStatus;
        }
        #endregion
    }
}
