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
using VirtualRadar.Interface.Listener;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="IPort30003MessageBytesExtractor"/>.
    /// </summary>
    class Port30003MessageBytesExtractor : IPort30003MessageBytesExtractor
    {
        /// <summary>
        /// The array of bytes that carries the payload from a network packet.
        /// </summary>
        byte[] _Payload;

        /// <summary>
        /// The current length of the payload in <see cref="_Payload"/>.
        /// </summary>
        int _PayloadLength;

        /// <summary>
        /// The object that is repeatedly returned from <see cref="ExtractMessageBytes"/>.
        /// </summary>
        ExtractedBytes _ExtractedBytes = new ExtractedBytes() { Format = ExtractedBytesFormat.Port30003 };

        /// <summary>
        /// True once the first line-end character has been seen.
        /// </summary>
        bool _SeenValidPacket;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public IEnumerable<ExtractedBytes> ExtractMessageBytes(byte[] bytes, int offset, int length)
        {
            if(_Payload == null || _PayloadLength + length > _Payload.Length) {
                var payload = new byte[_PayloadLength + length];
                if(_Payload != null) Array.ConstrainedCopy(_Payload, 0, payload, 0, _PayloadLength);
                _Payload = payload;
                _ExtractedBytes.Bytes = _Payload;
            }

            var endOfSource = offset + length;
            for(int si = offset, di = _PayloadLength;si < endOfSource;++si) {
                var ch = bytes[si];
                if(ch != '\r' && ch != '\n') _Payload[_PayloadLength++] = ch;
                else {
                    if(!_SeenValidPacket) _SeenValidPacket = true;
                    else if(_PayloadLength != 0) {
                        _ExtractedBytes.Length = _PayloadLength;
                        yield return _ExtractedBytes;
                    }
                    _PayloadLength = 0;
                }
            }

            // Make sure that message streams that don't contain line-ends don't cause the buffer to grow
            // out of control
            if(_PayloadLength > 1024) _PayloadLength = 0;
        }
    }
}
