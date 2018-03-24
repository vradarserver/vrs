// Copyright © 2018 onwards, Andrew Whewell
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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// Describes a connection to an HTTP endpoint.
    /// </summary>
    class HttpConnection : Connection
    {
        /// <summary>
        /// The HTTP client that is shared between all connections.
        /// </summary>
        private static HttpClient _HttpClient = new HttpClient();

        /// <summary>
        /// The date and time at UTC of the last fetch on the connection.
        /// </summary>
        private DateTime _LastReadTimeUtc;

        /// <summary>
        /// The uncopied chunk from a previous read.
        /// </summary>
        private ByteBuffer _PreviousReadResult = new ByteBuffer();

        /// <summary>
        /// Gets the web address to fetch from.
        /// </summary>
        public string WebAddress { get; }

        /// <summary>
        /// Gets the millisecond interval between each fetch.
        /// </summary>
        public int FetchIntervalMilliseconds { get; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="initialStatus"></param>
        /// <param name="webAddress"></param>
        /// <param name="fetchIntervalMilliseconds"></param>
        public HttpConnection(HttpConnector connector, ConnectionStatus initialStatus, string webAddress, int fetchIntervalMilliseconds) : base(connector, initialStatus)
        {
            WebAddress = webAddress;
            FetchIntervalMilliseconds = fetchIntervalMilliseconds;
        }

        /// <summary>
        /// Abandons the connection.
        /// </summary>
        protected override void AbandonConnection()
        {
            ConnectionStatus = ConnectionStatus.Disconnected;
        }

        /// <summary>
        /// Reads from the connection.
        /// </summary>
        /// <param name="operation"></param>
        protected override void DoRead(ReadWriteOperation operation)
        {
            try {
                int bytesRead = 0;

                if(_PreviousReadResult.LengthRemaining > 0) {
                    bytesRead = _PreviousReadResult.CopyChunkIntoBuffer(operation.Buffer, operation.Offset, operation.Length);
                } else {
                    var threshold = _LastReadTimeUtc.AddMilliseconds(FetchIntervalMilliseconds);
                    while(DateTime.UtcNow < threshold) {
                        // Give up our timeslice
                        Thread.Sleep(1);
                    }
                    _LastReadTimeUtc = DateTime.UtcNow;         // Set a minimum threshold for the next read even if this upcoming read fails

                    var bytes = _HttpClient.GetByteArrayAsync(WebAddress).Result;
                    _PreviousReadResult.SetBuffer(bytes, 0, bytes.Length);
                    bytesRead = _PreviousReadResult.CopyChunkIntoBuffer(operation.Buffer, operation.Offset, operation.Length);
                    ConnectionStatus = ConnectionStatus.Connected;

                    _LastReadTimeUtc = DateTime.UtcNow;         // Set the point that we want to pause from for a successful read
                }

                operation.BytesRead = bytesRead;
            } catch(Exception ex) {
                _Connector?.RaiseConnectionException(this, ex);
                ConnectionStatus = ConnectionStatus.Reconnecting;
            }
        }

        /// <summary>
        /// Does nothing - writes over HTTP connections are not supported.
        /// </summary>
        /// <param name="operation"></param>
        protected override void DoWrite(ReadWriteOperation operation)
        {
            ;
        }
    }
}
