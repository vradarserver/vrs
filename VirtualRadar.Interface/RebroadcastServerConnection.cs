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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Describes a connection to a rebroadcast server.
    /// </summary>
    public class RebroadcastServerConnection
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
        /// Gets or sets the address that the client connected from.
        /// </summary>
        public string EndpointAddress { get; set; }

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
    }
}
