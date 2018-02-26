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
using InterfaceFactory;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class Sbs3MessageBytesExtractorTests
    {
        public TestContext TestContext { get; set; }
        private ISbs3MessageBytesExtractor _Extractor;
        private CommonMessageBytesExtractorTests _CommonTests;

        [TestInitialize]
        public void TestInitialise()
        {
            _Extractor = Factory.Resolve<ISbs3MessageBytesExtractor>();
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

        /// <summary>
        /// Builds up a message packet from the payload passed across. The payload is modified by this method.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="isFirstMessage"></param>
        /// <returns></returns>
        private List<byte> BuildValidMessagePacket(List<byte> payload, bool isFirstMessage = true)
        {
            var checksumCalculator = new Crc16Ccitt(Crc16Ccitt.InitialiseToZero);
            var checksum = AddDCEStuffing(new List<byte>(checksumCalculator.ComputeChecksumBytes(payload.ToArray(), false)));

            var result = new List<byte>();
            if(isFirstMessage) result.Add(0x00); // prefix with a leading byte so that the listener will not reject it
            result.Add(0x10);
            result.Add(0x02);
            result.AddRange(AddDCEStuffing(payload));
            result.Add(0x10);
            result.Add(0x03);
            result.AddRange(checksum);

            return result;
        }

        /// <summary>
        /// Builds up a message packet from a payload described as a string of bytes in hex separated by spaces.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="isFirstMessage"></param>
        /// <returns></returns>
        private List<byte> BuildValidMessagePacket(string bytes, bool isFirstMessage = true)
        {
            var payload = new List<byte>();
            foreach(var byteText in bytes.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)) {
                payload.Add(Convert.ToByte(byteText, 16));
            }

            return BuildValidMessagePacket(payload, isFirstMessage);
        }

        [TestMethod]
        [DataSource("Data Source='RawDecodingTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "Sbs3RadarListener$")]
        public void Sbs3MessageBytesExtractor_ExtractModeSMessageBytes_Extracts_Mode_S_Messages_From_Bytes()
        {
            _CommonTests.Do_ExtractMessageBytes_Extracts_Messages_From_Bytes(false, 2, 2);
        }

        [TestMethod]
        public void Sbs3MessageBytesExtractor_Connect_Only_Translates_SBS3_Packet_Types_1_5_And_7()
        {
            for(int i = 0;i < 256;++i) {
                TestCleanup();
                TestInitialise();

                int payloadLength;
                switch(i) {
                    case 0x01:
                    case 0x05:  payloadLength = 18; break;
                    case 0x07:  payloadLength = 11; break;
                    case 0x09:  payloadLength = 6; break;
                    case 0x20:  payloadLength = 2; break;
                    case 0x21:  payloadLength = 10; break;
                    case 0x26:  payloadLength = 35; break;
                    case 0x2a:
                    case 0x2b:  payloadLength = 18; break; // no fixed length
                    case 0x2c:  payloadLength = 2; break;
                    case 0x38:  payloadLength = 26; break;
                    case 0x3b:  payloadLength = 129; break;
                    case 0x45:  payloadLength = 18; break; // no fixed length
                    case 0x57:  payloadLength = 18; break; // no fixed length
                    case 0x58:  payloadLength = 18; break; // no fixed length
                    default:    payloadLength = 18; break; // 18 or over probably has the best chance of confusing the code
                }

                var payload = new List<byte>();
                payload.Add((byte)i);
                payload.AddRange(new byte[payloadLength]);
                var message = BuildValidMessagePacket(payload).ToArray();

                var extracted = _Extractor.ExtractMessageBytes(message, 0, message.Length).SingleOrDefault();

                bool expectedTranslate = i == 1 || i == 5 || i == 7;
                if(expectedTranslate) Assert.IsNotNull(extracted, "Did not extract SBS3 packet type {0}", i);
                else                  Assert.IsNull(extracted, "Extracted SBS3 packet type {0}", i);
            }
        }

        [TestMethod]
        public void Sbs3MessageBytesExtractor_ExtractMessageBytes_Does_Not_Perpetually_Grow()
        {
            var buffer = new byte[512];

            for(var i = 0;i < 100;++i) {
                _Extractor.ExtractMessageBytes(buffer, 0, buffer.Length).ToArray();
            }

            Assert.IsTrue(_Extractor.BufferSize > 0);
            Assert.IsTrue(_Extractor.BufferSize <= 0x2800);
        }
    }
}
