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
using VirtualRadar.Interface.Network;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// A class that describes a read or write operation to perform on a connection.
    /// </summary>
    class ReadWriteOperation
    {
        /// <summary>
        /// Gets a value indicating whether this is a read or a write operation.
        /// </summary>
        public bool IsRead { get; private set; }

        /// <summary>
        /// Gets the buffer to use.
        /// </summary>
        public byte[] Buffer { get; private set; }

        /// <summary>
        /// Gets the offset from the start of the buffer.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Gets the number of bytes from the offset that can be used.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the delegate to call when reading bytes.
        /// </summary>
        public ConnectionReadDelegate ReadDelegate { get; private set; }

        /// <summary>
        /// Gets the date and time at UTC when the messsage becomes too old to send.
        /// </summary>
        public DateTime StaleThreshold { get; private set; }

        /// <summary>
        /// Gets or sets the number of bytes that were read into the buffer.
        /// </summary>
        public int BytesRead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the connection has gone into the error state and needs to be abandoned.
        /// </summary>
        public bool Abandon { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="isRead"></param>
        /// <param name="readDelegate"></param>
        /// <param name="staleThreshold"></param>
        public ReadWriteOperation(byte[] buffer, int offset, int length, bool isRead, ConnectionReadDelegate readDelegate = null, DateTime staleThreshold = default(DateTime))
        {
            Buffer = buffer;
            Offset = offset;
            Length = length;
            IsRead = isRead;
            ReadDelegate = readDelegate;
            StaleThreshold = staleThreshold == default(DateTime) ? DateTime.MaxValue : staleThreshold;
        }
    }
}
