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
using System.Net;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A service that can retrieve the state of TCP connections.
    /// </summary>
    /// <remarks>
    /// Getting the TCP connection state is fairly straight-forward - however under
    /// Mono the IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections()
    /// call is currently bugged, we need to handle it differently when running under
    /// Mono. This interface wraps the call and copes with the bugged call.
    /// </remarks>
    public interface ITcpConnectionStateService
    {
        /// <summary>
        /// Gets the count of connections that the service knows about.
        /// </summary>
        int CountConnections { get; }

        /// <summary>
        /// Reloads the cache of connection states that was established when the object
        /// was created. All other methods work off this cache, they do not perform live
        /// lookups.
        /// </summary>
        void RefreshTcpConnectionStates();

        /// <summary>
        /// Returns true if the connection to the remote end-point was in the ESTABLISHED
        /// state when the object was constructed.
        /// </summary>
        /// <param name="remoteEndPoint">The remote endpoint to check.</param>
        /// <returns>
        /// True if the connection is established, false if it does not exist or is not in the
        /// established state.
        /// </returns>
        bool IsRemoteConnectionEstablished(IPEndPoint remoteEndPoint);

        /// <summary>
        /// Returns a description of the state of a remote end-point.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <returns></returns>
        string DescribeRemoteConnectionState(IPEndPoint remoteEndPoint);
    }
}
