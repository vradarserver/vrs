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
    /// A utility class that calculates CCITT-16 CRC values.
    /// </summary>
    /// <remarks><para>
    /// Two algorithms were found that described themselves as CCITT-16 CRC calculators - the one that is used
    /// by Kinetic (and at least four other entities) and the one described in the Dr. Dobbs journal. The Dr. Dobbs
    /// one is included for the sake of completeness.
    /// </para><para>
    /// The Kinetic version was copied from the Sanity Free Coding
    /// blog - http://sanity-free.org/133/crc_16_ccitt_in_csharp.html. Note that the checksum that Kinetic use
    /// on SBS-3 packets is using an initial CRC value of zero. I modified the method that returns a byte array because
    /// Kinetic's CRC is sent big-endian and the original was returning it little-endian.
    /// </para><para>
    /// A second implementation of the standard algorithm was found here - http://www.ccsinfo.com/forum/viewtopic.php?t=24977.
    /// It purported to be faster than the lookup table version, which is probably true for implementations in hardware
    /// or languages that compile straight to assembly but was not found to be the case in testing under C#, it was marginally
    /// slower than Sanity Free Coding's implementation.
    /// </para><para>
    /// The Dr. Dobbs version was copied from the Dr. Dobbs journal site -
    /// http://www.drdobbs.com/implementing-the-ccitt-cyclical-redundan/199904926. The implementation here has been
    /// tested against the expected checksum in the examples listing on that page.
    /// </para></remarks>
    public class Crc16Ccitt
    {
        /// <summary>
        /// An initial CRC value of zero.
        /// </summary>
        public const ushort InitialiseToZero = 0x0000;

        /// <summary>
        /// An initial CRC value of all-ones.
        /// </summary>
        public const ushort InitialiseToAllOnes = 0xffff;

        /// <summary>
        /// The lookup table.
        /// </summary>
        ushort[] _Table = new ushort[256];

        /// <summary>
        /// The initial value for the checksum.
        /// </summary>
        ushort _InitialChecksumValue;

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="initialChecksumValue"></param>
        public Crc16Ccitt(ushort initialChecksumValue = InitialiseToAllOnes)
        {
            _InitialChecksumValue = initialChecksumValue;

            ushort temp, a;
            for(var i = 0; i < _Table.Length; ++i) {
                temp = 0;
                a = (ushort)(i << 8);
                for(var j = 0; j < 8; ++j) {
                    if(((temp ^ a) & 0x8000) != 0) {
                        temp = (ushort)((temp << 1) ^ 4129);
                    } else {
                        temp <<= 1;
                    }
                    a <<= 1;
                }
                _Table[i] = temp;
            }
        }

        /// <summary>
        /// Returns the CRC-16-CCITT checksum for the array of bytes passed across using the standard calculation.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public ushort ComputeChecksum(byte[] bytes)
        {
            var crc = _InitialChecksumValue;
            for(var i = 0; i < bytes.Length; ++i) {
                crc = (ushort)(
                      (crc << 8)
                    ^ _Table[((crc >> 8) ^ (0xff & bytes[i]))]
                );
            }

            return crc;
        }

        /// <summary>
        /// Returns the CRC-16-CCITT checksum for the array of bytes passed across using the standard calculation.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public ushort ComputeChecksum(byte[] bytes, int start, int length)
        {
            if(start < 0 || start > bytes.Length) {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if(start + length > bytes.Length) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            var crc = _InitialChecksumValue;
            var end = start + length;
            for(var i = start; i < end; ++i) {
                crc = (ushort)(
                      (crc << 8)
                    ^ _Table[((crc >> 8) ^ (0xff & bytes[i]))]
                );
            }

            return crc;
        }

        /// <summary>
        /// Returns the CRC-16-CCITT checksum for the array of bytes passed across using the standard calculation.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="littleEndian"></param>
        /// <returns></returns>
        public byte[] ComputeChecksumBytes(byte[] bytes, bool littleEndian = true) => ConvertToByteArray(ComputeChecksum(bytes), littleEndian);

        /// <summary>
        /// Returns the CRC-16-CCITT checksum for the array of bytes passed across using the standard calculation.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="littleEndian"></param>
        /// <returns></returns>
        public byte[] ComputeChecksumBytes(byte[] bytes, int start, int length, bool littleEndian = true) => ConvertToByteArray(ComputeChecksum(bytes, start, length), littleEndian);

        /// <summary>
        /// Converts the checksum into a byte array.
        /// </summary>
        /// <param name="crc"></param>
        /// <param name="littleEndian"></param>
        /// <returns></returns>
        private static byte[] ConvertToByteArray(uint crc, bool littleEndian)
        {
            var result = new byte[2];

            if(littleEndian) {
                result[0] = (byte)(crc & 0xff);
                result[1] = (byte)((crc >> 8) & 0xff);
            } else {
                result[0] = (byte)((crc >> 8) & 0xff);
                result[1] = (byte)(crc & 0xff);
            }

            return result;
        }

        /// <summary>
        /// Returns the CRC-16-CCITT checksum for the array of bytes passed across using the Dr. Dobbs algorithm.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public ushort ComputeChecksumDrDobbs(byte[] bytes)
        {
            int crc = _InitialChecksumValue;

            foreach(var b in bytes) {
                for(int i = 0, data = b;i < 8;++i, data >>= 1) {
                    if(((crc & 0x0001) ^ (data & 0x0001)) != 0) {
                        crc = (crc >> 1) ^ 0x8408;
                    } else {
                        crc >>= 1;
                    }
                }
            }
            crc = (~crc) & 0xffff;

            return (ushort)(((crc & 0xff00) >> 8) | ((crc & 0xff) << 8));
        }

        /// <summary>
        /// Returns the CRC-16-CCITT checksum for the array of bytes passed across using the Dr. Dobbs algorithm.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="littleEndian"></param>
        /// <returns></returns>
        public byte[] ComputeChecksumDrDobbsBytes(byte[] bytes, bool littleEndian = true) => ConvertToByteArray(ComputeChecksumDrDobbs(bytes), littleEndian);
    }
}
