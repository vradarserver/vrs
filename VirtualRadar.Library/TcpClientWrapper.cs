// Copyright © 2013 onwards, Andrew Whewell
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
using System.Net.Sockets;
using System.Threading;
using VirtualRadar.Interface;
using InterfaceFactory;

namespace VirtualRadar.Library
{
    /// <summary>
    /// A class that wraps TcpClients to support timeouts on asynchronous operations.
    /// </summary>
    class TcpClientWrapper : IDisposable
    {
        #region Private class - Operation
        /// <summary>
        /// A class that represents an operation that needs to be performed in background on the stream.
        /// </summary>
        /// <remarks>
        /// To make life a bit easier this is intended to be used as a part of a drop-in replacement for Begin/End asynchronous
        /// operations. However it does not implement the full set of IAsyncResult properties, specifically those concerned with
        /// polling and blocking.
        /// </remarks>
        class Operation : IAsyncResult
        {
            public DateTime Created;
            public int StaleMilliseconds;
            public byte[] Buffer;
            public int Offset;
            public int Size;
            public object State;
            public AsyncCallback Callback;
            public bool IsWrite;
            public Exception Exception;
            public bool Processed;
            public int Result;

            public object AsyncState { get { return State; } }
            public WaitHandle AsyncWaitHandle { get { throw new NotImplementedException(); } }
            public bool CompletedSynchronously { get { throw new NotImplementedException(); } }
            public bool IsCompleted { get { return Processed; } }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The client we're wrapping.
        /// </summary>
        private TcpClient _TcpClient;

        /// <summary>
        /// True if we own the client and are responsible for disposing of it.
        /// </summary>
        public bool _WeOwnTcpClient;

        /// <summary>
        /// The queue of operations that we need to process.
        /// </summary>
        private BackgroundThreadQueue<Operation> _Operations;

        /// <summary>
        /// A static counter of all wrappers ever created.
        /// </summary>
        private static int _CountAllThreads;

        /// <summary>
        /// The object that locks access to _GlobalException.
        /// </summary>
        private object _GlobalExceptionLock = new object();

        /// <summary>
        /// The object that locks access to the properties.
        /// </summary>
        private SpinLock _PropertiesLock = new SpinLock();

        /// <summary>
        /// The exception encountered by the BackgroundThreadQueue.
        /// </summary>
        private Exception _GlobalException;

        /// <summary>
        /// True if Dispose should ignore an attempt to shutdown the background thread.
        /// </summary>
        private bool _DeferThreadShutdown;

        /// <summary>
        /// True if a callback function to a End method called dispose while on the background thread.
        /// </summary>
        private bool _DeferredDispose;

        /// <summary>
        /// The object that provides us with timings.
        /// </summary>
        private IClock _Clock;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the TcpClient wrapped by the object. Note that access to this is not thread-safe. Do not call any read or
        /// write methods on this.
        /// </summary>
        public TcpClient TcpClient { get { return _TcpClient; } }

        /// <summary>
        /// Gets the number of bytes buffered for writing to the client.
        /// </summary>
        public long WriteBufferSize { get; private set; }

        /// <summary>
        /// Gets the number of bytes read from the client.
        /// </summary>
        public long BytesRead { get; private set; }

        /// <summary>
        /// Gets the number of bytes sent to the client.
        /// </summary>
        public long BytesWritten { get; private set; }

        /// <summary>
        /// Gets the number of bytes that were discarded because too great a period of time elapsed between them being
        /// buffered for send and the client being ready to accept them.
        /// </summary>
        public long BytesStale { get; private set; }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="takeOwnership">True if the wrapper should take ownership of the client and manage its disposal.</param>
        public TcpClientWrapper(TcpClient tcpClient, bool takeOwnership)
        {
            _TcpClient = tcpClient;
            _WeOwnTcpClient = takeOwnership;
            _Clock = Factory.Singleton.Resolve<IClock>();

            unchecked { ++_CountAllThreads; }
            _Operations = new BackgroundThreadQueue<Operation>(String.Format("NetworkStreamWrapperThread{0}", _CountAllThreads));
            _Operations.StartBackgroundThread(ProcessOperation, ProcessException);
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~TcpClientWrapper()
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
                if(_WeOwnTcpClient && _TcpClient != null) {
                    try {
                        ((IDisposable)_TcpClient).Dispose();
                    } catch {
                    }
                }
                _TcpClient = null;

                if(_DeferThreadShutdown) _DeferredDispose = true;
                else {
                    if(_Operations != null) _Operations.Dispose();
                    _Operations = null;
                }
            }
        }
        #endregion

        #region BeginRead, BeginWrite, EndRead, EndWrite
        /// <summary>
        /// Begins a read operation.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            if(callback == null) throw new ArgumentNullException("callback");

            var operation = new Operation() {
                Buffer = buffer,
                Offset = offset,
                Size = size,
                Callback = callback,
                State = state,
                IsWrite = false,
            };
            _Operations.Enqueue(operation);

            return operation;
        }

        /// <summary>
        /// Begins a write operation.
        /// </summary>
        /// <param name="created"></param>
        /// <param name="staleMilliseconds"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        public IAsyncResult BeginWrite(DateTime created, int staleMilliseconds, byte[] buffer, int offset, int size, AsyncCallback callback, Object state)
        {
            if(callback == null) throw new ArgumentNullException("callback");

            UpdateProperties(() => WriteBufferSize += buffer.LongLength);

            var operation = new Operation() {
                Created = created,
                StaleMilliseconds = staleMilliseconds,
                Buffer = buffer,
                Offset = offset,
                Size = size,
                Callback = callback,
                State = state,
                IsWrite = true,
            };
            _Operations.Enqueue(operation);

            return operation;
        }

        /// <summary>
        /// Completes a background read operation.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public int EndRead(IAsyncResult asyncResult)
        {
            var operation = (Operation)asyncResult;
            if(operation.Exception != null) throw operation.Exception;

            return operation.Result;
        }

        /// <summary>
        /// Completes a background write operation.
        /// </summary>
        /// <param name="asyncResult"></param>
        public void EndWrite(IAsyncResult asyncResult)
        {
            var operation = (Operation)asyncResult;
            if(operation.Exception != null) throw operation.Exception;
        }
        #endregion

        #region ProcessOperation, ProcessException
        /// <summary>
        /// Called on a background thread to process a pending operation.
        /// </summary>
        /// <param name="operation"></param>
        private void ProcessOperation(Operation operation)
        {
            try {
                lock(_GlobalExceptionLock) {
                    if(_GlobalException != null) {
                        operation.Exception = _GlobalException;
                        _GlobalException = null;
                    }
                }

                if(operation.Exception == null && _TcpClient != null) {
                    var stream = _TcpClient.GetStream();
                    if(!operation.IsWrite) {
                        operation.Result = stream.Read(operation.Buffer, operation.Offset, operation.Size);
                        UpdateProperties(() => BytesRead += operation.Result);
                    } else {
                        bool isStale = false;
                        if(operation.Created != DateTime.MinValue) {
                            isStale = (_Clock.UtcNow - operation.Created).TotalMilliseconds > operation.StaleMilliseconds;
                        }

                        UpdateProperties(() => {
                            WriteBufferSize -= operation.Buffer.LongLength;
                            if(isStale) BytesStale += operation.Buffer.LongLength;
                        });

                        if(!isStale) {
                            stream.Write(operation.Buffer, operation.Offset, operation.Size);
                            UpdateProperties(() => BytesWritten += operation.Buffer.LongLength);
                        }
                    }
                }

                operation.Processed = true;
            } catch(Exception ex) {
                operation.Exception = ex;
            }

            var currentDeferShutdown = _DeferThreadShutdown;
            try {
                _DeferThreadShutdown = true;    // <-- prevents Dispose from stopping the thread if Callback calls our Dispose
                operation.Callback(operation);
            } catch {
            } finally {
                _DeferThreadShutdown = currentDeferShutdown;
            }

            if(_DeferredDispose) Dispose(true);
        }

        /// <summary>
        /// Calls the action passed across from within a lock on _PropertiesLock.
        /// </summary>
        /// <param name="updateAction"></param>
        private void UpdateProperties(Action updateAction)
        {
            _PropertiesLock.Lock();
            try {
                updateAction();
            } finally {
                _PropertiesLock.Unlock();
            }
        }

        /// <summary>
        /// Manages exceptions thrown on the background thread. The <see cref="ProcessOperation"/> method
        /// already deals with all exceptions so this method should never be called.
        /// </summary>
        /// <param name="ex"></param>
        /// <remarks>
        /// Although this should never be called we still need to deal with the possibility that it might
        /// be called. The BackgroundThreadQueue can potentially raise an exception if it hits a problem.
        /// If that happens then we record the exception and then queue it up to be reported on the next
        /// operation that the user attempts. If the exception in BackgroundThreadQueue leads to the
        /// situation where ProcessOperation is never called then we're kind of screwed as we don't have
        /// any other mechanism for reporting exceptions back to the caller.
        /// </remarks>
        private void ProcessException(Exception ex)
        {
            lock(_GlobalExceptionLock) _GlobalException = ex;
        }
        #endregion
    }
}
