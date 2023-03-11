// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.IO;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// A class that can take an array of bytes and stream bits from that byte array.
    /// </summary>
    /// <remarks><para>
    /// An exception is thrown if any operation moves past the end of the stream. Bits can be read across
    /// byte boundaries - e.g. if the bytes are 00001111 01010000 then doing a Skip(4) followed by a
    /// ReadByte(8) will return 11110101.
    /// </para><para>
    /// I did compare this against a much more capable BitStream class from CodeProject but the CodeProject
    /// version was too slow. This version is good enough for reading bits from raw Mode-S packets, which
    /// is all I need for now.
    /// </para><para>
    /// The implementation of the Read methods for UInt16 / 32 / 64 are pretty much cut &amp; pasted, which
    /// is a bit ugly. I did try to have a single generic method that took the return type as a type
    /// parameter but I couldn't constrain the type to just  uint, ushort and ulong, and the compiler
    /// wouldn't let me assign 0 or cast the result of a bit shift to the type parameter. I did consider
    /// using delegates for the parts that were type dependent (i.e. a delegate to do the bit shifting, one
    /// to do the or'ing etc.) but didn't want the function call overhead - bearing in mind that this will
    /// be used to process raw messages from ADS-B receivers and there could be hundreds of those per second.
    /// </para><para>
    /// That previous paragraph (and the implementation it describes) came from the .NET Framework world. I
    /// am still constrained by the lack of generic maths in .NET Core 6, which is the initial target in the
    /// .NET Core world... but when I move onto .NET Core 8 and can make use of its generic maths I should
    /// be able to make this a bit cleaner.
    /// </para><para>
    /// Not thread safe.
    /// </para></remarks>
    public class BitStream
    {
        /// <summary>
        /// A static readonly array of bit masks that isolate a single bit within a byte.
        /// </summary>
        private static readonly byte[] _SingleBitMasks =
        {
            0x80,
            0x40,
            0x20,
            0x10,
            0x08,
            0x04,
            0x02,
            0x01,
        };

        /// <summary>
        /// A static readonly array of bit masks that isolate all bits from a bit position through to the end of a byte.
        /// </summary>
        private static readonly byte[] _IncludeAllBitMasks =
        {
            0xff,
            0x7f,
            0x3f,
            0x1f,
            0x0f,
            0x07,
            0x03,
            0x01,
        };

        /// <summary>
        /// The array of bytes that we are streaming from.
        /// </summary>
        private byte[] _Bytes;

        /// <summary>
        /// The offset of the current byte we're reading from in <see cref="_Bytes"/>.
        /// </summary>
        private int _CurrentByteOffset;

        /// <summary>
        /// The bit that we will read next from <see cref="_CurrentByteOffset"/>. 0 is the left-most bit, 7 is the right-most bit.
        /// </summary>
        private int _CurrentBit;

        /// <summary>
        /// Gets the number of bits remaining on the stream.
        /// </summary>
        /// <remarks><para>
        /// This will decrement as the Read and Skip methods are used.
        /// </para><para>
        /// This property returns 0 when called before <see cref="Initialise"/>.
        /// </para>
        /// </remarks>
        public int LengthRemaining => _Bytes == null
            ? 0
            : (8 - _CurrentBit) + (8 * (_Bytes.Length - (_CurrentByteOffset + 1)));

        /// <summary>
        /// Initialises the stream with an array of bytes. The read methods will return bits from those bytes.
        /// </summary>
        /// <param name="bytes"></param>
        /// <remarks>
        /// Calling this method resets the stream to the start of the array passed across. The intention is that the bitstream can be instantiated
        /// once and then re-used many times to read bits from many byte arrays.
        /// </remarks>
        public void Initialise(byte[] bytes)
        {
            _Bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            _CurrentBit = 0;
            _CurrentByteOffset = 0;
        }

        /// <summary>
        /// Skips a number of bits in the stream.
        /// </summary>
        /// <param name="countBits">The number of bits to skip.</param>
        /// <remarks>The effect of calling this method before <see cref="Initialise"/> has been called is undefined.</remarks>
        public void Skip(int countBits)
        {
            _CurrentBit += countBits;
            if(_CurrentBit > 7) {
                _CurrentByteOffset += _CurrentBit / 8;
                _CurrentBit %= 8;
                if((_CurrentByteOffset == _Bytes.Length && _CurrentBit > 0) || _CurrentByteOffset > _Bytes.Length) {
                    throw new EndOfStreamException("Cannot skip past the end of the stream");
                }
            } else if(_CurrentBit < 0) {
                var overshoot = -_CurrentBit;
                var modOvershoot = overshoot % 8;
                _CurrentByteOffset -= (overshoot + 7) / 8;
                _CurrentBit = modOvershoot == 0
                    ? 0
                    : 8 - modOvershoot;
                if(_CurrentByteOffset < 0) {
                    throw new EndOfStreamException("Cannot skip past the start of the stream");
                }
            }
        }

        /// <summary>
        /// Returns the next bit from the stream. A true value indicates that the next bit was set, a false value indicates the next bit was clear.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The effect of calling this method before <see cref="Initialise"/> has been called is undefined.</remarks>
        public bool ReadBit()
        {
            if(_CurrentByteOffset >= _Bytes.Length) {
                throw new EndOfStreamException("Cannot read past the end of the stream");
            }

            var result = (_Bytes[_CurrentByteOffset] & _SingleBitMasks[_CurrentBit]) != 0;
            if(++_CurrentBit == 8) {
                ++_CurrentByteOffset;
                _CurrentBit = 0;
            }

            return result;
        }

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
        public byte ReadByte(int countBits)
        {
            if(countBits < 1 || countBits > 8) {
                throw new ArgumentOutOfRangeException(nameof(countBits));
            }

            byte result = 0;
            if(countBits == 8 && _CurrentBit == 0) {
                if(_Bytes.Length <= _CurrentByteOffset) {
                    throw new EndOfStreamException("Cannot read bytes past end of stream");
                }
                result = _Bytes[_CurrentByteOffset++];
            } else {
                var shiftRight = 8 - countBits;
                var bits = countBits;
                while(bits > 0) {
                    if(_Bytes.Length <= _CurrentByteOffset) {
                        throw new EndOfStreamException("Cannot read bytes past end of stream");
                    }

                    var bitsLeftInByte = 8 - _CurrentBit;
                    var bitsToTakeFromByte = bitsLeftInByte < bits ? bitsLeftInByte : bits;

                    var bitMask = _IncludeAllBitMasks[_CurrentBit];
                    if(bitsToTakeFromByte < bitsLeftInByte) {
                        bitMask &= (byte)(_IncludeAllBitMasks[8 - bitsToTakeFromByte] << (bitsLeftInByte - bitsToTakeFromByte));
                    }

                    var bitsFromByte = (byte)(_Bytes[_CurrentByteOffset] & bitMask);
                    if(shiftRight > _CurrentBit) {
                        bitsFromByte = (byte)(bitsFromByte >> (shiftRight - _CurrentBit));
                    } else if(shiftRight != _CurrentBit) {
                        bitsFromByte = (byte)(bitsFromByte << (_CurrentBit - shiftRight));
                    }

                    result |= bitsFromByte;

                    bits -= bitsToTakeFromByte;
                    _CurrentBit += bitsToTakeFromByte;
                    if(_CurrentBit > 7) {
                        ++_CurrentByteOffset;
                        _CurrentBit %= 8;
                        shiftRight = 8 - bits;
                    }
                }
            }

            return result;
        }

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
        public ushort ReadUInt16(int countBits)
        {
            if(countBits < 1 || countBits > 16) {
                throw new ArgumentOutOfRangeException(nameof(countBits));
            }
            ushort result = 0;

            if(countBits == 16 && _CurrentBit == 0) {
                if(_CurrentByteOffset + 1 >= _Bytes.Length) {
                    throw new EndOfStreamException("Cannot read words past the end of the stream");
                }
                result = (ushort)(_Bytes[_CurrentByteOffset++] << 8 | _Bytes[_CurrentByteOffset++]);
            } else {
                var bits = countBits;

                while(bits > 0) {
                    if(_CurrentByteOffset >= _Bytes.Length) {
                        throw new EndOfStreamException("Cannot read words past the end of the stream");
                    }
                    var bitsLeftInByte = 8 - _CurrentBit;
                    var bitsToTakeFromByte = bitsLeftInByte < bits ? bitsLeftInByte : bits;

                    var bitMask = _IncludeAllBitMasks[_CurrentBit];
                    if(bitsToTakeFromByte < bitsLeftInByte) {
                        bitMask &= (byte)(_IncludeAllBitMasks[8 - bitsToTakeFromByte] << (bitsLeftInByte - bitsToTakeFromByte));
                    }

                    var bitsFromByte = (ushort)(_Bytes[_CurrentByteOffset] & bitMask);

                    var rightShiftAlignByte = 7 - _CurrentBit;
                    var leftShiftAlignResult = bits - 1;
                    var deltaShiftLeft = leftShiftAlignResult - rightShiftAlignByte;
                    if(deltaShiftLeft > 0) {
                        bitsFromByte = (ushort)(bitsFromByte << deltaShiftLeft);
                    } else if(deltaShiftLeft < 0) {
                        bitsFromByte = (ushort)(bitsFromByte >> -deltaShiftLeft);
                    }

                    result |= bitsFromByte;

                    bits -= bitsToTakeFromByte;
                    _CurrentBit += bitsToTakeFromByte;
                    if(_CurrentBit > 7) {
                        ++_CurrentByteOffset;
                        _CurrentBit %= 8;
                    }
                }
            }

            return result;
        }

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
        public uint ReadUInt32(int countBits)
        {
            if(countBits < 1 || countBits > 32) {
                throw new ArgumentOutOfRangeException(nameof(countBits));
            }
            uint result = 0;

            if(countBits == 32 && _CurrentBit == 0) {
                if(_CurrentByteOffset + 3 >= _Bytes.Length) {
                    throw new EndOfStreamException("Cannot read double-words past the end of the stream");
                }
                result = (uint)(
                         _Bytes[_CurrentByteOffset++] << 24
                       | _Bytes[_CurrentByteOffset++] << 16
                       | _Bytes[_CurrentByteOffset++] << 8
                       | _Bytes[_CurrentByteOffset++]
                );
            } else {
                var bits = countBits;

                while(bits > 0) {
                    if(_CurrentByteOffset >= _Bytes.Length) {
                        throw new EndOfStreamException("Cannot read double-words past the end of the stream");
                    }
                    var bitsLeftInByte = 8 - _CurrentBit;
                    var bitsToTakeFromByte = bitsLeftInByte < bits ? bitsLeftInByte : bits;

                    var bitMask = _IncludeAllBitMasks[_CurrentBit];
                    if(bitsToTakeFromByte < bitsLeftInByte) {
                        bitMask &= (byte)(_IncludeAllBitMasks[8 - bitsToTakeFromByte] << (bitsLeftInByte - bitsToTakeFromByte));
                    }

                    var bitsFromByte = (uint)(_Bytes[_CurrentByteOffset] & bitMask);

                    var rightShiftAlignByte = 7 - _CurrentBit;
                    var leftShiftAlignResult = bits - 1;
                    var deltaShiftLeft = leftShiftAlignResult - rightShiftAlignByte;
                    if(deltaShiftLeft > 0) {
                        bitsFromByte = (uint)(bitsFromByte << deltaShiftLeft);
                    } else if(deltaShiftLeft < 0) {
                        bitsFromByte = (uint)(bitsFromByte >> -deltaShiftLeft);
                    }

                    result |= bitsFromByte;

                    bits -= bitsToTakeFromByte;
                    _CurrentBit += bitsToTakeFromByte;
                    if(_CurrentBit > 7) {
                        ++_CurrentByteOffset;
                        _CurrentBit %= 8;
                    }
                }
            }

            return result;
        }

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
        public ulong ReadUInt64(int countBits)
        {
            if(countBits < 1 || countBits > 64) {
                throw new ArgumentOutOfRangeException(nameof(countBits));
            }
            ulong result = 0;

            if(countBits == 64 && _CurrentBit == 0) {
                if(_CurrentByteOffset + 7 >= _Bytes.Length) {
                    throw new EndOfStreamException("Cannot read quad-words past the end of the stream");
                }
                result = (ulong)_Bytes[_CurrentByteOffset++] << 56
                       | (ulong)_Bytes[_CurrentByteOffset++] << 48
                       | (ulong)_Bytes[_CurrentByteOffset++] << 40
                       | (ulong)_Bytes[_CurrentByteOffset++] << 32
                       | (ulong)_Bytes[_CurrentByteOffset++] << 24
                       | (ulong)_Bytes[_CurrentByteOffset++] << 16
                       | (ulong)_Bytes[_CurrentByteOffset++] << 8
                       | (ulong)_Bytes[_CurrentByteOffset++];
            } else {
                var bits = countBits;

                while(bits > 0) {
                    if(_CurrentByteOffset >= _Bytes.Length) {
                        throw new EndOfStreamException("Cannot read quad-words past the end of the stream");
                    }
                    var bitsLeftInByte = 8 - _CurrentBit;
                    var bitsToTakeFromByte = bitsLeftInByte < bits ? bitsLeftInByte : bits;

                    var bitMask = _IncludeAllBitMasks[_CurrentBit];
                    if(bitsToTakeFromByte < bitsLeftInByte) {
                        bitMask &= (byte)(_IncludeAllBitMasks[8 - bitsToTakeFromByte] << (bitsLeftInByte - bitsToTakeFromByte));
                    }

                    var bitsFromByte = (ulong)(_Bytes[_CurrentByteOffset] & bitMask);

                    var rightShiftAlignByte = 7 - _CurrentBit;
                    var leftShiftAlignResult = bits - 1;
                    var deltaShiftLeft = leftShiftAlignResult - rightShiftAlignByte;
                    if(deltaShiftLeft > 0) {
                        bitsFromByte = (ulong)(bitsFromByte << deltaShiftLeft);
                    } else if(deltaShiftLeft < 0) {
                        bitsFromByte = (ulong)(bitsFromByte >> -deltaShiftLeft);
                    }

                    result |= bitsFromByte;

                    bits -= bitsToTakeFromByte;
                    _CurrentBit += bitsToTakeFromByte;
                    if(_CurrentBit > 7) {
                        ++_CurrentByteOffset;
                        _CurrentBit %= 8;
                    }
                }
            }

            return result;
        }
    }
}
