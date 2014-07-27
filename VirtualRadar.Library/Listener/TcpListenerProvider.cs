// Copyright © 2012 onwards, Andrew Whewell
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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interop;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="ITcpListenerProvider"/>.
    /// </summary>
    class TcpListenerProvider : ITcpListenerProvider
    {
        #region AsyncResultException
        /// <summary>
        /// A class that can pass an exception from the BeginConnect call to the EndConnect call.
        /// </summary>
        class AsyncResultException : IAsyncResult
        {
            public Exception Exception { get; set; }
            public object AsyncState { get { throw new NotImplementedException(); } }
            public WaitHandle AsyncWaitHandle { get { throw new NotImplementedException(); } }
            public bool CompletedSynchronously { get { throw new NotImplementedException(); } }
            public bool IsCompleted { get { throw new NotImplementedException(); } }

            public AsyncResultException(Exception ex)
            {
                Exception = ex;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The socket connected to the address and port specified in the <see cref="BeginConnect"/> call.
        /// </summary>
        private Socket _Socket;

        /// <summary>
        /// True if the application is running under Mono. Some socket calls aren't available under Mono.
        /// </summary>
        private bool _IsMono;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public byte[] ReadBuffer { get; private set; }
        #endregion

        #region Ctors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public TcpListenerProvider()
        {
            ReadBuffer = new byte[2048];
            _IsMono = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono;
        }
        #endregion

        #region BeginConnect, EndConnect, Close, BeginRead, EndRead, Sleep
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IAsyncResult BeginConnect(AsyncCallback callback)
        {
            Close();

            Exception resolveException = null;
            IPAddress[] ipAddresses = null;
            var retryCounter = 6;
            while(ipAddresses == null && retryCounter >= 0) {
                try {
                    ipAddresses = Dns.GetHostAddresses(Address);
                    retryCounter = -1;
                } catch(Exception ex) {
                    ipAddresses = null;
                    if(retryCounter-- > 0) Thread.Sleep(500);
                    else {
                        resolveException = ex;
                        break;
                    }
                }
            }

            // If we can't resolve the adddress then what we'd ideally do is call the callback method and force that
            // to throw a socket exception, all from a background thread, so that we can fit in with what the listener
            // is expecting to see and cause the listener to retry when appropriate.
            IAsyncResult result = null;
            if(resolveException != null) {
                var dummyAsyncResult = new AsyncResultException(resolveException);
                ThreadPool.QueueUserWorkItem(state => {
                    try {
                        callback((IAsyncResult)state);
                    } catch {
                        // Can't let this bubble up, it would stop the runtime, and we have no mechanism for
                        // showing it to the user. It's unlikely to ever get this far though.
                    }
                }, dummyAsyncResult);
            } else {

                if(ipAddresses == null || ipAddresses.Length == 0) throw new InvalidOperationException(String.Format("Cannot find an IP address for {0}", Address));
                var ipAddress = ipAddresses.First();

                var endPoint = new IPEndPoint(ipAddress, Port);
                _Socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                if(!_IsMono) _Socket.IOControl(IOControlCode.KeepAliveValues, new TcpKeepAlive() { OnOff = 1, KeepAliveTime = 10 * 1000, KeepAliveInterval = 1 * 1000 }.BuildBuffer(), null);

                result = _Socket.BeginConnect(endPoint, callback, _Socket);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public bool EndConnect(IAsyncResult asyncResult)
        {
            var rethrowResult = asyncResult as AsyncResultException;
            if(rethrowResult != null) throw rethrowResult.Exception;

            var socket = (Socket)asyncResult.AsyncState;
            socket.EndConnect(asyncResult);

            return socket == _Socket;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Close()
        {
            if(_Socket != null) {
                try {
                    ((IDisposable)_Socket).Dispose();
                } catch(Exception ex) {
                    Debug.WriteLine(String.Format("SocketProvider.Close caught exception {0}", ex.ToString()));
                }

                _Socket = null;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IAsyncResult BeginRead(AsyncCallback callback)
        {
            IAsyncResult result = null;

            if(_Socket != null) {
                result = _Socket.BeginReceive(ReadBuffer, 0, ReadBuffer.Length, SocketFlags.None, callback, _Socket);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        public int EndRead(IAsyncResult asyncResult)
        {
            int result = -1;

            var socket = (Socket)asyncResult.AsyncState;
            result = socket.EndReceive(asyncResult);
            if(socket != _Socket) result = -1;

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="milliseconds"></param>
        public void Sleep(int milliseconds)
        {
            Thread.Sleep(milliseconds);
        }
        #endregion
    }
}
