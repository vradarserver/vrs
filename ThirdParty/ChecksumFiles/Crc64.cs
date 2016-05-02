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

namespace ChecksumFiles
{
    /// <summary>
    /// A class that can calculate CRC64 checksums on a set of arbitrary bytes.
    /// </summary>
    /// <remarks>
    /// Pretty much lifted straight from the Sanity Free coding blog, but switched
    /// around to use a 64-bit polynominal and 64-bit values instead of 32-bit.
    /// See http://sanity-free.org/12/crc32_implementation_in_csharp.html for the
    /// original 32-bit version.
    /// </remarks>
    public class Crc64
    {
        /// <summary>
        /// The polynomial used in calculating the checksum.
        /// </summary>
        private const ulong _Polynomial = 0xC96C5795D7870F42;

        /// <summary>
        /// The table of pre-calculated checksum values for all 256 values of a byte.
        /// </summary>
        public ulong[] _LookupTable = new ulong[256];

        /// <summary>
        /// Calculates the checksum on the bytes passed across.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public ulong ComputeChecksum(byte[] bytes, int offset, int length)
        {
            ulong crc = 0xFFFFFFFFFFFFFFFF;

            for(int i = offset;i < offset + length;++i) {
                var index = (byte)((crc & 0xff) ^ bytes[i]);
                crc = (ulong)((crc >> 8) ^ _LookupTable[index]);
            }

            return crc;
        }

        /// <summary>
        /// Calculates an eight-byte checksum on the bytes passed across.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="littleEndian"></param>
        /// <returns></returns>
        public byte[] ComputeChecksumBytes(byte[] bytes, int offset, int length, bool littleEndian = false)
        {
            return ConvertToByteArray(ComputeChecksum(bytes, offset, length), littleEndian);
        }

        /// <summary>
        /// Calculates the checksum on the bytes passed across and returns it as a string of digit characters of a fixed length.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string ComputeChecksumString(byte[] bytes, int offset, int length)
        {
            var crc = ComputeChecksum(bytes, offset, length);
            return String.Format("{0:X16}", crc);
        }

        /// <summary>
        /// Converts the checksum into a byte array.
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="littleEndian"></param>
        /// <returns></returns>
        private byte[] ConvertToByteArray(ulong crc, bool littleEndian)
        {
            byte[] result = new byte[8];
            if(littleEndian) {
                result[0] = (byte)(crc & 0xff);
                result[1] = (byte)((crc >> 8) & 0xff);
                result[2] = (byte)((crc >> 16) & 0xff);
                result[3] = (byte)((crc >> 24) & 0xff);
                result[4] = (byte)((crc >> 32) & 0xff);
                result[5] = (byte)((crc >> 40) & 0xff);
                result[6] = (byte)((crc >> 48) & 0xff);
                result[7] = (byte)(crc >> 56);
            } else {
                result[0] = (byte)(crc >> 56);
                result[1] = (byte)((crc >> 48) & 0xff);
                result[2] = (byte)((crc >> 40) & 0xff);
                result[3] = (byte)((crc >> 32) & 0xff);
                result[4] = (byte)((crc >> 24) & 0xff);
                result[5] = (byte)((crc >> 16) & 0xff);
                result[6] = (byte)((crc >> 8) & 0xff);
                result[7] = (byte)(crc & 0xff);
            }

            return result;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Crc64()
        {
            for(uint i = 0;i < _LookupTable.Length;++i) {
                ulong crc = i;
                for(int j = 8;j > 0;--j) {
                    if((crc & 1) == 1) crc = (ulong)((crc >> 1) ^ _Polynomial);
                    else               crc >>= 1;
                }

                _LookupTable[i] = crc;
            }
        }
    }
}
