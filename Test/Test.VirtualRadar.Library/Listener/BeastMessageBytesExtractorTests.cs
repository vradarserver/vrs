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
            _Extractor = Factory.Singleton.Resolve<IBeastMessageBytesExtractor>();
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
    }
}
