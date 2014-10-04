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
        #region Static Fields
        /// <summary>
        /// The number of connections that have ever been created.
        /// </summary>
        private static long _ConnectionCount;
        #endregion

        #region Fields
        /// <summary>
        /// The spinlock that protects our internals.
        /// </summary>
        private SpinLock _SpinLock = new SpinLock();

        /// <summary>
        /// The queue of operations to perform on the connection.
        /// </summary>
        private BackgroundThreadQueue<ReadWriteOperation> _OperationQueue;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public DateTime Created { get; private set; }

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

        private long _BytesRead;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BytesRead
        {
            get {
                _SpinLock.Lock();
                try {
                    return _BytesRead;
                } finally {
                    _SpinLock.Unlock();
                }
            }
        }

        private long _WriteQueueBytes;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public long WriteQueueBytes
        {
            get {
                _SpinLock.Lock();
                try {
                    return _WriteQueueBytes;
                } finally {
                    _SpinLock.Unlock();
                }
            }
        }

        private long _BytesWritten;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BytesWritten
        {
            get {
                _SpinLock.Lock();
                try {
                    return _BytesWritten;
                } finally {
                    _SpinLock.Unlock();
                }
            }
        }

        private long _StaleBytesDiscarded;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public long StaleBytesDiscarded
        {
            get {
                _SpinLock.Lock();
                try {
                    return _StaleBytesDiscarded;
                } finally {
                    _SpinLock.Unlock();
                }
            }
        }


        /// <summary>
        /// See interface docs.
        /// </summary>
        public virtual int OperationQueueEntries
        {
            get {
                using(_SpinLock.AcquireLock()) {
                    return _OperationQueue.GetQueueLength();
                }
            }
        }
        #endregion

        #region Events
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
        #endregion

        #region Ctor, finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="initialStatus"></param>
        public Connection(Connector connector, ConnectionStatus initialStatus)
        {
            if(connector == null) throw new ArgumentNullException("connector");
            _Connector = connector;

            Created = DateTime.UtcNow;

            string operationQueueName;
            using(_SpinLock.AcquireLock()) {
                operationQueueName = String.Format("ConnectionOpQueue-{0}-{1}", _Connector.Name ?? "unnamed", ++_ConnectionCount);
            }
            _OperationQueue = new BackgroundThreadQueue<ReadWriteOperation>(operationQueueName, surrenderTimeSliceOnEmptyQueue: true);
            _OperationQueue.StartBackgroundThread(OperationQueue_ProcessOperation, OperationQueue_ProcessException);

            ConnectionStatus = initialStatus;
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~Connection()
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
        /// Disposes of or finalises the connection.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                try {
                    if(_OperationQueue != null) {
                        _OperationQueue.Stop();
                    }
                } catch {
                    ;
                }

                try {
                    DoAbandon(raiseExceptionEvent: false);
                } catch {
                    ;
                }
            }
        }
        #endregion

        #region IncrementBytesRead, IncrementBytesWritten, IncrementWriteQueueBytes, IncrementStaleBytesDiscarded
        /// <summary>
        /// Adds a number of bytes to <see cref="BytesRead"/>.
        /// </summary>
        /// <param name="bytes"></param>
        protected void IncrementBytesRead(int bytes)
        {
            _SpinLock.Lock();
            try {
                _BytesRead += bytes;
            } finally {
                _SpinLock.Unlock();
            }
        }

        /// <summary>
        /// Adds a number of bytes to <see cref="BytesWritten"/>.
        /// </summary>
        /// <param name="bytes"></param>
        protected void IncrementBytesWritten(int bytes)
        {
            _SpinLock.Lock();
            try {
                _BytesWritten += bytes;
            } finally {
                _SpinLock.Unlock();
            }
        }

        /// <summary>
        /// Adds a number of bytes to <see cref="WriteQueueBytes"/>.
        /// </summary>
        /// <param name="bytes"></param>
        protected void IncrementWriteQueueBytes(int bytes)
        {
            _SpinLock.Lock();
            try {
                _WriteQueueBytes += bytes;
            } finally {
                _SpinLock.Unlock();
            }
        }

        /// <summary>
        /// Adds a number of bytes to <see cref="StaleBytesDiscarded"/>.
        /// </summary>
        /// <param name="bytes"></param>
        protected void IncrementStaleBytesDiscarded(int bytes)
        {
            _SpinLock.Lock();
            try {
                _StaleBytesDiscarded += bytes;
            } finally {
                _SpinLock.Unlock();
            }
        }
        #endregion

        #region GetOperationQueue, GetConnector
        /// <summary>
        /// Returns the operation queue that is currently in force.
        /// </summary>
        /// <returns></returns>
        private BackgroundThreadQueue<ReadWriteOperation> GetOperationQueue()
        {
            _SpinLock.Lock();
            try {
                return _OperationQueue;
            } finally {
                _SpinLock.Unlock();
            }
        }

        /// <summary>
        /// Returns the connector that currently owns this connection.
        /// </summary>
        /// <returns></returns>
        private IConnector GetConnector()
        {
            _SpinLock.Lock();
            try {
                return _Connector;
            } finally {
                _SpinLock.Unlock();
            }
        }
        #endregion

        #region Abandon, AbandonConnection
        /// <summary>
        /// See interface docs.
        /// </summary>
        public virtual void Abandon()
        {
            DoAbandon(true);
        }

        private bool _InAbandon;
        private void DoAbandon(bool raiseExceptionEvent)
        {
            var inAbandon = false;
            BackgroundThreadQueue<ReadWriteOperation> operationQueue = null;

            using(_SpinLock.AcquireLock()) {
                inAbandon = _InAbandon;
                if(!inAbandon) {
                    _InAbandon = true;
                    operationQueue = _OperationQueue;
                    _OperationQueue = null;
                }
            }
            if(!inAbandon) {
                try {
                    try {
                        AbandonConnection();
                    } catch(Exception ex) {
                        if(_Connector != null && raiseExceptionEvent) {
                            _Connector.RaiseConnectionException(this, ex);
                        }
                    }

                    if(operationQueue != null) {
                        // This will throw a ThreadAborted exception - any code outside of a catch or finally
                        // after this point will not run.
                        operationQueue.Dispose();
                    }
                } finally {
                    _InAbandon = false;
                }
            }
        }

        /// <summary>
        /// Abandons the connection. Exceptions are allowed to bubble up out of the method.
        /// Guaranteed not to be called recursively.
        /// </summary>
        protected abstract void AbandonConnection();
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
        public virtual void Read(byte[] buffer, int offset, int length, ConnectionReadDelegate readDelegate)
        {
            var operationQueue = GetOperationQueue();
            if(operationQueue != null) {
                var operation = new ReadWriteOperation(buffer, offset, length, isRead: true, readDelegate: readDelegate);
                operationQueue.Enqueue(operation);
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
            var operationQueue = GetOperationQueue();
            var connector = GetConnector();
            if(operationQueue != null && connector != null) {
                var staleTimeout = staleMessageTimeoutOverride != -1 ? staleMessageTimeoutOverride : connector.StaleMessageTimeout;
                var staleThreshold = staleTimeout > 0 ? DateTime.UtcNow.AddMilliseconds(staleTimeout) : default(DateTime);

                IncrementWriteQueueBytes(length);
                var operation = new ReadWriteOperation(buffer, offset, length, isRead: false, staleThreshold: staleThreshold);
                operationQueue.Enqueue(operation);
            }
        }
        #endregion

        #region Operation Queue
        /// <summary>
        /// Called when a read or write operation is received.
        /// </summary>
        /// <param name="operation"></param>
        private void OperationQueue_ProcessOperation(ReadWriteOperation operation)
        {
            if(operation.IsRead) {
                DoRead(operation);
                if(!operation.Abandon) {
                    IncrementBytesRead(operation.BytesRead);
                    if(operation.ReadDelegate != null) {
                        operation.ReadDelegate(this, operation.Buffer, operation.Offset, operation.Length, operation.BytesRead);
                    }
                }
            } else {
                IncrementWriteQueueBytes(-operation.Length);
                if(DateTime.UtcNow >= operation.StaleThreshold) {
                    IncrementStaleBytesDiscarded(operation.Length);
                } else {
                    DoWrite(operation);
                    if(!operation.Abandon) {
                        IncrementBytesWritten(operation.Length);
                    }
                }
            }

            if(operation.Abandon) {
                // The Abandon call will shut down this thread - do not put any code after this point,
                // it will never run.
                Abandon();
            }
        }

        protected abstract void DoRead(ReadWriteOperation operation);
        protected abstract void DoWrite(ReadWriteOperation operation);

        /// <summary>
        /// Called when an exception is thrown while processing an operation.
        /// </summary>
        /// <param name="ex"></param>
        private void OperationQueue_ProcessException(Exception ex)
        {
            if(_Connector != null) _Connector.RaiseConnectionException(this, ex);
        }

        /// <summary>
        /// Stops the operation queue. Once this has been called no further read or write
        /// calls will work, they will all silently fail.
        /// </summary>
        protected void ShutdownOperationQueue()
        {
            _SpinLock.Lock();
            try {
                if(_OperationQueue != null) {
                    _OperationQueue.Stop();
                }
            } finally {
                _SpinLock.Unlock();
            }
        }
        #endregion
    }
}
