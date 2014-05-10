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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for classes that can take an array of bytes and stream bits from that byte array.
    /// </summary>
    /// <remarks><para>
    /// An exception is thrown if any operation moves past the end of the stream. Bits can be read across
    /// byte boundaries - e.g. if the bytes are 00001111 01010000 then doing a Skip(4) followed by a
    /// ReadByte(8) will return 11110101.
    /// </para><para>
    /// Implementations do not need to be thread-safe. If thread-safety is required then it is up to the
    /// caller to lock the BitStream.
    /// </para>
    /// </remarks>
    public interface IBitStream
    {
        /// <summary>
        /// Gets the number of bits remaining on the stream.
        /// </summary>
        /// <remarks><para>
        /// This will decrement as the Read and Skip methods are used.
        /// </para><para>
        /// This property returns 0 when called before <see cref="Initialise"/>.
        /// </para>
        /// </remarks>
        int LengthRemaining { get; }

        /// <summary>
        /// Initialises the stream with an array of bytes. The read methods will return bits from those bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <remarks>
        /// Calling this method resets the stream to the start of the array passed across. The intention is that the bitstream can be instantiated
        /// once and then re-used many times to read bits from many byte arrays.
        /// </remarks>
        void Initialise(byte[] bytes);

        /// <summary>
        /// Skips a number of bits in the stream.
        /// </summary>
        /// <param name="countBits">The number of bits to skip.</param>
        /// <remarks>The effect of calling this method before <see cref="Initialise"/> has been called is undefined.</remarks>
        void Skip(int countBits);

        /// <summary>
        /// Returns the next bit from the stream. A true value indicates that the next bit was set, a false value indicates the next bit was clear.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The effect of calling this method before <see cref="Initialise"/> has been called is undefined.</remarks>
        bool ReadBit();

        /// <summary>
        /// Returns up to the next 8 bits from the stream.
        /// </summary>
        /// <param name="countBits">Cannot be less than 1 or greater than 8.</param>
        /// <returns></returns>
        /// <remarks><para>
        /// The bits are right-shifted so that the LSB read is the LSB of the return value. For example, if three bits from the stream '101' are read then the resulting
        /// byte will be '00000101'.
        /// </para><para>
        /// The effect of calling this method before <see cref="Initialise"/> is undefined.
        /// </para>
        /// </remarks>
        byte ReadByte(int countBits);

        /// <summary>
        /// Returns up to the next 16 bits from the stream in big-endian order.
        /// </summary>
        /// <param name="countBits">Cannot be less than 1 or greater than 16.</param>
        /// <returns></returns>
        /// <remarks><para>
        /// The bits are right-shifted so that the LSB read is the LSB of the return value. For example, if three bits from the stream '101' are read then the resulting
        /// word will be '00000000 00000101'.
        /// </para><para>
        /// The effect of calling this method before <see cref="Initialise"/> is undefined.
        /// </para>
        /// </remarks>
        UInt16 ReadUInt16(int countBits);

        /// <summary>
        /// Returns up to the next 32 bits from the stream in big-endian order.
        /// </summary>
        /// <param name="countBits">Cannot be less than 1 or greater than 32.</param>
        /// <returns></returns>
        /// <remarks><para>
        /// The bits are right-shifted so that the LSB read is the LSB of the return value. For example, if three bits from the stream '101' are read then the resulting
        /// double-word will be '00000000  00000000 00000000 00000101'.
        /// </para><para>
        /// The effect of calling this method before <see cref="Initialise"/> is undefined.
        /// </para>
        /// </remarks>
        UInt32 ReadUInt32(int countBits);

        /// <summary>
        /// Returns up to the next 64 bits from the stream in big-endian order.
        /// </summary>
        /// <param name="countBits">Cannot be less than 1 or greater than 64.</param>
        /// <returns></returns>
        /// <remarks><para>
        /// The bits are right-shifted so that the LSB read is the LSB of the return value. For example, if three bits from the stream '101' are read then the resulting
        /// quad-word will be '00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000101'.
        /// </para><para>
        /// The effect of calling this method before <see cref="Initialise"/> is undefined.
        /// </para>
        /// </remarks>
        UInt64 ReadUInt64(int countBits);
    }
}
