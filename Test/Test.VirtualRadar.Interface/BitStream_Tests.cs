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
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class BitStream_Tests
    {
        private BitStream _BitStream;

        [TestInitialize]
        public void TestInitialise()
        {
            _BitStream = new();
        }

        private static byte[] ParseBinary(string binary)
        {
            List<byte> result = new();

            var bit = 7;
            byte b = 0;

            foreach(var ch in binary) {
                if(ch == '0') {
                    --bit;
                } else if(ch == '1') {
                    b += (byte)(1 << bit--);
                }
                if(bit == -1) {
                    result.Add(b);
                    bit = 7;
                    b = 0;
                }
            }

            if(bit != 7) {
                result.Add(b);
            }

            return result.ToArray();
        }

        #region LengthRemaining
        [TestMethod]
        public void LengthRemaining_Returns_Zero_If_Called_Before_Initialise()
        {
            Assert.AreEqual(0, _BitStream.LengthRemaining);
        }

        [TestMethod]
        public void LengthRemaining_Returns_Zero_If_Called_After_Stream_Has_Been_Depleted()
        {
            _BitStream.Initialise(new byte[] { 0x0f });
            _BitStream.ReadByte(8);
            Assert.AreEqual(0, _BitStream.LengthRemaining);
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Initialise_Throws_If_Passed_Null()
        {
            _BitStream.Initialise(null);
        }

        [TestMethod]
        public void Initialise_Does_Not_Throw_If_Passed_Empty_Array()
        {
            _BitStream.Initialise(new byte[] {});
        }

        [TestMethod]
        public void Initialise_Resets_The_Stream()
        {
            _BitStream.Initialise(new byte[] { 0x00, 0x00 });
            for(var i = 0;i < 16;++i) {
                Assert.AreEqual(16 - i, _BitStream.LengthRemaining);
                Assert.IsFalse(_BitStream.ReadBit());
            }

            _BitStream.Initialise(new byte[] { 0xff, 0xff });
            for(var i = 0;i < 16;++i) {
                Assert.AreEqual(16 - i, _BitStream.LengthRemaining);
                Assert.IsTrue(_BitStream.ReadBit());
            }
        }
        #endregion

        #region ReadBit
        [TestMethod]
        public void ReadBit_Returns_Correct_Values()
        {
            var binary = "00110100 11001011";
            _BitStream.Initialise(ParseBinary(binary));

            var posn = 0;
            foreach(char ch in binary) {
                if(ch != '0' && ch != '1') {
                    continue;
                }
                var bit = _BitStream.ReadBit();
                if(ch == '0') {
                    Assert.IsFalse(bit, "Expected false at bit {0}, ReadBit returned true", posn);
                } else {
                    Assert.IsTrue(bit, "Expected true at bit {0}, ReadBit returned false", posn);
                }
                ++posn;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void ReadBit_Throws_Exception_If_Reading_Past_End_Of_Stream()
        {
            _BitStream.Initialise(new byte[] {});
            _BitStream.ReadBit();
        }

        [TestMethod]
        public void ReadBit_Updates_LengthRemaining()
        {
            _BitStream.Initialise(new byte[] { 0x00, 0x00 });
            for(var i = 16;i > 0;--i) {
                Assert.AreEqual(i, _BitStream.LengthRemaining);
                _BitStream.ReadBit();
                Assert.AreEqual(i - 1, _BitStream.LengthRemaining);
            }
        }
        #endregion

        #region Skip
        [TestMethod]
        public void Skip_Jumps_Over_Bits_In_The_Stream()
        {
            var binary = "11001010001101011101001111110000";
            for(var i = 0;i < binary.Length;++i) {
                var expectedBit = binary[i] == '1';

                _BitStream.Initialise(ParseBinary(binary));
                _BitStream.Skip(i);

                Assert.AreEqual(expectedBit, _BitStream.ReadBit(), "Expected {0} after skipping {1} bits, ReadBit returned {2}", expectedBit, i, !expectedBit);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void Skip_Throws_If_Moved_Past_End_Of_Stream()
        {
            _BitStream.Initialise(new byte[] { 0x00 });
            _BitStream.Skip(9);
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void Skip_Throws_If_Moved_Past_Start_Of_Stream()
        {
            _BitStream.Initialise(new byte[] { 0x00 });
            _BitStream.Skip(-1);
        }

        [TestMethod]
        public void Skip_Does_Not_Throw_If_Skipped_To_End_Of_Stream()
        {
            _BitStream.Initialise(new byte[] { 0x00 });
            _BitStream.Skip(8);
        }

        [TestMethod]
        public void Skip_Can_Move_Backwards_Through_Stream()
        {
            _BitStream.Initialise(new byte[] { 0x0C }); // 00001100
            _BitStream.Skip(4);
            _BitStream.ReadByte(2);
            _BitStream.Skip(-2);
            Assert.AreEqual((byte)3, _BitStream.ReadByte(2));
        }

        [TestMethod]
        public void Skip_Can_Move_Backwards_Over_Byte_Boundaries()
        {
            _BitStream.Initialise(new byte[] { 0x03, 0xC0 }); // 00000011 11000000
            _BitStream.Skip(6);
            _BitStream.ReadByte(4);
            _BitStream.Skip(-4);
            Assert.AreEqual((byte)0x0f, _BitStream.ReadByte(4));
        }

        [TestMethod]
        public void Skip_Can_Jump_Backwards_Over_Bits_In_The_Stream()
        {
            var binary = "11001010001101011101001111110000";
            for(var i = 0;i < binary.Length;++i) {
                var index = (binary.Length - 1) - i;
                var skipBackwardsFromEnd = 0 - (i + 1);

                var expectedBit = binary[index] == '1';

                _BitStream.Initialise(ParseBinary(binary));
                _BitStream.Skip(binary.Length);
                _BitStream.Skip(skipBackwardsFromEnd);

                Assert.AreEqual(expectedBit, _BitStream.ReadBit(), "Expected {0} after skipping {1} bits from end, ReadBit returned {2}",
                    expectedBit,
                    skipBackwardsFromEnd,
                    !expectedBit
                );
            }
        }
        #endregion

        #region ReadByte
        [TestMethod]
        public void ReadByte_Throws_If_Reading_Less_Than_One_Or_More_Than_8_Bits()
        {
            for(var i = -2;i < 10;++i) {
                _BitStream.Initialise(new byte[] { 0x00 });

                var expectException = i < 1 || i > 8;
                var sawException = false;
                try {
                    _BitStream.ReadByte(i);
                } catch(ArgumentOutOfRangeException) {
                    sawException = true;
                }

                Assert.AreEqual(expectException, sawException, "ReadByte did not behave correctly when asked to read {0} bits", i);
            }
        }

        [TestMethod]
        public void ReadByte_Can_Read_A_Simple_8_Bit_Value()
        {
            _BitStream.Initialise(new byte[] { 0x00, 0xff });
            Assert.AreEqual(0x00, _BitStream.ReadByte(8));
            Assert.AreEqual(0xff, _BitStream.ReadByte(8));
        }

        [TestMethod]
        public void ReadByte_Can_Read_Partial_Bytes_That_Straddle_Byte_Boundaries()
        {
            var binary = "00110101 11001010".Replace(" ", "");

            for(var bits = 1;bits <= 8;++bits) {
                for(var startPosition = 0;startPosition < binary.Length - bits;++startPosition) {
                    var expectedBinary = String.Format("{0}{1}", new String('0', 8 - bits), binary.Substring(startPosition, bits));
                    var expectedValue = ParseBinary(expectedBinary)[0];

                    _BitStream.Initialise(ParseBinary(binary));
                    _BitStream.Skip(startPosition);
                    var actualValue = _BitStream.ReadByte(bits);
                    Assert.AreEqual(expectedValue, actualValue, "Expected the value 0x{0:X} ({1}) when reading {2} bits starting at {3} - ReadByte returned 0x{4:X}", expectedValue, expectedBinary, bits, startPosition, actualValue);
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void ReadByte_Throws_Exception_When_Reading_Past_End_Of_Stream()
        {
            _BitStream.Initialise(new byte[] {});
            _BitStream.ReadByte(8);
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void ReadByte_Throws_Exception_When_Reading_Past_End_Of_Stream_Over_Byte_Boundary()
        {
            _BitStream.Initialise(new byte[] { 0xff });
            _BitStream.Skip(6);
            _BitStream.ReadByte(4);
        }
        #endregion

        #region ReadUInt16
        [TestMethod]
        public void ReadUInt16_Throws_If_Reading_Less_Than_One_Or_More_Than_16_Bits()
        {
            for(var i = -2;i < 18;++i) {
                _BitStream.Initialise(new byte[] { 0x00, 0x00 });

                var expectException = i < 1 || i > 16;
                var sawException = false;
                try {
                    _BitStream.ReadUInt16(i);
                } catch(ArgumentOutOfRangeException) {
                    sawException = true;
                }

                Assert.AreEqual(expectException, sawException, "ReadUInt16 did not behave correctly when asked to read {0} bits", i);
            }
        }

        [TestMethod]
        public void ReadUInt16_Can_Read_A_Simple_16_Bit_Value()
        {
            _BitStream.Initialise(new byte[] { 0xff, 0x00, 0x00, 0xff });
            Assert.AreEqual(0xff00, _BitStream.ReadUInt16(16));
            Assert.AreEqual(0x00ff, _BitStream.ReadUInt16(16));
        }

        [TestMethod]
        public void ReadUInt16_Can_Read_Partial_Words_That_Straddle_Byte_Boundaries()
        {
            var binary = "00110101 11001010 10101010 01010101".Replace(" ", "");

            for(var bits = 1;bits <= 16;++bits) {
                for(var startPosition = 0;startPosition < binary.Length - bits;++startPosition) {
                    var expectedBinary = ParseBinary(String.Format("{0}{1}", new String('0', 16 - bits), binary.Substring(startPosition, bits)));
                    var expectedValue = (ushort)((expectedBinary[0] << 8) | expectedBinary[1]);

                    _BitStream.Initialise(ParseBinary(binary));
                    _BitStream.Skip(startPosition);
                    var actualValue = _BitStream.ReadUInt16(bits);
                    Assert.AreEqual(expectedValue, actualValue, "Expected the value 0x{0:X} ({1}) when reading {2} bits starting at {3} - ReadUInt16 returned 0x{4:X}", expectedValue, expectedBinary, bits, startPosition, actualValue);
                }
            }
        }

        [TestMethod]
        public void ReadUInt16_Throws_Exception_When_Reading_Past_End_Of_Stream()
        {
            for(var i = 0;i < 2;++i) {
                _BitStream.Initialise(new byte[i]);
                var seenException = false;
                try {
                    _BitStream.ReadUInt16(16);
                } catch(EndOfStreamException) {
                    seenException = true;
                }
                Assert.IsTrue(seenException);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void ReadUInt16_Throws_Exception_When_Reading_Past_End_Of_Stream_Over_Byte_Boundary()
        {
            _BitStream.Initialise(new byte[] { 0xff, 0xff });
            _BitStream.Skip(6);
            _BitStream.ReadUInt16(11);
        }
        #endregion

        #region ReadUInt32
        [TestMethod]
        public void ReadUInt32_Throws_If_Reading_Less_Than_One_Or_More_Than_32_Bits()
        {
            for(var i = -2;i < 34;++i) {
                _BitStream.Initialise(new byte[] { 0x00, 0x00, 0x00, 0x00 });

                var expectException = i < 1 || i > 32;
                var sawException = false;
                try {
                    _BitStream.ReadUInt32(i);
                } catch(ArgumentOutOfRangeException) {
                    sawException = true;
                }

                Assert.AreEqual(expectException, sawException, "ReadUInt32 did not behave correctly when asked to read {0} bits", i);
            }
        }

        [TestMethod]
        public void ReadUInt32_Can_Read_A_Simple_32_Bit_Value()
        {
            _BitStream.Initialise(new byte[] { 0xff, 0x01, 0x02, 0x03, 0x03, 0x02, 0x01, 0xff });
            Assert.AreEqual(0xff010203U, _BitStream.ReadUInt32(32));
            Assert.AreEqual(0x030201ffU, _BitStream.ReadUInt32(32));
        }

        [TestMethod]
        public void ReadUInt32_Can_Read_Partial_Double_Words_That_Straddle_Byte_Boundaries()
        {
            var binary = "00110101 11001010 10101010 01010101 11110000 00001111 11000011 00111100".Replace(" ", "");

            for(var bits = 1;bits <= 32;++bits) {
                for(int startPosition = 0;startPosition < binary.Length - bits;++startPosition) {
                    var expectedBinary = ParseBinary(String.Format("{0}{1}", new String('0', 32 - bits), binary.Substring(startPosition, bits)));
                    var expectedValue = (uint)((expectedBinary[0] << 24) | expectedBinary[1] << 16 | expectedBinary[2] << 8 | expectedBinary[3]);

                    _BitStream.Initialise(ParseBinary(binary));
                    _BitStream.Skip(startPosition);
                    var actualValue = _BitStream.ReadUInt32(bits);
                    Assert.AreEqual(expectedValue, actualValue, "Expected the value 0x{0:X} ({1}) when reading {2} bits starting at {3} - ReadUInt32 returned 0x{4:X}", expectedValue, expectedBinary, bits, startPosition, actualValue);
                }
            }
        }

        [TestMethod]
        public void ReadUInt32_Throws_Exception_When_Reading_Past_End_Of_Stream()
        {
            for(var i = 0;i < 4;++i) {
                _BitStream.Initialise(new byte[i]);
                var seenException = false;
                try {
                    _BitStream.ReadUInt32(32);
                } catch(EndOfStreamException) {
                    seenException = true;
                }
                Assert.IsTrue(seenException);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void ReadUInt32_Throws_Exception_When_Reading_Past_End_Of_Stream_Over_Byte_Boundary()
        {
            _BitStream.Initialise(new byte[] { 0xff, 0xff, 0xff, 0xff });
            _BitStream.Skip(6);
            _BitStream.ReadUInt32(27);
        }
        #endregion

        #region ReadUInt64
        [TestMethod]
        public void ReadUInt64_Throws_If_Reading_Less_Than_One_Or_More_Than_64_Bits()
        {
            for(var i = -2;i < 66;++i) {
                _BitStream.Initialise(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

                var expectException = i < 1 || i > 64;
                var sawException = false;
                try {
                    _BitStream.ReadUInt64(i);
                } catch(ArgumentOutOfRangeException) {
                    sawException = true;
                }

                Assert.AreEqual(expectException, sawException, "ReadUInt64 did not behave correctly when asked to read {0} bits", i);
            }
        }

        [TestMethod]
        public void ReadUInt64_Can_Read_A_Simple_64_Bit_Value()
        {
            _BitStream.Initialise(new byte[] { 0xff, 0xfe, 0xfd, 0xfc, 0xfb, 0xfa, 0xf9, 0xf8,
                                               0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 });
            var value1 = _BitStream.ReadUInt64(64);
            var value2 = _BitStream.ReadUInt64(64);
            Assert.AreEqual(0xfffefdfcfbfaf9f8UL, value1, "Expected {0:X}, actual {1:X}", 0xfffefdfcfbfaf9f8UL, value1);
            Assert.AreEqual(0x0102030405060708UL, value2, "Expected {0:X}, actual {1:X}", 0x0102030405060708UL, value2);
        }

        [TestMethod]
        public void ReadUInt64_Can_Read_Partial_Quad_Words_That_Straddle_Byte_Boundaries()
        {
            var binary = ("00110101 11001010 10101010 01010101 11110000 00001111 11000011 00111100" +
                          "11111111 00000000 10111101 01000010 11011011 00100100 11101110 00010001").Replace(" ", "");

            for(var bits = 1;bits <= 64;++bits) {
                for(var startPosition = 0;startPosition < binary.Length - bits;++startPosition) {
                    var expectedBinary = ParseBinary(String.Format("{0}{1}", new String('0', 64 - bits), binary.Substring(startPosition, bits)));
                    var expectedValue = (ulong)expectedBinary[0] << 56 | (ulong)expectedBinary[1] << 48 | (ulong)expectedBinary[2] << 40 | (ulong)expectedBinary[3] << 32 |
                                        (ulong)expectedBinary[4] << 24 | (ulong)expectedBinary[5] << 16 | (ulong)expectedBinary[6] << 8  | (ulong)expectedBinary[7];

                    _BitStream.Initialise(ParseBinary(binary));
                    _BitStream.Skip(startPosition);
                    var actualValue = _BitStream.ReadUInt64(bits);
                    Assert.AreEqual(expectedValue, actualValue, "Expected the value 0x{0:X} ({1}) when reading {2} bits starting at {3} - ReadUInt64 returned 0x{4:X}", expectedValue, expectedBinary, bits, startPosition, actualValue);
                }
            }
        }

        [TestMethod]
        public void ReadUInt64_Throws_Exception_When_Reading_Past_End_Of_Stream()
        {
            for(var i = 0;i < 8;++i) {
                _BitStream.Initialise(new byte[i]);
                var seenException = false;
                try {
                    _BitStream.ReadUInt64(64);
                } catch(EndOfStreamException) {
                    seenException = true;
                }
                Assert.IsTrue(seenException);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EndOfStreamException))]
        public void ReadUInt64_Throws_Exception_When_Reading_Past_End_Of_Stream_Over_Byte_Boundary()
        {
            _BitStream.Initialise(new byte[] { 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff });
            _BitStream.Skip(6);
            _BitStream.ReadUInt64(59);
        }
        #endregion

        #region All Read Methods
        [TestMethod]
        public void AllReadMethods_Consume_Stream()
        {
            var binary = "1" +                                                                        // true
                         "00111111" +                                                                 // 3F
                         "01010101 01010101" +                                                        // 5555
                         "11111111 00000000 11110000 00001111" +                                      // FF00F00F
                         "01111110 11111111 00110011 01010101 11001111 00110000 10101010 11000011";   // 7EFF3355CF30AAC3
            _BitStream.Initialise(ParseBinary(binary));
            Assert.IsTrue(_BitStream.ReadBit());
            Assert.AreEqual(0x3f, _BitStream.ReadByte(8));
            Assert.AreEqual(0x5555, _BitStream.ReadUInt16(16));
            Assert.AreEqual(0xFF00F00FU, _BitStream.ReadUInt32(32));
            Assert.AreEqual(0x7EFF3355CF30AAC3UL, _BitStream.ReadUInt64(64));
        }
        #endregion
    }
}
