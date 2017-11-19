// Copyright © 2017 onwards, Andrew Whewell
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface;

namespace Test.VirtualRadar.Interface
{
    [TestClass]
    public class TextContentTests
    {
        [TestMethod]
        public void TextContent_Constructor_Initialises_To_Known_Values()
        {
            var textContent = new TextContent();

            TestUtilities.TestProperty(textContent, r => r.IsDirty, false);
            TestUtilities.TestProperty(textContent, r => r.Content, "", "Abc");
            TestUtilities.TestProperty(textContent, r => r.Encoding, Encoding.UTF8, Encoding.UTF32);
            TestUtilities.TestProperty(textContent, r => r.HadPreamble, false);
        }

        [TestMethod]
        public void TextContent_Content_Sets_IsDirty()
        {
            var textContent = new TextContent();
            textContent.Content = "1";

            Assert.IsTrue(textContent.IsDirty);
        }

        [TestMethod]
        public void TextContent_GetBytes_Returns_Correct_Bytes()
        {
            var sourceText = "£$€abc123XYZ°©";

            foreach(var encoding in new Encoding[] { Encoding.Default, Encoding.UTF7, Encoding.UTF8, Encoding.Unicode, Encoding.BigEndianUnicode, Encoding.UTF32 }) {
                var noPreamble = encoding.GetBytes(sourceText);
                var withPreamble = encoding.GetPreamble().Concat(noPreamble);

                var tcNoPreamble = new TextContent() { Content = sourceText, Encoding = encoding, HadPreamble = false };
                var tcWithPreamble = new TextContent() { Content = sourceText, Encoding = encoding, HadPreamble = true };

                Assert.IsTrue(noPreamble.SequenceEqual(tcNoPreamble.GetBytes(includePreamble: false)));
                Assert.IsTrue(noPreamble.SequenceEqual(tcNoPreamble.GetBytes(includePreamble: true)));

                Assert.IsTrue(noPreamble.SequenceEqual(tcWithPreamble.GetBytes(includePreamble: false)));
                Assert.IsTrue(withPreamble.SequenceEqual(tcWithPreamble.GetBytes(includePreamble: true)));
            }
        }

        [TestMethod]
        public void TextContent_Load_Decodes_Stream_Correctly()
        {
            var sourceText = "£$€abc123XYZ°©";

            foreach(var encoding in new Encoding[] { Encoding.Default, Encoding.UTF7, Encoding.UTF8, Encoding.Unicode, Encoding.BigEndianUnicode, Encoding.UTF32 }) {
                foreach(var withPreamble in new bool[] { false, true }) {
                    var errorText = $"encoding={encoding} withPreamble={withPreamble}";

                    var preamble = withPreamble ? encoding.GetPreamble() : new byte[0];
                    var bytes = preamble.Concat(encoding.GetBytes(sourceText)).ToArray();

                    using(var stream = new MemoryStream(bytes)) {
                        var textContent = TextContent.Load(stream);

                        Assert.AreEqual(preamble.Length > 0, textContent.HadPreamble, errorText);
                        Assert.IsFalse(textContent.IsDirty);

                        // Without a preamble we always assume that the content is UTF8, so things might get a bit weird here
                        if(preamble.Length > 0) {
                            Assert.AreEqual(sourceText, textContent.Content, errorText);
                            Assert.AreEqual(encoding.EncodingName, textContent.Encoding.EncodingName, errorText);
                        } else {
                            var nativeDetected = textContent.Content == sourceText && textContent.Encoding.EncodingName == encoding.EncodingName;
                            var utf8Detected = textContent.Content == Encoding.UTF8.GetString(bytes) && textContent.Encoding.EncodingName == Encoding.UTF8.EncodingName;

                            Assert.IsTrue(nativeDetected || utf8Detected, errorText);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void TextContent_Load_Decodes_Bytes_Correctly()
        {
            var sourceText = "£$€abc123XYZ°©";

            foreach(var encoding in new Encoding[] { Encoding.Default, Encoding.UTF7, Encoding.UTF8, Encoding.Unicode, Encoding.BigEndianUnicode, Encoding.UTF32 }) {
                foreach(var withPreamble in new bool[] { false, true }) {
                    var errorText = $"encoding={encoding} withPreamble={withPreamble}";

                    var preamble = withPreamble ? encoding.GetPreamble() : new byte[0];
                    var bytes = preamble.Concat(encoding.GetBytes(sourceText)).ToArray();

                    var textContent = TextContent.Load(bytes);

                    Assert.AreEqual(preamble.Length > 0, textContent.HadPreamble, errorText);
                    Assert.IsFalse(textContent.IsDirty);

                    // Without a preamble we always assume that the content is UTF8, so things might get a bit weird here
                    if(preamble.Length > 0) {
                        Assert.AreEqual(sourceText, textContent.Content, errorText);
                        Assert.AreEqual(encoding.EncodingName, textContent.Encoding.EncodingName, errorText);
                    } else {
                        var nativeDetected = textContent.Content == sourceText && textContent.Encoding.EncodingName == encoding.EncodingName;
                        var utf8Detected = textContent.Content == Encoding.UTF8.GetString(bytes) && textContent.Encoding.EncodingName == Encoding.UTF8.EncodingName;

                        Assert.IsTrue(nativeDetected || utf8Detected, errorText);
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void TextContent_Load_Disposes_Of_Stream_By_Default()
        {
            var stream = new MemoryStream(new byte[0]);
            TextContent.Load(stream);

            stream.Read(new byte[1], 0, 1);
        }

        [TestMethod]
        public void TextContent_Load_Can_Leave_Stream_Open()
        {
            using(var stream = new MemoryStream(new byte[0])) {
                TextContent.Load(stream, leaveOpen: true);

                stream.Read(new byte[1], 0, 1);
            }
        }
    }
}
