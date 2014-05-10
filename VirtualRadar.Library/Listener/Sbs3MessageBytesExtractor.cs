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
using VirtualRadar.Interface;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="ISbs3MessageBytesExtractor"/>.
    /// </summary>
    class Sbs3MessageBytesExtractor : ISbs3MessageBytesExtractor
    {
        /// <summary>
        /// The object that can calculate CRC16-CCITT checksums for us.
        /// </summary>
        private Crc16Ccitt _ChecksumCalculator = new Crc16Ccitt(Crc16Ccitt.InitialiseToZero);

        /// <summary>
        /// The buffer of unprocessed bytes from the last read.
        /// </summary>
        private byte[] _ReadBuffer;

        /// <summary>
        /// The number of unprocessed bytes within the read buffer.
        /// </summary>
        private int _ReadBufferLength;

        /// <summary>
        /// The buffer that we use (and reuse) when extracting the unstuffed packet content from the stream.
        /// </summary>
        private byte[] _Payload;

        /// <summary>
        /// The buffer we use and reuse when extracting the unstuffed checksum from the stream.
        /// </summary>
        private byte[] _Checksum = new byte[2];

        /// <summary>
        /// Set to true after the first valid packet has been seen, even if it could / would not be decoded.
        /// </summary>
        private bool _SeenFirstPacket = false;

        /// <summary>
        /// The object that is returned on each iteration of the enumerator.
        /// </summary>
        private ExtractedBytes _ExtractedBytes = new ExtractedBytes() { Format = ExtractedBytesFormat.ModeS };

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
            if(_ReadBuffer == null || length > _ReadBuffer.Length) {
                var newReadBuffer = new byte[length];
                if(_ReadBuffer != null) _ReadBuffer.CopyTo(newReadBuffer, 0);
                _ReadBuffer = newReadBuffer;
            }
            Array.ConstrainedCopy(bytes, 0, _ReadBuffer, _ReadBufferLength, bytesRead);
            _ReadBufferLength = length;

            int startOfPacket = FindDCEMarker(0, 0x02);
            if(startOfPacket == 0 && !_SeenFirstPacket) startOfPacket = FindDCEMarker(1, 0x02);

            int firstByteAfterLastValidPacket = -1;
            while(startOfPacket != -1) {
                var endOfPacket = FindDCEMarker(startOfPacket + 2, 0x03);
                if(endOfPacket == -1) break;

                int lengthOfChecksum;
                var packetChecksum = ReadChecksum(endOfPacket, out lengthOfChecksum);
                if(packetChecksum == null) break;

                _SeenFirstPacket = true;
                firstByteAfterLastValidPacket = endOfPacket + 2 + lengthOfChecksum;

                var transponderDataLength = 0;
                switch(_ReadBuffer[startOfPacket + 2]) {
                    case 0x01:
                    case 0x05:  transponderDataLength = 14; break;
                    case 0x07:  transponderDataLength = 7; break;
                }

                if(transponderDataLength != 0) {
                    var dataLength = (endOfPacket - startOfPacket) - 2;
                    if(_Payload == null || _Payload.Length < dataLength) _Payload = new byte[dataLength];

                    int payloadIndex = 0;
                    for(int i = startOfPacket + 2;i < endOfPacket;++i) {
                        var b = _ReadBuffer[i];
                        _Payload[payloadIndex++] = b != 0x10 ? b : _ReadBuffer[++i];
                    }
                    if(payloadIndex >= transponderDataLength + 5) {  // first 5 bytes are packet type / unused / 3-byte rolling timestamp - latter 4 can be DCE stuffed so we can't skip them earlier than this
                        var calculatedChecksum = _ChecksumCalculator.ComputeChecksumBytes(_Payload, 0, payloadIndex, false);

                        _ExtractedBytes.Bytes = _Payload;
                        if(calculatedChecksum[0] != packetChecksum[0] || calculatedChecksum[1] != packetChecksum[1]) {
                            _ExtractedBytes.ChecksumFailed = true;
                            _ExtractedBytes.Length = _Payload.Length;
                        } else {
                            _ExtractedBytes.ChecksumFailed = false;
                            _ExtractedBytes.Offset = 5;
                            _ExtractedBytes.Length = transponderDataLength;
                        }

                        yield return _ExtractedBytes;
                    }
                }

                startOfPacket = FindDCEMarker(firstByteAfterLastValidPacket, 0x02);
            }

            if(firstByteAfterLastValidPacket != -1) {
                var unusedBytesCount = _ReadBufferLength - firstByteAfterLastValidPacket;
                if(unusedBytesCount > 0) {
                    if(unusedBytesCount > 0x2800) {
                        // We don't want the read buffer growing out of control when reading a source that doesn't contain
                        // anything that looks like valid messages. I'm not sure how large some of the other packet types
                        // from SBS-3s are going to be so for now I'm allowing a very generous buffer size.
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
        /// Returns the index of the next occurence of an 0x10 0x?? marker in the read buffer.
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="marker"></param>
        /// <returns></returns>
        private int FindDCEMarker(int startIndex, byte marker)
        {
            int result = -1;

            var lastIndex = _ReadBufferLength - 1;
            for(int i = startIndex;i < lastIndex;++i) {
                var b = _ReadBuffer[i];
                if(b == 0x10) {
                    var b1 = _ReadBuffer[i + 1];
                    if(b1 == 0x10) ++i;
                    else if(b1 == marker) {
                        result = i;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Extracts the checksum from the read buffer.
        /// </summary>
        /// <param name="startOfMarker"></param>
        /// <param name="lengthOfChecksum"></param>
        /// <returns></returns>
        private byte[] ReadChecksum(int startOfMarker, out int lengthOfChecksum)
        {
            lengthOfChecksum = 0;

            int index = 0;
            var startOfChecksum = startOfMarker + 2;
            for(int i = startOfChecksum;i < _ReadBufferLength;++i) {
                var b = _ReadBuffer[i];
                if(b == 0x10) {
                    if(++i >= _ReadBufferLength) break;
                    b = _ReadBuffer[i];
                }
                _Checksum[index++] = b;
                if(index == 2) {
                    lengthOfChecksum = (i - startOfChecksum) + 1;
                    break;
                }
            }

            return index == 2 ? _Checksum : null;
        }
    }
}
