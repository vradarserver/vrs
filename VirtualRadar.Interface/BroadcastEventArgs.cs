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
using System.Linq;
using System.Text;
using System.Net;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The event args carrying information about bytes transmitted across the network.
    /// </summary>
    public class BroadcastEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the ID of the rebroadcast server associated with the event.
        /// </summary>
        public int RebroadcastServerId { get; private set; }

        /// <summary>
        /// Gets the address of the machine listening to the broadcast.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Gets the port that the broadcast event was sent on.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the count of bytes sent (or being sent) to the listener of the broadcast.
        /// </summary>
        public int BytesSent { get; private set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="rebroadcastServerId"></param>
        /// <param name="endPoint"></param>
        /// <param name="bytesSent"></param>
        /// <param name="port"></param>
        public BroadcastEventArgs(int rebroadcastServerId, IPEndPoint endPoint, int bytesSent, int port)
        {
            RebroadcastServerId = rebroadcastServerId;
            EndPoint = endPoint;
            BytesSent = bytesSent;
            Port = port;
        }
    }
}
