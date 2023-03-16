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
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace VirtualRadar.Interface.Feeds
{
    /// <summary>
    /// Describes a connection to a rebroadcast server.
    /// </summary>
    public class RebroadcastServerConnection : ICloneable
    {
        /// <summary>
        /// Gets or sets the identifier of the rebroadcast server.
        /// </summary>
        public int RebroadcastServerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the rebroadcast server.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the local port that the rebroadcast server listens to.
        /// </summary>
        public int LocalPort { get; set; }

        /// <summary>
        /// Gets or sets the IP address that the client connected from.
        /// </summary>
        public IPAddress EndpointIPAddress { get; set; }

        /// <summary>
        /// Gets the address held by <see cref="EndpointIPAddress"/>.
        /// </summary>
        public string RemoteAddress => EndpointIPAddress == null ? null : EndpointIPAddress.ToString();

        /// <summary>
        /// Gets or sets the port that the client connected from.
        /// </summary>
        public int EndpointPort { get; set; }

        /// <summary>
        /// Gets or sets a count of bytes currently buffered and awaiting transmission.
        /// </summary>
        public long BytesBuffered { get; set; }

        /// <summary>
        /// Gets or sets a count of bytes sent to the client.
        /// </summary>
        public long BytesWritten { get; set; }

        /// <summary>
        /// Gets or sets a count of bytes that were discarded from the buffer because they took
        /// too long to send.
        /// </summary>
        public long StaleBytesDiscarded { get; set; }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{RebroadcastServerId}: {Name}";

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var result = Activator.CreateInstance(GetType()) as RebroadcastServerConnection;
            result.BytesBuffered = BytesBuffered;
            result.BytesWritten = BytesWritten;
            result.EndpointIPAddress = EndpointIPAddress;
            result.EndpointPort = EndpointPort;
            result.LocalPort = LocalPort;
            result.Name = Name;
            result.RebroadcastServerId = RebroadcastServerId;
            result.StaleBytesDiscarded = StaleBytesDiscarded;

            return result;
        }

        /// <summary>
        /// Returns true if this connection is the same as the other connection.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSameConnection(RebroadcastServerConnection other)
        {
            var result = Object.ReferenceEquals(this, other);
            if(!result && other != null) {
                result = RebroadcastServerId == other.RebroadcastServerId &&
                         LocalPort == other.LocalPort &&
                         EndpointPort == other.EndpointPort &&
                         IPAddressComparer.Singleton.Compare(EndpointIPAddress, other.EndpointIPAddress) == 0;
            }

            return result;
        }
    }
}
