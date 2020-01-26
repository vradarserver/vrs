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
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Listener;
using InterfaceFactory;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class BeastMessageBytesExtractorTests
    {
        public TestContext TestContext { get; set; }
        private IBeastMessageBytesExtractor _Extractor;
        private CommonMessageBytesExtractorTests _CommonTests;

        [TestInitialize]
        public void TestInitialise()
        {
            _Extractor = Factory.Resolve<IBeastMessageBytesExtractor>();
            _CommonTests = new CommonMessageBytesExtractorTests(TestContext, _Extractor, ExtractedBytesFormat.ModeS);
        }

        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "BeastListenerText$")]
        public void BeastMessageBytesExtractor_ExtractMessageBytes_Extracts_Messages_From_AVR_Formats()
        {
            _CommonTests.Do_ExtractMessageBytes_Extracts_Messages_From_Bytes(true, 3, 2, (bytes) => {
                var result = new List<byte>();
                for(var i = 0;i < 25;++i) {
                    result.Add(0);
                }
                result.AddRange(bytes);

                return result.ToArray();
            });
        }

        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "BeastListenerBinary$")]
        public void BeastMessageBytesExtractor_ExtractMessageBytes_Extracts_Messages_From_Beast_Binary()
        {
            _CommonTests.Do_ExtractMessageBytes_Extracts_Messages_From_Bytes(false, 2, 2);
        }

        [TestMethod]
        public void BeastMessageBytesExtractor_ExtractMessageBytes_Ignores_Trash_At_End_Of_Buffer_In_Text_Mode()
        {
            var buffer = Encoding.ASCII.GetBytes("123456789.123456789.123  *01020304050607;*A1A2A3A4A5A6A7;");

            var extracted = _Extractor.ExtractMessageBytes(buffer, 0, 25 + 16).Single();  // needs at least 22 characters before it can establish the stream type

            Assert.AreEqual(7, extracted.Length);
            var extractedBytes = new byte[extracted.Length];
            Array.Copy(extracted.Bytes, extracted.Offset, extractedBytes, 0, extracted.Length);

            Assert.IsTrue(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 }.SequenceEqual(extractedBytes));
        }

        [TestMethod]
        public void BeastMessageBytesExtractor_ExtractMessageBytes_Does_Not_Perpetually_Grow()
        {
            var buffer = new byte[2048];
            for(var i = 0;i < buffer.Length;++i) {
                buffer[i] = 0x1a;
            }

            for(var i = 0;i < 100;++i) {
                _Extractor.ExtractMessageBytes(buffer, 0, buffer.Length).ToArray();
            }

            Assert.IsTrue(_Extractor.BufferSize > 0);
            Assert.IsTrue(_Extractor.BufferSize < 10240);
        }

        [TestMethod]
        public void BeastMessageBytesExtractor_Recognises_Mode_A_Messages_Have_A_Two_Byte_Payload()
        {
            // Issue #23 on Github from srsampson (25-Jan-2020) notes that the Beast message bytes
            // extractor is erroneously using a four byte payload for mode A/C messages. The Beast
            // documentation has it at 2 bytes, and that has been confirmed by looking at a dump
            // of Beast messages I took back in 2012. The effect will be that any message following
            // a Mode A or C message in the stream will be ignored, it skips until it sees the
            // magic 1A that denotes the start of a message...

            // This is a copy of the bytes extracted on 14/09/2012 17:26:51
            var buffer = new byte[] {
                // This isn't in the recording, it's just here so the extractor can tell the next 1A is safe to use
                0x00,
                // Initial 0x32 message
                0x1A, 0x32, 0x01, 0x26, 0xC4, 0xD1, 0xFD, 0x55, 0x34, 0x02, 0xE1, 0x97, 0xB0, 0x08, 0x24, 0x71,
                // 0x31 message with two byte payload
                0x1A, 0x31, 0x01, 0x26, 0xC4, 0xD5, 0x35, 0x1B, 0x2D, 0x27, 0x56,
                // 0x33 message - bug would have attached the leading 0x1A 0x33 to the end of the previous 0x31 message so this entire message would be ignored
                0x1A, 0x33, 0x01, 0x26, 0xC4, 0xD8, 0x37, 0xAB, 0x16, 0x8D, 0x39, 0x4A, 0x18, 0x99, 0x11, 0xB2, 0x9C, 0xE0, 0x04, 0x8D, 0x21, 0x47, 0xB3, 
            };

            var extracted = _Extractor
                .ExtractMessageBytes(buffer, 0, buffer.Length)
                .Select(r => (ExtractedBytes)r.Clone())
                .ToArray();

            // If everything is working then we should get two messages out (the extractor only extracts 7 and 14 byte messages
            // so the 0x31 message is always missing from the output). If the bug is in force then we only get the initial 0x32
            // message, the 0x33 following the 0x31 gets skipped.
            Assert.AreEqual(2, extracted.Length);
            Assert.AreEqual(7, extracted[0].Length);
            Assert.AreEqual(14, extracted[1].Length);
        }
    }
}
