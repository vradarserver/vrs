// Copyright © 2016 onwards, Andrew Whewell
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
    /// The default implementation of <see cref="IPlaneFinderMessageBytesExtractor"/>.
    /// </summary>
    /// <remarks><para>
    /// The PlaneFinder scheme is similar to Kinetic's raw format. The data is sent in packets.
    /// The packet format is (values are hex):
    /// </para><code>
    /// Byte  Len  Value  Notes
    /// ----  ---  -----  -----
    /// 0     1    10     Start of packet
    /// 1     1    ??     Packet type (will never be 0x10 or 0x03)
    /// 2     N    ??     Packet body, up to 255 bytes before DLE stuffing
    /// 2+N   1    10     Footer
    /// 3+N   1    03     Footer
    /// </code><para>
    /// If 0x10 appears in the packet body then it is escaped with a second 0x10.
    /// </para><para>
    /// Aircraft messages are sent in the 0xC1 packet. The body of 0xC1 packets look like this:
    /// </para><code>
    /// Byte  Len     Value  Notes
    /// ----  ---     -----  -----
    /// 0     1       C1     Message packet ID
    /// 1     1       ??     Unused
    /// 2     1       ??     Bitflags, message type
    /// 3     1       ??     Signal strength
    /// 4     4       ??     Timestamp, seconds
    /// 8     4       ??     Timestamp, nanoseconds
    /// 12    2/7/14  ??     Message bytes
    /// </code><para>
    /// The message type bitflags are:
    /// </para><code>
    /// Bit  Description
    /// ---  -----------
    /// 7    -
    /// 6    -
    /// 5    Parity wrong, message has been error corrected
    /// 4    Parity was correct
    /// 3-0  Message type: 0 = Mode-A 2 byte, 1 = Mode-S 7 byte, 2 = Mode-S 14 bytes
    /// </code></remarks>
    class PlaneFinderMessageBytesExtractor : IPlaneFinderMessageBytesExtractor
    {
        /// <summary>
        /// The number of bytes that preceeds the data in a destuffed packet.
        /// </summary>
        /// <remarks>
        /// 1 byte padding + 1 byte packet type + 1 byte signal strength + 4 bytes timestamp + 4 bytes timestamp no.2.
        /// </remarks>
        private const int _DestuffedPacketPreambleLength = 11;

        /// <summary>
        /// The largest we allow the read buffer to grow to. The documentation states a maximum packet size of 512 bytes after
        /// stuffing, we want to allow for two of those (0x400) plus a bit of overhead.
        /// </summary>
        private const int _MaximumReadBufferSize = 0x500;

        /// <summary>
        /// The buffer of unprocessed bytes from the last read.
        /// </summary>
        private byte[] _ReadBuffer;

        /// <summary>
        /// The number of unprocessed bytes within the read buffer.
        /// </summary>
        private int _ReadBufferLength;

        /// <summary>
        /// The packet with 0x10 stuffing taken out.
        /// </summary>
        private byte[] _DestuffedPacket = new byte[50];

        /// <summary>
        /// Set to true after the first valid packet has been seen, even if it could / would not be decoded.
        /// </summary>
        private bool _SeenFirstPacket = false;

        /// <summary>
        /// The object that is returned on each iteration of the enumerator.
        /// </summary>
        private ExtractedBytes _ExtractedBytes = new ExtractedBytes() {
            Format = ExtractedBytesFormat.ModeS,
            HasParity = true,
            ChecksumFailed = false,
            Offset = _DestuffedPacketPreambleLength,
        };

        /// <summary>
        /// See interface docs.
        /// </summary>
        public long BufferSize
        {
            get {
                var result = _ReadBuffer == null ? 0 : _ReadBuffer.Length;
                result += _DestuffedPacket.Length;
                return result;
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public PlaneFinderMessageBytesExtractor()
        {
            _ExtractedBytes.Bytes = _DestuffedPacket;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="bytesRead"></param>
        /// <returns></returns>
        public IEnumerable<ExtractedBytes> ExtractMessageBytes(byte[] bytes, int offset, int bytesRead)
        {
            var length = _ReadBufferLength + bytesRead;
            if(length > _MaximumReadBufferSize) {
                _ReadBufferLength = 0;
                length = bytesRead;
            }

            if(_ReadBuffer == null || length > _ReadBuffer.Length) {
                var newReadBuffer = new byte[length];
                if(_ReadBuffer != null) _ReadBuffer.CopyTo(newReadBuffer, 0);
                _ReadBuffer = newReadBuffer;
            }
            Array.ConstrainedCopy(bytes, 0, _ReadBuffer, _ReadBufferLength, bytesRead);
            _ReadBufferLength = length;

            int startOfPacket = -1;
            if(_SeenFirstPacket) {
                startOfPacket = FindDCEMarker(0, findEndMarker: false);
            } else {
                var endOfPacket = FindDCEMarker(0, findEndMarker: true);
                if(endOfPacket != -1) {
                    startOfPacket = FindDCEMarker(endOfPacket + 2, findEndMarker: false);
                }
            }

            const int smallestDestuffedPacket = _DestuffedPacketPreambleLength + 2;

            int firstByteAfterLastValidPacket = -1;
            while(startOfPacket != -1) {
                var endOfPacket = FindDCEMarker(startOfPacket + 2, findEndMarker: true);
                if(endOfPacket == -1) break;

                _SeenFirstPacket = true;
                firstByteAfterLastValidPacket = endOfPacket + 2;

                if(_ReadBuffer[startOfPacket + 1] == 0xC1) {
                    var destuffedPacketLength = DestuffPacket(startOfPacket, endOfPacket);
                    if(destuffedPacketLength >= smallestDestuffedPacket) {
                        var transponderDataLength = 0;
                        switch(_DestuffedPacket[1] & 0x0F) {
                            case 2: transponderDataLength = 14; break;
                            case 1: transponderDataLength = 7; break;
                            case 0: transponderDataLength = 0; break;       // actually 2 but we don't decode A/C
                        }

                        if(transponderDataLength > 0 && destuffedPacketLength == transponderDataLength + _DestuffedPacketPreambleLength) {
                            _ExtractedBytes.SignalLevel = _DestuffedPacket[2];
                            _ExtractedBytes.Length = transponderDataLength;
                            yield return _ExtractedBytes;
                        }
                    }
                }

                startOfPacket = FindDCEMarker(endOfPacket + 2, findEndMarker: false);
            }

            if(firstByteAfterLastValidPacket != -1) {
                var unusedBytesCount = _ReadBufferLength - firstByteAfterLastValidPacket;
                if(unusedBytesCount > 0) {
                    if(unusedBytesCount > _MaximumReadBufferSize) {
                        unusedBytesCount = 0;
                    } else {
                        for(int si = firstByteAfterLastValidPacket, di = 0;di < unusedBytesCount;++si, ++di) {
                            _ReadBuffer[di] = _ReadBuffer[si];
                        }
                    }
                }
                _ReadBufferLength = unusedBytesCount;
            }
        }

        /// <summary>
        /// Returns the index of the next occurence of an 0x10 marker in the read buffer.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="findEndMarker"></param>
        /// <returns></returns>
        private int FindDCEMarker(int startIndex, bool findEndMarker)
        {
            int result = -1;

            var lastIndex = _ReadBufferLength - 1;
            for(int i = startIndex;i < lastIndex;++i) {
                var b = _ReadBuffer[i];
                if(b == 0x10) {
                    var b1 = _ReadBuffer[i + 1];
                    if(b1 == 0x10) {
                         ++i;
                    } else if(findEndMarker) {
                        if(b1 == 0x03) {
                            result = i;
                            break;
                        }
                    } else {
                        if(b1 != 0x03) {
                            result = i;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Copies the bytes from the read buffer to the destuffed packet buffer, removing stuffing
        /// bytes as it goes. Returns the length of the destuffed packet. If the destuffed packet
        /// overflows the buffer then -1 is returned.
        /// </summary>
        /// <param name="startOfPacket"></param>
        /// <param name="endOfPacket"></param>
        /// <returns></returns>
        private int DestuffPacket(int startOfPacket, int endOfPacket)
        {
            var result = 0;

            var readIdx = startOfPacket + 2;
            var destuffedPacketLength = _DestuffedPacket.Length;
            for(;readIdx < endOfPacket && result < destuffedPacketLength;++readIdx, ++result) {
                var b = _ReadBuffer[readIdx];
                if(b == 0x10) {
                    ++readIdx;
                    if(readIdx == endOfPacket) {
                        break;
                    }
                    b = _ReadBuffer[readIdx];
                }

                _DestuffedPacket[result] = b;
            }

            if(readIdx != endOfPacket) {
                result = -1;
            }

            return result;
        }
    }
}
