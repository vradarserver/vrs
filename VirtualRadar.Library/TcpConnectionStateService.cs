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
using System.Net;
using System.Net.NetworkInformation;
using InterfaceFactory;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="ITcpConnectionStateService"/>.
    /// </summary>
    class TcpConnectionStateService : ITcpConnectionStateService
    {
        /// <summary>
        /// The collection of TCP connections as-at the time the object was constructed.
        /// </summary>
        private TcpConnectionInformation[] _TcpConnections;

        /// <summary>
        /// True if running under Mono, false if running under .NET.
        /// </summary>
        private bool _IsMono;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int CountConnections
        {
            get { return _TcpConnections.Length; }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public TcpConnectionStateService()
        {
            _IsMono = Factory.Singleton.Resolve<IRuntimeEnvironment>().Singleton.IsMono;
            RefreshTcpConnectionStates();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void RefreshTcpConnectionStates()
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            _TcpConnections = properties == null ? null : properties.GetActiveTcpConnections();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <returns></returns>
        public bool IsRemoteConnectionEstablished(IPEndPoint remoteEndPoint)
        {
            var connection = GetRemoteConnection(remoteEndPoint);
            return connection == null ? false : !_IsMono ? connection.State == TcpState.Established : (int)connection.State == 1;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <returns></returns>
        public string DescribeRemoteConnectionState(IPEndPoint remoteEndPoint)
        {
            var connection = GetRemoteConnection(remoteEndPoint);
            return connection == null ? "missing" : !_IsMono ? connection.State.ToString() : ((int)connection.State).ToString();
        }

        /// <summary>
        /// Returns the TcpConnectionInformation for a remote endpoint, or null if one
        /// could not be found.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <returns></returns>
        private TcpConnectionInformation GetRemoteConnection(IPEndPoint remoteEndPoint)
        {
            TcpConnectionInformation result = null;

            if(_TcpConnections != null && _TcpConnections.Length > 0) {
                var endPointText = remoteEndPoint.ToString();
                result = _TcpConnections.FirstOrDefault(r => r.RemoteEndPoint != null && r.RemoteEndPoint.ToString() == endPointText);
            }

            return result;
        }
    }
}
