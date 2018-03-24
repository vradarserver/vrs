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
using System.Text;
using System.Threading.Tasks;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// Describes a byte array that represents a buffer that needs to be copied in chunks into
    /// a smaller buffer over repeated calls.
    /// </summary>
    class ByteBuffer
    {
        private byte[] _Buffer;

        private int _Offset;

        private int _Length;

        private int _OffsetNextRead;

        /// <summary>
        /// Gets the number of bytes left to copy.
        /// </summary>
        public int LengthRemaining { get => _Length - (_OffsetNextRead - _Offset); }

        /// <summary>
        /// Records the buffer's content for subsequent copies via <see cref="CopyChunkIntoBuffer"/>.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SetBuffer(byte[] buffer, int offset, int length)
        {
            _Buffer = buffer;
            _Offset = offset;
            _Length = length;

            _OffsetNextRead = _Offset;
        }

        /// <summary>
        /// Copies a chunk of the buffer recorded with <see cref="SetBuffer"/> into the buffer passed across.
        /// Automatically adjusts <paramref name="length"/> so that it cannot exceed <see cref="LengthRemaining"/>.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns>The number of bytes actually copied into the buffer.</returns>
        public int CopyChunkIntoBuffer(byte[] buffer, int offset, int length)
        {
            length = Math.Min(length, LengthRemaining);
            Array.ConstrainedCopy(_Buffer, _OffsetNextRead, buffer, offset, length);
            _OffsetNextRead += length;

            return length;
        }
    }
}
