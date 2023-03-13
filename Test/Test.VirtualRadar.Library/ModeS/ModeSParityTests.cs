// Copyright © 2012 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using Test.Framework;
using VirtualRadar.Interface.ModeS;
using VirtualRadar.Library.ModeS;

namespace Test.VirtualRadar.Library.ModeS
{
    [TestClass]
    public class ModeSParityTests
    {
        public TestContext TestContext { get; set; }

        private IModeSParity _ModeSParity;

        [TestInitialize]
        public void TestInitialise()
        {
#pragma warning disable CS0618 // Type or member is obsolete (used to warn people not to instantiate directly)
            _ModeSParity = new ModeSParity();
#pragma warning restore CS0618
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StripParity_Throws_If_Byte_Array_Is_Null()
        {
            _ModeSParity.StripParity(null, 0, 7);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void StripParity_Throws_If_Offset_Is_Not_Within_Bounds()
        {
            _ModeSParity.StripParity(new byte[7], 7, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void StripParity_Throws_If_Offset_Is_Negative()
        {
            _ModeSParity.StripParity(new byte[7], -1, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void StripParity_Throws_If_Length_Exceeds_Bytes_Available()
        {
            _ModeSParity.StripParity(new byte[6], 0, 7);
        }

        [TestMethod]
        [DataRow("8D 46 92 D8 60 8F B0 E0 0C 96 24 F2 42 95",    0, 14, "8D 46 92 D8 60 8F B0 E0 0C 96 24 00 00 00")]
        [DataRow("00 00 00 00 00 00 00",                         0, 7,  "00 00 00 00 00 00 00")]
        [DataRow("00 00 00 01 FF F4 09",                         0, 7,  "00 00 00 01 00 00 00")]
        [DataRow("01 00 00 00 2B FD 53",                         0, 7,  "01 00 00 00 00 00 00")]
        [DataRow("8D 5E 92 D8 99 45 B3 11 89 DB AE AF 5B 7B",    0, 14, "8D 5E 92 D8 99 45 B3 11 89 DB AE EA 7F DF")]
        [DataRow("8D 46 92 D8 60 91 80 D8 E6 A0 63 AF 87 EA",    0, 14, "8D 46 92 D8 60 91 80 D8 E6 A0 63 00 00 00")]
        [DataRow("FF 8D 46 92 D8 60 91 80 D8 E6 A0 63 AF 87 EA", 1, 14, "FF 8D 46 92 D8 60 91 80 D8 E6 A0 63 00 00 00")]
        [DataRow("8D 46 92 D8 60 91 80 D8 E6 A0 63 AF 87 EA FF", 0, 14, "8D 46 92 D8 60 91 80 D8 E6 A0 63 00 00 00 FF")]
        [DataRow("FF",                                           0, 1,  "FF")]
        [DataRow("FF FF",                                        0, 2,  "FF FF")]
        [DataRow("FF FF FF",                                     0, 3,  "FF FF FF")]
        [DataRow("FF FF FF FF",                                  0, 4,  "FF FF FF FF")]
        [DataRow("FF FF FF FF FF",                               0, 5,  "FF FF FF FF FF")]
        [DataRow("FF FF FF FF FF FF",                            0, 6,  "FF FF FF FF FF FF")]
        [DataRow("FF FF FF FF FF FF FF",                         0, 7,  "FF FF FF FF 01 06 45")]
        [DataRow("FF FF FF FF FF FF FF FF",                      0, 8,  "FF FF FF FF FF F9 B1 F6")]
        public void StripParity_Removes_Parity_From_Last_Three_Bytes(string bytesText, int offset, int length, string expectedText)
        {
            var bytes = TestDataParser.ConvertByteStringToBytes(bytesText);
            var expected = TestDataParser.ConvertByteStringToBytes(expectedText);

            _ModeSParity.StripParity(bytes, offset, length);

            Assert.IsTrue(expected.SequenceEqual(bytes));
        }
    }
}
