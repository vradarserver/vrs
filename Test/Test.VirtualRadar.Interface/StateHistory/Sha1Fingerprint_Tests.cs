// Copyright © 2020 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Test.Framework;
using VirtualRadar.Interface.StateHistory;

namespace Test.VirtualRadar.Interface.StateHistory
{
    [TestClass]
    public class Sha1Fingerprint_Tests
    {
        [TestMethod]
        public void CreateFingerprintFromText_Produces_Correct_Results()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Text = (string)null,  Expected = (string)null, },
                new { Text = "",            Expected = "", },
                new { Text = "A",           Expected = "6dcd4ce23d88e2ee9568ba546c007c63d9131c1b", },
                new { Text = "a",           Expected = "86f7e437faa5a7fce15d1ddcb9eaeaea377667b8", },
            }, row => {
                var actualBytes = Sha1Fingerprint.CreateFingerprintFromText(row.Text);
                var actualText = actualBytes == null ? null : String.Join("", actualBytes.Select(r => r.ToString("x2")));

                Assert.AreEqual(row.Expected, actualText, ignoreCase: true);
            });
        }

        [TestMethod]
        public void CreateFingerprintFromObjects_Produces_Correct_Results()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new { Input = (object[])null,               Expected = (string)null, },
                new { Input = new object[] { },             Expected = "", },
                new { Input = new object[] { "A" },         Expected = "7d157d7c000ae27db146575c08ce30df893d3a64", },  // A\n
                new { Input = new object[] { null },        Expected = "49594bfbf3a97620aa5dea68d4440b39b6bede04", },  // \0\n
                new { Input = new object[] { "a", "B", },   Expected = "627c3f93e5bde43824efcfdc4d7462abd2974282", },  // a\nB\n
            }, row => {
                var actualBytes = Sha1Fingerprint.CreateFingerprintFromObjects(row.Input);
                var actualText = actualBytes == null ? null : String.Join("", actualBytes.Select(r => r.ToString("x2")));

                Assert.AreEqual(row.Expected, actualText, ignoreCase: true);
            });
        }

        [TestMethod]
        public void ConvertToString_Expresses_Byte_Array_As_String()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new {
                    Input = (byte[])null,
                    Expected = (string)null
                },
                new {
                    Input = new byte[0],
                    Expected = "",
                },
                new {
                    Input = new byte[] { 0x7d, 0x15, 0x7d, 0x7c, 0x00, 0x0a, 0xe2, 0x7d, 0xb1, 0x46, 0x57, 0x5c, 0x08, 0xce, 0x30, 0xdf, 0x89, 0x3d, 0x3a, 0x64 },
                    Expected = "7d157d7c000ae27db146575c08ce30df893d3a64",
                },
            }, row => {
                var actual = Sha1Fingerprint.ConvertToString(row.Input);
                Assert.AreEqual(row.Expected, actual);
            });
        }

        [TestMethod]
        public void ConvertFromString_Converts_String_To_Byte_Array()
        {
            new InlineDataTest(this).TestAndAssert(new [] {
                new {
                    Input = (string)null,
                    Expected = (byte[])null,
                },
                new {
                    Input = "",
                    Expected = new byte[0],
                },
                new {
                    Input = "7d157d7c000ae27db146575c08ce30df893d3a64",
                    Expected = new byte[] { 0x7d, 0x15, 0x7d, 0x7c, 0x00, 0x0a, 0xe2, 0x7d, 0xb1, 0x46, 0x57, 0x5c, 0x08, 0xce, 0x30, 0xdf, 0x89, 0x3d, 0x3a, 0x64 },
                },
                new {
                    Input = "7D157D7C000AE27DB146575C08CE30DF893D3A64",
                    Expected = new byte[] { 0x7d, 0x15, 0x7d, 0x7c, 0x00, 0x0a, 0xe2, 0x7d, 0xb1, 0x46, 0x57, 0x5c, 0x08, 0xce, 0x30, 0xdf, 0x89, 0x3d, 0x3a, 0x64 },
                },
            }, row => {
                var actual = Sha1Fingerprint.ConvertFromString(row.Input);
                if(row.Expected == null) {
                    Assert.IsNull(actual);
                } else {
                    Assert.IsTrue(row.Expected.SequenceEqual(actual));
                }
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ConvertFromString_Throws_If_Invalid_Length()
        {
            Sha1Fingerprint.ConvertFromString("7d157d7c000ae27db146575c08ce30df893d3a6");   // missing last digit
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ConvertFromString_Throws_If_Contains_Invalid_Characters()
        {
            Sha1Fingerprint.ConvertFromString("gd157d7c000ae27db146575c08ce30df893d3a64");   // first character is not hex
        }
    }
}
