// Copyright © 2016 onwards, Andrew Whewell
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
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.Listener;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class PlaneFinderMessageBytesExtractorTests
    {
        public TestContext TestContext { get; set; }
        private IPlaneFinderMessageBytesExtractor _Extractor;
        private CommonMessageBytesExtractorTests _CommonTests;

        [TestInitialize]
        public void TestInitialise()
        {
            _Extractor = Factory.Singleton.Resolve<IPlaneFinderMessageBytesExtractor>();
            _CommonTests = new CommonMessageBytesExtractorTests(TestContext, _Extractor, ExtractedBytesFormat.ModeS);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        /// <summary>
        /// Adds DCE stuffing to the list of bytes passed across.
        /// </summary>
        /// <param name="bytes"></param>
        private List<byte> AddDCEStuffing(List<byte> bytes)
        {
            for(var i = 0;i < bytes.Count;++i) {
                if(bytes[i] == 0x010) {
                    bytes.Insert(i, 0x10);
                    ++i;
                }
            }

            return bytes;
        }

        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "PlaneFinderListener$")]
        public void PlaneFinderMessageBytesExtractor_ExtractModeSMessageBytes_Extracts_Mode_S_Messages_From_Bytes()
        {
            _CommonTests.Do_ExtractMessageBytes_Extracts_Messages_From_Bytes(false, 2, 2);
        }

        [TestMethod]
        public void PlaneFinderMessageBytesExtractor_ExtractModeSMessageBytes_Only_Translates_Packet_Type_C1()
        {
            var bytes = new byte[] {
                0x10, 0x03, 0x10, 0xC1, 0x00, 0x11, 0xFF, 0x51, 0x52, 0x53, 0x54, 0x61, 0x62, 0x63, 0x64, 0xA1, 0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0x10, 0x03,
            };
            for(int i = 0;i < 256;++i) {
                switch(i) {
                    case 0x03:
                    case 0x10:
                        // The documentation says that these will never be used as a packet type
                        continue;
                }

                TestCleanup();
                TestInitialise();

                bytes[3] = (byte)i;

                var extracted = _Extractor.ExtractMessageBytes(bytes, 0, bytes.Length).SingleOrDefault();

                bool expectedTranslate = i == 0xc1;
                if(expectedTranslate) Assert.IsNotNull(extracted, "Did not extract PlaneFinder packet type 0x{0:X2}", i);
                else                  Assert.IsNull(extracted, "Extracted PlaneFinder packet type 0x{0:X2}", i);
            }
        }

        [TestMethod]
        public void PlaneFinderMessageBytesExtractor_ExtractMessageBytes_Does_Not_Perpetually_Grow()
        {
            var buffer = new byte[512];

            for(var i = 0;i < 100;++i) {
                _Extractor.ExtractMessageBytes(buffer, 0, buffer.Length).ToArray();
            }

            Assert.IsTrue(_Extractor.BufferSize > 0);
            Assert.IsTrue(_Extractor.BufferSize <= 0x1000);
        }
    }
}
