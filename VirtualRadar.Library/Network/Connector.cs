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
        #region Static fields
        /// <summary>
        /// The maximum number of exceptions recorded by the connector.
        /// </summary>
        public static readonly int MaxExceptions = 20;
        #endregion

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
        /// The thread created when <see cref="EstablishConnection"/> is called. This may or may
        /// not live for the lifetime of the connector.
        /// </summary>
        private Thread _EstablishConnectionThread;

        /// <summary>
        /// True once <see cref="EstablishConnection"/> has been called.
        /// </summary>
        protected bool _EstablishConnectionCalled;
        #endregion

        #region Derivee behavioural properties
        protected abstract bool PassiveModeSupported         { get; }
        protected abstract bool ActiveModeSupported          { get; }
        protected abstract bool SingleConnectionSupported    { get; }
        protected abstract bool MultiConnectionSupported     { get; }
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Name { get; set; }

        private bool _IsPassive;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public virtual bool IsPassive
        {
            get { return _IsPassive; }
            set {
                if(_EstablishConnectionCalled) throw new InvalidOperationException("Cannot change IsPassive after EstablishConnection has been called");
                _IsPassive = value;
            }
        }

        private bool _IsSingleConnection;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public virtual bool IsSingleConnection
        {
            get { return _IsSingleConnection; }
            set {
                if(_EstablishConnectionCalled) throw new InvalidOperationException("Cannot change IsSingleConnection after EstablishConnection has been called");
                _IsSingleConnection = value;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int StaleMessageTimeout { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public virtual IConnection Connection
        {
            get { return GetFirstConnection(); }
        }

        /// <summary>
        /// The backing field used by <see cref="LastException"/> and <see cref="GetExceptionHistory"/>.
        /// </summary>
        private List<TimestampedException> _Exceptions = new List<TimestampedException>();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public TimestampedException LastException
        {
            get {
                using(_SpinLock.AcquireLock()) {
                    return _Exceptions.Count == 0 ? null : _Exceptions[_Exceptions.Count - 1];
                }
            }
        }

        /// <summary>
        /// A count of all exceptions ever recorded by the connector.
        /// </summary>
        private long _TotalExceptions;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long CountExceptions
        {
            get {
                using(_SpinLock.AcquireLock()) {
                    return _TotalExceptions;
                }
            }
        }


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
                RecordException(args.Value);
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
                    RecordException(ex);
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
                } catch(Exception ex) {
                    OnExceptionCaught(new EventArgs<Exception>(ex));
                } finally {
                    _SpinLock.Unlock();
                }
            }
        }
        #endregion

        #region RecordException, GetExceptionHistory
        /// <summary>
        /// Records an exception in <see cref="Exceptions"/>.
        /// </summary>
        /// <param name="ex"></param>
        private void RecordException(Exception ex)
        {
            if(ex != null && !(ex is ThreadAbortException)) {
                using(_SpinLock.AcquireLock()) {
                    while(_Exceptions.Count >= MaxExceptions) {
                        _Exceptions.RemoveAt(0);
                    }
                    _Exceptions.Add(new TimestampedException(ex));
                    ++_TotalExceptions;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public TimestampedException[] GetExceptionHistory()
        {
            using(_SpinLock.AcquireLock()) {
                return _Exceptions.ToArray();
            }
        }
        #endregion

        #region EstablishConnection, CloseConnection, RestartConnection, GetConnections, GetFirstConnection
        /// <summary>
        /// See interface docs.
        /// </summary>
        public virtual void EstablishConnection()
        {
            _EstablishConnectionCalled = true;

            if(IsPassive && !PassiveModeSupported) throw new InvalidOperationException(String.Format("Passive mode is not supported on {0} connectors", GetType().Name));
            if(!IsPassive && !ActiveModeSupported) throw new InvalidOperationException(String.Format("Active mode is not supported on {0} connectors", GetType().Name));
            if(IsSingleConnection && !SingleConnectionSupported) throw new InvalidOperationException(String.Format("Single connection mode is not supported on {0} connectors", GetType().Name));
            if(!IsSingleConnection && !MultiConnectionSupported) throw new InvalidOperationException(String.Format("Multi-connection mode is not supported on {0} connectors", GetType().Name));

            using(_SpinLock.AcquireLock()) {
                if(_EstablishConnectionThread == null) {
                    _EstablishConnectionThread = new Thread(BackgroundEstablishConnection);
                    _EstablishConnectionThread.Start();
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
                _EstablishConnectionThread = null;
            }

            using(_SpinLock.AcquireLock()) {
                _EstablishConnectionThread = null;
            }
        }

        /// <summary>
        /// Waits until the thread that is running <see cref="EstablishConnection"/> has finished.
        /// </summary>
        /// <param name="timeoutMilliseconds">Pass -1 to wait forever.</param>
        /// <returns></returns>
        protected void WaitForEstablishConnectionThreadToFinish(int timeoutMilliseconds = 1000)
        {
            var finished = false;
            var threshold = timeoutMilliseconds > 0 ? DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds) : DateTime.MaxValue;

            while(!finished) {
                _SpinLock.Lock();
                try {
                    finished = _EstablishConnectionThread == null;
                    if(!finished && DateTime.UtcNow >= threshold) {
                        try {
                            _EstablishConnectionThread.Abort();
                        } catch {
                        }
                        _EstablishConnectionThread = null;
                        finished = true;
                    }
                } finally {
                    _SpinLock.Unlock();
                }

                if(!finished) {
                    Thread.Sleep(0);
                }
            }
        }

        /// <summary>
        /// Does the work of establishing a connection.
        /// </summary>
        protected abstract void DoEstablishConnection();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public virtual void CloseConnection()
        {
            try {
                DoCloseConnection();
            } catch(Exception ex) {
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        /// <summary>
        /// Does the work of closing the connection.
        /// </summary>
        protected abstract void DoCloseConnection();

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

        /// <summary>
        /// Returns the first connection or null if there are no connections.
        /// </summary>
        /// <returns></returns>
        public virtual IConnection GetFirstConnection()
        {
            IConnection result;
            _SpinLock.Lock();
            try {
                result = _Connections.Count > 0 ? _Connections[0] : null;
            } finally {
                _SpinLock.Unlock();
            }

            return result;
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

        #region Read
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="readDelegate"></param>
        public void Read(byte[] buffer, ConnectionReadDelegate readDelegate)
        {
            Read(buffer, 0, buffer.Length, readDelegate);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="readDelegate"></param>
        public void Read(byte[] buffer, int offset, int length, ConnectionReadDelegate readDelegate)
        {
            var connection = GetFirstConnection();
            if(connection != null) {
                connection.Read(buffer, offset, length, readDelegate);
            }
        }
        #endregion

        #region Write
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="staleMessageTimeoutOverride"></param>
        public virtual void Write(byte[] buffer, int staleMessageTimeoutOverride = -1)
        {
            Write(buffer, 0, buffer.Length, staleMessageTimeoutOverride);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="staleMessageTimeoutOverride"></param>
        public virtual void Write(byte[] buffer, int offset, int length, int staleMessageTimeoutOverride = -1)
        {
            foreach(var connection in GetConnections()) {
                connection.Write(buffer, offset, length, staleMessageTimeoutOverride);
            }
        }
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
