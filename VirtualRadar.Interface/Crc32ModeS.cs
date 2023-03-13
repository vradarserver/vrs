// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A class that can calculate CRC32 parity bits that are applied to Mode-S messages.
    /// </summary>
    /// <remarks>
    /// Note that the CRC for a message of all zeros is zero.
    /// </remarks>
    public static class Crc32ModeS
    {
        /// <summary>
        /// The polynomial used in calculating the checksum.
        /// </summary>
        private const uint _Polynomial = 0xFFFA0480;
        
        /// <summary>
        /// The table of pre-calculated checksum values for all 256 values of a byte.
        /// </summary>
        private static readonly uint[] _LookupTable;

        /// <summary>
        /// The static initialiser
        /// </summary>
        static Crc32ModeS()
        {
            var lookupTable = new uint[256];

            for(uint i = 0;i < lookupTable.Length;++i) {
                var crc = i << 24;
                for(var j = 0;j < 8;++j) {
                    if((crc & 0x80000000) != 0x80000000) {
                        crc <<= 1;
                    } else {
                        crc = (crc ^ _Polynomial) << 1;
                    }
                }

                lookupTable[i] = crc;
            }

            _LookupTable = lookupTable;
        }

        /// <summary>
        /// Calculates the parity on the message passed across. Only the first three bytes (big-endian)
        /// are applied against Mode-S messages.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static uint ComputeChecksum(byte[] bytes, int offset, int length)
        {
            uint crc = 0;

            uint index;
            for(var i = offset; i < offset + length; ++i) {
                index = ((crc & 0xff000000) >> 24) ^ bytes[i];
                crc = (uint)((crc << 8) ^ _LookupTable[index]);
            }

            return crc;
        }

        /// <summary>
        /// Calculates a four-byte parity on the message passed across. Only the first three bytes are applied
        /// against Mode-S messages (when big-endian format is used).
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="littleEndian"></param>
        /// <returns></returns>
        public static byte[] ComputeChecksumBytes(byte[] bytes, int offset, int length, bool littleEndian = false) => ConvertToByteArray(ComputeChecksum(bytes, offset, length), littleEndian);

        /// <summary>
        /// Calculates the four-byte parity on an 11 byte message using the traditional (slow) method
        /// recommended by the Eurocontrol document '022_CRC_calculations_for_Mode_S'.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <remarks>I've put this here for timing comparison tests. Don't use for live code.</remarks>
        public static byte[] ComputeChecksumBytesTraditional88(byte[] bytes)
        {
            var data =  (uint)(bytes[0] << 24)  + (uint)(bytes[1] << 16) + (uint)(bytes[2] << 8) + bytes[3];
            var data1 = (uint)(bytes[4] << 24)  + (uint)(bytes[5] << 16) + (uint)(bytes[6] << 8) + bytes[7];
            var data2 = (uint)(bytes[8] << 24)  + (uint)(bytes[9] << 16) + (uint)(bytes[10] << 8);

            for(var i = 0;i < 88;++i) {
                if((data & 0x80000000) != 0) {
                    data ^= _Polynomial;
                }
                data <<= 1;
                if((data1 & 0x80000000) != 0) {
                    data |= 1;
                }
                data1 <<= 1;
                if((data2 & 0x80000000) != 0) {
                    data1 |= 1;
                }
                data2 <<= 1;
            }

            return ConvertToByteArray(data, false);
        }

        /// <summary>
        /// Calculates the four-byte parity on a 4 byte message using the traditional (slow) method
        /// recommended by the Eurocontrol document '022_CRC_calculations_for_Mode_S'.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        /// <remarks>I've put it here for timing comparison tests. Don't use for live code.</remarks>
        public static byte[] ComputeChecksumBytesTraditional32(byte[] bytes)
        {
            var data = (uint)(bytes[0] << 24)
                     + (uint)(bytes[1] << 16)
                     + (uint)(bytes[2] << 8)
                     +        bytes[3];

            for(var i = 0;i < 32;++i) {
                if((data & 0x80000000) != 0) {
                    data ^= _Polynomial;
                }
                data <<= 1;
            }

            return ConvertToByteArray(data, false);
        }

        /// <summary>
        /// Converts the checksum into a byte array.
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="littleEndian"></param>
        /// <returns></returns>
        private static byte[] ConvertToByteArray(uint crc, bool littleEndian)
        {
            var result = new byte[4];
            if(littleEndian) {
                result[0] = (byte)(crc & 0xff);
                result[1] = (byte)((crc >> 8) & 0xff);
                result[2] = (byte)((crc >> 16) & 0xff);
                result[3] = (byte)(crc >> 24);
            } else {
                result[0] = (byte)(crc >> 24);
                result[1] = (byte)((crc >> 16) & 0xff);
                result[2] = (byte)((crc >> 8) & 0xff);
                result[3] = (byte)(crc & 0xff);
            }

            return result;
        }
    }
}
