// This was copied entirely from the Sanity Free Coding blog:
// http://sanity-free.com/134/standard_crc_16_in_csharp.html

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A utility class that calculates CRC16 checksums.
    /// </summary>
    /// <remarks>
    /// This was copied entirely from the Sanity Free Coding blog:
    /// http://sanity-free.com/134/standard_crc_16_in_csharp.html
    /// </remarks>
    public class Crc16
    {
        const ushort _Polynominal = 0xA001;
        ushort[] _Table = new ushort[256];

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public Crc16()
        {
            ushort value;
            ushort temp;
            for(ushort i = 0; i < _Table.Length; ++i) {
                value = 0;
                temp = i;
                for(byte j = 0; j < 8; ++j) {
                    if(((value ^ temp) & 0x0001) != 0) {
                        value = (ushort)((value >> 1) ^ _Polynominal);
                    } else {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                _Table[i] = value;
            }
        }

        /// <summary>
        /// Calculates the CRC16 checksum for an array of bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>The checksum as an unsigned 16-bit value.</returns>
        public ushort ComputeChecksum(byte[] bytes)
        {
            ushort crc = 0;
            for(var i = 0; i < bytes.Length; ++i) {
                var index = (byte)(crc ^ bytes[i]);
                crc = (ushort)((crc >> 8) ^ _Table[index]);
            }

            return crc;
        }

        /// <summary>
        /// Calculates the CRC16 checksum for an array of bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>The checksum as an array of bytes.</returns>
        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            var crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }
    }
}
