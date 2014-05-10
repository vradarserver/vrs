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
using Test.Framework;
using VirtualRadar.Interface.Listener;

namespace Test.VirtualRadar.Library.Listener
{
    public class CommonMessageBytesExtractorTests
    {
        public TestContext TestContext { get; set; }

        public IMessageBytesExtractor Extractor { get; set; }

        public ExtractedBytesFormat Format { get; set; }

        public CommonMessageBytesExtractorTests(TestContext testContext, IMessageBytesExtractor extractor, ExtractedBytesFormat format)
        {
            TestContext = testContext;
            Extractor = extractor;
            Format = format;
        }

        public void Do_ExtractMessageBytes_Extracts_Messages_From_Bytes(bool inputIsText, int blobCount, int extractedCount, Func<byte[], byte[]> alterFirstPacket = null)
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            var comments = worksheet.String("Comments");

            var input = new List<byte[]>();
            for(var i = 1;i <= blobCount;++i) {
                var blobColumn = String.Format("Blob{0}", i);
                if(worksheet.String(blobColumn) != null) {
                    byte[] bytes;
                    if(inputIsText) bytes = Encoding.ASCII.GetBytes(worksheet.String(blobColumn).Replace("\\n", "\n").Replace("\\r", "\r"));
                    else bytes = worksheet.Bytes(blobColumn);

                    if(input.Count == 0 && alterFirstPacket != null) bytes = alterFirstPacket(bytes);
                    input.Add(bytes);
                }
            }

            var expectedOutput = new List<ExtractedBytes>();
            for(var i = 1;i <= extractedCount;++i) {
                var bytesColumn = String.Format("Extracted{0}", i);
                var parityColumn = String.Format("HadParity{0}", i);
                var checksumColumn = String.Format("BadChecksum{0}", i);
                var signalLevelColumn = String.Format("SignalLevel{0}", i);
                if(worksheet.String(bytesColumn) != null || worksheet.String(checksumColumn) != null) {
                    expectedOutput.Add(new ExtractedBytes() { 
                        Bytes = worksheet.Bytes(bytesColumn),
                        HasParity = worksheet.Bool(parityColumn),
                        ChecksumFailed = worksheet.Bool(checksumColumn),
                        SignalLevel = worksheet.NInt(signalLevelColumn),
                    });
                }
            }

            var output = new List<ExtractedBytes>();
            foreach(var inputBytes in input) {
                foreach(var extractedBytes in Extractor.ExtractMessageBytes(inputBytes, 0, inputBytes.Length)) {
                    output.Add((ExtractedBytes)extractedBytes.Clone());
                }
            }

            Assert.AreEqual(expectedOutput.Count, output.Count, comments);
            for(var i = 0;i < expectedOutput.Count;++i) {
                var expected = expectedOutput[i];
                var actual = output[i];
                Assert.AreEqual(Format, actual.Format);
                Assert.AreEqual(expected.ChecksumFailed, actual.ChecksumFailed, comments);
                if(!expected.ChecksumFailed) {
                    Assert.AreEqual(expected.HasParity, actual.HasParity, comments);
                    Assert.AreEqual(expected.SignalLevel, actual.SignalLevel, comments);

                    var actualBytes = new byte[actual.Length];
                    Array.Copy(actual.Bytes, actual.Offset, actualBytes, 0, actual.Length);

                    Assert.IsTrue(expected.Bytes.SequenceEqual(actualBytes), comments);
                }
            }
        }
    }
}
