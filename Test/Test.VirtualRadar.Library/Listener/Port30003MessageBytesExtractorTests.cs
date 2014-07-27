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
    public class Port30003MessageBytesExtractorTests
    {
        public TestContext TestContext { get; set; }
        private IPort30003MessageBytesExtractor _Extractor;
        private CommonMessageBytesExtractorTests _CommonTests;

        [TestInitialize]
        public void TestInitialise()
        {
            _Extractor = Factory.Singleton.Resolve<IPort30003MessageBytesExtractor>();
            _CommonTests = new CommonMessageBytesExtractorTests(TestContext, _Extractor, ExtractedBytesFormat.Port30003);
        }

        [TestMethod]
        [DataSource("Data Source='BaseStationTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "MessageBytesExtractor$")]
        public void Port30003MessageBytesExtractor_ExtractMessageBytes_Extracts_Mode_S_Messages_From_Bytes()
        {
            _CommonTests.Do_ExtractMessageBytes_Extracts_Messages_From_Bytes(true, 3, 2);
        }

        [TestMethod]
        public void Port30003MessageBytesExtractor_ExtractMessageBytes_Honours_Byte_Array_Length()
        {
            var fullMessage = Encoding.ASCII.GetBytes("\nABC\n123\n");

            var extracted = _Extractor.ExtractMessageBytes(fullMessage, 0, 5).Single();
            Assert.AreEqual("ABC", Encoding.ASCII.GetString(extracted.Bytes, extracted.Offset, extracted.Length));

            extracted = _Extractor.ExtractMessageBytes(fullMessage, 4, 5).Single();
            Assert.AreEqual("123", Encoding.ASCII.GetString(extracted.Bytes, extracted.Offset, extracted.Length));
        }

        [TestMethod]
        public void Port30003MessageBytesExtractor_ExtractMessageBytes_Does_Not_Perpetually_Grow()
        {
            var buffer = new byte[512];

            for(var i = 0;i < 100;++i) {
                _Extractor.ExtractMessageBytes(buffer, 0, buffer.Length).ToArray();
            }

            Assert.IsTrue(_Extractor.BufferSize > 0);
            Assert.IsTrue(_Extractor.BufferSize < 10240);
        }
    }
}
