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

namespace VirtualRadar.Interface.Listener
{
    /// <summary>
    /// The interface for objects that can extract raw message bytes from an array of bytes.
    /// </summary>
    public interface IMessageBytesExtractor
    {
        /// <summary>
        /// Gets the number of bytes that the message extractor is currently consuming in buffers.
        /// </summary>
        long BufferSize { get; }

        /// <summary>
        /// Returns a collection of byte arrays, each byte array corresponding to a complete message.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <remarks><para>
        /// Extractors need to be miserly with the resources that they consume and so implementations are allowed
        /// to reuse the <see cref="ExtractedBytes"/> that they return and the byte array within the 
        /// <see cref="ExtractedBytes"/>. If a caller wants to ensure that each element returned by the enumerator
        /// is independent of the others then it should clone each of them as they are returned - e.g.
        /// </para><code>
        /// var list = extractor.ExtractMessageBytes(bytes, 0, bytes.Length).Select(r => (ExtractedBytes)r.Clone()).ToList();
        /// </code></remarks>
        IEnumerable<ExtractedBytes> ExtractMessageBytes(byte[] bytes, int offset, int length);
    }
}
